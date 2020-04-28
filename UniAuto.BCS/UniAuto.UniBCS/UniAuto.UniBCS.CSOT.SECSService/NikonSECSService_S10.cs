using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class NikonSECSService
    {
        #region Recipte form equipment-S10
        public void S10F0_E_AbortTransaction(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F0_E");
            #region Handle Logic
            //TODO:Check control mode and abort setting and reply SnF0 or not.
            //TODO:Logic handle.
            #endregion
        }
        public void S10F1_E_TerminalRequest(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F1_E");
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
                    TS10F2_H_TerminalRequestAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS10F2_H_TerminalRequestAcknowledge(eqpno, agent, tid, sysbytes, 0);
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("Equipment request terminal display. '{0}:{1}'.",
                            recvTrx["secs"]["message"]["body"]["array1"]["TID"].InnerText.Trim(),
                            recvTrx["secs"]["message"]["body"]["array1"]["TEXT"].InnerText.Trim()));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S10F4_E_TerminalDisplaySingleAcknowledge(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F4_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                string ack = recvTrx["secs"]["message"]["body"]["ACKC10"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("Equipment reply terminal display result, single. '{0}:{1}'.",
                            ack, ConvertDescriptionACKC10(ack)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S10F6_E_TerminalDisplayMultiAcknowledge(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F6_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                string ack = recvTrx["secs"]["message"]["body"]["ACKC10"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("Equipment reply terminal display result, multi-block. '{0}:{1}'.",
                            ack, ConvertDescriptionACKC10(ack)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }

        #endregion

        #region Send by host-S10
        public void TS10F2_H_TerminalRequestAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, byte ack)
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
                XmlDocument sendTrx = agent.GetTransactionFormat("S10F2_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S10F2_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["ACKC10"].InnerText = ack.ToString();
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
        public void TS10F3_H_TerminalDisplaySingle(string eqpno, string eqpid, string text, string trxid, string tag)
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
                XmlDocument sendTrx = agent.GetTransactionFormat("S10F3_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S10F3_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["array1"]["TID"].InnerText = "0";
                sendTrx["secs"]["message"]["body"]["array1"]["TEXT"].InnerText = text;
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S10F3_H";
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
        public void TS10F5_H_TerminalDisplayMulti(string eqpno, string eqpid, byte tid, string[] text, string tag)
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
                XmlDocument sendTrx = agent.GetTransactionFormat("S10F5_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S10F5_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = base.CreateTrxID();
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["array1"]["TID"].InnerText = tid.ToString();
                if (text != null && text.Length > 0)
                {
                    int iCount = text.Length > 20 ? 20 : text.Length;
                    XmlNodeList xNodeList = sendTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                    for (int i0 = 0; i0 < iCount; i0++)
                    {
                        xNodeList[i0].InnerText = text[i0];
                    }
                }
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S10F5_H";
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

        #region Sent form host-S10
        public void S10F2_H_TerminalRequestAcknowledge(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F2_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F2_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S10F3_H_TerminalDisplaySingle(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F3_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F3_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S10F5_H_TerminalDisplayMulti(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F5_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S10F5_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}