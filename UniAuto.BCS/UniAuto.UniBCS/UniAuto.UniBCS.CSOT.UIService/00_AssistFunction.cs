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
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;
using System.Diagnostics;


namespace UniAuto.UniBCS.CSOT.UIService
{
    public partial class UIService : AbstractService
    {
        public FileFormatManager FileFormatManager { get; set; }

        private bool _run = false;
        //private Timer _linkTestTimer = null;
        private ManualResetEvent _linkTestMRE = new ManualResetEvent(true);
        private int _maxCount = 6;
        private int _eachCount = 4;


        //用來記錄OPI Client資訊
        private class ClientRecord
        {
            public string userID = string.Empty;
            public string userGroup = string.Empty;
            public string loginTime = string.Empty;
            public string loginServerIP = string.Empty;
            public string loginServerName = string.Empty;

            public ClientRecord(string id, string group, string ip, string name, string time)
            {
                userID = id;
                userGroup = group;
                loginServerIP = ip;
                loginServerName = name;
                loginTime = time;
            }
        }
        private Dictionary<string, ClientRecord> dicClient = new Dictionary<string, ClientRecord>();
        //用來記錄DenseBox資料，避免拆解兩次 (key:EqpNo_PortNo)
        private Dictionary<string, List<string>> dicDenseBoxData = new Dictionary<string, List<string>>();
        //記錄OperationPermission命令
        //Key: EqpNo_MessageName, Value: ReplyIP, OperationPermission
        private Dictionary<string, Tuple<string, eCELLATSOperPermission>> dicOperationPermission = new Dictionary<string, Tuple<string, eCELLATSOperPermission>>();
        //記錄OperationRunMode命令
        //Key: EqpNo_MessageName, Value: ReplyIP
        private Dictionary<string, string> dicOpertationRunMode = new Dictionary<string, string>();

        public int MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; }
        }

        public int EachCount
        {
            get { return _eachCount; }
            set { _eachCount = value; }
        }

        private class ForceCleanOutCmd
        {
            public string COMMAND = string.Empty;
            public string STATUS = string.Empty;

            public ForceCleanOutCmd(string cmd, string status)
            {
                COMMAND = cmd;
                STATUS = status;
            }
        }
        private Dictionary<string, ForceCleanOutCmd> dicForceCleanOut = new Dictionary<string, ForceCleanOutCmd>();

        /// <summary>
        /// Service 初始化方法
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            bool ret = false;
            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                //_opiAgent = GetServerAgent();
                _run = true;
                //_linkTestTimer = new Timer(LinkTestTimerFunc);
                //_linkTestTimer.Change(1000, Timeout.Infinite);
                ret = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                Destory();
                ret = false;
            }
            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
            return ret;
        }

        public void Destory()
        {
            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                if (_run)
                {
                    _run = false;
                    //_linkTestTimer.Dispose();
                    _linkTestMRE.WaitOne();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            NLogManager.Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
        }

        private IServerAgent GetServerAgent()
        {
            //小心Spring的_applicationContext.GetObject(name)
            //在Init-Method與Destory-Method會與Spring的GetObject使用相同LOCK
            //需留心以免死結
            return GetServerAgent("OPIAgent");
        }

        //private IServerAgent _opiAgent = null;

        #region 定時連線測試

        //private void LinkTestTimerFunc(object obj)
        //{
        //    if (_run)
        //    {
        //        _linkTestMRE.Reset();

        //        if (Workbench.State == eWorkbenchState.RUN)
        //        {
        //            MethodInfo mi = GetServerAgent().GetType().GetMethod("GetClients");
        //            object dic = mi.Invoke(GetServerAgent(), null);
        //            if (dic is Dictionary<string, object[]>)
        //            {
        //                Dictionary<string, object[]> client_infos = dic as Dictionary<string, object[]>;
        //                foreach (string session_id in client_infos.Keys)
        //                {
        //                    DateTime last_recv_send = (DateTime)(client_infos[session_id][0]);
        //                    TimeSpan ts = DateTime.Now - last_recv_send;
        //                    if (ts.TotalSeconds > 600)
        //                    {
        //                        //OPI和BC之間若60秒沒有收送動作，BC就向OPI發出LinkTest訊息
        //                        List<string> _session_id = new List<string>();
        //                        _session_id.Add(session_id);
        //                        BCSLinkTest("", Workbench.ServerName, _session_id);

        //                        //BCSTerminalMessageInform(GetTrxID(""), "FCMPH100", "TEST123");

        //                    }
        //                }
        //            }
        //        }
        //        if (_run)
        //            _linkTestTimer.Change(5000, Timeout.Infinite);
        //        _linkTestMRE.Set();
        //    }
        //}

        #endregion

        #region Private Method

        /// <summary>
        /// Get TrxID
        /// </summary>
        /// <returns>TrxID</returns>
        private string GetTrxID(string trxID)
        {
            string ret = string.Empty;

            //BCS主動report的TrxID需在另外加上一碼以做區別(yyyyMMddHHmmssfffx)
            if (string.IsNullOrEmpty(trxID))
                ret = Spec.GetTransactionID() + "0";
            else
                ret = trxID + "0";

            return ret;
        }

        /// <summary>
        /// 轉換ProcessType為對應的代碼
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="product"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetArrayProcessType(LOTc lot, PRODUCTc product, Line line)
        {
            string productProcessType = lot.PRODUCTPROCESSTYPE.Trim();
            string processType = lot.PROCESSTYPE.Trim();
            switch (line.Data.LINETYPE)
            {
                case eLineType.ARRAY.CVD_AKT:
                case eLineType.ARRAY.CVD_ULVAC:
                    if (productProcessType.ToUpper().Trim() == eMES_PRODUCT_PROCESS_TYPE.IGZO)
                        return "4"; // IGZO
                    else
                    {
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SAMPLING_MODE)
                        {
                            if (product.TEMPERATUREFLAG.ToUpper().Trim() == eMES_TEMPERATURE_FLAG.HT)
                                return "2"; // HT
                            else if (product.TEMPERATUREFLAG.ToUpper().Trim() == eMES_TEMPERATURE_FLAG.LT)
                                return "3"; // LT
                            else
                                return "1";
                        }
                        else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        {
                            switch (product.OWNERTYPE.ToUpper().Trim())
                            {
                                case eMES_OWNER_TYPE.DUMMY: return "5"; // MQC
                                default:
                                    {
                                        if (product.TEMPERATUREFLAG.ToUpper().Trim() == eMES_TEMPERATURE_FLAG.HT)
                                            return "2"; // HT
                                        else if (product.TEMPERATUREFLAG.ToUpper().Trim() == eMES_TEMPERATURE_FLAG.LT)
                                            return "3"; // LT        
                                        else
                                            return "1";
                                    }
                            }
                        }
                        else
                            return "1";
                    }
                case eLineType.ARRAY.DRY_YAC:
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_TEL:
                    if (productProcessType.ToUpper().Trim() == eMES_PRODUCT_PROCESS_TYPE.IGZO)
                        return "2"; // IGZO
                    else
                    {
                        switch (product.OWNERTYPE.ToUpper().Trim())
                        {
                            case eMES_OWNER_TYPE.DUMMY: return "3"; // MQC
                            case eMES_OWNER_TYPE.ENGINEER: return "4"; // ENG
                            default: return "1";
                        }
                    }
                case eLineType.ARRAY.PHL_TITLE:
                case eLineType.ARRAY.PHL_EDGEEXP:
                    if (processType.Equals(eMES_PRODUCT_PROCESS_TYPE.MMG))
                        return "2";
                    else
                        return "1";
                /*case eLineType.ARRAY.ILC:
                    switch (product.CHAMBERRUNMODE.Trim())
                    {
                        case eRUNMODE.ILC.ILC_1: return "1";
                        case eRUNMODE.ILC.FLC_2: return "2";
                        default: return "0";
                    }*/
                default:
                    return "0";
            }
        }

        /// <summary>
        /// 轉換CuttingFlag為對應的代碼
        /// </summary>
        /// <param name="cuttingFlag"></param>
        /// <returns></returns>
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

        //轉換JobJudge描述
        //Modify By Frank 20150728 for T3//sy modify 加入fabtype區分
        private string ConvertJobJudge(string jobJudge, Line line)
        {
            string judge = string.Empty;
            if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
            {
                #region [ARRAY]
                switch (jobJudge)
                {
                    case "0":
                        judge = "0: Inspection Skip or No Judge";
                        break;
                    case "1":
                        judge = "1: OK";
                        break;
                    case "2":
                        judge = "2: NG - Insp. Result";
                        break;
                    case "3":
                        judge = "3: RW - Required Rework";
                        break;
                    case "4":
                        judge = "4: PD –Pending judge";
                        break;
                    case "5":
                        judge = "5: RP - Required Repair";
                        break;
                    case "6":
                        judge = "6: IR–Ink Repair";
                        break;
                    case "7":
                        judge = "7: SC –Scrap";
                        break;
                    case "8":
                        judge = "8: Other";
                        break;
                    case "9":
                        judge = "RV –PI Reivew";
                        break;
                    case "10":
                        judge = "RL";
                        break;
                    case "11":
                        judge = "LP";


                        break;
                    default:
                        judge = jobJudge;
                        break;
                }
                #endregion
            }
            else if (line.Data.FABTYPE == eFabType.CF.ToString())
            {
                #region [CF]
                switch (jobJudge)
                {
                    case "0":
                        judge = "0: Inspection Skip or No Judge";
                        break;
                    case "1":
                        judge = "1: OK";
                        break;
                    case "2":
                        judge = "2: NG - Insp. Result";
                        break;
                    case "3":
                        judge = "3: RW - Required Rework";
                        break;
                    case "4":
                        judge = "4: PD –Pending judge";
                        break;
                    case "5":
                        judge = "5: RP - Required Repair";
                        break;
                    case "6":
                        judge = "6: IR–Ink Repair";
                        break;
                    case "7":
                        judge = "7: Other";
                        break;
                    case "8":
                        judge = "8: RV –PI Reivew";
                        break;
                    default:
                        judge = jobJudge;
                        break;
                }
                #endregion
            }
            else
            {
                #region [CELL]
                switch (jobJudge)
                {
                    case "0":
                        judge = "0: Inspection Skip or No Judge";
                        break;
                    case "1":
                        judge = "1: OK";
                        break;
                    case "2":
                        judge = "2: NG - Insp. Result";
                        break;
                    case "3":
                        judge = "3: RW - Required Rework";
                        break;
                    case "4":
                        judge = "4: PD –Pending judge";
                        break;
                    case "5":
                        judge = "5: RP - Required Repair";
                        break;
                    case "6":
                        judge = "6: IR–Ink Repair";
                        break;
                    case "7":
                        judge = "7: Other";
                        break;
                    case "8":
                        judge = "8: RV –PI Reivew";
                        break;
                    case "9":
                        judge = "9: RJ –Re-judge";
                        break;
                    default:
                        judge = jobJudge;
                        break;
                }
                #endregion
            }
            return judge;
        }

        //轉換SamplingSlotFlag描述
        private string ConvertSamplingSlotFlag(string samplingSlotFlag)
        {
            string flag = string.Empty;

            switch (samplingSlotFlag)
            {
                case "0":
                    flag = "0: Not Sampling Slot";
                    break;
                case "1":
                    flag = "1: Sampling Slot";
                    break;
                default:
                    flag = samplingSlotFlag;
                    break;
            }

            return flag;
        }

        //轉換OXInformationRequestFlag描述
        private string ConvertOXInformationRequestFlag(string oxInfo)
        {
            string info = string.Empty;
            switch (oxInfo)
            {
                case "0":
                    info = "0: Disable";
                    break;
                case "1":
                    info = "1: Enable";
                    break;
                default:
                    info = oxInfo;
                    break;
            }

            return info;
        }

        //轉換FirstRunFlag描述
        private string ConvertFirstRun(string firstRun)
        {
            string run = string.Empty;
            switch (firstRun)
            {
                case "0":
                    run = "0: Not First Run";
                    break;
                case "1":
                    run = "1: First Run";
                    break;
                default:
                    run = firstRun;
                    break;
            }
            return run;
        }

        //轉換ProcessType描述
        private string ConvertProcessType(string processType)
        {
            string type = string.Empty;
            switch (processType)
            {
                case "1":
                    type = "1: NORMAL";
                    break;
                case "2":
                    type = "2: NORMAL-HT";
                    break;
                case "3":
                    type = "3: NORMAL-LT";
                    break;
                case "4":
                    type = "4: IGZO";
                    break;
                case "5":
                    type = "5: MQC";
                    break;
                default:
                    type = processType;
                    break;
            }
            return type;
        }

        /// <summary>
        /// 重载ConvertProcessType 
        /// </summary>
        /// <param name="lineType"></param>
        /// <param name="processType"></param>
        /// <returns></returns>
        private string ConvertProcessType(string lineType, string processType)
        {
            string type = string.Empty;
            
            switch (lineType)
            {
                case eLineType.ARRAY.CVD_ULVAC:
                case eLineType.ARRAY.CVD_AKT:
                    type = ConstantManager["PROCESSTYPE_CVD"][processType].Value;
                    break;
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_YAC:
                case eLineType.ARRAY.DRY_TEL:
                    type = ConstantManager["PROCESSTYPE_DRY"][processType].Value;
                    break;
                case eLineType.ARRAY.MSP_ULVAC:
                case eLineType.ARRAY.ITO_ULVAC:
                    type = ConstantManager["PROCESSTYPE_PVD"][processType].Value;
                    break;
                default:
                    if (processType.Equals("0"))
                        type = "0:PRODUCT";
                    else if (processType.Equals("1"))
                        type = "1:MQC";
                    else
                        type = "NotDefine";
                    break;
            }
            return type;

        }
        //轉換LastGlassFlag描述
        private string ConvertLastGlassFlag(string lastGlassFlag)
        {
            string last = string.Empty;
            switch (lastGlassFlag)
            {
                case "1":
                    last = "1: Last Glass";
                    break;
                case "0":
                    last = "0: Not Last Glass";
                    break;
                default:
                    last = lastGlassFlag;
                    break;
            }
            return last;
        }

        //轉換VCRMismatchFlag描述
        private string ConvertVCRMismatchFlag(string vcrMismatchFlag)
        {
            string vcr = string.Empty;
            switch (vcrMismatchFlag)
            {
                case "1":
                    vcr = "1: VCR Mismatch";
                    break;
                case "0":
                    vcr = "0: VCR Match";
                    break;
                default:
                    vcr = vcrMismatchFlag;
                    break;
            }
            return vcr;
        }

        //轉換RTCFlag描述
        private string ConvertRTC(string rtcFlag)
        {
            string rtc = string.Empty;
            switch (rtcFlag)
            {
                case "1":
                    rtc = "1: Temporay return to Cassette";
                    break;
                case "0":
                    rtc = "0: Normal Process";
                    break;
                default:
                    rtc = rtcFlag;
                    break;
            }
            return rtc;
        }

        //转换EQPRTCFlag描述 --yang
        private string ConvertEQPRTC(string eqprtcFlag)
        {
            string eqprtc = string.Empty;
            switch (eqprtcFlag)
            {
                case "True":
                    eqprtc = "1:DownStream Refuse return to Cassette";
                    break;
                case "False":
                    eqprtc = "0:Normal Flow";
                    break;               
            }
            return eqprtc;
        }
        //轉換LoaderBufferingFlag描述
        private string ConvertLoaderBufferingFlag(string loaderBufferingFlag)
        {
            string flag = string.Empty;
            switch (loaderBufferingFlag)
            {
                case "1":
                    flag = "1: Temporary store in Loader";
                    break;
                case "0":
                    flag = "0: Normal Process";
                    break;
                default:
                    flag = loaderBufferingFlag;
                    break;
            }
            return flag;
        }

        //轉換MainEQInFlag描述
        private string ConvertMainEQInFlag(string mainEQinFlag)
        {
            string flag = string.Empty;
            switch (mainEQinFlag)
            {
                case "1":
                    flag = "1: Tracking Data Error from Main EQ";
                    break;
                case "0":
                    flag = "0: No Error";
                    break;
                default:
                    flag = mainEQinFlag;
                    break;
            }
            return flag;
        }

        //轉換TurnAngle / ReturnModeTurnAngle描述
        private string ConvertTurnAngle(string turnAngle)
        {
            string angle = string.Empty;
            switch (turnAngle)
            {
                case "1":
                    angle = "1: 0°";
                    break;
                case "2":
                    angle = "2: 90°";
                    break;
                case "3":
                    angle = "3: -90°";
                    break;
                case "4":
                    angle = "4: 180°";
                    break;
                default:
                    angle = turnAngle;
                    break;
            }
            return angle;
        }

        //轉換ProductOwner / ReturnModeProductOwner描述
        private string ConvertProductOwner(string turnProductOwner)
        {
            string productOwner = string.Empty;
            switch (turnProductOwner)
            {
                case "1":
                    productOwner = "1: P";
                    break;
                case "2":
                    productOwner = "2: E";
                    break;
                case "3":
                    productOwner = "3: M";
                    break;
                default:
                    productOwner = turnProductOwner;
                    break;
            }
            return productOwner;
        }

        //轉換CELL PMT Line RunMode描述
        private string ConvertPMTRunMode(string runMode)
        {
            string mode = string.Empty;
            switch (runMode)
            {
                case "1":
                    mode = "1: Sampling Mode_PMI";
                    break;
                case "2":
                    mode = "2: Sort Mode_PMI";
                    break;
                case "3":
                    mode = "3: Cassette Changer Mode_PMI";
                    break;
                case "4":
                    mode = "4: Sort Mode_PTI";
                    break;
                case "5":
                    mode = "5: Cassette Changer Mode_PTI";
                    break;
                case "6":
                    mode = "6: Thickness Inspection_PTI";
                    break;
                case "7":
                    mode = "7: Photo Inspection_PTI";
                    break;
                default:
                    mode = runMode;
                    break;
            }
            return mode;
        }

        //轉換CELL PRM Line RunMode描述
        private string ConvertPRMRunMode(string runMode)
        {
            string mode = string.Empty;
            switch (runMode)
            {
                case "1":
                    mode = "1: Remove Mode(TFT+CF)";
                    break;
                case "2":
                    mode = "2: Remove Mode(TFT)";
                    break;
                case "3":
                    mode = "3: Remove Mode(CF)";
                    break;
                case "4":
                    mode = "4: Remove Mode(Virtual Loader Port TFT+CF)";
                    break;
                case "5":
                    mode = "5: Remove Mode(Virtual Loader Port TFT)";
                    break;
                case "6":
                    mode = "6: Remove Mode(Virtual Loader Port CF";
                    break;
                case "7":
                    mode = "7: Remove Mode(Virtual Loader+Unloader Port TFT+CF)";
                    break;
                case "8":
                    mode = "8: Remove Mode(Virtual Loader+Unloader Port TFT)";
                    break;
                case "9":
                    mode = "9: Remove Mode(Virtual Loader+Unloader Port CF)";
                    break;
                case "10":
                    mode = "10: Inspection Mode(TFT+CF)";
                    break;
                case "11":
                    mode = "11: Inspection Mode(TFT)";
                    break;
                case "12":
                    mode = "12: Inspection Mode(CF)";
                    break;
                case "13":
                    mode = "13: Inspection Mode(Virtaul Port TFT+CF)";
                    break;
                case "14":
                    mode = "14: Inspection Mode(Virtaul Port TFT)";
                    break;
                case "15":
                    mode = "15: Inspection Mode(Virtaul Port CF)";
                    break;
                default:
                    mode = runMode;
                    break;
            }
            return mode;
        }

        //轉換CELL LOI Line RunMode描述
        private string ConvertLOIRunMode(string runMode)
        {
            string mode = string.Empty;
            switch (runMode)
            {
                case "1":
                    mode = "1: 3600_LOI1";
                    break;
                case "2":
                    mode = "2: 3650_RLOI1";
                    break;
                case "3":
                    mode = "3: 3660_RG";
                    break;
                case "4":
                    mode = "4: 4600_MAIN1";
                    break;
                case "5":
                    mode = "5: 4650_RLOI2";
                    break;
                case "6":
                    mode = "6: 4660_LOI2F";
                    break;
                case "7":
                    mode = "7: 4670_OQCSMP";
                    break;
                case "8":
                    mode = "8: 4680_MAIN2";
                    break;
                case "71":
                    mode = "71: 3699_BSOR";
                    break;
                case "72":
                    mode = "72: 3698_BSOR2";
                    break;
                case "73":
                    mode = "73: 4699_CSOR";
                    break;
                case "74":
                    mode = "74: 4699_CSOR2";
                    break;
                case "1071":
                    mode = "1071: 696_MERGE01";
                    break;
                case "1072":
                    mode = "1072: 4697_MERGE02";
                    break;
                default:
                    mode = runMode;
                    break;
            }
            return mode;
        }

        //轉換CELL NRP Line RunMode描述
        private string ConvertNRPRunMode(string runMode)
        {
            string mode = string.Empty;
            switch (runMode)
            {
                case "0":
                    mode = "0: Inspection Mode";
                    break;
                case "1":
                    mode = "1: Repair Mode";
                    break;
                default:
                    mode = runMode;
                    break;
            }
            return mode;
        }

        //轉換CELL SOR Line RunMode描述
        private string ConvertSORRunMode(string runMode)
        {
            string mode = string.Empty;
            switch (runMode)
            {
                case "1":
                    mode = "1: Sorting Mode[SOR#1,#2,#3]";
                    break;
                case "2":
                    mode = "2: Changer Mode[SOR#1,#2,#3]";
                    break;
                case "3":
                    mode = "3: Virtual Port Loader Mode[SOR#1,#2]";
                    break;
                case "4":
                    mode = "4: Virtual Port Unload Mode[SOR#1,#2]";
                    break;
                case "5":
                    mode = "5: Pack Mode[SOR#3]";
                    break;
                case "6":
                    mode = "6: Unpack Mode[SOR#3]";
                    break;
                default:
                    mode = runMode;
                    break;
            }
            return mode;
        }

        //轉換RepairResult描述
        private string ConvertRepairResult(string repairResult)
        {
            string result = string.Empty;
            switch (repairResult)
            {
                case "0":
                    result = "0: The glass will not be judged by NRP but it will be judged by LOI-2.";
                    break;
                case "1":
                    result = "1: The highest L level.";
                    break;
                case "2":
                    result = "2: The glass will be scrapped compulsorily by LOI-2.";
                    break;
                case "3":
                    result = "3: The glass will be converted to RP/RT compulsorily by LOI-2 and will be sent to 3660/4660 after Laser Cut.";
                    break;
                case "4":
                    result = "4: The glass will be convert to K/C level compulsorily by LOI-2.";
                    break;
                default:
                    result = repairResult;
                    break;
            }
            return result;
        }

        #endregion


        /// <summary>
        /// 當Agent發送Message後, Agent會呼叫MessageSend以通知Service, 由Service決定是否加入T3
        /// </summary>
        /// <param name="sendDt"></param>
        /// <param name="xml"></param>
        /// <param name="t3TimeoutSecond"></param>
        public void OPI_MessageSend(DateTime sendDt, string sessionId, string xml, int t3TimeoutSecond)
        {
            try
            {
                Message msg = Spec.XMLtoMessage(xml);
                if (msg.WaitReply != string.Empty && t3TimeoutSecond > 0)
                {
                    string key = string.Format("{0}_{1}", sessionId, msg.HEADER.TRANSACTIONID);
                    //NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Add to WaitForT3", msg.HEADER.MESSAGENAME, key));
                    //T3Manager.AddT3(new T3Manager.T3Message(GetType().Name, sendDt, key, msg, t3TimeoutSecond));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 當Agent收到Message後, Agent會先呼叫MessageRecv以通知Service, 由Service決定是否移除T3
        /// Agent呼叫MessageRecv之後才會根據不同Message, Invoke不同Service Method
        /// </summary>
        /// <param name="recvDt"></param>
        /// <param name="sessionId"></param>
        /// <param name="xml"></param>
        public void OPI_MessageRecv(DateTime recvDt, string sessionId, string xml)
        {
            try
            {
                Message reply_msg = Spec.XMLtoMessage(xml);
                string command_msg_name = Spec.GetMessageByReply(reply_msg.HEADER.MESSAGENAME);
                if (command_msg_name != string.Empty)
                {
                    string key = string.Format("{0}_{1}", sessionId, reply_msg.HEADER.TRANSACTIONID);
                    //T3Manager.T3Message command_t3 = T3Manager.TryRemoveT3(GetType().Name, key);
                    //if (command_t3 != null)
                    //{
                    //    Message command_msg = command_t3.Obj as Message;
                    //    NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}({1}) Remove from WaitForT3", command_msg.HEADER.MESSAGENAME, key));
                    //}
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  OPI MessageSet : OPI send to BC Are you There
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void OPI_AreYouThereRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            AreYouThereRequest command = Spec.XMLtoMessage(xmlDoc) as AreYouThereRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("AreYouThereReply") as XmlDocument;
            AreYouThereReply reply = Spec.XMLtoMessage(xml_doc) as AreYouThereReply;

            try
            {
                //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                IList<Line> lines = ObjectManager.LineManager.GetLines();
                bool bCheck = true;
                foreach (Line line in lines)
                {
                    if (command.BODY.LINENAME != line.Data.SERVERNAME)
                        bCheck = false;
                }

                int user_count = 0;
                lock (dicClient)
                {
                    RemoveDisconnectClientRecord();//2016/08/01
                    foreach (ClientRecord cr in dicClient.Values)
                    {
                        if (cr.userID == command.BODY.USERID)
                        {
                            user_count++;
                        }
                    }
                }

                if (ParameterManager.Parameters.ContainsKey("OPIMAXCOUNT"))
                    MaxCount = Convert.ToInt32(ParameterManager.Parameters["OPIMAXCOUNT"].Value.ToString());

                if (ParameterManager.Parameters.ContainsKey("OPIEACHCOUNT"))
                    EachCount = Convert.ToInt32(ParameterManager.Parameters["OPIEACHCOUNT"].Value.ToString());

                if (lines == null)
                {
                    reply.RETURN.RETURNCODE = "0010001";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI({2}) connect to UniBCS NG, because can't find this Line.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.LOGINSERVERIP));
                }
                else if (!bCheck)
                {
                    reply.RETURN.RETURNCODE = "0010000";
                    reply.RETURN.RETURNMESSAGE = string.Format("OPI and BCS line name are not match. OPI[{0}]/BCS[{1}].)", command.BODY.LINENAME, Workbench.ServerName);

                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI({2}) connect to UniBCS NG, because OPI and BCS line name are not match. OPI({3})/BCS({4}).",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.LOGINSERVERIP, command.BODY.LINENAME, Workbench.ServerName));
                }
                else if (command.BODY.USERGROUP.ToUpper() != "CIMADMIN" && (dicClient.Count >= MaxCount || user_count >= EachCount))
                {
                    reply.RETURN.RETURNCODE = "0010002";
                    reply.RETURN.RETURNMESSAGE = string.Format("OPI user count is full. (Max[{0}], EachUser[{1}]", MaxCount, EachCount);

                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI({2}) connect to UniBCS NG, because OPI user count is full. (Max[{3}], EachUser[{4}])",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.LOGINSERVERIP, MaxCount, EachCount));
                }
                else
                {
                    //加入Client記錄
                    lock (dicClient)
                    {
                        if (!dicClient.ContainsKey(command.HEADER.REPLYSUBJECTNAME))
                        {
                            ClientRecord client = new ClientRecord(command.BODY.USERID, command.BODY.USERGROUP, command.BODY.LOGINSERVERIP, command.BODY.LOGINSERVERNAME, command.BODY.LOGINTIME);
                            dicClient.Add(command.HEADER.REPLYSUBJECTNAME, client);
                        }
                    }
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI({2}) connect to UniBCS by User({3}).",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.LOGINSERVERNAME, command.BODY.USERID));
                    //OPI 连线状态发生变化通知Evisor
                    Invoke("EvisorService", "OPI_Connect_Count", new object[] { ServerName, dicClient.Count.ToString(), MaxCount.ToString() });
                }

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.FACTORYTYPE = command.BODY.FACTORYTYPE;

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
            }
        }

        /// <summary>
        /// OPI MessageSet: OPI Connect
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_CONNECT(string clinetSessionID)
        {
            //TODO: 上报OPI Agent 连线状态

        }

        /// <summary>
        /// OPI MessageSet: OPI Disconnect
        /// </summary>
        /// <param name="clientSessionID"></param>
        public void OPI_DISCONNECT(string clientSessionID)
        {
            lock (dicClient)
            {
                //Client斷線，刪除該Client記錄
                if (dicClient.ContainsKey(clientSessionID))
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] OPI({2}) disconnect by User({3})",
                        Workbench.ServerName, DateTime.Now.ToString("yyyyMMddHHmmssfff"), dicClient[clientSessionID].loginServerName, dicClient[clientSessionID].userID));
                }
                RemoveDisconnectClientRecord();//2016/08/01

                //Invoke("EvisorService", "OPI_Connect_Count", new object[] { ServerName, dicClient.Count.ToString(), MaxCount.ToString() });
                //换成t3 的BMS 20160201 tom .bian
                Invoke("EvisorService", "OPIConnectCountReport", new object[] { ServerName, dicClient.Count.ToString(), MaxCount.ToString() });
            }
        }

        /// <summary>
        /// OPI MessageSet: OPI Link Test to BC
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_OPILinkTest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            OPILinkTest command = Spec.XMLtoMessage(xmlDoc) as OPILinkTest;
            XmlDocument xml_doc = agent.GetTransactionFormat("OPILinkTestReply") as XmlDocument;
            OPILinkTestReply reply = Spec.XMLtoMessage(xml_doc) as OPILinkTestReply;

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME{0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
            }
        }

        ///// <summary>
        ///// OPI MessageSet: BCS Link Test to OPI
        ///// </summary>
        ///// <param name="trxID"></param>
        ///// <param name="lineName"></param>
        //public void BCSLinkTest(string trxID, string lineName, List<string> session_id)
        //{
        //    try
        //    {
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("BCSLinkTest") as XmlDocument;
        //        BCSLinkTest trx = Spec.XMLtoMessage(xml_doc) as BCSLinkTest;

        //        //OPI LineName為ServerName
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        trx.BODY.LINENAME = line == null ? lineName : line.Data.SERVERNAME;
        //        //trx.BODY.LINENAME = lineName;

        //        xMessage msg = SendToOPI(trxID, trx, session_id);

        //        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //               string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
        //               trx.BODY.LINENAME, msg.TransactionID));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        ///// <summary>
        ///// OPI MessageSet: BCS Link Test Reply to BC
        ///// </summary>
        ///// <param name="xmlDoc"></param>
        //public void OPI_BCSLinkTestReply(XmlDocument xmlDoc)
        //{
        //    try
        //    {
        //        //IServerAgent agent = GetServerAgent();
        //        //{
        //        //    Message bcsLinkTest = Spec.CheckXMLFormat(xmlDoc);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        private void SendToMES(XmlDocument xml)
        {
            xMessage msg = new xMessage();
            msg.Name = xml[keyHost.MESSAGE][keyHost.HEADER][keyHost.MESSAGENAME].InnerText;
            msg.TransactionID = xml["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
            msg.ToAgent = eAgentName.MESAgent;
            msg.Data = xml.OuterXml;
            PutMessage(msg);
        }

        private xMessage SendToOPI(string trxID, UniAuto.UniBCS.OpiSpec.Message trx, List<string> session_id)
        {
            xMessage msg = new xMessage();
            trx.HEADER.TRANSACTIONID = GetTrxID(trxID);
            trx.HEADER.REPLYSUBJECTNAME = (string)Invoke("OPIAgent", "GetSocketSessionID", new object[] { });
            string xml = trx.WriteToXml();
            msg.Name = trx.HEADER.MESSAGENAME;
            msg.FromAgent = "OPIAgent";
            msg.ToAgent = "OPIAgent";
            msg.Data = xml;
            msg.TransactionID = trx.HEADER.TRANSACTIONID;
            foreach (string _ip in session_id)
            {
                msg.UseField.Add(_ip, null);
            }
            PutMessage(msg);
            return msg;
        }

        private xMessage SendReportToAllOPI(string trxID, UniAuto.UniBCS.OpiSpec.Message report)
        {
            xMessage msg = new xMessage();
            report.HEADER.TRANSACTIONID = GetTrxID(trxID);
            report.HEADER.REPLYSUBJECTNAME = (string)Invoke("OPIAgent", "GetSocketSessionID", new object[] { });
            string xml = report.WriteToXml();
            msg.Name = report.HEADER.MESSAGENAME;
            msg.FromAgent = "OPIAgent";
            msg.ToAgent = "OPIAgent";
            msg.Data = xml;
            msg.TransactionID = report.HEADER.TRANSACTIONID;
            PutMessage(msg);
            return msg;
        }

        private xMessage SendReplyToOPI(UniAuto.UniBCS.OpiSpec.Message command, UniAuto.UniBCS.OpiSpec.Message reply)
        {
            xMessage msg = new xMessage();
            reply.HEADER.TRANSACTIONID = command.HEADER.TRANSACTIONID;
            reply.HEADER.REPLYSUBJECTNAME = (string)Invoke("OPIAgent", "GetSocketSessionID", new object[] { });
            string xml = reply.WriteToXml();
            msg.Name = reply.HEADER.MESSAGENAME;
            msg.FromAgent = "OPIAgent";
            msg.ToAgent = "OPIAgent";
            msg.Data = xml;
            msg.TransactionID = reply.HEADER.TRANSACTIONID;
            msg.UseField.Add(command.HEADER.REPLYSUBJECTNAME, null);
            PutMessage(msg);
            return msg;
        }

        #region TimeCheck
        Dictionary<string, DateTime> dicExecuteTime = new Dictionary<string, DateTime>();//記錄每個key的執行時間
        object lockTimeCheck = new object();//lock用
        int durationTime = 4;//持續時間

        /// <summary>
        /// 檢查OPI是否在短時間內重覆按下button執行,有下command的才使用
        /// </summary>
        /// <param name="keyName">唯一的key值,ex:eqp+method or eqp+port+method</param>
        /// <returns>true:表示可以往下執行,false:表示在4秒(durationTime設定)內已經有執行了,要回error</returns>
        public bool TimeCheck(string keyName)
        {
            lock (lockTimeCheck)
            {
                if (!dicExecuteTime.ContainsKey(keyName))
                {
                    dicExecuteTime.Add(keyName, DateTime.Now);
                    return true;
                }
                else
                {
                    if ((DateTime.Now - dicExecuteTime[keyName]).TotalSeconds < durationTime)
                    {
                        return false;
                    }
                    else
                    {
                        dicExecuteTime[keyName] = DateTime.Now;
                        return true;
                    }
                }
            }
        }
        #endregion

        // ========= 給BCS Information 程式來收集各BC的版本資訊 , 非OPI在使用=======
        private System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

        /// <summary>
        /// OPI MessageSet: BCS Info Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_BCSInfoRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            BCSInfoRequest command = Spec.XMLtoMessage(xmlDoc) as BCSInfoRequest;
            BCSInfoReply reply = new BCSInfoReply();

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.APPSTARTUPPATH = System.Windows.Forms.Application.StartupPath;
                reply.BODY.APPVERSION = System.Windows.Forms.Application.ProductVersion;
                //reply.BODY.NETMACADDR
                //reply.BODY.NETWORKIPADDR
                reply.BODY.PACKAGEVERSION = Workbench.Version;
                reply.BODY.APPSTARTDATETIMEdt = Workbench.Instance.MainForm.StartTime;
                currentProcess.Refresh();//務必先Refresh才取值, 否則值不會更新
                reply.BODY.PROCESSID = currentProcess.Id.ToString();
                reply.BODY.WORKINGSET = string.Format("{0} K", (currentProcess.WorkingSet64 / 1024).ToString("0,0"));
                reply.BODY.PEAKWORKINGSET = string.Format("{0} K", (currentProcess.PeakWorkingSet64 / 1024).ToString("0,0"));
                reply.BODY.PRIVATEMEMORY = string.Format("{0} K", (currentProcess.PrivateMemorySize64 / 1024).ToString("0,0"));
                reply.BODY.THREADCOUNT = currentProcess.Threads.Count.ToString();

                foreach (IServerAgent ag in Workbench.Instance.AgentList.Values)
                {
                    BCSInfoReply.AGENTINFOc info = new BCSInfoReply.AGENTINFOc();
                    info.CFGDATA = ag.Configuration.ToFormatString();
                    info.CFGPATH = ag.ConfigFileName;
                    info.CONNECTSTATE = ag.ConnectedState;
                    //info.DLLVER
                    info.FMTPATH = ag.FormatFileName;
                    info.NAME = ag.Name;
                    info.STATUS = ag.AgentStatus.ToString();
                    foreach (string key in ag.RuntimeInfo.Keys)
                    {
                        BCSInfoReply.RUNTIMEITEMc runtime = new BCSInfoReply.RUNTIMEITEMc();
                        runtime.NAME = key;
                        runtime.VAL = ag.RuntimeInfo[key].ToString();
                        info.RUNTIMEINFOLIST.Add(runtime);
                    }
                    reply.BODY.AGENTINFOLIST.Add(info);
                }

                UniAuto.UniBCS.MISC.ParameterManager parameter_manager = (UniAuto.UniBCS.MISC.ParameterManager)GetObject("ParameterManager");
                foreach (string parameter_key in parameter_manager.Keys)
                {
                    BCSInfoReply.PARAMETERc p = new BCSInfoReply.PARAMETERc();
                    p.NAME = parameter_key;
                    p.VALUE = parameter_manager.Parameters[parameter_key].Value.ToString();
                    p.DESCRIPTION = parameter_manager.Parameters[parameter_key].Discription;
                    p.TYPE = parameter_manager.Parameters[parameter_key].DataType;
                    reply.BODY.PARAMETERSLIST.Add(p);
                }

                UniAuto.UniBCS.MISC.ConstantManager constant_manager = (UniAuto.UniBCS.MISC.ConstantManager)GetObject("ConstantManager");
                foreach (string constant_key in constant_manager.ConstantList.Keys)
                {
                    BCSInfoReply.CONSTANTc c = new BCSInfoReply.CONSTANTc();
                    c.NAME = constant_manager.ConstantList[constant_key].Name;
                    c.DEFAULT = constant_manager.ConstantList[constant_key].DefaultValue.Value;

                    foreach (string key in constant_manager.ConstantList[constant_key].Values.Keys)
                    {
                        BCSInfoReply.KEYc k = new BCSInfoReply.KEYc();
                        k.NAME = key;
                        k.VAL = constant_manager.ConstantList[constant_key].Values[key].Value;
                        c.KEYLIST.Add(k);
                    }
                    reply.BODY.CONSTANTSLIST.Add(c);
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                xMessage msg = SendReplyToOPI(command, reply);

                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }


        private void RemoveDisconnectClientRecord()
        {
            // 2016/08/01
            // 向 OPI Agent 取得連線 SessionID
            // 與 UI Service 保存的 dicClient 做比較
            // 將已經斷線的 SessionID 從 UI Service 的 dicClient 中移除
            lock (dicClient)
            {
                Dictionary<string, object[]> session_opi_agent = (Dictionary<string, object[]>)Invoke(eAgentName.OPIAgent, "GetClients", new object[] { });
                List<string> session_ui_service = dicClient.Keys.ToList();
                foreach (string session in session_ui_service)
                {
                    if (!session_opi_agent.ContainsKey(session))
                    {
                        dicClient.Remove(session);
                    }
                }
            }
        }
    }
}
