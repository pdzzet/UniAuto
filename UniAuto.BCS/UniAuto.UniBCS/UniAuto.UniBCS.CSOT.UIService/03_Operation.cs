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
using System.IO;
using System.Collections;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.UIService
{
      public partial class UIService
      {
            #region LINENAMEKeyName
            private const string Key_MPLCInterlockChangeRequest = "{0}_MPLCInterlockChangeRequest";
            private const string Key_IndexerOperationModeChange = "{0}_IndexerOperationModeChange";
            private const string Key_CoolRunSet = "{0}_CoolRunSet";
            private const string Key_VCRStatusChange = "{0}_VCRStatusChange";
            private const string Key_CassetteOnPortQTime = "{0}_CassetteOnPortQTime";
            #endregion

            #region Status
            /// <summary>
            /// OPI MessageSet: Incomplete Cassette Data Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_IncompleteCassetteDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IncompleteCassetteDataRequest command = Spec.XMLtoMessage(xmlDoc) as IncompleteCassetteDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("IncompleteCassetteDataReply") as XmlDocument;
                  IncompleteCassetteDataReply reply = Spec.XMLtoMessage(xml_doc) as IncompleteCassetteDataReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;
                        reply.BODY.MESTRXID = command.BODY.MESTRXID;
                        reply.BODY.FILENAME = command.BODY.FILENAME;

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Set IncompleteCassetteDataRequest", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        //取得 Incomplete Cassette Data
                        string strDesc; //回傳的 Message { (OK or NG) + Message}
                        XmlDocument xmlDocument; //回傳的 XmlDocument
                        ObjectManager.CassetteManager.IncompleteCassetteDataReply(command.BODY.PORTID.Trim(), command.BODY.CASSETTEID.Trim(), command.BODY.MESTRXID.Trim(), command.BODY.FILENAME.Trim(), command.BODY.INCOMPLETEDATE.Trim(), out strDesc, out xmlDocument);

                        //判斷回傳結果
                        if (xmlDocument == null)
                        {
                              if (strDesc == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010250";
                                    reply.RETURN.RETURNMESSAGE = "Get No Data & No Message";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Get No Data & No Message",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    string[] strDescSplit = strDesc.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strDescSplit != null && strDescSplit.Length >= 2)
                                    {
                                          switch (strDescSplit[1].ToString().Trim())
                                          {
                                                case "IncompleteCSTPath_Error":
                                                      reply.RETURN.RETURNCODE = "0010251";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Parameters_Error":
                                                      reply.RETURN.RETURNCODE = "0010252";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "File_Error":
                                                      reply.RETURN.RETURNCODE = "0010253";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Exception_Error":
                                                      reply.RETURN.RETURNCODE = "0010254";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;
                                          }
                                    }
                              }
                        }
                        else
                        {
                              //增加 body 層級資料
                              XmlNode xnBody = xmlDocument[keyHost.MESSAGE];
                              reply.BODY.CARRIERNAME = xnBody[keyHost.CARRIERNAME].InnerText;
                              reply.BODY.LINERECIPENAME = xnBody[keyHost.LINERECIPENAME].InnerText;
                              reply.BODY.HOSTLINERECIPENAME = xnBody[keyHost.HOSTLINERECIPENAME].InnerText;
                              reply.BODY.PPID = xnBody[keyHost.PPID].InnerText;
                              reply.BODY.HOSTPPID = xnBody[keyHost.HOSTPPID].InnerText;
                              reply.BODY.RETURNMSG = xnBody[keyHost.RETURNMSG].InnerText;

                              //增加 Product 層級資料
                              IncompleteCassetteDataReply.PRODUCTc product;
                              XmlNodeList xnlProductList = xnBody.SelectNodes(string.Format("{0}/{1}", keyHost.INCOMPLETECASSETTEDATALIST, keyHost.PRODUCT));

                              //移除結構內初始化的 Product 資料
                              if (xnlProductList != null && xnlProductList.Count > 0 && reply.BODY.INCOMPLETECASSETTEDATALIST.Count >= 0)
                                    reply.BODY.INCOMPLETECASSETTEDATALIST.RemoveAt(0);

                              foreach (XmlNode xnProduct in xnlProductList)
                              {
                                    product = new IncompleteCassetteDataReply.PRODUCTc();
                                    product.POSITION = xnProduct.SelectSingleNode(keyHost.POSITION).InnerText;
                                    product.PRODUCTNAME = xnProduct.SelectSingleNode(keyHost.PRODUCTNAME).InnerText;
                                    product.HOSTPRODUCTNAME = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTNAME).InnerText;
                                    product.DENSEBOXID = xnProduct.SelectSingleNode(keyHost.DENSEBOXID).InnerText;
                                    product.PRODUCTJUDGE = xnProduct.SelectSingleNode(keyHost.PRODUCTJUDGE).InnerText;

                                    product.PRODUCTGRADE = xnProduct.SelectSingleNode(keyHost.PRODUCTGRADE).InnerText;
                                    //product.SUBPRODUCTGRADES = xnProduct.SelectSingleNode(keyHost.SUBPRODUCTGRADES).InnerText;
                                    product.PAIRPRODUCTNAME = xnProduct.SelectSingleNode(keyHost.PAIRPRODUCTNAME).InnerText;
                                    product.LOTNAME = xnProduct.SelectSingleNode(keyHost.LOTNAME).InnerText;
                                    product.PRODUCTRECIPENAME = xnProduct.SelectSingleNode(keyHost.PRODUCTRECIPENAME).InnerText;

                                    product.HOSTPRODUCTRECIPENAME = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTRECIPENAME).InnerText;
                                    product.PRODUCTSPECNAME = xnProduct.SelectSingleNode(keyHost.PRODUCTSPECNAME).InnerText;
                                    product.PROCESSOPERATIONNAME = xnProduct.SelectSingleNode(keyHost.PROCESSOPERATIONNAME).InnerText;
                                    product.PRODUCTOWNER = xnProduct.SelectSingleNode(keyHost.PRODUCTOWNER).InnerText;
                                    product.VCRREADFLAG = xnProduct.SelectSingleNode(keyHost.VCRREADFLAG).InnerText;

                                    product.SHORTCUTFLAG = xnProduct.SelectSingleNode(keyHost.SHORTCUTFLAG).InnerText;
                                    product.SAMPLEFLAG = xnProduct.SelectSingleNode(keyHost.SAMPLEFLAG).InnerText;
                                    product.PROCESSFLAG = xnProduct.SelectSingleNode(keyHost.PROCESSFLAG).InnerText;
                                    product.PROCESSCOMMUNICATIONSTATE = xnProduct.SelectSingleNode(keyHost.PROCESSCOMMUNICATIONSTATE).InnerText;

                                    reply.BODY.INCOMPLETECASSETTEDATALIST.Add(product);
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            public void OPI_IncompleteBoxDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IncompleteBoxDataRequest command = Spec.XMLtoMessage(xmlDoc) as IncompleteBoxDataRequest;
                  IncompleteBoxDataReply reply = new IncompleteBoxDataReply();

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.MESTRXID = command.BODY.MESTRXID;
                        reply.BODY.FILENAME = command.BODY.FILENAME;

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Set OPI_IncompleteBoxDataRequest", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        //取得 Incomplete Cassette Data
                        string strDesc; //回傳的 Message { (OK or NG) + Message}
                        XmlDocument xmlDocument; //回傳的 XmlDocument
                        ObjectManager.CassetteManager.IncompleteBoxDataReply(command.BODY.PORTID.Trim(), command.BODY.MESTRXID.Trim(), command.BODY.FILENAME.Trim(), command.BODY.INCOMPLETEDATE.Trim(), out strDesc, out xmlDocument);

                        //判斷回傳結果
                        if (xmlDocument == null)
                        {
                              if (strDesc == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010250";
                                    reply.RETURN.RETURNMESSAGE = "Get No Data & No Message";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Get No Data & No Message",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    string[] strDescSplit = strDesc.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strDescSplit != null && strDescSplit.Length >= 2)
                                    {
                                          switch (strDescSplit[1].ToString().Trim())
                                          {
                                                case "IncompleteCSTPath_Error":
                                                      reply.RETURN.RETURNCODE = "0010251";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Parameters_Error":
                                                      reply.RETURN.RETURNCODE = "0010252";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "File_Error":
                                                      reply.RETURN.RETURNCODE = "0010253";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Exception_Error":
                                                      reply.RETURN.RETURNCODE = "0010254";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;
                                          }
                                    }
                              }
                        }
                        else
                        {
                              MesSpec.BoxProcessEnd mes = (MesSpec.BoxProcessEnd)MesSpec.Spec.XMLtoMessage(xmlDocument);

                              //增加 body 層級資料
                              reply.BODY.BOXQUANTITY = mes.BODY.BOXQUANTITY;
                              reply.BODY.FILENAME = command.BODY.FILENAME;
                              reply.BODY.HOSTLINERECIPENAME = mes.BODY.HOSTLINERECIPENAME;
                              reply.BODY.HOSTPPID = mes.BODY.HOSTPPID;
                              reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                              reply.BODY.LINENAME = command.BODY.LINENAME;
                              reply.BODY.LINERECIPENAME = mes.BODY.LINERECIPENAME;
                              reply.BODY.MESTRXID = command.BODY.MESTRXID;
                              reply.BODY.PORTID = command.BODY.PORTID;
                              reply.BODY.PPID = mes.BODY.PPID;
                              reply.BODY.RETURNMSG = string.Format("[{0}]{1}", mes.RETURN.RETURNCODE, mes.RETURN.RETURNMESSAGE);
                              reply.BODY.SAMPLEFLAG = mes.BODY.SAMPLEFLAG;

                              foreach (MesSpec.BoxProcessEnd.BOXc mes_box in mes.BODY.BOXLIST)
                              {
                                    IncompleteBoxDataReply.BOXc opi_box = new IncompleteBoxDataReply.BOXc();
                                    opi_box.BOXNAME = mes_box.BOXNAME;
                                    opi_box.PRODUCTQUANTITY = mes_box.PRODUCTQUANTITY;
                                    foreach (MesSpec.BoxProcessEnd.PRODUCTc mes_prod in mes_box.PRODUCTLIST)
                                    {
                                          IncompleteBoxDataReply.PRODUCTc opi_prod = new IncompleteBoxDataReply.PRODUCTc();
                                          foreach (MesSpec.BoxProcessEnd.CODEc mes_code in mes_prod.ABNORMALCODELIST)
                                          {
                                                IncompleteBoxDataReply.ABNORMALCODEc opi_code = new IncompleteBoxDataReply.ABNORMALCODEc();
                                                opi_code.ABNORMALCODE = mes_code.ABNORMALCODE;
                                                opi_code.ABNORMALSEQ = mes_code.ABNORMALSEQ;
                                                opi_prod.ABNORMALCODELIST.Add(opi_code);
                                          }
                                          opi_prod.BOXULDFLAG = mes_prod.BOXULDFLAG;
                                          opi_prod.DPIPROCESSFLAG = mes_prod.DPIPROCESSFLAG;
                                          opi_prod.HOSTPPID = mes_prod.HOSTPPID;
                                          opi_prod.HOSTPRODUCTNAME = mes_prod.HOSTPRODUCTNAME;
                                          opi_prod.POSITION = mes_prod.POSITION;
                                          opi_prod.PPID = mes_prod.PPID;
                                          opi_prod.PRODUCTNAME = mes_prod.PRODUCTNAME;
                                          opi_prod.RTPFLAG = mes_prod.RTPFLAG;
                                          opi_prod.SHORTCUTFLAG = mes_prod.SHORTCUTFLAG;
                                          opi_box.PRODUCTLIST.Add(opi_prod);
                                    }
                                    reply.BODY.BOXLIST.Add(opi_box);
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
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
            /// Watson Add 20150413 For DPI OFFLINE
            /// OPI MessageSet: Cassette Data Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalCassetteDataRequest_DPI(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalCassetteDataRequest_DPI command = Spec.XMLtoMessage(xmlDoc) as LocalCassetteDataRequest_DPI;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalCassetteDataReply_DPI") as XmlDocument;
                  LocalCassetteDataReply_DPI reply = Spec.XMLtoMessage(xml_doc) as LocalCassetteDataReply_DPI;

                  try
                  {

                        string log = string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO);
                        if (command.BODY.PORTLIST.Count > 0)
                        {
                              foreach (LocalCassetteDataRequest_DPI.PORTc pp in command.BODY.PORTLIST)
                                    log += string.Format(", PORTNO=[{0}], BOXID=[{1}]", pp.PORTID, pp.CASSETTEID);
                        }
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", log);

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        reply.BODY.PORTLIST = new List<LocalCassetteDataReply_DPI.PORTc>();

                        foreach (LocalCassetteDataRequest_DPI.PORTc pp in command.BODY.PORTLIST)
                        {
                              LocalCassetteDataReply_DPI.PORTc replyPort = new LocalCassetteDataReply_DPI.PORTc();

                              replyPort.PORTNO = pp.PORTNO;
                              replyPort.PORTID = pp.PORTID;
                              replyPort.CASSETTEID = pp.CASSETTEID;

                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                              IList<Line> lines = ObjectManager.LineManager.GetLines();
                              Port port = ObjectManager.PortManager.GetPort(pp.PORTID);
                              Cassette cst = ObjectManager.CassetteManager.GetCassette(pp.CASSETTEID);

                              replyPort.CSTSETTINGCODE = cst.MES_CstData.CARRIERSETCODE;

                              //從JobEachCassetteSlotPositionBlock中取出目前有的玻璃
                              string SlotPosTrxName = string.Format("{0}_DP#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                              Trx positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { SlotPosTrxName }) as Trx;
                              if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { SlotPosTrxName, false }) as Trx;

                              if (lines == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010170";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line{{0}] in LineEntity.", command.BODY.LINENAME);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else if (eqp == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010171";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment{{0}] in EquipmentEntity.", command.BODY.EQUIPMENTNO);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                              }
                              else if (port == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010172";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port{{0}] in PortEntity.", pp.PORTID);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                        pp.PORTID, command.HEADER.TRANSACTIONID));
                              }
                              else if (cst == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010173";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find BOX[{0}] in CassetteEntity!", pp.CASSETTEID);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find BOX({0}) in CassetteEntity.",
                                        pp.CASSETTEID, command.HEADER.TRANSACTIONID));
                              }
                              else if (positionTrx == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010175";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] SlotPosition Information invalid", pp.PORTID);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) SlotPosition Information invalid.",
                                        pp.PORTID, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                                    //記錄Port上的玻璃資訊
                                    Dictionary<string, Job> jobs = new Dictionary<string, Job>();

                                    if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                                    {
                                          //回給OPI資料前先更新OPI Port狀態，機台不會觸發事件改變Port的資訊(Port Count,Job Existance)
                                          string portTrxName = string.Format("{0}_Port#{1}PortStatusChangeReport", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                                          Trx portTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { portTrxName }) as Trx;
                                          if (portTrx != null) portTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { portTrxName, false }) as Trx;
                                          lock (port.File)
                                          {
                                                port.File.JobCountInCassette = portTrx.EventGroups[0].Events[0].Items[4].Value;
                                                port.File.JobExistenceSlot = portTrx.EventGroups[0].Events[0].Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                                          }
                                          ObjectManager.PortManager.EnqueueSave(port.File);
                                          PortCSTStatusReport(command.HEADER.TRANSACTIONID, port);

                                          //取得slot資訊
                                          int index = 1;
                                          for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                          {
                                                string cassetteSeqNo = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                                string jobSeqNo = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                if (cassetteSeqNo != string.Empty && jobSeqNo != string.Empty && cassetteSeqNo != "0" && jobSeqNo != "0")
                                                {
                                                      Job job = ObjectManager.JobManager.GetJob(cassetteSeqNo, jobSeqNo);
                                                      if (job != null) jobs.Add(index.ToString(), job);
                                                }
                                                index++;
                                          }
                                    }

                                    //取出由MESService存入的CstData回覆
                                    foreach (LOTc lot in cst.MES_CstData.LOTLIST)
                                    {
                                          LocalCassetteDataReply_DPI.LOTDATAc _lot = new LocalCassetteDataReply_DPI.LOTDATAc();
                                          _lot.LOTNAME = lot.LOTNAME;
                                          _lot.PRODUCTSPECNAME = lot.PRODUCTSPECNAME;
                                          _lot.PROCESSOPERATIONNAME = lot.PROCESSOPERATIONNAME;
                                          _lot.PRODUCTOWNER = lot.PRODUCTOWNER;
                                          _lot.BCPRODUCTTYPE = lot.BCPRODUCTTYPE;
                                          _lot.CSTSETTINGCODE = lot.PRDCARRIERSETCODE;

                                          foreach (PRODUCTc product in lot.PRODUCTLIST)
                                          {
                                                LocalCassetteDataReply_DPI.PRODUCTDATAc _product = new LocalCassetteDataReply_DPI.PRODUCTDATAc();
                                                string pos = int.Parse(product.POSITION).ToString();

                                                //LocalMode有兩次要Data的時間點: 1.WaitingForCassetteData, 2:下貨後再Remap
                                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA ||
                                                    (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP && jobs.ContainsKey(pos)))
                                                {
                                                      _product.SLOTNO = product.POSITION;
                                                      _product.PROCESSFLAG = product.PROCESSFLAG;
                                                      _product.PRODUCTNAME = product.PRODUCTNAME;
                                                      //_product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
                                                      if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                                                            _product.PRODUCTRECIPENAME = eqp.File.CurrentRecipeID;
                                                      else
                                                      {
                                                            //_product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;

                                                            //BOX 取的是line recipe name
                                                            _product.PRODUCTRECIPENAME = lot.LINERECIPENAME;
                                                      }

                                                      if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                                                            _product.PPID = string.IsNullOrEmpty(jobs[pos].MES_PPID) ? jobs[pos].MesProduct.PPID : jobs[pos].MES_PPID;
                                                      else
                                                            _product.PPID = product.PPID;

                                                      _product.PRODUCTTYPE = product.PRODUCTTYPE;

                                                      _product.PRODUCTGRADE = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.PRODUCTGRADE : jobs[pos].JobGrade;
                                                      _product.PRODUCTJUDGE = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.PRODUCTJUDGE : jobs[pos].JobJudge;
                                                      _product.SUBPRODUCTGRADES = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.SUBPRODUCTGRADES : jobs[pos].OXRInformation;

                                                      _product.GROUPID = product.GROUPID;

                                                      foreach (CODEc code in product.ABNORMALCODELIST)
                                                      {
                                                            if (code.ABNORMALSEQ == "ABCODEULD")
                                                            {
                                                                  _product.ABNORMALCODE = code.ABNORMALCODE;
                                                                  break;
                                                            }
                                                      }

                                                      _product.OWNERTYPE = product.OWNERTYPE;
                                                      _product.OWNERID = product.OWNERID;
                                                      _product.REVPROCESSOPERATIONNAME = product.REVPROCESSOPERATIONNAME;

                                                      _lot.PRODUCTLIST.Add(_product);
                                                }

                                          }
                                          replyPort.LOTDATA = _lot;
                                    }
                              }
                              reply.BODY.PORTLIST.Add(replyPort);
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

            ///// <summary>
            ///// OPI MessageSet: Cassette Map Download Result Reply
            ///// </summary>
            ///// <param name="xml"></param>
            //public void OPI_CassetteMapDownloadResultReply_DPI(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message lineStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            public void OPI_StagePositionInfoRequest(XmlDocument xmlDoc)
            {
                  StagePositionInfoRequest command = Spec.XMLtoMessage(xmlDoc) as StagePositionInfoRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("StagePositionInfoReply") as XmlDocument;
                  StagePositionInfoReply reply = Spec.XMLtoMessage(xml_doc) as StagePositionInfoReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010050";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        //由PLCAgent取回Stage Position資料
                        string trxName = string.Format("{0}_Stage#01PositionInfo", command.BODY.EQUIPMENTNO);
                        Trx trx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { trxName }) as Trx;
                        if (trx != null) trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trx == null)
                        {
                              reply.RETURN.RETURNCODE = "0010052";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't get PLC value from Trx[{0}].", trxName);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get PLC value from Trx({2}).",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, trxName));
                        }
                        else
                        {
                              string evg_name = string.Format("{0}_EG_Stage#01PositionInfo", command.BODY.EQUIPMENTNO);
                              EventGroup evg = trx.EventGroups[evg_name];
                              if (evg == null)
                                    throw new Exception(string.Format("[{0}] is not exist in [{1}]", evg_name, trxName));

                              string ev_name = string.Format("{0}_B_Stage#01SendReady", command.BODY.EQUIPMENTNO);
                              Event ev = evg.Events[ev_name];
                              if (ev == null)
                                    throw new Exception(string.Format("[{0}] is not exist in [{1}][{2}]", ev_name, trxName, evg_name));
                              reply.BODY.SENDYREADY = ev.Items[0].Value;

                              ev_name = string.Format("{0}_B_Stage#01ReceiveReady", command.BODY.EQUIPMENTNO);
                              ev = evg.Events[ev_name];
                              if (ev == null)
                                    throw new Exception(string.Format("[{0}] is not exist in [{1}][{2}]", ev_name, trxName, evg_name));
                              reply.BODY.RECEIVEREADY = ev.Items[0].Value;

                              //ev_name = string.Format("{0}_B_Stage#01GlassExist", command.BODY.EQUIPMENTNO);
                              //ev = evg.Events[ev_name];
                              //if (ev == null)
                              //    throw new Exception(string.Format("[{0}] is not exist in [{1}][{2}]", ev_name, trxName, evg_name));
                              //reply.BODY.GLASSEXIST = ev.Items[0].Value;

                              ev_name = string.Format("{0}_B_Stage#01DoubleGlassExist", command.BODY.EQUIPMENTNO);
                              ev = evg.Events[ev_name];
                              if (ev == null)
                                    throw new Exception(string.Format("[{0}] is not exist in [{1}][{2}]", ev_name, trxName, evg_name));
                              reply.BODY.DOUBLEGLASSEXIST = ev.Items[0].Value;

                              ev_name = string.Format("{0}_B_Stage#01ExchangePossible", command.BODY.EQUIPMENTNO);
                              ev = evg.Events[ev_name];
                              if (ev == null)
                                    throw new Exception(string.Format("[{0}] is not exist in [{1}][{2}]", ev_name, trxName, evg_name));
                              reply.BODY.EXCHANGEPOSSIBLE = ev.Items[0].Value;
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
            /// OPI MessageSet: Offline Cassette Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_OfflineCassetteDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  OfflineCassetteDataRequest command = Spec.XMLtoMessage(xmlDoc) as OfflineCassetteDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("OfflineCassetteDataReply") as XmlDocument;
                  OfflineCassetteDataReply reply = Spec.XMLtoMessage(xml_doc) as OfflineCassetteDataReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}, CASSETTEID={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(command.BODY.CASSETTEID);

                        //從JobEachCassetteSlotPositionBlock中取出目前有的玻璃
                        string SlotPosTrxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                        Trx positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { SlotPosTrxName }) as Trx;
                        if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { SlotPosTrxName, true }) as Trx;

                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010760";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line{{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010761";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment{{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (lines[0].File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                        {
                              reply.RETURN.RETURNCODE = "0010764";
                              reply.RETURN.RETURNMESSAGE = string.Format("Line{{0}] Operation Mode is ChangerMode", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Line{{0}] Operation Mode is ChangerMode.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010762";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port{{0}] in PortEntity", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else if (cst == null)
                        {
                              reply.RETURN.RETURNCODE = "0010763";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Cassette[{0}] in CassetteEntity", command.BODY.CASSETTEID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette({0}) in CassetteEntity.",
                                  command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                        }
                        else if (positionTrx == null)
                        {
                              reply.RETURN.RETURNCODE = "0010765";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] SlotPosition Information invalid", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) SlotPosition Information invalid.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              reply.BODY.CSTSETTINGCODE = cst.LDCassetteSettingCode;

                              // 先Mark 機台報Remap也會報Cst Status Change Report, 所以應該不用下面這段
                              ////回給OPI資料前先更新OPI Port狀態，機台不會觸發事件改變Port的資訊(Port Count,Job Existance)
                              //string portTrxName = string.Format("{0}_Port#{1}PortStatusChangeReport", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                              //Trx portTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { portTrxName }) as Trx;
                              //if (portTrx != null) portTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { portTrxName, false }) as Trx;
                              //lock (port.File)
                              //{
                              //    port.File.JobCountInCassette = portTrx.EventGroups[0].Events[0].Items[4].Value;
                              //    port.File.JobExistenceSlot = portTrx.EventGroups[0].Events[0].Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                              //}
                              //ObjectManager.PortManager.EnqueueSave(port.File);
                              //PortCSTStatusReport(command.HEADER.TRANSACTIONID, port);

                              //記錄Port上的玻璃資訊
                              Dictionary<string, Job> jobs = new Dictionary<string, Job>();
                              int index = 1;
                              for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                              {
                                    string cassetteSeqNo = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                    string jobSeqNo = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                    if (cassetteSeqNo != "0" && jobSeqNo != "0" && cassetteSeqNo != string.Empty && jobSeqNo != string.Empty)
                                    {
                                          Job job = ObjectManager.JobManager.GetJob(cassetteSeqNo, jobSeqNo);
                                          if (job != null) jobs.Add(index.ToString(), job);
                                    }
                                    index++;
                              }

                              //reply.BODY.LINEOPERMODE = ObjectManager.LineManager.GetLine(command.BODY.LINENAME).File.LineOperMode;
                              //reply.BODY.PRODUCTQUANTITY = cst.MES_CstData.PRODUCTQUANTITY;
                              reply.BODY.PRODUCTQUANTITY = port.File.JobCountInCassette; //直接将CST中实际Count 送个OPI ,在BCS 重新下货时BC会检查数量。 20150304 Tom
                              //reply.BODY.CLEANFLAG = cst.OFFLINE_CstData.CLEANFLAG;

                              //取出由MESService存入的CstData回覆
                              foreach (LOTDATAc lot in cst.OFFLINE_CstData.LOTLIST)
                              {
                                    OfflineCassetteDataReply.LOTDATAc _lot = new OfflineCassetteDataReply.LOTDATAc();
                                    _lot.LOTNAME = lot.LOTNAME;
                                    _lot.PRODUCTSPECNAME = lot.PRODUCTSPECNAME;
                                    _lot.PROCESSOPERATIONNAME = lot.PROCESSOPERATIONNAME;
                                    _lot.PRODUCTOWNER = lot.PRODUCTOWNER;
                                    _lot.BCPRODUCTTYPE = lot.BCPRODUCTTYPE;
                                    //_lot.BCPRODUCTTYPE_POL = lot.BCPRODUCTTYPE_POL;
                                    _lot.BCPRODUCTTYPE_CUT = lot.BCPRODUCTTYPE_CUT;
                                    //_lot.BCPRODUCTTYPE_CUT_CROSS = lot.BCPRODUCTTYPE_CUT_CROSS;
                                    _lot.PRODUCTID = lot.PRODUCTID;
                                    //_lot.PRODUCTID_POL = lot.PRODUCTID_POL;
                                    _lot.PRODUCTID_CUT = lot.PRODUCTID_CUT;
                                    //_lot.PRODUCTID_CUT_CROSS = lot.PRODUCTID_CUT_CROSS;
                                    _lot.CFREWORKCOUNT = lot.CFREWORKCOUNT;
                                    //_lot.PRODUCTSIZE = lot.PRODUCTSIZE;
                                    _lot.LINERECIPENAME = lot.LINERECIPENAME;
                                    _lot.PPID = lot.PPID;
                                    //_lot.CSTSETTINGCODE = lot.PRDCARRIERSETCODE;
                                    _lot.CSTSETTINGCODE = lot.CSTSETTINGCODE;
                                    //_lot.INLINE_REWORK_MAX_COUNT = lot.CFINLINEREWORKMAXCOUNT;
                                    _lot.TARGETCSTID_CF = lot.TARGETCSTID_CF;
                                    //20151222 cy:Add for CUT
                                    _lot.CSTSETTINGCODE_CUT = lot.CSTSETTINGCODE_CUT;

                                    foreach (OFFLINEPROCESSLINEc processline in lot.PROCESSLINELIST)
                                    {
                                          OfflineCassetteDataReply.PROCESSLINEc _processline = new OfflineCassetteDataReply.PROCESSLINEc();
                                          _processline.LINENAME = processline.LINENAME;
                                          _processline.LINERECIPENAME = processline.LINERECIPENAME;
                                          _processline.PPID = processline.PPID;
                                          //_processline.CSTSETTINGCODE = processline.CARRIERSETCODE;
                                          _processline.CSTSETTINGCODE = processline.CSTSETTINGCODE;

                                          _lot.PROCESSLINELIST.Add(_processline);
                                    }

                                    foreach (OFFLINESTBPRODUCTSPECc stb in lot.STBPRODUCTSPECLIST)
                                    {
                                          OfflineCassetteDataReply.STBPRODUCTSPECc _stb = new OfflineCassetteDataReply.STBPRODUCTSPECc();
                                          _stb.LINENAME = stb.LINENAME;
                                          _stb.LINERECIPENAME = stb.LINERECIPENAME;
                                          _stb.PPID = stb.PPID;
                                          //_stb.CSTSETTINGCODE = stb.CARRIERSETCODE;
                                          _stb.CSTSETTINGCODE = stb.CSTSETTINGCODE;

                                          _lot.STBPRODUCTSPECLIST.Add(_stb);
                                    }

                                    foreach (OFFLINEPRODUCTDATAc product in lot.PRODUCTLIST)
                                    {
                                          OfflineCassetteDataReply.PRODUCTDATAc _product = new OfflineCassetteDataReply.PRODUCTDATAc();
                                          string pos = int.Parse(product.SLOTNO).ToString();

                                          if (jobs.ContainsKey(pos))
                                          {
                                                //_product.SLOTNO = product.POSITION;
                                                _product.SLOTNO = product.SLOTNO;
                                                _product.PROCESSFLAG = product.PROCESSFLAG;
                                                _product.PRODUCTNAME = product.PRODUCTNAME;
                                                _product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
                                                //_product.PPID = product.PPID;
                                                _product.PPID = product.OPI_PPID;
                                                _product.PRODUCTTYPE = product.PRODUCTTYPE;
                                                //_product.PRODUCTGRADE = product.PRODUCTGRADE;
                                                _product.PRODUCTGRADE = jobs[pos].JobGrade;

                                                //string cName = string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE);
                                                //for (int i = 0; i < ConstantManager[cName].Values.Count; i++)
                                                //{
                                                //    if (ConstantManager[cName][i].Value == product.PRODUCTJUDGE)
                                                //    {
                                                //        _product.PRODUCTJUDGE = i.ToString();
                                                //        break;
                                                //    }
                                                //}
                                                _product.PRODUCTJUDGE = jobs[pos].JobJudge;

                                                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                                                _product.GROUPID = product.GROUPID;
                                                //_product.TEMPERATUREFLAG = product.TEMPERATUREFLAG;
                                                //_product.PROCESSTYPE = GetArrayProcessType(lot, product, line);
                                                _product.PROCESSTYPE = product.PROCESSTYPE;
                                                _product.TARGETCSTID = product.TARGETCSTID;
                                                //if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                                //{
                                                //    // 資料來源由 PlanManager
                                                //    IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(port.File.CassetteID.Trim());
                                                //    SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == product.PRODUCTNAME);
                                                //    if (plan != null)
                                                //        _product.TARGETCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                                //}

                                                _product.INSPRESERVATION = product.INSPRESERVATION;
                                                _product.PREINLINEID = product.PREINLINEID;
                                                _product.FLOWPRIORITY = product.FLOWPRIORITY;
                                                //_product.NETWORKNO = ParameterManager["NETWORKNO"].Value.ToString();
                                                _product.NETWORKNO = product.NETWORKNO;
                                                //_product.REPAIRCOUNT = product.REPAIRCOUNT;
                                                //_product.ABNORMALCODE = product.ABNORMALCODE;
                                                //foreach (CODEc code in product.ABNORMALCODELIST)
                                                //{
                                                //    if (code.ABNORMALSEQ == "ABCODEULD")
                                                //    {
                                                //        _product.ABNORMALCODE = code.ABNORMALCODE;
                                                //        break;
                                                //    }
                                                //}

                                                //_product.CUTTINGFLAG = CellCuttingFlag(product.SHORTCUTFLAG);
                                                //_product.CUTTINGFLAG = product.CUTTINGFLAG;
                                                _product.OWNERTYPE = product.OWNERTYPE;
                                                _product.OWNERID = product.OWNERID;
                                                _product.REVPROCESSOPERATIONNAME = product.REVPROCESSOPERATIONNAME;
                                                //_product.EQPFLAG = jobs[pos].EQPFlag;
                                                _product.EQPFLAG = product.EQPFLAG;
                                                //_product.SUBSTRATETYPE = ((int)jobs[pos].SubstrateType).ToString();
                                                _product.SUBSTRATETYPE = product.SUBSTRATETYPE;
                                                //_product.AGINGENABLE = product.AGINGENABLE;
                                                _product.OXR = jobs[pos].OXRInformation;
                                                _product.OXR = product.OXR;
                                                //_product.OXRFLAG = jobs[pos].OXRInformationRequestFlag;
                                                //_product.OXRFLAG = product.OXRFLAG;
                                                //_product.COAVERSION = product.ARRAYPRODUCTSPECVER;
                                                _product.COAVERSION = product.COAVERSION;
                                                _product.SCRAPCUTFLAG = product.SCRAPCUTFLAG;
                                                _product.PANELSIZE = product.PANELSIZE;
                                                //_product.PANELSIZE_CROSS = product.PANELSIZE_CROSS;
                                                //_product.PANELSIZEFLAG = product.PANELSIZEFLAG;
                                                //_product.PANELSIZEFLAG_CROSS = product.PANELSIZEFLAG_CROSS;
                                                _product.TRUNANGLEFLAG = product.TRUNANGLE;
                                                //_product.ARRAYTTPEQVERSION = product.ARRAYTTPEQVERSION;
                                                //_product.NODESTACK = product.NODESTACK;
                                                //_product.REPAIRRESULT = product.REPAIRRESULT;OPI_LocalCassetteDataRequest
                                                _product.TARGET_SLOTNO = product.TARGET_SLOTNO;
                                                _product.CFINLINEREWORKMAXCOUNT = product.CFINLINEREWORKMAXCOUNT;
                                                //20151222 cy: add for CUT
                                                _product.REJUDGE_COUNT = product.REJUDGE_COUNT;
                                                _product.VENDER_NAME = product.VENDOR_NAME;
                                                _product.BUR_CHECK_COUNT = product.BUR_CHECK_COUNT;

                                                _lot.PRODUCTLIST.Add(_product);
                                          }
                                    }
                                    reply.BODY.LOTLIST.Add(_lot);
                              }
                        }

                        if (reply.BODY.LOTLIST.Count > 0)
                              reply.BODY.LOTLIST.RemoveAt(0);

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
            /// OPI MessageSet: Cassette Data Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalCassetteDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalCassetteDataRequest command = Spec.XMLtoMessage(xmlDoc) as LocalCassetteDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalCassetteDataReply") as XmlDocument;
                  LocalCassetteDataReply reply = Spec.XMLtoMessage(xml_doc) as LocalCassetteDataReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}, CASSETTEID={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.PORTID, command.BODY.CASSETTEID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(command.BODY.CASSETTEID);



                        //從JobEachCassetteSlotPositionBlock中取出目前有的玻璃
                        string SlotPosTrxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                        {
                              if (lines != null)
                              {
                                    if (lines[0].Data.FABTYPE == eFabType.CELL.ToString())
                                          SlotPosTrxName = string.Format("{0}_DP#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                              }
                        }

                        Trx positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { SlotPosTrxName }) as Trx;
                        if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { SlotPosTrxName, false }) as Trx;


                        //else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                        //{
                        //    reply.RETURN.RETURNCODE = "0010174";
                        //    reply.RETURN.RETURNMESSAGE = string.Format("Line{{0}] Operation Mode is ChangerMode", command.BODY.LINENAME);

                        //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Line{{0}] Operation Mode is ChangerMode.",
                        //        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        //}
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010170";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line{{0}] in LineEntity.", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010171";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment{{0}] in EquipmentEntity.", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010172";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port{{0}] in PortEntity.", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else if (cst == null)
                        {
                              reply.RETURN.RETURNCODE = "0010173";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Cassette[{0}] in CassetteEntity!", command.BODY.CASSETTEID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette({0}) in CassetteEntity.",
                                  command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                        }
                        else if (positionTrx == null)
                        {
                              reply.RETURN.RETURNCODE = "0010175";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] SlotPosition Information invalid", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) SlotPosition Information invalid.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                              //記錄Port上的玻璃資訊
                              Dictionary<string, Job> jobs = new Dictionary<string, Job>();

                              if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                              {
                                    //回給OPI資料前先更新OPI Port狀態，機台不會觸發事件改變Port的資訊(Port Count,Job Existance)
                                    string portTrxName = string.Format("{0}_Port#{1}PortStatusChangeReport", command.BODY.EQUIPMENTNO, port.Data.PORTNO);
                                    Trx portTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { portTrxName }) as Trx;
                                    if (portTrx != null) portTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { portTrxName, false }) as Trx;
                                    lock (port.File)
                                    {
                                          port.File.JobCountInCassette = portTrx.EventGroups[0].Events[0].Items[4].Value;
                                          port.File.JobExistenceSlot = portTrx.EventGroups[0].Events[0].Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                                    }
                                    ObjectManager.PortManager.EnqueueSave(port.File);
                                    PortCSTStatusReport(command.HEADER.TRANSACTIONID, port);

                                    //取得slot資訊
                                    int index = 1;
                                    for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                    {
                                          string cassetteSeqNo = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                          string jobSeqNo = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                          if (cassetteSeqNo != string.Empty && jobSeqNo != string.Empty && cassetteSeqNo != "0" && jobSeqNo != "0")
                                          {
                                                Job job = ObjectManager.JobManager.GetJob(cassetteSeqNo, jobSeqNo);
                                                if (job != null) jobs.Add(index.ToString(), job);
                                          }
                                          index++;
                                    }
                              }

                              //reply.BODY.LINEOPERMODE = ObjectManager.LineManager.GetLine(command.BODY.LINENAME).File.LineOperMode;
                              reply.BODY.PRODUCTQUANTITY = cst.MES_CstData.PRODUCTQUANTITY;
                              //reply.BODY.CLEANFLAG = cst.MES_CstData.CLEANFLAG;
                              reply.BODY.CSTSETTINGCODE = cst.MES_CstData.CARRIERSETCODE;

                              //取出由MESService存入的CstData回覆
                              foreach (LOTc lot in cst.MES_CstData.LOTLIST)
                              {
                                    LocalCassetteDataReply.LOTDATAc _lot = new LocalCassetteDataReply.LOTDATAc();
                                    _lot.LOTNAME = lot.LOTNAME;
                                    _lot.PRODUCTSPECNAME = lot.PRODUCTSPECNAME;
                                    _lot.PROCESSOPERATIONNAME = lot.PROCESSOPERATIONNAME;
                                    _lot.PRODUCTOWNER = lot.PRODUCTOWNER;
                                    _lot.BCPRODUCTTYPE = lot.BCPRODUCTTYPE;
                                    _lot.PRODUCTID = lot.BCPRODUCTID;
                                    _lot.CFREWORKCOUNT = lot.CFREWORKCOUNT;
                                    //_lot.CFINLINEREWORKMAXCOUNT = lot.CFINLINEREWORKMAXCOUNT;
                                    _lot.TARGETCSTID_CF = lot.TARGETCSTID_CF;
                                    //_lot.PRODUCTSIZE = lot.PRODUCTSIZE;
                                    _lot.LINERECIPENAME = lot.LINERECIPENAME;
                                    _lot.PPID = lot.PPID;
                                    _lot.CSTSETTINGCODE = lot.PRDCARRIERSETCODE;
                                    //TODO:201511222 cy:Add for CUT. (MES尚無資料來源)
                                    _lot.CSTSETTINGCODE_CUT = string.Empty;
                                    _lot.BCPRODUCTTYPE_CUT = string.Empty;
                                    _lot.PRODUCTID_CUT = string.Empty;

                                    foreach (PROCESSLINEc processline in lot.PROCESSLINELIST)
                                    {
                                          LocalCassetteDataReply.PROCESSLINEc _processline = new LocalCassetteDataReply.PROCESSLINEc();
                                          _processline.LINENAME = processline.LINENAME;
                                          _processline.LINERECIPENAME = processline.LINERECIPENAME;
                                          _processline.PPID = processline.PPID;
                                          _processline.CSTSETTINGCODE = processline.CARRIERSETCODE;
                                          _processline.BCPRODUCTTYPE = processline.BCPRODUCTTYPE;
                                          _processline.PRODUCTID = processline.BCPRODUCTID;
                                          _lot.PROCESSLINELIST.Add(_processline);
                                    }

                                    foreach (STBPRODUCTSPECc stb in lot.STBPRODUCTSPECLIST)
                                    {
                                          LocalCassetteDataReply.STBPRODUCTSPECc _stb = new LocalCassetteDataReply.STBPRODUCTSPECc();
                                          _stb.LINENAME = stb.LINENAME;
                                          _stb.LINERECIPENAME = stb.LINERECIPENAME;
                                          _stb.PPID = stb.PPID;
                                          _stb.CSTSETTINGCODE = stb.CARRIERSETCODE;
                                          _stb.BCPRODUCTTYPE = stb.BCPRODUCTTYPE;
                                          _stb.PRODUCTID = stb.BCPRODUCTID;
                                          _lot.STBPRODUCTSPECLIST.Add(_stb);
                                    }

                                    foreach (PRODUCTc product in lot.PRODUCTLIST)
                                    {
                                          LocalCassetteDataReply.PRODUCTDATAc _product = new LocalCassetteDataReply.PRODUCTDATAc();
                                          string pos = int.Parse(product.POSITION).ToString();

                                          //LocalMode有兩次要Data的時間點: 1.WaitingForCassetteData, 2:下貨後再Remap
                                          if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA ||
                                              (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP && jobs.ContainsKey(pos)))
                                          {
                                                _product.SLOTNO = product.POSITION;
                                                _product.PROCESSFLAG = product.PROCESSFLAG;
                                                _product.PRODUCTNAME = product.PRODUCTNAME;
                                                //_product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
                                                if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                                                      _product.PRODUCTRECIPENAME = eqp.File.CurrentRecipeID;
                                                else
                                                {
                                                      _product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
                                                      if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && ((port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE) || (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.VirtualPort)))
                                                      {
                                                            //BOX 取的是line recipe name
                                                            _product.PRODUCTRECIPENAME = lot.LINERECIPENAME;
                                                      }
                                                }

                                                if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                                                      _product.PPID = string.IsNullOrEmpty(jobs[pos].MES_PPID) ? jobs[pos].MesProduct.PPID : jobs[pos].MES_PPID;
                                                else
                                                      _product.PPID = product.PPID;

                                                _product.PRODUCTTYPE = product.PRODUCTTYPE;

                                                _product.PRODUCTGRADE = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.PRODUCTGRADE : jobs[pos].JobGrade;
                                                _product.PRODUCTJUDGE = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.PRODUCTJUDGE : jobs[pos].JobJudge;
                                                //_product.SUBPRODUCTGRADES = port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP ? product.SUBPRODUCTGRADES : jobs[pos].OXRInformation;

                                                //string cName = string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE);
                                                //for (int i = 0; i < ConstantManager[cName].Values.Count; i++)
                                                //{
                                                //    if (ConstantManager[cName][i].Value == product.PRODUCTJUDGE)
                                                //    {
                                                //        _product.PRODUCTJUDGE = i.ToString();
                                                //        break;
                                                //    }
                                                //}
                                                _product.GROUPID = product.GROUPID;
                                                //_product.TEMPERATUREFLAG = product.TEMPERATUREFLAG;
                                                _product.PROCESSTYPE = GetArrayProcessType(lot, product, line);

                                                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                                {
                                                      // 資料來源由 PlanManager
                                                      IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(line.File.CurrentPlanID, port.File.CassetteID.Trim());
                                                      SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == product.PRODUCTNAME);
                                                      if (plan != null)
                                                            _product.TARGETCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                                }

                                                //int networkNo = Convert.ToInt32(line.Data.LINEID.Substring(5, 1), 16);  //Jun Add 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                                                int networkNo = 0;
                                                int.TryParse(line.Data.LINEID.Substring(5, 1), out networkNo); //add by qiumin 20180214 Convert.ToInt32 only use 0-f, G LINE NG
                                                _product.NETWORKNO = networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數

                                                //foreach (CODEc code in product.ABNORMALCODELIST)
                                                //{
                                                //    if (code.ABNORMALSEQ == "ABCODEULD")
                                                //    {
                                                //        _product.ABNORMALCODE = code.ABNORMALCODE;
                                                //        break;
                                                //    }
                                                //}

                                                //_product.CUTTINGFLAG = CellCuttingFlag(product.SHORTCUTFLAG);
                                                _product.OWNERTYPE = product.OWNERTYPE;
                                                _product.OWNERID = product.OWNERID;
                                                _product.REVPROCESSOPERATIONNAME = product.REVPROCESSOPERATIONNAME;
                                                _product.TARGET_SLOTNO = product.TARGET_SLOTNO;
                                                _product.CFINLINEREWORKMAXCOUNT = product.MAXINLINERWCOUNT;
                                                //TODO:201511222 cy:Add for CUT. (MES尚無資料來源)
                                                
                                                _lot.PRODUCTLIST.Add(_product);
                                          }
                                    }

                                    reply.BODY.LOTLIST.Add(_lot);
                              }
                        }

                        if (reply.BODY.LOTLIST.Count > 0)
                              reply.BODY.LOTLIST.RemoveAt(0);

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
            /// OPI MessageSet: Force Clean Out Command Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ForceCleanOutCommandReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ForceCleanOutCommandReportRequest command = Spec.XMLtoMessage(xmlDoc) as ForceCleanOutCommandReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ForceCleanOutCommandReportReply") as XmlDocument;
                  ForceCleanOutCommandReportReply reply = Spec.XMLtoMessage(xml_doc) as ForceCleanOutCommandReportReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. COMMANDTYPE={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.COMMANDTYPE));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010850";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        //else if (!dicForceCleanOut.ContainsKey(command.BODY.LINENAME))
                        //{
                        //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Force Clean Out Command.",
                        //    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        //    reply.RETURN.RETURNCODE = "0010851";
                        //    reply.RETURN.RETURNMESSAGE = string.Format("Line[{0}] Can't find Force Clean Out Command.", command.BODY.LINENAME);
                        //}
                        else
                        {
                              reply.BODY.COMMANDTYPE = command.BODY.COMMANDTYPE;
                              if (dicForceCleanOut.ContainsKey(command.BODY.LINENAME))
                                    reply.BODY.SETSTATUS = dicForceCleanOut[command.BODY.LINENAME].STATUS;
                              else
                                    reply.BODY.SETSTATUS = "0";

                              reply.BODY.COMMANDLIST.Clear();

                              IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(command.BODY.LINENAME);
                              foreach (Equipment eqp in eqps)
                              {
                                    //if (eqp.Data.NODENO == "L7")
                                    if (!eqp.Data.REPORTMODE.Contains("PLC"))
                                          continue;//ForceCleanOutCommand只有ARRAY會用, ARRAY只有PHOTO有L7, L7不需要查ForceCleanOutCommandReply
                                    string trxName = string.Empty;
                                    string trxReply = string.Empty;
                                    if (command.BODY.COMMANDTYPE == "FORCECLEAN")
                                    {
                                          trxName = string.Format("{0}_ForceCleanOutCommand", eqp.Data.NODENO);
                                          trxReply = string.Format("{0}_ForceCleanOutCommandReply", eqp.Data.NODENO);
                                    }
                                    else if (command.BODY.COMMANDTYPE == "ABNORMAL")
                                    {
                                          trxName = string.Format("{0}_AbnormalForceCleanOutCommand", eqp.Data.NODENO);
                                          trxReply = string.Format("{0}_AbnormalForceCleanOutCommandReply", eqp.Data.NODENO);
                                    }

                                    Trx resultTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                                    Trx resultReplyTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxReply, false }) as Trx;

                                    if (resultTrx == null && resultReplyTrx == null) continue;

                                    ForceCleanOutCommandReportReply.COMMANDc cmd = new ForceCleanOutCommandReportReply.COMMANDc();
                                    cmd.EQUIPMENTNO = eqp.Data.NODENO;

                                    if (resultTrx != null)
                                    {
                                          cmd.BCSTATUS = resultTrx.EventGroups[0].Events[0].Items[0].Value;
                                    }
                                    if (resultReplyTrx != null)
                                    {
                                          cmd.EQSTATUS = resultReplyTrx.EventGroups[0].Events[0].Items[0].Value;
                                    }
                                    reply.BODY.COMMANDLIST.Add(cmd);
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        //Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] User ID [{2}] UIService reply message({3}) to OPI.",
                        //    command.BODY.LINENAME, command.HEADER.TRANSACTIONID,command.BODY.USERID,reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        ////NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] User ID [{2}] UIService catch message({3}) exception : {4} ",
                        //    command.BODY.LINENAME, command.HEADER.TRANSACTIONID,command.BODY.USERID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Robot Operation Mode Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_RobotOperationModeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  RobotOperationModeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotOperationModeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("RobotOperationModeReply") as XmlDocument;
                  RobotOperationModeReply reply = Spec.XMLtoMessage(xml_doc) as RobotOperationModeReply;

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
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                              command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010710";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              List<RobotPosition> positions = ObjectManager.RobotPositionManager.GetRobotPositions();
                              foreach (RobotPosition pos in positions)
                              {
                                    RobotOperationModeReply.ROBOTPOSITIONc _pos = new RobotOperationModeReply.ROBOTPOSITIONc();
                                    _pos.OPERATIONMODE = ((int)pos.Mode).ToString();
                                    _pos.ROBOTPOSITIONNO = pos.Data.ROBOTPOSITIONNO;

                                    reply.BODY.ROBOTPOSITIONLIST.Add(_pos);
                              }
                        }

                        if (reply.BODY.ROBOTPOSITIONLIST.Count > 0)
                              reply.BODY.ROBOTPOSITIONLIST.RemoveAt(0);

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
            /// OPI MessageSet: Robot Operation Mode Command
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_RobotOperationModeCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  RobotOperationModeCommand command = Spec.XMLtoMessage(xmlDoc) as RobotOperationModeCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("RobotOperationModeCommandReply") as XmlDocument;
                  RobotOperationModeCommandReply reply = Spec.XMLtoMessage(xml_doc) as RobotOperationModeCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENT={3}, ROBOTPOSITIONNO={4}, OPERATIONMODE={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.ROBOTPOSITIONNO, command.BODY.OPERATIONMODE));

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010430";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke ArraySpecialService RobotOperationModeCommand.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              eRobotOperationMode mode = new eRobotOperationMode();
                              if (command.BODY.OPERATIONMODE == "1") mode = eRobotOperationMode.NormalMode;
                              else if (command.BODY.OPERATIONMODE == "2") mode = eRobotOperationMode.DualMode;
                              else if (command.BODY.OPERATIONMODE == "3") mode = eRobotOperationMode.SingleMode;

                              Invoke("ArraySpecialService", "RobotOperationModeCommand",
                                  new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTPOSITIONNO, mode, eRobotOperationAction.Unknown });
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

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

            ///// <summary>
            ///// OPI MessageSet: Recipe Register Validation Return Reply to BC
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_RecipeRegisterValidationReturnReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message recipeRegisterValidationReturnReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            private void OperationPermissionRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] OperationPermissionRequest Reply Timeout.", sArray[0], trackKey));

                        lock (dicOperationPermission)
                        {
                              if (dicOperationPermission.ContainsKey(tmp))
                                    dicOperationPermission.Remove(tmp);
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Operation Permission Result Report
            ///// </summary>
            ///// <param name="lineName"></param>
            ///// <param name="eqpNo"></param>
            ///// <param name="trxID"></param>
            ///// <param name="replyName"></param>
            ///// <param name="oper"></param>
            //public void OperationPermissionResultReport(string lineName, string eqpNo, string returnCode)
            //{
            //      try
            //      {
            //            IServerAgent agent = GetServerAgent();
            //            XmlDocument xml_doc = agent.GetTransactionFormat("OperationPermissionResultReport") as XmlDocument;
            //            OperationPermissionResultReport trx = Spec.XMLtoMessage(xml_doc) as OperationPermissionResultReport;
            //            trx.BODY.LINENAME = lineName;
            //            trx.BODY.EQUIPMENTNO = eqpNo;

            //            string key = string.Format("{0}_OperationPermissionRequestReplyTimeout", eqpNo);
            //            trx.BODY.OPERATORPERMISSION = ((int)dicOperationPermission[key].Item2).ToString();
            //            trx.BODY.RETURNCODE = ((int)Enum.Parse(typeof(eReturnCode1), returnCode)).ToString();
            //            //取得機台回覆
            //            //string trxName = string.Format("{0}_OperationPermissionRequestCommandReply", eqpNo);
            //            //Trx resultTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { trxName }) as Trx;
            //            //if (resultTrx != null)
            //            //{
            //            //    resultTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, true }) as Trx;
            //            //    trx.BODY.RETURNCODE = resultTrx.EventGroups[0].Events[0].Items[0].Value;
            //            //}

            //            if (dicOperationPermission.ContainsKey(key))
            //            {
            //                  xMessage msg = SendToOPI(string.Empty, trx, new List<string>(1) { dicOperationPermission[key].Item1 });

            //                  Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
            //                  trx.BODY.LINENAME, msg.TransactionID));

            //                  lock (dicOperationPermission) dicOperationPermission.Remove(key);
            //            }
            //            else
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Can't get OperationPermission information.",
            //                  trx.BODY.LINENAME, GetTrxID("")));
            //            }

            //            //刪除記錄
            //            if (_timerManager.IsAliveTimer(key)) _timerManager.TerminateTimer(key);

            //      }
            //      catch (Exception ex)
            //      {
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //      }
            //}

            ///// <summary>
            ///// OPI MessageSet: Operation Permission Result Report Reply
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_OperationPermissionResultReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message OperationPermissionResultReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            private void OperationRunModeChangeRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] OperationPermissionRequest Reply Timeout.", sArray[0], trackKey));

                        lock (dicOpertationRunMode)
                        {
                              if (dicOpertationRunMode.ContainsKey(tmp))
                                    dicOpertationRunMode.Remove(tmp);
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Operation Run Mode Result Report
            ///// </summary>
            ///// <param name="lineName"></param>
            ///// <param name="eqpNo"></param>
            ///// <param name="cmdType">RUNMODE/LOADEROPERATIONMODE</param>
            ///// <param name="rtnCode">1:OK, 2:NG</param>
            //public void OperationRunModeChangeResultReport(string lineName, string eqpNo, string cmdType, string rtnCode)
            //{
            //      try
            //      {
            //            IServerAgent agent = GetServerAgent();
            //            XmlDocument xml_doc = agent.GetTransactionFormat("OperationRunModeChangeResultReport") as XmlDocument;
            //            OperationRunModeChangeResultReport trx = Spec.XMLtoMessage(xml_doc) as OperationRunModeChangeResultReport;

            //            trx.BODY.LINENAME = lineName;
            //            trx.BODY.EQUIPMENTNO = eqpNo;
            //            //COMMANDTYPE:  RUNMODE / LOADEROPERATIONMODE
            //            trx.BODY.COMMANDTYPE = cmdType;
            //            //1:OK, 2:NG
            //            //Watson Add 2015/3/7 Modify For 數值型態非字型態
            //            trx.BODY.RETURNCODE = ((int)Enum.Parse(typeof(eReturnCode1), rtnCode)).ToString();
            //            //trx.BODY.RETURNCODE = rtnCode;

            //            string key = string.Format("{0}_OperationRunModeChangeRequestReplyTimout", eqpNo);

            //            if (dicOpertationRunMode.ContainsKey(key))
            //            {
            //                  xMessage msg = SendToOPI(string.Empty, trx, new List<string>() { dicOpertationRunMode[key] });

            //                  Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
            //                  trx.BODY.LINENAME, msg.TransactionID));

            //                  lock (dicOpertationRunMode) dicOpertationRunMode.Remove(key);
            //            }
            //            else
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Can't get OperationRunMode Information.",
            //                  trx.BODY.LINENAME, GetTrxID("")));
            //            }

            //            //刪除記錄
            //            if (_timerManager.IsAliveTimer(key)) _timerManager.TerminateTimer(key);
            //      }
            //      catch (Exception ex)
            //      {
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //      }
            //}

            /// <summary>
            /// OPI MessageSet: Operation Run Mode Result Report Reply
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_OperationRunModeResultReportReply(XmlDocument xmlDoc)
            {
                  try
                  {
                        //IServerAgent agent = GetServerAgent();
                        //{
                        //    Message OperationRunModeResultReportReply = Spec.CheckXMLFormat(xmlDoc);
                        //}
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            /// <summary>
            /// OPI MessageSet: Equipment Fetch Glass Rule Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentFetchGlassRuleRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentFetchGlassRuleRequest command = Spec.XMLtoMessage(xmlDoc) as EquipmentFetchGlassRuleRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentFetchGlassRuleReply") as XmlDocument;
                  EquipmentFetchGlassRuleReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentFetchGlassRuleReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                          if (eqp != null)
                          {
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                              if (line != null && (line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT))
                              {
                                  reply.BODY.RULENAME1 = eqp.File.ProportionalRule01Type.ToString();
                                  reply.BODY.RULEVALUE1 = eqp.File.ProportionalRule01Value.ToString();
                                  reply.BODY.RULENAME2 = eqp.File.ProportionalRule02Type.ToString();
                                  reply.BODY.RULEVALUE2 = eqp.File.ProportionalRule02Value.ToString();

                                  if (line.Data.LINEID == "TCCVD700")
                                  {
                                      #region Robot Get Data [hujunpeng Add 20190429,Deng,20190823]
                                      Robot robot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);
                                      if (robot != null)
                                      {
                                          robot.File.CurCVDProportionalRule = new CVDProportionalRule();

                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);

                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);

                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);

                                          if (reply.BODY.RULENAME1 == "0" && reply.BODY.RULENAME2 == "1")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          else if (reply.BODY.RULENAME1 == "0" && reply.BODY.RULENAME2 == "2")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          else if (reply.BODY.RULENAME1 == "1" && reply.BODY.RULENAME2 == "0")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          else if (reply.BODY.RULENAME1 == "1" && reply.BODY.RULENAME2 == "2")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          else if (reply.BODY.RULENAME1 == "2" && reply.BODY.RULENAME2 == "0")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                              robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          else if (reply.BODY.RULENAME1 == "2" && reply.BODY.RULENAME2 == "1")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                              robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }

                                          ObjectManager.RobotManager.EnqueueSave(robot.File);//200151221存檔以備份
                                      }
                                      #endregion
                                  }
                                  else{
                                      #region Robot Get Data [Watson Add 20151014 Add]
                                      Robot robot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);
                                      if (robot != null)
                                      {
                                          robot.File.CurCVDProportionalRule = new CVDProportionalRule();

                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);

                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);

                                          if (reply.BODY.RULENAME1 == "0")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }


                                          if (reply.BODY.RULENAME1 == "1")
                                          {
                                              robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;  //先抽此種片
                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(reply.BODY.RULEVALUE1)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                              robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(reply.BODY.RULEVALUE1);

                                              robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(reply.BODY.RULEVALUE2)); //Keep Value
                                              robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(reply.BODY.RULEVALUE2);
                                          }
                                          ObjectManager.RobotManager.EnqueueSave(robot.File);//200151221存檔以備份
                                      }
                                      #endregion
                                  }
                                  SendReplyToOPI(command, reply);
                                  return;
                              }
                          }
                        string trxName = string.Format("{0}_EquipmentFetchGlassProportionalRuleReport", command.BODY.EQUIPMENTNO);
                        Trx ruleTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { trxName }) as Trx;
                        if (ruleTrx != null) ruleTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, true }) as Trx;

                        if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIOPMENTNO={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010910";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (ruleTrx == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIOPMENTNO={0}] [BCS <- OPI][{1}] Can't find the Trx({2})",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, trxName));

                              reply.RETURN.RETURNCODE = "0010911";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find the Trx[{0}]", trxName);
                        }
                        else
                        {
                              reply.BODY.RULENAME1 = ruleTrx.EventGroups[0].Events[0].Items[0].Value;
                              reply.BODY.RULEVALUE1 = ruleTrx.EventGroups[0].Events[0].Items[1].Value;
                              reply.BODY.RULENAME2 = ruleTrx.EventGroups[0].Events[0].Items[2].Value;
                              reply.BODY.RULEVALUE2 = ruleTrx.EventGroups[0].Events[0].Items[3].Value;
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
            /// OPI MessageSet: Unloading Port Setting Report
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_UnloadingPortSettingReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  UnloadingPortSettingReportRequest command = Spec.XMLtoMessage(xmlDoc) as UnloadingPortSettingReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("UnloadingPortSettingReportReply") as XmlDocument;
                  UnloadingPortSettingReportReply reply = Spec.XMLtoMessage(xml_doc) as UnloadingPortSettingReportReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3},PORT NO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;

                        //TO DO
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010761";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment{{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010762";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port{{0}] in PortEntity", command.BODY.PORTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              reply.BODY.OKSTOREQTIME = eqp.File.OKPortModeStoreQTime.ToString();
                              reply.BODY.OKPRODUCTTYPECHECKMODE = (int)eqp.File.OKPort + ":" + eqp.File.OKPort.ToString();
                              reply.BODY.NGSTOREQTIME = eqp.File.NGPortModeStoreQTime.ToString();
                              reply.BODY.NGPRODUCTTYPECHECKMODE = (int)eqp.File.NGPort + ":" + eqp.File.NGPort.ToString();
                              reply.BODY.NGPORTJUDGE = eqp.File.NGPortJudge;
                              reply.BODY.PDSTOREQTIME = eqp.File.PDPortModeStoreQTime.ToString();
                              reply.BODY.PDPRODUCTTYPECHECKMODE = (int)eqp.File.PDPort + ":" + eqp.File.PDPort.ToString();
                              reply.BODY.RPSTOREQTIME = eqp.File.RPPortModeStoreQTime.ToString();
                              reply.BODY.RPPRODUCTTYPECHECKMODE = (int)eqp.File.RPPort + ":" + eqp.File.RPPort.ToString();
                              reply.BODY.IRSTOREQTIME = eqp.File.IRPortModeStoreQTime.ToString();
                              reply.BODY.IRPRODUCTTYPECHECKMODE = (int)eqp.File.IRPort + ":" + eqp.File.IRPort.ToString();
                              reply.BODY.MIXSTOREQTIME = eqp.File.MIXPortModeStoreQTime.ToString();
                              reply.BODY.MIXPRODUCTTYPECHECKMODE = (int)eqp.File.MIXPort + ":" + eqp.File.MIXPort.ToString();
                              reply.BODY.MIXPORTJUDGE = eqp.File.MIXPortJudge.ToString();
                              reply.BODY.OPERATORID = eqp.File.OperatorID;
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

            /// OPI MessageSet: Buffer RW Judge Capacity Change Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_BufferRWJudgeCapacityChangeReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  BufferRWJudgeCapacityChangeReportRequest command = Spec.XMLtoMessage(xmlDoc) as BufferRWJudgeCapacityChangeReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("BufferRWJudgeCapacityChangeReportReply") as XmlDocument;
                  BufferRWJudgeCapacityChangeReportReply reply = Spec.XMLtoMessage(xml_doc) as BufferRWJudgeCapacityChangeReportReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        //TO DO
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
                              reply.BODY.BUFFERJUDGECAPACITY = eqp.File.CV06BufferInfo;
                              reply.BODY.BF01RWJUDGECAPACITY = eqp.File.Buffer01RWJudgeCapacity;
                              reply.BODY.BF02RWJUDGECAPACITY = eqp.File.Buffer02RWJudgeCapacity;
                              reply.BODY.OPERATORID = eqp.File.OperatorID;
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

            ///// OPI MessageSet:Unit Run Mode Report Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_UnitRunModeReportRequest(XmlDocument xmlDoc)
            //{
            ////wucc add 20150817  
            //IServerAgent agent = GetServerAgent();
            //UnitRunModeReportRequest command = Spec.XMLtoMessage(xmlDoc) as UnitRunModeReportRequest;
            //XmlDocument xml_doc = agent.GetTransactionFormat("UnitRunModeReportReply") as XmlDocument;
            //UnitRunModeReportReply reply = Spec.XMLtoMessage(xml_doc) as UnitRunModeReportReply;
            //try
            //{
            //    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.  EQUIPMENTNO={3}",
            //        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

            //    reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //    reply.BODY.LINENAME = command.BODY.LINENAME;

            //    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);

            //    if (eqp != null)
            //    {
            //        //先加入NODE RUNMODE , UNITNO為00
            //        UnitRunModeReportReply.UNITc NodeUnits = new UnitRunModeReportReply.UNITc();
            //        NodeUnits.UNITID = eqp.Data.NODEID;
            //        NodeUnits.UNITNO = "00";
            //        NodeUnits.CURRENT_RUNMODE = eqp.File.EquipmentRunMode;
            //        reply.BODY.UNITLIST.Add(NodeUnits);
            //        //加入 UNIT RUNMODE
            //        foreach (Unit unit in ObjectManager.UnitManager.GetUnitsByEQPNo(reply.BODY.EQUIPMENTNO))
            //        {
            //            UnitRunModeReportReply.UNITc Units = new UnitRunModeReportReply.UNITc();
            //            Units.UNITID = unit.Data.UNITID;
            //            if (unit.Data.UNITNO.Length == 1)
            //                Units.UNITNO = "0"+unit.Data.UNITNO;
            //            else
            //                Units.UNITNO = unit.Data.UNITNO;
            //            Units.CURRENT_RUNMODE = unit.File.RunMode;
            //            reply.BODY.UNITLIST.Add(Units);
            //        }
            //    }
            //    xMessage msg = SendReplyToOPI(command, reply);
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //           string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //}
            //}
            #endregion

            #region Command
            /// <summary>
            /// OPI MessageSet: Line Mode Change Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LineModeChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LineModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as LineModeChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LineModeChangeReply") as XmlDocument;
                  LineModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as LineModeChangeReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. LINEID={3}, OPERATORID={4}, LINEMODE={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.LINEID, command.BODY.OPERATORID, command.BODY.LINEMODE));

                        //轉拋給LineService協助確認基本狀態
                        string returnMsg = string.Empty;
                        //for特殊Line，Transaction format新增LineID，永以區別LineName(ServerName)
                        object[] parameters = new object[] { command.HEADER.TRANSACTIONID, command.BODY.LINEID, command.BODY.LINEMODE, returnMsg };
                        string strResult = Invoke("LineService", "CheckControlCondition", parameters,
                                new Type[] { typeof(string), typeof(string), typeof(string), typeof(string).MakeByRefType() }).ToString();
                        returnMsg = (string)parameters[3];
                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINEID);
                        //記錄Mode切換類型
                        int iCase = 0;
                        if (line == null)
                        {
                              reply.RETURN.RETURNCODE = "0010105";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINEID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINEID={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                                  command.BODY.LINEID, command.HEADER.TRANSACTIONID));
                        }
                        else if (line.File.HostMode.ToString() == "OFFLINE" && (command.BODY.LINEMODE == "REMOTE" || command.BODY.LINEMODE == "LOCAL"))
                        {
                              if (strResult == "N")
                              {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, CheckControlCondition({3}) / Line IndexOperMode({4}). Error Msg: {5}",
                                    command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, strResult, line.File.IndexOperMode.ToString(), returnMsg));

                                    reply.RETURN.RETURNCODE = "0010101";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Offline to Online NG, Error Msg: {0}", returnMsg);
                              }
                              else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                              {
                                    returnMsg = string.Format("Line[{0}] IndexOperMode is \"COOL_RUN_MODE\", Can not change \"{1}\" Mode",
                                        command.BODY.LINEID, command.BODY.LINEMODE);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, Error Msg: {3}",
                                    command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, returnMsg));

                                    reply.RETURN.RETURNCODE = "0010101";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Offline to Online NG, Error Msg: {0}", returnMsg);
                              }

                              //check all port's status for array
                              if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                              {
                                  List<Port> lstPort;
                                  lstPort = ObjectManager.PortManager.GetPorts();
                                  if (lstPort != null && lstPort.Count > 0)
                                  {
                                      foreach(Port pt in lstPort)
                                      {
                                          if(pt.File.Status == ePortStatus.LC)
                                          {
                                              returnMsg = string.Format("Port[{0}] Status is LoadComplete", pt.Data.PORTNO);

                                              Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                              string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, Error Msg: {3}",
                                              command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, returnMsg));

                                              reply.RETURN.RETURNCODE = "0010101";
                                              reply.RETURN.RETURNMESSAGE = string.Format("Offline to Online NG, Error Msg: {0}", returnMsg);
                                              break;
                                          }
                                      }
                                  }
                              }
                              iCase = 1;
                        }
                        else if (line.File.HostMode.ToString() == "REMOTE" && command.BODY.LINEMODE == "LOCAL")
                        {
                              if (strResult != "Y")
                              {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, CheckControlCondition({3}). Error Msg: {4}",
                                    command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, strResult, returnMsg));

                                    reply.RETURN.RETURNCODE = "0010102";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Online Remote to Online Local NG, Error Msg: {0}", returnMsg);
                              }
                              iCase = 2;
                        }
                        else if (line.File.HostMode.ToString() == "LOCAL" && command.BODY.LINEMODE == "REMOTE")
                        {
                              if (strResult != "Y")
                              {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, CheckControlCondition({3}). Error Msg: {4}",
                                    command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, strResult, returnMsg));

                                    reply.RETURN.RETURNCODE = "0010103";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Online Local to Online NG, Error Msg: {0}", returnMsg);
                              }
                              iCase = 3;
                        }
                        else if ((line.File.HostMode.ToString() == "LOCAL" || line.File.HostMode.ToString() == "REMOTE") && command.BODY.LINEMODE == "OFFLINE")
                        {
                              if (strResult != "Y" || line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                              {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINEID={0}] [BCS <- OPI][{1}] LineMode change to ({2}) NG, CheckControlCondition({3}) / IndexOperMode({4}). Error Msg: {5}",
                                    command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE, strResult, line.File.IndexOperMode.ToString(), returnMsg));

                                    reply.RETURN.RETURNCODE = "0010104";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Online to Offline NG, Error Msg: {0}", returnMsg);
                              }
                        }
                        else if (strResult == "A")
                        {
                              Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINEID={0}] [BCS <- OPI][{1}] OPI LineMode and EQP Line Mode ({2}) is the same mode.",
                                  command.BODY.LINEID, command.HEADER.TRANSACTIONID, command.BODY.LINEMODE));

                              reply.RETURN.RETURNCODE = "0010100";
                              reply.RETURN.RETURNMESSAGE = "Line Mode Is The Same";
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.LINEID = command.BODY.LINEID;

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                        //如果無異常，則修改HostMode狀態，並根據切換類型上報event給MESService
                        if (reply.RETURN.RETURNCODE == "0000000")
                        {
                              lock (line)
                              {
                                    line.File.PreHostMode = line.File.HostMode;
                                    line.File.OPICtlMode = (eHostMode)Enum.Parse(typeof(eHostMode), command.BODY.LINEMODE);
                              }
                              ObjectManager.LineManager.EnqueueSave(line.File);

                              switch (iCase)
                              {
                                    case 1:   //Offline -> Online report MES AreYouThereRequest
                                          Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINEID={0}] [BCS <- OPI][{1}] Invoke MESService AreYouThereRequest.",
                                          command.BODY.LINEID, command.HEADER.TRANSACTIONID));

                                          //Watson Modify 20141115 For UI ON Line 使用，Areyouther不只有online會用，bc initail 也會發送，定時也會發送
                                          Invoke(eServiceName.MESService, "AreYouThereRequest_UI", new object[] { command.HEADER.TRANSACTIONID, line.Data.LINEID });

                                          string timeId = string.Format("{0}_OPI_LineModeChangeRequest", line.Data.LINEID);
                                          if (Timermanager.IsAliveTimer(timeId))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                                                Timermanager.TerminateTimer(timeId);
                                          Timermanager.CreateTimer(timeId, false, 15000, new System.Timers.ElapsedEventHandler(LineModeChangeTimeout), line.Data.LINEID);

                                          break;

                                    case 2:   //Remote -> Local
                                    case 3:   //Local -> Remote
                                    default:  //Online -> Offline report MES MachineControlStateChanged
                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINEID={0}] [BCS <- OPI][{1}] Invoke MESService MachineControlStateChanged.",
                                          command.BODY.LINEID, command.HEADER.TRANSACTIONID));

                                          Invoke(eServiceName.MESService, "MachineControlStateChanged", new object[] { command.HEADER.TRANSACTIONID, line.Data.LINEID, command.BODY.LINEMODE });
                                          break;
                              }
                              //report OPI LineStatusReport
                              string trxid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                              //LineStatusReport(trxid, line);
                        }
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
            /// OPI MessageSet: DateTime Calibration Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_DateTimeCalibrationRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  DateTimeCalibrationRequest command = Spec.XMLtoMessage(xmlDoc) as DateTimeCalibrationRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("DateTimeCalibrationReply") as XmlDocument;
                  DateTimeCalibrationReply reply = Spec.XMLtoMessage(xml_doc) as DateTimeCalibrationReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, DATETIME={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.OPERATORID, command.BODY.DATETIME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010110";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Invoke DateTimeService OPISetEQDateTimeCommand.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              //轉拋給DataTimeService處理
                              Invoke("DateTimeService", "OPISetEQDateTimeCommand", new object[] { command });
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
            /// OPI MessageSet: Port Command Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PortCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  PortCommandRequest command = Spec.XMLtoMessage(xmlDoc) as PortCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("PortCommandReply") as XmlDocument;
                  PortCommandReply reply = Spec.XMLtoMessage(xml_doc) as PortCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, EQUIPMENTNO={4}, PORTNO={5}, COMMANDTYPE={6}, PORTCOMMAND={7}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.COMMANDTYPE, command.BODY.PORTCOMMAND));

                        #region ServerName != LineName 的處理方式
                        //Watson 20150301 Add If ServerName != Line ID
                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        if (command.BODY.LINENAME.Contains(eLineType.CELL.CBPMT))
                        {
                              line = GetLineByLines(keyCELLPMTLINE.CBPMI);
                        }

                        if (line == null)
                        {
                              reply.RETURN.RETURNCODE = "0010140";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORTID={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        #endregion
                        //Watson 20150301 Add If ServerName != Line ID
                        Port port = ObjectManager.PortManager.GetPort(line.Data.LINEID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                        //Port port = ObjectManager.PortManager.GetPort(command.BODY.LINENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO);

                        if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010140";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity!", command.BODY.PORTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORTID={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //Watson Add 2015301 For Dense or Normal Port Command different
                              eFabType fabType;
                              Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                              //轉拋給PortService各對應method
                              switch (command.BODY.COMMANDTYPE)
                              {
                                    case "TYPE":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[PORTID={0}] [BCS <- OPI][{1}] Invoke PortService PortTypeChangeCommand.",
                                              port.Data.PORTID, command.HEADER.TRANSACTIONID));

                                          if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                                Invoke("DenseBoxPortService", "DPPortTypeChangeCommand", new object[] { line.Data.LINEID, command });
                                          else
                                                Invoke("PortService", "PortTypeChangeCommand", new object[] { line.Data.LINEID, command });

                                          break;
                                    case "MODE":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[PORTID={0}] [BCS <- OPI][{1}] Invoke PortService PortModeChangeCommand.",
                                              port.Data.PORTID, command.HEADER.TRANSACTIONID));
                                          if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                                Invoke("DenseBoxPortService", "DPPortModeChangeCommand", new object[] { command.HEADER.TRANSACTIONID, line.Data.LINEID, command });
                                          else
                                                Invoke("PortService", "PortModeChangeCommand", new object[] { command.HEADER.TRANSACTIONID, line.Data.LINEID, command });
                                          break;
                                    case "ENABLED":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[PORTID={0}] [BCS <- OPI][{1}] Invoke PortService PortEnableModeChangeCommand.",
                                              port.Data.PORTID, command.HEADER.TRANSACTIONID));

                                          if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                                Invoke("DenseBoxPortService", "DPPortEnableModeChangeCommand", new object[] { line.Data.LINEID, command });
                                          else
                                                Invoke("PortService", "PortEnableModeChangeCommand", new object[] { line.Data.LINEID, command });
                                          break;
                                    case "TRANSFER":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[PORTID={0}] [BCS <- OPI][{1}] Invoke PortService PortTransferModeChangeCommand.",
                                              port.Data.PORTID, command.HEADER.TRANSACTIONID));
                                          if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                                Invoke("DenseBoxPortService", "DPPortTransferModeChangeCommand", new object[] { line.Data.LINEID, command });
                                          else
                                                Invoke("PortService", "PortTransferModeChangeCommand", new object[] { line.Data.LINEID, command });
                                          break;
                              }
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.PORTCOMMAND = command.BODY.PORTCOMMAND;

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
            /// OPI MessageSet: CIM Message Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CIMMessageDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CIMMessageDataRequest command = Spec.XMLtoMessage(xmlDoc) as CIMMessageDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CIMMessageDataReply") as XmlDocument;
                  CIMMessageDataReply reply = Spec.XMLtoMessage(xml_doc) as CIMMessageDataReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null && command.BODY.EQUIPMENTNO != "00")
                        {
                              reply.RETURN.RETURNCODE = "0010150";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              IDictionary<string, List<CIMMessage>> dicMessage = new Dictionary<string, List<CIMMessage>>();
                              List<CIMMessage> messages = new List<CIMMessage>();

                              //EquipmentNo為00時，回覆全部機台CIM Message Data
                              if (command.BODY.EQUIPMENTNO != "00")
                                    dicMessage = ObjectManager.CIMMessageManager.CIMMessageRequest(command.BODY.EQUIPMENTNO);
                              else
                                    dicMessage = ObjectManager.CIMMessageManager.CIMMessageRequest("");

                              if (dicMessage != null && dicMessage.Count != 0)
                              {

                                    if (command.BODY.EQUIPMENTNO != "00")
                                          messages = dicMessage[command.BODY.EQUIPMENTNO];
                                    //TODO: 顯示全部機台CIM Message

                                    foreach (CIMMessage message in messages)
                                    {
                                          CIMMessageDataReply.MESSAGEc _message = new CIMMessageDataReply.MESSAGEc();

                                          _message.MESSAGEID = message.MessageID;
                                          _message.TOUCHPANELNO = message.TouchPanelNo;
                                          _message.MESSAGEDATETIME = message.OccurDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                          _message.MESSAGETEXT = message.Message;

                                          reply.BODY.MESSAGELIST.Add(_message);
                                    }
                              }
                        }

                        if (reply.BODY.MESSAGELIST.Count > 0)
                              reply.BODY.MESSAGELIST.RemoveAt(0);

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
            /// OPI MessageSet: CIM Message Clear Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CIMMessageCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CIMMessageCommandRequest command = Spec.XMLtoMessage(xmlDoc) as CIMMessageCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CIMMessageCommandReply") as XmlDocument;
                  CIMMessageCommandReply reply = Spec.XMLtoMessage(xml_doc) as CIMMessageCommandReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, TOUCHPANELNO={4}, EQUIPMENTNO={5}, MESSAGEID={6}, COMMAND={7}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.OPERATORID, command.BODY.TOUCHPANELNO, command.BODY.EQUIPMENTNO, command.BODY.MESSAGEID, command.BODY.COMMAND));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.MESSAGEID = command.BODY.MESSAGEID;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010162";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (eqp == null && command.BODY.EQUIPMENTNO != "00")
                        {
                              reply.RETURN.RETURNCODE = "0010160";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.EQUIPMENTNO == "00" && command.BODY.COMMAND == "Clear")
                        {
                              reply.RETURN.RETURNCODE = "0010161";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't execute clear command when EQUIPMENTNO is 00.", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't execute clear command when EQUIPMENTNO is 00.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              if (command.BODY.EQUIPMENTNO != "00")
                              {
                                    #region 打給特定機台

                                    //轉拋給CIMMessageService各對應method
                                    switch (command.BODY.COMMAND)
                                    {
                                          case "Set":
                                                if (!string.IsNullOrEmpty(command.BODY.TOUCHPANELNO))
                                                {
                                                      NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageSetCommandForCELL.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                      Invoke("CIMMessageService", "CIMMessageSetCommandForCELL", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGETEXT, command.BODY.OPERATORID, command.BODY.TOUCHPANELNO });
                                                }
                                                else
                                                {
                                                      NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageSetCommand.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                      Invoke("CIMMessageService", "CIMMessageSetCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGETEXT, command.BODY.OPERATORID });
                                                }
                                                break;
                                          case "Clear":
                                                if (!string.IsNullOrEmpty(command.BODY.TOUCHPANELNO))
                                                {
                                                      NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageClearCommandForCELL.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                      Invoke("CIMMessageService", "CIMMessageClearCommandForCELL", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGEID, command.BODY.OPERATORID, command.BODY.TOUCHPANELNO });
                                                }
                                                else
                                                {
                                                      NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageClearCommand.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                      Invoke("CIMMessageService", "CIMMessageClearCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGEID, command.BODY.OPERATORID });
                                                      break;
                                                }
                                                break;
                                    }

                                    #endregion
                              }
                              else
                              {
                                    #region 對所有機台廣播

                                    foreach (Equipment eq in ObjectManager.EquipmentManager.GetEQPs())
                                    {
                                          switch (command.BODY.COMMAND)
                                          {
                                                case "Set":
                                                      if (!string.IsNullOrEmpty(command.BODY.TOUCHPANELNO))
                                                      {
                                                            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageSetCommandForCELL.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("CIMMessageService", "CIMMessageSetCommandForCELL", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGETEXT, command.BODY.OPERATORID, command.BODY.TOUCHPANELNO });
                                                      }
                                                      else
                                                      {
                                                            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageSetCommand.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("CIMMessageService", "CIMMessageSetCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGETEXT, command.BODY.OPERATORID });
                                                      }
                                                      break;
                                                case "Clear":
                                                      if (!string.IsNullOrEmpty(command.BODY.TOUCHPANELNO))
                                                      {
                                                            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageClearCommandForCELL.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("CIMMessageService", "CIMMessageClearCommandForCELL", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGEID, command.BODY.OPERATORID, command.BODY.TOUCHPANELNO });
                                                      }
                                                      else
                                                      {
                                                            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageClearCommand.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("CIMMessageService", "CIMMessageClearCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.MESSAGEID, command.BODY.OPERATORID });
                                                            break;
                                                      }
                                                      break;
                                          }
                                          NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CIMMessageService CIMMessageSetCommand.",
                                              eq.Data.NODENO, command.HEADER.TRANSACTIONID));
                                          Invoke("CIMMessageService", "CIMMessageSetCommand", new object[] { command.HEADER.TRANSACTIONID, eq.Data.NODENO, command.BODY.MESSAGETEXT, command.BODY.OPERATORID });
                                    }

                                    #endregion
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
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
            /// OPI MessageSet: CIM Mode Change Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CIMModeChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CIMModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as CIMModeChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CIMModeChangeReply") as XmlDocument;
                  CIMModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as CIMModeChangeReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, EQUIPMENTNO={4}, CIMMODE={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.CIMMODE));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.EQUIPMENTID = command.BODY.EQUIPMENTID;
                        reply.BODY.CIMMODE = command.BODY.CIMMODE;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010180";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find  Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService CIMModeChangeCommand.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              eCIMModeCmd cimMode = command.BODY.CIMMODE == "1" ? eCIMModeCmd.CIM_ON : eCIMModeCmd.CIM_OFF;
                              //轉拋給EquipmentService處理
                              Invoke("EquipmentService", "CIMModeChangeCommand", new object[] { command.BODY.EQUIPMENTNO, cimMode, command.HEADER.TRANSACTIONID });
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
            /// OPI MessageSet: Incomplete Cassette Command to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_IncompleteCassetteCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IncompleteCassetteCommand command = Spec.XMLtoMessage(xmlDoc) as IncompleteCassetteCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("IncompleteCassetteCommandReply") as XmlDocument;
                  IncompleteCassetteCommandReply reply = Spec.XMLtoMessage(xml_doc) as IncompleteCassetteCommandReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Operator ID ({2}) Set IncompleteCassetteCommand , Command ({3}), PortID ({4}), CassetteID ({5}), MesTrxID ({6})"
                           , command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.OPERATORID, command.BODY.COMMAND, command.BODY.PORTID, command.BODY.CASSETTEID, command.BODY.MESTRXID));

                        string strDesc;
                        XmlDocument xmlDocument;
                        if (!ObjectManager.CassetteManager.IncompleteCassetteCommandReply(command.BODY.COMMAND.Trim(), command.BODY.PORTID.Trim(), command.BODY.CASSETTEID.Trim(), command.BODY.MESTRXID.Trim(), command.BODY.FILENAME.Trim(), command.BODY.INCOMPLETEDATE.Trim(), out strDesc, out xmlDocument))
                        {
                              if (strDesc == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010210";
                                    reply.RETURN.RETURNMESSAGE = "Get No Data & No Message";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Get No Data & No Message",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    string[] strDescSplit = strDesc.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strDescSplit != null && strDescSplit.Length >= 2)
                                    {
                                          switch (strDescSplit[1].ToString().Trim())
                                          {
                                                case "Path_Error":
                                                      reply.RETURN.RETURNCODE = "0010211";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Parameters_Error":
                                                      reply.RETURN.RETURNCODE = "0010212";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "File_Error":
                                                      reply.RETURN.RETURNCODE = "0010213";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "UpdateDB_Error":
                                                      reply.RETURN.RETURNCODE = "0010214";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Command_Error":
                                                      reply.RETURN.RETURNCODE = "0010215";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Exception_Error":
                                                      reply.RETURN.RETURNCODE = "0010216";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;
                                          }
                                    }
                              }
                        }
                        else
                        {
                              if (command.BODY.COMMAND.ToUpper() == "RESEND")
                              {
                                    if (xmlDocument != null)
                                    {
                                          SendToMES(xmlDocument);

                                          Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                             string.Format("[LINENAME={0}] [BCS <- OPI][{1}], Operator ID ({2}) Set ReSendLotPorcessEnd , Command ({3}), PortID ({4}), CassetteID ({5}), MesTrxID ({6})"
                                             , command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.OPERATORID, command.BODY.COMMAND, command.BODY.PORTID, command.BODY.CASSETTEID, command.BODY.MESTRXID));
                                    }
                                    else
                                    {
                                          Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()"
                                              , string.Format("[LINENAME={0}] [BCS <- OPI][{1}], Operator ID ({2}) IncompleteCassetteCommand XmlDocument is null , Command ({3}), PortID ({4}), CassetteID ({5}), MesTrxID ({6})"
                                              , command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.OPERATORID, command.BODY.COMMAND, command.BODY.PORTID, command.BODY.CASSETTEID, command.BODY.MESTRXID));
                                    }
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
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
            /// OPI MessageSet: Incomplete Cassette Edit Save
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_IncompleteCassetteEditSave(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IncompleteCassetteEditSave command = Spec.XMLtoMessage(xmlDoc) as IncompleteCassetteEditSave;
                  XmlDocument xml_doc = agent.GetTransactionFormat("IncompleteCassetteEditSaveReply") as XmlDocument;
                  IncompleteCassetteEditSaveReply reply = Spec.XMLtoMessage(xml_doc) as IncompleteCassetteEditSaveReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}], Operator ID ({2}) Set IncompleteCassetteEditSave", command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.OPERATORID));

                        string strDesc;
                        if (!ObjectManager.CassetteManager.IncompleteCassetteEditSaveReply(command.BODY.PORTID.Trim(), command.BODY.CASSETTEID.Trim(), command.BODY.MESTRXID.Trim(), command.BODY.FILENAME.Trim(), command.BODY.INCOMPLETEDATE.Trim(), xmlDoc, out strDesc))
                        {
                              if (strDesc == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010220";
                                    reply.RETURN.RETURNMESSAGE = "Get No Data & No Message";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Get No Data & No Message",
                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    string[] strDescSplit = strDesc.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strDescSplit != null && strDescSplit.Length >= 2)
                                    {
                                          switch (strDescSplit[1].ToString().Trim())
                                          {
                                                case "IncompleteCSTPath_Error":
                                                      reply.RETURN.RETURNCODE = "0010221";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Parameters_Error":
                                                      reply.RETURN.RETURNCODE = "0010222";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "File_Error":
                                                      reply.RETURN.RETURNCODE = "0010223";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Exception_Error":
                                                      reply.RETURN.RETURNCODE = "0010224";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;
                                          }
                                    }
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
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
            /// OPI MessageSet: Incomplete Box Edit Save
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_IncompleteBoxEditSave(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IncompleteBoxEditSave command = Spec.XMLtoMessage(xmlDoc) as IncompleteBoxEditSave;
                  IncompleteBoxEditSaveReply reply = new IncompleteBoxEditSaveReply();

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.INCOMPLETEDATE = command.BODY.INCOMPLETEDATE;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.FILENAME = command.BODY.FILENAME;
                        reply.BODY.MESTRXID = command.BODY.MESTRXID;

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}], Operator ID ({2}) Set OPI_IncompleteBoxEditSave", command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.OPERATORID));

                        MesSpec.BoxProcessEnd.TrxBody mes = new MesSpec.BoxProcessEnd.TrxBody();
                        #region IncompleteBoxEditSave 轉 BoxProcessEnd
                        {
                              foreach (IncompleteBoxEditSave.BOXc opi_box in command.BODY.BOXLIST)
                              {
                                    MesSpec.BoxProcessEnd.BOXc mes_box = new MesSpec.BoxProcessEnd.BOXc();
                                    mes_box.BOXNAME = opi_box.BOXNAME;
                                    foreach (IncompleteBoxEditSave.PRODUCTc opi_prod in opi_box.PRODUCTLIST)
                                    {
                                          MesSpec.BoxProcessEnd.PRODUCTc mes_prod = new MesSpec.BoxProcessEnd.PRODUCTc();
                                          foreach (IncompleteBoxEditSave.ABNORMALCODEc opi_code in opi_prod.ABNORMALCODELIST)
                                          {
                                                MesSpec.BoxProcessEnd.CODEc mes_code = new MesSpec.BoxProcessEnd.CODEc();
                                                mes_code.ABNORMALCODE = opi_code.ABNORMALCODE;
                                                mes_code.ABNORMALSEQ = opi_code.ABNORMALSEQ;
                                                mes_prod.ABNORMALCODELIST.Add(mes_code);
                                          }
                                          mes_prod.BOXULDFLAG = opi_prod.BOXULDFLAG;
                                          mes_prod.DPIPROCESSFLAG = opi_prod.DPIPROCESSFLAG;
                                          mes_prod.HOSTPPID = opi_prod.HOSTPPID;
                                          mes_prod.HOSTPRODUCTNAME = opi_prod.HOSTPRODUCTNAME;
                                          mes_prod.POSITION = opi_prod.POSITION;
                                          mes_prod.PPID = opi_prod.PPID;
                                          mes_prod.PRODUCTNAME = opi_prod.PRODUCTNAME;
                                          mes_prod.RTPFLAG = opi_prod.RTPFLAG;
                                          mes_prod.SHORTCUTFLAG = opi_prod.SHORTCUTFLAG;
                                          mes_box.PRODUCTLIST.Add(mes_prod);
                                    }
                                    mes_box.PRODUCTQUANTITY = opi_box.PRODUCTQUANTITY;
                                    mes.BOXLIST.Add(mes_box);
                              }
                              mes.BOXQUANTITY = command.BODY.BOXQUANTITY;
                              mes.HOSTLINERECIPENAME = command.BODY.HOSTLINERECIPENAME;
                              mes.HOSTPPID = command.BODY.HOSTPPID;
                              mes.LINENAME = command.BODY.LINENAME;
                              mes.LINERECIPENAME = command.BODY.LINERECIPENAME;
                              mes.PORTNAME = command.BODY.PORTID;
                              mes.PPID = command.BODY.PPID;
                              mes.SAMPLEFLAG = command.BODY.SAMPLEFLAG;
                              mes.TIMESTAMPdt = DateTime.Now;
                        }
                        #endregion

                        string strDesc;
                        if (!ObjectManager.CassetteManager.IncompleteBoxEditSaveReply(command.BODY.PORTID.Trim(), command.BODY.MESTRXID.Trim(), command.BODY.FILENAME.Trim(), command.BODY.INCOMPLETEDATE.Trim(), mes, out strDesc))
                        {
                              if (strDesc == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010220";
                                    reply.RETURN.RETURNMESSAGE = "Get No Data & No Message";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Get No Data & No Message",
                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                              else
                              {
                                    string[] strDescSplit = strDesc.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strDescSplit != null && strDescSplit.Length >= 2)
                                    {
                                          switch (strDescSplit[1].ToString().Trim())
                                          {
                                                case "IncompleteCSTPath_Error":
                                                      reply.RETURN.RETURNCODE = "0010221";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Parameters_Error":
                                                      reply.RETURN.RETURNCODE = "0010222";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "File_Error":
                                                      reply.RETURN.RETURNCODE = "0010223";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;

                                                case "Exception_Error":
                                                      reply.RETURN.RETURNCODE = "0010224";
                                                      reply.RETURN.RETURNMESSAGE = strDesc;

                                                      Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] {2}",
                                                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strDesc));
                                                      break;
                                          }
                                    }
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
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
            /// OPI MessageSet: MPLC Interlock Change Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_MPLCInterlockChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  MPLCInterlockChangeRequest command = Spec.XMLtoMessage(xmlDoc) as MPLCInterlockChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("MPLCInterlockChangeReply") as XmlDocument;
                  MPLCInterlockChangeReply reply = Spec.XMLtoMessage(xml_doc) as MPLCInterlockChangeReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.MPLCINTERLOCKNO = command.BODY.MPLCINTERLOCKNO;
                        reply.BODY.MPLCINTERLOCK = command.BODY.MPLCINTERLOCK;
                        reply.BODY.PLCTRX = command.BODY.PLCTRX;

                        eBitResult eb;

                        if (Int32.Parse(command.BODY.MPLCINTERLOCK) == 1)
                              eb = eBitResult.ON;
                        else
                              eb = eBitResult.OFF;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        bool executeCheck = false;

                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010430";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line ({0}) in LineEntity", command.BODY.LINENAME);

                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010431";
                              reply.RETURN.RETURNMESSAGE = "OPI and BCS line name isn't match";
                        }
                        else
                        {
                              //假如EQUIPMENTNO為L02,要把0去掉
                              string eqpNo = command.BODY.EQUIPMENTNO;

                              string keyName = string.Format(Key_MPLCInterlockChangeRequest, eqpNo);
                              if (TimeCheck(keyName))
                              {
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={4}] [BCS <- OPI][{0}] Call MPLCInterlockCommand Set LINENAME ({8}), EQUIPMENTNO ({4}),MPLCINTERLOCKNO ({5}),MPLCINTERLOCK ({6}),PLCTRX ({7}) , {1} {2} {3}", command.HEADER.TRANSACTIONID, command.GetType().Name,
                                    reply.RETURN.RETURNCODE != "0000000" ? "NG" : "OK", reply.RETURN.RETURNMESSAGE,
                                        command.BODY.EQUIPMENTNO, command.BODY.MPLCINTERLOCKNO, command.BODY.MPLCINTERLOCK, command.BODY.PLCTRX, command.BODY.LINENAME));

                                    //Do CoolRunCountSetCommand
                                    Invoke(eServiceName.SubBlockService, "MPLCInterlockCommand", new object[] { eqpNo, command.BODY.PLCTRX, eb });
                                    executeCheck = true;
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010432";
                                    reply.RETURN.RETURNMESSAGE = "Can't repeat execute,please wait for a moment";
                              }
                        }

                        if (!executeCheck)   //被卡掉了
                        {
                              Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={4}] [BCS <- OPI][{0}] Call MPLCInterlockCommand Set LINENAME ({8}), EQUIPMENTNO ({4}),MPLCINTERLOCKNO ({5}),MPLCINTERLOCK ({6}),PLCTRX ({7}) , {1} {2} {3}", command.HEADER.TRANSACTIONID, command.GetType().Name,
                                  reply.RETURN.RETURNCODE != "0000000" ? "NG" : "OK", reply.RETURN.RETURNMESSAGE,
                                  command.BODY.EQUIPMENTNO, command.BODY.MPLCINTERLOCKNO, command.BODY.MPLCINTERLOCK, command.BODY.PLCTRX, command.BODY.LINENAME));
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={2}] [BCS -> OPI][{0}],{1} OK", reply.HEADER.TRANSACTIONID, reply.GetType().Name, command.BODY.EQUIPMENTNO));
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
            /// OPI MessageSet: Equipment Fetch Glass Command
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentFetchGlassCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentFetchGlassCommand command = Spec.XMLtoMessage(xmlDoc) as EquipmentFetchGlassCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentFetchGlassCommandReply") as XmlDocument;
                  EquipmentFetchGlassCommandReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentFetchGlassCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={3}] [BCS <- OPI][{0}] {1} {2},RULENAME1 ({4}),RULEVALUE1 ({5}),RULENAME2 ({6}),RULEVALUE2 ({7})",
                                            command.HEADER.TRANSACTIONID,
                                            command.GetType().Name,
                                            "OK",
                                            command.BODY.EQUIPMENTNO,
                                            command.BODY.RULENAME1,
                                            command.BODY.RULEVALUE1,
                                            command.BODY.RULENAME2,
                                            command.BODY.RULEVALUE2));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        bool execute = true;

                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010290";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line ({0}) in LineEntity", command.BODY.LINENAME);
                              execute = false;
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010291";
                              reply.RETURN.RETURNMESSAGE = "OPI and BCS line name isn't match";
                              execute = false;
                        }
                        else
                        {
                              //假如EQUIPMENTNO為L02,要把0去掉
                              string eqpNo = command.BODY.EQUIPMENTNO;

                              int firstName = 0;
                              int firstValue = 0;
                              int secondName = 0;
                              int secondValue = 0;

                              if (!Int32.TryParse(command.BODY.RULENAME1, out firstName))
                              {
                                    reply.RETURN.RETURNCODE = "0010292";
                                    reply.RETURN.RETURNMESSAGE = "RULENAME1 must be Integer";
                                    execute = false;
                              }

                              if (!Int32.TryParse(command.BODY.RULEVALUE1, out firstValue))
                              {
                                    reply.RETURN.RETURNCODE = "0010293";
                                    reply.RETURN.RETURNMESSAGE = "RULEVALUE1 must be Integer";
                                    execute = false;
                              }

                              if (!Int32.TryParse(command.BODY.RULENAME2, out secondName))
                              {
                                    reply.RETURN.RETURNCODE = "0010294";
                                    reply.RETURN.RETURNMESSAGE = "RULENAME2 must be Integer";
                                    execute = false;
                              }

                              if (!Int32.TryParse(command.BODY.RULEVALUE2, out secondValue))
                              {
                                    reply.RETURN.RETURNCODE = "0010295";
                                    reply.RETURN.RETURNMESSAGE = "RULEVALUE2 must be Integer";
                                    execute = false;
                              }

                              //TODO: Wait ArraySpecialService EquipmentFetchGlassCommand
                              if (execute)
                                  if (lines.Count > 0)
                                  {
                                      if (lines[0].Data.LINETYPE != eLineType.ARRAY.CVD_AKT && lines[0].Data.LINETYPE != eLineType.ARRAY.CVD_ULVAC)
                                      {
                                          Invoke(eServiceName.ArraySpecialService, "EquipmentFetchGlassProportionalRuleCommand",
                                              new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, firstName, firstValue, secondName, secondValue });
                                      }
                                      else
                                      {
                                          if (firstName == secondName)
                                          {
                                              reply.RETURN.RETURNCODE = "0010296";
                                              reply.RETURN.RETURNMESSAGE = "Fetch Proportional Rule_1's Type = Rule_2's Type Error";
                                              execute = false;
                                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] {2}", command.BODY.EQUIPMENTNO, reply.HEADER.TRANSACTIONID, "Fetch Proportional Rule_1 & Rule_2 Type is Same!"));
                                          }
                                          else
                                          {
                                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                                              eqp.File.ProportionalRule01Type = firstName;
                                              eqp.File.ProportionalRule01Value = firstValue;
                                              eqp.File.ProportionalRule02Type = secondName;
                                              eqp.File.ProportionalRule02Value = secondValue;

                                              if (lines[0].Data.LINEID == "TCCVD700")
                                              {
                                                  #region Robot Get Data [hujunpeng Add 20190429，Deng，20190823]
                                                  Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);
                                                  if (robot != null)
                                                  {
                                                      robot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                                      //modify by hujunpeng 20190426 for TCCVD700 混run3支产品
                                                      if (command.BODY.RULENAME1 == "0" && command.BODY.RULENAME2 == "1")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE1)); //Keep Value

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);

                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                                                      }

                                                      if (command.BODY.RULENAME1 == "0" && command.BODY.RULENAME2 == "2")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE1)); //Keep Value

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);

                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                                      }

                                                      if (command.BODY.RULENAME1 == "1" && command.BODY.RULENAME2 == "0")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE1));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);

                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                                                      }

                                                      if (command.BODY.RULENAME1 == "1" && command.BODY.RULENAME2 == "2")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE1));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);

                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                                      }

                                                      if (command.BODY.RULENAME1 == "2" && command.BODY.RULENAME2 == "0")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(command.BODY.RULEVALUE1));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);

                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                                      }

                                                      if (command.BODY.RULENAME1 == "2" && command.BODY.RULENAME2 == "1")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD1);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, int.Parse(command.BODY.RULEVALUE1));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE2));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);

                                                          robot.File.CurCVDProportionalRule.curPorportionalPROD1Count = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                                      }
                                                  }
                                                  ObjectManager.RobotManager.EnqueueSave(robot.File);
                                                  #endregion
                                              }
                                              else {
                                                  #region Robot Get Data [Watson Add 20151014 Add]
                                                  Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);
                                                  if (robot != null)
                                                  {
                                                      robot.File.CurCVDProportionalRule = new CVDProportionalRule();

                                                      if (command.BODY.RULENAME1 == "0")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE1)); //Keep Value

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE2));

                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值

                                                      }
                                                      if (command.BODY.RULENAME1 == "1")
                                                      {
                                                          robot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                                          robot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.MQC);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, int.Parse(command.BODY.RULEVALUE1));

                                                          if (robot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                                              robot.File.CVDProportionalRule.Remove(eCVDIndexRunMode.PROD);
                                                          robot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, int.Parse(command.BODY.RULEVALUE2));

                                                          robot.File.CurCVDProportionalRule.curProportionalMQCCount = int.Parse(command.BODY.RULEVALUE1); //會隨著抽片比例變動的值
                                                          robot.File.CurCVDProportionalRule.curProportionalPRODCount = int.Parse(command.BODY.RULEVALUE2); //會隨著抽片比例變動的值
                                                      }
                                                  }
                                                  ObjectManager.RobotManager.EnqueueSave(robot.File);
                                                  #endregion
                                              }
                                          }
                                      }
                                  }
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={3}] [BCS -> OPI][{0}] {1} {2}", reply.HEADER.TRANSACTIONID, reply.GetType().Name, execute ? "OK" : "NG", command.BODY.EQUIPMENTNO));

                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Glass Grade Mapping Command
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_GlassGradeMappingCommand(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      GlassGradeMappingCommand command = Spec.XMLtoMessage(xmlDoc) as GlassGradeMappingCommand;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("GlassGradeMappingCommandReply") as XmlDocument;
            //      GlassGradeMappingCommandReply reply = Spec.XMLtoMessage(xml_doc) as GlassGradeMappingCommandReply;

            //      try
            //      {
            //            //TODO: Wait ArraySpecialService GlassGradeMappingCommand
            //            reply.BODY.LINENAME = command.BODY.LINENAME;
            //            reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

            //            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                            string.Format("[EQUIPMENT={3}] [BCS <- OPI][{0}] {1} {2} ,OKGLASSGRADE ({4}),NGGLASSGRADE ({5})",
            //                            command.HEADER.TRANSACTIONID,
            //                            command.GetType().Name,
            //                            "OK",
            //                            command.BODY.EQUIPMENTNO,
            //                            command.BODY.OKGLASSGRADE,
            //                            command.BODY.NGGLASSGRADE));

            //            //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //            IList<Line> lines = ObjectManager.LineManager.GetLines();
            //            bool execute = true;

            //            if (lines == null)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010300";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line ({0}) in LineEntity", command.BODY.LINENAME);
            //                  execute = false;
            //            }
            //            else if (command.BODY.LINENAME != ServerName)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010301";
            //                  reply.RETURN.RETURNMESSAGE = "OPI and BCS line name isn't match";
            //                  execute = false;
            //            }
            //            else
            //            {
            //                  //假如EQUIPMENTNO為L02,要把0去掉
            //                  string eqpNo = command.BODY.EQUIPMENTNO;

            //                  string okGrade = command.BODY.OKGLASSGRADE.Trim();
            //                  string ngGrade = command.BODY.NGGLASSGRADE.Trim();

            //                  if (okGrade.Length != 16)
            //                  {
            //                        reply.RETURN.RETURNCODE = "0010302";
            //                        reply.RETURN.RETURNMESSAGE = "OKGLASSGRADE must be 16 char";
            //                        execute = false;
            //                  }
            //                  else
            //                  {
            //                        foreach (var c in okGrade)
            //                        {
            //                              if (c != '0' && c != '1')
            //                              {
            //                                    reply.RETURN.RETURNCODE = "0010303";
            //                                    reply.RETURN.RETURNMESSAGE = "OKGLASSGRADE must be 0 or 1";
            //                                    execute = false;
            //                                    break;
            //                              }
            //                        }
            //                  }

            //                  if (ngGrade.Length != 16)
            //                  {
            //                        reply.RETURN.RETURNCODE = "0010304";
            //                        reply.RETURN.RETURNMESSAGE = "NGGLASSGRADE must be 16 char";
            //                        execute = false;
            //                  }
            //                  else
            //                  {
            //                        foreach (var c in ngGrade)
            //                        {
            //                              if (c != '0' && c != '1')
            //                              {
            //                                    reply.RETURN.RETURNCODE = "0010305";
            //                                    reply.RETURN.RETURNMESSAGE = "NGGLASSGRADE must be 0 or 1";
            //                                    execute = false;
            //                                    break;
            //                              }
            //                        }
            //                  }

            //                  //TODO: Wait ArraySpecialService EquipmentFetchGlassCommand
            //                  if (execute)
            //                        Invoke(eServiceName.ArraySpecialService, "GlassGradeMappingCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, okGrade, ngGrade });
            //            }

            //            xMessage msg = SendReplyToOPI(command, reply);

            //            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                                    string.Format("[EQUIPMENT={3}] [BCS -> OPI][{0}] {1} {2}",
            //                                    reply.HEADER.TRANSACTIONID,
            //                                    reply.GetType().Name, execute ? "OK" : "NG", command.BODY.EQUIPMENTNO));
            //      }
            //      catch (Exception ex)
            //      {
            //            //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //      }
            //}

            private void LineModeChangeTimeout(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        if (Timermanager.IsAliveTimer(tmp))
                        {
                              Timermanager.TerminateTimer(tmp);
                        }
                        string[] sArray = tmp.Split('_');

                        Line line = ObjectManager.LineManager.GetLine(sArray[0]);
                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE ID=[{0}) in LINEENTITY!", sArray[0]));

                        string err = string.Format("CHANGE LINE MODE [{0}] -> [{1}] MES REPLY TIME OUT", line.File.PreHostMode, line.File.OPICtlMode);
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- MES]=[{1}] " + err, sArray[0], sArray[1]));


                        BCSTerminalMessageInform(GetTrxID(""), sArray[0], err);
                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                  }
            }

            /// <summary>
            /// OPI MessageSet: Equipment RunMode Set Command to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentRunModeSetCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentRunModeSetCommand command = Spec.XMLtoMessage(xmlDoc) as EquipmentRunModeSetCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentRunModeSetCommandReply") as XmlDocument;
                  EquipmentRunModeSetCommandReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentRunModeSetCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, COMMAND={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.COMMAND));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        string runMode = string.Empty;
                        string runTrxName = string.Format("{0}_EquipmentRunModeChangeReport", command.BODY.EQUIPMENTNO);
                        Trx runTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { runTrxName }) as Trx;
                        if (runTrx != null)
                        {
                            runTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { runTrxName, false }) as Trx;
                            runMode = runTrx.EventGroups[0].Events[0].Items[0].Value;
                        }
                        else if (eqp != null)
                        {
                            runMode = eqp.File.EquipmentRunMode; //modify by yang 20161227
                        }
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010250";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.COMMAND == runMode)
                        {
                              reply.RETURN.RETURNCODE = "0010252";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI command[{0}] is the same as EquipmentRunMode[{1}]", command.BODY.COMMAND, runMode);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] OPI command({2}) is the same as EquipmentRunMode({3})",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.COMMAND, runMode));
                        }
                        else
                        {
                              // 20141216 Michael : for ILC Special Line 
                              // if (Workbench.LineType == eLineType.ARRAY.ILC) t3 not use cc.kuang 2015/07/03
                              if (false)
                              {
                                    string key = string.Empty;
                                    foreach (KeyValuePair<string, ConstantItem> item in ConstantManager["ARRAY_RUNMODE_ILC"].Values)
                                          if (item.Value.Discription == eqp.File.EquipmentRunMode) key = item.Key;

                                    if (key == command.BODY.COMMAND)
                                    {
                                          reply.RETURN.RETURNCODE = "0010252";
                                          reply.RETURN.RETURNMESSAGE = string.Format("OPI command[{0}] is the same as EquipmentRunMode[{1}]", command.BODY.COMMAND, key);

                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] OPI command({2}) is the same as EquipmentRunMode({3})",
                                              command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.COMMAND, key));
                                    }
                                    else
                                    {
                                          if (command.BODY.COMMAND == "1" || command.BODY.COMMAND == "2")
                                          {
                                                lock (eqp.File) eqp.File.EquipmentRunMode = ConstantManager["ARRAY_RUNMODE_ILC"][command.BODY.COMMAND].Discription;
                                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                string no = eqp.Data.NODENO.Remove(0, 1).PadLeft(2, '0');
                                                Invoke(eServiceName.ArraySpecialService, "FLCModeInUseLocal", new object[]{
                                        no, !eqp.File.EquipmentRunMode.Equals("ILC"), UtilityMethod.GetAgentTrackKey()});
                                          }
                                          Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { eqp.Data.LINEID });

                                          Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                                          if (line != null && line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SAMPLING_MODE)
                                          {
                                                Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest", new object[] { command.HEADER.TRANSACTIONID, eqp.Data.LINEID });
                                          }
                                    }

                              }
                              else
                              {
                                    eReportMode reportMode;
                                    Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);

                                    switch (reportMode)
                                    {
                                          case eReportMode.PLC:
                                          case eReportMode.PLC_HSMS:
                                                # region PLC Type

                                              if (Workbench.LineType.Contains("ELA") && runTrx == null) //modify by yang 20161227
                                              {
                                                  Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Change ELA Backup Mode.",
                                                     "L2", command.HEADER.TRANSACTIONID));
                                                  Invoke("EquipmentService", "LineBackupModeChangeCommand", new object[3] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO,command.BODY.COMMAND });
                                              }
                                              else
                                              {
                                                  Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService EquipmentRunModeSetCommand.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                                  Invoke("EquipmentService", "EquipmentRunModeSetCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.COMMAND });
                                              }
                                                break;

                                                #endregion

                                          case eReportMode.HSMS_CSOT:
                                          case eReportMode.HSMS_PLC:
                                                #region SECS Type

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F119_H_EquipmentModeChangeCommandSend.",
                                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                                /**** eqptmode : NORN/PASS/APAS/2S/2D/4P1/4P2/2O/2Q/4Q/ENG/MQC/ENG/MQC/IGZO/A/B/MIX ***/
                                                // wucc add  20150807 
                                                // NORN = Normal Mode.
                                                //PASS = Pass Mode.
                                                //APAS = Abnormal Pass Mode (Glass data loss)
                                                //MQC = MQC Mode
                                                //MIX = Mix Mode

                                                string strMode = string.Empty;

                                                //if (command.BODY.LINENAME.Substring(0, 5) == "TCCVD")
                                                //{
                                                //    switch (command.BODY.COMMAND)
                                                //    {
                                                //        case "1": strMode = "NORN"; break;
                                                //        case "2": strMode = "MQC"; break;

                                                //    }
                                                //}
                                                //else 
                                                if (command.BODY.LINENAME.Substring(0, 5) == "TCDRY")
                                                {
                                                      switch (command.BODY.COMMAND)
                                                      {
                                                            case "1": strMode = "NORN"; break;
                                                            case "2": strMode = "PASS"; break;
                                                            case "3": strMode = "APAS"; break;
                                                            case "4": strMode = "MQC"; break;
                                                            case "5": strMode = "MIX"; break;
                                                      }
                                                }
                                                Invoke("CSOTSECSService", "TS2F119_H_EquipmentModeChangeCommandSend", new object[] { command.BODY.EQUIPMENTNO, eqp.Data.NODEID, eqp.Data.NODEID, strMode, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          default:

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment report mode({2}) is invalid.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, eqp.Data.REPORTMODE));

                                                reply.RETURN.RETURNCODE = "0010251";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Equipment report mode({0}) is invalid", eqp.Data.REPORTMODE);
                                                break;

                                                #endregion
                                    }
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        if (eqp != null)
                              EquipmentStatusReport(command.HEADER.TRANSACTIONID, eqp);
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
            /// OPI MessageSet: Indexer Operation Mode Change Request
            /// </summary>
            public void OPI_IndexerOperationModeChangeRequest(XmlDocument xmlDoc)
            {
                  IndexerOperationModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as IndexerOperationModeChangeRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("IndexerOperationModeChangeReply") as XmlDocument;
                  IndexerOperationModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as IndexerOperationModeChangeReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call EquipmentService.IndexerOperationModeChangeCommand {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),EQUIPMENTNO({1}),INDEXEROPERATIONMODE({2})", command.BODY.LINENAME, command.BODY.EQUIPMENTNO, command.BODY.INDEXEROPERATIONMODE)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010570";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010571";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              string keyName = string.Format(Key_IndexerOperationModeChange, command.BODY.EQUIPMENTNO);
                              if (TimeCheck(keyName))
                              {
                                    Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService IndexerOperationModeChangeCommand.",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                    //Do IndexerOperationModeChange
                                    Invoke(eServiceName.EquipmentService, "IndexerOperationModeChangeCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.INDEXEROPERATIONMODE });
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010572";
                                    reply.RETURN.RETURNMESSAGE = "Can not repeat execute,please wait for a moment";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can not repeat execute,please wait for a moment",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call EquipmentService.IndexerOperationModeChangeCommand OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: VCR Status Change Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_VCRStatusChangeRequest(XmlDocument xmlDoc)
            {
                  VCRStatusChangeRequest command = Spec.XMLtoMessage(xmlDoc) as VCRStatusChangeRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("VCRStatusChangeReply") as XmlDocument;
                  VCRStatusChangeReply reply = Spec.XMLtoMessage(xml_doc) as VCRStatusChangeReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.VCRNO = command.BODY.VCRNO;
                        reply.BODY.VCRMODE = command.BODY.VCRMODE;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call VCRService.VCRModeChangeCommand {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),EQUIPMENTNO({1}),VCRNO({2}),VCRMODE({3})",
                            command.BODY.LINENAME, command.BODY.EQUIPMENTNO, command.BODY.VCRNO, command.BODY.VCRMODE)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010590";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010591";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI[{0}]/BCS[{2}]",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              string keyName = string.Format(Key_VCRStatusChange, command.BODY.EQUIPMENTNO);
                              if (TimeCheck(keyName))
                              {
                                    Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke VCRService VCRModeChangeCommand.",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                    //Do VCRModeChangeCommand
                                    Invoke(eServiceName.VCRService, "VCRModeChangeCommand",
                                        new object[] { command.BODY.EQUIPMENTNO, (eBitResult)Enum.Parse(typeof(eBitResult), 
                                command.BODY.VCRMODE), command.BODY.VCRNO, command.HEADER.TRANSACTIONID });
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010592";
                                    reply.RETURN.RETURNMESSAGE = "Can't repeat execute,please wait for a moment";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't repeat execute,please wait for a moment",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call VCRService.VCRModeChangeCommand OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: Cool Run Set Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CoolRunSetRequest(XmlDocument xmlDoc)
            {
                  CoolRunSetRequest command = Spec.XMLtoMessage(xmlDoc) as CoolRunSetRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("CoolRunSetReply") as XmlDocument;
                  CoolRunSetReply reply = Spec.XMLtoMessage(xml_doc) as CoolRunSetReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call EquipmentService.CoolRunCountSetCommand {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),COOLRUNCOUNT({1}))", command.BODY.LINENAME, command.BODY.COOLRUNCOUNT)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010580";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010581";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                            Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                            string eqpNO = string.Empty;
                            if (line.Data.LINEID == "FCUPK100")
                                eqpNO = "L5";//UPK LINE的Indexer在L5
                            else
                                eqpNO = "L2";//f固定傳L2
                              string keyName = string.Format(Key_CoolRunSet, eqpNO);
                              if (TimeCheck(keyName))
                              {
                                    Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Invoke EquipmentService CoolRunCountSetCommand.",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                                    //Do CoolRunCountSetCommand
                                    Invoke(eServiceName.EquipmentService, "CoolRunCountSetCommand", new object[] { eqpNO, int.Parse(command.BODY.COOLRUNCOUNT), command.HEADER.TRANSACTIONID });
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010582";
                                    reply.RETURN.RETURNMESSAGE = "Can't repeat execute,please wait for a moment";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't repeat execute,please wait for a moment",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call EquipmentService.CoolRunCountSetCommand OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: Cassette On Port QTime Request
            /// </summary>
            public void OPI_CassetteOnPortQTimeRequest(XmlDocument xmlDoc)
            {
                  CassetteOnPortQTimeRequest command = Spec.XMLtoMessage(xmlDoc) as CassetteOnPortQTimeRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("CassetteOnPortQTimeReply") as XmlDocument;
                  CassetteOnPortQTimeReply reply = Spec.XMLtoMessage(xml_doc) as CassetteOnPortQTimeReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.OPERATORID = command.BODY.OPERATORID;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call CassetteService.CassetteonPortQTimeChangeCommand {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),OPERATORID({1}),EQUIPMENTNO({2}),QTIME({3})",
                            command.BODY.LINENAME, command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.QTIME)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010510";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010511";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              string keyName = string.Format(Key_CassetteOnPortQTime, command.BODY.EQUIPMENTNO);
                              if (TimeCheck(keyName))
                              {
                                    Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteOnPortQTimeChangeCommand.",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                                    //Do CassetteonPortQTimeChangeCommand
                                    Invoke(eServiceName.CassetteService, "CassetteonPortQTimeChangeCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, int.Parse(command.BODY.QTIME) });
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010512";
                                    reply.RETURN.RETURNMESSAGE = "Can't repeat execute,please wait for a moment";

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't repeat execute,please wait for a moment",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call CassetteService.CassetteonPortQTimeChangeCommand OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: BC Control Command to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_BCControlCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  BCControlCommand command = Spec.XMLtoMessage(xmlDoc) as BCControlCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("BCControlCommandReply") as XmlDocument;
                  BCControlCommandReply reply = Spec.XMLtoMessage(xml_doc) as BCControlCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, UNITNO={4}, COMMANDTYPE={5}, COMMAND={6}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.UNITNO, command.BODY.COMMANDTYPE, command.BODY.COMMAND));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.UNITNO = command.BODY.UNITNO;

                      //20171226 huangjiayin
                        #region CellSpecialCheck
                        if (command.BODY.COMMANDTYPE.Split(':').Length > 1)
                        {
                            if (command.BODY.COMMANDTYPE.Split(':')[0] == "CellSpecialCheck")
                            {
                                //Cell Special Check Function:
                                string _para = command.BODY.COMMANDTYPE.Split(':')[1];
                                //string _rtn = string.Empty;

                                switch (command.BODY.COMMAND)
                                {
                                    case "Request":
                                        if (ParameterManager.ContainsKey(_para))
                                        {
                                            reply.BODY.EQUIPMENTNO = _para + ";" + ParameterManager[_para].Discription + ";" + (ParameterManager[_para].GetBoolean() ? "Enable" : "Disable");
                                        }
                                        break;
                                    case "Set:Enable":
                                        if (ParameterManager.ContainsKey(_para))
                                        {
                                            ParameterManager[_para].Value = true;
                                        }
                                        break;
                                    case "Set:Disable":
                                        if (ParameterManager.ContainsKey(_para))
                                        {
                                            ParameterManager[_para].Value = false;
                                        }
                                        break;
                                    default:
                                        break;
                                }

                               // reply.RETURN.RETURNCODE = "0";
                                //reply.RETURN.RETURNMESSAGE = _rtn;
                                xMessage _msg = SendReplyToOPI(command, reply);
                                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                       string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                       command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                return;
                            }
                        }
                        #endregion

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010260";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (string.IsNullOrEmpty(command.BODY.COMMANDTYPE) || string.IsNullOrEmpty(command.BODY.COMMAND))
                        {
                              reply.RETURN.RETURNCODE = "0010261";
                              reply.RETURN.RETURNMESSAGE = string.Format("BC Control Command Invalid. CommandType({1})/Command({2})",
                                  command.BODY.COMMANDTYPE, command.BODY.COMMAND);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] BC Control Command Invalid. CommandType({2})/Command({3})",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.COMMANDTYPE, command.BODY.COMMAND));
                        }
                        else
                        {
                              if (eqp.Data.REPORTMODE == "HSMS_CSOT")
                              {
                                    string cmd = string.Empty;
                                    //需要建立一個空的dataList給SECSService，不可為Null
                                    List<Tuple<string, string>> dataList = new List<Tuple<string, string>>();
                                    dataList.Add(Tuple.Create("", ""));

                                    switch (command.BODY.COMMANDTYPE)
                                    {
                                          case "PROCESSPAUSE":
                                                if (command.BODY.COMMAND == "1")
                                                {
                                                      cmd = "PAUSE";
                                                      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F41_H_HostCommandSend({2}).",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, cmd));
                                                      Invoke("CSOTSECSService", "TS2F41_H_HostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, cmd, dataList, "OPI", command.HEADER.TRANSACTIONID });
                                                }
                                                else
                                                {
                                                      cmd = "RESUME";
                                                      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F41_H_HostCommandSend({2}).",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, cmd));
                                                      Invoke("CSOTSECSService", "TS2F41_H_HostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, cmd, dataList, "OPI", command.HEADER.TRANSACTIONID });
                                                }
                                                break;

                                          case "PROCESSSTOP":
                                                if (command.BODY.COMMAND == "2")
                                                {
                                                      cmd = "RUN";
                                                      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F41_H_HostCommandSend({2}).",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, cmd));
                                                      Invoke("CSOTSECSService", "TS2F41_H_HostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, cmd, dataList, "OPI", command.HEADER.TRANSACTIONID });
                                                }
                                                else
                                                      goto default;
                                                break;

                                          default:
                                                reply.RETURN.RETURNCODE = "0010262";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Equipment({0}) DO NOT support this command by SECS.", command.BODY.EQUIPMENTNO);// wucc  20150729 補參數 command.BODY.EQUIPMENTNO

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment DO NOT support this command by SECS.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                break;
                                    }
                              }
                              //20150413 cy:增加Nikon的command
                              else if (eqp.Data.REPORTMODE == "HSMS_NIKON")
                              {
                                    switch (command.BODY.COMMANDTYPE)
                                    {
                                          case "PROCESSSTOP":
                                                if (command.BODY.COMMAND == "1") //STOP
                                                {
                                                      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F41_STOP_H_STOPHostCommandSend.",
                                                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                      Invoke("NikonSECSService", "TS2F41_STOP_H_STOPHostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                }
                                                else
                                                      goto default;
                                                break;

                                          default:
                                                reply.RETURN.RETURNCODE = "0010262";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Equipment({0}) DO NOT support this command by SECS.");

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment DO NOT support this command by SECS.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                break;
                                    }
                              }
                              else
                              {
                                    switch (command.BODY.COMMANDTYPE)
                                    {
                                          case "PROCESSPAUSE":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService ProcessPauseCommand.",
                                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("EquipmentService", "ProcessPauseCommand", new object[] { command.BODY.EQUIPMENTNO, command.BODY.COMMAND, command.BODY.UNITNO, command.HEADER.TRANSACTIONID });
                                                break;
                                          case "TRANSFERSTOP":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService TransferStopCommand.",
                                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("EquipmentService", "TransferStopCommand", new object[] { command.BODY.EQUIPMENTNO, command.BODY.COMMAND, command.HEADER.TRANSACTIONID });
                                                break;
                                          case "PROCESSSTOP":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService ProcessStopCommand.",
                                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("EquipmentService", "ProcessStopCommand", new object[] { command.BODY.EQUIPMENTNO, command.BODY.UNITNO, command.BODY.COMMAND, command.HEADER.TRANSACTIONID });
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
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }
            public void OPI_MaterialStatusRequest(XmlDocument xmlDoc)//add by hujunpeng 20180523
            {
                IServerAgent agent = GetServerAgent();
                MaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as MaterialStatusRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialStatusReply") as XmlDocument;
                MaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as MaterialStatusReply;
                try
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, COMMAND={4}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                        command.BODY.EQUIPMENTNO, command.BODY.COMMAND));

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                    Equipment eqp2 = ObjectManager.EquipmentManager.GetEQP("L4");
                    if (eqp == null)
                    {
                        reply.RETURN.RETURNCODE = "0010260";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                    }
                    else
                    {
                        #region

                        Invoke("EquipmentService", "MaterialStatusRequest", new object[] { command.BODY.EQUIPMENTNO, command.BODY.COMMAND, command.HEADER.TRANSACTIONID });

                        #endregion
                    }
                    Thread.Sleep(1000);

                    if (!string.IsNullOrEmpty(eqp.Data.NODENAME.ToString()) || !string.IsNullOrEmpty(eqp2.Data.NODENAME.ToString()))
                    {
                        if (eqp.Data.NODENAME.Count() != 30)
                        {
                            NLogManager.Logger.LogErrorWrite("nodename", "UIService", "OPI_MaterialStatusRequest", "PRID1不是30码");
                            eqp.Data.NODENAME = "000000000000000000000000000000";
                            ObjectManager.EquipmentManager.UpdateDB(eqp.Data);
                        }
                        if (eqp2.Data.NODENAME.Count() != 30)
                        {
                            NLogManager.Logger.LogErrorWrite("nodename", "UIService", "OPI_MaterialStatusRequest", "PRID2不是30码");
                            eqp2.Data.NODENAME = "000000000000000000000000000000";
                            ObjectManager.EquipmentManager.UpdateDB(eqp2.Data);
                        }
                        if (eqp.Data.NODENAME.Substring(24, 1) == "I")
                        {
                            reply.BODY.MATERIALSTATUS = eqp.Data.NODENAME.Substring(24, 1);
                            reply.BODY.MATERIALVALUE = eqp.Data.NODENAME.Substring(25, 5);
                            reply.BODY.MATERIALNAME = eqp.Data.NODENAME.Substring(0, 23);
                            reply.BODY.SLOTNO = "1";
                        }
                        else if (eqp2.Data.NODENAME.Substring(24, 1) == "I")
                        {

                            reply.BODY.MATERIALSTATUS = eqp2.Data.NODENAME.Substring(24, 1);
                            reply.BODY.MATERIALVALUE = eqp2.Data.NODENAME.Substring(25, 5);
                            reply.BODY.MATERIALNAME = eqp2.Data.NODENAME.Substring(0, 23);
                            reply.BODY.SLOTNO = "2";
                        }
                        else
                        {
                            reply.BODY.MATERIALSTATUS = string.Empty;
                            reply.BODY.MATERIALVALUE = string.Empty;
                            reply.BODY.MATERIALNAME = string.Empty;
                            reply.BODY.SLOTNO = "0";
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
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                }
            }

            /// <summary>
            /// OPI MessageSet: MES Alive Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_MESAliveRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  MESAliveRequest command = Spec.XMLtoMessage(xmlDoc) as MESAliveRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("MESAliveReply") as XmlDocument;
                  MESAliveReply reply = Spec.XMLtoMessage(xml_doc) as MESAliveReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //已MESAgent狀態回報
                        IServerAgent mesAgent = GetServerAgent("MESAgent");
                        reply.BODY.ALIVE = mesAgent.AgentStatus.ToString() == "RUN" ? "ON" : "OFF";

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
            /// OPI MessageSet: SECS Function Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_SECSFunctionRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  SECSFunctionRequest command = Spec.XMLtoMessage(xmlDoc) as SECSFunctionRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("SECSFunctionReply") as XmlDocument;
                  SECSFunctionReply reply = Spec.XMLtoMessage(xml_doc) as SECSFunctionReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, SECSNAME={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.SECSNAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.SECSNAME = command.BODY.SECSNAME;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010440";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Cant find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (!eqp.HsmsSelected)
                        {
                              reply.RETURN.RETURNCODE = "0010441";
                              reply.RETURN.RETURNMESSAGE = string.Format("Equipment[{0}] HSMSSelected is False", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment({0}) HSMSSelected is False.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              List<SECSFunctionRequest.PARAMETERc> parameter = command.BODY.PARAMETERLIST;

                              //先判斷機台上報格式種類
                              if (eqp.Data.REPORTMODE != "HSMS_NIKON")
                              {
                                    #region HSMS_CSOT SECS
                                    switch (command.BODY.SECSNAME)
                                    {
                                          case "S1F1":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS1F1_H_AreYouThereRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS1F1_H_AreYouThereRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F5":
                                                string sfcd = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "SFCD"; }).VALUE;
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS1F5_H_FormattedStatusRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, sfcd, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F13":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS1F13_H_EstablishCommunicationsRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS1F13_H_EstablishCommunicationsRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F15":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS1F15_H_OfflineRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS1F15_H_OfflineRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F17":
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS1F17_H_OnlineRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS1F17_H_OnlineRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F15":
                                                string bcid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECID"; }).VALUE;
                                                string ecv = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECV"; }).VALUE;
                                                List<Tuple<string, string>> tuple = new List<Tuple<string, string>>();
                                                tuple.Add(Tuple.Create(bcid, ecv));
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F15_H_NewEquipmentConstantSend.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F15_H_NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, tuple, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F19":
                                                string functionName;
                                                string subFunctionCode;
                                                string useFlag;
                                                string reportFrequency;
                                                if (parameter.Where(p => p.VALUE.Contains("_")).Count() > 0)  //yang 20161129
                                                { 
                                                    string trid=parameter.Where(p=>p.VALUE.Contains("_")).FirstOrDefault().VALUE.ToString();
                                                    functionName = trid.Split('_')[0];
                                                    subFunctionCode = trid.Split('_')[1];
                                                    useFlag = "0";
                                                    string datatype = parameter.FirstOrDefault().NAME;
                                                    reportFrequency="3600000";
                                                    if (datatype.ToUpper().Contains("APC"))
                                                    {
                                                        if (eqp.Data.NODEID.ToUpper().Contains("TCPDN"))  //add by qiumin20171123 ,for DNS APC 默认上报频率为5s
                                                        {
                                                            reportFrequency = (eqp.File.APCNormalIntervalMSForID < 5000) ? "5000" : eqp.File.APCNormalIntervalMS.ToString();
                                                        }
                                                        else
                                                        {
                                                            reportFrequency = (eqp.File.APCNormalIntervalMSForID == 0) ? "1000" : eqp.File.APCNormalIntervalMS.ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        reportFrequency = (eqp.File.SpecialDataIntervalMS == 0) ? "3600000" : eqp.File.SpecialDataIntervalMS.ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    functionName = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "FUNCTIONNAME"; }).VALUE;
                                                    subFunctionCode = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "SUBFUNCTIONCODE"; }).VALUE;
                                                    useFlag = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "USEFLAG"; }).VALUE;
                                                    reportFrequency = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "REPORTFREQUENCY"; }).VALUE;
                                                    //string dataName = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "DATANAME"; }).VALUE;
                                                }
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F19_H_DataSetCommand.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F19_H_DataSetCommand", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, functionName, subFunctionCode, useFlag, reportFrequency, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F21":
                                                string functionName1;
                                                string subFunctionCode1;
                                                string useFlag1;
                                                string reportFrequency1;
                                                if (parameter.Where(p => p.VALUE.Contains("_")).Count() > 0) //yang 20161129
                                                {
                                                    string trid = parameter.Where(p => p.VALUE.Contains("_")).FirstOrDefault().VALUE.ToString();
                                                    functionName1 = trid.Split('_')[0];
                                                    subFunctionCode1 = trid.Split('_')[1];
                                                    useFlag1 = "0";
                                                    string datatype = parameter.FirstOrDefault().NAME;
                                                    reportFrequency1 = "3600000";
                                                    if (datatype.ToUpper().Contains("APC"))
                                                    {
                                                        if (eqp.Data.NODEID.ToUpper().Contains("TCPDN"))  //add by qiumin20171123 ,for DNS APC 默认上报频率为5s
                                                        {
                                                            reportFrequency = (eqp.File.APCNormalIntervalMSForID < 5000) ? "5000" : eqp.File.APCNormalIntervalMS.ToString();
                                                        }
                                                        else
                                                        {
                                                            reportFrequency = (eqp.File.APCNormalIntervalMSForID == 0) ? "1000" : eqp.File.APCNormalIntervalMS.ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        reportFrequency = (eqp.File.SpecialDataIntervalMS == 0) ? "3600000" : eqp.File.SpecialDataIntervalMS.ToString();
                                                    }
                                                }

                                                else
                                                {
                                                    functionName1 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "FUNCTIONNAME"; }).VALUE;
                                                    subFunctionCode1 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "SUBFUNCTIONCODE"; }).VALUE;
                                                    useFlag1 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "USEFLAG"; }).VALUE;
                                                    reportFrequency1 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "REPORTFREQUENCY"; }).VALUE;
                                                    //string dataID = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "DATAID"; }).VALUE;
                                                }
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F21_H_DataSetCommandforID.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F21_H_DataSetCommandforID", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, functionName1, subFunctionCode1, useFlag1, reportFrequency1, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F23":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F23_H_DataItemMappingTableRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F23_H_DataItemMappingTableRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F25":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F25_H_LoopbackDiagnosticRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F25_H_LoopbackDiagnosticRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F29":
                                                string ecid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECID"; }).VALUE;

                                                List<string> ecidList = new List<string>();
                                                if (ecid != string.Empty)
                                                      ecidList = ecid.Split(',').ToList();
                                                else
                                                      ecidList = null;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F29_H_EquipmentConstantNamlistRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F29_H_EquipmentConstantNamlistRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ecidList, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F31":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F31_H_DateandTimeSetRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F31_H_DateandTimeSetRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F41":
                                                string rcmd = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "RCMD"; }).VALUE;
                                                //string cpName = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "CPNAME"; }).VALUE;
                                                //string cpVal = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "CPVAL"; }).VALUE;

                                                List<Tuple<string, string>> dataList3 = new List<Tuple<string, string>>();
                                                //dataList3.Add(Tuple.Create(cpName, cpVal));
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F41_H_HostCommandSend.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS2F41_H_HostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, rcmd, dataList3, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          //20150119 cy mark: 透過OPI Force Clean Out的功能即可
                                          //case "S2F111":
                                          //    string cleanOut = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "CLEANOUT"; }).VALUE;

                                          //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F111_H_ForcedCleanOutCommandSend.",
                                          //    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                          //    Invoke("CSOTSECSService", "TS2F111_H_ForcedCleanOutCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, eqp.Data.NODEID, cleanOut, "OPI" });
                                          //    break;

                                          //20150119 cy mark: 透過OPI Eqp Mode Change的功能即可
                                          //case "S2F119":
                                          //    string eqpMode = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "EQPMODE"; }).VALUE;

                                          //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS2F119_H_EquipmentModeChangeCommandSend.",
                                          //    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                          //    Invoke("CSOTSECSService", "TS2F119_H_EquipmentModeChangeCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, eqp.Data.NODEID, eqpMode, "OPI" });
                                          //    break;

                                          case "S5F5":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS5F5_H_ListAlarmRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS5F5_H_ListAlarmRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S7F19":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS7F19_H_CurrentEPPDRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS7F19_H_CurrentEPPDRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S7F25":
                                                string ppid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "PPID"; }).VALUE;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS7F25_H_FormattedProcessProgramRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("CSOTSECSService", "TS7F25_H_FormattedProcessProgramRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ppid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          //20150119 cy:mark -- 透過OPI原本的cim message功能即可
                                          //case "S10F3":
                                          //    string text = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "TEXT"; }).VALUE;
                                          //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CSOTSECSService TS10F3_H_TerminalDisplaySingle.",
                                          //    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                          //    Invoke("CSOTSECSService", "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, text, command.HEADER.TRANSACTIONID, "OPI" });
                                          //    break;

                                          case "S64F1":
                                                //TODO: SECSService尚未處理
                                                break;
                                    }
                                    #endregion
                              }
                              else
                              {
                                    #region HSMS_NIKON SECS
                                    switch (command.BODY.SECSNAME)
                                    {
                                          case "S1F1":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS1F1_H_AreYouThereRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS1F1_H_AreYouThereRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F3":
                                                string strSVID = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "SVID"; }).VALUE;
                                                uint svid = uint.Parse(strSVID);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS1F3_H_SelectedEquipmentStatusRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, svid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F13":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS1F13_H_EstablishCommunicationsRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS1F13_H_EstablishCommunicationsRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F15":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS1F15_H_RequestOffLine.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS1F15_H_RequestOffLine", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S1F17":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS1F17_H_RequestOnLine.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS1F17_H_RequestOnLine", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F13":
                                                uint ecid = uint.Parse(parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECID"; }).VALUE);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F13_H_EquipmentConstantRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F13_H_EquipmentConstantRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ecid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F15":
                                                string ecid1 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECID"; }).VALUE;
                                                string ecv = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECV"; }).VALUE;

                                                if (string.IsNullOrEmpty(ecv))
                                                {
                                                      reply.RETURN.RETURNCODE = "0010442";
                                                      reply.RETURN.RETURNMESSAGE = string.Format("Parameter({0}) value is invalid.", "ECV");

                                                      Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] {2}",
                                                          command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, reply.RETURN.RETURNMESSAGE));
                                                      break;
                                                }
                                                switch (ecid1)
                                                {
                                                      case "501":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_501_H_ECID501NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_501_H_ECID501NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "503":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_503_H_ECID503NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_503_H_ECID503NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ushort.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "505":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_505_H_ECID505NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_505_H_ECID505NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "506":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_506_H_ECID506NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_506_H_ECID506NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "507":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_507_H_ECID507NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_507_H_ECID507NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, bool.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "508":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_508_H_ECID508NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_508_H_ECID508NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "509":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_509_H_ECID509NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_509_H_ECID509NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "510":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F15_510_H_ECID510NewEquipmentConstantSend.",
                                                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F15_510_H_ECID510NewEquipmentConstantSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, uint.Parse(ecv), "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                }
                                                break;

                                          case "S2F17":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F17_H_DateandTimeRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F17_H_DateandTimeRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F23":
                                               string trid;
                                               string dsper;
                                               string totsmp;
                                                if(parameter.Count()<=1)
                                                {
                                                    trid = parameter.FirstOrDefault().VALUE;
                                                    if (trid.Contains("APC")) dsper="000001";
                                                    else dsper="010000";
                                                    totsmp ="-1";
                                                }
                                                else
                                                {
                                                 trid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "TRID"; }).VALUE;
                                                 dsper = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "DSPER"; }).VALUE;
                                                 totsmp = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "TOTSMP"; }).VALUE;
                                                //string svid2 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "SVID"; }).VALUE;
                                                }
                                                //List<string> svidList = new List<string>();
                                                //if (svid2 != string.Empty)
                                                //    svidList = svid2.Split(',').ToList();
                                                //else
                                                //    svidList = null;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F23_H_TraceInitializeSend.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F23_H_TraceInitializeSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trid, dsper, totsmp, "1", "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F25":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F25_H_LoopbackDiagnosticRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F25_H_LoopbackDiagnosticRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F29":
                                                uint ecid2 = uint.Parse(parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ECID"; }).VALUE);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F29_H_EquipmentConstantNamelistRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F29_H_EquipmentConstantNamelistRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ecid2, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F31":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F31_H_DateandTimeSetRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS2F31_H_DateandTimeSetRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S2F41":
                                                string rcmd = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "RCMD"; }).VALUE.Trim();
                                                switch (rcmd)
                                                {
                                                      case "STOP":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F41_STOP_H_STOPHostCommandSend.",
                                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F41_STOP_H_STOPHostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                      case "ABORT":
                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS2F41_ABORT_H_ABORTHostCommandSend.",
                                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                            Invoke("NikonSECSService", "TS2F41_ABORT_H_ABORTHostCommandSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                            break;
                                                }

                                                break;

                                          case "S5F3":
                                                bool aled = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ALED"; }).VALUE == "Enable" ? true : false;
                                                string alid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ALID"; }).VALUE;

                                                List<uint> alidList = new List<uint>();
                                                string[] alids;
                                                if (alid != string.Empty)
                                                {
                                                      alids = alid.Split(',');
                                                      for (int i = 0; i < alids.Length; i++)
                                                      {
                                                            alidList.Add(uint.Parse(alids[i]));
                                                      }
                                                }
                                                else
                                                      alidList = null;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS5F3_H_EnableDisableAlarmSen.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS5F3_H_EnableDisableAlarmSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, aled, alidList, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S5F5":
                                                string alid2 = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "ALID"; }).VALUE;

                                                string[] alids2;
                                                uint[] alids3;
                                                if (alid2 != string.Empty)
                                                {
                                                      alids2 = alid2.Split(',');
                                                      alids3 = new uint[alids2.Length];
                                                      for (int i = 0; i < alids2.Length; i++)
                                                      {
                                                            alids3[i] = uint.Parse(alids2[i]);
                                                      }
                                                }
                                                else
                                                      alids3 = null;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS5F5_H_ListAlarmsRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS5F5_H_ListAlarmsRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, alids3, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S6F15":
                                                uint ceid = uint.Parse(parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "CEID"; }).VALUE);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS6F15_H_EventReportRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS6F15_H_EventReportRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ceid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S6F19":
                                                uint rptid = uint.Parse(parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "RPTID"; }).VALUE);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS6F19_H_IndividualReportRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS6F19_H_IndividualReportRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, rptid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S6F23":
                                                byte rsdc = byte.Parse(parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "RSDC"; }).VALUE);

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS6F23_H_RequestSpooledData.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS6F23_H_RequestSpooledData", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, rsdc, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          case "S7F19":
                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS7F19_H_CurrentEPPDRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS7F19_H_CurrentEPPDRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI", command.HEADER.TRANSACTIONID });
                                                break;

                                          //20150119 cy:目前不提供
                                          //case "S7F23":
                                          //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS7F23_H_FormattedProcessProgramSend.",
                                          //    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                          //    Invoke("NikonSECSService", "TS7F23_H_FormattedProcessProgramSend", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "OPI" });
                                          //    break;

                                          case "S7F25":
                                                string ppid = parameter.Find(delegate(SECSFunctionRequest.PARAMETERc p) { return p.NAME == "PPID"; }).VALUE;

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke NikonSECSService TS7F25_H_FormattedProcessProgramRequest.",
                                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                                Invoke("NikonSECSService", "TS7F25_H_FormattedProcessProgramRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, ppid, "OPI", command.HEADER.TRANSACTIONID });
                                                break;
                                    }
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
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Inspection Idle Time Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_InspectionIdleTimeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  InspectionIdleTimeRequest command = Spec.XMLtoMessage(xmlDoc) as InspectionIdleTimeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("InspectionIdleTimeReply") as XmlDocument;
                  InspectionIdleTimeReply reply = Spec.XMLtoMessage(xml_doc) as InspectionIdleTimeReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, EQUIPMENTNO={4}, IDLETIME={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.IDLETIME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.OPERATORID = command.BODY.OPERATORID;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010530";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                            if (eqp.Data.NODEATTRIBUTE.Equals("IN"))
                            {
                                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService InspectionIdleTimeSettingCommand.",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                //轉拋給EquipmentService處理
                                Invoke("EquipmentService", "InspectionIdleTimeSettingCommand", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, int.Parse(command.BODY.IDLETIME) });
                            }
                            else if (eqp.Data.NODEID.Contains("TC") && eqp.Data.NODENO != "L2")  // add by qiumin 20180106 Array opi add EqProcessTimeSetting function ,both with inspection ilde time set use same button
                            {
                                eqp.File.EqProcessTimeSetting = int.Parse(command.BODY.IDLETIME);

                                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] EqProcessTimeSetting to[{2}] .",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.IDLETIME));
                            }
                            else if(eqp.Data.NODEID.Contains("CCSDP")||eqp.Data.NODEID.Contains("CCLCD")||eqp.Data.NODEID.Contains("CCSLI"))//ADD BY hujunpeng 20181207 odf opi add eqprocesstimesetting function
                            {
                                eqp.File.EqProcessTimeSetting = int.Parse(command.BODY.IDLETIME);

                                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] EqProcessTimeSetting to[{2}] .",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.IDLETIME));
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
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Recipe Register Validation Command Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_RecipeRegisterValidationCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  RecipeRegisterValidationCommandRequest command = Spec.XMLtoMessage(xmlDoc) as RecipeRegisterValidationCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("RecipeRegisterValidationCommandReply") as XmlDocument;
                  RecipeRegisterValidationCommandReply reply = Spec.XMLtoMessage(xml_doc) as RecipeRegisterValidationCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PORTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.PORTNO = command.BODY.PORTNO;

                        bool bCheck = (bool)Invoke(eServiceName.RecipeService, "CheckRecipeRegisterValidationCommand", new object[] { });
                        bool bResult = false;

                        //Dictionary<string, IList<RecipeCheckInfo>> dicRecipe = new Dictionary<string, IList<RecipeCheckInfo>>();
                        Dictionary<string, Dictionary<string, IList<RecipeCheckInfo>>> dicLineRecipe = new Dictionary<string, Dictionary<string, IList<RecipeCheckInfo>>>();
                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010650";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (!bCheck)
                        {
                              reply.RETURN.RETURNCODE = "0010652";
                              reply.RETURN.RETURNMESSAGE = "RecipeService is busy, please wait for a while and retry";

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] RecipeService is busy, please wait for a while and retry",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        //else
                        //{
                        //string trxID = command.HEADER.TRANSACTIONID;
                        //int iPortNo = command.BODY.PORTNO != "0" ? int.Parse(command.BODY.PORTNO.Substring(1)) : 0;

                        //foreach (RecipeRegisterValidationCommandRequest.LINEc _line in command.BODY.LINELIST)
                        //{
                        //    foreach (RecipeRegisterValidationCommandRequest.RECIPECHECKc recipe in _line.RECIPECHECKLIST)
                        //    {
                        //        foreach (RecipeRegisterValidationCommandRequest.EQUIPMENTc eqp in recipe.EQUIPMENTLIST)
                        //        {
                        //            RecipeCheckInfo rci = new RecipeCheckInfo(eqp.EQUIPMENTNO, 2, iPortNo, eqp.RECIPENO);
                        //            rci.RecipeID = eqp.RECIPENO;

                        //            if (!dicRecipe.ContainsKey(recipe.RECIPENAME))
                        //                dicRecipe.Add(recipe.RECIPENAME, new List<RecipeCheckInfo> { rci });
                        //            else
                        //                dicRecipe[recipe.RECIPENAME].Add(rci);
                        //        }

                        //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService RecipeRegisterValidationCommandFromUI",
                        //                command.BODY.LINENAME, trxID));

                        //        //Recipe Register Validation Command download to EQ
                        //        bResult = (bool)Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromUI", new object[] { trxID, dicRecipe[recipe.RECIPENAME] });
                        //    }

                        //    dicLineRecipe.Add(_line.RECIPELINENAME, dicRecipe);

                        //    if (!bResult)
                        //    {
                        //        reply.RETURN.RETURNCODE = "0010651";
                        //        reply.RETURN.RETURNMESSAGE = "RecipeRegisterValidationCommandFromUI return NG";

                        //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService RecipeRegisterValidationCommandFromUI return NG.",
                        //        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        //    }
                        //}
                        //}

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                        //RecipeRegisterValidationReturnReport處理
                        if (bCheck && reply.RETURN.RETURNCODE == "0000000")
                        {
                              string trxID = command.HEADER.TRANSACTIONID;
                              int iPortNo = command.BODY.PORTNO != "0" ? int.Parse(command.BODY.PORTNO.Substring(1)) : 0;

                              foreach (RecipeRegisterValidationCommandRequest.LINEc _line in command.BODY.LINELIST)
                              {
                                    Dictionary<string, IList<RecipeCheckInfo>> dicRecipe = new Dictionary<string, IList<RecipeCheckInfo>>();

                                    foreach (RecipeRegisterValidationCommandRequest.RECIPECHECKc recipe in _line.RECIPECHECKLIST)
                                    {
                                          foreach (RecipeRegisterValidationCommandRequest.EQUIPMENTc eqp in recipe.EQUIPMENTLIST)
                                          {
                                                RecipeCheckInfo rci = new RecipeCheckInfo(eqp.EQUIPMENTNO, 2, iPortNo, eqp.RECIPENO);

                                                foreach (Equipment node in ObjectManager.EquipmentManager.GetEQPs())
                                                {
                                                      if (node.Data.NODENO == eqp.EQUIPMENTNO)
                                                      {
                                                            rci.EqpID = node.Data.NODEID;
                                                            rci.RecipeID = eqp.RECIPENO;
                                                            break;
                                                      }
                                                }

                                                if (!dicRecipe.ContainsKey(recipe.RECIPENAME))
                                                      dicRecipe.Add(recipe.RECIPENAME, new List<RecipeCheckInfo> { rci });
                                                else
                                                      dicRecipe[recipe.RECIPENAME].Add(rci);
                                          }


                                          //Recipe Register Validation Command download to EQ
                                          if (command.BODY.LINENAME != _line.RECIPELINENAME)
                                          {

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService RecipeRegisterValidationCommand (CROSS LINE)",
                                                        command.BODY.LINENAME, trxID));

                                                IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos = new Dictionary<string, IList<RecipeCheckInfo>>();
                                                recipeCheckInfos.Add(_line.RECIPELINENAME, dicRecipe[recipe.RECIPENAME]);
                                                bResult = (bool)Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeCheckInfos, new List<string>() });
                                          }
                                          else
                                          {

                                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService RecipeRegisterValidationCommandFromUI",
                                                        command.BODY.LINENAME, trxID));

                                                bResult = (bool)Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromUI", new object[] { trxID, dicRecipe[recipe.RECIPENAME] });
                                          }
                                    }

                                    dicLineRecipe.Add(_line.RECIPELINENAME, dicRecipe);

                                    if (!bResult)
                                    {
                                          //reply.RETURN.RETURNCODE = "0010651";
                                          //reply.RETURN.RETURNMESSAGE = "RecipeRegisterValidationCommandFromUI return NG";

                                          BCSTerminalMessageInform(trxID, command.BODY.LINENAME, "Call RecipeService.RecipeRegisterValidationCommandFromUI NG");

                                          Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService RecipeRegisterValidationCommandFromUI return NG.",
                                          command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                                    }
                              }

                              RecipeRegisterValidationReturnReport(command, dicLineRecipe);
                        }
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
            /// OPI MessageSet: Recipe Register Validation Return Report to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineName"></param>
            /// <param name="recipeID"></param>
            /// <param name="portNo"></param>
            /// <param name="eqpNo"></param>
            public void RecipeRegisterValidationReturnReport(RecipeRegisterValidationCommandRequest cmd, Dictionary<string, Dictionary<string, IList<RecipeCheckInfo>>> lineRecipes)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("RecipeRegisterValidationReturnReport") as XmlDocument;
                        RecipeRegisterValidationReturnReport trx = Spec.XMLtoMessage(xml_doc) as RecipeRegisterValidationReturnReport;
                        trx.BODY.LINENAME = cmd.BODY.LINENAME;
                        trx.BODY.PORTNO = cmd.BODY.PORTNO;
                        string trxID = GetTrxID(cmd.HEADER.TRANSACTIONID);

                        //< string RecipeLineName, Dictionary< string RecipeName, IList<RecipeCheckInfo >>
                        foreach (KeyValuePair<string, Dictionary<string, IList<RecipeCheckInfo>>> _lineRecipes in lineRecipes)
                        {
                              RecipeRegisterValidationReturnReport.LINEc _line = new RecipeRegisterValidationReturnReport.LINEc();
                              _line.RECIPELINENAME = _lineRecipes.Key;

                              foreach (KeyValuePair<string, IList<RecipeCheckInfo>> recipe in _lineRecipes.Value)
                              {
                                    RecipeRegisterValidationReturnReport.RECIPECHECKc _recipe = new RecipeRegisterValidationReturnReport.RECIPECHECKc();
                                    _recipe.RECIPENAME = recipe.Key;

                                    foreach (RecipeCheckInfo info in recipe.Value)
                                    {
                                          RecipeRegisterValidationReturnReport.EQUIPMENTc _eqp = new RecipeRegisterValidationReturnReport.EQUIPMENTc();

                                          //string strName = string.Format("{0}_RecipeRegisterValidationCommandReply", info.EQPNo);
                                          //Trx recipeTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName }) as Trx;

                                          _eqp.EQUIPMENTNO = info.EQPNo;
                                          _eqp.RECIPENO = info.RecipeID;
                                          //_eqp.RETURN = info.Result.ToString() == "OK" ? "OK" : "NG";
                                          //_eqp.NGMESSAGE = info.MESVALICODEText == null ? string.Empty : info.MESVALICODEText;
                                          switch (info.Result)
                                          {
                                                case eRecipeCheckResult.OK:
                                                      _eqp.RETURN = "OK";
                                                      _eqp.NGMESSAGE = string.Empty;
                                                      break;
                                                case eRecipeCheckResult.NG:
                                                case eRecipeCheckResult.MACHINENG:
                                                case eRecipeCheckResult.NOMACHINE:
                                                case eRecipeCheckResult.RECIPELENNG:
                                                case eRecipeCheckResult.TIMEOUT:
                                                      _eqp.RETURN = "NG";
                                                      _eqp.NGMESSAGE = info.Result.ToString();
                                                      break;
                                                case eRecipeCheckResult.ZERO:  //Watson Modify 20150319 For 區別
                                                case eRecipeCheckResult.CIMOFF:
                                                case eRecipeCheckResult.NOCHECK:
                                                      _eqp.RETURN = "NONE";
                                                      _eqp.NGMESSAGE = info.Result.ToString();
                                                      break;
                                          }
                                          //if (recipeTrx == null)
                                          //    _eqp.RETURN = "NG";
                                          //else
                                          //    _eqp.RETURN = recipeTrx.EventGroups[0].Events[0].Items[0].Value == "1" ? "OK" : "NG";

                                          _recipe.EQUIPMENTLIST.Add(_eqp);
                                    }
                                    _line.RECIPECHECKLIST.Add(_recipe);
                              }
                              trx.BODY.LINELIST.Add(_line);
                        }

                        if (trx.BODY.LINELIST.Count > 0)
                              trx.BODY.LINELIST.RemoveAt(0);

                        xMessage msg = SendToOPI(cmd.HEADER.TRANSACTIONID, trx, new List<string>(1) { cmd.HEADER.REPLYSUBJECTNAME });

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                            trx.BODY.LINENAME, msg.TransactionID));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Glass Grade Mapping Change Report Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_GlassGradeMappingChangeReportRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      GlassGradeMappingChangeReportRequest command = Spec.XMLtoMessage(xmlDoc) as GlassGradeMappingChangeReportRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("GlassGradeMappingChangeReportReply") as XmlDocument;
            //      GlassGradeMappingChangeReportReply reply = Spec.XMLtoMessage(xml_doc) as GlassGradeMappingChangeReportReply;

            //      try
            //      {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;
            //            reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

            //            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
            //            if (eqp == null)
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
            //                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                  reply.RETURN.RETURNCODE = "0010720";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
            //            }
            //            else
            //            {
            //                  //由Repository取出儲存的資料
            //                  Trx result = Repository.Get(string.Format("{0}_GlassGradeMappingChangeReport", command.BODY.EQUIPMENTNO)) as Trx;
            //                  if (result == null)
            //                  {
            //                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't get {0}_GlassGradeMappingChangeReport value",
            //                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                        reply.RETURN.RETURNCODE = "0010721";
            //                        reply.RETURN.RETURNMESSAGE = string.Format("Can't get {0}_GlassGradeMappingChangeReport value", command.BODY.EQUIPMENTNO);
            //                  }
            //                  else
            //                  {
            //                        reply.BODY.OKGLASSGRADE = result.EventGroups[0].Events[0].Items[0].Value;
            //                        reply.BODY.NGGLASSGRADE = result.EventGroups[0].Events[0].Items[1].Value;
            //                  }
            //            }

            //            xMessage msg = SendReplyToOPI(command, reply);

            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

            //      }
            //      catch (Exception ex)
            //      {
            //            //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //      }
            //}

            /// <summary>
            /// OPI MessageSet: Equipment Report Setting Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentReportSettingRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentReportSettingRequest command = Spec.XMLtoMessage(xmlDoc) as EquipmentReportSettingRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentReportSettingRequestReply") as XmlDocument;
                  EquipmentReportSettingRequestReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentReportSettingRequestReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, REPORTTYPE={4}, REPORTENABLE={5}, REPORTTIME={6}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.REPORTTYPE, command.BODY.REPORTENABLE, command.BODY.REPORTTIME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010700";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              string type = command.BODY.REPORTTYPE;
                              string enable = command.BODY.REPORTENABLE;
                              int time = int.Parse(command.BODY.REPORTTIME);

                              lock (eqp)
                              {
                                    switch (type)
                                    {
                                          case "APC":
                                                if (enable == "Y") eqp.File.ApcIntervalMS = time;
                                                else eqp.File.ApcIntervalMS = 0;
                                                break;

                                          case "ENERGY":
                                                //Modify By James.Yan at 2015/01/24
                                                if (enable == "Y") eqp.File.EnergyIntervalS = time;
                                                else eqp.File.EnergyIntervalS = 0;
                                                break;
                                          case "OXINFO":
                                                //Modify By qiumin at 20180508
                                                if (enable == "Y") eqp.File.OxinfoCheckFlag = true;
                                                else eqp.File.OxinfoCheckFlag = false;
                                                break;
                                          #region Old Version
                                          //if (enable == "Y") eqp.File.ApcIntervalMS = time;
                                          //else eqp.File.ApcIntervalMS = 0;
                                          //break;
                                          #endregion
                                    }
                              }
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                              Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment REPORTTYPE({2}), REPORTENABLE ({3}), REPORTTIME({4})",
                              command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, type, enable, time.ToString()));
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
            /// OPI MessageSet: Job Data Delete Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_JobDataOperationRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  JobDataOperationRequest command = Spec.XMLtoMessage(xmlDoc) as JobDataOperationRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("JobDataOperationReply") as XmlDocument;
                  JobDataOperationReply reply = Spec.XMLtoMessage(xml_doc) as JobDataOperationReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. COMMADN={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                             command.BODY.COMMAND));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.COMMAND = command.BODY.COMMAND;

                        //Job job = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);
                        //if (job == null)
                        //{
                        //    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //        string.Format("[JOBID={0}] [BCS <- OPI][{1}] Can't find Job({0}) in JobEntity.",
                        //        command.BODY.GLASSID, command.HEADER.TRANSACTIONID));

                        //    reply.RETURN.RETURNCODE = "0010860";
                        //    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Job[{0}] in JobEntity", command.BODY.GLASSID);
                        //}

                        string removeFlag = string.Empty;
                        string reason = string.Empty;

                        switch (command.BODY.COMMAND)
                        {
                              case "DELETE":
                                    removeFlag = "1";
                                    reason = "REMOVE BY OPI";
                                    foreach (JobDataOperationRequest.JOBDATAc job in command.BODY.JOBDATALIST)
                                    {
                                          //OPI removeFlag: 0.Unknow, 1.Normal Remove, 2.Recovery
                                          Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[JOBID={0}] [BCS <- OPI][{1}] Invoke JobService RemoveRecoveryJobDataForUI.",
                                              job.GLASSID, command.HEADER.TRANSACTIONID));

                                          object[] objRemove = new object[6]
                            {
                                command.HEADER.TRANSACTIONID,
                                job.CASSETTESEQNO,
                                job.JOBSEQNO,
                                job.GLASSID,
                                removeFlag,
                                reason
                            };
                                          Invoke(eServiceName.JobService, "RemoveRecoveryJobDataForUI", objRemove);
                                    }
                                    break;

                              case "RECOVERY":
                                    removeFlag = "2";
                                    reason = "RECOVERY BY OPI";
                                    foreach (JobDataOperationRequest.JOBDATAc job in command.BODY.JOBDATALIST)
                                    {
                                          Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[JOBID={0}] [BCS <- OPI][{1}] Invoke JobService RemoveRecoveryJobDataForUI.",
                                          job.GLASSID, command.HEADER.TRANSACTIONID));

                                          object[] objRecovery = new object[6]
                            {
                                command.HEADER.TRANSACTIONID,
                                job.CASSETTESEQNO,
                                job.JOBSEQNO,
                                job.GLASSID,
                                removeFlag,
                                reason
                            };
                                          Invoke(eServiceName.JobService, "RemoveRecoveryJobDataForUI", objRecovery);
                                    }
                                    break;
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


            // ==== Changer Line ====
            /// <summary>
            /// OPI Messageset: Changeer Plan Download Set Command Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ChangerPlanDownloadSetCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ChangerPlanDownloadSetCommandRequest command = Spec.XMLtoMessage(xmlDoc) as ChangerPlanDownloadSetCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ChangerPlanDownloadSetCommandReply") as XmlDocument;
                  ChangerPlanDownloadSetCommandReply reply = Spec.XMLtoMessage(xml_doc) as ChangerPlanDownloadSetCommandReply;
                  reply.BODY.LINENAME = command.BODY.LINENAME;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. RETURNCODE={3}, PLANID={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.RETURNCODE, command.BODY.PLANID));

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                        if (line == null)
                        {
                              reply.RETURN.RETURNCODE = "0010900";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              IList<string> sourceList = new List<string>();
                              IList<string> targetList = new List<string>();

                              switch (command.BODY.RETURNCODE)
                              {
                                    case "OK":
                                      if (line.File.PlanStatus == ePLAN_STATUS.REQUEST || line.File.PlanStatus == ePLAN_STATUS.READY || line.File.PlanStatus == ePLAN_STATUS.START || line.File.PlanStatus == ePLAN_STATUS.ABORTING)
                                          {
                                                reply.RETURN.RETURNCODE = "0010902";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Current Changer Plan[{0}] Status={1} Error!", line.File.CurrentPlanID, line.File.PlanStatus);
                                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    reply.RETURN.RETURNMESSAGE);
                                                break;
                                          }

                                          ObjectManager.PlanManager.RemoveChangePlan();
                                          ObjectManager.PlanManager.RemoveChangePlanStandby();
                                          bool ret = ObjectManager.PlanManager.ReloadDBPlan(command.BODY.LINENAME, command.BODY.PLANID.Trim());
                                          if (ret)
                                          {
                                                IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlans(command.BODY.PLANID.Trim());

                                                if (plans == null || plans.Count == 0)
                                                {
                                                      reply.RETURN.RETURNCODE = "0010902";
                                                      reply.RETURN.RETURNMESSAGE = string.Format("Can't find Changer Plan Data (PlanID={0}) in DataBase!",
                                                          command.BODY.PLANID);

                                                      Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          reply.RETURN.RETURNMESSAGE);
                                                }
                                                else
                                                {
                                                    line.File.CurrentPlanID = command.BODY.PLANID.Trim();
                                                    line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                                                    ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                                    
                                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                                                    Invoke(eServiceName.CassetteService, "PlanReadyStatusCheck", new object[3] { line, eqp, command.HEADER.TRANSACTIONID });

                                                    /*
                                                      foreach (SLOTPLAN s in plans)
                                                      {
                                                            if (sourceList.IndexOf(s.SOURCE_CASSETTE_ID) < 0)
                                                            {
                                                                  if (!string.IsNullOrEmpty(s.SOURCE_CASSETTE_ID.Trim()))
                                                                  {
                                                                        sourceList.Add(s.SOURCE_CASSETTE_ID);
                                                                  }
                                                            }

                                                            if (targetList.IndexOf(s.TARGET_CASSETTE_ID) < 0)
                                                            {
                                                                  if (!string.IsNullOrEmpty(s.TARGET_CASSETTE_ID.Trim()))
                                                                  {
                                                                        targetList.Add(s.TARGET_CASSETTE_ID);
                                                                  }
                                                            }
                                                      }

                                                      if (sourceList.Count == 0)
                                                      {
                                                            ObjectManager.PlanManager.RemoveChangePlan();
                                                            reply.RETURN.RETURNCODE = "0010902";
                                                            reply.RETURN.RETURNMESSAGE = string.Format("Can't find \"Source Cassette ID\"in Changer Plan Data (PlanID={0})!",
                                                                command.BODY.PLANID);

                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                reply.RETURN.RETURNMESSAGE);
                                                      }
                                                      else if (targetList.Count == 0)
                                                      {
                                                            ObjectManager.PlanManager.RemoveChangePlan();
                                                            reply.RETURN.RETURNCODE = "0010902";
                                                            reply.RETURN.RETURNMESSAGE = string.Format("Can't find \"Target Cassette ID\"in Changer Plan Data (PlanID={0})!",
                                                                command.BODY.PLANID);

                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                reply.RETURN.RETURNMESSAGE);
                                                      }
                                                      else
                                                      {
                                                          //t3 BC control Plan
                                                          line.File.CurrentPlanID = command.BODY.PLANID.Trim();
                                                          line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                                                          //Invoke(eServiceName.PortService, "CassetteBondToPort", new object[] { line, plans, true, command.HEADER.TRANSACTIONID }); //manual download plan, don't use bonding cst 2016/01/03 cc.kuang
                                                          Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });

                                                          //check plan if ready
                                                          Equipment eqp = null;
                                                          var lsSourceCassette = from slot in plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                                                          var lsTargetCassette = from slot in plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                                                          eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                                                          bool bPlanReadyStatus = true;
                                                          foreach (string cstid in lsSourceCassette)
                                                          {
                                                                  if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                                                                      p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC && p.File.Type == ePortType.LoadingPort
                                                                                      && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                                                                  {
                                                                      bPlanReadyStatus = false;
                                                                      break;
                                                                  }
                                                          }
                                                          foreach (string cstid in lsTargetCassette)
                                                          {
                                                              if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                                                                      p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC && p.File.Type == ePortType.UnloadingPort
                                                                                      && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                                                                  {
                                                                      bPlanReadyStatus = false;
                                                                      break;
                                                                  }
                                                          }

                                                          //curr Plan & All Plan CST on Port
                                                          if (bPlanReadyStatus)
                                                          {
                                                              line.File.PlanStatus = ePLAN_STATUS.READY;
                                                              Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                                          }
                                                      }
                                                    */
                                                }
                                          }
                                          else
                                          {
                                                ObjectManager.PlanManager.RemoveChangePlan();
                                                reply.RETURN.RETURNCODE = "0010902";
                                                reply.RETURN.RETURNMESSAGE = string.Format("Can't find Changer Plan Data (PlanID={0}) in DataBase!",
                                                    command.BODY.PLANID);

                                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    reply.RETURN.RETURNMESSAGE);
                                          }
                                          break;
                                    case "NG":
                                          IList<Port> ports = ObjectManager.PortManager.GetPorts().Where(p => p.File.Status == ePortStatus.LC).ToList<Port>();

                                          //if (ports != null && ports.Count > 0) 
                                          if (ports != null) //quit all cst, 2015/10/12 cc.kuang 
                                          {
                                                if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()))
                                                      Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          "Changer Plan Download \"NG\", Cancel/Abort All Port");
                                                else
                                                      Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          string.Format("Changer Plan Download \"NG\", Cancel/Abort All Port in this PLAN (PLANID={0})", command.BODY.PLANID.Trim()));

                                                foreach (Port port in ports)
                                                {
                                                      port.File.PlannedCassetteID = "";

                                                      if (command.BODY.PLANID.Trim() != "ALL")  //if cancel plan, don't cancel port
                                                            continue;

                                                      switch (port.File.CassetteStatus) 
                                                      {
                                                            case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                                            case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                                            case eCassetteStatus.WAITING_FOR_PROCESSING:
                                                                  {
                                                                        //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                            //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                        {
                                                                              Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                        }
                                                                  }
                                                                  break;
                                                            case eCassetteStatus.IN_PROCESSING:
                                                                  {
                                                                        //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                            //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                        {
                                                                              Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                        }
                                                                  }
                                                                  break;
                                                      }
                                                }
                                          }

                                        string currPlanID = string.Empty;
                                        string standByPlanID = string.Empty;
                                        currPlanID = line.File.CurrentPlanID;
                                        IList<SLOTPLAN> standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out standByPlanID);
                                        if (command.BODY.PLANID == line.File.CurrentPlanID)//Cancel Current Plan add by box.zhai
                                        {
                                            foreach (Port port in ports)
                                            {
                                                if (ObjectManager.PlanManager.CstExistInChangerPlan(line.File.CurrentPlanID, port.File.CassetteID) && !ObjectManager.PlanManager.CstExistInStandByChangerPlan(standByPlanID, port.File.CassetteID))
                                                {
                                                    switch (port.File.CassetteStatus)
                                                    {
                                                        case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                                        case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                                        case eCassetteStatus.WAITING_FOR_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                        case eCassetteStatus.IN_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                    }
                                                } 
                                            }
                                            if (currPlanID.Trim().Length != 0)
                                            {
                                                if (line.File.HostMode != eHostMode.OFFLINE)
                                                {
                                                    if (line.File.PlanStatus == ePLAN_STATUS.REQUEST || line.File.PlanStatus == ePLAN_STATUS.READY)
                                                        Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, currPlanID.Trim(), "0", "BC OPI Changer Plan Cancel", "" });
                                                    else if (line.File.PlanStatus == ePLAN_STATUS.START)
                                                        Invoke(eServiceName.MESService, "ChangePlanAborted", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, currPlanID.Trim(), "0", "BC OPI Changer Plan Aborted", "" });
                                                }
                                                BCSTerminalMessageInform(command.HEADER.TRANSACTIONID, command.BODY.LINENAME, "Cancel CurrPlan[" + currPlanID + "] OK");
                                            }

                                            //line.File.CurrentPlanID = string.Empty;
                                            line.File.PlanStatus = ePLAN_STATUS.CANCEL;
                                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                            ObjectManager.PlanManager.RemoveChangePlan();
                                            if (standByPlanID != string.Empty)
                                            {
                                                line.File.CurrentPlanID = standByPlanID;
                                                line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                                                ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                                                ObjectManager.PlanManager.AddOnlinePlans(standByPlanID, standbyPlans);
                                                ObjectManager.PlanManager.RemoveChangePlanStandby();
                                            }                                                
                                        }
                                        else if (command.BODY.PLANID == standByPlanID)//Cancel StandBy Plan add by box.zhai
                                        {
                                            foreach (Port port in ports)
                                            {
                                                if (!ObjectManager.PlanManager.CstExistInChangerPlan(line.File.CurrentPlanID, port.File.CassetteID) && ObjectManager.PlanManager.CstExistInStandByChangerPlan(standByPlanID, port.File.CassetteID))
                                                {
                                                    switch (port.File.CassetteStatus)
                                                    {
                                                        case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                                        case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                                        case eCassetteStatus.WAITING_FOR_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                        case eCassetteStatus.IN_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            //add cancel standby plan 2016/08/25 cc.kuang  
                                            if (standByPlanID.Trim().Length > 0)
                                            {
                                                if (line.File.HostMode != eHostMode.OFFLINE)
                                                    Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, standByPlanID.Trim(), "0", "BC OPI StandBy Changer Plan Cancel", "" });
                                                ObjectManager.PlanManager.RemoveChangePlanStandby();
                                                BCSTerminalMessageInform(command.HEADER.TRANSACTIONID, command.BODY.LINENAME, "StandByPlan[" + standByPlanID + "] OK");
                                            }
                                        }
                                        else if (command.BODY.PLANID == "ALLPLAN")
                                        {
                                            if (line.File.HostMode != eHostMode.OFFLINE)
                                            {
                                                if (line.File.PlanStatus == ePLAN_STATUS.REQUEST || line.File.PlanStatus == ePLAN_STATUS.READY)
                                                    Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, currPlanID.Trim(), "0", "BC OPI Changer Plan Cancel", "" });
                                                else if (line.File.PlanStatus == ePLAN_STATUS.START)
                                                    Invoke(eServiceName.MESService, "ChangePlanAborted", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, currPlanID.Trim(), "0", "BC OPI Changer Plan Aborted", "" });
                                                if (standByPlanID.Trim().Length > 0)
                                                {
                                                    Thread.Sleep(300);
                                                    Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { command.HEADER.TRANSACTIONID, line.Data.LINEID, standByPlanID.Trim(), "0", "BC OPI StandBy Changer Plan Cancel", "" });
                                                }
                                            }
                                            foreach (Port port in ports)
                                            {
                                                if (ObjectManager.PlanManager.CstExistInChangerPlan(line.File.CurrentPlanID, port.File.CassetteID) || ObjectManager.PlanManager.CstExistInStandByChangerPlan(standByPlanID, port.File.CassetteID))
                                                {
                                                    switch (port.File.CassetteStatus)
                                                    {
                                                        case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                                        case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                                        case eCassetteStatus.WAITING_FOR_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                        case eCassetteStatus.IN_PROCESSING:
                                                            {
                                                                //if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                //ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                {
                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                            //line.File.CurrentPlanID = string.Empty;
                                            line.File.PlanStatus = ePLAN_STATUS.CANCEL;
                                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                            ObjectManager.PlanManager.RemoveChangePlan();
                                            ObjectManager.PlanManager.RemoveChangePlanStandby();
                                            BCSTerminalMessageInform(command.HEADER.TRANSACTIONID, command.BODY.LINENAME, "Cancel CurrPlan[" + currPlanID + "], StandByPlan[" + standByPlanID + "] OK");
                                        }        
                                        Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                    break;
                                    default:
                                          reply.RETURN.RETURNCODE = "0010903";
                                          reply.RETURN.RETURNMESSAGE = string.Format("\"ChangerPlanDownloadSetCommand\" RETURN_CODE=[{0}] IS INVALID!",
                                              command.BODY.RETURNCODE);

                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              reply.RETURN.RETURNMESSAGE);
                                          break;
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

            public void OPI_ChangerPlanDownloadSetCommandRequest_t2bkup(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ChangerPlanDownloadSetCommandRequest command = Spec.XMLtoMessage(xmlDoc) as ChangerPlanDownloadSetCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ChangerPlanDownloadSetCommandReply") as XmlDocument;
                  ChangerPlanDownloadSetCommandReply reply = Spec.XMLtoMessage(xml_doc) as ChangerPlanDownloadSetCommandReply;
                  reply.BODY.LINENAME = command.BODY.LINENAME;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. RETURNCODE={3}, PLANID={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.RETURNCODE, command.BODY.PLANID));

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                        if (line == null)
                        {
                              reply.RETURN.RETURNCODE = "0010900";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              string trxName = "L2_ChangerPlanDownloadSetCommand";
                              Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;

                              if (trx == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010901";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Trx[{0}]!", trxName);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Trx=[{2}]!",
                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, trxName));
                              }
                              else
                              {
                                    IList<string> sourceList = new List<string>();
                                    IList<string> targetList = new List<string>();

                                    switch (command.BODY.RETURNCODE)
                                    {
                                          case "OK":
                                                bool ret = ObjectManager.PlanManager.ReloadDBPlan(command.BODY.LINENAME, command.BODY.PLANID.Trim());

                                                if (ret)
                                                {
                                                      IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlans(command.BODY.PLANID.Trim());

                                                      if (plans == null || plans.Count == 0)
                                                      {
                                                            reply.RETURN.RETURNCODE = "0010902";
                                                            reply.RETURN.RETURNMESSAGE = string.Format("Can't find Changer Plan Data (PlanID={0}) in DataBase!",
                                                                command.BODY.PLANID);

                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                reply.RETURN.RETURNMESSAGE);
                                                      }
                                                      else
                                                      {
                                                            foreach (SLOTPLAN s in plans)
                                                            {
                                                                  if (sourceList.IndexOf(s.SOURCE_CASSETTE_ID) < 0)
                                                                  {
                                                                        if (!string.IsNullOrEmpty(s.SOURCE_CASSETTE_ID.Trim()))
                                                                        {
                                                                              sourceList.Add(s.SOURCE_CASSETTE_ID);
                                                                        }
                                                                  }

                                                                  if (targetList.IndexOf(s.TARGET_CASSETTE_ID) < 0)
                                                                  {
                                                                        if (!string.IsNullOrEmpty(s.TARGET_CASSETTE_ID.Trim()))
                                                                        {
                                                                              targetList.Add(s.TARGET_CASSETTE_ID);
                                                                        }
                                                                  }
                                                            }

                                                            if (sourceList.Count == 0)
                                                            {
                                                                  reply.RETURN.RETURNCODE = "0010902";
                                                                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find \"Source Cassette ID\"in Changer Plan Data (PlanID={0})!",
                                                                      command.BODY.PLANID);

                                                                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                      reply.RETURN.RETURNMESSAGE);
                                                            }
                                                            else if (targetList.Count == 0)
                                                            {
                                                                  reply.RETURN.RETURNCODE = "0010902";
                                                                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find \"Target Cassette ID\"in Changer Plan Data (PlanID={0})!",
                                                                      command.BODY.PLANID);

                                                                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                      reply.RETURN.RETURNMESSAGE);
                                                            }
                                                            else
                                                            {
                                                                  Invoke(eServiceName.EquipmentService, "ChangerPlanDownloadSetCommand",
                                                                      new object[] { eReturnCode1.OK, "L2", command.BODY.PLANID, sourceList, targetList, command.HEADER.TRANSACTIONID });
                                                            }
                                                      }
                                                }
                                                else
                                                {
                                                      reply.RETURN.RETURNCODE = "0010902";
                                                      reply.RETURN.RETURNMESSAGE = string.Format("Can't find Changer Plan Data (PlanID={0}) in DataBase!",
                                                          command.BODY.PLANID);

                                                      Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          reply.RETURN.RETURNMESSAGE);
                                                }
                                                break;
                                          case "NG":
                                                Invoke(eServiceName.EquipmentService, "ChangerPlanDownloadSetCommand",
                                                    new object[] { eReturnCode1.NG, "L2", command.BODY.PLANID, sourceList, targetList, command.HEADER.TRANSACTIONID });

                                                IList<Port> ports = ObjectManager.PortManager.GetPorts().Where(p => p.File.Status == ePortStatus.LC).ToList<Port>();

                                                if (ports != null && ports.Count > 0)
                                                {
                                                      if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()))
                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                "Changer Plan Download \"NG\", Cancel/Abort All Port");
                                                      else
                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                string.Format("Changer Plan Download \"NG\", Cancel/Abort All Port in this PLAN (PLANID={0})", command.BODY.PLANID.Trim()));

                                                      foreach (Port port in ports)
                                                      {
                                                            switch (port.File.CassetteStatus)
                                                            {
                                                                  case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                                                  case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                                                  case eCassetteStatus.WAITING_FOR_PROCESSING:
                                                                        {
                                                                              if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                                  ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                              {
                                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                              }
                                                                        }
                                                                        break;
                                                                  case eCassetteStatus.IN_PROCESSING:
                                                                        {
                                                                              if (string.IsNullOrEmpty(command.BODY.PLANID.Trim()) ||
                                                                                  ObjectManager.PlanManager.CstExistInChangerPlan(command.BODY.PLANID, port.File.CassetteID))
                                                                              {
                                                                                    Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                                              }
                                                                        }
                                                                        break;
                                                            }
                                                      }
                                                }
                                                break;
                                          default:
                                                reply.RETURN.RETURNCODE = "0010903";
                                                reply.RETURN.RETURNMESSAGE = string.Format("\"ChangerPlanDownloadSetCommand\" RETURN_CODE=[{0}] IS INVALID!",
                                                    command.BODY.RETURNCODE);

                                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    reply.RETURN.RETURNMESSAGE);
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

            // ==== ARRAY ====
            /// <summary>
            /// OPI MessageSet: Force Clean Out Command
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ForceCleanOutCommand(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ForceCleanOutCommand command = Spec.XMLtoMessage(xmlDoc) as ForceCleanOutCommand;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ForceCleanOutCommandReply") as XmlDocument;
                  ForceCleanOutCommandReply reply = Spec.XMLtoMessage(xml_doc) as ForceCleanOutCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. COMMAND={3}, STATUS={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.COMMAND, command.BODY.STATUS));

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Invoke ArraySpecialService ForceCleanOutCommand.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        eBitResult result = command.BODY.STATUS == "1" ? eBitResult.ON : eBitResult.OFF;
                        bool bAbnormal = command.BODY.COMMAND == "ABNORMAL" ? true : false;
                        if (bAbnormal)
                              Invoke("ArraySpecialService", "AbnormalForceCleanOutCommand", new object[] { result, command.HEADER.TRANSACTIONID });
                        else
                              Invoke("ArraySpecialService", "ForceCleanOutCommand", new object[] { result, command.HEADER.TRANSACTIONID });

                        //儲存起來以便OPI詢問時回覆
                        ForceCleanOutCmd cmd = new ForceCleanOutCmd(command.BODY.COMMAND, command.BODY.STATUS);
                        lock (dicForceCleanOut)
                        {
                              if (dicForceCleanOut.ContainsKey(command.BODY.LINENAME))
                                    dicForceCleanOut[command.BODY.LINENAME] = cmd;
                              else
                                    dicForceCleanOut.Add(command.BODY.LINENAME, cmd);
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;

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

            // ==== CELL ====
            /// <summary>
            /// OPI MessageSet: ODF Track Time Setting Change Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ODFTrackTimeSettingChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ODFTrackTimeSettingChangeRequest command = Spec.XMLtoMessage(xmlDoc) as ODFTrackTimeSettingChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ODFTrackTimeSettingChangeReply") as XmlDocument;
                  ODFTrackTimeSettingChangeReply reply = Spec.XMLtoMessage(xml_doc) as ODFTrackTimeSettingChangeReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010820";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else
                        {
                              string checkUnitKey = string.Format("ODF_TACKTIME_CHECKUNIT_{0}", command.BODY.EQUIPMENTNO);
                              string delayTimeKey = string.Format("ODF_TACKTIME_DELAYTIME_{0}", command.BODY.EQUIPMENTNO);

                              //CHECKUNIT
                              string units = string.Empty;
                              for (int i = 0; i < command.BODY.UNITLIST.Count; i++)
                              {
                                    if (i != command.BODY.UNITLIST.Count - 1)
                                          units += string.Format("{0},", command.BODY.UNITLIST[i].UNITNO);
                                    else
                                          units += command.BODY.UNITLIST[i].UNITNO;
                              }
                              ConstantManager[checkUnitKey].Values["CHECKUNIT"].Value = units;

                              //DELAYTIME
                              ConstantManager[delayTimeKey].Values.Clear();
                              for (int j = 0; j < command.BODY.DELAYTIMELIST.Count; j++)
                              {
                                    string downKey = string.Format("DOWN{0}", command.BODY.DELAYTIMELIST[j].SEQNO);
                                    ConstantItem item = new ConstantItem(command.BODY.DELAYTIMELIST[j].VALUE, string.Empty);
                                    ConstantManager[delayTimeKey].Values.Add(downKey, item);
                              }

                              ConstantManager.WriteToXML();
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

            ///// <summary>
            ///// OPI MessageSet: Operation Run Mode Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_OperationRunModeChangeRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      OperationRunModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as OperationRunModeChangeRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("OperationRunModeChangeReply") as XmlDocument;
            //      OperationRunModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as OperationRunModeChangeReply;

            //      try
            //      {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENT={3}, COMMANDTYPE={4}, ITEMVALUE={5}",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
            //                command.BODY.EQUIPMENTNO, command.BODY.COMMANDTYPE, command.BODY.ITEMVALEU));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;
            //            reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //            reply.BODY.COMMANDTYPE = command.BODY.COMMANDTYPE;

            //            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);

            //            if (eqp == null)
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
            //                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                  reply.RETURN.RETURNCODE = "0010860";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment({0}) in EquipmentEntity", command.BODY.EQUIPMENTNO);
            //            }
            //            else if (string.IsNullOrEmpty(command.BODY.COMMANDTYPE) || string.IsNullOrEmpty(command.BODY.ITEMVALEU))
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] CommandType({2}) or ItemValue({3}) is empty.",
            //                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, command.BODY.COMMANDTYPE, command.BODY.ITEMVALEU));

            //                  reply.RETURN.RETURNCODE = "0010862";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("CommandType[{0}] or ItemValue[{1}] is empty", command.BODY.COMMANDTYPE, command.BODY.ITEMVALEU);
            //            }
            //            else
            //            {
            //                  string key = string.Format("{0}_OperationRunModeChangeRequestReplyTimout", command.BODY.EQUIPMENTNO);

            //                  if (_timerManager.IsAliveTimer(key))
            //                  {
            //                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment({0}) is busy, please wait for a minute and retry.",
            //                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                        reply.RETURN.RETURNCODE = "0010862";
            //                        reply.RETURN.RETURNMESSAGE = string.Format("Equipment[{0}] is busy, please wait for a minute and retry", command.BODY.EQUIPMENTNO);
            //                  }
            //                  else
            //                  {
            //                        switch (command.BODY.COMMANDTYPE)
            //                        {
            //                              case "RUNMODE":
            //                                    eCELLATSRunMode runMode = (eCELLATSRunMode)int.Parse(command.BODY.ITEMVALEU);

            //                                    object[] _runModeData = new object[5]
            //                    { 
            //                        command.BODY.EQUIPMENTNO,     // eqp.Data.NODENO,   /*0 eqpno*/
            //                        eBitResult.ON,                                      /*1 ON*/
            //                        command.HEADER.TRANSACTIONID,                       /*trx id */
            //                        runMode,
            //                        "OPI"
            //                    };

            //                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CELLSpecialService RunModeChangeCommand.",
            //                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                                    Invoke(eServiceName.CELLSpecialService, "RunModeChangeCommand", _runModeData);
            //                                    break;

            //                              case "LOADEROPERATIONMODE":
            //                                    eCELLATSLDOperMode operMode = (eCELLATSLDOperMode)int.Parse(command.BODY.ITEMVALEU);

            //                                    object[] _operModeData = new object[5]
            //                    { 
            //                        command.BODY.EQUIPMENTNO,     // eqp.Data.NODENO,   /*0 eqpno*/
            //                        eBitResult.ON,                                      /*1 ON*/
            //                        command.HEADER.TRANSACTIONID,                       /*trx id */
            //                        operMode,
            //                        "OPI"
            //                    };

            //                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CELLSpecialService ATSLoaderOperationModeChangeCommand.",
            //                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                                    Invoke(eServiceName.CELLSpecialService, "ATSLoaderOperationModeChangeCommand", _operModeData);
            //                                    break;
            //                        }

            //                        //建立Timer計算BC
            //                        _timerManager.CreateTimer(key, false, 8000, new System.Timers.ElapsedEventHandler(OperationRunModeChangeRequestReplyTimeout), command.HEADER.TRANSACTIONID);

            //                        lock (dicOpertationRunMode)
            //                        {
            //                              if (!dicOpertationRunMode.ContainsKey(key))
            //                                    dicOpertationRunMode.Add(key, command.HEADER.REPLYSUBJECTNAME);
            //                              else
            //                                    dicOpertationRunMode[key] = command.HEADER.REPLYSUBJECTNAME;
            //                        }
            //                  }
            //            }

            //            xMessage msg = SendReplyToOPI(command, reply);

            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            //      }
            //      catch (Exception ex)
            //      {
            //            //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //      }
            //}

            /// <summary>
            /// OPI MessageSet: ODF Track Time Setting Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ODFTrackTimeSettingRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ODFTrackTimeSettingRequest command = Spec.XMLtoMessage(xmlDoc) as ODFTrackTimeSettingRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ODFTrackTimeSettingReply") as XmlDocument;
                  ODFTrackTimeSettingReply reply = Spec.XMLtoMessage(xml_doc) as ODFTrackTimeSettingReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010930";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else
                        {
                            //目前僅提供SDP VPO LCD VAC SUV SMO機台設定
                              string[] eqps = new string[] { "L7", "L11", "L13", "L14","L16","L19" };

                              reply.BODY.EQUIPMENTLIST.Clear();
                              for (int i = 0; i < eqps.Length; i++)
                              {
                                    ODFTrackTimeSettingReply.EQUIPMENTc _eqp = new ODFTrackTimeSettingReply.EQUIPMENTc();
                                    string checkUnitKey = string.Format("ODF_TACKTIME_CHECKUNIT_{0}", eqps[i]);
                                    string delayTimeKey = string.Format("ODF_TACKTIME_DELAYTIME_{0}", eqps[i]);

                                    _eqp.EQUIPMENTNO = eqps[i];
                                    //CHECKUNIT
                                    string[] units = ConstantManager[checkUnitKey].Values["CHECKUNIT"].Value.Split(',');
                                    foreach (string unit in units)
                                    {
                                          _eqp.UNITLIST.Add(new ODFTrackTimeSettingReply.UNITc() { UNITNO = unit });
                                    }
                                    //DELAYTIME
                                    foreach (KeyValuePair<string, ConstantItem> value in ConstantManager[delayTimeKey].Values)
                                    {
                                          _eqp.DELAYTIMELIST.Add(new ODFTrackTimeSettingReply.DELAYTIMEc() { SEQNO = value.Key.Substring(4), VALUE = value.Value.Value });
                                    }

                                    reply.BODY.EQUIPMENTLIST.Add(_eqp);
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
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            //public void OPI_LocalModeCassetteDataSend_DPI(XmlDocument xmlDoc)
            //{
            //    IServerAgent agent = GetServerAgent();
            //    LocalModeCassetteDataSend_DPI command = Spec.XMLtoMessage(xmlDoc) as LocalModeCassetteDataSend_DPI;
            //    XmlDocument xml_doc = agent.GetTransactionFormat("LocalModeCassetteDataSendReply_DPI") as XmlDocument;
            //    LocalModeCassetteDataSendReply_DPI reply = Spec.XMLtoMessage(xml_doc) as LocalModeCassetteDataSendReply_DPI;

            //    try
            //    {
            //        string log = string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO=[{3}]",
            //             command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO);

            //        if (command.BODY.PORTLIST.Count > 0)
            //        {
            //            foreach (LocalModeCassetteDataSend_DPI.PORTc pp in command.BODY.PORTLIST)
            //                log += string.Format(", PORTNO=[{0}], BOXID=[{1}]", pp.PORTID, pp.CASSETTEID);
            //        }
            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", log);

            //        IList<Line> lines = ObjectManager.LineManager.GetLines();
            //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
            //        XmlDocument opiXml = new XmlDocument();

            //        if (reply.BODY.PORTLIST.Count > 0)
            //            reply.BODY.PORTLIST.RemoveAt(0);

            //        #region 比對OPI傳的msg是否符合BC下貨原則
            //        foreach (LocalModeCassetteDataSend_DPI.PORTc pp in command.BODY.PORTLIST)
            //        {
            //            Port port = ObjectManager.PortManager.GetPort(pp.PORTID);
            //            Cassette cst = ObjectManager.CassetteManager.GetCassette(pp.CASSETTEID);

            //            if (lines == null)
            //            {
            //                reply.RETURN.RETURNCODE = "0010120";
            //                reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity.", command.BODY.LINENAME);

            //                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
            //                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //            else if (eqp == null)
            //            {
            //                reply.RETURN.RETURNCODE = "0010121";
            //                reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity.", command.BODY.EQUIPMENTNO);

            //                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
            //                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
            //            }
            //            else if (port == null)
            //            {
            //                reply.RETURN.RETURNCODE = "0010122";
            //                reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity.", pp.PORTID);

            //                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
            //                    pp.PORTID, command.HEADER.TRANSACTIONID));
            //            }
            //            else if (cst == null)
            //            {
            //                reply.RETURN.RETURNCODE = "0010123";
            //                reply.RETURN.RETURNMESSAGE = string.Format("Can't find BOX1[{0}] in CassetteEntity.", pp.CASSETTEID);

            //                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find BOX1({0}) in CassetteEntity.",
            //                    pp.CASSETTEID, command.HEADER.TRANSACTIONID));
            //            }
            //            else
            //            {
            //                //收集下貨的資料
            //                XmlDocument mesXml = new XmlDocument();
            //                mesXml.LoadXml(port.File.Mes_ValidateBoxReply);

            //                XmlNode body = mesXml.SelectSingleNode("//MESSAGE/BODY");
            //                body["OPI_LINERECIPENAME"].InnerText = pp.LOTDATA.PRODUCTLIST[0].PRODUCTRECIPENAME;
            //                body["OPI_CARRIERSETCODE"].InnerText = pp.CSTSETTINGCODE;
            //                XmlNodeList boxListNode = mesXml.SelectSingleNode("//MESSAGE/BODY/BOXLIST").ChildNodes;

            //                int i = 0;
            //                foreach (XmlNode boxNode in boxListNode)
            //                {
            //                    if (boxNode["BOXNAME"].InnerText == pp.LOTDATA.LOTNAME)
            //                    {
            //                        boxNode["OPI_PRDCARRIERSETCODE"].InnerText = pp.LOTDATA.CSTSETTINGCODE;
            //                        XmlNodeList productListNode = boxNode.SelectSingleNode("PRODUCTLIST").ChildNodes;
            //                        int l = 0;
            //                        foreach (XmlNode productNode in productListNode)
            //                        {
            //                            if (productNode["POSITION"].InnerText == pp.LOTDATA.PRODUCTLIST[l].SLOTNO)
            //                            {
            //                                productNode["PROCESSFLAG"].InnerText = pp.LOTDATA.PRODUCTLIST[l].PROCESSFLAG;
            //                                //OPI是直接塞入各slot PRODUCT RECIPE NAME, 但CELL BOX只需要給一個LOT RECIPE NAME即可
            //                                body["OPI_LINERECIPENAME"].InnerText = pp.LOTDATA.PRODUCTLIST[0].PRODUCTRECIPENAME;

            //                                productNode["OPI_PRODUCTRECIPENAME"].InnerText = pp.LOTDATA.PRODUCTLIST[l].PRODUCTRECIPENAME;
            //                                productNode["OPI_PPID"].InnerText = pp.LOTDATA.PRODUCTLIST[l].PPID;
            //                            }
            //                            l++;
            //                        }
            //                    }
            //                    i++;
            //                }
            //                opiXml = mesXml;
            //            }
            //            LocalModeCassetteDataSendReply_DPI.PORTc replyPort = new LocalModeCassetteDataSendReply_DPI.PORTc();
            //            //收集回覆OPI的資料
            //            replyPort.PORTNO = pp.PORTNO;
            //            replyPort.PORTID = pp.PORTID;
            //            replyPort.CASSETTEID = pp.CASSETTEID;
            //            reply.BODY.PORTLIST.Add(replyPort);
            //        }
            //        #endregion

            //        #region 送給BC做解析、下貨
            //        //轉拋回給MESService處理後續動作
            //        //Decode_ValidateBoxDataDPI(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst,Cassette cst2)
            //        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
            //        if (command.BODY.PORTLIST.Count > 0)
            //        {
            //            Port port1 = ObjectManager.PortManager.GetPort(command.BODY.PORTLIST[0].PORTID);
            //            Cassette box1 = ObjectManager.CassetteManager.GetCassette(command.BODY.PORTLIST[0].CASSETTEID);
            //            Cassette box2 = new Cassette();
            //            if (command.BODY.PORTLIST.Count > 1)
            //                box2 = ObjectManager.CassetteManager.GetCassette(command.BODY.PORTLIST[1].CASSETTEID);

            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[BOX1={0}][BOX2={1}] [BCS <- OPI][{2}] Invoke MESService Decode_ValidateBOXData_DPI",
            //            box1.CassetteID, box2.CassetteID, command.HEADER.TRANSACTIONID));

            //            Invoke(eServiceName.MESService, "Decode_ValidateBoxDataDPI", new object[] { opiXml, line, eqp, port1, box1, box2 });
            //        }
            //        #endregion

            //        #region 回覆給OPI 無論ok，不ok都要回覆
            //        reply.BODY.LINENAME = command.BODY.LINENAME;
            //        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

            //        xMessage msg = SendReplyToOPI(command, reply);
            //        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            //        #endregion


            //    }
            //    catch (Exception ex)
            //    {
            //        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
            //    }
            //}

            //暫時無用、可能需要一個command 觸發兩個port, 先寫來預防
            private void OPI_CassetteCommandRequest_DPI(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CassetteCommandRequest command = Spec.XMLtoMessage(xmlDoc) as CassetteCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CassetteCommandReply") as XmlDocument;
                  CassetteCommandReply reply = Spec.XMLtoMessage(xml_doc) as CassetteCommandReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, EQUIPMENTNO={4}, PORTNO={5}, CASSETTEID={6}, CASSETTECOMMAND={7}, PROCESSCOUNT={8}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, command.BODY.CASSETTECOMMAND, command.BODY.PROCESSCOUNT));


                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(command.BODY.CASSETTEID.Trim());

                        Port port2 = ObjectManager.PortManager.GetPortByDPI("L2", port.Data.PORTNO);
                        Cassette cst2 = new Cassette();
                        if (port != null)
                              cst2 = ObjectManager.CassetteManager.GetCassette(port2.File.CassetteID);

                        Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);

                        //1:Process Start, 2:Process Start By Count, 3:Process Pause, 4:Process Resume
                        //5:Process Abort, 6:Process Cancel, 7:Reload, 8:Load, 9:Re-Map, 11:Map Download
                        switch (command.BODY.CASSETTECOMMAND)
                        {
                              case "1":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessStart_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessStart_UI", new object[] { command });

                                    break;
                              case "2":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessStartByCount_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessStartByCount_UI", new object[] { command });
                                    break;
                              case "3":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessPause_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessPause_UI", new object[] { command });
                                    break;
                              case "4":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessPause_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessPause_UI", new object[] { command });
                                    break;
                              case "5":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessAbort_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessAbort_UI", new object[] { command });
                                    break;
                              case "6":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessCancel_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteProcessCancel_UI", new object[] { command });
                                    break;
                              case "7":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteReload_UI.",
                                    command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteReload_UI", new object[] { command });
                                    break;
                              case "8":
                                    //Load此時沒有CST資訊
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteLoad_UI.",
                                    command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteLoad_UI", new object[] { command });
                                    break;
                              case "9":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteReMap_UI.",
                                        cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("CassetteService", "CassetteReMap_UI", new object[] { command });
                                    break;
                              case "11":
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteMapDownload_UI.",
                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                    Invoke("DenseBoxCassetteService", "DPCassetteMapDownload_UI", new object[] { command });
                                    break;
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;
                        reply.BODY.CASSETTECOMMAND = command.BODY.CASSETTECOMMAND;
                        reply.BODY.PROCESSCOUNT = command.BODY.PROCESSCOUNT;

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
            /// OPI MessageSet: Local Mode Dense Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalModeDenseDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalModeDenseDataRequest command = Spec.XMLtoMessage(xmlDoc) as LocalModeDenseDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalModeDenseDataReply") as XmlDocument;
                  LocalModeDenseDataReply reply = Spec.XMLtoMessage(xml_doc) as LocalModeDenseDataReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010790";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010791";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010792";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (string.IsNullOrEmpty(port.File.Mes_ValidateBoxReply))
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) doesn't has MES information.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010793";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] doesn't has MES information.", command.BODY.PORTNO);
                        }
                        else
                        {
                              //取得MES所給的資訊
                              XmlDocument mesXml = new XmlDocument();
                              mesXml.LoadXml(port.File.Mes_ValidateBoxReply);

                              reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                              reply.BODY.PORTNO = command.BODY.PORTNO;

                              string returnCode = mesXml.SelectSingleNode("//MESSAGE/RETURN/RETURNCODE").InnerText;
                              string linerecipename = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                              string portid = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                              string setcode = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
                              string producttype = "0";
                              string boxid1 = string.Empty, boxid2 = string.Empty, grade1 = string.Empty, grade2 = string.Empty;
                              string cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0";

                              XmlNode boxlist = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];

                              int i = 0;
                              foreach (XmlNode box in boxlist)
                              {
                                    Cassette cst = new Cassette();
                                    if (i == 0)
                                    {
                                        boxid1 = box[keyHost.BOXNAME].InnerText;
                                        reply.BODY.BOXID01 = boxid1;
                                        grade1 = box[keyHost.BOXGRADE].InnerText;
                                        reply.BODY.JOBGRADE01 = grade1;
                                        cstsetcode1 = setcode;
                                        reply.BODY.CASSETTESETTINGCODE01 = cstsetcode1;
                                        boxqty1 = box[keyHost.PRODUCTQUANTITY].InnerText;
                                        reply.BODY.BOXGLASSCOUNT01 = boxqty1;

                                        cst.CassetteID = reply.BODY.BOXID01;
                                    }
                                    else
                                    {
                                        boxid2 = box[keyHost.BOXNAME].InnerText;
                                        reply.BODY.BOXID02 = boxid2;
                                        grade2 = box[keyHost.BOXGRADE].InnerText;
                                        reply.BODY.JOBGRADE02 = grade2;
                                        cstsetcode2 = setcode;
                                        reply.BODY.CASSETTESETTINGCODE02 = cstsetcode2;
                                        boxqty2 = box[keyHost.PRODUCTQUANTITY].InnerText;
                                        reply.BODY.BOXGLASSCOUNT02 = boxqty2;

                                        cst.CassetteID = reply.BODY.BOXID02;
                                    }
             
                                    cst.PortID = portid;
                                    cst.PortNo = portid;
                                    cst.LineRecipeName = linerecipename;
                                    ObjectManager.CassetteManager.CreateBoxforPacking(cst);//20161102 sy modify
                                    i++;

                                    XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;

                                    if (productList.Count > 0)
                                    {
                                          List<string> items = new List<string>();

                                          int slotNo;
                                          int.TryParse(productList[0][keyHost.POSITION].InnerText, out slotNo);
                                          Job job = new Job(1, slotNo);

                                          if (productList[0][keyHost.OWNERTYPE].InnerText.Substring(productList[0][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                              && productList[0][keyHost.OWNERID].InnerText != "RESD")
                                          {
                                                //Product Type
                                                items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                                                items.Add(productList[i][keyHost.GROUPID].InnerText);
                                                job.ProductType.SetItemData(items);
                                          }
                                          else
                                          {
                                                //Product Type
                                                items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                                                job.ProductType.SetItemData(items);
                                          }

                                          string err;
                                          IList<Job> jobs = new List<Job>();
                                          if (ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[0][keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                                          {
                                                producttype = job.ProductType.Value.ToString();
                                          }
                                    }
                              }

                              reply.BODY.PRODUCTTYPE = producttype;

                              List<string> replylist = new List<string>();
                              replylist.AddRange(new string[] { returnCode, portid, boxid1, boxid2, producttype, grade1, grade2, cstsetcode1, cstsetcode2, boxqty1, boxqty2 });
                              //將資訊存在Dictionary中避免拆解兩次
                              string strKey = string.Format("{0}_{1}", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                              lock (dicDenseBoxData)
                              {
                                    if (dicDenseBoxData.ContainsKey(strKey))
                                          dicDenseBoxData[strKey] = replylist;
                                    else
                                          dicDenseBoxData.Add(strKey, replylist);
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
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

            ///// <summary>
            ///// OPI MessageSet: Operation Permission Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_OperationPermissionRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      OperationPermissionRequest command = Spec.XMLtoMessage(xmlDoc) as OperationPermissionRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("OperationPermissionReply") as XmlDocument;
            //      OperationPermissionReply reply = Spec.XMLtoMessage(xml_doc) as OperationPermissionReply;

            //      try
            //      {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, OPERATORPERMISSION={4}",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.OPERATORPERMISSION));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;

            //            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);

            //            if (eqp == null)
            //            {
            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
            //                      command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                  reply.RETURN.RETURNCODE = "0010841";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment({0}) in EquipmentEntity", command.BODY.EQUIPMENTNO);
            //            }
            //            else
            //            {
            //                  reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

            //                  eCELLATSOperPermission oper = command.BODY.OPERATORPERMISSION == "1" ? eCELLATSOperPermission.Request : eCELLATSOperPermission.Complete;

            //                  //記錄OPI Client
            //                  string key = string.Format("{0}_OperationPermissionRequestReplyTimeout", command.BODY.EQUIPMENTNO);

            //                  if (_timerManager.IsAliveTimer(key))
            //                  {
            //                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment({0}) is busy, please wait a minute and retry.",
            //                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                        reply.RETURN.RETURNCODE = "0010842";
            //                        reply.RETURN.RETURNMESSAGE = string.Format("Equipment[{0}] is busy, please wait a minute and reply.", command.BODY.EQUIPMENTNO);
            //                  }
            //                  else
            //                  {
            //                        object[] data = new object[5]
            //            {
            //                command.BODY.EQUIPMENTNO,
            //                eBitResult.ON,
            //                command.HEADER.TRANSACTIONID,
            //                oper,
            //                "OPI"
            //            };

            //                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CELLSpecialService OperationPermissionRequestCommand.",
            //                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

            //                        Invoke(eServiceName.CELLSpecialService, "OperationPermissionRequestCommand", data);

            //                        //EQ T2 Timeout為4秒 暫時先設為5秒
            //                        _timerManager.CreateTimer(key, false, 5000, new System.Timers.ElapsedEventHandler(OperationPermissionRequestReplyTimeout), command.HEADER.TRANSACTIONID);

            //                        lock (dicOperationPermission)
            //                        {
            //                              if (!dicOperationPermission.ContainsKey(key))
            //                                    dicOperationPermission.Add(key, Tuple.Create<string, eCELLATSOperPermission>(command.HEADER.REPLYSUBJECTNAME, oper));
            //                              else
            //                                    dicOperationPermission[key] = Tuple.Create<string, eCELLATSOperPermission>(command.HEADER.REPLYSUBJECTNAME, oper);
            //                        }
            //                  }
            //            }

            //            xMessage msg = SendReplyToOPI(command, reply);

            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            //      }
            //      catch (Exception ex)
            //      {
            //            //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //      }
            //}

            /// <summary>
            /// OPI MessageSet: Unloading Port Setting Command Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_UnloadingPortSettingCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  UnloadingPortSettingCommandRequest command = Spec.XMLtoMessage(xmlDoc) as UnloadingPortSettingCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("UnloadingPortSettingCommandReply") as XmlDocument;
                  UnloadingPortSettingCommandReply reply = Spec.XMLtoMessage(xml_doc) as UnloadingPortSettingCommandReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);

                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010960";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              reply.RETURN.RETURNCODE = "0010961";
                              reply.RETURN.RETURNMESSAGE = string.Format("EQUIPMENT=[{0}] CIM OFF", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              //TO DO
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                 string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CFSpecialService UnloadingPortSettingCommand.",
                                 command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              Invoke("CFSpecialService", "UnloadingPortSettingCommand", new object[] 
                    { command.HEADER.TRANSACTIONID, command.BODY.OKSTOREQTIME,command.BODY.OKPRODUCTTYPECHECKMODE,command.BODY.NGSTOREQTIME ,
                            command.BODY.NGPRODUCTTYPECHECKMODE ,command.BODY.NGPORTJUDGE,command.BODY.PDSTOREQTIME,command.BODY.PDPRODUCTTYPECHECKMODE ,
                            command.BODY.RPSTOREQTIME,command.BODY.RPPRODUCTTYPECHECKMODE ,command.BODY.IRSTOREQTIME ,command.BODY.IRPRODUCTTYPECHECKMODE ,
                            command.BODY.MIXSTOREQTIME ,command.BODY.MIXPRODUCTTYPECHECKMODE,command.BODY.MIXPORTJUDGE ,command.BODY.OPERATORID});
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
            /// OPI MessageSet: Buffer RW Judge Capacity Change Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_BufferRWJudgeCapacityChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  BufferRWJudgeCapacityChangeRequest command = Spec.XMLtoMessage(xmlDoc) as BufferRWJudgeCapacityChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("BufferRWJudgeCapacityChangeReply") as XmlDocument;
                  BufferRWJudgeCapacityChangeReply reply = Spec.XMLtoMessage(xml_doc) as BufferRWJudgeCapacityChangeReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010970";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              reply.RETURN.RETURNCODE = "0010971";
                              reply.RETURN.RETURNMESSAGE = string.Format("EQUIPMENT=[{0}] CIM OFF", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              //TO DO
                              if (command.BODY.BUFFERJUDGECAPACITY == "2")
                              {
                                    reply.RETURN.RETURNCODE = "0010972";
                                    reply.RETURN.RETURNMESSAGE = string.Format("CV#06 Buffer Judge Capacity={0}[DISABLE],Can't Set Buffer RW Judge Capacity", command.BODY.BUFFERJUDGECAPACITY);
                              }
                              else
                              {
                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke CFSpecialService BufferRWJudgeCapacityChangeCommand.",
                                        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                                    //轉拋給CFSpecialService處理
                                    Invoke("CFSpecialService", "BufferRWJudgeCapacityChangeCommand",
                                        new object[] { command.HEADER.TRANSACTIONID, command.BODY.BUFFERJUDGECAPACITY, 
                            command.BODY.BF01RWJUDGECAPACITY, command.BODY.BF02RWJUDGECAPACITY, command.BODY.OPERATORID});
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

            public void OPI_RobotFetchSequenceModeChangeRequest(XmlDocument xmlDoc)
            {

                IServerAgent agent = GetServerAgent();
                RobotFetchSequenceModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotFetchSequenceModeChangeRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotFetchSequenceModeChangeReply") as XmlDocument;
                RobotFetchSequenceModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as RobotFetchSequenceModeChangeReply;

                try
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                    reply.BODY.LINENAME = command.BODY.LINENAME;

                    Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                    string robotFetchsequenceMode = command.BODY.ROBOT_FETCH_SEQUENCE_MODE;

                    if (line == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010970";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                    }
                    else
                    {
                            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Invoke EquipmentService RobotFetchSequenceModeCommand.",
                                "L2", command.HEADER.TRANSACTIONID));
                            //轉拋給CFSpecialService處理
                            Invoke("EquipmentService", "RobotFetchSequenceModeCommand",
                                new object[] { command.HEADER.TRANSACTIONID,"L2", robotFetchsequenceMode});
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

            /// OPI MessageSet:Unit Run Mode Change Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_UnitRunModeChangeRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  UnitRunModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as UnitRunModeChangeRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("UnitRunModeChangeReply") as XmlDocument;
                  UnitRunModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as UnitRunModeChangeReply;
                  string msgTemp = string.Empty;
                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        #region Line object check
                        if (line == null)
                        {
                              reply.RETURN.RETURNCODE = "0010990";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINE={0}] [BCS <- OPI][{1}] Can't find  Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                              goto replytag;
                        }
                        #endregion
                        #region Equipment object check
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010991";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find  Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                              goto replytag;
                        }
                        #endregion
                        List<Tuple<Unit, string, string>> unitRpt = new List<Tuple<Unit, string, string>>();
                        List<UnitRunModeChangeRequest.UNITc> cmdUnits = command.BODY.UNITLIST;
                        msgTemp = string.Empty;
                        string modeDescript = string.Empty;
                        #region Collect need handle unit
                        foreach (UnitRunModeChangeRequest.UNITc unitc in cmdUnits)
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitc.UNITNO);
                              if (unit == null)
                              {
                                    msgTemp = string.Format("{0} Can not find Unit({1}) in UnitEntity!", msgTemp, unitc.UNITNO);
                                    continue;
                              }
                              if (unit.Data.UNITTYPE == null || !unit.Data.UNITTYPE.Equals("CHAMBER"))
                              {
                                    msgTemp = string.Format("{0} Unit({1}) is not CHAMBER!", msgTemp, unitc.UNITNO);
                                    continue;
                              }
                              if (unit.Data.UNITATTRIBUTE == "VIRTUAL")
                              {
                                    msgTemp = string.Format("{0} Unit({1}) is VIRTUAL!", msgTemp, unitc.UNITNO);
                                    continue;
                              }

                              string cRunMode = Invoke(eServiceName.EquipmentService, "GetRunMode",
                                    new object[] { line, unitc.NEW_RUNMODE, modeDescript },
                                    new Type[] { typeof(Line), typeof(string), typeof(string).MakeByRefType() }) as string;
                              unitRpt.Add(Tuple.Create(unit, unitc.NEW_RUNMODE, cRunMode));
                        }
                        #endregion
                        #region Check unit collection object have data
                        if (unitRpt.Count <= 0)
                        {
                              reply.RETURN.RETURNCODE = "0010992";
                              reply.RETURN.RETURNMESSAGE = string.Format("No need handle unit! {0}", msgTemp);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] No need handel unit! {2}",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, msgTemp));
                              goto replytag;
                        }
                        #endregion
                        //Check run mode rule by line
                        switch (eqp.Data.NODEATTRIBUTE.Trim().ToUpper())
                        {
                              case "DRY":
                                    {
                                          if (line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD||line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL)
                                          {
                                                string smsg = string.Empty;
                                                string lmsg = string.Empty;
                                                object[] checkParam = new object[] { line, eqp, unitRpt, command.BODY.NEW_RUNMODE, lmsg, smsg };
                                                bool result = (bool)Invoke(eServiceName.CSOTSECSService, "EquipmentRunModeCheck_Common",
                                                      checkParam,
                                                      new Type[] { typeof(Line), typeof(Equipment), typeof(List<Tuple<Unit, string, string>>), typeof(string), typeof(string).MakeByRefType(), typeof(string).MakeByRefType() });
                                                if (!result)
                                                {
                                                      reply.RETURN.RETURNCODE = "0010993";
                                                      reply.RETURN.RETURNMESSAGE = string.Format("Run Mode Check Error - {0}", checkParam[5]);

                                                      Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] {2}",
                                                          command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID, checkParam[4]));
                                                      goto replytag;
                                                }
                                                //發Command to equipment
                                                Invoke(eServiceName.CSOTSECSService, "TS2F119_H_EquipmentModeChangeCommandSend",
                                                      new object[] { command.BODY.EQUIPMENTNO, eqp.Data.NODEID, command.BODY.NEW_RUNMODE, unitRpt, "OPI", command.HEADER.TRANSACTIONID });
                                          }
                                    }
                                    break;
                        }



                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                        //reply.RETURN.RETURNCODE = 
                  }

            replytag:
                  xMessage msg = SendReplyToOPI(command, reply);

                  Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                      string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }

            /// OPI MessageSet:Port Assignment Command Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PortAssignmentCommandRequest(XmlDocument xmlDoc)
            {
                IServerAgent agent = GetServerAgent();
                PortAssignmentCommandRequest command = Spec.XMLtoMessage(xmlDoc) as PortAssignmentCommandRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("PortAssignmentCommandReply") as XmlDocument;
                PortAssignmentCommandReply reply = Spec.XMLtoMessage(xml_doc) as PortAssignmentCommandReply;
                string msgTemp = string.Empty;

                reply.BODY.LINENAME = command.BODY.LINENAME;
                Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                #region Line object check
                if (line == null)
                {
                    reply.RETURN.RETURNCODE = "0010990";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINENAME);

                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINE={0}] [BCS <- OPI][{1}] Can't find  Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                    return;
                }
                #endregion
                Invoke("CELLSpecialService", "PortAssignmentCommand", new object[] { line.Data.LINEID, command });
            }

            public void OPI_PPKLocalModeDenseDataRequest(XmlDocument xmlDoc)
            {
                IServerAgent agent = GetServerAgent();
                PPKLocalModeDenseDataRequest command = Spec.XMLtoMessage(xmlDoc) as PPKLocalModeDenseDataRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("PPKLocalModeDenseDataReply") as XmlDocument;
                PPKLocalModeDenseDataReply reply = Spec.XMLtoMessage(xml_doc) as PPKLocalModeDenseDataReply;

                try
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                    reply.BODY.LINENAME = command.BODY.LINENAME;

                    //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                    IList<Line> lines = ObjectManager.LineManager.GetLines();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                    if (lines == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010790";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                    }
                    else if (eqp == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010791";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                    }
                    else if (port == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                            command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010792";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.EQUIPMENTNO);
                    }
                    else if (string.IsNullOrEmpty(port.File.Mes_ValidateBoxReply))
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) doesn't has MES information.",
                            command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010793";
                        reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] doesn't has MES information.", command.BODY.PORTNO);
                    }
                    else
                    {
                        //取得MES所給的資訊
                        XmlDocument mesXml = new XmlDocument();
                        mesXml.LoadXml(port.File.Mes_ValidateBoxReply);

                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;

                        string returnCode = mesXml.SelectSingleNode("//MESSAGE/RETURN/RETURNCODE").InnerText;
                        string linerecipename = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                        string portid = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                        //string setcode = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
                        string producttype = "0";
                        
                        string boxid1 = string.Empty, boxid2 = string.Empty, grade1 = string.Empty, grade2 = string.Empty, boxType = string.Empty;
                        string cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0";

                        XmlNode boxlist = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];

                        int i = 0;
                        foreach (XmlNode box in boxlist)
                        {
                            Cassette cst = new Cassette();
                            if (box[keyHost.BOXTYPE].InnerText == "InBox")
                            {
                                boxType ="InBox";
                                cst.CassetteID = boxid1;
                                boxid1 = box[keyHost.BOXNAME].InnerText;
                            }
                            else
                            {
                                boxType ="OutBox";
                                cst.CassetteID = boxid2;
                                boxid2 = box[keyHost.BOXNAME].InnerText;
                            }
                            
                                reply.BODY.BOXID = boxid1;                            
                                reply.BODY.PAPER_BOXID = boxid2;
                                grade1 = box[keyHost.BOXGRADE].InnerText;
                                reply.BODY.JOBGRADE = grade1;
                                //cstsetcode1 = setcode;
                                //boxqty1 = box[keyHost.PRODUCTQUANTITY].InnerText;

                            cst.PortID = portid;
                            cst.PortNo = portid;
                            cst.LineRecipeName = linerecipename;
                            ObjectManager.CassetteManager.CreateBoxforPacking(cst);
                            i++;

                            XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;
                            if (productList.Count == 0)
                    {
                        List<string> items = new List<string>();

                        Job job = new Job(1, 0);
                        if (box[keyHost.OWNERTYPE].InnerText.Substring(box[keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                            && box[keyHost.OWNERID].InnerText != "RESD")
                        {
                            //Product Type
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            items.Add(box[keyHost.GROUPID].InnerText);
                            job.ProductType.SetItemData(items);
                        }
                        else
                        {
                            //Product Type
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            job.ProductType.SetItemData(items);
                        }
                        string err;
                        IList<Job> jobs = new List<Job>();
                        if (ObjectManager.JobManager.GetProductType(eFabType.CELL, box[keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                        {
                            producttype = job.ProductType.Value.ToString();
                        }
                    }
                            if (productList.Count > 0)
                            {
                                List<string> items = new List<string>();

                                int slotNo;
                                int.TryParse(productList[0][keyHost.POSITION].InnerText, out slotNo);
                                Job job = new Job(1, slotNo);

                                if (productList[0][keyHost.OWNERTYPE].InnerText.Substring(productList[0][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                    && productList[0][keyHost.OWNERID].InnerText != "RESD")
                                {
                                    //Product Type
                                    items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                                    items.Add(productList[i][keyHost.OWNERID].InnerText);
                                    items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                                    items.Add(productList[i][keyHost.GROUPID].InnerText);
                                    job.ProductType.SetItemData(items);
                                }
                                else
                                {
                                    //Product Type
                                    items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                                    items.Add(productList[i][keyHost.OWNERID].InnerText);
                                    items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                                    job.ProductType.SetItemData(items);
                                }

                                string err;
                                IList<Job> jobs = new List<Job>();
                                if (ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[0][keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                                {
                                    producttype = job.ProductType.Value.ToString();
                                }
                            }
                        }

                        reply.BODY.PRODUCTTYPE = producttype;
                        //TODO
            //            PaperBoxDataRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string portNO,
            //string boxID, string paperBoxID,eBoxType boxType, string productType, string grade)
                        
                        List<string> replylist = new List<string>();
                        replylist.AddRange(new string[] { returnCode, portid, boxid1, boxid2, boxType,producttype, grade1 });
                        //將資訊存在Dictionary中避免拆解兩次
                        string strKey = string.Format("{0}_{1}", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                        lock (dicDenseBoxData)
                        {
                            if (dicDenseBoxData.ContainsKey(strKey))
                                dicDenseBoxData[strKey] = replylist;
                            else
                                dicDenseBoxData.Add(strKey, replylist);
                        }
                    }

                    xMessage msg = SendReplyToOPI(command, reply);

                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
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
            /// OPI MessageSet: CIM Mode Change Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_FileDataCreateRequest(XmlDocument xmlDoc)
            {
                IServerAgent agent = GetServerAgent();
                FileDataCreateRequest command = Spec.XMLtoMessage(xmlDoc) as FileDataCreateRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("FileDataCreateReply") as XmlDocument;
                FileDataCreateReply reply = Spec.XMLtoMessage(xml_doc) as FileDataCreateReply;

                try
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. CassetteSeqNo={3}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                        command.BODY.CassetteSeqNo));

                    reply.BODY.LINENAME = command.BODY.LINENAME;

                    IList<Job> Jobs = new List<Job>();
                    Jobs = ObjectManager.JobManager.GetJobs(command.BODY.CassetteSeqNo);
                    Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                    eFabType FabType;
                    Enum.TryParse<eFabType>(line.Data.FABTYPE, out FabType);

                    string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, command.BODY.CassetteSeqNo);

                    if (Jobs.Count==0)
                    {
                        reply.BODY.Result = "NG";
                    }
                    else
                    {
                        reply.BODY.Result = "OK";
                        if (FabType != eFabType.CELL)
                        {
                            //20161123 Modify by huangjiayin--此功能之前AC使用会报错，修改为单个Job循环调用
                            //不能用List Job作为该function的参数
                            foreach (Job job in Jobs)
                            {
                                FileFormatManager.CreateFormatFile("ACShop", subPath, job, true);
                            }
                        }
                        else
                        {
                            foreach (Job job in Jobs)
                            {
                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, job });
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
            #endregion
      }
}
