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
    public class PassiveSocketService : AbstractService
    {
        private bool _run = false;
        private Timer _linkTestTimer = null;
        private ManualResetEvent _linkTestMRE = new ManualResetEvent(true);
        private readonly string AGENT_NAME = "PassiveSocketAgent";
        private string _localSocketSessionId = string.Empty;
        private string _remoteSocketSessionId = string.Empty;
        private IServerAgent _passiveSocketAgent = null;
        private string _activeSocketLineName = string.Empty;
        public FileFormatManager FileFormatManager { get; set; }
        
        private IServerAgent GetServerAgent()
        {
            //小心Spring的_applicationContext.GetObject(name)
            //在Init-Method與Destory-Method會與Spring的GetObject使用相同LOCK
            //需留心以免死結
            if (_passiveSocketAgent != null)
                return _passiveSocketAgent;
            if (Workbench.Instance.AgentList.ContainsKey(AGENT_NAME))
            {
                _passiveSocketAgent = GetServerAgent(AGENT_NAME);
                return _passiveSocketAgent;
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

        /// <summary>PassiveSocketAgent回報連線
        /// 
        /// </summary>
        /// <param name="localSessionId"></param>
        /// <param name="remoteSessionId"></param>
        public void PASSIVE_CONNECT(string localSessionId, string remoteSessionId)
        {
            _localSocketSessionId = localSessionId;
            _remoteSocketSessionId = remoteSessionId;
        }

        /// <summary>PassiveSocketAgent回報斷線
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        public void PASSIVE_DISCONNECT(string sessionId)
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
        public void PASSIVE_MessageSend(DateTime sendDt, string sessionId, string xml, int t3TimeoutSecond)
        {
            //try
            //{
            //    Message msg = Spec.XMLtoMessage(xml);
            //    if (msg.WaitReply != string.Empty && t3TimeoutSecond > 0)
            //    {
            //        string key = string.Format("{0}_{1}", sessionId, msg.HEADER.TRANSACTIONID);
            //        //NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Add to WaitForT3", msg.HEADER.MESSAGENAME, key));
            //        //T3Manager.AddT3(new T3Manager.T3Message(GetType().Name, sendDt, key, msg, t3TimeoutSecond));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //}
        }

        /// <summary>當Agent收到Message後, Agent會先呼叫MessageRecv以通知Service, 由Service決定是否移除T3
        /// Agent呼叫MessageRecv之後才會根據不同Message, Invoke不同Service Method
        /// </summary>
        /// <param name="recvDt"></param>
        /// <param name="sessionId"></param>
        /// <param name="xml"></param>
        public void PASSIVE_MessageRecv(DateTime recvDt, string sessionId, string xml)
        {
            try
            {
                //Message reply_msg = Spec.XMLtoMessage(xml);
                //string command_msg_name = Spec.GetMessageByReply(reply_msg.HEADER.MESSAGENAME);
                //if (command_msg_name != string.Empty)
                //{
                //    string key = string.Format("{0}_{1}", sessionId, reply_msg.HEADER.TRANSACTIONID);
                //    //T3Manager.T3Message command_t3 = T3Manager.TryRemoveT3(GetType().Name, key);
                //    //if (command_t3 != null)
                //    //{
                //    //    Message command_msg = command_t3.Obj as Message;
                //    //    NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Remove from WaitForT3", command_msg.HEADER.MESSAGENAME, key));
                //    //}
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>PassiveSocketAgent收到AreYouThereRequest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_AreYouThereRequest(XmlDocument xmlDoc)
        {
            try
            {
                AreYouThereRequest msg = Spec.XMLtoMessage(xmlDoc) as AreYouThereRequest;
                AreYouThereReply reply = new AreYouThereReply();
                reply.HEADER.TRANSACTIONID = msg.HEADER.TRANSACTIONID;
                reply.BODY.FACTORYTYPE = msg.BODY.FACTORYTYPE;
                _activeSocketLineName = reply.BODY.LINENAME = msg.BODY.LINENAME;

                xMessage xmsg = new xMessage();
                xmsg.Name = reply.HEADER.MESSAGENAME;
                xmsg.TransactionID = reply.HEADER.TRANSACTIONID;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.WriteToXml();
                QueueManager.PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>PassiveSocketAgent收到BCLinkTest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_BCSLinkTest(XmlDocument xmlDoc)
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
                QueueManager.PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>PassiveSocketAgent收到BCLinkTestReply
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_BCSLinkTestReply(XmlDocument xmlDoc)
        {
            try
            {
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>PassiveSocketAgent收到JOBShortCutPermit
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_JOBShortCutPermit(XmlDocument xmlDoc)
        {
            try
            {
                GetServerAgent();
                //JOBShortCutPermit的BODY與 MES SPEC的CFShortCutPermitReply的BODY一樣
                XmlDocument reply = _passiveSocketAgent.GetTransactionFormat("JOBShortCutPermitReply") as XmlDocument;
                reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                reply["MESSAGE"]["BODY"]["LINENAME"].InnerText = xmlDoc["MESSAGE"]["XML"]["MESSAGE"]["BODY"]["LINENAME"].InnerText;
                //發送REPLY
                xMessage xmsg = new xMessage();
                xmsg.Name = reply["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText;
                xmsg.TransactionID = reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply.OuterXml;
                QueueManager.PutMessage(xmsg);

                // job xml反序列化
                string job_xml = xmlDoc.SelectSingleNode("//MESSAGE/JOB").InnerXml;
                XmlDocument job_xml_doc = new XmlDocument();
                job_xml_doc.LoadXml(job_xml);
                System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(Job));
                System.IO.StringReader sr = new System.IO.StringReader(job_xml_doc.InnerXml);
                Job job = (Job)xmlserializer.Deserialize(sr);

                // 取出MES CFShortCutPermitReply
                string mes_xml = xmlDoc.SelectSingleNode("//MESSAGE/XML").InnerXml;
                XmlDocument mes_xml_doc = new XmlDocument();
                mes_xml_doc.LoadXml(mes_xml);

                //Invoke CFSpecialService
                string dispatch_name = "JobDataCreater";
                Invoke(eServiceName.CFSpecialService, dispatch_name, new object[] { job, mes_xml_doc });
                NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name, string.Format("Dispatch[{0}]", dispatch_name));
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
        public void CellShortCut(Job job)
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
                XmlDocument xml_doc = _passiveSocketAgent.GetTransactionFormat("CellShortCut") as XmlDocument;
                XmlNode body = xml_doc.SelectSingleNode("//MESSAGE/BODY");
                body.InnerXml = job_xml_doc["Job"].OuterXml;
                //將XML發送到Passive端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                string msg = string.Format("CELL Short Cut Send Message !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2})", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
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
                XmlDocument xml_doc = _passiveSocketAgent.GetTransactionFormat("ArrayShortCut") as XmlDocument;
                XmlNode body = xml_doc.SelectSingleNode("//MESSAGE/BODY");
                body.InnerXml = job_xml_doc["Job"].OuterXml;
                //將XML發送到Passive端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                string msg = string.Format("ARRAY Short Cut Send Message !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2})", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
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
        public void PASSIVE_CellShortCut(XmlDocument xmlDoc)
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

                XmlDocument reply = _passiveSocketAgent.GetTransactionFormat("CellShortCutReply") as XmlDocument;
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
            catch(Exception ex_)
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
        public void PASSIVE_CellShortCutReply(XmlDocument xmlDoc)
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


        /// <summary>收到Array ShortCut (ELA)
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void PASSIVE_ArrayShortCut(XmlDocument xmlDoc)
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

                XmlDocument reply = _passiveSocketAgent.GetTransactionFormat("ArrayShortCutReply") as XmlDocument;
                reply["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                reply["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText = job.CassetteSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBNo"].InnerText = job.JobSequenceNo;
                reply["MESSAGE"]["BODY"]["JOBID"].InnerText = job.GlassChipMaskBlockID;

                try
                {
                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                    string msg = string.Format("ARRAY Short Cut Receive Message OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2})", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
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


        /// <summary>收到ArrayShortCutReply (ELA)
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void PASSIVE_ArrayShortCutReply(XmlDocument xmlDoc)
        {
            try
            {
                string cst_seq = xmlDoc["MESSAGE"]["BODY"]["CSTSEQNo"].InnerText;
                string job_seq = xmlDoc["MESSAGE"]["BODY"]["JOBNo"].InnerText;
                string glass_chip_mask_cut_id = xmlDoc["MESSAGE"]["BODY"]["JOBID"].InnerText;
                string return_code = xmlDoc["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText;
                string trxID=xmlDoc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;

                Job job = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                if (job == null) throw new Exception(string.Format(string.Format("ARRAY Short Cut Reply Message Error !! Can't Find Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", cst_seq, job_seq, glass_chip_mask_cut_id)));

                if (!string.IsNullOrEmpty(return_code))
                {
                    if (return_code == "0")
                    {
                        string msg = string.Format("ARRAY Short Cut Receive ReplyMessage OK !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                        //ObjectManager.JobManager.DeleteJob(job);

                        Invoke(eServiceName.ArraySpecialService, "JobLineOutCheckReportReply", new object[]{"L2",eBitResult.ON,eReturnCode1.OK,trxID });
                    }
                    else
                    {
                        string msg = string.Format("ARRAY Short Cut Receive ReplyMessage NG !! Cassette Seq =[{0}], Job Seq =[{1}], GlassID =[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                        Invoke(eServiceName.ArraySpecialService, "JobLineOutCheckReportReply", new object[] { "L2", eBitResult.ON, eReturnCode1.NG, trxID });
                    }
                }
            }
            catch (Exception ex_)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex_);
            }
        }

        /// <summary>收到RecipeIDRegisterCheckRequest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_RecipeIDRegisterCheckRequest(XmlDocument xmlDoc)
        {
            try
            {
                GetServerAgent();
                //收到XML後Invoke RecipeService RecipeRegisterValidationCommandFromBCS
                XmlNode cmd_body_node = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                string trxid = string.Empty, line_name = string.Empty;
                List<RecipeCheckInfo> recipeCheckInfos = null;
                List<string> noCheckList = null;
                SocketServiceUtility.Recipe.FromXml(cmd_body_node, out trxid, out line_name, out recipeCheckInfos, out noCheckList);
                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromBCS", new object[] { trxid, recipeCheckInfos, noCheckList });

                //Invoke的結果reply給ACTIVE
                XmlDocument reply_xml_doc = _passiveSocketAgent.GetTransactionFormat("RecipeIDRegisterCheckReply") as XmlDocument;
                reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                string reply_body_xml = SocketServiceUtility.Recipe.ToXml(trxid, line_name, recipeCheckInfos, noCheckList);
                XmlNode reply_body_node = reply_xml_doc.SelectSingleNode("//MESSAGE/BODY");
                reply_body_node.InnerXml = reply_body_xml;
                
                xMessage xmsg = new xMessage();
                xmsg.Name = reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply_xml_doc.OuterXml;
                PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>收到RecipeParameterRequest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PASSIVE_RecipeParameterRequest(XmlDocument xmlDoc)
        {
            try
            {
                GetServerAgent();
                //收到XML後Invoke RecipeService RecipeRegisterValidationCommandFromBCS
                XmlNode cmd_body_node = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                string trxid = string.Empty, line_name = string.Empty;
                List<RecipeCheckInfo> recipeCheckInfos = null;
                List<string> noCheckList = null;
                SocketServiceUtility.Recipe.FromXml(cmd_body_node, out trxid, out line_name, out recipeCheckInfos, out noCheckList);
                Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandFromBCS", new object[] { trxid, recipeCheckInfos, noCheckList });

                //Invoke的結果reply給ACTIVE
                XmlDocument reply_xml_doc = _passiveSocketAgent.GetTransactionFormat("RecipeParameterReply") as XmlDocument;
                reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                string reply_body_xml = SocketServiceUtility.Recipe.ToXml(trxid, line_name, recipeCheckInfos, noCheckList);
                XmlNode reply_body_node = reply_xml_doc.SelectSingleNode("//MESSAGE/BODY");
                reply_body_node.InnerXml = reply_body_xml;
                
                xMessage xmsg = new xMessage();
                xmsg.Name = reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = reply_xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = reply_xml_doc.OuterXml;
                PutMessage(xmsg);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eqpID">Passive side Glass In by which equipment ID</param>
        /// <param name="cstSeqNo">Passive side glass in Job’s Cassette Sequence Number</param>
        /// <param name="jobSeqNo">Passive side glass in Job’s Number</param>
        /// <param name="jobID">Passive side glass in Job’s Job ID</param>
        public void JOBShortCutGlassIn(string eqpID, string cstSeqNo, string jobSeqNo, string jobID)
        {
            try
            {
                GetServerAgent();
                XmlDocument xml_doc = _passiveSocketAgent.GetTransactionFormat("JOBShortCutGlassIn") as XmlDocument;
                XmlNode body = xml_doc.SelectSingleNode("//MESSAGE/BODY");
                body["SourceLineName"].InnerText = Workbench.ServerName;
                body["TargetLineName"].InnerText = _activeSocketLineName;
                body["EqpID"].InnerText = eqpID;
                body["CSTSEQNo"].InnerText = cstSeqNo;
                body["JOBNo"].InnerText = jobSeqNo;
                body["JOBID"].InnerText = jobID;
                //將XML發送到Active端
                xMessage xmsg = new xMessage();
                xmsg.Name = xml_doc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
                xmsg.TransactionID = xml_doc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                xmsg.ToAgent = AGENT_NAME;
                xmsg.Data = xml_doc.OuterXml;
                PutMessage(xmsg);
                string msg = string.Format("CF JOBShortCutGlassIn Send message OK!! EQ =[{0}], Cassette Seq =[{1}], Job Seq =[{2}], GlassID =[{3})", eqpID, cstSeqNo, jobSeqNo, jobID);
                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
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
                BCSLinkTest trx = new BCSLinkTest();
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
                if (_passiveSocketAgent != null)
                {
                    MethodInfo mi = _passiveSocketAgent.GetType().GetMethod("GetRemotes");
                    object dic = mi.Invoke(_passiveSocketAgent, null);
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
