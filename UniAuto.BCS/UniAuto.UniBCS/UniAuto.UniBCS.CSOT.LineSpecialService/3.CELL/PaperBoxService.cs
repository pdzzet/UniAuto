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

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public class PaperBoxService : AbstractService
    {
        public override bool Init()
        {
            return true;
        }
        #region [PaperBoxLineInReport]
        private const string PaperBoxLineInTimeout = "PaperBoxLineInTimeout";
        /// <summary>
        /// Paper Box Line In Report For PPK: 1.PaperBox in Car ; 2.Box in port before packing
        /// </summary>
        /// <param name="inputData">PLC Data</param>
        public void PaperBoxLineInReport(Trx inputData)
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
                   
                    PaperBoxLineInReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[1].Value.PadLeft(2,'0');
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PaperBoxLineInReport BoxID =[{2}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                    boxID.Trim()));
                #endregion
                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null && portNo != "00") 
                    throw new Exception(string.Format("Can't find Port No =[{0}] in PortEntity!", portNo));
                else if (port != null)
                {
                    lock (port)
                    {
                        port.File.PortPackMode = ePalletMode.PACK;
                        port.File.PortDBDataRequest = "0";
                        port.File.PortUnPackSource = "0";
                        //port.File.PortDBDataRequest = ((int)bitResult).ToString();
                        port.File.Mes_ValidateBoxReply = string.Empty;
                        port.File.PortBoxID1 = boxID.Trim();
                        port.File.PortBoxID2 = string.Empty;
                    }
                }//依設定 回機台OK/NG
                eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, boxID, eBoxType.NODE.ToString(), 
                    "2", "", portNo, "", "", "", "", "", "",line.File.HostMode.ToString());
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    //Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                    PaperBoxLineInReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, rtncode);
                    return;
                }
                #endregion
                #region MES Data ValidateBoxRequest
                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                if (port != null)
                {
                    object[] _data = new object[3]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            port,                     /*1 PortID*/
                            boxID                      /*2 boxId or paperBoxID by MES check*/
                        };

                    object retVal = base.Invoke(eServiceName.MESService, "ValidateBoxRequest_CCPPK", _data);
                }
                else
                {
                    object[] _data = new object[3]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp,
                            boxID                      /*2 boxId or paperBoxID by MES check*/
                        };

                    object retVal = base.Invoke(eServiceName.MESService, "ValidateBoxRequest_CCPPK", _data);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PaperBoxLineInReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, eReturnCode1.NG);
            }
        }
        /// <summary>
        /// Paper Box Line In Report Reply
        /// </summary>
        /// <param name="eqpNo">eqp No</param>
        /// <param name="value">Bit on/off</param>
        /// <param name="trackKey">Trx</param>
        /// <param name="rtncode">Return Code:Online By MES Data Reply:Offline By PaperBoxLineInReport to Reply</param>
        public void PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo +  "_PaperBoxLineInReportReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxLineInTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxLineInTimeout);
                    }
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PaperBoxLineInReportReply Set Bit =[{2}).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion
                    return;
                }
                #endregion
                #region[MES Data ] 
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxLineInTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxLineInTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + PaperBoxLineInTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PaperBoxLineInReportReplyTimeout), trackKey);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PaperBoxLineInReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", "", "", eBoxType.NODE.ToString(), 
                    "2", "", "", "", "", "", rtncode.ToString(), "", "",line.File.HostMode.ToString());
                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PaperBoxLineInReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Paper Box Line In Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PaperBoxLineInReportReply(sArray[0], eBitResult.OFF, trackKey,eReturnCode1.Unknown);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region[PaperBoxLabelInformationRequest]
        private const string PaperBoxLabelInfoReqTimeout = "PaperBoxLabelInfoReqTimeout";
        /// <summary>
        /// Paper Box Label Information Request : Request to MES
        /// </summary>
        /// <param name="inputData">PLC Data</param>
        public void PaperBoxLabelInformationRequest(Trx inputData)
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
                    PaperBoxLabelInformationRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string paperBoxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string paperBoxType = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string boxID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string boxType = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PaperBoxLabelInformationRequest paperBoxID =[{4}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, paperBoxID));
                #endregion
                //依設定 回機台OK/NG
                eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID,
                    eBoxType.OutBox.ToString(), "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    PaperBoxLabelInformationRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtncode, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion
                #region [MES Data]
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK)
                {
                    object[] _data = new object[8]
                { 
                    inputData.TrackKey,  
                    eqp.Data.LINEID,    
                    eqp.Data.NODENO,
                    eqp.Data.NODEID,
                    paperBoxID, /*BOXNAME*/
                    "OutBox",   /*BOXNAME*/
                    boxID,         /*SUBBOXNAME*/
                    "InBox"     /*BOXTYPE*/
                };
                    Invoke(eServiceName.MESService, "PaperBoxLabelInformationRequest", _data);
                }
                else //PCK USE//MES 要求 InBox 不用報SUBBOX
                {
                    object[] _data1 = new object[8]
                { 
                    inputData.TrackKey,  
                    eqp.Data.LINEID,    
                    eqp.Data.NODENO,
                    eqp.Data.NODEID,
                    boxID,   /*BOXNAME*/
                    "InBox",   /*BOXTYPE*/
                    "",      /*SUBBOXNAME*/
                    ""     /*BOXTYPE*/
                };
                    Invoke(eServiceName.MESService, "PaperBoxLabelInformationRequest", _data1);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PaperBoxLabelInformationRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
            }
        }
        /// <summary>
        /// Paper Box Label Information Request Reply
        /// </summary>
        /// <param name="eqpNo">eqp No</param>
        /// <param name="value">Bit On/Off</param>
        /// <param name="trackKey">Trx</param>
        /// <param name="rtncode">Return Code</param>
        /// <param name="reply">Data :Offline Null : Online MES Data Download</param>
        //public void PaperBoxLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string paperBoxID,string modelName ,string modelVersion ,string cartonQuantity)
        public void PaperBoxLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string itemName1, string itemValue1,
        string itemName2, string itemValue2, string itemName3, string itemValue3, string itemName4, string itemValue4, string itemName5, string itemValue5,
            string itemName6, string itemValue6, string itemName7, string itemValue7, string itemName8, string itemValue8, string itemName9, string itemValue9,
            string itemName10, string itemValue10, string itemName11, string itemValue11)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PaperBoxLabelInformationRequestReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PaperBoxLabelInformationRequestReply Set Bit =[{2}] ).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxLabelInfoReqTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxLabelInfoReqTimeout);
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
                if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxLabelInfoReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxLabelInfoReqTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + PaperBoxLabelInfoReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PaperBoxLabelInformationRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                string datalog = string.Format("SHIPID=[{0}]:[{1}],QUANTITY=[{2}]:[{3}],BOMVERSION=[{4}]:[{5}],MODELNAME=[{6}]:[{7}],MODELVERSION=[{8}]:[{9}],ENVIRONMENTFLAG=[{10}]:[{11}],PARTID=[{12}]:[{13}],CARRIERNAME=[{14}]:[{15}],NOTE=[{16}]:[{17}],COUNTRY=[{18}]:[{19}],WT=[{20}]:[{21}]",
                   itemName1, itemValue1, itemName2, itemValue2, itemName3, itemValue3, itemName4, itemValue4, itemName5, itemValue5, itemName6, itemValue6, itemName7, itemValue7, itemName8, itemValue8, itemName9, itemValue9, itemName10, itemValue10, itemName11, itemValue11);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PaperBoxLabelInformationRequestReply Set Bit =[{2}],ReturnCode =[{3}],[{4}]).",
                    eqpNo, trackKey, value.ToString(), rtncode.ToString(), datalog));
                #endregion
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", "", "",
                    eBoxType.OutBox.ToString(), "", "", "", "", "", "", rtncode.ToString(), "", "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PaperBoxLabelInformationRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PaperBoxLabelInformationRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PaperBoxLabelInformationRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region[FetchOutPaperBoxReport]
        private const string FetchOutPaperBoxTimeout = "FetchOutPaperBoxTimeout";
        public void FetchOutPaperBoxReport(Trx inputData)
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

                    FetchOutPaperBoxReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string paperBoxID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string boxorPaperBox = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string stageorPortorPalletorCar = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletNo = inputData.EventGroups[0].Events[0].Items[6].Value.PadLeft(2, '0');
                string carNo = inputData.EventGroups[0].Events[0].Items[7].Value.PadLeft(2, '0');
                #endregion
                //20151209 sy
                Port port = null; Cassette cst = null;Pallet pallet = null;
                #region [Check Box data]
                if (boxorPaperBox == "1")
                {
                    cst = ObjectManager.CassetteManager.GetCassette(boxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! INBOX", boxID));
                }
                else
                {
                    cst = ObjectManager.CassetteManager.GetCassette(paperBoxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! OUTBOX", paperBoxID));
                }                
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,BoxID =[{4}] ,PaperBoxID =[{5}] ,Fecth Out stageorPortorPalletorCar =[{6}],stageNo =[{7}] ,portNo =[{8}] ,palletNo =[{9}],carNo =[{10}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID, paperBoxID,
                    stageorPortorPalletorCar, stageNo, portNo, palletNo, carNo));
                #endregion
                FetchOutPaperBoxReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);
                if (stageorPortorPalletorCar == "2" && portNo.PadLeft(2, '0') != "00")
                    port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));
                if (port != null)
                    port.File.CassetteID = boxorPaperBox == "1" ? boxID : paperBoxID;
                if (stageorPortorPalletorCar == "3" && palletNo.PadLeft(2, '0') != "00")                    
                    pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo.PadLeft(2, '0'));
                //insertDB
                //RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, string.Empty, string.Empty);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID, 
                    boxorPaperBox=="1"?eBoxType.InBox.ToString():eBoxType.OutBox.ToString(),stageorPortorPalletorCar, stageNo, portNo,
                    pallet == null ? "" : pallet.File.PalletID, palletNo, carNo, "", "", "", line.File.HostMode.ToString());
                #region [Updata UI]
                if (port != null)
                {
                    lock (port)
                    {
                        port.File.PortBoxID1 = "";
                        port.File.PortBoxID2 = "";
                        port.File.Status = ePortStatus.UC;
                    }
                    if (port.Data.PORTATTRIBUTE != keyCELLPORTAtt.PALLET)//20161102 sy modify
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                }
                #endregion
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                #endregion
                #region[BoxProcessStarted]
                //if (stageorPortorPalletorCar == "2" && boxorPaperBox == "1" && portNo != "00")
                //{
                //    Invoke(eServiceName.MESService, "BoxProcessStarted", new object[] { inputData.TrackKey, line, boxID });
                //}
                //if (stageorPortorPalletorCar == "2" && boxorPaperBox == "2" && portNo != "00")
                //{                    
                //    if (port != null)
                //        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX_MANUAL )
                //            Invoke(eServiceName.MESService, "BoxProcessStarted", new object[] { inputData.TrackKey, line, paperBoxID });
                //}
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    FetchOutPaperBoxReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void FetchOutPaperBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_FetchOutPaperBoxReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;

                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + FetchOutPaperBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + FetchOutPaperBoxTimeout);
                }                
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + FetchOutPaperBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(FetchOutPaperBoxReportReplyTimeout), trackKey);  
                }
                #endregion              
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,FetchOutPaperBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void FetchOutPaperBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, FetchOutPaperBoxReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));
                
                FetchOutPaperBoxReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region[StorePaperBoxReport]
        private const string StorePaperBoxTimeout = "StorePaperBoxTimeout";
        public void StorePaperBoxReport(Trx inputData)
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

                    StorePaperBoxReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string paperBoxID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string boxorPaperBox = inputData.EventGroups[0].Events[0].Items[2].Value;
                string stageorPortorPalletorCar = inputData.EventGroups[0].Events[0].Items[3].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletNo = inputData.EventGroups[0].Events[0].Items[6].Value.PadLeft(2, '0');
                string carNo = inputData.EventGroups[0].Events[0].Items[7].Value.PadLeft(2, '0');
                #endregion
                //20151209 sy
                Port port = null; Cassette cst = null; Pallet pallet = null;
                #region [Check Box data]
                if (boxorPaperBox == "1")
                {
                    cst = ObjectManager.CassetteManager.GetCassette(boxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! INBOX", boxID));
                }
                else
                {
                    cst = ObjectManager.CassetteManager.GetCassette(paperBoxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! OUTBOX", paperBoxID));
                }

                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,BoxID =[{4}] ,PaperBoxID =[{5}] ,Store stageorPortorPalletorCar =[{6}],stageNo =[{7}] ,portNo =[{8}] ,palletNo =[{9}] ,carNo =[{10}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID, paperBoxID,
                    stageorPortorPalletorCar, stageNo, portNo, palletNo, carNo));
                #endregion
                StorePaperBoxReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);
                if (stageorPortorPalletorCar == "2" && portNo.PadLeft(2, '0') != "00")
                    port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));
                if (port != null)
                    port.File.CassetteID = boxorPaperBox == "1" ? boxID : paperBoxID;
                if (stageorPortorPalletorCar == "3" && palletNo.PadLeft(2, '0') != "00")
                    pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo.PadLeft(2, '0'));
                //insertDB
                //RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, string.Empty, string.Empty);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID,
                    boxorPaperBox == "1" ? eBoxType.InBox.ToString() : eBoxType.OutBox.ToString(), stageorPortorPalletorCar, stageNo, portNo,
                    pallet == null ? "" : pallet.File.PalletID, palletNo, carNo, "", "", "", line.File.HostMode.ToString());
                #region [Updata UI]
                if (port != null)
                {
                    lock (port)
                    {
                        port.File.PortBoxID1 = boxID;
                        port.File.PortBoxID2 = paperBoxID;
                        port.File.Status = ePortStatus.LC;
                    }
                    if (port.Data.PORTATTRIBUTE != keyCELLPORTAtt.PALLET)//20161102 sy modify   
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                }
                #endregion
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                #endregion
                #region[BoxProcessStarted]
                //PaperBox first store in car
                //if (stageorPortorPalletorCar == "4" && boxorPaperBox == "2")
                //{
                //    //List<Cassette> boxList = new List<Cassette>();

                //    //Cassette cst1 = ObjectManager.CassetteManager.GetCassette(paperBoxID);
                //    //if (cst1 != null)
                //    //{
                //    //    cst1.IsProcessed = true;
                //    //    boxList.Add(cst1);
                //    //}

                //    //Invoke(eServiceName.MESService, "BoxProcessStarted", new object[] { inputData.TrackKey, line, paperBoxID });
                //}
                #endregion
                #region[BoxProcessend]
                //if (stageorPortorPalletorCar == "3" && boxorPaperBox == "2")
                //{
                //    #region Add Reply Key
                //    //區分是box or paperbox ,MES reply 無法區分
                //    string key = keyBoxReplyPLCKey.PaperBoxReply;
                //    string rep = paperBoxID;
                //    if (Repository.ContainsKey(key))
                //        Repository.Remove(key);
                //    Repository.Add(key, rep);
                //    #endregion 
                //    Invoke(eServiceName.MESService, "BoxProcessend_CCPPK", new object[] { inputData.TrackKey, line, paperBoxID, eqp.Data.NODENO });
                //}
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    StorePaperBoxReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void StorePaperBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_StorePaperBoxReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + StorePaperBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + StorePaperBoxTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + StorePaperBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(StorePaperBoxReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,StorePaperBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void StorePaperBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, StorePaperBoxReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));
                StorePaperBoxReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region[RemovePaperBoxReport]
        private const string RemovePaperBoxTimeout = "RemovePaperBoxTimeout";
        public void RemovePaperBoxReport(Trx inputData)
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

                    RemovePaperBoxReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string paperBoxID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string boxorPaperBox = inputData.EventGroups[0].Events[0].Items[2].Value;
                string stageorPortorPalletorCar = inputData.EventGroups[0].Events[0].Items[3].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletNo = inputData.EventGroups[0].Events[0].Items[6].Value.PadLeft(2, '0');
                string carNo = inputData.EventGroups[0].Events[0].Items[7].Value.PadLeft(2, '0');
                string removeReasonFlag = inputData.EventGroups[0].Events[0].Items[8].Value;//"1：Weight NG Box   ,2：MES Return NG ,3：Normal Remove"
                #endregion
                //20151209 sy
                Port port = null; Cassette cst = null; Pallet pallet = null; string PortID = string.Empty;
                #region [Check Box data]
                if (boxorPaperBox == "1")
                {
                    cst = ObjectManager.CassetteManager.GetCassette(boxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! INBOX", boxID));
                }
                else
                {
                    cst = ObjectManager.CassetteManager.GetCassette(paperBoxID);
                    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT! OUTBOX", paperBoxID));
                }

                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,BoxID =[{4}] ,PaperBoxID =[{5}] ,Remove stageorPortorPalletorCar =[{6}],stageNo =[{7}] ,portNo =[{8}] ,palletNo =[{9}] ,carNo =[{10}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID, paperBoxID,
                    stageorPortorPalletorCar, stageNo, portNo, palletNo, carNo));
                #endregion
                RemovePaperBoxReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                if (stageorPortorPalletorCar == "2" && portNo.PadLeft(2, '0') != "00")
                    port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));
                if (port != null)
                    port.File.CassetteID = boxorPaperBox == "1" ? boxID : paperBoxID;
                if (stageorPortorPalletorCar == "3" && palletNo.PadLeft(2, '0') != "00")
                    pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo.PadLeft(2, '0'));
                //insertDB
                //RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, string.Empty, string.Empty);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID,
                    boxorPaperBox == "1" ? eBoxType.InBox.ToString() : eBoxType.OutBox.ToString(), stageorPortorPalletorCar, stageNo, portNo,
                    pallet == null ? "" : pallet.File.PalletID, palletNo, carNo, "", "", "", line.File.HostMode.ToString());
                #region [Updata UI]
                if (port != null)
                {
                    lock (port)
                    {
                        port.File.PortBoxID1 = "";
                        port.File.PortBoxID2 = "";
                        port.File.Status = ePortStatus.UC;
                    }
                    Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                }
                #endregion                
                //DELETE BOXDATA TO　ＤＯ
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                #endregion
                #region [Report MES]
                if (port != null) PortID = port.Data.PORTID;
                string realBoxID = boxorPaperBox == "1" ? boxID : boxorPaperBox == "2" ? paperBoxID : string.Empty;
                if (boxorPaperBox == "2" && !realBoxID.Trim().Equals(string.Empty) && port != null && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX_MANUAL)
                {
                    #region [MES BoxLineOutReport]
                    Invoke(eServiceName.MESService, "BoxLineOutReport", new object[] { inputData.TrackKey, line.Data.LINEID, PortID, realBoxID });
                    #endregion
                }
                else
                {
                    #region [Report to MES  Process Canceled or Abort ]
                    Invoke(eServiceName.MESService, "BoxProcessCanceled", new object[] {inputData.TrackKey,line.Data.LINEID,PortID,realBoxID,
                                    removeReasonFlag,removeReasonFlag=="1"? "Weight NG Box":  removeReasonFlag=="2" ?"MES Return NG" :"Normal Remove"});
                    #endregion
                }
                #endregion                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    RemovePaperBoxReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void RemovePaperBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RemovePaperBoxReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + RemovePaperBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + RemovePaperBoxTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + RemovePaperBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RemovePaperBoxReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,RemovePaperBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RemovePaperBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, RemovedPaperBoxReportReplyTimeout Set Bit =[OFF].", sArray[0], trackKey));
                RemovePaperBoxReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion       
        #region[BoxProcessFinishReport]
        private const string BoxProcessFinishReportTimeout = "BoxProcessFinishReportTimeout";
        public void BoxProcessFinishReport(Trx inputData)
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

                    BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "","","","");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}]  ,BoxID =[{4}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID));
                #endregion
                Cassette cst = ObjectManager.CassetteManager.GetCassette(boxID);
                if (cst == null)//20161102 sy modify
                {
                    BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "OutBox", boxID, "InBox");
                    throw new Exception(string.Format("CAN NOT FIND BOX=[{0}] IN CST OBJECT!", boxID));
                }
                if (cst.eBoxType != eBoxType.InBox)//20161102 sy modify
                {
                    BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "OutBox", boxID, "InBox");
                    throw new Exception(string.Format("CAN NOT FIND INBOX=[{0}] IN CST OBJECT!", boxID));
                }
                if (cst.SubBoxID == string.Empty)//20161102 sy modify
                {
                    BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "OutBox", boxID, "InBox");
                    throw new Exception(string.Format("CAN NOT FIND INBOX=[{0}] NO OUTBOX ID", boxID));
                }
                BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, cst.SubBoxID, "OutBox", boxID, "InBox");

                List<Cassette> boxIDList = new List<Cassette>();
                boxIDList.Add(cst);
                
                //依設定 回機台OK/NG
                eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                //裝箱完畢將OUTBOX 為KEY
                #region [InBox ->OutBox]
                Cassette newCst = new Cassette();
                lock (newCst)
                {
                    newCst.CassetteID = cst.SubBoxID;
                    newCst.eBoxType = eBoxType.OutBox;
                    newCst.SubBoxID = boxID;
                    newCst.LDCassetteSettingCode = cst.LDCassetteSettingCode;
                    newCst.LineRecipeName = cst.LineRecipeName;
                    newCst.ProductType = cst.ProductType;
                    newCst.Grade = cst.Grade;
                    newCst.IsBoxed = true;
                }
                ObjectManager.CassetteManager.CreateBoxforPacking(newCst);
                #endregion

                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, "",
                    eBoxType.InBox.ToString(), "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    //BoxProcessFinishReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, rtncode, "", "", "", "");
                    //刪除INBOX 
                    ObjectManager.CassetteManager.DeleteBoxforPacking(cst);
                    return;
                }
                #endregion
                #region Add Reply Key
                //MES Reply no Unit No (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.BoxProcessEndReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                //OUT 從BOX 要報OUTBOXEND
                Invoke(eServiceName.MESService, "OutBoxProcessEnd", new object[] { inputData.TrackKey, line.Data.LINEID, "",cst });
                //刪除INBOX 
                ObjectManager.CassetteManager.DeleteBoxforPacking(cst);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void BoxProcessFinishReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string paperBoxID, string paperBoxType, string boxID, string boxType)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_BoxProcessFinishReportReply") as Trx;

                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + BoxProcessFinishReportTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + BoxProcessFinishReportTimeout);
                    }
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,BoxProcessFinishReportReply Set Bit =[{2}).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion
                    return;
                }
                #endregion
                #region[MES Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[0].Items[1].Value = paperBoxID;  // paperBoxID ;
                outputdata.EventGroups[0].Events[0].Items[2].Value = paperBoxType;  // paperBoxType;
                outputdata.EventGroups[0].Events[0].Items[3].Value = boxID;  // boxID ;
                outputdata.EventGroups[0].Events[0].Items[4].Value = boxType;  // boxType;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + BoxProcessFinishReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + BoxProcessFinishReportTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + BoxProcessFinishReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(BoxProcessFinishReportReplyTimeout), trackKey);
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
        private void BoxProcessFinishReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Box Process Finish Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                BoxProcessFinishReportReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "", "", "");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [PaperBoxDataRequest]
        private const string PaperBoxDataReqTimeout = "PaperBoxDataReqTimeout";
        public void PaperBoxDataRequest(Trx inputData)
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
                #region [PLCAgent Data Word]
                string portNo = inputData.EventGroups[0].Events[0].Items[0].Value.PadLeft(2, '0');
                string boxID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string paperBoxID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                eBoxType boxType = eBoxType.NODE;
                if (inputData.EventGroups[0].Events[0].Items[3].Value.Trim() == ((int)eBoxType.InBox).ToString())
                    boxType = eBoxType.InBox;
                else if (inputData.EventGroups[0].Events[0].Items[3].Value.Trim() == ((int)eBoxType.OutBox).ToString())
                    boxType = eBoxType.OutBox;
                else
                    throw new Exception("Can't find BoxType");
                //eBoxType boxType = inputData.EventGroups[0].Events[0].Items[3].Value.Trim() == ((int)eBoxType.InBox).ToString() ? eBoxType.InBox : eBoxType.OutBox;
                #endregion
                Port port = null; Cassette box = null;
                string boxIDReq = boxType == eBoxType.InBox ? boxID : paperBoxID;
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null && portNo !="00") throw new Exception(string.Format("Can't find Port No =[{0}] in PortEntity!", portNo));
                if (port != null)
                {
                    lock (port)
                    {
                        port.File.CassetteID = boxIDReq.ToString();
                        port.File.PortDBDataRequest = ((int)bitResult).ToString();
                        port.File.Mes_ValidateBoxReply = string.Empty;
                        port.File.PortBoxID1 = boxID;
                        port.File.PortBoxID2 = paperBoxID;
                        port.File.BoxType = boxType;
                    }
                }                

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    PaperBoxDataRequestReply(eqp.Data.NODENO, bitResult, inputData.TrackKey, eReturnCode1.Unknown, "", "", "", eBoxType.NODE, "", "");
                    if (port == null) return;
                    if (line.File.HostMode == eHostMode.OFFLINE)
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,PortNo =[{4}] ,BoxID  =[{5}] ,PaperBoxID =[{6}] ,BoxType =[{7}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, portNo, boxID, paperBoxID, boxType.ToString()));
                
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID,
                    boxType.ToString(), "2", "", portNo, "", "", "", "", "", "", line.File.HostMode.ToString());
                if (line != null)
                {
                    if (line.File.HostMode != eHostMode.OFFLINE)
                    {
                        //string boxIDReq = boxType == eBoxType.InBox ? boxID : paperBoxID;
                        box = ObjectManager.CassetteManager.GetCassette(boxIDReq);
                        if (box == null)
                        {
                            PaperBoxDataRequestReply(eqp.Data.NODENO, bitResult, inputData.TrackKey, eReturnCode1.NG, portNo, boxID, paperBoxID, boxType, "0", "");
                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES] =[{0}] Can,t find Box =[{1}].", inputData.TrackKey, boxIDReq));
                        }
                        else if (boxType != box.eBoxType)
                        {
                            PaperBoxDataRequestReply(eqp.Data.NODENO, bitResult, inputData.TrackKey, eReturnCode1.NG, portNo, boxID, paperBoxID, boxType, "0", "");
                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS <- MES] =[{0}] Can find Box =[{1}] ,But type different", inputData.TrackKey, boxIDReq));
                        }
                        else
                        {
                            PaperBoxDataRequestReply(eqp.Data.NODENO, bitResult, inputData.TrackKey, eReturnCode1.OK, portNo, boxType == eBoxType.InBox ? box.CassetteID : box.SubBoxID
                                    , boxType == eBoxType.InBox ? box.SubBoxID : box.CassetteID, boxType, box.ProductType, box.Grade);                            
                        }
                    }
                    else
                    {
                        if (port == null) return;
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                    }                 
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                //// 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PaperBoxDataRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "", "", eBoxType.NODE, "", "");
            }
        }
        public void PaperBoxDataRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string portNO,
            string boxID, string paperBoxID,eBoxType boxType, string productType, string grade)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PaperBoxDataRequestReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxDataReqTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxDataReqTimeout);
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DenseBoxDataRequestReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
                    return;
                }
                #endregion
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString();// returncode;
                outputdata.EventGroups[0].Events[0].Items[1].Value = portNO;//portno; 
                outputdata.EventGroups[0].Events[0].Items[2].Value = boxID.Trim();//boxID; 
                outputdata.EventGroups[0].Events[0].Items[3].Value = paperBoxID.Trim();//paperBoxID;  
                outputdata.EventGroups[0].Events[0].Items[4].Value = ((int)boxType).ToString();//boxType;  
                outputdata.EventGroups[0].Events[0].Items[5].Value = productType;//producttype; 
                outputdata.EventGroups[0].Events[0].Items[6].Value = grade.Trim();//grade;  
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, paperBoxID,
                    boxType.ToString(), "2", "", portNO, "", "", "", returnCode.ToString(), productType, grade, line.File.HostMode.ToString());
                Port port = null; Cassette box = null;
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNO);
                if (boxType == eBoxType.InBox)
                {
                    box = ObjectManager.CassetteManager.GetCassette(boxID.Trim());
                }
                else if (boxType == eBoxType.OutBox)
                {
                    box = ObjectManager.CassetteManager.GetCassette(paperBoxID.Trim());
                }
                if (line.File.HostMode == eHostMode.OFFLINE)//Offline OPI下才產生 BOX
                {
                    //Cassette cst = new Cassette();
                    //if (boxType == eBoxType.InBox && boxID.Trim() != string.Empty)
                    //{
                    //    cst.CassetteID = boxID.Trim();
                    //    cst.eBoxType = eBoxType.InBox;
                    //}
                    //else if (boxType == eBoxType.OutBox && paperBoxID.Trim() != string.Empty)
                    //{
                    //    cst.CassetteID = paperBoxID.Trim();
                    //    cst.eBoxType = eBoxType.OutBox;
                    //    cst.SubBoxID = boxID.Trim();
                    //}
                    //else
                    //    throw new Exception(string.Format("BOX Type & ID Mixmatch"));       
                    //cst.Grade = grade;
                    //cst.ProductType = productType;
                    //cst.PortID = portNO;
                    //cst.PortNo = portNO;
                    //ObjectManager.CassetteManager.CreateBoxforPacking(cst);
                }
                
                if (_timerManager.IsAliveTimer(eqpNo + "_" + PaperBoxDataReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PaperBoxDataReqTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PaperBoxDataReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PaperBoxDataRequestReplyTimeout), trackKey);

                    //if (box != null)
                    //    RecordCassetteHistory(outputdata.TrackKey, eqp, port, box, string.Empty, returnCode.ToString(), string.Empty);
                }
                string datalog = string.Format("PortNO= [{0}],BoxID= [{1}],PaperBoxID= [{2}],BoxType= [{3}],ProductType= [{4}],Grade= [{5}]",
                    portNO, boxID, paperBoxID, boxType.ToString(), productType, grade);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,PaperBoxDataRequestReply Set Bit =[{2}].,Rtncode =[{3}],[{4}]",
                    eqpNo, trackKey, value.ToString(),returnCode.ToString(), datalog));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PaperBoxDataRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DenseBoxDataRequest Timeout Set Bit [OFF].", sArray[0], trackKey));
                PaperBoxDataRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "", "", eBoxType.NODE, "", "");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion        
        #region [Common Function]
        private void RecordPPKEventHistory(string trxID, string eventName, Equipment eqp, string boxCount, string boxID1, 
            string boxID2, string boxType,string stageorPortorPalletorCar, string stageNo, string portNo, string palletId,
            string palletNo, string carNo, string returnCode, string productType, string grade, string remark)
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
                ppkHis.GRADE01 = grade;
                ppkHis.REMARK = remark;
                ObjectManager.PalletManager.RecordPPKEventHistory(ppkHis);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }       
        /// <summary>
        /// SendPLCData Write PLC Function
        /// </summary>
        /// <param name="outputData">PLC Data</param>
        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
        #endregion
    }
}
