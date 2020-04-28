using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.EntityManager;
using System.Text.RegularExpressions;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class AlarmService : AbstractService
    {
        private string AlarmReportTimeout = "AlarmReportTimeout";
        
        public override bool Init()
        {
            return true;
        }

        /// <summary>
        /// Alarm Status Change Report
        /// </summary>
        public void AlarmStatusChangeReport(Trx inputData)
        {
            bool IsBFG = false;
            if (inputData.Name.Substring(inputData.Name.Length - 3, 3) == "BFG") IsBFG = true;

            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = IsBFG ? "L3" : inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                string[] tmp = inputData.Name.Split('_');
                string name = tmp[tmp.Length - 1];
                
                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS<-EQP][{1}] BIT=[OFF].", eqpNo, inputData.TrackKey));
                    AlarmStatusChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, IsBFG, inputData.TrackKey);
                    return;
                }

                string log = string.Empty;
                Event evt = inputData.EventGroups[0].Events[0];
                #region [拆出PLCAgent Data]
                string alarmSts = ConstantManager["ALARMSTATUS"][evt.Items[0].Value].Value;
                log += string.Format(" ALARM_STATUS=[{0}]({1})", evt.Items[0].Value, alarmSts);

                string alarmUnit = evt.Items[1].Value;
                log += string.Format(" ALARM_UNIT=[{0}]", evt.Items[1].Value);

                string alarmID = evt.Items[2].Value;
                log += string.Format(" ALARM_ID=[{0}]", evt.Items[2].Value);
                
                string alarmCode = evt.Items[3].Value;
                log += string.Format(" ALARM_CODE=[{0}]", evt.Items[3].Value);

                string alarmLvl = ConstantManager["ALARMLEVEL"][evt.Items[4].Value].Value;
                log += string.Format(" ALARM_LEVEL=[{0}]({1})", evt.Items[4].Value, alarmLvl);

                bool useBcText = evt.Items[5].Value.Equals("1");
                log += string.Format(" ALARM_TEXT_USING_FLAG=[{0}]({1})", evt.Items[5].Value,
                    useBcText ? "USE_BC" : "USE_EQ");
                
                string alarmTxt = evt.Items[6].Value.Trim();
                alarmTxt = Regex.Replace(alarmTxt, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                log += string.Format(" ALARM_TEXT=[{0}]", evt.Items[6].Value);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS<-EQP][{1}] BIN=[ON] CIM_MODE=[{2}]{3}.",
                    inputData.Metadata.NodeNo, inputData.TrackKey, eqp.File.CIMMode, log));

                AlarmStatusChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, IsBFG, inputData.TrackKey);
                //add by qiumin 20171124 CF Photo 取消CV 及LUL的warning report
                if (eqp.Data.LINEID.Contains("FC") && eqp.Data.LINEID.Contains("PH")&&(eqp.Data.NODEID.Contains("CV") || eqp.Data.NODENO == "L2" || eqp.Data.NODENO == "L22") && alarmLvl == "W")
                {
                    return;
                }
                #region [CF Report MES - MachineInspectionOverRatio]
                string _overResult = string.Empty;
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                switch (line.Data.LINETYPE)
                {
                    case eLineType.CF.FCMPH_TYPE1:
                    case eLineType.CF.FCRPH_TYPE1:
                    case eLineType.CF.FCBPH_TYPE1:
                    case eLineType.CF.FCGPH_TYPE1:
                    case eLineType.CF.FCSPH_TYPE1:
                    case eLineType.CF.FCOPH_TYPE1: 
                        if ((alarmID == "65507") || (alarmID == "65508"))   //Watson  Modify 20150326 For CF Photo Line 僅有65507,65508才會上報MachineInspectionOverRatio
                        {
                            if (alarmSts == eALARM_STATE.SET)
                            {
                                switch (alarmID)
                                {
                                    case "65507": //Sampling Rule Bypass First Warning
                                        _overResult = "A";
                                        break;
                                    case "65508": //Sampling Rule Bypass Second Warning
                                        _overResult = "W";
                                        break;
                                }
                            }
                            else if (alarmSts == eALARM_STATE.CLEAR)
                            {
                                _overResult = "N";
                            }

                            object[] _mesData = new object[4]
                            { 
                                inputData.TrackKey,      /*0  TrackKey*/
                                eqp.Data.LINEID,         /*1  LineName*/
                                eqp.Data.NODEID,         /*2  MachineName*/
                                _overResult,             /*3  OverResult*/
                            };
                            //呼叫MES方法
                            Invoke(eServiceName.MESService, "MachineInspectionOverRatio", _mesData);
                        }
                        if (eqp.Data.NODENO == "L10" && ((alarmID == "00033") || (alarmID == "00034") || (alarmID == "33") || (alarmID == "34"))) //Add by  qiumin 20170927 Aligner check warning show down
                        {
                            string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            if (alarmSts == eALARM_STATE.SET)
                            {
                                lock (eqp)
                                {
                                    eqp.File.MESStatus = "DOWN";
                                }
                                
                            }
                            else if (alarmSts == eALARM_STATE.CLEAR && eqp.File.Status != eEQPStatus.STOP)
                            {
                                lock (eqp)
                                {

                                    eqp.File.MESStatus = EQPStatus2MESStatus2(eqp.File.Status);
                                }
                            }
                            object[] obj = new object[]          
                            {
                            inputData.TrackKey ,                               /*0 TrackKey*/
                            eqp,                                               /*1 Equipment*/
                            "BC2MES-0001",                                     /*2 alarmID*/
                            "Aligner check warning show down",                 /*3 alarmText*/
                            alarmTime                                      /*4 alarmTime*/
                            };
                            Invoke(eServiceName.MESService, "MachineStateChanged", obj);
                        }
                        break;
                }
                #endregion

                // MES Data 
                object[] _data = new object[8]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    alarmUnit,          /*3 UnitID*/
                    alarmID,            /*4 AlarmID*/ 
                    alarmLvl,           /*5 AlarmLevel*/
                    alarmSts,           /*6 AlarmState*/
                    alarmTxt,           /*7 AlarmText*/
                };

                // DB History
                ALARMHISTORY his = new ALARMHISTORY();
                his.EVENTNAME = "";
                his.UPDATETIME = DateTime.Now;
                his.ALARMID = alarmID;
                his.ALARMCODE = inputData.EventGroups[0].Events[0].Items[3].Value;
                his.ALARMLEVEL = alarmLvl;
                his.ALARMTEXT = alarmTxt;
                his.ALARMSTATUS = alarmSts;
                his.NODEID = eqp.Data.NODEID;
                his.ALARMUNIT = alarmUnit;
                his.TRANSACTIONID = inputData.TrackKey;

                #region [Check Unit]
                _data[3] = string.Empty;
                if (!alarmUnit.Equals("0"))
                {
                    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, alarmUnit);
                    if (unit != null)
                    {
                        _data[3] = his.ALARMUNIT = unit.Data.UNITID;
                    }
                }
                #endregion

                AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, alarmUnit, alarmID);

                if (alarm != null)
                {
                    // Check Alarm Text Using Flag
                    if (useBcText) _data[7] = his.ALARMTEXT = alarm.ALARMTEXT;
                }
                else
                {
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("NOT FOUND ALARM! (EQUIPMENT_NO=[{0}] ALARM_ID=[{1}] ALARM_LEVEL=[{2}]), BC CREATE ALARM PROFILE!", inputData.Metadata.NodeNo, his.ALARMID, his.ALARMLEVEL));
                    #region [新增新的Alarm設定檔]
                    alarm = new AlarmEntityData()
                    {
                        LINEID = eqp.Data.LINEID,
                        NODENO = eqp.Data.NODENO,
                        UNITNO = alarmUnit,
                        ALARMLEVEL = alarmLvl,
                        ALARMID = alarmID,
                        ALARMCODE = alarmCode,
                        ALARMTEXT = alarmTxt,
                        SERVERNAME = ServerName
                    };
                    ObjectManager.AlarmManager.CreateAlarmProfile(alarm);
                    #endregion
                }

                HandleAlarm(eqp, alarm, _data, his, inputData.TrackKey);

                //AlarmStatusChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    AlarmStatusChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, IsBFG, inputData.TrackKey);
                }
            }
        }

        public void HandleAlarm(Equipment eqp, AlarmEntityData alarm, object[] obj, ALARMHISTORY his, string trackKey)
        {
            try
            {
                if (his.ALARMUNIT != "A1")  //add by bruce 20160216 不是 Array 附屬設備才處理
                {
                    // 記錄及清除機台當前的Alarm
                    switch (his.ALARMSTATUS)
                    {
                        case eALARM_STATE.SET:
                            {
                                if (ObjectManager.AlarmManager.CheckAlarmState(eqp.Data.NODENO, his.ALARMID))
                                {
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("ALARM_ID=[{0}] EQUIPMENT_NO=[{1}], ALARM IS ALREADY INVOKED!", his.ALARMID, eqp.Data.NODENO));
                                    
                                }
                                else
                                {
                                    ObjectManager.AlarmManager.AddHappeningAlarm(alarm);
                                }
                            }
                            break;
                        case eALARM_STATE.CLEAR:
                            {
                                if (!ObjectManager.AlarmManager.CheckAlarmState(eqp.Data.NODENO, his.ALARMID))
                                {
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("ALARM_ID=[{0}] EQUIPMENT_NO=[{1}], ALARM IS ALREADY CLEARED!", his.ALARMID, eqp.Data.NODENO));
                                }
                                else
                                {
                                    ObjectManager.AlarmManager.DeleteHappeningAlarm(alarm);
                                }
                            }
                            break;
                    }
                }
                else
                {
                    his.ALARMUNIT = ""; // add by bruce 20160219 Array 附屬設備填入空值
                }
                // 1: Report to MES Agent
                Invoke(eServiceName.MESService, "AlarmReport", obj);

                // 2: Record to DB
                ObjectManager.AlarmManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AlarmStatusChangeReportReply(string eqpNo, eBitResult value, bool isBFG, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_AlarmStatusChangeReportReply{1}", eqpNo, isBFG ? "BFG" : "");

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + this.AlarmReportTimeout + (isBFG ? "BFG" : "")))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + this.AlarmReportTimeout + (isBFG ? "BFG" : ""));
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + this.AlarmReportTimeout + (isBFG ? "BFG" : ""), false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(AlarmStatusChangeReportReplyTimeout), trackKey + ";" + isBFG.ToString());
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + (isBFG ? "BFG()" : "()"), 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //==========Handle Event Timeout============
        private void AlarmStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ALARM STATUS CHANGE REPORT REPLY{2} TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, (isBFG ? " BFG" : "")));

                AlarmStatusChangeReportReply(sArray[0], eBitResult.OFF, isBFG, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //==========
        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
        private string EQPStatus2MESStatus2(eEQPStatus eqpStatus)   //Add qiumin 20170929  EQ status change to MES status
        {
            switch (eqpStatus)
            {
                case eEQPStatus.IDLE:
                case eEQPStatus.RUN: return eqpStatus.ToString();
                default: return "DOWN";
            }
        }
    }
}
