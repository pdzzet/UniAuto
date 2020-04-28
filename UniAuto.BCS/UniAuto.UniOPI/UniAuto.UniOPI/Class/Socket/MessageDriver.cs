#region [Using]
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using UniAuto.UniBCS.OpiSpec;
using Unicom.UniAuto.Net.Socket;

#endregion

namespace UniOPI
{    
    
    public class MessageDriver : AbstractMessageDriver
    {

        //public Dictionary<string, FormMainMDI.OnRecvMessage_delegate> DicRecvMessageEvent = new Dictionary<string, FormMainMDI.OnRecvMessage_delegate>();

        public delegate void SocketErrorMessage_delegate(SocketLogLevelType LogLevle, string MethodName, string Message);
        public event SocketErrorMessage_delegate SocketErrorMessage;

        public delegate void ReceiveErrorMessage_delegate( string MethodName,string Caption,string ErrCode, string ErrMsg);
        //public event ReceiveErrorMessage_delegate ReceiveErrorMessage;

        public delegate void SocketCloseMessage_delegate();
        public event SocketCloseMessage_delegate SocketClose;

        public BackgroundWorker bgwSocket;

        private FormBase frmMsgBase = new FormBase();

        private void bgwSocket_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                AreYouThereRequest _trx = new AreYouThereRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.FACTORYTYPE = FormMainMDI.G_OPIAp.CurLine.FabType;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.USERID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.USERGROUP = FormMainMDI.G_OPIAp.LoginGroupID;
                _trx.BODY.LOGINTIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _trx.BODY.LOGINSERVERIP = FormMainMDI.G_OPIAp.SessionID; //string.Format("{0}:{1}", FormMainMDI.G_OPIAp.LocalIPAddress, FormMainMDI.G_OPIAp.SocketPort);
                _trx.BODY.LOGINSERVERNAME = FormMainMDI.G_OPIAp.LocalHostName;
                string _xml = _trx.WriteToXml();


                MessageRequest req = new MessageRequest();
                req.TimeStamp = DateTime.Now;
                req.Xml = _xml;
                req.TrxMsgName = "AreYouThereRequest";
                req.TrxId = _trx.HEADER.TRANSACTIONID;
                req.SessionId = FormMainMDI.G_OPIAp.SessionID;
                req.RelationKey = _trx.HEADER.TRANSACTIONID;
                int timeoutMS = 0;
                int.TryParse((FormMainMDI.G_OPIAp.SocketWaitTime * 1000).ToString(), out timeoutMS);
                //Stopwatch stopWatch = new Stopwatch();
                //stopWatch.Start();//開始記時
                MessageResponse rsp = FormMainMDI.SocketDriver.RequestRsponse(req, timeoutMS);
                //stopWatch.Stop();//結束記時
                //TimeSpan ts = stopWatch.Elapsed;
                //NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("The elapsed time of transaction [{0}-({1})] is {2} Milliseconds", "AreYouThereRequest", _trx.HEADER.TRANSACTIONID, ts.TotalMilliseconds.ToString()));
                if (!rsp.RetCode.Equals(0))
                {
                    FormMainMDI.ConnectBCSResult = rsp.RetMsg;
                    return;
                }

                #region AreYouThereReply
                AreYouThereReply _areYouThereReply = (AreYouThereReply)Spec.CheckXMLFormat(rsp.Xml);

                #region Check Msg
                if (FormMainMDI.G_OPIAp.CurLine.FabType != _areYouThereReply.BODY.FACTORYTYPE)
                {
                    FormMainMDI.ConnectBCSResult = "Fab Type disaccords with current Fab Type";
                    return;
                }

                if (FormMainMDI.G_OPIAp.CurLine.ServerName != _areYouThereReply.BODY.LINENAME)
                {
                    FormMainMDI.ConnectBCSResult = "Line Name disaccords with current Line Name";
                    return;
                }

                if (!_areYouThereReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                {
                    FormMainMDI.ConnectBCSResult = _areYouThereReply.RETURN.RETURNMESSAGE;
                    return;
                }
                
                FormMainMDI.ConnectBCSResult = FormMainMDI.G_OPIAp.ReturnCodeSuccess;

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                OnDriverError(SocketLogLevelType.EXCEPTION, MethodBase.GetCurrentMethod().Name, string.Empty, ex.ToString());
                FormMainMDI.ConnectBCSResult = ex.Message;
            }
        }
        
        protected override string GetMsgName(XmlDocument response)
        {
            string trxId = string.Empty;
            XmlNode trxIdNode = response.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME");
            if (trxIdNode != null)
            {
                trxId = trxIdNode.InnerText;
            }
            return trxId;
        }
        
        protected override string GetMsgTrxId(XmlDocument response)
        {
            string trxId = string.Empty;
            XmlNode trxIdNode = response.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID");
            if (trxIdNode != null)
            {
                trxId = trxIdNode.InnerText;
            }
            return trxId;
        }

        protected override string GetMsgRelationKey(XmlDocument response)
        {
            string relationKey = string.Empty;
            XmlNode trxIdNode = response.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID");
            if (trxIdNode != null)
            {
                relationKey = trxIdNode.InnerText;
            }
            return relationKey;
        }

        protected override void OnConnectEvent(string session)
        {
            string _err =string.Empty ;
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] session=" + session + " connected");

            FormMainMDI.G_OPIAp.SessionID = session;

            bgwSocket = new BackgroundWorker();            

            bgwSocket.DoWork += new DoWorkEventHandler(bgwSocket_DoWork);

            bgwSocket.RunWorkerAsync();
        }

        protected override void OnCloseEvent(string session)
        {
            //Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] session=" + session + " closed");
            NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("session[{0}] closed", session));
            SocketClose();
            
        }

        protected override void OnDriverError(SocketLogLevelType LogLevle, string MethodName, string code, string Message)
        {            
            NLogManager.Logger.LogErrorWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, Message);            
        }

        protected override void OnLogEvent(SocketLogLevelType LogLevle, string MethodName, string Message)
        {
            switch (LogLevle)
            {
                case SocketLogLevelType.DEBUG:
                    NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, Message);
                    break;

                case SocketLogLevelType.TRACE:
                    NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, Message);
                    break;

                case SocketLogLevelType.EXCEPTION:
                    NLogManager.Logger.LogErrorWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, Message);
                    break;

                case SocketLogLevelType.ERROR:
                    NLogManager.Logger.LogErrorWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, Message);
                    break;
                default:
                    break;
            }
          
            SocketErrorMessage(LogLevle, MethodName, Message);
        }

        public void RecvMessageTest(DateTime timeStamp, string msgName, string trxId, string xml, string sessionId)
        {
            OnRecvMessage(timeStamp, msgName, trxId, xml, sessionId);
        }

        protected override void OnRecvMessage(DateTime timeStamp,string msgName, string trxId, string xml, string sessionId)
         {
            string _err = string.Empty;
            string _xml = string.Empty;
            string _key = string.Empty;
            
            string _msg = string.Format("Receive TrxId [{0}] ,xml [{1}] ", trxId, xml);

            try
            {
                //NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, msgName);
                //NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, _msg);

                #region 紀錄opi history
                UniTools.InsertOPIHistory_Trx_Receive(timeStamp, msgName, trxId, xml, sessionId);
                #endregion

                SocketInfo _socket = new SocketInfo();
                _socket.SocketXml = xml;
                _socket.SocketDateTime = timeStamp ;
                _socket.MsgName = msgName ;
                _socket.TrxId = trxId;
                _socket.SessionId = sessionId ;

                #region socket report
                switch (msgName)
                {

                    //case "BCSLinkTest":

                    //    #region BCSLinkTest

                    //    BCSLinkTest _bcsLinkTest = (BCSLinkTest)Spec.CheckXMLFormat(xml);

                    //    #region Send BCSLinkTestReply
                    //    BCSLinkTestReply _bcsLinkTestReply = new BCSLinkTestReply();
                    //    _bcsLinkTestReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _bcsLinkTestReply.HEADER.TRANSACTIONID = _bcsLinkTest.HEADER.TRANSACTIONID;
                    //    _bcsLinkTestReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _xml = _bcsLinkTestReply.WriteToXml();

                    //    SendMessage(_bcsLinkTestReply.HEADER.TRANSACTIONID, _bcsLinkTestReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "BCSTerminalMessageInform":

                    //    #region BCSTerminalMessageInform
                    //    BCSTerminalMessageInform _bcsTerminalMessageInform = (BCSTerminalMessageInform)Spec.CheckXMLFormat(xml);

                    //    #region BCS Terminal Message Inform Reply
                    //    BCSTerminalMessageInformReply _bcsTerminalMessageInformReply = new BCSTerminalMessageInformReply();
                    //    _bcsTerminalMessageInformReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _bcsTerminalMessageInformReply.HEADER.TRANSACTIONID = _bcsTerminalMessageInform.HEADER.TRANSACTIONID;
                    //    _bcsTerminalMessageInformReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _bcsTerminalMessageInformReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_bcsTerminalMessageInformReply.HEADER.TRANSACTIONID, _bcsTerminalMessageInformReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "LineStatusReport":

                    //    #region LineStatusReport
                    //    LineStatusReport _lineStatusReport = (LineStatusReport)Spec.CheckXMLFormat(xml);

                    //    #region Send LineStatusReportReply
                    //    LineStatusReportReply _lineStatusReportReply = new LineStatusReportReply();
                    //    _lineStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _lineStatusReportReply.HEADER.TRANSACTIONID = _lineStatusReport.HEADER.TRANSACTIONID;
                    //    _lineStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _xml = _lineStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_lineStatusReportReply.HEADER.TRANSACTIONID, _lineStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "EquipmentStatusReport":

                    //    #region EquipmentStatusReport
                    //    EquipmentStatusReport _equipmentStatusReport = (EquipmentStatusReport)Spec.CheckXMLFormat(xml);
                        
                    //    #region Send EquipmentStatusReply
                    //    EquipmentStatusReportReply _equipmentStatusReportReply = new EquipmentStatusReportReply();
                    //    _equipmentStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _equipmentStatusReportReply.HEADER.TRANSACTIONID = _equipmentStatusReport.HEADER.TRANSACTIONID;
                    //    _equipmentStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _equipmentStatusReportReply.BODY.EQUIPMENTNO = _equipmentStatusReport.BODY.EQUIPMENTNO;
                    //    _xml = _equipmentStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_equipmentStatusReportReply.HEADER.TRANSACTIONID, _equipmentStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion               

                    //case "PortCSTStatusReport":

                    //    #region PortCSTStatusReport
                    //    PortCSTStatusReport _portCSTStatusReport = (PortCSTStatusReport)Spec.CheckXMLFormat(xml);

                    //    #region Send PortCSTStatusReportReply
                    //    PortCSTStatusReportReply _portCSTStatusReportReply = new PortCSTStatusReportReply();
                    //    _portCSTStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _portCSTStatusReportReply.HEADER.TRANSACTIONID = _portCSTStatusReport.HEADER.TRANSACTIONID;
                    //    _portCSTStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _portCSTStatusReportReply.BODY.EQUIPMENTNO = _portCSTStatusReport.BODY.EQUIPMENTNO;
                    //    _portCSTStatusReportReply.BODY.PORTNO = _portCSTStatusReport.BODY.PORTNO;
                    //    _xml = _portCSTStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_portCSTStatusReportReply.HEADER.TRANSACTIONID, _portCSTStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion              

                    //case "DenseStatusReport":

                    //    #region DenseStatusReport
                    //    DenseStatusReport _denseStatusReport = (DenseStatusReport)Spec.CheckXMLFormat(xml);

                    //    #region DenseStatusReportReply
                    //    DenseStatusReportReply _denseStatusReportReply = new DenseStatusReportReply();

                    //    _denseStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _denseStatusReportReply.HEADER.TRANSACTIONID = _denseStatusReport.HEADER.TRANSACTIONID;
                    //    _denseStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _denseStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_denseStatusReportReply.HEADER.TRANSACTIONID, _denseStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "PalletStatusReport":

                    //    #region PalletStatusReport
                    //    PalletStatusReport _palletStatusReport = (PalletStatusReport)Spec.CheckXMLFormat(xml);

                    //    #region Reply
                    //    PalletStatusReportReply _palletStatusReportReply = new PalletStatusReportReply();
                    //    _palletStatusReportReply.HEADER.TRANSACTIONID = _palletStatusReport.HEADER.TRANSACTIONID;
                    //    _palletStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _palletStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _palletStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_palletStatusReportReply.HEADER.TRANSACTIONID, _palletStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "RobotCurrentModeReport":

                    //    #region RobotCurrentModeReport

                    //    RobotCurrentModeReport _robotCurrentModeReport = (RobotCurrentModeReport)Spec.CheckXMLFormat(xml);

                    //    #region Send RobotCurrentModeReportReply
                    //    RobotCurrentModeReportReply _robotCurrentModeReportReply = new RobotCurrentModeReportReply();
                    //    _robotCurrentModeReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _robotCurrentModeReportReply.HEADER.TRANSACTIONID = _robotCurrentModeReport.HEADER.TRANSACTIONID;
                    //    _robotCurrentModeReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _robotCurrentModeReportReply.BODY.EQUIPMENTNO = _robotCurrentModeReport.BODY.EQUIPMENTNO;
                    //    _robotCurrentModeReportReply.BODY.ROBOTNAME = _robotCurrentModeReport.BODY.ROBOTNAME;
                    //    _xml = _robotCurrentModeReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_robotCurrentModeReportReply.HEADER.TRANSACTIONID, _robotCurrentModeReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion            

                    //case "RobotStageInfoReport":

                    //    #region RobotStageInfoReport

                    //    RobotStageInfoReport _robotStageInfoReport = (RobotStageInfoReport)Spec.CheckXMLFormat(xml);

                    //    #region Send RobotCurrentModeReportReply
                    //    RobotStageInfoReportReply _robotStageInfoReportReply = new RobotStageInfoReportReply();
                    //    _robotStageInfoReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _robotStageInfoReportReply.HEADER.TRANSACTIONID = _robotStageInfoReport.HEADER.TRANSACTIONID;
                    //    _robotStageInfoReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _robotStageInfoReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_robotStageInfoReportReply.HEADER.TRANSACTIONID, _robotStageInfoReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "RobotCommandReport":

                    //    #region RobotCommandReport
                    //    RobotCommandReport _robotCommandReport = (RobotCommandReport)Spec.CheckXMLFormat(xml);

                    //    #region RobotCommandReport Reply
                    //    RobotCommandReportReply _robotCommandReportReply = new RobotCommandReportReply();
                    //    _robotCommandReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _robotCommandReportReply.HEADER.TRANSACTIONID = _robotCommandReport.HEADER.TRANSACTIONID;
                    //    _robotCommandReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _robotCommandReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_robotCommandReportReply.HEADER.TRANSACTIONID, _robotCommandReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "ClientDisconnectRequest":

                    //    #region ClientDisconnectRequest
                    //    ClientDisconnectRequest _clientDisconnectReq = (ClientDisconnectRequest)Spec.CheckXMLFormat(xml);
                    //    if (FormMainMDI.G_OPIAp.CurLine.ServerName == _clientDisconnectReq.BODY.LINENAME && _clientDisconnectReq.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess)
                    //        && _clientDisconnectReq.BODY.USERGROUP.Equals(FormMainMDI.G_OPIAp.LoginGroupID) && _clientDisconnectReq.BODY.USERID.Equals(FormMainMDI.G_OPIAp.LoginUserID))
                    //    {

                    //        #region 送出ClientDisconnectReply
                    //        ClientDisconnectReply _clientDisconnectReply = new ClientDisconnectReply();
                    //        _clientDisconnectReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //        _clientDisconnectReply.HEADER.TRANSACTIONID = _clientDisconnectReq.HEADER.TRANSACTIONID;
                    //        _clientDisconnectReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //        _clientDisconnectReply.BODY.LOGINSERVERIP = _clientDisconnectReq.BODY.LOGINSERVERIP;
                    //        _clientDisconnectReply.BODY.USERID = _clientDisconnectReq.BODY.USERID;
                    //        _xml = _clientDisconnectReply.WriteToXml();
                    //        SendMessage(_clientDisconnectReply.HEADER.TRANSACTIONID, _clientDisconnectReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //        #endregion

                    //    }
                    //    break;
                    //    #endregion

                    //case "CassetteMapDownloadResultReport":

                    //    #region CassetteMapDownloadResultReport
                    //    CassetteMapDownloadResultReport _cassetteMapDownloadResultReport = (CassetteMapDownloadResultReport)Spec.CheckXMLFormat(xml);

                    //    #region Cassette MapDownload Result Reply
                    //    CassetteMapDownloadResultReply _cassetteMapDownloadResultReply = new CassetteMapDownloadResultReply();
                    //    _cassetteMapDownloadResultReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _cassetteMapDownloadResultReply.HEADER.TRANSACTIONID = _cassetteMapDownloadResultReport.HEADER.TRANSACTIONID;
                    //    _cassetteMapDownloadResultReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _cassetteMapDownloadResultReply.BODY.EQUIPMENTNO = _cassetteMapDownloadResultReport.BODY.EQUIPMENTNO;
                    //    _cassetteMapDownloadResultReply.BODY.PORTNO = _cassetteMapDownloadResultReport.BODY.PORTNO;
                    //    _xml = _cassetteMapDownloadResultReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_cassetteMapDownloadResultReply.HEADER.TRANSACTIONID, _cassetteMapDownloadResultReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "CassetteMapDownloadResultReport_DPI":

                    //    #region CassetteMapDownloadResultReport_DPI
                    //    CassetteMapDownloadResultReport_DPI _cassetteMapDownloadResultReport_DPI = (CassetteMapDownloadResultReport_DPI)Spec.CheckXMLFormat(xml);

                    //    #region Cassette MapDownload Result Reply
                    //    CassetteMapDownloadResultReply_DPI _cassetteMapDownloadResultReply_DPI = new CassetteMapDownloadResultReply_DPI();
                    //    _cassetteMapDownloadResultReply_DPI.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _cassetteMapDownloadResultReply_DPI.HEADER.TRANSACTIONID = _cassetteMapDownloadResultReport_DPI.HEADER.TRANSACTIONID;
                    //    _cassetteMapDownloadResultReply_DPI.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _cassetteMapDownloadResultReply_DPI.BODY.EQUIPMENTNO = _cassetteMapDownloadResultReport_DPI.BODY.EQUIPMENTNO;

                    //    foreach (CassetteMapDownloadResultReport_DPI.PORTc _port in _cassetteMapDownloadResultReport_DPI.BODY.PORTLIST)
                    //    {
                    //        CassetteMapDownloadResultReply_DPI.PORTc _replyPort = new CassetteMapDownloadResultReply_DPI.PORTc();
                    //        _replyPort.PORTNO = _port.PORTNO;
                    //        _cassetteMapDownloadResultReply_DPI.BODY.PORTLIST.Add(_replyPort);
                    //    }

                    //    _xml = _cassetteMapDownloadResultReply_DPI.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_cassetteMapDownloadResultReply_DPI.HEADER.TRANSACTIONID, _cassetteMapDownloadResultReply_DPI.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "CurrentChangerPlanReport":

                    //    #region CurrentChangerPlanReport
                    //    CurrentChangerPlanReport _currentChangerPlanReport = (CurrentChangerPlanReport)Spec.CheckXMLFormat(xml);

                    //    #region CurrentChangerPlanReportReply
                    //    CurrentChangerPlanReportReply _currentChangerPlanReportReply = new CurrentChangerPlanReportReply();
                    //    _currentChangerPlanReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _currentChangerPlanReportReply.HEADER.TRANSACTIONID = _currentChangerPlanReport.HEADER.TRANSACTIONID;
                    //    _currentChangerPlanReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _currentChangerPlanReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_currentChangerPlanReportReply.HEADER.TRANSACTIONID, _currentChangerPlanReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion
                   
                    //    break;
                    //    #endregion

                    //case "EquipmentDataLinkStatusReport":

                    //    #region EquipmentDataLinkStatusReport
                    //    EquipmentDataLinkStatusReport _equipmentDataLinkStatusReport = (EquipmentDataLinkStatusReport)Spec.CheckXMLFormat(xml);

                    //    #region Equipment Data Link Status Report Reply
                    //    EquipmentDataLinkStatusReportReply _equipmentDataLinkStatusReportReply = new EquipmentDataLinkStatusReportReply();
                    //    _equipmentDataLinkStatusReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _equipmentDataLinkStatusReportReply.HEADER.TRANSACTIONID = _equipmentDataLinkStatusReport.HEADER.TRANSACTIONID;
                    //    _equipmentDataLinkStatusReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _xml = _equipmentDataLinkStatusReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_equipmentDataLinkStatusReportReply.HEADER.TRANSACTIONID, _equipmentDataLinkStatusReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "RecipeRegisterValidationReturnReport":

                    //    #region RecipeRegisterValidationReturnReport
                    //    RecipeRegisterValidationReturnReport _recipeRegisterValidationReturnReport = (RecipeRegisterValidationReturnReport)Spec.CheckXMLFormat(xml);

                    //    #region Recipe Register Validation Return Reply
                    //    RecipeRegisterValidationReturnReply _recipeRegisterValidationReturnReply = new RecipeRegisterValidationReturnReply();
                    //    _recipeRegisterValidationReturnReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _recipeRegisterValidationReturnReply.HEADER.TRANSACTIONID = _recipeRegisterValidationReturnReport.HEADER.TRANSACTIONID;
                    //    _recipeRegisterValidationReturnReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _recipeRegisterValidationReturnReply.BODY.PORTNO = _recipeRegisterValidationReturnReport.BODY.PORTNO;
                    //    _xml = _recipeRegisterValidationReturnReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_recipeRegisterValidationReturnReply.HEADER.TRANSACTIONID, _recipeRegisterValidationReturnReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "RecipeParameterReturnReport":

                    //    #region RecipeParameterReturnReport
                    //    RecipeParameterReturnReport _recipeParameterReturnReport = (RecipeParameterReturnReport)Spec.CheckXMLFormat(xml);

                    //    #region Recipe Parameter Return Report Reply
                    //    RecipeParameterReturnReportReply _recipeParameterReturnReportReply = new RecipeParameterReturnReportReply();
                    //    _recipeParameterReturnReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _recipeParameterReturnReportReply.HEADER.TRANSACTIONID = _recipeParameterReturnReport.HEADER.TRANSACTIONID;
                    //    _recipeParameterReturnReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _recipeParameterReturnReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_recipeParameterReturnReportReply.HEADER.TRANSACTIONID, _recipeParameterReturnReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "OperationPermissionResultReport":

                    //    #region OperationPermissionResultReport
                    //    OperationPermissionResultReport _operationPermissionResultReport = (OperationPermissionResultReport)Spec.CheckXMLFormat(xml);

                    //    #region OperationPermissionResultReportReply
                    //    OperationPermissionResultReportReply _operationPermissionResultReportReply = new OperationPermissionResultReportReply();
                    //    _operationPermissionResultReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _operationPermissionResultReportReply.HEADER.TRANSACTIONID = _operationPermissionResultReport.HEADER.TRANSACTIONID;
                    //    _operationPermissionResultReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _operationPermissionResultReportReply.BODY.EQUIPMENTNO = _operationPermissionResultReport.BODY.EQUIPMENTNO;
                    //    _xml = _operationPermissionResultReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_operationPermissionResultReportReply.HEADER.TRANSACTIONID, _operationPermissionResultReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "OperationRunModeChangeResultReport":

                    //    #region OperationRunModeChangeResultReport
                    //    OperationRunModeChangeResultReport _operationRunModeChangeResultReport = (OperationRunModeChangeResultReport)Spec.CheckXMLFormat(xml);

                    //    #region OperationPermissionResultReportReply
                    //    OperationRunModeResultReportReply _operationRunModeResultReportReply = new OperationRunModeResultReportReply();
                    //    _operationRunModeResultReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _operationRunModeResultReportReply.HEADER.TRANSACTIONID = _operationRunModeChangeResultReport.HEADER.TRANSACTIONID;
                    //    _operationRunModeResultReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //    _operationRunModeResultReportReply.BODY.EQUIPMENTNO = _operationRunModeChangeResultReport.BODY.EQUIPMENTNO;
                    //    _xml = _operationRunModeResultReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_operationRunModeResultReportReply.HEADER.TRANSACTIONID, _operationRunModeResultReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    //case "RobotUnloaderDispatchRuleReport":

                    //    #region RobotUnloaderDispatchRuleReport

                    //    RobotUnloaderDispatchRuleReport _robotUnloaderDispatchRuleReport = (RobotUnloaderDispatchRuleReport)Spec.CheckXMLFormat(xml);

                    //    #region RobotUnloaderDispatchRuleReportReply
                    //    RobotUnloaderDispatchRuleReportReply _robotUnloaderDispatchRuleReportReply = new RobotUnloaderDispatchRuleReportReply();
                    //    _robotUnloaderDispatchRuleReportReply.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //    _robotUnloaderDispatchRuleReportReply.HEADER.TRANSACTIONID = _robotUnloaderDispatchRuleReport.HEADER.TRANSACTIONID;
                    //    _robotUnloaderDispatchRuleReportReply.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    //    _xml = _robotUnloaderDispatchRuleReportReply.WriteToXml();

                    //    FormMainMDI.SocketDriver.SendMessage(_robotUnloaderDispatchRuleReportReply.HEADER.TRANSACTIONID, _robotUnloaderDispatchRuleReportReply.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    //    #endregion

                    //    break;
                    //    #endregion

                    default:
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        protected override void OnSendMessage(DateTime timeStamp,string msgName, string trxId, string xml, string sessionId)
        {
            string _msg = string.Format("Send TrxId [{0}] ,xml [{1}] ", trxId, xml);
           
            try
            {
                NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, msgName);
                NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, _msg);
                
                #region 紀錄opi history
                UniTools.InsertOPIHistory_Trx_Send(timeStamp, msgName, trxId, xml, sessionId);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }            
        }
    }
}
