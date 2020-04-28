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
        #region Recipte form equipment-S64
        public void S64F0_E_AbortTransaction(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F0_E");
            #region Handle Logic
			try {
				string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
				string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
				Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
				if (eqp == null) {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                           "Can not find Equipment Number in EquipmentEntity!");
					return;
				}

				string rtnID = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
				if (string.IsNullOrEmpty(rtnID)) {
					return;
				}

				//check if opi request
				string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
				switch (rtn) {
					case "OPI":
						Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
							new object[4] { tid, eqp.Data.LINEID, "Reply S64F0", _common.ToFormatString(recvTrx.OuterXml) });
						break;
				}
			} catch (Exception ex) {
				NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
			}
            #endregion
        }
        public void S64F2_E_MeasurementResultSendAcknowledge(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F2_E");
            try
            {
                #region Handle Logic
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

                //<ACKC64 name="ACKC64" type="B" len="1" fixlen="False" />
                //body
                string ACKC64 = recvTrx["secs"]["message"]["body"]["ACKC64"].InnerText.Trim();

                //check if opi request
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S64F2", _common.ToFormatString(recvTrx.OuterXml) });
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S64F3_E_FeedbackResultSend(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F3_E");
            try
            {
                #region Handle Logic
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                string wbit = recvTrx["secs"]["message"]["header"].Attributes["wbit"].InnerText.Trim();

                //20141119 cy:Waiting reply not must be. 
                //reply secondary
                if (wbit == "1")
                    TS64F4_H_FeedbackResultSendAcknowledge(eqpno, agent, tid, sysbytes);

                //<array1 name="List" type="L" len="2">
                //  <RTNCODE name="RTNCODE" type="U1" len="1" fixlen="False" />
                //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                //</array1>
                //body
                string rtncode = recvTrx["secs"]["message"]["body"]["array1"]["RTNCODE"].InnerText.Trim();
                string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();


                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Send by host-S64
        public void TS64F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
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
                XmlDocument sendTrx = agent.GetTransactionFormat("S64F0_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S64F0_H)");
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
        public void TS64F1_H_MeasurementResultSend(string eqpno, string eqpid, string ppid,
			List<Tuple<string,string,List<Tuple<string,string,string,string,string>>>> glasslist , string tag, string trxid)
        {
            try
            {
				//check argument
				if (glasslist == null || glasslist.Count <= 0) {
					NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "glasslist argument is null or empty.");
					return;
				}

                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0}]", eqpid));
                    return;
                }

                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S64F1_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S64F1_H)");
                    return;
                }

                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                
	//<array1 name="List" type="L" len="2">
	//    <PPID name="PPID" type="A" len="16" fixlen="False" />
	//    <array2 name="List (number of glass)" type="L" len="?">
	//      <array3 name="List" type="L" len="3">
	//        <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
	//        <INSPDT name="INSPDT" type="A" len="14" fixlen="False" />
	//        <array4 name="List (number of measurement data)" type="L" len="?">
	//          <array5 name="List" type="L" len="5">
	//            <SHOTNO name="SHOTNO" type="U1" len="1" fixlen="False" />
	//            <MPX name="MPX" type="A" len="9" fixlen="False" />
	//            <MPY name="MPY" type="A" len="9" fixlen="False" />
	//            <ODX name="ODX" type="A" len="9" fixlen="False" />
	//            <ODY name="ODY" type="A" len="9" fixlen="False" />
	//          </array5>
	//        </array4>
	//      </array3>
	//    </array2>
	//  </array1>
				//body
				_common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["PPID"], ppid);
				XmlNode xNode2 = sendTrx["secs"]["message"]["body"]["array1"]["array2"];			
				int loop2 = 0;
				if (glasslist != null) {
					loop2 = glasslist.Count;
				}
				xNode2.Attributes["len"].InnerText = loop2.ToString();
				if (loop2 == 0) {
					xNode2.RemoveChild(xNode2["array3"]);
				}
				for (int i = 0; i < loop2; i++) {
					_common.CloneChildNode(xNode2, i);
				}
				for (int i = 0; i < loop2; i++) 
				{										
					_common.SetItemData(xNode2.ChildNodes[i]["GLASSID"], glasslist[i].Item1);
					_common.SetItemData(xNode2.ChildNodes[i]["INSPDT"], glasslist[i].Item2);
					XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
					int loop4 = 0;
					List<Tuple<string, string, string, string, string>> tuples = glasslist[i].Item3;
					if (tuples != null) {
						loop4 = tuples.Count;
					}
					xNode4.Attributes["len"].InnerText = loop4.ToString();
					if (loop4 == 0) {
						xNode4.RemoveChild(xNode4["array5"]);
					}
					for (int j = 0; j < loop4; j++) {
						_common.CloneChildNode(xNode4, j);
					}
					for (int j = 0; j < loop4; j++) {						
						_common.SetItemData(xNode4.ChildNodes[j]["SHOTNO"], tuples[j].Item1);
						_common.SetItemData(xNode4.ChildNodes[j]["MPX"], tuples[j].Item2);
						_common.SetItemData(xNode4.ChildNodes[j]["MPY"], tuples[j].Item3);
						_common.SetItemData(xNode4.ChildNodes[j]["ODX"], tuples[j].Item4);
						_common.SetItemData(xNode4.ChildNodes[j]["ODY"], tuples[j].Item5);
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
        public void TS64F4_H_FeedbackResultSendAcknowledge(string eqpno, string eqpid, string tid, string sysbytes)
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
                XmlDocument sendTrx = agent.GetTransactionFormat("S64F4_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S64F4_H)");
                    return;
                }
                
				//Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["ACKC64"].InnerText = "0";
                
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

        #region Sent form host-S64
        public void S64F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F0_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F0_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S64F1_H_MeasurementResultSend(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F1_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F1_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S64F4_H_FeedbackResultSendAcknowledge(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F4_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S64F4_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}