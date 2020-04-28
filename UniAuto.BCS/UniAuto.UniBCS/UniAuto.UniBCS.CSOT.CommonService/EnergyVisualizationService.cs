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
using UniAuto.UniBCS.Core;
using System.Text.RegularExpressions;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class EnergyVisualizationService : AbstractService
    {
        private const string Key_EnergyVisualizationDataBlock = "{0}_EnergyVisualizationDataBlock";
        //private const string OVNKey_EnergyVisualizationDataBlock = "{0}_EnergyVisualizationDataBlock#1";  //add by qiumin 20180210 OVN100 plc map change

        private bool isRuning = false;

        private Thread checkThread;
        private Thread reportThread;

        private ConcurrentQueue<HandleData> reportList;
        private ConcurrentQueue<HandleSECSData> reportListSECS;

        public override bool Init()
        {
            reportList = new ConcurrentQueue<HandleData>();
            reportListSECS = new ConcurrentQueue<HandleSECSData>();
            checkThread = new Thread(new ThreadStart(EnergyVisualizationCheck)) { IsBackground = true };
            checkThread.Start();
            reportThread = new Thread(new ThreadStart(EnergyVisualizationReport_PLC)) { IsBackground = true };
            reportThread.Start();
            reportThread = new Thread(new ThreadStart(EnergyVisualizationReport_SECS)) { IsBackground = true };
            reportThread.Start();
            isRuning = true;
            return true;
        }

        /// <summary>
        /// Energy Visualization Cycle Check
        /// </summary>
        private void EnergyVisualizationCheck()
        {
            while (true)
            {

                Thread.Sleep(1000);
                try
                {
                    if ((!this.isRuning)||(Workbench.State!=eWorkbenchState.RUN)) continue;
                    //Jun Modify 20150521 不需要判斷ConnectedState == eAGENT_STATE.DISCONNECTED
                    //if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED) continue;
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                    {                        
                        //no report
                        if (eqp.File.EnergyIntervalS == 0) continue;
                        DateTime now = DateTime.Now;

                        bool hasSerialUnit = false;//20161229 sy add Serial
                        List<Unit> serialUnit = new List<Unit>();
                        foreach (Unit unit in ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO))
                        {
                            if (unit.Data.UNITATTRIBUTE == "ATTACHMENT")
                                serialUnit.Add(unit);
                        }
                        if (serialUnit.Count != 0)
                        {
                            //hasSerialUnit = true;//20161229 sy 暫時 不使用
                        }

                        if (now.Subtract(eqp.File.EnergyLastDT).TotalMilliseconds >= eqp.File.EnergyIntervalS)
                        {
                            switch (eqp.Data.REPORTMODE)
                            {
                                case "PLC":
                                case "PLC_HSMS":

                                    IList<EnergyVisualizationData> dataFormats = ObjectManager.EnergyVisualizationManager.GetEnergyVisualizationProfile(eqp.Data.NODENO);
                                    Trx trx= Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_EnergyVisualizationDataBlock, eqp.Data.NODENO), false }) as Trx;
                                   
                                    HandleData hd;
                                    if (hasSerialUnit)
                                    {
                                        string keyForService = string.Format("{0}_{1}_SecsSpecialDataReqForService", eqp.Data.LINEID, eqp.Data.NODEID);
                                        List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepositoryForService = Repository.Get(keyForService) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                        if (dataFormats == null && specialData4IDRepositoryForService ==null)
                                        {
                                            continue;
                                        }
                                        else if (dataFormats != null && specialData4IDRepositoryForService == null)
                                        {
                                            if (trx == null) continue;
                                            hd = new HandleData(eqp, dataFormats, trx);
                                            reportList.Enqueue(hd);
                                        }
                                        else if (dataFormats == null && specialData4IDRepositoryForService != null)//只有serail 格式同SECS 用SECS上報
                                        {
                                            #region [SECS格式]
                                            List<Tuple<string, string, string>> energyCheckRepositoryForPLCMC = new List<Tuple<string, string, string>>();
                                            int dataGroupNoForPLCMC = 1;
                                            foreach (var specialDatas in specialData4IDRepositoryForService)
                                            {
                                                int dataItemNo = 1;
                                                foreach (var specialData in specialDatas.Item2)
                                                {
                                                    energyCheckRepositoryForPLCMC.Add(Tuple.Create(specialData.Item1, dataGroupNoForPLCMC + "_" + dataItemNo, specialData.Item3));
                                                    dataItemNo++;
                                                }
                                                dataGroupNoForPLCMC++;
                                            }
                                            #endregion
                                            HandleSECSData hdSECSForPLCMC = new HandleSECSData(eqp, energyCheckRepositoryForPLCMC);
                                            reportListSECS.Enqueue(hdSECSForPLCMC);                                            
                                        }
                                        else
                                        {
                                            if (trx == null) continue;
                                            #region [SECS格式]
                                            List<Tuple<string, string, string>> energyCheckRepositoryForPLCMC = new List<Tuple<string, string, string>>();
                                            int dataGroupNoForPLCMC = 10000;
                                            foreach (var specialDatas in specialData4IDRepositoryForService)
                                            {
                                                int dataItemNo = 1;
                                                foreach (var specialData in specialDatas.Item2)
                                                {
                                                    energyCheckRepositoryForPLCMC.Add(Tuple.Create(specialData.Item1, dataGroupNoForPLCMC + "_" + dataItemNo, specialData.Item3));
                                                    dataItemNo++;
                                                }
                                                dataGroupNoForPLCMC++;
                                            }
                                            #endregion
                                            hd = new HandleData(eqp, dataFormats, trx, energyCheckRepositoryForPLCMC);
                                            reportList.Enqueue(hd);
                                        }
                                    }
                                    else
                                    {
                                        if (dataFormats == null) continue;
                                        if (trx == null) continue;
                                        hd = new HandleData(eqp, dataFormats, trx);
                                        reportList.Enqueue(hd);
                                    } 

                                    lock (eqp.File)
                                    {
                                        eqp.File.EnergyLastDT = DateTime.Now;
                                    }

                                    break;

                                case "HSMS_PLC":
                                case "HSMS_CSOT":
                                case "HSMS_NIKON":
                                    
                                   //string key = string.Format("{0}_{1}_SecsSpecialData", eqp.Data.LINEID, eqp.Data.NODEID);
                                    string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);//20161019 sy modify

                                    List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepository =  Repository.Get(key) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    int dataGroupNo = 1;

                                    if (hasSerialUnit)
                                    {
                                        #region [hasSerialUnit]
                                        string keyForService = string.Format("{0}_{1}_SecsSpecialDataReqForService", eqp.Data.LINEID, eqp.Data.NODEID);
                                        List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepositoryForService = Repository.Get(keyForService) as List<Tuple<string, List<Tuple<string, string, string>>>>; 
                                        
                                        if (specialData4IDRepository == null && specialData4IDRepositoryForService == null)
                                        {
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T GET EQUIPMENT_NO=[{0}] ENERGY CHECK PROFILE! And CAN'T GET SERIAL DATA", eqp.Data.NODENO));
                                            continue;
                                        }
                                        else if (specialData4IDRepository == null && specialData4IDRepositoryForService != null)
                                        {
                                            specialData4IDRepository = specialData4IDRepositoryForService;
                                            dataGroupNo = 10000;
                                        }
                                        else if (specialData4IDRepository != null && specialData4IDRepositoryForService == null)
                                        {
                                            //不用調整
                                        }
                                        else //都不為Null 結合上報
                                        {
                                            specialData4IDRepository.AddRange(specialData4IDRepositoryForService);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        if (specialData4IDRepository == null)
                                        {
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T GET EQUIPMENT_NO=[{0}] ENERGY CHECK PROFILE!", eqp.Data.NODENO));
                                            continue;
                                        }
                                    }
                                   
                                    List<Tuple<string, string, string>> energyCheckRepository = new  List<Tuple<string, string, string>>();
                                    foreach (var specialDatas in specialData4IDRepository)
                                    {
                                        int dataItemNo = 1;
                                        foreach (var specialData in specialDatas.Item2)
                                        {                                            
                                            energyCheckRepository.Add(Tuple.Create(specialData.Item1, dataGroupNo + "_" + dataItemNo, specialData.Item3));
                                            dataItemNo++;
                                            if(dataItemNo>7) //modify by yang 20170120 For Group List
                                            {
                                                dataItemNo -= 7;
                                                dataGroupNo++;
                                            }
                                        }
                                     //   dataGroupNo++;
                                    }

                                    HandleSECSData hdSECS = new HandleSECSData(eqp, energyCheckRepository);
                                    reportListSECS.Enqueue(hdSECS);
                                    lock (eqp.File)
                                    {
                                        eqp.File.EnergyLastDT = DateTime.Now;
                                    }

                                    break;
                            }
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                }
            } 
            
        }

        /// <summary>
        /// Energy Visualization Cycle Report
        /// </summary>
        private void EnergyVisualizationReport_PLC()
        {
            string decodeItemName = "";

            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    if (reportList.Count > 0)
                    {
                        HandleData handleData;
                        reportList.TryDequeue(out handleData);

                        #region [report]
                        short[] rawData = handleData.Trx.EventGroups[0].Events[0].RawData;

                        string value = string.Empty;
                        int startaddress10 = 0;
                        Dictionary<string, EnergyMeter> resultDic = new Dictionary<string, EnergyMeter>();
                        #region [PLC]
                        foreach (EnergyVisualizationData pd in handleData.DataFormats)
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
                                     value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
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
                                                break;

                                            case "EDA":
                                                break;

                                            case "OEE":
                                                #region OEE List
                                                string paraGroup = (pd.Data.ITEM == "" ? "" : pd.Data.ITEM);
                                                if (!resultDic.ContainsKey(paraGroup))
                                                {
                                                    EnergyMeter energyMeter = new EnergyMeter();
                                                    resultDic.Add(paraGroup, energyMeter);
                                                }

                                                switch (pd.Data.SITE)
                                                {
                                                    case "1":
                                                        if (itemValue == "1") resultDic[paraGroup].EnergyType = "Liquid";
                                                        if (itemValue == "2") resultDic[paraGroup].EnergyType = "Electricity";
                                                        if (itemValue == "3") resultDic[paraGroup].EnergyType = "Gas";
                                                        if (itemValue == "4") resultDic[paraGroup].EnergyType = "N2";
                                                        if (itemValue == "5") resultDic[paraGroup].EnergyType = "Stripper";
                                                        break;

                                                    case "2":
                                                        //resultDic[paraGroup].UnitID = itemValue;
                                                        //modify by yang 20170123 for unitNO Changed to unitID
                                                        if (!itemValue.Trim().Equals("0"))
                                                        {
                                                            Unit unit = ObjectManager.UnitManager.GetUnit(handleData.EQP.Data.NODENO, itemValue);
                                                            resultDic[paraGroup].UnitID = unit.Data.UNITID;
                                                        }
                                                        else
                                                            resultDic[paraGroup].UnitID = itemValue;
                                                        break;

                                                    case "3":
                                                        resultDic[paraGroup].MeterNo = itemValue;
                                                        break;

                                                    case "4": 
                                                        //Liquid: Instant liquid flow value, unit is "L/Min" 
                                                        // Electricity: Instant electric current value, unit is "A"
                                                        //Gas: Instant gas flow value, unit is "L/Min"
                                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                                        break;

                                                    case "5":
                                                        //Liquid: Instant liquid pressure value, unit is "Mpa"
                                                        //Electricity: Instant electric voltage value, unit is "V"
                                                        //Gas: Instant gas pressure value, unit is "Mpa"
                                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);   // modify by bruce 20160406 fix bug 
                                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                                        break;

                                                    case "6":
                                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                                        break;

                                                    case "7":
                                                        resultDic[paraGroup].Refresh = itemValue;
                                                        break;
                                                }
                                                #endregion
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region [Serial Unit]
                        if (handleData.SeraldataFormats != null)
                        {
                            EnergyMeter energyMeterSerial = new EnergyMeter();
                            foreach (Tuple<string, string, string> nodeTuple in handleData.SeraldataFormats)
                            {
                                string paraGroup = nodeTuple.Item2.ToString().Split('_')[0];
                                string paraItemNo = nodeTuple.Item2.ToString().Split('_')[1];
                                string itemValue = nodeTuple.Item3.ToString();

                                if (!resultDic.ContainsKey(paraGroup))
                                {
                                    energyMeterSerial = new EnergyMeter();
                                    resultDic.Add(paraGroup, energyMeterSerial);
                                }
                                switch (paraItemNo)
                                {
                                    case "1":
                                        if (itemValue == "1") resultDic[paraGroup].EnergyType = "Liquid";
                                        if (itemValue == "2") resultDic[paraGroup].EnergyType = "Electricity";
                                        if (itemValue == "3") resultDic[paraGroup].EnergyType = "Gas";
                                        if (itemValue == "4") resultDic[paraGroup].EnergyType = "N2";
                                        if (itemValue == "5") resultDic[paraGroup].EnergyType = "Stripper";
                                        break;

                                    case "2":
                                        resultDic[paraGroup].UnitID = itemValue;
                                        break;

                                    case "3":
                                        resultDic[paraGroup].MeterNo = itemValue;
                                        break;

                                    case "4":
                                        //Liquid: Instant liquid flow value, unit is "L/Min" 
                                        // Electricity: Instant electric current value, unit is "A"
                                        //Gas: Instant gas flow value, unit is "L/Min"
                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                        break;

                                    case "5":
                                        //Liquid: Instant liquid pressure value, unit is "Mpa"
                                        //Electricity: Instant electric voltage value, unit is "V"
                                        //Gas: Instant gas pressure value, unit is "Mpa"
                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);   // modify by bruce 20160406 fix bug 
                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                        break;

                                    case "6":
                                        if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                        if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                        break;

                                    case "7":
                                        resultDic[paraGroup].Refresh = itemValue;
                                        break;
                                }
                            }
                        }
                        #endregion
                        Invoke(eServiceName.OEEService, "EnergyReport", new object[] { this.CreateTrxID(), handleData.EQP.Data.LINEID, handleData.EQP, resultDic });

                        decodeItemName = "";

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM (" + decodeItemName + "), " + ex);
                }
            }                                   
        }

        private void EnergyVisualizationReport_SECS()
        { 
            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    if (reportListSECS.Count > 0)
                    {
                        HandleSECSData handleData;
                        reportListSECS.TryDequeue(out handleData);

                        Dictionary<string, EnergyMeter> resultDic = new Dictionary<string, EnergyMeter>();
                        //20161019 sy modify
                        EnergyMeter energyMeter = new EnergyMeter();
                        foreach (Tuple<string, string, string> nodeTuple in handleData.DataFormats)
                        {
                            string paraGroup = nodeTuple.Item2.ToString().Split('_')[0];
                            string paraItemNo = nodeTuple.Item2.ToString().Split('_')[1];
                            string itemValue = nodeTuple.Item3.ToString();


                            if (!resultDic.ContainsKey(paraGroup))
                            {
                                energyMeter = new EnergyMeter();
                                resultDic.Add(paraGroup, energyMeter);
                            }
                            switch (paraItemNo)
                            {
                                case "1":
                                    if (itemValue == "1") resultDic[paraGroup].EnergyType = "Liquid";
                                    if (itemValue == "2") resultDic[paraGroup].EnergyType = "Electricity";
                                    if (itemValue == "3") resultDic[paraGroup].EnergyType = "Gas";
                                    if (itemValue == "4") resultDic[paraGroup].EnergyType = "N2";
                                    if (itemValue == "5") resultDic[paraGroup].EnergyType = "Stripper";
                                    break;

                                case "2":
                                    int outvalue;
                                    if (int.TryParse(itemValue, out outvalue)&&!itemValue.Trim().Equals("0")) //modify by yang 20170123 for unitNO Changed to unitID
                                    {
                                        Unit unit = ObjectManager.UnitManager.GetUnit(handleData.EQP.Data.NODENO,itemValue);
                                        resultDic[paraGroup].UnitID = unit.Data.UNITID;
                                    }
                                    else
                                        resultDic[paraGroup].UnitID = itemValue;
                                    break;

                                case "3":
                                    resultDic[paraGroup].MeterNo = itemValue;
                                    break;

                                case "4":
                                    //Liquid: Instant liquid flow value, unit is "L/Min" 
                                    // Electricity: Instant electric current value, unit is "A"
                                    //Gas: Instant gas flow value, unit is "L/Min"
                                    if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("CURRENT", itemValue);
                                    break;

                                case "5":
                                    //Liquid: Instant liquid pressure value, unit is "Mpa"
                                    //Electricity: Instant electric voltage value, unit is "V"
                                    //Gas: Instant gas pressure value, unit is "Mpa"
                                    if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);   // modify by bruce 20160406 fix bug 
                                    if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("PRESSURE", itemValue);
                                    break;

                                case "6":
                                    if (resultDic[paraGroup].EnergyType == "Liquid") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Electricity") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Gas") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "N2") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                    if (resultDic[paraGroup].EnergyType == "Stripper") resultDic[paraGroup].EnergyParam.Add("QUANTITY", itemValue);
                                    break;

                                case "7":
                                    resultDic[paraGroup].Refresh = itemValue;
                                    break;
                            }                        
                        }
                        Invoke(eServiceName.OEEService, "EnergyReport", new object[] { this.CreateTrxID(), handleData.EQP.Data.LINEID, handleData.EQP, resultDic });
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
        }

        private void StopReport()
        {
            isRuning = false;
        }

        private void StartReport()
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

            private IList<EnergyVisualizationData> _dataFormats;

            public IList<EnergyVisualizationData> DataFormats
            {
                get { return _dataFormats; }
                set { _dataFormats = value; }
            }

            private List<Tuple<string, string, string>> _seraldataFormats;

            public List<Tuple<string, string, string>> SeraldataFormats
            {
                get { return _seraldataFormats; }
                set { _seraldataFormats = value; }
            }

            public HandleData(Equipment eqp, IList<EnergyVisualizationData> dataFormats, Trx trx)
            {
                _eQP = eqp;
                _dataFormats = dataFormats;
                _trx = trx;
            }
            public HandleData(Equipment eqp, IList<EnergyVisualizationData> dataFormats, Trx trx, List<Tuple<string, string, string>> seraldataFormats)
            {
                _eQP = eqp;
                _dataFormats = dataFormats;
                _seraldataFormats = seraldataFormats;
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

            private List<Tuple<string, string, string>> _dataFormats;

            public List<Tuple<string, string, string>> DataFormats
            {
                get { return _dataFormats; }
                set { _dataFormats = value; }
            }

            public HandleSECSData(Equipment eqp, List<Tuple<string, string, string>> dataFormats)
            {
                _eqp = eqp;
                _dataFormats = dataFormats;
            }
        }
    }
}
