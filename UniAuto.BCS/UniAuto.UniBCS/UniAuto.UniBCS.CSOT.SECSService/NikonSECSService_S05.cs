using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using System.Collections.Generic;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class NikonSECSService
    {
        #region Recipte form equipment-S5
        public void S5F0_E_AbortTransaction(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F0_E");
            #region Handle Logic
            //TODO:Check control mode and abort setting and reply SnF0 or not.
            //TODO:Logic handle.
            #endregion
        }
        public void S5F1_E_AlarmReportSend(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F1_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS5F2_H_AlarmReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS5F2_H_AlarmReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                string unitId = string.Empty;
                string alarmCode = recvTrx["secs"]["message"]["body"]["array1"]["ALCD"].InnerText.Trim();
                string alarmID = recvTrx["secs"]["message"]["body"]["array1"]["ALID"].InnerText.Trim();
                string alarmText = recvTrx["secs"]["message"]["body"]["array1"]["ALTX"].InnerText.Trim();
                string alarmLevel = ConstantManager["ALARMLEVEL"]["1"].Value;
                string codeBinary = Convert.ToString(Convert.ToByte(alarmCode, 16), 2).PadLeft(8, '0');
                string alarmState = ConstantManager["ALARMSTATUS"][codeBinary.Substring(0, 1)].Value;
                string alarmCategory =  ConvertAlarmCategories(Convert.ToByte(codeBinary, 2).ToString());
                string plPlateID_1 = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PL1PLATEID"].InnerText.Trim();
                string plPlateID_2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PL2PLATEID"].InnerText.Trim();
                string psPlateID = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PSPLATEID"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Alarm ({0})({1}], ALID({2}], ALCD({3}], ALCATEGORY({4}], PL1_PlateID({5}], PL2_PlateID({6}], PS_PlateID({7}]",
                                                alarmState, alarmText, alarmID, alarmCode, alarmCategory, plPlateID_1,plPlateID_2,psPlateID));
                // Set MES Data 
                object[] mesParameter = new object[8]
                { 
                    tid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    unitId,          /*3 UnitID*/
                    alarmID,            /*4 AlarmID*/ 
                    alarmLevel,           /*5 AlarmLevel*/
                    alarmState,           /*6 AlarmState*/
                    alarmText,           /*7 AlarmText*/
                };
                // Set DB History
                ALARMHISTORY hisTrx = new ALARMHISTORY();
                hisTrx.EVENTNAME = "";
                hisTrx.UPDATETIME = DateTime.Now;
                hisTrx.ALARMID = alarmID;
                hisTrx.ALARMCODE = alarmCode;
                hisTrx.ALARMLEVEL = alarmLevel;
                hisTrx.ALARMTEXT = alarmText;
                hisTrx.ALARMSTATUS = alarmState;
                hisTrx.NODEID = eqp.Data.NODEID;
                hisTrx.ALARMUNIT = "0";
                //Check and Set DB Entity Data
                AlarmEntityData alarmEntity = alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqpno, "0", alarmID); //20150415 cy:修改key的組合

                if (alarmEntity == null)
                {
                    _common.LogError(this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Not found alarm in entity. UnitNo({0}], AlarmID({1}], AlarmLevel({2}).", hisTrx.ALARMUNIT, hisTrx.ALARMID, hisTrx.ALARMLEVEL));
                    #region [新增新的Alarm設定檔]
                    alarmEntity = new AlarmEntityData()
                    {
                        LINEID = eqp.Data.LINEID,
                        NODENO = eqp.Data.NODENO,
                        UNITNO = "0",
                        ALARMLEVEL = alarmLevel,
                        ALARMID = alarmID,
                        ALARMCODE = alarmCode,
                        ALARMTEXT = alarmText,
                        SERVERNAME = ServerName
                    };
                    ObjectManager.AlarmManager.CreateAlarmProfile(alarmEntity);
                    #endregion
                }
                Invoke(eServiceName.AlarmService, "HandleAlarm", new object[] { eqp, alarmEntity, mesParameter, hisTrx, tid });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S5F4_E_EnableDisableAlarmAcknowledge(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F4_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                //get eqp object
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string ack = recvTrx["secs"]["message"]["body"]["ACKC5"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                    string.Format("Enable/Disable alarm acknowledge. ({0}:{1})", ack, (ack == "0" ? "Accepted" : "Error, not accepted")));
                //check if opi request
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S5F4", string.Format("Enable/Disable alarm acknowledge. ({0}:{1})", ack, (ack == "0" ? "Accepted" : "Error, not accepted")) });
                        break;
                }			
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S5F6_E_ListAlarmData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F6_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                if (recvTrx["secs"]["message"]["body"]["array1"].ChildNodes.Count > 0)
                {
                    string unitId = string.Empty;
                    string alarmCode = string.Empty;
                    string alarmID = string.Empty;
                    string alarmText = string.Empty;
                    string alarmLevel = string.Empty;
                    string codeBinary = string.Empty;
                    string alarmState = string.Empty;
                    string alarmCategory = string.Empty;
                    XmlNode xAlarm = recvTrx["secs"]["message"]["body"]["array1"].FirstChild;
                    while (xAlarm != null)
                    {
                        alarmCode = xAlarm["ALCD"].InnerText.Trim();
                        alarmID = xAlarm["ALID"].InnerText.Trim();
                        alarmText = xAlarm["ALTX"].InnerText.Trim();
                        alarmLevel = ConstantManager["ALARMLEVEL"]["1"].Value;
                        codeBinary = Convert.ToString(Convert.ToByte(alarmCode, 16), 2).PadLeft(8, '0');
                        alarmState = ConstantManager["ALARMSTATUS"][codeBinary.Substring(0, 1)].Value;
                        alarmCategory = ConvertAlarmCategories(Convert.ToByte(codeBinary, 2).ToString());
                        
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Alarm ({0})({1}], ALID({2}], ALCD({3}], ALCATEGORY({4}]",
                                                alarmState, alarmText, alarmID, alarmCode, alarmCategory));
                        // Set MES Data 
                        object[] mesParameter = new object[8]
                        { 
                            tid,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp.Data.NODEID,    /*2 EQPID*/
                            unitId,          /*3 UnitID*/
                            alarmID,            /*4 AlarmID*/ 
                            alarmLevel,           /*5 AlarmLevel*/
                            alarmState,           /*6 AlarmState*/
                            alarmText,           /*7 AlarmText*/
                        };
                        // Set DB History
                        ALARMHISTORY hisTrx = new ALARMHISTORY();
                        hisTrx.EVENTNAME = "";
                        hisTrx.UPDATETIME = DateTime.Now;
                        hisTrx.ALARMID = alarmID;
                        hisTrx.ALARMCODE = alarmCode;
                        hisTrx.ALARMLEVEL = alarmLevel;
                        hisTrx.ALARMTEXT = alarmText;
                        hisTrx.ALARMSTATUS = alarmState;
                        hisTrx.NODEID = eqp.Data.NODEID;
                        hisTrx.ALARMUNIT = "0";
                        //Check and Set DB Entity Data
                        AlarmEntityData alarmEntity = alarmEntity = ObjectManager.AlarmManager.GetAlarmProfile(eqpno, alarmID);

                        if (alarmEntity == null)
                        {
                            _common.LogError(this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Not found alarm in entity. UnitNo({0}], AlarmID({1}], AlarmLevel({2}).", hisTrx.ALARMUNIT, hisTrx.ALARMID, hisTrx.ALARMLEVEL));
                            #region [新增新的Alarm設定檔]
                            alarmEntity = new AlarmEntityData()
                            {
                                LINEID = eqp.Data.LINEID,
                                NODENO = eqp.Data.NODENO,
                                UNITNO = "0",
                                ALARMLEVEL = alarmLevel,
                                ALARMID = alarmID,
                                ALARMCODE = alarmCode,
                                ALARMTEXT = alarmText,
                                SERVERNAME = ServerName
                            };
                            ObjectManager.AlarmManager.CreateAlarmProfile(alarmEntity);
                            #endregion
                        }
                        Invoke(eServiceName.AlarmService, "HandleAlarm", new object[] { eqp, alarmEntity, mesParameter, hisTrx, tid });

                        xAlarm = xAlarm.NextSibling;
                    }
                }
                //check if opi request
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S5F6", string.Format("List alarm count({0})", recvTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText.Trim()) });
                        break;
                }			
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        #endregion

        #region Send by host-S5
        public void TS5F2_H_AlarmReportAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, byte ack)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0}]", eqpid));
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
        public void TS5F3_H_EnableDisableAlarmSend(string eqpno, string eqpid, bool aled, List<uint> alid, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0}]", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S5F3_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S5F3_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["array1"]["ALED"].InnerText = aled ? "80" : "0";
                if (alid == null)
                    sendTrx["secs"]["message"]["body"]["array1"]["ALID"].Attributes["len"].InnerText = "0";
                else
                {
                    sendTrx["secs"]["message"]["body"]["array1"]["ALID"].Attributes["len"].InnerText = alid.Count.ToString();
                    sendTrx["secs"]["message"]["body"]["array1"]["ALID"].InnerText = string.Join(" ", alid.ToArray());
                }
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S5F3_H";
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
        public void TS5F5_H_ListAlarmsRequest(string eqpno, string eqpid, uint[] alid, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0}]", eqpid));
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
                if (alid == null || alid.Length == 0)
                    sendTrx["secs"]["message"]["body"]["ALID"].Attributes["len"].InnerText = "0";
                else
                {
                    sendTrx["secs"]["message"]["body"]["ALID"].Attributes["len"].InnerText = alid.Length.ToString();
                    sendTrx["secs"]["message"]["body"]["ALID"].InnerText = string.Join(" ", alid);
                }
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
        public void S5F3_H_EnableDisableAlarmSend(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F3_H T3-Timeout", false);
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
                                new object[4] { tid, eqp.Data.LINEID, "Rqeuest S5F3", "T3 Timeout" });
                            break;
                    }
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S5F3_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S5F5_H_ListAlarmsRequest(XmlDocument recvTrx, bool timeout)
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