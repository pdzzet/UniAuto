using System;
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
using System.Xml;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public partial class CFSpecialService : AbstractService
    {
        private const string LapmUsedTimeReportTimeout = "LapmUsedTimeReportTimeout";
        private const string TankChangeReportTimeout = "TankChangeReportTimeout";
        private const string QTimeCheckRequestReplyTimeout = "QTimeCheckRequestReplyTimeout";
        private const string HighCVModeChangeReportTimeout = "HighCVModeChangeReportTimeout";
        private const string UnloadingPortSettingCommandTimeout = "UnloadingPortSettingCommandTimeout";
        private const string UnloadingPortSettingReportTimeout = "UnloadingPortSettingReportTimeout";
        private const string BufferRWJudgeCapacityChangeCommandTimeout = "BufferRWJudgeCapacityChangeCommandTimeout";
        private const string BufferRWJudgeCapacityChangeReportTimeout = "BufferRWJudgeCapacityChangeReportTimeout";
        private const string GlassReworkJudgeReportTimeout = "GlassReworkJudgeReportTimeout";
        private const string InlineReworkModeChangeReportTimeout = "InlineReworkModeChangeReportTimeout";
        private const string ExposureMaskCheckCommandTimeout = "ExposureMaskCheckCommandTimeout";

        #region [9.2.7 Lamp Used Time Report]
        public void LampUsedTimeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [取得EQP資訊]
                Equipment eqp;               
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string lampuseTime = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                #region[取得LampArea]
                string lampArea = string.Empty;
                if (eqp.Data.NODEATTRIBUTE == "EXPOSURE")
                {
                    switch (inputData.EventGroups[0].Events[0].Items[0].Value.ToString().ToUpper())
                    {
                        case "RD":
                            lampArea = "RDLAMP";
                            break;
                        case "YW":
                            lampArea = "YWLAMP";
                            break;
                        case "BE":
                            lampArea = "BELAMP";
                            break;
                        case "PE":
                            lampArea = "PELAMP";
                            break;
                    }
                }
                else
                {
                    lampArea = "LAMP";
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    LampUsedTimeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON)", inputData.Metadata.NodeNo, inputData.TrackKey));
                }
                #endregion

                #region [Reply EQ]
                LampUsedTimeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Report MES]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.Data.LINETYPE != "FCUPK_TYPE1")// 20151230 MES 陳良棟表示UPK Line無須上報
                {
                    IList<ChangeMaterialLifeReport.MATERIALc> materialList = new List<ChangeMaterialLifeReport.MATERIALc>();
                    ChangeMaterialLifeReport.MATERIALc mat = new ChangeMaterialLifeReport.MATERIALc();
                    mat.CHAMBERID = "";
                    mat.MATERIALNAME = "";
                    mat.MATERIALTYPE = lampArea;
                    mat.QUANTITY = lampuseTime;
                    materialList.Add(mat);
                    object[] _data = new object[5]
                { 
                    inputData.TrackKey,   /*0  TrackKey*/
                    eqp.Data.LINEID,      /*1  LineName*/
                    eqp.Data.NODEID,      /*2  EQPID*/
                    "",                   /*3  PRODUCTNAME*/
                    materialList,         /*4  materialList*/
                };
                    //呼叫MES方法
                    Invoke(eServiceName.MESService, "ChangeMaterialLife", _data);
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LampUsedTimeReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_LampUsedTimeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                //outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, LapmUsedTimeReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(LapmUsedTimeReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LAMP USED TIME REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LapmUsedTimeReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] LAMP USED TIME REPORT REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));

                LampUsedTimeReportReply(trackKey, eBitResult.OFF, sArray[0]);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.13 Sampling Side Status Report]
        public void SamplingSideStatusReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                string sidestatus = inputData[0][0][0].Value;
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [更新機台資訊]
                //save data in progarm
                switch (eqp.Data.NODEATTRIBUTE.ToUpper())
                {
                    case "COATER":
                        lock (eqp) eqp.File.VCD01 = (eBitResult)int.Parse(sidestatus.Substring(0, 1));
                        lock (eqp) eqp.File.VCD02 = (eBitResult)int.Parse(sidestatus.Substring(1, 1));
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SAMPLING SIDE STATUS REPORT, COATER VCD01=[{2}], VCD02=[{3}].",
                            eqpNo, inputData.TrackKey, eqp.File.VCD01, eqp.File.VCD02));
                        break;
                    case "EXPOSURE":
                        lock (eqp) eqp.File.CP01 = (eBitResult)int.Parse(sidestatus.Substring(0, 1));
                        lock (eqp) eqp.File.CP02 = (eBitResult)int.Parse(sidestatus.Substring(1, 1));
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SAMPLING SIDE STATUS REPORT, EXPOSURE CP01=[{2}], CP02=[{3}].",
                            eqpNo, inputData.TrackKey, eqp.File.CP01, eqp.File.CP02));
                        break;
                    case "OVEN":
                        lock (eqp) eqp.File.HP01 = (eBitResult)int.Parse(sidestatus.Substring(0, 1));
                        lock (eqp) eqp.File.HP02 = (eBitResult)int.Parse(sidestatus.Substring(1, 1));
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SAMPLING SIDE STATUS REPORT, OVEN HP01=[{2}], HP02=[{3}].",
                            eqpNo, inputData.TrackKey, eqp.File.HP01, eqp.File.HP02));
                        break;
                }
                //save progarm data in file
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.14 Q Time Check Request]
        public void QTimeCheckRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string cstseqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    QTimeCheckRequestReply(inputData.TrackKey, eBitResult.OFF, eCFQTime.OK, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON)", inputData.Metadata.NodeNo, inputData.TrackKey));
                }
                #endregion

                #region [計算Q Time是否有超過，下貨時，所得到的Q Time]
                eCFQTime cfQTime = new eCFQTime();
                switch (ObjectManager.QtimeManager.GetQtimeisOver(cstseqNo, jobseqNo, eqp).ToString().ToUpper())
                {
                    case "OK":
                        cfQTime = eCFQTime.OK;
                        break;
                    case "NG":
                        cfQTime = eCFQTime.NG;
                        break;
                    case "RW":
                        cfQTime = eCFQTime.RW;
                        break;
                }
                #endregion

                #region [Reply EQ]
                QTimeCheckRequestReply(inputData.TrackKey, eBitResult.ON, cfQTime, eqpNo);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void QTimeCheckRequestReply(string trxID, eBitResult bitResutl, eCFQTime reslut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_QTimeCheckRequestReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (bitResutl == eBitResult.ON)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)reslut).ToString();
                }
                else
                {
                    // H.S.正常結束，無須填寫 Word
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)bitResutl).ToString(); //寫入bit
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, QTimeCheckRequestReplyTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResutl.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(QTimeCheckRequestTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Q-TIME CHECK REQUEST REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResutl));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 紀錄EQP Clear Bit time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void QTimeCheckRequestTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, Q-TIME CHECK REQUEST TIMEOUT.",
                    sArray[0], trackKey));

                QTimeCheckRequestReply(trackKey, eBitResult.OFF, eCFQTime.OK, sArray[0].ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.15 Tank Change Report]
        public void TankChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string lowconcentrationKOHtanknumber = inputData[0][0][0].Value;
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    TankChangeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) LOW_CONCERNTRATION_KOH_TANK_NUMBER=[{2}]", 
                        inputData.Metadata.NodeNo, inputData.TrackKey, lowconcentrationKOHtanknumber));
                }
                #endregion

                #region [Reply EQ]
                TankChangeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Report MES]
                object[] _data = new object[4]
                { 
                    inputData.TrackKey,               /*0  TrackKey*/
                    eqp.Data.LINEID,                  /*1  LineName*/
                    eqp.Data.NODEID,                  /*2  EQPID*/
                    lowconcentrationKOHtanknumber,    /*3  QUANTITY*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "ChangeTankReport", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void TankChangeReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_TankChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0][0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, TankChangeReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(TankChangeReportTimeoutForEQP), trxID);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] TANK CHANGE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void TankChangeReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TANK CHANGE REPORT TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));

                TankChangeReportReply(trackKey, eBitResult.OFF, sArray[0]);

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.17 Special Mode Report]
        public void BypassMode(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult bypassMode;
            bypassMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm
            lock (eqp) eqp.File.BypassMode = bypassMode;
            //save progarm data in file
            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BYPASS MODE =[{2}].", eqpNo, inputData.TrackKey, bypassMode));
            #endregion
        }

        public void TurnTableMode(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult turnTableMode;
            turnTableMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm
            lock (eqp) eqp.File.TurnTableMode = turnTableMode;
            //save progarm data in file
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TURN TABLE MODE =[{2}].", eqpNo, inputData.TrackKey, turnTableMode));
            #endregion
        }

        public void BypassInspectionEquipment01Mode(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult bypassInspectionEquipment01Mode;
            bypassInspectionEquipment01Mode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm
            lock (eqp) eqp.File.BypassInspectionEquipment01Mode = bypassInspectionEquipment01Mode;
            //save progarm data in file
            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BYPASS INSPECTION EQUIPMENT #01 MODE =[{2}].", eqpNo, inputData.TrackKey, bypassInspectionEquipment01Mode));
            #endregion
        }

        public void BypassInspectionEquipment02Mode(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult bypassInspectionEquipment02Mode;
            bypassInspectionEquipment02Mode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm
            lock (eqp) eqp.File.BypassInspectionEquipment02Mode = bypassInspectionEquipment02Mode;
            //save progarm data in file
            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BYPASS INSPECTION EQUIPMENT #02 MODE =[{2}].", eqpNo, inputData.TrackKey, bypassInspectionEquipment02Mode));
            #endregion
        }

        public void NextLineBCStatus(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult nextLineBCStatus;
            nextLineBCStatus = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm  OFF:代表NG ON:代表OK
            lock (eqp) eqp.File.NextLineBCStatus = nextLineBCStatus;
            //save progarm data in file
            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] NEXTLINE BC STATUS =[{2}].", eqpNo, inputData.TrackKey, nextLineBCStatus));
            #endregion
        }

        public void CV07Status(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult cV07Status = (eBitResult)int.Parse(inputData[0][0][0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm  OFF:代表NG ON:代表OK
            lock (eqp) eqp.File.CV07Status = cV07Status;
            //save progarm data in file
            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CV#07 STATUS =[{2}].", eqpNo, inputData.TrackKey, cV07Status));
            #endregion
        }

        public void IndexerAlignerDispatchMode(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult indexerExposureDispatchMode;
            indexerExposureDispatchMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm  OFF:代表NG ON:代表OK
            lock (eqp) eqp.File.IndexerExposureDispatchMode = indexerExposureDispatchMode;
            //save progarm data in file
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] INDEXER EXPOSURE DISPATCH MODE =[{2}].", eqpNo, inputData.TrackKey, indexerExposureDispatchMode));
            #endregion
 
        }

        public void ReworkForceToUnloaderCST(Trx inputData)
        {
            #region [拆出PLCAgent Data]
            eBitResult reworkForceToUnloaderCST;
            reworkForceToUnloaderCST = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
            #endregion

            #region [取得EQP資訊]
            Equipment eqp;
            string eqpNo = inputData.Metadata.NodeNo;
            eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
            #endregion

            #region [更新機台資訊]
            //save data in progarm  OFF:代表NG ON:代表OK
            lock (eqp) eqp.File.ReworkForceToUnloaderCST = reworkForceToUnloaderCST;
            //save progarm data in file
            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Rework Force To Unloader CST =[{2}].", eqpNo, inputData.TrackKey, reworkForceToUnloaderCST));
            #endregion
 
        }


        #endregion

        #region [9.2.18 Oven CP Safety Check Report]
        public void OvenCPSafetyCheckReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]  待拆解功能完成後，進行拆解
                string ovenCPSafety = inputData[0][0][0].Value;
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [紀錄Log以供查詢]
                // 保存最後一筆 Trx Data
                Repository.Add(inputData.Name, inputData);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] OVEN CP SAFETY BIT =[{2}].", eqp.Data.NODENO, inputData.TrackKey, ovenCPSafety.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion      

        #region [9.2.21 HighCVModeChangeReport]
        public void HighCVModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region[拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//B_HighCVModeChangeReport Bit ON
                eHightCVmode highCVmode = (eHightCVmode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report OFF]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    HighCVModeChangeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON),HighCVModeChangeReport=[{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, highCVmode));
                    HighCVModeChangeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                    lock (eqp)
                    {
                        eqp.File.HighCVMode = highCVmode;
                    }
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void HighCVModeChangeReportReply(string trxID, eBitResult bitResult, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_HighCVModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                #region[Reply Time Out]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + HighCVModeChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + HighCVModeChangeReportTimeout);
                }

                if (bitResult.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + HighCVModeChangeReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(HighCVModeChangeReportReplyTimeout), trxID);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, HIGH CV MODE CHANFE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResult));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HighCVModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], UnloadingPortSettingReportTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] HIGH CV MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_HighCVModeChangeReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("HIGH CV MODE CHANGE REPORT REPLY - EQUIPMENT=[{0}] \"T2 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.22 UnloadingPortSettingReport]
        public void UnloadingPortSettingReport(Trx inputData)
        {
            try
            {
                //if (inputData.IsInitTrigger == true)
                //    return; //程式初始化的時候"不要"執行

                if (inputData.IsInitTrigger) return; //程式初始化的時候"要"執行               

                #region [拆出PLCAgent Data]
                StringBuilder log = new StringBuilder();
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string okqtime = inputData.EventGroups[0].Events[0].Items[0].Value;//OKPortModeStoreQTime
                log.AppendFormat("OK_PORT_MODE_STORE_Q_TIME = [{0}]", okqtime);

                string okproducttypecheck = inputData.EventGroups[0].Events[0].Items[1].Value;  //OKPortModeProductTypeCheckMode
                log.AppendFormat("OK_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", okproducttypecheck);

                string ngqtime = inputData.EventGroups[0].Events[0].Items[2].Value;             //NGPortModeStoreQTime
                log.AppendFormat("NG_PORT_MODE_STORE_Q_TIME = [{0}]", ngqtime);

                string ngproducttypecheck = inputData.EventGroups[0].Events[0].Items[3].Value;  //NGPortModeProductTypeCheckMode
                log.AppendFormat("NG_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", ngproducttypecheck);

                string ngjudge = inputData.EventGroups[0].Events[0].Items[4].Value;             //NGPortJudge
                log.AppendFormat("NG_PORT_JUDGE = [{0}]", ngjudge);

                string pdqtime = inputData.EventGroups[0].Events[0].Items[5].Value;             //PDPortModeStoreQTime
                log.AppendFormat("PD_PORT_MODE_STORE_Q_TIME = [{0}]", pdqtime);

                string pdproducttypecheck = inputData.EventGroups[0].Events[0].Items[6].Value;  //PDPortModeProductTypeCheckMode
                log.AppendFormat("PD_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", pdproducttypecheck);

                string rpqtime = inputData.EventGroups[0].Events[0].Items[7].Value;             //RPPortModeStoreQTime
                log.AppendFormat("RP_PORT_MODE_STORE_Q_TIME = [{0}]", rpqtime);

                string rpproducttypecheck = inputData.EventGroups[0].Events[0].Items[8].Value;  //RPPortModeProductTypeCheckMode
                log.AppendFormat("RP_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", rpproducttypecheck);

                string irqtime = inputData.EventGroups[0].Events[0].Items[9].Value;             //IRPortModeStoreQTime
                log.AppendFormat("IR_PORT_MODE_STORE_Q_TIME = [{0}]", irqtime);

                string irproducttypecheck = inputData.EventGroups[0].Events[0].Items[10].Value;  //IRPortModeProductTypeCheckMode
                log.AppendFormat("IR_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", irproducttypecheck);

                string mixqtime = inputData.EventGroups[0].Events[0].Items[11].Value;             //MIXPortModeStoreQTime
                log.AppendFormat("MIX_PORT_MODE_STORE_Q_TIME = [{0}]", mixqtime);

                string mixproducttypecheck = inputData.EventGroups[0].Events[0].Items[12].Value;  //MIXPortModeProductTypeCheckMode
                log.AppendFormat("MIX_PORT_MODE_PRODUCT_TYPE_CHECK_MODE = [{0}]", mixproducttypecheck);

                string mixjudge = inputData.EventGroups[0].Events[0].Items[13].Value;             //MIXPortJudge
                log.AppendFormat("MIX_PORT_JUDGE = [{0}]", mixjudge);

                string operatorid = inputData.EventGroups[0].Events[0].Items[14].Value;                 //OperatorID
                log.AppendFormat("OPERATOR_ID = [{0}]", operatorid);

                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Reply EQ]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNLOADING PORT SETTING REPORT BIT (OFF)",
                        inputData.Metadata.NodeNo, inputData.TrackKey));
                    UnloadingPortSettingReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNLOADING PORT SETTING REPORT BIT (ON)" + "{2}", 
                        inputData.Metadata.NodeNo, inputData.TrackKey, log));
                    lock (eqp)
                    {
                        eqp.File.OKPortModeStoreQTime = int.Parse(okqtime);
                        eqp.File.OKPort = (ePortModeProductTypeCheck)int.Parse(okproducttypecheck);
                        eqp.File.NGPortModeStoreQTime = int.Parse(ngqtime);
                        eqp.File.NGPort = (ePortModeProductTypeCheck)int.Parse(ngproducttypecheck);
                        eqp.File.NGPortJudge = ngjudge;
                        eqp.File.PDPortModeStoreQTime = int.Parse(pdqtime);
                        eqp.File.PDPort = (ePortModeProductTypeCheck)int.Parse(pdproducttypecheck);
                        eqp.File.RPPortModeStoreQTime = int.Parse(rpqtime);
                        eqp.File.RPPort = (ePortModeProductTypeCheck)int.Parse(rpproducttypecheck);
                        eqp.File.IRPortModeStoreQTime = int.Parse(irqtime);
                        eqp.File.IRPort = (ePortModeProductTypeCheck)int.Parse(irproducttypecheck);
                        eqp.File.MIXPortModeStoreQTime = int.Parse(mixqtime);
                        eqp.File.MIXPort = (ePortModeProductTypeCheck)int.Parse(mixproducttypecheck);
                        eqp.File.MIXPortJudge = mixjudge;
                        eqp.File.OperatorID = operatorid;
                    }
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File); //儲存eqp.file
                    UnloadingPortSettingReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void UnloadingPortSettingReportReply(string trxID, eBitResult bitResult, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_UnloadingPortSettingReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                #region[Reply Time Out]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + UnloadingPortSettingReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + UnloadingPortSettingReportTimeout);
                }

                if (bitResult.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + UnloadingPortSettingReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(UnloadingPortSettingReportReplyTimeout), trxID);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, UNLOADING PORT SETTING REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResult));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void UnloadingPortSettingReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], UnloadingPortSettingReportTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNLOADING PORT SETTING REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_UnloadingPortSettingReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("UNLOADING PORT SETTING REPORT REPLY - EQUIPMENT=[{0}] \"T2 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region[9.2.24	Unloading Port Setting Command]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="OKPortModeStoreQTime"></param>
        /// <param name="OKPortModeProductTypeCheckMode"></param>
        /// <param name="NGPortModeStoreQTime"></param>
        /// <param name="NGPortModeProductTypeCheckMode"></param>
        /// <param name="NGPortJudge"></param>
        /// <param name="PDPortModeStoreQTime"></param>
        /// <param name="PDPortModeProductTypeCheckMode"></param>
        /// <param name="RPPortModeStoreQTime"></param>
        /// <param name="RPPortModeProductTypeCheckMode"></param>
        /// <param name="IRPortModeStoreQTime"></param>
        /// <param name="IRPortModeProductTypeCheckMode"></param>
        /// <param name="MIXPortModeStoreQTime"></param>
        /// <param name="MIXPortModeProductTypeCheckMode"></param>
        /// <param name="MIXPortJudge"></param>
        /// <param name="OperatorID"></param>
        public void UnloadingPortSettingCommand(string trxID, string OKPortModeStoreQTime, string OKPortModeProductTypeCheckMode, 
            string NGPortModeStoreQTime, string NGPortModeProductTypeCheckMode, string NGPortJudge, string PDPortModeStoreQTime, 
            string PDPortModeProductTypeCheckMode, string RPPortModeStoreQTime, string RPPortModeProductTypeCheckMode, 
            string IRPortModeStoreQTime, string IRPortModeProductTypeCheckMode, string MIXPortModeStoreQTime, 
            string MIXPortModeProductTypeCheckMode, string MIXPortJudge, string OperatorID)
        {
            try
            {
                //trxID = this.CreateTrxID();

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                string equipmentNodeNo = string.Empty;
                IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment equipment in equipments)
                {
                    if (equipment.Data.NODEATTRIBUTE.ToUpper() == "UD" || equipment.Data.NODEATTRIBUTE.ToUpper() == "LD")
                    {
                        equipmentNodeNo = equipment.Data.NODENO;
                    }
                }
                #endregion

                #region[CIM MODE OFF 不能下命令]
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(equipmentNodeNo);
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM_MODE=[OFF], CAN NOT SEND UNLOADING PORT SETTING COMMAND!", eqp.Data.NODENO);
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                #region[寫入PLCAgent Data]
                string trxName = string.Format("{0}_UnloadingPortSettingCommand", equipmentNodeNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)//Check Trx
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN NOT FOUND TRX {0}", trxName));
                    return;
                }

                StringBuilder log = new StringBuilder();

                outputdata.EventGroups[0].Events[0].Items[0].Value = OKPortModeStoreQTime;
                log.AppendFormat("OKPortModeStoreQTime=[{0}]", OKPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[1].Value = OKPortModeProductTypeCheckMode;
                log.AppendFormat("OKPortModeProductTypeCheckMode=[{0}]", OKPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[2].Value = NGPortModeStoreQTime;
                log.AppendFormat("NGPortModeStoreQTime=[{0}]", NGPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[3].Value = NGPortModeProductTypeCheckMode;
                log.AppendFormat("NGPortModeProductTypeCheckMode=[{0}]", NGPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[4].Value = NGPortJudge;
                log.AppendFormat("NGPortJudge=[{0}]", NGPortJudge);

                outputdata.EventGroups[0].Events[0].Items[5].Value = PDPortModeStoreQTime;
                log.AppendFormat("PDPortModeStoreQTime=[{0}]", PDPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[6].Value = PDPortModeProductTypeCheckMode;
                log.AppendFormat("PDPortModeProductTypeCheckMode=[{0}]", PDPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[7].Value = RPPortModeStoreQTime;
                log.AppendFormat("RPPortModeStoreQTime=[{0}]", RPPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[8].Value = RPPortModeProductTypeCheckMode;
                log.AppendFormat("RPPortModeProductTypeCheckMode=[{0}]", RPPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[9].Value = IRPortModeStoreQTime;
                log.AppendFormat("IRPortModeStoreQTime=[{0}]", IRPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[10].Value = IRPortModeProductTypeCheckMode;
                log.AppendFormat("IRPortModeProductTypeCheckMode=[{0}]", IRPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[11].Value = MIXPortModeStoreQTime;
                log.AppendFormat("MIXPortModeStoreQTime=[{0}]", MIXPortModeStoreQTime);

                outputdata.EventGroups[0].Events[0].Items[12].Value = MIXPortModeProductTypeCheckMode;
                log.AppendFormat("MIXPortModeProductTypeCheckMode=[{0}]", MIXPortModeProductTypeCheckMode);

                outputdata.EventGroups[0].Events[0].Items[13].Value = MIXPortJudge;
                log.AppendFormat("MIXPortJudge=[{0}]", MIXPortJudge);

                outputdata.EventGroups[0].Events[0].Items[14].Value = OperatorID;
                log.AppendFormat("OperatorID=[{0}]", OperatorID);

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();//UnloadingPortSettingCommand Bit ON
                outputdata.TrackKey = trxID;
                outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();//Bit ON後,延遲EVENTDELAYTIME(200ms),再寫Word

                SendPLCData(outputdata);

                #region[Timeout]
                string timeName = string.Format("{0}_{1}", equipmentNodeNo, UnloadingPortSettingCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(UnloadingPortSettingCommandReplyTimeout), outputdata.TrackKey);
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [{2}], [{3}]", equipmentNodeNo, trxID, eBitResult.ON, log));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName,
                    this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void UnloadingPortSettingCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//UnloadingPortSettingCommandReply Bit
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);//UnloadingPortSettingCommandReply ReturnCode
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Timeout]
                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, UnloadingPortSettingCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [ON]. UNLOADER PORTT SETTING COMMAND REPLY [{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, returnCode));
                }
                #endregion

                #region [Reply EQ]
                string trxName = string.Format("{0}_UnloadingPortSettingCommand", eqp.Data.NODENO);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();//UnloadingPortSettingCommand Bit Off

                outputdata[0][0].IsDisable = true;
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);
               
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void UnloadingPortSettingCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], UnloadingPortSettingCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNLOADING PORT SETTING COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_UnloadingPortSettingCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Unloading Port Setting Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.23	BufferRWJudgeCapacityChangeReport]
        public void BufferRWJudgeCapacityChangeReport(Trx inputData)
        {
            try
            {
                //if (inputData.IsInitTrigger == true)
                //    return; //程式初始化的時候"不要"執行

                if (inputData.IsInitTrigger) return; //程式初始化的時候"要"執行

                #region[拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//B_BufferRWJudgeCapacityChangeReport Bit ON
                string cvBUF = inputData.EventGroups[0].Events[0].Items[0].Value;//CV#06 Buffer Judge Capacity Info
                string b01RW = inputData.EventGroups[0].Events[0].Items[1].Value;//Buffer#01 RW Judge Capacity
                string b02RW = inputData.EventGroups[0].Events[0].Items[2].Value;//Buffer#02 RW Judge Capacity
                string operID = inputData.EventGroups[0].Events[0].Items[3].Value;//Operator ID
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report OFF]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    BufferRWJudgeCapacityChangeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON), CV#06BufferJudgeCapacityInfo=[{2}], Buffer#01RWJudgeCapacity=[{3}], Buffer#02RWJudgeCapacity=[{4}], OperatorID=[{5}]"
                        , inputData.Metadata.NodeNo, inputData.TrackKey, cvBUF, b01RW, b02RW, operID));
                    lock (eqp)
                    {
                        eqp.File.CV06BufferInfo = cvBUF;
                        eqp.File.Buffer01RWJudgeCapacity = b01RW;
                        eqp.File.Buffer02RWJudgeCapacity = b02RW;
                        eqp.File.OperatorID = operID;
                    }
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File); //儲存eqp.file
                    BufferRWJudgeCapacityChangeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void BufferRWJudgeCapacityChangeReportReply(string trxID, eBitResult bitResult, string eqpNo)
        {
            try
            {

                string trxName = string.Format("{0}_BufferRWJudgeCapacityChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                #region[Reply Time Out]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + BufferRWJudgeCapacityChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + BufferRWJudgeCapacityChangeReportTimeout);
                }

                if (bitResult.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + BufferRWJudgeCapacityChangeReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(BufferRWJudgeCapacityChangeReportReplyTimeout), trxID);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, BUFFER RW JUDGE CAPACITY CHANGE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResult));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void BufferRWJudgeCapacityChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], BufferRWJudgeCapacityChangeReportTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BUFFER RW JUDGE CAPACITY CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_BufferRWJudgeCapacityChangeReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("BUFFER RW JUDGE CAPACITY CHANGE REPORT REPLY - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion        
               
        #region [9.2.25	BufferRWJudgeCapacityChangeCommand]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="cvBUF"></param>
        /// <param name="b01RW"></param>
        /// <param name="b02RW"></param>
        /// <param name="operID"></param>
        public void BufferRWJudgeCapacityChangeCommand(string trxID, string cvBUF, string b01RW, string b02RW, string operID)
        {
            try
            {
                //trxID = this.CreateTrxID();

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                string equipmentNodeNo = string.Empty;
                IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment equipment in equipments)
                {
                    if (equipment.Data.NODEATTRIBUTE.ToUpper() == "BF")
                        equipmentNodeNo = equipment.Data.NODENO;
                }
                #endregion

                #region[CIM MODE OFF 不能下命令]
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(equipmentNodeNo);
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM_MODE=[OFF], CAN NOT SEND BUFFER RW JUDGE CAPACITY CHANGE COMMAND!", eqp.Data.NODENO);
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                #region[寫入PLCAgent Data]
                string trxName = string.Format("{0}_BufferRWJudgeCapacityChangeCommand", equipmentNodeNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)//Check Trx
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN NOT FOUND TRX {0}", trxName));
                    return;
                }

                //string log = string.Empty;
                StringBuilder log = new StringBuilder();

                outputdata.EventGroups[0].Events[0].Items[0].Value = cvBUF;
                log.AppendFormat("CV#06BufferJudgeCapacityInfo=[{0}]", cvBUF);

                outputdata.EventGroups[0].Events[0].Items[1].Value = b01RW;
                log.AppendFormat("Buffer#01RWJudgeCapacity=[{0}]", b01RW);

                outputdata.EventGroups[0].Events[0].Items[2].Value = b02RW;
                log.AppendFormat("Buffer#02RWJudgeCapacity=[{0}]", b02RW);

                outputdata.EventGroups[0].Events[0].Items[3].Value = operID;
                log.AppendFormat("OperatorID=[{0}]", operID);

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString(); //BufferRWJudgeCapacityChangeCommand Bit ON
                outputdata.TrackKey = trxID;
                outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Bit ON後,延遲EVENTDELAYTIME(200ms),再寫Word

                SendPLCData(outputdata);

                #region[Timeout]
                string timeName = string.Format("{0}_{1}", equipmentNodeNo, BufferRWJudgeCapacityChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(BufferRWJudgeCapacityChangeCommandReplyTimeout), outputdata.TrackKey);
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [{2}], [{3}]", equipmentNodeNo, trxID, eBitResult.ON, log));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName,
                    this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void BufferRWJudgeCapacityChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//BufferRWJudgeCapacityChangeCommandReply Bit
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);//BufferRWJudgeCapacityChangeCommandReply ReturnCode
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Timeout]
                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, BufferRWJudgeCapacityChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [ON]. BUFFER RW JUDGE CAPACITY CHANGE COMMAND REPLY [{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, returnCode));
                }
                #endregion

                #region [Reply EQ]
                string trxName = string.Format("{0}_BufferRWJudgeCapacityChangeCommand", eqp.Data.NODENO);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();//BufferRWJudgeCapacityChangeCommand Bit OFF
                
                outputdata[0][0].IsDisable = true;
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void BufferRWJudgeCapacityChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], BufferRWJudgeCapacityChangeCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BUFFER RW JUDGE CAPACITY CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_BufferRWJudgeCapacityChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("BUFFER RW JUDGE CAPACITY CHANGE COMMAND REPLY - EQUIPMENT=[{0}] \"T2 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.26	GlassReworkJudgeReport]
        public void GlassReworkJudgeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region[拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//B_GlassReworkJudgeReport Bit ON
                string cstseq = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseq = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitORport = inputData.EventGroups[0].Events[0].Items[2].Value;
                string unitno = inputData.EventGroups[0].Events[0].Items[3].Value;
                string portno = inputData.EventGroups[0].Events[0].Items[4].Value;
                string slotno = inputData.EventGroups[0].Events[0].Items[5].Value;
                string operID = inputData.EventGroups[0].Events[0].Items[6].Value;
                #endregion

                #region [取得Line資訊
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report OFF]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassReworkJudgeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON), CASSETTE SEQUENCE NO=[{2}], JOB SEQUENCE NO=[{3}], UNIT OR UNIT=[{4}], UNITNO=[{5}], PORTNO=[{6}], SLOT NO=[{7}], OPERATOR ID=[{8}]"
                    , inputData.Metadata.NodeNo, inputData.TrackKey, cstseq, jobseq, unitORport, unitno, portno, slotno, operID));
                    GlassReworkJudgeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                }
                #endregion

                #region [Report MES]
                Job job = ObjectManager.JobManager.GetJob(cstseq,jobseq);
                lock (job)
                {
                    job.JobJudge = "3"; //Update Job Judge = RW
                }
                object[] _data = new object[5]
                { 
                    inputData.TrackKey,       /*0  TrackKey*/
                    eqp.Data.LINEID,          /*1  LineName*/
                    eqpNo,                    /*2  EQPNo*/
                    unitno,                   /*3  UnitNo*/
                    job,                      /*4  Job*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "GlassReworkJudgeReport", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassReworkJudgeReportReply(string trxID, eBitResult bitResult, string eqpNo)
        {
            try
            {

                string trxName = string.Format("{0}_GlassReworkJudgeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                #region[Reply Time Out]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassReworkJudgeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + GlassReworkJudgeReportTimeout);
                }

                if (bitResult.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + GlassReworkJudgeReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(GlassReworkJudgeReportReplyTimeout), trxID);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, GLASS REWORK JUDGE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResult));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void GlassReworkJudgeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], GlassReworkJudgeReportTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS REWORK JUDGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_GlassReworkJudgeReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("GLASS REWORK JUDGE REPORT REPLY - EQUIPMENT=[{0}] \"T2 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.27	InlineReworkModeChangeReport]
        public void InlineReworkModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region[拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//B_InlineReworkModeChangeReport Bit ON
                string inlinereworkmode = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Reply EQ]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    InlineReworkModeChangeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON),InlineReworkModeChangeReport=[{2}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, inlinereworkmode));                    
                    lock (eqp)
                    {
                        eqp.File.InlineRework = (ePortEnableMode)int.Parse(inlinereworkmode);
                    }
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File); //儲存eqp.file
                    InlineReworkModeChangeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InlineReworkModeChangeReportReply(string trxID, eBitResult bitResult, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_InlineReworkModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                #region[Reply Time Out]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + InlineReworkModeChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InlineReworkModeChangeReportTimeout);
                }

                if (bitResult.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InlineReworkModeChangeReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(InlineReworkModeChangeReportReplyTimeout), trxID);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, INLINE REWORK MODE CHANGE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResult));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InlineReworkModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], InlineReworkModeChangeReportTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INLINE REWORK MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_InlineReworkModeChangeReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("INLINE REWORK MODE CHANGE REPORT REPLY - EQUIPMENT=[{0}] \"T2 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region[9.2.28 Exposure Mask Check Command]
        public void ExposureMaskCheckCommand(string trxID, string recipe, string equipmentNodeNo)
        {
            try
            {
                //trxID = this.CreateTrxID();

                #region [取得EQP資訊]
                //string equipmentNodeNo = string.Empty;
                //IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                //foreach (Equipment equipment in equipments)
                //{
                //    if (equipment.Data.NODEATTRIBUTE.ToUpper() == "EX")
                //        equipmentNodeNo = equipment.Data.NODENO;
                //}
                #endregion

                #region[CIM MODE OFF 不能下命令]
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(equipmentNodeNo);
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM_MODE=[OFF], CAN NOT SEND EXPOSURE MASK CHECK COMMAND!", eqp.Data.NODENO);
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                #region[寫入PLCAgent Data]
                string trxName = string.Format("{0}_ExposureMaskCheckCommand", equipmentNodeNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)//Check Trx
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN NOT FOUND TRX {0}", trxName));
                    return;
                }
                
                outputdata.EventGroups[0].Events[0].Items[0].Value = recipe;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();//ExposureMaskCheckCommand Bit ON
                outputdata.TrackKey = trxID;
                outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();//Bit ON後,延遲EVENTDELAYTIME(200ms),再寫Word

                SendPLCData(outputdata);

                #region[Timeout]
                string timeName = string.Format("{0}_{1}", equipmentNodeNo, ExposureMaskCheckCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ExposureMaskCheckCommandReplyTimeout), outputdata.TrackKey);
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [{2}], [{3}]", equipmentNodeNo, trxID, eBitResult.ON, recipe));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName,
                    this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ExposureMaskCheckCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);//ExposureMaskCheckCommandReply Bit
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);//ExposureMaskCheckCommandReply ReturnCode
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Timeout]
                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ExposureMaskCheckCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [ON]. EXPOSURE MASK CHECK COMMAND REPLY [{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, returnCode));
                }
                #endregion

                #region [Reply EQ]
                string trxName = string.Format("{0}_ExposureMaskCheckCommand", eqp.Data.NODENO);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();//ExposureMaskCheckCommand Bit Off

                outputdata[0][0].IsDisable = true;
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);
               
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ExposureMaskCheckCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], UnloadingPortSettingCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EXPOSURE MASK CHECK REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_ExposureMaskCheckCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Exposure Mask Check Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        public void AnalysisExposuePPID(string trackKey, string lineName, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos)
        {
            try
            {
                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", lineName));
                #endregion

                if (recipeCheckInfos.ContainsKey(lineName))
                {
                    foreach (RecipeCheckInfo rci in recipeCheckInfos[lineName])
                    {
                        if (rci.EQPNo == "L10")
                            ExposureMaskCheckCommand(trackKey, rci.RecipeID, rci.EQPNo);
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void AnalysisExposuePPID(string trackKey, string lineName, IList<RecipeCheckInfo> recipeCheckInfos)
        {
            try
            {
                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", lineName));
                #endregion

                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    if (rci.EQPNo == "L10")
                        ExposureMaskCheckCommand(trackKey, rci.RecipeID, rci.EQPNo);
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}
