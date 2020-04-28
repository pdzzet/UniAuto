using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Timers;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class JobService
    {
        #region [OXR Information Request]
        public void OXRInformationRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                eReturnCode1 returnCode = eReturnCode1.Unknown;

                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", eqpNo, inputData.TrackKey));
                    OXRInformationRequestReportReply(eqpNo, "", "", "", returnCode, "", eBitResult.OFF, "", inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]
                string cstSeqNO = inputData[0][0][0].Value;
                string jobSeqNO = inputData[0][0][1].Value;
                string unitNO = inputData[0][0][2].Value;
                string oxrInfo = string.Empty;
                #endregion

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}].",
                        eqpNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO));

                #region [取得FabType]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                string oxrType=string.Empty;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    oxrType = "PLC_OXRINFO_CELL";
                else
                    oxrType = "PLC_OXRINFO_AC";
                #endregion

                #region [取得OXR]
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);
                if (slotData == null)
                {
                    throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", eqpNo, cstSeqNO, jobSeqNO));
                }
                if (slotData.OXRInformation == "")
                {
                    returnCode = eReturnCode1.NG;
                }
                else
                {   //20160711 modify by Frank THK來Request時，都用MES原始資訊回覆。 
                    if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1 )
                    {
                        if (eqp.Data.NODENO == "L8")
                        {
                            for (int i = 0; i < slotData.MesProduct.SUBPRODUCTGRADES.Length; i++)
                            {
                                oxrInfo += ConstantManager[oxrType][slotData.MesProduct.SUBPRODUCTGRADES.Substring(i, 1)].Value;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < slotData.OXRInformation.Length; i++)
                            {
                                oxrInfo += ConstantManager[oxrType][slotData.OXRInformation.Substring(i, 1)].Value;
                            }
                        }
                        returnCode = eReturnCode1.OK;
                    }
                    else
                    {
                        for (int i = 0; i < slotData.OXRInformation.Length; i++)
                        {
                            oxrInfo += ConstantManager[oxrType][slotData.OXRInformation.Substring(i, 1)].Value;
                        }
                        returnCode = eReturnCode1.OK;
                    }
                }
                #endregion
                
                OXRInformationRequestReportReply(inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO, unitNO, returnCode, oxrInfo,eBitResult.ON, slotData.GlassChipMaskBlockID, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    OXRInformationRequestReportReply(inputData.Metadata.NodeNo,
                        inputData[0][0][0].Value,
                        inputData[0][0][1].Value,
                        inputData[0][0][2].Value,
                        eReturnCode1.NG, "", eBitResult.ON, "", inputData.TrackKey);
                }
            }
        }

        private void OXRInformationRequestReportReply(string eqpNo, string cstSeqNO, string jobSeqNO, string unitNO, 
            eReturnCode1 returnCode, string oxrInfo, eBitResult value, string glsID, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_OXRInformationRequestReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                //string log = string.Empty;
                StringBuilder log = new StringBuilder();

                if (outputdata != null)
                {
                    outputdata[0][1][0].Value = ((int)value).ToString();

                    if (value == eBitResult.OFF)
                    {
                        outputdata[0][0].IsDisable = true;
                    }
                    else
                    {
                        //20150712 Modify by Frank
                        outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();

                        outputdata[0][0][0].Value = cstSeqNO;
                        log.AppendFormat("CST_SEQNO=[{0}]", cstSeqNO);

                        outputdata[0][0][1].Value = jobSeqNO;
                        log.AppendFormat(" JOB_SEQNO=[{0}]", jobSeqNO);

                        outputdata[0][0][2].Value = unitNO;
                        log.AppendFormat(" UNIT_NO=[{0}]", unitNO);

                        outputdata[0][0][3].Value = ((int)returnCode).ToString();
                        log.AppendFormat(" RETURN_CODE=[{0}]", returnCode.ToString());

                        outputdata[0][0][4].Value = oxrInfo;
                        log.AppendFormat(" OXR_INFO=[{0}], ", oxrInfo);
                    }
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "OXRInformationRequestReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(OXRInformationRequestReportReplyTimeout), trackKey);
                    }

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] {2}SET BIT=[{3}].", eqpNo, trackKey, log, value));
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void OXRInformationRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] OXR INFORMATION REQUEST REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                //OXRInformationRequestReportReply(sArray[0], eBitResult.OFF, trackKey);
                OXRInformationRequestReportReply(sArray[0], "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [OXR Information Update]
        public void OXRInformationUpdateReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", eqpNo, inputData.TrackKey));
                    OXRInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value;
                string oxrInfo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}] OXR_INFO=[{5}].", 
                    eqpNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO, oxrInfo));

                #region [取得FabType]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                string fabType = line.Data.FABTYPE;
                string oxrType = string.Empty;
                int len = 0;
                if (fabType == "CELL")
                {
                    oxrType = "PLC_OXRINFO2_CELL";
                    len = 2;
                }
                else
                {
                    oxrType = "PLC_OXRINFO2_AC";
                    len = 4;
                }
                #endregion

                #region [OX Info]
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);
                if (slotData == null) throw new Exception(string.Format("CAN'T FIND JOB DATA, CST_SEQNO=[{0}] JOB_SEQNO=[{1}]!", cstSeqNO, jobSeqNO));
                string oxrInfomation = string.Empty;
                int x = 0; //add by qiumin 20180301 CF  增加BC卡控O品小于50%时，BC切EQ transfer Stop并弹窗报警
                bool oxinfocheckflag = false;
                oxinfocheckflag = eqp.File.OxinfoCheckFlag ;
                for (int i = 0; i < slotData.ChipCount; i++)
                {
                    oxrInfomation += ConstantManager[oxrType][oxrInfo.ToString().Substring(i * len, len)].Value;
                    if (ConstantManager[oxrType][oxrInfo.ToString().Substring(i * len, len)].Value!="O")
                    {
                        x ++;
                    }
                }
                lock (slotData)
                {
                    slotData.OXRInformation = oxrInfomation;
                }

                if ((eqp.Data.LINEID.Contains("REP") || eqp.Data.LINEID.Contains("QMA")) && fabType == "CF" && x > (slotData.ChipCount - x) && oxinfocheckflag)  //add by qiumin 20180301 CF  增加BC卡控O品小于50%时，BC切EQ transfer Stop并弹窗报警
                {
                    Invoke("EquipmentService", "TransferStopCommand", new object[] { eqpNo , "1", inputData.TrackKey });
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS - EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}] OXR_INFO!=O COUNT[{5}].",
                    eqpNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO, x.ToString()));
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData .TrackKey , line.Data.LINENAME  , 
                                    string.Format("[{0}]OXR_INFO!=O COUNT[{1}],Please check REP ",inputData.Metadata.NodeNo,x.ToString()  )});
                }
                ObjectManager.JobManager.EnqueueSave(slotData);
              
                #endregion
                OXRInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                ObjectManager.JobManager.RecordJobHistory(slotData, eqp.Data.NODEID, eqpNo, "0", "00", "0", eJobEvent.OXUpdate.ToString(), inputData.TrackKey);// OXRUpdate Save Job  History 20150406 Tom
            
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    OXRInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void OXRInformationUpdateReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_OXRInformationUpdateReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "OXRInformationUpdateReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(OXRInformationUpdateReportReplyTimeout), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void OXRInformationUpdateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] OXR INFORMATION UPDATE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
                OXRInformationUpdateReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Fetch,Send,Stor,Receive CST
        public void StoreCSTReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNO = inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    StoreCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word 1
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string type = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIN=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] PORTorUNIT=[{4}]({5}) UNIT_NO=[{6}] PORT_NO=[{7}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, type, 
                    type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"), unitNo, portNo));

                StoreCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(casseqno));

                if (cst == null)
                {
                    cst = new Cassette();
                    cst.CassetteSequenceNo = casseqno;
                    ObjectManager.CassetteManager.CreateCassette(cst);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CST_SEQNO=[{1}] CAN NOT FOUND CST DATA IN STORE CST, CREATE NEW CST DATA.",
                            eqp.Data.NODENO, casseqno));
                }
                RecordCST_JOBHistory(eqp, casseqno, cst.CassetteID, eJobEvent.Store, "", inputData.TrackKey);

                //Jun Add 20150602 For POL Unloader AutoClave Report LotProcessStart
                if (cst.CellBoxProcessed == eboxReport.Processing)
                {
                    Port port = ObjectManager.PortManager.GetPort("03");
                    if (port != null)
                    {
                        object[] _data = new object[3]
                        { 
                            inputData.TrackKey,               /*0  TrackKey*/
                            port,                             /*1  Port*/
                            cst,                              /*2  Cassette*/
                        };

                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "LotProcessStarted", _data);
                        Invoke(eServiceName.APCService, "LotProcessStart", _data);
                    }
                }
                //if (type == "1")
                //{
                //    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                //    if (unit == null) throw new Exception(string.Format("Can't find Unit No =[{0}) in UnitEntity!", unitNo));
                //}
                //else
                //{
                //    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2,'0'));
                //    if (port == null) throw new Exception(string.Format("Can't find Port No =[{0}) in PortEntity!", portNo));
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    StoreCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void StoreCSTDataReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_StoreCSTReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + StoreTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + StoreTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + StoreTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(StoreCSTDataReportReplyTimeout), trackKey);
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
        private void StoreCSTDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] STORE CST DATA REPORT TIMEOUT, SET=[OFF].", sArray[0], trackKey));

                StoreCSTDataReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void FetchOutCSTReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNO = inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    FetchCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word 1
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string type = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIN=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] PORTorUNIT=[{4}]({5}) UNIT_NO=[{6}] PORT_NO=[{7}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, type,
                    type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"), unitNo, portNo));

                FetchCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(casseqno));

                if (cst == null)
                {
                    cst = new Cassette();
                    cst.CassetteSequenceNo = casseqno;
                    ObjectManager.CassetteManager.CreateCassette(cst);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CST_SEQNO=[{1}] CAN NOT FOUND CST DATA IN FETCH OUT CST, CREATE NEW CST DATA.",
                            eqp.Data.NODENO, casseqno));
                }
                RecordCST_JOBHistory(eqp, casseqno, cst.CassetteID, eJobEvent.FetchOut, "", inputData.TrackKey);
                //if (type == "1")
                //{
                //    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                //    if (unit == null) throw new Exception(string.Format("Can't find Unit No =[{0}) in UnitEntity!", unitNo));
                //}
                //else
                //{
                //    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2,'0'));
                //    if (port == null) throw new Exception(string.Format("Can't find Port No =[{0}) in PortEntity!", portNo));
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    FetchCSTDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void FetchCSTDataReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_FetchOutCSTReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + FetchTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + FetchTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + FetchTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(FetchCSTDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void FetchCSTDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FETCH OUT CST DATA REPORT TIMEOUT, SET BIT=[OFF].", 
                    sArray[0], trackKey));
                FetchCSTDataReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        public void ReceiveCSTReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNO = inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    ReceiveCSTReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data]  Word 1
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}].", 
                    inputData.Metadata.NodeNo, inputData.TrackKey, casseqno));

                ReceiveCSTReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(casseqno));

                if (cst == null)
                {
                    cst = new Cassette();
                    cst.CassetteSequenceNo = casseqno;
                    ObjectManager.CassetteManager.CreateCassette(cst);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CST_SEQNO=[{1}] CAN NOT FOUND CST DATA IN RECEIVE CST, CREATE NEW CST DATA.",
                            eqp.Data.NODENO, casseqno));
                }
                RecordCST_JOBHistory(eqp, casseqno, cst.CassetteID, eJobEvent.Receive, "", inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    ReceiveCSTReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void ReceiveCSTReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_ReceiveCSTReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ReceiveTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ReceiveTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ReceiveTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(ReceiveCSTReportReplyTimeout), trackKey);
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
        private void ReceiveCSTReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECEIVE CST DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                ReceiveCSTReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SendOutCSTReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", eqpNo, inputData.TrackKey));
                    SendOutCSTReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data]  Word 1
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string trackingData = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                string log = string.Empty;

                IDictionary<string, string> data = ObjectManager.SubJobDataManager.Decode(trackingData, "");

                if (data != null && data.Count > 0)
                {
                    log = ":";
                    foreach (string key in data.Keys)
                    {
                        string value = data[key];
                        switch (value)
                        {
                            case "0": log += string.Format(" {0}=[{1}](NOT PROCESSED)", key, value); break;
                            case "1": log += string.Format(" {0}=[{1}](NORMAL PROCESSED)", key, value); break;
                            case "2": log += string.Format(" {0}=[{1}](ABNORMAL PROCESSED)", key, value); break;
                            case "3": log += string.Format(" {0}=[{1}](PROCESS SKIP)", key, value); break;
                            default: log += string.Format(" {0}=[{1}](UNKNOWN)", key, value); break;
                        }
                    }
                }
                else
                    log = string.Format("=[{0}]", trackingData);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIN=[ON] CIM_MODE=[{2}] CST_SEQNO=[{4}] TRACKING_DATA{5}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, log));

                SendOutCSTReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(casseqno));

                if (cst == null)
                {
                    cst = new Cassette();
                    cst.CassetteSequenceNo = casseqno;
                    cst.TrackingData = trackingData;
                    ObjectManager.CassetteManager.CreateCassette(cst);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CST_SEQNO=[{1}] CAN NOT FOUND CST DATA IN SEND OUT CST, CREATE NEW CST DATA.",
                            eqp.Data.NODENO, casseqno));
                }
                RecordCST_JOBHistory(eqp, casseqno, cst.CassetteID, eJobEvent.SendOut, trackingData, inputData.TrackKey);
                //To Do Tracking Data

                //"0：Not Processed (Not In EQ)
                //1：Normal Processed [In EQ]
                //2：Abnormal Processed [In EQ]
                //3：Process Skip [In EQ]
                //1st Word：Wet Cleaner
                //2nd Word：Dry 1
                //3rd Word：Dry 2

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void SendOutCSTReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_SendOutCSTReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ReceiveTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ReceiveTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ReceiveTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(SendOutCSTReportReplyTimeout), trackKey);
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
        private void SendOutCSTReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SEND OUT CST DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                SendOutCSTReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RemoveCSTReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNO = inputData.Metadata.NodeNo;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    RemoveCSTReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word 1
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string type = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIN=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] PORTorUNIT=[{4}]({5}) UNIT_NO=[{6}] PORT_NO=[{7}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, type,type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"), unitNo, portNo));

                RemoveCSTReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(casseqno));
                string cstid = string.Empty;
                if (line.Data.LINETYPE == eLineType.CELL.CCOVP && cst != null)
                {
                    IList<Job> jobs = ObjectManager.JobManager.GetJobs(cst.CassetteSequenceNo);
                    int delayTime = ParameterManager["FILEMOVEDELAYTIME"].GetInteger();//20161115 sy add                    
                    ObjectManager.JobManager.MoveJobs(line, jobs, delayTime);//20161031 sy add Move job, Delete wip
                }
                //sy modify 20160926
                #region [MES Data]
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CCOVP && cst != null)
                    {
                        Invoke(eServiceName.MESService, "BoxLineOutReport", new object[4] { inputData.TrackKey, eqp.Data.LINEID, "", cst.CassetteID });//(string trxID, string lineID, string portID, string boxID                        
                    }
                }
                #endregion
                if (cst != null)
                {
                    ObjectManager.CassetteManager.DeleteCassette(casseqno);
                    cstid = cst.CassetteID;
                }

                RecordCST_JOBHistory(eqp, casseqno, cstid, eJobEvent.Remove, "", inputData.TrackKey);
                //if (type == "1")
                //{
                //    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                //    if (unit == null) throw new Exception(string.Format("Can't find Unit No =[{0}) in UnitEntity!", unitNo));
                //}
                //else
                //{
                //    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2,'0'));
                //    if (port == null) throw new Exception(string.Format("Can't find Port No =[{0}) in PortEntity!", portNo));
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RemoveCSTReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RemoveCSTReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ReceiveTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ReceiveTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ReceiveTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(RemoveCSTReportReplyTimeout), trackKey);
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
        private void RemoveCSTReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] REMOVE CST DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                RemoveCSTReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Watson Modify 20150421 For Cassette Cleaner. Cassette like as Job.
        //change to public by yang
        public void RecordCST_JOBHistory(Equipment eqp, string cstSeq, string cstID, eJobEvent eventname, string trackingData, string transactionID)
        {
            try
            {

                Job cstJob = new Job(); //是錯誤資料要存入JOB HISTORY ,所以新增一個JOB，但不存檔也不會是WIP，存完即丟
                cstJob.CassetteSequenceNo = cstSeq;
                cstJob.JobSequenceNo = "1";
                cstJob.GlassChipMaskBlockID = cstID;
                cstJob.GroupIndex = "0";
                cstJob.CSTOperationMode = eqp.File.CSTOperationMode;
                cstJob.SubstrateType = eSubstrateType.Cassette;
                cstJob.CIMMode = eqp.File.CIMMode;
                cstJob.JobType = eJobType.Unknown;
                cstJob.JobJudge = string.Empty;
                cstJob.SamplingSlotFlag = string.Empty;
                cstJob.OXRInformationRequestFlag =string.Empty;
                cstJob.FirstRunFlag = string.Empty;
                cstJob.JobGrade = string.Empty;
                cstJob.PPID = string.Empty;
                cstJob.INSPReservations = string.Empty;
                cstJob.LastGlassFlag = string.Empty;
                cstJob.INSPReservations = string.Empty;
                cstJob.TrackingData = trackingData;
                cstJob.EQPFlag = string.Empty;
                cstJob.OXRInformation = string.Empty;
                cstJob.ChipCount = 0;

                ObjectManager.JobManager.RecordJobHistory(cstJob, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eventname.ToString(), transactionID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //public void RecordCstHistory2(Equipment eqp, string cstSeq, string cstID, eJobEvent eventname, string trackingData)
        //{
        //    try
        //    {
        //        // Save DB
        //        int i = 0;
        //        JOBHISTORY his = new JOBHISTORY()
        //        {
        //            UPDATETIME = DateTime.Now,
        //            EVENTNAME = eventname.ToString(),
        //            CASSETTESEQNO = int.TryParse(cstSeq, out i) == true ? i : 0,
        //            JOBSEQNO = 1,
        //            JOBID = cstID,
        //            GROUPINDEX = 0,
        //            PRODUCTTYPE = 0,
        //            CSTOPERATIONMODE = eqp.File.CSTOperationMode.ToString(),
        //            SUBSTRATETYPE = "Cassette",
        //            CIMMODE = eqp.File.CIMMode.ToString(),
        //            JOBTYPE = "",
        //            JOBJUDGE = "",
        //            SAMPLINGSLOTFLAG = "",
        //            OXRINFORMATIONREQUESTFLAG = "",
        //            FIRSTRUNFLAG = "",
        //            JOBGRADE = "",
        //            PPID = "",
        //            INSPRESERVATIONS = "",
        //            LASTGLASSFLAG = "",
        //            INSPJUDGEDDATA = "",
        //            TRACKINGDATA = trackingData,
        //            EQPFLAG = "",
        //            OXRINFORMATION = "",
        //            CHIPCOUNT = 0
        //        };
        //        ObjectManager.JobManager.InsertDB(his);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        #endregion

        #region CELL Special Function
        //T3 shisyang add 20150825
        #region[Process Start Report]
        private const string ProcessStartReportTimeout = "ProcessStartReportTimeout";
        public void ProcessStartReport(Trx inputData)
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
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    ProcessStartReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glassID = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}]  ,CassetteSequenceNo =[{4}],JobSequenceNo =[{5}],GlassID =[{6}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, cassetteSequenceNo, jobSequenceNo, glassID));
                #endregion
                ProcessStartReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);
                #region [Get Job] 
                Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);

                if (job == null)
                {
                    if ((cassetteSequenceNo == "0") || (jobSequenceNo == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, "", "".PadRight(2, '0'), "", eJobEvent.FetchOut.ToString(), inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] FETCHJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, cassetteSequenceNo, jobSequenceNo, glassID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { line.Data.LINEID, err });
                    }
                    throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!!", cassetteSequenceNo, jobSequenceNo));                    
                }
                #endregion
                #region Qtime 確認
                if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.ProcStartEvent, eqp.File.CurrentRecipeID))
                {
                    //to do
                    //EX:job.EQPFlag.QtimeFlag = true;
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ProcessStartReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ProcessStartReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ProcessStartReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ProcessStartReportTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ProcessStartReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ProcessStartReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,BoxProcessFinishReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ProcessStartReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Box Process Start Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                ProcessStartReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region[Process Finish Report]
        private const string ProcessFinishReportTimeout = "ProcessFinishReportTimeout";
        public void ProcessFinishReport(Trx inputData)
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
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    ProcessFinishReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glassID = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}]  ,CassetteSequenceNo =[{4}],JobSequenceNo =[{5}],GlassID =[{6}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, cassetteSequenceNo, jobSequenceNo, glassID));
                #endregion
                ProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);
                #region [Get Job]
                Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);

                if (job == null)
                {
                    if ((cassetteSequenceNo == "0") || (jobSequenceNo == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, "", "".PadRight(2, '0'), "", eJobEvent.FetchOut.ToString(), inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] FETCHJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, cassetteSequenceNo, jobSequenceNo, glassID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { line.Data.LINEID, err });
                    }
                    throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!!", cassetteSequenceNo, jobSequenceNo));
                }
                #endregion
                #region Qtime 確認
                if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.ProcCompEvent, eqp.File.CurrentRecipeID))
                {
                    //to do
                    //EX:job.EQPFlag.QtimeFlag = true;
                }
                #endregion
               
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ProcessFinishReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ProcessFinishReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ProcessFinishReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ProcessFinishReportTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ProcessFinishReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ProcessFinishReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,BoxProcessFinishReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ProcessFinishReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply,  Process Finish Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                ProcessFinishReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private void CELL_SendReceiveEvent_Report_Check(Trx inputData, Equipment eqp, Job job)
        {
            try
            {
                #region[Get LINE]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion

                if (line.Data.FABTYPE != eFabType.CELL.ToString())return;

                #region CELL AssemblyComplete
                AssemblyCompletReport(inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, job);
                #endregion

                #region CELL Report CUTComplete
                CUTCompleteReportAgain(inputData, line, eqp, job);
                #endregion

                #region CELL CrossLineReport
                CrossLineReportAgain(inputData, line, eqp, job);
                #endregion

                //huangjiayin 20171222
                #region T3 ODF UV MASK CHECK REPORT
                //if (inputData.Name.Contains("ReceiveJobDataReport") && eqp.Data.NODEID.Contains("CCDRC")&&job.JobType==eJobType.TFT&&job.MesProduct.OWNERID!="DUMY")
                if (inputData.Name.Contains("ReceiveJobDataReport") && eqp.Data.NODEID.Contains("CCDRC") && job.JobType == eJobType.TFT && job.MesProduct.OWNERTYPE != "OwnerD")//OWNERTYPE："OwnerD" Dummy玻璃不进行UVMaskCheck,20190808
                {
                    if (!ParameterManager.ContainsKey("ODFUVMaskCheckFlag")) return;
                    if (ParameterManager["ODFUVMaskCheckFlag"].GetBoolean())
                    {
                        object[] _data = new object[] {
                            inputData.TrackKey,
                            eqp,
                            job
                        };
                        Invoke(eServiceName.CELLSpecialService, "UVMaskCheckInEQP", _data);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Equipment=[{0}], JobKey=[{1}], JobId=[{2}] Invoke method[{3}]", eqp.Data.NODENO, job.JobKey, job.GlassChipMaskBlockID, "UVMaskCheckInEQP()"));
                    }
                }
                #endregion


                MaxCutGlassReportAgain(inputData, line, eqp, job);


            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }

        }



        #region CELL AssemblyComplete - AssembleComplete[MES]
        private void AssemblyCompletReport(string trxID, string lineName, string eqpID, Job job)
        {
            try
            {
                if (!(lineName.Contains(keyCellLineType.ODF))) return;//sy add 20160907
                if (!eqpID.Contains(keyCELLMachingName.CCVAC)) return;
                #region [T2 會影響 先Mark]
                //if (!lineName.Contains(eLineType.CELL.CBODF))
                //    return;
                //if (!eqpID.Contains(keyCELLMachingName.CBSUV))
                //    return;
                #endregion
                if (job.CellSpecial.AssemblyCompleteFlag == eBitResult.ON) return;
                if ((job.CellSpecial.CFCassetteSeqNo == "0") || (job.CellSpecial.CFJobSeqNo == "0")) return;
                if ((job.CellSpecial.CFCassetteSeqNo == string.Empty) || (job.CellSpecial.CFJobSeqNo == string.Empty)) return;
                if (job.JobType != eJobType.TFT) return;

                #region Send Cell Special AssemblyComplate Report to MES Data
                //AssemblyReport(string trxID, string lineName, string nodeID, string tftCasSeqNo, string tftJobSeqNo, string cfCasSeqNo, string cfJobSeqNo)
                object[] _data = new object[8]
                    { 
                        trxID,  /*0 TrackKey*/
                        lineName,    /*1 LineName*/
                        eqpID, /*2 EQP ID */
                        job.CassetteSequenceNo,/*3 tftCasSeqNo */
                        job.JobSequenceNo,/*4 tftJobSeqNo */
                        job.CellSpecial.CFCassetteSeqNo,/*5 cfCasSeqNo */
                        job.CellSpecial.CFJobSeqNo,/*6cfJobSeqNo */
                        true
                    };
                //string trxID, string lineName, Equipment eqp,Job job, string recoveReasonCode)
                object retVal = base.Invoke(eServiceName.CELLSpecialService, "AssemblyReport", _data);
                #endregion
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        #endregion
        #region CELL Cross Line Report - MaxCutGlassProcessEndReply[MES]
        private void MaxCutGlassReportAgain(Trx inputData, Line line, Equipment eqp, Job job)
        {
            try
            {

                if (job.CellSpecial.MaxCutReportFlag != eBitResult.OFF)
                    return;

                //CUT Line 專用
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CBCUT_3:
                        #region CUT500 [TCV -> OCV Send Out Job Data Event#05,#06 or OCV <- TCV Receive Job Data Event]
                        if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBTCV)) //TCV  -> OCV 
                        {
                            if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                            {
                                if (((inputData.Name.Contains("#05")) || (inputData.Name.Contains("#06"))))     //TCV SendOut#5 or SendOut#6 一定是TCV to OCV
                                {
                                    break;
                                }
                            }
                        }

                        if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBOCV))  //OCV <- TCV
                        {
                            if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))  // OCV Receive 一定是跨線
                            {
                                break;
                            }
                        }
                        return;
                        #endregion
                    default:
                        return;
                }

                job.CellSpecial.MaxCutReportFlag = eBitResult.ON;
                job.LineRecipeName = job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].LINERECIPENAME;
                job.MesProduct.PPID = job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID;
                job.MES_PPID = job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID; //TODO 待處理Local Mode的MES PPID


                #region [BC -> MES  MaxCutGlassProcessEnd]
                object[] _data = new object[4]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp,                /* 2 Equipment Entity */
                    job,               /*3 WIP Data */
                };
                base.Invoke(eServiceName.MESService, "MaxCutGlassProcessEnd", _data);
                #endregion


                #region [CUT -> CUT  Glass Cross Line Socket Transfer to other BC]
                lock (job.CellSpecial)
                {
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        #endregion
        #region CELL Cross Line Report - CrossLineReportAgain[MES]
        private void CrossLineReportAgain(Trx inputData, Line line, Equipment eqp, Job job)
        {
            try
            {
                //sy add for T3 //先判斷job是否 有去有回 
                if (job.CellSpecial.CrossLineReportFlag == eBitResult.ON && job.CellSpecial.CrossLineBackReportFlag == eBitResult.ON) return;
                //sy add for T3//Line1->2=>1 確認 有去無回時才判斷
                if (job.CellSpecial.CrossLineReportFlag == eBitResult.ON && job.CellSpecial.CrossLineBackReportFlag == eBitResult.OFF)
                {
                    switch (line.Data.LINETYPE)
                    {
                        #region [T3 USE]
                        case eLineType.CELL.CCPIL:
                        case eLineType.CELL.CCPIL_2:
                            #region [CCPAA=>CCPIT3]
                            if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPPA))
                            {
                                if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                                {
                                    if (inputData.Name.Contains("#05") || inputData.Name.Contains("#06"))
                                    {
                                        //string crossLineName = CrossLineMcMapping(line.Data.LINEID.ToString());
                                        #region [BC -> MES  GlassProcessLineChanged]
                                        object[] _data = new object[4]
                                    { 
                                        inputData.TrackKey,  /*0 TrackKey*/
                                        eqp.Data.LINEID,    /*1 LineName*/
                                        line.Data.NEXTLINEID,                /* 2 NextLineName */
                                        job,               /*3 WIP Data */
                                    };
                                        //(string trxID, string lineName, string crosslineName, Job job)
                                        base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                                        break;
                                        #endregion
                                    }
                                }
                            }

                            if (eqp.Data.NODENO.Contains("L14"))
                            {
                                if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                                {
                                    if (inputData.Name.Contains("#06") || inputData.Name.Contains("#07"))
                                    {
                                        //string crossLineName = CrossLineMcMapping(line.Data.LINEID.ToString());
                                        #region [BC -> MES  GlassProcessLineChanged]
                                        object[] _data = new object[4]
                                    { 
                                        inputData.TrackKey,  /*0 TrackKey*/
                                        eqp.Data.LINEID,    /*1 LineName*/
                                        line.Data.NEXTLINEID,                /* 2 NextLineName */
                                        job,               /*3 WIP Data */
                                    };
                                        //(string trxID, string lineName, string crosslineName, Job job)
                                        base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                                        break;
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                            return;
                        #endregion
                        default:
                            return;
                    }
                    job.CellSpecial.CrossLineBackReportFlag = eBitResult.ON;

                    #region [2->1   Glass Cross Line Socket Transfer to other BC]
                    lock (job.CellSpecial)
                    {
                        ObjectManager.JobManager.EnqueueSave(job);
                    }

                    object[] _objJob = new object[3] { line.Data.LINEID, eqp.Data.NODEID, job };
                    base.Invoke(eServiceName.ActiveSocketService, "CellShortCut", _objJob);
                    #endregion
                }

                if (job.CellSpecial.CrossLineReportFlag == eBitResult.ON) return;

                //Line1=>2
                switch (line.Data.LINETYPE)
                {
                    #region [T3 USE]
                    case eLineType.CELL.CCPIL://sy add for T3
                    case eLineType.CELL.CCPIL_2:
                        #region [CCPIT3=>CCPAA]
                        if (eqp.Data.NODENO.Contains("L14"))
                        {
                            if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                            {
                                if (inputData.Name.Contains("#07") || inputData.Name.Contains("#08"))
                                {
                                    //string crossLineName = CrossLineMcMapping(line.Data.LINEID.ToString());
                                    #region [BC -> MES  GlassProcessLineChanged]
                                    object[] _data = new object[4]
                                    { 
                                        inputData.TrackKey,  /*0 TrackKey*/
                                        eqp.Data.LINEID,    /*1 LineName*/
                                        line.Data.NEXTLINEID,                /* 2 NextLineName */
                                        job,               /*3 WIP Data */
                                    };
                                    //(string trxID, string lineName, string crosslineName, Job job)
                                    base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                                    break;
                                    #endregion
                                }
                            }
                        }

                        if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPPA))
                        {
                            if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                            {
                                if (inputData.Name.Contains("#05") || inputData.Name.Contains("#06"))
                                {
                                    //string crossLineName = CrossLineMcMapping(line.Data.LINEID.ToString());
                                    #region [BC -> MES  GlassProcessLineChanged]
                                    object[] _data = new object[4]
                                    { 
                                        inputData.TrackKey,  /*0 TrackKey*/
                                        eqp.Data.LINEID,    /*1 LineName*/
                                        line.Data.NEXTLINEID,                /* 2 NextLineName */
                                        job,               /*3 WIP Data */
                                    };
                                    //(string trxID, string lineName, string crosslineName, Job job)
                                    base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                                    break;
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        return;
                    #endregion
                    #region [T2 USE]
                    //case eLineType.CELL.CBCUT_1:
                    //    #region CUT100~300 [BUR -> MMG Send Out Job Data Event#02 or MMG <- BUR Receive Job Data Event#01]
                    //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBBUR)) //BUR -> MMG Send Out Job Data Event#02
                    //    {
                    //        if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                    //        {
                    //            if (inputData.Name.Contains("#02"))
                    //            {
                    //                #region [BC -> MES  GlassProcessLineChanged]
                    //                object[] _data = new object[4]
                    //                { 
                    //                    inputData.TrackKey,  /*0 TrackKey*/
                    //                    eqp.Data.LINEID,    /*1 LineName*/
                    //                    "CBCUT500",                /* 2 Equipment Entity */
                    //                    job,               /*3 WIP Data */
                    //                };
                    //                //(string trxID, string lineName, string crosslineName, Job job)
                    //                base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                    //                #endregion
                    //                break;
                    //            }
                    //        }
                    //    }

                    //    if (eqp.Data.NODENO == "L9")  //MMG <- BUR Receive Job Data Event#01
                    //    {
                    //        if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                    //        {
                    //            if (inputData.Name.Contains("#01"))
                    //            {
                    //                #region [BC -> MES  GlassProcessLineChanged]
                    //                object[] _data = new object[4]
                    //                { 
                    //                    inputData.TrackKey,  /*0 TrackKey*/
                    //                    eqp.Data.LINEID,    /*1 LineName*/
                    //                    "CBCUT500",                /* 2 Equipment Entity */
                    //                    job,               /*3 WIP Data */
                    //                };
                    //                //(string trxID, string lineName, string crosslineName, Job job)
                    //                base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                    //                #endregion
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    #endregion
                    //    return;
                    //case eLineType.CELL.CBCUT_2:
                    //    #region CUT400 [CUT -> MMG Send Out Job Data Event#02 or MMG <- CUT Receive Job Data Event#01]
                    //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBCUT)) //CUT -> MMG Send Out Job Data Event#02
                    //    {
                    //        if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                    //        {
                    //            if (inputData.Name.Contains("#02"))
                    //            {
                    //                #region [BC -> MES  GlassProcessLineChanged]
                    //                object[] _data = new object[4]
                    //                { 
                    //                    inputData.TrackKey,  /*0 TrackKey*/
                    //                    eqp.Data.LINEID,    /*1 LineName*/
                    //                    "CBCUT500",                /* 2 Equipment Entity */
                    //                    job,               /*3 WIP Data */
                    //                };
                    //                //(string trxID, string lineName, string crosslineName, Job job)
                    //                base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                    //                #endregion
                    //                break;
                    //            }
                    //        }
                    //    }

                    //    if (eqp.Data.NODENO == "L9")  //MMG <- CUT Receive Job Data Event#01
                    //    {
                    //        if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                    //        {
                    //            if (inputData.Name.Contains("#01"))
                    //            {
                    //                #region [BC -> MES  GlassProcessLineChanged]
                    //                object[] _data = new object[4]
                    //                { 
                    //                    inputData.TrackKey,  /*0 TrackKey*/
                    //                    eqp.Data.LINEID,    /*1 LineName*/
                    //                    "CBCUT500",                /* 2 Equipment Entity */
                    //                    job,               /*3 WIP Data */
                    //                };
                    //                //(string trxID, string lineName, string crosslineName, Job job)
                    //                base.Invoke(eServiceName.MESService, "GlassProcessLineChanged", _data);
                    //                #endregion
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    #endregion
                    //    return;
                    #endregion
                    default:
                        return;
                }
                job.CellSpecial.CrossLineReportFlag = eBitResult.ON;
                #region [CUT -> CUT   Glass Cross Line Socket Transfer to other BC]
                lock (job.CellSpecial)
                {
                    ObjectManager.JobManager.EnqueueSave(job);
                }

                object[] _objJob1 = new object[3] { line.Data.LINEID, eqp.Data.NODEID, job };
                base.Invoke(eServiceName.ActiveSocketService, "CellShortCut", _objJob1);
                #endregion
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        //sy add 20151202 暫定
        private string CrossLineMcMapping(string line)
        {
            string returnLine = string.Empty;
            switch (line)
            {
                case "CCPIL100":
                    returnLine = "CCPIL200";
                    break;
                case "CCPIL200":
                    returnLine = "CCPIL100";
                    break;
                case "CCPIL300":
                    returnLine = "CCPIL400";
                    break;
                case "CCPIL400":
                    returnLine = "CCPIL300";
                    break;
                default:
                    break;
            }
            return returnLine;
        }
        #endregion
        #region CELL Glass Cutting Start -CUTCompleteReport [MES]
        private void CUTCompleteReportAgain(Trx inputData, Line line, Equipment eqp, Job job)
        {
            try
            {
                #region [T3]
                //eBitResult CompleteReportAgainFlag = eBitResult.OFF;
                //if (line.Data.LINETYPE.Contains(eLineType.CELL.CCPCS))
                //{
                //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPCS) && inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                //        CompleteReportAgainFlag = eBitResult.ON;
                //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPCC) && inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                //        CompleteReportAgainFlag = eBitResult.ON;
                //}
                //if (line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_1)||line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_2)||
                //    line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_3)||line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_4)||
                //    line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_5)||line.Data.LINETYPE.Contains(eLineType.CELL.CCCUT_6))
                //{
                //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCCUT) && inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                //        CompleteReportAgainFlag = eBitResult.ON;
                //    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCBEV) && inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                //        CompleteReportAgainFlag = eBitResult.ON;
                //}              
                //#region [CompleteReportAgainFlag ON]
                //if (CompleteReportAgainFlag == eBitResult.ON)
                //{
                    Invoke(eServiceName.CELLSpecialService, "CUTCompleteReportAgain", new object[] { inputData, line, eqp, job });
                    //int jobSeq;
                    //int.TryParse(job.JobSequenceNo, out jobSeq);
                    //Job monJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, (jobSeq / 1000).ToString());
                    //if (monJob != null && monJob.CellSpecial.CutCompleteFlag != eBitResult.ON)
                    //{
                    //    List<Job> JobList = new List<Job>();
                    //    object[] _data1 = new object[4]
                    //            { 
                    //                inputData.TrackKey,  /*0 TrackKey*/
                    //                eqp.Data.LINEID,    /*1 LineName*/
                    //                monJob,               /*2 WIP Data */
                    //                JobList
                    //            };
                    //    Invoke(eServiceName.CELLSpecialService, "Create_CELL_ChipPanel", new object[] { eqp, monJob, monJob.ChipCount, JobList, false });
                    //    Invoke(eServiceName.CELLSpecialService, "CUTLine_MES_CUTCompleteReport", _data1);
                    //}
                //}
                //#endregion
                #endregion
                #region [T2]
                if (line.Data.LINETYPE.Contains(eLineType.CELL.CBCUT_1) || line.Data.LINETYPE.Contains(eLineType.CELL.CBCUT_2)
                    || line.Data.LINETYPE.Contains(eLineType.CELL.CBCUT_3))
                {
                    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBCUT))
                    {
                        if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                        {
                            int jobSeq;
                            int.TryParse(job.JobSequenceNo, out jobSeq);
                            Job monJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, (jobSeq / 1000).ToString());
                            if (monJob != null && monJob.CellSpecial.CutCompleteFlag != eBitResult.ON)
                            {
                                object[] _data1 = new object[3]
                                { 
                                    inputData.TrackKey,  /*0 TrackKey*/
                                    eqp.Data.LINEID,    /*1 LineName*/
                                    monJob,               /*2 WIP Data */
                                };
                                base.Invoke(eServiceName.CELLSpecialService, "CUTLine_MES_CUTCompleteReport", _data1);
                                base.Invoke(eServiceName.CELLSpecialService, "Create_CELL_ChipPanel", new object[] { eqp, monJob, monJob.ChipCount, false });
                            }
                        }
                    }

                    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBBUR))
                    {
                        if (inputData.Name.Contains(keyEQPEvent.ReceiveJobDataReport))
                        {
                            int jobSeq;
                            int.TryParse(job.JobSequenceNo, out jobSeq);
                            Job monJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, (jobSeq / 1000).ToString());
                            if (monJob != null && monJob.CellSpecial.CutCompleteFlag != eBitResult.ON)
                            {
                                object[] _data1 = new object[3]
                                { 
                                    inputData.TrackKey,  /*0 TrackKey*/
                                    eqp.Data.LINEID,    /*1 LineName*/
                                    monJob,               /*2 WIP Data */
                                };
                                base.Invoke(eServiceName.CELLSpecialService, "CUTLine_MES_CUTCompleteReport", _data1);
                                base.Invoke(eServiceName.CELLSpecialService, "Create_CELL_ChipPanel", new object[] { eqp, monJob, monJob.ChipCount, false });
                            }
                        }
                    }
                #endregion
                }
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        #endregion
        #region CELL Defect Code Report - ProductInspectionDataReport [MES]
        private void CELLDefectCodeReport(Trx inputData, Line line, Equipment eqp, Job job)
        {
            try
            {
                #region [T2 USE]
                //PIL: PMI
                if (line.Data.LINETYPE == eLineType.CELL.CBPIL)
                {
                    if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBPMI))
                        return;
                }

                //ODF: MAI, LCI                
                if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                {
                    if (!((eqp.Data.NODEID.Contains(keyCELLMachingName.CBMAI)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CBLCI))))
                        return;
                }
                //HVA: AOI
                if (line.Data.LINETYPE == eLineType.CELL.CBHVA)
                {
                    if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBAOI))
                        return;
                }
                //PIS: PIS
                if (line.Data.LINETYPE == eLineType.CELL.CBPIS)
                {
                    if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBPIS))
                        return;
                }
                //GMO: GMO
                if (line.Data.LINETYPE == eLineType.CELL.CBGMO)
                {
                    if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBGMO))
                        return;
                }
                #endregion
                #region [T3 USE]
                //CCPIL: BPI,PIN          
                if (line.Data.LINETYPE == eLineType.CELL.CCPIL || line.Data.LINETYPE == eLineType.CELL.CCPIL_2)
                {
                    if (!((eqp.Data.NODEID.Contains(keyCELLMachingName.CCBPI)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPIN))))
                        return;
                }
                //CCODF:SLI,MAI,LCI 
                if (line.Data.LINETYPE.Contains(keyCellLineType.ODF))//sy add 20160907
                {
                    if (!((eqp.Data.NODEID.Contains(keyCELLMachingName.CCSLI)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCMAI)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCLCI))))
                        return;
                }
                //CCCUT:BUR,TST 
                if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))
                {
                    if (!((eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCTST))))
                        return;
                }
                //CCRWT:RWT 
                if (line.Data.LINETYPE == eLineType.CELL.CCRWT)
                {
                    if (!((eqp.Data.NODEID.Contains(keyCELLMachingName.CCRWT)) || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCRWT))))
                        return;
                }
                #endregion
                base.Invoke(eServiceName.MESService, "ProductInspectionDataReport", new object[] { inputData.TrackKey, eqp, job });
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        #endregion
        #region CELL BoxIDRequest Check Again[MES]
        public void BoxIDRequestCheck(Trx inputData, Line line, Equipment eqp, Job job,Port port)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)return;
                if (!line.Data.LINENAME.Contains(eLineType.CELL.CCPCS)) return;
                if (port ==null)return;
                if (port.File.Type != ePortType.UnloadingPort) return;
                if (!inputData.Name.Contains(keyEQPEvent.StoreJobDataReport)) return;
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                if (cst == null) return;
                if (cst.BoxName !=string.Empty) return;
                if (cst.IsBoxed == true) return;
                cst.IsBoxed = true;
                #region Add NodeNo Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string rep = inputData.Metadata.NodeNo;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region Add Port Key
                string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                string Portrep = port.Data.PORTNO;
                if (Repository.ContainsKey(Portkey))
                    Repository.Remove(Portkey);
                Repository.Add(Portkey, Portrep);
                #endregion
                Invoke(eServiceName.MESService, "BoxIdCreateRequest", new object[4] { inputData.TrackKey, eqp.Data.LINEID, job.GlassChipMaskBlockID, "DPBox" });

            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        #endregion
        #region For CELL Robort AUTO Abort CASSETTE BY CHECK FETCH LD PORT  OR CHECK STORE BOTH PORT.
        private void CELL_Robot_FetchStoreEvent_AutoAbortCST(Trx inputData, Equipment eqp, Port port, Job job)
        {
            try
            {
                if (port == null)
                    return;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line.Data.FABTYPE != eFabType.CELL.ToString())
                    return;

                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) || (line.Data.LINETYPE == eLineType.CELL.CBPRM) ||
                    (line.Data.LINETYPE == eLineType.CELL.CBSOR_1) || (line.Data.LINETYPE == eLineType.CELL.CBSOR_2))
                {


                    #region CELL Loader Port
                    if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.NORMAL)
                    {
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (!inputData.Name.Contains(keyEQPEvent.FetchOutJobDataReport))
                                return;
                            //Watson Modify 20150320 For LD Port Count =0 不需要由bc來退卡
                            int count = 0;
                            int.TryParse(port.File.JobCountInCassette, out count);
                            if (count == 0)
                            {
                                string err = string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] CASETTE_ID=[{3}] COUNT=[{4}] BC DOESN'T ABORT CASSETTE.",
                                  eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteStatus.ToString(), port.File.CassetteID.Trim(), count);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                return;
                            }
                            if ((port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING) || (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING))
                            {
                                IList<Job> jobs = new List<Job>();
                                GetPortJobData(eqp, port, ref jobs);

                                //Watson 20150321 Modify 撈出所有待抽的片，但是把Fetch out的這片排除，避免機台還沒清除
                                count = jobs.Where(j => (j.SamplingSlotFlag == "1") && (j.GlassChipMaskBlockID != job.GlassChipMaskBlockID)).ToList().Count;
                                if (count > 0)
                                {
                                    string err = string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] CASSETTE_ID=[{3}] BC DOESN'T ABORT CASSETTE. " +
                                        "SAMPLING_SLOT_FLAG ='1' OF GLASS COUNT=[{4}] IN CASSETTE.",
                                    eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteStatus.ToString(), port.File.CassetteID.Trim(), count);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    return;
                                }
                                else
                                {
                                    string err = string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] BC ABORT CASSETTEID=[{3}] REASON={4}.",
                                      eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteStatus.ToString(), port.File.CassetteID.Trim(),
                                      " FETCH EVENT CHECK SAMPLING_SLOT_FLAG = '0' ,NO WAIT FOR PROCESS GLASS IN CASSETTE.");
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, line.Data.LINEID, err });
                                    Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                                    return;
                                }
                            }
                        }
                    }
                    #endregion

                    //#region CELL Both Port
                    //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.NORMAL)
                    //{
                    //    if (port.File.Type == ePortType.BothPort)
                    //    {
                    //        if (!inputData.Name.Contains(keyEQPEvent.StoreJobDataReport))
                    //            return;

                    //        job.RobotProcessFlag = keyCELLROBOTProcessFlag.ROBOT_PROCESSEND;

                    //        if ((port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING) || (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING))
                    //        {
                    //            IList<Job> jobs = new List<Job>();
                    //            GetPortJobData(eqp, port, ref jobs);
                    //            if (jobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count > 0)
                    //                return;

                    //            //卡匣內已處理的片數
                    //            int completeCount = jobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.ROBOT_PROCESSEND).ToList().Count;
                    //            //線內被移除的片數 符合cassette seq no
                    //            int removeCount = ObjectManager.JobManager.GetJobs().Where(j => j.CassetteSequenceNo == port.File.CassetteSequenceNo
                    //                    && j.RemoveFlag == true).ToList().Count;

                    //            if (completeCount+removeCount == port.File.RobotWaitProcCount)
                    //            {
                    //                string err = string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] BC ABORT CASSETTEID=[{3}]",eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteStatus.ToString(), port.File.CassetteID.Trim());
                    //                err +=  string.Format(" STORE EVENT CHECK SAMPLING_SLOT_FLAG = '0' GLASS COUNT=[{0}] MISMATCH PROCESSED GLASS COUNT=[{1}].",port.File.RobotWaitProcCount, completeCount);
                    //                if (removeCount > 0)
                    //                    err += string.Format(" AND REMOVE GLASS COUNT=[{0}].", removeCount);

                    //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    //                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, line.Data.LINEID, err });
                    //                Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                    //            }
                    //        }
                    //    }
                    //}
                    //#endregion

                }

            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }

        }

        //取得CASSETTE內的玻璃資料
        private void GetPortJobData(Equipment eqp, Port port, ref IList<Job> jobs)
        {
            try
            {
                //"PLC_Port#01CassetteStatusChangeReport"
                string strName = string.Format("{0}_Port#{1}CassetteStatusChangeReport", eqp.Data.NODENO, port.Data.PORTNO.PadLeft(2, '0'));
                bool bolRecodeLog = ParameterManager["CELL_PMT_AUTO_CANCEL_CST_LOG"].GetBoolean();
                Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, bolRecodeLog }) as Trx;

                for (int i = 0; i < trx.EventGroups[0].Events[0].Items.Count; i += 2)
                {
                    // Cassette Sequence No 為0時, 表示沒有資料, 找下一片
                    if (trx.EventGroups[0].Events[0].Items[i].Value.Equals("0")) continue;
                    // Job Sequence No 為0時, 表示沒有資料, 找下一片
                    if (trx.EventGroups[0].Events[0].Items[i + 1].Value.Equals("0")) continue;

                    Job job = ObjectManager.JobManager.GetJob(trx.EventGroups[0].Events[0].Items[i].Value, trx.EventGroups[0].Events[0].Items[i + 1].Value);

                    if (job == null)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] JOB IS NULL , CAS_SEQ_NO=[{3}], JOB_SEQ_NO=[{4}] .",
                            trx.Metadata.NodeNo, port.Data.PORTNO, port.File.CassetteStatus.ToString(), trx.EventGroups[0].Events[0].Items[i].Value,
                            trx.EventGroups[0].Events[0].Items[i + 1].Value));
                        continue;
                    }
                    jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region ARRAY ELA Short CUT
        //private void ARRAY_ShortCutCreateWIP(Line line, Equipment eqp, string unitNo,Job job,string trxId)
        //{
        //    if (line.Data.LINETYPE != eLineType.ARRAY.ELA_JSW) return;  //add by bruce 2015/09/30 Array ELA 跨line 使用
        //    if (eqp.Data.NODENO == "L2" && unitNo == "2") //Indexer Turn Table
        //    {
        //        IServerAgent activeSocketAgent = null;
        //        IServerAgent passiveSocketAgent = null;
        //        activeSocketAgent = GetServerAgent("ActiveSocketAgent");
        //        passiveSocketAgent = GetServerAgent("PassiveSocketAgent");
        //        object[] _objJob = new object[1] { job };
        //        switch (line.Data.LINEID)
        //        { 
        //            case "TCELA100":
        //                Invoke(eServiceName.PassiveSocketService, "ArrayShortCut", _objJob);
        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
        //                break;
        //            case "TCELA200":
        //                Invoke(eServiceName.ActiveSocketService, "ArrayShortCut", _objJob);
        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
        //                break;
        //            default :
        //                break ;
        //        }
        //    }
        //}
        #endregion
    }
}
