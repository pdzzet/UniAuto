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
using System.Xml;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class APCDataService : AbstractService
    {
        private const string Key_APCDataBlock = "{0}_APCDataBlock";
       // private const string OVNKey_APCDataBlock = "{0}_APCDataBlock#1"; //add by qiumin 20180210 OVN100 plc map change
        private const string Key_APCDataDownload = "{0}_APCDataDownload";
        private const string Key_APCDataDownloadTimeout = "{0}_APCDataDownloadTimeout";

        private bool isRuning = false;

        private Thread checkThread;
        private Thread reportThread_PLC;
        private Thread reportThread_SECS;
        private Thread productInOutThread_FA;//by huangjiayin 20180411
        private ConcurrentQueue<HandleData> reportList;
        private ConcurrentQueue<HandleSECSData> reportListSECS;
        private Dictionary<string, bool> faInOutStatus;//by huangjiayin 20180411
        

        public override bool Init()
        {
            reportList = new ConcurrentQueue<HandleData>();
            reportListSECS = new ConcurrentQueue<HandleSECSData>();
            faInOutStatus = new Dictionary<string, bool>();//by huangjiayin 20180411
            init_FAeqp();//by huangjiayin 20180411
            checkThread = new Thread(new ThreadStart(APCDataCheck)) { IsBackground = true };
            checkThread.Start();
            reportThread_PLC = new Thread(new ThreadStart(APCDataReport_PLC)) { IsBackground = true };
            reportThread_PLC.Start();
            reportThread_SECS = new Thread(new ThreadStart(APCDataRpoert_SECS)) { IsBackground = true };
            reportThread_SECS.Start();
            productInOutThread_FA = new Thread(new ThreadStart(ProductInOutReport_FA)) { IsBackground = true };//by huangjiayin 20180411
            productInOutThread_FA.Start();//by huangjiayin 20180411
            isRuning = true;
            return true;
        }


        //by huangjiayin 20180411
        private void init_FAeqp()
        {
            List<Equipment> fa_eqps = ObjectManager.EquipmentManager.GetEQPs_FA();
            foreach (var eq in fa_eqps)
            {

                faInOutStatus.Add(eq.Data.NODEID, false);
            }
        }


        //by huangjiayin 20180411
        private void ProductInOutReport_FA()
        {
            while (true)
            {
                Thread.Sleep(1000);

                try
                {
                    if (faInOutStatus.Count == 0) continue;
                    bool nowStatus;
                    bool oldStatus;
                    var fa_eqps = faInOutStatus.Keys.ToList<string>();
                    lock (faInOutStatus)
                    {
                        foreach (var eqp in fa_eqps)
                        {
                            Equipment eq = ObjectManager.EquipmentManager.GetEQPByID(eqp);
                            Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_APCDataBlock, eq.Data.NODENO), false }) as Trx;
                            if (trx == null) continue;
                            APCDataReport faApcData=null;
                            if (ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eq.Data.NODENO) ==null) continue;
                            faApcData = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eq.Data.NODENO).FirstOrDefault(w => w.Data.PARAMETERNAME == "FDC_Status");
                            if (faApcData == null) continue;
                            ItemExpressionEnum ie;
                            if (!Enum.TryParse(faApcData.Data.EXPRESSION.ToUpper(), out ie))
                            {
                                continue;
                            }
                            if (ie != ItemExpressionEnum.INT) continue;

                            oldStatus = faInOutStatus[eqp];
                            short[] rawData = trx.EventGroups[0].Events[0].RawData;
                            string value = ExpressionINT.Decode(0, int.Parse(faApcData.Data.WOFFSET), int.Parse(faApcData.Data.WPOINTS), int.Parse(faApcData.Data.BOFFSET), int.Parse(faApcData.Data.BPOINTS), rawData).ToString();
                            switch (value)
                            {
                                case "0":
                                    nowStatus = false;
                                    break;
                                case "1":
                                    nowStatus = true;
                                    break;
                                default:
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Equipment=[{0}], ParameterName=[{1}], Report Invalid Value=[{2}], Skip FA Equipment ProductInOut Report!",eq.Data.NODEID,faApcData.Data.PARAMETERNAME,value));
                                    continue;

                            }

                            if (oldStatus == nowStatus)
                            { continue; }
                            else
                            {
                                faInOutStatus[eqp] = nowStatus;
                                DateTime nowDate = DateTime.Now;
                                //invoke Product In Out
                                #region[ProductIn]
                                if (nowStatus)
                                {
                                    XmlDocument xml_doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("PRODUCTIN") as XmlDocument; 
                                    XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                                    bodyNode["MESSAGE_ID"].InnerText = "PRODUCTIN";
                                    bodyNode[keyHost.TRANSACTIONID].InnerText = nowDate.ToString("yyyyMMddHHmmssfff");
                                    bodyNode[keyHost.LINENAME].InnerText =eq.Data.LINEID ;
                                    bodyNode[keyHost.MACHINENAME].InnerText = eq.Data.NODEID;
                                    bodyNode[keyHost.UNITNAME].InnerText = eq.Data.NODEID;
                                    bodyNode[keyHost.PORTNAME].InnerText = string.Empty;
                                    bodyNode[keyHost.TRACELEVEL].InnerText = eMESTraceLevel.M.ToString();
                                    bodyNode[keyHost.JOBRECIPENAME].InnerText = eq.File.CurrentRecipeID;
                                    bodyNode[keyHost.LOTNAME].InnerText = eq.Data.LINEID + "_" + nowDate.ToString("yyyyMMddHH");//lot by hr
                                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = "Product_NA";
                                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = "Oper_NA";
                                    bodyNode[keyHost.PRODUCTOWNER].InnerText = "OwnerP";
                                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                                    bodyNode[keyHost.PRODUCTNAME].InnerText = eq.Data.LINEID + "_" + nowDate.ToString("yyyyMMddHHmmss");//product by second
                                    bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = eq.Data.LINEID + "_" + nowDate.ToString("yyyyMMddHHmmss");//product by second
                                    bodyNode[keyHost.TIMESTAMP].InnerText = nowDate.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                                    bodyNode[keyHost.LASTGLASSFLAG].InnerText ="N";// 20161019 sy modify

                                    //存入数据仓库
                                    if(Repository.ContainsKey(eq.Data.NODENO+"_FaAPCInOut"))
                                    {
                                        Repository.Remove(eq.Data.NODENO + "_FaAPCInOut");
                                    }

                                    Repository.Add(eq.Data.NODENO + "_FaAPCInOut", bodyNode[keyHost.LOTNAME].InnerText + ":" + bodyNode[keyHost.PRODUCTNAME].InnerText);
                                    Invoke(eServiceName.APCService, "SendToAPCForInvoke", new object[1] { xml_doc });
                                    
                                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                        bodyNode[keyHost.TRANSACTIONID].InnerText, bodyNode[keyHost.LINENAME].InnerText));



                                }
                                #endregion

                                #region[ProductOut]
                                else
                                {
                                    //取出数据仓库
                                    if (!Repository.ContainsKey(eq.Data.NODENO + "_FaAPCInOut")) continue;
                                    string lot_product=Repository.Get(eq.Data.NODENO + "_FaAPCInOut").ToString();

                                    XmlDocument xml_doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("PRODUCTOUT") as XmlDocument;
                                    XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                                    bodyNode["MESSAGE_ID"].InnerText = "PRODUCTOUT";
                                    bodyNode[keyHost.TRANSACTIONID].InnerText = nowDate.ToString("yyyyMMddHHmmssfff");
                                    bodyNode[keyHost.LINENAME].InnerText = eq.Data.LINEID;
                                    bodyNode[keyHost.MACHINENAME].InnerText = eq.Data.NODEID;
                                    bodyNode[keyHost.UNITNAME].InnerText = eq.Data.NODEID;
                                    bodyNode[keyHost.PORTNAME].InnerText = string.Empty;
                                    bodyNode[keyHost.TRACELEVEL].InnerText = eMESTraceLevel.M.ToString();
                                    bodyNode[keyHost.JOBRECIPENAME].InnerText = eq.File.CurrentRecipeID; //modify by yang 20161227 product in/out report current eqp recipe ID
                                    bodyNode[keyHost.LOTNAME].InnerText = lot_product.Split(':')[0];
                                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = "Product_NA";
                                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = "Oper_NA";
                                    bodyNode[keyHost.PRODUCTOWNER].InnerText = "OwnerP";
                                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                                    bodyNode[keyHost.PRODUCTNAME].InnerText = lot_product.Split(':')[1];
                                    bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = lot_product.Split(':')[1];
                                    bodyNode[keyHost.PRODUCTGRADE].InnerText = "OK";
                                    bodyNode[keyHost.TIMESTAMP].InnerText = nowDate.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                                    bodyNode[keyHost.LASTGLASSFLAG].InnerText = "N";

                                    Invoke(eServiceName.APCService, "SendToAPCForInvoke", new object[1] { xml_doc });

                                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                        bodyNode[keyHost.TRANSACTIONID].InnerText, bodyNode[keyHost.LINENAME].InnerText));
                                }
                                #endregion


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



        //TODO:提供OPI方法，是否有参数?
        //public void APCDataReload()
        //{
        //    try
        //    {
        //        isRuning = false;
        //        bool done = ObjectManager.APCDataReportManager.ReloadAll();
        //        if (done)
        //        {
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()","[OPI -> BCS] APC Data Reload OK.");
        //        }
        //        else
        //        {
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "[OPI -> BCS] APC Data Reload NG.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //    finally
        //    {
        //        isRuning = true;
        //    }
        //}

        #region [APC Data Download Report]
        public void APCDataDownload(string eqpNo, eBitResult command, Dictionary<string, string> hostAPCData)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));
                //取得DB中APC Download Format
                IList<APCDataDownload> apcDataDownload = ObjectManager.APCDataDownloadManager.GetAPCDataDownloadProfile(eqpNo);
                if (apcDataDownload == null) return;
                short[] RawData;

                //ToDo SECS Decode and Send


                // PLC Decode and Send
                APCDownlaod(apcDataDownload, hostAPCData, out RawData);

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_APCDataDownload, eqpNo)) as Trx;
                outputData.EventGroups[0].Events[0].RawData = RawData;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                string timeName = string.Format(Key_APCDataDownloadTimeout, eqpNo);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(APCDataDownloadReplyTimeout), outputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] APC DATA DOWNLOAD, SET BIT(ON)", eqp.Data.NODENO,
                        outputData.TrackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void APCDataDownloadReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS<- EQP][{1}] APC DATA DOWNLOAD REPLY, SET BIT ({2})",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format(Key_APCDataDownloadTimeout, inputData.Metadata.NodeNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_APCDataDownload, inputData.Metadata.NodeNo)) as Trx;
                outputData.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS REPLY, APC DATA DOWNLOAD REPLY, RETURN CODE({2}) SET BIT(OFF).", eqpNo, inputData.TrackKey, returnCode.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void APCDataDownloadReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format(Key_APCDataDownloadTimeout, sArray[0]);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_APCDataDownload, sArray[0])) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS REPLY, APC DATA DOWNLOAD REPLY TIMEOUT SET VALUE(OFF).",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }

        /// <summary>
        /// APC Data Cycle Check
        /// </summary>
        private void APCDataCheck()
        {
           
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    if ((!this.isRuning)||(Workbench.State!=eWorkbenchState.RUN)) continue;
                    //Jun Modify 20150521 不需要判斷ConnectedState == eAGENT_STATE.DISCONNECTED
                    //if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED) continue;
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                    {
                        if (eqp.File.ApcIntervalMS == 0) continue;
                        DateTime now = DateTime.Now;
                        if (now.Subtract(eqp.File.ApcLastDT).TotalMilliseconds >= eqp.File.ApcIntervalMS)
                        {
                            switch (eqp.Data.REPORTMODE)
                            {
                                case "PLC":
                                case "PLC_HSMS":
                                    //Format List
                                    IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO);
                                    if (dataFormats == null) continue;
                                    //modify by edison20150119:follow新的PLC Agent SyncReadTrx方法，多加一个参数
                                    Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format(Key_APCDataBlock, eqp.Data.NODENO), false }) as Trx;
                                    if (trx == null) continue;
                                    HandleData hd = new HandleData(eqp, dataFormats, trx);
                                    reportList.Enqueue(hd);

                                    lock (eqp.File)
                                    {
                                        eqp.File.ApcLastDT = DateTime.Now;
                                    }

                                    break;

                                case "HSMS_PLC":
                                case "HSMS_CSOT":
                                case "HSMS_NIKON":
                                    string normalKey = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                    string importkey = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcDataNormal = Repository.Get(normalKey) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    if (apcDataNormal == null)
                                    {
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("CAN'T GET EQUIPMENT NO({0}) APC NORMAL DATA REPORT PROFILE!", eqp.Data.NODENO));
                                    }
                                    else
                                    {
                                        HandleSECSData hdSECSNormal = new HandleSECSData(eqp, apcDataNormal);
                                        reportListSECS.Enqueue(hdSECSNormal);
                                    }

                                    List<Tuple<string, List<Tuple<string, string, string>>>> apcDataImport = Repository.Get(importkey) as List<Tuple<string, List<Tuple<string, string, string>>>>;
                                    if (apcDataImport == null)
                                    {
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("CAN'T GET EQUIPMENT NO({0}) APC IMPORTANT DATA REPORT PROFILE!", eqp.Data.NODENO));
                                    }
                                    else
                                    {
                                        HandleSECSData hdSECSImport = new HandleSECSData(eqp, apcDataImport);
                                        reportListSECS.Enqueue(hdSECSImport);
                                    }

                                    lock (eqp.File)
                                    {
                                        eqp.File.ApcLastDT = DateTime.Now;
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
        /// APC Data Cycle Report
        /// </summary>
        private void APCDataReport_PLC()
        {
            string  decodeItemName = "";
            //long i =0;
            while (true)
            {
                try
                {
                    Thread.Sleep(5);
                    if (reportList.Count > 0)
                    {
                        HandleData handleData;
                        reportList.TryDequeue(out handleData);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "APC Report Queue Length[" + reportList.Count +"]");
                        if (handleData == null) continue;

                        #region report
                        short[] rawData = handleData.Trx.EventGroups[0].Events[0].RawData;

                        string value = string.Empty;
                        int startaddress10 = 0;

                        Dictionary<string, List<string>> resultDic = new Dictionary<string, List<string>>();
                        List<string> paraList = new List<string>();

                        foreach (APCDataReport pd in handleData.DataFormats)
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
                                                break;

                                            case "EDA":
                                                break;

                                            case "OEE":
                                                break;

                                            case "APC":
                                                paraList.Add(pd.Data.PARAMETERNAME + "=" + itemValue +";"+pd.Data.REPORTUNITNO);// sy add 20160928
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        decodeItemName = "";
                        #endregion

                        resultDic.Add(handleData.EQP.Data.NODEID, paraList);

                        Invoke("APCService", "APCDataReport", new object[] { this.CreateTrxID(), handleData.EQP.Data.LINEID, handleData.EQP, resultDic });
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM : ("+decodeItemName + "), " +  ex);
                }
            }            
        }

        private void APCDataRpoert_SECS()
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
                        
                        IList<APCDataReport> aPCDataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(handleData.EQP.Data.NODENO);// sy add 20160928

                        /*
                        APCImportantRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantDataByReq'
                        APCNormalRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalDataByReq'
                        SpecialDataRepository key=>lineid+'_'+nodeid+'_SecsSpecialDataReq'
                        List<Tuple<subeqpid, List<Tuple<dcname, dctype, dcvalue>>>
                        List<Tuple<string,   List<Tuple<string, string, string>>>>
                        */
                        Dictionary<string, List<string>> resultDic = new Dictionary<string, List<string>>();


                        foreach (Tuple<string, List<Tuple<string, string, string>>> subEQPTuple in handleData.DataFormats)
                        {
                            List<string> paraList = new List<string>();

                            foreach (Tuple<string, string, string> itemTuple in subEQPTuple.Item2)
                            {
                                foreach (APCDataReport aPCDataFormat in aPCDataFormats)// sy add 20160928
                                {
                                    int nameOrID;
                                    string checkKey = int.TryParse(itemTuple.Item1, out nameOrID) ? aPCDataFormat.Data.SVID : aPCDataFormat.Data.PARAMETERNAME;
                                    string paraName = int.TryParse(itemTuple.Item1, out nameOrID) ? aPCDataFormat.Data.PARAMETERNAME : itemTuple.Item1;
                                    if (itemTuple.Item1 == checkKey)// sy add 20160928
                                    {
                                        paraList.Add(paraName + "=" + itemTuple.Item3 + ";" + aPCDataFormat.Data.REPORTUNITNO);// sy add 20160928
                                        break;
                                    }
                                }
                            }
                            
                            if (!resultDic.ContainsKey(subEQPTuple.Item1))
                                resultDic.Add(subEQPTuple.Item1, paraList);
                            else
                                resultDic[subEQPTuple.Item1] = paraList;
                        }

                        //foreach (Tuple<string, string, string> nodeTuple in handleData.DataFormats)
                        //{
                        //    paraList.Add(nodeTuple.Item1 + "=" + nodeTuple.Item3);
                        //}

                        //resultDic.Add(handleData.EQP.Data.NODEID, paraList);

                        if (resultDic.Count > 0)
                        {
                            Invoke("APCService", "APCDataReport", new object[] { this.CreateTrxID(), handleData.EQP.Data.LINEID, handleData.EQP, resultDic });
                        }
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

        private void APCDownlaod(IList<APCDataDownload> apcDataDownload, Dictionary<string, string> hostAPCData, out short[] RawData)
        {
            short[] sData = new short[apcDataDownload.Count];
            try
            {
                foreach (APCDataDownload item in apcDataDownload)
                {
                    if (hostAPCData.ContainsKey(item.Data.PARAMETERNAME.Trim()))
                    {
                        string value = hostAPCData[item.Data.PARAMETERNAME.Trim()];
                        ItemExpressionEnum ie;
                        if (!Enum.TryParse(item.Data.EXPRESSION.ToUpper(), out ie))
                        {
                            continue;
                        }

                        #region 算法
                        string itemValue = string.Empty;
                        switch (item.Data.OPERATOR)
                        {
                            case ArithmeticOperator.PlusSign:
                                itemValue = (double.Parse(value) - double.Parse(item.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.MinusSign:
                                itemValue = (double.Parse(value) + double.Parse(item.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.TimesSign:
                                itemValue = (double.Parse(value) / double.Parse(item.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.DivisionSign:
                                itemValue = (double.Parse(value) * double.Parse(item.Data.DOTRATIO)).ToString();
                                break;
                            default:
                                itemValue = value;
                                break;
                        }
                        #endregion

                        #region encode by expression
                        switch (ie)
                        {
                            case ItemExpressionEnum.BIT:
                                ExpressionBIT.Encode(itemValue, int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.ASCII:
                                ExpressionASCII.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.BIN:
                                ExpressionBIN.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.EXP:
                                ExpressionEXP.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), sData);
                                break;
                            case ItemExpressionEnum.HEX:
                                ExpressionHEX.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.INT:
                                ExpressionINT.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.LONG:
                                ExpressionLONG.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.SINT:
                                ExpressionSINT.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
                            case ItemExpressionEnum.SLONG:
                                ExpressionSLONG.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
                                break;
							case ItemExpressionEnum.BCD:
								ExpressionBCD.Encode(itemValue, int.Parse(item.Data.WOFFSET), int.Parse(item.Data.WPOINTS), int.Parse(item.Data.BOFFSET), int.Parse(item.Data.BPOINTS), sData);
								break;
                            default:
                                break;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            RawData = sData;
        }

        public class HandleData
        {
            private Equipment _eqp;

            public Equipment EQP
            {
                get { return _eqp; }
                set { _eqp = value; }
            }

            private Trx _trx;

            public Trx Trx
            {
                get { return _trx; }
                set { _trx = value; }
            }

            private IList<APCDataReport> _dataFormats;

            public IList<APCDataReport> DataFormats
            {
                get { return _dataFormats; }
                set { _dataFormats = value; }
            }

            public HandleData(Equipment eqp, IList<APCDataReport> dataFormats, Trx trx)
            {
                _eqp = eqp;
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
