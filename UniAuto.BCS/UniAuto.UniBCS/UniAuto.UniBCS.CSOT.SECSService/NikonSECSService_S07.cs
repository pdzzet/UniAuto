using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class NikonSECSService
      {
            #region Recipte form equipment-S7
            public void S7F0_E_AbortTransaction(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F0_E");
                  #region Handle Logic
                  //TODO:Check control mode and abort setting and reply SnF0 or not.
                  //TODO:Logic handle.
                  #endregion
            }
            public void S7F20_E_CurrentEPPDData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F20_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "CheckRecipe":
                                    ConcurrentDictionary<string, RecipeInCheck> checkings = _common.GetCheckingRecipeIDs(eqpno);
                                    if (checkings == null || checkings.Count == 0)
                                    {
                                          _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Check Recipe Info!");
                                          return;
                                    }
                                    RecipeInCheck checking = new RecipeInCheck();
                                    foreach (string ppid in checkings.Keys)
                                    {
                                          checkings.TryGetValue(ppid, out checking);
                                          if (checking.Info.TrxId == tid)
                                                break;
                                    }
                                    if (checking.Info == null)
                                    {
                                          _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Check Recipe Info!");
                                          return;
                                    }
                                    RecipeCheckInfo rcInfo = checking.Info;
                                    rcInfo.Result = eRecipeCheckResult.NG;
                                    if (recvTrx["secs"]["message"]["body"]["array1"].FirstChild.ChildNodes.Count > 0)
                                    {
                                          //20150918 t3:Line Recipe管理的ppid,Array 4碼,CF 2碼,Nikon會固定報4碼往前填0,檢查時要填滿
                                          XmlNode xPPID = recvTrx["secs"]["message"]["body"]["array1"].FirstChild.FirstChild;
                                          string ppid = string.Empty;
                                          while (xPPID != null)
                                          {
                                                ppid = xPPID.InnerText.Trim();   //Nikon報的,4碼
                                                if (!string.IsNullOrEmpty(ppid))
                                                {
                                                      if (ppid == rcInfo.RecipeID.PadLeft(4,'0')) //Line Recipe管理的PPID都往前補足4碼
                                                      {
                                                            rcInfo.Result = eRecipeCheckResult.OK;
                                                            break;
                                                      }
                                                }
                                                xPPID = xPPID.NextSibling;
                                          }
                                    }
                                    Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { rcInfo, rcInfo.TrxId });
                                    _common.RemoveCheckingRecipeID(eqpno, rcInfo.RecipeID);
                                    break;
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F20", string.Format("Number of process programs in the directory is ({0})", recvTrx["secs"]["message"]["body"]["array1"].FirstChild.Attributes["len"].InnerText.Trim()) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S7F24_E_FormattedProcessProgramAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F24_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string ack = string.Empty;
                        lock (eqp.File)
                        {
                            eqp.File.IsReveive = true;
                            eqp.File.R2REQParameterDownloadRetrunCode = recvTrx["secs"]["message"]["body"]["ACKC7"].InnerText.Trim();
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        switch (recvTrx["secs"]["message"]["body"]["ACKC7"].InnerText.Trim())
                        {
                              case "0":
                                    ack = "0:Accepted";
                                    break;
                              case "1":
                                    ack = "1:Permission not granted";
                                    break;
                              case "2":
                                    ack = "2:Length error";
                                    break;
                              case "3":
                                    ack = "3:Matrix overflow";
                                    break;
                              case "4":
                                    ack = "4:PPID not found";
                                    break;
                              case "5":
                                    ack = "5:Mode unsupported";
                                    break;
                              default:
                                    ack = recvTrx["secs"]["message"]["body"]["ACKC7"].InnerText.Trim() + ":Other error";
                                    break;
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Equipment reply formatted process program send result, ACKC7({0}).", ack));
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F24", string.Format("Equipment reply formatted process program send result, ACKC7({0}).", ack) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S7F26_E_FormattedProcessProgramData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F26_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPID"].InnerText.Trim();
                        string changeTime = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPCHANGETIME"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "CheckRecipe":
                                    ConcurrentDictionary<string, RecipeInCheck> checkings = _common.GetCheckingRecipeParameters(eqpno);
                                    if (checkings == null || checkings.Count == 0)
                                    {
                                          _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Check Recipe Parameter Info!");
                                          return;
                                    }
                                    RecipeInCheck checking = new RecipeInCheck();
                                    foreach (RecipeInCheck ric in checkings.Values)
                                    {
                                          if (ppid.Equals(ric.Info.RecipeID.PadLeft(4, '0')))
                                          {
                                                checking = ric;
                                                break;
                                          }
                                    }
                                    if (checking.Info == null)
                                    {
                                          _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Check Recipe Parameter Info!");
                                          return;
                                    }
                                    RecipeCheckInfo rcInfo = checking.Info;
                                    rcInfo.Result = eRecipeCheckResult.NG;
                                    if (recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"].ChildNodes.Count > 0)
                                    {
                                          XmlNode xParameter = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"].FirstChild;
                                          IList<RecipeParameter> rpList = ObjectManager.RecipeManager.GetRecipeParameter(eqpno);
                                          RecipeParameter rp = null;
                                          string paramCode = string.Empty;
                                          string paramName = string.Empty;
                                          string paramValue = string.Empty;
                                          string paramNameKey = string.Empty;
                                          while (xParameter != null)
                                          {
                                                paramCode = xParameter["PARAMNAME"].InnerText.Trim();
                                                paramValue = xParameter["PARAMVALUE"].InnerText.Trim();
                                                if (rpList == null)
                                                      paramName = paramCode.TrimStart('0');
                                                else
                                                {
                                                      rp = rpList.FirstOrDefault(p => p.Data.SVID == paramCode.TrimStart('0'));
                                                      if (rp == null)
                                                            paramName = paramCode.TrimStart('0');
                                                      else
                                                            paramName = rp.Data.PARAMETERNAME;
                                                }
                                                //t3 以原始來要求的為ID上報,因為機台報的,可能有補0,如"0011",實際是要求"11"
                                                //paramNameKey = eqp.Data.NODEID + "@" + ppid + "^" + paramName;
                                                paramNameKey = eqp.Data.NODEID + "@" + rcInfo.RecipeID + "^" + paramName;
                                                if (rcInfo.Parameters.ContainsKey(paramNameKey))
                                                {
                                                      _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                  string.Format("Recipe parameter name duplicate! Name({0})", paramName));
                                                      rcInfo.Parameters.Remove(paramNameKey);
                                                }
                                                //20150413 cy: 在MESService上報時會檢查，格式為EQID@UNITID^PARANAME
                                                //rcInfo.Parameters.Add(paramName, paramValue);
                                                //20150624 cy: 統一上報格式為NodeID_ppid_parameterName
                                                rcInfo.Parameters.Add(paramNameKey, paramValue);
                                                xParameter = xParameter.NextSibling;
                                          }
                                          rcInfo.Result = eRecipeCheckResult.OK;
                                          rpList = null;
                                          rp = null;
                                    }
                                    Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { rcInfo, rcInfo.TrxId });
                                    _common.RemoveCheckingRecipeParameter(eqpno, ppid);
                                    break;
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F26", string.Format("Recipe parameter count({0})", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"].Attributes["len"].InnerText.Trim()) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S7F27_E_ProcessProgramVerificationSend(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F27_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        TS7F28_H_ProcessProgramVerificationAcknowledge(eqpno, agent, tid, sysbytes);

                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();
                        string log = "ACKC7A:SEQNUM:ERRW7";
                        XmlNode xArray2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        if (xArray2.ChildNodes.Count > 0)
                        {
                              XmlNode xArray3 = xArray2.FirstChild;
                              while (xArray3 != null)
                              {
                                    string ack = string.Empty;
                                    string seq = string.Empty;
                                    string err = string.Empty;
                                    switch (xArray3["ACKC7A"].InnerText.Trim())
                                    {
                                          case "0":
                                                ack = "0-Accepted";
                                                break;
                                          case "1":
                                                ack = "1-MDLN is inconsistent";
                                                break;
                                          case "2":
                                                ack = "2-SOFTREV is inconsistent";
                                                break;
                                          case "3":
                                                ack = "3-Invalid CCODE";
                                                break;
                                          case "4":
                                                ack = "4-Invalid PPARM value";
                                                break;
                                          default:
                                                ack = xArray3["ACKC7A"].InnerText.Trim() + "-Other error";
                                                break;
                                    }
                                    seq = xArray3["SEQNUM"].InnerText.Trim();
                                    err = xArray3["ERRW7"].InnerText.Trim();
                                    log += string.Format("({0}:{1}:{2})", ack, seq, err);
                                    xArray3 = xArray3.NextSibling;
                              }
                              _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Equipment send formatted process program verification result, {0}.", log));
                        }
                        else
                              _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Equipment send formatted process program verification result, {0}.", "(No error reported)"));

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            #endregion

            #region Send by host-S7
            public void TS7F19_H_CurrentEPPDRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F19_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F19_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F19_H";
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
            public void TS7F19_H_CurrentEPPDRequest(string eqpno, string eqpid, string tag)
            {
                  TS7F19_H_CurrentEPPDRequest(eqpno, eqpid, tag, base.CreateTrxID());
            }
            public void TS7F19_H_CurrentEPPDRequest(RecipeCheckInfo check, string trxid)
            {
                  try
                  {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(check.EQPNo);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment Number({0}) in EquipmentEntity!", check.EQPNo));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              "Can not send current ppid list requst when CIM mode is OFF!");
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        if (string.IsNullOrEmpty(trxid))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              "Can not send requst if transaction ID is empty!");
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        //20150918 t3:限定長度是4碼,來源若超過4碼,表示有問題,直接回NG (modify by CY)
                        if (check.RecipeID.Trim().Length > 4)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              string.Format("RecipeID[{0}] length is more then 4.", check.RecipeID.Trim()));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        TS7F19_H_CurrentEPPDRequest(eqp.Data.NODENO, eqp.Data.NODEID, "CheckRecipe", trxid);

                        //保存RecipeCheckInfo,待S7F20做檢查並返還結果
                        _common.AddCheckingRecipeID(check);
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
            /// <summary>
            /// 
            /// </summary>
            /// <param name="eqpno"></param>
            /// <param name="eqpid"></param>
            /// <param name="ppid"></param>
            /// <param name="parame">{CCODE},{PPARM,TYPE}</param>
            /// <param name="tag"></param>
            public void TS7F23_H_FormattedProcessProgramSend(string eqpno, string eqpid, string ppid,
                List<Tuple<ushort, List<Tuple<string, string>>>> parame, string tag, string trxid)
            {
                  try
                  {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment No({0}) in EquipmentEntity!", eqpno));
                              return;
                        }
                        lock (eqp.File)
                        {
                            eqp.File.R2REQParameterDownloadDT = DateTime.Now;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F23_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F23_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F23_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        sendTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText = ppid;
                        sendTrx["secs"]["message"]["body"]["array1"]["MDLN"].InnerText = eqp.MDLN;
                        sendTrx["secs"]["message"]["body"]["array1"]["SOFTREV"].InnerText = eqp.SOFTREV;
                        XmlNode xArray2 = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop2 = 0;
                        if (parame != null)
                        {
                              loop2 = parame.Count;
                        }
                        xArray2.Attributes["len"].InnerText = loop2.ToString();
                        if (loop2 == 0)
                        {
                              xArray2.RemoveChild(xArray2["array3"]);
                        }
                        for (int i = 0; i < loop2; i++)
                        {
                              _common.CloneChildNode(xArray2, i);
                        }
                        for (int i = 0; i < loop2; i++)
                        {
                              xArray2.ChildNodes[i]["CCODE"].InnerText = parame[i].Item1.ToString();
                              XmlNode xArray4 = xArray2.ChildNodes[i]["array4"];
                              int loop4 = 0;
                              List<Tuple<string, string>> tuples = parame[i].Item2;
                              if (tuples != null)
                              {
                                    loop4 = tuples.Count;
                              }
                              xArray4.Attributes["len"].InnerText = loop4.ToString();
                              if (loop4 == 0)
                              {
                                    xArray4.RemoveChild(xArray4["PPARM"]);
                              }
                              for (int j = 0; j < loop4; j++)
                              {
                                    _common.CloneChildNode(xArray4, j);
                              }
                              for (int j = 0; j < loop4; j++)
                              {
                                    xArray4.ChildNodes[j].InnerText = tuples[j].Item1;
                                    xArray4.ChildNodes[j].Attributes["type"].InnerText = tuples[j].Item2;
                              }
                        }
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
            public void TS7F25_H_FormattedProcessProgramRequest(string eqpno, string eqpid, string ppid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F25_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F25_H)");
                              return;
                        }
                        //20151012 t3:限定長度是4碼,來源若超過4碼,表示有問題,直接回NG (modify by CY)
                        if (ppid.Trim().Length > 4)
                        {
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                       new object[4] { trxid, string.Empty, "Send S7F26", string.Format("Recipe parameter request PPID({0}) error.", ppid) });
                              }
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                     string.Format("Recipe parameter request PPID({0}) error.", ppid));
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["PPID"].InnerText = ppid.Trim().PadLeft(4, '0'); //t3:往左補足4碼
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F25_H";
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
            public void TS7F25_H_FormattedProcessProgramRequest(string eqpno, string eqpid, string ppid, string tag)
            {
                  TS7F25_H_FormattedProcessProgramRequest(eqpno, eqpid, ppid, tag, base.CreateTrxID());
            }
            public void TS7F25_H_FormattedProcessProgramRequest(RecipeCheckInfo check, string trxid)
            {
                  try
                  {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(check.EQPNo);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment No({0}) in EquipmentEntity!", check.EQPNo));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              "Can not send recipe parameter requst when CIM mode is OFF!");
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }
                        //20151012 t3:限定長度是4碼,來源若超過4碼,表示有問題,直接回NG (modify by CY)
                        if (check.RecipeID.Length > 4) 
                        {
                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                      string.Format("Recipe ID({0}) format is not correct. (Mix Length:4)", check.RecipeID));
                                    check.Result = eRecipeCheckResult.NG;
                                    Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                                    return;
                        }
                        
                        if (string.IsNullOrEmpty(trxid))
                              trxid = base.CreateTrxID();
                        TS7F25_H_FormattedProcessProgramRequest(eqp.Data.NODENO, eqp.Data.NODEID, check.RecipeID, "CheckRecipe", trxid);
                        //保存RecipeCheckInfo,待S7F26做檢查並返還結果
                        _common.AddCheckingRecipeParameter(check);
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
            public void TS7F28_H_ProcessProgramVerificationAcknowledge(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F28_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F28_H)");
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
            #endregion

            #region Sent form host-S7
            public void S7F19_H_CurrentEPPDRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F19_H T3-Timeout", false);

                              string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              switch (rtn)
                              {
                                    case "CheckRecipe":
                                          ConcurrentDictionary<string, RecipeInCheck> checkings = _common.GetCheckingRecipeIDs(eqpno);
                                          if (checkings == null || checkings.Count == 0)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Check Recipe Info!");
                                                return;
                                          }
                                          RecipeInCheck checking = new RecipeInCheck();
                                          foreach (string ppid in checkings.Keys)
                                          {
                                                checkings.TryGetValue(ppid, out checking);
                                                if (checking.Info.TrxId == tid)
                                                      break;
                                          }
                                          if (checking.Info == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Check Recipe Info!");
                                                return;
                                          }
                                          RecipeCheckInfo rcInfo = checking.Info;
                                          rcInfo.Result = eRecipeCheckResult.NG;

                                          //Time out只記log
                                          //Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { rcInfo, rcInfo.TrxId });
                                          _common.RemoveCheckingRecipeID(eqpno, rcInfo.RecipeID);
                                          break;
                                    case "OPI":
                                          Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                          if (eqp == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                                return;
                                          }
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S7F19", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F19_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S7F23_H_FormattedProcessProgramSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F23_H T3-Timeout", false);
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
                                    case "ConnectedCheckContorl":
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          break;
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S7F23", "T3 Timeout" });
                                          break;
                                    case "R2R":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { tid, eqp.Data.LINEID, "R2R Parameter Download, EQ Reply Timeout Cancel CST" });
                                          //Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { eqp.Data.NODENO, eqp.Data.PORTNO });
                                          NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                               "R2R Parameter Download(S7F23_H), EQ Reply(S7F24_H) Timeout ");
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F23_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S7F25_H_FormattedProcessProgramRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F25_H T3-Timeout", false);

                              string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string ppid = recvTrx["secs"]["message"]["body"]["PPID"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              switch (rtn)
                              {
                                    case "CheckRecipe":
                                          RecipeInCheck checking = _common.GetCheckingRecipeParameter(eqpno, ppid);
                                          if (checking.Info == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Check Recipe Info!)");
                                                return;
                                          }
                                          RecipeCheckInfo rcInfo = checking.Info;
                                          rcInfo.Result = eRecipeCheckResult.NG;

                                          //Time out只記Log
                                          //Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { rcInfo, rcInfo.TrxId });
                                          _common.RemoveCheckingRecipeParameter(eqpno, ppid);
                                          break;
                                    case "OPI":
                                          Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                          if (eqp == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                                return;
                                          }
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S7F25", "T3 Timeout" });
                                          break;
                              }

                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F25_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S7F28_H_ProcessProgramVerificationAcknowledge(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F28_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F28_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

      }
}