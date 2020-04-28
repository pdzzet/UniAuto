using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class DailyCheckService : AbstractService
    {
        /// <summary>
        /// MES Transaction
        /// FacilityCriteriaSend : MES 修改Daily Check Report Time
        /// FacilityParameterRequest & FacilityParameterReply : MES Request Daily Check & BC Reply
        /// FacilityCheckReport : BCS 定時Report Daily Check
        /// FacilityCheckRequest & FacilityCheckReply : 待確認是否需要
        /// </summary>

        private const string Key_DailyCheckProcessDataBlock = "{0}_DailyCheckProcessDataBlock";
        //private const string OVNKey_DailyCheckProcessDataBlock = "{0}_DailyCheckProcessDataBlock#1"; //add by qiumin 20180210 OVN100 plc map change
        private bool isRuning = false;

        private Thread checkThread;
        //private Thread reportThread;

        //private ConcurrentQueue<HandleData> reportList;

        public override bool Init()
        {
            //reportList = new ConcurrentQueue<HandleData>();
            checkThread = new Thread(new ThreadStart(DailyCheckCheck)) { IsBackground = true };
            checkThread.Start();
            //reportThread = new Thread(new ThreadStart(DailyCheckReport)) { IsBackground = true };
            //reportThread.Start();
            isRuning = true;
            return true;
        }

        /// <summary>
        /// Daily Check Cycle Check
        /// </summary>
        private void DailyCheckCheck()
        {
           
            ConcurrentQueue<HandleData> reportList;
            ConcurrentQueue<HandleSECSData> reportListSECS;
            IList<FacilityParameterReply.MACHINEc> facilityReplyList;
            IList<FacilityCheckReport.MACHINEc> facilityReportList;
            List<string> facilityList;
            while (true)
            {
                try
                {
                    reportList = new ConcurrentQueue<HandleData>();
                    reportListSECS = new ConcurrentQueue<HandleSECSData>();
                    facilityReplyList = new List<FacilityParameterReply.MACHINEc>();
                    facilityReportList = new List<FacilityCheckReport.MACHINEc>();
                    facilityList = new List<string>();

                    Thread.Sleep(1000);
                    if ((!this.isRuning)||(Workbench.State!=eWorkbenchState.RUN)) continue;
                    //Jun Modify 20150521 不需要判斷ConnectedState == eAGENT_STATE.DISCONNECTED
                    //if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED) continue;

                    List<Line> lines = new List<Line>();
                    lines = ObjectManager.LineManager.GetLines();

                    if (lines == null) return;

                    foreach (Line line in lines)
                    {
                        //Watson Add 20150314 For 俊成、CSOT 福杰、登京
                       if (line.File.HostMode == eHostMode.OFFLINE) continue;

                        if (line.File.DailyCheckIntervalS == 0) continue;
                        DateTime now = DateTime.Now;

                        if (now.Subtract(line.File.DailyCheckLastDT).TotalSeconds >= line.File.DailyCheckIntervalS)
                        {
                            List<Equipment> eqps = new List<Equipment>();
                            eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID.ToString());

                            #region Watson Add 20150314 For CELL PMT Type PTI Line Report L2(Loader) Daily check)
                            if (line.Data.FABTYPE == eFabType.CELL.ToString())
                            {
                                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                                {
                                    Equipment eqpLD = ObjectManager.EquipmentManager.GetEQP("L2");
                                    if (eqpLD != null)
                                        eqps.Add(eqpLD);
                                }
                            }
                            #endregion

                            if (eqps == null) return;

                            foreach (Equipment eqp in eqps)
                            {
                                //Jun Add 20150521 機台CIM OFF不需要上報Daily Check
                                if (eqp.File.CIMMode == eBitResult.OFF)
                                {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("EQUIPMENT_NO=[{0}] CIMMODE=[{1}] NO NEED REPORT DAILYCHECKDATA!", eqp.Data.NODENO, eqp.File.CIMMode.ToString()));
                                    continue;
                                }
                                //Jun Add 20150521 機台狀態不是IDEL or RUN就不上報Daily Check
                                if (eqp.File.Status != eEQPStatus.IDLE && eqp.File.Status != eEQPStatus.RUN)
                                {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("EQUIPMENT_NO=[{0}] STATUS=[{1}] NO NEED REPORT DAILYCHECKDATA!", eqp.Data.NODENO, eqp.File.Status.ToString()));
                                    continue;
                                }

                                switch (eqp.Data.REPORTMODE)
                                {
                                    case "PLC":
                                    case "PLC_HSMS":
                                        IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqp.Data.NODENO);
                                        if (dataFormats == null)  //Jun Modify 20141203 如果DailyCheckData Table沒有資料時 記Log
                                        {
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqp.Data.NODENO));
                                            continue;
                                        }
                                        //取得PLC资料
                                        //modify by edison20150119:follow新的PLC Agent SyncReadTrx方法，多加一个参数
                                        Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_DailyCheckProcessDataBlock, eqp.Data.NODENO), false }) as Trx;  //add by qiumin 20180210 OVN100 plc map change
                                        if (trx == null)
                                        {
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] TRACTION!", eqp.Data.NODENO));
                                            continue;
                                        }

                                        HandleData hd = new HandleData(eqp, dataFormats, trx);
                                        reportList.Enqueue(hd);

                                        break;

                                    case "HSMS_PLC": //20150408 cy:增加這個report mode
                                    case "HSMS_CSOT":
                                    case "HSMS_NIKON":
                                        string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                                        List<Tuple<string, List<Tuple<string, string, string>>>> dailycheckRepository = Repository.Get(key) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                        if (dailycheckRepository == null)
                                        {
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T GET EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqp.Data.NODENO));
                                            continue;
                                        }

                                        HandleSECSData hdSECS = new HandleSECSData(eqp, dailycheckRepository);
                                        reportListSECS.Enqueue(hdSECS);
                                        break;
                                }
                            }
                            DailyCheckReport(reportList, ref facilityReplyList, ref facilityReportList, ref facilityList);
                            DailyCheckReport(reportListSECS, ref facilityReplyList, ref facilityReportList, ref facilityList);

                            if (facilityReportList.Count > 0)
                                Invoke(eServiceName.MESService, "FacilityCheckReport", new object[] { this.CreateTrxID(), line.Data.LINEID, facilityReportList });

                            lock (line.File)
                            {
                                line.File.DailyCheckLastDT = DateTime.Now;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                }
            }
        }

        /// <summary>
        /// Dail Check Request From MES
        /// </summary>
        public void DailyCheckForceCheck(Line line, string lineName,string mesINBOXName)
        {
            try
            {
                ConcurrentQueue<HandleData> reportList = new ConcurrentQueue<HandleData>();
                ConcurrentQueue<HandleSECSData> reportListSECS = new ConcurrentQueue<HandleSECSData>();
                IList<FacilityParameterReply.MACHINEc> facilityReplyList = new List<FacilityParameterReply.MACHINEc>();
                IList<FacilityCheckReport.MACHINEc> facilityReportList = new List<FacilityCheckReport.MACHINEc>();
                List<string> facilityList = new List<string>();

                if ((!this.isRuning) || (Workbench.State != eWorkbenchState.RUN))
                //Jun Modify 20150521 不需要判斷ConnectedState == eAGENT_STATE.DISCONNECTED
                //if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED)
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PLC CONNECTED STATE IS DISCONNECTED");
                    return;
                }

                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                    lineName = "F" + lineName.Substring(1);

                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineName);

                #region Watson Add 20150314 For CELL PMT Type PTI Line Report L2(Loader) Daily check)
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                    {
                        Equipment eqpLD = ObjectManager.EquipmentManager.GetEQP("L2");
                        if (eqpLD != null)
                            eqps.Add(eqpLD);
                    }
                }
                #endregion

                if (eqps == null) return;

                foreach (Equipment eqp in eqps)
                {
                    //Jun Add 20150521 機台CIM OFF不需要上報Daily Check
                    if (eqp.File.CIMMode == eBitResult.OFF)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("EQUIPMENT_NO=[{0}] CIMMODE=[{1}] NO NEED REPORT DAILYCHECKDATA!", eqp.Data.NODENO, eqp.File.CIMMode.ToString()));
                        continue;
                    }
                    //Jun Add 20150521 機台狀態不是IDEL or RUN就不上報Daily Check
                    if (eqp.File.Status != eEQPStatus.IDLE && eqp.File.Status != eEQPStatus.RUN)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("EQUIPMENT_NO=[{0}] STATUS=[{1}] NO NEED REPORT DAILYCHECKDATA!", eqp.Data.NODENO, eqp.File.Status.ToString()));
                        continue;
                    }

                    switch (eqp.Data.REPORTMODE)
                    {
                        case "PLC":
                        case "PLC_HSMS":
                            IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqp.Data.NODENO);
                            if (dataFormats == null)  //Jun Modify 20141203 如果DailyCheckData Table沒有資料時 記Log
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("CAN'T GET EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqp.Data.NODENO));
                                continue;
                            }
                            //取得PLC资料
                            //modify by edison20150119:follow新的PLC Agent SyncReadTrx方法，多加一个参数
                            Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_DailyCheckProcessDataBlock, eqp.Data.NODENO), false }) as Trx;

                            if (trx == null)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] TRACTION!", eqp.Data.NODENO));
                                continue;
                            }

                            HandleData hd = new HandleData(eqp, dataFormats, trx);
                            reportList.Enqueue(hd);

                            break;

                        case "HSMS_PLC":
                        case "HSMS_CSOT":
                        case "HSMS_NIKON":
                            string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                            List<Tuple<string, List<Tuple<string, string, string>>>> dailycheckRepository = Repository.Get(key) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                            if (dailycheckRepository == null)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("CAN'T GET EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqp.Data.NODENO));
                                continue;
                            }

                            HandleSECSData hdSECS = new HandleSECSData(eqp, dailycheckRepository);
                            reportListSECS.Enqueue(hdSECS);
                            break;
                    }
                }
                DailyCheckReport(reportList, ref facilityReplyList, ref facilityReportList, ref facilityList);
                DailyCheckReport(reportListSECS, ref facilityReplyList, ref facilityReportList, ref facilityList);

                //if (facilityReplyList.Count > 0) //Jun Modify 20141203 不需要這在判斷是否有Data
                Invoke(eServiceName.MESService, "FacilityParameterReply", new object[] { this.CreateTrxID(), lineName, facilityReplyList, mesINBOXName });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Daily Check Report
        /// </summary>
        private void DailyCheckReport(ConcurrentQueue<HandleData> reportList, ref IList<FacilityParameterReply.MACHINEc> facilityReplyList,
            ref IList<FacilityCheckReport.MACHINEc> facilityReportList, ref List<string> facilityList)
        {
            //facilityReplyList = new List<FacilityParameterReply.MACHINEc>();
            //facilityReportList = new List<FacilityCheckReport.MACHINEc>();
            //facilityList = new List<string>();

            string decodeItemName = "";

            try
            {
                while (reportList.Count > 0)
                {
                    HandleData handleData;
                    reportList.TryDequeue(out handleData);

                    #region report
                    short[] rawData = handleData.Trx.EventGroups[0].Events[0].RawData;

                    string value = string.Empty;
                    int startaddress10 = 0;

                    FacilityParameterReply.MACHINEc facilityReplyMachine = new FacilityParameterReply.MACHINEc();
                    facilityReplyMachine.MACHINENAME = handleData.EQP.Data.NODEID;
                    facilityReplyMachine.MACHINESTATENAME = handleData.EQP.File.MESStatus;  //Jun Modify 20141127 上報MES定義的Status

                    FacilityCheckReport.MACHINEc facilityReportMachine = new FacilityCheckReport.MACHINEc();
                    facilityReportMachine.MACHINENAME = handleData.EQP.Data.NODEID;
                    facilityReportMachine.MACHINESTATENAME = handleData.EQP.File.MESStatus;  //Jun Modify 20141127 上報MES定義的Status

                    foreach (DailyCheckData pd in handleData.DataFormats)
                    {
                        decodeItemName = pd.Data.PARAMETERNAME;

                        ItemExpressionEnum ie;
                        if (!Enum.TryParse(pd.Data.EXPRESSION.ToUpper(), out ie))
                        {
                            continue;
                        }
                        #region decode by expression
                        switch (ie)
                        {
                            case ItemExpressionEnum.BIT:
                                value = ExpressionBIT.Decode(startaddress10, int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                break;
                            case ItemExpressionEnum.ASCII:
                                value = ExpressionASCII.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                break;
                            case ItemExpressionEnum.BIN:
                                value = ExpressionBIN.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                break;
                            case ItemExpressionEnum.EXP:
                                value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), rawData).ToString();
                                break;
                            case ItemExpressionEnum.HEX:
                                value = ExpressionHEX.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData);
                                break;
                            case ItemExpressionEnum.INT:
                                value = ExpressionINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                break;
                            case ItemExpressionEnum.LONG:
                                value = ExpressionLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                break;
                            case ItemExpressionEnum.SINT:
                                value = ExpressionSINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                break;
                            case ItemExpressionEnum.SLONG:
                                value = ExpressionSLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
                                break;
							case ItemExpressionEnum.BCD:
								value = ExpressionBCD.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), rawData).ToString();
								break;
                            default:
                                break;
                        }
                        #endregion

                        #region 算法[modify][by yang 20170329 转不出(value超过类型值范围)报default]
                        string itemValue = string.Empty;
                        double doubleresult;
                        switch (pd.Data.OPERATOR) //目前operator只有'/',only modify '/'
                        {
                            case ArithmeticOperator.PlusSign:
                                itemValue = (long.Parse(value) + long.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.MinusSign:
                                itemValue = (long.Parse(value) - long.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.TimesSign:
                                itemValue = (double.Parse(value) * double.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.DivisionSign:

                                if (double.TryParse(value, out doubleresult))
                                    itemValue = (doubleresult / double.Parse(pd.Data.DOTRATIO)).ToString();
                                else
                                {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + decodeItemName + "), ");
                                    itemValue = "-999";  //default
                                }
                                break;
                            default:
                                itemValue = value;
                                break;
                        }
                        #endregion

                        if (pd.Data.REPORTTO != null)
                        {
                            string[] hostReportList = pd.Data.REPORTTO.Split(',');
                            if (hostReportList.Length > 0)
                            {
                                foreach (string report in hostReportList)
                                {
                                    switch (report.ToUpper())
                                    {
                                        case "MES":
                                            #region MES List
                                            FacilityParameterReply.PARAc facilityReplyPara = new FacilityParameterReply.PARAc();
                                            //to Do 欄位名稱是什麼？
                                            if (pd.Data.PARAMETERNAME.Replace(" ", "") == "RecipeID")
                                                facilityReplyPara.RECIPEID = handleData.EQP.File.CurrentRecipeID;
                                            else
                                                facilityReplyPara.RECIPEID = "EQ_COMMON";
                                            facilityReplyPara.PARANAME = pd.Data.PARAMETERNAME;
                                            facilityReplyPara.VALUETYPE = (ie == ItemExpressionEnum.ASCII) ? "TEXT" : "NUMBER";
                                            facilityReplyPara.PARAVALUE = itemValue;
                                            facilityReplyMachine.FACILITYPARALIST.Add(facilityReplyPara);

                                            FacilityCheckReport.PARAc facilityReportPara = new FacilityCheckReport.PARAc();
                                            //to Do 欄位名稱是什麼？
                                            if (pd.Data.PARAMETERNAME.Replace(" ", "") == "RecipeID")
                                                facilityReportPara.RECIPEID = handleData.EQP.File.CurrentRecipeID;
                                            else
                                                facilityReportPara.RECIPEID = "EQ_COMMON";
                                            facilityReportPara.PARANAME = pd.Data.PARAMETERNAME;
                                            facilityReportPara.VALUETYPE = (ie == ItemExpressionEnum.ASCII) ? "TEXT" : "NUMBER";
                                            facilityReportPara.PARAVALUE = itemValue;
                                            facilityReportMachine.FACILITYPARALIST.Add(facilityReportPara);
                                            #endregion
                                            break;

                                        case "EDA":
                                            break;

                                        case "OEE":
                                            break;

                                        case "APC":
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        facilityList.Add(pd.Data.PARAMETERNAME + "=" + itemValue);
                    }

                    decodeItemName = "";

                    facilityReplyList.Add(facilityReplyMachine);

                    facilityReportList.Add(facilityReportMachine);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM (" + decodeItemName + "), " + ex);
            }

        }

        private void DailyCheckReport(ConcurrentQueue<HandleSECSData> reportListSecs, ref IList<FacilityParameterReply.MACHINEc> facilityReplyList,
            ref IList<FacilityCheckReport.MACHINEc> facilityReportList, ref List<string> facilityList)
        {
            try
            {
                while (reportListSecs.Count > 0)
                {
                    HandleSECSData handleData;
                    reportListSecs.TryDequeue(out handleData);

                    //Jun Modify 20150407 For SECS結構修正
                    FacilityParameterReply.MACHINEc facilityReplyMachine = new FacilityParameterReply.MACHINEc();
                    facilityReplyMachine.MACHINENAME = handleData.EQP.Data.NODEID;
                    facilityReplyMachine.MACHINESTATENAME = handleData.EQP.File.MESStatus;  //Jun Modify 20141127 上報MES定義的Status

                    FacilityCheckReport.MACHINEc facilityReportMachine = new FacilityCheckReport.MACHINEc();
                    facilityReportMachine.MACHINENAME = handleData.EQP.Data.NODEID;
                    facilityReportMachine.MACHINESTATENAME = handleData.EQP.File.MESStatus;  //Jun Modify 20141127 上報MES定義的Status

                    //List<Tuple<subeqpid, List<Tuple<dcname, dctype, dcvalue>>>
                    foreach (Tuple<string, List<Tuple<string, string, string>>> nodeTuple in handleData.DataFormats)
                    {
                        //Tuple<dcname, dctype, dcvalue>
                        foreach (Tuple<string, string, string> itemTuple in nodeTuple.Item2)
                        {
                            FacilityParameterReply.PARAc facilityReplyPara = new FacilityParameterReply.PARAc();
                            //to Do 欄位名稱是什麼？
                            if (itemTuple.Item1.Replace(" ", "") == "RecipeID")
                                facilityReplyPara.RECIPEID = handleData.EQP.File.CurrentRecipeID;
                            else
                                facilityReplyPara.RECIPEID = "EQ_COMMON";
                            facilityReplyPara.PARANAME = nodeTuple.Item1 + "_" + itemTuple.Item1;
                            facilityReplyPara.VALUETYPE = itemTuple.Item2;
                            facilityReplyPara.PARAVALUE = itemTuple.Item3;
                            facilityReplyMachine.FACILITYPARALIST.Add(facilityReplyPara);

                            FacilityCheckReport.PARAc facilityReportPara = new FacilityCheckReport.PARAc();
                            //to Do 欄位名稱是什麼？
                            if (itemTuple.Item1.Replace(" ", "") == "RecipeID")
                                facilityReportPara.RECIPEID = handleData.EQP.File.CurrentRecipeID;
                            else
                                facilityReportPara.RECIPEID = "EQ_COMMON";
                            facilityReportPara.PARANAME = nodeTuple.Item1 + "_" + itemTuple.Item1;
                            facilityReportPara.VALUETYPE = itemTuple.Item2;
                            facilityReportPara.PARAVALUE = itemTuple.Item3;
                            facilityReportMachine.FACILITYPARALIST.Add(facilityReportPara);

                            facilityList.Add(itemTuple.Item1 + "=" + itemTuple.Item3);
                        }
                    }

                    facilityReplyList.Add(facilityReplyMachine);

                    facilityReportList.Add(facilityReportMachine);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Daily Check Request By Node No
        /// </summary>
        public bool DailyCheckRequestByNO(string eqpNo, out List<string> parameter, out string desc)
        {
            parameter = new List<string>();
            desc = "";

            try
            {
                ConcurrentQueue<HandleData> reportList = new ConcurrentQueue<HandleData>();
                ConcurrentQueue<HandleSECSData> reportListSECS = new ConcurrentQueue<HandleSECSData>();
                IList<FacilityParameterReply.MACHINEc> facilityReplyList = new List<FacilityParameterReply.MACHINEc>();
                IList<FacilityCheckReport.MACHINEc> facilityReportList = new List<FacilityCheckReport.MACHINEc>();
                List<string> facilityList = new List<string>();

                if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED)
                    throw new Exception("PLC CONNECTED STATE IS DISCONNECTED");

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqpNo));

                switch (eqp.Data.REPORTMODE)
                {
                    case "PLC":
                    case "PLC_HSMS":
                        IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpNo);
                        if (dataFormats == null)
                            throw new Exception(string.Format("CAN'T GET EQUIPMENT_NO=[{0}] DAILY CHECK PROFILE!", eqpNo));

                        //取得PLC资料
                        //modify by edison20150119:follow新的PLC Agent SyncReadTrx方法，多加一个参数
                        Trx trx =Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_DailyCheckProcessDataBlock, eqp.Data.NODENO), false }) as Trx; 

                        if (trx == null)
                            throw new Exception(string.Format("CAN'T GET EQUIPMENT_NO=[{0}] DATA FORM PLC!", eqpNo));

                        HandleData hd = new HandleData(eqp, dataFormats, trx);
                        reportList.Enqueue(hd);

                        DailyCheckReport(reportList, ref facilityReplyList, ref facilityReportList, ref facilityList);

                        break;

                    case "HSMS_PLC": //20150225 cy 增加對此mode的處理
                    case "HSMS_CSOT":
                    case "HSMS_NIKON":
                        string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                        List<Tuple<string, List<Tuple<string, string, string>>>> dailycheckRepository = Repository.Get(key) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                        if (dailycheckRepository == null)
                            throw new Exception(string.Format("Can't Get Equipment No=[{0}) Daily Check Profile!", eqpNo));

                        HandleSECSData hdSECS = new HandleSECSData(eqp, dailycheckRepository);
                        reportListSECS.Enqueue(hdSECS);

                        DailyCheckReport(reportListSECS, ref facilityReplyList, ref facilityReportList, ref facilityList);

                        break;
                }                

                if (facilityList.Count > 0)
                {
                    parameter = facilityList;
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("CAN'T DECODE EQUIPMENT_NO=[{0}) PLC DATA, OR EQUIPMENT_NO=[{0}) PROCESS DATA ITEM SETTING PROBLEM IN DB!", eqpNo));
                }
            }
            catch (Exception ex)
            {
                desc = ex.Message.ToString();

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void StopReport()
        {
            isRuning = false;
        }

        public void StartReport()
        {
            isRuning = true;
        }

        public class HandleData
        {
            private Equipment _eQP;

            public Equipment EQP
            {
                get { return _eQP; }
                set { _eQP = value; }
            }

            private Trx _trx;

            public Trx Trx
            {
                get { return _trx; }
                set { _trx = value; }
            }

            private IList<DailyCheckData> _dataFormats;

            public IList<DailyCheckData> DataFormats
            {
                get { return _dataFormats; }
                set { _dataFormats = value; }
            }

            public HandleData(Equipment eqp, IList<DailyCheckData> dataFormats, Trx trx)
            {
                _eQP = eqp;
                _dataFormats = dataFormats;
                _trx = trx;
            }
        }

        public class HandleSECSData
        {
            private Equipment _eqp;

            public Equipment EQP
            {
                get { return _eqp; }
                set { _eqp = value; }
            }

            private List<Tuple<string, List<Tuple<string, string, string>>>> _dataFormats;

            public List<Tuple<string, List<Tuple<string, string, string>>>> DataFormats
            {
                get { return _dataFormats; }
                set { _dataFormats = value; }
            }

            public HandleSECSData(Equipment eqp, List<Tuple<string, List<Tuple<string, string, string>>>> dataFormats)
            {
                _eqp = eqp;
                _dataFormats = dataFormats;
            }
        }
    }
}