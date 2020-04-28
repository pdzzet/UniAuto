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
    public partial class PalletService : AbstractService
    {
        private enum eUnloadResult
        {
            NormalEnd = 1,
            CancelEnd =2,
            AbortEnd =3
        }
        private enum eStageorPallet
        {
            Stage = 1,
            Pallet  =2
        }
        private enum eResult
        {
            Unknown = 0,
            OK = 1,
            NG = 2
        }
        IServerAgent _plcAgent = null;
        private IServerAgent PLCAgent
        {
            get
            {
                if (_plcAgent == null)
                {
                    _plcAgent = GetServerAgent(eAgentName.PLCAgent);
                }
                return _plcAgent;
            }
        }

        public override bool Init()
        {
            return true;
        }

        #region [LabelInformationforPalletRequest]
        private const string LabelInformationforPalletRequestTimeout = "LabelInformationforPalletRequestTimeout";
        public void LabelInformationforPalletRequest(Trx inputData)
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
                    LabelInformationforPalletRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string palletID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,LabelInformationforPalletRequest PalletID =[{4}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletID));
                #endregion
                //依設定 回機台OK/NG
                eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    LabelInformationforPalletRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtncode, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID, "", "", ""
                    , "", line.File.HostMode.ToString());
                #region MES Data
                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.PalletLabelInformationRequestReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                //Send MES Data
                Invoke(eServiceName.MESService, "PalletLabelInformationRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, palletID, eqp.Data.NODENO });

                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void LabelInformationforPalletRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string itemName1, string itemValue1,
        string itemName2, string itemValue2, string itemName3, string itemValue3, string itemName4, string itemValue4, string itemName5, string itemValue5,
            string itemName6, string itemValue6, string itemName7, string itemValue7, string itemName8, string itemValue8, string itemName9, string itemValue9,
            string itemName10, string itemValue10, string itemName11, string itemValue11, string itemName12, string itemValue12)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "LabelInformationforPalletRequestReply") as Trx;
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
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,LabelInformationforPalletRequestReply Set Bit =[{2}] ).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + LabelInformationforPalletRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + LabelInformationforPalletRequestTimeout);
                    }
                    return;
                }
                #endregion
                #region[MES Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[0].Items[1].Value = itemName1;  // SHIPID
                outputdata.EventGroups[0].Events[0].Items[2].Value = itemValue1;  // SHIPID;
                outputdata.EventGroups[0].Events[0].Items[3].Value = itemName2;  // BOMVERSION;
                outputdata.EventGroups[0].Events[0].Items[4].Value = itemValue2;  // BOMVERSION;
                outputdata.EventGroups[0].Events[0].Items[5].Value = itemName3;  // MODELNAME;
                outputdata.EventGroups[0].Events[0].Items[6].Value = itemValue3;  // MODELNAME;
                outputdata.EventGroups[0].Events[0].Items[7].Value = itemName4;  // MODELVERSION;
                outputdata.EventGroups[0].Events[0].Items[8].Value = itemValue4;  // MODELVERSION;
                outputdata.EventGroups[0].Events[0].Items[9].Value = itemName5;  // BOXQUANTITY;
                outputdata.EventGroups[0].Events[0].Items[10].Value = itemValue5;  // BOXQUANTITY;
                outputdata.EventGroups[0].Events[0].Items[11].Value = itemName6;  // PRODUCTQUANTITY;
                outputdata.EventGroups[0].Events[0].Items[12].Value = itemValue6;  // PRODUCTQUANTITY;
                outputdata.EventGroups[0].Events[0].Items[13].Value = itemName7;  // WEEKCODE;
                outputdata.EventGroups[0].Events[0].Items[14].Value = itemValue7;  // WEEKCODE;
                outputdata.EventGroups[0].Events[0].Items[15].Value = itemName8;  // ENVIRONMENTFLAG;
                outputdata.EventGroups[0].Events[0].Items[16].Value = itemValue8;  // ENVIRONMENTFLAG;
                outputdata.EventGroups[0].Events[0].Items[17].Value = itemName9;  // NOTE;
                outputdata.EventGroups[0].Events[0].Items[18].Value = itemValue9;  // NOTE;
                outputdata.EventGroups[0].Events[0].Items[19].Value = itemName10;  // COUNTRY;
                outputdata.EventGroups[0].Events[0].Items[20].Value = itemValue10;  // COUNTRY;
                outputdata.EventGroups[0].Events[0].Items[21].Value = itemName11;  // CARRIERNAME;
                outputdata.EventGroups[0].Events[0].Items[22].Value = itemValue11;  // CARRIERNAME;
                outputdata.EventGroups[0].Events[0].Items[23].Value = itemName12;  // WT;
                outputdata.EventGroups[0].Events[0].Items[24].Value = itemValue12;  // WT;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + LabelInformationforPalletRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LabelInformationforPalletRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + LabelInformationforPalletRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LabelInformationforPalletRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                string datalog = string.Format("SHIPID=[{0}]:[{1}],BOMVERSION=[{2}]:[{3}],MODELNAME=[{4}]:[{5}],MODELVERSION=[{6}]:[{7}],BOXQUANTITY=[{8}]:[{9}],PRODUCTQUANTITY=[{10}]:[{11}],WEEKCODE=[{12}]:[{13}],ENVIRONMENTFLAG=[{14}]:[{15}],NOTE=[{16}]:[{17}],COUNTRY=[{18}]:[{19}],CARRIERNAME=[{20}]:[{21}],WT=[{22}]:[{23}]",
                   itemName1, itemValue1, itemName2, itemValue2, itemName3, itemValue3, itemName4, itemValue4, itemName5, itemValue5, itemName6, itemValue6, itemName7, itemValue7, itemName8, itemValue8, itemName9, itemValue9, itemName10, itemValue10, itemName11, itemValue11, itemName12, itemValue12);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,LabelInformationforPalletRequestReply Set Bit =[{2}],Rtncode =[{3}],[{4}]).",
                eqpNo, trackKey, value.ToString(), rtncode.ToString(), datalog));
                #endregion
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", "", "", ""
                    , "", rtncode.ToString(), line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LabelInformationforPalletRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, LabelInformationforPalletRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                //LabelInformationforPalletRequestReply(sArray[0], eBitResult.OFF, trackKey, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [PalletLabelInformationRequest]
        private const string PalletLabelInfoReqTimeout = "PalletLabelInfoReqTimeout";
        public void PalletLabelInformationRequest(Trx inputData)
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
                    PalletLabelInformationRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, null);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string palletID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PalletLabelInformationRequest PalletID =[{4}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletID));

                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + PalletLabelInfoReqTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + PalletLabelInfoReqTimeout);
                }
                _timerManager.CreateTimer(eqp.Data.NODENO + "_" + PalletLabelInfoReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletLabelInformationRequestReplyTimeout), inputData.TrackKey);

                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID, "", "", ""
                    , "", line.File.HostMode.ToString());
                #region MES Data
                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.PalletLabelInformationRequestReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                //Send MES Data
                Invoke(eServiceName.MESService, "PalletLabelInformationRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, palletID, eqp.Data.NODENO });

                #endregion



            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PalletLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey, object reply)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletLabelInformationRequestReply") as Trx;
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
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletLabelInfoReqTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PalletLabelInfoReqTimeout);
                    }
                    return;
                }


                string[] replyArray = reply.ToString().Split(',');
                if (replyArray.Length < 5)
                    return;

                outputdata.EventGroups[0].Events[0].Items[0].Value = replyArray[0];  // returnCode(INT);
                outputdata.EventGroups[0].Events[0].Items[1].Value = replyArray[1];  // palletID;
                outputdata.EventGroups[0].Events[0].Items[2].Value = replyArray[2];  // modelName;
                outputdata.EventGroups[0].Events[0].Items[3].Value = replyArray[3];  // modelVersion;
                outputdata.EventGroups[0].Events[0].Items[4].Value = replyArray[4];  // cartonQuantity(INT);

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletLabelInfoReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletLabelInfoReqTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletLabelInfoReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletLabelInformationRequestReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletLabelInformationRequestReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", replyArray[1], "", "", ""
                    ,replyArray[0] == "1" ? eReturnCode1.OK.ToString() : eReturnCode1.NG.ToString(), line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletLabelInformationRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletLabelInformationRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletLabelInformationRequestReply(sArray[0], eBitResult.OFF, trackKey, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [PalletMoveToStageReport]
        private const string PalletMoveToStageTimeout = "PalleMoveToStageTimeout";
        public void PalletMoveToStageReport(Trx inputData)
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
                    PalletMoveToStageReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string palletMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string palletID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string stageNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PalletMoveToStageReport PalletMode =[{4}],PalletID =[{5}],StageNo =[{6}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode, palletID, stageNo));

                PalletMoveToStageReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID, "", "", stageNo
            , "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PalletMoveToStageReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletMoveToStageReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletMoveToStageTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletMoveToStageTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletMoveToStageTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletMoveToStageReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletMoveToStageReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletMoveToStageReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletMoveToStageReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletMoveToStageReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [PalletMoveOutStageReport]
        private const string PalletMoveOutStageTimeout = "PalletMoveOutStageTimeout";
        public void PalletMoveOutStageReport(Trx inputData)
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
                    PalletMoveOutStageReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string palletMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string palletID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string stageNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PalletMoveOutStageReport PalletMode =[{4}],PalletID =[{5}],StageNo =[{6}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode, palletID, stageNo));

                PalletMoveOutStageReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID, "", "", stageNo
            , "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PalletMoveOutStageReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletMoveOutStageReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletMoveOutStageTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletMoveOutStageTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletMoveOutStageTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletMoveOutStageReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletMoveOutStageReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletMoveOutStageReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletMoveOutStageReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletMoveOutStageReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }        
        #endregion
        #region [PalletDataRequestReport]
        private const string PalletDataReqTimeout = "PalletDataReqTimeout";
        public void PalletDataRequestReport(Trx inputData)
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
                ePalletMode palletMode = (ePalletMode)int.Parse( inputData.EventGroups[0].Events[0].Items[0].Value);
                string palletID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string palletNo = inputData.EventGroups[0].Events[0].Items[2].Value.PadLeft(2, '0');
                #endregion
                #region Update Pallet Data
                Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo);
                if (pallet == null)
                {
                    pallet = new Pallet(new PalletEntityFile());
                    pallet.File.PalletMode = palletMode;
                    pallet.File.PalletID = palletID.Trim();
                    pallet.File.PalletNo = palletNo;
                    pallet.File.PalletDataRequest = ((int)bitResult).ToString();
                    pallet.File.NodeNo = eqp.Data.NODENO;

                    ObjectManager.PalletManager.AddPallet(pallet);
                }
                else
                {
                    lock (pallet)
                    {
                        pallet.File.PalletID = palletID;
                        pallet.File.PalletMode = palletMode;
                        pallet.File.PalletNo = palletNo;
                        pallet.File.PalletDataRequest = ((int)bitResult).ToString();
                        pallet.File.NodeNo = eqp.Data.NODENO;

                        ObjectManager.PalletManager.EnqueueSave(pallet.File);
                    }
                }
                
                Invoke(eServiceName.UIService, "PalletStatusReport", new object[] { eqp.Data.LINEID, pallet });
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    PalletDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, null, null);
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PalletDataRequestReport PalletMode =[{4}],PalletID =[{5}],PalletNO =[{6}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode, palletID, palletNo));

                if (palletID.Trim() == string.Empty)
                {
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,PalletDataRequestReport PalletID is Empty!!",
                                 eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));
                    //"1：OK ,2：NG ,3：etc NG…"

                    PalletDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, ((int)eResult.NG).ToString(), null);
                    return;
                }

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    #region MES Send ValidatePalletRequest
                    //trxID,  lineName, portID, portmode, palletID,boxlist
                    List<string> boxlist = new List<string>();
                    #region Add Reply Key
                    //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                    string key = keyBoxReplyPLCKey.PalletDataRequestReportReply;
                    string rep = eqp.Data.NODENO;
                    if (Repository.ContainsKey(key))
                        Repository.Remove(key);
                    Repository.Add(key, rep);
                    #endregion
                    //ValidatePalletRequest(string trxID, string lineName, string portID, string portmode, string palletID, IList boxlist,string eqpNoforTimeout)
                    Invoke(eServiceName.MESService, "ValidatePalletRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, palletNo,
                   palletMode.ToString(), palletID, boxlist, eqp.Data.NODENO });
                    #endregion
                }
                else
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CCPPK)
                    {
                        PalletDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, ((int)eResult.OK).ToString(), pallet);
                    }
                    else
                    Invoke(eServiceName.UIService, "PalletStatusReport", new object[] { eqp.Data.LINEID, pallet });
                }

                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID,
            palletNo, palletMode.ToString(), "", "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PalletDataRequestReportReply(string eqpNo, eBitResult value, string trackKey, string returncode, Pallet pallet)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletDataRequestReportReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                if (outputdata == null)
                    return;
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletDataReqTimeout))
                        _timerManager.TerminateTimer(eqpNo + "_" + PalletDataReqTimeout);
                    return;
                }

                outputdata.ClearTrxWith0();
                if ((pallet != null) && (pallet.File.DenseBoxList.Count != 0))
                {

                    outputdata.EventGroups[0].Events[0].Items[0].Value = returncode;  // ReturnCode;
                    outputdata.EventGroups[0].Events[0].Items[1].Value = pallet.File.PalletID;  // PalletID;
                    outputdata.EventGroups[0].Events[0].Items[2].Value = pallet.File.PalletNo;  // PalletNo;
                    outputdata.EventGroups[0].Events[0].Items[3].Value = pallet.File.DenseBoxList.Count.ToString();  // DenseBoxCount;

                    int i = 0;
                    foreach (string boxid in pallet.File.DenseBoxList)
                    {
                        outputdata.EventGroups[0].Events[0].Items[4 + i].Value = boxid;
                        i++;
                    }

                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = returncode;  // ReturnCode;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputdata.EventGroups[0].Events[0].Items[1].Value = pallet.File.PalletID;  // PalletID;
                    outputdata.EventGroups[0].Events[0].Items[2].Value = pallet.File.PalletNo;  // PalletNo;
                    outputdata.EventGroups[0].Events[0].Items[3].Value = pallet.File.DenseBoxList.Count.ToString();  // DenseBoxCount;

                    for (int i = 4; i < outputdata.EventGroups[0].Events[0].Items.Count; i++)
                    {
                        outputdata.EventGroups[0].Events[0].Items[i].Value = ""; //DenseBoxID#00
                    }


                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                }
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletDataReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletDataReqTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletDataReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletDataRequestReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletDataRequestReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));

                lock (pallet) pallet.File.Mes_ValidatePalletReply = string.Empty;

                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, pallet.File.DenseBoxList.Count.ToString(), "", pallet.File.PalletID,
            pallet.File.PalletNo, pallet.File.PalletMode.ToString(), "", returncode.ToString(), line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletDataRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletDataRequestReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletDataRequestReportReply(sArray[0], eBitResult.OFF, trackKey, null, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion        
        #region [PalletStartReport]
        private const string PalletStartTimeout = "PalletStartTimeout";
        public void PalletStartReport(Trx inputData)
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
                    PalletStartReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                ePalletMode  palletMode = (ePalletMode)int.Parse( inputData.EventGroups[0].Events[0].Items[0].Value);
                string palletID = inputData.EventGroups[0].Events[0].Items[1].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}],PalletStartReport PalletMode =[{4}],PalletID =[{5}],StageNo =[{6}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode, palletID, stageNo));

                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + PalletStartTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + PalletStartTimeout);
                }
                _timerManager.CreateTimer(eqp.Data.NODENO + "_" + PalletStartTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletStartReportReplyTimeout), inputData.TrackKey);


                PalletStartReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", "", palletID,
                    "", palletMode.ToString(), stageNo, "", line.File.HostMode.ToString());
                #region	MES Send PalletProcessStarted
                Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletID.Trim()); // Pallet沒有ID，所以改用stageNo取 by tom.su 2016.06.06 //sy 修正回來[PPK QPP 共同使用不調整這邊] 2016.06.06

                if (pallet == null) throw new Exception(string.Format("Can't find Pallet ID=[{0}) in PalletEntity!", palletID));

                pallet.File.PalletMode = palletMode;
                //trxID,string lineName,string palletteID,string boxQty,string pPID,List<string>boxList
                object[] _data = new object[5]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    pallet.File.PalletName, /*1 palletteID*///sy modify20151130 For MES  要求上報PalletName
                    pallet.File.LineRecipeName, /*1 LineRecipeName*/
                    pallet.File.DenseBoxList /*1 boxList*/
                };
                base.Invoke(eServiceName.MESService, "PalletProcessStarted", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PalletStartReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletStartReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletStartTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletStartTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletStartTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletStartReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletStartReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletStartReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletStartReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletStartReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion        
        #region [PalletPackingModeChangeReport]
        //目前沒有用了Watson 20150409 For 博章
        private const string PalletPackingModeTimeout = "PalletPackingModeTimeout";
        public void PalletPackingModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    PalletPackingModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                ePalletMode palletMode = (ePalletMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string palletID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string palletNo = inputData.EventGroups[0].Events[0].Items[2].Value.PadLeft(2, '0');
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}],PalletPackingModeChangeReport PalletMode =[{4}],PalletID =[{5}],PalletNo =[{6}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode.ToString(), palletID, palletNo));               

                PalletPackingModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //Jun Modify 20141229 Pack/UnPack在Data Request的時候一起上報給MES
                #region Update Pallet Data
                Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo);
                if (pallet == null)
                {
                    pallet = new Pallet(new PalletEntityFile());
                    pallet.File.PalletMode = palletMode;
                    pallet.File.PalletID = palletID.Trim();
                    pallet.File.PalletNo = palletNo;
                    pallet.File.NodeNo = eqp.Data.NODENO;
                }
                else
                {
                    pallet.File.PalletID = palletID;
                    pallet.File.PalletMode = palletMode;
                    pallet.File.PalletNo = palletNo;
                    pallet.File.NodeNo = eqp.Data.NODENO;
                }
                ObjectManager.PalletManager.AddPallet(pallet);
                Invoke(eServiceName.UIService, "PalletStatusReport", new object[] { eqp.Data.LINEID, pallet });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PalletPackingModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PalletPackingModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PalletPackingModeChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PalletPackingModeTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PalletPackingModeTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PalletPackingModeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PalletPackingModeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PalletPackingModeChangeReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PalletPackingModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PalletPackingModeChangeReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                PalletPackingModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [UnPalletDataReport]
        private const string UnPalletDataTimeout = "UnPalletDataTimeout";
        public void UnPalletDataReport(Trx inputData)
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
                    UnPalletDataReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string palletMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string unloadingResult = inputData.EventGroups[0].Events[0].Items[1].Value;
                string stageorPallet = inputData.EventGroups[0].Events[0].Items[2].Value;
                string palletID = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string palletNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string stageNo = inputData.EventGroups[0].Events[0].Items[5].Value;

                int boxposition = 1;
                List<string> DenseBoxlist = new List<string>();
                foreach (object value in inputData.EventGroups[0].Events[0].Items.AllValues)
                {
                    if (inputData.EventGroups[0].Events[0].Items["DenseBoxID#" + boxposition.ToString()] != null)
                    {
                        if (inputData.EventGroups[0].Events[0].Items["DenseBoxID#" + boxposition.ToString()].Value.Trim() != string.Empty)
                        {
                            DenseBoxlist.Add(inputData.EventGroups[0].Events[0].Items["DenseBoxID#" + boxposition.ToString()].Value.Trim());
                            boxposition++;
                        }
                    }
                }

                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,UnPalletDataReport PalletMode =[{4}],UnloadingResult=[{5}]" +
                        "StageorPallet =[{6}],PalletID =[{7}],PalletNo=[{8}],StageNo =[{9}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, palletMode, unloadingResult, stageorPallet,
                             palletID, palletNo, stageNo));


                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + UnPalletDataTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + UnPalletDataTimeout);
                }
                _timerManager.CreateTimer(eqp.Data.NODENO + "_" + UnPalletDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UnPalletDataReportReplyTimeout), inputData.TrackKey);

                eUnloadResult unloadresult;
                Enum.TryParse<eUnloadResult>(unloadingResult, out unloadresult);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, boxposition.ToString(), "", palletID,
                    palletNo, palletMode, stageNo, "", line.File.HostMode.ToString());
                //MES
                switch (unloadresult)
                {
                    case eUnloadResult.NormalEnd:
                    case eUnloadResult.AbortEnd:
                        #region	MES Send PalletProcessEnd
                        //trxID,string lineName,string palletteID,string pPID,List<string>boxList)
                        object[] _data = new object[6]
                            { 
                                inputData.TrackKey,  /*0 TrackKey*/
                                eqp.Data.LINEID,    /*1 LineName*/
                                palletID, /*2 palletteID*/
                                palletID, /*3 carrierName*/
                                "", /*4 pPID*/
                                DenseBoxlist/*boxList*/
                            };
                        base.Invoke(eServiceName.MESService, "PalletProcessEnd", _data);
                        #endregion
                        //20151110 shihyang add BoxProcessEnd 將DenseBoxlist (非pallet上本來就有的) END//Mark 20151130
                        #region [BoxProcessEnd]
                        //Cassette cst;
                        //List<Cassette> boxList = new List<Cassette>();
                        //foreach (string Box in DenseBoxlist)
                        //{
                        //    cst = ObjectManager.CassetteManager.GetCassette(Box);
                        //    if (cst != null)
                        //    {
                        //        if (cst .IsProcessed) boxList.Add(cst);
                        //    } 
                        //}
                        //if (boxList.Count !=0)
                        //{
                        //    //(string trxID, string lineID, List<Cassette> boxList)
                        //    object[] _data1 = new object[3]
                        //    { 
                        //        inputData.TrackKey,  /*0 TrackKey*/
                        //        eqp.Data.LINEID,    /*1 LineName*/
                        //        boxList/*boxList*/
                        //    };
                        //    Invoke(eServiceName.MESService, "BoxProcessEnd_PPK", _data1);
                        //}
                        #endregion
                        break;
                    case eUnloadResult.CancelEnd:
                        #region	MES Send PalletProcessCanceled
                        //(string trxID,string lineName,string palletteID,string portID)
                        object[] _data2 = new object[4]
                            { 
                                inputData.TrackKey,  /*0 TrackKey*/
                                eqp.Data.LINEID,    /*1 LineName*/
                                palletID,
                                palletNo
                            };
                        base.Invoke(eServiceName.MESService, "PalletProcessCanceled", _data2);
                        #endregion
                        break;
                    default:
                        break;
                }

                UnPalletDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //Watson Add 20150102 For Box Out
                foreach (string boxid in DenseBoxlist)
                {
                    //ObjectManager.CassetteManager.DeleteBox(boxid); //20161102 sy modify 舊方式會將所有flie都delete
                    Cassette cstTmp = ObjectManager.CassetteManager.GetCassette(boxid);
                    if (cstTmp != null) ObjectManager.CassetteManager.DeleteBoxforPacking(cstTmp);
                }

                //Jun Add 20150202
                ObjectManager.PalletManager.DeletePallet(palletNo);//20161226 sy modify PalletID=> PalletNo
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void UnPalletDataReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UnPalletDataReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + UnPalletDataTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + UnPalletDataTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + UnPalletDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UnPalletDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,UnPalletDataReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UnPalletDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, UnPalletDataReportReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                UnPalletDataReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Common Function]
        private void RecordPPKEventHistory(string trxID, string eventName, Equipment eqp, string boxCount, string portNo, string palletId,
            string palletNo, string packMode, string stageNo, string returnCode, string remark)
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
                ppkHis.PORTNO = portNo;
                ppkHis.PALLETID = palletId;
                ppkHis.PALLETNO = palletNo;
                ppkHis.PACKUNPACKMODE = packMode;
                ppkHis.STAGENO = stageNo;
                ppkHis.RETURNCODE = returnCode;
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
        #endregion
    }
}

