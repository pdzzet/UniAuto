#region [Using]
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unicom.UniAuto.Net.Socket;
using System.Reflection;
#endregion	 

namespace UniOPI
{
    /// <summary>
    /// NOTE:must dispose driver if no use
    /// </summary>
    public abstract class AbstractMessageDriver : IDisposable
    {
        #region [private]
        private IAsyncSocket _Socket; //asyncsocket object
        private bool _TraceEnable = false; //enable socket detail trace option
        private Encoding _DataEncoding = Encoding.UTF8; //data encoding,default=UTF8
        private Task _Task; //loop Task
        private CancellationTokenSource _LocalSource; //task cancel token source
        private CancellationToken _Token; //task cancel token
        private int _TaskWaitFrequency = 100; //every 10 times
        private int _TaskWaitPeriodMS = 5; //sleep 5ms
        private ConcurrentQueue<Tuple<string, string, XmlDocument>> _DataMessageQueue = new ConcurrentQueue<Tuple<string, string, XmlDocument>>();
        private ConcurrentDictionary<string, AsyncResult<MessageResponse>> _MsgRequestDict = new ConcurrentDictionary<string, AsyncResult<MessageResponse>>();
        #endregion

        #region [constructor]
        public AbstractMessageDriver()
        {
            //init task
            this._LocalSource = new CancellationTokenSource();
            this._Token = this._LocalSource.Token;
            this._Task = new Task(TaskProc, this._Token, TaskCreationOptions.LongRunning);
            this._Task.Start();
        }
        #endregion

        #region [public property]
        /// <summary>
        /// get/set enable socket trace log option
        /// </summary>
        public bool TraceEnable
        {
            get
            {
                bool enable = this._TraceEnable;
                try
                {
                    if (this._Socket != null)
                    {
                        enable = this._Socket.TraceEnable;
                    }
                }
                catch { }
                return enable;
            }
            set
            {
                try
                {
                    if (this._Socket != null)
                    {
                        this._Socket.TraceEnable = value;
                    }
                    this._TraceEnable = value;
                }
                catch { }
            }
        }
        /// <summary>
        /// get/set socket data encoding
        /// </summary>
        public Encoding DataEncoding
        {
            get
            {
                Encoding encoding = this._DataEncoding;
                try
                {
                    if (this._Socket != null)
                    {
                        encoding = this._Socket.DataEncoding;
                    }
                }
                catch { }
                return encoding;
            }
            set
            {
                try
                {
                    if (this._Socket != null)
                    {
                        this._Socket.DataEncoding = value;
                    }
                    this._DataEncoding = value;
                }
                catch { }
            }
        }
        /// <summary>
        /// get/set is driver disposed flag
        /// </summary>
        public bool IsDisposed { get; internal set; }
        /// <summary>
        /// get/set is driver started flag
        /// </summary>
        public bool IsStarted { get; set; }
        /// <summary>
        /// get/set is driver client flag
        /// </summary>
        public bool IsClient { get; set; }
        /// <summary>
        /// get session info
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>session info list</returns>
        List<SessionInfo> GetSessionInfo(string sessionId = "")
        {
            return this._Socket.GetSessionInfo(sessionId);
        }
        #endregion

        #region [IDisposable Impletement]
        /// <summary>
        /// dispose plcdriver
        /// </summary>
        public void Dispose()
        {
            //check if already disposed
            if (this.IsDisposed)
            {
                return;
            }
            this.IsDisposed = true; //set disposed flag to true

            //cancl all tasks
            this._LocalSource.Cancel();

            //dispose socket
            this._Socket.Dispose();

            //clear Queue
            if (_DataMessageQueue != null)
            {
                Tuple<string, string, XmlDocument> _msg = null;
                while (_DataMessageQueue.TryDequeue(out _msg))
                {
                    // do nothing
                }
            }
        }
        #endregion

        #region [driver operation]
        /// <summary>
        /// start driver,client auto reconnect ,server auto relisten
        /// </summary>
        /// <returns>true=OK,false=NG</returns>
        public bool Start(bool isClient, string ip, string port)
        {
            try
            {
                Encoding tmp = this.DataEncoding;//DataEncoding的get/set有問題, 必須在new出_Socket之前把值取出
                //dispose prev socket
                if (this._Socket != null)
                {
                    this._Socket.Dispose();
                }

                //create socket
                if (isClient)
                {
                    this._Socket = AsyncSocketFactory.CreateClient(32 * 1024); //32K
                    ((AsyncSocketClient)this._Socket).TaskWaitFrequency = 100;// by chia cheng, 2014-12-24
                    ((AsyncSocketClient)this._Socket).TaskWaitPeriodMS = 5;// by chia cheng, 2014-12-24
                }
                else
                {
                    this._Socket = AsyncSocketFactory.CreateServer(32 * 1024, 512); //<==may throw exception
                }
                this.IsClient = IsClient;

                //register socket event handler				
                this._Socket.LogEvent += LogEventHandler;
                this._Socket.ConnectedEvent += ConnectedEventHandler;
                this._Socket.CloseEvent += CloseEventHandler;

                //NOTE:set socket dataencoding
                this._Socket.DataEncoding = tmp;

                //init socket
                bool done = this._Socket.Init(ip, port, true);
                if (!done)
                {
                    return false;
                }

                //start socket
                done = this._Socket.Start();
                if (done)
                {
                    this.IsStarted = true;
                }
                return done;
            }
            catch (Exception ex)
            {
                OnDriverError(SocketLogLevelType.EXCEPTION, "Start", string.Empty, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// stop drvier
        /// </summary>
        public void Stop()
        {
            try
            {
                this.IsStarted = false;
                if (this._Socket != null)
                {
                    this._Socket.Stop();
                }
            }
            catch { }
        }

        /// <summary>
        /// get is session connected(session id is ignore by client)
        /// </summary>		
        /// <returns>true=connected,false=disconnected</returns>
        public bool IsConnected(string sessionId = "")
        {
            try
            {
                bool conn = false;
                if (this._Socket != null)
                {
                    conn = this._Socket.IsConnected(sessionId);
                }
                return conn;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// poll message from queue,null if no message
        /// </summary>	
        /// <returns>tuple object({trxid,msgname,xmldocument}),null if no message</returns>
        public Tuple<string, string, XmlDocument> PollMessage()
        {
            Tuple<string, string, XmlDocument> tuple = null;
            try
            {
                this._DataMessageQueue.TryDequeue(out tuple);
            }
            catch (Exception ex)
            {
                OnDriverError(SocketLogLevelType.EXCEPTION, "PollMessage", string.Empty, ex.Message);
            }
            return tuple;
        }

        public void InQueueMessage(string trxId, string msgName, XmlDocument document)
        {
            this._DataMessageQueue.Enqueue(Tuple.Create(trxId, msgName, document));
        }

        /// <summary>
        /// send message
        /// </summary>
        /// <param name="trxid">trx id</param>		
        /// <param name="xml">xml text</param>
        /// <param name="err">error</param>
        /// <param name="sessionId">session id</param>
        /// <returns></returns>
        public bool SendMessage(string trxid,string msgName, string xml, out string err, string sessionId = "")
        {
            err = string.Empty;

            try
            {
                if (FormMainMDI.G_OPIAp.SocketReplyFlag)
                {
                    return false;
                }
                //check xml is not null or empty
                if (string.IsNullOrEmpty(xml))
                {
                    err = "xml is null or empty";
                    return false;
                }

                //check driver started
                if (!this.IsStarted)
                {
                    err = "driver not started yet";
                    return false;
                }

                //(1)send string data
                //bool done = this.m_Socket.Send(xml, trxid, sessionId);

                //(2)send byte arrary data
                //convert string to byte arrary
                byte[] databytes = this.DataEncoding.GetBytes(xml);
                bool done = this._Socket.Send(databytes, databytes.Length, trxid, sessionId);

                if (msgName == "EquipmentStatusRequest")
                    FormTraceSocket.StatusRequest++;
                else if (msgName == "EquipmentStatusReportReply")
                    FormTraceSocket.StatusReportReply++;

                //(3)send datamessage								
                //DataMessage message = new DataMessage();
                //message.TimeStamp = DateTime.Now;
                //message.ReqSNo = trxid;
                //message.SessionId = sessionId;
                //message.Message = this.DataEncoding.GetBytes(xml);\
                //message.Length = message.Message.Length;
                //bool done = this.m_Socket.Send(message);

                if (!done)
                {
                    err = "socket send fail";
                }

                //log send
                OnSendMessage(DateTime.Now,msgName, trxid, xml, sessionId);
                return done;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// begin driver asyncronous request-response operation		
        /// </summary>
        /// <param name="request">msg request object</param>
        /// <param name="callback">msg response callback</param>	
        /// <returns>IAsyncResult</returns>
        internal IAsyncResult BeginRequestResponse(MessageRequest request, AsyncCallback callback)
        {
            MessageResponse response = new MessageResponse();
            AsyncResult<MessageResponse> ar = new AsyncResult<MessageResponse>(callback, request.RelationKey);
            try
            {
                DateTime now = DateTime.Now;
                //check if driver started
                if (!this.IsStarted)
                {
                    response.Set(MessageResponse.NG, "driver not started yet");
                    response.CompDT = now;
                    ar.SetAsCompleted(response, false);
                    return ar;
                }

                //check if driver connected
                //if (!this.IsConnected(Program.sessionId)) 
                if (!this.IsConnected(FormMainMDI.G_OPIAp.SessionID))
                {
                    response.Set(MessageResponse.NG, "driver not connected");
                    response.CompDT = now;
                    ar.SetAsCompleted(response, false);
                    return ar;
                }

                //check if request xml is null or empty
                if (string.IsNullOrEmpty(request.Xml))
                {
                    response.Set(MessageResponse.NG, "reqeust xml is null or empty");
                    response.CompDT = now;
                    ar.SetAsCompleted(response, false);
                    return ar;
                }

                //check if relationkey is null or empty
                if (string.IsNullOrEmpty(request.RelationKey))
                {
                    response.Set(MessageResponse.NG, "reqeust relationKey is null or empty");
                    response.CompDT = now;
                    ar.SetAsCompleted(response, false);
                    return ar;
                }

                //do real send
                bool done = this._Socket.Send(request.Xml, request.TrxId, request.SessionId);
                if (!done)
                {
                    response.Set(MessageResponse.NG, "socket send fail");
                    response.CompDT = now;
                    ar.SetAsCompleted(response, false);
                    return ar;
                }

                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(request.Xml);
                    string msgName = this.GetMsgName(document);
                    if (msgName == "EquipmentStatusRequest")
                        FormTraceSocket.StatusRequest++;
                    else if (msgName == "EquipmentStatusReportReply")
                        FormTraceSocket.StatusReportReply++;
                }

                //add to dict & wait response				
                this._MsgRequestDict.TryAdd(request.RelationKey, ar);

                //log send
                OnSendMessage(DateTime.Now, request.TrxMsgName, request.TrxId, request.Xml, request.SessionId);
            }
            catch (Exception ex)
            {
                response.Set(MessageResponse.EXCEPTION, ex.ToString());
                ar.SetAsCompleted(response, false);
            }
            return ar;
        }

        /// <summary>
        /// end driver asyncronous request-respone operation,wait until comp or timeout
        /// </summary>
        /// <param name="timeoutMS">request timout time(ms),-1=wait forever</param>
        /// <param name="asyncResult">asyncResult</param>
        /// <returns>msg response object</returns>
        internal MessageResponse EndRequestResponse(IAsyncResult asyncResult, int timeoutMS)
        {
            MessageResponse response = new MessageResponse();
            AsyncResult<MessageResponse> ar = (AsyncResult<MessageResponse>)asyncResult;
            try
            {
                response = ar.EndInvoke(timeoutMS);
                if (response == null)
                {
                    response = new MessageResponse();
                }
                string relationKey = ar.AsyncState as string;
                if (ar.IsTimeOut)
                {
                    response.Set(MessageResponse.NG, "request-response operation timeout");
                    response.IsTimeOut = true;
                    //remove from dict
                    AsyncResult<MessageResponse> tmp;
                    this._MsgRequestDict.TryRemove(relationKey, out tmp);
                }
                response.CompDT = DateTime.Now;
            }
            catch (Exception ex)
            {
                response.Set(MessageResponse.EXCEPTION, ex.ToString());
            }
            return response;
        }

        /// <summary>
        /// driver syncronous reqeust-response operation	
        /// NOTE:thread will be blocked until response or timeout
        /// </summary>
        /// <param name="request">msg request object</param>
        /// <param name="timeoutMS">request timout time(ms),-1=wait forever</param>
        /// <returns>msg response object</returns>
        public MessageResponse RequestRsponse(MessageRequest request, int timeoutMS)
        {
            return EndRequestResponse(BeginRequestResponse(request, null), timeoutMS);
        }
        #endregion

        #region [loop task]
        private void TaskProc()
        {
            int cc = 0;
            FormTraceSocket.PollThreadID = Thread.CurrentThread.ManagedThreadId;
            while (true)
            {
                try
                {
					//wait N ms every N times
					cc++;
					if (cc > this._TaskWaitFrequency) {
						cc = 0;
						Thread.Sleep(this._TaskWaitPeriodMS);
					}

					//20141013
					//check if request cancel task(在Dispose()內),exit task
					if (this._Token.IsCancellationRequested) {
						return; //exit task
					}

                    //check if socket is null
                    if (this._Socket == null)
                    {
                        continue;
                    }

                    //check if driver started
                    if (!this.IsStarted)
                    {
                        continue;
                    }

                    FormTraceSocket.SocketInQueue = ((AsyncSocketClient)this._Socket).GetInMessageQueueDepth();
                    FormTraceSocket.SocketOutQueue = ((AsyncSocketClient)this._Socket).GetOutMessageQueueDepth();

                    //poll message from socket
                    DataMessage dataMsg = this._Socket.Poll();
                    if (dataMsg == null)
                    {
                        continue;
                    }
                    if (dataMsg.Message == null)
                    {
                        continue;
                    }

                    //parse xml
                    string xml = string.Empty;
					string _msg = string.Empty;
                    XmlDocument document = new XmlDocument();
                    try
                    {
                        xml = this.DataEncoding.GetString(dataMsg.Message);
                        document.LoadXml(xml);						
                    }
                    catch (Exception ex)
                    {
						OnDriverError(SocketLogLevelType.EXCEPTION, "TaskProc.LoadXml", string.Empty, ex.Message);
						//20150204 log error xml
						_msg = string.Format("Receive error xml [{0}] ", xml);
						NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, _msg);                        
                        continue;
                    }

                    // log recv
                    string msgName = this.GetMsgName(document);
                    string trxId = this.GetMsgTrxId(document);
                    string relationKey = this.GetMsgRelationKey(document);

					#region log recv xml
                    _msg = string.Format("Receive TrxId [{0}] ,xml [{1}] ", trxId, xml);
                    
                    NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, msgName);
                    NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, _msg);
                    #endregion

                    //沒地方用到pollMessage 所以不要將訊息加入Queue!!
                    //check relation key is null
                    if (string.IsNullOrEmpty(relationKey))
                    {
                        this._DataMessageQueue.Enqueue(Tuple.Create(trxId, msgName, document)); //just add to poll
                        OnRecvMessage(DateTime.Now, msgName, trxId, xml, dataMsg.SessionId);
                        continue;

                        //if (msgName != "AreYouThereReply") continue;
                    }

                    //get & remove ar from dict
                    AsyncResult<MessageResponse> ar;
                    this._MsgRequestDict.TryRemove(relationKey, out ar);
                    //沒地方用到pollMessage 所以不要將訊息加入Queue!!
                    if (ar == null)
                    {
                        this._DataMessageQueue.Enqueue(Tuple.Create(trxId, msgName, document)); //just add to poll
                        OnRecvMessage(DateTime.Now, msgName, trxId, xml, dataMsg.SessionId);
                        continue;

                        //if (msgName != "AreYouThereReply") continue;
                    }                    
					if (ar != null) 
					{
						//check if timeout
						if (!ar.IsTimeOut) {
							//create response
							MessageResponse response = new MessageResponse();
							response.TrxId = trxId;
							response.Xml = xml;
							response.RelationKey = relationKey;
							response.SessionId = dataMsg.SessionId;
							ar.SetAsCompleted(response, false);
						}
					}
                    //if (msgName == "EquipmentStatusReport")
                    //    FormTraceSocket.StatusReport++;
                    //else if (msgName == "EquipmentStatusReply")
                    //    FormTraceSocket.StatusReply++;
                    
                    OnRecvMessage(DateTime.Now, msgName, trxId, xml, dataMsg.SessionId);

                    //this._DataMessageQueue.Enqueue(Tuple.Create(trxId, msgName, document)); //just add to poll

                } catch (Exception ex ) {
					//20150203
					OnDriverError(SocketLogLevelType.EXCEPTION, "TaskProc", string.Empty, ex.ToString());
				}
            }
        }
        #endregion

        #region [socket event handler]
        private void LogEventHandler(object sender, SocketLogEventArgs e)
        {
            OnLogEvent(e.LogLevel, e.MethodName, e.Message);
        }

        private void ConnectedEventHandler(object sender, SocketConnectedEventArgs e)
        {
            OnConnectEvent(e.SessionId);
        }

        private void CloseEventHandler(object sender, SocketCloseEventArgs e)
        {
            OnCloseEvent(e.SessionId);
        }
        #endregion

        #region [virtual method]
        protected virtual void OnLogEvent(SocketLogLevelType LogLevle, string MethodName, string Message) { }
        protected virtual void OnConnectEvent(string session) { }
        protected virtual void OnCloseEvent(string session) { }
        protected virtual void OnDriverError(SocketLogLevelType LogLevle, string MethodName, string code, string Message) { }
        protected virtual void OnSendMessage(DateTime timeStamp, string msgName,string trxId, string xml, string sessionId) { }
        protected virtual void OnRecvMessage(DateTime timeStamp,string msgName, string trxId, string xml, string sessionId) { }
        protected virtual string GetMsgRelationKey(XmlDocument response) { return string.Empty; }
        protected virtual string GetMsgTrxId(XmlDocument response) { return string.Empty; }
        protected virtual string GetMsgName(XmlDocument response) { return string.Empty; }
        #endregion
    }
}
