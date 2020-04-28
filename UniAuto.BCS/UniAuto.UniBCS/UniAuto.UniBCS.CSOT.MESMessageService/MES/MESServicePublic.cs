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
using System.Collections.Concurrent;


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        private ConcurrentQueue<xMessage> _equipmentStateQueue = new ConcurrentQueue<xMessage>();
        private DateTime _eqpStateLastReportTime = DateTime.Now;
        private object _timeSync = new object();
        private ConcurrentQueue<xMessage> _processEndQueue = new ConcurrentQueue<xMessage>();
        private DateTime _processEndLastReportTime = DateTime.Now;
        private System.Timers.Timer _queueTimer = new System.Timers.Timer(500);
        private bool _waitForMachineStateChangeReply = false;
        private bool _processEndMESReplyFlag = false;   // add by bruce 20160413 for T2 Issue
        private ConcurrentQueue<xMessage> _cimModeQueue = new ConcurrentQueue<xMessage>();  //add by yang 20161204 for CIM Mode Report Queue
        public DateTime _cimModeChangeLastReportTime = DateTime.Now;

        /// <summary>
        /// 設定送出的Message Set 裏需要Return code, Return Message
        /// Mes回傳時會帶回，可以此區別事件的差別
        /// </summary>
        /// <param name="xml">Message Xml</param>
        /// <param name="returnCode">RETURN\RETURNCODE</param>
        /// <param name="returnMsg">RETURN\RETURNMESSAGE</param>
        private void SETReturnCodeMsg(XmlDocument xml, string returnCode, string returnMsg)
        {
            try
            {
                if (returnCode != string.Empty)
                {
                    xml[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText = returnCode;
                }
                if (returnMsg != string.Empty)
                {
                    xml[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText = returnMsg;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
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

        private void SendToOEE(XmlDocument xml)
        {
            xMessage msg = new xMessage();
            msg.ToAgent = eAgentName.OEEAgent;
            msg.Data = xml.OuterXml;
            PutMessage(msg);
        }

        private void SendToEDA(XmlDocument xml)
        {
            xMessage msg = new xMessage();
            msg.ToAgent = eAgentName.EDAAgent;
            msg.Data = xml.OuterXml;
            PutMessage(msg);
        }

        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                xMessage message = null;
                bool hasSend = false;
                #region MachineStateChanged Delay
                if (_equipmentStateQueue.Count > 0)
                {
                    if (_waitForMachineStateChangeReply == false)
                    { //BCS 没有等待MES回复可以直接上报
                        hasSend = true;
                    }
                    else
                    {//BCS 在等待MES回复需要Check 有没有Time out
                        TimeSpan ts = DateTime.Now.Subtract(_eqpStateLastReportTime);//MES  Time out                      
                        if (Math.Abs(ts.TotalMilliseconds) > ParameterManager["MESTIMEOUT"].GetInteger())
                        {
                            hasSend = true;
                        }
                    }
                    if (hasSend)
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("MES Synchronization MachineStateChanged Queue count=({0}).", _equipmentStateQueue.Count));
                        bool done1 = _equipmentStateQueue.TryDequeue(out message);

                        if (done1)
                        {
                            lock (_timeSync)
                            {
                                _eqpStateLastReportTime = DateTime.Now;
                                _waitForMachineStateChangeReply = true;
                            }
                            PutMessage(message);
                        }
                    }
                }
                #endregion
                #region ProcessEnd
                if (_processEndQueue.Count > 0)
                {
                    if (_processEndMESReplyFlag)    // add by bruce 20160413 for T2 Issue
                    {
                        bool done1 = _processEndQueue.TryDequeue(out message);

                        if (done1)
                        {
                            lock (_timeSync)
                            {
                                _processEndLastReportTime = DateTime.Now;
                                _processEndMESReplyFlag = false;
                            }
                            PutMessage(message);
                        }
                    }
                    else
                    {
                        TimeSpan ts = DateTime.Now.Subtract(_processEndLastReportTime);//MES  Time out
                        if (Math.Abs(ts.TotalMilliseconds) > ParameterManager["MESTIMEOUT"].GetInteger())
                        {
                            LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("MES Synchronization LotProcessEnd Queue count=({0}).", _equipmentStateQueue.Count));

                            bool done1 = _processEndQueue.TryDequeue(out message);
                            if (done1)
                            {
                                lock (_timeSync)
                                {
                                    _processEndLastReportTime = DateTime.Now;
                                    _processEndMESReplyFlag = false;
                                }
                                PutMessage(message);
                            }
                        }
                    }
                }
                #endregion
                #region[CIMModeChangeReport Delay]
                if (_cimModeQueue.Count() > 0)
                {
                    TimeSpan ts = DateTime.Now.Subtract(_cimModeChangeLastReportTime);//MES  Time out                      
                    if (Math.Abs(ts.TotalMilliseconds) > 1000) //hardcode timeout=1000ms
                        hasSend = true;
                    if (hasSend)
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("MES Synchronization CIMModeChangeReport Queue count=({0}).", _cimModeQueue.Count));
                        bool done1 = _cimModeQueue.TryDequeue(out message);
                        if (done1)
                        {
                            lock (_timeSync)
                            {
                                _cimModeChangeLastReportTime = DateTime.Now;
                            }
                            PutMessage(message);
                        }
                    }
                }
                #endregion
            }
            catch
            {

            }
        }

        private IServerAgent GetServerAgent()
        {
            return GetServerAgent(eAgentName.MESAgent);
        }
        /// <summary>
        /// Agent回報Service, 訊息已經送出, Service決定是否加入T3
        /// </summary>
        /// <param name="sendDt"></param>
        /// <param name="xml"></param>
        /// <param name="t3TimeoutSecond"></param>
        public void MES_MessageSend(DateTime sendDt, string xml, int t3TimeoutSecond)
        {
            try
            {
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        System.Timers.Timer timer;
        double interval = 4 * 3600 * 1000;

        /// <summary>
        /// Agent回報Service, Tibco Open成功
        /// </summary>PortTransferStateChangedPort
        public void MES_TibcoOpen()
        {
            //尚未決定Service要做何處理
            try
            {
                //Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                //string trxID = CreateTrxID();
                //Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                   string.Format("[LINENAME={0}] [BCS -> MES] [{1}] MES Tibrv Open.",line.Data.LINEID,trxID));
                //AreYouThereRequest(trxID, line.Data.LINEID);
                foreach (Line line in ObjectManager.LineManager.GetLines())
                {
                    string trxID = CreateTrxID();
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                       string.Format("[LINENAME={0}] [BCS -> MES] [{1}] MES Tibrv Open.", line.Data.LINEID, trxID));
                    line.File.MesConnectState = "Connect";
                    AreYouThereRequest(trxID, line.Data.LINEID);
                }
                timer = new System.Timers.Timer(interval);
                timer.AutoReset = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                timer.Start();
                _queueTimer.AutoReset = true;
                _queueTimer.Elapsed += new System.Timers.ElapsedEventHandler(QueueTimer_Elapsed);
                _queueTimer.Start();
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        private XmlNode GetMESBodyNode(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/BODY");
        }

        //Watson Modify 20150131 For MES 
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

        private string GetMESPermitFlag(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/BODY/PERMITFLAG").InnerText;
        }



        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                AreYouTherePeriodicityRequest();

            }
            catch
            {

            }
        }

        /// <summary>
        /// 需要先Queue住，等MES回复或Time out 在发送
        /// </summary>
        /// <param name="doc"></param>
        public void SendToQueue(XmlDocument doc)
        {
            xMessage msg = new xMessage();
            msg.Name = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.MESSAGENAME].InnerText;
            msg.TransactionID = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
            msg.ToAgent = eAgentName.MESAgent;
            msg.Data = doc.OuterXml;
            if (msg.Name == "MachineStateChanged")
            {
                _equipmentStateQueue.Enqueue(msg);
            }
            else if ((msg.Name == "LotProcessEnd") || (msg.Name == "LotProcessAbnormalEnd"))
            {
                _processEndQueue.Enqueue(msg);
            }
            if (msg.Name == "CIMModeChangeReport")
                _cimModeQueue.Enqueue(msg);

        }




        

    }
}
