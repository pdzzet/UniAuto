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
            #region Status
            /// <summary>
            /// OPI MessageSet: Each Position Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EachPositionRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EachPositionRequest command = Spec.XMLtoMessage(xmlDoc) as EachPositionRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EachPositionReply") as XmlDocument;
                  EachPositionReply reply = Spec.XMLtoMessage(xml_doc) as EachPositionReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, UNITNO={4}, PLCTRXNO={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.EQUIPMENTNO, command.BODY.UNITNO, command.BODY.PLCTRXNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.UNITNO = command.BODY.UNITNO;
                        reply.BODY.PLCTRXNO = command.BODY.PLCTRXNO;

                        Equipment node = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Line line = ObjectManager.LineManager.GetLine(node.Data.LINEID);
                        if (node == null)
                        {
                              reply.RETURN.RETURNCODE = "0010070";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0} in EquipmentEntity.)",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Trx positionTrx = new Trx();
                              string strName = string.Empty;
                              //bool bEqpQuery = false;

                              //20150210 cy:增加判斷種類
                              //判斷機台種類
                              switch (node.Data.REPORTMODE)
                              {
                                    //if (node.Data.REPORTMODE == "PLC")
                                    case "PLC":
                                    case "PLC_HSMS":
                                          {
                                                #region PLC Type EQ

                                                //REPORTPOSITIONNAME: 是否上報POSITION NAME- FOR SECS機台, Y:表示POSITION NO內填寫的內容為POSITION NAME, N:表示填寫POSITION NOs
                                                reply.BODY.REPORTPOSITIONNAME = "N";

                                                //若OPI給查詢EQ

                                              //Yang Add For Array PositionRequest
                                                #region Array Special
                                                if (line != null && line.Data.FABTYPE == eFabType.ARRAY.ToString())
                                                {
                                                    if (string.IsNullOrEmpty(command.BODY.EQUIPMENTNO)) break;
                                                    if (command.BODY.UNITNO == "0")
                                                    {
                                                        strName = string.Format("{0}_JobEachPositionBlock", command.BODY.EQUIPMENTNO);
                                                        positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                    }
                                                    else
                                                    {
                                                        if (command.BODY.PLCTRXNO == "00")
                                                        {
                                                            strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.UNITNO.PadLeft(2,'0'));
                                                            positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                            if (positionTrx == null)
                                                            {
                                                                strName = string.Format("{0}_JobEachPositionBlock", command.BODY.EQUIPMENTNO);
                                                                positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.PLCTRXNO);
                                                            positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;

                                                        }
                                                    }

                                                        if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;

                                                        if (positionTrx == null)
                                                        {
                                                            reply.RETURN.RETURNCODE = "0010071";
                                                            reply.RETURN.RETURNMESSAGE = string.Format("Can't get value from Trx[{0}]", strName);

                                                            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            string.Format("[Line={0}] [BCS <- OPI][{1}] TrxName is wrong, can't get Trx({2}) value.",
                                                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strName));
                                                        }
                                                        else
                                                        {
                                                            if (command.BODY.UNITNO != "0"&&command.BODY.PLCTRXNO == "00" && strName.Contains("_JobEachPositionBlock"))
                                                            {
                                                                int unitno=int.Parse(command.BODY.UNITNO);
                                                                EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                                pos.POSITIONNO = command.BODY.UNITNO;
                                                                pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[unitno*2-1].Value;
                                                                pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[unitno*2].Value;

                                                                Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                                if (job == null)
                                                                    pos.JOBID = "";
                                                                else
                                                                    pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                                reply.BODY.POSITIONLIST.Add(pos);
                                                            }
                                                            else
                                                            {
                                                                int iEqpPos = 1;
                                                                for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                                                {
                                                                    EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                                    //機台為00
                                                                    //pos.UNITNO = bEqpQuery ? "00" : command.BODY.PLCTRXNO;
                                                                    pos.POSITIONNO = iEqpPos.ToString();
                                                                    pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                                                    pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                                    Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                                    if (job == null)
                                                                        pos.JOBID = "";
                                                                    else
                                                                       pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                                    reply.BODY.POSITIONLIST.Add(pos);

                                                                    iEqpPos++;
                                                                }
                                                            }
                                                        }                                             
                                                }
                                                #endregion
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(command.BODY.EQUIPMENTNO) && command.BODY.UNITNO == "0")
                                                    {
                                                        strName = string.Format("{0}_JobEachPositionBlock", command.BODY.EQUIPMENTNO);
                                                        positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                        //bEqpQuery = true;
                                                    }
                                                    //OPI查詢Unit
                                                    else
                                                    {
                                                        strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.PLCTRXNO);
                                                        positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                    }

                                                    if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;

                                                    if (positionTrx == null)
                                                    {
                                                        reply.RETURN.RETURNCODE = "0010071";
                                                        reply.RETURN.RETURNMESSAGE = string.Format("Can't get value from Trx[{0}]", strName);

                                                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                        string.Format("[Line={0}] [BCS <- OPI][{1}] TrxName is wrong, can't get Trx({2}) value.",
                                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strName));
                                                    }
                                                    else
                                                    {


                                                        int iEqpPos = 1;
                                                        for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                                        {
                                                            EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                            //機台為00
                                                            //pos.UNITNO = bEqpQuery ? "00" : command.BODY.PLCTRXNO;
                                                            pos.POSITIONNO = iEqpPos.ToString();
                                                            pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                                            pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                            Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                            if (job == null)
                                                                pos.JOBID = "";
                                                            else
                                                                if (line != null && line.Data.LINETYPE == eLineType.CELL.CBMCL)
                                                                    pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).CellSpecial.MASKID;
                                                                else
                                                                    pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                            reply.BODY.POSITIONLIST.Add(pos);

                                                            iEqpPos++;
                                                        }
                                                        //顯示其餘Unit資訊
                                                        //if (bEqpQuery)
                                                        //{
                                                        //    for (int j = 0; j < node.Data.UNITCOUNT; j++)
                                                        //    {
                                                        //        string plcNo = j.ToString("00");
                                                        //        string unitTrxName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, plcNo);
                                                        //        Trx UnitTrx = new Trx();
                                                        //        UnitTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { unitTrxName }) as Trx;
                                                        //        if (UnitTrx != null) UnitTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { unitTrxName, true }) as Trx;
                                                        //        else
                                                        //        {
                                                        //            reply.RETURN.RETURNCODE = "0010073";
                                                        //            reply.RETURN.RETURNMESSAGE = string.Format("Can't Get Unit[{0}] Position Information", unitTrxName);

                                                        //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                        //            string.Format("[Line={0}] [BCS <- OPI][{1}] TrxName is wrong, Can't Get Unit[{0}] Position Information",
                                                        //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, unitTrxName));
                                                        //            break;
                                                        //        }

                                                        //        int iUnitPos = 1;
                                                        //        for (int i = 0; i < UnitTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                                        //        {
                                                        //            EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                        //            pos.UNITNO = plcNo;
                                                        //            pos.POSITIONNO = iUnitPos.ToString();
                                                        //            pos.CASSETTESEQNO = UnitTrx.EventGroups[0].Events[0].Items[i].Value;
                                                        //            pos.JOBSEQNO = UnitTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                        //            Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                        //            if (job == null)
                                                        //                pos.JOBID = "";
                                                        //            else
                                                        //                pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                        //            reply.BODY.POSITIONLIST.Add(pos);

                                                        //            iUnitPos++;
                                                        //        }
                                                        //    }
                                                        //}
                                                    }
                                                }
                                                #endregion
                                          }
                                          break;
                                    //else if (node.Data.REPORTMODE == "HSMS_CSOT")
                                    case "HSMS_CSOT":
                                    case "HSMS_PLC":
                                          {
                                                #region SECS Type EQ

                                                //使用SECS S6F11(CEID:15)取得結果， SECS機台沒有區分Unit
                                                List<Tuple<string, string, string>> positions = Repository.Get(string.Format("{0}_PositionInfo", node.Data.NODENO)) as List<Tuple<string, string, string>>;
                                                if (positions != null && positions.Count > 0)
                                                {
                                                      reply.BODY.REPORTPOSITIONNAME = "Y";

                                                      foreach (Tuple<string, string, string> _position in positions)
                                                      {
                                                            EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                            pos.POSITIONNO = _position.Item1;
                                                            pos.CASSETTESEQNO = _position.Item2;
                                                            pos.JOBSEQNO = _position.Item3;
                                                            Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                            if (job == null) pos.JOBID = "";
                                                            else pos.JOBID = job.GlassChipMaskBlockID;

                                                            reply.BODY.POSITIONLIST.Add(pos);
                                                      }
                                                }
                                                else //SECS不行改為試著取PLC的值
                                                {
                                                      reply.BODY.REPORTPOSITIONNAME = "N";

                                                      //若OPI給查詢EQ
                                                      #region Array Special
                                                      if (line != null && line.Data.FABTYPE == eFabType.ARRAY.ToString())
                                                      {
                                                          if (string.IsNullOrEmpty(command.BODY.EQUIPMENTNO)) break;
                                                          if (command.BODY.UNITNO == "0")
                                                          {
                                                              strName = string.Format("{0}_JobEachPositionBlock", command.BODY.EQUIPMENTNO);
                                                              positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                          }
                                                          else
                                                          {
                                                              if (command.BODY.PLCTRXNO == "00")
                                                              {
                                                                  strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.UNITNO.PadLeft(2,'0'));
                                                                  positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                                  if (positionTrx == null)
                                                                  {
                                                                      strName = string.Format("{0}_JobEachPositionBlock", command.BODY.EQUIPMENTNO);
                                                                      positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                                  }
                                                              }
                                                              else
                                                              {
                                                                  strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.PLCTRXNO);
                                                                  positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;

                                                              }
                                                          }

                                                              if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;

                                                              if (positionTrx == null)
                                                              {
                                                                  reply.RETURN.RETURNCODE = "0010072";
                                                                  reply.RETURN.RETURNMESSAGE = string.Format("Can't Get Equipment[{0}] Position Infomation", command.BODY.EQUIPMENTNO);

                                                                  Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                  string.Format("[Line={0}] [BCS <- OPI][{1}] Can't Get Equipment Position Infomation.",
                                                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                                                              }
                                                              else
                                                              {
                                                                  if (command.BODY.UNITNO != "0" && command.BODY.PLCTRXNO == "00" && strName.Contains("_JobEachPositionBlock"))
                                                                  {
                                                                      int unitno = int.Parse(command.BODY.UNITNO);
                                                                      EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                                      pos.POSITIONNO = command.BODY.UNITNO;
                                                                      pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[unitno * 2 - 1].Value;
                                                                      pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[unitno * 2].Value;

                                                                      Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                                      if (job == null)
                                                                          pos.JOBID = "";
                                                                      else
                                                                          pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                                      reply.BODY.POSITIONLIST.Add(pos);
                                                                  }
                                                                  else
                                                                  {
                                                                      int iEqpPos = 1;
                                                                      for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                                                      {
                                                                          EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                                          //機台為00
                                                                          //pos.UNITNO = bEqpQuery ? "00" : command.BODY.PLCTRXNO;
                                                                          pos.POSITIONNO = iEqpPos.ToString();
                                                                          pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                                                          pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                                          Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                                          if (job == null)
                                                                              pos.JOBID = "";
                                                                          else
                                                                             pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                                          reply.BODY.POSITIONLIST.Add(pos);

                                                                          iEqpPos++;
                                                                      }
                                                                  }
                                                              }
                                                      }
                                                      #endregion
                                                      else
                                                      {
                                                          if (!string.IsNullOrEmpty(command.BODY.EQUIPMENTNO) && string.IsNullOrEmpty(command.BODY.UNITNO))
                                                          {
                                                              strName = string.Format("{0}_JobEachPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.PLCTRXNO);
                                                              positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                          }
                                                          //OPI查詢Unit
                                                          else
                                                          {
                                                              strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", command.BODY.EQUIPMENTNO, command.BODY.PLCTRXNO);
                                                              positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                                          }

                                                          if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;

                                                          if (positionTrx == null)
                                                          {
                                                              reply.RETURN.RETURNCODE = "0010072";
                                                              reply.RETURN.RETURNMESSAGE = string.Format("Can't Get Equipment[{0}] Position Infomation", command.BODY.EQUIPMENTNO);

                                                              Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              string.Format("[Line={0}] [BCS <- OPI][{1}] Can't Get Equipment Position Infomation.",
                                                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                                                          }
                                                          else
                                                          {
                                                              int iPos = 1;
                                                              for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                                              {
                                                                  EachPositionReply.POSITIONc pos = new EachPositionReply.POSITIONc();
                                                                  pos.POSITIONNO = iPos.ToString();
                                                                  pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                                                  pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                                                  Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                                                  if (job == null) pos.JOBID = "";
                                                                  else
                                                                      pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;

                                                                  //if (command.BODY.UNITNO == "0")
                                                                  //    reply.BODY.POSITIONLIST.Add(pos);
                                                                  //else
                                                                  //{
                                                                  //    if (int.Parse(command.BODY.UNITNO) == iPos)
                                                                  //        reply.BODY.POSITIONLIST.Add(pos);
                                                                  //}
                                                                  reply.BODY.POSITIONLIST.Add(pos);

                                                                  iPos++;
                                                              }
                                                          }
                                                      }
                                                }

                                                #endregion
                                          }
                                          break;
                              }
                        }

                        if (reply.BODY.POSITIONLIST.Count > 0)
                              reply.BODY.POSITIONLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
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
            /// OPI MessageSet: Material Status Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_MaterialStatusReportRequest(XmlDocument xmlDoc)
            {
                  //20150421 cy add.
                  MaterialStatusReportRequest command = Spec.XMLtoMessage(xmlDoc) as MaterialStatusReportRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("MaterialStatusReportReply") as XmlDocument;
                  MaterialStatusReportReply reply = Spec.XMLtoMessage(xml_doc) as MaterialStatusReportReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Get Trx L5_MaterialStatusChangeReport {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0})", command.BODY.LINENAME)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010500";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010501";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              Dictionary<string, List<MaterialStatusReportReply.MATERIALc>> eqps = new Dictionary<string, List<MaterialStatusReportReply.MATERIALc>>();
                              #region 組material
                              List<MaterialEntity> materials = ObjectManager.MaterialManager.GetMaterials();
                              if (materials != null && materials.Count > 0)
                              {
                                    foreach (MaterialEntity m in materials)
                                    {
                                          if (string.IsNullOrEmpty(m.MaterialID.Trim()))
                                                continue;
                                          MaterialStatusReportReply.MATERIALc mrc = new MaterialStatusReportReply.MATERIALc();
                                          mrc.MATERIALNAME = m.MaterialID;
                                          mrc.MATERIALSTATUS = m.MaterialStatus.ToString();
                                          mrc.SLOTNO = m.MaterialSlotNo;
                                          mrc.MATERIALTYPE = m.MaterialType;

                                          if (eqps.ContainsKey(m.NodeNo))
                                                eqps[m.NodeNo].Add(mrc);
                                          else
                                          {
                                                List<MaterialStatusReportReply.MATERIALc> mrcs = new List<MaterialStatusReportReply.MATERIALc>();
                                                mrcs.Add(mrc);
                                                eqps.Add(m.NodeNo, mrcs);
                                          }
                                    }
                              }
                              #endregion
                              #region 組mask
                              List<MaterialEntity> masks = ObjectManager.MaterialManager.GetMasks();
                              if (masks != null && masks.Count > 0)
                              {
                                    foreach (MaterialEntity m in masks)
                                    {
                                          if (string.IsNullOrEmpty(m.MaterialID.Trim()))
                                                continue;
                                          MaterialStatusReportReply.MATERIALc mrc = new MaterialStatusReportReply.MATERIALc();
                                          mrc.MATERIALNAME = m.MaterialID;
                                          mrc.MATERIALSTATUS = m.MaterialStatus.ToString();
                                          mrc.SLOTNO = m.MaterialSlotNo;
                                          mrc.MATERIALTYPE = m.MaterialType;

                                          if (eqps.ContainsKey(m.NodeNo))
                                                eqps[m.NodeNo].Add(mrc);
                                          else
                                          {
                                                List<MaterialStatusReportReply.MATERIALc> mrcs = new List<MaterialStatusReportReply.MATERIALc>();
                                                mrcs.Add(mrc);
                                                eqps.Add(m.NodeNo, mrcs);
                                          }
                                    }
                              }
                              #endregion
                              #region 組回覆的資料
                              if (eqps.Count > 0)
                              {
                                    foreach (KeyValuePair<string, List<MaterialStatusReportReply.MATERIALc>> kvp in eqps)
                                    {
                                          MaterialStatusReportReply.EQUIPMENTc eqpc = new MaterialStatusReportReply.EQUIPMENTc();
                                          eqpc.EQUIPMENTNO = kvp.Key;
                                          eqpc.MATERIALLIST = kvp.Value;
                                          reply.BODY.EQUIPMENTLIST.Add(eqpc);
                                    }
                              }
                              #endregion

                              if (reply.BODY.EQUIPMENTLIST.Count > 0)
                                    reply.BODY.EQUIPMENTLIST.RemoveAt(0);
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "MaterialStatusReport OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: POL Material Status Request
            /// </summary>
            public void OPI_POLMaterialStatusRequest(XmlDocument xmlDoc)
            {
                  POLMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as POLMaterialStatusRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("POLMaterialStatusReply") as XmlDocument;
                  POLMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as POLMaterialStatusReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010501";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              List<MaterialEntity> materials = ObjectManager.MaterialManager.GetMaterials();
                              SortedDictionary<string, List<POLMaterialStatusReply.MATERIALc>> eqp_material_list = new SortedDictionary<string, List<POLMaterialStatusReply.MATERIALc>>();
                              foreach (MaterialEntity material in materials)
                              {
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(material.NodeNo);
                                    //20151113 cy:PAM的Line(POL,RWK),暫時只會有一台機台報Material,所以只判斷是否取到物件即可
                                    if (eqp == null) // || eqp.Data.NODEID.IndexOf(keyCellLineType.POL) != 0)
                                          continue;

                                    Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
                                    //20151116 cy:T3存放位置不同,改一下寫法
                                    material.CellPAMMateril.Sort((x, y) => { return x.MaterialPosition.CompareTo(y.MaterialPosition); });
                                    for (int i = 0; i < material.CellPAMMateril.Count; i++)
                                    {
                                          dic.Add((i + 1).ToString(), new string[] { material.MaterialID, material.MaterialPosition, material.MaterialCount });
                                    }
                                    //foreach (string cell_pol_material in material.CellPOLMaterial)
                                    //{
                                    //    //cell_pol_material = materialC.POSITION + "," + materialC.MATERIALNAME + "," + materialC.COUNT
                                    //    string[] strs = cell_pol_material.Split(',');
                                    //    if (strs.Length == 3)
                                    //    {
                                    //        strs[0] = strs[0].Trim().TrimStart('0');
                                    //        dic.Add(strs[0], strs);
                                    //    }
                                    //}

                                    POLMaterialStatusReply.MATERIALc polMaterial = new POLMaterialStatusReply.MATERIALc();
                                    polMaterial.UNITNO = material.UnitNo;
                                    polMaterial.MATERIALID = material.MaterialID;
                                    polMaterial.MATERIAL_STATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
                                    polMaterial.RECIPEID = material.MaterialRecipeID;
                                    //polMaterial.POSITION = material.MaterialPosition;
                                    //polMaterial.POLTYPE = material.MaterialType;
                                    if (dic.ContainsKey("1"))
                                    {
                                          polMaterial.LOTID01 = dic["1"][0];
                                          polMaterial.LOTNO01 = dic["1"][1];
                                          polMaterial.COUNT01 = dic["1"][2];
                                    }
                                    if (dic.ContainsKey("2"))
                                    {
                                          polMaterial.LOTID01 = dic["2"][0];
                                          polMaterial.LOTNO02 = dic["2"][1];
                                          polMaterial.COUNT02 = dic["2"][2];
                                    }
                                    if (dic.ContainsKey("3"))
                                    {
                                          polMaterial.LOTID01 = dic["3"][0];
                                          polMaterial.LOTNO03 = dic["3"][1];
                                          polMaterial.COUNT03 = dic["3"][2];
                                    }
                                    if (dic.ContainsKey("4"))
                                    {
                                          polMaterial.LOTID01 = dic["4"][0];
                                          polMaterial.LOTNO04 = dic["4"][1];
                                          polMaterial.COUNT04 = dic["4"][2];
                                    }
                                    if (dic.ContainsKey("5"))
                                    {
                                          polMaterial.LOTID01 = dic["5"][0];
                                          polMaterial.LOTNO05 = dic["5"][1];
                                          polMaterial.COUNT05 = dic["5"][2];
                                    }
                                    polMaterial.PORTID = material.MaterialPort;
                                    polMaterial.OPERATIONERID = material.OperatorID;

                                    if (eqp_material_list.ContainsKey(eqp.Data.NODENO))
                                    {
                                          eqp_material_list[eqp.Data.NODENO].Add(polMaterial);
                                    }
                                    else
                                    {
                                          eqp_material_list.Add(eqp.Data.NODENO, new List<POLMaterialStatusReply.MATERIALc>());
                                          eqp_material_list[eqp.Data.NODENO].Add(polMaterial);
                                    }
                              }

                              if (reply.BODY.EQUIPMENTLIST.Count > 0)
                                    reply.BODY.EQUIPMENTLIST.RemoveAt(0);

                              foreach (string eqp_no in eqp_material_list.Keys)
                              {
                                    POLMaterialStatusReply.EQUIPMENTc eqp = new POLMaterialStatusReply.EQUIPMENTc();
                                    eqp.EQUIPMENTNO = eqp_no;
                                    eqp.MATERIALLIST = eqp_material_list[eqp_no];
                                    reply.BODY.EQUIPMENTLIST.Add(eqp);
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: Real Time Glass Count Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            //public void OPI_RealTimeGlassCountRequest(XmlDocument xmlDoc)
            //{
            //    RealTimeGlassCountRequest command = Spec.XMLtoMessage(xmlDoc) as RealTimeGlassCountRequest;
            //    XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("RealTimeGlassCountReply") as XmlDocument;
            //    RealTimeGlassCountReply reply = Spec.XMLtoMessage(xml_doc) as RealTimeGlassCountReply;

            //    try
            //    {
            //        reply.BODY.LINENAME = command.BODY.LINENAME;

            //        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call CELLSpecialService.GlassCountRequest {3}",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
            //            string.Format("Set LINENAME({0})", command.BODY.LINENAME)));

            //        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //        IList<Line> lines = ObjectManager.LineManager.GetLines();
            //        if (lines == null)
            //        {
            //            reply.RETURN.RETURNCODE = "0010520";
            //            reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

            //            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //        }
            //        else if (command.BODY.LINENAME != ServerName)
            //        {
            //            reply.RETURN.RETURNCODE = "0010521";
            //            reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}]",
            //                command.BODY.LINENAME, ServerName);

            //            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI[{0}]/BCS[{2}]",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
            //        }
            //        else
            //        {
            //            //Do GlassCountRequest
            //            IList<RealGlassCount> result = (IList<RealGlassCount>)Invoke(eServiceName.CELLSpecialService, "GlassCountRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.LINENAME });
            //            if (result != null)
            //            {
            //                foreach (RealGlassCount item in result)
            //                {
            //                    if (item.IsReply)//機台有回的才回傳OPI
            //                    {
            //                        reply.BODY.EQUIPMENTLIST.Add(new RealTimeGlassCountReply.EQUIPMENTc()
            //                        {
            //                            EQUIPMENTNO = item.EqpNo,
            //                            ASSEMBLYTFTGLASSCNT = item.AssemblyTFTGlassCnt,
            //                            NOTASSEMBLYTFTGLASSCNT = item.NotAssemblyTFTGlassCnt,
            //                            CFGLASSCNT = item.CFGlassCnt,
            //                            THROUGHGLASSCNT = item.ThroughGlassCnt,
            //                            PIDUMMYGLASSCNT = item.PIDummyGlassCnt,
            //                            UVMASKGLASSCNT = item.UVMaskGlassCnt
            //                        });
            //                    }
            //                }
            //                if (reply.BODY.EQUIPMENTLIST.Count > 0)
            //                    reply.BODY.EQUIPMENTLIST.RemoveAt(0);
            //            }
            //            else
            //            {
            //                reply.RETURN.RETURNCODE = "0010522";
            //                reply.RETURN.RETURNMESSAGE = "Call CELLSpecialService.GlassCountRequest NG";

            //                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Call CELLSpecialService.GlassCountRequest NG",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //        }

            //        xMessage msg = SendReplyToOPI(command, reply);

            //        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
            //            reply.RETURN.RETURNCODE == "0000000" ? "Call CELLSpecialService.GlassCountRequest OK" : reply.RETURN.RETURNMESSAGE));
            //    }
            //    catch (Exception ex)
            //    {
            //        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Process Data Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ProcessDataReportRequest(XmlDocument xmlDoc)
            {
                  ProcessDataReportRequest command = Spec.XMLtoMessage(xmlDoc) as ProcessDataReportRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("ProcessDataReportReply") as XmlDocument;
                  ProcessDataReportReply reply = Spec.XMLtoMessage(xml_doc) as ProcessDataReportReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENT={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call ProcessDataService.ProcessDataRequestByNO {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),EQUIPMENTNO({1})", command.BODY.LINENAME, command.BODY.EQUIPMENTNO)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010320";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line[{0}] in LineEntity.",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010321";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match. OPI[{0}]/BCS[{1}]",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match. OPI({0})/BCS({2})",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else
                        {
                              object[] objParam = new object[] { command.BODY.EQUIPMENTNO, new List<string>(), string.Empty };
                              bool result = (bool)Invoke(eServiceName.ProcessDataService, "ProcessDataRequestByNO", objParam, new Type[] { typeof(string), typeof(List<string>).MakeByRefType(), typeof(string).MakeByRefType() });
                              if (result)
                              {
                                    foreach (string param in (List<string>)objParam[1])
                                    {
                                          string[] split = param.Split('=');
                                          reply.BODY.DATALIST.Add(new ProcessDataReportReply.DATAc() { NAME = split[0], VALUE = split[1] });
                                    }
                                    if (reply.BODY.DATALIST.Count > 0)
                                          reply.BODY.DATALIST.RemoveAt(0);
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010322";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Call ProcessDataService.ProcessDataRequestByNO NG {0}", objParam[2].ToString());

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Call ProcessDataService.ProcessDataRequestByNO NG {2}",
                                     command.BODY.LINENAME, command.HEADER.TRANSACTIONID, objParam[2].ToString()));
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call ProcessDataService.ProcessDataRequestByNO OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: Daily Check Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_DailyCheckReportRequest(XmlDocument xmlDoc)
            {
                  DailyCheckReportRequest command = Spec.XMLtoMessage(xmlDoc) as DailyCheckReportRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("DailyCheckReportReply") as XmlDocument;
                  DailyCheckReportReply reply = Spec.XMLtoMessage(xml_doc) as DailyCheckReportReply;

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call DailyCheckService.DailyCheckRequestByNO {3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            string.Format("Set LINENAME({0}),EQUIPMENTNO({1})", command.BODY.LINENAME, command.BODY.EQUIPMENTNO)));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment node = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010420";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line[{0}] in LineEntity.",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010421";
                              reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name isn't match, OPI[{0}]/BCS[{1}].",
                                  command.BODY.LINENAME, ServerName);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({0})/BCS({2})",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, ServerName));
                        }
                        else if (node == null)
                        {
                              reply.RETURN.RETURNCODE = "0010423";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Equipment({2}) in EquipmentEntity!",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO));
                        }
                        else
                        {
                              object[] objParam = new object[] { command.BODY.EQUIPMENTNO, new List<string>(), string.Empty };
                              bool result = (bool)Invoke(eServiceName.DailyCheckService, "DailyCheckRequestByNO", objParam, new Type[] { typeof(string), typeof(List<string>).MakeByRefType(), typeof(string).MakeByRefType() });
                              if (result)
                              {
                                    foreach (string param in (List<string>)objParam[1])
                                    {
                                          string[] split = param.Split('=');
                                          reply.BODY.DATALIST.Add(new DailyCheckReportReply.DATAc() { NAME = split[0], VALUE = split[1] });
                                    }
                                    if (reply.BODY.DATALIST.Count > 0)
                                          reply.BODY.DATALIST.RemoveAt(0);
                              }
                              else
                              {
                                    reply.RETURN.RETURNCODE = "0010422";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Call DailyCheckService.DailyCheckRequestByNO NG {0}", objParam[2].ToString());

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Call DailyCheckService.DailyCheckRequestByNO NG {2}",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, objParam[2].ToString()));
                              }

                              //20150225 底層已經自己區分判斷REPORTMODE
                              //if (node.Data.REPORTMODE == "PLC" || node.Data.REPORTMODE == "PLC_HSMS")
                              //{
                              //    #region PLC Type EQ

                              //    object[] objParam = new object[] { command.BODY.EQUIPMENTNO, new List<string>(), string.Empty };
                              //    bool result = (bool)Invoke(eServiceName.DailyCheckService, "DailyCheckRequestByNO", objParam, new Type[] { typeof(string), typeof(List<string>).MakeByRefType(), typeof(string).MakeByRefType() });
                              //    if (result)
                              //    {
                              //        foreach (string param in (List<string>)objParam[1])
                              //        {
                              //            string[] split = param.Split('=');
                              //            reply.BODY.DATALIST.Add(new DailyCheckReportReply.DATAc() { NAME = split[0], VALUE = split[1] });
                              //        }
                              //        if (reply.BODY.DATALIST.Count > 0)
                              //            reply.BODY.DATALIST.RemoveAt(0);
                              //    }
                              //    else
                              //    {
                              //        reply.RETURN.RETURNCODE = "0010422";
                              //        reply.RETURN.RETURNMESSAGE = string.Format("Call DailyCheckService.DailyCheckRequestByNO NG {0}", objParam[2].ToString());

                              //        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              //        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Call DailyCheckService.DailyCheckRequestByNO NG {2}",
                              //        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, objParam[2].ToString()));
                              //    }

                              //    #endregion
                              //}
                              //else if (node.Data.REPORTMODE == "HSMS_CSOT" || node.Data.REPORTMODE == "HSMS_NIKON" || node.Data.REPORTMODE == "HSMS_PLC")
                              //{
                              //    #region SECS Type EQ

                              //    List<Tuple<string, List<Tuple<string, string, string>>>> checks = Repository.Get(string.Format("{0}_{1}_SecsDailyCheck", node.Data.LINEID, node.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                              //    if (checks != null)
                              //    {
                              //        foreach (Tuple<string, List<Tuple<string, string, string>>> _unit in checks)
                              //        {
                              //            for (int i = 0; i < _unit.Item2.Count; i++)
                              //            {
                              //                DailyCheckReportReply.DATAc data = new DailyCheckReportReply.DATAc();
                              //                data.NAME = string.Format("{0}_{1}", _unit.Item1, _unit.Item2[i].Item1);
                              //                data.VALUE = _unit.Item2[i].Item3;

                              //                reply.BODY.DATALIST.Add(data);
                              //            }
                              //        }
                              //    }
                              //    else
                              //    {
                              //        reply.RETURN.RETURNCODE = "0010424";
                              //        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] DailyCheck Data", command.BODY.EQUIPMENTNO);

                              //        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              //        string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment[{0}] DailyCheck Data.",
                              //        command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                              //    }

                              //    if (reply.BODY.DATALIST.Count > 0)
                              //        reply.BODY.DATALIST.RemoveAt(0);

                              //    #endregion
                              //}
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call DailyCheckService.DailyCheckRequestByNO OK" : reply.RETURN.RETURNMESSAGE));
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
            /// OPI MessageSet: Recipe Parameter Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_RecipeParameterReportRequest(XmlDocument xmlDoc)
            {
                  RecipeParameterReportRequest command = Spec.XMLtoMessage(xmlDoc) as RecipeParameterReportRequest;
                  XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("RecipeParameterReportRequestReply") as XmlDocument;
                  RecipeParameterReportRequestReply reply = Spec.XMLtoMessage(xml_doc) as RecipeParameterReportRequestReply;

                  IList<RecipeCheckInfo> recipeCheckInfoList = new List<RecipeCheckInfo>();

                  try
                  {
                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.RECIPELINENAME = command.BODY.RECIPELINENAME;

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI , Call RecipeService.RecipeParametreRequestCommandFromUI",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        //確認RecipeService是否正在處理其他命令
                        bool bCheck = (bool)Invoke(eServiceName.RecipeService, "CheckRecipeParameterCommand", new object[] { });
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010560";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (command.BODY.LINENAME != ServerName)
                        {
                              reply.RETURN.RETURNCODE = "0010561";
                              reply.RETURN.RETURNMESSAGE = "OPI and BCS line name isn't match";

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI and BCS line name isn't match, OPI({2}) / BCS({3})",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.LINENAME, ServerName));
                        }
                        else if (!bCheck)
                        {
                              reply.RETURN.RETURNCODE = "0010562";
                              reply.RETURN.RETURNMESSAGE = "RecipeService is busy, please wait for a while and retry";

                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] RecipeService is busy, please wait for a while and retry",
                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI , {3}", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME,
                            reply.RETURN.RETURNCODE == "0000000" ? "Call RecipeService.RecipeParametreRequestCommandFromUI OK" : reply.RETURN.RETURNMESSAGE));

                        //需先回Reply給OPI
                        if (bCheck && reply.RETURN.RETURNCODE == "0000000")
                        {
                              foreach (RecipeParameterReportRequest.EQUIPMENTc eqp in command.BODY.EQUIPMENTLIST)
                              {
                                    recipeCheckInfoList.Add(new RecipeCheckInfo()
                                    {
                                          EQPNo = eqp.EQUIPMENTNO,
                                          //PortNo = Convert.ToInt32(command.BODY.PORTNO),
                                          RecipeNo = eqp.RECIPENO,
                                          RecipeID = eqp.RECIPENO,
                                          TrxId = command.HEADER.TRANSACTIONID
                                    });
                              }

                              RecipeParameterReturnReport(command, recipeCheckInfoList);
                        }
                        //else
                        //{
                        //    BCSTerminalMessageInform(command.HEADER.TRANSACTIONID, command.BODY.LINENAME, "Call RecipeService.RecipeParametreRequestCommandFromUI NG");
                        //    //reply.RETURN.RETURNCODE = "0010563";
                        //    //reply.RETURN.RETURNMESSAGE = "Call RecipeService.RecipeParametreRequestCommandFromUI NG";
                        //}
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
            /// OPI MessageSet: Recipe Parameter Return Report
            /// </summary>
            /// <param name="lineID"></param>
            /// <param name="recipeCheckInfoList"></param>
            public void RecipeParameterReturnReport(RecipeParameterReportRequest cmd, IList<RecipeCheckInfo> recipeCheckInfoList)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("RecipeParameterReturnReport") as XmlDocument;
                        RecipeParameterReturnReport trx = Spec.XMLtoMessage(xml_doc) as RecipeParameterReturnReport;
                        trx.BODY.LINENAME = cmd.BODY.LINENAME;
                        trx.BODY.RECIPELINENAME = cmd.BODY.RECIPELINENAME;

                        object[] objParam = new object[] { cmd.HEADER.TRANSACTIONID, recipeCheckInfoList };
                        //Do RecipeParametreRequestCommandFromUI
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Invoke RecipeService.RecipeParametreRequestCommandFromUI",
                            trx.BODY.LINENAME, cmd.HEADER.TRANSACTIONID));

                        bool result = (bool)Invoke(eServiceName.RecipeService, "RecipeParametreRequestCommandFromUI", objParam);

                        if (!result)
                        {
                              //通知OPI異常
                              BCSTerminalMessageInform(cmd.HEADER.TRANSACTIONID, cmd.BODY.LINENAME, "Call RecipeService.RecipeParametreRequestCommandFromUI NG");

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Call RecipeService.RecipeParametreRequestCommandFromUI NG",
                              trx.BODY.LINENAME, cmd.HEADER.TRANSACTIONID));
                              return;
                        }
                        else
                        {
                              //20151229 cy:Invoke RecipeService時, Service就會阻塞了, 所以收到result時, 表示都做完了, 不用再等一次
                              //需等待一段時間，才可取到Parameters，因為無法判斷所需時間，所以取Timeout時間做為最大化的時間
                              //Thread.Sleep(ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger());

                              for (int i = 0; i < recipeCheckInfoList.Count; i++)
                              {
                                    RecipeParameterReturnReport.EQUIPMENTc eqp = new OpiSpec.RecipeParameterReturnReport.EQUIPMENTc();

                                    Equipment node = ObjectManager.EquipmentManager.GetEQP(recipeCheckInfoList[i].EQPNo);

                                    IList<RecipeParameter> recipeParams = ObjectManager.RecipeManager.GetRecipeParameter(recipeCheckInfoList[i].EQPNo);
                                    eqp.EQUIPMENTNO = recipeCheckInfoList[i].EQPNo;
                                    eqp.RECIPENO = recipeCheckInfoList[i].RecipeNo;
                                    //eqp.RETURN = recipeCheckInfoList[i].Result.ToString();
                                    //eqp.NGMESSAGE = recipeCheckInfoList[i].MESVALICODEText == null ? "" : recipeCheckInfoList[i].MESVALICODEText;
                                    switch (recipeCheckInfoList[i].Result)
                                    {
                                          case eRecipeCheckResult.OK:
                                                eqp.RETURN = "OK";
                                                eqp.NGMESSAGE = string.Empty;
                                                break;
                                          case eRecipeCheckResult.NG:
                                          case eRecipeCheckResult.MACHINENG:
                                          case eRecipeCheckResult.NOMACHINE:
                                          case eRecipeCheckResult.RECIPELENNG:
                                          case eRecipeCheckResult.TIMEOUT:
                                                eqp.RETURN = "NG";
                                                eqp.NGMESSAGE = recipeCheckInfoList[i].Result.ToString();
                                                break;
                                          case eRecipeCheckResult.ZERO:
                                          case eRecipeCheckResult.CIMOFF:
                                          case eRecipeCheckResult.NOCHECK:
                                                eqp.RETURN = "NONE";
                                                eqp.NGMESSAGE = recipeCheckInfoList[i].Result.ToString();
                                                break;
                                    }

                                    if (node != null)
                                    {
                                          if ((node.Data.REPORTMODE == "PLC" || node.Data.REPORTMODE == "PLC_HSMS")&&!(node.Data.LINEID.Contains("DRY")&&node.Data.REPORTMODE=="PLC_HSMS"))
                                          {
                                                #region PLC Type

                                                if (recipeCheckInfoList[i].Parameters != null)
                                                {
                                                      int index = 0;
                                                      foreach (KeyValuePair<string, string> item in recipeCheckInfoList[i].Parameters)
                                                      {
                                                            RecipeParameterReturnReport.DATAc data = new RecipeParameterReturnReport.DATAc();

                                                            data.NO = recipeParams[index].Data.SVID;
                                                            data.PARAMETERNAME = recipeParams[index].Data.PARAMETERNAME;
                                                            data.VALUE = item.Value.Trim();
                                                            data.EXPRESSION = recipeParams[index].Data.EXPRESSION;

                                                            eqp.DATALIST.Add(data);
                                                            index++;
                                                      }
                                                }

                                                #endregion
                                          }
                                          else
                                          {
                                                #region SECS Type

                                                if (recipeCheckInfoList[i].Parameters != null)
                                                {
                                                      //20150304 cy:修改判斷DB不存在資料的做法
                                                      if (recipeParams == null)
                                                      {
                                                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Can't find recipe parameter in BCSDB. (EQUIPMENT={2})", trx.BODY.LINENAME, cmd.HEADER.TRANSACTIONID, node.Data.NODENO));
                                                      }

                                                      foreach (KeyValuePair<string, string> item in recipeCheckInfoList[i].Parameters)
                                                      {
                                                            //SECS機台key組成：EQID@UNITID^PARANAME
                                                            //20150326 cy:改parameter name組成
                                                            string pName = string.Empty;//item.Key.Substring(item.Key.IndexOf('_', item.Key.IndexOf('_') + 1) + 1);
                                                            int p = item.Key.IndexOf('^');
                                                            if (p > 0)
                                                                  pName = item.Key.Substring(p + 1, item.Key.Length - p - 1);
                                                            else
                                                            {
                                                                  p = item.Key.IndexOf("@");
                                                                  if (p > 0)
                                                                        pName = item.Key.Substring(p + 1, item.Key.Length - p - 1);
                                                                  else
                                                                        pName = item.Key;
                                                            }

                                                            RecipeParameterReturnReport.DATAc data = new RecipeParameterReturnReport.DATAc();
                                                            //以item.key完整名稱回覆，方便查詢
                                                            data.PARAMETERNAME = item.Key;
                                                            data.VALUE = item.Value.Trim();

                                                            if (recipeParams != null)
                                                            {
                                                                  RecipeParameter parameter = recipeParams.FirstOrDefault(t => t.Data.PARAMETERNAME == pName);
                                                                  if (parameter != null)
                                                                  {
                                                                        data.NO = parameter.Data.SVID;
                                                                        data.EXPRESSION = parameter.Data.EXPRESSION;
                                                                  }
                                                                  else
                                                                  {
                                                                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] ParameterName({2}) is invalid, can't find this parameter in BCSDB.",
                                                                            trx.BODY.LINENAME, cmd.HEADER.TRANSACTIONID, pName));
                                                                  }
                                                            }
                                                            eqp.DATALIST.Add(data);

                                                            ////SECS機台key組成：EqpID_SubRecipe_ParameterName
                                                            //string[] keys = item.Key.Split('_');

                                                            //if (recipeParams != null)
                                                            //{
                                                            //    //因為SECS上報順序不一定會一樣,需比對ParameterName
                                                            //    bool bParameterCheck = false;
                                                            //    foreach (RecipeParameter parameter in recipeParams)
                                                            //    {
                                                            //        if (parameter.Data.PARAMETERNAME == keys[2])
                                                            //        {
                                                            //            RecipeParameterReturnReport.DATAc data = new RecipeParameterReturnReport.DATAc();

                                                            //            data.NO = parameter.Data.SVID;
                                                            //            //以item.key完整名稱回覆，方便查詢
                                                            //            data.PARAMETERNAME = item.Key;
                                                            //            data.VALUE = item.Value.Trim();
                                                            //            data.EXPRESSION = parameter.Data.EXPRESSION;

                                                            //            eqp.DATALIST.Add(data);
                                                            //            bParameterCheck = true;
                                                            //            break;
                                                            //        }
                                                            //    }
                                                            //    //記錄異常情況的Log
                                                            //    if (!bParameterCheck)
                                                            //    {
                                                            //        //找不到DB中相對的ParameterName，則只報Name跟value
                                                            //        RecipeParameterReturnReport.DATAc data = new RecipeParameterReturnReport.DATAc();
                                                            //        data.PARAMETERNAME = item.Key;
                                                            //        data.VALUE = item.Value.Trim();
                                                            //        eqp.DATALIST.Add(data);

                                                            //        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            //        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] ParameterName({2}) is invalid, can't find this parameter in BCSDB.",
                                                            //        trx.BODY.LINENAME, trxID, keys[2]));
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //    RecipeParameterReturnReport.DATAc data = new RecipeParameterReturnReport.DATAc();
                                                            //    data.PARAMETERNAME = item.Key;
                                                            //    data.VALUE = item.Value.Trim();
                                                            //    eqp.DATALIST.Add(data);

                                                            //    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                            //    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] ParameterName({2}) is invalid, can't find this parameter in BCSDB.",
                                                            //    trx.BODY.LINENAME, trxID, keys[2]));
                                                            //}
                                                      }
                                                }

                                                #endregion
                                          }
                                    }
                                    else
                                    {
                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINENAME={0}] [BCS -> OPI][{1}] RecipeCheckInfoList EQPNO({2}) is invalid.",
                                          trx.BODY.LINENAME, cmd.HEADER.TRANSACTIONID, recipeCheckInfoList[i].EQPNo));
                                    }

                                    trx.BODY.EQUIPMENTLIST.Add(eqp);

                                    //if (recipeCheckInfoList[i].Result == eRecipeCheckResult.OK)
                                    //{
                                    //    IList<RecipeParameter> recipeParams = ObjectManager.RecipeManager.GetRecipeParameter(recipeCheckInfoList[i].EQPNo);

                                    //    string recipeIDName = string.Empty;
                                    //    string recipeVersionName = string.Empty;
                                    //    string portNoName = string.Empty;

                                    //    if (recipeParams != null)
                                    //    {
                                    //        recipeIDName = recipeParams.Where(r => r.Data.WOFFSET == "0").Select(r => r.Data.PARAMETERNAME).FirstOrDefault();
                                    //        recipeVersionName = recipeParams.Where(r => r.Data.WOFFSET == "6").Select(r => r.Data.PARAMETERNAME).FirstOrDefault();
                                    //        portNoName = recipeParams.Where(r => r.Data.WOFFSET == "9").Select(r => r.Data.PARAMETERNAME).FirstOrDefault();
                                    //    }

                                    //        trx.BODY.RECIPEID = recipeCheckInfoList[i].Parameters.Where(r => r.Key.Contains(recipeIDName)).Select(r => r.Value).FirstOrDefault();
                                    //        trx.BODY.RECIPEVERSION = recipeCheckInfoList[i].Parameters.Where(r => r.Key.Contains(recipeVersionName)).Select(r => r.Value).FirstOrDefault();
                                    //        trx.BODY.PORTNO = recipeCheckInfoList[i].Parameters.Where(r => r.Key.Contains(portNoName)).Select(r => r.Value).FirstOrDefault();

                                    //        trx.BODY.RECIPEID = trx.BODY.RECIPEID == null ? "" : trx.BODY.RECIPEID.Trim();
                                    //        trx.BODY.RECIPEVERSION = trx.BODY.RECIPEVERSION == null ? "" : trx.BODY.RECIPEVERSION;
                                    //        trx.BODY.PORTNO = trx.BODY.PORTNO == null ? "" : trx.BODY.PORTNO;

                                    //        if (recipeCheckInfoList[i].Parameters != null)
                                    //        {
                                    //            foreach (KeyValuePair<string, string> item in recipeCheckInfoList[i].Parameters)
                                    //            {
                                    //                if (item.Key.Contains(recipeIDName) | item.Key.Contains(recipeVersionName) | item.Key.Contains(portNoName))
                                    //                {
                                    //                    continue;
                                    //                }
                                    //                else
                                    //                {
                                    //                    trx.BODY.DATALIST.Add(new RecipeParameterReturnReport.DATAc() { PARAMETERNAME = item.Key, VALUE = item.Value });
                                    //                }
                                    //            }
                                    //            if (trx.BODY.DATALIST.Count > 0)
                                    //                trx.BODY.DATALIST.RemoveAt(0);
                                    //        }
                                    //    }
                                    //}
                              }
                        }

                        if (trx.BODY.EQUIPMENTLIST.Count > 0)
                              trx.BODY.EQUIPMENTLIST.RemoveAt(0);

                        xMessage msg = SendToOPI(cmd.HEADER.TRANSACTIONID, trx, new List<string>(1) { cmd.HEADER.REPLYSUBJECTNAME });

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report transaction({2}) to OPI.",
                            cmd.BODY.LINENAME, msg.TransactionID, trx.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            /// <summary>
            /// OPI MessageSet: Equipment Alarm Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentAlarmStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentAlarmStatusRequest command = Spec.XMLtoMessage(xmlDoc) as EquipmentAlarmStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentAlarmStatusReply") as XmlDocument;
                  EquipmentAlarmStatusReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentAlarmStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010310";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line{0} in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        //reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        //EquipmentNo = "00" 則回傳全部Equipment的結果
                        if (command.BODY.EQUIPMENTNO == "00")
                        {
                              #region 回傳全部機台結果

                              IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                              foreach (Equipment eqp in eqps)
                              {
                                    EquipmentAlarmStatusReply.EQUIPMENTc _eqp = new EquipmentAlarmStatusReply.EQUIPMENTc();
                                    _eqp.EQUIPMENTNO = eqp.Data.NODENO;

                                    AlarmHistoryFile alarmFile = ObjectManager.AlarmManager.GetEQPAlarm(eqp.Data.NODENO);
                                    if (alarmFile != null)
                                    {
                                          foreach (KeyValuePair<string, HappeningAlarm> alarm in alarmFile.HappingAlarms)
                                          {
                                                EquipmentAlarmStatusReply.ALARMc _alarm = new EquipmentAlarmStatusReply.ALARMc();
                                                _alarm.ALARMUNIT = alarm.Value.Alarm.UNITNO;
                                                _alarm.ALARMID = alarm.Value.Alarm.ALARMID;
                                                _alarm.ALARMCODE = alarm.Value.Alarm.ALARMCODE;
                                                _alarm.ALARMLEVEL = alarm.Value.Alarm.ALARMLEVEL;
                                                _alarm.ALARMTEXT = alarm.Value.Alarm.ALARMTEXT;

                                                _eqp.ALARMLIST.Add(_alarm);
                                          }

                                          reply.BODY.EQUIPMENTLIST.Add(_eqp);
                                    }
                              }

                              #endregion
                        }
                        else
                        {
                              #region 回傳特定機台結果

                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                              EquipmentAlarmStatusReply.EQUIPMENTc _eqp = new EquipmentAlarmStatusReply.EQUIPMENTc();
                              _eqp.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                              AlarmHistoryFile alarmFile = ObjectManager.AlarmManager.GetEQPAlarm(command.BODY.EQUIPMENTNO);
                              if (alarmFile != null)
                              {
                                    foreach (KeyValuePair<string, HappeningAlarm> alarm in alarmFile.HappingAlarms)
                                    {
                                          EquipmentAlarmStatusReply.ALARMc _alarm = new EquipmentAlarmStatusReply.ALARMc();
                                          _alarm.ALARMUNIT = alarm.Value.Alarm.UNITNO;
                                          _alarm.ALARMID = alarm.Value.Alarm.ALARMID;
                                          _alarm.ALARMCODE = alarm.Value.Alarm.ALARMCODE;
                                          _alarm.ALARMLEVEL = alarm.Value.Alarm.ALARMLEVEL;
                                          _alarm.ALARMTEXT = alarm.Value.Alarm.ALARMTEXT;

                                          _eqp.ALARMLIST.Add(_alarm);
                                    }
                              }

                              reply.BODY.EQUIPMENTLIST.Add(_eqp);

                              #endregion
                        }

                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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
            /// OPI MessageSet: All Equipment Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_AllEquipmentStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  AllEquipmentStatusRequest command = Spec.XMLtoMessage(xmlDoc) as AllEquipmentStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("AllEquipmentStatusReply") as XmlDocument;
                  AllEquipmentStatusReply reply = Spec.XMLtoMessage(xml_doc) as AllEquipmentStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010410";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line{0} in LineEntity", command.BODY.LINENAME);

                              Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        foreach (Equipment node in ObjectManager.EquipmentManager.GetEQPs())
                        {
                              //if (node.Data.LINEID == line.Data.LINEID)
                              //{
                              AllEquipmentStatusReply.EQUIPMENTc eqp = new AllEquipmentStatusReply.EQUIPMENTc();
                              eqp.EQUIPMENTNO = node.Data.NODENO;

                              //string aliveName = string.Format("{0}_EquipmentAlive", node.Data.NODENO);
                              //Trx aliveTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { aliveName }) as Trx;
                              //if (aliveTrx != null) aliveTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { aliveName,true }) as Trx;
                              //eqp.EQUIPMENTALIVE = aliveTrx == null ? "0" : aliveTrx.EventGroups[0].Events[0].Items[0].Value;
                              //20150130 modify by edison:昌爷要求，取EQENTITY的AliveTimeout报给OPI
                              eqp.EQUIPMENTALIVE = node.File.AliveTimeout == true ? "0" : "1";

                              eqp.CURRENTSTATUS = ((int)node.File.Status).ToString() == "" ? "0" : ((int)node.File.Status).ToString();
                              eqp.CIMMODE = ((int)node.File.CIMMode).ToString() == "" ? "0" : ((int)node.File.CIMMode).ToString();
                              eqp.HSMSSTATUS = node.HsmsConnStatus;
                              eqp.HSMSCONTROLMODE = node.File.HSMSControlMode;
                              eqp.CSTOPERMODE = node.File.CSTOperationMode.ToString();
                              eqp.UPSTREAMINLINEMODE = node.File.UpstreamInlineMode.ToString();
                              eqp.DOWNSTREAMINLINEMODE = node.File.DownstreamInlineMode.ToString();
                              eqp.AUTORECIPECHANGEMODE = node.File.AutoRecipeChangeMode.ToString();
                              eqp.PARTIALFULLMODE = ((int)node.File.PartialFullMode).ToString();
                              eqp.BYPASSMODE = node.File.BypassMode.ToString();
                              eqp.CURRENTRECIPEID = node.File.CurrentRecipeID.Trim();

                              ////CBPMT Line的Loader(L2)有兩個EquipmentRunMode，需要特殊處理
                              //if ((node.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI) || node.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                              //    && node.Data.NODENO == "L2")
                              //{
                              //      Line line1 = ObjectManager.LineManager.GetLine("CBPMI500");
                              //      Line line2 = ObjectManager.LineManager.GetLine("CBPTI100");

                              //      eqp.EQUIPMENTRUNMODE = line1.File.CellLineOperMode;
                              //      eqp.EQUIPMENTRUNMODE2 = line2.File.CellLineOperMode;
                              //}
                              //else
                              //      eqp.EQUIPMENTRUNMODE = node.File.EquipmentRunMode;

                              eqp.EQUIPMENTRUNMODE = node.File.EquipmentRunMode;

                              eqp.TFTJOBCNT = node.File.TotalTFTJobCount.ToString();
                              eqp.CFJOBCNT = node.File.TotalCFProductJobCount.ToString();
                              eqp.DMYJOBCNT = node.File.TotalDummyJobCount.ToString();
                              eqp.THROUGHDMYJOBCNT = node.File.ThroughDummyJobCount.ToString();
                              eqp.THICKNESSDMYJOBCNT = node.File.ThicknessDummyJobCount.ToString();
                              eqp.UNASSEMBLEDTFTDMYJOBCNT = node.File.TotalUnassembledTFTJobCount.ToString();//sy add 20160826
                              eqp.ITODMYJOBCNT = node.File.TotalITODummyJobCount.ToString();//sy add 20160826
                              eqp.NIPDMYJOBCNT = node.File.TotalNIPDummyJobCount.ToString();//sy add 20160826
                              eqp.METALONEDMYJOBCNT = node.File.TotalMetalOneDummyJobCount.ToString();//sy add 20160826
                              eqp.UVMASKJOBCNT = node.File.UVMASKJobCount.ToString();
                              eqp.TURNTABLEMODE = ((int)node.File.TurnTableMode).ToString() == "" ? "0" : ((int)node.File.TurnTableMode).ToString();
                              eqp.LOCALALARMSTATUS = node.File.LocalAlarmStatus.ToString();
                              eqp.CASSETTEQTIME = node.File.CassetteonPortQTime.ToString();
                              if (node.Data.NODEATTRIBUTE.Equals("IN"))  //qiumin add 20180831 
                              {
                                  eqp.INSPECTIONIDLETIME = node.File.InspectionIdleTime.ToString();
                              }
                              else 
                              {
                                  eqp.INSPECTIONIDLETIME = node.File.EqProcessTimeSetting.ToString();
                              }
                              eqp.EQPOPERATIONMODE = ((int)node.File.EquipmentOperationMode).ToString() == "" ? "0" : ((int)node.File.EquipmentOperationMode).ToString();
                              eqp.BYPASSINSP01MODE = ((int)node.File.BypassInspectionEquipment01Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment01Mode).ToString();
                              eqp.BYPASSINSP02MODE = ((int)node.File.BypassInspectionEquipment02Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment02Mode).ToString();
                              eqp.HIGHCVMODE = ((int)node.File.HighCVMode).ToString() == "" ? "0" : ((int)node.File.HighCVMode).ToString();
                              eqp.NEXTLINEBCSTATUS = ((int)node.File.NextLineBCStatus).ToString() == "" ? "0" : ((int)node.File.NextLineBCStatus).ToString();
                              eqp.JOBDATACHECKMODE = ((int)node.File.JobDataCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDataCheckMode).ToString();
                              eqp.COAVERSIONCHECKMODE = ((int)node.File.COAVersionCheckMode).ToString() == "" ? "0" : ((int)node.File.COAVersionCheckMode).ToString();
                              eqp.JOBDUPLICATECHECKMODE = ((int)node.File.JobDuplicateCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDuplicateCheckMode).ToString();
                              eqp.PRODUCTIDCHECKMODE = ((int)node.File.ProductIDCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductIDCheckMode).ToString();
                              eqp.GROUPINDEXCHECKMODE = ((int)node.File.GroupIndexCheckMode).ToString() == "" ? "0" : ((int)node.File.GroupIndexCheckMode).ToString();
                              eqp.RECIPEIDCHECKMODE = ((int)node.File.RecipeIDCheckMode).ToString() == "" ? "0" : ((int)node.File.RecipeIDCheckMode).ToString();
                              eqp.PRODUCTTYPECHECKMODE = ((int)node.File.ProductTypeCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductTypeCheckMode).ToString();
                              //eqp.LOADEROPERATIONMODE_ATS = ((int)node.File.ATSLoaderOperMode).ToString() == "" ? "0" : ((int)node.File.ATSLoaderOperMode).ToString();
                              eqp.LASTGLASSID = node.File.FinalReceiveGlassID;
                              eqp.LASTRECIVETIME = node.File.FinalReceiveGlassTime;
                              eqp.TIMEOUTFLAG = node.File.TactTimeOut ? "1" : "0";
                              eqp.OXINFOCHECKFLAG = node.File.OxinfoCheckFlag?"1":"0";  //add by qiumin 20180607
                              if (node.Data.NODENO == "L3")
                              {
                                    switch (lines[0].Data.LINETYPE)
                                    {
                                          case eLineType.ARRAY.WEI_DMS:
                                          case eLineType.ARRAY.WET_DMS:
                                          case eLineType.ARRAY.STR_DMS:
                                          case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                                eqp.MATERIALSTATUS = lines[0].File.Array_Material_Change ? "1" : "0";
                                                break;
                                    }
                              }

                              #region Sampling Side

                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling1 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling1.ITEMNAME = "VCD01";
                              sampling1.SIDESTATUS = node.File.VCD01.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling1);
                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling2 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling2.ITEMNAME = "VCD02";
                              sampling2.SIDESTATUS = node.File.VCD02.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling2);
                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling4 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling4.ITEMNAME = "CP01";
                              sampling4.SIDESTATUS = node.File.CP01.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling4);
                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling5 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling5.ITEMNAME = "CP02";
                              sampling5.SIDESTATUS = node.File.CP02.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling5);
                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling6 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling6.ITEMNAME = "HP01";
                              sampling6.SIDESTATUS = node.File.HP01.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling6);
                              AllEquipmentStatusReply.SAMPLINGSIDEc sampling7 = new AllEquipmentStatusReply.SAMPLINGSIDEc();
                              sampling7.ITEMNAME = "HP02";
                              sampling7.SIDESTATUS = node.File.HP02.ToString();
                              eqp.SAMPLINGSIDELIST.Add(sampling7);

                              #endregion

                              #region VCR List

                              if (node.Data.VCRCOUNT == 1)
                              {
                                    AllEquipmentStatusReply.VCRc vcr = new AllEquipmentStatusReply.VCRc();
                                    vcr.VCRNO = "01";
                                    vcr.VCRENABLEMODE = ((int)node.File.VcrMode[0]).ToString();
                                    eqp.VCRLIST.Add(vcr);
                              }
                              else if (node.Data.VCRCOUNT > 1)
                              {
                                    Line line = ObjectManager.LineManager.GetLine(node.Data.LINEID);
                                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                                    {
                                          string strVCR = node.File.UnitVCREnableStatus;
                                          for (int index = 0; index < strVCR.Length; index++)
                                          {
                                                if (index < node.Data.VCRCOUNT)
                                                {
                                                      AllEquipmentStatusReply.VCRc vcr = new AllEquipmentStatusReply.VCRc();
                                                      vcr.VCRNO = index.ToString().PadLeft(2, '0');
                                                      vcr.VCRENABLEMODE = strVCR[index].ToString() == "1" ? "1" : "0";
                                                      eqp.VCRLIST.Add(vcr);
                                                }
                                          }
                                    }
                                    else
                                    {
                                          for (int j = 0; j < node.File.VcrMode.Count(); j++)
                                          {
                                                AllEquipmentStatusReply.VCRc vcr = new AllEquipmentStatusReply.VCRc();
                                                vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                                                vcr.VCRENABLEMODE = node.File.VcrMode[j].ToString() == "1" ? "1" : "0";
                                                eqp.VCRLIST.Add(vcr);
                                          }
                                    }
                              }

                              #endregion

                              #region Interlock List

                              for (int j = 1; j <= node.Data.MPLCINTERLOCKCOUNT; j++)
                              {
                                    #region 從倉庫取MPLCInterlockCommand
                                    //string interlockNo = j.ToString("00");
                                    //Trx interlockTrx = Repository.Get(string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo)) as Trx;
                                    //AllEquipmentStatusReply.INTERLOCKc interlock = new AllEquipmentStatusReply.INTERLOCKc();
                                    //interlock.INTERLOCKNO = interlockNo;
                                    //if (interlockTrx == null) continue;
                                    //interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    //eqp.INTERLOCKLIST.Add(interlock);
                                    #endregion
                                    #region 從PLC讀MPLCInterlockCommand
                                    string interlockNo = j.ToString("00");
                                    string trx_name = string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo);
                                    Trx interlockTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trx_name, false }) as Trx;
                                    AllEquipmentStatusReply.INTERLOCKc interlock = new AllEquipmentStatusReply.INTERLOCKc();
                                    interlock.INTERLOCKNO = interlockNo;
                                    if (interlockTrx == null) continue;
                                    interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    eqp.INTERLOCKLIST.Add(interlock);
                                    #endregion
                              }

                              #endregion

                              reply.BODY.EQUIPMENTLIST.Add(eqp);
                              //}
                        }

                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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
            /// OPI MessageSet: CF Material Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CFMaterialStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CFMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as CFMaterialStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CFMaterialStatusReply") as XmlDocument;
                  CFMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as CFMaterialStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010450";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //先將Material依機台分類
                              Dictionary<string, List<CFMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<CFMaterialStatusReply.MATERIALc>>();
                              foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
                              {
                                    CFMaterialStatusReply.MATERIALc cfMaterial = new CFMaterialStatusReply.MATERIALc();
                                    cfMaterial.MATERIALID = material.MaterialID.Trim();
                                    cfMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
                                    cfMaterial.MATERIALVALUE = material.MaterialValue;
                                    cfMaterial.OPERATIONERID = material.OperatorID;
                                    cfMaterial.SLOTNO = material.MaterialSlotNo;
                                    cfMaterial.UNITNO = material.UnitNo;

                                    if (!dicMaterial.ContainsKey(material.NodeNo))
                                    {
                                          List<CFMaterialStatusReply.MATERIALc> materials = new List<CFMaterialStatusReply.MATERIALc>();
                                          materials.Add(cfMaterial);
                                          dicMaterial.Add(material.NodeNo, materials);
                                    }
                                    else
                                          dicMaterial[material.NodeNo].Add(cfMaterial);
                              }

                              //依機台區分Material
                              foreach (KeyValuePair<string, List<CFMaterialStatusReply.MATERIALc>> m in dicMaterial)
                              {
                                    CFMaterialStatusReply.EQUIPMENTc _eqp = new CFMaterialStatusReply.EQUIPMENTc();
                                    _eqp.EQUIPMENTNO = m.Key;
                                    _eqp.MATERIALLIST = m.Value;

                                    reply.BODY.EQUIPMENTLIST.Add(_eqp);
                              }
                        }

                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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
            /// OPI MessageSet: CELL Material Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CellMaterialStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CellMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as CellMaterialStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CellMaterialStatusReply") as XmlDocument;
                  CellMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as CellMaterialStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0011000";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Dictionary<string, List<CellMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<CellMaterialStatusReply.MATERIALc>>();
                              foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
                              {
                                    CellMaterialStatusReply.MATERIALc cellMaterial = new CellMaterialStatusReply.MATERIALc();
                                    cellMaterial.MATERIALID = material.MaterialID;
                                    cellMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
                                    cellMaterial.RECIPEID = material.MaterialRecipeID;
                                    cellMaterial.OPERATIONERID = material.OperatorID;
                                    cellMaterial.UNITNO = material.UnitNo;
                                    cellMaterial.UV_MASK_USE_COUNT = material.UVMaskUseCount;
                                    //pipMaterial.POSITION = material.MaterialPosition;
                                    //pipMaterial.WEIGHT = material.MaterialWeight;

                                    if (!dicMaterial.ContainsKey(material.NodeNo))
                                    {
                                          List<CellMaterialStatusReply.MATERIALc> materials = new List<CellMaterialStatusReply.MATERIALc>();
                                          materials.Add(cellMaterial);
                                          dicMaterial.Add(material.NodeNo, materials);
                                    }
                                    else
                                          dicMaterial[material.NodeNo].Add(cellMaterial);
                              }

                              foreach (KeyValuePair<string, List<CellMaterialStatusReply.MATERIALc>> m in dicMaterial)
                              {
                                    CellMaterialStatusReply.EQUIPMENTc _eqp = new CellMaterialStatusReply.EQUIPMENTc();
                                    _eqp.EQUIPMENTNO = m.Key;
                                    _eqp.MATERIALLIST = m.Value;

                                    reply.BODY.EQUIPMENTLIST.Add(_eqp);
                              }
                        }

                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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

            #region T2 Transcation.  T3 change to use CellMaterialStatusRequest
            ///// <summary>
            ///// OPI MessageSet: PIP Material Status Report
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_PIPMaterialStatusRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      PIPMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as PIPMaterialStatusRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("PIPMaterialStatusReply") as XmlDocument;
            //      PIPMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as PIPMaterialStatusReply;

            //      try
            //      {
            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;

            //            //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //            IList<Line> lines = ObjectManager.LineManager.GetLines();
            //            if (lines == null)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010460";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
            //                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //            else
            //            {
            //                  Dictionary<string, List<PIPMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<PIPMaterialStatusReply.MATERIALc>>();
            //                  foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
            //                  {
            //                        PIPMaterialStatusReply.MATERIALc pipMaterial = new PIPMaterialStatusReply.MATERIALc();
            //                        pipMaterial.MATERIALID = material.MaterialID;
            //                        pipMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
            //                        pipMaterial.RECIPEID = material.MaterialRecipeID;
            //                        pipMaterial.OPERATIONERID = material.OperatorID;
            //                        pipMaterial.UNITNO = material.UnitNo;
            //                        pipMaterial.UV_MASK_USE_COUNT = material.UVMaskUseCount;
            //                        //pipMaterial.POSITION = material.MaterialPosition;
            //                        //pipMaterial.WEIGHT = material.MaterialWeight;

            //                        if (!dicMaterial.ContainsKey(material.NodeNo))
            //                        {
            //                              List<PIPMaterialStatusReply.MATERIALc> materials = new List<PIPMaterialStatusReply.MATERIALc>();
            //                              materials.Add(pipMaterial);
            //                              dicMaterial.Add(material.NodeNo, materials);
            //                        }
            //                        else
            //                              dicMaterial[material.NodeNo].Add(pipMaterial);
            //                  }

            //                  foreach (KeyValuePair<string, List<PIPMaterialStatusReply.MATERIALc>> m in dicMaterial)
            //                  {
            //                        PIPMaterialStatusReply.EQUIPMENTc _eqp = new PIPMaterialStatusReply.EQUIPMENTc();
            //                        _eqp.EQUIPMENTNO = m.Key;
            //                        _eqp.MATERIALLIST = m.Value;

            //                        reply.BODY.EQUIPMENTLIST.Add(_eqp);
            //                  }
            //            }

            //            if (reply.BODY.EQUIPMENTLIST.Count > 0)
            //                  reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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

            ///// <summary>
            ///// OPI MessageSet: Seal Material Status Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_SealMaterialStatusRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      SealMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as SealMaterialStatusRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("SealMaterialStatusReply") as XmlDocument;
            //      SealMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as SealMaterialStatusReply;

            //      try
            //      {
            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;

            //            //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //            IList<Line> lines = ObjectManager.LineManager.GetLines();
            //            if (lines == null)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010470";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
            //                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //            else
            //            {
            //                  Dictionary<string, List<SealMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<SealMaterialStatusReply.MATERIALc>>();
            //                  foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
            //                  {
            //                        SealMaterialStatusReply.MATERIALc sealMaterial = new SealMaterialStatusReply.MATERIALc();
            //                        sealMaterial.UNITNO = material.UnitNo;
            //                        sealMaterial.MATERIALID = material.MaterialID;
            //                        sealMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
            //                        sealMaterial.RECIPEID = material.MaterialRecipeID;
            //                        sealMaterial.OPERATIONERID = material.OperatorID;
            //                        sealMaterial.MATERIALTYPE = material.MaterialType;
            //                        //1: Normal End, 2: Abnormal End
            //                        sealMaterial.COMPLETESTATUS = material.MaterialCompleteStatus == "NORMALEND" ? "1" : "2";
            //                        sealMaterial.POSITION = material.MaterialPosition;
            //                        sealMaterial.WEIGHT = material.MaterialWeight;

            //                        if (!dicMaterial.ContainsKey(material.NodeNo))
            //                        {
            //                              List<SealMaterialStatusReply.MATERIALc> materials = new List<SealMaterialStatusReply.MATERIALc>();
            //                              materials.Add(sealMaterial);
            //                              dicMaterial.Add(material.NodeNo, materials);
            //                        }
            //                        else
            //                              dicMaterial[material.NodeNo].Add(sealMaterial);
            //                  }

            //                  foreach (KeyValuePair<string, List<SealMaterialStatusReply.MATERIALc>> m in dicMaterial)
            //                  {
            //                        SealMaterialStatusReply.EQUIPMENTc _eqp = new SealMaterialStatusReply.EQUIPMENTc();
            //                        _eqp.EQUIPMENTNO = m.Key;
            //                        _eqp.MATERIALLIST = m.Value;

            //                        reply.BODY.EQUIPMENTLIST.Add(_eqp);
            //                  }
            //            }

            //            if (reply.BODY.EQUIPMENTLIST.Count > 0)
            //                  reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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

            ///// <summary>
            ///// OPI MessageSet: LC Material Status Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_LCMaterialStatusRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      LCMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as LCMaterialStatusRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("LCMaterialStatusReply") as XmlDocument;
            //      LCMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as LCMaterialStatusReply;

            //      try
            //      {
            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;

            //            //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //            IList<Line> lines = ObjectManager.LineManager.GetLines();
            //            if (lines == null)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010480";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
            //                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //            else
            //            {
            //                  Dictionary<string, List<LCMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<LCMaterialStatusReply.MATERIALc>>();
            //                  foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
            //                  {
            //                        LCMaterialStatusReply.MATERIALc lcMaterial = new LCMaterialStatusReply.MATERIALc();
            //                        lcMaterial.UNITNO = material.UnitNo;
            //                        lcMaterial.MATERIALID = material.MaterialID;
            //                        lcMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
            //                        lcMaterial.RECIPEID = material.MaterialRecipeID;
            //                        lcMaterial.OPERATIONERID = material.OperatorID;
            //                        lcMaterial.GROUPID = material.GroupId;
            //                        lcMaterial.POSITION = material.MaterialPosition;
            //                        lcMaterial.WEIGHT = material.MaterialWeight;
            //                        lcMaterial.CARTRIDGEID = material.MaterialCartridgeID;

            //                        if (!dicMaterial.ContainsKey(material.NodeNo))
            //                        {
            //                              List<LCMaterialStatusReply.MATERIALc> materials = new List<LCMaterialStatusReply.MATERIALc>();
            //                              materials.Add(lcMaterial);
            //                              dicMaterial.Add(material.NodeNo, materials);
            //                        }
            //                        else
            //                              dicMaterial[material.NodeNo].Add(lcMaterial);
            //                  }

            //                  foreach (KeyValuePair<string, List<LCMaterialStatusReply.MATERIALc>> m in dicMaterial)
            //                  {
            //                        LCMaterialStatusReply.EQUIPMENTc _eqp = new LCMaterialStatusReply.EQUIPMENTc();
            //                        _eqp.EQUIPMENTNO = m.Key;
            //                        _eqp.MATERIALLIST = m.Value;

            //                        reply.BODY.EQUIPMENTLIST.Add(_eqp);
            //                  }
            //            }

            //            if (reply.BODY.EQUIPMENTLIST.Count > 0)
            //                  reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
            //      }
            //}

            ///// <summary>
            ///// OPI MessageSet: UV Material Status Request
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_UVMaterialStatusRequest(XmlDocument xmlDoc)
            //{
            //      IServerAgent agent = GetServerAgent();
            //      UVMaterialStatusRequest command = Spec.XMLtoMessage(xmlDoc) as UVMaterialStatusRequest;
            //      XmlDocument xml_doc = agent.GetTransactionFormat("UVMaterialStatusReply") as XmlDocument;
            //      UVMaterialStatusReply reply = Spec.XMLtoMessage(xml_doc) as UVMaterialStatusReply;

            //      try
            //      {
            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

            //            reply.BODY.LINENAME = command.BODY.LINENAME;

            //            //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //            IList<Line> lines = ObjectManager.LineManager.GetLines();
            //            if (lines == null)
            //            {
            //                  reply.RETURN.RETURNCODE = "0010490";
            //                  reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

            //                  Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                      string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
            //                      command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
            //            }
            //            else
            //            {
            //                  Dictionary<string, List<UVMaterialStatusReply.MATERIALc>> dicMaterial = new Dictionary<string, List<UVMaterialStatusReply.MATERIALc>>();
            //                  foreach (MaterialEntity material in ObjectManager.MaterialManager.GetMaterials())
            //                  {
            //                        UVMaterialStatusReply.MATERIALc uvMaterial = new UVMaterialStatusReply.MATERIALc();
            //                        uvMaterial.UNITNO = material.UnitNo;
            //                        uvMaterial.UVMASKID = uvMaterial.UVMASKID;
            //                        uvMaterial.MATERIALSTATUS = ((int)material.MaterialStatus).ToString() == "" ? "5" : ((int)material.MaterialStatus).ToString();
            //                        uvMaterial.OPERATIONERID = material.OperatorID;

            //                        if (!dicMaterial.ContainsKey(material.NodeNo))
            //                        {
            //                              List<UVMaterialStatusReply.MATERIALc> materials = new List<UVMaterialStatusReply.MATERIALc>();
            //                              materials.Add(uvMaterial);
            //                              dicMaterial.Add(material.NodeNo, materials);
            //                        }
            //                        else
            //                              dicMaterial[material.NodeNo].Add(uvMaterial);
            //                  }

            //                  foreach (KeyValuePair<string, List<UVMaterialStatusReply.MATERIALc>> m in dicMaterial)
            //                  {
            //                        UVMaterialStatusReply.EQUIPMENTc _eqp = new UVMaterialStatusReply.EQUIPMENTc();
            //                        _eqp.EQUIPMENTNO = m.Key;
            //                        _eqp.MATERIALLIST = m.Value;

            //                        reply.BODY.EQUIPMENTLIST.Add(_eqp);
            //                  }
            //            }

            //            if (reply.BODY.EQUIPMENTLIST.Count > 0)
            //                  reply.BODY.EQUIPMENTLIST.RemoveAt(0);

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
            #endregion

            /// <summary>
            /// OPI MessageSet: PLC Trx Name Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PLCTrxNameRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  PLCTrxNameRequest command = Spec.XMLtoMessage(xmlDoc) as PLCTrxNameRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("PLCTrxNameReply") as XmlDocument;
                  PLCTrxNameReply reply = Spec.XMLtoMessage(xml_doc) as PLCTrxNameReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010630";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //取得PLCAgent PLCFormat.xml
                              IServerAgent plcAgent = GetServerAgent(eAgentName.PLCAgent);
                              XmlDocument trxXml = new XmlDocument();
                              trxXml.Load(plcAgent.FormatFileName);
                              XmlNodeList trxNodes = trxXml.SelectNodes("//plcdriver/transaction/receive/trx");
                              foreach (XmlNode node in trxNodes)
                              {
                                    PLCTrxNameReply.PLCTRXc trx = new PLCTrxNameReply.PLCTRXc();
                                    XmlElement element = (XmlElement)node;
                                    trx.PLCTRXNAME = element.GetAttribute("name");

                                    reply.BODY.PLCTRXLIST.Add(trx);
                              }
                              //增加 BCS Send的Trx  20150227 Tom
                              trxNodes = trxXml.SelectNodes("//plcdriver/transaction/send/trx");
                              foreach (XmlNode node in trxNodes)
                              {
                                    PLCTrxNameReply.PLCTRXc trx = new PLCTrxNameReply.PLCTRXc();
                                    XmlElement element = (XmlElement)node;
                                    trx.PLCTRXNAME = element.GetAttribute("name");

                                    reply.BODY.PLCTRXLIST.Add(trx);
                              }
                        }

                        if (reply.BODY.PLCTRXLIST.Count > 0)
                              reply.BODY.PLCTRXLIST.RemoveAt(0);

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
            /// OPI MessageSet: PLC Trx Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PLCTrxDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  PLCTrxDataRequest command = Spec.XMLtoMessage(xmlDoc) as PLCTrxDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("PLCTrxDataReply") as XmlDocument;
                  PLCTrxDataReply reply = Spec.XMLtoMessage(xml_doc) as PLCTrxDataReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PLCTRXNAME={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PLCTRXNAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.PLCTRXNAME = command.BODY.PLCTRXNAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010660";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //由PLCAgent取得Trx資訊
                              string trxName = command.BODY.PLCTRXNAME;
                              Trx trx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { trxName }) as Trx;
                              if (trx != null) trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, true }) as Trx;
                              if (trx == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010661";
                                    if (trxName.IndexOf("CassetteControlCommand") > 0)
                                    {
                                          reply.RETURN.RETURNMESSAGE = "Not Allowed to Query ZR Data from PLC";

                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Not Allowed to Query ZR Data from PLC.",
                                          command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                                    }
                                    else
                                    {
                                          reply.RETURN.RETURNMESSAGE = "PLCTRXNAME is wrong, can't get trx result";

                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          string.Format("[LINENAME={0}] [BCS <- OPI][{1}] PLCTRXNAME is wrong, can't get trx result.",
                                          command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                                    }
                              }
                              else
                              {
                                    foreach (EventGroup group in trx.EventGroups.AllValues)
                                    {
                                          PLCTrxDataReply.EVENTGROUPc _group = new PLCTrxDataReply.EVENTGROUPc();
                                          _group.NAME = group.Metadata.Name;
                                          _group.DIR = group.Metadata.Dir.ToString();

                                          foreach (Event evt in group.Events.AllValues)
                                          {
                                                PLCTrxDataReply.EVENTc _evt = new PLCTrxDataReply.EVENTc();
                                                _evt.NAME = evt.Metadata.Name;
                                                _evt.DEVCODE = evt.Metadata.DeviceCode;
                                                _evt.ADDR = evt.Metadata.Address;
                                                _evt.POINTS = evt.Metadata.Points.ToString();
                                                _evt.SKIPDECODE = evt.Metadata.SkipDecode.ToString();

                                                foreach (Item item in evt.Items.AllValues)
                                                {
                                                      PLCTrxDataReply.ITEMc _item = new PLCTrxDataReply.ITEMc();
                                                      _item.NAME = item.Name;
                                                      _item.VAL = item.Value;
                                                      _item.WOFFSET = item.Metadata.WordOffset.ToString();
                                                      _item.WPOINTS = item.Metadata.WordPoints.ToString();
                                                      _item.BOFFSET = item.Metadata.BitOffset.ToString();
                                                      _item.BPOINTS = item.Metadata.BitPoints.ToString();
                                                      _item.EXPERESSION = item.Metadata.Expression.ToString();

                                                      _evt.ITEMLIST.Add(_item);
                                                }
                                                _group.EVENTLIST.Add(_evt);
                                          }
                                          reply.BODY.EVENTGROUPLIST.Add(_group);
                                    }
                              }
                        }

                        if (reply.BODY.EVENTGROUPLIST.Count > 0)
                              reply.BODY.EVENTGROUPLIST.RemoveAt(0);

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
            /// OPI MessageSet: Ionizer Fan Mode Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_IonizerFanModeReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  IonizerFanModeReportRequest command = Spec.XMLtoMessage(xmlDoc) as IonizerFanModeReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("IonizerFanModeReportReply") as XmlDocument;
                  IonizerFanModeReportReply reply = Spec.XMLtoMessage(xml_doc) as IonizerFanModeReportReply;

                  try
                  {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
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

                              reply.RETURN.RETURNCODE = "0010700";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              //由Repository取出儲存的資料
                              Trx result = Repository.Get(string.Format("{0}_IonizerFanEnableModeBlock", command.BODY.EQUIPMENTNO)) as Trx;
                              if (result == null)
                              {
                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't get {0}_IonizerFanEnableMode value.",
                                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                                    reply.RETURN.RETURNCODE = "0010701";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't get {0}_IonizerFanEnableMode value", command.BODY.EQUIPMENTNO);
                              }
                              else
                              {
                                    //OPI Spec format區分為兩個項目上報
                                    string mode1 = result.EventGroups[0].Events[0].Items[0].Value.Substring(0, 16);
                                    string mode2 = result.EventGroups[0].Events[0].Items[0].Value.Substring(16, 16);
                                    reply.BODY.ENABLEMODE1 = mode1;
                                    reply.BODY.ENABLEMODE2 = mode2;
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
            /// OPI MessageSet: Equipment Data Link Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentDataLinkStatusRequst(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentDataLinkStatusRequst command = Spec.XMLtoMessage(xmlDoc) as EquipmentDataLinkStatusRequst;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentDataLinkStatusReply") as XmlDocument;
                  EquipmentDataLinkStatusReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentDataLinkStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010640";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else
                        {
                              //由Repository中取出儲存的資料
                              Trx batonTrx = Repository.Get("L1_batonpassStatus") as Trx;
                              if (batonTrx == null)
                              {
                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get L1_BatonPassStatus value",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                                    reply.RETURN.RETURNCODE = "0010641";
                                    reply.RETURN.RETURNMESSAGE = "Can't get L1_BatonPassStatus value";
                              }
                              else
                              {
                                    reply.BODY.BATONPASSSTATUS = batonTrx.EventGroups[0].Events[0].Items[0].Value;
                                    reply.BODY.BATONPASSINTERRUPTION = batonTrx.EventGroups[0].Events[0].Items[1].Value;
                                    reply.BODY.DATALINKSTOP = batonTrx.EventGroups[0].Events[0].Items[2].Value;
                                    reply.BODY.STATIONLOOPSTATUS = batonTrx.EventGroups[0].Events[0].Items[3].Value;
                                    reply.BODY.BATONPASSEACHSTATION = batonTrx.EventGroups[0].Events[0].Items[4].Value;
                                    reply.BODY.CYCLETRANSMISSIONSTATUS = batonTrx.EventGroups[0].Events[0].Items[5].Value;
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
            /// OPI MessageSet: Energy Visualization Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EnergyVisualizationReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EnergyVisualizationReportRequest command = Spec.XMLtoMessage(xmlDoc) as EnergyVisualizationReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EnergyVisualizationReportReply") as XmlDocument;
                  EnergyVisualizationReportReply reply = Spec.XMLtoMessage(xml_doc) as EnergyVisualizationReportReply;

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
                            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIOPMENTNO={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                            reply.RETURN.RETURNCODE = "0010890";
                            reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                            switch (eqp.Data.REPORTMODE)
                            {
                                case "PLC":
                                case "PLC_HSMS":
                                {
                                    #region PLC
                                    reply.BODY.DATALIST.Clear();
                                    IList<EnergyVisualizationData> dataFormats = ObjectManager.EnergyVisualizationManager.GetEnergyVisualizationProfile(command.BODY.EQUIPMENTNO);
                                    Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_EnergyVisualizationDataBlock", eqp.Data.NODENO), false }) as Trx;
                                    if (trx != null && dataFormats != null)
                                    {
                                        Dictionary<string, EnergyMeter> resultDic = new Dictionary<string, EnergyMeter>();
                                        #region [decode - Copy from EnergyVisualizationService.EnergyVisualizationReport_PLC()]
                                        {
                                            short[] rawData = trx.EventGroups[0].Events[0].RawData;
                                            string value = string.Empty;
                                            int startaddress10 = 0;
                                            foreach (EnergyVisualizationData pd in dataFormats)
                                            {
                                                ItemExpressionEnum ie;
                                                if (!Enum.TryParse(pd.Data.EXPRESSION.ToUpper(), out ie))
                                                {
                                                    continue;
                                                }
                                                #region decode by expression
                                                switch (ie)
                                                {
                                                    case ItemExpressionEnum.BIT:
                                                        value = ExpressionBIT.Decode(startaddress10, int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.ASCII:
                                                        value = ExpressionASCII.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        value = System.Text.RegularExpressions.Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                                                        break;
                                                    case ItemExpressionEnum.BIN:
                                                        value = ExpressionBIN.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.EXP:
                                                        value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.HEX:
                                                        value = ExpressionHEX.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.INT:
                                                        value = ExpressionINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.LONG:
                                                        value = ExpressionLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.SINT:
                                                        value = ExpressionSINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.SLONG:
                                                        value = ExpressionSLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.BCD:
                                                        value = ExpressionBCD.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                #endregion

                                                #region 算法[modify][by yang 20170329 转不出(value超过类型值范围)报default]
                                                string itemValue = string.Empty;
                                                double doubleresult;
                                                switch (pd.Data.OPERATOR) //目前operator只有'/',only modify '/'
                                                {
                                                    case ArithmeticOperator.PlusSign:
                                                        itemValue = (long.Parse(value) + long.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.MinusSign:
                                                        itemValue = (long.Parse(value) - long.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.TimesSign:
                                                        itemValue = (double.Parse(value) * double.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.DivisionSign:

                                                        if (double.TryParse(value, out doubleresult))
                                                            itemValue = (doubleresult / double.Parse(pd.Data.DOTRATIO)).ToString();
                                                        else
                                                        {
                                                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + pd.Data.PARAMETERNAME + "), ");
                                                            itemValue = "-999";  //default
                                                        }
                                                        break;
                                                    default:
                                                        itemValue = value;
                                                        break;
                                                }
                                                #endregion

                                                #region Item Value
                                                string paraGroup = (pd.Data.ITEM == "" ? pd.Data.PARAMETERNAME : pd.Data.ITEM);
                                                if (!resultDic.ContainsKey(paraGroup))
                                                {
                                                    EnergyMeter energyMeter = new EnergyMeter();
                                                    resultDic.Add(paraGroup, energyMeter);
                                                }

                                                switch (pd.Data.SITE)
                                                {
                                                    case "1":
                                                        {
                                                            if (itemValue == "1") resultDic[paraGroup].EnergyType = "Liquid";
                                                            if (itemValue == "2") resultDic[paraGroup].EnergyType = "Electricity";
                                                            if (itemValue == "3") resultDic[paraGroup].EnergyType = "Gas";
                                                            if (itemValue == "4") resultDic[paraGroup].EnergyType = "N2";
                                                            if (itemValue == "5") resultDic[paraGroup].EnergyType = "Stripper";
                                                        }
                                                        break;

                                                    case "2":
                                                        {
                                                            resultDic[paraGroup].UnitID = itemValue;
                                                        }
                                                        break;

                                                    case "3":
                                                        {
                                                            resultDic[paraGroup].MeterNo = itemValue;
                                                        }
                                                        break;

                                                    case "4":
                                                        {
                                                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        }
                                                        break;

                                                    case "5":
                                                        {
                                                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                        }
                                                        break;

                                                    case "6":
                                                        {
                                                            if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                            if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        }
                                                        break;

                                                    case "7":
                                                        {
                                                            resultDic[paraGroup].Refresh = itemValue;
                                                        }
                                                        break;
                                                }
                                                #endregion
                                            }
                                        }
                                        #endregion
                                        foreach (string key in resultDic.Keys)
                                        {


                                            EnergyMeter energy_meter = resultDic[key];
                                            EnergyVisualizationReportReply.DATAc d1 = new EnergyVisualizationReportReply.DATAc();
                                            d1.NAME = string.Format("{0}-DataType", key);
                                            d1.VALUE = energy_meter.EnergyType;
                                            reply.BODY.DATALIST.Add(d1);
                                            d1 = new EnergyVisualizationReportReply.DATAc();
                                            d1.NAME = string.Format("{0}-UnitID", key);
                                            d1.VALUE = energy_meter.UnitID;
                                            reply.BODY.DATALIST.Add(d1);
                                            d1 = new EnergyVisualizationReportReply.DATAc();
                                            d1.NAME = string.Format("{0}-MeterNo", key);
                                            d1.VALUE = energy_meter.MeterNo;
                                            reply.BODY.DATALIST.Add(d1);
                                            d1 = new EnergyVisualizationReportReply.DATAc();
                                            d1.NAME = string.Format("{0}-Refresh", key);
                                            d1.VALUE = energy_meter.Refresh;
                                            reply.BODY.DATALIST.Add(d1);

                                            foreach (string item in energy_meter.EnergyParam.Keys)
                                            {
                                                EnergyVisualizationReportReply.DATAc d = new EnergyVisualizationReportReply.DATAc();
                                                d.NAME = string.Format("{0}-{1}", key, item);
                                                d.VALUE = energy_meter.EnergyParam[item];
                                                reply.BODY.DATALIST.Add(d);
                                            }
                                        }
                                        if (reply.BODY.DATALIST.Count == 0)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty EnergyVisualizationReportReply to OPI, because there is no item after decode.",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                    }
                                    else
                                    {
                                        if (dataFormats == null)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty EnergyVisualizationReportReply to OPI, because GetEnergyVisualizationProfile() return null",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                        if (trx == null)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty EnergyVisualizationReportReply to OPI, because PLCAgent.SyncReadTrx() return null",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                    }
                                    #endregion
                                }
                                break;
                                case "HSMS_CSOT":
                                case "HSMS_PLC":
                                {
                                    #region SECS CSOT EQ

                                    //使用SECS S1F5(CEID:6)取得結果， SECS機台沒有區分Unit
                                    //请求前先清除仓库中对应的值
                                    //add by box.zhai
                                    //Repository.Remove(string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID));

                                    Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "10", string.Empty, command.HEADER.TRANSACTIONID });
                                    Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "06", string.Empty, command.HEADER.TRANSACTIONID }); //add by yang 2017/5/9 for AKT Energy
                                    Thread.Sleep(300);
                                    List<Tuple<string, List<Tuple<string, string, string>>>> SpecialData = Repository.Get(string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;

                                    if (SpecialData != null && SpecialData.Count > 0)
                                    {
                                        foreach (Tuple<string, List<Tuple<string, string, string>>> _apc in SpecialData)
                                        {
                                            foreach (Tuple<string, string, string> _apc1 in _apc.Item2)
                                            {
                                                EnergyVisualizationReportReply.DATAc Energy = new EnergyVisualizationReportReply.DATAc();
                                                Energy.NAME = _apc1.Item1;
                                                Energy.VALUE = _apc1.Item3;
                                                reply.BODY.DATALIST.Add(Energy);
                                            }
                                        }
                                    }
                                    reply.BODY.DATALIST.RemoveAt(0);
                                    #endregion
                                }
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
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Product Type Info Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ProductTypeInfoRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ProductTypeInfoRequest command = Spec.XMLtoMessage(xmlDoc) as ProductTypeInfoRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ProductTypeInfoRequestReply") as XmlDocument;
                  ProductTypeInfoRequestReply reply = Spec.XMLtoMessage(xml_doc) as ProductTypeInfoRequestReply;

                  Unit unit = null;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010950";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else
                        {
                              reply.BODY.EQUIPMENTLIST.Clear();

                              foreach (Equipment node in ObjectManager.EquipmentManager.GetEQPs())
                              {
                                    ProductTypeInfoRequestReply.EQUIPMENTc eqp = new ProductTypeInfoRequestReply.EQUIPMENTc();
                                    eqp.EQUIPMENTNO = node.Data.NODENO;
                                    eqp.PRODUCTTYPE = node.File.ProductType.ToString();

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "1");
                                    if (unit != null) eqp.UNIT01_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT01_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "2");
                                    if (unit != null) eqp.UNIT02_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT02_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "3");
                                    if (unit != null) eqp.UNIT03_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT03_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "4");
                                    if (unit != null) eqp.UNIT04_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT04_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "5");
                                    if (unit != null) eqp.UNIT05_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT05_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "6");
                                    if (unit != null) eqp.UNIT06_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT06_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "7");
                                    if (unit != null) eqp.UNIT07_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT07_PRODUCTTYPE = "0";

                                    unit = ObjectManager.UnitManager.GetUnit(node.Data.NODENO, "8");
                                    if (unit != null) eqp.UNIT08_PRODUCTTYPE = unit.File.ProductType.ToString();
                                    else eqp.UNIT08_PRODUCTTYPE = "0";

                                    reply.BODY.EQUIPMENTLIST.Add(eqp);
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
            /// OPI MessageSet: Defect Code Report Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_DefectCodeReportRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  DefectCodeReportRequest command = Spec.XMLtoMessage(xmlDoc) as DefectCodeReportRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("DefectCodeReportReply") as XmlDocument;
                  DefectCodeReportReply reply = Spec.XMLtoMessage(xml_doc) as DefectCodeReportReply;

                  try
                  {
                        LogDebug(MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010980";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity!", command.BODY.EQUIPMENTNO);

                              LogWarn(MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0} in EquipmentEntity.)",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              reply.BODY.DEFECTLIST.Clear();

                              foreach (Job job in ObjectManager.JobManager.GetJobs())
                              {
                                    if (!(job.DefectCodes.Count.Equals(0)))
                                    {
                                          for (int i = 0; i < job.DefectCodes.Count; i++)
                                          {
                                                if (job.DefectCodes[i].EqpNo == command.BODY.EQUIPMENTNO)
                                                {
                                                      DefectCodeReportReply.DEFECTc def = new DefectCodeReportReply.DEFECTc();
                                                      def.CASSETTE_SEQNO = job.DefectCodes[i].CSTSeqNo;
                                                      def.JOB_SEQNO = job.DefectCodes[i].JobSeqNo;
                                                      def.CHIP_POSITION = job.DefectCodes[i].ChipPostion;
                                                      def.UNITNO = job.DefectCodes[i].UnitNo;
                                                      def.DEFECT_CODE = job.DefectCodes[i].DefectCodes;
                                                      //def.DEFECT_CODE01 = job.DefectCodes[i].DefectCodes.Substring(0, 4);
                                                      //def.DEFECT_CODE02 = job.DefectCodes[i].DefectCodes.Substring(5, 4);
                                                      //def.DEFECT_CODE03 = job.DefectCodes[i].DefectCodes.Substring(10, 4);
                                                      //def.DEFECT_CODE04 = job.DefectCodes[i].DefectCodes.Substring(15, 4);
                                                      //def.DEFECT_CODE05 = job.DefectCodes[i].DefectCodes.Substring(20, 4);
                                                      reply.BODY.DEFECTLIST.Add(def);
                                                }
                                          }
                                    }
                                    else
                                          continue;

                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        LogDebug(MethodInfo.GetCurrentMethod().Name + "()",
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

            /// <summary>
            /// OPI MessageSet: APC Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_APCDataReportRequest(XmlDocument xmlDoc)
            {
                IServerAgent agent = GetServerAgent();
                APCDataReportRequest command = Spec.XMLtoMessage(xmlDoc) as APCDataReportRequest;
                XmlDocument xml_doc = agent.GetTransactionFormat("APCDataReportReply") as XmlDocument;
                APCDataReportReply reply = Spec.XMLtoMessage(xml_doc) as APCDataReportReply;

                try
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                    reply.BODY.LINENAME = command.BODY.LINENAME;
                    reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                    string requestType=command.BODY.SECSREQUESTBYIDORNAME;//仅仅For SECS_CSOT使用

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                    if (eqp == null)
                    {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIOPMENTNO={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0011030";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                    }
                    else
                    {
                        switch (eqp.Data.REPORTMODE)
                        {
                            case "PLC":
                            case "PLC_HSMS":
                                {
                                    #region PLC Type EQ
                                    reply.BODY.DATALIST.Clear();
                                    IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(command.BODY.EQUIPMENTNO);
                                    Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_APCDataBlock", eqp.Data.NODENO), false }) as Trx;
                                    if (trx != null && dataFormats != null)
                                    {
                                        Dictionary<string, string> resultDic = new Dictionary<string, string>();
                                        List<string> paraList = new List<string>();
                                        #region [decode - Copy from APCService.APCDataReport_PLC()]
                                        {
                                            short[] rawData = trx.EventGroups[0].Events[0].RawData;
                                            string value = string.Empty;
                                            int startaddress10 = 0;
                                            foreach (APCDataReport pd in dataFormats)
                                            {
                                                ItemExpressionEnum ie;
                                                if (!Enum.TryParse(pd.Data.EXPRESSION.ToUpper(), out ie))
                                                {
                                                    continue;
                                                }
                                                #region decode by expression
                                                switch (ie)
                                                {
                                                    case ItemExpressionEnum.BIT:
                                                        value = ExpressionBIT.Decode(startaddress10, int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.ASCII:
                                                        value = ExpressionASCII.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        value = System.Text.RegularExpressions.Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                                                        break;
                                                    case ItemExpressionEnum.BIN:
                                                        value = ExpressionBIN.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.EXP:
                                                        value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.HEX:
                                                        value = ExpressionHEX.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                                        break;
                                                    case ItemExpressionEnum.INT:
                                                        value = ExpressionINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.LONG:
                                                        value = ExpressionLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.SINT:
                                                        value = ExpressionSINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.SLONG:
                                                        value = ExpressionSLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    case ItemExpressionEnum.BCD:
                                                        value = ExpressionBCD.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                #endregion

                                                #region 算法[modify][by yang 20170329 转不出(value超过类型值范围)报default]
                                                string itemValue = string.Empty;
                                                double doubleresult;
                                                switch (pd.Data.OPERATOR) //目前operator只有'/',only modify '/'
                                                {
                                                    case ArithmeticOperator.PlusSign:
                                                        itemValue = (long.Parse(value) + long.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.MinusSign:
                                                        itemValue = (long.Parse(value) - long.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.TimesSign:
                                                        itemValue = (double.Parse(value) * double.Parse(pd.Data.DOTRATIO)).ToString();
                                                        break;
                                                    case ArithmeticOperator.DivisionSign:

                                                        if (double.TryParse(value, out doubleresult))
                                                            itemValue = (doubleresult / double.Parse(pd.Data.DOTRATIO)).ToString();
                                                        else
                                                        {
                                                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + pd.Data.PARAMETERNAME + "), ");
                                                            itemValue = "-999";  //default
                                                        }
                                                        break;
                                                    default:
                                                        itemValue = value;
                                                        break;
                                                }
                                                #endregion

                                                #region Item Value

                                                if (!resultDic.ContainsKey(pd.Data.PARAMETERNAME))
                                                {
                                                    resultDic.Add(pd.Data.PARAMETERNAME, itemValue);
                                                }

                                                #endregion
                                            }
                                        }
                                        #endregion
                                        foreach (string key in resultDic.Keys)
                                        {
                                            APCDataReportReply.DATAc d = new APCDataReportReply.DATAc();
                                            d.NAME = string.Format("{0}", key);
                                            d.VALUE = resultDic[key];
                                            reply.BODY.DATALIST.Add(d);
                                        }
                                        if (reply.BODY.DATALIST.Count == 0)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty APCDataReport to OPI, because there is no item after decode.",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                    }
                                    else
                                    {
                                        if (dataFormats == null)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty APCDataReport to OPI, because GetAPCDataProfile() return null",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                        if (trx == null)
                                        {
                                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply empty APCDataReport to OPI, because PLCAgent.SyncReadTrx() return null",
                                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                                        }
                                    }
                                    #endregion
                                }
                                break;
                            case "HSMS_CSOT":
                            case "HSMS_PLC":
                                {
                                    #region SECS CSOT EQ  

                                    //根据SECS_CSOT 的 RequestType 来决定用 SECS S1F5(CEID:2/4) 或SECS S1F5(CEID:7/8)取得結果， SECS機台沒有區分Unit
                                    //请求前先清除仓库中对应的值
                                    //add by box.zhai
                                    //Repository.Remove(string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID));
                                    //Repository.Remove(string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID));

                                    if (requestType=="Name")
                                    {
                                        Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "02", string.Empty, command.HEADER.TRANSACTIONID });

                                        Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "04", string.Empty, command.HEADER.TRANSACTIONID });
                                    }
                                    else if (requestType == "ID")
                                    {
                                        Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "07", string.Empty, command.HEADER.TRANSACTIONID });

                                        Invoke("CSOTSECSService", "TS1F5_H_FormattedStatusRequest", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "08", string.Empty, command.HEADER.TRANSACTIONID });
                                    }
                                    
                                    Thread.Sleep(1000);
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcNormalData = Repository.Get(string.Format("{0}_{1}_SecsAPCNormalDataByReq",eqp.Data.LINEID, eqp.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcImportantData = Repository.Get(string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcData = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                                    if (apcNormalData != null)
                                    {
                                        apcData.AddRange(apcNormalData);
                                    }
                                    if (apcImportantData != null)
                                    {
                                        apcData.AddRange(apcImportantData);
                                    }
                                    if (apcData != null && apcData.Count > 0)
                                    {
                                        IList<APCDataReport> aPCDataReportProfile = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO);//20161122 sy add 將ID 轉NAME 再回OPI
                                        if (aPCDataReportProfile == null) break;//20161122 sy add 將ID 轉NAME 再回OPI

                                        foreach (Tuple<string, List<Tuple<string, string, string>>> _apc in apcData)
                                        {
                                            foreach (Tuple<string, string, string> _apc1 in _apc.Item2)
                                            {
                                                APCDataReportReply.DATAc APC = new APCDataReportReply.DATAc();
                                                int idOrName;
                                                if (int.TryParse(_apc1.Item1, out idOrName))//20161122 sy add Item1為INT
                                                {
                                                    foreach (APCDataReport aPCDataReport in aPCDataReportProfile)
                                                    {
                                                        if (aPCDataReport.Data.SVID.ToString() == _apc1.Item1.ToString())
                                                        {
                                                            APC.NAME = aPCDataReport.Data.PARAMETERNAME.ToString();
                                                            break;
                                                        }
                                                    }
                                                    if (string.IsNullOrEmpty(APC.NAME)) APC.NAME = _apc1.Item1;//表中沒有對應SVID 就已SVID當PARAMETERNAME
                                                }
                                                else
                                                    APC.NAME = _apc1.Item1;
                                                APC.VALUE= _apc1.Item3;
                                                reply.BODY.DATALIST.Add(APC);
                                            }
                                        }
                                    }
                                    reply.BODY.DATALIST.RemoveAt(0);
                                    #endregion
                                }
                                break;
                           
                            case "HSMS_NIKON":
                                {
                                    #region SECS NIKON EQ
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcNormalData = Repository.Get(string.Format("{0}_{1}_SecsAPCNormalDataByReq",eqp.Data.LINEID, eqp.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcImportantData = Repository.Get(string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID)) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcData = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                                    if (apcNormalData != null)
                                    {
                                        apcData.AddRange(apcNormalData);
                                    }
                                    if (apcImportantData != null)
                                    {
                                        apcData.AddRange(apcImportantData);
                                    }
                                    if (apcData != null && apcData.Count > 0)
                                    {
                                        IList<APCDataReport> aPCDataReportProfile = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO);//20161122 sy add 將ID 轉NAME 再回OPI
                                        if (aPCDataReportProfile == null) break;//20161122 sy add 將ID 轉NAME 再回OPI
                                        foreach (Tuple<string, List<Tuple<string, string, string>>> _apc in apcData)
                                        {
                                            foreach (Tuple<string, string, string> _apc1 in _apc.Item2)
                                            {
                                                APCDataReportReply.DATAc APC = new APCDataReportReply.DATAc();
                                                int idOrName;
                                                if (int.TryParse(_apc1.Item1, out idOrName))//20161122 sy add Item1為INT
                                                {
                                                    foreach (APCDataReport aPCDataReport in aPCDataReportProfile)
                                                    {
                                                        if (aPCDataReport.Data.SVID.ToString() == _apc1.Item1.ToString())
                                                        {
                                                            APC.NAME = aPCDataReport.Data.PARAMETERNAME.ToString();
                                                            break;
                                                        }
                                                    }
                                                    if (string.IsNullOrEmpty(APC.NAME)) APC.NAME = _apc1.Item1;//表中沒有對應SVID 就已SVID當PARAMETERNAME
                                                }
                                                else
                                                    APC.NAME = _apc1.Item1;
                                                APC.VALUE= _apc1.Item3;
                                                reply.BODY.DATALIST.Add(APC);
                                            }
                                        }
                                    }
                                    reply.BODY.DATALIST.RemoveAt(0);
                                    #endregion
                                }
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
                    //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                }
            }

            #region Command

            #endregion
      }
}
