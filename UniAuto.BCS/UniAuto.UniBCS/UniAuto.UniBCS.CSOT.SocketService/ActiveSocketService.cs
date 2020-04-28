using System;
using System.Collections;
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
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.BcSocketSpec;

namespace UniAuto.UniBCS.CSOT.SocketService
{
    public class ActiveSocketService : AbstractService
    {
        //private class T3Message
        //{
        //    public DateTime SendDt { get; set; }
        //    public string SessionId { get; set; }
        //    public Message Msg { get; set; }
        //    public int T3Second { get; set; }
        //    public T3Message(DateTime dt, string sessionId, Message msg, int t3Second)
        //    {
        //        SendDt = dt;
        //        SessionId = sessionId;
        //        Msg = msg;
        //        T3Second = t3Second;
        //    }
        //    public string Key { get { return string.Format("{0}_{1}", SessionId, Msg.HEADER.TRANSACTIONID); } }
        //}
        //private Dictionary<string, T3Message> _waitForT3 = new Dictionary<string, T3Message>();
        //private Thread _t3Thread = null;
        private bool _run = false;
        private Timer _linkTestTimer = null;
        private ManualResetEvent _linkTestMRE = new ManualResetEvent(true);
        private readonly string AGENT_NAME = "ActiveSocketAgent";
        private string _localSocketSessionId = string.Empty;
        private string _remoteSocketSessionId = string.Empty;
        private string _remoteLineName = string.Empty;
        private IServerAgent _activeSocketAgent = null;
        public FileFormatManager FileFormatManager { get; set; }
        
        private IServerAgent GetServerAgent()
        {
            //小心Spring的_applicationContext.GetObject(name)
            //在Init-Method與Destory-Method會與Spring的GetObject使用相同LOCK
            //需留心以免死結
            if (_activeSocketAgent != null)
                return _activeSocketAgent;
            if (Workbench.Instance.AgentList.ContainsKey(AGENT_NAME))
            {
                _activeSocketAgent = GetServerAgent(AGENT_NAME);
                return _activeSocketAgent;
            }
            return null;
        }

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
                _linkTestTimer = new Timer(LinkTestTimerFunc);
                _linkTestTimer.Change(1000, Timeout.Infinite);
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


        /// <summary>ActiveSocketAgent回報連線
        /// 
        /// </summary>
        /// <param name="localSessionId"></param>
        /// <param name="remoteSessionId"></param>
        public void ACTIVE_CONNECT(string localSessionId, string remoteSessionId)
        {
            try
            {
                GetServerAgent();
                _localSocketSessionId = localSessionId;
                _remoteSocketSessionId = remoteSessionId;
                
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("AreYouThereRequest") as XmlDocument;
                AreYouThereRequest trx = Spec.XMLtoMessage(xml_doc) as AreYouThereRequest;
                //trx.BODY.FACTORYTYPE =
                trx.BODY.LINENAME = Workbench.ServerName;

                string xml = trx.WriteToXml();
                xMessage msg = new xMessage();
                msg.Name = trx.HEADER.MESSAGENAME;
                msg.TransactionID = trx.HEADER.TRANSACTIONID;
                msg.ToAgent = AGENT_NAME;
                msg.Data = xml;
                PutMessage(msg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>ActiveSocketAgent回報斷線
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        public void ACTIVE_DISCONNECT(string sessionId)
        {
            _localSocketSessionId = string.Empty;
            _remoteSocketSessionId = string.Empty;
        }

        /// <summary>當Agent發送Message後, Agent會呼叫MessageSend以通知Service, 由Service決定是否加入T3
        /// 
        /// </summary>
        /// <param name="sendDt"></param>
        /// <param name="xml"></param>
        /// <param name="t3TimeoutSecond"></param>
        public void ACTIVE_MessageSend(DateTime sendDt, string sessionId, string xml, int t3TimeoutSecond)
        {
            try
            {
                //if (xml.IndexOf("<TRANSACTIONID>CellShortCut</TRANSACTIONID>") > 0)
                //{
                //    return;
                //}
                //Message msg = Spec.XMLtoMessage(xml);
                //if (msg.WaitReply != string.Empty && t3TimeoutSecond > 0)
                //{
                //    string key = string.Format("{0}_{1}", sessionId, msg.HEADER.TRANSACTIONID);
                //    //NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Add to WaitForT3", msg.HEADER.MESSAGENAME, key));
                //    //T3Manager.AddT3(new T3Manager.T3Message(GetType().Name, sendDt, key, msg, t3TimeoutSecond));
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>當Agent收到Message後, Agent會先呼叫MessageRecv以通知Service, 由Service決定是否移除T3
        /// Agent呼叫MessageRecv之後才會根據不同Message, Invoke不同Service Method
        /// </summary>
        /// <param name="recvDt"></param>
        /// <param name="sessionId"></param>
        /// <param name="xml"></param>
        public void ACTIVE_MessageRecv(DateTime recvDt, string sessionId, string xml)
        {
            //try
            //{
            //    Message reply_msg = Spec.XMLtoMessage(xml);
            //    string command_msg_name = Spec.GetMessageByReply(reply_msg.HEADER.MESSAGENAME);
            //    if (command_msg_name != string.Empty)
            //    {
            //        string key = string.Format("{0}_{1}", sessionId, reply_msg.HEADER.TRANSACTIONID);
            //        //T3Manager.T3Message command_t3 = T3Manager.TryRemoveT3(GetType().Name, key);
            //        //if (command_t3 != null)
            //        //{
            //        //    Message command_msg = command_t3.Obj as Message;
            //        //    NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Remove from WaitForT3", command_msg.HEADER.MESSAGENAME, key));
            //        //}
            //    }
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //}
        }

        /// <summary>ActiveSocketAgent收到AreYouThereReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_AreYouThereReply(XmlDocument xmlDoc)
        {
            try
            {
                AreYouThereReply msg = Spec.XMLtoMessage(xmlDoc) as AreYouThereReply;
                _remoteLineName = msg.BODY.LINENAME;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>ActiveSocketAgent收到BCLinkTest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_BCSLinkTest(XmlDocument xmlDoc)
        {
            try
            {
                BCSLinkTest msg = Spec.XMLtoMessage(xmlDoc) as BCSLinkTest;
                BCSLinkTestReply reply = new BCSLinkTestReply();
                reply.HEADER.TRANSACTIONID = msg.HEADER.TRANSACTIONID;
                reply.BODY.LINENAME = msg.BODY.LINENAME;

                xMessage xmsg = new xMessage();
                xmsg.Name = reply.HEADER.MESSAGENAME;
                xmsg.TransactionID = reply.HEADER.TRANSACTIONID;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.WriteToXml();
                PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>ActiveSocketAgent收到BCLinkTestReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_BCSLinkTestReply(XmlDocument xmlDoc)
        {
            try
            {
                // Do Nothing
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>提供給MES Service呼叫
        /// 
        /// </summary>
        /// <param name="CassetteSno"></param>
        /// <param name="JobSno"></param>
        /// <param name="xmlDoc">MES的CFShortCutPermitReply(包括HEADER, BODY, RETURN)</param>
        public XmlDocument JobShortCutPermit(Job job, string xml)
        {
            try
            {
                GetServerAgent();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                //移除BODY
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("JOBShortCutPermit") as XmlDocument;
                xml_doc["MESSAGE"].RemoveChild(xml_doc["MESSAGE"]["BODY"]);

                // job xml 序列化
                StringWriterUTF8 sw = new StringWriterUTF8();
                System.Xml.Serialization.XmlSerializerNamespaces names = new System.Xml.Serialization.XmlSerializerNamespaces();
                names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd
                System.Xml.Serialization.XmlSerializer xmlwrite = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                xmlwrite.Serialize(sw, job, names);
                string job_xml = sw.ToString();
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);

                // job xml 放到 JOB
                XmlElement job_element = xml_doc.CreateElement("JOB");
                xml_doc["MESSAGE"].AppendChild(job_element);
                job_element.InnerXml = job_xml_doc["Job"].OuterXml;

                // 把參數xml 放到 XML
                XmlElement xml_element = xml_doc.CreateElement("XML");
                xml_doc["MESSAGE"].AppendChild(xml_element);
                xml_element.InnerXml = xmlDoc["MESSAGE"].OuterXml;

                //將XML發送到Passive端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                return xml_doc;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            return null;
        }

        /// <summary>ActiveSocketAgent收到JOBShortCutPermitReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_JOBShortCutPermitReply(XmlDocument xmlDoc)
        {
            try
            {
                // do nothing
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //private void CopyNode(XmlDocument xmlDoc, XmlNode 

        /// <summary>提供給JunLon呼叫
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void CellShortCut(string lineID,string nodeID, Job job)
        {
            try
            {
                GetServerAgent();
                // job xml 序列化
                StringWriterUTF8 sw = new StringWriterUTF8();
                System.Xml.Serialization.XmlSerializerNamespaces names = new System.Xml.Serialization.XmlSerializerNamespaces();
                names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd
                System.Xml.Serialization.XmlSerializer xmlwrite = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                xmlwrite.Serialize(sw, job, names);
                string job_xml = sw.ToString();
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);
                // job xml 放到body
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("CellShortCut") as XmlDocument;
                XmlNode body = xml_doc.SelectSingleNode("//MESSAGE/BODY");
                body.InnerXml = job_xml_doc["Job"].OuterXml; //會讓//MESSAGE/BODY裡面的字變成亂碼, 找不出原因, 只好手動Copy XML Node
                //將XML發送到Passive端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                //string msg = string.Format("CELL Short Cut Send Message !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                string msg = string.Format("CELL SHOT CUT SEND MESSAGE !! LINEID=[{0}] EQUIPMENT=[{1}] CASSETTE_SEQNO=[{2}], JOB_SEQNO=[{3}],GLASSID =[{4}]",
                    lineID,nodeID, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                
                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>提供給JunLon呼叫
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ArrayShortCut(Job job)
        {
            try
            {
                GetServerAgent();
                // job xml 序列化
                StringWriterUTF8 sw = new StringWriterUTF8();
                System.Xml.Serialization.XmlSerializerNamespaces names = new System.Xml.Serialization.XmlSerializerNamespaces();
                names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd
                System.Xml.Serialization.XmlSerializer xmlwrite = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                xmlwrite.Serialize(sw, job, names);
                string job_xml = sw.ToString();
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);
                // job xml 放到body
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("ArrayShortCut") as XmlDocument;
                XmlNode body = xml_doc.SelectSingleNode("//MESSAGE/BODY");
                body.InnerXml = job_xml_doc["Job"].OuterXml; //會讓//MESSAGE/BODY裡面的字變成亂碼, 找不出原因, 只好手動Copy XML Node
                //將XML發送到Passive端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                //string msg = string.Format("CELL Short Cut Send Message !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                string msg = string.Format("ARRAY SHOT CUT SEND MESSAGE !! CASSETTE_SEQNO=[{0}], JOB_SEQNO=[{1}],GLASSID =[{2}]",
                   job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);

                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>收到CellShortCut
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ACTIVE_CellShortCut(XmlDocument xmlDoc)
        {
            xMessage xmsg = null;
            try
            {
                GetServerAgent();
                string job_xml = xmlDoc.SelectSingleNode("//MESSAGE/BODY").InnerXml;
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);
                System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                System.IO.StringReader sr = new System.IO.StringReader(job_xml_doc.InnerXml);
                Job job = (Job)xmlserializer.Deserialize(sr);

                XmlDocument reply = _activeSocketAgent.GetTransactionFormat("CellShortCutReply") as XmlDocument;
                reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                reply["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText = job.CassetteSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBNo"].InnerText = job.JobSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBID"].InnerText = job.GlassChipMaskBlockID;

                try
                {
                    ObjectManager.JobManager.AddJob(job);

                    //Jun Add 20150112 Create File Service
                    string subPath = string.Format(@"{0}\{1}", this.ServerName, job.CassetteSequenceNo);
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);

                    string msg = string.Format("CELL Short Cut Receive Message OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2})", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
                }
                catch (Exception ex)
                {
                    reply["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText = "1";
                    reply["MESSAGE"]["RETURN"]["RETURNMESSAGE"].InnerText = ex.Message;

                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }

                xmsg = new xMessage();
                xmsg.Name = reply["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText;
                xmsg.TransactionID = reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.OuterXml;
            }
            catch (Exception ex_)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex_);
            }

            if (xmsg != null)
                QueueManager.PutMessage(xmsg);
        }

        /// <summary>收到CellShortCutReply
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ACTIVE_CellShortCutReply(XmlDocument xmlDoc)
        {
            try
            {
                string cst_seq = xmlDoc["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText;
                string job_seq = xmlDoc["MESSAGE"]["BODY"]["JOBNo"].InnerText;
                string glass_chip_mask_cut_id = xmlDoc["MESSAGE"]["BODY"]["JOBID"].InnerText;
                string return_code = xmlDoc["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText;

                Job job = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                if (job == null) throw new Exception(string.Format(string.Format("CELL Short Cut Reply Message Error !! Can't Find Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", cst_seq, job_seq, glass_chip_mask_cut_id)));

                if (!string.IsNullOrEmpty(return_code))
                {
                    if (return_code == "0")
                    {
                        string msg = string.Format("CELL Short Cut Receive ReplyMessage OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                        ObjectManager.JobManager.DeleteJob(job);
                    }
                    else
                    {
                        string msg = string.Format("CELL Short Cut Receive ReplyMessage NG !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                    }
                }
            }
            catch (Exception ex_)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex_);
            }
        }

        /// <summary>收到ArrayShortCut (ELA)
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ACTIVE_ArrayShortCut(XmlDocument xmlDoc)
        {
            xMessage xmsg = null;
            try
            {
                GetServerAgent();
                string job_xml = xmlDoc.SelectSingleNode("//MESSAGE/BODY").InnerXml;
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);
                System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                System.IO.StringReader sr = new System.IO.StringReader(job_xml_doc.InnerXml);
                Job job = (Job)xmlserializer.Deserialize(sr);

                XmlDocument reply = _activeSocketAgent.GetTransactionFormat("ArrayShortCutReply") as XmlDocument;
                reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                reply["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText = job.CassetteSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBNo"].InnerText = job.JobSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBID"].InnerText = job.GlassChipMaskBlockID;

                try
                {
                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                    string msg = string.Format("Array Short Cut Receive Message OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2})", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
                }
                catch (Exception ex)
                {
                    reply["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText = "1";
                    reply["MESSAGE"]["RETURN"]["RETURNMESSAGE"].InnerText = ex.Message;

                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }

                xmsg = new xMessage();
                xmsg.Name = reply["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText;
                xmsg.TransactionID = reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.OuterXml;
            }
            catch (Exception ex_)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex_);
            }

            if (xmsg != null)
                QueueManager.PutMessage(xmsg);
        }

        /// <summary>收到ArrayShortCutReply
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ACTIVE_ArrayShortCutReply(XmlDocument xmlDoc)
        {
            try
            {
                string cst_seq = xmlDoc["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText;
                string job_seq = xmlDoc["MESSAGE"]["BODY"]["JOBNo"].InnerText;
                string glass_chip_mask_cut_id = xmlDoc["MESSAGE"]["BODY"]["JOBID"].InnerText;
                string return_code = xmlDoc["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText;
                string trxID = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;

                Job job = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                if (job == null) throw new Exception(string.Format(string.Format("ARRAY Short Cut Reply Message Error !! Can't Find Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", cst_seq, job_seq, glass_chip_mask_cut_id)));

                if (!string.IsNullOrEmpty(return_code))
                {
                    if (return_code == "0")
                    {
                        string msg = string.Format("ARRAY Short Cut Receive ReplyMessage OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                        //ObjectManager.JobManager.DeleteJob(job);

                        Invoke(eServiceName.ArraySpecialService, "JobLineOutCheckReportReply", new object[] { "L2", eBitResult.ON, eReturnCode1.OK, trxID });
                    }
                    else
                    {
                        string msg = string.Format("ARRAY Short Cut Receive ReplyMessage NG !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);

                        Invoke(eServiceName.ArraySpecialService, "JobLineOutCheckReportReply", new object[] { "L2", eBitResult.ON, eReturnCode1.OK, trxID });
                    }
                }
            }
            catch (Exception ex_)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex_);
            }
        }

        /// <summary>提供給Recipe Service呼叫
        /// 
        /// </summary>
        public void RecipeIDRegisterCheckRequest(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
        {
            try
            {
                GetServerAgent();
                string body_xml = SocketServiceUtility.Recipe.ToXml(trxid, lineName, recipeCheckInfos, noCheckList);
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("RecipeIDRegisterCheckRequest") as XmlDocument;
                xml_doc.SelectSingleNode("//MESSAGE/BODY").InnerXml = body_xml;

                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>收到RecipeIDRegisterCheckReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_RecipeIDRegisterCheckReply(XmlDocument xmlDoc)
        {
            try
            {
                //收到XML後Invoke RecipeService RecipeRegisterValidationCommandReplyForBCS
                XmlNode reply_body_node = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                string trxid = string.Empty, line_name = string.Empty;
                List<RecipeCheckInfo> recipeCheckInfos = null;
                List<string> noCheckList = null;
                SocketServiceUtility.Recipe.FromXml(reply_body_node, out trxid, out line_name, out recipeCheckInfos, out noCheckList);
                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandReplyForBCS", new object[] { trxid, line_name, recipeCheckInfos });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>提供給Recipe Service呼叫
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void RecipeParameterRequest(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
        {
            try
            {
                GetServerAgent();
                string body_xml = SocketServiceUtility.Recipe.ToXml(trxid, lineName, recipeCheckInfos, noCheckList);
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("RecipeParameterRequest") as XmlDocument;
                xml_doc.SelectSingleNode("//MESSAGE/BODY").InnerXml = body_xml;

                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>收到RecipeParameterReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void ACTIVE_RecipeParameterReply(XmlDocument xmlDoc)
        {
            try
            {
                //收到XML後Invoke RecipeService RecipeParameterRequestCommandReplyForBCS
                XmlNode reply_body_node = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                string trxid = string.Empty, line_name = string.Empty;
                List<RecipeCheckInfo> recipeCheckInfos = null;
                List<string> noCheckList = null;
                SocketServiceUtility.Recipe.FromXml(reply_body_node, out trxid, out line_name, out recipeCheckInfos, out noCheckList);
                Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandReplyForBCS", new object[] { trxid, line_name, recipeCheckInfos });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ACTIVE_JOBShortCutGlassIn(XmlDocument xmlDoc)
        {
            try
            {
                GetServerAgent();
                //發送REPLY
                XmlDocument reply = _activeSocketAgent.GetTransactionFormat("JOBShortCutGlassInReply") as XmlDocument;
                reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                reply["MESSAGE"]["BODY"]["SourceLineName"].InnerText = xmlDoc["MESSAGE"]["BODY"]["SourceLineName"].InnerText;
                reply["MESSAGE"]["BODY"]["TargetLineName"].InnerText = xmlDoc["MESSAGE"]["BODY"]["TargetLineName"].InnerText;
                string eqp_id = xmlDoc["MESSAGE"]["BODY"]["EqpID"].InnerText;
                string cst_seqno = reply["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText = xmlDoc["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText;
                string job_seqno = reply["MESSAGE"]["BODY"]["JOBNo"].InnerText = xmlDoc["MESSAGE"]["BODY"]["JOBNo"].InnerText;
                string job_id = reply["MESSAGE"]["BODY"]["JOBID"].InnerText = xmlDoc["MESSAGE"]["BODY"]["JOBID"].InnerText;
                xMessage xmsg = new xMessage();
                xmsg.Name = reply["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText;
                xmsg.TransactionID = reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.OuterXml;
                QueueManager.PutMessage(xmsg);

                //TODO Invoke Method
                Invoke(eServiceName.CFSpecialService, "JobWIPDelete", new object[] { eqp_id, cst_seqno, job_seqno, job_id });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendBCSLinkTest(string trxID, string lineName, List<string> session_id)
        {
            try
            {
                GetServerAgent();
                XmlDocument xml_doc = _activeSocketAgent.GetTransactionFormat("BCSLinkTest") as XmlDocument;
                BCSLinkTest trx = Spec.XMLtoMessage(xml_doc) as BCSLinkTest;
                trx.BODY.LINENAME = lineName;

                string xml = trx.WriteToXml();
                xMessage msg = new xMessage();
                msg.Name = trx.HEADER.MESSAGENAME;
                msg.FromAgent = AGENT_NAME;
                msg.ToAgent = AGENT_NAME;
                msg.Data = xml;
                msg.TransactionID = trxID;

                foreach (string _ip in session_id)
                {
                    msg.UseField.Add(_ip, null);
                }
                PutMessage(msg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //定時連線測試
        private void LinkTestTimerFunc(object obj)
        {
            if (_run)
            {
                _linkTestMRE.Reset();
                GetServerAgent();
                if (_activeSocketAgent != null)
                {
                    MethodInfo mi = _activeSocketAgent.GetType().GetMethod("GetRemotes");
                    object dic = mi.Invoke(_activeSocketAgent, null);
                    if (dic is Dictionary<string, object[]>)
                    {
                        Dictionary<string, object[]> client_infos = dic as Dictionary<string, object[]>;
                        foreach (string session_id in client_infos.Keys)
                        {
                            DateTime last_recv_send = (DateTime)(client_infos[session_id][0]);
                            TimeSpan ts = DateTime.Now - last_recv_send;
                            if (ts.TotalSeconds > 60)
                            {
                                //OPI和BC之間若60秒沒有收送動作
                                //BC就向OPI發出LinkTest訊息
                                string trxid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                List<string> _session_id = new List<string>();
                                _session_id.Add(session_id);
                                SendBCSLinkTest(trxid, Workbench.ServerName, _session_id);
                            }
                        }
                    }
                }
                if (_run)
                    _linkTestTimer.Change(1000, Timeout.Infinite);
                _linkTestMRE.Set();
            }
        }
    }
}
