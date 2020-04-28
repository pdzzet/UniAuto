using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.SerialPortService
{
    public class SerialPortService : AbstractService
    {
        private bool _run = false;
        private string AGENT_NAME = string.Empty;
        private string _localSocketSessionId = string.Empty;
        private string _remoteSocketSessionId = string.Empty;
        private IServerAgent _serialPortAgent = null;
        private Dictionary<string, int> _itemLenTable = null;

        /// <summary>
        /// ReturnCode List
        /// </summary>
        private static class ReturnCode
        {
            /// <summary>
            /// Normal
            /// Return:[0000]
            /// </summary>
            public const string Normal = "0000";
            /// <summary>
            /// FormatError 
            /// Return:[0001]
            /// </summary>
            public const string FormatError = "0001";
            /// <summary>
            /// NodeNoError
            /// Return:[0002]
            /// </summary>
            public const string NodeNoError = "0002";
            /// <summary>
            /// NotFindFileError
            /// Return:[0003]
            /// </summary>
            public const string NotFindFileError = "0003";
            /// <summary>
            /// OtherError
            /// Return:[0009]
            /// </summary>
            public const string OtherError = "0009";
        }

        private IServerAgent GetServerAgent()
        {
            //小心Spring的_applicationContext.GetObject(name)
            //在Init-Method與Destory-Method會與Spring的GetObject使用相同LOCK
            //需留心以免死結
            if(AGENT_NAME == string.Empty)
            {
                if (Workbench.Instance.AgentList.ContainsKey("SerialActiveAgent")) AGENT_NAME = "SerialActiveAgent";
                else if (Workbench.Instance.AgentList.ContainsKey("SerialPassiveAgent")) AGENT_NAME = "SerialPassiveAgent";
            }
            if (Workbench.Instance.AgentList.ContainsKey(AGENT_NAME))
            {
                return GetServerAgent(AGENT_NAME);
            }
            return null;
        }

        private Dictionary<string, int> ItemLenTable
        {
            get
            {
                if(_itemLenTable == null)
                    _itemLenTable = SerialPortAgent.GetTransactionFormat("GetItemLenTable") as Dictionary<string, int>;
                return _itemLenTable;
            }
        }

        private IServerAgent SerialPortAgent
        {
            get
            {
                if (_serialPortAgent == null)
                    _serialPortAgent = GetServerAgent();
                return _serialPortAgent;
            }
        }

        /// <summary>
        /// Service 初始化方法
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            bool ret = false;
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                //_serialPortAgent = GetServerAgent();
                //_run = true;
                //if (_serialPortAgent != null)
                //    _itemLenTable = _serialPortAgent.GetTransactionFormat("GetItemLenTable") as Dictionary<string, int>;
                //else
                //{
                //    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Not Found SerialPortAgent!");
                //    ret = false;
                //}
                ret = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                Destory();
                ret = false;
            }
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
            return ret;
        }

        public void Destory()
        {
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                if (_run)
                {
                    _run = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
        }

        public void SERIAL_CIMSystemConnectionTest_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "TestString" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion    

                #region Reply CIMSystemConnectionTest_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("CIMSystemConnectionTest_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/TestString").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/TestString").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_DateTimeSyncCommand_I(XmlDocument xmlDoc)
        {
            try
            {
                //do nothing
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_EQPAlarmWarningReport_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "AlarmLevel", "AlarmWarnType", "AlarmWarnCode", "AlarmID", "AlarmText" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                string AlarmText = string.Empty;
                string AlarmID = string.Empty;
                string AlarmCode = string.Empty;
                string AlarmWarnType = string.Empty;
                string AlarmLevel = string.Empty;
                string NodeID = string.Empty;
                #endregion   

                #region Reply EQPAlarmWarningReport_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("EQPAlarmWarningReport_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/AlarmWarnCode").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmWarnCode").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion

                #region rpeort to MES

                NodeID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                AlarmLevel = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmLevel").InnerText;
                AlarmWarnType = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmWarnType").InnerText;
                AlarmCode = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmWarnCode").InnerText;
                AlarmID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmID").InnerText;
                AlarmText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmText").InnerText;

                if (AlarmWarnType == "S") AlarmWarnType = "SET";
                if (AlarmWarnType == "R") AlarmWarnType = "CLEAR";

                string Trackkey = this.CreateTrxID();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                Unit unit = new Unit(new UnitEntityData(), new UnitEntityFile());

                //TCCVD100 TCCVD300 TCCVD600 TCCVD700 
                //TCDRY500 TCDRY600 TCDRY700 TCDRY800 TCDRY900 TCDRYA00 TCDRYD00 
                //TCIMP100 TCIMP300
                //TCPHL100 TCPHL200 TCPHL300 TCPHL400 TCPHL500 TCPHL600

                switch (line.Data.LINEID)
                {
                    case "TCCVD100":
                    case "TCCVD300":
                    case "TCCVD600":
                    case "TCCVD700":
                    case "TCCVD200":
                    case "TCCVD400":
                    case "TCCVD500":
                    case "TCCVD800":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L4");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "11");    // /CSC
                        NodeID = unit.Data.UNITID;
                        break;
                    case "TCDRY500": break;
                    case "TCDRY600": break;
                    case "TCDRY700": break;
                    case "TCDRY800": break;
                    case "TCDRY900": break;
                    case "TCDRYA00": break;
                    case "TCDRYD00": break;
                    case "TCIMP100": break;
                    case "TCIMP300": break;
                    case "TCPHL100":
                    case "TCPHL200":
                    case "TCPHL300":
                    case "TCPHL400":
                    case "TCPHL500":
                    case "TCPHL600":
                    case "TCPHL700"://jack add phase2
                    case "TCPHL800":
                    case "TCPHL900":
                    case "TCPHLA00":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L3");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "8");    // TCU
                        NodeID = unit.Data.UNITID;
                        
                        break;
                }
                
                AlarmEntityData alarm= new AlarmEntityData();

                // DB History
                ALARMHISTORY his = new ALARMHISTORY();
                his.EVENTNAME = "";
                his.UPDATETIME = DateTime.Now;
                his.ALARMID = AlarmID;
                his.ALARMCODE = AlarmCode;
                his.ALARMLEVEL = AlarmLevel;
                his.ALARMTEXT = AlarmText;
                his.ALARMSTATUS = AlarmWarnType;
                his.NODEID = NodeID; //eqp.Data.NODEID;
                his.ALARMUNIT = "A1";

                // MES Data 
                object[] mesdata = new object[8]
                { 
                    Trackkey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    NodeID,    /*2 EQPID*/
                    "A1",          /*3 UnitID*/
                    AlarmID,            /*4 AlarmID*/ 
                    AlarmLevel,           /*5 AlarmLevel*/
                    AlarmWarnType,           /*6 AlarmState*/
                    AlarmText           /*7 AlarmText*/
                };

                object[] _Data = new object[5]
                            { 
                                eqp,
                                alarm,
                                mesdata,
                                his,
                                Trackkey
                            };

                Invoke(eServiceName.AlarmService, "HandleAlarm", _Data);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_EQPStatusReport_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "EquipmentID", "EQPStatus", "INLINE", "PPID", "JobCount", "AlarmID", "AlarmText" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion 

                #region Reply EQPStatusReport_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("EQPStatusReport_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;

                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    
                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion  

                #region report to MES
                string TrackKey = this.CreateTrxID();
                string EQPStatus = xmlDoc.SelectSingleNode("//MESSAGE/BODY/EQPStatus").InnerText;
                string AlarmID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmID").InnerText;
                string AlarmText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmText").InnerText;
                string JobCount = xmlDoc.SelectSingleNode("//MESSAGE/BODY/JobCount").InnerText;
                string PPID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/PPID").InnerText;
                string INLINE = xmlDoc.SelectSingleNode("//MESSAGE/BODY/INLINE").InnerText;
                string AlarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string NodeID = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                Unit unit = new Unit(new UnitEntityData(), new UnitEntityFile());

                switch (line.Data.LINEID)
                {
                    case "TCCVD100":
                    case "TCCVD300":
                    case "TCCVD600":
                    case "TCCVD700":
                    case "TCCVD200":
                    case "TCCVD400":
                    case "TCCVD500":
                    case "TCCVD800":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L4");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "11");    // CSC
                        unit.File.TFTProductCount = int.Parse(JobCount);
                        unit.File.CurrentRecipeID = PPID;
                        unit.File.CurrentAlarmCode = AlarmID;
                        NodeID = unit.Data.UNITID;
                        switch (EQPStatus)
                        {
                            case "I": unit.File.Status = eEQPStatus.IDLE; break;
                            case "R": unit.File.Status = eEQPStatus.RUN; break;
                            case "S": unit.File.Status = eEQPStatus.STOP; break;
                            case "P": unit.File.Status = eEQPStatus.PAUSE; break;
                            case "T": unit.File.Status = eEQPStatus.SETUP; break;
                        }
                        break;
                    case "TCDRY500": break;
                    case "TCDRY600": break;
                    case "TCDRY700": break;
                    case "TCDRY800": break;
                    case "TCDRY900": break;
                    case "TCDRYA00": break;
                    case "TCDRYD00": break;
                    case "TCIMP100": break;
                    case "TCIMP300": break;
                    case "TCPHL100":
                    case "TCPHL200":
                    case "TCPHL300":
                    case "TCPHL400":
                    case "TCPHL500":
                    case "TCPHL600":
                    case "TCPHL700"://JACK ADD PHASE2
                    case "TCPHL800":
                    case "TCPHL900":
                    case "TCPHLA00":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L3");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "8");    // TCU
                        unit.File.TFTProductCount = int.Parse(JobCount);
                        unit.File.CurrentRecipeID = PPID;
                        unit.File.CurrentAlarmCode = AlarmID;
                        NodeID = unit.Data.UNITID;
                        switch (EQPStatus)
                        {
                            case "I" : unit.File.Status = eEQPStatus.IDLE; break ;
                            case "R" : unit.File.Status = eEQPStatus.RUN; break ;
                            case "S" : unit.File.Status = eEQPStatus.STOP; break ;
                            case "P" : unit.File.Status = eEQPStatus.PAUSE; break ;
                            case "T" : unit.File.Status = eEQPStatus.SETUP; break;
                        }
                        break;
                }

                Equipment _eqp = new Equipment(new EquipmentEntityData(), new EquipmentEntityFile());
                _eqp.Data.LINEID = line.Data.LINEID;
                _eqp.Data.NODEATTRIBUTE = "ATTACHMENT";
                _eqp.Data.NODENO = "  ";
                _eqp.Data.NODEID = NodeID;
                _eqp.File.Status = unit.File.Status;
                _eqp.File.CurrentRecipeID = unit.File.CurrentRecipeID;
                _eqp.File.TotalTFTJobCount = unit.File.TFTProductCount;
                if (unit.File.CurrentAlarmCode != "")
                    _eqp.File.LocalAlarmStatus = eBitResult.ON;
                else
                    _eqp.File.LocalAlarmStatus = eBitResult.OFF;

                if (_eqp.File.Status == eEQPStatus.IDLE || _eqp.File.Status == eEQPStatus.RUN)
                {
                    _eqp.File.MESStatus = _eqp.File.Status.ToString();
                    AlarmText = "";
                    AlarmTime = "";
                }
                else
                {
                    _eqp.File.MESStatus = "DOWN";
                }
                        object[] obj = new object[]
                        {
                            TrackKey,       /*0 TrackKey*/
                            _eqp,            /*1 Equipment*/
                            AlarmID,        /*2 alarmID*/
                            AlarmText,      /*3 alarmText*/
                            AlarmTime       /*4 alarmTime*/
                        };
                Invoke(eServiceName.MESService, "MachineStateChanged", obj);

                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                ObjectManager.EquipmentManager.RecordEquipmentHistory(TrackKey, _eqp);
                Invoke(eServiceName.LineService, "CheckLineState", new object[] { TrackKey, eqp.Data.LINEID });
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { TrackKey, eqp });
                ObjectManager.UnitManager.EnqueueSave(unit.File);
                
                ObjectManager.UnitManager.RecordUnitHistory(TrackKey,unit);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_FacilityDataReport_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "DataItemList", "Reserve" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion 

                #region Reply FacilityDataReport_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("FacilityDataReport_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion

                #region report to MES
                int ItemCount= int.Parse(xmlDoc.SelectSingleNode("//MESSAGE/BODY/DataCount").InnerText);
                // 獲取 Body層資料
                XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");

                XmlNodeList ItemList = body["DataItemList"].ChildNodes;
                List<string> list=new List<string>();
                        foreach (XmlNode item in ItemList)
                        {
                            list.Add(item.InnerText);
                        }
                
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                Unit unit = new Unit(new UnitEntityData(), new UnitEntityFile());
                Dictionary<string, EnergyMeter> resultDic = new Dictionary<string, EnergyMeter>();
                string trxid = this.CreateTrxID();
                string NodeID = string.Empty;
                string status = string.Empty;
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                string itemValue=string.Empty ;
                switch (line.Data.LINEID.Substring(0,5))
                {
					//modify 2016/08/18 cc.kuang
                    /*case "TCCVD100":
                    case "TCCVD300":
                    case "TCCVD600":
                    case "TCCVD700":*/
					case "TCCVD":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L4");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "11");    // CSC
                        NodeID = unit.Data.UNITID;
                        break;
                    case "TCDRY500": break;
                    case "TCDRY600": break;
                    case "TCDRY700": break;
                    case "TCDRY800": break;
                    case "TCDRY900": break;
                    case "TCDRYA00": break;
                    case "TCDRYD00": break;
                    case "TCIMP100": break;
                    case "TCIMP300": break;
                    /*case "TCPHL100":
                    case "TCPHL200":
                    case "TCPHL300":
                    case "TCPHL400":
                    case "TCPHL500":
                    case "TCPHL600":*/
					case "TCPHL":
                        eqp = ObjectManager.EquipmentManager.GetEQP("L3");
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "8");    // TCU
                        NodeID = unit.Data.UNITID;
                        break;
                }

                Equipment _eqp = new Equipment(new EquipmentEntityData(), new EquipmentEntityFile());
                _eqp.Data.LINEID = line.Data.LINEID;
                _eqp.Data.NODENO = "LA";
                _eqp.Data.NODEID = NodeID;
                _eqp.File.Status = unit.File.Status;
                _eqp.File.CurrentRecipeID = unit.File.CurrentRecipeID;
                if (_eqp.File.Status == eEQPStatus.IDLE || _eqp.File.Status == eEQPStatus.RUN)
                    _eqp.File.MESStatus = _eqp.File.Status.ToString();
                else
                    _eqp.File.MESStatus = "DOWN";

                string paraGroup = "0";
                for (int i=0;i<list.Count ;i++)
                {
                    itemValue = list[i];
                    int Itempos=(i % 7);
                    switch (Itempos)
                    {
                        case 0:
                            paraGroup = (int.Parse(paraGroup) + 1).ToString();
                            if (!resultDic.ContainsKey(paraGroup))
                            {
                                EnergyMeter energyMeter = new EnergyMeter();
                                resultDic.Add(paraGroup, energyMeter);
                            }
                            if (itemValue == "1") resultDic[paraGroup].EnergyType = "Liquid";
                            if (itemValue == "2") resultDic[paraGroup].EnergyType = "Electricity";
                            if (itemValue == "3") resultDic[paraGroup].EnergyType = "Gas";
                            if (itemValue == "4") resultDic[paraGroup].EnergyType = "N2";
                            if (itemValue == "5") resultDic[paraGroup].EnergyType = "Stripper";
                            break;

                        case 1:
                            resultDic[paraGroup].UnitID = itemValue;
                            break;

                        case 2:
                            resultDic[paraGroup].MeterNo = itemValue;
                            break;

                        case 3:
                            //Liquid: Instant liquid flow value, unit is "L/Min" 
                            // Electricity: Instant electric current value, unit is "A"
                            //Gas: Instant gas flow value, unit is "L/Min"
                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                            break;

                        case 4:
                            //Liquid: Instant liquid pressure value, unit is "Mpa"
                            //Electricity: Instant electric voltage value, unit is "V"
                            //Gas: Instant gas pressure value, unit is "Mpa"
                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                            break;

                        case 5:
                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                            break;

                        case 6:
                            resultDic[paraGroup].Refresh = itemValue;
                            break;
                    }
                }
                //Invoke(eServiceName.OEEService, "EnergyReport", new object[] { this.CreateTrxID(), line.Data.LINEID, _eqp, resultDic });

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_FacilityDataReportByFile_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "FilePath", "FileName" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion 

                #region Reply FacilityDataReportByFile_I
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("FacilityDataReportByFile_I") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_NiceDay_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion

                #region Reply NiceDay_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("NiceDay_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;
                    
                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion

                #region Send DateTimeSyncCommand_O
                {
                    XmlDocument cmd = SerialPortAgent.GetTransactionFormat("DateTimeSyncCommand_O") as XmlDocument;
                    cmd.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    cmd.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    cmd.SelectSingleNode("//MESSAGE/BODY/SyncTime").InnerText = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo);

                    xMessage msg = new xMessage();
                    msg.Data = cmd.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_ProcessDataReport_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "DataItemList" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion 

                #region Reply ProcessDataReport_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("ProcessDataReport_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_ProcessDataReportByFile_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "FilePath", "FileName" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion 

                #region Reply ProcessDataReportByFile_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("ProcessDataReportByFile_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_UtilityDataReport_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Check Input Data
                List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "DataItemList" });
                string strRtnCode = CheckInputData(xmlDoc, lsItems);
                #endregion

                #region Reply UtilityDataReport_O
                {
                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("UtilityDataReport_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion

                #region[Get UNIT & EQP & LINE]
                //一條LINE 只有 一個NODE 有附屬設備 才能用這方式
                Unit unit = ObjectManager.UnitManager.GetUnits().FirstOrDefault(u => u.Data.UNITATTRIBUTE == "ATTACHMENT");
                if (unit == null) throw new Exception(string.Format("Can't find Unit is ATTACHMENTin UnitEntity!"));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(unit.Data.NODENO);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", unit.Data.NODENO));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion

                XmlNode bodyNode = xmlDoc[keyHost.MESSAGE][keyHost.BODY];
                string nodeNoDeviceID = bodyNode[keyHost.NODENODEVICEID] == null ? "" : bodyNode[keyHost.NODENODEVICEID].InnerText;
                string dataType = bodyNode[keyHost.DATATYPE] == null ? "" : bodyNode[keyHost.DATATYPE].InnerText;
                string dataCount = bodyNode == null ? "" : bodyNode[keyHost.DATACOUNT].InnerText;
                XmlNode dataList = bodyNode[keyHost.DATAITEMLIST];

                if ((int.Parse(dataCount) % 7) != 0)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", string.Format("Data Count [{0}] is Error", dataCount));
                    return;
                }
               
                List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                //for (int i = 0; i < int.Parse(dataCount)/7; i++)
                //{
                //    specialData4IDRepository.Add(Tuple.Create(i.ToString(), itemList));
                //}
                
                string key = string.Format("{0}_{1}_SecsSpecialDataReqForService", eqp.Data.LINEID, eqp.Data.NODEID);
                string keyTmp = string.Format("{0}_{1}_SecsSpecialDataReqForServiceTmp", eqp.Data.LINEID, eqp.Data.NODEID);

                if (!eqp.File.SpecialDataEnableReq)
                {
                    Repository.Remove(key);
                    Repository.Remove(keyTmp);
                    return;
                }
                int dataIndex = 0;
                foreach (XmlNode data in dataList)
                {
                    if (dataIndex % 14 == 0)
                    {
                        itemList = new List<Tuple<string, string, string>>();
                        specialData4IDRepository.Add(Tuple.Create((dataIndex /14).ToString(), itemList));
                                            }
                    if (data.Name == keyHost.DATAITEM)
                    {
                        //itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                        itemList.Add(Tuple.Create("", "", data.InnerText));                        
                    }
                    dataIndex++;
                }

                if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE || line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP)
                {
                    #region [PHL_TITLE  nodeNoDeviceID 0001 +0002 在一起上報]
                    if (nodeNoDeviceID == "0002") //資料結尾
                    {
                        List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepositoryForServiceTmp = Repository.Get(keyTmp) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                        if (specialData4IDRepositoryForServiceTmp != null)
                        {
                            specialData4IDRepository.AddRange(specialData4IDRepositoryForServiceTmp);
                        }
                        Repository.Add(key, specialData4IDRepository);                        
                    }
                    else if (nodeNoDeviceID == "0001")
                    {
                        Repository.Add(keyTmp, specialData4IDRepository);
                    }
                    #endregion
                }
                else
                {
                    Repository.Add(key, specialData4IDRepository);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SERIAL_UtilityDataReportByFile_I(XmlDocument xmlDoc)
        {
            try
            {
                #region Reply UtilityDataReportByFile_O
                {
                    #region Check Input Data
                    List<string> lsItems = new List<string>(new string[] { "TRANSACTIONID", "NodeNoDeviceID", "DataType", "FilePath", "FileName" });
                    string strRtnCode = CheckInputData(xmlDoc, lsItems);
                    #endregion 

                    XmlDocument reply = SerialPortAgent.GetTransactionFormat("UtilityDataReportByFile_O") as XmlDocument;
                    reply.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/REPLYSUBJECTNAME").InnerText;
                    reply.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                    reply.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText = strRtnCode;

                    xMessage msg = new xMessage();
                    msg.Data = reply.OuterXml;
                    msg.ToAgent = AGENT_NAME;
                    QueueManager.PutMessage(msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// CheckInputData 
        /// 檢查傳入的 XmlDocument 資料格式與字串長度等是否有錯，若有錯傳回相對應的 Return Code
        /// </summary>
        /// <param name="xmlDoc">傳入的 XmlDocument</param>
        /// <param name="lsData">傳入要檢查的項目 List</param>
        /// <returns></returns>
        private string CheckInputData(XmlDocument xmlDoc, List<string> lsData)
        {
            string strRtnCode = ReturnCode.Normal;
            int intFormat;
            foreach (string strData in lsData)
            {
                switch (strData)
                {
                    case "NodeNoDeviceID":
                        string strNodeNoDeviceID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/NodeNoDeviceID").InnerText;
                        if (!Int32.TryParse(strNodeNoDeviceID, out intFormat) || strNodeNoDeviceID.Length != ItemLenTable["NodeNoDeviceID"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "TRANSACTIONID":
                        string strTRANSACTIONID = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                        if (!Int32.TryParse(strTRANSACTIONID, out intFormat) || strTRANSACTIONID.Length != ItemLenTable["SNo"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "DataType":
                        string strDataType = xmlDoc.SelectSingleNode("//MESSAGE/BODY/DataType").InnerText;
                        if (!Int32.TryParse(strDataType, out intFormat) || strDataType.Length != ItemLenTable["DataType"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    //case "FilePath":
                    //    string strFilePath = xmlDoc.SelectSingleNode("//MESSAGE/BODY/FilePath").InnerText;
                    //    if (strFilePath.Length != ItemLenTable["FilePath"]) strRtnCode = ReturnCode.FormatError;
                    //    break;
                    //case "FileName":
                    //    string strFileName = xmlDoc.SelectSingleNode("//MESSAGE/BODY/FileName").InnerText;
                    //    if (strFileName.Length != ItemLenTable["FileName"]) strRtnCode = ReturnCode.FormatError;
                    //    break;
                    //case "Reserve":
                    //    string strReserve = xmlDoc.SelectSingleNode("//MESSAGE/BODY/Reserve").InnerText;
                    //    if (strReserve.Length != ItemLenTable["Reserve"]) strRtnCode = ReturnCode.FormatError;
                    //    break;
                    case "TestString":
                        string strTestString = xmlDoc.SelectSingleNode("//MESSAGE/BODY/TestString").InnerText;
                        if (strTestString.Length != ItemLenTable["TestString"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "EquipmentID":
                        string strEquipmentID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/EquipmentID").InnerText;
                        if (strEquipmentID.Length != ItemLenTable["EquipmentID"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "EQPStatus":
                        string strEQPStatus = xmlDoc.SelectSingleNode("//MESSAGE/BODY/EQPStatus").InnerText;
                        if (strEQPStatus.Length != ItemLenTable["EQPStatus"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "INLINE":
                        string strINLINE = xmlDoc.SelectSingleNode("//MESSAGE/BODY/INLINE").InnerText;
                        if (!Int32.TryParse(strINLINE, out intFormat) || strINLINE.Length != ItemLenTable["INLINE"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "PPID":
                        string strPPID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/PPID").InnerText;
                        if (strPPID.Length != ItemLenTable["PPID"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "JobCount":
                        string strJobCount = xmlDoc.SelectSingleNode("//MESSAGE/BODY/JobCount").InnerText;
                        if (!Int32.TryParse(strJobCount, out intFormat) || strJobCount.Length != ItemLenTable["JobCount"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "AlarmID":
                        string strAlarmID = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmID").InnerText;
                        if (!Int32.TryParse(strAlarmID, out intFormat) || strAlarmID.Length != ItemLenTable["AlarmID"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    //case "AlarmText":
                    //    string strAlarmText = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmText").InnerText;
                    //    if (strAlarmText.Length != ItemLenTable["AlarmText"]) strRtnCode = ReturnCode.FormatError;
                    //    break;
                    case "AlarmLevel":
                        string strAlarmLevel = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmLevel").InnerText;
                        if (strAlarmLevel.Length != ItemLenTable["AlarmLevel"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "AlarmWarnType":
                        string strAlarmWarnType = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmWarnType").InnerText;
                        if (strAlarmWarnType.Length != ItemLenTable["AlarmWarnType"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "AlarmWarnCode":
                        string strAlarmWarnCode = xmlDoc.SelectSingleNode("//MESSAGE/BODY/AlarmWarnCode").InnerText;
                        if (!Int32.TryParse(strAlarmWarnCode, out intFormat) || strAlarmWarnCode.Length != ItemLenTable["AlarmWarnCode"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "RetCode":
                        string strRetCode = xmlDoc.SelectSingleNode("//MESSAGE/BODY/RetCode").InnerText;
                        if (!Int32.TryParse(strRetCode, out intFormat) || strRetCode.Length != ItemLenTable["RetCode"]) strRtnCode = ReturnCode.FormatError;
                        break;
                    case "DataItemList":
                        XmlNodeList xnsDataItems = xmlDoc.SelectNodes("//MESSAGE/BODY/DataItemList/DataItem");
                        //20161229 sy modify xml 已tirm()處理過 此處不用CHECK
                        //foreach (XmlNode xnDataItem in xnsDataItems)
                        //{
                        //    if (xnDataItem.InnerText.Length != ItemLenTable["DataItem"])
                        //    {
                        //        strRtnCode = ReturnCode.FormatError;
                        //        break;
                        //    }
                        //}
                        break;
                }
                if (strRtnCode != ReturnCode.Normal) 
                    break;
            }
            return strRtnCode;
        }
    }
}
