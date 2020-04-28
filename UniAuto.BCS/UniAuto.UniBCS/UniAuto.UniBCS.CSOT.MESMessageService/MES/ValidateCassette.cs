using System;
using System.IO;
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


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        public FileFormatManager FileFormatManager { get; set; }

        enum eCVD_MIX_TYPE
        {
            UNKNOWN,
            TYPE1_LT_HT ,
            TYPE2_IGZO_TFT,
            TYPE3_IGZO_MQC,
            TYPE4_TFT_MQC
        }

        enum eDRY_MIX_TYPE
        {
            UNKNOWN,
            TYPE1_TFT_ENG,
            TYPE2_TFT_IGZO,
            TYPE3_IGZO_MQC
        }

        
        /// <summary>
        ///  6.154.	ValidateCassetteRequest     MES MessageSet : Cassette Validate Request Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="portID">Port Host ID</param>
        /// <param name="CassetteID">Cassette ID</param>
        public void ValidateCassetteRequest(string trxID, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, port.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateCassetteRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(port.Data.LINEID);
                // --James.Yan 2015/01/23
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID.Trim();
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME=[{1})] [BCS -> MES][{0}] , EQUIPMENT=[{2}] PORT_ID=[{3}] CSTID=[{4}].",
                         trxID, ObjectManager.LineManager.GetLineID(port.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(port.Data.NODENO),
                         ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID), port.File.CassetteID));

                #region MES ValidateCassetteReply Timeout
                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", ObjectManager.LineManager.GetLineID(port.Data.LINEID),
                    ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateCassetteT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ValidateCleanCassetteT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES][{0}] LINE_ID=[{1}] PORT_ID=[{2}] DATA REQUEST T9 TIMEOUT!",
                    trackKey, sArray[0], sArray[1]);

                string timeoutName = string.Format("{0}_{1}_MES_ValidateCleanCassetteReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0], sArray[1]);  //GetPort(sArray[1]);
                if (port != null)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                    if (cst != null)
                    {
                        switch (cst.CassetteControlCommand)
                        {
                            case eCstControlCmd.None:
                                err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - MES NO REPLY \"ValidateCleanCassetteReply\"!",
                                     sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                break;
                            case eCstControlCmd.MapDownload:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                                {
                                    err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE STATUS NO CHANGE \"WAITING_FOR_START_COMMAND\"!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                            case eCstControlCmd.ProcessStart:
                            case eCstControlCmd.ProcessStartByCount:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND)
                                {
                                    err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE STATUS NO CHANGE \"WAITING_FOR_PROCESSING\"!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                            case eCstControlCmd.ProcessCancel:
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE[{4}] HAS BEEN CANCELED!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO, port.File.CassetteID));
                                return;
                        }
                    }

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, port.Data.LINEID, err });
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ValidateCassetteT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES][{0}] LINE_ID=[{1}] PORT_ID=[{2}] DATA REQUEST T9 TIMEOUT!",
                    trackKey, sArray[0], sArray[1]);

                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0], sArray[1]);  //GetPort(sArray[1]);
                if (port != null)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                    if (cst != null)
                    {
                        switch (cst.CassetteControlCommand)
                        {
                            case eCstControlCmd.None:
                                err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - MES NO REPLY \"ValidateCassetteReply\"!",
                                     sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                break;
                            case eCstControlCmd.MapDownload:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                                {
                                    err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE STATUS NO CHANGE \"WAITING_FOR_START_COMMAND\"!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                            case eCstControlCmd.ProcessStart:
                            case eCstControlCmd.ProcessStartByCount:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND)
                                {
                                    err = string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE STATUS NO CHANGE \"WAITING_FOR_PROCESSING\"!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                            case eCstControlCmd.ProcessCancel:
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] DATA REQUEST T9 TIMEOUT - CASSETTE[{4}] HAS BEEN CANCELED!",
                                         sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO, port.File.CassetteID));
                                return;
                        }
                    }

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, port.Data.LINEID, err });
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                }
                
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.155.	ValidateCassetteReply       MES 主要回覆Target的Method
        /// 下一層是Handle_HostCassetteData
        /// </summary>
        public void MES_ValidateCassetteReply(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);

            // 獲取MES Body層資料
            XmlNode body = GetMESBodyNode(xmlDoc);

            try
            {
                XmlDocument newXmlDoc = AddOPIItemInMESXml(xmlDoc);

                string err = string.Empty;

                // 檢查 Common Data
                if (!ValidateCassetteCheckData_Common(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err))
                {
                    if (port == null)
                        throw new Exception(err);
                    else
                    {
                        // 要刪掉T9 TIMEOUT, REMOTE部份需等到 下Process Start之後再下
                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151118
                        if (_timerManager.IsAliveTimer(timeoutName1))
                        {
                            _timerManager.TerminateTimer(timeoutName1);
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }

                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    err = string.Format("T9 TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                        lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.T9_Timeout_Cassette_Have_Been_Cancel;
                    return;
                }

                if (lineName.Trim() == "TCCVD700") // Chamber of CVD724 is down,can't run [PROD1],Deng,20190909
                {
                    XmlNodeList listCHAMBERRUNMODE_PROD1 = xmlDoc.SelectNodes("//MESSAGE/BODY/LOTLIST/LOT/PRODUCTLIST/PRODUCT[CHAMBERRUNMODE='PROD1']");
                    Unit unit_CVD724 = ObjectManager.UnitManager.GetUnit("L4", "4");
                    if (listCHAMBERRUNMODE_PROD1.Count > 0 && unit_CVD724.File.MESStatus == "DOWN")
                    {
                        err = "Chamber of CVD724 is down,can't run [PROD1]";
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }

                #region By Shop 去檢查各自的資料
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                if (ParameterManager[eREPORT_SWITCH.ONLINE_SPECIAL_RULE].GetBoolean())
                {
                    bool result = false;
                    switch (fabType)
                    {
                        case eFabType.ARRAY: result = ValidateCassetteCheckData_Array(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err); break;
                        case eFabType.CF: result = ValidateCassetteCheckData_CF(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err); break;
                        case eFabType.CELL: result = ValidateCassetteCheckData_CELL(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err); break;
                        case eFabType.MODULE: result = ValidateCassetteCheckData_MODULE(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err); break;
                        default: break;
                    }

                    if (!result)
                    {
                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151118
                        if (_timerManager.IsAliveTimer(timeoutName1))
                        {
                            _timerManager.TerminateTimer(timeoutName1);
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }
                #endregion

                // 先將MES資料放入Cassette 
                MESDataIntoCstObject(body, ref cst);
                lock (cst) cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;

                // Array BFG 機台 檢查SCRAPCUTFLAG Item 
                if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG)
                {
                    if (CheckTBBFG_SCRAPCUTFLAG(xmlDoc) == false)
                    {
                        err = string.Format("[BCS -> MES][{0}] There is no \"S\" or \"C\" value in each SCRAPCUTFLAG under all Product list of all Lot list.", trxID);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);

                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }

                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, err });
                        return;
                    }
                }

                #region SAVE REMOTE/LOCAL RECIPE
                //Watson Add 20150209 Save PPID TO DB 不管Remote,Local
                ValidateCassetteReply_Recipe_Save2DB(xmlDoc, port);
                #endregion

                
                if (line.File.HostMode == eHostMode.LOCAL)
                {
                    if (fabType == eFabType.CELL && port.File.Type == ePortType.UnloadingPort)  //CELL LOCAL RUN ULD Port Auto Mapdownload,Not need OP ，qiumin20181012
                    {
                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151118
                        if (_timerManager.IsAliveTimer(timeoutName1))
                        {
                            _timerManager.TerminateTimer(timeoutName1);
                        }
                    }
                    else
                    {
                        // 要刪掉T9 TIMEOUT, REMOTE部份需等到 下Process Start之後再下
                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151118
                        if (_timerManager.IsAliveTimer(timeoutName1))
                        {
                            _timerManager.TerminateTimer(timeoutName1);
                        }
                        // 通知OPI 可供OP修改資料 
                        lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                        ObjectManager.PortManager.EnqueueSave(port.File);
                        Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { trxID, port });
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] Local Mode, Check MES Data Complete, Wait UI Edit Data.",
                            lineName, trxID));

                        return;
                    }
                }

                Decode_ValidateCassetteData(xmlDoc, line, eqp, port, cst, false);

            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                }
                string message = string.Format("EQPNo=[{0}] PORTNO=[{1}] {2}", ex.EQPNo, ex.PortNo, ex.Message);
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", message);

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, message });
                Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
                if (cst == null)
                {
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())//20161101 sy add By CELL 是不同IO
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, ex.EQPNo, " Cannot found Cassette Object.", "BCS","0" }); 
                    else
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, ex.EQPNo, " Cannot found Cassette Object.", "BCS" }); //add by bruce 2016/1/1 bc dwonload quit cst reason message to indexer
                }
                else
                {
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())//20161101 sy add By CELL 是不同IO
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, cst.NodeNo, cst.QuitCstReasonCode, "BCS", "0" });
                    else
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, cst.NodeNo, cst.QuitCstReasonCode, "BCS" }); //add by bruce 2016/1/1 bc dwonload quit cst reason message to indexer
                    }
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                    new object[] { trxID, lineName, "MES ValidateCassetteReply - NG", 
                        "Cassette Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 解析資料到Job Data
        /// Local Mode 時, 組完資料直接Call這個Method
        /// </summary>
        public void Decode_ValidateCassetteData(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst, bool IsRemap)
        {
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);
            try
            {
                string err = string.Empty;

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                List<string> lstNoCehckEQ = new List<string>();
                // Array Use : 計算Recipe Group Number
                Dictionary<string, string> recipeGroup = new Dictionary<string, string>();

                // 從MES Data 取出 Recipe Parameter 是否Check部份
                bool recipePara = body[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText.Equals("Y");

                IDictionary<string, IList<RecipeCheckInfo>> recipeIDCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IDictionary<string, IList<RecipeCheckInfo>> recipeParaCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
                IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

                IList<Job> mesJobs = new List<Job>(); 
                IList<Job> JobsforCut = new List<Job>();//sy add for Cut 前資料也要比較
                Dictionary<string, int> nodeStack = new Dictionary<string, int>();

                //Jun Add 20150205 For Cell Loader Cassette Setting Code
                cst.LDCassetteSettingCode = body[keyHost.CARRIERSETCODE].InnerText.Trim();
                string disProcessFlag = new string('0', 600);  //add by zhuxingxing 20161020

                //cc.kuang Add 2015/10/05 for Changer mode port's type check
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                {
                    if (port.File.Type != ePortType.LoadingPort && port.File.Type != ePortType.UnloadingPort)
                    {
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + "_"+"Port Type Not PL/PU at Changer Mode or Exchange Mode";
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "Port Type Not PL/PU at Changer Mode or Exchange Mode");
                    }
                }
                
                foreach (XmlNode n in lotNodeList)
                {
                    string ppid = string.Empty;
                    string mesppid = string.Empty; //Watson Add 20141125 For Report MES PPID 正在運行的(Remote,OnLineControl)
                    string crossPPID = string.Empty;
                    string productProcessType = n[keyHost.PRODUCTPROCESSTYPE].InnerText.Trim();
                    List<string> pcsSubProuctSpecs = new List<string>();//sy 20151217 add for CCPCS
                    #region [CCPCS LINE SubProuctSpecs Data]
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)//sy 20151217 add for CCPCS
                    {
                        pcsSubProuctSpecs.Add(n[keyHost.SUBPRODUCTSPECS].InnerText.Split(';')[0]);//先將第一筆加入
                        foreach (string SubProuctSpec in n[keyHost.SUBPRODUCTSPECS].InnerText.Split(';'))
                        {
                            bool same = false;
                            foreach (string pcsSubProuctSpec in pcsSubProuctSpecs)
                            {
                                if (pcsSubProuctSpec == SubProuctSpec) same = true;
                            }
                            if (!same)
                                pcsSubProuctSpecs.Add(SubProuctSpec);
                        }
                    }
                    #endregion

                    #region 將下一條LINE 的PPID 檢查內容, 放入Recipe Check Information
                    XmlNodeList processLineList = n[keyHost.PROCESSLINELIST].ChildNodes;
                    foreach (XmlNode processLineNode in processLineList)
                    {
                        // 取得Line Name
                        string nextLineName = processLineNode[keyHost.LINENAME].InnerText.Trim();
                        // Line Name 不可為空白
                        if (string.IsNullOrEmpty(nextLineName)) continue;
                        // 取得Line Recipe
                        string nextLineRecipeName = processLineNode[keyHost.LINERECIPENAME].InnerText;
                        // 將PPID 拆將成Reicpe ID數組
                        string[] recipeIDs = processLineNode[keyHost.PPID].InnerText.Split(';');

                        IList<RecipeCheckInfo> nextIdCheckInfos = new List<RecipeCheckInfo>();
                        IList<RecipeCheckInfo> nextParaCheckInfos = new List<RecipeCheckInfo>();

                        if (fabType != eFabType.CELL)
                        {
                            //// CF Short Cut Recipe ID Check
                            //if (line.Data.NEXTLINEID != nextLineName) continue;

                            //for (int i = 0; i < recipeIDs.Length; i++)
                            //{
                            //    string eqpNo = "L" + (i + 2).ToString();
                            //    nextIdCheckInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, recipeIDs[i], nextLineRecipeName));
                            //}

                            //// Recipe ID
                            //if (recipeIDCheckData.ContainsKey(nextLineName))
                            //{
                            //    List<RecipeCheckInfo> rci = recipeIDCheckData[nextLineName] as List<RecipeCheckInfo>;
                            //    rci.AddRange(nextIdCheckInfos);
                            //}
                            //else
                            //{
                            //    recipeIDCheckData.Add(nextLineName, nextIdCheckInfos);
                            //}
                        }
                        else
                        {
                            string[] lotRecipeIDs = n[keyHost.PPID].InnerText.Split(';');

                            if (line.Data.LINEID != nextLineName)
                            {
                                //跨Line的時候
                                //Cross Line的Loader, CUT 的Recipe ID and Parameter 不需要Check
                                nextIdCheckInfos.Add(new RecipeCheckInfo("L2", 1, 0, "00", nextLineRecipeName));
                                nextIdCheckInfos.Add(new RecipeCheckInfo("L3", 1, 0, "00", nextLineRecipeName));

                                nextParaCheckInfos.Add(new RecipeCheckInfo("L2", 1, 0, "00", nextLineRecipeName));
                                nextParaCheckInfos.Add(new RecipeCheckInfo("L3", 1, 0, "00", nextLineRecipeName));

                                for (int i = 2; i < recipeIDs.Length; i++)
                                {
                                    string eqpNo = "L" + (i + 2).ToString();
                                    nextIdCheckInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, recipeIDs[i], nextLineRecipeName));

                                    // CELL 的Short cut分別才需要Check 下一條Line 的Recipe Parameter
                                    nextParaCheckInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, recipeIDs[i], nextLineRecipeName));
                                }

                                // Recipe ID
                                if (recipeIDCheckData.ContainsKey(nextLineName))
                                {
                                    List<RecipeCheckInfo> rci = recipeIDCheckData[nextLineName] as List<RecipeCheckInfo>;
                                    rci.AddRange(nextIdCheckInfos);
                                }
                                else
                                {
                                    recipeIDCheckData.Add(nextLineName, nextIdCheckInfos);
                                }

                                // Recipe Parameter
                                if (recipeParaCheckData.ContainsKey(nextLineName))
                                {
                                    List<RecipeCheckInfo> rci = recipeParaCheckData[nextLineName] as List<RecipeCheckInfo>;
                                    rci.AddRange(nextParaCheckInfos);
                                }
                                else
                                {
                                    recipeParaCheckData.Add(nextLineName, nextParaCheckInfos);
                                }
                            }
                            else
                            {
                                //跨Line的時候
                                //Current Line的Loader, CUT 的Recipe ID and Parameter 需要Check
                                //nextIdCheckInfos.Add(new RecipeCheckInfo("L2", 1, 0, lotRecipeIDs[0], nextLineRecipeName));
                                //nextIdCheckInfos.Add(new RecipeCheckInfo("L3", 1, 0, lotRecipeIDs[1], nextLineRecipeName));

                                //nextParaCheckInfos.Add(new RecipeCheckInfo("L2", 1, 0, lotRecipeIDs[0], nextLineRecipeName));
                                //nextParaCheckInfos.Add(new RecipeCheckInfo("L3", 1, 0, lotRecipeIDs[1], nextLineRecipeName));
                            }
                        }
                    }
                    #endregion

                    #region SAVE REMOTE/LOCAL RECPE
                    //Watson Add 20150209 Save PPID TO DB 不管Remote,Local
                    ValidateCassetteReply_Recipe_Save2DB(xmlDoc, port);
                    #endregion

                    //T3 cancel T2超大線PPID檢查
                    #region T2超大線檢查
                    //if (fabType == eFabType.CELL)
                    //{
                    //    #region 將切割超大線後面偏貼段的PPID檢查內容, 放入Recipe Check Information
                    //    XmlNodeList stbList = n[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                    //    foreach (XmlNode stbNode in stbList)
                    //    {
                    //        string stbName = stbNode[keyHost.LINENAME].InnerText.Trim();
                    //        if (string.IsNullOrEmpty(stbName)) continue;

                    //        string stbLineRecipeName = stbNode[keyHost.LINERECIPENAME].InnerText;
                    //        string[] recipeIDs = stbNode[keyHost.PPID].InnerText.Split(';');
                    //        IList<RecipeCheckInfo> stbIdRecipeInfos = new List<RecipeCheckInfo>();
                    //        IList<RecipeCheckInfo> stbParaRecipeInfos = new List<RecipeCheckInfo>();

                    //        for (int i = 0; i < recipeIDs.Length; i++)
                    //        {
                    //            string eqpNo = "L" + (i + 10).ToString();
                    //            stbIdRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, recipeIDs[i], stbLineRecipeName));
                    //            stbParaRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, recipeIDs[i], stbLineRecipeName));
                    //        }

                    //        // Recipe ID
                    //        if (recipeIDCheckData.ContainsKey(stbName))
                    //        {
                    //            List<RecipeCheckInfo> rci = recipeIDCheckData[stbName] as List<RecipeCheckInfo>;
                    //            rci.AddRange(stbIdRecipeInfos);
                    //        }
                    //        else
                    //        {
                    //            recipeIDCheckData.Add(stbName, stbIdRecipeInfos);
                    //        }

                    //        // Recipe Parameter
                    //        if (recipeParaCheckData.ContainsKey(stbName))
                    //        {
                    //            List<RecipeCheckInfo> rci = recipeIDCheckData[stbName] as List<RecipeCheckInfo>;
                    //            rci.AddRange(stbParaRecipeInfos);
                    //        }
                    //        else
                    //        {
                    //            recipeParaCheckData.Add(stbName, stbParaRecipeInfos);
                    //        }
                    //    }
                    //    #endregion
                    //}
                    #endregion

                    #region Create Slot Data
                    string _mmgflag = n[keyHost.PROCESSTYPE].InnerText.Trim().Equals("MMG") ? "Y" : "N";
                    string recipe_Assembly_log_tmp = string.Empty;
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                        Job job = null;

                        if (IsRemap)
                        {
                            job = ObjectManager.JobManager.GetJob(port.File.CassetteSequenceNo, slotNo.ToString());
                            // 表示要重新量測, 清掉值
                            if (productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Equals("Y"))
                            {
                                job.TargetPortID = "0";
                                job.ToSlotNo = "0";
                                job.TrackingData = SpecialItemInitial("TrackingData", 32);
                                job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
                                job.HoldInforList.Clear();
                                job.JobProcessStartTime = DateTime.Now;
                                job.QtimeList.Clear();
                                job.DefectCodes.Clear();
                                job.JobProcessFlows.Clear();
                            }
                        }

                        if (job == null)
                        {
                            job = new Job(int.Parse(port.File.CassetteSequenceNo), slotNo);
                            job.TargetPortID = "0";
                            job.ToSlotNo = "0";
                            job.TrackingData = SpecialItemInitial("TrackingData", 32);
                            job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                            job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
                        }

                        job.EQPJobID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();
                        job.SourcePortID = port.Data.PORTID;
                        
                        job.FromCstID = port.File.CassetteID;
                        job.ToCstID = string.Empty;
                        job.FromSlotNo = slotNo.ToString();
                        job.CurrentSlotNo = job.FromSlotNo; // add by bruce 20160412 for T2 Issue
                        job.CurrentEQPNo = eqp.Data.NODENO;
                        job.MMGFlag = _mmgflag;

                        //CF Photo Line Unloader沒有CSTOperationMode By Jm.Pan
                        if (fabType == eFabType.CF && port.File.Type == ePortType.UnloadingPort &&
                           (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS || line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB))
                        {
                            Equipment eqps = ObjectManager.EquipmentManager.GetEQP("L2");
                            job.CSTOperationMode = eqps.File.CSTOperationMode;

                        }
                        else
                        {
                            job.CSTOperationMode = eqp.File.CSTOperationMode;
                        }

                        // 有OPI的部份使用OPI的
                        if (string.IsNullOrEmpty(productList[i][keyHost.OPI_PRODUCTRECIPENAME].InnerText.Trim()))
                            job.LineRecipeName = productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim();
                        else
                            job.LineRecipeName = productList[i][keyHost.OPI_PRODUCTRECIPENAME].InnerText.Trim();
                        if (line.Data.LINETYPE == "DRY_TEL")  // add by qiumin 20170915
                        {
                            job.GroupIndex ="1";
                        }
                        else 
                        {
                            job.GroupIndex = (fabType == eFabType.CELL ? ObjectManager.JobManager.M2P_GetCellGroupIndex(line, productList[i][keyHost.GROUPID].InnerText) : "0");
                        }


                        job.SubstrateType = ObjectManager.JobManager.M2P_GetSubstrateType(n[keyHost.PRODUCTGCPTYPE].InnerText);
                        job.CIMMode = eBitResult.ON;
                        job.JobType = ObjectManager.JobManager.M2P_GetJobType(productList[i][keyHost.PRODUCTTYPE].InnerText);

                        switch (fabType)
                        {
                            case eFabType.ARRAY:
                                {
                                    int lotpriority=0;
                                    job.JobJudge = "1";
                                    job.JobLotPriority = int.TryParse(n[keyHost.LOTPRIORITY].InnerText.Trim(),out lotpriority)?int.Parse(n[keyHost.LOTPRIORITY].InnerText.Trim()):0;//ADD BY HUJUNPENG 20190617
                                    break;
                                }
                                
                            case eFabType.CF:
                                {
                                    switch (line.Data.LINETYPE)
                                    {
                                        case eLineType.CF.FCSRT_TYPE1:
                                            job.JobJudge = "1"; break;
                                        default:
                                            // CF Special 必須使用 PorductGrade轉成Judge
                                            job.JobJudge = ConstantManager[string.Format("PLC_{0}_JOBJUDGE", line.Data.FABTYPE)][productList[i][keyHost.PRODUCTGRADE].InnerText.Trim()].Value;
                                            break;
                                    }
                                    break;
                                }
                            case eFabType.CELL:
                                switch (line.Data.LINETYPE)
                                {
                                    case eLineType.CELL.CCPIL:
                                    case eLineType.CELL.CCPIL_2:
                                    case eLineType.CELL.CCODF:
                                    case eLineType.CELL.CCODF_2://sy add 20160907
                                    case eLineType.CELL.CCPTH:
                                    case eLineType.CELL.CCTAM:
                                    case eLineType.CELL.CCGAP:
                                    case eLineType.CELL.CCPDR:
                                        job.JobJudge = "1";
                                        break;
                                    default:
                                        job.JobJudge = "0";
                                        break;
                                }
                                break;
                            case eFabType.MODULE: job.JobJudge = "0"; break;
                        }

                        // 有OPI的部份使用OPI的
                        // yang 2016/11/12 MES给的Process Flag要keep住,BC修改的Process Flag要update MES validatereply xml
                        if (string.IsNullOrEmpty(productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Trim()))
                        {
                            job.SamplingSlotFlag = productList[i][keyHost.PROCESSFLAG].InnerText.Equals("Y") ? "1" : "0";
                            //Watson Add 20150316 For Auto Abort Cassette.    
                            job.RobotProcessFlag = productList[i][keyHost.PROCESSFLAG].InnerText.Equals("Y") ? keyCELLROBOTProcessFlag.WAIT_PROCESS : keyCELLROBOTProcessFlag.NO_PROCESS; //Watson Add 20150316 For Auto Abort Cassette.               

                           // job.MesProduct.MESPROCESSFLAG = productList[i][keyHost.PROCESSFLAG].InnerText;
                           productList[i][keyHost.MESPROCESSFLAG].InnerText = productList[i][keyHost.PROCESSFLAG].InnerText;
                        }
                        else if (IsRemap) //yang
                        {
                            job.SamplingSlotFlag = productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Equals("Y") ? "1" : "0";
                            job.RobotProcessFlag = productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Equals("Y") ? keyCELLROBOTProcessFlag.WAIT_PROCESS : keyCELLROBOTProcessFlag.NO_PROCESS;

                            productList[i][keyHost.PROCESSFLAG].InnerText = productList[i][keyHost.OPI_PROCESSFLAG].InnerText;
                        }
                        else
                        {
                            job.SamplingSlotFlag = productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Equals("Y") ? "1" : "0";                             
                            job.RobotProcessFlag = productList[i][keyHost.OPI_PROCESSFLAG].InnerText.Equals("Y") ? keyCELLROBOTProcessFlag.WAIT_PROCESS : keyCELLROBOTProcessFlag.NO_PROCESS;

                          //  job.MesProduct.MESPROCESSFLAG = productList[i][keyHost.PROCESSFLAG].InnerText;
                            productList[i][keyHost.MESPROCESSFLAG].InnerText = productList[i][keyHost.PROCESSFLAG].InnerText;
                            productList[i][keyHost.PROCESSFLAG].InnerText = productList[i][keyHost.OPI_PROCESSFLAG].InnerText;
                        }                      
                        job.FirstRunFlag = "0";
                        job.JobGrade = productList[i][keyHost.PRODUCTGRADE].InnerText.Trim();
                        job.GlassChipMaskBlockID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();

                        string mesOXR = productList[i][keyHost.SUBPRODUCTGRADES].InnerText.Trim();
                        if (line.Data.LINETYPE == eLineType.CELL.CCPCS)//T3 shihyang add ChipCount代表 BLOCK COUNT 一切 時照這個ChipCount 
                        {
                            job.ChipCount = productList[i][keyHost.BLOCKJUDGES].InnerText.Trim().Length;
                        }
                        else
                        {
                            job.ChipCount = mesOXR.Length;
                        }                        
                        job.OXRInformationRequestFlag = job.ChipCount > 56 ? "1" : "0";
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add MES By LINE e
                        {
                            job.OXRInformation = productList[i][keyHost.SUBPRODUCTGRADES].InnerText;
                            if (job.OXRInformation == "0")
                            {
                                job.OXRInformation = string.Empty;
                            }
                        }
                        else
                        {
                            job.OXRInformation = productList[i][keyHost.SUBPRODUCTGRADES].InnerText;
                        }
                        
                        job.HostDefectCodeData = Transfer_DefectCodeToFileFormat(productList[i][keyHost.SUBPRODUCTDEFECTCODE].InnerText);

                        #region Watson Add 20150318 For 福杰、佐斌 CUT OXINFO COUNT <> CHIP COUNT 討論做開關來退Cassette
                        if ((fabType == eFabType.CELL) && ( line.Data.JOBDATALINETYPE  ==eJobDataLineType.CELL.CBCUT))
                        {
                            if (!ParameterManager[eCELL_SWITCH.CUT_OXINFO_COUNT_MISMATCH_CANCEL_CST].GetBoolean())
                            {
                                string[] chipID = n[keyHost.SUBPRODUCTNAMES].InnerText.Split(';');
                                if (chipID.Length != job.ChipCount)
                                {
                                    //Cancel Cassette OXINFO COUNT != SUB PRODUCEID COUNT
                                    err = string.Format("CASSETTE HAVE BEEN CANCEL. LINE_ID={0}, Port_ID={1} ,CASSETTE_ID=[{2}].",
                                    lineName, port.Data.PORTID, port.File.CassetteID);
                                    err += string.Format("POSITION (SLOTNO)=[{0}] ,GLASSID=[{1}],", slotNo.ToString(), job.GlassChipMaskBlockID);
                                    err += string.Format("MES DATA OXINFO COUNT=[{0}]({1}) AND CHIP PANELID COUNT=[{2}]({3}) MISMATCH!!",
                                    job.ChipCount, job.OXRInformation, chipID.Length, n[keyHost.SUBPRODUCTNAMES].InnerText);
                                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                                    cst.ReasonText = MES_ReasonText.Loader_BC_Cancel_Validation_NG_From_MES + "_" +  err;
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, err });
                                    return;
                                }
                            }
                        }
                        #endregion
                        #region calculation product type
                        List<string> items = new List<string>();
                        List<string> idItems = new List<string>();
                        job.ProductType.Items.Clear();  //add by yang 2017/6/26 ,when re-map, clear job.ProductType
                        switch (fabType)
                        {
                            case eFabType.ARRAY:
                                {
                                    items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                    items.Add(keyHost.PRODUCTSPECVER + "_" + n[keyHost.PRODUCTSPECVER].InnerText);
                                    items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                    items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                    items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                    items.Add(keyHost.OWNERTYPE + "_" + productList[i][keyHost.OWNERTYPE].InnerText);
                                    job.ProductType.SetItemData(items);
                                }
                                break;
                            case eFabType.CF:
                                {
                                    switch (line.Data.LINETYPE)
                                    {
                                        case eLineType.CF.FCREW_TYPE1:
                                            items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            //items.Add(keyHost.SOURCEPART + "_" + productList[i][keyHost.SOURCEPART].InnerText);//20160829 Modified by Zhangwei 
                                            items.Add(keyHost.SOURCELOTNAME + "_" + productList[i][keyHost.SOURCELOTNAME].InnerText);//Add For CF Lot To Lot 
                                            items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);//20151202 Add by Frank For CF All Line
                                            job.ProductType.SetItemData(items);
                                            break;
                                        case eLineType.CF.FCUPK_TYPE1:
                                            items.Add(keyHost.PLANNEDPRODUCTSPECNAME + "_" + body[keyHost.PLANNEDPRODUCTSPECNAME].InnerText);
                                            items.Add(keyHost.PLANNEDPROCESSOPERATIONNAME + "_" + body[keyHost.PLANNEDPROCESSOPERATIONNAME].InnerText);
                                            items.Add(keyHost.UPKOWNERTYPE + "_" + body[keyHost.UPKOWNERTYPE].InnerText);
                                            //items.Add(keyHost.PLANNEDSOURCEPART + "_" + body[keyHost.PLANNEDSOURCEPART].InnerText);//20160829 Modified by Zhangwei 
                                            if ((port.File.Type == ePortType.BothPort) &&
                                                (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL) &&
                                                (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.Re_Clean))//Add For CF Lot To Lot
                                                items.Add(keyHost.SOURCELOTNAME + "_" + productList[i][keyHost.SOURCELOTNAME].InnerText);
                                            job.ProductType.SetItemData(items);                                         
                                            break;
                                        default:
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            items.Add(keyHost.PRODUCTSPECVER + "_" + n[keyHost.PRODUCTSPECVER].InnerText);
                                            items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            //items.Add(keyHost.SOURCEPART + "_" + productList[i][keyHost.SOURCEPART].InnerText);//20160829 Modified by Zhangwei 
                                            if (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL)    //Add For CF Lot To Lot 
                                                items.Add(keyHost.SOURCELOTNAME + "_" + productList[i][keyHost.SOURCELOTNAME].InnerText);
                                            items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);//20151202 Add by Frank For CF All Line
                                            job.ProductType.SetItemData(items);
                                            break;
                                    }
                                }
                                break;
                            case eFabType.CELL:
                                #region [CELL]
                                {
                                    switch (line.Data.LINETYPE)
                                    {
                                        #region [T3]
                                        case eLineType.CELL.CCPIL://5/1/PRODUCTOWNER"E"/PFCD*2
                                        case eLineType.CELL.CCPIL_2:
                                        //Product Type: ProductOwner,Oper ID, OwnerID,PFCD*2, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            if (job.JobType == eJobType.TFT)
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            else if (job.JobType == eJobType.CF)
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }
                                            else
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PAIRPRODUCTSPECNAME + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,Oper ID, PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            if (job.JobType == eJobType.TFT)
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            else if (job.JobType == eJobType.CF)
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }
                                            else
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PAIRPRODUCTSPECNAME + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCPDR://5/1/PRODUCTOWNER"E"
                                        case eLineType.CELL.CCTAM://5/1/PRODUCTOWNER"E"
                                        case eLineType.CELL.CCPTH://5/1/PRODUCTOWNER"E"
                                            //Product Type: ProductOwner,Oper ID, OwnerID,PFCD, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,Oper ID, PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCRWK:
                                        case eLineType.CELL.CCNLS:
                                        case eLineType.CELL.CCNRD:
                                        //5/2/Owner Type is "E" &Owner ID isn't "RESD"
                                            //Product Type: ProductOwner,Oper ID, OwnerID,PFCD, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                               && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,Oper ID, PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                            idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                               && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCODF://4/1/PRODUCTOWNER"E"/PFCD*2
                                        case eLineType.CELL.CCODF_2://sy add 20160907
                                            //Product Type: ProductOwner,OwnerID,PFCD*2, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            if (job.JobType == eJobType.TFT)
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            else if (job.JobType == eJobType.CF)
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }
                                            else
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PAIRPRODUCTSPECNAME + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,PFCD*2, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            //idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            if (job.JobType == eJobType.TFT)
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);

                                            }
                                            else if (job.JobType == eJobType.CF)
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORTFT + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAMEFORCF + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }
                                            else
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PAIRPRODUCTSPECNAME + "_" + n[keyHost.PAIRPRODUCTSPECNAME].InnerText);
                                            }
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCPCS://4/1/Owner Type is "E"
                                            //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E")//sy add 20160829
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E")//sy add 20160829
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCPCK:      //4/1/Product Owner is "E"
                                        case eLineType.CELL.CCRWT:      //4/1/Product Owner is "E"
                                        case eLineType.CELL.CCSOR:      //4/1/Product Owner is "E"
                                        case eLineType.CELL.CCCHN:      //4/1/Product Owner is "E"
                                            //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);

                                            if (line.Data.LINETYPE == eLineType.CELL.CCPCK && line.File.LineOperMode==eMES_LINEOPERMODE.NORMAL)
                                            {

                                                items.Add(keyHost.SHIPPRODUCTSPECNAME + "_" + n[keyHost.SHIPPRODUCTSPECNAME].InnerText);
                                            }
                                            else//20170703 huangjiayin: Pck use ShipProductSpecName Instead...
                                            {
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }

                                            //20161208 huangjiayin: add Opeation for sor,chn,pck
                                            if (line.Data.LINETYPE != eLineType.CELL.CCRWT)
                                            { items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText); }
                                            ///
                                            //20170204 huangjiayin: PCK Add Grade
                                            if ( line.Data.LINETYPE == eLineType.CELL.CCPCK)
                                            { 
                                                items.Add(keyHost.PRODUCTGRADE + "_" + n[keyHost.PRODUCTLIST][keyHost.PRODUCT][keyHost.PRODUCTGRADE].InnerText); 
                                                
                                            }

                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            if (line.Data.LINETYPE == eLineType.CELL.CCSOR || 
                                                line.Data.LINETYPE == eLineType.CELL.CCRWT || 
                                                line.Data.LINETYPE==eLineType.CELL.CCCHN) //Add By YangZhenteng For SOR&RWT 原站点Check;
                                            {
                                                items.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                            } 
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);

                                            if (line.Data.LINETYPE == eLineType.CELL.CCPCK && line.File.LineOperMode == eMES_LINEOPERMODE.NORMAL)
                                            {
                                                idItems.Add(keyHost.SHIPPRODUCTSPECNAME + "_" + n[keyHost.SHIPPRODUCTSPECNAME].InnerText);

                                            }
                                            else//20170703 huangjiayin: Pck use ShipProductSpecName Instead...
                                            {
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            }

                                            //20161208 huangjiayin: add Opeation for sor,chn,pck
                                            if (line.Data.LINETYPE != eLineType.CELL.CCRWT)
                                            { idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText); }
                                            ///
                                            //20170204 huangjiayin: PCK Add Grade
                                            if (line.Data.LINETYPE == eLineType.CELL.CCPCK)
                                            { idItems.Add(keyHost.PRODUCTGRADE + "_" + n[keyHost.PRODUCTLIST][keyHost.PRODUCT][keyHost.PRODUCTGRADE].InnerText);}

                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCCRP://4/2
                                        case eLineType.CELL.CCCRP_2:
                                        case eLineType.CELL.CCQUP://4/2/Owner Type is "E" & Owner ID isn't "RESD"
                                        case eLineType.CELL.CCQPP://4/2/Owner Type is "E" & Owner ID isn't "RESD"
                                        case eLineType.CELL.CCPPK://4/2/Owner Type is "E" & Owner ID isn't "RESD"
                                        case eLineType.CELL.CCGAP://4/2/Owner Type is "E" & Owner ID isn't "RESD"
                                            //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                               && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID: ProductOwner,PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                            {
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                               && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CCQSR://3/1/Product Owner is "E"
                                            //Product Type,OwnerID,PFCD, GroupID
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductType.SetItemData(items);
                                            //Product ID,PFCD, GroupID
                                            idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                            if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                            {
                                                if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            }
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        #endregion
                                        #region [T2]
                                        case eLineType.CELL.CBPIL:
                                        case eLineType.CELL.CBODF:
                                            //PI, ODF Product Type: ProductOwner, OwnerID, GroupID
                                            items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                            items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            job.ProductType.SetItemData(items);

                                            //PI, ODF Product ID: ProductOwner, GroupID
                                            idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                            idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                            job.ProductID.SetItemData(idItems);
                                            break;
                                        case eLineType.CELL.CBPMT:
                                            //Jun Add 20150603 ProductType的生成, 若是RW Judge, 则使用ProductOwner, OwnerID, GroupID. 其它Judge则是ProductSpecName, OwnerID, ProductOwner
                                            if (productList[i][keyHost.PRODUCTJUDGE].InnerText != "R")
                                            {
                                                //PMT Product Type: ProductSpecName, OwnerID, ProductOwner
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                job.ProductType.SetItemData(items);

                                                //PMT Product ID: ProductSpecName, ProductOwner
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                job.ProductID.SetItemData(idItems);
                                            }
                                            else
                                            {
                                                //PMT Product Type: ProductOwner, OwnerID, GroupID
                                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                job.ProductType.SetItemData(items);

                                                //PMT Product ID: ProductOwner, GroupID
                                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                job.ProductID.SetItemData(idItems);
                                            }
                                            break;

                                        case eLineType.CELL.CBCUT_1:
                                        case eLineType.CELL.CBCUT_2:
                                        case eLineType.CELL.CBCUT_3:
                                            //MES 會生成Product Type and Product ID
                                            break;

                                        case eLineType.CELL.CBATS:
                                            if (n[keyHost.CURRENTFACTORYNAME].InnerText == "CELL")
                                            {
                                                if (port.Data.PORTID != "07")
                                                {
                                                    if (n[keyHost.ISPIREWORK].InnerText == "Y")
                                                    {
                                                        //RW    產品生成的ProductType:       OWNER ID + NodeStack + (TFT PFCD , CF PFCD)
                                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        items.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                        if (productList[i][keyHost.PRODUCTTYPE].InnerText == "TFT")
                                                            items.Add(string.Format("TFT_PRODUCTSPECNAME_{0},CF_PRODUCTSPECNAME_{1}", n[keyHost.PRODUCTSPECNAME].InnerText, n[keyHost.PAIRPRODUCTSPECNAME].InnerText));
                                                        else
                                                            items.Add(string.Format("TFT_PRODUCTSPECNAME_{0},CF_PRODUCTSPECNAME_{1}", n[keyHost.PAIRPRODUCTSPECNAME].InnerText, n[keyHost.PRODUCTSPECNAME].InnerText));
                                                        job.ProductType.SetItemData(items);

                                                        //Porduct ID same as Porduct Type
                                                        idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        idItems.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                        if (productList[i][keyHost.PRODUCTTYPE].InnerText == "TFT")
                                                            idItems.Add(string.Format("TFT_PRODUCTSPECNAME_{0},CF_PRODUCTSPECNAME_{1}", n[keyHost.PRODUCTSPECNAME].InnerText, n[keyHost.PAIRPRODUCTSPECNAME].InnerText));
                                                        else
                                                            idItems.Add(string.Format("TFT_PRODUCTSPECNAME_{0},CF_PRODUCTSPECNAME_{1}", n[keyHost.PAIRPRODUCTSPECNAME].InnerText, n[keyHost.PRODUCTSPECNAME].InnerText));
                                                        job.ProductID.SetItemData(idItems);
                                                    }
                                                    else
                                                    {
                                                        //Normal產品生成的ProductType:PFCD + OWNER ID
                                                        items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        job.ProductType.SetItemData(items);

                                                        //Porduct ID same as Porduct Type
                                                        idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        job.ProductID.SetItemData(idItems);
                                                    }
                                                }
                                                else
                                                {
                                                    if (n[keyHost.ISPIREWORK].InnerText == "Y")
                                                    {
                                                        //RW    產品生成的ProductType:PFCD + OWNER ID + NodeStack
                                                        items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        items.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                        job.ProductType.SetItemData(items);

                                                        //Porduct ID same as Porduct Type
                                                        idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        idItems.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                        job.ProductID.SetItemData(idItems);
                                                    }
                                                    else
                                                    {
                                                        //Normal產品生成的ProductType:PFCD + OWNER ID
                                                        items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        job.ProductType.SetItemData(items);

                                                        //Porduct ID same as Porduct Type
                                                        idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                        idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                        job.ProductID.SetItemData(idItems);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //Jun Add20150603 AC厂的玻璃
                                                //ProductType:Shop + ProductSpecName + ProductSpecVer + ProcessOperationName + OwnerID + NodeStack
                                                items.Add(keyHost.CURRENTFACTORYNAME + "_" + n[keyHost.CURRENTFACTORYNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                items.Add(keyHost.PRODUCTSPECVER + "_" + n[keyHost.PRODUCTSPECVER].InnerText);
                                                items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                job.ProductType.SetItemData(items);

                                                //Porduct ID same as Porduct Type
                                                idItems.Add(keyHost.CURRENTFACTORYNAME + "_" + n[keyHost.CURRENTFACTORYNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECVER + "_" + n[keyHost.PRODUCTSPECVER].InnerText);
                                                idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                                idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                idItems.Add(keyHost.NODESTACK + "_" + n[keyHost.NODESTACK].InnerText);
                                                job.ProductID.SetItemData(idItems);
                                            }
                                            break;
                                        #endregion
                                        default:
                                            if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                                                #region [CUT]
                                            {
                                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID //4/1/Product Owner is "E"
                                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                                {
                                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                }
                                                job.ProductType.SetItemData(items);
                                                //Product ID: ProductOwner,PFCD, GroupID
                                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                                {
                                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                                        idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                }
                                                job.ProductID.SetItemData(idItems);
                                            }
                                            #endregion
                                            else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                                                #region [POL]
                                            {
                                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                                {
                                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                }
                                                job.ProductType.SetItemData(items);
                                                //Product ID: ProductOwner,PFCD, GroupID
                                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                                {
                                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                }
                                                job.ProductID.SetItemData(idItems);
                                            }
                                            #endregion
                                            else
                                                #region [Other]
                                            {
                                                //BEOL, HVA, GAP 
                                                //Product Type: 若OwnerType最后一码是E,但Owner ID不为RESD: ProductSpecName, Owner ID, ProductOwner, Group ID
                                                //                                              其它情况: ProductSpecName, Owner ID, ProductOwner.
                                                //Product ID  : 若OwnerType最后一码是E,但Owner ID不为RESD: ProductSpecName, ProductOwner, Group ID
                                                //                                              其它情况: ProductSpecName, ProductOwner.
                                                if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                                    && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                                {
                                                    //Product Type
                                                    items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                    items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                    items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                    job.ProductType.SetItemData(items);

                                                    //Product ID
                                                    idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                    idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                                    job.ProductID.SetItemData(idItems);
                                                }
                                                else
                                                {
                                                    //Product Type
                                                    items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                    items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                                    items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                    job.ProductType.SetItemData(items);

                                                    //Product ID
                                                    idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                                    idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                                    job.ProductID.SetItemData(idItems);
                                                }
                                            }
                                            #endregion
                                            break;
                                    }
                                }
                                break;
                                #endregion
                            case eFabType.MODULE:
                            default:
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (n[keyHost.PRODUCTOWNER].InnerText == "E" & productList[i][keyHost.OWNERID].InnerText == "RESD")
                                    items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                job.ProductType.SetItemData(items);
                                //Product ID: ProductOwner,PFCD, GroupID
                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                //idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (n[keyHost.PRODUCTOWNER].InnerText == "E" & productList[i][keyHost.OWNERID].InnerText == "RESD")
                                    idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                job.ProductID.SetItemData(idItems);
                                break;
                        }

                        if (!ObjectManager.JobManager.GetProductType(fabType, productList[i][keyHost.OWNERTYPE].InnerText, mesJobs, job, out err))
                        {
                            string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                            if (_timerManager.IsAliveTimer(timeoutName))
                            {
                                _timerManager.TerminateTimer(timeoutName);
                            }
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                        }
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            //if (!line.Data.LINETYPE.Contains(keyCellLineType.CUT.ToString()))//sy mark T3 MES 不會給
                            //{
                            if (!ObjectManager.JobManager.GetProductID(port, mesJobs, job, out err))
                            {
                                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                            }
                            //}
                            if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                            {
                                #region [PCS Product Types & IDs]
                                JobsforCut.Add(job);
                                List<string> pcsitems1 = new List<string>();                                
                                List<string> pcsidItems1 = new List<string>();                                
                                Job pcsJob1 = new Job(0, 0);//                                
                                string pcsProuctSpecs1 = pcsSubProuctSpecs[0]; 
                                #region [PCS1]
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                pcsitems1.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                pcsitems1.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                //items.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                pcsitems1.Add(keyHost.PRODUCTSPECNAME + "_" + pcsProuctSpecs1);
                                if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                {
                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                        pcsitems1.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                pcsJob1.ProductType.SetItemData(pcsitems1);
                                //Product ID: ProductOwner,PFCD, GroupID
                                pcsidItems1.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                pcsidItems1.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                //idItems.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                pcsidItems1.Add(keyHost.PRODUCTSPECNAME + "_" + pcsProuctSpecs1); if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                {
                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                        pcsidItems1.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                pcsJob1.ProductID.SetItemData(pcsidItems1);
                                if (!ObjectManager.JobManager.GetProductType(fabType, productList[i][keyHost.OWNERTYPE].InnerText, JobsforCut, pcsJob1, out err))
                                {
                                    string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                    if (_timerManager.IsAliveTimer(timeoutName))
                                    {
                                        _timerManager.TerminateTimer(timeoutName);
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                                if (!ObjectManager.JobManager.GetProductID(port, JobsforCut, pcsJob1, out err))
                                {
                                    string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                    if (_timerManager.IsAliveTimer(timeoutName))
                                    {
                                        _timerManager.TerminateTimer(timeoutName);
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                                job.CellSpecial.CUTProductType = pcsJob1.ProductType.Value.ToString();
                                job.CellSpecial.ProductType1 = pcsJob1.ProductType;
                                job.CellSpecial.CUTProductID = pcsJob1.ProductID.Value.ToString();
                                job.CellSpecial.ProductID1 = pcsJob1.ProductID;
                                #endregion
                                #region [不等分切]
                                if (pcsSubProuctSpecs.Count == 2)
                                {
                                    List<string> pcsitems2 = new List<string>();
                                    List<string> pcsidItems2 = new List<string>();
                                    Job pcsJob2 = new Job(0, 1);//
                                    string pcsProuctSpecs2 = pcsSubProuctSpecs[1];
                                    JobsforCut.Add(pcsJob1);//把pcsJob1加入判別，以免pcsJob1的Type&ID 會與pcsJob2
                                    #region [PCS2]
                                    //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                    pcsitems2.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                    pcsitems2.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                    //items.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                    pcsitems2.Add(keyHost.PRODUCTSPECNAME + "_" + pcsProuctSpecs2);
                                    if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                    {
                                        if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                            pcsitems2.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                    }
                                    pcsJob2.ProductType.SetItemData(pcsitems2);
                                    //Product ID: ProductOwner,PFCD, GroupID
                                    pcsidItems2.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                    pcsidItems2.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                    //idItems.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                    pcsidItems2.Add(keyHost.PRODUCTSPECNAME + "_" + pcsProuctSpecs2);
                                    if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                    {
                                        if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                            pcsidItems2.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                    }
                                    pcsJob2.ProductID.SetItemData(pcsidItems2);
                                    if (!ObjectManager.JobManager.GetProductType(fabType, productList[i][keyHost.OWNERTYPE].InnerText, JobsforCut, pcsJob2, out err))
                                    {
                                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                        if (_timerManager.IsAliveTimer(timeoutName))
                                        {
                                            _timerManager.TerminateTimer(timeoutName);
                                        }
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                    }
                                    if (!ObjectManager.JobManager.GetProductID(port, JobsforCut, pcsJob2, out err))
                                    {
                                        string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                        if (_timerManager.IsAliveTimer(timeoutName))
                                        {
                                            _timerManager.TerminateTimer(timeoutName);
                                        }
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                    }
                                    job.CellSpecial.CUTProductType2 = pcsJob2.ProductType.Value.ToString();
                                    job.CellSpecial.ProductType2 = pcsJob2.ProductType;
                                    job.CellSpecial.CUTProductID2 = pcsJob2.ProductID.Value.ToString();
                                    job.CellSpecial.ProductID2 = pcsJob1.ProductID;
                                    #endregion
                                    ObjectManager.JobManager.DeleteJob(pcsJob2);
                                }
                                #endregion
                                ObjectManager.JobManager.DeleteJob(pcsJob1);
                                #endregion
                            }
                            if (line.Data.LINETYPE.Contains(keyCellLineType.CUT.ToString()))
                            {
                                #region [Cut Product Type & ID]
                                JobsforCut.Add(job);
                                List<string> cutitems = new List<string>();
                                List<string> cutidItems = new List<string>();
                                Job cutJob = new Job(0, 0);//
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID

                                cutitems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                cutitems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                //items.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                cutitems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.SUBPRODUCTSPECS].InnerText);
                                if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                {
                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                        cutitems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                cutJob.ProductType.SetItemData(cutitems);
                                //Product ID: ProductOwner,PFCD, GroupID

                                cutidItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                cutidItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                //idItems.Remove(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                cutidItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.SUBPRODUCTSPECS].InnerText);
                                if (!(string.IsNullOrEmpty(n[keyHost.PRODUCTOWNER].InnerText.Trim())))
                                {
                                    if (n[keyHost.PRODUCTOWNER].InnerText.Substring(n[keyHost.PRODUCTOWNER].InnerText.Length - 1, 1) == "E")//sy modify for MES 1.51
                                        cutidItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                cutJob.ProductID.SetItemData(cutidItems);
                                if (!ObjectManager.JobManager.GetProductType(fabType, productList[i][keyHost.OWNERTYPE].InnerText, JobsforCut, cutJob, out err))
                                {
                                    string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                    if (_timerManager.IsAliveTimer(timeoutName))
                                    {
                                        _timerManager.TerminateTimer(timeoutName);
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                                if (!ObjectManager.JobManager.GetProductID(port, JobsforCut, cutJob, out err))
                                {
                                    string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                    if (_timerManager.IsAliveTimer(timeoutName))
                                    {
                                        _timerManager.TerminateTimer(timeoutName);
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                                job.CellSpecial.CUTProductType = cutJob.ProductType.Value.ToString();
                                job.CellSpecial.ProductType1 = cutJob.ProductType;
                                job.CellSpecial.CUTProductID = cutJob.ProductID.Value.ToString();
                                job.CellSpecial.ProductID1 = cutJob.ProductID;
                                ObjectManager.JobManager.DeleteJob(cutJob);
                                #endregion
                            }
                        }
                        #endregion

                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                        {
                            job.SamplingSlotFlag = "1"; //modify for standby plan need 2015/10/05 cc.kuang
                            /* cc.kuang 2015/10/05
                            // 資料來源由 PlanManager
                            IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(line.File.CurrentPlanID, port.File.CassetteID.Trim());
                            SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == job.GlassChipMaskBlockID.Trim());
                            if (plan != null)
                            {
                                job.TargetCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                // 跟登今確認, 在Changer Mode只要有Plan就是要抽, 即使MES 的原資料是不抽, 也要turn On
                                if (!string.IsNullOrEmpty(job.TargetCSTID.Trim()))
                                {
                                    job.SamplingSlotFlag = "1";
                                }
                                else
                                {
                                    job.SamplingSlotFlag = "0";
                                }
                            }
                            */
                        }
                        #region  T3 SOR/CHN 机台记录SamplingSlotFlag by zhuxingxing 20161019
                        if (line.Data.LINETYPE.Contains(eLineType.CELL.CCSOR) || line.Data.LINETYPE.Contains(eLineType.CELL.CCCHN))
                        {
                            bool has_Sortflag=false;

                            foreach (XmlNode ab in productList[i].SelectNodes("ABNORMALCODELIST/CODE"))
                            {
                                if (ab[keyHost.ABNORMALVALUE].InnerText == "SORTFLAG" && !string.IsNullOrEmpty(ab[keyHost.ABNORMALCODE].InnerText))
                                {
                                    has_Sortflag=true;
                                    break;
                                }
                            }


                            if(has_Sortflag)
                            {
                                int job_slt=Convert.ToInt32(job.FromSlotNo);
                                disProcessFlag = disProcessFlag.Substring(0, job_slt - 1) + "1" + disProcessFlag.Substring(job_slt, disProcessFlag.Length-job_slt);
                            }
                        }
                        #endregion
                        if (fabType == eFabType.CELL)//sy 更改順序  加入MODULE
                        {
                            #region CELL Recipe Data
                            if (line.Data.JOBDATALINETYPE != eJobDataLineType.CELL.CBCUT)
                            {
                                if (!ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                                job.PPID = ppid;
                                job.MES_PPID = mesppid;

                                // 相同的Recipe Name不用再處理一次
                                if (!mesJobs.Any(j => j.MesProduct.PRODUCTRECIPENAME.Trim().Equals(productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim())))
                                {
                                    string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;
                                    ObjectManager.JobManager.SaveLineRecipeToDB(line, productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim(),
                                        productList[i][keyHost.PPID].InnerText.Trim(), ip);
                                }
                            }
                            else
                            {
                                switch (line.Data.LINETYPE)
                                {
                                    case eLineType.CELL.CBCUT_1:
                                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_1(body, n, productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out crossPPID, out mesppid, out err))
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                            }
                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                        }
                                        job.PPID = ppid;
                                        job.CellSpecial.CrossLinePPID = crossPPID;
                                        job.MES_PPID = mesppid;
                                        break;

                                    case eLineType.CELL.CBCUT_2:
                                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_2(body, n, productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out crossPPID, out mesppid, out err))
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                            }
                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                        }
                                        job.PPID = ppid;
                                        job.CellSpecial.CrossLinePPID = crossPPID;
                                        job.MES_PPID = mesppid;
                                        break;

                                    case eLineType.CELL.CBCUT_3:
                                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_3(body, n, productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                            }
                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                        }
                                        job.PPID = ppid;
                                        job.MES_PPID = mesppid;
                                        break;
                                    default:
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                        }

                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }

                            }
                            #endregion
                        }
                        else if (fabType == eFabType.MODULE)
                        {
                            #region MODULE Recipe Data
                            if (!ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                            {
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                            }
                            job.PPID = ppid;
                            job.MES_PPID = mesppid;

                            if (!mesJobs.Any(j => j.MesProduct.PRODUCTRECIPENAME.Trim().Equals(productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim())))
                            {
                                string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;
                                ObjectManager.JobManager.SaveLineRecipeToDB(line, productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim(),
                                    productList[i][keyHost.PPID].InnerText.Trim(), ip);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 取得Slot Recipe Data
                            if (!ObjectManager.JobManager.AnalysisMesPPID_AC(productList[i], line, port, productProcessType, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                            {
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                            }

                            //check PPID after ppid create 2016/05/27 cc.kuang
                            if (fabType == eFabType.ARRAY && line.Data.LINETYPE != eLineType.ARRAY.CAC_MYTEK &&
                                (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE && line.File.LineOperMode != eMES_LINEOPERMODE.EXCHANGE) &&
                                (port.File.Type == ePortType.BothPort || port.File.Type == ePortType.LoadingPort))
                            {
                                if(ppid.Trim().Trim('0').Length < 3)
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "PPID is Ivalid");
                                }
                            }

                            job.PPID = ppid;
                            job.MES_PPID = mesppid;

                            // 相同的Recipe Name不用再處理一次
                            if (!mesJobs.Any(j => j.MesProduct.PRODUCTRECIPENAME.Trim().Equals(productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim())))
                            {
                                string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;
                                ObjectManager.JobManager.SaveLineRecipeToDB(line, productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim(),
                                    productList[i][keyHost.PPID].InnerText.Trim(), ip);
                            }
                            #endregion
                        }

                        #region 把MES Download的PPID重新組合後記錄
                        string recipe_Assembly_log = string.Format("Adjust PPID OK, MES PPID =[{0}], EQP PPID(Job Data) =[{1}] ", job.MES_PPID, job.PPID);
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            if (recipe_Assembly_log_tmp != recipe_Assembly_log)
                            {
                                recipe_Assembly_log_tmp = recipe_Assembly_log;
                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                            }                            
                        }
                        else
                            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                        #endregion

                        #region 各Shop Job Special Data
                        switch (fabType)
                        {
                            case eFabType.ARRAY: M2P_SpecialDataBy_Array(n, productList[i], line, port, ref recipeGroup, ref job); break;
                            case eFabType.CF: M2P_SpecialDataBy_CF(body, n, productList[i], mesOXR, line, ref job, port, cst, eqp); break;
                            case eFabType.CELL: M2P_SpecialDataBy_CELL(body, n, productList[i], i, mesOXR, line, port, nodeStack, ref job); break;
                            case eFabType.MODULE: M2P_SpecialDataBy_MODULE(body, n, productList[i], i, mesOXR, line, port, nodeStack, ref job); break;
                        }
                        #endregion

                        #region 將MES Data 存到Job Information
                        MESCstDataIntoJobObject(body, n, ref job, ref cst);
                        MESProductDataIntoJobObject(productList[i], ref job);
                        #endregion
                        ObjectManager.QtimeManager.ValidateQTimeSetting(job);


                
                         
                        mesJobs.Add(job);

                        //20151106 Modify  for [ Robot Offline Create RobotWIP ]
                        //Invoke(eServiceName.RobotCoreService, "CreateJobRobotWIPInfo", new object[] { eqp.Data.NODENO, job });
                        #region Robot Create WIP   Return Failed Message and Cassette Command Failed!
                        //Watson Modify 20151106 For 新增錯誤訊息及重大錯誤不能再下貨成功!!
                        string returnMsg = string.Empty;
                        object[] parameters = new object[] { eqp.Data.NODENO, job, returnMsg };
                        bool result = (bool)Invoke(eServiceName.RobotCoreService, "CreateJobRobotWIPInfo", parameters,
                                new Type[] { typeof(string), typeof(Job), typeof(string).MakeByRefType() });
                        returnMsg = (string)parameters[2];

                        //由BC決定要不要退卡匣，
                        if ((!result) && (returnMsg != string.Empty))
                        {
                            err = string.Format(" CASSETTEID=[{0}] ROBOT ROUTE CREATE FAILED !!  REASON=[{1}]", port.File.CassetteID, returnMsg);
                            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                            if (ParameterManager[eRobot_Check_ByPass.ROBOT_CHECK_ROUTE_BYPASS].GetBoolean())
                            {
                                lock (cst)
                                {
                                    cst.ReasonCode = err;
                                    cst.ReasonText = ERR_CST_MAP.ROBOT_ROUTE_CREATER_FAILED + err;
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, returnMsg);
                            }
                        }
                        #endregion 

                    }
                    #endregion

                    //20171222 huangjiayin 移到所有Job生成完成，只Check 1次
                    Job portJob = mesJobs.Count > 0 ? mesJobs[0] : null;
                    #region[PILProductPairCheckSpecial]
                    // Add By Yangzhenteng 2017/12/21 For PIL Line Special
                    bool PILPairProuctCheckFlag = ParameterManager.ContainsKey("PILPairProuctCheckFlag")?ParameterManager["PILPairProuctCheckFlag"].GetBoolean():false;                 
                    if (line.Data.JOBDATALINETYPE == eJobDataLineType.CELL.CCPIL && eqp.Data.NODENO == "L2" && portJob != null
                        && (portJob.JobType == eJobType.TFT || portJob.JobType == eJobType.CF) && portJob.OWNERTYPE == "P" && PILPairProuctCheckFlag)
                    {
                        string errMsg = string.Empty;
                        string[] eqpList = { "L2", "L3", "L4", "L5", "L6" };
                        List<Job> PILCheckJobs = ObjectManager.JobManager.GetJobsbyEQPList(eqpList.ToList<string>());
                        string _sourcePairproduct = string.Empty;
                        if (portJob.JobType == eJobType.TFT)
                        { _sourcePairproduct = portJob.MesCstBody.LOTLIST[0].PRODUCTSPECNAME + "_" + portJob.MesCstBody.LOTLIST[0].PAIRPRODUCTSPECNAME; }
                        else
                        { _sourcePairproduct = portJob.MesCstBody.LOTLIST[0].PAIRPRODUCTSPECNAME + "_" + portJob.MesCstBody.LOTLIST[0].PRODUCTSPECNAME; }
                        if (PILCheckJobs.Count>0)
                        {
                            Dictionary<string, List<string>> job_pairproducts = new Dictionary<string, List<string>>();
                            foreach (Job j in PILCheckJobs)
                            {
                                if (j.OWNERTYPE != "P") continue;
                                if (j.JobType != eJobType.TFT && j.JobType != eJobType.CF) continue;
                                string _key = string.Empty;
                                string _value = string.Empty;
                                if (j.JobType == eJobType.TFT)
                                { _key = j.MesCstBody.LOTLIST[0].PRODUCTSPECNAME + "_" + j.MesCstBody.LOTLIST[0].PAIRPRODUCTSPECNAME; }
                                else
                                { _key = j.MesCstBody.LOTLIST[0].PAIRPRODUCTSPECNAME + "_" + j.MesCstBody.LOTLIST[0].PRODUCTSPECNAME; }
                                _value = j.JobKey + "_" + j.GlassChipMaskBlockID;

                                lock (job_pairproducts)
                                {
                                    if (!job_pairproducts.ContainsKey(_key))
                                    {
                                        job_pairproducts.Add(_key, new List<string>());
                                        job_pairproducts[_key].Add(_value);
                                    }
                                    else
                                    {
                                        job_pairproducts[_key].Add(_value);
                                    }
                                }
                            }
                            if (job_pairproducts.Count > 0)
                            {
                                List<string> _keys = job_pairproducts.Keys.ToList<string>();
                                foreach (string _k in _keys)
                                {
                                    if (_sourcePairproduct == _k) continue;
                                    errMsg = string.Format("CST[{0}] Pair ProductSpec[{1}] Check Error!\r\nLoader->PIC Remains Other Product as Below:\r\n", cst.CassetteID, _sourcePairproduct);

                                    lock (job_pairproducts)
                                    {
                                        List<string> errJobList = job_pairproducts[_k];
                                        errMsg += _k + ":\t";
                                        foreach (string s in errJobList)
                                        {
                                            errMsg += s + ";";
                                        }
                                        errMsg += "\r\n";
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, errMsg);
                        }
                    }
                    #endregion

                    //20171222 huangjiayin Uv Mask 上Port Check
                    #region[t3ODFUVMaskCheckOnPortSpecial]
                    bool ODFUVMaskCheckFlag = ParameterManager.ContainsKey("ODFUVMaskCheckFlag") ? ParameterManager["ODFUVMaskCheckFlag"].GetBoolean() : false;    
                    if (line.Data.JOBDATALINETYPE==eJobDataLineType.CELL.CCODF&&portJob != null&&ODFUVMaskCheckFlag)
                    {
                        if (
                            eqp.Data.NODENO=="L2"
                            &&portJob.JobType==eJobType.TFT
                            &&portJob.MesProduct.OWNERID!="DUMY"
                            )
                        {
                            //TFT非DUMY产品UVMASK列表为空时，上Port NG
                            if (string.IsNullOrEmpty(portJob.CellSpecial.UVMaskNames))
                            {
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, string.Format("CST[{0}] PFCD[{1}] UVMASKNAMES is Empty!",cst.CassetteID,portJob.MesCstBody.LOTLIST[0].PRODUCTSPECNAME));
                             }
                        }
                    }
                    #endregion
                    //20180116 Add By Yangzhenteng For Cell SOR&RWT&CHN Producttype Check
                    #region[T3 Cell SOR & RWT Special]
                    if ((line.Data.LINETYPE == eLineType.CELL.CCRWT ||
                         line.Data.LINETYPE == eLineType.CELL.CCSOR ||
                        line.Data.LINETYPE == eLineType.CELL.CCCHN ) //Modify By Yangzhenteng For Cell CHN
                        && eqp.Data.NODENO == "L2" && portJob != null
                        && port.File.Type == ePortType.LoadingPort)
                    {
                        string ErrMessage = string.Empty;
                        string SourceJobProductType = string.Empty;
                        string PairJobProductType = string.Empty;
                        List<Port> UnloadingCheckPorts = ObjectManager.PortManager.GetPorts().Where(P => P.File.Type == ePortType.UnloadingPort && P.File.ProductType != "0").ToList();
                        if (UnloadingCheckPorts.Count > 0)
                        {
                            Port UNloadingCheckPort = UnloadingCheckPorts.FirstOrDefault();
                            SourceJobProductType = UNloadingCheckPort.File.ProductType;
                            PairJobProductType = portJob.ProductType.Value.ToString();
                            if (SourceJobProductType != PairJobProductType)
                            {
                                ErrMessage = string.Format("CST[{0}] Job ProductType[{1}] IS Mismatch With  The Line Remains ProductType[{2}]", cst.CassetteID, PairJobProductType, SourceJobProductType);
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, ErrMessage);
                            }
                        }
                        else
                        {
                            List<Port> LoadingCheckPorts = ObjectManager.PortManager.GetPorts().Where(P => P.File.Type == ePortType.LoadingPort && P.Data.PORTID != port.Data.PORTID && (P.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || P.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)).ToList<Port>();
                            foreach (Port p in LoadingCheckPorts)
                            {
                                Job LoadingPortCheckJob = ObjectManager.JobManager.GetJobs(p.File.CassetteSequenceNo).FirstOrDefault();
                                SourceJobProductType = LoadingPortCheckJob.ProductType.Value.ToString();
                                PairJobProductType = portJob.ProductType.Value.ToString();
                                if (SourceJobProductType != PairJobProductType)
                                {
                                    ErrMessage = string.Format("CST[{0}] Job ProductType[{1}] IS Mismatch With  The Line Remains ProductType[{2}]", cst.CassetteID, PairJobProductType, SourceJobProductType);
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, ErrMessage);
                                }
                            }
                        }
                    }
    #endregion
                    //T3 CUT 要下報廢list 給機台//sy搬到 OXrequest
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT.ToString()))
                    {
                        #region [DISCARDJUDGES]
                        //if (n[keyHost.DISCARDJUDGES] != null)
                        //{
                        //    if (n[keyHost.DISCARDJUDGES].InnerText != "X" & n[keyHost.DISCARDJUDGES].InnerText != "")//預設同機台預設值 就不報
                        //    {
                        //        string disCardJudges = new string('0', 32);
                        //        //0:A 25:Z 23:X 不管有沒有給X 要下給機台都要有X
                        //        disCardJudges = disCardJudges.Substring(0, 23) + "1" + disCardJudges.Substring(23 + 1, disCardJudges.Length - 1 -23);
                        //        foreach (string disCardJudge in n[keyHost.DISCARDJUDGES].InnerText.Split(';'))
                        //        {
                        //            disCardJudgesUpdate(ref disCardJudges, disCardJudge);//updata EX: 00000110001110011
                        //        }
                        //        // ScrapRuleCommand(string eqpNo, eBitResult value, string trackKey, string CassetteSequenceNo, string disCardJudges)
                        //        // 只會對L3 下Command
                        //        Invoke(eServiceName.CELLSpecialService, "ScrapRuleCommand", new object[] { "L3", eBitResult.ON, trxID, port.File.CassetteSequenceNo, disCardJudges });
                        //    }
                        //}
                        #endregion
                    }
                }

                //CF FCUPK Line 資料要由BCS產生
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                {
                    if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE && mesJobs.Count() == 0)
                    {
                        idCheckInfos = new List<RecipeCheckInfo>();
                        paraCheckInfos = new List<RecipeCheckInfo>();
                        ObjectManager.JobManager.UPK_CREATE_JOBDataFile(xmlDoc, eqp, port, line, ref idCheckInfos, ref paraCheckInfos, ref mesJobs);
                    }
                }
                //20160721 Add by Frank for FCUPK Unloader Product Type
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 && port.File.Type == ePortType.UnloadingPort)
                {
                    List<string> items = new List<string>();
                    //items.Add(keyHost.PLANNEDPRODUCTSPECNAME + "_" + body[keyHost.PLANNEDPRODUCTSPECNAME].InnerText);
                    //items.Add(keyHost.PLANNEDPROCESSOPERATIONNAME + "_" + body[keyHost.PLANNEDPROCESSOPERATIONNAME].InnerText);
                    //items.Add(keyHost.UPKOWNERTYPE + "_" + body[keyHost.UPKOWNERTYPE].InnerText);
                    items.Add(keyHost.PLANNEDSOURCEPART + "_" + body[keyHost.PLANNEDSOURCEPART].InnerText);
                    items.Add(keyHost.PLANNEDPCPLANID + "_" + body[keyHost.PLANNEDPCPLANID].InnerText);// qiumin 20170324 PRODUCT type create add planID
                    Job j = new Job();
                    j.ProductType.SetItemData(items);               
                    IList<Job> tmpJobs = new List<Job>();                    

                    if (!ObjectManager.JobManager.GetProductType(eFabType.CF, "", tmpJobs, j, out err))
                    {
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }

                    port.File.ProductType = j.ProductType.Value.ToString();
                    tmpJobs.Add(j);
                    ObjectManager.JobManager.DeleteJobs(tmpJobs);
 
                }

                #region R2R 逻辑判断
                //modify by hujunpeng 20181110
                if (fabType == eFabType.ARRAY)
                {
                    if (ParameterManager.ContainsKey("R2RTIMEOUT"))
                    {
                        if (ParameterManager["R2RTIMEOUT"].GetInteger() != 0 && line.File.HostMode == eHostMode.REMOTE)
                        {
                            if (eqp.Data.LINEID.Contains("TCPHL"))
                            {
                                R2RRecipeReport(xmlDoc, line, eqp, port, cst, mesJobs, trxID);
                                Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L4");
                                while (new TimeSpan(DateTime.Now.Ticks - eqp1.File.R2RRecipeReportStartDT.Ticks).TotalMilliseconds < ParameterManager["VALIDATETIMEOUT"].GetInteger())
                                {
                                    Thread.Sleep(300);//阻塞呼叫的Function
                                    if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                    {
                                        err = string.Format(" R2R Recipe Report Reply TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                            lineName, port.Data.PORTID, port.File.CassetteID);
                                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                        cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                        cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                        return;
                                    }
                                    if (!string.IsNullOrEmpty(eqp1.File.R2RRecipeReportReplyReturnCode))
                                    {
                                        if (eqp1.File.R2RRecipeReportReplyReturnCode.Equals("0"))
                                        {
                                            while (new TimeSpan(DateTime.Now.Ticks - eqp1.File.R2REQParameterDownloadDT.Ticks).TotalMilliseconds < ParameterManager["NIKONSECSDATATIMEOUT"].GetInteger())
                                            {
                                                Thread.Sleep(300);//阻塞呼叫的Function
                                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                                {
                                                    err = string.Format(" EQ Reply TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                                    return;
                                                }
                                                if (eqp1.File.IsReveive)
                                                {
                                                    if (!string.IsNullOrEmpty(eqp1.File.R2REQParameterDownloadRetrunCode))
                                                    {
                                                        if (eqp1.File.R2REQParameterDownloadRetrunCode == "0")
                                                        {
                                                            #region R2R FLOW
                                                            // 20141231 福杰說 Unloader 上CST不需要檢查Recipe
                                                            if (port.File.Type != ePortType.UnloadingPort)
                                                            {
                                                                #region Check Recipe ID
                                                                if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_ID].GetBoolean()) ||
                                                                    (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_ID].GetBoolean()))
                                                                {
                                                                    if (idCheckInfos.Count() > 0)
                                                                    {
                                                                        if (recipeIDCheckData.ContainsKey(line.Data.LINEID))
                                                                        {
                                                                            List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;
                                                                            for (int i = 0; i < idCheckInfos.Count(); i++)
                                                                            {
                                                                                // 過濾掉重覆的部份
                                                                                if (rci.Any(r => r.EQPNo == idCheckInfos[i].EQPNo && r.RecipeID == idCheckInfos[i].RecipeID)) continue;
                                                                                rci.AddRange(idCheckInfos);
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            recipeIDCheckData.Add(line.Data.LINEID, idCheckInfos);
                                                                        }
                                                                    }

                                                                    // Check Recipe ID 
                                                                    if (recipeIDCheckData.Count() > 0)
                                                                    {
                                                                        Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeIDCheckData, new List<string>() });

                                                                        //檢查完畢, 返回值
                                                                        bool _NGResult = false;
                                                                        bool _ForceOKResult = false;
                                                                        foreach (string key in recipeIDCheckData.Keys)
                                                                        {
                                                                            IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                                                            string log = string.Format("Line Name=[{0}) Recipe ID Check", key);
                                                                            string log2 = string.Empty;

                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                                                                    recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                                                {
                                                                                    log2 += string.Format(", EQID=[{0}({1})] RecipeID=[{2}] Result=[{3}]", recipeIDList[i].EqpID, recipeIDList[i].EQPNo, recipeIDList[i].RecipeID, recipeIDList[i].Result.ToString());
                                                                                    _NGResult = true;
                                                                                }
                                                                            }
                                                                            if (!string.IsNullOrEmpty(log2))
                                                                            {
                                                                                err = string.IsNullOrEmpty(err) ? "" : err += ";";
                                                                                err += log + log2;
                                                                            }

                                                                            #region add by bruce 20160321 for Array Special Eq 只要有一個機台回覆OK就不退CST
                                                                            switch (line.Data.LINEID)
                                                                            {
                                                                                case "TCATS200":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS200))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCATS400":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS400))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCTEG200":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG200))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCTEG400":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG400))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCFLR200":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR200))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCFLR300":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR300))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCELA100":

                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA100))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCELA300":

                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA300))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCELA200":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA200))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCAOH800":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH800))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCAOH400":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH400))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH400].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCAOH300":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH300))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH300].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCAOH900":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH900))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH900].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCCDO400":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO400))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                case "TCCDO300":
                                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO300))
                                                                                    {
                                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO300].GetString().Split(',');
                                                                                        foreach (string bypasseqp in byPasseqps)
                                                                                        {
                                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                                            {
                                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                                {
                                                                                                    _ForceOKResult = true;
                                                                                                    _NGResult = false;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (_ForceOKResult) break;
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                            }
                                                                            #endregion
                                                                        }
                                                                        if (_NGResult)
                                                                        {
                                                                            lock (cst)
                                                                            {
                                                                                cst.ReasonCode = reasonCode;
                                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG;
                                                                            }
                                                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                                                        }
                                                                    }
                                                                }
                                                                #endregion

                                                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                                                {
                                                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                                                    return;
                                                                }
                                                                #region Check Parameter
                                                                if (recipePara && port.File.Type != ePortType.UnloadingPort)
                                                                {
                                                                    if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_PARAMETER].GetBoolean()) ||
                                                                        (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_PARAMETER].GetBoolean()))
                                                                    {
                                                                        XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                                                                        for (int i = 0; i < recipeNoCheckList.Count; i++)
                                                                        {
                                                                            lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                                                                        }
                                                                        //Watson 20141126 DB Setting Recipe Parameter Check Enable/Disable 在RecipeService :RecipeParameterRequestCommand裏加入

                                                                        if (paraCheckInfos.Count() > 0)
                                                                        {
                                                                            if (recipeParaCheckData.ContainsKey(line.Data.LINEID))
                                                                            {
                                                                                List<RecipeCheckInfo> rcp = recipeParaCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;

                                                                                for (int i = 0; i < paraCheckInfos.Count(); i++)
                                                                                {
                                                                                    // 過濾掉重覆的部份
                                                                                    if (rcp.Any(r => r.EQPNo == paraCheckInfos[i].EQPNo && r.RecipeID == paraCheckInfos[i].RecipeID)) continue;
                                                                                    rcp.AddRange(paraCheckInfos);
                                                                                }
                                                                                rcp.AddRange(paraCheckInfos);
                                                                            }
                                                                            else
                                                                            {
                                                                                recipeParaCheckData.Add(line.Data.LINEID, paraCheckInfos);
                                                                            }
                                                                        }

                                                                        if (recipeParaCheckData.Count > 0)
                                                                        {
                                                                            object[] obj = new object[]
                            {
                                trxID,
                                recipeParaCheckData,
                                lstNoCehckEQ,
                                port.Data.PORTID,
                                port.File.CassetteID,
                            };
                                                                            eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                                                                            //檢查完畢, 返回值
                                                                            if (retCode != eRecipeCheckResult.OK)
                                                                            {
                                                                                err = "[MES] Check Recipe Parameter Reply NG: ";
                                                                                foreach (string key in recipeParaCheckData.Keys)
                                                                                {
                                                                                    IList<RecipeCheckInfo> recipeParaList = recipeParaCheckData[key];
                                                                                    string log = string.Format("Line Name=[{0}) Recipe Parameter Check", key);
                                                                                    string log2 = string.Empty;

                                                                                    for (int i = 0; i < recipeParaList.Count; i++)
                                                                                    {
                                                                                        if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                                                                            recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                                                        {
                                                                                            log2 += string.Format(", EQID=[{0}({1})], Result=[{2}]", recipeParaList[i].EqpID, recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                                                                        }
                                                                                    }
                                                                                    if (!string.IsNullOrEmpty(log2))
                                                                                    {
                                                                                        //err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                                                                        err += log + log2;
                                                                                    }
                                                                                }
                                                                                if (string.IsNullOrEmpty(cst.ReasonCode))
                                                                                {
                                                                                    lock (cst)
                                                                                    {
                                                                                        cst.ReasonCode = reasonCode;
                                                                                        cst.ReasonText = ERR_CST_MAP.RECIPE_PARAMATER_VALIDATION_NG;
                                                                                    }
                                                                                }
                                                                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                #endregion
                                                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                                                {
                                                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                                                    return;
                                                                }
                                                            }
                                                            //將資料放入JobManager
                                                            if (mesJobs.Count() > 0)
                                                            {
                                                                ObjectManager.JobManager.AddJobs(mesJobs);
                                                                ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20141225 Tom  Add Job History

                                                                //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                                                                //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                                                                //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, mesJobs);
                                                            }

                                                            //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
                                                            port.File.RobotWaitProcCount = mesJobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

                                                            if (fabType == eFabType.ARRAY && recipeGroup.Count > 30) //add  cc.kuang 2016/03/09
                                                            {
                                                                lock (cst)
                                                                {
                                                                    cst.ReasonCode = reasonCode;
                                                                    cst.ReasonText = MES_ReasonText.MES_Download_Recipe_Group_Count_Error;
                                                                }
                                                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "Recipe Group Count > 30, Pls Check the jobs on CST");
                                                            }
                                                            #region  [当机台的Mode 为RANDOMMODE的时候下Command 给机台 20161020 zhuxingxing]
                                                            //fix by huangjiayin add port type check
                                                            if (line.Data.LINETYPE.Contains(eLineType.CELL.CCSOR) || line.Data.LINETYPE.Contains(eLineType.CELL.CCCHN))
                                                            {
                                                                if (port.File.Type == ePortType.LoadingPort)
                                                                {

                                                                    if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT" || eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN")
                                                                    {
                                                                        if (disProcessFlag.Equals(new string('0', 600)))    //Random Mode Loading Port必须要求SortFlag，不然会默停
                                                                        {
                                                                            lock (cst)
                                                                            {
                                                                                cst.ReasonCode = reasonCode;
                                                                                cst.ReasonText = ERR_CST_MAP.CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR;
                                                                            }
                                                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "eqp &cst body are both random, but no product has sortflag!");

                                                                        }

                                                                        Invoke(eServiceName.CELLSpecialService, "SamplingFlagCommand", new object[] { trxID, lineName, disProcessFlag, eqp.Data.NODENO, eBitResult.ON, cst.CassetteSequenceNo });
                                                                    }
                                                                }
                                                            }
                                                            #endregion


                                                            // Download to PLC
                                                            Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxID });

                                                            #region Check MplcInterlock
                                                            //Check Mplcinterlock On By Port
                                                            if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort && port.File.Mode != ePortMode.Dummy)
                                                            {
                                                                string productType = "";
                                                                for (int i = 0; i < mesJobs.Count(); i++)
                                                                {
                                                                    productType = mesJobs[i].ProductType.Value.ToString();
                                                                    if (productType != "")
                                                                    {
                                                                        break;
                                                                    }
                                                                }
                                                                if (productType != "")
                                                                {
                                                                    string portNo = "P" + port.Data.PORTNO;
                                                                    Invoke(eServiceName.SubBlockService, "CheckMplcBlockByPort", new object[] { eqp.Data.NODENO, portNo, productType });
                                                                }
                                                            }
                                                            #endregion

                                                            #region Create FTP File
                                                            if (port.File.Type != ePortType.UnloadingPort)
                                                            {
                                                                ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                                                                string Path = @"D:\FileData\";
                                                                string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                                                                DateTime now = DateTime.Now;
                                                                int iOverTime = 10;
                                                                //for t2,t3 sync 2016/04/25 cc.kuang
                                                                if (para.ContainsKey("FileFormatPath") && para["FileFormatPath"].Value != null)
                                                                {
                                                                    Path = para["FileFormatPath"].GetString();
                                                                }

                                                                if (para.ContainsKey("FileFormatOverTimer") && para["FileFormatOverTimer"].Value != null)
                                                                {
                                                                    iOverTime = para["FileFormatOverTimer"].GetInteger();
                                                                }

                                                                if (Directory.Exists(Path + subPath))
                                                                {
                                                                    string[] fileEntries = Directory.GetFiles(Path + subPath);

                                                                    foreach (string fileName in fileEntries)
                                                                    {
                                                                        DateTime dt = Directory.GetLastWriteTime(fileName);
                                                                        if (dt.AddDays(iOverTime).CompareTo(now) < 0)
                                                                        {
                                                                            if (File.Exists(fileName))
                                                                            {
                                                                                File.Delete(fileName);
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                for (int i = 0; i < mesJobs.Count(); i++)
                                                                {
                                                                    if (fabType != eFabType.CELL)
                                                                        FileFormatManager.CreateFormatFile("ACShop", subPath, mesJobs[i], true);
                                                                    else
                                                                    {
                                                                        switch (line.Data.LINETYPE)
                                                                        {
                                                                            #region [T2]
                                                                            case eLineType.CELL.CBPIL:
                                                                            case eLineType.CELL.CBODF:
                                                                            case eLineType.CELL.CBHVA:
                                                                            case eLineType.CELL.CBPMT:
                                                                            case eLineType.CELL.CBGAP:
                                                                            case eLineType.CELL.CBUVA:
                                                                            case eLineType.CELL.CBMCL:
                                                                            case eLineType.CELL.CBATS:
                                                                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                                                                break;

                                                                            case eLineType.CELL.CBLOI: // LOI 线要产生两种类型的File Data  20150313 Tom
                                                                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                                break;
                                                                            case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                                                            case eLineType.CELL.CBCUT_2:
                                                                            case eLineType.CELL.CBCUT_3:
                                                                                if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                                                                    port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                                                                {
                                                                                    //break;
                                                                                }
                                                                                else if (port.Data.PORTID == "07" || port.Data.PORTID == "08" ||
                                                                                         port.Data.PORTID == "C07" || port.Data.PORTID == "C08" ||
                                                                                         port.Data.PORTID == "P01" || port.Data.PORTID == "P02" ||
                                                                                         port.Data.PORTID == "P03" || port.Data.PORTID == "P04" || port.Data.PORTID == "P06")
                                                                                {
                                                                                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                                                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                                }
                                                                                break;
                                                                            #endregion
                                                                            //case eLineType.CELL.CCPIL:
                                                                            //case eLineType.CELL.CCODF:
                                                                            //case eLineType.CELL.CCPDR:
                                                                            //case eLineType.CELL.CCTAM:
                                                                            //case eLineType.CELL.CCPTH:
                                                                            //case eLineType.CELL.CCGAP:
                                                                            //    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                                                            //    break;
                                                                            default://T3 使用default 集中管理 sy
                                                                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, mesJobs[i], port });
                                                                                //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                                break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            #endregion

                                                            #region CF Exposure Special Rule
                                                            if (port.File.Type == ePortType.LoadingPort)
                                                            {
                                                                if ((line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                                                                    (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) ||
                                                                    (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1))
                                                                {
                                                                    object[] _object = new object[]
                    {
                        trxID,
                        lineName,
                        recipeIDCheckData
                    };
                                                                    Invoke(eServiceName.CFSpecialService, "AnalysisExposuePPID", _object);
                                                                }
                                                            }
                                                            #endregion
                                                            #endregion
                                                            lock (eqp1.File)
                                                            {
                                                                eqp1.File.IsReveive = false;
                                                                eqp1.File.R2REQParameterDownloadRetrunCode = string.Empty;
                                                                eqp1.File.R2REQParameterDownloadDT = DateTime.Now;
                                                                eqp1.File.R2RRecipeReportReplyReturnCode = string.Empty;
                                                                eqp1.File.R2RRecipeReportStartDT = DateTime.Now;
                                                            }
                                                            ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);                                                      
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, port.Data.LINEID, "R2R Parameter Download, EQ Reply NG Cancel CST" });
                                                            Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                                            break;
                                                        }
                                                    }
                                                }
                                            }  
                                        }
                                        else if (eqp1.File.R2RRecipeReportReplyReturnCode.Equals("1"))
                                        {
                                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, port.Data.LINEID, "R2R Parameter Reply NG!" });
                                            Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                                        }
                                        else
                                        {
                                            #region R2R FLOW
                                            // 20141231 福杰說 Unloader 上CST不需要檢查Recipe
                                            if (port.File.Type != ePortType.UnloadingPort)
                                            {
                                                #region Check Recipe ID
                                                if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_ID].GetBoolean()) ||
                                                    (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_ID].GetBoolean()))
                                                {
                                                    if (idCheckInfos.Count() > 0)
                                                    {
                                                        if (recipeIDCheckData.ContainsKey(line.Data.LINEID))
                                                        {
                                                            List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;
                                                            for (int i = 0; i < idCheckInfos.Count(); i++)
                                                            {
                                                                // 過濾掉重覆的部份
                                                                if (rci.Any(r => r.EQPNo == idCheckInfos[i].EQPNo && r.RecipeID == idCheckInfos[i].RecipeID)) continue;
                                                                rci.AddRange(idCheckInfos);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            recipeIDCheckData.Add(line.Data.LINEID, idCheckInfos);
                                                        }
                                                    }

                                                    // Check Recipe ID 
                                                    if (recipeIDCheckData.Count() > 0)
                                                    {
                                                        Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeIDCheckData, new List<string>() });

                                                        //檢查完畢, 返回值
                                                        bool _NGResult = false;
                                                        bool _ForceOKResult = false;
                                                        foreach (string key in recipeIDCheckData.Keys)
                                                        {
                                                            IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                                            string log = string.Format("Line Name=[{0}) Recipe ID Check", key);
                                                            string log2 = string.Empty;

                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                                                    recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                                {
                                                                    log2 += string.Format(", EQID=[{0}({1})] RecipeID=[{2}] Result=[{3}]", recipeIDList[i].EqpID, recipeIDList[i].EQPNo, recipeIDList[i].RecipeID, recipeIDList[i].Result.ToString());
                                                                    _NGResult = true;
                                                                }
                                                            }
                                                            if (!string.IsNullOrEmpty(log2))
                                                            {
                                                                err = string.IsNullOrEmpty(err) ? "" : err += ";";
                                                                err += log + log2;
                                                            }

                                                            #region add by bruce 20160321 for Array Special Eq 只要有一個機台回覆OK就不退CST
                                                            switch (line.Data.LINEID)
                                                            {
                                                                case "TCATS200":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS200))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCATS400":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS400))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCTEG200":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG200))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCTEG400":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG400))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCFLR200":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR200))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCFLR300":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR300))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCELA100":

                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA100))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCELA300":

                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA300))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCELA200":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA200))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCAOH800":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH800))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCAOH400":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH400))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH400].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCAOH300":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH300))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH300].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCAOH900":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH900))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH900].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCCDO400":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO400))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "TCCDO300":
                                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO300))
                                                                    {
                                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO300].GetString().Split(',');
                                                                        foreach (string bypasseqp in byPasseqps)
                                                                        {
                                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                                            {
                                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                                {
                                                                                    _ForceOKResult = true;
                                                                                    _NGResult = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (_ForceOKResult) break;
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                            #endregion
                                                        }
                                                        if (_NGResult)
                                                        {
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG;
                                                            }
                                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                                        }
                                                    }
                                                }
                                                #endregion

                                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                                {
                                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                                    return;
                                                }
                                                #region Check Parameter
                                                if (recipePara && port.File.Type != ePortType.UnloadingPort)
                                                {
                                                    if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_PARAMETER].GetBoolean()) ||
                                                        (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_PARAMETER].GetBoolean()))
                                                    {
                                                        XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                                                        for (int i = 0; i < recipeNoCheckList.Count; i++)
                                                        {
                                                            lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                                                        }
                                                        //Watson 20141126 DB Setting Recipe Parameter Check Enable/Disable 在RecipeService :RecipeParameterRequestCommand裏加入

                                                        if (paraCheckInfos.Count() > 0)
                                                        {
                                                            if (recipeParaCheckData.ContainsKey(line.Data.LINEID))
                                                            {
                                                                List<RecipeCheckInfo> rcp = recipeParaCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;

                                                                for (int i = 0; i < paraCheckInfos.Count(); i++)
                                                                {
                                                                    // 過濾掉重覆的部份
                                                                    if (rcp.Any(r => r.EQPNo == paraCheckInfos[i].EQPNo && r.RecipeID == paraCheckInfos[i].RecipeID)) continue;
                                                                    rcp.AddRange(paraCheckInfos);
                                                                }
                                                                rcp.AddRange(paraCheckInfos);
                                                            }
                                                            else
                                                            {
                                                                recipeParaCheckData.Add(line.Data.LINEID, paraCheckInfos);
                                                            }
                                                        }

                                                        if (recipeParaCheckData.Count > 0)
                                                        {
                                                            object[] obj = new object[]
                            {
                                trxID,
                                recipeParaCheckData,
                                lstNoCehckEQ,
                                port.Data.PORTID,
                                port.File.CassetteID,
                            };
                                                            eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                                                            //檢查完畢, 返回值
                                                            if (retCode != eRecipeCheckResult.OK)
                                                            {
                                                                err = "[MES] Check Recipe Parameter Reply NG: ";
                                                                foreach (string key in recipeParaCheckData.Keys)
                                                                {
                                                                    IList<RecipeCheckInfo> recipeParaList = recipeParaCheckData[key];
                                                                    string log = string.Format("Line Name=[{0}) Recipe Parameter Check", key);
                                                                    string log2 = string.Empty;

                                                                    for (int i = 0; i < recipeParaList.Count; i++)
                                                                    {
                                                                        if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                                                            recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                                        {
                                                                            log2 += string.Format(", EQID=[{0}({1})], Result=[{2}]", recipeParaList[i].EqpID, recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                                                        }
                                                                    }
                                                                    if (!string.IsNullOrEmpty(log2))
                                                                    {
                                                                        //err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                                                        err += log + log2;
                                                                    }
                                                                }
                                                                if (string.IsNullOrEmpty(cst.ReasonCode))
                                                                {
                                                                    lock (cst)
                                                                    {
                                                                        cst.ReasonCode = reasonCode;
                                                                        cst.ReasonText = ERR_CST_MAP.RECIPE_PARAMATER_VALIDATION_NG;
                                                                    }
                                                                }
                                                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion
                                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                                {
                                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                                    return;
                                                }
                                            }
                                            //將資料放入JobManager
                                            if (mesJobs.Count() > 0)
                                            {
                                                ObjectManager.JobManager.AddJobs(mesJobs);
                                                ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20141225 Tom  Add Job History

                                                //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                                                //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                                                //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, mesJobs);
                                            }

                                            //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
                                            port.File.RobotWaitProcCount = mesJobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

                                            if (fabType == eFabType.ARRAY && recipeGroup.Count > 30) //add  cc.kuang 2016/03/09
                                            {
                                                lock (cst)
                                                {
                                                    cst.ReasonCode = reasonCode;
                                                    cst.ReasonText = MES_ReasonText.MES_Download_Recipe_Group_Count_Error;
                                                }
                                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "Recipe Group Count > 30, Pls Check the jobs on CST");
                                            }
                                            #region  [当机台的Mode 为RANDOMMODE的时候下Command 给机台 20161020 zhuxingxing]
                                            //fix by huangjiayin add port type check
                                            if (line.Data.LINETYPE.Contains(eLineType.CELL.CCSOR) || line.Data.LINETYPE.Contains(eLineType.CELL.CCCHN))
                                            {
                                                if (port.File.Type == ePortType.LoadingPort)
                                                {

                                                    if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT" || eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN")
                                                    {
                                                        if (disProcessFlag.Equals(new string('0', 600)))    //Random Mode Loading Port必须要求SortFlag，不然会默停
                                                        {
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR;
                                                            }
                                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "eqp &cst body are both random, but no product has sortflag!");

                                                        }

                                                        Invoke(eServiceName.CELLSpecialService, "SamplingFlagCommand", new object[] { trxID, lineName, disProcessFlag, eqp.Data.NODENO, eBitResult.ON, cst.CassetteSequenceNo });
                                                    }
                                                }
                                            }
                                            #endregion


                                            // Download to PLC
                                            Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxID });

                                            #region Check MplcInterlock
                                            //Check Mplcinterlock On By Port
                                            if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort && port.File.Mode != ePortMode.Dummy)
                                            {
                                                string productType = "";
                                                for (int i = 0; i < mesJobs.Count(); i++)
                                                {
                                                    productType = mesJobs[i].ProductType.Value.ToString();
                                                    if (productType != "")
                                                    {
                                                        break;
                                                    }
                                                }
                                                if (productType != "")
                                                {
                                                    string portNo = "P" + port.Data.PORTNO;
                                                    Invoke(eServiceName.SubBlockService, "CheckMplcBlockByPort", new object[] { eqp.Data.NODENO, portNo, productType });
                                                }
                                            }
                                            #endregion

                                            #region Create FTP File
                                            if (port.File.Type != ePortType.UnloadingPort)
                                            {
                                                ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                                                string Path = @"D:\FileData\";
                                                string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                                                DateTime now = DateTime.Now;
                                                int iOverTime = 10;
                                                //for t2,t3 sync 2016/04/25 cc.kuang
                                                if (para.ContainsKey("FileFormatPath") && para["FileFormatPath"].Value != null)
                                                {
                                                    Path = para["FileFormatPath"].GetString();
                                                }

                                                if (para.ContainsKey("FileFormatOverTimer") && para["FileFormatOverTimer"].Value != null)
                                                {
                                                    iOverTime = para["FileFormatOverTimer"].GetInteger();
                                                }

                                                if (Directory.Exists(Path + subPath))
                                                {
                                                    string[] fileEntries = Directory.GetFiles(Path + subPath);

                                                    foreach (string fileName in fileEntries)
                                                    {
                                                        DateTime dt = Directory.GetLastWriteTime(fileName);
                                                        if (dt.AddDays(iOverTime).CompareTo(now) < 0)
                                                        {
                                                            if (File.Exists(fileName))
                                                            {
                                                                File.Delete(fileName);
                                                            }
                                                        }
                                                    }
                                                }

                                                for (int i = 0; i < mesJobs.Count(); i++)
                                                {
                                                    if (fabType != eFabType.CELL)
                                                        FileFormatManager.CreateFormatFile("ACShop", subPath, mesJobs[i], true);
                                                    else
                                                    {
                                                        switch (line.Data.LINETYPE)
                                                        {
                                                            #region [T2]
                                                            case eLineType.CELL.CBPIL:
                                                            case eLineType.CELL.CBODF:
                                                            case eLineType.CELL.CBHVA:
                                                            case eLineType.CELL.CBPMT:
                                                            case eLineType.CELL.CBGAP:
                                                            case eLineType.CELL.CBUVA:
                                                            case eLineType.CELL.CBMCL:
                                                            case eLineType.CELL.CBATS:
                                                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                                                break;

                                                            case eLineType.CELL.CBLOI: // LOI 线要产生两种类型的File Data  20150313 Tom
                                                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                break;
                                                            case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                                            case eLineType.CELL.CBCUT_2:
                                                            case eLineType.CELL.CBCUT_3:
                                                                if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                                                    port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                                                {
                                                                    //break;
                                                                }
                                                                else if (port.Data.PORTID == "07" || port.Data.PORTID == "08" ||
                                                                         port.Data.PORTID == "C07" || port.Data.PORTID == "C08" ||
                                                                         port.Data.PORTID == "P01" || port.Data.PORTID == "P02" ||
                                                                         port.Data.PORTID == "P03" || port.Data.PORTID == "P04" || port.Data.PORTID == "P06")
                                                                {
                                                                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                }
                                                                break;
                                                            #endregion
                                                            //case eLineType.CELL.CCPIL:
                                                            //case eLineType.CELL.CCODF:
                                                            //case eLineType.CELL.CCPDR:
                                                            //case eLineType.CELL.CCTAM:
                                                            //case eLineType.CELL.CCPTH:
                                                            //case eLineType.CELL.CCGAP:
                                                            //    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                                            //    break;
                                                            default://T3 使用default 集中管理 sy
                                                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, mesJobs[i], port });
                                                                //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region CF Exposure Special Rule
                                            if (port.File.Type == ePortType.LoadingPort)
                                            {
                                                if ((line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                                                    (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) ||
                                                    (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1))
                                                {
                                                    object[] _object = new object[]
                    {
                        trxID,
                        lineName,
                        recipeIDCheckData
                    };
                                                    Invoke(eServiceName.CFSpecialService, "AnalysisExposuePPID", _object);
                                                }
                                            }
                                            #endregion
                                            #endregion
                                            lock (eqp1.File)
                                            {
                                                eqp1.File.IsReveive = false;
                                                eqp1.File.R2REQParameterDownloadRetrunCode = string.Empty;
                                                eqp1.File.R2REQParameterDownloadDT = DateTime.Now;
                                                eqp1.File.R2RRecipeReportReplyReturnCode = string.Empty;
                                                eqp1.File.R2RRecipeReportStartDT = DateTime.Now;
                                            }
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region 之前flow
                            // 20141231 福杰說 Unloader 上CST不需要檢查Recipe
                            if (port.File.Type != ePortType.UnloadingPort)
                            {
                                #region Check Recipe ID
                                if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_ID].GetBoolean()) ||
                                    (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_ID].GetBoolean()))
                                {
                                    if (idCheckInfos.Count() > 0)
                                    {
                                        if (recipeIDCheckData.ContainsKey(line.Data.LINEID))
                                        {
                                            List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;
                                            for (int i = 0; i < idCheckInfos.Count(); i++)
                                            {
                                                // 過濾掉重覆的部份
                                                if (rci.Any(r => r.EQPNo == idCheckInfos[i].EQPNo && r.RecipeID == idCheckInfos[i].RecipeID)) continue;
                                                rci.AddRange(idCheckInfos);
                                            }
                                        }
                                        else
                                        {
                                            recipeIDCheckData.Add(line.Data.LINEID, idCheckInfos);
                                        }
                                    }

                                    // Check Recipe ID 
                                    if (recipeIDCheckData.Count() > 0)
                                    {
                                        Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeIDCheckData, new List<string>() });

                                        //檢查完畢, 返回值
                                        bool _NGResult = false;
                                        bool _ForceOKResult = false;
                                        foreach (string key in recipeIDCheckData.Keys)
                                        {
                                            IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                            string log = string.Format("Line Name=[{0}) Recipe ID Check", key);
                                            string log2 = string.Empty;

                                            for (int i = 0; i < recipeIDList.Count; i++)
                                            {
                                                if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                                    recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                {
                                                    log2 += string.Format(", EQID=[{0}({1})] RecipeID=[{2}] Result=[{3}]", recipeIDList[i].EqpID, recipeIDList[i].EQPNo, recipeIDList[i].RecipeID, recipeIDList[i].Result.ToString());
                                                    _NGResult = true;
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(log2))
                                            {
                                                err = string.IsNullOrEmpty(err) ? "" : err += ";";
                                                err += log + log2;
                                            }

                                            #region add by bruce 20160321 for Array Special Eq 只要有一個機台回覆OK就不退CST
                                            switch (line.Data.LINEID)
                                            {
                                                case "TCATS200":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS200))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCATS400":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS400))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCTEG200":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG200))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCTEG400":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG400))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCFLR200":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR200))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCFLR300":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR300))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCELA100":

                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA100))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCELA300":

                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA300))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCELA200":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA200))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCAOH800":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH800))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCAOH400":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH400))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH400].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCAOH300":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH300))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH300].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCAOH900":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH900))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH900].GetString().Split(',');
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            for (int i = 0; i < recipeIDList.Count; i++)
                                                            {
                                                                if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                                {
                                                                    _ForceOKResult = true;
                                                                    _NGResult = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCCDO400":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO400))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                                                        List<RecipeCheckInfo> query = recipeIDList.ToList<RecipeCheckInfo>();
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            //modify by hujunpeng 20190227 for 同一机台不同recipe都要check通过才可以下账，不然退port
                                                            List<RecipeCheckInfo> query1 = query.Where(r => r.EQPNo == bypasseqp).ToList<RecipeCheckInfo>();
                                                            query1.RemoveAll(r=>r.Result==eRecipeCheckResult.OK);
                                                            if (query1.Count == 0)
                                                            {
                                                                _ForceOKResult = true;
                                                                _NGResult = false;
                                                                break;
                                                            }
                                                            //for (int i = 0; i < recipeIDList.Count; i++)
                                                            //{
                                                            //    if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                            //    {
                                                            //        _ForceOKResult = true;
                                                            //        _NGResult = false;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                                case "TCCDO300":
                                                    if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO300))
                                                    {
                                                        string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO300].GetString().Split(',');
                                                        List<RecipeCheckInfo> query = recipeIDList.ToList<RecipeCheckInfo>();
                                                        foreach (string bypasseqp in byPasseqps)
                                                        {
                                                            List<RecipeCheckInfo> query1 = query.Where(r => r.EQPNo == bypasseqp).ToList<RecipeCheckInfo>();
                                                            query1.RemoveAll(r => r.Result == eRecipeCheckResult.OK);
                                                            if (query1.Count == 0)
                                                            {
                                                                _ForceOKResult = true;
                                                                _NGResult = false;
                                                                break;
                                                            }
                                                            //for (int i = 0; i < recipeIDList.Count; i++)
                                                            //{
                                                            //    if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                            //    {
                                                            //        _ForceOKResult = true;
                                                            //        _NGResult = false;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //if (_ForceOKResult) break;
                                                        }
                                                    }
                                                    break;
                                            }
                                            #endregion
                                        }
                                        if (_NGResult)
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG;
                                            }
                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                        }
                                    }
                                }
                                #endregion

                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                {
                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                    return;
                                }
                                #region Check Parameter
                                if (recipePara && port.File.Type != ePortType.UnloadingPort)
                                {
                                    if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_PARAMETER].GetBoolean()) ||
                                        (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_PARAMETER].GetBoolean()))
                                    {
                                        XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                                        for (int i = 0; i < recipeNoCheckList.Count; i++)
                                        {
                                            lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                                        }
                                        //Watson 20141126 DB Setting Recipe Parameter Check Enable/Disable 在RecipeService :RecipeParameterRequestCommand裏加入

                                        if (paraCheckInfos.Count() > 0)
                                        {
                                            if (recipeParaCheckData.ContainsKey(line.Data.LINEID))
                                            {
                                                List<RecipeCheckInfo> rcp = recipeParaCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;

                                                for (int i = 0; i < paraCheckInfos.Count(); i++)
                                                {
                                                    // 過濾掉重覆的部份
                                                    if (rcp.Any(r => r.EQPNo == paraCheckInfos[i].EQPNo && r.RecipeID == paraCheckInfos[i].RecipeID)) continue;
                                                    rcp.AddRange(paraCheckInfos);
                                                }
                                                rcp.AddRange(paraCheckInfos);
                                            }
                                            else
                                            {
                                                recipeParaCheckData.Add(line.Data.LINEID, paraCheckInfos);
                                            }
                                        }

                                        if (recipeParaCheckData.Count > 0)
                                        {
                                            object[] obj = new object[]
                            {
                                trxID,
                                recipeParaCheckData,
                                lstNoCehckEQ,
                                port.Data.PORTID,
                                port.File.CassetteID,
                            };
                                            eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                                            //檢查完畢, 返回值
                                            if (retCode != eRecipeCheckResult.OK)
                                            {
                                                err = "[MES] Check Recipe Parameter Reply NG: ";
                                                foreach (string key in recipeParaCheckData.Keys)
                                                {
                                                    IList<RecipeCheckInfo> recipeParaList = recipeParaCheckData[key];
                                                    string log = string.Format("Line Name=[{0}) Recipe Parameter Check", key);
                                                    string log2 = string.Empty;

                                                    for (int i = 0; i < recipeParaList.Count; i++)
                                                    {
                                                        if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                                            recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                        {
                                                            log2 += string.Format(", EQID=[{0}({1})], Result=[{2}]", recipeParaList[i].EqpID, recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(log2))
                                                    {
                                                        //err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                                        err += log + log2;
                                                    }
                                                }
                                                if (string.IsNullOrEmpty(cst.ReasonCode))
                                                {
                                                    lock (cst)
                                                    {
                                                        cst.ReasonCode = reasonCode;
                                                        cst.ReasonText = ERR_CST_MAP.RECIPE_PARAMATER_VALIDATION_NG;
                                                    }
                                                }
                                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                            }
                                        }
                                    }
                                }
                                #endregion
                                //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                                {
                                    err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                        lineName, port.Data.PORTID, port.File.CassetteID);
                                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                                    cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                                    return;
                                }
                            }
                            //將資料放入JobManager
                            if (mesJobs.Count() > 0)
                            {
                                ObjectManager.JobManager.AddJobs(mesJobs);
                                ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20141225 Tom  Add Job History

                                //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                                //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                                //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, mesJobs);
                            }

                            //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
                            port.File.RobotWaitProcCount = mesJobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

                            if (fabType == eFabType.ARRAY && recipeGroup.Count > 30) //add  cc.kuang 2016/03/09
                            {
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = MES_ReasonText.MES_Download_Recipe_Group_Count_Error;
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "Recipe Group Count > 30, Pls Check the jobs on CST");
                            }
                            #region  [当机台的Mode 为RANDOMMODE的时候下Command 给机台 20161020 zhuxingxing]
                            //fix by huangjiayin add port type check
                            if (line.Data.LINETYPE.Contains(eLineType.CELL.CCSOR) || line.Data.LINETYPE.Contains(eLineType.CELL.CCCHN))
                            {
                                if (port.File.Type == ePortType.LoadingPort)
                                {

                                    if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT" || eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN")
                                    {
                                        if (disProcessFlag.Equals(new string('0', 600)))    //Random Mode Loading Port必须要求SortFlag，不然会默停
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR;
                                            }
                                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "eqp &cst body are both random, but no product has sortflag!");

                                        }

                                        Invoke(eServiceName.CELLSpecialService, "SamplingFlagCommand", new object[] { trxID, lineName, disProcessFlag, eqp.Data.NODENO, eBitResult.ON, cst.CassetteSequenceNo });
                                    }
                                }
                            }
                            #endregion


                            // Download to PLC
                            Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxID });

                            #region Check MplcInterlock
                            //Check Mplcinterlock On By Port
                            if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort && port.File.Mode != ePortMode.Dummy)
                            {
                                string productType = "";
                                for (int i = 0; i < mesJobs.Count(); i++)
                                {
                                    productType = mesJobs[i].ProductType.Value.ToString();
                                    if (productType != "")
                                    {
                                        break;
                                    }
                                }
                                if (productType != "")
                                {
                                    string portNo = "P" + port.Data.PORTNO;
                                    Invoke(eServiceName.SubBlockService, "CheckMplcBlockByPort", new object[] { eqp.Data.NODENO, portNo, productType });
                                }
                            }
                            #endregion

                            #region Create FTP File
                            if (port.File.Type != ePortType.UnloadingPort)
                            {
                                ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                                string Path = @"D:\FileData\";
                                string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                                DateTime now = DateTime.Now;
                                int iOverTime = 10;
                                //for t2,t3 sync 2016/04/25 cc.kuang
                                if (para.ContainsKey("FileFormatPath") && para["FileFormatPath"].Value != null)
                                {
                                    Path = para["FileFormatPath"].GetString();
                                }

                                if (para.ContainsKey("FileFormatOverTimer") && para["FileFormatOverTimer"].Value != null)
                                {
                                    iOverTime = para["FileFormatOverTimer"].GetInteger();
                                }

                                if (Directory.Exists(Path + subPath))
                                {
                                    string[] fileEntries = Directory.GetFiles(Path + subPath);

                                    foreach (string fileName in fileEntries)
                                    {
                                        DateTime dt = Directory.GetLastWriteTime(fileName);
                                        if (dt.AddDays(iOverTime).CompareTo(now) < 0)
                                        {
                                            if (File.Exists(fileName))
                                            {
                                                File.Delete(fileName);
                                            }
                                        }
                                    }
                                }

                                for (int i = 0; i < mesJobs.Count(); i++)
                                {
                                    if (fabType != eFabType.CELL)
                                        FileFormatManager.CreateFormatFile("ACShop", subPath, mesJobs[i], true);
                                    else
                                    {
                                        switch (line.Data.LINETYPE)
                                        {
                                            #region [T2]
                                            case eLineType.CELL.CBPIL:
                                            case eLineType.CELL.CBODF:
                                            case eLineType.CELL.CBHVA:
                                            case eLineType.CELL.CBPMT:
                                            case eLineType.CELL.CBGAP:
                                            case eLineType.CELL.CBUVA:
                                            case eLineType.CELL.CBMCL:
                                            case eLineType.CELL.CBATS:
                                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                                break;

                                            case eLineType.CELL.CBLOI: // LOI 线要产生两种类型的File Data  20150313 Tom
                                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                break;
                                            case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                            case eLineType.CELL.CBCUT_2:
                                            case eLineType.CELL.CBCUT_3:
                                                if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                                    port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                                {
                                                    //break;
                                                }
                                                else if (port.Data.PORTID == "07" || port.Data.PORTID == "08" ||
                                                         port.Data.PORTID == "C07" || port.Data.PORTID == "C08" ||
                                                         port.Data.PORTID == "P01" || port.Data.PORTID == "P02" ||
                                                         port.Data.PORTID == "P03" || port.Data.PORTID == "P04" || port.Data.PORTID == "P06")
                                                {
                                                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                }
                                                break;
                                            #endregion
                                            //case eLineType.CELL.CCPIL:
                                            //case eLineType.CELL.CCODF:
                                            //case eLineType.CELL.CCPDR:
                                            //case eLineType.CELL.CCTAM:
                                            //case eLineType.CELL.CCPTH:
                                            //case eLineType.CELL.CCGAP:
                                            //    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                            //    break;
                                            default://T3 使用default 集中管理 sy
                                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, mesJobs[i], port });
                                                //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                                break;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region CF Exposure Special Rule
                            if (port.File.Type == ePortType.LoadingPort)
                            {
                                if ((line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                                    (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) ||
                                    (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1))
                                {
                                    object[] _object = new object[]
                    {
                        trxID,
                        lineName,
                        recipeIDCheckData
                    };
                                    Invoke(eServiceName.CFSpecialService, "AnalysisExposuePPID", _object);
                                }
                            }
                            #endregion
                            #endregion
                        }
                    }
                }
                else
                {
                    #region 之前flow
                    // 20141231 福杰說 Unloader 上CST不需要檢查Recipe
                    if (port.File.Type != ePortType.UnloadingPort)
                    {
                        #region Check Recipe ID
                        if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_ID].GetBoolean()) ||
                            (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_ID].GetBoolean()))
                        {
                            if (idCheckInfos.Count() > 0)
                            {
                                if (recipeIDCheckData.ContainsKey(line.Data.LINEID))
                                {
                                    List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;
                                    for (int i = 0; i < idCheckInfos.Count(); i++)
                                    {
                                        // 過濾掉重覆的部份
                                        if (rci.Any(r => r.EQPNo == idCheckInfos[i].EQPNo && r.RecipeID == idCheckInfos[i].RecipeID)) continue;
                                        rci.AddRange(idCheckInfos);
                                    }
                                }
                                else
                                {
                                    recipeIDCheckData.Add(line.Data.LINEID, idCheckInfos);
                                }
                            }

                            // Check Recipe ID 
                            if (recipeIDCheckData.Count() > 0)
                            {
                                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeIDCheckData, new List<string>() });

                                //檢查完畢, 返回值
                                bool _NGResult = false;
                                bool _ForceOKResult = false;
                                foreach (string key in recipeIDCheckData.Keys)
                                {
                                    IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                    string log = string.Format("Line Name=[{0}) Recipe ID Check", key);
                                    string log2 = string.Empty;

                                    for (int i = 0; i < recipeIDList.Count; i++)
                                    {
                                        if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                            recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                        {
                                            log2 += string.Format(", EQID=[{0}({1})] RecipeID=[{2}] Result=[{3}]", recipeIDList[i].EqpID, recipeIDList[i].EQPNo, recipeIDList[i].RecipeID, recipeIDList[i].Result.ToString());
                                            _NGResult = true;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(log2))
                                    {
                                        err = string.IsNullOrEmpty(err) ? "" : err += ";";
                                        err += log + log2;
                                    }

                                    #region add by bruce 20160321 for Array Special Eq 只要有一個機台回覆OK就不退CST
                                    switch (line.Data.LINEID)
                                    {
                                        case "TCATS200":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS200))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCATS400":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCATS400))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCTEG200":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG200))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCTEG400":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCTEG400))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCFLR200":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR200))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCFLR300":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCFLR300))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCELA100":

                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA100))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCELA300":

                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA300))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCELA200":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCELA200))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCAOH800":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH800))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCAOH400":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH400))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH400].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCAOH300":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH300))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH300].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCAOH900":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCAOH900))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCAOH900].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCCDO400":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO400))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                        case "TCCDO300":
                                            if (ParameterManager.ContainsKey(eArrayPPIDByPass.TCCDO300))
                                            {
                                                string[] byPasseqps = ParameterManager[eArrayPPIDByPass.TCCDO300].GetString().Split(',');
                                                foreach (string bypasseqp in byPasseqps)
                                                {
                                                    for (int i = 0; i < recipeIDList.Count; i++)
                                                    {
                                                        if (bypasseqp == recipeIDList[i].EQPNo && recipeIDList[i].Result == eRecipeCheckResult.OK)
                                                        {
                                                            _ForceOKResult = true;
                                                            _NGResult = false;
                                                            break;
                                                        }
                                                    }
                                                    if (_ForceOKResult) break;
                                                }
                                            }
                                            break;
                                    }
                                    #endregion
                                }
                                if (_NGResult)
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG;
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                }
                            }
                        }
                        #endregion

                        //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                        if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                        {
                            err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                lineName, port.Data.PORTID, port.File.CassetteID);
                            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                            cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                            cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                            return;
                        }
                        #region Check Parameter
                        if (recipePara && port.File.Type != ePortType.UnloadingPort)
                        {
                            if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_PARAMETER].GetBoolean()) ||
                                (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_PARAMETER].GetBoolean()))
                            {
                                XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                                for (int i = 0; i < recipeNoCheckList.Count; i++)
                                {
                                    lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                                }
                                //Watson 20141126 DB Setting Recipe Parameter Check Enable/Disable 在RecipeService :RecipeParameterRequestCommand裏加入

                                if (paraCheckInfos.Count() > 0)
                                {
                                    if (recipeParaCheckData.ContainsKey(line.Data.LINEID))
                                    {
                                        List<RecipeCheckInfo> rcp = recipeParaCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;

                                        for (int i = 0; i < paraCheckInfos.Count(); i++)
                                        {
                                            // 過濾掉重覆的部份
                                            if (rcp.Any(r => r.EQPNo == paraCheckInfos[i].EQPNo && r.RecipeID == paraCheckInfos[i].RecipeID)) continue;
                                            rcp.AddRange(paraCheckInfos);
                                        }
                                        rcp.AddRange(paraCheckInfos);
                                    }
                                    else
                                    {
                                        recipeParaCheckData.Add(line.Data.LINEID, paraCheckInfos);
                                    }
                                }

                                if (recipeParaCheckData.Count > 0)
                                {
                                    object[] obj = new object[]
                            {
                                trxID,
                                recipeParaCheckData,
                                lstNoCehckEQ,
                                port.Data.PORTID,
                                port.File.CassetteID,
                            };
                                    eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                                    //檢查完畢, 返回值
                                    if (retCode != eRecipeCheckResult.OK)
                                    {
                                        err = "[MES] Check Recipe Parameter Reply NG: ";
                                        foreach (string key in recipeParaCheckData.Keys)
                                        {
                                            IList<RecipeCheckInfo> recipeParaList = recipeParaCheckData[key];
                                            string log = string.Format("Line Name=[{0}) Recipe Parameter Check", key);
                                            string log2 = string.Empty;

                                            for (int i = 0; i < recipeParaList.Count; i++)
                                            {
                                                if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                                    recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                                {
                                                    log2 += string.Format(", EQID=[{0}({1})], Result=[{2}]", recipeParaList[i].EqpID, recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(log2))
                                            {
                                                //err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                                err += log + log2;
                                            }
                                        }
                                        if (string.IsNullOrEmpty(cst.ReasonCode))
                                        {
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.RECIPE_PARAMATER_VALIDATION_NG;
                                            }
                                        }
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                                    }
                                }
                            }
                        }
                        #endregion
                        //Recipe Check 的時間可能大於Timeout時間, 要檢查CST是否已經被下過Cancel了
                        if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                        {
                            err = string.Format(" RECIPE CHECK TIMEOUT, CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                                lineName, port.Data.PORTID, port.File.CassetteID);
                            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                            cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                            cst.ReasonText = MES_ReasonText.Recipe_Check_NG_From_EQP + err;
                            return;
                        }
                    }
                    //將資料放入JobManager
                    if (mesJobs.Count() > 0)
                    {
                        ObjectManager.JobManager.AddJobs(mesJobs);
                        ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20141225 Tom  Add Job History

                        //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                        //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                        //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, mesJobs);
                    }

                    //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
                    port.File.RobotWaitProcCount = mesJobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

                    if (fabType == eFabType.ARRAY && recipeGroup.Count > 30) //add  cc.kuang 2016/03/09
                    {
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = MES_ReasonText.MES_Download_Recipe_Group_Count_Error;
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "Recipe Group Count > 30, Pls Check the jobs on CST");
                    }
                    #region  [当机台的Mode 为RANDOMMODE的时候下Command 给机台 20161020 zhuxingxing]
                    //fix by huangjiayin add port type check
                    if (line.Data.LINETYPE.Contains(eLineType.CELL.CCSOR) || line.Data.LINETYPE.Contains(eLineType.CELL.CCCHN))
                    {
                        if (port.File.Type == ePortType.LoadingPort)
                        {

                            if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT" || eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN")
                            {
                                if (disProcessFlag.Equals(new string('0', 600)))    //Random Mode Loading Port必须要求SortFlag，不然会默停
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR;
                                    }
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "eqp &cst body are both random, but no product has sortflag!");

                                }

                                Invoke(eServiceName.CELLSpecialService, "SamplingFlagCommand", new object[] { trxID, lineName, disProcessFlag, eqp.Data.NODENO, eBitResult.ON, cst.CassetteSequenceNo });
                            }
                        }
                    }
                    #endregion


                    // Download to PLC
                    Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxID });

                    #region Check MplcInterlock
                    //Check Mplcinterlock On By Port
                    if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort && port.File.Mode != ePortMode.Dummy)
                    {
                        string productType = "";
                        for (int i = 0; i < mesJobs.Count(); i++)
                        {
                            productType = mesJobs[i].ProductType.Value.ToString();
                            if (productType != "")
                            {
                                break;
                            }
                        }
                        if (productType != "")
                        {
                            string portNo = "P" + port.Data.PORTNO;
                            Invoke(eServiceName.SubBlockService, "CheckMplcBlockByPort", new object[] { eqp.Data.NODENO, portNo, productType });
                        }
                    }
                    #endregion

                    #region Create FTP File
                    if (port.File.Type != ePortType.UnloadingPort)
                    {
                        ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                        string Path = @"D:\FileData\";
                        string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                        DateTime now = DateTime.Now;
                        int iOverTime = 10;
                        //for t2,t3 sync 2016/04/25 cc.kuang
                        if (para.ContainsKey("FileFormatPath") && para["FileFormatPath"].Value != null)
                        {
                            Path = para["FileFormatPath"].GetString();
                        }

                        if (para.ContainsKey("FileFormatOverTimer") && para["FileFormatOverTimer"].Value != null)
                        {
                            iOverTime = para["FileFormatOverTimer"].GetInteger();
                        }

                        if (Directory.Exists(Path + subPath))
                        {
                            string[] fileEntries = Directory.GetFiles(Path + subPath);

                            foreach (string fileName in fileEntries)
                            {
                                DateTime dt = Directory.GetLastWriteTime(fileName);
                                if (dt.AddDays(iOverTime).CompareTo(now) < 0)
                                {
                                    if (File.Exists(fileName))
                                    {
                                        File.Delete(fileName);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < mesJobs.Count(); i++)
                        {
                            if (fabType != eFabType.CELL)
                                FileFormatManager.CreateFormatFile("ACShop", subPath, mesJobs[i], true);
                            else
                            {
                                switch (line.Data.LINETYPE)
                                {
                                    #region [T2]
                                    case eLineType.CELL.CBPIL:
                                    case eLineType.CELL.CBODF:
                                    case eLineType.CELL.CBHVA:
                                    case eLineType.CELL.CBPMT:
                                    case eLineType.CELL.CBGAP:
                                    case eLineType.CELL.CBUVA:
                                    case eLineType.CELL.CBMCL:
                                    case eLineType.CELL.CBATS:
                                        FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                        break;

                                    case eLineType.CELL.CBLOI: // LOI 线要产生两种类型的File Data  20150313 Tom
                                        FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                        break;
                                    case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                    case eLineType.CELL.CBCUT_2:
                                    case eLineType.CELL.CBCUT_3:
                                        if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                            port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                        {
                                            //break;
                                        }
                                        else if (port.Data.PORTID == "07" || port.Data.PORTID == "08" ||
                                                 port.Data.PORTID == "C07" || port.Data.PORTID == "C08" ||
                                                 port.Data.PORTID == "P01" || port.Data.PORTID == "P02" ||
                                                 port.Data.PORTID == "P03" || port.Data.PORTID == "P04" || port.Data.PORTID == "P06")
                                        {
                                            FileFormatManager.CreateFormatFile("CCLineJPS", subPath, mesJobs[i], true);
                                            FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                        }
                                        break;
                                    #endregion
                                    //case eLineType.CELL.CCPIL:
                                    //case eLineType.CELL.CCODF:
                                    //case eLineType.CELL.CCPDR:
                                    //case eLineType.CELL.CCTAM:
                                    //case eLineType.CELL.CCPTH:
                                    //case eLineType.CELL.CCGAP:
                                    //    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                    //    break;
                                    default://T3 使用default 集中管理 sy
                                        Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, mesJobs[i], port });
                                        //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesJobs[i], true);
                                        break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region CF Exposure Special Rule
                    if (port.File.Type == ePortType.LoadingPort)
                    {
                        if ((line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                            (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) ||
                            (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1))
                        {
                            object[] _object = new object[]
                    {
                        trxID,
                        lineName,
                        recipeIDCheckData
                    };
                            Invoke(eServiceName.CFSpecialService, "AnalysisExposuePPID", _object);
                        }
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);

                if (IsRemap) // Re-map data NG, Don't need cancel cassette
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, port.Data.LINEID, 
                                    string.Format("(CASSETTE RE-MAP DATA) {0}:[{1}]", cst.ReasonText, ex.Message )});
                }
                else
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, ex.Message });
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
                }
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                    new object[] { trxID, lineName, "MES ValidateCassetteReply - NG", 
                        "Cassette Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region R2RRecipeReport
        //add by hujunpeng 20181110
        public void R2RRecipeReport(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst, IList<Job> jobs, string trxID)
        {
            try
            {
                if (line.File.HostMode != eHostMode.REMOTE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L4");
                lock (eqp1.File)
                {
                    eqp1.File.R2RRecipeReportStartDT = DateTime.Now;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                #region 取Validate CST Data 的 xmlDoc的值
                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                string ORIENTEDSITE = string.Empty;
                string ORIENTEDFACTORYNAME = string.Empty;
                string CURRENTSITE = string.Empty;
                string CURRENTFACTORYNAME = string.Empty;
                string LOTNAME = string.Empty;
                string PROCESSOPERATIONNAME = string.Empty;
                string PRODUCTSPECNAME = string.Empty;
                string[] recipeIDs = body[keyHost.PPID].InnerText.Split(';');
                string[] recipeNode = recipeIDs[2].Split(':');
                foreach (XmlNode n in lotNodeList)
                {
                    ORIENTEDSITE = n[keyHost.ORIENTEDSITE].InnerText.Trim();
                    ORIENTEDFACTORYNAME = n[keyHost.ORIENTEDFACTORYNAME].InnerText.Trim();
                    CURRENTSITE = n[keyHost.CURRENTSITE].InnerText.Trim();
                    CURRENTFACTORYNAME = n[keyHost.CURRENTFACTORYNAME].InnerText.Trim();
                    LOTNAME = n[keyHost.LOTNAME].InnerText.Trim();
                    PROCESSOPERATIONNAME = n[keyHost.PROCESSOPERATIONNAME].InnerText.Trim();
                    PRODUCTSPECNAME = n[keyHost.PRODUCTSPECNAME].InnerText.Trim();
                }
                #endregion

                #region 给R2R xml_Doc赋值
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("R2RRecipeReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                XmlNode machinelist = bodyNode[keyHost.MACHINELIST];
                XmlNode machine = machinelist[keyHost.MACHINE];

                bodyNode[keyHost.LINENAME].InnerText = body[keyHost.LINENAME].InnerText.Trim();
                // --James.Yan 2015/01/23
                bodyNode[keyHost.CARRIERNAME].InnerText = body[keyHost.CARRIERNAME].InnerText.Trim();
                bodyNode[keyHost.PORTNAME].InnerText = body[keyHost.PORTNAME].InnerText.Trim();
                bodyNode[keyHost.LINERECIPENAME].InnerText = body[keyHost.LINERECIPENAME].InnerText.Trim();
                bodyNode[keyHost.ORIENTEDSITE].InnerText = ORIENTEDSITE;
                bodyNode[keyHost.ORIENTEDFACTORYNAME].InnerText = ORIENTEDFACTORYNAME;
                bodyNode[keyHost.CURRENTSITE].InnerText = CURRENTSITE;
                bodyNode[keyHost.CURRENTFACTORYNAME].InnerText = CURRENTFACTORYNAME;
                bodyNode[keyHost.LOTNAME].InnerText = LOTNAME;
                bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = PROCESSOPERATIONNAME;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = PRODUCTSPECNAME;
                machine[keyHost.MACHINENAME].InnerText = eqp1.Data.NODEID;
                machine[keyHost.RECIPEID].InnerText = recipeNode[1];
                machine[keyHost.MASKNAME].InnerText = string.Empty;
                XmlNode glsListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode glsNode = glsListNode[keyHost.PRODUCT].Clone();
                glsListNode.RemoveAll();
                foreach (Job job in jobs)
                {
                    XmlNode product = glsNode.Clone();
                    product[keyHost.POSITION].InnerText = job.CurrentSlotNo;
                    product[keyHost.PRODUCTNAME].InnerText = job.EQPJobID;
                    product[keyHost.PROCESSFLAG].InnerText = job.SamplingSlotFlag;
                    glsListNode.AppendChild(product);
                }

                #endregion

                SendToMES(xml_doc);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME=[{1})] [BCS -> MES][{0}] , EQUIPMENT=[{2}] PORT_ID=[{3}] CSTID=[{4}].",
                         trxID, line.Data.LINEID, eqp.Data.NODENO, port.Data.PORTID, port.File.CassetteID));

                #region MES R2RRecipeReportReply Timeout
                string timeoutName = string.Format("{0}_{1}_MES_R2RRecipeReportReply", line.Data.LINEID, port.Data.PORTID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["R2RTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(R2RT9Timeout), trxID);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region R2RT9Timeout
        //add by hujunpeng 20181110
        private void R2RT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[MES->BCS][{0}] LINE_ID=[{1}] PORT_ID=[{2}] R2R DATA REQUEST T9 TIMEOUT!",
                    trackKey, sArray[0], sArray[1]);

                string timeoutName = string.Format("{0}_{1}_MES_R2RRecipeReportReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L4");
                lock (eqp.File)
                {
                    eqp.File.R2RRecipeReportStartDT = DateTime.Now;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0], sArray[1]);  //GetPort(sArray[1]);
                if (port != null)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                    if (cst != null)
                    {
                        if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                        {

                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={0}][1] EQUIPMENT=[{2}] PORT_NO=[{3}] R2R DATA REQUEST T9 TIMEOUT - CASSETTE[{4}] HAS BEEN CANCELED!",
                                     sArray[0], trackKey, port.Data.NODENO, port.Data.PORTNO, port.File.CassetteID));
                            return;
                        }
                    }

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, port.Data.LINEID, err });
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region MES_R2RRecipeReportReply
        //add by hujunpeng 20181110
        public void MES_R2RRecipeReportReply(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
            Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L4");
            string trxID = GetTransactionID(xmlDoc);
            //string lineName = GetLineName(xmlDoc);
            string returnCode = GetMESReturnCode(xmlDoc);
            string returnMessage = GetMESReturnMessage(xmlDoc);
            string lineName = string.Empty;
            string portName = string.Empty;
            string carrierName = string.Empty;
            string paramName = string.Empty;
            ushort paramCode = 0;
            string paramValue = string.Empty;
            string paramType = string.Empty;
            string PPID = string.Empty;
            // 獲取MES Body層資料
            XmlNode body = GetMESBodyNode(xmlDoc);
            try
            {
                XmlNodeList linelist = body[keyHost.LINELIST].ChildNodes;
                foreach (XmlNode n in linelist)
                {
                    portName = n[keyHost.PORTNAME].InnerText.Trim();
                    carrierName = n[keyHost.CARRIERNAME].InnerText.Trim();
                    lineName = n[keyHost.LINENAME].InnerText.Trim();
                }
                port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portName);
                line = ObjectManager.LineManager.GetLine(lineName);
                eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                cst = ObjectManager.CassetteManager.GetCassette(lineName,"L2",portName);
                XmlNodeList machinelist = body["LINELIST"]["LINE"]["LINERECIPENAMELIST"]["LINERECIPE"]["MACHINELIST"].ChildNodes;
                foreach(XmlNode m in machinelist)
                {
                    PPID = m["RECIPEID"].InnerText.Trim();
                }
                XmlNodeList parameters = body["LINELIST"]["LINE"]["LINERECIPENAMELIST"]["LINERECIPE"]["MACHINELIST"]["MACHINE"]["RECIPEPARALIST"].ChildNodes;
                IList<RecipeParameter> rpList = ObjectManager.RecipeManager.GetRecipeParameter("L4");
                RecipeParameter rp = null;
                List<Tuple<ushort, List<Tuple<string, string>>>> PARAME = new List<Tuple<ushort, List<Tuple<string, string>>>>();


                foreach (XmlNode p in parameters)
                {
                    if (string.IsNullOrEmpty(p["PARAVALUE"].InnerText.Trim())) continue;
                    paramValue = p["PARAVALUE"].InnerText.Trim();
                    paramName = p["PARANAME"].InnerText.Trim();
                    if (rpList == null)
                        paramCode = Convert.ToUInt16(paramName);
                    else
                    {
                        rp = rpList.FirstOrDefault(m => m.Data.PARAMETERNAME == paramName.Trim());
                        if (rp == null)
                            paramCode = Convert.ToUInt16(paramName);
                        else
                            paramCode = Convert.ToUInt16(rp.Data.SVID);
                    }
                    switch (paramCode.ToString().Substring(paramCode.ToString().Length-1,1))
                    {                       
                        case "7":
                            paramType = "U4";
                            break;
                        case "0":
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                        case "8":
                            paramType = "F8";
                            break;
                        default:
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "paramCode is not found!");
                            break;
                    }
                    List<Tuple<string, string>> parame = new List<Tuple<string, string>>();
                    Tuple<string, string> tuple = new Tuple<string, string>(paramValue, paramType);
                    parame.Add(tuple);
                    Tuple<ushort, List<Tuple<string, string>>> TUPLE = new Tuple<ushort, List<Tuple<string, string>>>(paramCode, parame);
                    PARAME.Add(TUPLE);
                }
                string timeoutName = string.Format("{0}_{1}_MES_R2RRecipeReportReply", lineName, portName);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                if (line.File.HostMode == eHostMode.REMOTE)
                {
                    
                        lock (eqp1.File)
                        {
                            eqp1.File.R2RRecipeReportReplyReturnCode = returnCode;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                    
                    if (!returnCode.Equals("0"))
                    {
                        
                        string errMsg = string.Format("[LINENAME={0}] [BCS <- MES] [{1}] R2RRecipeReportReply NG, PORTID=[{2}], CSTID=[{3}], RETURNCODE=[{4}], RETURNMESSAGE=[{5}).",
                            lineName, trxID, port.Data.PORTID, "", returnCode, returnMessage);
                        lock (cst)
                        {
                            cst.ReasonText = returnMessage;
                            if (port.File.Type == ePortType.UnloadingPort)
                            {
                                cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Validation_NG_From_MES;
                            }
                            else
                            {
                                cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                            }

                            cst.QuitCstReasonCode = string.Format("PORTID=[{0}], CSTID=[{1}] , RETURNCODE=[{2}], MES ValidateCassetteReply NG", portName, carrierName, returnCode);
                        }
                        return;
                    }
                    else
                    {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Reply Sucess. ReturnCode=[{0}]", returnCode));
                    }
                }


                #region 把parameter打给机台
                Invoke(eServiceName.NikonSECSService, "TS7F23_H_FormattedProcessProgramSend", new object[] { "L4", eqp1.Data.NODEID, PPID, PARAME, "R2R", trxID });
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// 在即有原本的ValidateCassetteReply XML裡加上 OPI Item
        /// (OPI_LINERECIPENAME, OPI_PPID, OPI_PRODUCTRECIPENAME, OPI_PROCESSFLAG)
        /// </summary>
        private XmlDocument AddOPIItemInMESXml(XmlDocument xmlDoc)
        {
            try
            {
                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNode lineRcp = body[keyHost.LINERECIPENAME];
                XmlElement element = xmlDoc.CreateElement("OPI_LINERECIPENAME");
                body.InsertAfter(element, lineRcp);

                XmlElement opiRecipe = xmlDoc.CreateElement("OPI_PPID");
                body.InsertAfter(opiRecipe, body[keyHost.PPID]);

                #region OPI_PRDCARRIERSETCODE, OPI_CARRIERSETCODE
                {
                    #region BODY
                    {
                        XmlElement opi_carrierSetCode = xmlDoc.CreateElement("OPI_CARRIERSETCODE");
                        body.InsertAfter(opi_carrierSetCode, body[keyHost.CARRIERSETCODE]);
                    }
                    #endregion
                    #region BODY\LOTLIST\LOT
                    {
                        if (body[keyHost.LOTLIST] != null)
                        {
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                            foreach (XmlNode lot in lotList)
                            {
                                XmlElement opi_prdCarrierSetCode = xmlDoc.CreateElement("OPI_PRDCARRIERSETCODE");
                                lot.InsertAfter(opi_prdCarrierSetCode, lot[keyHost.PRDCARRIERSETCODE]);
                            }
                        }
                    }
                    #endregion
                    #region BODY\LOTLIST\LOT\PROCESSLINELIST\PROCESSLINE
                    {
                        if (body[keyHost.LOTLIST] != null)
                        {
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                            foreach (XmlNode lot in lotList)
                            {
                                if (lot[keyHost.PROCESSLINELIST] != null)
                                {
                                    XmlNodeList processLineList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                                    foreach (XmlNode processLine in processLineList)
                                    {
                                        XmlElement opi_carrierSetCode = xmlDoc.CreateElement("OPI_CARRIERSETCODE");
                                        processLine.InsertAfter(opi_carrierSetCode, processLine[keyHost.CARRIERSETCODE]);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region BODY\LOTLIST\LOT\STBPRODUCTSPECLIST\STBPRODUCTSPEC
                    {
                        if (body[keyHost.LOTLIST] != null)
                        {
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                            foreach (XmlNode lot in lotList)
                            {
                                if (lot[keyHost.STBPRODUCTSPECLIST] != null)
                                {
                                    XmlNodeList stbProductSpecList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                                    foreach (XmlNode stbProductSpec in stbProductSpecList)
                                    {
                                        XmlElement opi_carrierSetCode = xmlDoc.CreateElement("OPI_CARRIERSETCODE");
                                        stbProductSpec.InsertAfter(opi_carrierSetCode, stbProductSpec[keyHost.CARRIERSETCODE]);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                if (body[keyHost.LOTLIST] != null)
                {
                    XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                    foreach (XmlNode lot in lotList)
                    {
                        XmlNode lotRcp = lot[keyHost.LINERECIPENAME];
                        element = xmlDoc.CreateElement("OPI_LINERECIPENAME");
                        lot.InsertAfter(element, lotRcp);
                        XmlNode PPID = lot[keyHost.PPID];
                        XmlElement elementPPID = xmlDoc.CreateElement("OPI_PPID");
                        lot.InsertAfter(elementPPID, PPID);

                        XmlElement elementcurrentLinePPIDD = xmlDoc.CreateElement("OPI_CURRENTLINEPPID");
                        lot.InsertAfter(elementcurrentLinePPIDD, PPID);

                        XmlElement elementcrossLinePPID = xmlDoc.CreateElement("OPI_CROSSLINEPPID");
                        lot.InsertAfter(elementcrossLinePPID, PPID);

                        XmlNodeList procLineList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        foreach (XmlNode procLine in procLineList)
                        {
                            XmlNode procRcp = procLine[keyHost.LINERECIPENAME];
                            element = xmlDoc.CreateElement("OPI_LINERECIPENAME");
                            procLine.InsertAfter(element, procRcp);
                            XmlNode procPPID = procLine[keyHost.PPID];
                            elementPPID = xmlDoc.CreateElement("OPI_PPID");
                            procLine.InsertAfter(elementPPID, procPPID);
                        }

                        XmlNodeList stbproductList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        foreach (XmlNode stb in stbproductList)
                        {
                            XmlNode stbRcp = stb[keyHost.LINERECIPENAME];
                            element = xmlDoc.CreateElement("OPI_LINERECIPENAME");
                            stb.InsertAfter(element, stbRcp);
                            XmlNode stbPPID = stb[keyHost.PPID];
                            elementPPID = xmlDoc.CreateElement("OPI_PPID");
                            stb.InsertAfter(elementPPID, stbPPID);
                        }

                        XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                        foreach (XmlNode product in productList)
                        {
                            XmlNode productRcp = product[keyHost.PRODUCTRECIPENAME];
                            XmlElement elementPorduct = xmlDoc.CreateElement("OPI_PRODUCTRECIPENAME");
                            product.InsertAfter(elementPorduct, productRcp);

                            XmlNode productPPID = product[keyHost.PPID];
                            elementPPID = xmlDoc.CreateElement("OPI_PPID");
                            product.InsertAfter(elementPPID, productPPID);

                            XmlNode procFlag = product[keyHost.PROCESSFLAG];   //yang 
                            XmlElement elementFlag = xmlDoc.CreateElement("OPI_PROCESSFLAG");
                            product.InsertAfter(elementFlag, procFlag);

                            XmlNode mesprocFlag = product[keyHost.MESPROCESSFLAG];
                            XmlElement elementmesFlag = xmlDoc.CreateElement(keyHost.MESPROCESSFLAG);
                            product.InsertAfter(elementmesFlag, mesprocFlag);
                        }
                    }
                }
                return xmlDoc;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #region Check Data Method
        /// <summary>
        /// Check Common Data
        /// </summary>
        private bool ValidateCassetteCheckData_Common(XmlDocument xmlDoc, ref Line line, ref Equipment eqp,
            ref Port port, ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);
                //Edison 2014/11/21 MES回复NG时，body会缺失很多栏位，需要先检查Return Code
                #region Check Port Object
                string portName = body[keyHost.PORTNAME].InnerText;
                port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portName);  //GetPort(portName);
                if (port == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Port Object, Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    cst.QuitCstReasonCode = errMsg;
                    return false;
                }
                #endregion

                string portid = body[keyHost.PORTNAME].InnerText;
                string cstid = body[keyHost.CARRIERNAME].InnerText;

                #region Check Cassette
                cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}], CST_SEQ_NO=[{2}]", lineName, portName,port.File.CassetteSequenceNo);
                    //cst.QuitCstReasonCode = errMsg; modify cst is null can't assign value, 2016/03/23 cc.kuang
                    return false;
                }
                #endregion

                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Cannot found Line Object, Line Name=[{0}).", lineName);
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = ERR_CST_MAP.INVALID_LINE_DATA + errMsg;
                        cst.QuitCstReasonCode = errMsg;
                    }
                    return false;
                }
                #endregion
                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                #region Check Return Code
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    if (!returnCode.Equals("0"))
                    {
                        errMsg = string.Format("[LINENAME={0}] [BCS <- MES] [{1}] ValidateCassetteReply NG, PORTID=[{2}], CSTID=[{3}], RETURNCODE=[{4}], RETURNMESSAGE=[{5}).",
                            lineName, trxID, portid, cstid, returnCode, returnMessage);
                        lock (cst)
                        {
                            cst.ReasonText = returnMessage;
                            if (port.File.Type == ePortType.UnloadingPort)
                            {
                                cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Validation_NG_From_MES;
                            }
                            else
                            {
                                cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                            }

                            cst.QuitCstReasonCode = string.Format("PORTID=[{0}], CSTID=[{1}] , RETURNCODE=[{2}], MES ValidateCassetteReply NG", portid, cstid,returnCode);
                        }
                        return false;
                    }
                    else
                    {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Reply Sucess. ReturnCode=[{0}]", returnCode));
                    }
                }
                #endregion

                #region Check Equipment Object
                eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                if (eqp == null)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Cannot found EQP Object, Line Name=[{0}], Equipment No=[{1}).", lineName, port.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_EQUIPMENT_DATA + errMsg;
                        cst.QuitCstReasonCode = errMsg;
                    }
                    return false;
                }
                #endregion

                #region Check CIM Mode
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: CIM Mode Off. EQPID=[{0}]", eqp.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.CIM_MODE_OFF;
                        cst.QuitCstReasonCode = errMsg;
                    }
                    return false;
                }
                #endregion

                #region Check Port Cassette
                if (port.File.Type == ePortType.Unknown)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.Type.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                        cst.QuitCstReasonCode = string.Format("Port Name =[{0}], CSTID=[{1}], Port Type=[{2}] is Invalid.", portName, port.File.CassetteID, port.File.Type.ToString());
                    }
                    return false;
                }

                if (port.File.EnableMode != ePortEnableMode.Enabled)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Enable Mode=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.EnableMode.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                        cst.QuitCstReasonCode = string.Format(" Port Name =[{0}], CSTID=[{1}], Port Enable Mode=[{2}] is Invalid.", portName, port.File.CassetteID, port.File.EnableMode.ToString());
                    }
                    return false;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Status=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.Status.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                        cst.QuitCstReasonCode = string.Format(" Port Name =[{0}], CSTID=[{1}], Port Status=[{2}] is Invalid.", portName, port.File.CassetteID, port.File.Status.ToString());
                    }
                    return false;
                }



                if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Different Cassette SeqNo. Line Name=[{0}], Cassette SeqNo=[{1}], Port_Cassette SeqNo=[{2}].",
                        lineName, port.File.CassetteSequenceNo, portName);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.DIFFERENT_CST_SEQNO + errMsg;
                        cst.QuitCstReasonCode = string.Format("Different Cassette SeqNo. Cassette SeqNo=[{0}], Port_Cassette SeqNo=[{1}].",port.File.CassetteSequenceNo, portName);
                    }
                    return false;
                }

                if (!string.IsNullOrEmpty(cst.CassetteID.Trim()))
                {
                    if (!cst.CassetteID.Trim().Equals(cstid))
                    {
                        errMsg = string.Format(" Cassette Data Transfer Error: Different Cassette ID. Line Name=[{0}], BC_CSTID=[{1}], MES_CSTID=[{2}].",
                            lineName, cst.CassetteID, cstid);
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.INVALID_CSTID + errMsg;
                            cst.QuitCstReasonCode = string.Format("Different Cassette ID. BC_CSTID=[{0}], MES_CSTID=[{1}].", cst.CassetteID, cstid);
                        }
                        return false;
                    }
                }
                else
                {
                    lock (cst) cst.CassetteID = cstid;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], PORTID=[{1}], CSTID=[{2}], CSTSTATUS=[{3}], Cassette Status is not available.",
                        lineName, port.Data.PORTID, cst.CassetteID, port.File.CassetteStatus.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.UNEXPECTED_MES_MESSAGE + errMsg;
                        cst.QuitCstReasonCode = string.Format("PORTID=[{0}], CSTID=[{1}], CSTSTATUS=[{2}], Cassette Status is not available.", port.Data.PORTID, cst.CassetteID, port.File.CassetteStatus.ToString());
                    }
                    return false;
                }
                #endregion

                #region CF FCUPK Line Special
                // UPK Line MES 不會Download PRODUCTLIST 的內容。
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                    return true;
                #endregion
                if (line.Data.LINETYPE == eLineType.CELL.CCCLN||line.Data.LINETYPE==eLineType.ARRAY.CAC_MYTEK) return true;  //For cst cln ,no need check slot ,by yang
                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;//sy 移動到這 20160704

                if (ParameterManager[eREPORT_SWITCH.ONLINE_PORT_MODE].GetBoolean() && line.Data.FABTYPE != eFabType.CELL.ToString())
                {
                    #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
                    if (port.File.Mode == ePortMode.Unknown)
                    {
                        errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Mode=[{3}] is Invalid.",
                            lineName, portName, port.File.CassetteID, port.File.Mode.ToString());
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                            cst.QuitCstReasonCode = string.Format("Port Name =[{0}], CSTID=[{1}], Port Mode=[{2}] is Invalid.", portName, port.File.CassetteID, port.File.Mode.ToString());
                        }
                        return false;
                    }

                    if (line.Data.LINETYPE != eLineType.ARRAY.CVD_AKT && line.Data.LINETYPE != eLineType.ARRAY.CVD_ULVAC && line.Data.LINETYPE != eLineType.ARRAY.DRY_ICD &&
                        line.Data.LINETYPE != eLineType.ARRAY.DRY_YAC && line.Data.LINETYPE !=eLineType.ARRAY.DRY_TEL)
                    {
                        //// 檢查 Port mode(EQP) 和 MES 下來的 ProductType(Job Data) 是否一致，若不一致要退 CST。
                        string jobType = string.Empty;
                        foreach (XmlNode n in lotNodeList)
                        {
                            XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;

                            for (int i = 0; i < productList.Count; i++)
                            {
                                string productType = productList[i][keyHost.PRODUCTTYPE].InnerText.Trim();

                                if (string.IsNullOrEmpty(productType))
                                {
                                    errMsg = string.Format(" Cassette MAP Transfer Error: Mes Job Product Type Invalid. Equipment=[{0}], Port=[{1}]",
                                            eqp.Data.NODENO, port.Data.PORTNO);
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.CST_MAP_TRANSFER_ERROR + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Mes Job Product Type Invalid. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                    }
                                    return false;
                                }
                                else
                                {
                                    bool checkng = false;
                                    switch (productType)
                                    {
                                        case eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT: 
                                            switch (port.File.Mode)
                                            {
                                                case ePortMode.CF:
                                                case ePortMode.Dummy:
                                                case ePortMode.MQC:
                                                case ePortMode.ThicknessDummy:
                                                case ePortMode.ThroughDummy:
                                                case ePortMode.UVMask:
                                                    checkng = true;
                                                    break;
                                            }
                                            break;
                                        case eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT: 
                                            switch (port.File.Mode)
                                            {
                                                case ePortMode.TFT:
                                                case ePortMode.HT:
                                                case ePortMode.LT:
                                                case ePortMode.Dummy:
                                                case ePortMode.MQC:
                                                case ePortMode.ThicknessDummy:
                                                case ePortMode.ThroughDummy:
                                                case ePortMode.UVMask:
                                                    checkng = true;
                                                    break;
                                            }
                                            break;
                                        case eMES_PRODUCT_TYPE.THROUGH_DUMMY:
                                            if (port.File.Mode != ePortMode.Dummy &&
                                                port.File.Mode != ePortMode.MQC)
                                                checkng = true; break;
                                        case eMES_PRODUCT_TYPE.THICKNESS_DUMMY: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                                        case eMES_PRODUCT_TYPE.MQC_DUMMY: if (port.File.Mode != ePortMode.MQC) checkng = true; break;
                                        case eMES_PRODUCT_TYPE.UV_MASK: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
                                        default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                                    }

                                    if (checkng)
                                    {
                                        errMsg = string.Format(" Cassette MAP Transfer Error: Mes Job Product Type(PRODUCTTYPE)[{0}] mismatch with EQP Port Mode[{1}]. Equipment=[{2}], Port=[{3}]",
                                            productType, port.File.Mode, eqp.Data.NODENO, port.Data.PORTNO);
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CST_MAP_TRANSFER_ERROR + errMsg;
                                            cst.QuitCstReasonCode = string.Format("Mes Job Product Type(PRODUCTTYPE)[{0}] mismatch with EQP Port Mode[{1}]", productType, port.File.Mode);
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #region Check Slot Mapping
                string mesMapData = string.Empty;
                int productCount = 0;
                bool LoaderProductCountDoubleCheckFlag = ParameterManager.ContainsKey("LoaderProductCountDoubleCheckFlag") ? ParameterManager["LoaderProductCountDoubleCheckFlag"].GetBoolean() : false;
                if ((line.Data.LINEID.Contains("CCPOL") || line.Data.LINEID.Contains("CCPCK")) && (port.File.Type == ePortType.LoadingPort) && LoaderProductCountDoubleCheckFlag)
                {
                    if (port.Data.MAPPINGENABLE == "TRUE")
                    {
                        string CheckProductCount = body[keyHost.PRODUCTQUANTITY].InnerText;
                        Invoke(eServiceName.CELLSpecialService, "ProductCountSendCommand", new object[] { eqp.Data.NODENO, CheckProductCount, port.Data.PORTNO, trxID });
                        switch (port.Data.PORTNO)
                        {
                            case "01":
                                #region[Port01]
                                Port _Port01 = ObjectManager.PortManager.GetPort(line.Data.LINEID,eqp.Data.NODENO,port.Data.PORTNO);
                                while (new TimeSpan(DateTime.Now.Ticks - eqp.File.Port01ProductCountCommandSendTime.Ticks).TotalMilliseconds < ParameterManager["ProductCountCheckTimer"].GetInteger())
                                {
                                    Thread.Sleep(300); //阻塞呼叫的Function;
                                    if (eqp.File.Port01ProductCountCommandReplyFlag == true)
                                    {
                                        lock (eqp.File)
                                        {
                                            eqp.File.Port01ProductCountCommandReplyFlag = false;
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                        }
                                        foreach (XmlNode n in lotNodeList)
                                        {
                                            XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                                            productCount += productList.Count;
                                            for (int i = 0; i < productList.Count; i++)
                                            {
                                                int slotNo;
                                                int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                                                if (slotNo <= 0)
                                                {
                                                    errMsg = " Cassette Data Transfer Error: SlotNo Parsing Error [(slotNo <= 0)]";
                                                    lock (cst)
                                                    {
                                                        cst.ReasonCode = reasonCode;
                                                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                        cst.QuitCstReasonCode = errMsg;
                                                    }
                                                    return false;
                                                }
                                                if (line.Data.FABTYPE == eFabType.CELL.ToString() && _Port01.Data.PORTATTRIBUTE == keyCELLPORTAtt.TRAY)
                                                { }
                                                else
                                                {
                                                    if (_Port01.File.ArrayJobExistenceSlot.Length < slotNo)
                                                    {
                                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, _Port01.File.ArrayJobExistenceSlot.Length);
                                                        lock (cst)
                                                        {
                                                            cst.ReasonCode = reasonCode;
                                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, _Port01.File.ArrayJobExistenceSlot.Length);
                                                        }
                                                        lock (eqp.File)
                                                        {
                                                            
                                                            eqp.File.Port01ProductCountCommandReplyJobCount = string.Empty;
                                                            eqp.File.Port01ProductCountCommandSendTime = DateTime.Now;
                                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                        }
                                                        return false;
                                                    }
                                                    if (!_Port01.File.ArrayJobExistenceSlot[slotNo - 1])
                                                    {
                                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, _Port01.File.ArrayJobExistenceSlot[slotNo - 1]);
                                                        lock (cst)
                                                        {
                                                            cst.ReasonCode = reasonCode;
                                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, _Port01.File.ArrayJobExistenceSlot[slotNo - 1]);
                                                        }
                                                        lock (eqp.File)
                                                        {
                                                            eqp.File.Port01ProductCountCommandReplyJobCount = string.Empty;
                                                            eqp.File.Port01ProductCountCommandSendTime = DateTime.Now;
                                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                        }
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        if (int.Parse(_Port01.File.JobCountInCassette) != productCount)
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", _Port01.File.JobCountInCassette, productCount);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", _Port01.File.JobCountInCassette, productCount);
                                            }
                                            lock (eqp.File)
                                            {
                                                eqp.File.Port01ProductCountCommandReplyJobCount = string.Empty;
                                                eqp.File.Port01ProductCountCommandSendTime = DateTime.Now;
                                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                            }
                                            return false;
                                        }
                                        return true;
                                    }                                 
                                }
                                if (!(new TimeSpan(DateTime.Now.Ticks - eqp.File.Port01ProductCountCommandSendTime.Ticks).TotalMilliseconds < ParameterManager["ProductCountCheckTimer"].GetInteger()))
                                {
                                    errMsg = string.Format("Loader Port#01 Product Count Send Command Reply Timeout,Please Check Loader Status");
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Loader Port#01 Product Count Send Command Reply Timeout,Please Check Loader Status");
                                    }
                                    return false;
                                }
                                break;
                                #endregion
                            case "02":
                                #region[Port02]
                                Port _Port02 = ObjectManager.PortManager.GetPort(line.Data.LINEID, eqp.Data.NODENO, port.Data.PORTNO);
                                while (new TimeSpan(DateTime.Now.Ticks - eqp.File.Port02ProductCountCommandSendTime.Ticks).TotalMilliseconds < ParameterManager["ProductCountCheckTimer"].GetInteger())
                                {
                                    Thread.Sleep(300); //阻塞呼叫的Function;
                                    if (eqp.File.Port02ProductCountCommandReplyFlag == true)
                                    {
                                        lock (eqp.File)
                                        {
                                            eqp.File.Port02ProductCountCommandReplyFlag = false;
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                        }
                                        foreach (XmlNode n in lotNodeList)
                                        {
                                            XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                                            productCount += productList.Count;
                                            for (int i = 0; i < productList.Count; i++)
                                            {
                                                int slotNo;
                                                int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                                                if (slotNo <= 0)
                                                {
                                                    errMsg = " Cassette Data Transfer Error: SlotNo Parsing Error [(slotNo <= 0)]";
                                                    lock (cst)
                                                    {
                                                        cst.ReasonCode = reasonCode;
                                                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                        cst.QuitCstReasonCode = errMsg;
                                                    }
                                                    return false;
                                                }
                                                if (line.Data.FABTYPE == eFabType.CELL.ToString() && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.TRAY)
                                                { }
                                                else
                                                {
                                                    if (_Port02.File.ArrayJobExistenceSlot.Length < slotNo)
                                                    {
                                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, _Port02.File.ArrayJobExistenceSlot.Length);
                                                        lock (cst)
                                                        {
                                                            cst.ReasonCode = reasonCode;
                                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, _Port02.File.ArrayJobExistenceSlot.Length);
                                                        }
                                                        lock (eqp.File)
                                                        {
                                                            
                                                            eqp.File.Port02ProductCountCommandReplyJobCount = string.Empty;
                                                            eqp.File.Port02ProductCountCommandSendTime = DateTime.Now;
                                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                        }
                                                        return false;
                                                    }
                                                    if (!_Port02.File.ArrayJobExistenceSlot[slotNo - 1])
                                                    {
                                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, _Port02.File.ArrayJobExistenceSlot[slotNo - 1]);
                                                        lock (cst)
                                                        {
                                                            cst.ReasonCode = reasonCode;
                                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, _Port02.File.ArrayJobExistenceSlot[slotNo - 1]);
                                                        }
                                                        lock (eqp.File)
                                                        {
                                                            eqp.File.Port02ProductCountCommandReplyJobCount = string.Empty;
                                                            eqp.File.Port02ProductCountCommandSendTime = DateTime.Now;
                                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                        }
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        if (int.Parse(_Port02.File.JobCountInCassette) != productCount)
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", _Port02.File.JobCountInCassette, productCount);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                                cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", _Port02.File.JobCountInCassette, productCount);
                                            }
                                            lock (eqp.File)
                                            {
                                                eqp.File.Port02ProductCountCommandReplyJobCount = string.Empty;
                                                eqp.File.Port02ProductCountCommandSendTime = DateTime.Now;
                                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                            }
                                            return false;
                                        }
                                        return true;
                                    }                               
                                }
                                if (!(new TimeSpan(DateTime.Now.Ticks - eqp.File.Port02ProductCountCommandSendTime.Ticks).TotalMilliseconds < ParameterManager["ProductCountCheckTimer"].GetInteger()))
                                {
                                    errMsg = string.Format("Loader Port#02 Product Count Send Command Reply Timeout,Please Check Loader Status");
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Loader Port#02 Product Count Send Command Reply Timeout,Please Check Loader Status");
                                    }
                                    return false;
                                }
                                break;
                                #endregion
                        }
                    }
                }
                else
                {
                    if (port.Data.MAPPINGENABLE == "TRUE")
                    {
                        foreach (XmlNode n in lotNodeList)
                        {
                            XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                            productCount += productList.Count;
                            for (int i = 0; i < productList.Count; i++)
                            {
                                int slotNo;
                                int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                                if (slotNo <= 0)
                                {
                                    errMsg = " Cassette Data Transfer Error: SlotNo Parsing Error [(slotNo <= 0)]";
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                        cst.QuitCstReasonCode = errMsg;
                                    }
                                    return false;
                                }
                                if (line.Data.FABTYPE == eFabType.CELL.ToString() && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.TRAY)
                                { }
                                else
                                {
                                    if (port.File.ArrayJobExistenceSlot.Length < slotNo)
                                    {
                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, port.File.ArrayJobExistenceSlot.Length);
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch MES POSITION[{0}] > CASSETTE SLOT=[{1}]", slotNo, port.File.ArrayJobExistenceSlot.Length);
                                        }
                                        return false;
                                    }
                                    if (!port.File.ArrayJobExistenceSlot[slotNo - 1])
                                    {
                                        errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, port.File.ArrayJobExistenceSlot[slotNo - 1]);
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                            cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch Port Slot=[{0}] status=[{1}]", slotNo, port.File.ArrayJobExistenceSlot[slotNo - 1]);
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                        if (int.Parse(port.File.JobCountInCassette) != productCount)
                        {
                            errMsg = string.Format(" Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", port.File.JobCountInCassette, productCount);
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                                cst.QuitCstReasonCode = string.Format("Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", port.File.JobCountInCassette, productCount);
                            }
                            return false;
                        }
                    }
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",errMsg);

                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                        cst.QuitCstReasonCode = "Cassette Data Transfer Error: Abnormal Exception Error";
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Check Array Data
        /// </summary>
        private bool ValidateCassetteCheckData_Array(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port,
            ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                bool bSamplingFlag = false; //2016/05/20 cc.kuang
                string lineName = GetLineName(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNodeList lotNodeList;

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                int productQuantity;
                int.TryParse(body[keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);

                #region Check Port
                if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT))
                {
                    errMsg = " Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                        cst.QuitCstReasonCode = "Unloader Port loads full CST, unable to receive glass.";
                    }
                    return false;
                }
                else if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) &&
                    line.Data.LINETYPE != eLineType.ARRAY.CAC_MYTEK &&
                    productQuantity.Equals(0))
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                        cst.QuitCstReasonCode = string.Format("{0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                    }
                    return false;
                }

                if (port.File.Type == ePortType.UnloadingPort && line.File.HostMode != eHostMode.OFFLINE &&
                    line.File.IndexOperMode != eINDEXER_OPERATION_MODE.SORTER_MODE && line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE &&
                    (int.Parse(port.File.JobCountInCassette) > 0 && port.File.PartialFullFlag != eParitalFull.PartialFull)) //t3 array support parial full cst cc.kuang 2015/07/06
                {
                    errMsg = " Cassette Data Transfer Error: Unloader Port loads CST , but CST have data.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
                        cst.QuitCstReasonCode ="Unloader Port loads CST , but CST have data.";
                    }
                    return false;
                }

                if (port.File.Type == ePortType.UnloadingPort && port.File.JobCountInCassette.Equals("0")) return true;
                #endregion

                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                {
                    errMsg = " Cassette Data Transfer Error: Force Clean Out Mode can't loading cassette.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.ARRAY_FORCECLEANOUT + errMsg;
                        cst.QuitCstReasonCode = errMsg;
                    }
                    return false;
                }

                switch (line.Data.LINETYPE)
                {
                    case eLineType.ARRAY.CVD_AKT:
                    case eLineType.ARRAY.CVD_ULVAC:
                        {
                            break; //t3 CVD MES not use chamberrunmode and pass mix mode's glass flow check, 2015/10/09 cc.kuang
                            bool ngResult = false;
                            string LINEOPERATIONMODE = body[keyHost.LINEOPERMODE].InnerText.Trim();

                            #region 檢查MES的LineOperationMode是否有在CVD的5個Chamber中做設定
                            List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                            Equipment cvd = eqps.FirstOrDefault(e => e.Data.USERUNMODE != null && e.Data.USERUNMODE.Equals("Y"));

                            if (cvd == null)
                            {
                                errMsg = " Cassette Data Transfer Error: Check CVD Run Mode, can't find CVD Equipment Object.";
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode="Check CVD Run Mode, can't find CVD Equipment Object.";
                                }
                                return false;
                            }

                            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(cvd.Data.NODENO).Where(u => u.Data.UNITTYPE != null
                                    && u.Data.UNITTYPE.Equals("CHAMBER")).ToList<Unit>();

                            //因為要比對Product層的資料, 所以要一層一層取得資料
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;

                            if (lotList.Count == 0)
                            {
                                errMsg = string.Format(" Cassette Data Transfer Error: Check CVD Run Mode, MES Lot Data no Data.");
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode ="Check CVD Run Mode, MES Lot Data no Data.";
                                }
                                return false;
                            }

                            XmlNodeList productList = lotList[0][keyHost.PRODUCTLIST].ChildNodes;
                            if (productList.Count == 0)
                            {
                                errMsg = string.Format(" Cassette Data Transfer Error: Check CVD Run Mode, MES Product Data no Data.");
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode = "Check CVD Run Mode, MES Product Data no Data.";
                                }
                                return false;
                            }

                            string log = string.Empty;

                            #region Check Port Mode
                            //t3 not need check port mode for product type cc.kuang 2015/07/06                            
                            #endregion
                            string runMode = string.Empty;
                            #region [Check flow type及Chamber Run Mode 是否一致]
                            string flowtype = string.Empty;
                            string clnRcp = "0";
                            string cvdRcp = "0";
                            for (int i = 0; i < lotList.Count; i++)
                            {
                                XmlNodeList productNodeList = lotList[i][keyHost.PRODUCTLIST].ChildNodes;
                                for (int j = 0; j < productList.Count; j++)
                                {
                                    string ppid = ObjectManager.JobManager.ParsePPID(productNodeList[j][keyHost.PPID].InnerText).Item1.PadRight(18, '0');
                                    clnRcp = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    cvdRcp = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                    if (string.IsNullOrEmpty(flowtype))
                                    {
                                        flowtype = clnRcp + cvdRcp;
                                    }
                                    else
                                    {
                                        if (!flowtype.Equals(clnRcp + cvdRcp))
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            /* modify 2016/06/30 cc.kuang
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                                cst.QuitCstReasonCode = string.Format("glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                            */
                                        }
                                    }

                                    // MES說要看CHAMBERRUNMODE, 不要去看LINEOPERMODE
                                    if (string.IsNullOrEmpty(runMode))
                                    {
                                        runMode = productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim();
                                    }
                                    else
                                    {
                                        if (runMode != productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim())
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: \"CHAMBERRUNMODE\" mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                                cst.QuitCstReasonCode = string.Format("CHAMBERRUNMODE mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                            // 只進清洗機的話就不用再往下檢查
                            if (clnRcp.Equals("1") && cvdRcp.Equals("0")) return true;
                            #endregion

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                            {
                                if (port.File.Type != ePortType.BothPort)
                                {
                                    errMsg = string.Format(" Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}) is not \"Both Port\" in MIX RUN MODE.",
                                        lineName, port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Port Name =[{0}], CSTID=[{1}], Port Type=[{2}) is not \"Both Port\" in MIX RUN MODE.",  port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    }
                                    return false;
                                }

                                // 檢查MES的CahmberRunMode是否有在CVD的5個Chamber中做設定
                                if (!units.Any(u => u.File.RunMode.Trim().Equals(runMode)))
                                {
                                    errMsg = string.Format(" Cassette Data Transfer Error: Check CVD Run Mode, can't find RUNMODE=[{0}] in CVD Equipment.",
                                        runMode);
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                        cst.QuitCstReasonCode = string.Format(" Check CVD Run Mode, can't find RUNMODE=[{0}] in CVD Equipment.", runMode);
                                    }
                                    return false;
                                }

                                if (CheckMixRunOtherPortFlowType(cst.PortNo, flowtype, out errMsg) == false)
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR +"_"+errMsg;
                                        cst.QuitCstReasonCode = errMsg;
                                    }
                                    return false;
                                }

                                #region  Check PORT Mode 配對
                                // t3 not need check port mode cc.kuang 2015/07/07
                                #endregion
                            }
                            else
                            {
                                // 檢查MES的LineOperationMode是否有在CVD的5個Chamber中做設定
                                if (port.File.Type != ePortType.UnloadingPort)
                                {
                                    if (!units.Any(u => u.File.RunMode.Trim().Equals(runMode)))
                                    {
                                        errMsg = string.Format(" Cassette Data Transfer Error: Check CVD Run Mode, can't find RUNMODE=[{0}] in CVD Equipment.", runMode);
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                            cst.QuitCstReasonCode = string.Format("Check CVD Run Mode, can't find RUNMODE=[{0}] in CVD Equipment.", runMode);
                                        }
                                        return false;
                                    }
                                }
                            }
                            #endregion
                        }                        
                        break;

                    case eLineType.ARRAY.DRY_ICD:
                    case eLineType.ARRAY.DRY_YAC:
                    case eLineType.ARRAY.DRY_TEL:
                        {
                            bool ngResult = false;
                            string LINEOPERATIONMODE = body[keyHost.LINEOPERMODE].InnerText.Trim().ToUpper();
                            if (LINEOPERATIONMODE == "NORN") { LINEOPERATIONMODE = "NORMAL"; } // add by bruce 2014/12/8 o mode report to mes

                            #region 檢查MES的LineOperationMode是否有在DRY的3個Chamber中做設定
                            List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                            Equipment dry = eqps.FirstOrDefault(e => e.Data.USERUNMODE != null &&  e.Data.USERUNMODE.Equals("Y"));

                            if (dry == null)
                            {
                                errMsg = " Cassette Data Transfer Error: Check DRY Run Mode, can't find DRY Equipment Object.";
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode = " Check DRY Run Mode, can't find DRY Equipment Object.";
                                }
                                return false;
                            }

                            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(dry.Data.NODENO);

                            //因為要比對Product層的資料, 所以要一層一層取得資料
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;

                            if (lotList.Count == 0 && ngResult == false)
                            {
                                errMsg = string.Format(" Cassette Data Transfer Error: Check DRY Run Mode, MES Lot Data no Data.");
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode ="Check DRY Run Mode, MES Lot Data no Data.";
                                }
                                return false;
                            }

                            //XmlNodeList productList = lotList[0].ChildNodes;
                            XmlNodeList productList = lotList[0][keyHost.PRODUCTLIST].ChildNodes;
                            if (productList.Count == 0 && ngResult == false)
                            {
                                errMsg = string.Format(" Cassette Data Transfer Error: Check DRY Run Mode, MES Product Data no Data.");
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR + errMsg;
                                    cst.QuitCstReasonCode = "Check DRY Run Mode, MES Product Data no Data.";
                                }
                                return false;
                            }

                            string log = string.Empty;

                            #region Check Port Mode
                            // t3 not need check port mode cc.kuang 2015/07/07
                            #endregion
                            string runMode = string.Empty;
                            #region [Check flow type 及 Chamber Run Mode 是否一致]
                            string flowtype = string.Empty;
                            string clnRcp = "0";
                            string dryRcp = "0";
                            for (int i = 0; i < lotList.Count; i++)
                            {
                                XmlNodeList productNodeList = lotList[i][keyHost.PRODUCTLIST].ChildNodes;
                                for (int j = 0; j < productList.Count; j++)
                                {
                                    string ppid = ObjectManager.JobManager.ParsePPID(productNodeList[j][keyHost.PPID].InnerText).Item1.PadRight(18, '0');
                                    clnRcp = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    dryRcp = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                    if (string.IsNullOrEmpty(flowtype))
                                    {
                                        flowtype = clnRcp + dryRcp;
                                    }
                                    else
                                    {
                                        if (!flowtype.Equals(clnRcp + dryRcp))
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            /* modify 2016/06/30 cc.kuang
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR + errMsg;
                                                cst.QuitCstReasonCode = string.Format("glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                            */
                                        }
                                    }

                                    // MES說要看CHAMBERRUNMODE, 不要去看LINEOPERMODE
                                    if (string.IsNullOrEmpty(runMode))
                                    {
                                        runMode = productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim();
                                    }
                                    else
                                    {
                                        if (runMode != productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim())
                                        {
                                            errMsg = string.Format(" Cassette Data Transfer Error: \"CHAMBERRUNMODE\" mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                                cst.QuitCstReasonCode = string.Format("CHAMBERRUNMODE mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                            // 只進清洗機的話就不用再往下檢查
                            if (clnRcp.Equals("1") && dryRcp.Equals("0")) return true;
                            #endregion

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                            {
                                if (port.File.Type != ePortType.BothPort)
                                {
                                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}) is not \"Both Port\" in MIX RUN MODE.",
                                        lineName, port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS;
                                        cst.QuitCstReasonCode = string.Format("Port Name =[{0}], CSTID=[{1}], Port Type=[{2}) is not \"Both Port\" in MIX RUN MODE.", port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    }
                                    return false;
                                }

                                #region Check

                                // 檢查MES的CahmberRunMode是否有在DRY的5個Chamber中做設定
                                if (!units.Any(u => u.File.RunMode.Trim().Equals(runMode)))
                                {
                                    errMsg = string.Format(" Cassette Data Transfer Error: Check DRY Run Mode, can't find RUNMODE=[{0}] in DRY Equipment.",
                                        runMode);
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_CVD_DATA_CHECK_ERROR + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Check DRY Run Mode, can't find RUNMODE=[{0}] in DRY Equipment.", runMode);
                                    }
                                    return false;
                                }

                                int portModeCount = ObjectManager.PortManager.GetPorts().Select(p => p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().Count();

                                if (portModeCount == 0 || portModeCount == 3)
                                {
                                    if (portModeCount == 0)
                                        errMsg = " Cassette Data Transfer Error: Check DRY Run Mode, All the Port are \"Unknown\".";
                                    else if (portModeCount == 3)
                                        errMsg = " Cassette Data Transfer Error: Check DRY Run Mode, There are three different Port Mode.";

                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR + errMsg;
                                        cst.QuitCstReasonCode = errMsg;
                                    }
                                    return false;
                                }

                                if (CheckMixRunOtherPortFlowType(cst.PortNo, flowtype, out errMsg) == false)
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_DRY_DATA_CHECK_ERROR +"_"+ errMsg;
                                        cst.QuitCstReasonCode = errMsg;
                                    }
                                    return false;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        break;
                    case eLineType.ARRAY.BFG_SHUZTUNG:
                        // 因為Check SCRAPCUTFLAG 如果NG不退CST, 所以提到外層去檢查
                        break;
                    case eLineType.ARRAY.MSP_ULVAC:
                    case eLineType.ARRAY.ITO_ULVAC:
                        {
                            bool ngResult = false;
                            XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                            string log = string.Empty;
                            string runMode = string.Empty;
                            #region [Check flow type是否一致]
                            string flowtype = string.Empty;
                            string clnRcp1 = "0";
                            string clnRcp2 = "0";
                            string pvdRcp = "0";
                            for (int i = 0; i < lotList.Count; i++)
                            {
                                XmlNodeList productNodeList = lotList[i][keyHost.PRODUCTLIST].ChildNodes;
                                for (int j = 0; j < productNodeList.Count; j++)
                                {
                                    string ppid = ObjectManager.JobManager.ParsePPID(productNodeList[j][keyHost.PPID].InnerText).Item1.PadRight(22, '0');
                                    clnRcp1 = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    clnRcp2 = ppid.Substring(6, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    pvdRcp = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                    if (string.IsNullOrEmpty(flowtype))
                                    {
                                        flowtype = clnRcp1 + clnRcp2 + pvdRcp;
                                    }
                                    else
                                    {
                                        if (!flowtype.Equals(clnRcp1 + clnRcp2 + pvdRcp))
                                        {
                                            errMsg = string.Format("Cassette Data Transfer Error: glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            /* modify 2016/06/30 cc.kuang
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_PVD_DATA_CHECK_ERROR + "_" + errMsg;
                                                cst.QuitCstReasonCode = string.Format("glass flow type mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                            */
                                        }
                                    }

                                    // MES說要看CHAMBERRUNMODE, 不要去看LINEOPERMODE
                                    if (string.IsNullOrEmpty(runMode))
                                    {
                                        runMode = productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim();
                                    }
                                    else
                                    {
                                        if (runMode != productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim())
                                        {
                                            errMsg = string.Format("Cassette Data Transfer Error: \"CHAMBERRUNMODE\" mismatch with MES product data. Equipment=[{0}], Port=[{1}]",
                                                eqp.Data.NODENO, port.Data.PORTNO);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_PVD_DATA_CHECK_ERROR + "_" + errMsg;
                                                cst.QuitCstReasonCode = string.Format("CHAMBERRUNMODE mismatch with MES product data. Equipment=[{0}], Port=[{1}]", eqp.Data.NODENO, port.Data.PORTNO);
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                            // 只進清洗機的話就不用再往下檢查
                            if ((clnRcp1.Equals("1")||(clnRcp2.Equals("1"))) && pvdRcp.Equals("0")) return true;

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                            {
                                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                                Equipment pvd = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L2"));
                                String loadlock01Type = string.Empty;
                                String loadlock02Type = string.Empty;
                                switch(pvd.File.ProportionalRule01Type)
                                {
                                    case 0:
                                        loadlock01Type = "";
                                        break;
                                    case 1:
                                        loadlock01Type = "MQC1";
                                        break;
                                    case 2:
                                        loadlock01Type = "MQC2";
                                        break;
                                    case 3:
                                        loadlock01Type = "LS";
                                        break;
                                    case 4:
                                        loadlock01Type = "GE";
                                        break;
                                    case 5:
                                        loadlock01Type = "SD";
                                        break;
                                    case 6:
                                        loadlock01Type = "M3";
                                        break;
                                    case 7:
                                        loadlock01Type = "BITO";
                                        break;
                                    case 8:
                                        loadlock01Type = "TITO";
                                        break;
                                    default:
                                        loadlock01Type = "";
                                        break;
                                }
                                switch (pvd.File.ProportionalRule02Type)
                                {
                                    case 0:
                                        loadlock02Type = "";
                                        break;
                                    case 1:
                                        loadlock02Type = "MQC1";
                                        break;
                                    case 2:
                                        loadlock02Type = "MQC2";
                                        break;
                                    case 3:
                                        loadlock02Type = "LS";
                                        break;
                                    case 4:
                                        loadlock02Type = "GE";
                                        break;
                                    case 5:
                                        loadlock02Type = "SD";
                                        break;
                                    case 6:
                                        loadlock02Type = "M3";
                                        break;
                                    case 7:
                                        loadlock02Type = "BITO";
                                        break;
                                    case 8:
                                        loadlock02Type = "TITO";
                                        break;
                                    default:
                                        loadlock02Type = "";
                                        break;
                                }
                                if (port.File.Type != ePortType.BothPort)
                                {
                                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}) is not \"Both Port\" in MIX RUN MODE.",
                                        lineName, port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + "_" + errMsg;
                                        cst.QuitCstReasonCode = string.Format("Port Name =[{0}], CSTID=[{1}], Port Type=[{2}) is not \"Both Port\" in MIX RUN MODE.",  port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                    }
                                    return false;
                                }                               

                                if (CheckMixRunOtherPortFlowType(cst.PortNo, flowtype, out errMsg) == false)
                                {
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.ARRAY_PVD_DATA_CHECK_ERROR + "_" + errMsg;
                                        cst.QuitCstReasonCode = errMsg;
                                    }
                                    return false;
                                }

                                for (int i = 0; i < lotList.Count; i++)
                                {
                                    XmlNodeList productNodeList = lotList[i][keyHost.PRODUCTLIST].ChildNodes;
                                    for (int j = 0; j < productNodeList.Count; j++)
                                    {
                                        if (string.IsNullOrEmpty(productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim())
                                            && loadlock01Type.Length > 0 && loadlock02Type.Length > 0)
                                        {
                                            errMsg = string.Format("Cassette Data Transfer Error: \"CHAMBERRUNMODE\" space mismatch Mix Mode Process Type Setup. Equipment=[{0}], Port=[{1}], LoadLock01[{2}], LoadLock02[{3}]",
                                                eqp.Data.NODENO, port.Data.PORTNO, loadlock01Type, loadlock02Type);
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.ARRAY_PVD_DATA_CHECK_ERROR + "_" + errMsg;
                                                cst.QuitCstReasonCode = string.Format("CHAMBERRUNMODE space mismatch Mix Mode Process Type Setup. Equipment=[{0}], Port=[{1}], LoadLock01[{2}], LoadLock02[{3}]", eqp.Data.NODENO, port.Data.PORTNO, loadlock01Type, loadlock02Type);
                                            }
                                            return false;
                                        }
                                        else
                                        {
                                            if (productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim().Equals(loadlock01Type) &&
                                                productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim().Equals(loadlock02Type) )
                                            {
                                                errMsg = string.Format("Cassette Data Transfer Error: \"CHAMBERRUNMODE\" mismatch Mix Mode Process Type Setup. Equipment=[{0}], Port=[{1}], CHAMBERRUNMODE[{2}], LoadLock01[{3}], LoadLock02[{4}]",
                                                eqp.Data.NODENO, port.Data.PORTNO, productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim(), loadlock01Type, loadlock02Type);
                                                lock (cst)
                                                {
                                                    cst.ReasonCode = reasonCode;
                                                    cst.ReasonText = ERR_CST_MAP.ARRAY_PVD_DATA_CHECK_ERROR + "_" + errMsg;
                                                    cst.QuitCstReasonCode =string.Format ( "CHAMBERRUNMODE mismatch Mix Mode Process Type Setup. Equipment=[{0}], Port=[{1}], CHAMBERRUNMODE[{2}], LoadLock01[{3}], LoadLock02[{4}]",eqp.Data.NODENO, port.Data.PORTNO, productNodeList[j][keyHost.CHAMBERRUNMODE].InnerText.Trim(), loadlock01Type, loadlock02Type);
                                                }
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        break;
                }

                //check Sampling Flag=0 2016/05/20 cc.kuang
                lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                foreach (XmlNode n in lotNodeList)
                {
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        string processflag = productList[i][keyHost.PROCESSFLAG].InnerText;
                        if (!string.IsNullOrEmpty(processflag))
                        {
                            if (processflag.Equals("Y"))
                            {
                                bSamplingFlag = true;
                            }
                        }
                    }
                }

                if (port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort)
                {
                    if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        if (!bSamplingFlag)
                        {
                            errMsg = string.Format("MES Sampling Flag Count = 0");
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.SAMPLINGFLAG_COUNT_0 + "_" + errMsg;
                                cst.QuitCstReasonCode = "Check MES Sampling Flag Count = 0.";
                            }
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        private bool CheckTBBFG_SCRAPCUTFLAG(XmlDocument xmlDoc)
        {
            try
            {
                XmlNode body = GetMESBodyNode(xmlDoc);

                XmlNodeList lotList = body[keyHost.LOTLIST].ChildNodes;
                foreach (XmlNode lot in lotList)
                {
                    XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                    foreach (XmlNode product in productList)
                    {
                        string scrap = product[keyHost.SCRAPCUTFLAG].InnerText.Trim();
                        if (scrap.Equals("S") || scrap.Equals("C"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckMixRunOtherPortFlowType(string currPortNo, string currFlowType, out string err)
        {
            err = string.Empty;
            try
            {
                return true; // modify for not need check CVD/PVD/Dry 2016/06/30 cc.kuang
                // 尋找非本次上Port的其他Port
                List<Port> ports = ObjectManager.PortManager.GetPorts().Where(p => p.Data.PORTNO != currPortNo 
                    && (p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND
                    || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING
                    || p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)).ToList();

                foreach (Port p in ports)
                {
                    Job job = ObjectManager.JobManager.GetJobs(p.File.CassetteSequenceNo).FirstOrDefault();
                    if (job != null)
                    {
                        string flowtype = string.Empty;
                        string clnRcp = "0";
                        string cvdRcp = "0";
                        clnRcp = job.PPID.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                        cvdRcp = job.PPID.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                        flowtype = clnRcp + cvdRcp;
                        if (currFlowType != flowtype)
                        {
                            err = string.Format("Cassette Data Transfer Error: Mix Run Mode, Flow Type is different with Port#{0}", p.Data.PORTNO);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                err = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Check CF Data
        /// </summary>
        private bool ValidateCassetteCheckData_CF(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port,
            ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                IList<Equipment> eqpList = new List<Equipment>();
                int processCount = 0;
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                string cstid = body[keyHost.CARRIERNAME].InnerText;
                string portName = body[keyHost.PORTNAME].InnerText;
                bool Result = false;

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                int productQuantity;
                int.TryParse(body[keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);

                #region 取得 ProductQuantity 和 PlannedQuantity
                lock (port)
                {
                    port.File.ProductQuantity = body[keyHost.PRODUCTQUANTITY].InnerText;
                    port.File.PlannedQuantity = body[keyHost.PLANNEDQUANTITY].InnerText;
                    port.File.PlannedSourcePart = body[keyHost.PLANNEDSOURCEPART].InnerText;
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
                #endregion

                #region Check Port (Common condition)
                if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
                {
                    errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                    }
                    return false;
                }

                if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Partial Full Mode is Disable! portType=[{0}) PartialFullFlag=[{1}) ProductQuantity=[{2}]",
                                            port.File.Type, port.File.PartialFullFlag, productQuantity);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                    }
                    return false;
                }

                if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0) && line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1 && line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE2) //防止Loader/Both Port上空卡匣
                {
                    errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                    }
                    return false;
                }

                if (port.File.Type != ePortType.UnloadingPort && productQuantity > 0) //防止上實卡匣時，卡匣內的 Job ProcessFlag 都為 N
                {
                    foreach (XmlNode n in lotNodeList)
                    {
                        XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                        for (int i = 0; i < productList.Count; i++)
                        {
                            string processflag = productList[i][keyHost.PROCESSFLAG].InnerText;
                            if (!string.IsNullOrEmpty(processflag))
                            {
                                if (processflag.Equals("Y"))
                                {
                                    Result = true;
                                }
                            }
                        } 
                        if (!Result)
                        {
                            errMsg = string.Format("Cassette Data Transfer Error: Can't found Job Process Flag = 'Y' in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                eqp.Data.NODENO, port.Data.PORTNO);
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                            }
                            // Send CIM Message to EQP
                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't found Job Process Flag = 'Y' in Cassette MAP Data.", "MES" });
                            return false;
                        }
                    }
                }
                #endregion

                #region Check Prot Mode According to Indexer Run Mode 
                //20160519 Add by Fank Modified by zhangwei 20161213 去掉SORT 卡 Bothport 逻辑
                if (line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1 && line.Data.LINETYPE != eLineType.CF.FCREW_TYPE1 && line.Data.LINETYPE != eLineType.CF.FCSRT_TYPE1)
                {
                    switch (line.File.IndexOperMode)
                    {
                        case eINDEXER_OPERATION_MODE.NORMAL_MODE:
                        case eINDEXER_OPERATION_MODE.MQC_MODE:
                            if (port.File.Type != ePortType.BothPort)
                            {
                                errMsg = "Cassette Data Transfer Error: Normal/MQC Mode Need Both Port";
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.CF_PORT_TYPE_MISSMATCH + "_" + errMsg;
                                }
                                return false;
                            }
                            break;
                    }
                }

                #endregion

                #region Check Port (Special condition by Line Type)
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CF.FCREW_TYPE1:

                        if (port.File.Type == ePortType.LoadingPort)
                        {

                            #region Check CFREWORKCOUNT
                            foreach (XmlNode n in lotNodeList)
                            {
                                string cfreworkcount = n[keyHost.CFREWORKCOUNT].InnerText;
                                int CFReworkCount = 0;
                                int.TryParse(cfreworkcount, out CFReworkCount);
                                if (CFReworkCount == 0)
                                {
                                    errMsg = string.Format("Cassette Data Transfer Error: Can't found CFREWORKCOUNT value in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                        eqp.Data.NODENO, port.Data.PORTNO);
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                                    }
                                    // Send CIM Message to EQP
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't found CFREWORKCOUNT value in Cassette MAP Data.", "MES" });
                                    Result = false;
                                    continue;
                                }
                                XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                                for (int i = 0; i < productList.Count; i++)
                                {
                                    string processflag = productList[i][keyHost.PROCESSFLAG].InnerText;
                                    if (processflag.Equals("Y"))
                                    {
                                        processCount++;
                                        Result = true;
                                    }
                                }
                            }
                            if (!Result)
                            {
                                return false;
                            }
                            #endregion

                            #region Check Process Count

                            #region 取得目前機台最少可以Rework的片數
                            int ReworkWashableCount = 0;
                            IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                            foreach (Equipment _eqp in eqps)
                            {
                                if (ReworkWashableCount == 0)
                                {
                                    ReworkWashableCount = _eqp.File.ReworkWashableCount;
                                }
                                else
                                {
                                    if (ReworkWashableCount > _eqp.File.ReworkWashableCount)
                                    {
                                        ReworkWashableCount = _eqp.File.ReworkWashableCount;
                                    }
                                }
                            }
                            #endregion

                            #region 取得目前線上等待Rework的片數
                            IList<Job> jobs = ObjectManager.JobManager.GetJobs().
                                Where(j => (j.CfSpecial.TrackingData.Etching == "0" || j.CfSpecial.TrackingData.Stripper == "0") && j.MesProduct.PROCESSFLAG == "Y" && j.CurrentEQPNo == "L2").
                                Distinct().ToList<Job>();
                            #endregion

                            #region 實際線上可 Rrwork 的片數
                            int RealyReworkCount = ReworkWashableCount - jobs.Count;
                            #endregion

                            #region 取得 ValidateCassetteReply 內 PROCESSFLAG = 'Y' 的玻璃有幾片。
                            int CassetteReworkCount = 0;
                            foreach (XmlNode n in lotNodeList)
                            {
                                XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                                for (int i = 0; i < productList.Count; i++)
                                {
                                    string processflag = productList[i][keyHost.PROCESSFLAG].InnerText;
                                    if (!string.IsNullOrEmpty(processflag))
                                    {
                                        if (processflag.Equals("Y"))
                                        {
                                            CassetteReworkCount += 1;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 計算出可以Start的片數
                            if (RealyReworkCount <= 0)
                            {
                                errMsg = "Cassette Data Transfer Error: Rework equipment can't do more Job.";
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.CF_REWORK_WASHABLECOUNT_CHECK_NG;
                                }
                                return false;
                            }
                            else if (RealyReworkCount >= CassetteReworkCount)
                            {
                                lock (port)
                                    port.File.StartByCount = CassetteReworkCount.ToString();
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }
                            else if (RealyReworkCount <= CassetteReworkCount)
                            {
                                lock (port)
                                    port.File.StartByCount = RealyReworkCount.ToString();
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }
                            #endregion

                            #endregion
                        }
                        break;

                    case eLineType.CF.FCUPK_TYPE1:

                        #region Check PlannedSourcePart
                        if (eqp.Data.NODEATTRIBUTE == "UPK")
                        {
                            // 取得 PlannedSourcePart
                            String plannedsourcepart = body[keyHost.PLANNEDSOURCEPART].InnerText;
                            lock (port)
                            {
                                port.File.PlannedSourcePart = plannedsourcepart;
                            }

                            // 檢查 MES 是否沒給 PlannedSourcePart 
                            if (string.IsNullOrEmpty(plannedsourcepart) || plannedsourcepart == "0")
                            {
                                errMsg = string.Format("Cassette Data Transfer Error: PlannedSourcePart is 'null' or '0' in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                    eqp.Data.NODENO, port.Data.PORTNO);
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                                }
                                // Send CIM Message to EQP
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "PlannedSourcePart is 'null' or '0' in Cassette MAP Data.", "MES" });
                                return false;
                            }
                            // 檢查 MES Download 的資料與線上所有玻璃的 PlannedSourcePart 一不一致
                            Job job = new Job();
                            job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j => j.CfSpecial.PlannedSourcePart == plannedsourcepart);
                            int InLineJobsCount = ObjectManager.JobManager.GetJobCount();
                            if (job == null && InLineJobsCount != 0)
                            {
                                errMsg = string.Format("Cassette Data Transfer Error: PlannedSourcePart Mismatch. Equipment=[{0}], Port=[{1}]",
                                    eqp.Data.NODENO, port.Data.PORTNO);
                               /* lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                                } */
                                // Send CIM Message to EQP
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "PlannedSourcePart Mismatch.", "MES" });
                               // return false;
                            }
                        }
                        #endregion

                        #region 取得可以Start的片數
                        lock (port)
                            port.File.StartByCount = port.File.PlannedQuantity;
                        ObjectManager.PortManager.EnqueueSave(port.File);
                        #endregion
                        break;
                    case eLineType.CF.FCMAC_TYPE1:

                        #region Check Equipment Run Mode
                        eqpList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Where(e => e.Data.NODENO != "L2").ToList();
                        foreach (XmlNode n in lotNodeList)
                        {
                            XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                            for (int i = 0; i < productList.Count; i++)
                            {
                                string processflag = productList[i][keyHost.PROCESSFLAG].InnerText;
                                string chamberrunmode = productList[i][keyHost.CHAMBERRUNMODE].InnerText;

                                if (!string.IsNullOrEmpty(processflag))
                                {
                                    if (port.File.Type != ePortType.UnloadingPort)
                                    {
                                        if (processflag.Equals("Y"))
                                        {
                                            foreach (Equipment equipment in eqpList)
                                            {
                                                if (equipment.File.EquipmentRunMode.Equals(chamberrunmode))
                                                {
                                                    Result = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Result = true;
                                    }
                                }
                            }
                            if (!Result)
                            {
                                errMsg = string.Format("Cassette Data Transfer Error: Can't found Equipment Run Mode in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                    eqp.Data.NODENO, port.Data.PORTNO);
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                                }
                                // Send CIM Message to EQP
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't found Equipment Run Mode in Cassette MAP Data.", "MES" });
                                return false;
                            }
                        }
                        #endregion
                        break;

                    case eLineType.CF.FCREP_TYPE1:
                    case eLineType.CF.FCREP_TYPE2:
                    case eLineType.CF.FCREP_TYPE3:// 20160509 Add by Frank

                        // Add by Kasim 20150509 Unloader Port 不做任何檢查
                        if (port.File.Type == ePortType.UnloadingPort)
                            return true;

                        // Repair Line 的 Line Operation Mode 為 Normal，此時來一個CST全都為 IR的玻璃時，要直接退。
                        #region Check Equipment Run Mode
                        int productCount = 0;
                        int allProductCount = 0;
                        eqpList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Where(e => e.Data.NODENO != "L2" && e.File.EquipmentRunMode != "0").ToList();
                        if (eqpList.Select(e => e.File.EquipmentRunMode).Distinct().Count() == 1)
                        {                        
                            if (line.File.LineOperMode.ToUpper() == "NORMAL")
                            {
                                productCount = 0;
                                allProductCount = 0;
                                foreach (XmlNode n in lotNodeList)
                                {
                                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                                    allProductCount += productList.Count;
                                    for (int i = 0; i < productList.Count; i++)
                                    {
                                        if (productList[i][keyHost.PRODUCTGRADE].InnerText == "IR")// 20160509 Modify "DR" --> "IR" by Frank
                                        {
                                            productCount += 1;
                                        }
                                    }
                                }

                                if (productCount == allProductCount)
                                {
                                    errMsg = string.Format("Cassette Data Transfer Error: Can't process product grade(DR)'s Job. Equipment=[{0}], Port=[{1}]",
                                        eqp.Data.NODENO, port.Data.PORTNO);
                                    lock (cst)
                                    {
                                        cst.ReasonCode = reasonCode;
                                        cst.ReasonText = ERR_CST_MAP.GLASS_DATA_TRANSFER_ERROR + "_" + errMsg;
                                    }
                                    // Send CIM Message to EQP
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't process product grade (DR)'s Job", "MES" });
                                    return false;
                                }
                            }
                        }
                        #endregion

                        break;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Check CELL Data
        /// </summary>
        private bool ValidateCassetteCheckData_CELL(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port,
            ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);
                string cstid = body[keyHost.CARRIERNAME].InnerText;
                string portName = body[keyHost.PORTNAME].InnerText;

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                int productQuantity = 0;
                int.TryParse(body[keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);
                string sRanMod = body[keyHost.RANDOMFLAG] == null ? "N" : body[keyHost.RANDOMFLAG].InnerText;  //add  RanMod by Cell zhuxingxing 20160930

                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;

                #region Check Port (Common condition)
                if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
                {
                    errMsg = "CASSETTE DATA TRANSFER ERROR: UNLOADER PORT LOADS FULL CST, UNABLE TO RECEIVE GLASS.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                    }
                    return false;
                }
                else if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
                {
                    errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: CASSETTE SLOTMAP MISMATCH PORTTYPE=[{0}) PARTIALFULLFLAG=[{1}) PRODUCTQUANTITY=[{2}]",
                                            port.File.Type, port.File.PartialFullFlag, productQuantity);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                    }
                    return false;
                }
                else if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Count In Cassette，不需要進行比對
                {
                    if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0)) //防止Loader/Both Port上空卡匣
                    {
                        errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: {0} LOADS EMPTY CST , UNABLE TO FETCH GLASS.", port.File.Type.ToString());
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                        }
                        return false;
                    }
                }

                #endregion

                #region Check Port (Special condition by Line Type)
                switch (line.Data.LINETYPE)
                {
                    #region [T2 USE]
                    case eLineType.CELL.CBCUT_1:
                    case eLineType.CELL.CBCUT_2:
                    case eLineType.CELL.CBCUT_3:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                for (int i = 0; i < lotNodeList.Count; i++)
                                {
                                    string[] subProLine = lotNodeList[i][keyHost.SUBPRODUCTLINES].InnerText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    ArrayList result = new ArrayList();
                                    foreach (string proLine in subProLine)
                                    {
                                        if (!result.Contains(proLine.ToString()) && proLine.Trim() != "0")
                                            result.Add(proLine.ToString());
                                    }
                                    if (result.Count > 1)
                                    {
                                        lock (cst)
                                        {
                                            cst.CrossLineFlag = "Y";
                                            ObjectManager.CassetteManager.EnqueueSave(cst);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case eLineType.CELL.CBPOL_1:
                    case eLineType.CELL.CBPOL_2:
                    case eLineType.CELL.CBPOL_3:
                        //Jun Add 20150601 CSOT要求POL的LD Port要卡是否混Grade，而且還需要有開關
                        if (ParameterManager["CELL_POL_LD_CHECKGRADE"].GetBoolean())
                        {
                            if (port.File.Type == ePortType.LoadingPort)
                            {
                                if (lotNodeList.Count > 0)
                                {
                                    foreach (XmlNode n in lotNodeList)
                                    {
                                        XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;

                                        ArrayList result = new ArrayList();
                                        foreach (XmlNode product in productList)
                                        {
                                            if (!result.Contains(product[keyHost.PRODUCTGRADE].InnerText))
                                                result.Add(product[keyHost.PRODUCTGRADE].InnerText);
                                        }
                                        if (result.Count > 1)
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE IS MIX PRODUCT GRADE, BC AUTO CANCEL CASSETTE.");
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_POL_LD_CHECK_ERROR + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }

                        if (port.File.Type == ePortType.UnloadingPort)
                        {
                            //ArrayList result = new ArrayList();

                            //當POL ULD上Partial Cassette時,若該值為N則不可上Port
                            if (productQuantity > 0 && body[keyHost.AUTOCLAVESKIP].InnerText == "N")
                            {
                                if (lotNodeList.Count > 0)
                                {
                                    foreach (XmlNode n in lotNodeList)
                                    {
                                        XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;

                                        //當POL ULD上Partial Cassette時, Job Grade只能上P7的等級
                                        foreach (XmlNode product in productList)
                                        {
                                            if (productQuantity > 0 && product[keyHost.PRODUCTGRADE].InnerText != "P7")
                                            {
                                                errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PARTIAL FULL CASSETTE DATA PRODUCTQUANTITY=[{0}], BUT DOWNLOAD PRODUCTGRADE!=[P7].", productQuantity);
                                                lock (cst)
                                                {
                                                    cst.ReasonCode = reasonCode;
                                                    cst.ReasonText = ERR_CST_MAP.CELL_POL_ULDPARTIALCST_CHECK_ERROR + "_" + errMsg;
                                                }
                                                return false;
                                            }

                                            //if (!result.Contains(product[keyHost.PRODUCTGRADE].InnerText))
                                            //    result.Add(product[keyHost.PRODUCTGRADE].InnerText);
                                        }
                                    }
                                }

                                //if (result[0].ToString() != "P7")
                                //{
                                //    errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PARTIAL FULL CASSETTE DATA PRODUCTQUANTITY=[{0}], BUT DOWNLOAD AUTOCLAVESKIPFLAG=[N].", productQuantity);
                                //    lock (cst)
                                //    {
                                //        cst.ReasonCode = reasonCode;
                                //        cst.ReasonText = ERR_CST_MAP.CELL_POL_ULDPARTIALCST_CHECK_ERROR;
                                //    }
                                //    return false;
                                //}
                            }

                            if (productQuantity > 0) lock (cst) cst.CellBoxProcessed = eboxReport.Processing;
                            ObjectManager.CassetteManager.EnqueueSave(cst);
                        }
                        break;
                    //Jun Add 20150328 PRM需要卡不能上混Grade的Glass，且還需要卡Grade跟RunMode的一致
                    case eLineType.CELL.CBPRM:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;

                                    ArrayList result = new ArrayList();
                                    foreach (XmlNode product in productList)
                                    {
                                        if (!result.Contains(product[keyHost.PRODUCTGRADE].InnerText))
                                            result.Add(product[keyHost.PRODUCTGRADE].InnerText);
                                    }
                                    if (result.Count > 1)
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE IS MIX PRODUCT GRADE, BC AUTO CANCEL CASSETTE.");
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_PRM_LD_CHECK_ERROR + "_" + errMsg;
                                        }
                                        return false;
                                    }

                                    bool autoCancel = false;
                                    switch (line.File.CellLineOperMode)
                                    {
                                        //雙面撕除 - P5, P6
                                        case "1": //Remove Mode(TFT + CF)
                                        case "4": //Remove Mode(Virtual Loader Port TFT + CF)
                                        case "7": //Remove Mode(Virtual Loader + Unloader Port TFT + CF)
                                            if (result[0].ToString() != "P5") autoCancel = true;
                                            break;

                                        //TFT撕除 - PT
                                        case "2": //Remove Mode(TFT)
                                        case "5": //Remove Mode(Virtual Loader Port TFT)
                                        case "8": //Remove Mode(Virtual Loader + Unloader Port TFT)
                                            if (result[0].ToString() != "PT") autoCancel = true;
                                            break;

                                        //CF撕除 - PC
                                        case "3": //Remove Mode(CF)
                                        case "6": //Remove Mode(Virtual Loader Port CF)
                                        case "9": //Remove Mode(Virtual Loader + Unloader Port CF)
                                            if (result[0].ToString() != "PC") autoCancel = true;
                                            break;

                                        default:
                                            autoCancel = true;
                                            break;
                                    }
                                    if (autoCancel)
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCT_GRADE=[{0}], CAN'T RUN IN CURRENT RUN_MODE=[{1}]({2})"
                                            , result[0].ToString(), line.File.CellLineOperMode, line.File.LineOperMode);
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_PRM_LD_CHECK_ERROR + "_" + errMsg;
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                        break;

                    //Jun Add 20150603 Judge由机台比对. 比如机台切换为RW, 但玻璃Judge是OK, 则Download资料给机台时,机台要回复NG.
                    case eLineType.CELL.CBPMT:
                        if (port.File.Mode == ePortMode.Rework)
                        {
                            if (line.File.CellLineOperMode != "1" && line.File.CellLineOperMode != "7")
                            {
                                errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: PORT_MODE=[{0}], CAN'T RUN IN CURRENT RUN_MODE=[{1}]({2}).", port.File.Mode.ToString(), line.File.CellLineOperMode, line.File.LineOperMode);
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.CELL_PMT_PORTMODE_CHECK_ERROR + "_" + errMsg;
                                }
                                return false;
                            }
                        }
                        break;
                    #endregion
                    case eLineType.CELL.CCPIL:
                    case eLineType.CELL.CCPIL_2:
                        //T3 TODO
                        break;
                    #region [CCGAP]
                    case eLineType.CELL.CCGAP:
                        if (port.File.Type != ePortType.UnloadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                for (int i = 0; i < lotNodeList.Count; i++)
                                {
                                    string[] lotPPID = lotNodeList[i][keyHost.PPID].InnerText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (port.File.PortAssignment == eCELLPortAssignment.UNKNOW)
                                    {
                                        errMsg = string.Format("PortAssignment Unknow!!!");
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.PORY_ASSIGNMENT_UNKNOW;
                                        }
                                        return false;
                                    }
                                    switch (eqp.File.EquipmentRunMode)
                                    {
                                        case "GAPANDSORTERMODE":
                                            #region [Check GAP]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GAP)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case "GMIANDSORTERMODE":
                                            #region [Check GMI]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GMI)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case "GAPANDGMIMODE":
                                            #region [Check GAP]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GAP)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            #region [Check GMI]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GMI)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        default:
                                            errMsg = string.Format("RunMode Unknow!!!");
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.DIFFERENT_EQP_RUNMODE;
                                            }
                                            return false;
                                    }
                                }
                            }
                        }
                    #endregion
                        break;


                    case eLineType.CELL.CCPDR:
                        //20170604 huangjiayin add For PDR

                        #region [CCPDR]
                        if (port.File.Type != ePortType.UnloadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                for (int i = 0; i < lotNodeList.Count; i++)
                                {
                                    string[] lotPPID = lotNodeList[i][keyHost.PPID].InnerText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (port.File.PortAssignment == eCELLPortAssignment.UNKNOW)
                                    {
                                        errMsg = string.Format("PortAssignment Unknow!!!");
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.PORY_ASSIGNMENT_UNKNOW;
                                        }
                                        return false;
                                    }
                                    switch (eqp.File.EquipmentRunMode)
                                    {
                                        case "NORMAL":
                                            #region [Check PDR]
                                            if (port.File.PortAssignment == eCELLPortAssignment.PDR)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L3" || pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]PDR RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L5")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]CEM RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region [Check CEM]
                                            else if (port.File.PortAssignment == eCELLPortAssignment.CEM)
                                            {
                                                foreach (string pPID in lotPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L5")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]CEM RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L3" || pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]PDR RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            lock (cst)
                                                            {
                                                                cst.ReasonCode = reasonCode;
                                                                cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG + "_" + errMsg;
                                                            }
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            break;

                                        case "SORTERMODE":
                                            break;

                                        default:
                                            errMsg = string.Format("RunMode Unknow!!!");
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.DIFFERENT_EQP_RUNMODE;
                                            }
                                            return false;
                                            break;

                                    }
                                }
                            }
                        }
                            
#endregion
                          break;




                }
                #endregion

                #region [PCS Check SUBPRODUCTSPECS Data]
                foreach (XmlNode n in lotNodeList)
                {
                    List<string> pcsSubProuctSpecs = new List<string>();//sy 20151217 add for CCPCS
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)//sy 20151217 add for CCPCS
                    {
                        pcsSubProuctSpecs.Add(n[keyHost.SUBPRODUCTSPECS].InnerText.Split(';')[0]);//先將第一筆加入
                        foreach (string SubProuctSpec in n[keyHost.SUBPRODUCTSPECS].InnerText.Split(';'))
                        {
                            bool same = false;
                            foreach (string pcsSubProuctSpec in pcsSubProuctSpecs)
                            {
                                if (pcsSubProuctSpec == SubProuctSpec) same = true;
                            }
                            if (!same)
                                pcsSubProuctSpecs.Add(SubProuctSpec);
                        }
                    }
                    if (pcsSubProuctSpecs.Count > 2)// 等切只有一種 不等切最多二種
                    {
                        errMsg = string.Format("PCS CUT SubProuctSpecs Error");
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_SUBPRODUCTSPECS_ERROR;
                        }
                        return false;
                    }
                }
                #endregion
                //Jun Modify 20150327 CSOT要求 CELL Line不卡Port Mode與Job Type的一致
                #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
                //// 檢查 Port mode(EQP) 和 MES 下來的 ProductType(Job Data) 是否一致，若不一致要退 CST。
                //string jobType = string.Empty;
                //foreach (XmlNode n in lotNodeList)
                //{
                //    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;

                //    for (int i = 0; i < productList.Count; i++)
                //    {
                //        string productType = productList[i][keyHost.PRODUCTTYPE].InnerText.Trim();

                //        if (string.IsNullOrEmpty(productType))
                //        {
                //            errMsg = string.Format("Cassette MAP Transfer Error: Mes Job Product Type Invalid. Equipment=[{0}], Port=[{1}]",
                //                    eqp.Data.NODENO, port.Data.PORTNO);
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.CST_MAP_TRANSFER_ERROR;
                //            }
                //            return false;
                //        }
                //        else
                //        {
                //            bool checkng = false;
                //            //Jun Modify 20141208 Cell後段Unloader Port邏輯不同，所以移到Cell Special Check
                //            if (port.File.Type == ePortType.LoadingPort)
                //            {
                //                switch (productType)
                //                {
                //                    case eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT: if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.MIX) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT: if (port.File.Mode != ePortMode.CF && port.File.Mode != ePortMode.MIX) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.GENERAL_DUMMY: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.THROUGH_DUMMY: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.THICKNESS_DUMMY: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.UV_MASK: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
                //                    default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                //                }
                //            }
                //            else
                //            {
                //                switch (productType)
                //                {
                //                    case eMES_PRODUCT_TYPE.THROUGH_DUMMY: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                //                    case eMES_PRODUCT_TYPE.THICKNESS_DUMMY: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                //                    default: if (port.File.Mode == ePortMode.Dummy) checkng = true; break;
                //                }
                //            }

                //            if (checkng)
                //            {
                //                errMsg = string.Format("Cassette MAP Transfer Error: Mes Job Product Type[{0}] mismatch with EQP Port Mode[{1}]. Equipment=[{2}], Port=[{3}]",
                //                    productType,port.File.Mode,eqp.Data.NODENO, port.Data.PORTNO);
                //                lock (cst)
                //                {
                //                    cst.ReasonCode = reasonCode;
                //                    cst.ReasonText = ERR_CST_MAP.CST_MAP_TRANSFER_ERROR;
                //                }
                //                return false;
                //            }

                //        }
                //    }
                //}
                #endregion

                #region Check Job Data
                switch (line.Data.LINETYPE)
                {
                    #region [T2 USE]
                    case eLineType.CELL.CBPIL:
                    case eLineType.CELL.CBODF:
                    case eLineType.CELL.CBHVA:
                    case eLineType.CELL.CBGAP:
                    case eLineType.CELL.CBPMT:
                    case eLineType.CELL.CBUVA:
                    case eLineType.CELL.CBMCL:
                    case eLineType.CELL.CBATS:
                        break;

                    case eLineType.CELL.CBCUT_1:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    XmlNodeList processList = n[keyHost.PROCESSLINELIST].ChildNodes;
                                    XmlNodeList stbList = n[keyHost.STBPRODUCTSPECLIST].ChildNodes;

                                    if (string.IsNullOrEmpty(n[keyHost.PRDCARRIERSETCODE].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.PRDCARRIERSETCODE].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }
                                    if (string.IsNullOrEmpty(n[keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(n[keyHost.BCPRODUCTID].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.BCPRODUCTTYPE].InnerText.Trim(), n[keyHost.BCPRODUCTID].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }

                                    if (processList.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(processList[0][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim(), processList[0][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                    if (processList.Count > 1)
                                    {
                                        if (string.IsNullOrEmpty(processList[1][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , processList[1][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(processList[1][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(processList[1][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , processList[1][keyHost.BCPRODUCTTYPE].InnerText.Trim(), processList[1][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case eLineType.CELL.CBCUT_2:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    XmlNodeList processList = n[keyHost.PROCESSLINELIST].ChildNodes;
                                    XmlNodeList stbList = n[keyHost.STBPRODUCTSPECLIST].ChildNodes;

                                    if (string.IsNullOrEmpty(n[keyHost.PRDCARRIERSETCODE].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.PRDCARRIERSETCODE].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }
                                    if (string.IsNullOrEmpty(n[keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(n[keyHost.BCPRODUCTID].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.BCPRODUCTTYPE].InnerText.Trim(), n[keyHost.BCPRODUCTID].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }

                                    if (processList.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(processList[0][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim(), processList[0][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                    if (processList.Count > 1)
                                    {
                                        if (string.IsNullOrEmpty(processList[1][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , processList[1][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(processList[1][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(processList[1][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , processList[1][keyHost.BCPRODUCTTYPE].InnerText.Trim(), processList[1][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                    if (stbList.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(stbList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(stbList[0][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , stbList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim(), stbList[0][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case eLineType.CELL.CBCUT_3:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    XmlNodeList processList = n[keyHost.PROCESSLINELIST].ChildNodes;
                                    XmlNodeList stbList = n[keyHost.STBPRODUCTSPECLIST].ChildNodes;

                                    if (string.IsNullOrEmpty(n[keyHost.PRDCARRIERSETCODE].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.PRDCARRIERSETCODE].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }
                                    if (string.IsNullOrEmpty(n[keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(n[keyHost.BCPRODUCTID].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.BCPRODUCTTYPE].InnerText.Trim(), n[keyHost.BCPRODUCTID].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }

                                    if (processList.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(processList[0][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(processList[0][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , processList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim(), processList[0][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                    if (stbList.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                                , stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                        if (string.IsNullOrEmpty(stbList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim()) || string.IsNullOrEmpty(stbList[0][keyHost.BCPRODUCTID].InnerText.Trim()))
                                        {
                                            errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD PRODUCTTYPE OR PRODUCTID IS NULL, PRODUCTTYPE=[{0}] PRODUCTID=[{1}], BC AUTO CANCEL CASSETTE."
                                                , stbList[0][keyHost.BCPRODUCTTYPE].InnerText.Trim(), stbList[0][keyHost.BCPRODUCTID].InnerText.Trim());
                                            lock (cst)
                                            {
                                                cst.ReasonCode = reasonCode;
                                                cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL + "_" + errMsg;
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    #endregion
                    default:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    if (string.IsNullOrEmpty(n[keyHost.PRDCARRIERSETCODE].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.PRDCARRIERSETCODE].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" + errMsg;
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                        break;
                }

                #endregion
                #region Check Sort/CHN RANDOMFLAG MODE FOR T3 
                // add by zhuxingxing 20160923 RANDOMMODE CHECK RanModFlag is Y
                //fix by huangjiayin 20170511 RANDOMMODE Name Modify
                //fix by huangjiayin 20170517 port type condition
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CCCHN:
                    case eLineType.CELL.CCSOR:

                        if (port.File.Type == ePortType.UnloadingPort) return true;//unloading port no check randomflag


                        bool eqp_rdm = false;// eqprunmode is randomed?...

                        if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT" || eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN") eqp_rdm = true;
                            //run mode name modify...
                        if ((eqp_rdm && sRanMod == "Y") || (!eqp_rdm && sRanMod != "Y"))
                        { return true; }
                        else
                        {
                            errMsg = string.Format("RandomMod Check Error: MES CST Body Random Flag=[{0}] ,But Equipment RunMod=[{1}]", sRanMod, eqp.File.EquipmentRunMode);
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR + "_" + errMsg;
                                return false;
                        }

                    default:
                        break;

                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
        #endregion

                /// <summary>
        /// Check CELL Data
        /// </summary>
        private bool ValidateCassetteCheckData_MODULE(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port,
            ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);
                string cstid = body[keyHost.CARRIERNAME].InnerText;
                string portName = body[keyHost.PORTNAME].InnerText;

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                int productQuantity=0;
                int.TryParse(body[keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);

                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;         

                #region Check Port (Common condition)
                if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
                {
                    errMsg = "CASSETTE DATA TRANSFER ERROR: UNLOADER PORT LOADS FULL CST, UNABLE TO RECEIVE GLASS.";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" +  errMsg;
                    }
                    return false;
                }
                else if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
                {
                    errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: CASSETTE SLOTMAP MISMATCH PORTTYPE=[{0}) PARTIALFULLFLAG=[{1}) PRODUCTQUANTITY=[{2}]",
                                            port.File.Type, port.File.PartialFullFlag, productQuantity);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" +  errMsg;
                    }
                    return false;
                }
                else if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Count In Cassette，不需要進行比對
                {
                    if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0)) //防止Loader/Both Port上空卡匣
                    {
                        errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: {0} LOADS EMPTY CST , UNABLE TO FETCH GLASS.", port.File.Type.ToString());
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" +  errMsg;
                        }
                        return false;
                    }
                }

                #endregion
                
                #region Check Port (Special condition by Line Type)
                switch (line.Data.LINETYPE)
                {
                    default:
                        break;
                }
                #endregion

                #region Check Job Data
                switch (line.Data.LINETYPE)
                {                    
                    default:
                        if (port.File.Type == ePortType.LoadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                foreach (XmlNode n in lotNodeList)
                                {
                                    if (string.IsNullOrEmpty(n[keyHost.PRDCARRIERSETCODE].InnerText.Trim()))
                                    {
                                        errMsg = string.Format("MES CASSETTE DATA ERROR: MES DOWNLOAD CASSETTE_SETTING_CODE IS NULL, BC AUTO CANCEL CASSETTE."
                                            , n[keyHost.PRDCARRIERSETCODE].InnerText.Trim());
                                        lock (cst)
                                        {
                                            cst.ReasonCode = reasonCode;
                                            cst.ReasonText = ERR_CST_MAP.CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL + "_" +  errMsg;
                                        }
                                        return false;
                                    }
                                }
                            }
                        }
                        break;
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        #region [MES資料填入Job Data 各Shop Special區塊]
        private void M2P_SpecialDataBy_Array(XmlNode lotNode, XmlNode productNode, Line line, Port port, ref Dictionary<string, string> recipeGroup, ref Job job)
        {
            try
            {
                if (eLineType.ARRAY.TTP_VTEC == line.Data.LINETYPE)
                    job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, productNode[keyHost.AGINGENABLE].InnerText.Trim());
                else if (eLineType.ARRAY.BFG_SHUZTUNG == line.Data.LINETYPE)
                    job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, productNode[keyHost.SCRAPCUTFLAG].InnerText.Trim());
                else
                    job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, "");

                job.ArraySpecial.ProcessType = GetArrayProcessType(lotNode, productNode, line);

                job.InspJudgedData = new string('0', 32);

                job.ArraySpecial.LastMainPPID = productNode[keyHost.LASTMAINPPID].InnerText;
                job.ArraySpecial.LastMainEqpName = productNode[keyHost.LASTMAINEQPNAME].InnerText;
                job.ArraySpecial.LastMainChamberName = productNode[keyHost.LASTMAINCHAMBERNAME].InnerText;
                //add by qiumin 20171017 ELA one by one run
                #region  ELA1BY1Flag
                if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                {
                    Equipment ela;
                      
                    ela = ObjectManager.EquipmentManager.GetEQP("L4");
                    string  ela1_PPID = job.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                    ela = ObjectManager.EquipmentManager.GetEQP("L5");
                    string  ela2_PPID = job.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                    string  curByPassPPID = new string('0', ela.Data.RECIPELEN);
                    if (ela1_PPID != curByPassPPID && ela2_PPID == curByPassPPID)
                    {
                        job.ArraySpecial.ELA1BY1Flag = "L4";
                    }
                    else if (ela1_PPID == curByPassPPID && ela2_PPID != curByPassPPID)
                    {
                        job.ArraySpecial.ELA1BY1Flag = "L5";
                    }
                    else
                    {
                        job.ArraySpecial.ELA1BY1Flag = "L45";
                    }
                }
                #endregion

                #region EQP Flag By LINE SPECIAL
                IDictionary<string, string> eqpData = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");

                //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.PROCESS_EQ) t3 just use in Photo line cc.kuang 2015/07/08
                //add by yang 2017/5/23  DRY_TEL also need thickness
                if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE || line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP||line.Data.LINETYPE==eLineType.ARRAY.DRY_TEL)
                {
                    if (eqpData.ContainsKey(eEQPFLAG.Array.Thickness))
                        eqpData[eEQPFLAG.Array.Thickness] = ConstantManager["PLC_ARRAY_THICKNESS"][lotNode[keyHost.PRODUCTTHICKNESS].InnerText].Value;
                }

                //t3 use for MAC cc.kuang 2015/07/08
                if ((line.Data.LINETYPE == eLineType.ARRAY.MAC_CONTREL) || (line.Data.LINETYPE == eLineType.ARRAY.CLS_MACAOH))  // modify by bruce 20160107 for macro glass turn use
                {
                    if (eqpData.ContainsKey(eEQPFLAG.Array.MAC_TurnModeFlag))
                    {
                        if ("Y" == productNode[keyHost.GLASSTURNFLAG].InnerText.Trim())
                            eqpData[eEQPFLAG.Array.MAC_TurnModeFlag] = "1";
                        else
                            eqpData[eEQPFLAG.Array.MAC_TurnModeFlag] = "0";
                    }
                }

                //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.ABFG) //modify by bruce 2015/7/15 改用Line Type 判斷
                if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG)
                {
                    switch (productNode[keyHost.SCRAPCUTFLAG].InnerText.Trim())
                    {
                        case "S":
                            /* t3 not define scrap flag cc.kuang 2015/07/08
                            if (eqpData.ContainsKey(eEQPFLAG.Array.ScrapFlag))
                            {
                                eqpData[eEQPFLAG.Array.ScrapFlag] = "1";
                                if (eqpData.ContainsKey(eEQPFLAG.Array.SmashFlag))
                                    eqpData[eEQPFLAG.Array.SmashFlag] = "1";
                            }*/
                            if (eqpData.ContainsKey(eEQPFLAG.Array.SmashFlag))
                                eqpData[eEQPFLAG.Array.SmashFlag] = "1";
                            break;
                        case "C":
                            /* t3 not define scrap flag cc.kuang 2015/07/08
                            if (eqpData.ContainsKey(eEQPFLAG.Array.ScrapFlag))
                            {
                                eqpData[eEQPFLAG.Array.ScrapFlag] = "1";
                                if (eqpData.ContainsKey(eEQPFLAG.Array.CutFlag))
                                    eqpData[eEQPFLAG.Array.CutFlag] = "1";
                            }*/
                            if (eqpData.ContainsKey(eEQPFLAG.Array.CutFlag))
                                eqpData[eEQPFLAG.Array.CutFlag] = "1";
                            break;
                    }
                }

                job.EQPFlag = ObjectManager.SubJobDataManager.Encode(eqpData, "EQPFlag");
                #endregion

                //t3 each line has RecipeGroupNumber cc.kuang 2015/07/08
                //check cst on port's recipe group nember
                if (recipeGroup.Count == 0)
                {
                    List<Port> lstPort;
                    lstPort = ObjectManager.PortManager.GetPorts();
                    foreach (Port pt in lstPort)
                    {
                        if (pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA || pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING
                            || pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND
                            || pt.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || pt.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                        {
                            IList<Job> lstJob;
                            lstJob = ObjectManager.JobManager.GetJobs(pt.File.CassetteSequenceNo);
                            foreach (Job jb in lstJob)
                            {
                                if (jb.SamplingSlotFlag == "1")
                                {
                                    if (jb.ArraySpecial.RecipeGroupNumber.Trim().Length == 0)
                                        continue;

                                    if (int.Parse(jb.ArraySpecial.RecipeGroupNumber.Trim()) <= 0 || int.Parse(jb.ArraySpecial.RecipeGroupNumber.Trim()) > 30)
                                        continue;

                                    if (!recipeGroup.ContainsKey(jb.PPID))
                                    {
                                        recipeGroup.Add(jb.PPID, jb.ArraySpecial.RecipeGroupNumber.Trim());
                                    }
                                }
                            }
                        }
                    }
                }

                if (true)
                {
                    #region  Recipe Group Number
                    if (!recipeGroup.ContainsKey(job.PPID))
                    {
                        //recipeGroup.Add(job.PPID, (recipeGroup.Count + 1).ToString());
                        int i;
                        for (i = 1; i < 31; i++)
                        {
                            var item = recipeGroup.Where(k => k.Value.Trim().Equals(i.ToString())).Select(k => (KeyValuePair<string, string>?)k).FirstOrDefault();
                            if (null == item)
                                break;
                        }

                        if (i >= 31)
                        {
                            recipeGroup.Add(job.PPID, "0"); //array over 30, set 0 and quit cst
                        }
                        else
                        {
                            if (recipeGroup.Count == 0)
                                recipeGroup.Add(job.PPID, "1");
                            else
                                recipeGroup.Add(job.PPID, i.ToString());
                        }
                    }
                    job.ArraySpecial.RecipeGroupNumber = recipeGroup[job.PPID];
                    #endregion
                }

                job.ArraySpecial.SourcePortNo = port.Data.PORTNO;

                //Run Mode check 2015/10/02 cc.kuang
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                switch (line.Data.LINETYPE) 
                { 
                    case eLineType.ARRAY.MSP_ULVAC:
                    case eLineType.ARRAY.ITO_ULVAC:
                        {                    
                            Equipment indexer = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L2"));
                            Equipment pvd = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L4"));
                            string chambermode1 = string.Empty;
                            string chambermode2 = string.Empty;
                            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                            {
                                if (pvd.File.EquipmentRunMode == "MIX") 
                                {
                                    if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE)
                                    {
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "PVD EQ Run Mode[Mix] not Match Indexer Operation Mode[" + line.File.IndexOperMode + "] Error!");
                                    }

                                    chambermode1 = ConstantManager["ARRAY_CHAMBERMODE_PVD"].Values[indexer.File.ProportionalRule01Type.ToString()].Discription;
                                    chambermode2 = ConstantManager["ARRAY_CHAMBERMODE_PVD"].Values[indexer.File.ProportionalRule02Type.ToString()].Discription;
                                    if (!productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim().Equals(chambermode1) && !productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim().Equals(chambermode2))
                                    {
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] CHAMBERRUNMODE[" +
                                            productNode[keyHost.CHAMBERRUNMODE].InnerText + "] Not Match Indexer's Proportional Rule["
                                            + chambermode1 + "],[" + chambermode2 + "] Error!");
                                    }
                                }
                            }
                        }
                        break;
                    //case eLineType.ARRAY.OVNITO_CSUN: not need check 2016/06/30 cc.kuang
                    //case eLineType.ARRAY.OVNPL_YAC: not need check 2016/06/08 cc.kuang
                    //case eLineType.ARRAY.OVNSD_VIATRON:
                    /*    {
                            Equipment ovn = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L3"));
                            if (ovn.File.EquipmentRunMode == "MQC")
                            {
                                if (job.ArraySpecial.ProcessType == "0")
                                {
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match EQ Run Mode[MQC] Error!");
                                }
                            }
                            else if (ovn.File.EquipmentRunMode.ToUpper() == "NORMAL")
                            {
                                if (job.ArraySpecial.ProcessType == "1")
                                {
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match EQ Run Mode[Normal] Error!");
                                }
                            }
                        }
                        break;
                    */
                    case eLineType.ARRAY.ELA_JSW:
                        {
                            Equipment ela1 = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L4"));
                            Equipment ela2 = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L5"));
                            int glassflow = int.Parse(job.ArraySpecial.GlassFlowType);
                            char ela1Flg = Convert.ToString(glassflow, 2).PadLeft(4, '0')[2]; //modify by yang 20161110
                            char ela2Flg = Convert.ToString(glassflow, 2).PadLeft(4, '0')[1]; 
                            if (job.ArraySpecial.ProcessType == "1")
                            {
                                if (ela1Flg == '1' && ela2Flg == '1')
                                {
                                    if (!ela1.File.EquipmentRunMode.Equals("MQC") && !ela2.File.EquipmentRunMode.Equals("MQC"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA1&ELA2 Run Mode Error!");
                                }
                                else if (ela1Flg == '1')
                                {
                                    if (!ela1.File.EquipmentRunMode.Equals("MQC"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA1 Run Mode Error!");
                                }
                                else if (ela2Flg == '1')
                                {
                                    if (!ela2.File.EquipmentRunMode.Equals("MQC"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA2 Run Mode Error!");
                                }
                            }
                            else if (job.ArraySpecial.ProcessType == "0")
                            {
                                if (ela1Flg == '1' && ela2Flg == '1')
                                {
                                    if (!ela1.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && !ela2.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA1&ELA2 Run Mode Error!");
                                }
                                else if (ela1Flg == '1')
                                {
                                    if (!ela1.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA1 Run Mode Error!");
                                }
                                else if (ela2Flg == '1')
                                {
                                    if (!ela2.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] PRODUCTTYPE[" +
                                            productNode[keyHost.PRODUCTTYPE].InnerText + "] Not Match ELA2 Run Mode Error!");
                                }
                            }
                        }
                        break;
                    case eLineType.ARRAY.CVD_AKT:
                    case eLineType.ARRAY.CVD_ULVAC:
                        {
                            string chambermode;
                            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE)
                                break;

                            Equipment eq = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L4"));
                            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eq.Data.NODENO);
                            chambermode = productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim();
                            if (chambermode.Length > 0)                            
                            {
                                if (null == units.FirstOrDefault(u => (u.File.ChamberRunMode == chambermode) || (u.File.RunMode == chambermode)))
                                {
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] CHAMBERRUNMODE[" +
                                            productNode[keyHost.CHAMBERRUNMODE].InnerText + "] Not Match EQ's Chamber Mode!");
                                }
                            }
                        }
                        break;
                    case eLineType.ARRAY.DRY_ICD:
                    case eLineType.ARRAY.DRY_YAC:
                    case eLineType.ARRAY.DRY_TEL:
                        {
                            string chambermode;
                            Equipment eq = eqps.FirstOrDefault(e => e.Data.NODENO.Equals("L4"));
                            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eq.Data.NODENO);
                            chambermode = productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim();
                            if (chambermode.Length > 0)                            
                            {
                                if (null == units.FirstOrDefault(u => (u.File.ChamberRunMode == chambermode) || (u.File.RunMode == chambermode)))
                                {
                                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, "MES Slot[" + productNode[keyHost.POSITION].InnerText.Trim() + "] CHAMBERRUNMODE[" +
                                            productNode[keyHost.CHAMBERRUNMODE].InnerText + "] Not Match EQ's Chamber Mode!");
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                /* t3 not use item cc.kuang 2015/07/08
                if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT)
                {
                    job.ArraySpecial.SorterGrade = job.JobGrade;
                }
                */
                /* t3 not use item cc.kuang 2015/07/08
                switch (line.Data.LINETYPE)
                {
                    case eLineType.ARRAY.TTP_VTEC:
                    case eLineType.ARRAY.CLS_MACAOH:
                    case eLineType.ARRAY.CLS_PROCDO:
                    case eLineType.ARRAY.NAN_SEMILAB:
                        // 取出 Abnormal Code List 內容
                        XmlNodeList abnormalcodelist = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                        foreach (XmlNode abnormalcode in abnormalcodelist)
                        {
                            string abnormalSeq = abnormalcode[keyHost.ABNORMALSEQ].InnerText.Trim();

                            switch (abnormalSeq.ToUpper())
                            {
                                case "SB_HP_NUM":
                                    job.ArraySpecial.DNS_SB_HP_NUM = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                    break;
                                case "SB_CP_NUM":
                                    job.ArraySpecial.DNS_SB_CP_NUM = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                    break;
                                case "HB_HP_NUM":
                                    job.ArraySpecial.DNS_HB_HP_NUM = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                    break;
                                case "VCD_NUM":
                                    job.ArraySpecial.DNS_VCD_NUM = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                    break;
                            }
                        }
                        break;
                }
                */
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                throw (ex);
            }
        }

        private string GetArrayGlassFlowType(Line line, string ppid, string lineSpecial)
        {
            switch (line.Data.LINETYPE)
            {
                case eLineType.ARRAY.PHL_EDGEEXP:
                case eLineType.ARRAY.PHL_TITLE:
                case eLineType.ARRAY.ELA_JSW:
                    {   // Loader(2碼) + Cleaner(4碼) + EQ1(ELA 12碼) + EQ2(ELA 12碼) + Cleaner(4碼)
                        //  OO            OOOO          OOOOOOOOOOOO    OOOOOOOOOOOO      OOOO
                        //[1,1,1,1]=[clean1, EQ1, EQ2, clean2]
                        ppid = ppid.PadRight(34, '0');
                        int cleaner1 = ppid.Substring(2, 4).Equals(new string('0', 4)) ? 0 : 1;
                        int eq1 = ppid.Substring(6, 12).Equals(new string('0', 12)) ? 0 : 2;
                        int eq2 = ppid.Substring(18, 12).Equals(new string('0', 12)) ? 0 : 4;
                        int cleaner2 = ppid.Substring(30, 4).Equals(new string('0', 4)) ? 0 : 8;
                        return (cleaner1 + eq1 + eq2 + cleaner2).ToString();
                    }
                case eLineType.ARRAY.CVD_ULVAC:
                case eLineType.ARRAY.CVD_AKT:
                case eLineType.ARRAY.DRY_YAC:
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_TEL:
                    {   // Loader(2碼) + Cleaner(4碼) + EQ(CVD/DRY 12碼)
                        //  OO            OOOO           OOOOOOOOOOOO 
                        ppid = ppid.PadRight(18, '0');
                        string cleaner = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                        string eq = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";
                        return Convert.ToInt32(eq + cleaner, 2).ToString();
                    }
                case eLineType.ARRAY.MSP_ULVAC:
                case eLineType.ARRAY.ITO_ULVAC:
                    {   // Loader(2碼) + Cleaner(4碼) + EQ(PVD 12碼) + Cleaner(4碼)
                        //  OO            OOOO           OOOOOOOOOOOO       OOOO
                        ppid = ppid.PadRight(22, '0');
                        string cleaner = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                        string eq = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";
                        if (ppid.Substring(18, 4).Equals(new string('0', 4)))
                        {                            
                            return Convert.ToInt32(eq + cleaner, 2).ToString();
                        }
                        else
                        {
                            if (Convert.ToInt32(eq + cleaner, 2) == 2)
                                return "5";
                            else if (Convert.ToInt32(eq + cleaner, 2) == 3)
                                return "4";
                            else
                                return "0";
                        }
                    }
                case eLineType.ARRAY.RTA_VIATRON:
                    {   // Loader(2碼) + RTA(12碼) + USC(12碼)
                        ppid = ppid.PadRight(26, '0');
                        string rta = ppid.Substring(2, 12).Equals(new string('0', 12)) ? "0" : "1";
                        string usc = ppid.Substring(14, 12).Equals(new string('0', 12)) ? "0" : "1";
                        return Convert.ToInt32(rta + usc, 2).ToString();
                    }
                case eLineType.ARRAY.WEI_DMS:
                case eLineType.ARRAY.WET_DMS:
                case eLineType.ARRAY.STR_DMS:
                case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                case eLineType.ARRAY.IMP_NISSIN:
                case eLineType.ARRAY.CHN_SEEC:
                case eLineType.ARRAY.OVNSD_VIATRON:
                    {
                        return "1";
                    }
                case eLineType.ARRAY.OVNITO_CSUN:
                    {
                        return "3";
                    }
                case eLineType.ARRAY.OVNPL_YAC:
                    {
                        return "4";
                    }
                case eLineType.ARRAY.BFG_SHUZTUNG:
                    {
                        if (lineSpecial.Trim().Equals("C"))
                            return "1";
                        else if (lineSpecial.Trim().Equals("S"))
                            return "2";
                        else if (lineSpecial.Trim().Equals("CS") || lineSpecial.Trim().Equals("SC"))
                            return "3";
                        else
                            return "0";
                    }
                case eLineType.ARRAY.TTP_VTEC:
                    {
                        if (lineSpecial.Trim().Equals("Y"))
                            return "1";
                        else
                            return "2";
                    }
                case eLineType.ARRAY.CLS_MACAOH:
                case eLineType.ARRAY.CLS_PROCDO:
                    {   // Loader(2碼) + L3(12碼) + L4(12碼)
                        ppid = ppid.PadRight(26, '0');
                        string L3 = ppid.Substring(2, 12).Equals(new string('0', 12)) ? "0" : "1";
                        string L4 = ppid.Substring(14, 12).Equals(new string('0', 12)) ? "0" : "1";
                        return Convert.ToInt32(L4 + L3, 2).ToString();
                    }
                default: return "1";
            }
        }

        private string GetArrayProcessType(XmlNode lotNode, XmlNode productNode, Line line)
        {
            string productProcessType = lotNode[keyHost.PRODUCTPROCESSTYPE].InnerText.Trim();
            string processType = lotNode[keyHost.PROCESSTYPE].InnerText.Trim();
            switch (line.Data.LINETYPE)
            {
                case eLineType.ARRAY.CVD_AKT:
                case eLineType.ARRAY.CVD_ULVAC:
                    if (line.Data.LINEID != "TCCVD700")//modify by hujunpeng 20190424 for CVD700 混run,Deng:20190823.
                    {
                        if (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim() == "MQC")
                            return "1";
                        else
                            return "0";
                    }
                    else {
                        if (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim() == "MQC")
                            return "1";
                        else if (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim() == "PROD")
                            return "0";
                        else if (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim() == "PROD1")
                            return "2";
                        else
                            Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Current jobPorcType{0} is unknow!", productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim()));
                        return "99";
                    }
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_YAC:
                case eLineType.ARRAY.DRY_TEL:
                    switch (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim())
                    {
                        case "MQC":
                            return "1";
                        case "PS":
                            return "2";
                        case "GE":
                            return "3";
                        case "ILD":
                            return "4";
                        case "SD":
                            return "5";
                        case "PV":
                            return "6";
                        case "ASH":
                            return "7";
                        case "PLN":     //bruce modfiy 20160120 MES SPEC 原PL 改為PLN
                            return "8";
                        default:
                            return "0";
                    }
                case eLineType.ARRAY.MSP_ULVAC:
                case eLineType.ARRAY.ITO_ULVAC:
                    switch (productNode[keyHost.CHAMBERRUNMODE].InnerText.Trim())
                    {
                        case "MQC1":
                            return "1";
                        case "MQC2":
                            return "2";
                        case "LS":
                            return "3";
                        case "GE":
                            return "4";
                        case "SD":
                            return "5";
                        case "M3":
                            return "6";
                        case "BITO":
                            return "7";
                        case "TITO":
                            return "8";
                        default:
                            return "0";
                    }
                default:
                    {
                        /* 
                        if (ObjectManager.JobManager.M2P_GetJobType(productNode[keyHost.PRODUCTTYPE].InnerText) == eJobType.TFT)
                            return "0";
                        else
                            return "1";
                        */
                         
                        //modify 2016/03/17 cc.kuang
                        if (productNode[keyHost.PRODUCTTYPE].InnerText.Trim() == eMES_PRODUCT_TYPE.MQC_DUMMY)
                            return "1";
                        else
                            return "0";
                    }
            }
        }

        private eCVD_MIX_TYPE AnalysisCVD_PortMode(Port port)
        {
            IList<ePortMode> modes = ObjectManager.PortManager.GetPorts().Select(p => 
                p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().ToList<ePortMode>();
            if (modes.Count > 2) return eCVD_MIX_TYPE.UNKNOWN;

            switch (modes[0])
            {
                case ePortMode.HT:
                case ePortMode.LT: return eCVD_MIX_TYPE.TYPE1_LT_HT;
                case ePortMode.IGZO:
                    {
                        if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                        switch (modes[1])
                        {
                            case ePortMode.TFT: return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                            case ePortMode.MQC: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                            default: return eCVD_MIX_TYPE.UNKNOWN;   
                        }
                    }
                case ePortMode.MQC:
                    {
                        if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                        switch (modes[1])
                        {
                            case ePortMode.IGZO: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                            case ePortMode.TFT: return eCVD_MIX_TYPE.TYPE4_TFT_MQC;
                            default: return eCVD_MIX_TYPE.UNKNOWN;   
                        }
                    }
                case ePortMode.TFT:
                    {
                        if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                        switch (modes[1])
                        {
                            case ePortMode.IGZO: return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                            case ePortMode.MQC: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                            default: return eCVD_MIX_TYPE.UNKNOWN;
                        }
                    }
                default: return  eCVD_MIX_TYPE.UNKNOWN;
            }
        }

        private eDRY_MIX_TYPE AnalysisDRY_PortMode(Port port)
        {
            IList<ePortMode> modes = ObjectManager.PortManager.GetPorts().Select(p =>
                p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().ToList<ePortMode>();

            if (modes.Count > 2) return eDRY_MIX_TYPE.UNKNOWN;

            switch (modes[0])
            {
                case ePortMode.TFT:
                    {
                        if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                        switch (modes[1])
                        {
                            case ePortMode.ENG: return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                            case ePortMode.IGZO: return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                            default: return eDRY_MIX_TYPE.UNKNOWN;
                        }
                    }
                case ePortMode.ENG:
                    {
                        if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                        if (modes[1] == ePortMode.TFT)
                            return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                        else
                            return eDRY_MIX_TYPE.UNKNOWN;
                    }
                case ePortMode.IGZO:
                    {
                        if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                        switch (modes[1])
                        {
                            case ePortMode.TFT: return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                            case ePortMode.MQC: return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                            default: return eDRY_MIX_TYPE.UNKNOWN;
                        }
                    }
                case ePortMode.MQC:
                    {
                        if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                        if (modes[1] == ePortMode.IGZO)
                            return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                        else
                            return eDRY_MIX_TYPE.UNKNOWN;
                    }
                default: return eDRY_MIX_TYPE.UNKNOWN;
            }
        }

        //===================================================================================================================

        public void M2P_SpecialDataBy_CF(XmlNode body,XmlNode lotNode, XmlNode productNode, string mesOXR, Line line, ref Job job, Port port, Cassette cst, Equipment eqp)
        {
            try
            {
                // 取出 Abnormal Code List 內容
                XmlNodeList abnormalcodelist = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                string PDRReworkCount = string.Empty;  //Job Data
                foreach (XmlNode abnormalcode in abnormalcodelist)
                {
                    if (abnormalcode.InnerXml.Contains("ABNORMALSEQ"))
                    {
                        string abnormalSeq = abnormalcode[keyHost.ABNORMALSEQ].InnerText.Trim();
                        switch (abnormalSeq.ToUpper())
                        {
                            case "COA2MASKEQPID":
                                job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "COA2MASKNAME":
                                job.CfSpecial.AbnormalCode.COA2MASKNAME = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "TTPFLAG":
                                job.CfSpecial.AbnormalCode.TTPFLAG = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "ALNSIDE":
                                job.CfSpecial.AbnormalCode.ALNSIDE = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "OVENSIDE":
                                job.CfSpecial.AbnormalCode.OVENSIDE = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "VCDSIDE":
                                job.CfSpecial.AbnormalCode.VCDSIDE = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "PRLOT":
                                job.CfSpecial.AbnormalCode.PRLOT = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "CSPNUMBER":
                                job.CfSpecial.AbnormalCode.CSPNUMBER = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "HPCHAMBER":
                                job.CfSpecial.AbnormalCode.HPCHAMBER = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "CPCHAMBER":
                                job.CfSpecial.AbnormalCode.CPCHAMBER = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                            case "DISPENSESPEED":
                                job.CfSpecial.AbnormalCode.DISPENSESPEED = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                                break;
                        }
                    }
                    if (abnormalcode.InnerXml.Contains("ABNORMALVALUE"))
                    {
                        string abnormalVal = abnormalcode[keyHost.ABNORMALVALUE].InnerText.Trim();
                        switch(abnormalVal.ToUpper())//add by hujunpeng 20190117 for CF rework run cell rework PDR Current rework count add 1
                        {
                            case "PDRREWORKCOUNT": PDRReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                            default :
                            break;
                        }
                    }
                    
                }

                // 初始化參數內容
                job.EQPReservations = SpecialItemInitial("EQPReservations", 6);
                job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                job.InspJudgedData = SpecialItemInitial("Insp.JudgedData", 16);
                job.TrackingData = SpecialItemInitial("TrackingData", 16);
                job.CFSpecialReserved = SpecialItemInitial("CFSpecialReserved", 16);

                IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");

                // COAversion [CF All Line special]
                job.CfSpecial.PlannedSourcePart = body[keyHost.PLANNEDSOURCEPART].InnerText;
                job.CfSpecial.COAversion = productNode[keyHost.ARRAYPRODUCTSPECVER].InnerText;
                job.CfSpecial.ExposureDoperation = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;

 
                switch (line.Data.JOBDATALINETYPE)
                {
                    case eJobDataLineType.CF.PHOTO_BMPS:
                        // 20150812 Add by Frank
                        #region CStOprationMode = LTOL
                        if (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL)
                        {
                            int slotNo;
                            int.TryParse(productNode[keyHost.POSITION].InnerText, out slotNo);
                            job.CfSpecial.TargetCSTID = port.File.CassetteID;
                            job.CfSpecial.TargetSlotNo = slotNo.ToString();
                        }
                        #endregion

                        job.InspJudgedData2 = SpecialItemInitial("InspJudgedData2", 16);
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        //job.EQPFlag2 = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag2");
                        job.EQPFlag = SpecialItemInitial("EQPFlag", 16);
                        job.EQPFlag2 = SpecialItemInitial("EQPFlag2", 16);
                        job.CfSpecial.InlineReworkMaxCount = productNode[keyHost.MAXINLINERWCOUNT].InnerText;
                        // 20160311 Add by Frank 預防MES沒有給值
                        if(string.IsNullOrEmpty(productNode[keyHost.MACROFLAG].InnerText.Trim()))
                            job.CFMarcoReserveFlag = SpecialItemInitial("MarcoReserveFlag",16);
                        else
                            job.CFMarcoReserveFlag = productNode[keyHost.MACROFLAG].InnerText;
                        job.CfSpecial.ExposureName = productNode[keyHost.ALIGNERNAME].InnerText;
                        

                        job.CFProcessBackUp = CFPhotoProcessBackUpCheck(lotNode[keyHost.PROCESSOPERATIONNAME].InnerText);
                        break;

                    case eJobDataLineType.CF.PHOTO_GRB:
                        // 20150812 Add by Frank
                        #region CStOprationMode = LTOL
                        if (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL)
                        {
                            int slotNo;
                            int.TryParse(productNode[keyHost.POSITION].InnerText, out slotNo);
                            job.CfSpecial.TargetCSTID = port.File.CassetteID;
                            job.CfSpecial.TargetSlotNo = slotNo.ToString();
                        }
                        #endregion

                        job.InspJudgedData2 = SpecialItemInitial("InspJudgedData2", 16);
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        job.EQPFlag2 = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag2");
                        job.CfSpecial.InlineReworkMaxCount = productNode[keyHost.MAXINLINERWCOUNT].InnerText;
                        // 20160311 Add by Frank 預防MES沒有給值
                        if (string.IsNullOrEmpty(productNode[keyHost.MACROFLAG].InnerText.Trim()))
                            job.CFMarcoReserveFlag = SpecialItemInitial("MarcoReserveFlag", 16);
                        else
                            job.CFMarcoReserveFlag = productNode[keyHost.MACROFLAG].InnerText;
                        job.CfSpecial.ExposureName = productNode[keyHost.ALIGNERNAME].InnerText;

                        job.CFProcessBackUp = CFPhotoProcessBackUpCheck(lotNode[keyHost.PROCESSOPERATIONNAME].InnerText);
                        break;

                    case eJobDataLineType.CF.REWORK:
                        job.CfSpecial.ReworkMaxCount = lotNode[keyHost.CFREWORKCOUNT].InnerText;
                        if (string.IsNullOrEmpty(job.CfSpecial.ReworkMaxCount))
                        {
                            job.CfSpecial.ReworkMaxCount = "1";  //add by qiumin 20180710 for CF rework line
                        }
                           
                        job.CfSpecial.ReworkRealCount = "0";
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        //add by hujunpeng 20190117 for CF rework run cell rework PDR Current rework count add 1
                        job.CellSpecial.MaxRwkCount = productNode[keyHost.MAXRWCOUNT].InnerText== string.Empty ? "0" : productNode[keyHost.MAXRWCOUNT].InnerText;
                        job.CellSpecial.CurrentRwkCount = PDRReworkCount==string.Empty?"0":PDRReworkCount;
                        break;
                    case eJobDataLineType.CF.MASK:
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    case eJobDataLineType.CF.REPAIR:
                        job.CfSpecial.ITOSIDEFLAG = productNode[keyHost.ITOSIDEFLAG].InnerText;
                        switch (job.JobJudge)
                        {
                            case "6": //IR
                                if (sub.ContainsKey("InkRepairGlass"))
                                    sub["InkRepairGlass"] = "1";
                                break;
                            case "5": //RP
                                if (sub.ContainsKey("RepairGlass"))
                                    sub["RepairGlass"] = "1";
                                break;
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    case eJobDataLineType.CF.MQC_1:
                    case eJobDataLineType.CF.MQC_2: //add by qiumin 20180803 
                        job.CfSpecial.MaskID = productNode[keyHost.MASKNAME].InnerText;
                        job.CfSpecial.COAversion = productNode[keyHost.ARRAYPRODUCTSPECVER].InnerText;
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        #region Flow Priority Rule
                        string flowpriority = string.Empty;
                        string [] eqpList = productNode["MACHINEPROCESSSEQ"].InnerText.Split(';');
                        foreach (string _eqpid in eqpList)
                        {
                            Equipment priorityEQP = ObjectManager.EquipmentManager.GetEQPByID(_eqpid);
                            if (priorityEQP != null)
                            { flowpriority += priorityEQP.Data.NODENO.Substring(1).PadLeft(2, '0'); }
                            else
                            {
                                flowpriority = string.Empty;
                                continue;
                            }
                        }
                        job.CfSpecial.FlowPriorityInfo = FlowPriority(flowpriority.PadRight(6,'0'));
                        #endregion
                        break;
                    /* case eJobDataLineType.CF.MQC_2:
                         job.CfSpecial.COAversion = productNode[keyHost.ARRAYPRODUCTSPECVER].InnerText;
                         job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                         break;
                     case eJobDataLineType.CF.MQC_3:
                         string flowpriority = string.Empty;
                         string [] eqpList = productNode["MACHINEPROCESSSEQ"].InnerText.Split(';');
                         foreach (string _eqpid in eqpList)
                         {
                             Equipment priorityEQP = ObjectManager.EquipmentManager.GetEQPByID(_eqpid);
                             if (priorityEQP != null)
                             { flowpriority += priorityEQP.Data.NODENO.Substring(1).PadLeft(2, '0'); }
                             else
                             {
                                 flowpriority = string.Empty;
                                 continue;
                             }
                         }
                         job.CfSpecial.FlowPriorityInfo = FlowPriority(flowpriority.PadRight(6,'0'));
                         job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                         break;
                      */ 
                    case eJobDataLineType.CF.FCMAC:
                        if (productNode["CHAMBERRUNMODE"].InnerText.Trim() != "")
                        {
                            string changeRunMode = productNode["CHAMBERRUNMODE"].InnerText.Trim();
                            switch (changeRunMode)
                            {
                                case "MQC":
                                    if (sub.ContainsKey("MQCGlass"))
                                        sub["MQCGlass"] = "1";
                                    break;

                                case "BMACRO":
                                    if (sub.ContainsKey("BMacroGlass"))
                                        sub["BMacroGlass"] = "1";
                                    break;
                                
                                case "FIMACRO":
                                    if (sub.ContainsKey("FIPGlass"))
                                        sub["FIPGlass"] = "1";
                                    break;                                      
                            }
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    case eJobDataLineType.CF.FCSRT:
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CF.FCPSH:
                        job.CfSpecial.RecyclingFlag= productNode[keyHost.RECYCLINGFLAG].InnerText;
                    break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void M2P_SpecialDataBy_CELL(XmlNode body, XmlNode lotNode, XmlNode productNode, int i, string mesOXR, Line line, Port portObj, Dictionary<string, int> nodeStack, ref Job job)
        {
            try
            {
                //CELL 有跨超大線 PPID組成的問題, 當PRODUCTPROCESSTYPE為MMG時
                /* 4線跨5線時:
                        第一組PPID:4線BCS本身的PPID為 拆LOT PPID的前兩個EQ(LD,CUT) + ProcessLineLst裡 LineName為4線的PPID, 排除前兩個EQ的PPID
                        第二組PPID:Process Line List 的5線PPID + STB Product Spec List的PPID
                */

                // 初始化參數內容
                job.INSPReservations = new string('0', 6);
                job.EQPReservations = new string('0', 6);
                job.InspJudgedData = new string('0', 32);
                job.TrackingData = new string('0', 32);
                job.EQPFlag = new string('0', 32);
                //Watson Add 20141220 For MES Spec 3.89
                job.CellSpecial.RTPFlag = productNode[keyHost.RTPFLAG].InnerText;
                //shihyang Add 20151101 //20151102 MES閻波 確認為OXOO                
                //string[] blockOxList = new string[productNode[keyHost.BLOCKJUDGES].InnerText.Split(';').Length];
                //string blockList = string.Empty; int blockCount = 0;
                //foreach (string blockOx in blockOxList)
                //{
                //    blockList += productNode[keyHost.BLOCKJUDGES].InnerText.Split(';')[blockCount];
                //    blockCount++;
                //} 
                // 取出 Abnormal Code List 內容
                XmlNodeList abnormalcodelist = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                string abnormalULD = string.Empty;  //Job Data
                string beveledFlag = string.Empty;  //Job Data
                string bubbleSampleFlag = string.Empty;  //Job Data
                string chippigFlag = string.Empty;  //Job Data
                string cutGradeFlag = string.Empty;  //待確認 
                string cutSlimReworkFlag = string.Empty;  //Job Data
                string CFSideResidueFlag = string.Empty;  //Job Data
                string CellCutRejudgeCount = string.Empty;  //Job Data
                string dimpleFlag = string.Empty;  //Job Data
                string hvaChippigFlag = string.Empty;  //Job Data 
                string lineReworkCount = string.Empty;//Job Data
                string LOIFlag = string.Empty;//Job Data
                string mgvFlag = string.Empty;  //Job Data
                string pitype = string.Empty;  //Job Data
                string pointReworkCount = string.Empty;//Job Data
                string PDRReworkCount = string.Empty;  //Job Data
                string ribMarkFlag = string.Empty;  //Job Data
                string repairFlag = string.Empty;  //Job Data
                string shellFlag = string.Empty;  //Job Data
                string sealAbnormalFlag = string.Empty;  //Job Data
                string sortFlag = string.Empty;  //Job Data
                string ttpFlag = string.Empty;  //Job Data
                string turnAngle = string.Empty;  //Job Data


                string abnormalTFT = string.Empty;  //**File Service**
                //string abnormalCF = string.Empty;  //**File Service**
                string abnormalLCD = string.Empty;  //**File Service**
                string deCreateFlag = string.Empty;  //**File Service**
                string fGradeFlag = string.Empty;  //**File Service**

                foreach (XmlNode abnormalcode in abnormalcodelist)
                {
                    //20151022 cy: MES文件在2015/5/25時,將ABNORMALSEQ改為ABNORMALVALUE
                    string abnormalVal = abnormalcode[keyHost.ABNORMALVALUE].InnerText.Trim();

                    switch (abnormalVal)
                    {
                        //For Job Data Use
                        case "ABCODEULD": abnormalULD = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "BEVELEFLAG": beveledFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "BUBBLESAMPLEFLAG": bubbleSampleFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "CELLCUTREJUDGECOUNT": CellCutRejudgeCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "CHIPPINGFLAG": chippigFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "CFSIDERESIDUEFLAG": CFSideResidueFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "CUTSLIMREWORKFLAG": cutSlimReworkFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "CUTGradeFlag": cutGradeFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "DIMPLEFLAG": dimpleFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "GLASSCHANGEANGLE": turnAngle = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "HVACHIPPINGFLAG": hvaChippigFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "LINEREWORKCOUNT": lineReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "LOIFLAG": LOIFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "MGVFLAG": mgvFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "PITYPE": pitype = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "PDRREWORKCOUNT": PDRReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "POINTREWORKCOUNT": pointReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "RIBMARKFLAG": ribMarkFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "REPAIRRESULTFLAG": repairFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "SHEFLAG": shellFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "SEALABNORMALFLAG": sealAbnormalFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "SORTFLAG": sortFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        case "TTPFLAG": ttpFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                        //For File Service Use
                        case "ABNORMALTFT":
                            abnormalTFT = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                            job.CellSpecial.AbnormalTFT = abnormalTFT;
                            break;
                        //case "ABNORMALCF": 
                        //    abnormalCF = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                        //    job.CellSpecial.AbnormalCF = abnormalCF;
                        //    break;
                        case "ABNORMALLCD":
                            abnormalLCD = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                            job.CellSpecial.AbnormalLCD = abnormalLCD;
                            break;
                        case "DECREASEFLAG":
                            deCreateFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                            job.CellSpecial.DeCreateFlag = deCreateFlag;
                            break;
                        case "FGRADERISKFLAG":
                            fGradeFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                            job.CellSpecial.FGradeFlag = fGradeFlag;
                            break;
                    }
                }

                double lotPnlSize = 0;
                double subPnlSize = 0;
                string subProdLWH = string.Empty;
                if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText, out lotPnlSize))
                {
                    lotPnlSize = lotPnlSize * 100;
                }
                else//MES如果給LWH
                {
                    subProdLWH = lotNode[keyHost.PRODUCTSIZE].InnerText;
                    double l = 0; double w = 0; double h = 0;
                    if (lotNode[keyHost.PRODUCTSIZE].InnerText.Split('x').Length == 3)
                    {
                        if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText.Split('x')[0], out l)) l = l * 100;
                        if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText.Split('x')[1], out w)) w = w * 100;
                        if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText.Split('x')[2], out h)) h = h * 100;
                        lotPnlSize = l * w * h;
                    }
                }
                double lotThickness = 0;
                if (double.TryParse(lotNode[keyHost.PRODUCTTHICKNESS].InnerText, out lotThickness))
                    lotThickness = lotThickness * 1000;

                //20161228 modify by huangjiayin: G00开始往后的line，不用
                string net_nos = "123456789ABCDEF";
                int networkNo=1;

                if (net_nos.Contains(line.Data.LINEID.Substring(5, 1)))
                {
                    networkNo = Convert.ToInt32(line.Data.LINEID.Substring(5, 1), 16);  //Jun Add 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                }

                XmlNodeList processList = lotNode[keyHost.PROCESSLINELIST].ChildNodes;
                XmlNodeList stbList = lotNode[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                XmlNodeList reworkList = productNode[keyHost.REWORKLIST].ChildNodes;
                IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
                IDictionary<string, string> sub2 = ObjectManager.SubJobDataManager.GetSubItem("EQPReservations");//huangjiayin add 20170607 For PIL,ODF TTPFLAG
                
                #region Job Data Line Special
                job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                //將BLOCKJUDGES值存在job中，PI 上port不會有資料，需BC依layout產生，後續的LINE，MES 都會給值
                job.CellSpecial.BlockOXInformation = productNode[keyHost.BLOCKJUDGES] == null ? "" : (string.IsNullOrEmpty(productNode[keyHost.BLOCKJUDGES].InnerText.Trim()) ? "" : productNode[keyHost.BLOCKJUDGES].InnerText);
                job.CellSpecial.BlockCount = job.CellSpecial.BlockOXInformation.Length.ToString();
                //job.OXRInformation = CellBlockOXtoBin(job.OXRInformation);
                job.CellSpecial.ControlMode = line.File.HostMode;
                job.CellSpecial.HVAChippingFlagForJps = hvaChippigFlag == "Y" ? "Y" : "N";//huangjiayin add for Jps.dat 2017090515
                job.CellSpecial.TFTIdLastChar = string.IsNullOrEmpty(productNode[keyHost.ARRAYPRODUCTNAME].InnerText.Trim()) ? "B" : productNode[keyHost.ARRAYPRODUCTNAME].InnerText.Trim().Substring(productNode[keyHost.ARRAYPRODUCTNAME].InnerText.Trim().Length - 1, 1);//20171120 huangjiayin
                
                switch (line.Data.JOBDATALINETYPE)
                {
                    #region [CCPIL]
                    case eJobDataLineType.CELL.CCPIL:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        if (lotNode[keyHost.PRODUCTSPECLAYOUT] != null)//PRODUCTSPECLAYOUT (X,Y)
                        {//將BLOCKJUDGES值存在job中，PI 上port不會有資料，需BC依layout產生，後續的LINE，MES 都會給值
                            int Block = int.Parse(lotNode[keyHost.PRODUCTSPECLAYOUT].InnerText.Split(',')[0]) * int.Parse(lotNode[keyHost.PRODUCTSPECLAYOUT].InnerText.Split(',')[1]);
                            job.CellSpecial.BlockOXInformation = new string('O', Block);
                            job.CellSpecial.BlockCount = job.CellSpecial.BlockOXInformation.Length.ToString();
                        }
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        job.CellSpecial.PILiquidType = pitype == string.Empty ? "0" : pitype;
                        job.CellSpecial.CurrentRwkCount = PDRReworkCount == string.Empty ? "0" : PDRReworkCount;
                        #region[EQPFLAG]
                        if (sub == null) return;
                        if (sub.ContainsKey("BackCrackFlagY") || sub.ContainsKey("BackCrackFlagN"))//sy  modify MES 佳音&閰波 確認
                        {
                            if (shellFlag == "N")
                                sub["BackCrackFlagN"] = "1";
                            else if (shellFlag == "Y") 
                                sub["BackCrackFlagY"] = "1";
                        }
                        if (sub.ContainsKey("TAMFlag"))//IO 未定待確認
                        {
                            if (productNode[keyHost.TAMFLAG] != null)
                                sub["TAMFlag"] = productNode[keyHost.TAMFLAG].InnerText == "Y" ? "1" : "0";
                        }
                        if (sub.ContainsKey("PTHFlag"))//IO 未定待確認
                        {
                            if (productNode[keyHost.TAMFLAG] != null)
                                sub["PTHFlag"] = productNode[keyHost.PTHFLAG].InnerText == "Y" ? "1" : "0";
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        #endregion

                        //huangjiayin add 20170607 For PIL,ODF TTPFLAG
                        #region[EQReservations]
                        if (job.CellSpecial.ControlMode == eHostMode.REMOTE
                            && (lotNode[keyHost.PRODUCTOWNER].InnerText.Trim() == "OwnerP"|| job.CellSpecial.OwnerID=="RESD")
                            && sub2!=null
                            && sub2.ContainsKey("TTPedGlass")
                            && !string.IsNullOrEmpty(ttpFlag)
                            && ttpFlag=="Y"
                            )
                        {
                            sub2["TTPedGlass"] = "1";
                            job.EQPReservations = ObjectManager.SubJobDataManager.Encode(sub2, "EQPReservations");
 
                        }

                        #endregion

                        break;
                    #endregion
                    #region [CCODF]
                    case eJobDataLineType.CELL.CCODF:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        job.CellSpecial.UVMaskNames = lotNode[keyHost.UVMASKNAMES] == null ? "" : lotNode[keyHost.UVMASKNAMES].InnerText;
                        //job.CellSpecial.AssembleSeqNo =
                        //job.CellSpecial.UVMaskAlreadyUseCount = 

                        //huangjiayin add 20170607 For PIL,ODF TTPFLAG
                        #region[EQReservations]
                        if (job.CellSpecial.ControlMode == eHostMode.REMOTE
                            && (lotNode[keyHost.PRODUCTOWNER].InnerText.Trim() == "OwnerP" || job.CellSpecial.OwnerID == "RESD")
                            && sub2 != null
                            && sub2.ContainsKey("TTPedGlass")
                            && !string.IsNullOrEmpty(ttpFlag)
                            && ttpFlag == "Y"
                            )
                        {
                            sub2["TTPedGlass"] = "1";
                            job.EQPReservations = ObjectManager.SubJobDataManager.Encode(sub2, "EQPReservations");

                        }

                        #endregion

                        break;
                    #endregion
                    #region [CCPCS]
                    case eJobDataLineType.CELL.CCPCS:
                         Dictionary<string, int> PCSSubBlockPositions = new Dictionary<string, int>();
                            PCSSubBlockPositions.Add("A",0);
                            PCSSubBlockPositions.Add("B",1);
                            PCSSubBlockPositions.Add("C",2);
                            PCSSubBlockPositions.Add("D",3);
                            PCSSubBlockPositions.Add("E",4);
                            PCSSubBlockPositions.Add("F",5);
                            PCSSubBlockPositions.Add("G",6);
                            PCSSubBlockPositions.Add("H",7);
                        string[] subproductpositions = lotNode[keyHost.SUBPRODUCTPOSITIONS].InnerText.Split(';');
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.BlockLWH = lotNode[keyHost.SUBPRODUCTSIZES].InnerText;//21051105 MES閻波表示 T3 會給LxWxH 還未明確定義 
                        //job.CellSpecial.BlockSize//切割後再給值byBlock
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        //job.CellSpecial.PCSProductType //在外層給值
                        //job.CellSpecial.PCSProductType2 //在外層給值
                        //job.CellSpecial.PCSProductID //在外層給值
                        //job.CellSpecial.PCSProductID2 //在外層給值
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.GroupID = productNode[keyHost.GROUPID].InnerText;
                        //Add by hujunpeng 20190718 for PCS 新增CUTCASSETTESETTIONGCODE LIST
                        if (!(string.IsNullOrEmpty(lotNode[keyHost.SUBPRODUCTCARRIERSETCODES].InnerText.Trim()))&&!(string.IsNullOrEmpty(lotNode[keyHost.SUBPRODUCTPOSITIONS].InnerText.Trim())))
                        {
                            Dictionary<string, string> subproductpositionscstsetcodes = new Dictionary<string, string>();                                                     
                            string[] subcstsetcodes = lotNode[keyHost.SUBPRODUCTCARRIERSETCODES].InnerText.Split(';');
                            for (int k = 0; k < subproductpositions.Length; k++)
                            {
                                subproductpositionscstsetcodes.Add(subproductpositions[k],subcstsetcodes[k]);
                            }
                            job.CellSpecial.CUTCassetteSettingCode = lotNode[keyHost.SUBPRODUCTCARRIERSETCODES].InnerText.Split(';')[0];
                            string pcscstsetcodelist = "0000000000000000";
                            char[] cstsetcodelist = pcscstsetcodelist.ToCharArray();
                            StringBuilder setcodelist = new StringBuilder();
                            foreach (string key in subproductpositionscstsetcodes.Keys)
                            {
                                if (job.CellSpecial.CUTCassetteSettingCode != subproductpositionscstsetcodes[key])
                                {
                                    job.CellSpecial.CUTCassetteSettingCode2 = subproductpositionscstsetcodes[key];
                                    cstsetcodelist[PCSSubBlockPositions[key]] = '1';
                                }
                            }
                            for (int l = 0; l < cstsetcodelist.Length; l++)
                            {
                                setcodelist.Append(cstsetcodelist[l]);
                            }
                            job.CellSpecial.PCSCassetteSettingCodeList = setcodelist.ToString();
                            subproductpositionscstsetcodes.Clear();
                        }

                        //20170724 huangjiayin: BlockSize1&2
                        if (!(string.IsNullOrEmpty(lotNode[keyHost.SUBPRODUCTSIZES].InnerText.Trim())) && !(string.IsNullOrEmpty(lotNode[keyHost.SUBPRODUCTPOSITIONS].InnerText.Trim())))
                        {
                            Dictionary<string, string> subproductpositionsize = new Dictionary<string, string>();
                            string[] subproductszie = lotNode[keyHost.SUBPRODUCTSIZES].InnerText.Split(';');
                            for (int j = 0; j < subproductpositions.Length; j++)
                            {
                                subproductpositionsize.Add(subproductpositions[j],subproductszie[j]);
                            }
                            string blockSize1 = lotNode[keyHost.SUBPRODUCTSIZES].InnerText.Split(';')[0];
                            string blockSize2="0";
                            string pcsproductsizelist = "0000000000000000";
                            char[] productsizelist = pcsproductsizelist.ToCharArray();
                            StringBuilder sizelist = new StringBuilder();
                            foreach (string key in subproductpositionsize.Keys)
                            {
                                if (blockSize1 != subproductpositionsize[key])
                                {
                                    blockSize2 = subproductpositionsize[key];
                                    productsizelist[PCSSubBlockPositions[key]] = '1';
                                }
                            }
                            for (int m = 0; m < productsizelist.Length; m++)
                            {
                                sizelist.Append(productsizelist[m]);
                            }
                           
                            double db_blocksize1 = 0.0;
                            double db_blocksize2 = 0.0;

                            if (!double.TryParse(blockSize1, out db_blocksize1))
                            {
                                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",string.Format("blocksize1 ivalid value[{0}].",blockSize1));
                            }
                            if (!double.TryParse(blockSize2, out db_blocksize2))
                            {
                                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",string.Format("blocksize2 ivalid value[{0}].",blockSize2));
                            }

                            job.CellSpecial.BlockSize1 = (db_blocksize1 * 100).ToString();
                            job.CellSpecial.BlockSize2= (db_blocksize2 * 100).ToString();
                            job.CellSpecial.PCSBlockSizeList = sizelist.ToString();
                            subproductpositionsize.Clear();
                        }

                        PCSSubBlockPositions.Clear();
                        if (sub == null) return;
                        if (sub.ContainsKey("SealErrorFlag")) sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";//SealErrorFlag: From ODF to BlockCut
                        if (sub.ContainsKey("ChippigFlag")) sub["ChippigFlag"] = chippigFlag.Trim() == "" ? "0" : "1";//ChippingFlag: From ODF to BlockCut
                        if (sub.ContainsKey("BubbleSampleFlag")) sub["BubbleSampleFlag"] = bubbleSampleFlag.Trim() == "" ? "0" : "1";//BubbleSampleFlag: From ODF to BlockCut 
                        break;
                    #endregion
                    #region [CCCUT]
                    case eJobDataLineType.CELL.CCCUT:
                        job.CellSpecial.PanelLWH = lotNode[keyHost.SUBPRODUCTSIZES].InnerText;//21051105 MES閻波表示 T3 會給LxWxH 還未明確定義 
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //t3 jps for cut port
                        if (portObj.Data.NODENO == "L3")
                        {

                            string[] cutJPSCode = productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');
                            if (cutJPSCode.Length > 0 && !string.IsNullOrEmpty(cutJPSCode[0].ToString())) job.CellSpecial.AbnormalTFT = cutJPSCode[0].ToString();
                            if (cutJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = cutJPSCode[1].ToString();

                        }
                        if (turnAngle == "") //MES閻波說如果MES給的值是空的話turnAngle就固定給"2" by tom.su 20160613
                        {
                            job.CellSpecial.TurnAngle = "2";
                        }
                        else
                        {
                            job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        }
                        job.CellSpecial.CutLayout = lotNode[keyHost.SUBPRODUCTSPECLAYOUT].InnerText;
                        job.CellSpecial.CutPoint = lotNode[keyHost.SUBPRODUCTORIGINID].InnerText.Split(';')[i];
                        job.CellSpecial.CutSubProductSpecs = lotNode[keyHost.SUBPRODUCTSPECS].InnerText;
                        //job.CellSpecial.PanelOXInformation ="0" //預設值，切割後再給值byPanel
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.GroupID = productNode[keyHost.GROUPID].InnerText;
                        double lotPnlSize1 = 0;
                        if (double.TryParse(lotNode[keyHost.SUBPRODUCTSIZES].InnerText, out lotPnlSize1))
                        {
                            lotPnlSize1 = lotPnlSize1 * 100;
                        }
                        job.CellSpecial.PanelSize = lotPnlSize1.ToString();//一開始就會給
                        //job.CellSpecial.DefectCode = productNode[keyHost.SUBPRODUCTDEFECTCODE].InnerText.Split(',')[i];
                        //20171009 mark by huangjiayin: 有productlist多于subproduct的情况，会报错
                        job.CellSpecial.RejudgeCount = CellCutRejudgeCount == string.Empty ? "0" : CellCutRejudgeCount;
                        job.CellSpecial.VendorName = productNode[keyHost.VENDORNAME].InnerText;
                        job.CellSpecial.BURCheckCount = "0";//wait MES to do
                        job.CellSpecial.CUTCassetteSettingCode = lotNode[keyHost.SUBPRODUCTCARRIERSETCODES].InnerText;

                        if (lotNode[keyHost.DISCARDJUDGES] != null)
                        {
                            if (lotNode[keyHost.DISCARDJUDGES].InnerText != "X" & lotNode[keyHost.DISCARDJUDGES].InnerText != "")//預設同機台預設值 就不報
                            {
                                string disCardJudges = new string('0', 32);
                                //0:A 25:Z 23:X 不管有沒有給X 要下給機台都要有X
                                disCardJudges = disCardJudges.Substring(0, 23) + "1" + disCardJudges.Substring(23 + 1, disCardJudges.Length - 1 - 23);
                                foreach (string disCardJudge in lotNode[keyHost.DISCARDJUDGES].InnerText.Split(';'))
                                {
                                    disCardJudgesUpdate(ref disCardJudges, disCardJudge);//updata EX: 00000110001110011
                                }
                                job.CellSpecial.DisCardJudges = disCardJudges;
                                // ScrapRuleCommand(string eqpNo, eBitResult value, string trackKey, string CassetteSequenceNo, string disCardJudges)
                                // 只會對L3 下Command
                                //Invoke(eServiceName.CELLSpecialService, "ScrapRuleCommand", new object[] { "L3", eBitResult.ON, trxID, port.File.CassetteSequenceNo, disCardJudges });
                            }
                        }

                        if (sub == null) return;
                        if (sub.ContainsKey("SealErrorFlag")) sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";//SealErrorFlag: From BlockCut to CUT
                        if (sub.ContainsKey("ChippigFlag")) sub["ChippigFlag"] = chippigFlag.Trim() == "" ? "0" : "1";//ChippingFlag: From BlockCut to CUT
                        if (sub.ContainsKey("BeveledFlag")) sub["BeveledFlag"] = beveledFlag.Trim() == "" ? "0" : "1";//BeveledFlag: From CUT to CUT
                        if (sub.ContainsKey("CFSideResidueFlag")) sub["CFSideResidueFlag"] = CFSideResidueFlag.Trim() == "" ? "0" : "1";//CFSideResidueFlag: From CUT 
                        if (sub.ContainsKey("RibMarkFlag")) sub["RibMarkFlag"] = ribMarkFlag.Trim() == "" ? "0" : "1";//RibMarkFlag: From CUT
                        if (sub.ContainsKey("LOIFlag")) sub["LOIFlag"] = LOIFlag.Trim() == "" ? "0" : "1";//LOIFlag: From CUT to (CUT and RWT)
                        if (sub.ContainsKey("CutSlimReworkFlag")) sub["CutSlimReworkFlag"] = cutSlimReworkFlag.Trim() == "" ? "0" : "1";//CutSlimReworkFlag: From CUT to CUT
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCPOL]
                    case eJobDataLineType.CELL.CCPOL://shihyang add 20151029
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //job.CellSpecial.DotRepairCount = pointReworkCount==""?"0":pointReworkCount;
                        //job.CellSpecial.LineRepairCount = lineReworkCount==""?"0":lineReworkCount;
                        //job.CellSpecial.DefectCode = productNode[keyHost.SUBPRODUCTDEFECTCODE].InnerText;
                        //20171124 jps t3
                        string[] polJPSCode = productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');
                        if (polJPSCode.Length > 0 && !string.IsNullOrEmpty(polJPSCode[0].ToString())) job.CellSpecial.AbnormalTFT = polJPSCode[0].ToString();
                        if (polJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = polJPSCode[1].ToString();

                        if (sub == null) return;
                        if (sub.ContainsKey("DimpleFlag")) sub["DimpleFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCRWK]
                    case eJobDataLineType.CELL.CCRWK://shihyang add 20151029
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        //job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        //job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        //job.CellSpecial.DotRepairCount = pointReworkCount==""?"0":pointReworkCount;
                        //job.CellSpecial.LineRepairCount = lineReworkCount==""?"0":lineReworkCount;
                        //job.CellSpecial.DefectCode = productNode[keyHost.SUBPRODUCTDEFECTCODE].InnerText;//待確認
                        if (sub == null) return;
                        if (sub.ContainsKey("LOIFlag")) sub["LOIFlag"] = LOIFlag.Trim() == "" ? "0" : "1";//LOIFlag: From CUT to (CUT and RWT)
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCQUP]
                    case eJobDataLineType.CELL.CCQUP:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        break;
                    #endregion
                    #region [CCPCK]
                    case eJobDataLineType.CELL.CCPCK:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.OQCBank = productNode[keyHost.OQCBANK].InnerText;

                        //20180428 add PCKPICKFLAG by huangjiayin
                        #region[EQPFlag]
                        if (sub == null) return;
                        if (sub.ContainsKey("PCKPICKFLAG"))
                        {
                            if (productNode[keyHost.PCKPICKFLAG] != null)
                                sub["PCKPICKFLAG"] = productNode[keyHost.PCKPICKFLAG].InnerText == "Y" ? "1" : "0";
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        #endregion

                        break;
                    #endregion
                    #region [CCPDR]
                    case eJobDataLineType.CELL.CCPDR:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText); 
                        job.CellSpecial.MaxRwkCount = productNode[keyHost.MAXRWCOUNT].InnerText== string.Empty ? "0" : productNode[keyHost.MAXRWCOUNT].InnerText;;                       
                        job.CellSpecial.CurrentRwkCount = PDRReworkCount == string.Empty ? "0" : PDRReworkCount;                       
                        break;
                    #endregion
                    #region [CCPTH]
                    case eJobDataLineType.CELL.CCPTH:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        if (sub == null) return;
                        if (sub.ContainsKey("BackCrackFlagY") || sub.ContainsKey("BackCrackFlagN"))//sy  modify MES 佳音&閰波 確認
                        {
                            if (shellFlag == "N")
                                sub["BackCrackFlagN"] = "1";
                            else if (shellFlag == "Y")
                                sub["BackCrackFlagY"] = "1";
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCTAM]
                    case eJobDataLineType.CELL.CCTAM:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        break;
                    #endregion
                    #region [CCGAP]
                    case eJobDataLineType.CELL.CCGAP:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.GlassThickness = lotThickness.ToString();
                        job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        break;
                    #endregion
                    #region [CCRWT]
                    case eJobDataLineType.CELL.CCRWT://shihyang add 20151029
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //job.CellSpecial.OperationID = lotNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                        //job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        //job.CellSpecial.ProductOwner = CellProductOwner(lotNode[keyHost.PRODUCTOWNER].InnerText);
                        job.CellSpecial.DotRepairCount = pointReworkCount==""?"0":pointReworkCount;
                        job.CellSpecial.LineRepairCount = lineReworkCount==""?"0":lineReworkCount;
                        //job.CellSpecial.DefectCode = productNode[keyHost.SUBPRODUCTDEFECTCODE].InnerText;
                        //20171124 jps t3
                        string[] rwtJPSCode = productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');
                        if (rwtJPSCode.Length > 0 && !string.IsNullOrEmpty(rwtJPSCode[0].ToString())) job.CellSpecial.AbnormalTFT = rwtJPSCode[0].ToString();
                        if (rwtJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = rwtJPSCode[1].ToString();
                        break;
                    #endregion
                    #region [CCCRP]
                    case eJobDataLineType.CELL.CCCRP:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //job.CellSpecial.DefectCode = productNode[keyHost.SUBPRODUCTDEFECTCODE].InnerText;
                        job.CellSpecial.DotRepairCount = pointReworkCount==""?"0":pointReworkCount;
                        job.CellSpecial.LineRepairCount = lineReworkCount==""?"0":lineReworkCount;//LineReworkCount: From NRP to (NRP, POL and RWT)
                        //foreach (XmlNode defect in productNode[keyHost.DEFECTLIST])
                        //{
                        //    DefectDecode(job, defect);
                        //}
                        break;
                    #endregion
                        
                    #region [CCSOR,CCCHN]
                    case eJobDataLineType.CELL.CCSOR:
                    case eJobDataLineType.CELL.CCCHN:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        //job.CellSpecial.SortFlagNo = "00";
                        if (sub == null) return;
                        if (sub.ContainsKey("DCRandSorterFlag"))
                        {
                            switch (sortFlag)
                            {
                                #region [sortFlag]
                                case "1":
                                case "2":
                                case "3":
                                case "4":
                                case "5":
                                case "6":
                                case "7":
                                case "8":
                                case "9":
                                case "10":
                                case "11":
                                case "12":
                                case "13": 
                                case "14": 
                                case "15": 
                                case "16":
                                sub["DCRandSorterFlag"] = sortFlag;
                                    break;
                                #endregion
                                default:
                                    sub["DCRandSorterFlag"] = "00000";
                                    break;
                            }
                            //job.CellSpecial.SortFlagNo = Convert.ToInt32(sub["DCRandSorterFlag"], 2).ToString().PadLeft(2, '0');
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCQSR]
                    case eJobDataLineType.CELL.CCQSR:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        if (sub == null) return;
                        if (sub.ContainsKey("DCRandSorterFlag"))
                        {
                            switch (sortFlag)
                            {
                                case "1":
                                    sub["DCRandSorterFlag"] = "01";//DCR Flag Glass
                                    break;
                                case "2":
                                    sub["DCRandSorterFlag"] = "10";//Sorter Flag Glass
                                    break;
                                case "3":
                                    sub["DCRandSorterFlag"] = "11";//DCR and Sorter Flag Glass
                                    break;
                                default:
                                    sub["DCRandSorterFlag"] = "00";
                                    break;
                            }
                        }
                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion

                    #region[CCNLS]
                    case eLineType.CELL.CCNLS:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        double pnlSize_nls = 0;
                        if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText, out pnlSize_nls))
                        {
                            pnlSize_nls *= 100;
                        }
                        job.CellSpecial.PanelSize = pnlSize_nls.ToString();
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        break;
                    #endregion

                    #region[CCNRD]
                    case eLineType.CELL.CCNRD:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        double pnlSize_nrd = 0;
                        if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText, out pnlSize_nrd))
                        {
                            pnlSize_nrd *= 100;
                        }
                        job.CellSpecial.PanelSize = pnlSize_nrd.ToString();
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        job.CellSpecial.PanelOXInformation = mesOXR;
                        break;
                    #endregion

                    #region [T2]

                    case eJobDataLineType.CELL.CBPIL:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                        foreach (XmlNode rework in reworkList)
                        {
                            if (rework[keyHost.REWORKFLOWNAME].InnerText == "PIREWORK")
                                job.CellSpecial.ReworkCount = rework[keyHost.REWORKCOUNT].InnerText;
                        }

                        break;

                    //case eJobDataLineType.CELL.CBODF:
                    //    job.CellSpecial.ControlMode = line.File.HostMode;
                    //    job.CellSpecial.ArrayTTPEQVer = ConstantManager["CELL_ARRAYTTPEQVERCODE"][productNode[keyHost.ARRAYTTPEQVERCODE].InnerText.Trim()].Value;
                    //    job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                    //    job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                    //    job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                    //    job.CellSpecial.NetworkNo = networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                    //    job.CellSpecial.UVMaskAlreadyUseCount = "0";
                    //    job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);

                    //    if (sub == null) return;
                    //    //TTP Flag
                    //    //- 0：Not TTP Glass
                    //    //- 1：TTP Glass
                    //    if (sub.ContainsKey("TTPFlag"))
                    //        sub["TTPFlag"] = ttpFlag.Trim() == "" ? "0" : "1";

                    //    job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                    //    break;

                    case eJobDataLineType.CELL.CBHVA:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);

                        if (sub == null) return;
                        //Seal Error Flag
                        //- 0 : Not Seal Error Glass
                        //- 1 : Seal Error Glass
                        if (sub.ContainsKey("SealErrorFlag"))
                            sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBCUT:
                        int proType;
                        int.TryParse(lotNode[keyHost.BCPRODUCTTYPE].InnerText, out proType);
                        job.ProductType.Value = proType;
                        job.CellSpecial.ProductID = lotNode[keyHost.BCPRODUCTID].InnerText;
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);

                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.CELL.CBCUT_1:
                                if (processList.Count > 0)
                                {
                                    job.CellSpecial.CUTProductType = processList[0][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.CUTProductID = processList[0][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? processList[0][keyHost.CARRIERSETCODE].InnerText.Trim() : processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                if (processList.Count > 1)
                                {
                                    job.CellSpecial.CUTCrossProductType = processList[1][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.CUTCrossProductID = processList[1][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CrossLineCassetteSettingCode = string.IsNullOrEmpty(processList[1][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? processList[1][keyHost.CARRIERSETCODE].InnerText.Trim() : processList[1][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                break;

                            case eLineType.CELL.CBCUT_2:
                                if (processList.Count > 0)
                                {
                                    job.CellSpecial.CUTProductType = processList[0][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.CUTProductID = processList[0][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? processList[0][keyHost.CARRIERSETCODE].InnerText.Trim() : processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                if (processList.Count > 1)
                                {
                                    job.CellSpecial.CUTCrossProductType = processList[1][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.CUTCrossProductID = processList[1][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CrossLineCassetteSettingCode = string.IsNullOrEmpty(processList[1][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? processList[1][keyHost.CARRIERSETCODE].InnerText.Trim() : processList[1][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                if (stbList.Count > 0)
                                {
                                    job.CellSpecial.POLProductType = stbList[0][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.POLProductID = stbList[0][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CrossLineCassetteSettingCode = string.IsNullOrEmpty(stbList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim() : stbList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                break;

                            case eLineType.CELL.CBCUT_3:
                                if (processList.Count > 0)
                                {
                                    job.CellSpecial.CUTProductType = processList[0][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.CUTProductID = processList[0][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? processList[0][keyHost.CARRIERSETCODE].InnerText.Trim() : processList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                if (stbList.Count > 0)
                                {
                                    job.CellSpecial.POLProductType = stbList[0][keyHost.BCPRODUCTTYPE].InnerText;
                                    job.CellSpecial.POLProductID = stbList[0][keyHost.BCPRODUCTID].InnerText;
                                    job.CellSpecial.CrossLineCassetteSettingCode = string.IsNullOrEmpty(stbList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim()) ? stbList[0][keyHost.CARRIERSETCODE].InnerText.Trim() : stbList[0][keyHost.OPI_CARRIERSETCODE].InnerText.Trim();
                                }
                                break;
                        }
                        job.CellSpecial.NetworkNo = networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        job.CellSpecial.AbnormalCode = abnormalULD;

                        //判斷是否為Cross Line的Glass
                        string[] subProLine = lotNode[keyHost.SUBPRODUCTLINES].InnerText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        ArrayList result = new ArrayList();
                        foreach (string proLine in subProLine)
                        {
                            if (!result.Contains(proLine.ToString()) && proLine.Trim() != "0")
                                result.Add(proLine.ToString());
                        }
                        if (result.Count > 1)
                            job.CellSpecial.CrossLineFlag = "Y";

                        //Panel Size and Cross Panel Size
                        subProLine = lotNode[keyHost.SUBPRODUCTLINES].InnerText.Split(';');
                        string[] subProPnlSize = lotNode[keyHost.SUBPRODUCTSIZES].InnerText.Split(';');

                        if (subProLine.Length == subProPnlSize.Length)
                        {
                            for (int j = 0; j < subProLine.Length; j++)
                            {
                                if (subProLine[j].Trim() != "" && subProLine[j].Trim() != "0")
                                {
                                    if (double.TryParse(subProPnlSize[j], out subPnlSize))
                                        subPnlSize = subPnlSize * 100;

                                    if (subProLine[j] == line.Data.LINEID)
                                        job.CellSpecial.PanelSize = subPnlSize.ToString();
                                    else
                                        job.CellSpecial.CrossLinePanelSize = subPnlSize.ToString();
                                }
                            }
                        }

                        job.CellSpecial.FGradeFlag = fGradeFlag;
                        //job.CellSpecial.AbnormalTFT = abnormalTFT;
                        //job.CellSpecial.AbnormalCF = abnormalCF;
                        job.CellSpecial.AbnormalLCD = abnormalLCD;
                        //job.CellSpecial.GlassType = Owner Type(1)_Owner ID(10)_Product Type(1), Product Type是哪個東西

                        if (sub != null)
                        {
                            //Seal Erro Flag
                            //- 0 : Not Seal Error Glass
                            //- 1 : Seal Error Glass
                            if (sub.ContainsKey("SealErrorFlag"))
                                sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";

                            //Unfilled Corner Flag
                            //- 0：No Unfilled Corner
                            //- 1：Unfilled Corner Glass
                            if (sub.ContainsKey("UnfilledCornerFlag"))
                                sub["UnfilledCornerFlag"] = hvaChippigFlag.Trim() == "" ? "0" : "1";

                            //Panel Size Flag
                            //- 1：Big Size 
                            //- 2：Small Size 
                            //- 3：Normal Size 
                            if (sub.ContainsKey("PanelSizeFlag"))
                                sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                            job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        }


                        string[] subProdSizeType = lotNode[keyHost.SUBPRODUCTSIZETYPES].InnerText.Split(';');
                        IDictionary<string, string> cutSub = ObjectManager.SubJobDataManager.GetSubItem("PanelSizeFlag");
                        if (cutSub != null)
                        {
                            if (cutSub.ContainsKey("PanelSizeFlag") && cutSub.ContainsKey("CrossLinePanelSizeFlag"))
                            {
                                if (subProLine.Length == subProdSizeType.Length)
                                {
                                    for (int j = 0; j < subProLine.Length; j++)
                                    {
                                        if (subProLine[j].Trim() != "" && subProLine[j].Trim() != "0")
                                        {
                                            if (subProLine[j] == line.Data.LINEID)
                                                cutSub["PanelSizeFlag"] = CellPanelSizeType(subProdSizeType[j]);
                                            else
                                                cutSub["CrossLinePanelSizeFlag"] = CellPanelSizeType(subProdSizeType[j]);
                                        }
                                    }
                                }
                            }

                            job.CellSpecial.PanelSizeFlag = ObjectManager.SubJobDataManager.Encode(cutSub, "PanelSizeFlag");
                        }

                        job.MesProduct.CFSUBPRODUCTGRADE = job.OXRInformation;

                        if (portObj.Data.PORTID == "01" || portObj.Data.PORTID == "02" ||
                            portObj.Data.PORTID == "C01" || portObj.Data.PORTID == "C02")
                        {
                            //break;
                        }
                        else if (portObj.Data.PORTID == "07" || portObj.Data.PORTID == "08" ||
                                 portObj.Data.PORTID == "C07" || portObj.Data.PORTID == "C08" ||
                                 portObj.Data.PORTID == "P01" || portObj.Data.PORTID == "P02" ||
                                 portObj.Data.PORTID == "P03" || portObj.Data.PORTID == "P04" || portObj.Data.PORTID == "P06")
                        {
                            string[] cutJPSCode = productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');
                            if (cutJPSCode.Length > 0) job.CellSpecial.AbnormalTFT = cutJPSCode[0].ToString();
                            if (cutJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = cutJPSCode[1].ToString();
                        }

                        break;

                    case eJobDataLineType.CELL.CBPOL:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBDPK:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Cutting Flag
                        //- 0：Not Cutting    = NC    : Shorting Cut has not been processed yet.
                        //- 1：OLS Cutting OK = OLSOK : OLS EQ Shorting Cut has been processed and result is OK
                        //- 2：LSC Cutting OK = LSCOK : LSC EQ Shorting Cut has been processed and result is OK
                        //- 3：Cutting NG     = NG    : Shorting Cut has been processed and result is NG
                        if (sub.ContainsKey("CuttingFlag"))
                            sub["CuttingFlag"] = CellCuttingFlag(productNode[keyHost.SHORTCUTFLAG].InnerText.Trim());

                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBPMT:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        //Watson Modify For PMT by Port and Line Run Mode
                        Port port = ObjectManager.PortManager.GetPortByLineIDPortID(body[keyHost.LINENAME].InnerText.Trim(), body[keyHost.PORTNAME].InnerText.Trim());
                        if (port == null)
                            job.CellSpecial.RunMode = line.File.CellLineOperMode;
                        else
                        {
                            Line pmtline = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                            if (pmtline == null)
                                job.CellSpecial.RunMode = line.File.CellLineOperMode;
                            else
                                job.CellSpecial.RunMode = pmtline.File.CellLineOperMode;
                        }

                        job.CellSpecial.NodeStack = ObjectManager.JobManager.M2P_GetCellNodeStack(lotNode[keyHost.NODESTACK].InnerText, nodeStack);
                        job.CellSpecial.AbnormalCode = abnormalULD;

                        break;

                    case eJobDataLineType.CELL.CBGAP:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        job.CellSpecial.NodeStack = ObjectManager.JobManager.M2P_GetCellNodeStack(lotNode[keyHost.NODESTACK].InnerText, nodeStack);
                        job.CellSpecial.AbnormalCode = abnormalULD;

                        break;

                    case eJobDataLineType.CELL.CBPIS:
                        job.CellSpecial.NetworkNo = "1";//networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBPRM:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();
                        job.CellSpecial.RunMode = line.File.CellLineOperMode;

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBGMO:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBLOI:
                        //刘公俊龙要求LOI給Net Work NO=1 20150514 Tom
                        job.CellSpecial.NetworkNo = "1";//networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();
                        job.CellSpecial.RepairResult = repairFlag;
                        job.CellSpecial.RunMode = line.File.CellLineOperMode;

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        job.CellSpecial.FGradeFlag = fGradeFlag;
                        //job.CellSpecial.AbnormalCF = abnormalCF;
                        job.CellSpecial.AbnormalLCD = abnormalLCD;

                        //Jun Modify 20150518 For MES邏輯
                        //Jun Add 20150109 For File Service
                        string[] loiJPSCode = productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');
                        if (loiJPSCode.Length > 0) job.CellSpecial.AbnormalTFT = loiJPSCode[0].ToString();
                        if (loiJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = loiJPSCode[1].ToString();
                        //if (productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(',').Length == 2)
                        //{
                        //    job.CellSpecial.AbnormalTFT = (productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(','))[0].ToString();
                        //    job.CellSpecial.LcdQtapLotGroupID = (productNode[keyHost.SUBPRODUCTJPSCODE].InnerText.Split(','))[1].ToString();
                        //}

                        foreach (XmlNode defect in productNode[keyHost.DEFECTLIST])
                        {
                            DefectDecode(job, defect);
                        }

                        break;

                    case eJobDataLineType.CELL.CBNRP:
                        job.CellSpecial.NetworkNo = "1";//networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();
                        job.CellSpecial.RunMode = line.File.CellLineOperMode;

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBOLS:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Cutting Flag
                        //- 0：Not Cutting    = NC    : Shorting Cut has not been processed yet.
                        //- 1：OLS Cutting OK = OLSOK : OLS EQ Shorting Cut has been processed and result is OK
                        //- 2：LSC Cutting OK = LSCOK : LSC EQ Shorting Cut has been processed and result is OK
                        //- 3：Cutting NG     = NG    : Shorting Cut has been processed and result is NG
                        if (sub.ContainsKey("CuttingFlag"))
                            sub["CuttingFlag"] = CellCuttingFlag(productNode[keyHost.SHORTCUTFLAG].InnerText.Trim());
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBSOR:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();
                        job.CellSpecial.RunMode = line.File.CellLineOperMode;
                        job.CellSpecial.NodeStack = ObjectManager.JobManager.M2P_GetCellNodeStack(lotNode[keyHost.NODESTACK].InnerText, nodeStack);

                        if (sub == null) return;
                        //FMA Flag
                        //- 0：Off
                        //- 1：ON
                        if (sub.ContainsKey("MGVFlag"))
                            sub["MGVFlag"] = mgvFlag != "Y" ? "0" : "1";

                        //MHU Flag
                        //- 0：Off
                        //- 1：ON
                        if (sub.ContainsKey("MHUFlag"))
                            sub["MHUFlag"] = productNode[keyHost.MHUFLAG].InnerText.Trim() != "Y" ? "0" : "1";

                        //FMA Flag
                        //- 0：Off
                        //- 1：ON
                        if (sub.ContainsKey("FMAFlag"))
                            sub["FMAFlag"] = productNode[keyHost.FMAFLAG].InnerText.Trim() != "Y" ? "0" : "1";

                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBDPS:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBATS:
                        job.CellSpecial.NetworkNo = "1";//networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;
                        job.CellSpecial.PanelSize = lotPnlSize.ToString();
                        job.CellSpecial.NodeStack = ObjectManager.JobManager.M2P_GetCellNodeStack(lotNode[keyHost.NODESTACK].InnerText, nodeStack);

                        if (sub == null) return;
                        //Panel Size Flag
                        //- 1：Big Size 
                        //- 2：Small Size 
                        //- 3：Normal Size 
                        if (sub.ContainsKey("PanelSizeFlag"))
                            sub["PanelSizeFlag"] = CellPanelSizeType(lotNode[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                        job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBDPI:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;

                        break;

                    case eJobDataLineType.CELL.CBUVA:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.AbnormalCode = abnormalULD;

                        break;

                    #endregion
                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void M2P_SpecialDataBy_MODULE(XmlNode body, XmlNode lotNode, XmlNode productNode, int i, string mesOXR, Line line, Port portObj, Dictionary<string, int> nodeStack, ref Job job)
        {
            try
            {
                // 初始化參數內容
                job.INSPReservations = new string('0', 6);
                job.EQPReservations = new string('0', 6);
                job.InspJudgedData = new string('0', 32);
                job.TrackingData = new string('0', 32);
                job.EQPFlag = new string('0', 32);
                // 取出 Abnormal Code List 內容
                XmlNodeList abnormalcodelist = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                string turnAngle = string.Empty;  //Job Data

                foreach (XmlNode abnormalcode in abnormalcodelist)
                {
                    string abnormalVal = abnormalcode[keyHost.ABNORMALVALUE].InnerText.Trim();

                    switch (abnormalVal)
                    {
                        case "GLASSCHANGEANGLE": turnAngle = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;                       
                    }
                }

                double lotPnlSize = 0;
                string subProdLWH = string.Empty;
                if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText, out lotPnlSize))
                {
                    lotPnlSize = lotPnlSize * 100;
                }

                double lotThickness = 0;
                if (double.TryParse(lotNode[keyHost.PRODUCTTHICKNESS].InnerText, out lotThickness))
                    lotThickness = lotThickness * 1000;

                int networkNo = Convert.ToInt32(line.Data.LINEID.Substring(5, 1), 16);  //Jun Add 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                XmlNodeList processList = lotNode[keyHost.PROCESSLINELIST].ChildNodes;
                XmlNodeList stbList = lotNode[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                XmlNodeList reworkList = productNode[keyHost.REWORKLIST].ChildNodes;
                IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");

                #region Job Data Line Special
                job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? lotNode[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : lotNode[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                switch (line.Data.JOBDATALINETYPE)
                {
                    #region [MDABL]
                    case eJobDataLineType.MODULE.MDABL:
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        break;
                    #endregion
                    #region [MDOCR]
                    case eJobDataLineType.MODULE.MDOCR:
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        break;
                    #endregion
                    #region [MDBLL]
                    case eJobDataLineType.MODULE.MDBLL:
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        break;
                    #endregion
                    #region [MDRWR]
                    case eJobDataLineType.MODULE.MDRWR:
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        break;
                    #endregion
                    #region [MDPAK]
                    case eJobDataLineType.MODULE.MDPAK:
                        job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                        break;
                    #endregion
                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        #endregion

        /// <summary>
        /// 將MES Data塞入Cassette Data裡
        /// </summary>
        private void MESDataIntoCstObject(XmlNode bodyNode, ref Cassette cst)
        {
            try
            {
                ObjectManager.JobManager.ConvertXMLToObject(cst.MES_CstData, bodyNode);

                if (string.IsNullOrEmpty(bodyNode[keyHost.OPI_LINERECIPENAME].InnerText.Trim()))
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;
                else
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.OPI_LINERECIPENAME].InnerText;

                XmlNodeList recipeNoCheckList = bodyNode[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                for (int i = 0; i < recipeNoCheckList.Count; i++)
                {
                    cst.MES_CstData.RECIPEPARANOCHECKLIST.Add(recipeNoCheckList[i].InnerText.Trim());
                }
                XmlNodeList lotList = bodyNode[keyHost.LOTLIST].ChildNodes;
                foreach (XmlNode lotNode in lotList)
                {
                    LOTc lot = new LOTc();
                    ObjectManager.JobManager.ConvertXMLToObject(lot, lotNode);

                    XmlNodeList lineQTimeList = lotNode[keyHost.LINEQTIMELIST].ChildNodes;
                    foreach (XmlNode n in lineQTimeList)
                    {
                        LINEQTIMEc obj = new LINEQTIMEc();
                        ObjectManager.JobManager.ConvertXMLToObject(obj, n);

                        if (n[keyHost.MACHINEQTIMELIST] != null)
                        {
                            XmlNodeList qTimeList = n[keyHost.MACHINEQTIMELIST].ChildNodes;
                            foreach (XmlNode n2 in qTimeList)
                            {
                                MACHINEQTIMEc obj2 = new MACHINEQTIMEc();
                                ObjectManager.JobManager.ConvertXMLToObject(obj2, n2);
                                obj.MACHINEQTIMELIST.Add(obj2);
                            }
                            lot.LINEQTIMELIST.Add(obj);
                        }
                    }

                    #region 在Lot Data 加入 ProcessLineList
                    XmlNodeList ProcessLineList = lotNode[keyHost.PROCESSLINELIST].ChildNodes;
                    foreach (XmlNode n in ProcessLineList)
                    {
                        PROCESSLINEc obj = new PROCESSLINEc();
                        ObjectManager.JobManager.ConvertXMLToObject(obj, n);
                        lot.PROCESSLINELIST.Add(obj);
                    }
                    #endregion

                    #region 在Lot Data 加入 Stb Product List (CELL 超大線, 偏貼段)
                    XmlNodeList StbProductList = lotNode[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                    foreach (XmlNode n in StbProductList)
                    {
                        STBPRODUCTSPECc obj = new STBPRODUCTSPECc();
                        ObjectManager.JobManager.ConvertXMLToObject(obj, n);
                        lot.STBPRODUCTSPECLIST.Add(obj);
                    }
                    #endregion

                    #region 在Product Data 加入 Product List
                    XmlNodeList productList = lotNode[keyHost.PRODUCTLIST].ChildNodes;
                    foreach (XmlNode n in productList)
                    {
                        PRODUCTc product = new PRODUCTc();
                        ObjectManager.JobManager.ConvertXMLToObject(product, n);

                        XmlNodeList abnormal = n[keyHost.ABNORMALCODELIST].ChildNodes;
                        foreach (XmlNode a in abnormal)
                        {
                            CODEc code = new CODEc();
                            ObjectManager.JobManager.ConvertXMLToObject(code, a);
                            product.ABNORMALCODELIST.Add(code);
                        }

                        XmlNodeList lcdportList = n[keyHost.LCDROPLIST].ChildNodes;
                        foreach (XmlNode lcd in lcdportList)
                        {
                            product.LCDROPLIST.Add(lcd.InnerText);
                        }

                        XmlNodeList reworkList = n[keyHost.REWORKLIST].ChildNodes;
                        foreach (XmlNode r in reworkList)
                        {
                            REWORKc rework = new REWORKc();
                            ObjectManager.JobManager.ConvertXMLToObject(rework, r);
                            product.REWORKLIST.Add(rework);
                        }

                        XmlNodeList defectList = n[keyHost.DEFECTLIST].ChildNodes;
                        foreach (XmlNode d in defectList)
                        {
                            DEFECTc defect = new DEFECTc();
                            ObjectManager.JobManager.ConvertXMLToObject(defect, d);
                            product.DEFECTLIST.Add(defect);
                        }

                        lot.PRODUCTLIST.Add(product);
                    }
                    #endregion

                    cst.MES_CstData.LOTLIST.Add(lot);
                }
                ObjectManager.CassetteManager.EnqueueSave(cst);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 將MES Cst Data塞入Job Data裡
        /// </summary>
        private void MESCstDataIntoJobObject(XmlNode bodyNode, XmlNode lotNode, ref Job job, ref Cassette cst)
        {
            try
            {               
                if (string.IsNullOrEmpty(bodyNode[keyHost.OPI_LINERECIPENAME].InnerText.Trim()))
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;
                else
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.OPI_LINERECIPENAME].InnerText;

                ObjectManager.JobManager.ConvertXMLToObject(job.MesCstBody, bodyNode);
                LOTc lot = new LOTc();
                ObjectManager.JobManager.ConvertXMLToObject(lot, lotNode);
                job.MesCstBody.LOTLIST.Add(lot);

                #region 在Lot Data 加入 QT Time
                XmlNodeList lineQTimeList = lotNode[keyHost.LINEQTIMELIST].ChildNodes;
                foreach (XmlNode n in lineQTimeList)
                {
                    LINEQTIMEc obj = new LINEQTIMEc();
                    ObjectManager.JobManager.ConvertXMLToObject(obj, n);

                    if (n[keyHost.MACHINEQTIMELIST] != null)
                    {
                        XmlNodeList qTimeList = n[keyHost.MACHINEQTIMELIST].ChildNodes;
                        foreach (XmlNode n2 in qTimeList)
                        {
                            MACHINEQTIMEc obj2 = new MACHINEQTIMEc();
                            ObjectManager.JobManager.ConvertXMLToObject(obj2, n2);
                            obj.MACHINEQTIMELIST.Add(obj2);
                        }
                        lot.LINEQTIMELIST.Add(obj);
                    }
                }
                #endregion

                #region 在Lot Data 加入 ProcessLineList
                XmlNodeList ProcessLineList = lotNode[keyHost.PROCESSLINELIST].ChildNodes;
                foreach (XmlNode n in ProcessLineList)
                {
                    PROCESSLINEc obj = new PROCESSLINEc();
                    ObjectManager.JobManager.ConvertXMLToObject(obj, n);
                    lot.PROCESSLINELIST.Add(obj);
                }
                #endregion

                #region 在Lot Data 加入 Stb Product List (CELL 超大線, 偏貼段)
                XmlNodeList StbProductList = lotNode[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                foreach (XmlNode n in StbProductList)
                {
                    STBPRODUCTSPECc obj = new STBPRODUCTSPECc();
                    ObjectManager.JobManager.ConvertXMLToObject(obj, n);
                    lot.STBPRODUCTSPECLIST.Add(obj);
                }
                #endregion

                #region 在Lot Data 加入 Sub Product List (CELL) //Add By Yangzhenteng20190316 For CUT6#/7#
                XmlNodeList Subproductspeclist = lotNode["SUBPRODUCTSPECLIST"].ChildNodes;
                foreach (XmlNode n in Subproductspeclist)
                {
                    SUBPRODUCTSPECc obj = new SUBPRODUCTSPECc();
                    ObjectManager.JobManager.ConvertXMLToObject(obj, n);
                     // lot.SUBPRODUCTSPECLIST.Add(obj);
                    lot.SUBPRODUCTSPECLIST.Add(obj);
                }
                #endregion   

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 將MES Product Data塞入Job Data裡
        /// </summary>
        private void MESProductDataIntoJobObject(XmlNode productNode, ref Job job)
        {
            ObjectManager.JobManager.ConvertXMLToObject(job.MesProduct, productNode);

            XmlNodeList abnormal = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
            foreach (XmlNode a in abnormal)
            {
                CODEc code = new CODEc();
                ObjectManager.JobManager.ConvertXMLToObject(code, a);
                job.MesProduct.ABNORMALCODELIST.Add(code);
            }

            XmlNodeList lcdportList = productNode[keyHost.LCDROPLIST].ChildNodes;
            foreach (XmlNode lcd in lcdportList)
            {
                job.MesProduct.LCDROPLIST.Add(lcd.InnerText);
            }

            XmlNodeList reworkList = productNode[keyHost.REWORKLIST].ChildNodes;
            foreach (XmlNode r in reworkList)
            {
                REWORKc rework = new REWORKc();
                ObjectManager.JobManager.ConvertXMLToObject(rework, r);
                job.MesProduct.REWORKLIST.Add(rework);
            }

            XmlNodeList defectList = productNode[keyHost.DEFECTLIST].ChildNodes;
            foreach (XmlNode d in defectList)
            {
                DEFECTc defect = new DEFECTc();
                ObjectManager.JobManager.ConvertXMLToObject(defect, d);
                job.MesProduct.DEFECTLIST.Add(defect);
            }
        }

        /// <summary>
        /// 將MES Data裡Product中的ChamberRunMode轉成JobData的ProcessType
        /// </summary>
        private string Transfer_CVD_ProcessType(string chamberRunMode)
        {
            switch (chamberRunMode)
            {
                case eCVD_RUN_MODE.HT_2200_SINGLE:
                case eCVD_RUN_MODE.HT_2201_DOUBLE: return "2";
                case eCVD_RUN_MODE.LT_4200_PV1:
                case eCVD_RUN_MODE.LT_4201_PV2: return "3";
                case eCVD_RUN_MODE.IGZO_SiOX: return "4";
                case eCVD_RUN_MODE.MQC_2200_MQC:
                case eCVD_RUN_MODE.MQC_4200_MQC: return "5";
                default: return "";
            }
        }

        private string Transfer_DefectCodeToFileFormat(string hostDefectData)
        {
            string[] defect = hostDefectData.Split(',');
            string newData = string.Empty;

            for (int i = 0; i < defect.Count(); i++)
            {
                if (!string.IsNullOrEmpty(newData)) newData += ",";
                newData += defect[i].PadRight(5);
            }
            return newData;
        }

        public string CellPanelSizeType(string sizeType)
        {
            try
            {
                switch (sizeType)
                {
                    case "B1": return "1";
                    case "B2": return "2";
                    case "S1": return "3";
                    case "S2": return "4";
                    default: return "0";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }

        private string CellCuttingFlag(string cuttingFlag)
        {
            try
            {
                switch (cuttingFlag)
                {
                    case "NC": return "0";
                    case "OLSOK": return "1";
                    case "LSCOK": return "2";
                    case "NG": return "3";
                    default: return "0";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }

        private string CellTurnAngle(string turnAngleFlag)
        {
            try
            {
                //"1 : 0°, 2 : 90°, 3 : -90°, 4 : 180°"
                //Watson Modify 20150312 For MES Spec
                switch (turnAngleFlag.Trim())
                {
                    case "0": return "1";
                    case "90": return "2";
                    case "-90": return "3";
                    case "180": return "4";
                    //Watson Add 20150411 For HVA上報應是1,2,3,4，但MES是下載BC在前段上報的值，
                    //為避免mes可能下載角度, 也可能是1,2,3,4
                    case "1": return "1";   
                    case "2": return "2";
                    case "3": return "3";
                    case "4": return "4";
                    default: return "1";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }
          //20151020 cy:改為public,可供Offline Mode Cst叫用
        public string CellProductOwner(string ProductOwner)
        {
            try
            {
                //shihyang add 20150829 
                switch (ProductOwner.Trim())
                {
                    case "P": return "1";
                    case "E": return "2";
                    case "D": return "3"; //
                    case "M": return "3";//MES 會給M 
                    //為避免mes可能是1,2,3,4
                    case "1": return "1";
                    case "2": return "2";
                    case "3": return "3";
                    default: return "1";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }
        private void DefectDecode(Job job, XmlNode defect)
        {
            try
            {
                // .ARRAYDEFECTCODES =>TFTDEFECTCODES By T3 MES SPEC 1.21
                if (!string.IsNullOrEmpty(defect[keyHost.TFTDEFECTCODES].InnerText))
                {
                    string[] codeList = defect[keyHost.TFTDEFECTCODES].InnerText.Split(';');
                    string[] addressList = defect[keyHost.TFTDEFECTADDRESS].InnerText.Split(';');

                    if (codeList.Length == addressList.Length)
                    {
                        for (int k = 0; k < codeList.Length; k++)
                        {
                            string[] address = addressList[k].Split(',');
                            if (address.Length == 2)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("{0}{1}{2}",
                                    codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                job.CellSpecial.DefectList[k] = sb.ToString();
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(defect[keyHost.CFDEFECTCODES].InnerText))
                {
                    string[] codeList = defect[keyHost.CFDEFECTCODES].InnerText.Split(';');
                    string[] addressList = defect[keyHost.CFDEFECTADDRESS].InnerText.Split(';');

                    if (codeList.Length == addressList.Length)
                    {
                        for (int k = 0; k < codeList.Length; k++)
                        {
                            string[] address = addressList[k].Split(',');
                            if (address.Length == 2)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("{0}{1}{2}",
                                    codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                job.CellSpecial.DefectList[k + 8] = sb.ToString();
                            }
                        }
                    }
                }
                //shihyang T3 NO USE  Mark at 20151104
                //if (!string.IsNullOrEmpty(defect[keyHost.PIDEFECTCODES].InnerText))
                //{
                //    string[] codeList = defect[keyHost.PIDEFECTCODES].InnerText.Split(';');
                //    string[] addressList = defect[keyHost.PIDEFECTADDRESS].InnerText.Split(';');

                //    if (codeList.Length == addressList.Length)
                //    {
                //        for (int k = 0; k < codeList.Length; k++)
                //        {
                //            string[] address = addressList[k].Split(',');
                //            if (address.Length == 2)
                //            {
                //                StringBuilder sb = new StringBuilder();
                //                sb.AppendFormat("{0}{1}{2}",
                //                    codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                //                job.CellSpecial.DefectList[k + 18] = sb.ToString();
                //            }
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(defect[keyHost.ODFDEFECTCODES].InnerText))
                //{
                //    string[] codeList = defect[keyHost.ODFDEFECTCODES].InnerText.Split(';');
                //    string[] addressList = defect[keyHost.ODFDEFECTADDRESS].InnerText.Split(';');

                //    if (codeList.Length == addressList.Length)
                //    {
                //        for (int k = 0; k < codeList.Length; k++)
                //        {
                //            string[] address = addressList[k].Split(',');
                //            if (address.Length == 2)
                //            {
                //                StringBuilder sb = new StringBuilder();
                //                sb.AppendFormat("{0}{1}{2}",
                //                    codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                //                job.CellSpecial.DefectList[k + 24] = sb.ToString();
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private string SpecialItemInitial(string itemName, int defaultLen)
        {
            try
            {
                int len = ObjectManager.SubJobDataManager.GetItemLenth(itemName);
                if (len != 0)
                    return new string('0', len);
                else
                    return new string('0', defaultLen);
            }
            catch
            {
                return new string('0', defaultLen);
            }
        }

        private string FlowPriority(string _flowpriority)
        {
            try
            {
                int a1 = int.Parse(_flowpriority.Substring(0, 2));
                int a2 = int.Parse(_flowpriority.Substring(2, 2));
                int a3 = int.Parse(_flowpriority.Substring(4, 2));
                int Sum1 = 0;
                int Sum2 = 0;
                int Sum3 = 0;
                long Sum4 = 0;

                Sum1 = a1;
                Sum2 = a2 << 4; //往高位元Shift 4 bits
                Sum3 = a3 << 8; //往高位元Shift 8 bits
                Sum4 = Sum1 + Sum2 + Sum3 + Sum4;

                return Sum4.ToString();
            }
            catch
            {
                return "0";
            }
        }

        private string CFPhotoProcessBackUpCheck(string lineRecipeName)
        {
            try
            {
                return ConstantManager["CF_PHOTOBACKUP"][lineRecipeName].Value;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "00000000";
            }
 
        }

        private void disCardJudgesUpdate(ref string disCardJudges , string disCardJudge)
        {
            int n = Convert.ToInt32(Convert.ToChar(disCardJudge))-65;
            if (0 <=n & n<= 25)//A~Z
            {
                if (n == 0)
                    disCardJudges = "1" + disCardJudges.Substring(n + 1, disCardJudges.Length -1-n);
                else
                    disCardJudges = disCardJudges.Substring(0, n) + "1" + disCardJudges.Substring(n + 1, disCardJudges.Length - 1 - n);
            }

        }
        //add by qiumin 20171017 ELA one by one run
        private string GetNodeRecipe1(string PPID, string NodeNo)
        {
            int node = int.Parse(NodeNo.Substring(1));
            int start_index = 0;
            for (int i = 2; i < node; i++)
            {
                start_index += ObjectManager.EquipmentManager.GetEQP(string.Format("L{0}", i)).Data.RECIPELEN;
            }
            int len = ObjectManager.EquipmentManager.GetEQP(string.Format("L{0}", node)).Data.RECIPELEN;
            string node_pp = PPID.Substring(start_index, len);
            if (node_pp == string.Empty.PadLeft(len, '0'))
                return null;
            return node_pp;
        }
    }
}