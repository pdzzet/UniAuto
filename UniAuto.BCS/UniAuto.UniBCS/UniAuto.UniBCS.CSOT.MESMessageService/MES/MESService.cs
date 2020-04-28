using System;
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
using UniAuto.UniBCS.MesSpec;
using System.Collections;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    #region Import Library

    [StructLayout(LayoutKind.Sequential)]
    public class SystemTime
    {
        public ushort year;
        public ushort month;
        public ushort dayOfWeek;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
        public ushort milliseconds;
    }

    public class Win32API
    {
        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime([In, Out] SystemTime st);

        [DllImport("Kernel32.dll")]
        public static extern void SetSystemTime([In, Out] SystemTime st);
    }

    #endregion

    public partial class MESService : AbstractService
    {
        /// <summary>
        /// 記LOG用, 在UPK切換ServerName時記LOG用
        /// </summary>
        private string _serverName = string.Empty;

        private bool _run = false;

        private bool _tryOnline = true;
        private bool _firstRun = true;
        private Dictionary<string, string> _1stJobInCellUldPort = new Dictionary<string, string>();
        //20170904 huangjiayin add for t3 cell
        //key for PORTID_CSTSEQNO
        //value for PORTID
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
                _run = true;
                _serverName = Workbench.ServerName;//UPK切換ServerName之前, 把初始的ServerName記著, UPK切換的時候可以用來記LOG
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

        /// <summary>
        ///  6.1.	AlarmReport     MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="eQPID"></param>
        /// <param name="unitID"></param>
        /// <param name="alarmID"></param>
        /// <param name="alarmLevel"></param>
        /// <param name="alarmState"></param>
        /// <param name="alarmText"></param>
        public void AlarmReport(string trxID, string lineName, string eQPID, string unitID, string alarmID, string alarmLevel, string alarmState, string alarmText)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);
                Unit unit = ObjectManager.UnitManager.GetUnit(unitID);

                //Watson Add 20150319 For PMT Line Send 2 Time Loader Status.
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] AlarmReport Send But OFF LINE LINENAME=[{1}].", trxID, lineName));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                if (eqp != null)    //add by bruce 20160217 如果是附屬設備 eqp 會是null, 防呆
                {
                    // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                    if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                    if (eqp.File.EquipmentOperationMode == eEQPOperationMode.MANUAL && ParameterManager[eREPORT_SWITCH.MANUAL_ALARM_MES].GetBoolean() == false)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={0}] [BCS -> MES][{1}] Skip AlarmReport Send, Because Equipment=[{2}] is Manual Mode.",
                                lineName, trxID, eqp.Data.NODENO));
                        return;
                    }
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("AlarmReport") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                if(unitID =="A1")   // add by bruce 20160217 Array 弣屬設備固定給A1
                {
                    bodyNode[keyHost.MACHINENAME].InnerText = eQPID;
                    bodyNode[keyHost.UNITNAME].InnerText = "";
                }
                else
                {
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                }
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.ALARMCODE].InnerText = alarmID;
                bodyNode[keyHost.ALARMLEVEL].InnerText = alarmLevel;
                bodyNode[keyHost.ALARMSTATE].InnerText = alarmState;
                bodyNode[keyHost.ALARMTEXT].InnerText = alarmText;

                if (ObjectManager.LineManager.CheckSkipReportAlarmByID(lineName, eQPID, unitID, alarmID, eAgentName.MESAgent) == false)
                {
                    if (line.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] AlarmReport OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                    #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                    {
                        bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                        bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        if (unitID.Trim() != string.Empty)
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();

                        Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());

                        if (otherline.File.HostMode != eHostMode.OFFLINE)
                        {
                            SendToMES(xml_doc);
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS -> MES][{0}] AlarmReport OK LINENAME=[{1}].", trxID, bodyNode[keyHost.LINENAME].InnerText));
                        }
                        else
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                        }
                    }

                    #endregion
                }

                // BC2MES 類型的 Alarm 不用上報到 OEE .20160406 Modify by Frank according T2 method//sy modify BCS2MES => BC2MES
                if (alarmID.IndexOf("BC2MES") != -1)
                    return;

                if (ObjectManager.LineManager.CheckSkipReportAlarmByID(lineName, eQPID, unitID, alarmID, eAgentName.OEEAgent) == false)
                {
                    //没有呼叫OEEService？ --James.Yan at 2015/01/24
                    if (line.File.HostMode != eHostMode.OFFLINE)
                        SendToOEE(xml_doc);

                    #region Watson Add 20150320 For PMT Line Send 2 Time Loader Status.
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                    {
                        Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                        if (otherline.File.HostMode != eHostMode.OFFLINE)
                            SendToOEE(xml_doc);
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.2.	AreYouThereRequest      MES MessageSet : BC send to MES Are you There 做BC Initial and BC Active Send by Timer 
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void AreYouThereRequest(string trxID, string lineName)
        {
            try
            {
                //程序开启的开启的时候就发AraYouThereReqeust

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("AreYouThereRequest") as XmlDocument;
                GetTransactionID(xml_doc);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

                SendToMES(xml_doc);

                _tryOnline = false;//标记此处的AraYouThereRequest 不能做Online 

                string timeId = string.Format("{0}_AreYouThereRequest", ObjectManager.LineManager.GetLineID(lineName));
                if (Timermanager.IsAliveTimer(timeId))
                {
                    Timermanager.TerminateTimer(timeId);
                }
                Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(AreYouThereRequestTimeout), trxID);
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MES trx AreYouThereRequest.", ObjectManager.LineManager.GetLineID(lineName), trxID));


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.2.	AreYouThereRequest      MES MessageSet : BC send to MES Are you There For UI 做ONLINE使用
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void AreYouThereRequest_UI(string trxID, string lineName)
        {
            try
            {
                //程序开启的开启的时候就发AraYouThereReqeust

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("AreYouThereRequest") as XmlDocument;
                GetTransactionID(xml_doc);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

                SendToMES(xml_doc);


                _tryOnline = true;//标记此处的AraYouThereRequest 要做Online 使用的

                //Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //string.Format("[LINENAME={1}] [BCS <- MES][{0}] AreYouthereRequest OK LINENAME=[{1}].",
                //    trxID, lineName));

                string timeId = string.Format("{0}_AreYouThereRequest", ObjectManager.LineManager.GetLineID(lineName));
                if (Timermanager.IsAliveTimer(timeId))
                {
                    Timermanager.TerminateTimer(timeId);
                }
                Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(AreYouThereRequestTimeout), trxID);
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MES trx AreYouThereRequest.", ObjectManager.LineManager.GetLineID(lineName), trxID));


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.2.	AreYouThereRequest      4 Hour Send AreYouThereReqeust To MES
        /// </summary>
        public void AreYouTherePeriodicityRequest()
        {
            try
            {
                //Line line = ObjectManager.LineManager.GetLine(lineName);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("AreYouThereRequest") as XmlDocument;
                GetTransactionID(xml_doc);
                _tryOnline = false;//标记此处的AraYouThereRequest 不是做Online 使用
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                foreach (Line line in ObjectManager.LineManager.GetLines())
                {
                    string trxID = CreateTrxID();

                    bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                    SendToMES(xml_doc);
                    string timeId = string.Format("{0}_AreYouThereRequest", ObjectManager.LineManager.GetLineID(line.Data.LINEID));
                    if (Timermanager.IsAliveTimer(timeId))
                    {
                        Timermanager.TerminateTimer(timeId);
                    }
                    Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(AreYouThereRequestPeriodicityTimeout), trxID);

                    Thread.Sleep(200);
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        /// MES Don't Reply AreYouThereReqeust
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void AreYouThereRequestTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string lineID = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                     string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply AreYouThereRequest Timeout.", lineID, trackKey));
                //MES 没有回复AreYouThereReply 则直接切换到Offline 
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, lineID, "[AreYouThereRequestTimeout] MES NOT REPLY AreYouThereReply, BCS AND MES OFFLINE(DISONNECT)!!" });

                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    eHostMode preHostMode = line.File.HostMode;
                    lock (line)
                    {
                        line.File.HostMode = eHostMode.OFFLINE;
                        line.File.PreHostMode = preHostMode;
                        ObjectManager.LineManager.EnqueueSave(line.File);

                        #region OPI Service
                        Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trackKey, line });
                        #endregion
                        ObjectManager.LineManager.RecordLineHistory(trackKey, line);
                    }
                    MachineControlStateChanged(CreateTrxID(), lineID, "OFFLINE");
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AreYouThereRequestPeriodicityTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string lineID = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                     string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply AreYouThereRequest Timeout.", lineID, trackKey));
                //MES 没有回复AreYouThereReply 则直接切换到Offline 
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, lineID, "[AreYouThereRequestTimeout] MES NOT REPLY AreYouThereReply, BCS AND MES OFFLINE(DISONNECT)!!" });

                //此处不能切换到OFFLINE  福杰 20150311 提出  tom 
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.3.	AreYouThereReply        MES MessagetSet : Are You There Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_AreYouThereReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineId = GetLineName(xmlDoc);
                string trxId = GetTransactionID(xmlDoc);
                if (!CheckMESLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, lineId));
                }

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], OK =[{0}].", ServerName, trxId, lineId));


                string timeId = string.Format("{0}_AreYouThereRequest", lineId);
                if (Timermanager.IsAliveTimer(timeId))
                {
                    Timermanager.TerminateTimer(timeId);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.42.	CurrentDateTime      MES MessagetSet : Current Date time  Send to to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CurrentDateTime(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", lineName, trxID, lineName));
                }

                string crdatetime = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DATETIME].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] CurrentDateTime  NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "[MES_CurrentDateTime] MES REPLY CurrentDateTime Return Code <> '0' , BCS AND MES REMOTE FAILED(DISONNECT)!!" });
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] CurrentDateTime  OK LINENAME=[{1}],DATETIME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, crdatetime, returnCode, returnMessage));
                }

                try
                {

                    DateTime utcTime;
                    utcTime = DateTime.ParseExact(crdatetime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    utcTime = utcTime.ToUniversalTime();
                    SystemTime systime = new SystemTime();
                    systime.year = (ushort)utcTime.Year;
                    systime.month = (ushort)utcTime.Month;
                    systime.dayOfWeek = (ushort)utcTime.DayOfWeek;
                    systime.day = (ushort)utcTime.Day;
                    systime.hour = (ushort)utcTime.Hour;
                    systime.minute = (ushort)utcTime.Minute;
                    systime.second = (ushort)utcTime.Second;
                    systime.milliseconds = 0;

                    //2015/8/21 Add by Frank 待正式上線時，將PARAMETER.XML內MES_CurrentDateTime value改為False即可。
                    if (ParameterManager["MES_CurrentDateTime"].GetBoolean())
                        Win32API.SetSystemTime(systime);

                    Invoke(eServiceName.DateTimeService, "MESSetAllEQDataTimeCommand", new object[] { trxID });

                }
                catch (System.Exception ex)
                {
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={2}] [BCS <- MES][{3}] CurrentDateTime DateTime Format  is error dateTime={0}.Message={1}", crdatetime, ex.Message, lineName, trxID));
                }


                if (_firstRun)
                {
                    Thread.Sleep(5000);
                    _firstRun = false;
                    MachineControlStateChanged_FirstRun(trxID, lineName);
                }


                // BCS 不是ONline 时无需继续往下做，
                if (_tryOnline == false)
                    return;


                Thread.Sleep(5000);  //可能在程式啟動馬上收到CurrentDateTime，但程式裏的所有Agents還未啟動，所以先sleep後才開始
                //Watson Modify 20150211 一次一條line
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] CurrentDateTime  NG LINENAME=[{1}] CAN'T FIND IN LINEENTITY!!",
                            trxID, lineName));
                    return;
                }
                //做ON Line 的一些設定
                //line.File.HostMode = eHostMode.REMOTE;
                //On Line Sequence
                //Step 1
                //foreach (Line line in linelist)
                //{
                MachineControlStateChanged(trxID, line.Data.LINEID, line.File.OPICtlMode.ToString());
                ////Step 2 重要相關機台資訊上報
                //MES TEST 20141117 - TEST 切 REMOTE 只要報一次
                //if (line.File.HostMode != eHostMode.OFFLINE)
                //{

                //    MachineDataUpdate(trxID, line);
                //    PortDataUpdate(trxID, line);
                //    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                //    {
                //        MaskStateChanged_OnLine(trxID, eqp);  //if Mask Machine is mounted Report else not report
                //        MaterialStateChanged_OnLine(this.CreateTrxID(), eqp);

                //    }
                //    //MES say 不需要 20141115
                //    //QtimeSetChanged_OnLine(this.CreateTrxID(), line.Data.LINEID);
                //}
                //}

                //t3 MES Msg Add 2016/01/08 cc.kuang
                Thread.Sleep(1000);
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    IndexerOperModeChanged(UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, "GLASS_CHANGER");
                else
                    IndexerOperModeChanged(UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, "NORMAL");

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.17.	CassetteCleanEnd       MES MessageSet : BC reports when processing of Lot in EQP was completed. 
        /// Add by marine for MES 2015/7/11
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="portID"></param>
        /// <param name="cSTID"></param>
        /// <param name="reasoncode"></param>
        public void CassetteCleanEnd(string trxID, string lineName, string portID, string cSTID, string reasonCode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CassetteCleanEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.CARRIERNAME].InnerText = cSTID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                if (reasonCode != "")
                    bodyNode[keyHost.INSPECTIONFLAG].InnerText = "Y";
                else
                    bodyNode[keyHost.INSPECTIONFLAG].InnerText = "N";

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.18.	CassetteOperModeChanged          MES MessageSet : Reports when cassette operation mode has been changed. 
        ///  This message applies for EQP that has more than one specific operation modes that defined in equipment operation scenario.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="CstOperMode">'CTOC’KTOK’ Cassette to cassette operation mode; Kind to kind operation mode.</param>
        public void CassetteOperModeChanged(string trxID, string lineName, string CstOperMode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //WATSON mODIFY 20150320 FOR PMT SPECIAL RULE 改在下面再判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] CassetteOperModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CassetteOperModeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];


                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CSTOPERMODE].InnerText = CstOperMode;

                //SendToMES(xml_doc); cc.kuang 2015/08/03 double send to MES Error

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] CassetteOperModeChanged OK LINENAME=[{1}],CstOperMode=[{2}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), CstOperMode));

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                }
                else
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                            trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                        SendToMES(xml_doc);

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                                trxID, bodyNode[keyHost.LINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.52.	GlassProcessStarted 
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="job"></param>
        public void GlassProcessStarted(string trxID, string lineName, Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] GlassProcessStarted Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("GlassProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 6.53.	GlassProcessLineChanged     MES MessageSet : Glass Process Change Line Report to MES
        ///  BC reports Glass process Line changed from 1# to 2#
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Source Line ID</param>
        /// <param name="lineName">Target Line ID</param>
        /// <param name="job">Job Entity</param>
        public void GlassProcessLineChanged(string trxID, string lineName, string crosslineName, Job job)
        {
            try
            {//////
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("GlassProcessLineChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PROCESSLINENAME].InnerText = crosslineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PROCESSSTATE].InnerText = line.File.Status.ToString(); //不能確定上報內容

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.54.	GlassReworkJudgeReport   MES MessageSet : Reports when Inspection in Inline judge a Glass need to proceed Inline Rework.
        /// Add by marine for MES 2015/7/28
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="job"></param>
        public void GlassReworkJudgeReport(string trxID, string lineName, string eqpNo, string unitNo, Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("GlassReworkJudgeReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                Unit unit = ObjectManager.UnitManager.GetUnit(eqpNo, unitNo);
                if (unit == null)
                    bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqpNo);
                else
                    bodyNode[keyHost.UNITNAME].InnerText = unit.Data.UNITID;

                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);//'RW';//job data send out will upgrade

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] GlassReworkJudgeReport OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.55.	IndexerOperModeChanged      MES MessageSet : Reports when indexer operation mode has been changed. to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="indexOPerMode">Indexer Operation Mode</param>
        public void IndexerOperModeChanged(string trxID, string lineName, string indexOPerMode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] IndexerOperModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("IndexerOperModeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.INDEXEROPERMODE].InnerText = indexOPerMode;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] IndexerOperModeChanged OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.56.	InspectionModeChanged       MES MessageSet : Inspection Mode Change Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="inspMode">Inspection Mode</param>
        /// <param name="eqpID">EQP ID</param>
        /// <param name="pullmodeGrade">pull Mode Grade</param>
        /// <param name="waittime">Wait Time</param>
        /// <param name="samplerate">Sample Rate</param>
        /// <param name="reasoncode">Reason Code</param>
        public void InspectionModeChanged(string trxID, string lineName, string inspMode, string eqpID, string pullmodeGrade, string waittime, string samplerate, string reasoncode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("InspectionModeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.INSPECTIONMODE].InnerText = inspMode;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.PULLMODEGRADE].InnerText = pullmodeGrade;
                bodyNode[keyHost.WAITTIME].InnerText = waittime;
                bodyNode[keyHost.SAMPLERATE].InnerText = samplerate;
                bodyNode[keyHost.REASONCODE].InnerText = reasoncode;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.58.	LineStateChanged        MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="linestatus"></param>
        /// <param name="materialflag"></param>
        public void LineStateChanged(string trxID, string lineName, string linestatus)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });   //bruce modify 2016/01/20 Offline也要更新
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] LineStateChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.LINESTATENAME].InnerText = linestatus;
                bodyNode[keyHost.MATERIALCHANGEFLAG].InnerText = line.File.Array_Material_Change ? "Y" : "N";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);

                #region OPI Service
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LineStateChanged OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.61.	LotProcessAborted       MES MessageSet : BC reports processing of Lot has been canceled.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="port">Port Entity</param>
        /// <param name="cst">Cassette Entity</param>
        /// <param name="reasonCode">Reason Code</param>
        /// <param name="reasonTxt">Reason Text</param>
        public void LotProcessAborted(string trxID, Port port, string reasonCode, string reasonTxt)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAborted Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessAborted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID; ;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                //Watson Modify 20150413 For MES,CSOT MAIL要求，直接上報Cassette Reason Text
                bodyNode[keyHost.REASONTEXT].InnerText = reasonTxt;
                //bodyNode[keyHost.REASONTEXT].InnerText = GetCSTAbortReasonText(reasonCode); 

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAborted OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///  6.62.	LotProcessCanceled      MES MessageSet : BC reports processing of Lot has been canceled.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="port">Port Entity</param>
        /// <param name="cst">Cassette Entity</param>
        /// <param name="reasonCode">Cancel Reason code</param>
        /// <param name="reasonTxt">Cancel Reason Text</param>
        public void LotProcessCanceled(string trxID, Port port, string reasonCode, string reasonTxt)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);//可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.CARRIERHOLDFLAG].InnerText = (port.File.DirectionFlag == eDirection.Reverse || port.File.DistortionFlag == eDistortion.Distortion || port.File.GlassExist == eGlassExist.Exist) ? "Y" : "N";//20170112 sy modify  by MES SPEC 1.58
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                //Watson Modify 20150413 For MES,CSOT MAIL要求，直接上報Cassette Reason Text
                bodyNode[keyHost.REASONTEXT].InnerText = reasonTxt;
                //bodyNode[keyHost.REASONTEXT].InnerText = GetCSTAbortReasonText(reasonCode); 

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.65.	LotProcessStarted       MES MessageSet : BC reports Lot processing has been started.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="port">Port Entity</param>
        ///  <param name="cst">Cassette Entity</param>
        /// <param name="jobs">job Entity List</param>
        public void LotProcessStarted(string trxID, Port port, Cassette cst)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
                //Jun Add 20150602 For POL Unloader AutoClave Report LotProcessStart
                if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3 && cst.CellBoxProcessed == eboxReport.Processing)
                    bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
                else
                    bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.MES_CstData.LINERECIPENAME; //Add For T3
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
                lotListNode.RemoveAll();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    XmlNode lotnode = lotCloneNode.Clone();
                    Job job = null;
                    lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                    lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].PROCESSOPERATIONNAME;
                    lotnode[keyHost.PRODUCTQUANTITY].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count.ToString();
                    lotnode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTOWNER;
                    lotnode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;

                    if (cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count > 0)
                        job = ObjectManager.JobManager.GetJob(cst.MES_CstData.LOTLIST[i].PRODUCTLIST[0].PRODUCTNAME.Trim());

                    if (job != null)
                    {
                        lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                        lotnode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                        lotnode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                        if ((job.MES_PPID != null) && (job.MES_PPID != string.Empty))
                            //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                            lotnode[keyHost.PPID].InnerText = job.MES_PPID;  //ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.MES_PPID);
                        else
                            lotnode[keyHost.PPID].InnerText = job.MesProduct.PPID;
                        lotnode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                    }
                    else
                    {
                        lotnode[keyHost.PRODUCTRECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                        lotnode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                        lotnode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                        lotnode[keyHost.PPID].InnerText = cst.MES_CstData.LOTLIST[i].PPID;
                        lotnode[keyHost.HOSTPPID].InnerText = cst.MES_CstData.LOTLIST[i].PPID;
                    }

                    lotListNode.AppendChild(lotnode);
                }

                SendToMES(xml_doc);

                //Watson Add 20141212 For MES Spec 
                //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
                cst.IsOffLineProcessStarted = false;
                lock (cst)
                {
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));

                #region OEE
                if (cst.MES_CstData.LOTLIST.Count > 0)//sy add 防止T3 空 tray 只有 LOTLIST沒有PRODUCTLIST 會報Exception
                {
                    if (cst.MES_CstData.LOTLIST[0].PRODUCTLIST.Count > 0)
                    {
                        //没有呼叫OEEService？ --James.Yan at 2015/01/24
                        string ownerid = "";
                        if (cst.MES_CstData.LOTLIST.Count > 0)
                        {
                            ownerid = cst.MES_CstData.LOTLIST[0].PRODUCTLIST[0].OWNERID;
                        }

                        bodyNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.PORTTYPE, ConstantManager["MES_PORTTYPE"][((int)port.File.Type).ToString()].Value));


                        foreach (XmlNode lotNode in lotListNode)
                        {
                            string lotName = lotNode[keyHost.LOTNAME].InnerText;

                            foreach (LOTc lotInfo in cst.MES_CstData.LOTLIST)
                            {
                                if (lotName == lotInfo.LOTNAME)
                                {
                                    //lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.OWNERTYPE, lotInfo.PRODUCTLIST[0].OWNERTYPE));
                                    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                                    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                                    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                                    lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.OWNERTYPE, lotInfo.PRODUCTLIST[0].OWNERTYPE.Length > 0 ? lotInfo.PRODUCTLIST[0].OWNERTYPE.Substring(lotInfo.PRODUCTLIST[0].OWNERTYPE.Length - 1, 1) : ""));


                                    lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.LOTINFO, ownerid));
                                    break;
                                }
                            }
                            lotNode.RemoveChild(lotNode[keyHost.LINERECIPENAME]);
                            lotNode.RemoveChild(lotNode[keyHost.HOSTLINERECIPENAME]);
                            lotNode.RemoveChild(lotNode[keyHost.PPID]);
                            lotNode.RemoveChild(lotNode[keyHost.HOSTPPID]);
                        }
                    }
                }
                //lotListNode.RemoveChild(lotListNode[keyHost.LINERECIPENAME]);
                //lotListNode.RemoveChild(lotListNode[keyHost.HOSTLINERECIPENAME]);
                //lotListNode.RemoveChild(lotListNode[keyHost.PPID]);
                //lotListNode.RemoveChild(lotListNode[keyHost.HOSTPPID]);
                
                //SendToOEE(xml_doc);
                Invoke(eServiceName.OEEService, "LotProcessStarted", new object[3] { trxID, port, cst }); //2015/10/15 cc.kuang
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.66.	LotProcessStartRequest          MES MessageSet : BC reports when processing of Lot in EQP was completed and 
        ///                                                          all substrates of the Lot have been stored in to cassette for moving out of the EQP.
        /// Add by marine for MES 2015/7/7
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="cst"></param>
        public void LotProcessStartRequest(string trxID, Port port, Cassette cst, Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    if (cst.FirstGlassCheckReport.Equals("C1"))
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                    else
                    {
                        //20151022 add Offline 回復Y
                        cst.FirstGlassCheckReport = "Y";
                    }
                    return;
                }

                if (cst.FirstGlassCheckReport.Equals("C2")) //avoid robot resend this message 20151113 cc.kuang
                    return;

                #region[avoid request more than one cst for bc trigger] 
                   //add by yang 20160104
                   //For need Q-Time Linetype,有一cst已经在First Glass Check了就Return,避免Job Foreach,把另一个WAIT_FOR_PROCESSING的cst也Request To MES
                if (ParameterManager.ContainsKey(eFirstGlassCheck.LINETYPELIST))
                {
                    string[] linetypelist = ParameterManager[eFirstGlassCheck.LINETYPELIST].GetString().Split(',');
                    if (linetypelist.Where(s=>s.ToString().Equals(line.Data.LINETYPE)).ToList()!=null)
                    {
                        List<Cassette> csts = ObjectManager.CassetteManager.GetCassettes();
                        if (csts.Find(s => s.FirstGlassCheckReport.Equals("C2")) != null) return;
                    }
                }
                #endregion

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessStartRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                if (job != null) //t3 use cc.kuang 2015/07/10
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                else
                    bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;

                if (!cst.FirstGlassCheckReport.Equals("C1")) //C1:check triger by Indexer
                    cst.FirstGlassCheckReport = "C2"; //C2:check triger by BC

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
                lotListNode.RemoveAll();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    XmlNode lotnode = lotCloneNode.Clone();
                    lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;

                    lotListNode.AppendChild(lotnode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));

                //t3 use cc.kuang 2015/07/10
                #region MES LotProcessStartRequest Timeout
                string timeoutName = string.Format("{0}_{1}_MES_LotProcessStartReply", ObjectManager.LineManager.GetLineID(port.Data.LINEID),
                    ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(MES_LotProcessStartReply_Timeout), trxID);
                #endregion
            }

            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.67.	LotProcessStartReply      MES MessageSet : MES sends validation result of Qtime check
        /// Add by marine for MES 2015/7/7
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_LotProcessStartReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LineName={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}] mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string portname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string carriername = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;
                string linerecipename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                XmlNode lotlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTLIST];
                //增加判断Lot List 中是否有子节点 20160201 Tom.bian
                if (lotlistnode.HasChildNodes) {

                    XmlNode lotnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTLIST][keyHost.LOT];
                    string desc = string.Empty;

                    List<string> lotlist = new List<string>();
                    foreach (XmlNode node in lotnode) {
                        if (node != null) {
                            string lotname = node.InnerText;
                            lotlist.Add(lotname);
                        }
                    }
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessStartReply  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portname, carriername, returnCode, returnMessage));
                   // return;  modify by yang 2016//11/26 后面还是要判断的
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessStartReply  OK LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portname, carriername, returnCode, returnMessage));
                }

                Port port = ObjectManager.PortManager.GetPort(portname);
                object[] _data = new object[4]
                { 
                    trxID,             
                    eBitResult.ON ,   
                    eReturnCode1.OK,  
                    port.Data.NODENO
                };
                if (returnCode != "0" || !valiresult.Trim().Equals("Y"))
                    _data[2] = eReturnCode1.NG;

                //for t3 check if reply to indexer, if C1 will reply to indexer 2015/10/19 cc.kuang
                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                if (cst != null && cst.FirstGlassCheckReport.Equals("C1"))
                    Invoke(eServiceName.JobService, "FirstGlassCheckReportReply", _data);

                string timeoutName = string.Format("{0}_{1}_MES_LotProcessStartReply", lineName,
                    portname);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (returnCode != "0" || !valiresult.Trim().Equals("Y"))
                {
                    //Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                    if (cst != null)
                    {
                        cst.FirstGlassCheckReport = "N";
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                        cst.ReasonText = MES_ReasonText.MES_First_Glass_Check_Report_Reply_NG;
                        string message = string.Format("PORTNO=[{0}] {1}", port.Data.PORTNO, cst.ReasonText);
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", message);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, message });
                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                    }
                }
                else
                {
                    cst.FirstGlassCheckReport = "Y";    // add by bruce 2015/10/22 for Robot check Fetch Glass use
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MES_LotProcessStartReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;
                err = string.Format("[BCS -> MES]=[{0}]  LINEID=[{1}], PORTID=[{2}]  LotProcessStartReply MES Reply Timeout.", trackKey, sArray[0], sArray[1]);

                string timeoutName = string.Format("{0}_{1}_MES_LotProcessStartReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                Port port = ObjectManager.PortManager.GetPort(sArray[1]);
                if (port != null)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                    if (cst != null)
                        cst.FirstGlassCheckReport = "Y";
                }
                else
                {
                    err = string.Format("[BCS -> MES]=[{0}]  LINEID=[{1}], PORTID=[{2}]  LotProcessStartReply MES Reply Timeout & Port Can't Find.", trackKey, sArray[0], sArray[1]);
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                object[] _data = new object[4]
                { 
                    trackKey,             
                    eBitResult.ON ,   
                    eReturnCode1.OK,  //MES reply timeout, return ok t3 function, cc.kuang 2015/07/22
                    port.Data.NODENO
                };
                Invoke(eServiceName.JobService, "FirstGlassCheckReportReply", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.125.	ProductLineIn       MES MeaasgeSet : Reports when a Glass moves into Manual Port or Line conveyer Port
        /// Add by marine for MES 2015/7/13
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eQPID"></param>
        /// <param name="portID"></param>
        /// <param name="unitID"></param>
        /// <param name="job"></param>
        public void ProductLineIn(string trxID, string lineName, Job job, string currentEQPID, string portID, string unitID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductLineIn") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portID);

                //if (job.GlassChipMaskBlockID == string.Empty)
                //{
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                //}
                //else
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}] Report ProductLineIn to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
            }

            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.126.	ProductLineOut      MES MessageSet : Reports when a Glass moves out Manual Port or Line conveyer Port.
        /// Add by marine for MES 2015/7/14
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eQPID"></param>
        /// <param name="portID"></param>
        /// <param name="unitID"></param>
        /// <param name="job"></param>
        public void ProductLineOut(string trxID, string lineName, Job job, string currentEQPID, string portID, string unitID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductLineOut") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portID);

                //if (job.GlassChipMaskBlockID == string.Empty)
                //{
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                //}
                //else
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;

                //Modify 2015/7/13
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                if (fabType == eFabType.CF)
                {
                    bodyNode[keyHost.INLINEINFLAG].InnerText = "N";
                }
                if (fabType == eFabType.ARRAY || fabType == eFabType.CELL)
                {
                    bodyNode[keyHost.INLINEINFLAG].InnerText = "Y";
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}] Report ProductLineOut to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
            }

            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.127.	ProductIn       MES MessageSet  :  
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        /// <param name="currentEQPID">Process EQPID</param>
        /// <param name="unitID">Process UNITID</param>
        /// <param name="portid">Port ID</param>
        /// <param name="traceLvl">Process Level ‘M’ – Machine ,‘U’ – Unit ,‘P’ – Port</param>
        /// <param name="processtime">traceLvl='P' is empty,</param>
        public void ProductIn(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, string processtime)
        {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                Unit unit = null;
                if (unitID != "")
                    unit = ObjectManager.UnitManager.GetUnit(unitID);

                //Watson Modify 20150320 For PMT Line 2 Send 改成在送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductIn") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                //Watson Modify 20150313 For PMT Line Send Different Data To MES.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                else
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                if (unitID != "") {
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                        bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    else
                        bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                } else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;

                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);

                #region Watson Add 20150313 For PMT Line Store、Receive 做法.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2")) {
                    Port port = ObjectManager.PortManager.GetPort(portid);
                    if (port != null) {
                        if (port.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) {
                            bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                            bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        }
                    }
                }
                #endregion

                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();


                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                bodyNode[keyHost.CARRIERNAME].InnerText = job.ToCstID; //沒有就報空

                if (line.Data.LINETYPE == eLineType.CELL.CCPCK)//2015 12 12 MES TEST 要求 PCK CHN Inbox 報法不一樣//2015 12 21CHN 同CST
                {
                    Port uloadPort = ObjectManager.PortManager.GetPort(portid);
                    if (uloadPort != null)
                    {
                        if (uloadPort.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)
                        //if (uloadPort.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//2015 12 21 sy
                        {
                            if (job.MesCstBody.LOTLIST.Count > 0)
                                bodyNode[keyHost.LOTNAME].InnerText = job.ToCstID;
                            bodyNode[keyHost.CARRIERNAME].InnerText = "";
                        }
                    }
                }
                
                //Watson modidyf 2011215 For MES 王鵬
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = job.ToSlotNo.Equals("0") ? "" : job.ToSlotNo;

                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID == string.Empty ? job.MesProduct.PRODUCTNAME : job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.HOSTRECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                //bodyNode[keyHost.CURRENTRECIPEID].InnerText = job.PPID;

                //Watson Modify 20141124 job.PPID 是寫給 機台的，不能上報MES AABBCCDDEEFF，所以需要重新組AA;BB;CC;DD;EE;FF;GG
                if (job.MES_PPID != null)
                    //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                    bodyNode[keyHost.BCRECIPEID].InnerText = job.MES_PPID; //ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.MES_PPID).Trim();  
                else
                    bodyNode[keyHost.BCRECIPEID].InnerText = ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.PPID).Trim();
                bodyNode[keyHost.HOSTRECIPEID].InnerText = job.MesProduct.PPID;

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PROCESSINGTIME].InnerText = processtime; //是Port 就不為空

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";  //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                //Jun Add 20141128 For Spec 修改
                if (line.Data.FABTYPE == eFabType.CELL.ToString()) {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3) {
                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                        double pnlsize = 0;
                        if (double.TryParse(job.CellSpecial.PanelSize, out pnlsize))
                            pnlsize = pnlsize / 100;
                        bodyNode[keyHost.GLASSSIZE].InnerText = pnlsize.ToString();
                    }
                }

                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText.Trim());

                if (otherline.File.HostMode == eHostMode.OFFLINE) {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, otherline.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductIn", eAgentName.MESAgent) == false) {
                    #region Watson Add 20150319 For PMT Line Special Rule
                    //Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText.Trim());

                    //if (otherline.File.HostMode != eHostMode.OFFLINE)
                    //{
                    //20170904 huangjiayin add for t3 cell
                    #region[t3 cell panel productin cst 1st timer special]
                    try
                    {
                        if (traceLvl == eMESTraceLevel.P && line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            Port p = ObjectManager.PortManager.GetPort(portid);
                            string pId_seq = portid+"_"+ p.File.CassetteSequenceNo;
                            //key [portid_cassetteSeqNo]
                            //value [portid]
                            if (p != null)
                            {
                               

                                lock (_1stJobInCellUldPort)
                                {
                                    //先移除本port非当前Seq的key
                                    List<string> keys=new List<string>();
                                    keys = _1stJobInCellUldPort.Keys.Where(w => w.Split('_')[0] == portid && w != pId_seq).ToList();
                                    foreach (string k in keys) { _1stJobInCellUldPort.Remove(k); }

                                    //each port 1st time productIn
                                    string timeoutName = pId_seq;
                                    if (!_1stJobInCellUldPort.ContainsKey(pId_seq))
                                    {
                                        _1stJobInCellUldPort.Add(pId_seq, portid);

                                        if (!_timerManager.IsAliveTimer(timeoutName))
                                        {
                                            _timerManager.CreateTimer(timeoutName, false, 3000, new System.Timers.ElapsedEventHandler(cell1stJobInUldPortTimeOut), trxID);
                                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[BCS ->MES][{0}] cell1stJobInUldPortTimer Started,2nd ProductIn to MES will be delayed.", trxID));
                                        }
                                    }
                                    else
                                    {

                                        while (_timerManager.IsAliveTimer(timeoutName))
                                        { Thread.Sleep(500); }
                                        

                                    }
                                }
                            }


                        }


                    }
                    catch (Exception ex)
                    {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    }
                    #endregion

                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, bodyNode[keyHost.LINENAME].InnerText));
                    //}
                    //else
                    //{
                    //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //       string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, otherline.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    //}
                    #endregion
                } else {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductIn to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
                }

                #region Send OEE
                
                //if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductIn", eAgentName.OEEAgent) == false)
                //{
                //    //-------------20141224 tom Add for OEE 1.12版本
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.ORIENTEDSITE, "t2"));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.ORAIENTEDFACTORYNAME, line.Data.FABTYPE));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.CURRENTSITE, "t2"));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.CURRENTFACTORYNAME, line.Data.FABTYPE));
                //    //----------------------------
                //    //bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTSPECNAME, job.MesProduct.PRODUCTRECIPENAME));
                //    if (job.MesCstBody.LOTLIST.Count > 0)
                //    {
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTSPECNAME, job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME));
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PROCESSOPERATIONNAME, job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME));
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTOWNER, job.MesCstBody.LOTLIST[0].PRODUCTOWNER));
                //    }
                //    // bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.OWNERTYPE, job.MesProduct.OWNERTYPE));
                //    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                //    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                //    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.

                //    string ownerType = job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "";
                //    if (line.Data.LINETYPE.Contains("FCUPK_TYPE"))
                //    {
                //        ownerType = job.CfSpecial.UPKOWNERTYPE;
                //    }

                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.OWNERTYPE, ownerType));

                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTINFO, job.MesProduct.OWNERID));
                //    bodyNode.RemoveChild(bodyNode[keyHost.CARRIERNAME]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.HOSTRECIPENAME]);
                //    //bodyNode.RemoveChild(bodyNode[keyHost.CURRENTRECIPEID]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.BCRECIPEID]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.HOSTRECIPEID]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.PROCESSINGTIME]);

                //    //SendToOEE(xml_doc);
                //    object[] _data = new object[8]
                //    { 
                  //      trxID,  /*0 TrackKey*/
                //        eqp.Data.LINEID,    /*1 LineName*/
                //        job,/*2 WIP Data */
                //        eqp.Data.NODEID, /*3 EQP ID */
                //        portid, /*4 PortID */
                //        unitID,/*5 Unit ID */
                //        traceLvl,/*6 Trace Level */
                //        processtime/*7 Process Time  */
                //    };
                //    object retVal = base.Invoke(eServiceName.OEEService, "ProductIn", _data);
                //}
                #endregion
            } catch (Exception ex) {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void cell1stJobInUldPortTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS ->MES][{0}] cell1stJobInUldPortTimeOut Terminated,Can Send 2nd ProductIn to MES", trackKey));

                if (_timerManager.IsAliveTimer(tmp))
                {
                    _timerManager.TerminateTimer(tmp);
                }


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.128.	ProductOut      MES MessageSet  : Fetch out from EQ or Taken out from CST Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">job entity(WIP)</param>
        /// <param name="currentEQPID">假如job位於機台中 就報，不是就填空</param>
        /// <param name="portid">假如job位於Port 就報，不是就填空</param>
        /// <param name="unitID">假如job位於Unit 就報，不是就填空</param>
        /// <param name="traceLvl">Process Level ‘M’ – Machine ,‘U’ – Unit ,‘P’ – Port</param>
        public void ProductOut(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                Unit unit = null;
                if (unitID != "")
                    unit = ObjectManager.UnitManager.GetUnit(unitID);

                //Watson Add 20150319 For PMT Line Special Rule 在送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductOut") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                else
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);


                if (unitID != "")
                {
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                        bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    else
                        bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                }
                else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);

                #region Watson Add 20150313 For PMT Line Fetch Out、Send 做法.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    Port port = ObjectManager.PortManager.GetPort(portid);
                    if (port != null)
                    {
                        if (port.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                        {
                            bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                            bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        }
                    }
                }
                #endregion

                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();

                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                bodyNode[keyHost.CARRIERNAME].InnerText = job.FromCstID;
                //Watson modidyf 2011215 For MES 王
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = job.FromSlotNo;
                if (job.GlassChipMaskBlockID == string.Empty)
                {
                    bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                }
                else
                    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PRODUCTJUDGE].InnerText = GetProductJudge(line, fabType, job);
                bodyNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);

                bodyNode[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation;  //watson modify 20141122 For 應該是要報機台即時報
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N"; //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                //Jun Add 20141128 For Spec 修改
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                    {
                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                        double pnlsize = 0;
                        if (double.TryParse(job.CellSpecial.PanelSize, out pnlsize))
                            pnlsize = pnlsize / 100;
                        bodyNode[keyHost.GLASSSIZE].InnerText = pnlsize.ToString();
                    }
                }
                //Watson Add 20150210 For MES Spec 3.85
                if (job.LastGlassFlag == "1")
                    bodyNode[keyHost.LASTGLASSFLAG].InnerText = "Y";
                else
                    bodyNode[keyHost.LASTGLASSFLAG].InnerText = "N";


                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText);

                if (otherline.File.HostMode == eHostMode.OFFLINE) //Offline 无需上报资料 20150325 Tom 
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductOut", eAgentName.MESAgent) == false)
                {
                    #region Watson Add PMT 特殊邏輯
                    //Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText);

                    //if (otherline.File.HostMode != eHostMode.OFFLINE)
                    //{
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, bodyNode[keyHost.LINENAME].InnerText));
                    //}
                    //else
                    //{
                    //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //      string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    //}
                    #endregion

                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductOut to MES.",
                      trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
                }

                #region OEE
                //if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductOut", eAgentName.OEEAgent) == false)
                //{
                //    //--------------20141224 tom Add for 0EE 1.12
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.ORIENTEDSITE, "t2"));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.ORAIENTEDFACTORYNAME, line.Data.FABTYPE));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.CURRENTSITE, "t2"));
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.CURRENTFACTORYNAME, line.Data.FABTYPE));
                //    //-------------20141224
                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.LINERECIPENAME, job.LineRecipeName));
                //    if (job.MesCstBody.LOTLIST.Count > 0)
                //    {
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTSPECNAME, job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME));
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PROCESSOPERATIONNAME, job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME));
                //        bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTOWNER, job.MesCstBody.LOTLIST[0].PRODUCTOWNER));
                //    }
                //    //bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.OWNERTYPE, job.MesProduct.OWNERTYPE));
                //    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                //    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                //    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                //    //UPK Line 需要使用UPKOWNERTYPE
                //    string ownerType = job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "";
                //    if (line.Data.LINETYPE.Contains("FCUPK_TYPE"))
                //    {
                //        ownerType = job.CfSpecial.UPKOWNERTYPE;
                //    }

                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.OWNERTYPE, ownerType));

                //    bodyNode.AppendChild(CreateXmlNode(xml_doc, keyHost.PRODUCTINFO, job.MesProduct.OWNERID));

                //    bodyNode.RemoveChild(bodyNode[keyHost.CARRIERNAME]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.PRODUCTJUDGE]);
                //    bodyNode.RemoveChild(bodyNode[keyHost.SUBPRODUCTGRADES]);

                //    //SendToOEE(xml_doc);
                //    object[] _data = new object[7]
                //    { 
                //        trxID,  /*0 TrackKey*/
                //        eqp.Data.LINEID,    /*1 LineName*/
                //        job,               /*2 WIP Data */
                //        eqp.Data.NODEID,    /*3 EQP ID */
                //        portid,               /*4 PortID */
                //        unitID,               /*5 Unit ID */
                //        traceLvl,/*6 Trace Level */
                //    };
                //    object retVal = base.Invoke(eServiceName.OEEService, "ProductOut", _data);
                //}

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.130.	ProductScrapped     MES MessageSet  : Job Remove Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        /// <param name="removeReasonCode">Remove Job Reason Code</param>
        public void ProductScrapped(string trxID, string lineName, Equipment eqp, Job job, string removeReasonCode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different. 改成在程式送出前做判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductScrapped Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductScrapped") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                else
                    bodyNode[keyHost.LOTNAME].InnerText = "";
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                XmlNode prodListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode prodCloneNode = prodListNode[keyHost.PRODUCT].Clone();
                prodListNode.RemoveAll();

                //一次只有一片
                XmlNode prodNode = prodCloneNode.Clone();
                prodNode[keyHost.CARRIERNAME].InnerText = job.FromCstID;
                prodNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                prodNode[keyHost.POSITION].InnerText = job.FromSlotNo;
                prodNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
                prodNode[keyHost.PRODUCTJUDGE].InnerText = ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                prodNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                prodNode[keyHost.REASONCODE].InnerText = removeReasonCode;

                //Add by marine for MES 2015/7/14
                XmlNode abnormalCodeList = prodNode[keyHost.ABNORMALCODELIST];
                XmlNode nodeClone = abnormalCodeList[keyHost.CODE].Clone();
                abnormalCodeList.RemoveAll();
                foreach (CODEc code in job.MesProduct.ABNORMALCODELIST)
                {
                    XmlNode node = nodeClone.Clone();
                    node[keyHost.ABNORMALVALUE].InnerText = code.ABNORMALVALUE;
                    node[keyHost.ABNORMALCODE].InnerText = code.ABNORMALCODE;
                    abnormalCodeList.AppendChild(node);
                }

                prodListNode.AppendChild(prodNode);

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductScrapped OK LINENAME=[{1}],LOTNAME=[{2}],MACHINENAME=[{3}].",
                            trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), bodyNode[keyHost.LOTNAME].InnerText, ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO)));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductScrapped OK LINENAME=[{1}],LOTNAME=[{2}],MACHINENAME=[{3}].",
                                trxID, bodyNode[keyHost.LINENAME].InnerText, bodyNode[keyHost.LOTNAME].InnerText, bodyNode[keyHost.MACHINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>
        /// ProductDeleteReport     MES MessageSet  : Job Delete Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="machineName">NodeID</param>
        /// <param name="unitName">UnitID</param>
        /// <param name="productName">JobID</param>
        /// <param name="processOperationName">processOperationName</param>
        /// <param name="userID">userID</param>
        /// <param name="transactionstarttime">transactionstarttime</param>
        public void ProductDeleteReport(string trxID, string lineName, string machineName, string unitName, string productName, string processOperationName, string userID, string transactionstarttime)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductDeleteReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                Line line = ObjectManager.LineManager.GetLine(lineName);

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.UNITNAME].InnerText = unitName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = processOperationName;
                bodyNode[keyHost.USERID].InnerText = userID;
                bodyNode[keyHost.TRANSACTIONSTARTTIME].InnerText = transactionstarttime;

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductDeleteReport OK LINENAME=[{1}],MachineNAME=[{2}],UnitNAME=[{3}],ProductName=[{4}].",
                            trxID,lineName,machineName,unitName,productName));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }
               
            }
            catch (Exception ex)
            {

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);

            }
 
        }

        /// <summary>
        /// 6.131.	ProductUnscrapped       MES MessageSet  : Job Recovery Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        /// <param name="recoveReasonCode">Recovery Job Reason Code</param>
        public void ProductUnscrapped(string trxID, string lineName, Equipment eqp, Job job, string recoveReasonCode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different. 改成在送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductScrapped Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductUnscrapped") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST.Count == 0 ? string.Empty : job.MesCstBody.LOTLIST[0].LOTNAME;
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                //一次只有一片
                XmlNode prodListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode prodCloneNode = prodListNode[keyHost.PRODUCT].Clone();
                prodListNode.RemoveAll();

                XmlNode prodNode = prodCloneNode.Clone();
                prodNode[keyHost.CARRIERNAME].InnerText = job.FromCstID;
                prodNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                prodNode[keyHost.POSITION].InnerText = job.FromSlotNo;
                prodNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
                prodNode[keyHost.PRODUCTJUDGE].InnerText = ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                prodNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;//sy edit same ProductScrapped 20160503
                //prodNode[keyHost.PRODUCTNAME].InnerText = job.EQPJobID;
                prodNode[keyHost.REASONCODE].InnerText = recoveReasonCode;
                prodListNode.AppendChild(prodNode);

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, bodyNode[keyHost.LINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.132.	QtimeOverReport      MES MessageSet : Use this message when BC reports the Inline Q-Time over with a Product name.
        /// Add by marine for T3 MES 2015/9/6
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="user"></param>
        /// <param name="qtime"></param>
        /// <param name="fromEQ"></param>
        /// <param name="fromTracelvl"></param>
        /// <param name="toEQ"></param>
        /// <param name="toTracelvl"></param>
        public void QtimeOverReport(string trxID, string lineName, string productName, string qtime, string fromEQ, string fromTracelvl, string startTime, string toEQ, string toTracelvl, string endTime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("QtimeOverReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.QTIME].InnerText = qtime;

                XmlNode fromListNode = bodyNode[keyHost.QTIMEFROM];
                fromListNode[keyHost.QFROMNAME].InnerText = fromEQ;
                fromListNode[keyHost.TRACELEVEL].InnerText = fromTracelvl;
                fromListNode[keyHost.STARTTIME].InnerText = startTime;

                XmlNode toListNode = bodyNode[keyHost.QTIMETO];
                toListNode[keyHost.QTONAME].InnerText = toEQ;
                toListNode[keyHost.TRACELEVEL].InnerText = toTracelvl;
                toListNode[keyHost.ENDTIME].InnerText = endTime;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.133.	QtimeSetChanged     MES MessageSet  : Q Time Change Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="user">change Q Time userID</param>
        /// <param name="qtime">Q time is minute.</param>
        /// <param name="fromEQ">Source EQ ,Port or Unit</param>
        /// <param name="fromTracelvl">‘M’ – Machine,‘U’ – Unit ,‘P’ – Port</param>
        /// <param name="toEQ">Target EQ ,Port or Unit</param>
        ///  <param name="toTracelvl">‘M’ – Machine,‘U’ – Unit ,‘P’ – Port</param>
        public void QtimeSetChanged(string trxID, string lineName, string user, string qtime, string fromEQ, string fromTracelvl, string toEQ, string toTracelvl)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("QtimeSetChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.DATETIME].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.EVENTUSER].InnerText = user;
                bodyNode[keyHost.QTIME].InnerText = qtime;

                XmlNode fromListNode = bodyNode[keyHost.QTIMEFROM];
                fromListNode[keyHost.QFROMNAME].InnerText = fromEQ;
                fromListNode[keyHost.TRACELEVEL].InnerText = fromTracelvl;

                XmlNode toListNode = bodyNode[keyHost.QTIMETO];
                toListNode[keyHost.QTONAME].InnerText = toEQ;
                toListNode[keyHost.TRACELEVEL].InnerText = toTracelvl;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.133.	QtimeSetChanged
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        public void QtimeSetChanged_OnLine(string trxID, string lineName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("QtimeSetChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.DATETIME].InnerText = GetTIMESTAMP();

                foreach (QtimeEntityData qdata in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    string mesTraceLevel = "M";
                    string fromNodeId = "";
                    string toNodeId = "";
                    bodyNode[keyHost.EVENTUSER].InnerText = qdata.QTIMEID;
                    bodyNode[keyHost.QTIME].InnerText = qdata.SETTIMEVALUE.ToString();
                    XmlNode fromListNode = bodyNode[keyHost.QTIMEFROM];

                    if (qdata.STARTNUNITID != string.Empty)
                    {
                        mesTraceLevel = "U";
                        fromNodeId = qdata.STARTNUNITID;
                    }
                    else
                    {
                        mesTraceLevel = "M";
                        fromNodeId = qdata.STARTNODEID;
                    }
                    fromListNode[keyHost.QFROMNAME].InnerText = fromNodeId;
                    fromListNode[keyHost.TRACELEVEL].InnerText = mesTraceLevel;

                    if (qdata.ENDNUNITID != string.Empty)
                    {
                        mesTraceLevel = "U";
                        toNodeId = qdata.ENDNUNITID;
                    }
                    else
                    {
                        mesTraceLevel = "M";
                        toNodeId = qdata.ENDNODEID;

                    }
                    XmlNode toListNode = bodyNode[keyHost.QTIMETO];
                    toListNode[keyHost.QTONAME].InnerText = toNodeId;
                    toListNode[keyHost.TRACELEVEL].InnerText = mesTraceLevel;

                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.146.	TerminalMessageSend     取消 MES MessageSet :MES Send Termial Message to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        //public void MES_TerminalMessageSend(XmlDocument xmlDoc)
        //{
        //    try
        //    {
        //        string returnCode = GetMESReturnCode(xmlDoc);
        //        string returnMessage = GetMESReturnMessage(xmlDoc);
        //        string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
        //        string trxID = GetTransactionID(xmlDoc);

        //        if (!CheckMESLineID(lineName))
        //        {
        //            Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
        //        }

        //        //to Do
        //        string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
        //        string portID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
        //        string termsg = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.TERMINALTEXT].InnerText;

        //        if (returnCode != "0")
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]  TerminalMessageSend  NG LINENAME=[{1}],PORTNAME={2},TERMINALTEXT={3},CODE=[{4}],MESSAGE=[{5}].",
        //                                trxID, lineName, portID, termsg, returnCode, returnMessage));
        //            //to do?
        //            return;
        //        }
        //        else
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]  TerminalMessageSend  OK LINENAME=[{1}],PORTNAME={2},TERMINALTEXT={3},CODE=[{4}],MESSAGE=[{5}].",
        //                                trxID, lineName, portID, termsg, returnCode, returnMessage));
        //            //to do?
        //            //Display Termain Message
        //            Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName,portID);
        //            string LocalNo = port.Data.NODENO;

        //            //Invoke(eServiceName.XXService , "CIMMessageSetCommand", new object[] { trxID, lineName,LocalNo ,termsg});

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 6.147.	UnitStateChanged        MES MessageSet : Unit Status change Event Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="unit">Unit Class</param>
        /// <param name="materialChangeflag">is Material Change?</param>
        /// <param name="alarm">Alarm occurs if Unit Status is Down</param>
        public void UnitStateChanged(string trxID, Unit unit, string alarmID, string alarmText, string alarmTime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(unit.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different. 改成程式送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, unit.Data.LINEID));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    
                //    //return;   
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("UnitStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(unit.Data.NODENO, unit.Data.UNITID);
                bodyNode[keyHost.UNITSTATENAME].InnerText = unit.File.MESStatus.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                //Unit 不用報, 此Flag 是Array段, TBSTR, TBWET在使用的, 需要報時,使用 line.File.Array_Material_Change 來判斷
                bodyNode[keyHost.MATERIALCHANGEFLAG].InnerText = "N";
                //Modify By James at 2014/09/28
                if (unit.File.MESStatus == "DOWN")
                {
                    bodyNode[keyHost.ALARMCODE].InnerText = alarmID;
                    bodyNode[keyHost.ALARMTEXT].InnerText = alarmText;
                    bodyNode[keyHost.ALARMTIMESTAMP].InnerText = alarmTime;
                }
                else
                {
                    bodyNode[keyHost.ALARMCODE].InnerText = string.Empty;
                    bodyNode[keyHost.ALARMTEXT].InnerText = string.Empty;
                    bodyNode[keyHost.ALARMTIMESTAMP].InnerText = string.Empty;
                }

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, bodyNode[keyHost.LINENAME].InnerText));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (unit.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, bodyNode[keyHost.LINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.152.	ValidateCleanCassetteRequest    MES MessageSet : BC requests MES to validate cassette positioned on EQP port for Cassette Clener. 
        ///                                         This message must be sent when cassette is loaded on port of Indexer. BC has to wait for the MES reply before continuing process for the cassette.
        /// Add by marine for MES 2015/7/13                                        
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="portID"></param>
        /// <param name="cassetteID"></param>
        public void ValidateCleanCassetteRequest(string trxID, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateCleanCassetteRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID.Trim();
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME=[{1})] [BCS -> MES][{0}] , EQUIPMENT=[{2}] PORT_ID=[{3}] CSTID=[{4}].",
                         trxID, ObjectManager.LineManager.GetLineID(port.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(port.Data.NODENO),
                         ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID), port.File.CassetteID));

                #region MES ValidateCleanCassetteReply Timeout
                string timeoutName = string.Format("{0}_{1}_MES_ValidateCleanCassetteReply", ObjectManager.LineManager.GetLineID(port.Data.LINEID),
                    ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateCleanCassetteT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 6.153.	ValidateCleanCassetteReply  MES MessageSet : MES sends cassette validation result upon cassette validation request from BC.
        /// Add by marine for MES 2015/7/13
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_ValidateCleanCassetteReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineId = GetLineName(xmlDoc);
                string trxId = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, lineId));
                }

                string portName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string carrierNmae = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;
                string carrierType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERTYPE].InnerText;
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;    //MES SPEC 1.18

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_ValidateCleanCassetteReply NG LINENAME =[{1}],PORTNAME =[{2}],CARRIERNAME =[{3}],CARRIERTYPE =[{4}],CODE =[{5}],MESSAGE =[{6}].",
                                         trxId, lineId, portName, carrierNmae, carrierType, returnCode, returnMessage));
                }
                else
                {
                    Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
                    string err = string.Empty;
                    if (!ValidateCassetteCheckData_Common(xmlDoc, ref line, ref eqp, ref port, ref cst, trxId, out err))
                    {
                        // Remove T9 TIMEOUT
                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCleanCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                    }
                    port.File.CassetteCleanPPID = lineRecipeName;   //update mes download cassette clean ppid
                    eqp = ObjectManager.EquipmentManager.GetEQP(cst.NodeNo);

                    // Download to PLC
                    IList<Job> mesJobs = new List<Job>();
                    Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxId });
                    //Decode_ValidateCassetteData(xmlDoc, line, eqp, port, cst, false);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_ValidateCleanCassetteReply OK LINENAME =[{1}],MACHINENAME =[{2}],CARRIERNAME =[{3}],CARRIERTYPE =[{4}],CODE =[{5}],MESSAGE =[{6}].",
                                         trxId, lineId, portName, carrierNmae, carrierType, returnCode, returnMessage));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }


        /// <summary>
        /// MES MessageSet :MES Request BC WIP
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_GetBCWipRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                string inboxname = GetMESINBOXName(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (!CheckMESLineID(line.Data.LINEID))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] GetBCWipRequest LineName =[{2}], mismatch =[{0}].", ServerName, trxID, line.Data.LINEID));
                }

                //to Do
                string lineRecipe = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string eventname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.EVENTCOMMENT].InnerText;
                string reasoncode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.REASONCODE].InnerText;

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] GetBCWipRequest  OK LINENAME=[{1}],EVENTCOMMENT=[{2}],REASONCODE=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, line.Data.LINEID, eventname, reasoncode, returnCode, returnMessage));


                //BC Reply WIP data
                GetBCWipReply(trxID, line.Data.LINEID, lineRecipe, eventname, reasoncode, inboxname);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// MES MessageSet : Reports when UV Mask has been used.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="meslinerecipeid">MES Download Line Recipe ID(ref GetBCWipRequest)</param>
        /// <param name="mesEventComment">MES Download Event Comment(ref GetBCWipRequest)</param>
        /// <param name="mesReasonCode">MES Download Reason Code(ref GetBCWipRequest)</param>
        /// <param name="joblist">WIP In Line(BC)</param>
        public void GetBCWipReply(string trxID, string lineName, string meslinerecipeid, string mesEventComment, string mesReasonCode, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("GetBCWipReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                SetINBOXNAME(xml_doc, inboxname);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.EVENTCOMMENT].InnerText = mesEventComment;
                bodyNode[keyHost.LINERECIPENAME].InnerText = meslinerecipeid;
                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = ObjectManager.JobManager.GetJobs().Count.ToString();
                bodyNode[keyHost.REASONCODE].InnerText = mesReasonCode;

                XmlNode glassListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode glassCloneNode = glassListNode[keyHost.PRODUCT].Clone();
                glassListNode.RemoveAll();

                foreach (Job job in ObjectManager.JobManager.GetJobs())
                {
                    if (job.PPID != meslinerecipeid)
                        continue;
                    XmlNode glassnode = glassCloneNode.Clone();
                    glassnode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    glassnode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    glassListNode.AppendChild(glassnode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// MES MessageSet : Lot Process Start Request to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Cassette">Cassette Entity</param>
        /// <param name="Cassette">Cassette Entity</param>
        //public void  LotProcessStartRequest (string trxID, string lineName,Cassette cst,string ppid,IList<Job> joblist)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //           IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessStartRequest") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        //to do 
        //        bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
        //        bodyNode[keyHost.LINERECIPENAME].InnerText =ppid ;
        //        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MesLot.LINERECIPENAME;
        //        bodyNode[keyHost.PORTNAME].InnerText = cst.PortID;
        //        bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

        //        XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
        //        XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
        //        lotListNode.RemoveAll();

        //        XmlNode lotnode = lotCloneNode.Clone();
        //        lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTNAME;
        //        lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.PROCESSOPERATIONNAME;
        //        lotnode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.PRODUCTOWNER;
        //        lotnode[keyHost.PRODUCTQUANTITY].InnerText = joblist.Count.ToString();
        //        //lotnode[keyHost.PRODUCTRECIPENAME].InnerText = cst.MesLOT.PRODUCTRECIPENAME;
        //        //lotnode[keyHost.HOSTPRODUCTRECIPENAME].InnerText = cst.MesLOT.PRODUCTRECIPENAME;
        //        lotnode[keyHost.PRODUCTSPECNAME].InnerText = cst.MesLot.PRODUCTSPECNAME;

        //        XmlNode glassListNode = lotCloneNode[keyHost.PRODUCTLIST];
        //        XmlNode glassCloneNode = glassListNode[keyHost.PRODUCT].Clone();
        //        glassListNode.RemoveAll();
        //        int i=0;
        //        foreach (Job job in joblist)
        //        {
        //            if (i == 0)
        //            {
        //                lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.PPID;
        //                lotnode[keyHost.HOSTPRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
        //                XmlNode garbage = lotnode[keyHost.PRODUCTLIST];
        //                lotnode.RemoveChild(garbage);
        //                lotListNode.AppendChild(lotnode);
        //            }
        //            i++;
        //                XmlNode glassnode = glassCloneNode.Clone();
        //                glassnode[keyHost.POSITION].InnerText = job.FromSlotNo;
        //                glassnode[keyHost.PRODUCTNAME].InnerText = job.EQPJobID;
        //                glassnode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
        //                glassnode[keyHost.PRODUCTRECIPENAME].InnerText = job.PPID;
        //                glassnode[keyHost.HOSTPRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
        //                glassListNode.AppendChild(glassnode);
        //        }
        //        lotnode.AppendChild(glassListNode);
        //        bodyNode.AppendChild(lotListNode);

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        /// <summary>
        /// 取消MES MessageSet :When BC send LotProcessStart to MES, BC must wait for MES reply. If MES reply is NG or timeout, then BC must cancel the CST and unload it ;
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        //public void MES_LotProcessStartReply(XmlDocument xmlDoc)
        //{
        //    try
        //    {
        //        string returnCode = GetMESReturnCode(xmlDoc);
        //        string returnMessage = GetMESReturnMessage(xmlDoc);
        //        string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
        //        string trxID = GetTransactionID(xmlDoc);

        //        if (!CheckMESLineID(lineName))
        //        {
        //            Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
        //        }

        //        //to Do
        //        string portID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
        //         string cassetteID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;


        //        if (returnCode != "0")
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]  LotProcessStartReply  NG LINENAME=[{1}],CARRIERNAME={2},CODE=[{3}],MESSAGE=[{4}].",
        //                                trxID, lineName, cassetteID, returnCode, returnMessage));
        //            //to do?
        //            //Cancel the Cassette.
        //            //Writer PLC

        //        }
        //        else
        //        {
        //            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]   LotProcessStartReply LINENAME=[{1}],CARRIERNAME={2} CODE=[{3}],MESSAGE=[{4}].",
        //                                trxID, lineName, cassetteID, returnCode, returnMessage));
        //            //to do?
        //            //Cassette Is OK
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// CELL: Reports when MaxCut is changed to dispatch to POL ,Or changed to not dispatch to POL.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="eqpID">EQP ID</param>
        /// <param name="dispatchPOL">Y->MaxCut changed to dispatch to POL ;N-> MaxCut changed to not dispatch to POL</param>
        public void MachineLinkProcessChangedReport(string trxID, string lineName, string eqpID, bool dispatchPOL)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but LINENAME=[{1}] is error!!",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    return;
                }

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", 
                        //trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineLinkProcessChangedReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.LINKPROCESSFLAG].InnerText = dispatchPOL == true ? "Y" : "N";   //LINKPROCESSFLAG:Y->MaxCut changed to dispatch to POL    N-> MaxCut changed to not dispatch to POL
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.156.	ValidateGlassRequest        MES MessageSet :  Validate Glass Request to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        ///   <param name="nodeno">Node No</param>
        ///   <param name="joblist">Job List</param>
        public void ValidateGlassRequest(string trxID, string lineName, Equipment eqp, IList<Job> joblist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", 
                        //trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateGlassRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = joblist.Count.ToString();

                XmlNode jobListNode = bodyNode[keyHost.PRODUCTLIST];
                //Modify by marine for T3 MES
                XmlNode jobNode = jobListNode[keyHost.PRODUCT];
                //bodyNode.RemoveChild(jobListNode);
                jobListNode.RemoveAll();

                //int i = 0;
                foreach (Job job in joblist)
                {
                    XmlNode jobNodeClone = jobNode.Clone();
                    jobNodeClone[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    jobNodeClone[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    
                    if (line.Data.LINEID.Contains("CCPIL") )//sy add 20151201
                    {
                        //PIT3 => PPA
                        if (eqp.Data.NODENO.Contains("L14"))//用keyCELLMachingName PIT 有 1 2 3 所以直接抓取NodeNo
                        {
                            if (eqp.File.EquipmentRunMode == "CROSSLINEMODE1" || eqp.File.EquipmentRunMode == "CROSSLINEMODE2" || eqp.File.EquipmentRunMode == "CROSSLINEMODE3")
                            {
                                jobNodeClone[keyHost.LINECHANGEFLAG].InnerText = "Y";
                            }
                        }
                        else if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPPA))
                        {
                            if (job.CellSpecial.CrossLineReportFlag  == eBitResult.ON)
                            {
                                jobNodeClone[keyHost.LINECHANGEFLAG].InnerText = "Y";
                            }
                        }
                        else
                        {
                            jobNodeClone[keyHost.LINECHANGEFLAG].InnerText = "N";
                        }    
                    }
                    else
                    {
                        //Add by marine for T3 MES 2015/9/9
                        jobNodeClone[keyHost.LINECHANGEFLAG].InnerText = "";
                    }
                    
                    jobListNode.AppendChild(jobNodeClone);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                //sy add 20151201
                #region MES _ValidateGlassRequestReply Timeout
                string timeoutName = string.Format("{0}_MES_ValidateGlassRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateGlassT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.157.	ValidateGlassReply      MES MessagetSet : Validate Glass Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ValidateGlassReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }
                #region MES _ValidateGlassRequestReply Timeout
                string timeoutName = string.Format("{0}_MES_ValidateGlassRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                string machineName = xmlDoc["MESSAGE"]["BODY"]["MACHINENAME"].InnerText;
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.GlassChangeLineRequestReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_GlassChangeLineRequestReply", trxID));
                    return;
                }
                #endregion
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}] MES_ValidateGlassReply NG LINENAME =[{1}],MACHINENAME =[{2}],CODE={3},MESSAGE={4}.",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?

                    Invoke(eServiceName.CELLSpecialService, "GlassChangeLineRequestReply", new object[] { eqpID, eBitResult.ON, trxID, eReturnCode1.NG });

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}] MES_ValidateGlassReply OK LINENAME =[{1}],MACHINENAME =[{2}],CODE={3},MESSAGE={4}.",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?
                    Invoke(eServiceName.CELLSpecialService, "GlassChangeLineRequestReply", new object[] { eqpID, eBitResult.ON, trxID, eReturnCode1.OK });

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ValidateGlassT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.GlassChangeLineRequestReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_GlassChangeLineRequestReply", sArray[0]));
                    return;
                }
                #endregion
                string timeoutName = string.Format("{0}_MES_ValidateGlassRequestReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Invoke(eServiceName.CELLSpecialService, "GlassChangeLineRequestReply", new object[] { eqpID, eBitResult.ON, sArray[0], eReturnCode1.NG });

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "ValidateGlassT9Timeout");

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// MES MessageSet : MES  Reply Facility check Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        //public void  MES_PreCheckFacilityRequest (XmlDocument xmlDoc)
        //{
        //    try
        //    {
        //        string returnCode = GetMESReturnCode(xmlDoc);
        //        string returnMessage = GetMESReturnMessage(xmlDoc);
        //        string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
        //        string trxID = GetTransactionID(xmlDoc);

        //        if (!CheckMESLineID(lineName))
        //        {
        //            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
        //        }

        //        //to Do
        //         string mesLineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINENAME].InnerText;

        //        if (returnCode != "0")
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]   PreCheckFacilityRequest  NG LINENAME=[{1}],MESLineName={2},CODE=[{3}],MESSAGE=[{4}].",
        //                                trxID, lineName, mesLineName, returnCode, returnMessage));
        //            //to do?

        //        }
        //        else
        //        {
        //            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS <- MES][{0}]   PreCheckFacilityRequestOK LINENAME=[{1}],MESLineName={2} CODE=[{3}],MESSAGE=[{4}].",
        //                                trxID, lineName, mesLineName, returnCode, returnMessage));
        //            //to do?

        //            PreCheckFacilityReply(trxID, lineName, ObjectManager.EquipmentManager.GetEQPs());
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// MES MessageSet :BC  Reply Facility check Reply to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        ///  <param name="nodelist">Node List</param>
        //public void PreCheckFacilityReply(string trxID, string lineName,IList<Equipment> nodelist)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("PreCheckFacilityReply") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
        //        XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
        //        eqpListNode.RemoveAll();

        //        foreach (Equipment eqp in nodelist)
        //        {
        //             XmlNode eqpnode = eqpCloneNode.Clone();
        //             eqpnode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;

        //             XmlNode faciltListNode = eqpnode[keyHost.FACILITYPARALIST];
        //             XmlNode facCloneNode = faciltListNode[keyHost.PARA].Clone();
        //              faciltListNode.RemoveAll();
        //            //to do還不知道怎麼拆？
        //             //foreach (Equipment eqpin nodelist)
        //             //{
        //                    XmlNode facnode = facCloneNode.Clone();
        //                    facnode[keyHost.PARANAME].InnerText = "";
        //                    facnode[keyHost.VALUETYPE].InnerText = "";
        //                    faciltListNode.AppendChild(facnode);
        //             //}   
        //             eqpListNode.AppendChild(eqpnode);
        //       }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// MES MessageSet :  Check Facility Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodelist">所有機台iList</param>
        //public void  CheckFacilityReportReply(string trxID, string lineName,IList<Equipment> nodelist)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("CheckFacilityReportReply") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        bodyNode[keyHost.VALIRESULT].InnerText = "Y";
        //        XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
        //        XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
        //        eqpListNode.RemoveAll();

        //        foreach (Equipment eqp in nodelist)
        //        {
        //            XmlNode eqpnode = eqpCloneNode.Clone();
        //            eqpnode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;

        //            XmlNode faciltListNode = eqpnode[keyHost.FACILITYPARALIST];
        //            XmlNode facCloneNode = faciltListNode[keyHost.PARA].Clone();
        //            faciltListNode.RemoveAll();
        //            //to do還不知道怎麼拆？
        //            //foreach (Equipment eqpin nodelist)
        //            //{
        //            XmlNode facnode = facCloneNode.Clone();
        //            facnode[keyHost.PARANAME].InnerText = "";
        //            facnode[keyHost.VALUETYPE].InnerText = "";
        //            eqpnode.AppendChild(facnode);
        //            //}   
        //            eqpListNode.AppendChild(eqpnode);
        //        }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}


        /// <summary>
        /// MES MessageSet :  Glass Process Start Request to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="lineName">Current node Entity</param>
        /// <param name="job">Job entity</param>
        //public void  GlassProcessStartRequest (string trxID, string lineName,Equipment eqp,Job job)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //           IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("GlassProcessStartRequest") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        //to do 
        //        bodyNode[keyHost.LINERECIPENAME].InnerText = job.MesCstBody.LotData.LINERECIPENAME;
        //        bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
        //        bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

        //        XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
        //        XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
        //        lotListNode.RemoveAll();

        //        //目前只會有一個Lot
        //        //foreach (string param in recipeparam)
        //        //{
        //            XmlNode lotnode = lotCloneNode.Clone();
        //            lotnode[keyHost.LOTNAME].InnerText = job.MesCstBody.LotData.LOTNAME;
        //            lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
        //            lotnode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LotData.PRODUCTSPECNAME;
        //            lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LotData.PROCESSOPERATIONNAME;
        //            lotnode[keyHost.PRODUCTQUANTITY].InnerText = "1";
        //            lotnode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LotData.PRODUCTOWNER;
        //            lotListNode.AppendChild(lotnode);
        //        //}

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}


        /* 3.70 刪掉
        public void MaterialBomRequest(string trxID, string lineName, Equipment eqp,Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] MaterialBomRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialBomRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //To do?
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                if (job.MesCstBody.LOTLIST[0].BOMLIST.Count <=0)
                        bodyNode[keyHost.PRODUCTSPECVERSION].InnerText =string.Empty;
                bodyNode[keyHost.PRODUCTSPECVERSION].InnerText = job.MesCstBody.LOTLIST[0].BOMLIST[0].PRODUCTSPECVERSION;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }
        */

        public void MES_MaterialBomReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string linerecipename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string productspecname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string productspecversion = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECVERSION].InnerText;

                XmlNode bomlist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMLIST];
                foreach (XmlNode bom in bomlist)
                {
                    // <PRIORITY></PRIORITY>
                    //<MATERIALSPECNAME></MATERIALSPECNAME>
                    //<MATERIALTYPE></MATERIALTYPE>
                    //<MATERIALFACTORYNAME></MATERIALFACTORYNAME>
                }


                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MaxCutGlassProcessEndReply NG LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MaxCutGlassProcessEndReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>
        /// MES MessageSet : Check Facility Request to MES 
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        //public void CheckFacilityReport_(string trxID, string lineName, IList<CheckFacilityReport.MACHINEc> machineList)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("CheckFacilityReport") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫

        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
        //        bodyNode[keyHost.LINENAME].InnerText = lineName;

        //        XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
        //        XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
        //        eqpListNode.RemoveAll();

        //        foreach (CheckFacilityReport.MACHINEc machine in machineList)
        //        {
        //            XmlNode eqpNode = eqpCloneNode.Clone();
        //            eqpNode[keyHost.MACHINENAME].InnerText = machine.MACHINENAME;

        //            XmlNode paraNodeList = eqpNode[keyHost.FACILITYPARALIST];
        //            XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
        //            paraNodeList.RemoveAll();

        //            foreach (var para in machine.FACILITYPARALIST)
        //            {
        //                XmlNode paraNode = paraNodeClone.Clone();
        //                paraNode[keyHost.PARANAME].InnerText = para.PARANAME;
        //                paraNode[keyHost.VALUETYPE].InnerText = para.VALUETYPE;

        //                paraNodeList.AppendChild(paraNode);
        //            }
        //            eqpListNode.AppendChild(eqpNode);
        //        }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// //配合RecipeService GetRecipeParameter Decode Rule (EQID@UNITID^PARANAME or EQID@PARANAME)
        /// </summary>
        /// <param name="paraName">EQPID_UNITID_ParaName or EQPID_ParaName</param>
        /// <returns>ParaName</returns>
        public string GetPARANAMEORIENTED(string lineName, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName); //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                int p = 0;
                p = paraName.IndexOf('@');
                if (p > 0)
                    return (paraName.Substring(p + 1, paraName.Length - p - 1));  //檢查到@表示有unitid，直接傳回@後面的值(name)
                p = paraName.IndexOf('^');  //查到^表示只有eqpid，傳回^後面的值(name)
                return (paraName.Substring(p + 1, paraName.Length - p - 1));
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return paraName;
            }
        }

        /// <summary>
        /// MES 多了TRACELEVEL 要上報，只能從ParaName的結構去判斷是不是unit或machine，此案不會有port
        /// </summary>
        /// <param name="paraName">Recipe Parameter Name</param>
        /// <returns>‘M’ – Machine ;‘U’ – Unit </returns>
        public string GetPARANAME_TRACELEVEL(string lineName, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName); //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                int p = 0;
                p = paraName.IndexOf('@');
                if (p > 0)
                    return "U";  //檢查到@表示有unitid，直接傳回@後面的值(name)
                else
                    return "M";
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return "M";
            }
        }

        /// <summary>
        /// Recipe ParaName Rule is EQID@UNITID^PARANAME
        /// </summary>
        /// <param name="paraName">EQID@UNITID^PARANAME</param>
        /// <returns>PARANAME</returns>
        public string GetPARANAME(string lineName, string eqpNO, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName);  //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    if (lineName.Contains(keyCELLPMTLINE.CBPTI) && (eqpNO == "L2"))
                    {
                        if (paraName.Length > 8)
                        {
                            string pmtEQPID = paraName.Substring(0, 8);
                            paraName = paraName.Replace(pmtEQPID, ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString());
                        }
                    }
                }

                return (paraName.Replace('@', '_').Replace('^', '_'));
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return paraName;
            }
        }


        private string GetLineName(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/BODY/LINENAME").InnerText;
        }
        private string GetPortID(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/BODY/PORTID").InnerText;
        }
        private bool CheckMESLineID(string mesDownloadLineName)
        {
            foreach (Line line in ObjectManager.LineManager.GetLines())
            {
                if (ObjectManager.LineManager.GetLineID(line.Data.LINEID) == mesDownloadLineName)
                    return true;

            }
            return false;
        }

        private XmlNode CreateXmlNode(XmlDocument xml, string name, string value)
        {
            XmlNode xmlNode = xml.CreateElement(name);

            if (value != null)
                xmlNode.InnerText = value;

            return xmlNode;
        }

        private string GetTransactionID(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
        }

        private string GetMESReturnCode(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/RETURN/RETURNCODE").InnerText;
        }

        private string GetMESReturnMessage(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/RETURN/RETURNMESSAGE").InnerText;
        }

        private void SetTransactionID(XmlDocument aDoc, string transactionId)
        {
            aDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = transactionId;
        }

        private string GetTIMESTAMP()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }


        //private Line GetPMTOtherLine(Line line)
        //{
        //    foreach (Line ll in ObjectManager.LineManager.GetLines())
        //    {
        //        if (ll.Data.LINEID == line.Data.LINEID)//此处要使用LineID 不能使用LineType  20150306 Tom
        //            continue;
        //        else
        //            return ll;
        //    }
        //    return null;
        //}
        //private Equipment GetPMTOtherLoader(Equipment eqp)
        //{
        //    foreach (Line ll in ObjectManager.LineManager.GetLines())
        //    {
        //        if (ll.Data.LINEID == eqp.Data.LINEID)
        //            continue;
        //        foreach (Equipment ee in ObjectManager.EquipmentManager.GetEQPsByLine(ll.Data.LINEID))
        //        {
        //            if (ee.Data.NODENO == eqp.Data.NODENO)
        //                return ee;
        //            else
        //                continue;
        //        }
        //    }
        //    return null;
        //}
        //private Unit GetPMTOtherUnit(Unit unit)
        //{
        //    foreach (Line ll in ObjectManager.LineManager.GetLines())
        //    {
        //        if (ll.Data.LINEID == unit.Data.LINEID)
        //            continue;
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
        //        foreach (Unit uu in ObjectManager.UnitManager.GetUnits())
        //        {
        //            if (uu.Data.UNITID == unit.Data.UNITID)
        //                return uu;
        //            else
        //                continue;
        //        }
        //    }
        //    return null;
        //}
        //public bool PMTReSendFunction(Line line, string eqpID, out string otherlineid, out string othereqpid)
        //{
        //    otherlineid = string.Empty;
        //    othereqpid = string.Empty;
        //    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpID);
        //        if (eqp.Data.NODENO == "L2")
        //        {
        //            Line otherline = GetPMTOtherLine(line);
        //            if (otherline.File.HostMode == eHostMode.OFFLINE)
        //            {
        //                return false;
        //            }
        //            else
        //            {
        //                otherlineid = otherline.Data.LINEID;
        //                othereqpid = GetPMTOtherLoader(eqp).Data.NODEID;
        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //}

        /// <summary>
        ///  MES MessageSet : UPK Line切換CF or Array
        /// </summary>
        /// <param name="serverName">FCUPK100 or TBUPK100</param>
        public void MES_SwitchServerName(string serverName)
        {
            try
            {
                Invoke(eAgentName.MESAgent, "SwitchServerName", new object[] { serverName });
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("From [{0}] To [{1}]", _serverName, serverName));
                _serverName = serverName;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}