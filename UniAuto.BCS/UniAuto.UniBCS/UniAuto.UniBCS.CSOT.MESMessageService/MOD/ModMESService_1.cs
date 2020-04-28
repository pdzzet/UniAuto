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
   public  partial class ModMESService : AbstractService
    {
       private bool _tryOnline = true;
       private bool _firstRun = true;

       private IServerAgent GetServerAgent()
       {
           return GetServerAgent(eAgentName.MESAgent);
       }
       private void SendToMES(XmlDocument xml)
       {
           xMessage msg = new xMessage();
           msg.Name = xml[keyHost.MESSAGE][keyHost.HEADER][keyHost.MESSAGENAME].InnerText;
           msg.TransactionID = xml[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
           msg.ToAgent = eAgentName.MESAgent;
           msg.Data = xml.OuterXml;
           PutMessage(msg);
       }
       private void SetTransactionID(XmlDocument aDoc, string transactionId)
       {
           aDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = transactionId;
       }

       private string GetTIMESTAMP()
       {
           return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
       }
       private string GetTransactionID(XmlDocument aDoc)
       {
           return aDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
       }
       private string GetLineName(XmlDocument aDoc)
       {
           return aDoc.SelectSingleNode("//MESSAGE/BODY/LINENAME").InnerText;
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
       private string GetMESReturnCode(XmlDocument aDoc)
       {
           return aDoc.SelectSingleNode("//MESSAGE/RETURN/RETURNCODE").InnerText;
       }
       private string GetMESReturnMessage(XmlDocument aDoc)
       {
           return aDoc.SelectSingleNode("//MESSAGE/RETURN/RETURNMESSAGE").InnerText;
       }
       private string GetMESINBOXName(XmlDocument aDoc)
       {
           if (aDoc.SelectSingleNode("//MESSAGE/HEADER/INBOXNAME") == null)
               return string.Empty;
           return aDoc.SelectSingleNode("//MESSAGE/HEADER/INBOXNAME").InnerText;
       }
       private void SetINBOXNAME(XmlDocument aDoc, string mesINBOXNAme)
       {
           aDoc.SelectSingleNode("//MESSAGE/HEADER/INBOXNAME").InnerText = mesINBOXNAme;
       }

        //AlarmReport
       public void AlarmReport(string trxID, string lineName, string eQPID, string unitID, string alarmID, string alarmLevel, string alarmState, string alarmText)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);
               Unit unit = ObjectManager.UnitManager.GetUnit(unitID);

               // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
               if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

               if (eqp.File.EquipmentOperationMode == eEQPOperationMode.MANUAL && ParameterManager[eREPORT_SWITCH.MANUAL_ALARM_MES].GetBoolean() == false)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={0}] [BCS -> MES][{1}] Skip AlarmReport Send, Because Equipment=[{2}] is Manual Mode.",
                           lineName, trxID, eqp.Data.NODENO));
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AlarmReport") as XmlDocument;

               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
               bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
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
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AreYouThereRequest
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
        //AreYouThereReply
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
        //CurrentDateTime
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

           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AgingTemperatureReport
       public void AgingTemperatureReport(string trxID, string lineName, Equipment eqp)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AgingTemperatureReport") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
               //bodyNode[keyHost.TEMPERATURELIST].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               //bodyNode[keyHost.TEMPERATUREITEM].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               //bodyNode[keyHost.ITEMNAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               //bodyNode[keyHost.ITEMVALUE].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AgingWorkInReport
       public void AgingWorkInReport(string trxID, string lineName, Equipment eqp,string palletteID)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }
               Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletteID);

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AgingWorkInReport") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                if (pallet == null)
                {
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format(" CAN NOT FIND Pallet_ID=[{0}] IN Pallet OBJECT!", palletteID));
                    bodyNode[keyHost.PALLETNAME].InnerText = string.Empty;
                }
                else
                {
                    bodyNode[keyHost.PALLETNAME].InnerText = pallet.File.PalletName;
                }
               bodyNode[keyHost.DATETIME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AgingWorkOutReport
       public void AgingWorkOutReport(string trxID, string lineName, Equipment eqp, string palletteID)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AgingWorkOutReport") as XmlDocument;
               SetTransactionID(xml_doc, trxID);
               Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletteID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
               if (pallet == null)
               {
                   Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                   string.Format(" CAN NOT FIND Pallet_ID=[{0}] IN Pallet OBJECT!", palletteID));
                   bodyNode[keyHost.PALLETNAME].InnerText = string.Empty;
               }
               else
               {
                   bodyNode[keyHost.PALLETNAME].InnerText = pallet.File.PalletName;
               }
               bodyNode[keyHost.DATETIME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AssemblyInspectionRequest
       public void AssemblyInspectionRequest(string trxID, string lineName, Equipment eqp, string palletteID, string user)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AssemblyInspectionRequest") as XmlDocument;
               SetTransactionID(xml_doc, trxID);
               Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletteID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
               //bodyNode[keyHost.PANELNAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               if (pallet == null)
               {
                   Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                   string.Format(" CAN NOT FIND Pallet_ID=[{0}] IN Pallet OBJECT!", palletteID));
                   bodyNode[keyHost.PALLETNAME].InnerText = string.Empty;
               }
               else
               {
                   bodyNode[keyHost.PALLETNAME].InnerText = pallet.File.PalletName;
               }
               bodyNode[keyHost.EVENTUSER].InnerText = user;
               //bodyNode[keyHost.PROCESSSTATE].InnerText = line.File.Status.ToString();

               SendToMES(xml_doc);

               #region MES Assembly Inspection Reply Timeout
               string timeoutName = string.Format("MES_AssemblyInspectionReply");
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                   new System.Timers.ElapsedEventHandler(AssemblyInspectionReply_Timeout), trxID);
               #endregion

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AssemblyInspectionReply
       public void MES_AssemblyInspectionReply(XmlDocument xmlDoc)
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

               #region kill Timeout
               string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               #endregion

               string agingtime = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.AGINGTIME].InnerText;
               string palletname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;

           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       private void AssemblyInspectionReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
       {
           try
           {
               UserTimer timer = subjet as UserTimer;
               string tmp = timer.TimerId;
               string trackKey = timer.State.ToString();
               string[] sArray = tmp.Split('_');

               string err = string.Empty;

               err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  AssemblyInspection MES Reply Timeout.", trackKey, sArray[0]);

               string timeoutName = string.Format("{0}_MES_AssemblyInspectionReply", sArray[0]);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }

               Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
           }
           catch (Exception ex)
           {
               this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
           }
       }
        //AuthorityModificationReport
       public void AuthorityModificationReport(string trxID, string lineName, Equipment eqp)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("AuthorityModificationReport") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //BoxIdCreateRequest
       public void BoxIdCreateRequest(string trxID, string lineName, string productName, string boxType)
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
               XmlDocument xml_doc = agent.GetTransactionFormat("BoxIdCreateRequest") as XmlDocument;

               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

               bodyNode[keyHost.LINENAME].InnerText = lineName;
               bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
               //Add by marine for T3 MES 2015/9/16
               bodyNode[keyHost.BOXTYPE].InnerText = boxType;//機台不會知道 都給空值//PPK 用OUTBOX

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxIdCreateRequest OK LINENAME=[{1}], productName=[{2}].",
                        trxID, lineName, productName));
               #region MES BoxIdCreateRequest Timeout
               string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                   new System.Timers.ElapsedEventHandler(LotIdCreateRequestT9Timeout), trxID);
               #endregion
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       private void LotIdCreateRequestT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
       {
           try
           {
               UserTimer timer = subjet as UserTimer;
               string tmp = timer.TimerId;
               string trackKey = timer.State.ToString();
               string[] sArray = tmp.Split('_');
               #region Get and Remove Reply Key
               string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
               string eqpID = Repository.Remove(key).ToString();
               if (eqpID == null)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIdCreateRequestReply", sArray[0]));
                   return;
               }
               #endregion
               string timeoutName = string.Format("{0}_MES_LotIdCreateRequestReply", sArray[0]);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, sArray[0], eReturnCode1.NG, "", "0" });

               Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "LotIdCreateRequestT9Timeout");

           }
           catch (Exception ex)
           {
               this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
           }
       }
       //BoxIdCreateRequestRply
       public void MES_BoxIdCreateReply(XmlDocument xmlDoc)
       {
           try
           {
               string returnCode = GetMESReturnCode(xmlDoc);
               string returnMessage = GetMESReturnMessage(xmlDoc);
               string lineName = GetLineName(xmlDoc);
               string trxID = GetTransactionID(xmlDoc);

               if (!CheckMESLineID(lineName))
                   Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));

               eReturnCode1 rtcode = returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG;
               #region Get and Remove Reply Key
               string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
               string eqpID = Repository.Remove(key).ToString();
               if (eqpID == null)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                   return;
               }
               #endregion
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqpID));
               #region kill Timeout
               string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               #endregion
               string boxName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
               string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
               #region [Reply NG]
               if (rtcode == eReturnCode1.NG)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxIdCreateReply NG LINENAME=[{1}],LOTNAME=[{2}],PRODUCTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                       trxID, lineName, boxName, productName, returnCode, returnMessage));
                   Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, boxName, "0" });
                   return;
               }
               #endregion
               string capacity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CAPACITY].InnerText;//MES reply NG 時 capacity 不會有值。
               string boxType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
               if (line.Data.LINETYPE == eLineType.CELL.CCPPK)
               {
                   #region [UpDate CST]
                   #region Get and Remove Reply Key boxID
                   string boxIDkey = keyBoxReplyPLCKey.PaperBoxReply;
                   string boxID = Repository.Remove(boxIDkey).ToString();
                   if (boxID == null)
                   {
                       Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,BOX NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                       return;
                   }
                   #endregion
                   Cassette cst = ObjectManager.CassetteManager.GetCassette(boxID);
                   if (cst == null)
                   {
                       Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                       string.Format("EQUIPMENT=[{0}] CAN NOT FIND BOX=[{1}] IN CST OBJECT!", eqpID, boxID));
                       return;
                   }
                   lock (cst)
                   {
                       cst.SubBoxID = boxName;
                   }
                   ObjectManager.CassetteManager.EnqueueSave(cst);
                   BoxProcessStarted(trxID, line, boxName);
                   #endregion
               }
               else if (line.Data.LINETYPE == eLineType.CELL.CCPCK)
               {
                   #region [NEW CST]
                   #region Get and Remove Reply Key Port
                   string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                   string portNo = Repository.Remove(Portkey).ToString();
                   if (portNo == null)
                   {
                       Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,PORT NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                       return;
                   }
                   #endregion
                   if (portNo.PadLeft(2, '0') != "00")
                   {
                       Cassette cst = new Cassette();
                       cst.CassetteID = boxName;
                       cst.BoxName = boxName;
                       cst.PortID = portNo.PadLeft(2, '0');
                       cst.PortNo = portNo.PadLeft(2, '0');
                       ObjectManager.CassetteManager.CreateCassette(cst);
                   }
                   #endregion
               }
               else
               {
                   #region [UpDate CST]
                   #region Get and Remove Reply Key Port
                   string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                   string portNo = Repository.Remove(Portkey).ToString();
                   if (portNo == null)
                   {
                       Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,PORT NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                       return;
                   }
                   #endregion
                   if (portNo.PadLeft(2, '0') != "00")
                   {
                       Cassette cst = ObjectManager.CassetteManager.GetCassette(lineName, eqpID, portNo.PadLeft(2, '0'));
                       if (cst == null)
                       {
                           Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                           string.Format("EQUIPMENT=[{0}] CAN NOT FIND PORT_NO=[{1}] IN CST OBJECT!", eqpID, portNo));
                           return;
                       }
                       cst.BoxName = boxName;
                   }
                   #endregion
               }


               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxIdCreateReply OK LINENAME=[{1}],LOTNAME=[{2}],PRODUCTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                   trxID, lineName, boxName, productName, returnCode, returnMessage));

               Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, boxName, capacity });
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       //BoxProcessStarted
       public void BoxProcessStarted(string trxID, Port port, Cassette cst)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

               bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
               bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;

               //Modify by marine for T3 MES 2015/8/20
               //bodyNode[keyHost.PPID].InnerText = cst.PPID;
               //bodyNode[keyHost.HOSTPPID].InnerText = cst.MES_CstData.LOTLIST.Count == 0 ? cst.PPID : cst.MES_CstData.LOTLIST[0].PPID;

               bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
               bodyNode[keyHost.BOXNAME].InnerText = cst.CassetteID;

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}],LINERECIPENAME=[{2}],BOXNAME=[{3}].",
                   trxID, line.Data.LINEID, cst.LineRecipeName, cst.CassetteID));

           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       /// <summary>
       /// BoxProcessStarted[CCPPK]
       /// </summary>//shihyang add 20150817
       /// <param name="trxID"></param>
       /// <param name="port"></param>
       /// <param name="cst"></param>
       public void BoxProcessStarted(string trxID, Line line, string boxID)
       {
           try
           {
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
               bodyNode[keyHost.LINERECIPENAME].InnerText = "";
               bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = "";
               bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

               XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
               XmlNode box = boxListNode[keyHost.BOX];
               boxListNode.RemoveAll();

               XmlNode boxClone = box.Clone();
               boxClone[keyHost.BOXNAME].InnerText = boxID;
               boxListNode.AppendChild(boxClone);

               SendToMES(xml_doc);
               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}].",
                   trxID, line.Data.LINEID));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //BoxProcessAbnormalEnd
       public void BoxProcessAbnormalEnd(string trxID, string lineName, Equipment eqp, string portID, string boxName)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessAbnormalEnd") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
               bodyNode[keyHost.PORTNAME].InnerText = portID;
               bodyNode[keyHost.BOXNAME].InnerText = boxName;
               bodyNode[keyHost.LINERECIPENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

               XmlNode box = bodyNode[keyHost.BOX];
               box[keyHost.BOXNAME].InnerText = boxName;
               box[keyHost.PRDCARRIERSETCODE].InnerText = "";
               box[keyHost.PRODUCTQUANTITY].InnerText = "";

               XmlNode productList = bodyNode[keyHost.PRODUCTLIST];

               XmlNode product = bodyNode[keyHost.PRODUCT];
               product[keyHost.POSITION].InnerText = "";
               product[keyHost.PRODUCTNAME].InnerText = "";
               product[keyHost.PRODUCTGRADE].InnerText = "";

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //BoxProcessAbort
        //BoxProcdssAborteds
        //InBoxProcessEnd
        //InBoxProcessEndReply
        //ChangeTemperatureRequest
        //ChangeTemperatureReply
        //COFCastReport
        //CurrentDateTimeRequest
        //EquipmentTactTimeReport
        //FinalTestResultRequest
        //FinalTestResultReply
        //FacilityCriteriaSend
        //FacilityParameterRequest
        //FacilityCheckRequest
        //InspectionReport
        //LotProcessAbnormalEndReply
        //LotProcessCanceled
        //MachineControlStateChanged
       public void MachineControlStateChanged(string trxID, string lineName, string controlStatus)
       {
           try
           {
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChanged") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               bodyNode[keyHost.CONTROLSTATENAME].InnerText = controlStatus.ToString();
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

               SendToMES(xml_doc);
               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, controlStatus));

               // To Do 判断ControlState 是否是Offline ，如果不是Offline 则需要与MES 做资料同步
               Line line = ObjectManager.LineManager.GetLine(lineName);

               if (line == null) throw new Exception(string.Format("Can't find Line ID =[{0}] in LineEntity!", lineName));

               line.File.PreHostMode = line.File.HostMode;

               switch (controlStatus)
               {
                   case "LOCAL":
                       line.File.HostMode = eHostMode.LOCAL;
                       break;
                   case "REMOTE":
                       line.File.HostMode = eHostMode.REMOTE;
                       break;
                   case "OFFLINE":
                       line.File.HostMode = eHostMode.OFFLINE;
                       break;
                   default:
                       Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, controlStatus));
                       break;
               }

               if (line.File.HostMode != line.File.PreHostMode)
               {
                   #region OPI Service
                   string timeId = string.Format("{0}_OPI_LineModeChangeRequest", line.Data.LINEID);
                   if (Timermanager.IsAliveTimer(timeId))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                       Timermanager.TerminateTimer(timeId);

                   Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                   #endregion

                   ObjectManager.LineManager.EnqueueSave(line.File);
                   ObjectManager.LineManager.RecordLineHistory(trxID, line); //记录Line History
               }

               if (controlStatus != "OFFLINE")
               {
                   if (line.File.PreHostMode == eHostMode.OFFLINE)
                   {
                       Thread.Sleep(2000);
                       //MachineDataUpdate(trxID, line);    Tom.Su Mark 20151223 MachineDataUpdate格式與原先不同，待添加
                       Thread.Sleep(1500);
                       //PortDataUpdate(trxID, line);   Tom.Su Mark 20151223 PortDataUpdate格式與原先不同，待添加
                   }
               }


           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       public void MachineControlStateChanged_FirstRun(string trxID, string lineName)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line == null) throw new Exception(string.Format("Can't find Line ID =[{0}] in LineEntity!", lineName));

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChanged") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               bodyNode[keyHost.CONTROLSTATENAME].InnerText = line.File.HostMode.ToString();
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

               SendToMES(xml_doc);
               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, line.File.HostMode.ToString()));



               if (line.File.HostMode != eHostMode.OFFLINE)
               {
                   Thread.Sleep(2000);
                   //MachineDataUpdate(trxID, line);    Tom.Su Mark 20151223 MachineDataUpdate格式與原先不同，待添加
                   Thread.Sleep(1500);
                   //PortDataUpdate(trxID, line);   Tom.Su Mark 20151223 PortDataUpdate格式與原先不同，待添加
               }
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MachineControlStateChangeRequest
       public void MES_MachineControlStateChangeRequest(XmlDocument xmlDoc)
       {
           try
           {
               string err = string.Empty;
               string lineName = GetLineName(xmlDoc);
               string trxID = GetTransactionID(xmlDoc);
               string inboxname = GetMESINBOXName(xmlDoc);
               string controlState = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CONTROLSTATENAME].InnerText;

               Line line = ObjectManager.LineManager.GetLine(lineName);

               if (line != null)
               {
                   #region LineService CheckControlCondition
                   string strResult = Invoke("LineService", "CheckControlCondition",
                           new object[] { trxID, line.Data.LINEID, controlState, err },
                           new Type[] { typeof(string), typeof(string), typeof(string), typeof(string).MakeByRefType() }).ToString();
                   //err 目前不會用到
                   #endregion

                   MachineControlStateChangeReply(trxID, line.Data.LINEID, controlState, strResult, inboxname);

                   if (strResult == "Y")
                   {
                       if (line.File.HostMode == eHostMode.OFFLINE) //OFFLINE -> ONLINE or LOCAL
                       {
                           MachineControlStateChanged(trxID, line.Data.LINEID, controlState);
                       }
                       else //LOCAL -> REMOTE or REMOTE -> LOCAL
                       {
                           MachineControlStateChanged(trxID, line.Data.LINEID, controlState);
                       }
                   }

                   Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={0}] [BCS <-MES ] [{1}] MES Change Control State =[{2}].",
                         ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, controlState));

                   #region OPI Service
                   Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                   #endregion
               }
               else
               {
                   MachineControlStateChangeReply(trxID, lineName, controlState, "N", inboxname); //BC cannot change to online
                   Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={0}] [BCS <-MES ] [{1}] MES Change Control State =[{2}], mismatch Line =[{3}].",
                         ServerName, trxID, controlState, lineName));
                   //ServerName, trxID, controlState, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
               }


           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MachineControlStateChangeReply
       public void MachineControlStateChangeReply(string trxID, string lineName, string ctlStatus, string ackResult, string inboxname)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChangeReply") as XmlDocument;
               SetTransactionID(xml_doc, trxID);
               SetINBOXNAME(xml_doc, inboxname);

               #region kill Timeout
               string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               #endregion

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
               bodyNode[keyHost.CONTROLSTATENAME].InnerText = ctlStatus;
               bodyNode[keyHost.ACKNOWLEDGE].InnerText = ackResult;

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
        //MaterialInuseRequest
       public void MaterialInuseRequest(string trxID, string lineName, Equipment eqp, List<MaterialEntity> materlist)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MaterialInuseRequest") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

               XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
               XmlNode prodCloneNode = materListNode[keyHost.MATERIAL];
               materListNode.RemoveAll();

               foreach (MaterialEntity mater in materlist)
               {
                   XmlNode materNode = prodCloneNode.Clone();
                   materNode[keyHost.MATERIALTYPE].InnerText = mater.MaterialType;
                   materNode[keyHost.MATERIALNAME].InnerText = mater.MaterialID;
                   materNode[keyHost.MATERIALSTATE].InnerText = mater.MaterialStatus.ToString();

                   materListNode.AppendChild(materNode);
               }

               SendToMES(xml_doc);

               #region MES Material Inuse Reply Timeout
               string timeoutName = string.Format("MES_MaterialInuseReply");
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
               new System.Timers.ElapsedEventHandler(MaterialInuseReply_Timeout), trxID);
               #endregion

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       private void MaterialInuseReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
       {
           try
           {
               UserTimer timer = subjet as UserTimer;
               string tmp = timer.TimerId;
               string trackKey = timer.State.ToString();
               string[] sArray = tmp.Split('_');

               string err = string.Empty;

               err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  MaterialInuseRequest MES Reply Timeout.", trackKey, sArray[0]);

               string timeoutName = string.Format("{0}_MES_MaterialInuseReply", sArray[0]);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }

               Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
           }
           catch (Exception ex)
           {
               this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MaterialInuseReply
       public void MES_MaterialInuseReply(XmlDocument xmlDoc)
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

               #region kill Timeout
               string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               #endregion

               string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
               string materialName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST][keyHost.MATERIAL][keyHost.MATERIALNAME].InnerText;
               //string returnCode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST][keyHost.MATERIAL][keyHost.RETURNCODE].InnerText;
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MaterialPrepareRequest
       public void MaterialPrepareRequest(string trxID, string lineName, Equipment eqp, List<MaterialEntity> materlist)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MaterialPrepareRequest") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

               XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
               XmlNode prodCloneNode = materListNode[keyHost.MATERIAL];
               materListNode.RemoveAll();

               foreach (MaterialEntity mater in materlist)
               {
                   XmlNode materNode = prodCloneNode.Clone();
                   materNode[keyHost.MATERIALTYPE].InnerText = mater.MaterialType;
                   materNode[keyHost.MATERIALNAME].InnerText = mater.MaterialID;
                   materNode[keyHost.MATERIALSTATE].InnerText = mater.MaterialStatus.ToString();

                   materListNode.AppendChild(materNode);
               }

               SendToMES(xml_doc);

               #region MES Material Prepare Reply Timeout
               string timeoutName = string.Format("MES_MaterialPrepareReply");
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
               new System.Timers.ElapsedEventHandler(MaterialPrepareReply_Timeout), trxID);
               #endregion

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
       private void MaterialPrepareReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
       {
           try
           {
               UserTimer timer = subjet as UserTimer;
               string tmp = timer.TimerId;
               string trackKey = timer.State.ToString();
               string[] sArray = tmp.Split('_');

               string err = string.Empty;

               err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  MaterialPrepareRequest MES Reply Timeout.", trackKey, sArray[0]);

               string timeoutName = string.Format("{0}_MES_MaterialPrepareReply", sArray[0]);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }

               Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
           }
           catch (Exception ex)
           {
               this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MaterialPrepareReply
       public void MES_MaterialPrepareReply(XmlDocument xmlDoc)
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

               #region kill Timeout
               string timeoutName = string.Format("{0}_MES_MaterialPrepareReply", trxID);
               if (_timerManager.IsAliveTimer(timeoutName))
               {
                   _timerManager.TerminateTimer(timeoutName);
               }
               #endregion

               MaterialEntity material = new MaterialEntity();
               XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;

               foreach (XmlNode node in materialList)
               {
                   material.MaterialID = node[keyHost.MATERIAL][keyHost.MATERIALNAME].InnerText;
                   string materialReturnCode = node[keyHost.MATERIAL][keyHost.RETURNCODE].InnerText;
               }

           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //MaterialReport
       public void MaterialReport(string trxID, string lineName, Equipment eqp, List<MaterialEntity> materlist)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("MaterialReport") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);
               bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

               XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
               XmlNode prodCloneNode = materListNode[keyHost.MATERIAL];
               materListNode.RemoveAll();

               foreach (MaterialEntity mater in materlist)
               {
                   XmlNode materNode = prodCloneNode.Clone();
                   materNode[keyHost.MATERIALTYPE].InnerText = mater.MaterialType;
                   materNode[keyHost.MATERIALNAME].InnerText = mater.MaterialID;
                   materNode[keyHost.MATERIALSTATE].InnerText = mater.MaterialStatus.ToString();
                   materNode[keyHost.USEDCOUNT].InnerText = mater.UseCount;

                   materListNode.AppendChild(materNode);
               }

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //PortAccessModeChanged
       public void PortAccessModeChanged(string trxID, string lineName,List<PortEntityFile> portList)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
               {
                   return;
               }

               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortAccessModeChanged") as XmlDocument;
               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode cloneXml = bodyNode[keyHost.PORT];
               portListNode.RemoveAll();

               foreach (PortEntityFile portIfo in portList)
               {
                   XmlNode portNode = cloneXml.Clone();
                   portNode[keyHost.PORTNAME].InnerText = portIfo.CassetteID;
                   portNode[keyHost.PORTACCESSMODE].InnerText = portIfo.TransferMode.ToString();

                   portListNode.AppendChild(portNode);
               }

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
               string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                   trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }

       public void PortAccessModeChanged(string trxID, string lineName, Port _port)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                   string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortAccessModeChanged") as XmlDocument;

               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
               portListNode.RemoveAll();

               XmlNode portNode = portCloneNode.Clone();
               portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
               portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
               portListNode.AppendChild(portNode);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged OK LINENAME=[{1}].",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //PortEnableChanged
       public void PortEnableChanged(string trxID, string lineName, IList<Port> portlist)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortEnableChanged") as XmlDocument;

               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
               portListNode.RemoveAll();

               foreach (Port _port in portlist)
               {
                   XmlNode portNode = portCloneNode.Clone();
                   portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                   portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][((int)_port.File.EnableMode).ToString()].Value;
                   portListNode.AppendChild(portNode);
               }

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged OK LINENAME=[{1}].",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }

       public void PortEnableChanged(string trxID, string lineName, Port _port)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortEnableChanged") as XmlDocument;

               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
               portListNode.RemoveAll();

               XmlNode portNode = portCloneNode.Clone();
               portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
               portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][((int)_port.File.EnableMode).ToString()].Value;
               portListNode.AppendChild(portNode);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged OK LINENAME=[{1}].",
                       trxID, lineName));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }
        //PortTypeChanged
       public void PortTypeChanged(string trxID, string lineName, IList<Port> port)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortTypeChanged") as XmlDocument;
               SetTransactionID(xml_doc, trxID);
               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
               portListNode.RemoveAll();

               foreach (Port _port in port)
               {
                   XmlNode portNode = portCloneNode.Clone();
                   portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                   portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][((int)_port.File.Type).ToString()].Value;
                   portListNode.AppendChild(portNode);
               }

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged OK LINENAME=[{1}].",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }

       public void PortTypeChanged(string trxID, string lineName, Port port)
       {
           try
           {
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (line.File.HostMode == eHostMode.OFFLINE)
               {
                   Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                   return;
               }
               IServerAgent agent = GetServerAgent();
               XmlDocument xml_doc = agent.GetTransactionFormat("PortTypeChanged") as XmlDocument;

               SetTransactionID(xml_doc, trxID);

               XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
               XmlNode portListNode = bodyNode[keyHost.PORTLIST];
               XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
               portListNode.RemoveAll();

               XmlNode portNode = portCloneNode.Clone();
               portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
               portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][((int)port.File.Type).ToString()].Value;
               portListNode.AppendChild(portNode);

               SendToMES(xml_doc);

               Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged OK LINENAME=[{1}].",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
           }
           catch (Exception ex)
           {
               NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
           }
       }

    }
}
