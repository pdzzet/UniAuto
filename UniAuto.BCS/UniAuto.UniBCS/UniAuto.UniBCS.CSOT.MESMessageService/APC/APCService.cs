using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public class APCService : AbstractService
    {
        private IServerAgent _apcAgent = null;
        private System.Timers.Timer timer;
        private double interval = 1 * 3600 * 1000; //20161019 sy modify
        //Dictionary<NODE, Dictionary<UnitKey, PositionName>>
        private Dictionary<string, Dictionary<string, string>> _positionForAPC = new Dictionary<string, Dictionary<string, string>>();// sy add 20160928
        //Dictionary<NODE, Dictionary<UnitKey, changeTimes:CstNo_JobNo1;CstNo_JobNo2;....>>
        private Dictionary<string, Dictionary<string, string>> _productInOutData = new Dictionary<string, Dictionary<string, string>>();// sy add 20161003
        //Dictionary<NODE, Dictionary<UnitKey, trx>>
        private Dictionary<string, Dictionary<string, Trx>> _productInOutTrx = new Dictionary<string, Dictionary<string, Trx>>();// sy add 20161003

        public Dictionary<string, Dictionary<string, string>> PositionForAPC
        {
            get { return _positionForAPC; }
            set { _positionForAPC = value; }
        }

        private Thread reportThread_APCProductInOut;// sy add 20161003

        public override bool Init()
        {
            //_apcAgent = GetServerAgent();
            reportThread_APCProductInOut = new Thread(new ThreadStart(APCProductInOut)) { IsBackground = true };// sy add 20161003
            reportThread_APCProductInOut.Start();
            return true;
        }

        /// <summary>
        /// Agent回報Service, Tibco Open成功
        /// </summary>
        public void APC_TibcoOpen()
        {
            LogInfo(MethodBase.GetCurrentMethod().Name + "()", "APC Tibrv open success.");

            foreach (Line line in ObjectManager.LineManager.GetLines())
            {
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LINENAME={0}] [BCS -> APC] APC Tibrv Open.", line.Data.LINEID));

                AreYouThereRequest(line.Data.LINEID);
            }
            timer = new System.Timers.Timer(interval);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        /// <summary>
        /// APC MessagetSet : Are You There Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void APC_AreYouThereReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineId = xmlDoc[keyHost.MESSAGE][keyHost.LINENAME].InnerText;
                string trxId = xmlDoc[keyHost.MESSAGE][keyHost.TRANSACTIONID].InnerText;
                if (!CheckAPCLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- APC] APC Reply LineName =[{1}], mismatch =[{0}].", ServerName, lineId));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void APCDataReport(string trxid, string lineName, Equipment eqp, Dictionary<string, List<string>> apcData)
        {
            try
            {
                if (ParameterManager["APC_ENABLE"].GetBoolean() == false) {
                    return;
                }
                Line line = ObjectManager.LineManager.GetLine(lineName);
                XmlDocument doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("DAILYCHECKENDFORFDC") as XmlDocument;
                if (doc == null)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> APC][{1}] DAILYCHECKENDFORFDC transaction is Not found.", lineName, trxid));
                    return;
                }

                // sy add 20160928
                 //Dictionary<string, string> positionInfo =  GetPositionForAPC(eqp);
                //ADD QIUMIN 20170811 BINGN
                Dictionary<string, string> positionInfo = new Dictionary<string, string>();
                if(eqp.Data.NODEID=="TCOVN310") 
                {
                    positionInfo=GetPositionForAPCTCOVN310(eqp);  
                }
                else 
                {
                     positionInfo =  GetPositionForAPC(eqp);
                }
                //ADD QIUMIN 20170811  END
                 if (positionInfo.Count == 0) return;
                
                XmlNode bodyNode = doc[keyHost.MESSAGE];
                bodyNode["MESSAGE_ID"].InnerText = "DAILYCHECKENDFORFDC";
                //James Add at 20150528 By New Spec
                bodyNode["TRANSACTIONID"].InnerText = trxid;
                bodyNode["SYSTEMBYTE"].InnerText = trxid;
                bodyNode["TIMESTAMP"].InnerText = GetTIMESTAMP();
                bodyNode["LINENAME"].InnerText = lineName;
                bodyNode["EQP_LIST"]["EQP"]["MACHINENAME"].InnerText = eqp.Data.NODEID;
                bodyNode["EQP_LIST"]["EQP"]["CHECK_TIME"].InnerText = "";

                XmlNode subEQPList = bodyNode["EQP_LIST"]["EQP"]["SUBEQP_LIST"];
                XmlNode subEQP = subEQPList["SUBEQP"];
                XmlNode subParaList = subEQP["PARAM_LIST"];
                XmlNode subPara = subParaList["PARAM"];
                subEQPList.RemoveAll();

                #region [Create head]
                // sy add 20160928 建立表頭
                foreach (var item in positionInfo)
                {
                    if (item.Key == "0")
                    {
                        XmlNode node = subEQP.Clone();
                        node["UNITNAME"].InnerText = item.Value;
                        node["PARAM_LIST"].RemoveAll();
                        subParaList.RemoveAll();
                        subEQPList.AppendChild(node);
                        break;
                    }
                }
                foreach (var item in positionInfo)
                {
                    if (item.Key != "0")
                    {
                        XmlNode node = subEQP.Clone();
                        node["UNITNAME"].InnerText = item.Value;
                        node["PARAM_LIST"].RemoveAll();
                        subParaList.RemoveAll();
                        subEQPList.AppendChild(node);
                    }
                }
                #endregion              
                #region [Date]
                foreach (string subEQPName in apcData.Keys)// sy add 20160928
                {
                    foreach (string item in apcData[subEQPName])
                    {

                        string[] apcPara = item.Split('=');
                        if (apcPara.Length == 2)
                        {
                            if (apcPara[1].Split(';').Length == 2)
                            {
                                string apcParaKey = apcPara[1].Split(';')[1];
                                string apcParaValue;
                                //begin add by qiumin for TCOVN310 APC report
                                if (eqp.Data.NODEID == "TCOVN310")
                                {
                                    int x, y;
                                    x = -1 * ((-1 * (int.Parse(apcParaKey) - 1) / 26) + 1);
                                    y = (-1 * int.Parse(apcParaKey)) % 26;
                                    if (y == 0) y = 26;
                                    apcParaKey = x.ToString() + '_' + y.ToString();
                                }
                                //end add by qiumin for TCOVN310 APC report
                                positionInfo.TryGetValue(apcParaKey, out apcParaValue);

                                foreach (XmlNode subUnit in subEQPList)
                                {
                                    if (apcPara[1].Split(';')[1] == "0")//Apc Eeport unit = 0 
                                    {
                                        if (subUnit["UNITNAME"].InnerText == eqp.Data.NODEID)
                                        {
                                            XmlNode subParaList1 = subUnit["PARAM_LIST"];
                                            XmlNode para = subPara.Clone();
                                            para["PARAM_NAME"].InnerText = apcPara[0];
                                            para["PARAM_VALUE"].InnerText = apcPara[1].Split(';')[0];
                                            subParaList1.AppendChild(para);
                                        }
                                    }
                                    else
                                    {
                                        if (subUnit["UNITNAME"].InnerText == apcParaValue)
                                        {
                                            XmlNode subParaList1 = subUnit["PARAM_LIST"];
                                            XmlNode para = subPara.Clone();
                                            para["PARAM_NAME"].InnerText = apcPara[0];
                                            para["PARAM_VALUE"].InnerText = apcPara[1].Split(';')[0];
                                            subParaList1.AppendChild(para);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                #region [Remove Item without Para]
                //SECS 將未上報的item(沒有para 濾掉)
                for (int i = subEQPList.ChildNodes.Count-1; i > -1; i--)
                {
                    if (subEQPList.ChildNodes[i]["PARAM_LIST"].ChildNodes.Count == 0)
                    {
                        subEQPList.RemoveChild(subEQPList.ChildNodes[i]);
                    }
                }
                #endregion

                //foreach (string subEQPName in apcData.Keys)
                //{
                //    XmlNode node = subEQP.Clone();
                //    node["UNITNAME"].InnerText = subEQPName;

                //    XmlNode subParaList = node["PARAM_LIST"];
                //    XmlNode subPara = subParaList["PARAM"];
                //    subParaList.RemoveAll();

                //    foreach (string item in apcData[subEQPName])
                //    {
                //        XmlNode para = subPara.Clone();
                //        string[] apcPara = item.Split('=');
                //        if (apcPara.Length == 2)
                //        {
                //            para["PARAM_NAME"].InnerText = apcPara[0];
                //            para["PARAM_VALUE"].InnerText = apcPara[1].Split(';')[0];// sy add 20160928
                //        }

                //        subParaList.AppendChild(para);
                //    }

                //    subEQPList.AppendChild(node);
                //}

                SendToAPC(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> APC][{1}] send DAILYCHECKENDFORFDC to APC.", lineName, trxid));
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //huangjiayin add 20180411
        public void SendToAPCForInvoke(XmlDocument _doc)
        {
            SendToAPC(_doc);
        }

        public void ProductIn(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, int position, string processtime)
        {
            try
            {
                if (ParameterManager["APC_ENABLE"].GetBoolean() == false) {
                    return;
                }
              //  string time_stamp = traceLvl != eMESTraceLevel.M ? GetTIMESTAMP() : DateTime.Now.AddSeconds(-2).ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); //20161201 sy modify recv event -2 sec
               
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                #region[for traveLvl "M" product in/out time stamp adjust] modify by yang 2017/1/8               
                string time_stamp=string.Empty;  
                int adjust; //AddSeconds(adjust)
                string key = eqp.Data.LINEID + "_" + eqp.Data.NODENO;
                if (traceLvl != eMESTraceLevel.M) time_stamp = GetTIMESTAMP();
                else if (ConstantManager[eProductInOutTimeStamp.PRODUCTINOUTTIMESTAMP][key].Value.Length != 0)
                {
                    if (int.TryParse(ConstantManager[eProductInOutTimeStamp.PRODUCTINOUTTIMESTAMP][key].Value.Substring(0,2), out adjust))
                        time_stamp = DateTime.Now.AddSeconds(adjust).ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    else
                        time_stamp = GetTIMESTAMP();
                }
                  else time_stamp = GetTIMESTAMP();
                #endregion

                // sy add 20161003 Mark By 勝杰 不需卡 HostMode
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " Send APC but OFF LINE LINENAME=[{1}].", time_stamp, lineName));
                //    return;
                //}

                //XmlDocument xml_doc = _apcAgent.GetTransactionFormat("PRODUCTIN") as XmlDocument;
                XmlDocument xml_doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("PRODUCTIN") as XmlDocument; //2016/07/8 cc.kuang
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                //Add 2016/07/07 cc.kuang
                bodyNode["MESSAGE_ID"].InnerText = "PRODUCTIN";
                //James Add at 20150528 By New Spec
                bodyNode[keyHost.TRANSACTIONID].InnerText = trxID;
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = currentEQPID;
                bodyNode[keyHost.UNITNAME].InnerText = unitID;
                bodyNode[keyHost.PORTNAME].InnerText = portid;
                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();
                // bodyNode[keyHost.JOBRECIPENAME].InnerText = job.LineRecipeName;
                #region[ Machine Product In/Out 的Recipe为Job对应eqp的PPID][add by yang 20170122]
             //   if (traceLvl != eMESTraceLevel.M)
               //     bodyNode[keyHost.JOBRECIPENAME].InnerText = eqp.File.CurrentRecipeID;//modify by yang 20161227 product in/out report current eqp recipe ID
            //    else
             //   {      //mark by yang 2017/3/10  all report job ppid
                    string subppid = string.Empty;
                    int ppididx ;
                    int ppidlen ;
                    //re-clean
                    if (eqp.Data.RECIPESEQ.Contains(","))
                    {
                        IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");

                        if (trackings.Where(s => s.Key.ToString().Contains("1st")).FirstOrDefault().Value != "0")
                        { ppididx = job.PPID.Trim().Length - 4; ppidlen = eqp.Data.RECIPELEN; }
                        else
                        { ppididx = eqp.Data.RECIPEIDX; ppidlen = eqp.Data.RECIPELEN; }
                    }
                    else
                        { ppididx = eqp.Data.RECIPEIDX; ppidlen = eqp.Data.RECIPELEN; }
                    subppid = job.PPID.Trim().Substring(ppididx, ppidlen);
                    bodyNode[keyHost.JOBRECIPENAME].InnerText = subppid;
               // }
                #endregion
                if (job.MesCstBody.LOTLIST.Count != 0)
                {
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                    bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                }
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = position.ToString();
                if (eqp.Data.NODEID == "TCOVN310")
                {
                    bodyNode[keyHost.UNITNAME].InnerText = unitID.Substring(0, 7) + position.ToString(); 
                }//TCOVN310 UNITNAME ADD POSITION
                
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID == string.Empty ? job.MesProduct.PRODUCTNAME : job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.TIMESTAMP].InnerText = time_stamp;
                bodyNode[keyHost.LASTGLASSFLAG].InnerText = job.LastGlassFlag == "1" ? "Y" : (job.APCLastGlassFlag ? "Y" : "N");// 20161019 sy modify

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductIn", eAgentName.APCAgent) == false)
                {
                    if (eqp.Data.NODEID != "TCOVN310")
                    {
                        SendToAPC(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, lineName));
                    }
                    else  //ADD BY QIUMIN 20170816
                    {
                        if ((unitID.Length == 8 && unitID.Substring(7, 1) == "1") || traceLvl == eMESTraceLevel.M) //加U 的判断是防止OVN310 M 层漏报
                        {
                            SendToAPC(xml_doc);
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, lineName));
                        }
                    }

                        
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductIn to APC.",
                        trxID, lineName, currentEQPID, unitID));
                }
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProductOut(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, int position)
        {
            try
            {
                if (ParameterManager["APC_ENABLE"].GetBoolean() == false) {
                    return;
                }
               // string time_stamp = GetTIMESTAMP();
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                #region[for traveLvl "M" product in/out time stamp adjust] modify by yang 2017/1/8
                string time_stamp = string.Empty;
                int adjust; //AddSeconds(adjust)
                string key = eqp.Data.LINEID + "_" + eqp.Data.NODENO;
                if (traceLvl != eMESTraceLevel.M) time_stamp = GetTIMESTAMP();
                else if (ConstantManager[eProductInOutTimeStamp.PRODUCTINOUTTIMESTAMP][key].Value.Length != 0)
                {
                    if (int.TryParse(ConstantManager[eProductInOutTimeStamp.PRODUCTINOUTTIMESTAMP][key].Value.Substring(2, 2), out adjust))
                        time_stamp = DateTime.Now.AddSeconds(adjust).ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    else
                        time_stamp = GetTIMESTAMP();
                }
                else time_stamp = GetTIMESTAMP();
                #endregion

                // sy add 20161003 Mark By 勝杰 不需卡 HostMode
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " Send APC but OFF LINE LINENAME=[{1}].", time_stamp, lineName));
                //    return;
                //}

                //XmlDocument xml_doc = _apcAgent.GetTransactionFormat("PRODUCTOUT") as XmlDocument;
                XmlDocument xml_doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("PRODUCTOUT") as XmlDocument;
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                //Add 2016/07/07 cc.kuang
                bodyNode["MESSAGE_ID"].InnerText = "PRODUCTOUT";
                //James Add at 20150528 By New Spec
                bodyNode[keyHost.TRANSACTIONID].InnerText = trxID;
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = currentEQPID;
                bodyNode[keyHost.UNITNAME].InnerText = unitID;
                bodyNode[keyHost.PORTNAME].InnerText = portid;
                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();
                bodyNode[keyHost.JOBRECIPENAME].InnerText = eqp.File.CurrentRecipeID; //modify by yang 20161227 product in/out report current eqp recipe ID
                #region[Product In/Out 的Recipe为Job对应eqp的PPID]]
                string subppid = string.Empty;
                int ppididx;
                int ppidlen;
                //re-clean
                if (eqp.Data.RECIPESEQ.Contains(","))
                {
                    IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");

                    if (trackings.Where(s => s.Key.ToString().Contains("1st")).FirstOrDefault().Value != "0")
                    { ppididx = job.PPID.Trim().Length - 4; ppidlen = eqp.Data.RECIPELEN; }
                    else
                    { ppididx = eqp.Data.RECIPEIDX; ppidlen = eqp.Data.RECIPELEN; }
                }
                else
                { ppididx = eqp.Data.RECIPEIDX; ppidlen = eqp.Data.RECIPELEN; }
                subppid = job.PPID.Trim().Substring(ppididx, ppidlen);
                bodyNode[keyHost.JOBRECIPENAME].InnerText = subppid;
                #endregion

                if (job.MesCstBody.LOTLIST.Count != 0)
                {
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                    bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                }
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = position.ToString();
                if (eqp.Data.NODEID == "TCOVN310")
                {
                    bodyNode[keyHost.UNITNAME].InnerText = unitID.Substring(0, 7) + position.ToString();
                }//TCOVN310 UNITNAME ADD POSITION
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID == string.Empty ? job.MesProduct.PRODUCTNAME : job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;
                bodyNode[keyHost.TIMESTAMP].InnerText = time_stamp;
                bodyNode[keyHost.LASTGLASSFLAG].InnerText = job.LastGlassFlag == "1" ? "Y" : (job.APCLastGlassFlag ? "Y" : "N");// 20161019 sy modify

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductOut", eAgentName.APCAgent) == false)
                {
                    if (eqp.Data.NODEID != "TCOVN310" ) 
                    {
                        SendToAPC(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, lineName));
                    }
                    else   //ADD BY QIUMIN 20170816
                    {
                        if ((unitID.Length==8&&unitID.Substring(7, 1) == "1") || traceLvl == eMESTraceLevel.M) //加U 的判断是防止OVN310 M 层漏报
                        {
                            SendToAPC(xml_doc);
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, lineName));
                        }

                    }

                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductOut to APC.",
                        trxID, lineName, currentEQPID, unitID));
                }
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LotProcessStart(string trxID, Port port, Cassette cst)
        {
            try
            {
                if (ParameterManager["APC_ENABLE"].GetBoolean() == false) {
                    return;
                }
                string time_stamp = GetTIMESTAMP();
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " Send APC but OFF LINE LINENAME=[{1}].", time_stamp, line.Data.LINEID));
                    return;
                }
                //XmlDocument xml_doc = _apcAgent.GetTransactionFormat("LOTPROCESSSTART") as XmlDocument;
                XmlDocument xml_doc = GetServerAgent(eAgentName.APCAgent).GetTransactionFormat("LOTPROCESSSTART") as XmlDocument;

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                //James Add at 20150528 By New Spec
                bodyNode["MESSAGE_ID"].InnerText = "LOTPROCESSSTART";
                bodyNode[keyHost.TRANSACTIONID].InnerText = trxID;
                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)port.File.Type].Value;
                //Jun Add 20150602 For POL Unloader AutoClave Report LotProcessStart
                if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3 && cst.CellBoxProcessed == eboxReport.Processing)
                    bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
                else
                    bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.TIMESTAMP].InnerText = time_stamp;

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
                lotListNode.RemoveAll();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    XmlNode lotnode = lotCloneNode.Clone();
                    Job job = null;
                    lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                    lotnode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;
                    lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].PROCESSOPERATIONNAME;
                    lotnode[keyHost.PRODUCTQUANTITY].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count.ToString();
                    lotnode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTOWNER;
                    
                    if (cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count > 0)
                        job = ObjectManager.JobManager.GetJob(cst.MES_CstData.LOTLIST[i].PRODUCTLIST[0].PRODUCTNAME.Trim());

                    if (job != null)
                    {
                        //lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                        lotnode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                        //lotnode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                        //if ((job.MES_PPID != null) && (job.MES_PPID != string.Empty))
                        //    //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                        //    lotnode[keyHost.PPID].InnerText = job.MES_PPID;  //ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.MES_PPID);
                        //else
                        //    lotnode[keyHost.PPID].InnerText = job.MesProduct.PPID;
                        //lotnode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                    }
                    else
                    {
                        //lotnode[keyHost.PRODUCTRECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                        lotnode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                        //lotnode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                        //lotnode[keyHost.PPID].InnerText = cst.MES_CstData.LOTLIST[i].PPID;
                        //lotnode[keyHost.HOSTPPID].InnerText = cst.MES_CstData.LOTLIST[i].PPID;
                    }

                    lotListNode.AppendChild(lotnode);
                }

                SendToAPC(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LotProcessEnd(string trxID, Port port, Cassette cst, IList<Job> jobs)
        {
            // (string trxID, string portNo, IList<Job> jobs)
            // 生成的XML要存檔
            try
            {
                if (ParameterManager["APC_ENABLE"].GetBoolean() == false) {
                    return;
                }
                bool _ttp = false;
                string time_stamp = GetTIMESTAMP();
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].", time_stamp, line.Data.LINEID));
                    return;
                }
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LOTPROCESSEND") as XmlDocument;

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                //James Add at 20150528 By New Spec
                bodyNode["MESSAGE_ID"].InnerText = "LOTPROCESSEND";
                bodyNode[keyHost.TRANSACTIONID].InnerText = trxID;
                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)port.File.Type].Value;
                bodyNode[keyHost.TIMESTAMP].InnerText = time_stamp;

                //bodyNode[keyHost.EXPSAMPLING].InnerText = cst.MES_CstData.EXPSAMPLING;
                //bodyNode[keyHost.AUTOCLAVESAMPLING].InnerText = cst.MES_CstData.AUTOCLAVESAMPLING;

                XmlNode glsListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode glsNode = glsListNode[keyHost.PRODUCT].Clone();
                glsListNode.RemoveAll();
                
                foreach (Job job in jobs)
                {
                    XmlNode product = glsNode.Clone();
                    product[keyHost.POSITION].InnerText = job.ToSlotNo;
                    product[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    product[keyHost.PRODUCTJUDGE].InnerText = GetProductJudge(line, fabType, job);
                    product[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
                    product[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation.PadRight(job.ChipCount).Substring(0, job.ChipCount);

                    if (line.Data.LINETYPE == "CBODF")
                        product[keyHost.PAIRPRODUCTNAME].InnerText = job.MesProduct.CFPRODUCTNAME;

                    product[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    product[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                    if (job.MesCstBody.LOTLIST.Count != 0)
                    {
                        product[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                        product[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                        product[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                        product[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                    }

                    //TODO: CSOT 胡福杰說此部份會再整理一份詳細的文件, 等文件出來再寫這塊
                    #region [Abnormal Code List]
                    XmlNode abromalListNode = product[keyHost.ABNORMALCODELIST];
                    XmlNode codeNode = abromalListNode[keyHost.CODE];
                    abromalListNode.RemoveAll();
                    XmlNode code;

                    switch (fabType)
                    {
                        case eFabType.ARRAY:
                            if (line.Data.LINETYPE == eLineType.ARRAY.TTP_VTEC && _ttp)
                            {
                                code = codeNode.Clone();
                                code[keyHost.ABNORMALSEQ].InnerText = "TTPFlag";
                                code[keyHost.ABNORMALCODE].InnerText = "Y";
                                abromalListNode.AppendChild(code);
                            }
                            break;
                        case eFabType.CF:
                            #region CF
                            string _side = string.Empty;
                            IList<Unit> Units = ObjectManager.UnitManager.GetUnits();

                            #region OVENSIDE
                                    _side = string.Empty;
                                    if (job.CfSpecial.TrackingData.Photo_OvenHP01 == "1") { _side = "OVENHP1"; }
                                    if (job.CfSpecial.TrackingData.Photo_OvenHP02 == "1") { _side = "OVENHP2"; }
                                    foreach (Unit unit in Units)
                                    {
                                        if (unit.Data.UNITATTRIBUTE == _side)
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALSEQ].InnerText = "OVENSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion

                            #region VCDSIDE
                                    _side = string.Empty;
                                    if (job.CfSpecial.TrackingData.Photo_OvenHP01 == "1") { _side = "VCD1"; }
                                    if (job.CfSpecial.TrackingData.Photo_OvenHP02 == "1") { _side = "VCD2"; }
                                    if (job.CfSpecial.TrackingData.Photo_OvenHP02 == "1") { _side = "VCD3"; }
                                    foreach (Unit unit in Units)
                                    {
                                        if (unit.Data.UNITATTRIBUTE == _side)
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALSEQ].InnerText = "VCDSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion

                            #region PRLOT
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALSEQ].InnerText = "PRLOT";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.PRLOT;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                            #region CSPNUMBER
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALSEQ].InnerText = "CSPNUMBER";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.CSPNUMBER;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                            #region HPCHAMBER
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALSEQ].InnerText = "HPCHAMBER";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.HPCHAMBER;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                            #region DISPENSESPEED
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALSEQ].InnerText = "DISPENSESPEED";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.DISPENSESPEED;
                                    abromalListNode.AppendChild(code);
                                    #endregion
                            switch (line.Data.JOBDATALINETYPE)
                            {
                                case eJobDataLineType.CF.PHOTO_BMPS:
                                    #region TTPFlag
                                    if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 && _ttp)
                                    {
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALSEQ].InnerText = "TTPFlag";
                                        code[keyHost.ABNORMALCODE].InnerText = "Y";
                                        abromalListNode.AppendChild(code);
                                    }
                                    #endregion

                                    #region ALNSIDE
                                    _side = string.Empty;
                                    if (job.CfSpecial.TrackingData.BMPS_Exposure == "1") { _side = "CP1"; }
                                    if (job.CfSpecial.TrackingData.BMPS_Exposure2 == "1") { _side = "CP2"; }
                                    foreach (Unit unit in Units)
                                    {
                                        if (unit.Data.UNITATTRIBUTE == _side)
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion

                                    break;
                                case eJobDataLineType.CF.PHOTO_GRB:
                                    #region ALNSIDE
                                    _side = string.Empty;
                                    if (job.CfSpecial.TrackingData.RGB_ExposureCP01 == "1") { _side = "CP1"; }
                                    if (job.CfSpecial.TrackingData.RGB_ExposureCP02 == "1") { _side = "CP2"; }
                                    foreach (Unit unit in Units)
                                    {
                                        if (unit.Data.UNITATTRIBUTE == _side)
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion

                                    break;
                            }
                            #endregion
                            break;
                        case eFabType.CELL:
                            // TODO: CELL
                            break;
                    }

                    // MES SPEC 修改 2014.09.28 要重寫
                    //for (int i = 0; i < job.MesProduct.ABNORMALCODELIST.CODE.Count; i++)
                    //{
                    //    if (i > 0)
                    //    {
                    //        codeNode = codeNode.Clone();
                    //        abromalListNode.AppendChild(codeNode);
                    //    }
                    //    product[keyHost.ABNORMALSEQ].InnerText = job.MesProduct.ABNORMALCODELIST.CODE[i].ABNORMALSEQ;
                    //    product[keyHost.ABNORMALCODE].InnerText = job.MesProduct.ABNORMALCODELIST.CODE[i].ABNORMALCODE;
                    //}
                    #endregion

                    if (job.HoldInforList.Count > 0)
                    {
                        product[keyHost.HOLDFLAG].InnerText = "Y";
                    }
                    
                    //目前不使用
                    XmlNode psheighList = product[keyHost.PSHEIGHTLIST];
                    XmlNode psNodeClone = psheighList[keyHost.SITEVALUE].Clone();

                    psheighList.RemoveAll();
                    //<PSHEIGHTLIST>
                    //<SITEVALUE></SITEVALUE>   //No Download
                    //</PSHEIGHTLIST>

                    product[keyHost.PROCESSRESULT].InnerText = GetPROCESSRESULT(line, port, cst, job, out _ttp);

                    glsListNode.AppendChild(product);
                }

                SendToAPC(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> APC][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                        time_stamp, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.42.	CurrentDateTime     APC MessagetSet : Current Date time  Send to to BC
        /// </summary>
        /// <param name="xmlDoc">CURRENTDATETIME XML Document</param>
        public void APC_CURRENTDATETIME(XmlDocument xmlDoc)
        {
            try
            {
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void APCProductInOut()
        {
            string decodeItemName = "";
            //long i =0;
            while (true)
            {
                try
                {
                    #region [Log on/off]//20161207 sy modify
                    bool logEnable ;
                    string logInfo= string.Empty;
                    try
                    {
                        logEnable = ParameterManager["POSITIONAPCINOUTLOG"].GetBoolean();
                    }
                    catch (Exception)
                    {
                        logEnable = false;
                    }
                    #endregion
                    if (ParameterManager["POSITIONCHECKTIME"].GetInteger() != 0) //設定0 表示不啟用
                    {
                        Thread.Sleep(ParameterManager["POSITIONCHECKTIME"].GetInteger() > 100 ? ParameterManager["POSITIONCHECKTIME"].GetInteger() : 100); //默認 最短100
                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                        foreach (Equipment eqp in eqps)
                        {
                            List<string> jobListIn = new List<string>();//20161209 sy modify  Product out 報完 再報Product In
                            List<string> jobListOut= new List<string>();//20161209 sy modify  Product out 報完 再報Product In
                            Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                            IList<PositionData> positionFormats = ObjectManager.PositionManager.GetPositionProfile(eqp.Data.NODENO);
                          //  Dictionary<string, string> positionInfos = GetPositionForAPC(eqp);//將需要上報List 取出 只判斷position & APC Report 組成list 項目
                            Dictionary<string, string> positionInfos = new Dictionary<string, string>();
                            if (eqp.Data.NODEID == "TCOVN310")
                            {
                                positionInfos = GetPositionForAPCTCOVN310(eqp);
                            }
                            else
                            {
                                positionInfos = GetPositionForAPC(eqp);
                            }
                            Dictionary<string, string> productData = new Dictionary<string, string>();
                            Dictionary<string, Trx> productInOutTrx = new Dictionary<string, Trx>();
                            _productInOutData.TryGetValue(eqp.Data.NODENO, out productData);
                            _productInOutTrx.TryGetValue(eqp.Data.NODENO, out productInOutTrx);

                            if (positionFormats == null) continue;
                            if (positionInfos.Count == 0) continue;
                            if (eqp.Data.REPORTMODE == "PLC" || eqp.Data.REPORTMODE == "PLC_HSMS")
                            {
                                if (productInOutTrx == null&&eqp.Data.NODEID != "TCOVN310") // 判斷 Trx data 是否有了                            
                                    productInOutTrx = GetPositionForTrx(line, eqp, positionInfos);
                                if (productInOutTrx == null && eqp.Data.NODEID == "TCOVN310") // 判斷 Trx data 是否有了                            
                                    productInOutTrx = GetPositionForTrxTCOVN310(line, eqp, positionInfos);

                            }   

                            foreach (string positionInfosKey in positionInfos.Keys)
                            {
                                eMESTraceLevel mESTraceLevel = eMESTraceLevel.U; // report product in out para//20161121 sy modify by 勝杰 都是U
                                string trackKey = string.Empty; //trackKey
                                string productDataNewValue = string.Empty;// new position data
                                string changeTimes = string.Empty; // empty = init , 0 = no change ,1 = change flag                                 
                                
                                string positionInfosValue = string.Empty; // report product in out para [UnitName]
                                if (!positionInfos.TryGetValue(positionInfosKey, out positionInfosValue)) break;
                                if (productData == null)
                                    productData = new Dictionary<string, string>();

                                string productDataValue = string.Empty;// wip position data ["changeTimes:CstNo_JobNo1;CstNo_JobNo2;...."]
                                productData.TryGetValue(positionInfosKey, out productDataValue);
                                if (!string.IsNullOrEmpty(productDataValue))
                                    changeTimes = productDataValue.Split(':')[0];// empty = init , 0 = no change ,1 = change flag 

                                switch (eqp.Data.REPORTMODE)
                                {
                                    case "PLC":
                                    case "PLC_HSMS":
                                       if (eqp.Data.NODEID == "TCOVN310")
                                        {
                                            #region [PLC]
                                            Trx positionTrx = new Trx();
                                            productInOutTrx.TryGetValue(positionInfosKey, out positionTrx);

                                            if (positionTrx == null) break;

                                            string strName = string.Empty;
                                            positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { positionTrx.Name, false }) as Trx;
                                            if (int.Parse(positionInfosKey.Split('_')[0]) == 0) { }//P do nothing
                                            else
                                            {
                                                if (int.Parse(positionInfosKey.Split('_')[0]) > 0)//E 
                                                {
                                                    #region [Get PLC Data]
                                                    //E1 只會有position 1 ,E2 只會有position 2......
                                                    int positionTrxIdex = (int.Parse(positionInfosKey.Split('_')[0]) - 1) * 2;
                                                    productDataNewValue = productDataNewValue + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex].Value
                                                        + "_" + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex + 1].Value;
                                                    #endregion
                                                }
                                                else//U <0
                                                {
                                                    //mESTraceLevel = eMESTraceLevel.U;
                                                    #region [Get PLC Data]
                                                    //U1 U2 .... 1 不代表position , Trx 有些不是從1 開始編
                                                    bool firstJob = true;
                                                    foreach (PositionData positionFormat in positionFormats)
                                                    {
                                                        if (positionFormat.Data.UNITTYPE != "U") continue;
                                                        if (positionFormat.Data.UNITNO.PadLeft(2, '0') == (-1 * int.Parse(positionInfosKey.Split('_')[0])).ToString().PadLeft(2, '0'))
                                                        {
                                                            if (!firstJob) productDataNewValue += ";";
                                                            firstJob = false;
                                                            int positionTrxIdex = (positionFormat.Data.POSITIONNO - 1) * 2;
                                                            productDataNewValue = productDataNewValue + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex].Value
                                                                + "_" + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex + 1].Value;
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                            trackKey = positionTrx.TrackKey;
                                            #endregion

                                        }
                                        else
                                        {
                                            #region [PLC]
                                            Trx positionTrx = new Trx();
                                            productInOutTrx.TryGetValue(positionInfosKey, out positionTrx);

                                            if (positionTrx == null) break;

                                            string strName = string.Empty;
                                            positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { positionTrx.Name, false }) as Trx;
                                            if (int.Parse(positionInfosKey) == 0) { }//P do nothing
                                            else
                                            {
                                                if (int.Parse(positionInfosKey) > 0)//E 
                                                {
                                                    #region [Get PLC Data]
                                                    //E1 只會有position 1 ,E2 只會有position 2......
                                                    int positionTrxIdex = (int.Parse(positionInfosKey) - 1) * 2;
                                                    productDataNewValue = productDataNewValue + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex].Value
                                                        + "_" + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex + 1].Value;
                                                    #endregion
                                                }
                                                else//U <0
                                                {
                                                    //mESTraceLevel = eMESTraceLevel.U;
                                                    #region [Get PLC Data]
                                                    //U1 U2 .... 1 不代表position , Trx 有些不是從1 開始編
                                                    bool firstJob = true;
                                                    foreach (PositionData positionFormat in positionFormats)
                                                    {
                                                        if (positionFormat.Data.UNITTYPE != "U") continue;
                                                        if (positionFormat.Data.UNITNO.PadLeft(2, '0') == (-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0'))
                                                        {
                                                            if (!firstJob) productDataNewValue += ";";
                                                            firstJob = false;
                                                            int positionTrxIdex = (positionFormat.Data.POSITIONNO - 1) * 2;
                                                            productDataNewValue = productDataNewValue + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex].Value
                                                                + "_" + positionTrx.EventGroups[0].Events[0].Items[positionTrxIdex + 1].Value;
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                            trackKey = positionTrx.TrackKey;
                                            #endregion
                                        }
                                           
                                        break;
                                    case "HSMS_CSOT":
                                    case "HSMS_PLC":
                                        #region [SECS]
                                        //啟動時先將position layout 補1.2.3.4.5.6 以 .....=> 0_0.0_0.0_0.0_0.0_0.0_0.....,表示
                                        //再依據機台上報更新                                        
                                        if (changeTimes == string.Empty)
                                        {
                                            #region [init]
                                            if (int.Parse(positionInfosKey) == 0) { }//P do nothing
                                            else if (int.Parse(positionInfosKey) > 0) { productDataNewValue = "0_0"; }//E1 ...E2
                                            else
                                            {
                                                //mESTraceLevel = eMESTraceLevel.U;
                                                bool firstJob = true;
                                                foreach (PositionData positionFormat in positionFormats)
                                                {
                                                    if ((-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0') == positionFormat.Data.UNITNO.PadLeft(2, '0'))
                                                    {
                                                        if (!firstJob) productDataNewValue += ";";
                                                        firstJob = false;
                                                        productDataNewValue += "0_0";
                                                    }
                                                }
                                            }
                                            #endregion                                           
                                        }
                                        else
                                        {
                                            #region [Updata]
                                            #region [Get Min Position]
                                            int minPosititon = 65535;//U 才需要用到
                                            if (int.Parse(positionInfosKey) < 0)
                                            {
                                                foreach (PositionData positionFormat in positionFormats)
                                                {
                                                    if ((-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0') == positionFormat.Data.UNITNO.PadLeft(2, '0')) //同UNITNO
                                                    {
                                                        minPosititon = Math.Min(minPosititon, positionFormat.Data.POSITIONNO);
                                                    }
                                                }
                                            }                                            
                                            #endregion
                                            List<Tuple<string, string, string>> positions = Repository.Get(string.Format("{0}_PositionInfo", eqp.Data.NODENO)) as List<Tuple<string, string, string>>;
                                            List<string> positionValues = new List<string>();
                                            foreach (string positionValue in productDataValue.Split(':')[1].Split(';'))
                                            {
                                                positionValues.Add(positionValue);
                                            }
                                            //string[] positionValues;
                                            if (positions != null && positions.Count > 0)
                                            {
                                                foreach (Tuple<string, string, string> _position in positions)//POSITIONNO = Item1;//CASSETTESEQNO = Item2;//JOBSEQNO = Item3;
                                                {
                                                    int position = 0;
                                                    if (int.Parse(positionInfosKey) == 0) { }//P do nothing
                                                    else
                                                    {
                                                        string checkKey = string.Empty;
                                                        if (int.Parse(positionInfosKey) > 0)//E1 E2 
                                                        {
                                                            #region [E]
                                                            checkKey = int.TryParse(_position.Item1, out position) ? positionInfosKey : positionInfosValue;//機台上報 是DB[positionNo]用position 是否為int 區分 //position U & E 不能重覆 不然無法區分
                                                            if (checkKey == _position.Item1) //positionInfosKey 對 E = position
                                                            {
                                                                positionValues[0] = (_position.Item2 == string.Empty ? "0" : _position.Item2) + "_" + (_position.Item3 == string.Empty ? "0" : _position.Item3);
                                                                break;//E1 只會有一個 所以 找到就可以跳出
                                                            }
                                                            #endregion
                                                        }
                                                        else //> 0)//U1[position10.position11.position12.....]  U2 
                                                        {
                                                            #region [U]
                                                            //mESTraceLevel = eMESTraceLevel.U;
                                                            foreach (PositionData positionFormat in positionFormats)
                                                            {
                                                                if ((-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0') == positionFormat.Data.UNITNO.PadLeft(2, '0')) //同UNITNO
                                                                {
                                                                    checkKey = int.TryParse(_position.Item1, out position) ? positionFormat.Data.POSITIONNO.ToString() : positionFormat.Data.POSITIONNAME;
                                                                    int layoutIdex = positionFormat.Data.POSITIONNO - minPosititon;
                                                                    if (checkKey == _position.Item1)//positionInfosKey 對 U != position
                                                                    {
                                                                        if (positionValues.Count > layoutIdex)
                                                                            positionValues[layoutIdex] = (_position.Item2 == string.Empty ? "0" : _position.Item2) + "_" + (_position.Item3 == string.Empty ? "0" : _position.Item3);
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                                }
                                            }
                                            #region [string[] => string]
                                            //重組 string[] => string  //0_0;4_6;8_121210;0_0;0_0;0_0
                                            bool firstJob = true;
                                            foreach (string positionValue in positionValues)
                                            {
                                                if (!firstJob) productDataNewValue += ";";
                                                firstJob = false;
                                                productDataNewValue += positionValue;
                                            }
                                            #endregion
                                            #endregion
                                        }
                                        trackKey = GetTIMESTAMP();
                                        #endregion
                                        break;
                                }
                                #region [CheckPositionChange]
                                if (changeTimes == string.Empty)
                                {
                                    productDataNewValue = "0:" + productDataNewValue;
                                    productData.Add(positionInfosKey, productDataNewValue);

                                    if (_productInOutData.ContainsKey(eqp.Data.NODENO))
                                        _productInOutData.Remove(eqp.Data.NODENO);
                                    _productInOutData.Add(eqp.Data.NODENO, productData);
                                }
                                else if (changeTimes == "0")
                                {
                                    if (productDataValue.Split(':')[1] != productDataNewValue) // 表示有變化
                                    {
                                        //productDataNewValue = "1:" + productDataNewValue;
                                        productDataNewValue = "1:" + productDataValue.Split(':')[1];//指標記有變化 keep 變化前數值
                                        productData.Remove(positionInfosKey);
                                        productData.Add(positionInfosKey, productDataNewValue);

                                        _productInOutData.Remove(eqp.Data.NODENO);
                                        _productInOutData.Add(eqp.Data.NODENO, productData);
                                    }
                                }
                                else if (changeTimes == "1") //Report Product in out
                                {
                                    productDataNewValue = "0:" + productDataNewValue;
                                    productData.Remove(positionInfosKey);
                                    productData.Add(positionInfosKey, productDataNewValue);

                                    _productInOutData.Remove(eqp.Data.NODENO);
                                    _productInOutData.Add(eqp.Data.NODENO, productData);
                                    int position = 0;
                                    string[] jobDataOlds = productDataValue.Split(':')[1].Split(';');
                                    string[] jobDataNews = productDataNewValue.Split(':')[1].Split(';');
                                    #region [ProductOut]
                                    foreach (string jobDataOld in jobDataOlds) //jobDataOlds 有 jobDataNews 沒有 表示ProductOut
                                    {
                                        position++;
                                        if (jobDataOld.Split('_')[0] == "0" || jobDataOld.Split('_')[1] == "0") continue;//20161124 sy modify
                                        if (!jobDataNews.Contains(jobDataOld))
                                        {
                                            if (eqp.Data.NODEID == "TCOVN310")
                                            {
                                                logInfo = string.Format("ProductOut ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey.Split('_')[0]) > 0 ? "E" + positionInfosKey.Split('_')[0].ToString() : "U" + Math.Abs(int.Parse(positionInfosKey.Split('_')[0])).ToString(),
                                                position.ToString(), productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                                jobListOut.Add(string.Format("{0},{1},{2},{3},{4},{5}", trackKey, jobDataOld, positionInfosValue, mESTraceLevel.ToString(), position.ToString(), logInfo));
                                            }
                                            else
                                            {
                                                logInfo = string.Format("ProductOut ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey) > 0 ? "E" + positionInfosKey.ToString() : "U" + Math.Abs(int.Parse(positionInfosKey)).ToString(),
                                                    position.ToString(), productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                                jobListOut.Add(string.Format("{0},{1},{2},{3},{4},{5}", trackKey, jobDataOld, positionInfosValue, mESTraceLevel.ToString(), position.ToString(), logInfo));
                                            }
                                            //Job jobOut = ObjectManager.JobManager.GetJob(jobDataOld.Split('_')[0], jobDataOld.Split('_')[1]);
                                            //if (jobOut != null)
                                            //{
                                            //    //ProductOut ProductIn(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, string processtime)
                                            //    ProductOut(trackKey, line.Data.LINEID, jobOut, eqp.Data.NODEID, "", positionInfosValue, mESTraceLevel, position);
                                            //    if (logEnable)//20161207 sy modify
                                            //    {
                                            //logInfo = string.Format("ProductOut ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey) > 0 ? "E" + positionInfosKey.ToString() : "U" + Math.Abs(int.Parse(positionInfosKey)).ToString(),
                                                //position.ToString(), productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                            //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", logInfo);
                                            //    }
                                            //}
                                            
                                        }
                                    }
                                    #endregion
                                    #region [ProductIn]
                                    position = 0;
                                    foreach (string jobDataNew in jobDataNews) //jobDataNewds 有 jobDataNews 沒有 表示ProductIn
                                    {
                                        position++;
                                        if (jobDataNew.Split('_')[0] == "0" || jobDataNew.Split('_')[1] == "0") continue;//20161124 sy modify
                                        if (!jobDataOlds.Contains(jobDataNew))
                                        {
                                            if (eqp.Data.NODEID == "TCOVN310")
                                            {
                                                logInfo = string.Format("ProductIn ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey.Split('_')[0]) > 0 ? "E" + positionInfosKey.Split('_')[0].ToString() : "U" + Math.Abs(int.Parse(positionInfosKey.Split('_')[0])).ToString(),
                                                position.ToString(), productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                                jobListIn.Add(string.Format("{0},{1},{2},{3},{4},{5}", trackKey, jobDataNew, positionInfosValue, mESTraceLevel.ToString(), position.ToString(), logInfo));

                                            }
                                            else
                                            {
                                                logInfo = string.Format("ProductIn ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey) > 0 ? "E" + positionInfosKey.ToString() : "U" + Math.Abs(int.Parse(positionInfosKey)).ToString(),
                                                    position.ToString(), productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                                jobListIn.Add(string.Format("{0},{1},{2},{3},{4},{5}", trackKey, jobDataNew, positionInfosValue, mESTraceLevel.ToString(), position.ToString(), logInfo));
                                            }
                                            //Job jobIn = ObjectManager.JobManager.GetJob(jobDataNew.Split('_')[0], jobDataNew.Split('_')[1]);
                                            //if (jobIn != null)
                                            //{
                                            //    ProductIn(trackKey, line.Data.LINEID, jobIn, eqp.Data.NODEID, "", positionInfosValue, mESTraceLevel, position, "");
                                            //    if (logEnable)
                                            //    {
                                            //        if (logEnable)//20161207 sy modify
                                            //        {
                                            //            logInfo = string.Format("ProductIn ReportNo[{0}]Position[{1}][{2}]>[{3}]", int.Parse(positionInfosKey) > 0 ? "E" + positionInfosKey.ToString() : "U" + Math.Abs(int.Parse(positionInfosKey)).ToString(),
                                            //            position.ToString(),productDataValue.Split(':')[1], productDataNewValue.Split(':')[1]);
                                            //            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", logInfo);
                                            //        }
                                            //    }
                                            //}
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            #region [ProductOut Report]
                            // 0: trackKey 
                            // 1: job
                            // 2: positionInfosValue
                            // 3: mESTraceLevel
                            // 4: position
                            // 5: logInfo
                            foreach (string item in jobListOut)//20161209 sy modify  Product out 報完 再報Product In
                            {
                                Job job = ObjectManager.JobManager.GetJob(item.Split(',')[1].Split('_')[0], item.Split(',')[1].Split('_')[1]); //1_1
                                if (job != null)
                                {
                                    ProductOut(item.Split(',')[0], line.Data.LINEID, job, eqp.Data.NODEID, "", item.Split(',')[2], eMESTraceLevel.U, int.Parse(item.Split(',')[4]));

                                    if (logEnable)
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", item.Split(',')[5]);
                                }
                            }
                            #endregion                           
                            Thread.Sleep(100);//20161209 sy modify  Product out 報完 再報Product In
                            #region [ProductIn Report]
                            foreach (string item in jobListIn)//20161209 sy modify  Product out 報完 再報Product In
                            {
                                Job job = ObjectManager.JobManager.GetJob(item.Split(',')[1].Split('_')[0], item.Split(',')[1].Split('_')[1]); //1_1
                                if (job != null)
                                {
                                    ProductIn(item.Split(',')[0], line.Data.LINEID, job, eqp.Data.NODEID, "", item.Split(',')[2], eMESTraceLevel.U, int.Parse(item.Split(',')[4]), "");

                                    if (logEnable)
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", item.Split(',')[5]);
                                }
                            }
                            #endregion                            
                        }
                    }
                    else Thread.Sleep(1000); //當設定=0                    
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM : (" + decodeItemName + "), " + ex);
                }
            }
        }

        private IServerAgent GetServerAgent()
        {
            //小心Spring的_applicationContext.GetObject(name)
            //在Init-Method與Destory-Method會與Spring的GetObject使用相同LOCK
            //需留心以免死結
            return GetServerAgent("APCAgent");
        }

        /// <summary>
        ///  APC MessageSet : BC send to PAC Are you There
        /// </summary>
        /// <param name="lineName">LineID</param>
        private void AreYouThereRequest(string lineName)
        {
            try
            {
                //程序开启的开启的时候就发AraYouThereReqeust
                //20160324 marine modify for service error log
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("FDCAREYOUTHERE") as XmlDocument;//20161019 sy modify
                
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE];
                //James Add at 20150528 By New Spec
                //20160324 marine modify for service error log
                bodyNode[keyHost.TRANSACTIONID].InnerText = CreateTrxID();//20161019 sy modify
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CURRENTTIME].InnerText = GetTIMESTAMP();//20161019 sy modify

                SendToAPC(xml_doc);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> APC] Send APC trx AreYouThereRequest.", lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 4 Hour Send AreYouThereReqeust To MES
        /// </summary>
        private void AreYouTherePeriodicityRequest()
        {
            try
            {
                //20160324 marine modify for service error log
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("FDCAREYOUTHERE") as XmlDocument;//20161019 sy modify
                //xml_doc[keyHost.MESSAGE][keyHost.LINENAME].InnerText = Workbench.ServerName;
                foreach (Line line in ObjectManager.LineManager.GetLines())
                {
                    //James Add at 20150528 By New Spec
                    //20160324 marine modify for service error log
                    xml_doc[keyHost.MESSAGE][keyHost.TRANSACTIONID].InnerText = CreateTrxID();//20161019 sy modify
                    xml_doc[keyHost.MESSAGE][keyHost.LINENAME].InnerText = line.Data.LINEID;
                    xml_doc[keyHost.MESSAGE][keyHost.CURRENTTIME].InnerText = GetTIMESTAMP();//20161019 sy modify
                    SendToAPC(xml_doc);
                    Thread.Sleep(200);
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private bool CheckAPCLineID(string lineName)
        {
            foreach (Line line in ObjectManager.LineManager.GetLines())
            {
                if (line.Data.LINEID == lineName)
                    return true;
            }
            return false;
        }

        private void SendToAPC(XmlDocument doc)
        {
            xMessage msg = new xMessage();
            msg.ToAgent = eAgentName.APCAgent;
            msg.Data = doc.OuterXml;
            PutMessage(msg);
        }

        private string GetTIMESTAMP()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        private string GetProductJudge(Line line, eFabType fabType, Job job)
        {
            try
            {
                // Array不用報, CF報在ProductGrade裡, 只有CELL需要報
                if (fabType == eFabType.CELL)
                {
                    return ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }

        private string GetProductGrade(Line line, eFabType fabType, Job job)
        {
            try
            {
                if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1 ||
                    fabType == eFabType.ARRAY || fabType == eFabType.CELL)
                {
                    return job.JobGrade;
                }
                else
                {
                    return ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }

        private string GetPROCESSRESULT(Line line, Port port, Cassette cst, Job job, out bool TTPFlag)
        {
            TTPFlag = false;
            try
            {
                eFabType fabtype;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabtype);

                switch (fabtype)
                {
                    case eFabType.ARRAY:
                        //TODO: ARRAY  新版的值會有M的部份
                        {
                            /*
                                1,PHL,nikon Line 进机台就上报Y.其它主制程Line需要看机台tracking data上报
                                   Y(normal Process),
                                   N(No process/process skip)
                                   M(abnormal process).
                                2，主制程Line 根据机台tracking data上报Y,N,M.
                                3，量测Line同CF 量测Line.
                                4,Sorter,changer同CF.
                                */
                            string trackData = "00";
                            switch (line.Data.LINETYPE)
                            {
                            case eLineType.ARRAY.CHN_SEEC:
                                if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";
                                return "N";
                            case eLineType.ARRAY.PHL_TITLE:
                            case eLineType.ARRAY.PHL_EDGEEXP:
                                if (job.SamplingSlotFlag.Equals("1") && !job.TrackingData.Equals(new string('0', 32))) return "Y";
                                return "N";
                            case eLineType.ARRAY.WET_DMS:
                            case eLineType.ARRAY.WEI_DMS:
                            case eLineType.ARRAY.STR_DMS:
                            case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                trackData = job.TrackingData.Substring(0, 2);
                                switch (System.Convert.ToInt32(trackData, 2))
                                {
                                case 1: return "Y";
                                case 2: return "M";
                                default: return "N";
                                }
                            case eLineType.ARRAY.CVD_AKT:
                            case eLineType.ARRAY.CVD_ULVAC:
                            case eLineType.ARRAY.MSP_ULVAC:
                            case eLineType.ARRAY.ITO_ULVAC:
                            case eLineType.ARRAY.DRY_ICD:
                            case eLineType.ARRAY.DRY_YAC:
                            case eLineType.ARRAY.DRY_TEL:
                                if (job.ArraySpecial.GlassFlowType == "1")
                                {
                                    //取得Cleaner的Tracking Data Value
                                    trackData = job.TrackingData.Substring(0, 2);
                                }
                                else
                                {
                                    //取得主製程設備的Tracking Data Value
                                    trackData = job.TrackingData.Substring(2, 2);
                                }

                                trackData = UtilityMethod.Reverse(trackData);
                                switch (System.Convert.ToInt32(trackData, 2))
                                {
                                case 1: return "Y";
                                case 2: return "M";
                                default: return "N";
                                }
                            case eLineType.ARRAY.TTP_VTEC:
                                {
                                    //TrackingData
                                    IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");

                                    if (trackings.Count() > 0)
                                    {
                                        if (trackings.ElementAt(0).Key.Equals("1"))
                                        {
                                            TTPFlag = true;
                                            return "Y";
                                        }
                                    }
                                    return "N";
                                }
                            default:
                                {
                                    //TrackingData
                                    IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                    foreach (string v in trackings.Values)
                                    {
                                        if (v.Equals("1") || v.Equals("2")) return "Y";
                                    }
                                    return "N";
                                }
                            }
                        }
                    case eFabType.CF:
                        //TODO: CF
                        {
                            /*
                              + 1.CF ITO Line ITO Cleaner Mode时，即时未做Sputter 制程,BC 需要上报Y.
                              + 2.Sorter ,Changer Mode:如果PORT TYPE 是UnloadingPort ,退PORT時,即全为Y.
                              + 3.量测Line: 
                                  (1)Process Flag(MES Download)为Y,tracking data为0,则process result上报为N.
                                  (2)Process Flag(MES Download)为N,tracking data为0,则Process Result上报给N.
                                  (3)Process Flag(MES Download)为N,tracking data为1,则process result上报Y.
                              + 4.Photo Line Unloader :
                                  (1)Judge Value为NG,RW的glass,上报Process Result为Y. 
                                  (2)Judge Value为OK 的glass,如果Aligner Process Flag(eqp flag)为1的glass,上报Process Result为Y.(反之 0 -> N)
                              + 5.Loader: Process Flag 为N 的glass，Process Result也为N.
                              + 6.Rework Line:如果Process Flag为Y,如果实际rework次数<CF Rework max count且>1, 则上报Process Result为M.
                                  如果CF 实际rework次数为0,则上报Process Result为N. 如果实际rework次数=CF Rework max count，则上报Y.
                              + 7.UPK Line 的 Unloader 機台一律都報Y.
                             */

                            switch (line.Data.LINETYPE)
                            {
                            case eLineType.CF.FCMPH_TYPE1:
                            case eLineType.CF.FCRPH_TYPE1:
                            case eLineType.CF.FCGPH_TYPE1:
                            case eLineType.CF.FCBPH_TYPE1:
                            case eLineType.CF.FCOPH_TYPE1:
                            case eLineType.CF.FCSPH_TYPE1:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    if (job.CfSpecial.TrackingData.TotalPitch.Equals("1")) TTPFlag = true;
                                    switch (job.JobJudge)
                                    {
                                    case "1": //OK
                                        if (job.CfSpecial.EQPFlag2.ExposureProcessFlag.Equals("1")) return "Y";
                                        break;
                                    case "2": //NG
                                    case "3": //RW
                                        return "Y";
                                    }
                                }
                                return "N";
                            /*case eLineType.CF.FBITO_TYPE1:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    if (job.CfSpecial.EQPFlag.ITOCleanerFlag.Equals("1") && job.CfSpecial.TrackingData.Sputter1.Equals("0") || job.CfSpecial.TrackingData.Sputter2.Equals("0")) return "Y";
                                }
                                return "N";
                             */ 
                            case eLineType.CF.FCREW_TYPE1:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y"))
                                    {
                                        if (int.Parse(job.CfSpecial.ReworkRealCount) > 1 && int.Parse(job.CfSpecial.ReworkRealCount) < int.Parse(job.CfSpecial.ReworkMaxCount)) return "M";
                                        else if (int.Parse(job.CfSpecial.ReworkRealCount) == int.Parse(job.CfSpecial.ReworkRealCount)) return "Y";
                                        else if (job.CfSpecial.ReworkRealCount.Equals("0")) return "N";
                                    }
                                }
                                return "N";
                            case eLineType.CF.FCUPK_TYPE1:
                            case eLineType.CF.FCUPK_TYPE2:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    if (port.File.Type == ePortType.UnloadingPort) return "Y";
                                }
                                return "N";
                            case eLineType.CF.FCREP_TYPE1:
                            case eLineType.CF.FCREP_TYPE2:
                            case eLineType.CF.FCREP_TYPE3:// 20160509 Add by Frank
                            case eLineType.CF.FCMQC_TYPE1:
                            case eLineType.CF.FCMQC_TYPE2:
                            //case eLineType.CF.FCMQC_TYPE3:
                            case eLineType.CF.FCMAC_TYPE1:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                    foreach (string v in trackings.Values)
                                    {
                                        if (job.MesProduct.PROCESSFLAG.Equals("N") && v.Equals("1")) return "Y";
                                    }
                                }
                                return "N";
                            case eLineType.CF.FCSRT_TYPE1:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                }
                                else
                                {
                                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && port.File.Type == ePortType.UnloadingPort) return "Y";
                                }
                                return "N";
                            default:
                                return "N";
                            }

                        }
                    case eFabType.CELL:
                        //TODO: CELL
                        {
                            /*刚上PORT时,全部标记为N
                                1.PMT : 从EQ Send out时,标记为Y
                                2.其它线: Store到Unload CST时标记为Y
                                3. POL Line, 若Aut Clave被Pass, 也是Process Result為N
                                */
                            switch (line.Data.LINETYPE)
                            {
                            case eLineType.CELL.CBPMT:
                                if (job.JobProcessFlows.Count > 1 && job.SamplingSlotFlag == "1")
                                    return "Y";
                                else
                                    return "N"; //Watson Add 20141225 For MES Spec

                                case eLineType.CELL.CBPOL_1:
                                case eLineType.CELL.CBPOL_2:
                                case eLineType.CELL.CBPOL_3:
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    return "N";
                                }
                                else if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    if (cst.MES_CstData.AUTOCLAVESKIP == "N")
                                        return "Y";
                                    else
                                        return "N";
                                }
                                return "N";

                            case eLineType.CELL.CBDPK:
                            case eLineType.CELL.CBDPS:
                            case eLineType.CELL.CBDPI:
                            case eLineType.CELL.CBATS:
                            case eLineType.CELL.CBSOR_1:
                            case eLineType.CELL.CBSOR_2:
                                if (job.JobProcessFlows.Count > 0 && job.SamplingSlotFlag == "1")
                                    return "Y";
                                else
                                    return "N";

                            default:
                                if (job.JobProcessFlows.Count > 1 && job.SamplingSlotFlag == "1")
                                    return "Y";
                                else
                                    return "N";
                            }
                        }
                    default: return "N";
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public Dictionary<string, string> GetPositionForAPC(Equipment eqp)// sy add 20160928
        {
            IList<APCDataReport> aPCDataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO);
            IList<PositionData> positionFormats = ObjectManager.PositionManager.GetPositionProfile(eqp.Data.NODENO);
            Dictionary<string, string> positionInfo = new Dictionary<string, string>();
            if (aPCDataFormats == null) return positionInfo;
            if (positionFormats == null)
            {
                //LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[NODENO={0}] [BCS -> APC]Position transaction is Not found.", eqp.Data.NODEID));
                if (positionInfo.Count == 0)
                {
                    positionInfo.Add("0", eqp.Data.NODEID);
                }
                return positionInfo;
            }

            if (_positionForAPC.TryGetValue(eqp.Data.NODENO, out positionInfo) && positionFormats[0].PositionForAPCFlag )// sy add 20160928 表示 positionInfo 存在 不需要重新計算
            {

            }
            else//positionInfo 需要重新計算
            {
                #region [重新計算]
                positionInfo = new Dictionary<string, string>();
                foreach (PositionData item in positionFormats)
                {
                    item.PositionForAPCFlag = true;
                    int keyUnit = 0;
                    if (item.Data.UNITTYPE == "E") keyUnit = item.Data.POSITIONNO;
                    else if (item.Data.UNITTYPE == "U") keyUnit = -1 * int.Parse(item.Data.UNITNO);
                    else keyUnit = 0;//代表UNITTYPE == "P"

                    string positionInfoValue = string.Empty;
                    if (keyUnit != 0)
                    {                        
                        if (!positionInfo.TryGetValue(keyUnit.ToString(), out positionInfoValue))
                        {
                            foreach (APCDataReport aPCDataFormat in aPCDataFormats)
                            {
                                if (aPCDataFormat.Data.REPORTUNITNO == keyUnit)
                                {
                                    if (keyUnit > 0)
                                        positionInfo.Add(keyUnit.ToString(), item.Data.POSITIONNAME);
                                    else// sy add 20161003  UNITTYPE == "U"取" "以前

                                        positionInfo.Add(keyUnit.ToString(), item.Data.POSITIONNAME.Split('#')[0]);//DB data "#" 
                                    break;
                                }
                            }
                        }
                    }

                    foreach (APCDataReport aPCDataFormat in aPCDataFormats)//判斷 APC report unit 是否有=0的 
                    {
                        if (!positionInfo.TryGetValue("0", out positionInfoValue))
                        {
                            if (aPCDataFormat.Data.REPORTUNITNO == 0)
                            {
                                positionInfo.Add("0", eqp.Data.NODEID);
                                break;
                            }
                        }
                    }
                }
                _positionForAPC.Remove(eqp.Data.NODENO);
                _positionForAPC.Add(eqp.Data.NODENO, positionInfo);
                _productInOutData.Remove(eqp.Data.NODENO);
                _productInOutTrx.Remove(eqp.Data.NODENO);//20161123 sy add reload 此表也重新計算
                #endregion
            }
            return positionInfo;        
        }

        //add qiumin for TCOVN310 APC 20170811    format positionInfo.key= -Unitno_positionno 
        public Dictionary<string, string> GetPositionForAPCTCOVN310(Equipment eqp)
        {
            IList<APCDataReport> aPCDataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO);
            IList<PositionData> positionFormats = ObjectManager.PositionManager.GetPositionProfile(eqp.Data.NODENO);
            Dictionary<string, string> positionInfo = new Dictionary<string, string>();
            if (aPCDataFormats == null) return positionInfo;
            if (positionFormats == null)
            {
                //LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[NODENO={0}] [BCS -> APC]Position transaction is Not found.", eqp.Data.NODEID));
                if (positionInfo.Count == 0)
                {
                    positionInfo.Add("0", eqp.Data.NODEID);
                }
                return positionInfo;
            }

           if (_positionForAPC.TryGetValue(eqp.Data.NODENO, out positionInfo) && positionFormats[0].PositionForAPCFlag)// sy add 20160928 表示 positionInfo 存在 不需要重新計算
            {

            }
            else//positionInfo 需要重新計算
            {
                #region [重新計算]
                positionInfo = new Dictionary<string, string>();
                foreach (PositionData item in positionFormats)
                {
                    item.PositionForAPCFlag = true;
                    string keyUnit = string.Empty;
                    keyUnit = (-1 * int.Parse(item.Data.UNITNO)).ToString() + '_' + (item.Data.POSITIONNO).ToString();

                    //format positionInfo (-Unitno_positionno,POSITIONNAME)
                    string positionInfoValue = string.Empty;
                    if (keyUnit !=null )
                    {
                        if (!positionInfo.TryGetValue(keyUnit, out positionInfoValue))
                        {
                            foreach (APCDataReport aPCDataFormat in aPCDataFormats)
                            {
                                if (-1*aPCDataFormat.Data.REPORTUNITNO ==( -1*int.Parse(keyUnit.Split('_')[0])-1)*26+ int.Parse(keyUnit.Split('_')[1]))
                                {
                                        positionInfo.Add(keyUnit, item.Data.POSITIONNAME);
                                        break;
                                }
                            }
                        }
                    }

                    foreach (APCDataReport aPCDataFormat in aPCDataFormats)//判斷 APC report unit 是否有=0的 
                    {
                        if (!positionInfo.TryGetValue("0", out positionInfoValue))
                        {
                            if (aPCDataFormat.Data.REPORTUNITNO == 0)
                            {
                                positionInfo.Add("0", eqp.Data.NODEID);
                                break;
                            }
                        }
                    }
                }
                _positionForAPC.Remove(eqp.Data.NODENO);
                _positionForAPC.Add(eqp.Data.NODENO, positionInfo);
                _productInOutData.Remove(eqp.Data.NODENO);
                _productInOutTrx.Remove(eqp.Data.NODENO);//20161123 sy add reload 此表也重新計算
                #endregion
            }
            return positionInfo;
        }
        public Dictionary<string, Trx> GetPositionForTrx(Line line ,Equipment eqp, Dictionary<string, string> positionInfos)// sy add 20161003
        {
            string strName = string.Empty;
            Trx positionTrx = new Trx();
            Dictionary<string, Trx> productInOutTrx = new Dictionary<string, Trx>();
            foreach (string positionInfosKey in positionInfos.Keys)
            {
                if (int.Parse(positionInfosKey) == 0) { }//P do nothing
                else
                {
                    if (int.Parse(positionInfosKey) > 0)//E 
                    {
                        strName = string.Format("{0}_JobEachPositionBlock", eqp.Data.NODENO);
                        positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;                        
                    }
                    else//U
                    {
                        switch (line.Data.FABTYPE)//Unit 三廠規則不一樣
                        {
                            case "ARRAY": //modify by yang for ARRAY 20161222
                                // format = {0}_JobEachSlotPositionBlock#{1},nodeno,unit.data.POSITIONPLCTRXNO
                                string unitno = (-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0');
                                Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitno.TrimStart('0')); //移除左补'0'
                                strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", eqp.Data.NODENO, unit.Data.POSITIONPLCTRXNO != "00" ? unit.Data.POSITIONPLCTRXNO : unitno);
                                positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                if (positionTrx == null)
                                {
                                    strName = string.Format("{0}_JobEachPositionBlock", eqp.Data.NODENO);
                                    positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                }
                                break;
                            default://CELL & CF
                                strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", eqp.Data.NODENO, (-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0'));
                                positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                break;
                        }
                    }

                    if (positionTrx != null)
                    {
                        if (productInOutTrx == null)
                            productInOutTrx = new Dictionary<string, Trx>();
                        productInOutTrx.Add(positionInfosKey, positionTrx);
                        
                    }
                }
            }
            if (_productInOutTrx.ContainsKey(eqp.Data.NODENO))
                _productInOutTrx.Remove(eqp.Data.NODENO);
            _productInOutTrx.Add(eqp.Data.NODENO, productInOutTrx);
            return productInOutTrx;
        }

        //add qiumin for TCOVN310 APC 20170811  unitno=positionInfosKey.Split('_')[0]
        public Dictionary<string, Trx> GetPositionForTrxTCOVN310(Line line, Equipment eqp, Dictionary<string, string> positionInfos)// sy add 20161003
        {
            string strName = string.Empty;
            Trx positionTrx = new Trx();
            Dictionary<string, Trx> productInOutTrx = new Dictionary<string, Trx>();
            foreach (string positionInfosKey in positionInfos.Keys)
            {
                if (int.Parse(positionInfosKey.Split('_')[0]) == 0) { }//P do nothing
                else
                {
                    if (int.Parse(positionInfosKey.Split('_')[0]) > 0)//E 
                    {
                        strName = string.Format("{0}_JobEachPositionBlock", eqp.Data.NODENO);
                        positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                    }
                    else//U
                    {
                        switch (line.Data.FABTYPE)//Unit 三廠規則不一樣
                        {
                            case "ARRAY": //modify by yang for ARRAY 20161222
                                // format = {0}_JobEachSlotPositionBlock#{1},nodeno,unit.data.POSITIONPLCTRXNO
                                string unitno = (-1 * int.Parse(positionInfosKey.Split('_')[0])).ToString().PadLeft(2, '0');
                                Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitno.TrimStart('0')); //移除左补'0'
                                strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", eqp.Data.NODENO, unit.Data.POSITIONPLCTRXNO != "00" ? unit.Data.POSITIONPLCTRXNO : unitno);
                                positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                if (positionTrx == null)
                                {
                                    strName = string.Format("{0}_JobEachPositionBlock", eqp.Data.NODENO);
                                    positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                }
                                break;
                            default://CELL & CF
                                strName = string.Format("{0}_JobEachSlotPositionBlock#{1}", eqp.Data.NODENO, (-1 * int.Parse(positionInfosKey)).ToString().PadLeft(2, '0'));
                                positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                                break;
                        }
                    }

                    if (positionTrx != null)
                    {
                        if (productInOutTrx == null)
                            productInOutTrx = new Dictionary<string, Trx>();
                        productInOutTrx.Add(positionInfosKey, positionTrx);

                    }
                }
            }
            if (_productInOutTrx.ContainsKey(eqp.Data.NODENO))
                _productInOutTrx.Remove(eqp.Data.NODENO);
            _productInOutTrx.Add(eqp.Data.NODENO, productInOutTrx);
            return productInOutTrx;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                AreYouTherePeriodicityRequest();
                timer.Start();
            }
            catch
            {
            }
        }
    }
}
