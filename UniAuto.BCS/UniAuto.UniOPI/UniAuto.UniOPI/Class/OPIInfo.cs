using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class OPIInfo
    {
        public bool IsRunVshost = false;
               
        public string APName { get; set; }
        public string Version { get; set; }
        public string ErrMessage { get; set; }

        public string LoginUserID { get; set; }
        public string LoginPassword { get; set; }
        public string LoginUserName { get; set; }
        public string LoginGroupID { get; set; }
        public string LoginGroupName { get; set; }
       
        public string LocalIPAddress { get; set; }
        public string LocalHostName { get; set; }

        public bool RunModeHaveChange { get; set; }  //for CF PPK Line，用來判斷 L5 的run mode是否有變更
        public bool IndexerModeHaveChange { get; set; } //判斷indexer mode是否有變更

        public string SocketIp { get; set; }
        public string SocketPort { get; set; }
        public string SocketStepID { get; set; }
        public string SessionID { get; set; }
        public string SocketMode { get; set; }
        public bool SocketReplyFlag { get; set; }               //Socket 收到回覆訊息的flag -- True:已送出訊息尚未收到回覆 false:無等待需回覆訊息
        public int SocketWaitTime { get; set; }                 //Socket 等待最大時間，超過設定時間則允許使用者再次下commmand 
        public int SocketResponseTime { get; set; }             //Socket 等待最大時間，超過設定時間則允許使用者再次下commmand -- 毫秒
        public int SocketResponseTime_MES { get; set; }         //MES Command Socket 等待最大時間，超過設定時間則允許使用者再次下commmand -- 毫秒
        public int SocketResponseTime_MapDownload { get; set; } //Map Download Socket 等待最大時間，超過設定時間則允許使用者再次下commmand -- 毫秒
        public int SocketResponseTime_Query { get; set; }       //Equipment Query Socket 等待最大時間，超過設定時間則允許使用者再次下commmand -- 毫秒

        public int SocketRetryCount { get; set; }               //連不上Socket，重新連幾次後自動登出
        public int SocketMonitorWaitTime { get; set; }          //定義monitor request送出間格時間--秒

        public string ReturnCodeSuccess { get; set; }
        public List<string> KillUserAuthority { get; set; }
       
        public List<SBCS_OPIHISTORY_TRX> BCOpiTrx { get; set; } //記錄目前send/receive的資訊, 當收到時會記到db並將資訊從db刪除

        //public List<string> Lst_BCSParameterItem { get; set; } //記錄有提供修改的BCS Parameter Item Name 

        public bool RobotMixRunFlag { get; set; }  //設定是否提供Robot Mix Run Flag設定功能

        //History 分頁用
        public int QueryMaxCount { get; set; }
        public int QueryPageSixeCount { get; set; }

        public int RBCmdDisplayCnt { get; set; }  //Robot Command 顯示最大筆數
        public int BCSMessageDisplayTime { get; set; }  //BCS Message 顯示後自動關閉倒數時間，若設0表示不自動關閉

        public int GlassIDMaxLength { get; set; }   //glass id最大長度--for offline下貨使用

        public string DBConnStr { get; set; }  //DB連線字串
        public csDBConfigXML DBConfigXml { get; set; }

        public UniBCSDataContext DBCtx;

        private UniBCSDataContext _DBBRMCtx;

        public UniBCSDataContext DBBRMCtx
        {
            get
            {
                if (_DBBRMCtx == null) _DBBRMCtx = new UniBCSDataContext(DBConnStr);
                return _DBBRMCtx;
            }
            set
            {
                _DBBRMCtx = value;
            }
        }

        public void RefreshDBBRMCtx()
        {
            DBBRMCtx = new UniBCSDataContext(DBConnStr);
        }

        public Line CurLine = new Line();
        public MaterialRealWeight CurMaterialRealWeight = new MaterialRealWeight(); //Abb By Yangzhenteng For PI OPI Display 20180905
        public PIJobCount PIJobCount = new PIJobCount();//add by hujunpeng 20190723
        public Dictionary<string, Node> Dic_Node { get; set; } // Key: NODENO
        
        public Dictionary<string, Port> Dic_Port { get; set; } // Key: NODENO(3) + PORTNO(2) --port no by node
        public Dictionary<string, Unit> Dic_Unit { get; set; } // Key: NODENO(3) + UNITNO(2)
        public Dictionary<string, Line> Dic_Line { get; set; } // Key: LineID
        public Dictionary<string, Robot> Dic_Robot { get; set; } // Key: ROBOTNAME
        public Dictionary<string, Dense> Dic_Dense { get; set; } // Key: NODENO(3) + PORTNO(2) --port no by node --for Linetype="PPK" 
        public Dictionary<string, Pallet> Dic_Pallet { get; set; } // Key: PalletNo(2)  --for Linetype="PPK" 
        public Dictionary<string, Interface> Dic_Pipe { get; set; } // Key:pipe key 
        public List<RobotStage> Lst_RobotStage { get; set; }
        public SortedDictionary<int, string> Dic_RecipSeq { get; set; } //Key: SBRM_NODE -> RecipeSeq
        
        public List<LinkSignalType> Lst_LinkSignal_Type { get; set; }
        public Dictionary<string, LinkSignalBitDesc> Dic_LinkSignal_Desc { get; set; }  //key:LinkType
        public Dictionary<string, string> Dic_LinkSignal_JobDataPathNoU { get; set; } //key: Layout上interface id -- for link signal多組word顯示用 job data updateStream 
        public Dictionary<string, string> Dic_LinkSignal_JobDataPathNoD { get; set; } //key: Layout上interface id -- for link signal多組word顯示用 job data downStream 

        public List<CustButton> Lst_Button { get; set; }
        public static Queue<string> Q_BCMessage { get; set; } //BC要OPI Pop的訊息
        public static Queue<OPIMessage> Q_OPIMessage { get; set; } //OPI要pop的訊息
        public static Queue<string> Q_RobotMessage { get; set; } //Robot Command
        public static List<RobotCommandReport.COMMANDc> Lst_RobotCommand { get; set; } //Robot Command

        public BCS_EquipmentDataLinkStatusReply BC_EquipmentDataLinkStatusReply;

        private int _autoLogoutTime = 20;//OPI 自动登出等待时间 分钟为单位

        public int AutoLogoutTime {
            get { return _autoLogoutTime; }
            set { _autoLogoutTime = value; }
        }

        //20171123 by huangjiayin: 用户需求几个账号不做自动登出，for大屏幕展示
        public List<string> SuperUserList { get; set; }

        public OPIInfo(string strFabType,string strLineType,string strServerName,string userID,string userPassword)
        {
            try
            {
                //int _num = 0;
                string _err = string.Empty;
                
                ErrMessage = string.Empty;
                if (Process.GetCurrentProcess().ProcessName.ToUpper().Contains("VSHOST"))
                    IsRunVshost = true;
                else
                    IsRunVshost = false;
                SocketMode = "CLIENT";

                        //20171123 by huangjiayin: 用户需求几个账号不做自动登出，for大屏幕展示
                SuperUserList = ConfigurationManager.AppSettings["SuperUserList"].Split(';').ToList<string>();

                _autoLogoutTime = int.Parse(ConfigurationManager.AppSettings["AutoLogoutTime"]);
                Dic_Pipe = new Dictionary<string, Interface>();
                Dic_LinkSignal_JobDataPathNoU = new Dictionary<string, string>();
                Dic_LinkSignal_JobDataPathNoD = new Dictionary<string, string>();

                Q_BCMessage = new Queue<string>();
                Q_RobotMessage = new Queue<string>();
                Q_OPIMessage = new Queue<OPIMessage>();
                Lst_RobotCommand = new List<RobotCommandReport.COMMANDc>();
                //Lst_BCSParameterItem = new List<string>();

                KillUserAuthority = new List<string>();
                BCOpiTrx = new List<SBCS_OPIHISTORY_TRX>();
                LocalHostName = Dns.GetHostName();  // 取得本機名稱
                LocalIPAddress = GetIPAddress(LocalHostName);

                RunModeHaveChange = false;
                IndexerModeHaveChange = false;

                BCSMessageDisplayTime = 5;
                SocketReplyFlag = false;
                SocketMonitorWaitTime = 15;
                SocketWaitTime = 5;
                SocketRetryCount = 2;

                SocketResponseTime = 5000;
                //SocketResponseTime_Material = 15000;
                SocketResponseTime_MES = 15000;
                SocketResponseTime_MapDownload = 15000;
                SocketResponseTime_Query = 15000;

                RBCmdDisplayCnt = 50;
                QueryMaxCount = 1000;
                QueryPageSixeCount = 500;

                BC_EquipmentDataLinkStatusReply = new BCS_EquipmentDataLinkStatusReply();

                ReturnCodeSuccess = "0000000";

                RobotMixRunFlag = false;

                //初始化 DBConfig.xml
                if (File.Exists(OPIConst.ParamFolder + OPIConst.DBCFG_XML_FILE_NAME))
                {
                    DBConfigXml = new csDBConfigXML(OPIConst.ParamFolder + OPIConst.DBCFG_XML_FILE_NAME, out _err);

                    if (_err != string.Empty) ErrMessage = _err;
                }
                else ErrMessage = string.Format("File is not exists. Path [{0}]", OPIConst.ParamFolder + OPIConst.DBCFG_XML_FILE_NAME);

                if (ErrMessage != string.Empty ) return;


                SocketIp = DBConfigXml.dic_Setting[strFabType].dic_LineType[strLineType].dic_Line[strServerName].SocketIP.ToString();
                SocketPort = DBConfigXml.dic_Setting[strFabType].dic_LineType[strLineType].dic_Line[strServerName].SocketPort.ToString();

                #region 取得Package Version 20150422 tom
                XmlDocument doc =new XmlDocument();
                try
                {
                    doc.Load(@"..\Config\Startup.xml");
                    Version=doc["framework"]["version"].InnerText;
                }
                     
                catch (System.Exception ex)
                {
                    ErrMessage=ex.ToString();
                }
                #endregion

                #region 建立DB連線 & 判斷使用者是否正確
                DBConnStr = DBConfigXml.dic_Setting[strFabType].dic_LineType[strLineType].dic_Line[strServerName].DBConn.ToString();
                SqlConnection LoginDBConn = new SqlConnection(DBConnStr);
                DBCtx = new UniBCSDataContext(LoginDBConn);

                try
                {
                    LoginDBConn.Open();
                    LoginDBConn.Close();
                }
                catch (Exception ex)
                {
                    ErrMessage = string.Format("Connecting [ {0} ] DataBase Error \r\n\r\n  {1}", strServerName, ex.ToString());
                    return;
                }

                SBRM_OPI_USER_ACCOUNT sbrm_User = DBCtx.SBRM_OPI_USER_ACCOUNT.Where(r => r.USER_ID == userID && (r.CLIENT_KEY == "ALL" || r.CLIENT_KEY == strServerName)).FirstOrDefault();
                if (sbrm_User == null)
                {
                    ErrMessage = "Can not fine User";
                    return;
                }
                if (sbrm_User.PASSWORD.ToString() != userPassword)
                {
                    ErrMessage = "Login fail,User Passwor error.";
                    return;
                }

                if (sbrm_User.ACTIVE.ToUpper() != "Y")
                {
                    ErrMessage = "Login fail, Acrive is not Y.";
                    return;
                }

                if (sbrm_User.UACACTIVE.ToUpper()!= "Y")
                {
                    ErrMessage = "Login fail, UAC Active is not Y";
                    return;
                }

                LoginUserID = sbrm_User.USER_ID;
                LoginUserName = sbrm_User.USER_NAME;
                LoginPassword = sbrm_User.PASSWORD;
                LoginGroupID = sbrm_User.GROUP_ID;

                //mark ,有点问题,by yang
                //应该是Linq本身中文字库不完全,所以有些login的UserName无法找到
                string old_trx_datetime = sbrm_User.TRX_DATETIME.ToString();
                sbrm_User.TRX_DATETIME = System.DateTime.Now;         //update trx_datetime when login by yang 2017/4/10
                try
                {
                    DBCtx.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                    NLogManager.Logger.LogInfoWrite(this.GetType().Name, "TRX_DATETIME_UPDATE",string.Format( "User ({0}) TRX_DATETIME ({1}) UPDATE to ({2}) ! ",LoginUserID,old_trx_datetime,sbrm_User.TRX_DATETIME.ToString()));
                }
                catch (System.Data.Linq.ChangeConflictException err)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, "TRX_DATETIME_UPDATE", err);

                    foreach (System.Data.Linq.ObjectChangeConflict occ in DBCtx.ChangeConflicts)
                    {
                        // 將變更的欄位寫入資料庫（合併更新）
                        occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                    }

                    try
                    {
                        DBCtx.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, "TRX_DATETIME_UPDATE", ex);
                    }
                }

                #endregion

                #region Load DB Line Data

                var _var = DBCtx.SBRM_LINE.Where(r => r.SERVERNAME == strServerName);

                if (_var == null)
                {
                    ErrMessage = string.Format("SBRM_LINE can't find Server Name[{0}]", strServerName);
                    return;
                }

                foreach (SBRM_LINE sbrm_line in _var)
                {
                    if (CurLine.LineID == null)
                    {
                        CurLine.LineID = sbrm_line.LINEID;
                        CurLine.LineName = sbrm_line.LINENAME;
                        CurLine.LineType = sbrm_line.LINETYPE;
                        CurLine.FabType = sbrm_line.FABTYPE;
                        CurLine.ServerName = sbrm_line.SERVERNAME;
                        CurLine.JobDataLineType = sbrm_line.JOBDATALINETYPE;
                        CurLine.CrossLineRecipeCheck = sbrm_line.CHECKCROSSRECIPE == "Y" ? true : false;
                        CurLine.LineID2 = string.Empty;
                        CurLine.HistoryType = sbrm_line.HISTORYTYPE;
                        CurLine.LineSpecialFun = sbrm_line.OPI_FUNCTION;
                    }
                    else
                    {
                        CurLine.LineID2 = sbrm_line.LINEID;
                    }
                }
                #endregion


                #region 取得相同line type的連線資訊
                var dblines = DBCtx.SBRM_LINE.Where(r => r.FABTYPE == CurLine.FabType);
                if (dblines != null)
                {
                    Dic_Line = new Dictionary<string, Line>();
                    foreach (SBRM_LINE lineData in dblines)
                    {
                        Line _line = new Line();
                        _line.LineID = lineData.LINEID;
                        _line.LineName = lineData.LINENAME;
                        _line.LineType = lineData.LINETYPE;
                        _line.FabType = lineData.FABTYPE;
                        _line.ServerName = lineData.SERVERNAME;
                        _line.JobDataLineType = lineData.JOBDATALINETYPE;
                        _line.ConnectionData = DBConfigXml.getLineData(_line.ServerName);
                        _line.LineSpecialFun = lineData.OPI_FUNCTION;

                        if (Dic_Line.ContainsKey(_line.ServerName))
                        {
                            _line.LineID2 = lineData.LINEID;
                        }
                        else
                        Dic_Line.Add(_line.ServerName , _line);
                    }
                }
                #endregion

                #region Load SBRM_MPLC_INTERLOCK Node Data
                List<SBRM_MPLC_INTERLOCK> _lstInterLock = new List<SBRM_MPLC_INTERLOCK>();

                var _interlock = DBCtx.SBRM_MPLC_INTERLOCK.Where(d => d.LINETYPE.Equals(CurLine.LineType));

                if (_interlock != null)
                {
                    _lstInterLock = _interlock.ToList();
                }
                #endregion

                //#region Load SBRM_VCR Node Data
                //List<SBRM_VCR> _lstVCR = new List<SBRM_VCR>();

                //var _vcr = DBCtx.SBRM_VCR.Where(d => d.LINETYPE.Equals(CurLine.LineType));

                //if (_vcr != null)
                //{
                //    _lstVCR = _vcr.ToList();
                //}
                //#endregion

                #region Load DB Node Data
                var rstNODE = DBCtx.SBRM_NODE.Where(r => r.SERVERNAME == strServerName).OrderBy(r => r.RECIPEIDX);
                Dic_Node = new Dictionary<string, Node>();
                Dic_RecipSeq = new SortedDictionary<int, string>();

                foreach (SBRM_NODE sbrm_node in rstNODE)
                {
                    int _vcrCnt = sbrm_node.VCRCOUNT != null ? int.Parse(sbrm_node.VCRCOUNT.ToString()) : 0;

                    Node _Node = new Node();
                    _Node.LineID = sbrm_node.LINEID;
                    _Node.ServerName = sbrm_node.SERVERNAME;
                    _Node.NodeNo = sbrm_node.NODENO;
                    _Node.NodeID = sbrm_node.NODEID;
                    _Node.ReportMode = sbrm_node.REPORTMODE;
                    _Node.NodeAttribute = sbrm_node.NODEATTRIBUTE;
                    _Node.RecipeLen = sbrm_node.RECIPELEN;                    
                    _Node.DefaultRecipeNo = "0".PadLeft(_Node.RecipeLen, '0');
                    _Node.UnitCount = sbrm_node.UNITCOUNT;
                    _Node.NodeName = sbrm_node.NODENAME;
                    _Node.UseEDCReport = sbrm_node.USEEDCREPORT;
                    _Node.UseRunMode = sbrm_node.USERUNMODE; //Y : 有run mode 且OPI提供run mode切換, R : 有run mode 且OPI不提供run mode切換 ,N: 沒有run mode
                    _Node.UseIndexerMode = sbrm_node.USEINDEXERMODE == "Y" ? true : false;
                    _Node.VCRs = GetNodeVCRs(_Node.NodeNo, _vcrCnt);
                    _Node.InterLocks = GetNodeInterlocks(_lstInterLock, _Node.NodeNo);
                    _Node.SamplingSides = GetNodeSamplingSides(sbrm_node.OPITYPE);
                    _Node.OPISpecialType = sbrm_node.OPITYPE;
                    _Node.RecipeRegisterCheck = sbrm_node.RECIPEREGVALIDATIONENABLED == "Y" ? true : false;
                    _Node.RecipeParameterCheck = sbrm_node.RECIPEPARAVALIDATIONENABLED == "Y" ? true : false;
                    _Node.APCReport= sbrm_node.APCREPORT == "Y" ? true : false;
                    _Node.EnergyReport= sbrm_node.ENERGYREPORT == "Y" ? true : false;
                    _Node.APCReportTime = sbrm_node.APCREPORTTIME;
                    _Node.EnergyReportTime = sbrm_node.ENERGYREPORTTIME;

                    if (sbrm_node.RECIPESEQ == null) _Node.RecipeSeq = new List<int> { 0 };
                    else _Node.RecipeSeq = sbrm_node.RECIPESEQ.Split(',').Select(int.Parse).ToList();

                    Dic_Node.Add(sbrm_node.NODENO, _Node);

                    if (_Node.UseIndexerMode) CurLine.IndexerNode = _Node;
                    if (_Node.OPISpecialType == "CV06") CurLine.CV06_Node = _Node;

                    #region 依據Recipe Seq 新增recipe node 
                    if (_Node.RecipeSeq.Count == 0) continue;
                    if (_Node.RecipeSeq.Contains(0)) continue;

                    foreach (int _seq in _Node.RecipeSeq)
                    {
                        if (Dic_RecipSeq.ContainsKey(_seq))
                        {
                            ErrMessage = string.Format("SBRM_NODE RECIPESEQ[{0}] is duplicate ", _seq.ToString());
                            return;
                        }
                        Dic_RecipSeq.Add(_seq, _Node.NodeNo);
                    }
                    #endregion
                }


                //#region Check 是否有相同性質的insp機台，若有 提供same eqp flag顯示&設定 -- just for array
                //if (strFabType.Equals("ARRAY"))
                //{
                //    var _varTmp = Dic_Node.Values.Where(r => r.NodeAttribute.Equals("IN")).ToList();

                //    foreach (Node _node in _varTmp)
                //    {
                //        if (Dic_Node.Values.Where(r => r.NodeName.Equals(_node.NodeName)).Count() > 1)
                //        {
                //            RobotMixRunFlag = true;
                //            break;
                //        }
                //    }
                //}
                //#endregion

                #endregion

                #region Load DB Port Data
                var rstPort = DBCtx.SBRM_PORT.Where(r => r.SERVERNAME == strServerName);
                Dic_Port = new Dictionary<string, Port>();
                Dic_Dense = new Dictionary<string, Dense>();
                Dic_Pallet = new Dictionary<string, Pallet>();
                foreach (SBRM_PORT r in rstPort)
                {
                    //PPK : Pallet不需提供下貨；M Port不需提供下貨；Port 01 & Port 15 提供Box 下貨 --from pony
                    if (strLineType == "PPK" || strLineType == "QPP")                   
                    {
                        if (r.PORTATTRIBUTE == "PALLET")
                        {
                            #region Pallet Port
                            Pallet _p = new Pallet();
                            _p.PalletNo = r.PORTNO;
                            _p.PalletID = string.Empty;
                            _p.PalletMode = ePalletMode.UnKnown;
                            _p.PalletDataRequest = false;
                            Dic_Pallet.Add(_p.PalletNo, _p);
                            #endregion
                        }
                        else  //BOX,PPK_MANUAL,DENSE
                        {
                            #region Dense Port
                            Dense _dense = new Dense();

                            _dense.NodeNo = r.NODENO;
                            _dense.NodeID = r.NODEID;
                            _dense.PortNo = r.PORTNO;
                            _dense.PortID = r.PORTID;

                            _dense.BoxID01 = string.Empty;
                            _dense.BoxID02 = string.Empty;
                            _dense.PaperBoxID = string.Empty;
                            _dense.DenseDataRequest = false;
                            _dense.PackingMode = ePackingMode.UnKnown;
                            _dense.PortEnable = ePortEnable.Unknown;
                            _dense.UnpackSource = eUnpackSource.UnKnown;
                            _dense.BoxType = eBoxType.Unknown;

                            Dic_Dense.Add(r.NODENO.PadRight(3, ' ') + r.PORTNO.PadRight(2, '0'), _dense);
                            #endregion 
                        }
                    }
                    else
                    {
                        #region Get Port Info
                        Port _port = new Port();
                        _port.LineID = r.LINEID;
                        _port.ServerName = r.SERVERNAME;
                        _port.NodeNo = r.NODENO;
                        _port.NodeID = r.NODEID;
                        _port.PortNo = r.PORTNO;
                        _port.RelationPortNo = r.PORTNO;
                        _port.PortID = r.PORTID;
                        _port.MaxCount = r.MAXCOUNT != null ? int.Parse(r.MAXCOUNT.ToString()) : 0;
                        _port.PortName = string.Empty;
                        _port.PortAttribute = r.PORTATTRIBUTE;
                        _port.ProcessStartType = r.PROCESSSTARTTYPE;
                        _port.MapplingEnable = r.MAPPINGENABLE == "TRUE" ? true : false;
                        //_port.PositionTrxNo = r.POSITIONPLCTRXNO;

                        if (r.CSTTYPE == "WIRE") _port.CassetteType = eCassetteType.WireCassette;
                        else if (r.CSTTYPE == "CELL") _port.CassetteType = eCassetteType.CellCassette;
                        else if (r.CSTTYPE == "DENSE") _port.CassetteType = eCassetteType.DenseBox;
                        else if (r.CSTTYPE == "BUFFER") _port.CassetteType = eCassetteType.BufferCassette;
                        else if (r.CSTTYPE == "SCRAP") _port.CassetteType = eCassetteType.Scrap;
                        else _port.CassetteType = eCassetteType.NormalCassette;

                        if (strLineType == "CBDPI")
                        {
                            switch (r.PORTNO)
                            {
                                case "01": _port.RelationPortNo = "02"; break;
                                case "02": _port.RelationPortNo = "01"; break;
                                case "03": _port.RelationPortNo = "04"; break;
                                case "04": _port.RelationPortNo = "03"; break;
                                case "05": _port.RelationPortNo = "06"; break;
                                case "06": _port.RelationPortNo = "05"; break;
                            }
                        }

                        Dic_Port.Add(r.NODENO.PadRight(3, ' ') + r.PORTNO.PadRight(2, '0'), _port);
                        #endregion
                    }
                    
                }
                #endregion

                #region Load DB Unit Data
                Dic_Unit = new Dictionary<string, Unit>();
                var rstUnit = DBCtx.SBRM_UNIT.Where(r => r.SERVERNAME == strServerName);
                foreach (SBRM_UNIT r in rstUnit)
                {
                    Unit _unit = new Unit();
                    _unit.LineID = r.LINEID;
                    _unit.ServerName = r.SERVERNAME;
                    _unit.NodeNo = r.NODENO;
                    _unit.NodeID = r.NODEID;
                    _unit.UnitID = r.UNITID;
                    _unit.UnitNo = r.UNITNO;
                    _unit.UnitType = r.UNITTYPE;
                    _unit.UseRunMode = r.USERUNMODE.ToString().ToUpper();

                    Dic_Unit.Add(r.NODENO.PadRight(3, ' ') + r.UNITNO.PadLeft(2, '0'), _unit);
                }
                #endregion

                #region Update Unit ID -- interlock
                //string _unitKey = string.Empty;
                //foreach (Node _node in Dic_Node.Values)
                //{
                //    foreach (InterLock _interlock in _node.InterLocks)
                //    {
                //        // Key: NODENO(3) + UNITNO(2)
                //        _unitKey = _node.NodeNo.PadRight(3, ' ') + _interlock.UnitNo.PadLeft(2, '0');

                //        if (Dic_Unit.ContainsKey(_unitKey)) _interlock.UnitID = Dic_Unit[_unitKey].UnitID;

                //    }
                //}
                #endregion

                #region Load indexer robot Stage
                List<SBRM_EQPSTAGE> lstStage = DBCtx.SBRM_EQPSTAGE.Where(d => d.LineType == strLineType).ToList();

                CurLine.BC_RobotOperationModeReply.IndexerRobotStages = GetNodeStage(lstStage);
                #endregion

                #region Load DB Button Data
                Lst_Button = new List<CustButton>();
                var rstButton = from a in DBCtx.SBRM_OPI_BUTTON_RELATION.Where(d => d.ACTIVE.Equals("Y"))
                                join l in DBCtx.SBRM_OPI_LINE_FUNCTION.Where(d => d.LINETYPE.Equals(CurLine.LineType)) on a.BUTTON_KEY equals l.BUTTON_KEY 
                                join u in DBCtx.SBRM_OPI_USER_GROUP.Where(d => d.GROUP_ID.Equals(LoginGroupID)) on a.BUTTON_KEY equals u.BUTTON_KEY // into ug
                                //from d in ug.DefaultIfEmpty()
                                select new
                                {
                                    BUTTON_KEY=a.BUTTON_KEY,
                                    BUTTON_CAPTION=a.BUTTON_CAPTION,
                                    BUTTON_SEQUENCE=a.BUTTON_SEQUENCE,
                                    MIN_BUTTON_ID=a.MIN_BUTTON_ID,
                                    FUN_BUTTON_ID=a.FUN_BUTTON_ID,
                                    SUB_BUTTON_ID=a.SUB_BUTTON_ID,
                                    BUTTON_IMAGE=a.IMAGE,
                                    BUTTON_VISIBLE=(l.VISIBLE=="Y" && u.VISIBLE=="Y")?true:false,   //d.VISIBLE==null? l.VISIBLE=="Y"?true :false:  d.VISIBLE=="Y"?true :false,
                                    BUTTON_ENABLE=u.ENABLE=="Y"?true:false,
                                    BUTTON_NAME = a.OBJECTNAME,
                                    BUTTON_DESC = a.BUTTON_DESC
                                };
                foreach (var b in rstButton)
                {

                    CustButton button = new CustButton();
                    button.ButtonKey = b.BUTTON_KEY;
                    button.ButtonCaption = b.BUTTON_CAPTION;
                    button.ButtonSequence = b.BUTTON_SEQUENCE;
                    button.ButtonVisible = b.BUTTON_VISIBLE;
                    button.ButtonImage = b.BUTTON_IMAGE;
                    button.ButtonEnable = b.BUTTON_ENABLE;
                    button.ButtonName = b.BUTTON_NAME;
                    button.ButtonDesc = b.BUTTON_DESC;

                    if (string.IsNullOrWhiteSpace(b.SUB_BUTTON_ID) && string.IsNullOrWhiteSpace(b.FUN_BUTTON_ID))  //Main
                    {
                        button.ButtonType=buttonType.Main;
                        button.ButtonID = b.MIN_BUTTON_ID;
                        button.ButtonParentButtonKey = string.Empty;
                    }
                    else if (string.IsNullOrWhiteSpace(b.FUN_BUTTON_ID))//Sub
                    {
                        button.ButtonType = buttonType.Sub;
                        button.ButtonID = b.SUB_BUTTON_ID;
                        var parentButtonKey=rstButton.Where(d=>d.MIN_BUTTON_ID.Equals(b.MIN_BUTTON_ID)).OrderBy(d=>d.SUB_BUTTON_ID).ThenBy(d=>d.FUN_BUTTON_ID).FirstOrDefault() ;
                        button.ButtonParentButtonKey = parentButtonKey.BUTTON_KEY;
                    }
                    else//Function
                    {
                        button.ButtonType = buttonType.Function;
                        button.ButtonID = b.FUN_BUTTON_ID;
                        var parentButtonKey = rstButton.Where(d => d.MIN_BUTTON_ID.Equals(b.MIN_BUTTON_ID) && d.SUB_BUTTON_ID.Equals(b.SUB_BUTTON_ID)).OrderBy(d => d.FUN_BUTTON_ID).FirstOrDefault();
                        button.ButtonParentButtonKey = parentButtonKey.BUTTON_KEY;
                    }
                    Lst_Button.Add(button);
                }

                #endregion

                #region Load DB Robot Data
                var rstROBOT = DBCtx.SBRM_ROBOT.Where(r => r.SERVERNAME == strServerName);
                Dic_Robot = new Dictionary<string, Robot>();
                foreach (SBRM_ROBOT sbrm_robot in rstROBOT)
                {
                    Robot _Robot = new Robot();
                    _Robot.LineType = sbrm_robot.LINETYPE;
                    _Robot.RobotName = sbrm_robot.ROBOTNAME;
                    _Robot.LineID = sbrm_robot.LINEID;
                    _Robot.NodeNo = sbrm_robot.NODENO;
                    _Robot.ServerName = sbrm_robot.SERVERNAME;
                    _Robot.UnitNo = sbrm_robot.UNITNO;
                    _Robot.RobotControlMode = string.Empty;
                    
                    _Robot.RobotArmCount = sbrm_robot.ROBOTARMQTY;
                    _Robot.ArmMaxJobCount = sbrm_robot.ARMJOBQTY;

                    _Robot.LstArms = new List<ArmInfo>();

                    for (int i = 1 ; i <= _Robot.RobotArmCount ;i++)
                    {
                        ArmInfo _arm = new ArmInfo();
                        _arm.ArmNo = i.ToString().PadLeft(2,'0');
                        _arm.ArmName = "Arm"+i.ToString().PadLeft(2, '0');
                        _arm.JobExist_Front = eRobotJobStatus.UnKnown;
                        _arm.CstSeqNo_Front = string.Empty ;
                        _arm.JobSeqNo_Front = string.Empty;
                        _arm.TrackingData_Front =string.Empty;
                        _arm.JobExist_Back = eRobotJobStatus.UnKnown;
                        _arm.CstSeqNo_Back = string.Empty;
                        _arm.JobSeqNo_Back = string.Empty;
                        _arm.TrackingData_Back = string.Empty;

                        #region Test
                        //_arm.JobExist_Front = eRobotJobStatus.Exist;
                        //_arm.CstSeqNo_Front = i.ToString();
                        //_arm.JobSeqNo_Front = (i+1).ToString();
                        //_arm.TrackingData_Front = i.ToString();
                        //_arm.JobExist_Back = eRobotJobStatus.NoExist;
                        //_arm.CstSeqNo_Back = (i + 2).ToString();
                        //_arm.JobSeqNo_Back = (i + 3).ToString();
                        //_arm.TrackingData_Back = (i + 1).ToString();
                        #endregion

                        _Robot.LstArms.Add(_arm);

                        RobotCommandReport.COMMANDc _cmd = new RobotCommandReport.COMMANDc();
                        _cmd.COMMAND_SEQ=i.ToString();
                        Lst_RobotCommand.Add(_cmd);
                    }

                    Dic_Robot.Add(sbrm_robot.ROBOTNAME, _Robot);
                }
                #endregion

                #region Load DB Robot Stage Data
                Lst_RobotStage = new List<RobotStage>();

                var _rstStage = DBCtx.SBRM_ROBOT_STAGE.Where(r => r.SERVERNAME == strServerName ); //&& r.STAGETYPE != "PORT");

                foreach (SBRM_ROBOT_STAGE _data in _rstStage)
                {
                    RobotStage _stage = new RobotStage();

                    _stage.NodeNo = _data.NODENO;
                    _stage.RobotName = _data.ROBOTNAME;
                    _stage.StageID = _data.STAGEID.ToString().PadLeft(2, '0');
                    _stage.StageName = _data.STAGENAME;
                    _stage.StageType = _data.STAGETYPE;
                    _stage.Lst_JobData = new List<StageJobData>();
                    _stage.SlotMaxCount = _data.SLOTMAXCOUNT;
                    _stage.StageStatus = eRobotStageStatus.UnKnown;

                    #region Test
                    //for (int i = 1; i <= 2; i++)
                    //{
                    //    StageJobData _job = new StageJobData();
                    //    _job.SlotNo = (i).ToString().PadLeft(2, '0');
                    //    _job.JobExist = eRobotJobStatus.Exist;
                    //    _job.CstSeqNo = (i + 0).ToString();
                    //    _job.JobSeqNo = (i + 1).ToString();
                    //    _job.TrackingData = (i + 0).ToString();
                    //    _stage.Lst_JobData.Add(_job);
                    //}
                    #endregion

                    Lst_RobotStage.Add(_stage);
                }
                                    
                #endregion

                #region Load Link Signal Description
                Lst_LinkSignal_Type = new List<LinkSignalType>();
                Dic_LinkSignal_Desc = new Dictionary<string, LinkSignalBitDesc>();

                string _path = string.Format(@"..\Config\OPI\LinkSignal\{0}\{1}\LinkSignal.xml", CurLine.FabType, CurLine.LineType);
                string _descPatch = string.Format(@"..\Config\OPI\LinkSignal\LinkSignalType.xml");
                if (File.Exists(_path) && File.Exists(_descPatch))
                {
                    XDocument _doc = XDocument.Load(_path);

                    if (_doc.Element("LinkSignal") == null) return ;
                    if (_doc.Element("LinkSignal").Elements("Bit") == null) return;

                    #region 依照FABType取值
                    #endregion

                    IEnumerable<XElement> _element = _doc.Element("LinkSignal").Elements("Bit");

                    XDocument _typeDoc = XDocument.Load(_descPatch);

                    foreach (XElement _e in _element)
                    {
                        LinkSignalType _type = new LinkSignalType();

                        if (_e.Attribute("UpStream") == null) continue;
                        if (_e.Attribute("DownStream") == null) continue;
                        if (_e.Attribute("SeqNo") == null) continue;
                        if (_e.Attribute("LinkType") == null) continue;
                        if (_e.Attribute("TimingChart") == null) continue;

                        //Upstream path no + Downstream path no
                        if (_e.Attribute("SeqNo").Value.Length != 4) continue;

                        _type.UpStreamLocalNo = _e.Attribute("UpStream").Value;
                        _type.DownStreamLocalNo = _e.Attribute("DownStream").Value;
                        _type.SeqNo = _e.Attribute("SeqNo").Value;
                        _type.LinkType = _e.Attribute("LinkType").Value;
                        _type.TimingChart = _e.Attribute("TimingChart").Value;


                        if (GetLinkSignalDesc(_type.LinkType, _typeDoc, strFabType))
                        {
                            Lst_LinkSignal_Type.Add(_type);
                        }
                    }
                }
                #endregion

                #region 取得OPI Param
                List<SBRM_OPI_PARAMETER> _lstSbrmOpiParam = DBCtx.SBRM_OPI_PARAMETER.Where(d => d.LINETYPE == CurLine.LineType || d.LINETYPE == "ALL").ToList();

                int _cnt = 0;

                if (CurLine.FabType == "ARRAY") GlassIDMaxLength = 9;
                else if (CurLine.FabType == "CF") GlassIDMaxLength = 7;
                else GlassIDMaxLength = 6;

                foreach (SBRM_OPI_PARAMETER _param in _lstSbrmOpiParam)
                {
                    switch (_param.KEYWORD)
                    {
                        case "INDEXER_MODE" :   //取得current line設定的 indexer Mode
                            CurLine.LineIndexerModes = GetLineIndexerMode(_param);
                            break;

                        case "LineRunMode": //取得current line設定的 run Mode & 取得哪些line run Mode允許選擇unit run mode
                            GetLineRunMode(_param);
                            break;

                        case "UnitRunMode": //取得current line設定的 Unit run Mode 
                            GetUnitRunMode(_param);
                            break;

                        case "FetchProportionalName":
                            CurLine.FetchGlassProportionalNames = GetFetchGlassProportionalName(_param);
                            
                            break;

                        case "ProcessType_Array": //取得Array Process Type定義
                            CurLine.Lst_ProcessType_Array = GetProcessType_Array(_param);
                            break;

                        case "SocketRetryCount": //取得socket 重連次數
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketRetryCount = _cnt;

                            break;

                        case "SocketMonitorWaitTime": //取得Monitor Request socket送出間隔時間(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketMonitorWaitTime = _cnt;

                            break;

                        case "SocketResponseTime_Comm": //取得同步socket response timeoute時間(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketResponseTime = _cnt;

                            break;

                        case "SocketResponseTime_MES": //取得同步MES Command socket response timeoute時間(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketResponseTime_MES = _cnt;

                            break;

                        case "SocketResponseTime_MapDownload": //取得同步Map Download Command socket response timeoute時間(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketResponseTime_MapDownload = _cnt;

                            break;

                        case "SocketResponseTime_Query": //取得同步Map Download Command socket response timeoute時間(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            SocketResponseTime_Query = _cnt;

                            break;

                        case "KillUserAhthority": //取得可刪除client連線的groupid
                            KillUserAuthority = _param.ITEMVALUE.Split(',').ToList();

                            break;

                        case "BCSMessageDisplayTime": //BCS Terminal Message 自動關閉時間(秒)，若設為0表示不會自動關閉(秒)
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            BCSMessageDisplayTime = _cnt;

                            break;

                        case "GlassIDMaxLength"://offline下貨提供user輸入的glass id限制長度,預設Glass ID 長度限制 -- Array & CF：最大8 码,CELL： 最大7码
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            GlassIDMaxLength = _cnt;

                            break;

                        case "QueryMaxCount"://History Query Max Count
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            QueryMaxCount = _cnt;

                            break;

                        case "QueryPageSixeCount": //History Query 分頁顯示筆數
                            int.TryParse(_param.ITEMVALUE, out _cnt);
                            QueryPageSixeCount = _cnt;

                            break;

                        case "RBCmdDisplayCount"://Robot Command Display Count

                            RBCmdDisplayCnt = (int.TryParse(_param.ITEMVALUE, out _cnt) == true) ? int.Parse(_param.ITEMVALUE) : 50;

                            break;

                        //case "BCS_ParameterItem":

                        //    Lst_BCSParameterItem = _param.ITEMVALUE.ToString().Split(',').ToList();

                        //    break;

                        case "JobDataForLinkSignalUp":  //Link Signal UpStream job Data對應path no

                            Dic_LinkSignal_JobDataPathNoU.Add(_param.SUBKEY, _param.ITEMVALUE.ToString());

                            break;

                        case "JobDataForLinkSignalDown":  //Link Signal DownStream job Data對應path no

                            Dic_LinkSignal_JobDataPathNoD.Add(_param.SUBKEY, _param.ITEMVALUE.ToString());

                            break;

                        case "RobotMixRunFlag": //是否提供Robot Mix Run Flag設定功能

                            string[] _line = _param.SUBKEY.ToString().Split(',');

                            if (_line.Contains(strServerName)) RobotMixRunFlag = true;

                            break;

                        default :
                            break;
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                ErrMessage = ex.ToString();
            }
        }

        private bool GetLinkSignalDesc(string LinkType, XDocument XmlDoc,string FabType)
        {
            try
            {
                if (Dic_LinkSignal_Desc.ContainsKey(LinkType)) return true;

                if (XmlDoc.Element("LinkSignal") == null) return false;
                if (XmlDoc.Element("LinkSignal").Elements("Type") == null) return false;

                var _v = from page in XmlDoc.Element("LinkSignal").Elements("Type")
                         where LinkType == page.Attribute("Name").Value && page.Attribute("FABTYPE").Value.Equals(FabType)
	                    select page;

                int _seqNo = 0;

                if (_v ==null || _v.Count() ==0 ) return false;

                XElement _e = _v.First();

                if (_e.Element("UpStreamBit") == null) return false;
                if (_e.Element("UpStreamBit").Elements("Bit") == null) return false;

                IEnumerable<XElement> _upElement = _e.Element("UpStreamBit").Elements("Bit");

                LinkSignalBitDesc _bitDesc = new LinkSignalBitDesc();

                foreach (XElement _d in _upElement)
                {
                    if (_d.Attribute("SeqNo") == null) continue;
                    if (_d.Attribute("Description") == null) continue;

                    int.TryParse(_d.Attribute("SeqNo").Value, out _seqNo);

                    if (_bitDesc.UpStreamBit.ContainsKey(_seqNo)) continue;

                    _bitDesc.UpStreamBit.Add(_seqNo, _d.Attribute("Description").Value);                        
                }

                if (_e.Element("DownStreamBit") == null) return false;
                if (_e.Element("DownStreamBit").Elements("Bit") == null) return false;

                IEnumerable<XElement> _downElement = _e.Element("DownStreamBit").Elements("Bit");

                foreach (XElement _d in _downElement)
                {

                    if (_d.Attribute("SeqNo") == null) continue;
                    if (_d.Attribute("Description") == null) continue;

                    int.TryParse(_d.Attribute("SeqNo").Value, out _seqNo);

                    if (_bitDesc.DownStreamBit.ContainsKey(_seqNo)) continue;

                    _bitDesc.DownStreamBit.Add(_seqNo, _d.Attribute("Description").Value);
                }

                Dic_LinkSignal_Desc.Add(LinkType, _bitDesc);

                return true;
            }
            catch (Exception ex)
            {
                ErrMessage = ex.Message;

                return false;
            }
        }

        private string GetIPAddress(string LocalHostName)
        {
            try
            {
                //// 取得本機名稱
                //string strHostName = Dns.GetHostName();
                //// 取得本機的IpHostEntry類別實體，用這個會提示已過時

                ////IPHostEntry iphostentry = Dns.GetHostByName(strHostName);


                // 取得本機的IpHostEntry類別實體，MSDN建議新的用法
                IPHostEntry iphostentry = Dns.GetHostEntry(LocalHostName);

                // 取得所有 IP 位址
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    // 只取得IP V4的Address
                    if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipaddress.ToString();
                    }
                }
                return string.Empty ;
            }
            catch (Exception ex)
            {
                ErrMessage = ex.Message;
                return string.Empty;
            }
        }

        private List<LoaderOperationMode_ATS> GetLoaderOperationModes(List<SBRM_OPI_PARAMETER> lstOPIParam)
        {
            List<LoaderOperationMode_ATS> lstOperationModes = new List<LoaderOperationMode_ATS>();
            string operationModeParamKey = "OperationMode";
            if (lstOPIParam == null && lstOPIParam.Count == 0) return lstOperationModes;

            SBRM_OPI_PARAMETER opiParam = lstOPIParam.Find(d => d.KEYWORD.Equals(operationModeParamKey));
            if (opiParam == null) return lstOperationModes;
            string[] runModes = opiParam.ITEMVALUE.Split(',');   //1：t1 Loader Mode,2：t2 Loader Mode,3：Auto Change Mode
            foreach (string runMode in runModes)
            {
                string[] operationModeDet = runMode.Split(':');
                if (operationModeDet.Count() < 2) continue;
                LoaderOperationMode_ATS operationMode = new LoaderOperationMode_ATS();
                int intRunModeNo = 0;
                int.TryParse(operationModeDet[0], out intRunModeNo);
                operationMode.OperationModeNo = intRunModeNo;
                operationMode.OperationModeName = operationModeDet[1];
                lstOperationModes.Add(operationMode);
            }
            return lstOperationModes;
        }

        private void GetLineRunMode(SBRM_OPI_PARAMETER opiParam)
        {
            if (Dic_Node.ContainsKey(opiParam.SUBKEY) == false) return;

            #region  取得哪些line run Mode
            List<LineRunMode> _lstLineRunModes = new List<LineRunMode>();

            string[] _runModes = opiParam.ITEMVALUE.Split(',');
            foreach (string _runMode in _runModes)
            {
                string[] runModeDet = _runMode.Split(':');

                LineRunMode _lineRunMode = new LineRunMode();

                _lineRunMode.RunModeNo = runModeDet[0];
                _lineRunMode.RunModeName = runModeDet.Count() == 1 ? runModeDet[0] : runModeDet[1];
                _lstLineRunModes.Add(_lineRunMode);
            }
            
            Dic_Node[opiParam.SUBKEY].LineRunModes = _lstLineRunModes;
            #endregion

            #region  取得哪些line run Mode允許選擇unit run mode
            if (opiParam.CONDITION != string.Empty)
            {
                List<string> _lstAllowUnitModes = opiParam.CONDITION.Split(',').ToList();

                Dic_Node[opiParam.SUBKEY].AllowUnitRunMode = _lstAllowUnitModes;
            }
            #endregion
        }

        private void GetUnitRunMode(SBRM_OPI_PARAMETER opiParam)
        {
            if (Dic_Node.ContainsKey(opiParam.SUBKEY) == false) return;

            #region  取得哪些line run Mode
            List<LineRunMode> _lstLineRunModes = new List<LineRunMode>();

            string[] _runModes = opiParam.ITEMVALUE.Split(',');
            foreach (string _runMode in _runModes)
            {
                string[] runModeDet = _runMode.Split(':');

                LineRunMode _lineRunMode = new LineRunMode();

                _lineRunMode.RunModeNo = runModeDet[0];
                _lineRunMode.RunModeName = runModeDet.Count() == 1 ? runModeDet[0] : runModeDet[1];
                _lstLineRunModes.Add(_lineRunMode);
            }
            
            Dic_Node[opiParam.SUBKEY].LineUnitRunModes = _lstLineRunModes;
            #endregion

        }

        private List<FetchGlassProportionalName> GetFetchGlassProportionalName(SBRM_OPI_PARAMETER opiParam)
        {
            List<FetchGlassProportionalName> lstFetchGlassProportionalNames = new List<FetchGlassProportionalName>();
            //string _key = "FetchProportionalName";
            //if (lstOPIParam == null && lstOPIParam.Count == 0) return lstFetchGlassProportionalNames;

            //SBRM_OPI_PARAMETER opiParam = lstOPIParam.Find(d => d.KEYWORD.Equals(_key));

            //if (opiParam == null) return lstFetchGlassProportionalNames;

            string[] _items = opiParam.ITEMVALUE.Split(',');

            foreach (string _item in _items)
            {
                string[] _detail = _item.Split(':');

                if (_detail.Count() < 2) continue;

                FetchGlassProportionalName _fetch = new FetchGlassProportionalName();

                int _num = 0;

                int.TryParse(_detail[0], out _num);

                _fetch.ProportionalNameNo = _num;
                _fetch.ProportionalName = _detail[1];
                lstFetchGlassProportionalNames.Add(_fetch);
            }

            return lstFetchGlassProportionalNames;
        }

        private List<ProcessType_Array> GetProcessType_Array(SBRM_OPI_PARAMETER opiParam)
        {
            List<ProcessType_Array> _lst = new List<ProcessType_Array>();
            //string _key = "ProcessType_Array";
            //if (lstOPIParam == null && lstOPIParam.Count == 0) return _lst;

            //SBRM_OPI_PARAMETER opiParam = lstOPIParam.Find(d => d.KEYWORD.Equals(_key));
            //if (opiParam == null) return _lst;
            string[] _items = opiParam.ITEMVALUE.Split(',');

            foreach (string _item in _items)
            {
                string[] _desc = _item.Split(':');
                if (_desc.Count() < 2) continue;

                ProcessType_Array _data = new ProcessType_Array();
                int _no = 0;
                int.TryParse(_desc[0], out _no);

                _data.ProcessTypeNo = _no;
                _data.ProcessTypeName = _desc[1];
                _lst.Add(_data);
            }
            return _lst;
        }

        private List<LineIndexerMode> GetLineIndexerMode(SBRM_OPI_PARAMETER opiParam)
        {
            List<LineIndexerMode> lstLineIndexerModes = new List<LineIndexerMode>();
            //string indexerModeParamKey = "INDEXER_MODE";
            //if (lstOPIParam == null && lstOPIParam.Count == 0) return lstLineIndexerModes;

            //SBRM_OPI_PARAMETER opiParam = lstOPIParam.Find(d => d.KEYWORD.Equals(indexerModeParamKey));
            //if (opiParam == null) return lstLineIndexerModes;

            string[] indexerModes = opiParam.ITEMVALUE.Split(',');  

            foreach (string indexerMode in indexerModes)
            {
                string[] indexerModeDet = indexerMode.Split(':');
                if (indexerModeDet.Count() < 2) continue;
                LineIndexerMode lineIndexerMode = new LineIndexerMode();
                int intIndexerModeNo = 0;
                int.TryParse(indexerModeDet[0], out intIndexerModeNo);
                lineIndexerMode.IndexerModeNo = intIndexerModeNo;
                lineIndexerMode.IndexerModeName = indexerModeDet[1];
                lstLineIndexerModes.Add(lineIndexerMode);
            }
            return lstLineIndexerModes;
        }

        private List<VCR> GetNodeVCRs( string NodeNo,int VCRCnt)
        {
            List<VCR> lstVCRNode = new List<VCR>();

            for (int i = 1; i <= VCRCnt; i++)
            {
                VCR vcr = new VCR();

                vcr.VCRNO = i.ToString().PadLeft(2, '0');

                lstVCRNode.Add(vcr);
            }

            return lstVCRNode;
        }
        
        private List<InterLock> GetNodeInterlocks(List<SBRM_MPLC_INTERLOCK> lstPLCTrxRel, string NodeNo)
        {
            string _unitKey = string.Empty ;

            List<InterLock> lstInterlocks= new List<InterLock>();
            List<SBRM_MPLC_INTERLOCK> fetchInterLocks = lstPLCTrxRel.FindAll(d => d.NODENO.Equals(NodeNo));
            foreach (SBRM_MPLC_INTERLOCK fetchInterlock in fetchInterLocks)
            {
                InterLock interlock = new InterLock();
                interlock.PLCTrxNo = fetchInterlock.PLCTRXNO;
                interlock.Description = fetchInterlock.REMARK;
                interlock.Status = eInterlockMode.OFF;
                lstInterlocks.Add(interlock);
            }
            return lstInterlocks;
        }

        private List<IndexerRobotStage> GetNodeStage(List<SBRM_EQPSTAGE> lstEqpStage)
        {
            List<IndexerRobotStage> lstStage = new List<IndexerRobotStage>();

            List<SBRM_EQPSTAGE> fetchEQPStage = lstEqpStage;
            
            foreach (SBRM_EQPSTAGE eqpStage in fetchEQPStage)
            {
                IndexerRobotStage stage = new IndexerRobotStage();
                stage.LocalNo = eqpStage.NODENO;
                
                stage.RobotPosNo = eqpStage.ROBOTPOSITIONNO;
                stage.Direction = eqpStage.DIRECTION;
                stage.Description = eqpStage.DESCRIPTION;
                //stage.IsReply = true;

                if (Dic_Node.ContainsKey(stage.LocalNo))
                {
                    stage.LocalID = Dic_Node[stage.LocalNo].NodeID;
                }
                else
                {
                    stage.LocalID = string.Empty;
                }

                lstStage.Add(stage);
            }
            return lstStage;
        }

        private List<SamplingSide> GetNodeSamplingSides(string OPISpecialType)
        {
            List<SamplingSide> lstSamplingSide = new List <SamplingSide>();

            if (CurLine.FabType != "CF") return lstSamplingSide;

             SamplingSide _sampling;

            switch (OPISpecialType)
            {
                case "COATER":
                    _sampling = new SamplingSide();
                    _sampling.ItemName = "VCD#01";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);

                    _sampling = new SamplingSide();
                    _sampling.ItemName = "VCD#02";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);

                    _sampling = new SamplingSide();
                    _sampling.ItemName = "VCD#03";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);
                    break ;

                case "OVEN":
                    _sampling = new SamplingSide();
                    _sampling.ItemName = "HP#01";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);

                    _sampling = new SamplingSide();
                    _sampling.ItemName = "HP#02";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);
                    break ;

                case "ALIGNER":
                    _sampling = new SamplingSide();
                    _sampling.ItemName = "CP#01";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);

                    _sampling = new SamplingSide();
                    _sampling.ItemName = "CP#02";
                    _sampling.SideStatus = "0";
                    lstSamplingSide.Add(_sampling);
                    break ;
            }

            return lstSamplingSide;
        }

        private string GetOPIParameter(List<SBRM_OPI_PARAMETER> lstOPIParam , string ItemKey)
        {
            List<LineRunMode> lstLineRunModes = new List<LineRunMode>();

            if (lstOPIParam == null && lstOPIParam.Count == 0) return string.Empty ;

            SBRM_OPI_PARAMETER opiParam = lstOPIParam.Find(d => d.KEYWORD.Equals(ItemKey));

            if (opiParam == null) return string.Empty ;

            return opiParam.ITEMVALUE.ToString();
        }

        public void DisposeAp()
        {
            if (Q_BCMessage!=null) Q_BCMessage.Clear();
            if (Q_RobotMessage != null) Q_RobotMessage.Clear();
            if (Q_OPIMessage != null) Q_OPIMessage.Clear();
            if (Lst_Button != null) Lst_Button.Clear();
        }
    }

    public class OPIMessage
    {

        private string _timeStamp;
        private string _msgCaption;
        private string _msgCode;
        private string _msgData;
        private eOPIMessageType _msgType;

        public OPIMessage(string timeStamp, string msgCaption, string msgData, string msgCode, eOPIMessageType msgTyp = eOPIMessageType.Error)
        {
            _timeStamp =  timeStamp;
            _msgCaption= msgCaption;
            _msgCode = msgCode;
            _msgData = msgData;
            _msgType = msgTyp;
        }

        public OPIMessage(string msgCaption, string msgCode, string msgData, eOPIMessageType msgTyp = eOPIMessageType.Error)
        {
            _timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _msgCaption = msgCaption;
            _msgCode = msgCode;
            _msgData = msgData;
            _msgType = msgTyp;
        }

        public OPIMessage(string msgCaption, Exception msgData)
        {
            _timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _msgCaption = msgCaption;
            _msgCode = string.Empty;
            _msgData = msgData.ToString();
            _msgType = eOPIMessageType.Error;
        }

        public string MsgDateTime
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public string MsgCaption
        {
            get { return _msgCaption; }
            set { _msgCaption = value; }
        }

        public string MsgCode
        {
            get { return _msgCode; }
            set { _msgCode = value; }
        }

        public string MsgData
        {
            get { return _msgData; }
            set { _msgData = value; }
        }

        public eOPIMessageType MsgType
        {
            get { return _msgType; }
            set { _msgType = value; }
        }
    }

    public class SocketInfo
    {
        private DateTime _timeStamp;
        private string _msgName;
        private string _trxId; 
        private string _xml;
        private string _sessionId;

        public DateTime SocketDateTime
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public string MsgName
        {
            get { return _msgName; }
            set { _msgName = value; }
        }

        public string TrxId
        {
            get { return _trxId; }
            set { _trxId = value; }
        }

        public string SocketXml
        {
            get { return _xml; }
            set { _xml = value; }
        }

        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }
    }
}
