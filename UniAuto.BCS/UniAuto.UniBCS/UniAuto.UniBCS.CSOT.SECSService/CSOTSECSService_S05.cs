using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using System.Collections.Generic;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class CSOTSECSService
    {
        #region Recipte form equipment-S5
        public void S5F0_E_AbortTransaction(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F0_E");
            #region Handle Logic
			try {
				string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
				string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
				Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
				if (eqp == null) {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                           "Can not find Equipment Number in EquipmentEntity!");
					return;
				}

				string rtnID = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
				if (string.IsNullOrEmpty(rtnID)) {
					return;
				}
				
				//check if opi request
				string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
				switch (rtn) {
					case "OPI":
						Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
							new object[4] { tid, eqp.Data.LINEID, "Reply S5F0", _common.ToFormatString(recvTrx.OuterXml) });
						break;
				}
			} catch (Exception ex) {
				NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
			}
            #endregion
        }
        public void S5F1_E_AlarmReportSend(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F1_E");
            #region Handle Logic
            //Logic handle.
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                //Fine Node
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            "Can not find Equipment Number in EquipmentEntity!");
                    TS5F2_H_AlarmReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS5F2_H_AlarmReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                //Fine Unit
                string unitId = recvTrx["secs"]["message"]["body"]["array1"]["SUBEQUIPMENTID"].InnerText.Trim();
                Unit unit = null;
                if (unitId != eqp.Data.NODEID.Trim())
                {
                    unit = ObjectManager.UnitManager.GetUnit(unitId);
                    if (unit == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Can not find Unit ID ({0}) in UnitEntity!", unitId));
                        return;
                    }
                }
                else
                    unitId = string.Empty;
                string alarmCode = recvTrx["secs"]["message"]["body"]["array1"]["ALCD"].InnerText.Trim();
                string alarmID = recvTrx["secs"]["message"]["body"]["array1"]["ALID"].InnerText.Trim();
                string alarmText = recvTrx["secs"]["message"]["body"]["array1"]["ALTX"].InnerText.Trim();
                string alarmLevel = ConstantManager["CSOTSECS_ALARMLEVEL"][recvTrx["secs"]["message"]["body"]["array1"]["ALLEVEL"].InnerText.Trim()].Value;
                string codeBinary = Convert.ToString(Convert.ToByte(alarmCode, 16), 2).PadLeft(8, '0');
                string alarmState = ConstantManager["ALARMSTATUS"][codeBinary.Substring(0, 1)].Value;
                string alarmCategory = "Other Categories";
                try
                {
                    //alarmCategory = ConstantManager["CSOTSECS_ALARMCATEGORIES"][Convert.ToByte(codeBinary, 2).ToString()].Value;  
                    alarmCategory = ConstantManager["CSOTSECS_ALARMCATEGORIES"][Convert.ToByte(codeBinary.Substring(1, 7), 2).ToString()].Value;  //wucc 20150713 取後7碼
                }
                catch { };
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",eqpno,true,tid,
                                string.Format("Alarm ({0})({1}), ALID ({2}), ALCD ({3}), ALCATEGORY ({4})", alarmState, alarmText, alarmID, alarmCode, alarmCategory));
                S5Fx_AlarmReportHandle(eqp, unit, alarmID, alarmCode, alarmLevel, alarmText, alarmState, tid);
                //// Set MES Data 
                //object[] mesParameter = new object[8]
                //{ 
                //    tid,  /*0 TrackKey*/
                //    eqp.Data.LINEID,    /*1 LineName*/
                //    eqp.Data.NODEID,    /*2 EQPID*/
                //    unitId,          /*3 UnitID*/
                //    alarmID,            /*4 AlarmID*/ 
                //    alarmLevel,           /*5 AlarmLevel*/
                //    alarmState,           /*6 AlarmState*/
                //    alarmText,           /*7 AlarmText*/
                //};
                //// Set DB History
                //ALARMHISTORY hisTrx = new ALARMHISTORY();
                //hisTrx.EVENTNAME = "";
                //hisTrx.UPDATETIME = DateTime.Now;
                //hisTrx.ALARMID = alarmID;
                //hisTrx.ALARMCODE = alarmCode;
                //hisTrx.ALARMLEVEL = alarmLevel;
                //hisTrx.ALARMTEXT = alarmText;
                //hisTrx.ALARMSTATUS = alarmState;
                //hisTrx.NODEID = eqp.Data.NODEID;
                //hisTrx.ALARMUNIT = unit == null ? "0" : unit.Data.UNITNO;
                ////Check and Set DB Entity Data
                //AlarmEntityData alarmEntity = null;
                //if (unit == null)
                //    alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqpno, "0", alarmID); //20150210 cy:因為AlarmManager變更做修改.
                //else
                //    alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqpno, unit.Data.UNITNO, alarmID);

                //if (alarmEntity == null)
                //{
                //    _common.LogWarn(this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",eqpno,true,tid,
                //        string.Format("Not found alarm(Unit({2}), AlarmID({0}), AlarmLevel({1})", hisTrx.ALARMID, hisTrx.ALARMLEVEL, hisTrx.ALARMUNIT));
                //    #region [新增新的Alarm設定檔]
                //    alarmEntity = new AlarmEntityData()
                //    {
                //        LINEID = eqp.Data.LINEID,
                //        NODENO = eqp.Data.NODENO,
                //        UNITNO = unit == null ? "0" : unit.Data.UNITNO,
                //        ALARMLEVEL = alarmLevel,
                //        ALARMID = alarmID,
                //        ALARMCODE = alarmCode,
                //        ALARMTEXT = alarmText,
                //        SERVERNAME = ServerName
                //    };
                //    ObjectManager.AlarmManager.CreateAlarmProfile(alarmEntity);
                //    #endregion
                //}
                //Invoke(eServiceName.AlarmService, "HandleAlarm", new object[] { eqp, alarmEntity, mesParameter, hisTrx, tid });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        private void S5Fx_AlarmReportHandle(Equipment eqp, Unit unit, string id, string code, string level, string text, string state, string trxid)
        {
            
            // Set MES Data 
            object[] mesParameter = new object[8]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    unit == null ? string.Empty : unit.Data.UNITID,          /*3 UnitID*/
                    id,            /*4 AlarmID*/ 
                    level,           /*5 AlarmLevel*/
                    state,           /*6 AlarmState*/
                    text,           /*7 AlarmText*/
                };
            // Set DB History
            ALARMHISTORY hisTrx = new ALARMHISTORY();
            hisTrx.EVENTNAME = "";
            hisTrx.UPDATETIME = DateTime.Now;
            hisTrx.ALARMID = id;
            hisTrx.ALARMCODE = code;
            hisTrx.ALARMLEVEL = level;
            hisTrx.ALARMTEXT = text;
            hisTrx.ALARMSTATUS = state;
            hisTrx.NODEID = eqp.Data.NODEID;
            hisTrx.ALARMUNIT = unit == null ? "0" : unit.Data.UNITNO;
            //Check and Set DB Entity Data
            AlarmEntityData alarmEntity = null;
            if (unit == null)
                alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqp.Data.NODENO, "0", id); //20150210 cy:因為AlarmManager變更做修改.
            else
                alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqp.Data.NODENO, unit.Data.UNITNO, id);

            if (alarmEntity == null)
            {
                _common.LogWarn(this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqp.Data.NODENO, true, trxid,
                    string.Format("Not found alarm(Unit({2}), AlarmID({0}), AlarmLevel({1})", hisTrx.ALARMID, hisTrx.ALARMLEVEL, hisTrx.ALARMUNIT));
                #region [新增新的Alarm設定檔]
                alarmEntity = new AlarmEntityData()
                {
                    LINEID = eqp.Data.LINEID,
                    NODENO = eqp.Data.NODENO,
                    UNITNO = unit == null ? "0" : unit.Data.UNITNO,
                    ALARMLEVEL = level,
                    ALARMID = id,
                    ALARMCODE = code,
                    ALARMTEXT = text,
                    SERVERNAME = ServerName
                };
                ObjectManager.AlarmManager.CreateAlarmProfile(alarmEntity);
                #endregion
            }
            Invoke(eServiceName.AlarmService, "HandleAlarm", new object[] { eqp, alarmEntity, mesParameter, hisTrx, trxid });
        }
        public void S5F6_E_ListAlarmData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F6_E");
            #region Handle Logic
			try {
				//get basic
				string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
				string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
				string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
				string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

				//get eqp object
				Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
				if (eqp == null) {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                           "Can not find Equipment Number in EquipmentEntity!");
					return;
				}

		  //<array1 name="List" type="L" len="?">
		  //  <array2 name="List" type="L" len="4">
		  //    <ALCD name="ALCD" type="B" len="1" fixlen="False" />
		  //    <ALID name="ALID" type="U4" len="1" fixlen="False" />
		  //    <ALTX name="ALTX" type="A" len="80" fixlen="False" />
		  //    <ALLEVEL name="ALLEVEL" type="A" len="1" fixlen="False" />
		  //  </array2>
		  //</array1>
				//body
                List<string> alarms = new List<string>();
				XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"];
				string len = xNode.Attributes["len"].InnerText.Trim();
				int loop = 0;
				int.TryParse(len, out loop);
				for (int i = 0; i < loop; i++) {
					string alcd = xNode.ChildNodes[i]["ALCD"].InnerText.Trim();
					string alid = xNode.ChildNodes[i]["ALID"].InnerText.Trim();
					string altx = xNode.ChildNodes[i]["ALTX"].InnerText.Trim();
					string allevel = xNode.ChildNodes[i]["ALLEVEL"].InnerText.Trim();
                    alarms.Add(alid);
				}

				//check if opi request
				string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
				switch (rtn) {
					case "OPI":
						Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
							new object[4] { tid, eqp.Data.LINEID, "Reply S1F2", _common.ToFormatString(recvTrx.OuterXml) });
						break;
				}

                ////20150323 cy:比對記錄中的happing alarm,若不存在機台報的happing alarm,表示已清除,則報clean
                //#region 更新alarm history
                //AlarmHistoryFile hisAlarm = ObjectManager.AlarmManager.GetEQPAlarm(eqpno);
                ////History沒Alarm,機台有Alarm
                //#endregion
            } catch (Exception ex) {
				NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
			}
            #endregion
        }
        #endregion

        #region Send by host-S5
        public void TS5F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S5F0_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S5F0_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS5F2_H_AlarmReportAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, byte ack)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S5F2_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S5F2_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["ACKC5"].InnerText = ack.ToString();
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS5F5_H_ListAlarmRequest(string eqpno, string eqpid, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }

                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S5F5_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S5F5_H)");
                    return;
                }
                
				//Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S5F5_H";
                sendTrx["secs"]["message"]["return"].InnerText = tag;
                
				//Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Sent form host-S5
        public void S5F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F0_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F0_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S5F2_H_AlarmReportAcknowledge(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F2_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F2_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S5F5_H_ListAlarmRequest(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F5_H T3-Timeout", false);
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                    string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                        return;
                    }
                    switch (rtn)
                    {
                        case "OPI":
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                new object[4] { tid, eqp.Data.LINEID, "Rqeuest S5F5", "T3 Timeout" });
                            break;
                    }
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F5_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}