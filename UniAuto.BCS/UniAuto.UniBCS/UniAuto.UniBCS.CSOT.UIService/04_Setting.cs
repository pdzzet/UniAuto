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
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;
using System.IO;
using System.Collections;

namespace UniAuto.UniBCS.CSOT.UIService
{
    public partial class UIService
    {
        #region Status
        /// <summary>
        /// OPI MessageSet: Sampling Rule Data Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_SamplingRuleDataRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            SamplingRuleDataRequest command = Spec.XMLtoMessage(xmlDoc) as SamplingRuleDataRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("SamplingRuleDataReply") as XmlDocument;
            SamplingRuleDataReply reply = Spec.XMLtoMessage(xml_doc) as SamplingRuleDataReply;

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                if (eqp == null)
                {
                    reply.RETURN.RETURNCODE = "0010610";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                }
                else
                {
                    reply.BODY.SAMPLINGRULE = ((int)eqp.File.SamplingRule).ToString();
                    reply.BODY.SAMPLINGUNIT = eqp.File.SamplingCount.ToString();

                    string sideInfo = "0000000000000000";

                    #region SideInfomation拆解
                    string[] info = eqp.File.SamplingUnit.Split(',');

                    for (int i = 0; i < info.Length; i++)
                    {
                        switch (info[i])
                        {
                            case "CoaterVCD01":
                                sideInfo = sideInfo.Remove(0, 1).Insert(0, "1");
                                break;
                            case "CoaterVCD02":
                                sideInfo = sideInfo.Remove(1, 1).Insert(1, "1");
                                break;
                            case "CoaterVCD03":
                                sideInfo = sideInfo.Remove(2, 1).Insert(2, "1");
                                break;
                            case "AlignerCP01":
                                sideInfo = sideInfo.Remove(3, 1).Insert(3, "1");
                                break;
                            case "AlignerCP02":
                                sideInfo = sideInfo.Remove(4, 1).Insert(4, "1");
                                break;
                            case "OvenHP01":
                                sideInfo = sideInfo.Remove(5, 1).Insert(5, "1");
                                break;
                            case "OvenHP02":
                                sideInfo = sideInfo.Remove(6, 1).Insert(6, "1");
                                break;
                        }
                    }
                    #endregion

                    reply.BODY.SIDEINFORMATION = sideInfo;
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary>
        /// OPI MessageSet: BCS Parameter Data Info Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_BCSParameterDataInfoRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            BCSParameterDataInfoRequest command = Spec.XMLtoMessage(xmlDoc) as BCSParameterDataInfoRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("BCSParameterDataInfoReply") as XmlDocument;
            BCSParameterDataInfoReply reply = Spec.XMLtoMessage(xml_doc) as BCSParameterDataInfoReply;

            try
            {                

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                if (line == null)
                {
                    reply.RETURN.RETURNCODE = "0010060";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                }
                else
                {
                    foreach (BCSParameterDataInfoRequest.PARAMETERc _param in command.BODY.PARAMETERLIST)
                    {
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.ARRAY.ELA_JSW:
                                {
                                    if (_param.NAME.Equals("ProcessRecipe") && (_param.EQUIPMENTNO.Equals("L4") || _param.EQUIPMENTNO.Equals("L5")))
                                    {
                                        Trx Trx = null;
                                        Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _param.EQUIPMENTNO + "_ProcessTimeQueryBlock", false }) as Trx;
                                        if (Trx == null)
                                        {
                                            Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _param.EQUIPMENTNO + "_ProcessTimeQueryBlock Trx Not Define !");
                                        }
                                        else
                                        {
                                            BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                            par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                            par.NAME = _param.NAME;
                                            par.VALUE = Trx.EventGroups[0].Events[0].Items[0].Value;
                                            reply.BODY.PARAMETERLIST.Add(par);
                                        }
                                    }
                                    else if (_param.NAME.Equals("CompensationTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                        par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                        par.NAME = _param.NAME;
                                        par.VALUE = line.File.FetchCompensationTime.ToString();
                                        reply.BODY.PARAMETERLIST.Add(par);
                                    }
                                    else if (_param.NAME.Equals("ProcessTime_RecipeID") && (_param.EQUIPMENTNO.Equals("L4") || _param.EQUIPMENTNO.Equals("L5")))
                                    {
                                        Trx Trx = null;
                                        Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _param.EQUIPMENTNO + "_ProcessTimeBlock", false }) as Trx;
                                        if (Trx == null)
                                        {
                                            Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _param.EQUIPMENTNO + "_ProcessTimeBlock Trx Not Define !");
                                        }
                                        else
                                        {
                                            BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                            par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                            par.NAME = _param.NAME;
                                            par.VALUE = Trx.EventGroups[0].Events[0].Items[1].Value;
                                            reply.BODY.PARAMETERLIST.Add(par);
                                        }
                                    }
                                    else if (_param.NAME.Equals("ProcessTime_ReplyTime") && (_param.EQUIPMENTNO.Equals("L4") || _param.EQUIPMENTNO.Equals("L5")))
                                    {
                                        Trx Trx = null;
                                        Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _param.EQUIPMENTNO + "_ProcessTimeBlock", false }) as Trx;
                                        if (Trx == null)
                                        {
                                            Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _param.EQUIPMENTNO + "_ProcessTimeBlock Trx Not Define !");
                                        }
                                        else
                                        {
                                            BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                            par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                            par.NAME = _param.NAME;
                                            par.VALUE = Trx.EventGroups[0].Events[0].Items[0].Value;
                                            reply.BODY.PARAMETERLIST.Add(par);
                                        }
                                    }
                                    else if (_param.NAME.Equals("WarringTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                        par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                        par.NAME = _param.NAME;
                                        par.VALUE = line.File.SendOverTimeWarring.ToString();
                                        reply.BODY.PARAMETERLIST.Add(par);
                                    }
                                    else if (_param.NAME.Equals("SendToCassetteTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        BCSParameterDataInfoReply.PARAMETERc par = new BCSParameterDataInfoReply.PARAMETERc();
                                        par.EQUIPMENTNO = _param.EQUIPMENTNO;
                                        par.NAME = _param.NAME;
                                        par.VALUE = line.File.SendOverTimeAlarm.ToString();
                                        reply.BODY.PARAMETERLIST.Add(par);
                                    }
                                    else
                                    {
                                        reply.RETURN.RETURNCODE = "0010061";
                                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Parameter EQUIPMENTNO[{0}], Name[{1}] ", _param.EQUIPMENTNO, _param.NAME);

                                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", reply.RETURN.RETURNMESSAGE);
                                    }
                                }
                                break;
                            default:
                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", command.BODY.LINENAME + " Not Define in this Function");
                                break;
                        }
                    }
                }

                //移除多餘的項目
                if (reply.BODY.PARAMETERLIST.Count > 0)
                    reply.BODY.PARAMETERLIST.RemoveAt(0);

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
             }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }
        #endregion

        #region Command
        /// <summary>
        ///  OPI MessageSet: BCS Parameter Data Change Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_BCSParameterDataChangeRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            BCSParameterDataChangeRequest command = Spec.XMLtoMessage(xmlDoc) as BCSParameterDataChangeRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("BCSParameterDataChangeReply") as XmlDocument;
            BCSParameterDataChangeReply reply = Spec.XMLtoMessage(xml_doc) as BCSParameterDataChangeReply;

            try
            {
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                if (line == null)
                {
                    reply.RETURN.RETURNCODE = "0010060";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                }
                else
                {
                    foreach (BCSParameterDataChangeRequest.PARAMETERc _param in command.BODY.PARAMETERLIST)
                    {
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.ARRAY.ELA_JSW:
                                {
                                    if (_param.NAME.Equals("ProcessRecipe") && (_param.EQUIPMENTNO.Equals("L4") || _param.EQUIPMENTNO.Equals("L5")))
                                    {
                                        List<Port> lstPort = ObjectManager.PortManager.GetPorts();
                                        foreach(Port pt in lstPort)
                                        {
                                            if (pt.File.Status == ePortStatus.LC)
                                            {
                                                reply.RETURN.RETURNCODE = "0010062";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] Status = LC, Can't Change ProcessTimeQueryBlock Data", pt.Data.PORTNO);
                                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", reply.RETURN.RETURNMESSAGE);
                                                SendReplyToOPI(command, reply);
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, 
                                                       string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, reply.RETURN.RETURNMESSAGE) });
                                                return;
                                            }
                                        }
                                        Trx Trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(_param.EQUIPMENTNO + "_ProcessTimeQueryBlock") as Trx;
                                        if (Trx == null)
                                        {
                                            Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _param.EQUIPMENTNO + "_ProcessTimeQueryBlock Trx Not Define !");
                                        }
                                        else
                                        {
                                            Trx.EventGroups[0].Events[0].Items[0].Value = _param.VALUE;
                                            Trx.TrackKey = command.HEADER.TRANSACTIONID;
                                            SendPLCData(Trx);
                                        }
                                    }
                                    else if (_param.NAME.Equals("CompensationTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        line.File.FetchCompensationTime = int.Parse(_param.VALUE);
                                        ObjectManager.LineManager.EnqueueSave(line.File);
                                        ObjectManager.LineManager.RecordLineHistory(command.HEADER.TRANSACTIONID, line);
                                    }
                                    else if (_param.NAME.Equals("WarringTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        line.File.SendOverTimeWarring = int.Parse(_param.VALUE);
                                        ObjectManager.LineManager.EnqueueSave(line.File);
                                        ObjectManager.LineManager.RecordLineHistory(command.HEADER.TRANSACTIONID, line);
                                    }
                                    else if (_param.NAME.Equals("SendToCassetteTime") && _param.EQUIPMENTNO.Equals("00"))
                                    {
                                        line.File.SendOverTimeAlarm = int.Parse(_param.VALUE);
                                        ObjectManager.LineManager.EnqueueSave(line.File);
                                        ObjectManager.LineManager.RecordLineHistory(command.HEADER.TRANSACTIONID, line);
                                    }
                                    else
                                    {
                                        reply.RETURN.RETURNCODE = "0010061";
                                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Parameter EQUIPMENTNO[{0}], Name[{1}] ", _param.EQUIPMENTNO, _param.NAME);

                                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", reply.RETURN.RETURNMESSAGE);
                                    }
                                }
                                break;
                            default:
                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", command.BODY.LINENAME + " Not Define in this Function");
                                break;
                        }
                    }
                }


                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary>
        /// OPI MessageSet: Database Reload Request to BC
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_DatabaseReloadRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            DatabaseReloadRequest command = Spec.XMLtoMessage(xmlDoc) as DatabaseReloadRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("DatabaseReloadReply") as XmlDocument;
            DatabaseReloadReply reply = Spec.XMLtoMessage(xml_doc) as DatabaseReloadReply;

            try
            {
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, TABLENAME={4}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.OPERATORID, command.BODY.TABLENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.TABLENAME = command.BODY.TABLENAME;

                //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                IList<Line> lines = ObjectManager.LineManager.GetLines();
                if (lines == null)
                {
                    reply.RETURN.RETURNCODE = "0010060";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                }
                else
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Reload UniBCS DB Table({2}) by user({3}).",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.TABLENAME, command.BODY.OPERATORID));

                    switch (command.BODY.TABLENAME)
                    {
                        #region DB Table Reload

                        case "SBRM_QTIME_DEF":
                            //上報MES，MESService更新資訊
                            //foreach (DatabaseReloadRequest.MODIFYc modify in command.BODY.MODIFYLIST)
                            //{

                            //    string trxID = command.HEADER.TRANSACTIONID;
                            //    string userID = command.BODY.OPERATORID;
                            //    string lineName = command.BODY.LINENAME;
                            //    IList<QtimeEntityData> qTimeDatas = ObjectManager.QtimeManager.Find(new string[] { "QTIMEID" }, new string[] { modify.MODIFYKEY });

                            //    string qTime = qTimeDatas[0].SETTIMEVALUE.ToString();
                            //    string fromEQ = qTimeDatas[0].STARTNODEID;
                            //    //如果Unit有值，則tracelvl = U; 如果unit沒有值，則tracelvl = M
                            //    string fromTracelvl = string.IsNullOrEmpty(qTimeDatas[0].STARTNUNITID) ? "M" : "U";
                            //    string toEQ = qTimeDatas[0].ENDNODEID;
                            //    string toTracelvl = string.IsNullOrEmpty(qTimeDatas[0].ENDNUNITID) ? "M" : "U";

                            //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            //    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Invoke MESService QtimeSetChanged. UserID({2}), QTime({3}), FromEQ({4}), FromTracelvl({5}), ToEQ({6}), ToTracelvl({7}))",
                            //    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, userID, qTime, fromEQ, fromTracelvl, toEQ, toTracelvl));

                            //    Invoke("MESService", "QtimeSetChanged", new object[] { trxID, lineName, userID, qTime, fromEQ, fromTracelvl, toEQ, toTracelvl });
                            //}
                            ObjectManager.QtimeManager.ReloadQTime();

                            break;
                        case "SBRM_SUBBLOCK_CTRL":
                            ObjectManager.SubBlockManager.ReloadByUI();
                            break;
                        case "SBRM_SKIPREPORT":
                            ObjectManager.LineManager.ReloadSkipReport();
                            break;
                        case "SBRM_RECIPE": //BC 没有用SBRM_RECIPE
                            //ObjectManager.RecipeManager.ReloadAll();
                            break;
                        case "SBRM_ALARM":
                            //ObjectManager.AlarmManager
                            ObjectManager.AlarmManager.ReloadAll(); //alarm也需要Reload 20150321 Tom
                            break;
                        case "SBRM_LINESTATUSSPEC":
                            ObjectManager.LineManager.ReloadLineStatusSpec();
                            break;
                        case "SBRM_APCDATADOWNLOAD":
                            ObjectManager.APCDataDownloadManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_APCDATAREPORT":
                            ObjectManager.APCDataReportManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            ObjectManager.PositionManager.ReloadAll();// sy add 20161003 POSITION & APCDATAREPORT 關聯 需POSITION Reload 才會重新計算
                            break;
                        case "SBRM_DAILYCHECKDATA":
                            ObjectManager.DailyCheckManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_ENERGYVISUALIZATIONDATA":
                            ObjectManager.EnergyVisualizationManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_PROCESSDATA":
                            ObjectManager.ProcessDataManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_POSITION":// sy add 20161003
                            ObjectManager.PositionManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_CIMMESSAGE":
                            //不需要，已刪除
                            break;                       
                        case "SBRM_EQPSTAGE":
                            ObjectManager.RobotPositionManager.Reload();
                            break;
                        case "SBRM_RECIPEPARAMETER":
                            ObjectManager.RecipeManager.ReloadAll();
                            ObjectManager.EquipmentManager.ReloadProfileVersion();
                            break;
                        case "SBRM_NODE:RECIPEREGVALIDATIONENABLED":
                            ObjectManager.EquipmentManager.ReloadRecipeCheckFlagAll();
                            break;

                        case "SBRM_NODE:RECIPEPARAVALIDATIONENABLED":
                            ObjectManager.EquipmentManager.ReloadRecipeParamCheckFlagAll();
                            break;
                        case "SBRM_LINE:CHECKCROSSRECIPE":
                            ObjectManager.LineManager.ReloadCheckCrossRecipe();
                            break;

                            //20171214 huangjiayin add for Sub Job Data
                            //子项修改不需要重启BC程式
                        case "SBRM_SUBJOBDATA":
                            ObjectManager.SubJobDataManager.ReloadBy_UI();
                            break;

                        #region [ 20150812 Modify For Robot Use ][ Wait_Proc_0019 ] 相關處理

                        case eRobotCommonConst.ROBOT_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_STAGE_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotStagRequest", new object[] { });
                            break;
                        
                        case eRobotCommonConst.ROBOT_ROUTE_CONDITION_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRouteConditionRequest", new object[] { });                           
                            break;

                        case eRobotCommonConst.ROBOT_ROUTE_STEP_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRouteStepRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_ROUTE_MST_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRouteMasterRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_RULE_JOB_SELECT_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleSelectRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_RULE_FILTER_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleFilterRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_RULE_STAGE_SELECT_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleStageSelectRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_PROC_RESULT_HANDLE_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRouteResultHandleRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_ROUTE_STEP_BYPASS_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleRouteStepByPassRequest", new object[] { });
                            break;

                        case eRobotCommonConst.ROBOT_ROUTE_STEP_JUMP_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleRouteStepJumpRequest", new object[] { });
                            break;

                        //20151201 add for Reload Robot Rule OrderBy
                        case eRobotCommonConst.ROBOT_RULE_ORDERBY_TABLE_NAME:

                            Invoke(eServiceName.RobotStatusService, "ReloadRobotRuleOrderByRequest", new object[] { });
                            break;

                        #endregion


                        #endregion
                    }
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
            }
        }

        /// <summary>
        /// OPI MessageSet: Sampling Rule Change Command to BC
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_SamplingRuleChangeCommand(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            SamplingRuleChangeCommand command = Spec.XMLtoMessage(xmlDoc) as SamplingRuleChangeCommand;
            XmlDocument xml_doc = agent.GetTransactionFormat("SamplingRuleChangeCommandReply") as XmlDocument;
            SamplingRuleChangeCommandReply reply = Spec.XMLtoMessage(xml_doc) as SamplingRuleChangeCommandReply;

            try
            {
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, SAMPINGRULE={4}, SAMPINGUNIT={5}, SIDEINFORMATION={6}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                    command.BODY.EQUIPMENTNO, command.BODY.SAMPLINGRULE, command.BODY.SAMPLINGUNIT, command.BODY.SIDEINFORMATION));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                if (eqp == null)
                {
                    reply.RETURN.RETURNCODE = "0010240";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                }
                else
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService SamplingRuleChangeCommand.",
                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                    //轉拋給EquipmentService處理
                    Invoke("EquipmentService", "SamplingRuleChangeCommand", new object[] { command.BODY.EQUIPMENTNO, command.BODY.SAMPLINGRULE, command.BODY.SAMPLINGUNIT, command.BODY.SIDEINFORMATION, command.HEADER.TRANSACTIONID });
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary>
        /// OPI MessageSet: Create InspFile Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_CreateInspFileRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            CreateInspFileRequest command = Spec.XMLtoMessage(xmlDoc) as CreateInspFileRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("CreateInspFileReply") as XmlDocument;
            CreateInspFileReply reply = Spec.XMLtoMessage(xml_doc) as CreateInspFileReply;

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. CASSETTESEQNO={3}, JOBSEQNO={4}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

                //ObjectManager.JobManager.TEST_CREATE_JOBDataFile();

                //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                IList<Line> lines = ObjectManager.LineManager.GetLines();
                Job job = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);
                if (lines == null)
                {
                    reply.RETURN.RETURNCODE = "0010620";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                }
                else if (job == null)
                {
                    reply.RETURN.RETURNCODE = "0010621";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Job in JobEntity, CassetteSeqNo[{0}], JobSeqNo[{1}]",
                        command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Job in JobEntity, CassetteSeqNo({2}) / JobSeqNo({3})",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO));
                }
                else
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[JOB={0}] [BCS <- OPI][{1}] Create Job({0}) inspection equipment file.)",
                        job.GlassChipMaskBlockID, command.HEADER.TRANSACTIONID));

                    xMessage msg = SendReplyToOPI(command, reply);

                    Line line = ObjectManager.LineManager.GetLine(job.MesCstBody.LINENAME);
                    //建立InspFile
                    if (line == null)
                        line = ObjectManager.LineManager.GetLine(this.ServerName);

                    string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, command.BODY.CASSETTESEQNO);

                    if (line.Data.FABTYPE != eFabType.CELL.ToString())
                        FileFormatManager.CreateFormatFile("ACShop", subPath, job, true);
                    else
                    {
                        //FileFormatManager.CreateFormatFile("CELLShop", subPath, job, true);
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.CELL.CBPIL:
                            case eLineType.CELL.CBODF:
                            case eLineType.CELL.CBHVA:
                            case eLineType.CELL.CBPMT:
                            case eLineType.CELL.CBGAP:
                            case eLineType.CELL.CBUVA:
                            case eLineType.CELL.CBMCL:
                            case eLineType.CELL.CBATS:
                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, job, true);
                                break;

                            case eLineType.CELL.CBLOI:// LOI 线要产生两种类型的File Data  20150313 Tom
                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                                break;
                            case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                            case eLineType.CELL.CBCUT_2:
                            case eLineType.CELL.CBCUT_3:

                                if (job.SourcePortID == "01" || job.SourcePortID == "02" ||
                                    job.SourcePortID == "C01" || job.SourcePortID == "C02")
                                {
                                    if (job.ChipCount > 1)
                                    {
                                        //Jun Modify 20150521 Create_CELL_ChipPanel傳入參數有改
                                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                                        Invoke(eServiceName.CELLSpecialService, "Create_CELL_ChipPanel", new object[] { eqp, job, job.ChipCount, false });
                                    }
                                    else
                                    {
                                        FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                                    }
                                }
                                else if (job.SourcePortID == "07" || job.SourcePortID == "08" ||
                                         job.SourcePortID == "C07" || job.SourcePortID == "C08" ||
                                         job.SourcePortID == "P01" || job.SourcePortID == "P02" ||
                                         job.SourcePortID == "P03" || job.SourcePortID == "P04" || job.SourcePortID == "P06")
                                {
                                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                                }

                                break;
                            default://T3 使用default 集中管理 sy
                                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(line.Data.LINEID, job.SourcePortID);
                                    Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, job, port });
                                //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
                                break;
                        }
                    }
                }

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
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
