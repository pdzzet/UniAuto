using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class CSOTSECSService
      {
            #region Recipte form equipment-S7
            public void S7F0_E_AbortTransaction(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F0_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        string rtnID = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                        if (string.IsNullOrEmpty(rtnID))
                        {
                              return;
                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F0", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S7F20_E_CurrentEPPDData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F20_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
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

                        //<array1 name="List (number of process programs)" type="L" len="?">
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //</array1>
                        //body
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              string ppid = xNode.ChildNodes[i].InnerText.Trim();

                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F20", _common.ToFormatString(recvTrx.OuterXml) });
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
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //get ack 0 = accepted, other = error
                        string ackc7 = recvTrx["secs"]["message"]["body"]["ACKC7"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S7F26_E_FormattedProcessProgramData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F26_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
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
                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S7F26", _common.ToFormatString(recvTrx.OuterXml) });
                                    return;
                        }
                        //<array1 name="List" type="L" len="6">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //  <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //  <array2 name="List (number of subprocess programs)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBRPID name="SUBRPID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of parameter items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="2">
                        //          <PPNAME name="PPNAME" type="A" len="32" fixlen="False" />
                        //          <PPVALUE name="PPVALUE" type="A" len="32" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();
                        string ppver = recvTrx["secs"]["message"]["body"]["array1"]["PPVER"].InnerText.Trim();

                        //get recipecheckinfo
                        if (!this._RecipeParameters.ContainsKey(eqpno))
                        {
                              return;
                        }
                        ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeParameters = this._RecipeParameters[eqpno];

                        //20150624 cy:return的值若與PPID相等,表示不需再下另一組PPID
                        bool noMorePPIDReq = (ppid == rtn);
                        RecipeCheckInfo info = null;
                        if (NodeRecipeParameters.ContainsKey(tid))
                        {
                              string ppid2 = string.Empty;
                              if (noMorePPIDReq)
                              {
                                    NodeRecipeParameters.TryRemove(tid, out info);
                              }
                              else
                              {
                                    NodeRecipeParameters.TryGetValue(tid, out info);
                                    ppid2 = rtn.Replace(ppid, "");
                                    if (!string.IsNullOrEmpty(ppid2))
                                          TS7F25_H_FormattedProcessProgramRequest(eqp.Data.NODENO, eqp.Data.NODEID, ppid2, ppid2, tid);
                              }

                              if (info != null)
                              {
                                    Dictionary<string, string> dict = info.Parameters as Dictionary<string, string>;

                                    #region Add parameters     
                                  
                                    //add by yang 20161019 ,add ppid,ppver to parameters
                                    dict.Add(eqp.Data.NODEID + "^" + "Recipe_ID", ppid);
                                    dict.Add(eqp.Data.NODEID + "^" + "Recipe_Version", ppver);

                                    XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                                    int loop2 = 0;
                                    string len2 = xNode2.Attributes["len"].InnerText.Trim();
                                    int.TryParse(len2, out loop2);
                                    for (int i = 0; i < loop2; i++)
                                    {
                                          string subppid = xNode2.ChildNodes[i]["SUBRPID"].InnerText.Trim();
                                          XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                          int loop4 = 0;
                                          string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                          int.TryParse(len4, out loop4);
                                          for (int j = 0; j < loop4; j++)
                                          {
                                                string ppname = string.Empty;
                                                //20141217 cy: 登京說明，"MachineID_SubRecipeID_Name"，不看subppid是否等於ppid
                                                //20150326 cy: 在MESService上報時會檢查，格式為EQID@UNITID^PARANAME
                                                ppname = eqp.Data.NODEID + "@" + subppid + "^" + xNode4.ChildNodes[j]["PPNAME"].InnerText.Trim();
                                                //if (subppid == ppid) {
                                                //    ppname = xNode4.ChildNodes[j]["PPNAME"].InnerText.Trim();
                                                //} else {
                                                //    ppname = subppid + "_" + xNode4.ChildNodes[j]["PPNAME"].InnerText.Trim();
                                                //}
                                                string ppvalue = xNode4.ChildNodes[j]["PPVALUE"].InnerText.Trim();
                                                //20150316 cy:增加判斷是否重複key,若重覆,則後蓋前
                                                if (dict.ContainsKey(ppname))
                                                {
                                                      _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                          string.Format("Recipe parameter name duplicate! SubPPID({0}, Name({1})", subppid, xNode4.ChildNodes[j]["PPNAME"].InnerText.Trim()));
                                                      dict.Remove(ppname);
                                                }

                                                dict.Add(ppname, ppvalue);
                                          }
                                    }
                                    #endregion
                                    if (noMorePPIDReq || string.IsNullOrEmpty(ppid2))
                                    {
                                          info.Result = eRecipeCheckResult.OK;
                                          Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { info, tid });
                                    }

                              }
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
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS7F28_H_ProcessProgramVerificationAcknowledge(eqpno, agent, tid, sysbytes);

                        //<array1 name="List" type="L" len="3">
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //  <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //  <array2 name="List (number of errors)" type="L" len="?">
                        //    <array3 name="List" type="L" len="3">
                        //      <ACKC7A name="ACKC7A" type="U1" len="1" fixlen="False" />
                        //      <PPNAME name="PPNAME" type="A" len="32" fixlen="False" />
                        //      <ERRW7 name="ERRW7" type="A" len="40" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();
                        string ppver = recvTrx["secs"]["message"]["body"]["array1"]["PPVER"].InnerText.Trim();
                        List<Tuple<string, string, string>> errs = new List<Tuple<string, string, string>>();
                        string len = recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              errs.Add(Tuple.Create(
                                      xNode.ChildNodes[i]["ACKC7A"].InnerText.Trim(),
                                      xNode.ChildNodes[i]["PPNAME"].InnerText.Trim(),
                                      xNode.ChildNodes[i]["ERRW7"].InnerText.Trim()
                                  )
                              );
                        }
                        if (errs.Count > 0)
                        {

                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S7F74_E_RecipeIDCheckAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F74_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //get ack 0 = OK,1 = NG.
                        string ack6 = recvTrx["secs"]["message"]["body"]["ACK6"].InnerText.Trim();

                        //get recipecheckinfo
                        if (!this._RecipeCheckInfos.ContainsKey(eqpno))
                        {
                              return;
                        }
                        ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeCheckInfos = this._RecipeCheckInfos[eqpno];

                        RecipeCheckInfo info = null;
                        if (NodeRecipeCheckInfos.ContainsKey(tid))
                        {
                              NodeRecipeCheckInfos.TryRemove(tid, out info);
                        }

                        //update info
                        if (info != null)
                        {
                              //0 = OK ,1 = NG
                              info.Result = (ack6 == "1" ? eRecipeCheckResult.NG : eRecipeCheckResult.OK);
                        }
                        Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { info, tid });
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            #endregion

            #region Send by host-S7
            public void TS7F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F0_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F0_H)");
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
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
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
            public void TS7F23_H_FormattedProcessProgramSend(string eqpno, string eqpid, string tag, string ppid, string ppver,
                List<Tuple<string, List<Tuple<string, string>>>> recipeParams, string trxid)
            {
                  try
                  {
                        //check argument
                        if (recipeParams == null || recipeParams.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "recipeParams argument is null or empty.");
                              return;
                        }

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
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = base.CreateTrxID();
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F23_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="3">
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //  <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //  <array2 name="List (numbers of each subprocess program)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBRPID name="SUBRPID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of parameter item)" type="L" len="?">
                        //        <array5 name="List" type="L" len="2">
                        //          <PPNAME name="PPNAME" type="A" len="32" fixlen="False" />
                        //          <PPVALUE name="PPVALUE" type="A" len="32" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["PPID"], ppid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["PPVER"], ppver);
                        XmlNode xNode2 = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop2 = 0;
                        if (recipeParams != null)
                        {
                              loop2 = recipeParams.Count;
                        }
                        xNode2.Attributes["len"].InnerText = loop2.ToString();
                        if (loop2 == 0)
                        {
                              xNode2.RemoveChild(xNode2["array3"]);
                        }
                        for (int i = 0; i < loop2; i++)
                        {
                              _common.CloneChildNode(xNode2, i);
                        }
                        for (int i = 0; i < loop2; i++)
                        {
                              _common.SetItemData(xNode2.ChildNodes[i]["SUBRPID"], recipeParams[i].Item1);
                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              int loop4 = 0;
                              List<Tuple<string, string>> tuples = recipeParams[i].Item2;
                              if (tuples != null)
                              {
                                    loop4 = tuples.Count;
                              }
                              xNode4.Attributes["len"].InnerText = loop4.ToString();
                              if (loop4 == 0)
                              {
                                    xNode4.RemoveChild(xNode4["array5"]);
                              }
                              for (int j = 0; j < loop4; j++)
                              {
                                    _common.CloneChildNode(xNode4, j);
                              }
                              for (int j = 0; j < loop4; j++)
                              {
                                    _common.SetItemData(xNode4.ChildNodes[j]["PPNAME"], tuples[j].Item1);
                                    _common.SetItemData(xNode4.ChildNodes[j]["PPVALUE"], tuples[j].Item2);
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
                        
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F25_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<PPID name="PPID" type="A" len="16" fixlen="False" />
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["PPID"], ppid);

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
            public void TS7F25_H_FormattedProcessProgramRequest(RecipeCheckInfo check, string trxid)
            {
                  try
                  {
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(check.EQPNo);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment No ({0}) in EquipmentEntity!", check.EQPNo));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }

                        //if eq cim off,return ng
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          "Can not send recipe parameter request when CIM mode is OFF!");
                              check.Result = eRecipeCheckResult.CIMOFF;
                              Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }


                        //check trxid valid
                        if (string.IsNullOrEmpty(trxid))
                        {
                              trxid = base.CreateTrxID();
                              check.TrxId = trxid; //set
                        }

                        //get node dict
                        if (!this._RecipeParameters.ContainsKey(check.EQPNo))
                        {
                              bool done = this._RecipeParameters.TryAdd(check.EQPNo, new ConcurrentDictionary<string, RecipeCheckInfo>());
                              if (!done)
                              {
                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("FormattedProcessProgramRequest EQPNo  ({0}) fail to Add!", check.EQPNo));
                                    check.Result = eRecipeCheckResult.NG;
                                    Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                                    return;
                              }
                        }
                        ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeParameters = this._RecipeParameters[check.EQPNo];

                        if (NodeRecipeParameters.ContainsKey(trxid))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("FormattedProcessProgramRequest TrxId ({0}) duplicate!", trxid));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }

                        //send primary
                              TS7F25_H_FormattedProcessProgramRequest(check.EQPNo, eqp.Data.NODEID, check.RecipeID, check.RecipeID, trxid);

                        //add to dict
                        NodeRecipeParameters.TryAdd(trxid, check);
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
            private void TS7F73_H_RecipeIDCheck(string eqpno, string eqpid, string tag, string automanual, List<Tuple<string, string>> recipes, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S7F73_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S7F73_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S7F73_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="3">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <AUTOMANUAL name="AUTOMANUAL" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (number of need checked recipe)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //      <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"], eqpid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["AUTOMANUAL"], automanual);
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop = 0;
                        if (recipes != null)
                        {
                              loop = recipes.Count;
                        }
                        xNode.Attributes["len"].InnerText = loop.ToString();
                        if (loop == 0)
                        {
                              xNode.RemoveChild(xNode["array3"]);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.SetItemData(xNode.ChildNodes[i]["PPID"], recipes[i].Item1);
                              _common.SetItemData(xNode.ChildNodes[i]["PPVER"], recipes[i].Item2);
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
            public void TS7F73_H_RecipeIDCheck(RecipeCheckInfo check, string trxid)
            {
                  try
                  {
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(check.EQPNo);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment No ({0}) in EquipmentEntity!", check.EQPNo));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }

                        //if eq cim off,return ng
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                          "Can not send recipe ID check request when CIM mode is OFF!");
                              check.Result = eRecipeCheckResult.CIMOFF;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }

                        //check trxid valid
                        if (string.IsNullOrEmpty(trxid))
                        {
                              trxid = base.CreateTrxID();
                              check.TrxId = trxid; //set
                        }

                        //get node dict
                        if (!this._RecipeCheckInfos.ContainsKey(check.EQPNo))
                        {
                              bool done = this._RecipeCheckInfos.TryAdd(check.EQPNo, new ConcurrentDictionary<string, RecipeCheckInfo>());
                              if (!done)
                              {
                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("recipecheckInfo EQPNo ({0}) fail to Add!", check.EQPNo));
                                    check.Result = eRecipeCheckResult.NG;
                                    Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                                    return;
                              }
                        }
                        ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeCheckInfos = this._RecipeCheckInfos[check.EQPNo];

                        if (NodeRecipeCheckInfos.ContainsKey(trxid))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("RecipeRegisterValidationCommandReplyForSECS TrxId ({0}) duplicate!", trxid));
                              check.Result = eRecipeCheckResult.NG;
                              Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { check, trxid });
                              return;
                        }

                        //20150523 cy:若長度為8碼,且為Canon機台,則拆成2組
                        List<Tuple<string, string>> checkList = new List<Tuple<string, string>>();
                        if (eqp.Data.NODEATTRIBUTE.Equals("CANON") && check.RecipeID.Length == 8)
                        {
                              checkList.Add(Tuple.Create(check.RecipeID.Substring(0, 4), string.Empty));
                              if (check.RecipeID.Substring(4) != "0000")
                                    checkList.Add(Tuple.Create(check.RecipeID.Substring(4), string.Empty));
                        }
                        else
                              checkList.Add(Tuple.Create(check.RecipeID, string.Empty));
                        //send primary
                        TS7F73_H_RecipeIDCheck(check.EQPNo, eqp.Data.NODEID, trxid,
                              //1：Auto
                              //2：Manual
                            check.Mode == 1 ? "A" : "M",
                            //new List<Tuple<string, string>>() { Tuple.Create(check.RecipeID, string.Empty) },
                            checkList,
                            trxid);

                        //add to node dict
                        NodeRecipeCheckInfos.TryAdd(trxid, check);
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
            public void S7F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F0_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F0_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S7F19_H_CurrentEPPDRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F19_H T3-Timeout", false);
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
                              //get basic
                              string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              string ppid = recvTrx["secs"]["message"]["body"]["PPID"].InnerText.Trim();
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S7F19", "T3 Timeout" });
                                          break;
                              }
                              
                              //Timeout要清
                              //get recipecheckinfo
                              if (!this._RecipeParameters.ContainsKey(eqpno))
                              {
                                    return;
                              }
                              ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeParameters = this._RecipeParameters[eqpno];
                              RecipeCheckInfo info = null;
                              if (NodeRecipeParameters.ContainsKey(tid))
                              {
                                    NodeRecipeParameters.TryRemove(tid, out info);
                              }

                              //若timeout只記Log,不回報
                              //Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForSECS", new object[] { info, tid });
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid,
                                  "FormattedProcessProgramRequest result timeout!");

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
            public void S7F73_H_RecipeIDCheck(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F73_H T3-Timeout", false);

                              //get basic
                              string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                              //Timeout要清
                              //get recipecheckinfo
                              if (!this._RecipeCheckInfos.ContainsKey(eqpno))
                              {
                                    return;
                              }
                              ConcurrentDictionary<string, RecipeCheckInfo> NodeRecipeCheckInfos = this._RecipeCheckInfos[eqpno];
                              RecipeCheckInfo info = null;
                              if (NodeRecipeCheckInfos.ContainsKey(tid))
                              {
                                    NodeRecipeCheckInfos.TryRemove(tid, out info);
                              }

                              //若timeout只記Log,不回報
                              //Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForSECS", new object[] { info, tid });
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "RecipeIDCheck result timeout!");
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S7F73_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
      }
}