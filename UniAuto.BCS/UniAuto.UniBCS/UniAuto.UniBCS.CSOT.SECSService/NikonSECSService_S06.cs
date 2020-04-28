using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Collections.Generic;
using System.Linq;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.MesSpec;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class NikonSECSService
    {
        #region Recipte form equipment-S6
        public void S6F0_E_AbortTransaction(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F0_E");
            #region Handle Logic
            //TODO:Check control mode and abort setting and reply SnF0 or not.
            //TODO:Logic handle.
            #endregion
        }
        public void S6F1_E_TraceDataSend(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F1_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string trid = recvTrx["secs"]["message"]["body"]["array1"]["TRID"].InnerText.Trim();
                //check collection key
                if (!_traceData.ContainsKey(trid))
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("Can not find TRID=[{0}) in setting collection! Please confirm this TRID is request by S2F23.", trid));
                    return;
                }
                string smpln = recvTrx["secs"]["message"]["body"]["array1"]["SMPLN"].InnerText.Trim();
                string stime = recvTrx["secs"]["message"]["body"]["array1"]["STIME"].InnerText.Trim();
                XmlNode xArray2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                //check data exist
                if (!xArray2.HasChildNodes)
                {
                    _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("There are no data in this trace data send. TRID({0}),SMPLN({1}),STIME({2}).", trid, smpln, stime));
                    return;
                }
                List<Tuple<string, string, string>> setItems = new List<Tuple<string, string, string>>();
                //check collection exist
                if (!_traceData.TryGetValue(trid, out setItems) || setItems == null || setItems.Count == 0)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("There are no variable data setting. Please check \"SECS Variable Data\" has setted.", trid, smpln, stime));
                    return;
                }

                XmlNode xV = xArray2.FirstChild;
                int itemCount = 0;
                List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>(); //Tuple<name, type, value>
                while (xV != null)
                {
                    itemList.Add(Tuple.Create(setItems[itemCount].Item2, setItems[itemCount].Item3, xV.InnerText.Trim()));
                    itemCount++;
                    xV = xV.NextSibling;
                }
                  //20150710 cy:倉庫內容改為與DailyCheck一樣,修改倉庫的Key值,並判斷不再要求時,把倉庫刪除
                switch (trid)
                {
                    case "APCIM":
                        #region APC Importain
                        {
                              //*APCImportantDataRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                            //List<Tuple<string, string, string>> apcimportantDataRepository = itemList;
                              List<Tuple<string, List<Tuple<string, string, string>>>> apcimportantDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                              apcimportantDataRepository.Add(Tuple.Create(eqp.Data.NODEID, itemList));
                            //*APCImportantDataRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantData'
                            //string key = string.Format("{0}_{1}_SecsAPCImportantData", eqp.Data.LINEID, eqp.Data.NODEID);
                            string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                            Repository.Add(key, apcimportantDataRepository);
                        }
                        #endregion
                        break;
                    case "APCNO":
                        #region APC Normal
                        {
                              //*APCNormalDataRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                            //List<Tuple<string, string, string>> apcnormalDataRepository = itemList;
                              List<Tuple<string, List<Tuple<string, string, string>>>> apcnormalDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                              apcnormalDataRepository.Add(Tuple.Create(eqp.Data.NODEID, itemList));
                            //*APCNormalDataRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalData'
                            //string key = string.Format("{0}_{1}_SecsAPCNormalData", eqp.Data.LINEID, eqp.Data.NODEID);
                            string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                            Repository.Add(key, apcnormalDataRepository);
                        }
                        #endregion
                        break;
                    case "UTILY":
                        #region Daily Check (by line report)
                        {
                            //*DailyCheckRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                            List<Tuple<string, List<Tuple<string, string, string>>>> dailycheckRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                            dailycheckRepository.Add(Tuple.Create(eqp.Data.NODEID, itemList));
                            //*DailyCheckRepository key=>lineid+'_'+nodeid+'_SecsDailyCheck'
                            string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                            Repository.Add(key, dailycheckRepository);
                        }
                        #endregion
                        break;
                    case "SPCAL":
                        #region Special Data
                        {
                              //*SpecialDataRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                            //List<Tuple<string, string, string>> specialDataRepository = itemList;
                              List<Tuple<string, List<Tuple<string, string, string>>>> specialDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();
                              specialDataRepository.Add(Tuple.Create(eqp.Data.NODEID, itemList));
                            //*SpecialDataRepository key=>lineid+'_'+nodeid+'_SecsSpecialData'
                            //string key = string.Format("{0}_{1}_SecsSpecialData", eqp.Data.LINEID, eqp.Data.NODEID);
                            string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                            Repository.Add(key, specialDataRepository);
                        }
                        #endregion
                        break;
                    default:
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Unknow TRID({0}).", trid));
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F3_1_0_E_CEID1SUBCD0ProcessResultReport(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_1_0_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                //與Host為Offline，則不Keep
                //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop process data because host mode is OFFLINE.");
                //    return;
                //}
                Job job = null;
                XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (xNode != null)
                {
                    //20150311 cy:增加設定chamberID
                    if (xNode.FirstChild.Name == "array4" && job != null)
                    {
                        job.ChamberName = xNode["array4"]["CHAMBERID"].InnerText.Trim();
                        break;
                    }
                    if (xNode["DCNAME"].InnerText.Trim() == "GLASSID")
                    {
                        _common.AddRawProcessData(eqp.Data.NODENO, xNode["DCVALUE"].InnerText.Trim(), "0", recvTrx);
                        job = ObjectManager.JobManager.GetJob(xNode["DCVALUE"].InnerText.Trim());
                        //break;
                    }
                    
                    xNode = xNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F3_1_99_E_CEID1SUBCD99ProcessResultReport(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_1_99_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                //與Host為Offline，則不Keep
                //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop process data because host mode is OFFLINE.");
                //    return;
                //}
                Job job = null;
                XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (xNode != null)
                {
                    //20150311 cy:增加設定chamberID
                    if (xNode.FirstChild.Name == "array4" && job != null)
                    {
                        job.ChamberName = xNode["array4"]["CHAMBERID"].InnerText.Trim();
                        break;
                    }
                    if (xNode["DCNAME"].InnerText.Trim() == "GLASSID")
                    {
                        _common.AddRawProcessData(eqp.Data.NODENO, xNode["DCVALUE"].InnerText.Trim(), "99", recvTrx);
                        job = ObjectManager.JobManager.GetJob(xNode["DCVALUE"].InnerText.Trim());
                        //break;
                    }
                    xNode = xNode.NextSibling;
                }

                //Add By Yangzhenteng 20191017 For PHL PNK Hold Glass
                #region[PHL LINE Hold Glass Special]
                bool PHLSPECIALPROCESSDATAHOLDGLASSFLAG = ParameterManager.ContainsKey("PHLSPECIALPROCESSDATAHOLDGLASSFLAG") ? ParameterManager["PHLSPECIALPROCESSDATAHOLDGLASSFLAG"].GetBoolean() : false;
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (eqp.Data.NODEID.Contains("TCPNK") && (line.Data.LINETYPE.Contains("PHL_TITLE")||line.Data.LINETYPE.Contains("PHL_EDGEEXP")) && PHLSPECIALPROCESSDATAHOLDGLASSFLAG)
                {
                    XmlNode _xArray3 = recvTrx["secs"]["message"]["body"]["array1"]["array2"].LastChild;
                    if (_xArray3.LastChild.Name == "array4")
                    {
                        for (int i = 0; i < Convert.ToInt32(_xArray3.Attributes["len"].InnerText.Trim()); i++)
                        {
                            XmlNode _Everyvalue = _xArray3.ChildNodes[i];
                            if (_Everyvalue["DVNAME"].InnerText.Trim() == "SCAN04_EXPENERGY" && string.IsNullOrEmpty(_Everyvalue["DVVALUE"].InnerText.Trim()))
                            {
                                HoldInfo hold = null;
                                hold = new HoldInfo()
                                {
                                    NodeNo = eqp.Data.NODENO,
                                    NodeID = eqp.Data.NODEID,
                                    UnitNo = "0",
                                    UnitID = string.Empty,
                                    HoldReason = string.Format("PNK_EDGEEXP GlassID=[{0}],ProcessData [SCAN04_EXPENERGY] Is Null,Need Hold The Glass!!!",job.GlassChipMaskBlockID),
                                    OperatorID = eqp.Data.NODEID,
                                };
                                if (hold != null)
                                {
                                    ObjectManager.JobManager.HoldEventRecord(job, hold);
                                    lock (job)
                                    {
                                        job.ArraySpecial.ProcessDataNGFlag = "1";
                                    }
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", string.Format("PNK ProcessData [SCAN04_EXPENERGY]=0, BCS Hold The Glass"));
                                }
                            }
                        }
                    }
                }
                else
                { }
                #endregion


                //asir add 2014/12/26 for BC Recive EQ 【LAMP1_TIME、LAMP2_TIME、LAMP3_TIME 】report to mes use ChangeMaterialLife
                //XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"].LastChild;
                //if (xNode2 != null && Convert.ToInt32(xNode2.Attributes["len"].InnerText.Trim()) > 0)
                //{
                //    IList<ChangeMaterialLifeReport.MATERIALc> materialList = new List<ChangeMaterialLifeReport.MATERIALc>();
                //    for (int i = 0; i < Convert.ToInt32(xNode2.Attributes["len"].InnerText.Trim()); i++)
                //    {
                //        XmlNode xNodeLampTime = xNode2.ChildNodes[i];
                //        if (xNodeLampTime["DVNAME"].InnerText.Trim() == "LAMP1_TIME"
                //            || xNodeLampTime["DVNAME"].InnerText.Trim() == "LAMP2_TIME"
                //            || xNodeLampTime["DVNAME"].InnerText.Trim() == "LAMP3_TIME")
                //        {
                //            ChangeMaterialLifeReport.MATERIALc mat = new ChangeMaterialLifeReport.MATERIALc();
                //            mat.CHAMBERID = "";
                //            mat.MATERIALNAME = xNodeLampTime["DVNAME"].InnerText.Trim();
                //            mat.MATERIALTYPE = xNodeLampTime["DVTYPE"].InnerText.Trim();
                //            mat.QUANTITY = xNodeLampTime["DVVALUE"].InnerText.Trim();
                //            materialList.Add(mat);
                //        }
                //    }
                //    if (materialList.Count > 0)
                //    {
                //        object[] _data = new object[5]
                //       { 
                //           tid,   /*0  TrackKey*/
                //           eqp.Data.LINEID,      /*1  LineName*/
                //           eqp.Data.NODEID,      /*2  EQPID*/
                //          "",                   /*3  PRODUCTNAME*/
                //            materialList,         /*4  materialList*/
                //        };
                //        //呼叫MES方法
                //        Invoke(eServiceName.MESService, "ChangeMaterialLife", _data);
                //    }
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F3_2_0_E_CEID2SUBCD0ProcessResultReport(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_2_0_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                //與Host為Offline，則不Keep
                //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop lot process data because host mode is OFFLINE.");
                //    return;
                //}
                XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (xNode != null)
                {
                    if (xNode["DCNAME"].InnerText.Trim() == "LOTID")
                    {
                        string lotid = xNode["DCVALUE"].InnerText.Trim();
                        if (string.IsNullOrEmpty(lotid))
                        {
                            _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop lot process data because LOTID value is empty.");
                            return;
                        }
                        //要由Glass去取CST資料，避免CST已被退走
                        List<Job> jobs = ObjectManager.JobManager.GetJobs() as List<Job>;
                        Job job = jobs.Find(j => j.MesCstBody.LOTLIST.Any(l => l.LOTNAME == lotid));
                        if (job == null)
                        {
                            _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, 
                                string.Format( "Drop lot process data because LotID({0}) is not exist in JobEntity.", lotid));
                            return;
                        }
                        _common.AddRawLotProcessData(eqp.Data.NODENO, job.CassetteSequenceNo, "0", recvTrx);
                        break;
                    }
                    xNode = xNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F3_2_99_E_CEID2SUBCD99ProcessResultReport(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_2_99_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F4_H_ProcessResultReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                //與Host為Offline，則不Keep
                //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop lot process data because host mode is OFFLINE.");
                //    return;
                //}
                XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (xNode != null)
                {
                    if (xNode["DCNAME"].InnerText.Trim() == "LOTID")
                    {
                        string lotid = xNode["DCVALUE"].InnerText.Trim();
                        if (string.IsNullOrEmpty(lotid))
                        {
                            _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Drop lot process data because LOTID value is empty.");
                            return;
                        }
                        //要由Glass去取CST資料，避免CST已被退走
                        List<Job> jobs = ObjectManager.JobManager.GetJobs() as List<Job>;
                        Job job = jobs.Find(j => j.MesCstBody.LOTLIST.Any(l => l.LOTNAME == lotid));
                        if (job == null)
                        {
                            _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Drop lot process data because LotID({0}) is not exist in JobEntity.", lotid));
                            return;
                        }
                        _common.AddRawLotProcessData(eqp.Data.NODENO, job.CassetteSequenceNo, "99", recvTrx);
                        break;
                    }
                    xNode = xNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F5_E_MultiblockDataSendInquire(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F5_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F6_H_MultiblockGrant(eqpno, agent, tid, sysbytes, 4);
                    return;
                }
                TS6F6_H_MultiblockGrant(eqpno, agent, tid, sysbytes, 0);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1_E_CEID1EquipmentOffLine(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                #region [Control Mode - CIM Mode]
                if (eqp.File.CIMMode == eBitResult.ON)
                {
                    lock (eqp)
                        eqp.File.CIMMode = eBitResult.OFF;
                    
                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("CIM Mode Status({0}).", eqp.File.CIMMode.ToString()));
                }
                eqp.File.HSMSControlMode = "OFF-LINE";
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //20150616 cy:增加record to history的功能
                ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                //20141023 cy:Report to OPI
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //20161204 yang:CIM Mode要报给MES
                Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                #endregion
                //20150319 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                #region check opi operate and reply message
                string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                if (_timerManager.IsAliveTimer(timerId))
                {
                    _timerManager.TerminateTimer(timerId);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, "Equipment Offline.") });
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_2_E_CEID2HostOffLine(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_2_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                #region [Control Mode - CIM Mode]
                if (eqp.File.CIMMode == eBitResult.ON)
                {
                    lock (eqp)
                        eqp.File.CIMMode = eBitResult.OFF;
                    
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                }
                eqp.File.HSMSControlMode = "OFF-LINE";
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //20150616 cy:增加record to history的功能
                ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                //20141023 cy:Report to OPI
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //20161204 yang:CIM Mode要报给MES
                Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                #endregion
                //20150319 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                #region check opi operate and reply message
                string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                if (_timerManager.IsAliveTimer(timerId))
                {
                    _timerManager.TerminateTimer(timerId);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, "Host Offline.") });
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_3_E_CEID3OnLineLocal(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_3_E");
            #region Handle Logic
            try
            {
                  string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                  string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                  string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                  string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                  if (eqp == null)
                  {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                  }
                  TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                  #region [Control Mode - CIM Mode]
                  if (eqp.File.CIMMode == eBitResult.OFF)
                  {
                        lock (eqp)
                              eqp.File.CIMMode = eBitResult.ON;

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                  }
                  eqp.File.HSMSControlMode = "ON-LINE-LOCAL";
                  ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                  //20150616 cy:增加record to history的功能
                  ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                  //20141023 cy:Report to OPI
                  Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                  //20161204 yang:CIM Mode要报给MES
                  Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                  #endregion
                  //20150319 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                  #region check opi operate and reply message
                  string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                  if (_timerManager.IsAliveTimer(timerId))
                  {
                        _timerManager.TerminateTimer(timerId);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, "Online Local") });
                  }
                  #endregion

                  XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                  while (rptNode != null)
                  {
                        switch (rptNode["RPTID"].InnerText.Trim())
                        {
                              case "1002":
                                    if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                                    {
                                          string alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                                          lock (eqp)
                                                eqp.File.CurrentAlarmCode = alid;
                                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                    }
                                    break;
                              case "1007":
                                    #region [Process State - Equipemnt status]
                                    string procState = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                                    lock (eqp)
                                    {
                                          eqp.File.PreStatus = eqp.File.Status;
                                          eqp.File.Status = ConvertCsotEquipmentStatus(procState);
                                          _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                                    }
                                    #endregion
                                    break;
                              case "1008":
                                    #region [Mask Status]
                                    if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                                    {
                                          XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                          string maskSlot = string.Empty;
                                          string maskName = string.Empty;
                                          string maskState = string.Empty;
                                          List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                          while (maskNode != null)
                                          {
                                                maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                                maskName = maskNode["MASKNAME"].InnerText.Trim();
                                                maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                                masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                                maskNode = maskNode.NextSibling;
                                          }
                                          HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                                    }
                                    #endregion
                                    break;
                              case "1009":
                                    #region [Mask Buffer Slot]
                                    if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                                    {
                                          XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                          string maskSlot = string.Empty;
                                          string maskState = string.Empty;
                                          while (maskNode != null)
                                          {
                                                maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                                maskState = maskNode["MASKSLOTSTATE"].InnerText.Trim();
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                    string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                                maskNode = maskNode.NextSibling;
                                          }
                                    }
                                    #endregion
                                    break;
                              case "1010":
                                    #region [Allocation of Mask Buffer]
                                    if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                                    {
                                          XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                          string maskSlot = string.Empty;
                                          string maskState = string.Empty;
                                          while (maskNode != null)
                                          {
                                                maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                                maskState = maskNode["MASKALLOCATESTATE"].InnerText.Trim();
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                    string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                                maskNode = maskNode.NextSibling;
                                          }
                                    }
                                    #endregion
                                    break;
                        }
                        rptNode = rptNode.NextSibling;
                  }
                  // 收到狀態變化為online就對時
                  TS2F31_H_DateandTimeSetRequest(eqpno, agent, string.Empty, tid);
                  // 要求Current Recipe
                  TS1F3_H_SelectedEquipmentStatusRequest(eqpno, agent, 1019, string.Empty, tid);

                  //20160128 cy:Online後,再發S2F23設定
                  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "UTILY") != null)
                        TS2F23_H_TraceInitializeSend(eqpno, agent, "UTILY", eqp.File.UtilityIntervalNK, "-1", "1", "S2F23_APCIM", tid);
                  else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCIM") != null)
                        TS2F23_H_TraceInitializeSend(eqpno, agent, "APCIM", eqp.File.APCImportanIntervalNK, "-1", "1", "S2F23_APCNO", tid);
                  else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                        TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                  else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                        TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
            }
            catch (Exception ex)
            {
                  NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_4_E_CEID4OnLineRemote(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_4_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                #region [Control Mode - CIM Mode]
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    lock (eqp)
                        eqp.File.CIMMode = eBitResult.ON;
                    
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                }
                eqp.File.HSMSControlMode = "ON-LINE-REMOTE";
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //20150616 cy:增加record to history的功能
                ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                //20141023 cy:Report to OPI
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //20161204 yang:CIM Mode要报给MES
                Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                #endregion
                //20150319 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                #region check opi operate and reply message
                string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                if (_timerManager.IsAliveTimer(timerId))
                {
                    _timerManager.TerminateTimer(timerId);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, "Online Remote") });
                }
                #endregion

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                string alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                                lock (eqp)
                                    eqp.File.CurrentAlarmCode = alid;
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                            }
                            break;
                        case "1007":
                            #region [Process State - Equipemnt status]
                            string procState = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            lock (eqp)
                            {
                                eqp.File.PreStatus = eqp.File.Status;
                                eqp.File.Status = ConvertCsotEquipmentStatus(procState);
                                _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                            }
                            #endregion
                            break;
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskName = string.Empty;
                                string maskState = string.Empty;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    maskNode = maskNode.NextSibling;
                                }
                                HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                            }
                            #endregion
                            break;
                        case "1009":
                            #region [Mask Buffer Slot]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                        case "1010":
                            #region [Allocation of Mask Buffer]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                // 收到狀態變化為online就對時
                TS2F31_H_DateandTimeSetRequest(eqpno, agent, string.Empty, tid);
                // 要求Current Recipe
                TS1F3_H_SelectedEquipmentStatusRequest(eqpno, agent, 1019, string.Empty, tid);

                //20160128 cy:Online後,再發S2F23設定
                if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "UTILY") != null)
                      TS2F23_H_TraceInitializeSend(eqpno, agent, "UTILY", eqp.File.UtilityIntervalNK, "-1", "1", "S2F23_APCIM", tid);
                else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCIM") != null)
                      TS2F23_H_TraceInitializeSend(eqpno, agent, "APCIM", eqp.File.APCImportanIntervalNK, "-1", "1", "S2F23_APCNO", tid);
                else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                      TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                      TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_101_E_CEID101InitializeStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_101_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_102_E_CEID102InitializeEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_102_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_103_E_CEID103SetupStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_103_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_104_E_CEID104JobNotPossibleState(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_104_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_105_E_CEID105JobReadyState(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_105_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_106_E_CEID106StartTransitiontoJobNotPossibleState(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_106_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_107_E_CEID107ExposureRelatedSequenceStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_107_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_108_E_CEID108ExposureRelatedSequenceEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_108_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_109_E_CEID109PlateProcessingStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_109_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
               
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                string plateID = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                        case "3002":
                            plateID = rptNode["array4"]["STARTPLATEID"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }

                //20141215 cy add:job data內記錄使用的mask id
                #region Update Mask ID in Job
                Job job = ObjectManager.JobManager.GetJob(plateID);
                if (job == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, 
                            string.Format("Can not find Job ID({0}) in JobEntity", plateID));
                    return;
                }

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
              // wucc modify 20150826  
                //if ((line.Data.LINETYPE == "FCMPH_TYPE1") || (line.Data.LINETYPE == "FCSPH_TYPE1"))
                //{
                //      job.CfSpecial.MaskID = eqp.InUseMaskID;
                //      // eqp.
                //      //  job.CfSpecial.MaskUseCount = 
                //      //object[] _dataPrepare = new object[5]
                //      //            { 
                //      //                inputData.TrackKey,  /*0 TrackKey*/
                //      //                eqp.Data.LINEID,     /*1 LineName*/
                //      //                eqp.Data.NODEID,     /*2 EQPID*/
                //      //                materialID,          /*3 MASKNAME*/
                //      //                glassId,             /*4 PRODUCTNAME*/ 
                //      //            };
                //      ////呼叫MES方法
                //      //Invoke(eServiceName.MESService, "ValidateMaskPrepareRequest", _dataPrepare);
                //}
                //else
                    job.ArraySpecial.ExposureMaskID = eqp.InUseMaskID;
                    job.CfSpecial.MaskID = eqp.InUseMaskID;

                ObjectManager.JobManager.EnqueueSave(job);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_110_E_CEID110PlateProcessingEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_110_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_111_E_CEID111AlarmOccurred(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_111_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_112_E_CEID112AlarmCleared(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_112_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_113_E_CEID113AssistOccurred(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_113_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_114_E_CEID114AssistCleared(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_114_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_115_E_CEID115AnotherCommandStarted(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_115_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_116_E_CEID116AnotherCommandEnded(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_116_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                    return;

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_121_E_CEID121WarmingOccurred(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_121_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3012":
                            string warning = rptNode["array4"]["OCCURRENCENAME"].InnerText.Trim();
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Warning occurred name({0})", warning));
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_122_E_CEID122WarmingAcknowledged(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_122_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3013":
                            string warning = rptNode["array4"]["ACKNOWLEDGENAME"].InnerText.Trim();
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Warning acknowledged name({0})", warning));
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_201_E_CEID201AllMaskInitialized(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_201_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                //TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //需等MES回應
                bool reportMes = false;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskName = string.Empty;
                                string maskState = string.Empty;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while(maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    maskNode = maskNode.NextSibling;
                                }
                                HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                                reportMes = true;
                            }
                            #endregion
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                if(!reportMes)
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_202_E_CEID202MaskInserted(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_202_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["MASKCARRYINMASKSLOT"].InnerText.Trim();
                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Mask inserted. Mask slot({0})", slot));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_203_E_CEID203MaskRemoved(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_203_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                //TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskBarcode = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3015":
                            maskSlot = rptNode["array4"]["MASKCARRYOUTMASKSLOT"].InnerText.Trim();
                            break;
                        case "5064":
                            maskName = rptNode["array4"]["MASKCARRYOUTBARCODESTRING"].InnerText.Trim();
                            break;
                        case "3501":
                            maskBarcode = rptNode["array4"]["MASKCARRYOUTMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon 68S
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask removed. Mask slot({0})", maskSlot));
                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                masks.Add(Tuple.Create(maskSlot, maskName.Length != 0 ? maskName : maskBarcode, eMaterialStatus.DISMOUNT)); //modify by yang 2017/6/13
                HandleMaskStatus(eqp, masks, tid, true, sysbytes);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_204_E_CEID204MaskLoadedfromCase(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_204_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                
                XmlNode rptnode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"].FirstChild;
                string slot=string.Empty;
                string maskbarcode=string.Empty;
                if (rptnode != null)
                    slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEOUTMASKSLOT"].InnerText.Trim();
                rptnode = rptnode.NextSibling; //add by yang 2017/6/6 for Nikon 68S
                if (rptnode != null)
                    maskbarcode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEOUTMASKBARCODE"].InnerText.Trim();

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask loaded from case. Mask slot({0}). Mask barcode({1}).)", slot, maskbarcode));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_205_E_CEID205MaskUnloadedtoCase(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_205_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                
                XmlNode rptnode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"].FirstChild;
                string slot = string.Empty;
                string maskbarcode = string.Empty;
                if (rptnode != null)
                    slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEINMASKSLOT"].InnerText.Trim();
                rptnode = rptnode.NextSibling; //add by yang 2017/6/6 for Nikon 68S
                if (rptnode != null)
                    maskbarcode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEOUTMASKBARCODE"].InnerText.Trim();

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask unloaded to case. Mask slot({0}). Mask barcode({1}) ", slot, maskbarcode));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_206_E_CEID206IDCheckStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_206_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["IDCHECKSTARTMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("ID check start. Mask slot[{0})", slot));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_207_E_CEID207IDCheckEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_207_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                //TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //可能需等MES回應
                bool reportMes = false;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string checkState = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3019":
                            maskSlot = rptNode["array4"]["IDCHECKENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3020":
                            maskName = rptNode["array4"]["IDCHECKBARCODESTRING"].InnerText.Trim();
                            break;
                        case "3022":
                            checkState = rptNode["array4"]["IDCHECKSTATE"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("ID check end. Mask slot {0}", maskSlot));
                //20150409 cy:Nikon修改spec,增加一種checkstate
                //if (checkState == "0")
                //{
                //    List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                //    masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.MOUNT));
                //    HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                //    reportMes = true;
                //}
                //else
                //{
                //    _common.LogError( GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                //                string.Format("Mask ({0}) in slot ({1}) ID Check state is [{2}:NG]", maskName, maskSlot, checkState));
                //}
                switch (checkState)
                {
                    case "0":
                        {
                            List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                            masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.MOUNT));
                            HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                            reportMes = true;
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Mask ({0}) in slot ({1}) ID Check state is [{2}:OK]", maskName, maskSlot, checkState));
                        }
                        break;
                    case "1":
                        _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Mask ({0}) in slot ({1}) ID Check state is [{2}:NG]", maskName, maskSlot, checkState));
                        break;
                    case "2": //2表示由台車堆入,未與recipe做檢查,所以此時也是mount
                        {
                            List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                            masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.MOUNT));
                            HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                            reportMes = true;
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Mask ({0}) in slot ({1}) ID Check state is [{2}:No execution]", maskName, maskSlot, checkState));
                        }
                        break;
                }
                if(!reportMes)
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_208_E_CEID208ParticleCheckStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_208_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskBarcode = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3023":
                            maskSlot = rptNode[i0]["array4"]["PARTICLECHECKSTARTMASKSLOT"].InnerText.Trim();
                            break;
                        case "5062":
                            maskName = rptNode[i0]["array4"]["PARTICLECHECKSTARTBARCODESTRING"].InnerText.Trim();
                            break;
                        case "3504":
                            maskBarcode = rptNode[i0]["array4"]["PARTICLECHECKSTARTMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon 68S
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Particle check start. Mask slot({0}). Mask name({1}). Mask barcode({2}). ", maskSlot, maskName, maskBarcode));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_209_E_CEID209ParticleCheckEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_209_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                //TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                bool reportMes = false;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskBarcode = string.Empty;
                string maskName = string.Empty;
                string checkState = string.Empty;
                string glass = string.Empty;
                string pellicle = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3024":
                            maskSlot = rptNode["array4"]["PARTICLECHECKENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3505":
                            maskBarcode = rptNode["array4"]["PARTICLECHECKENDMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon68S
                            break;
                        case "3025":
                            glass = string.Format("{0} GlassA({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSA"].InnerText.Trim());
                            break;
                        case "3026":
                            glass = string.Format("{0} GlassB({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSB"].InnerText.Trim());
                            break;
                        case "3027":
                            glass = string.Format("{0} GlassC({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSC"].InnerText.Trim());
                            break;
                        case "3028":
                            pellicle = string.Format("{0} PellicleA({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEA"].InnerText.Trim());
                            break;
                        case "3029":
                            pellicle = string.Format("{0} PellicleB({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEB"].InnerText.Trim());
                            break;
                        case "3030":
                            pellicle = string.Format("{0} PellicleC({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEC"].InnerText.Trim());
                            break;
                        case "3031":
                            checkState = rptNode["array4"]["PARTICLECHECKSTATE"].InnerText.Trim();
                            break;
                        case "5063":
                            maskName = rptNode["array4"]["PARTICLECHECKENDBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                    string.Format("Particle check end. Mask slot({0}), Mask name({1}), Result({2})", maskSlot, maskName, (checkState == "0" ? "OK" : "NG")));
                if (checkState == "0")
                {
                    List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                    masks.Add(Tuple.Create(maskSlot, maskName.Length != 0 ? maskName : maskBarcode, eMaterialStatus.PREPARE));  //modify by yang 2017/6/13
                    HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                    reportMes = true;
                }
                else
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Mask ({0}) in slot ({1}) Particle Check state is {2}:NG. For{3}{4}", maskName, maskSlot, checkState, glass, pellicle));
                }
                if (!reportMes)
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_210_E_CEID210MaskSetupCompleted(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_210_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskBarcdoe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3032":
                            maskSlot = rptNode[i0]["array4"]["SETUPENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3506":
                            maskBarcdoe = rptNode[i0]["array4"]["SETUPENDMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon68S
                            break;
                        //case "3020":
                        //    maskName = rptNode[i0]["array4"]["IDCHECKBARCODESTRING"].InnerText.Trim();
                        //    break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask setup completed. Mask slot({0}). Mask name({1}). Mask barcode({2}).", maskSlot, maskName, maskBarcdoe));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_211_E_CEID211MaskLoadCompleted(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_211_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                //TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //要等MES回應

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskBarcode = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3033":
                            maskSlot = rptNode["array4"]["LOADMASKSLOT"].InnerText.Trim();
                            break;
                        case "5065":
                            maskName = rptNode["array4"]["LOADBARCODESTRING"].InnerText.Trim();
                            break;
                        case "3507":
                            maskBarcode = rptNode["array4"]["LOADMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon68S
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask load completed. Mask slot({0}). Mask name({1}). Mask barcode({2}).", maskSlot, maskName, maskBarcode));
                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                masks.Add(Tuple.Create(maskSlot, maskName.Length != 0 ? maskName : maskBarcode, eMaterialStatus.INUSE));  //modify by yang 2017/6/13
                HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_212_E_CEID212MaskUnloadedComplete(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_212_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskState = string.Empty;
                string maskBarcode = string.Empty;
                //for (int i0 = 0; i0 < rptNode.Count; i0++)
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3034":
                            maskSlot = rptNode["array4"]["UNLOADMASKSLOT"].InnerText.Trim();
                            break;
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while (maskNode != null)
                                {
                                    if (maskSlot == maskNode["MASKSLOT"].InnerText.Trim())
                                    {
                                        maskName = maskNode["MASKNAME"].InnerText.Trim();
                                        maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                        masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.MOUNT)); //ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                        break;
                                    }
                                    maskNode = maskNode.NextSibling;
                                }
                                if (masks.Count > 0)
                                    HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                            }
                            #endregion
                            break;
                        case "3508":
                            maskBarcode = rptNode["array4"]["UNLOADMASKBARCODE"].InnerText.Trim(); //add by yang 2017/6/6 for Nikon68S
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask unload completed. Mask slot({0}). Mask name({1}). Mask barcode({2}) ", maskSlot, maskName, maskBarcode));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_251_E_CEID251AllMaskSlotsInitialized(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_251_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskState = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "1009":
                            #region [Mask Buffer Slot]
                            if (rptNode[i0]["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNodeList maskNodes = rptNode[i0]["array4"]["array5"].ChildNodes;
                                for (int i1 = 0; i1 < maskNodes.Count; i1++)
                                {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                }
                            }
                            #endregion
                            break;
                        case "1010":
                            #region [Mask Allocation Slot]
                            if (rptNode[i0]["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNodeList maskNodes = rptNode[i0]["array4"]["array5"].ChildNodes;
                                for (int i1 = 0; i1 < maskNodes.Count; i1++)
                                {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                }
                            }
                            #endregion
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_252_E_CEID252MaskCaseInserted(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_252_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYINMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask case inserted. Mask slot[{0})", slot));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_253_E_CEID253MaskCaseRemoved(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_253_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptnodes = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"].ChildNodes;
                 string slot=string.Empty;
                 string maskbarcdoe = string.Empty;
                 if (rptnodes.Count == 1)
                     slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYOUTMASKSLOT"].InnerText.Trim();
                 else if (rptnodes.Count == 2)      //add by yang 2017/6/6 for Nikon68S
                 {
                     slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYOUTMASKSLOT"].InnerText.Trim();
                     maskbarcdoe = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYOUTMASKBARCODE"].InnerText.Trim();
                 }

                 _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                             string.Format("Mask case removed. Mask slot({0}). Mask barcode ({1})", slot, maskbarcdoe));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_254_E_CEID254EquipmentMaskUseStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_254_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                  XmlNodeList rptnodes = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"].ChildNodes;
                 string slot=string.Empty;
                 string maskbarcdoe = string.Empty;
                 if (rptnodes.Count == 1)     
                     slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["USEMASKSLOT"].InnerText.Trim();
                 else if (rptnodes.Count == 2)     //add by yang 2017/6/6 for Nikon68S
                 {
                     slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["USEMASKSLOT"].InnerText.Trim();
                     maskbarcdoe = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["USEMASKBARCODE"].InnerText.Trim();
                 }
                 _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                             string.Format("Equipment mask use start. Mask slot({0}). Mask barcode({1})", slot, maskbarcdoe));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_255_E_CEID255EquipmentMaskUseEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_255_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptnodes = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"].ChildNodes;
                string slot = string.Empty;
                string maskbarcdoe = string.Empty;
                if (rptnodes.Count == 1)
                    slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["UNUSEMASKSLOT"].InnerText.Trim();
                else if (rptnodes.Count == 2)     //add by yang 2017/6/6 for Nikon68S
                {
                    slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["UNUSEMASKSLOT"].InnerText.Trim();
                    maskbarcdoe = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["UNUSEMASKBARCODE"].InnerText.Trim();
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Equipment mask use end. Mask slot({0}). Mask barcode({1})", slot, maskbarcdoe));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_256_E_CEID256RegistertheMaskinMAT(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_256_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3039":
                        case "3520":
                            maskSlot = rptNode[i0]["array4"]["MATASSIGNEDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3040":
                        case "3521":
                            maskName = rptNode[i0]["array4"]["MATASSIGNEDMASKNAME"].InnerText.Trim();
                            break;
                        case "3041":
                        case "3522":  //add by yang for Nikon68S
                            maskName = rptNode[i0]["array4"]["MATASSIGNEDCOPYMASKID"].InnerText.Trim();
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Register the mask in MAT. Mask slot {0}. Mask name {1}. Mask copy ID {2}", maskSlot, maskName, maskID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_257_E_CEID257DeletetheMaskfromMAT(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_257_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["MATDEASSIGNEDMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Delete the mask from MAT. Mask slot({0})", slot));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_301_E_CEID301PlateLoaded(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_301_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                //for (int i0 = 0; i0 < rptNode.Count; i0++)
                while (rptNode != null) 
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3039":
                            lotID = rptNode["array4"]["PLATELOADLOTID"].InnerText.Trim();
                            break;
                        case "3040":
                            plateSlot = rptNode["array4"]["PLATELOADPLATESLOT"].InnerText.Trim();
                            break;
                        case "3041":
                            plateID = rptNode["array4"]["PLATELOADPLATEID"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                Invoke(eServiceName.JobService, "ReceiveJobDataReportForSECS", new object[4] { tid, plateID, eqp, string.Empty });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_302_E_CEID302PlateUnloaded(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_302_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                string plateState = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3042":
                            lotID = rptNode[i0]["array4"]["PLATEUNLOADLOTID"].InnerText.Trim();
                            break;
                        case "3043":
                            plateSlot = rptNode[i0]["array4"]["PLATEUNLOADPLATESLOT"].InnerText.Trim();
                            break;
                        case "3044":
                            plateID = rptNode[i0]["array4"]["PLATEUNLOADPLATEID"].InnerText.Trim();
                            break;
                        case "3530":   //add by yang 2017/6/6 for Nikon68S
                            plateState = rptNode[i0]["array4"]["PLATEUNLOADSTATE"].InnerText.Trim();
                            break;
                    }
                }
                Invoke(eServiceName.JobService, "SendOutJobDataReportForSECS", new object[4] { tid, plateID, eqp, string.Empty });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_303_E_CEID303PlateScrap(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_303_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "5066":
                        case "3531":   //add by yang 2017/6/6 for Nikon 68S
                            lotID = rptNode[i0]["array4"]["PLATESCRAPLOTID"].InnerText.Trim();
                            break;
                        case "5067":
                        case "3532":
                            plateSlot = rptNode[i0]["array4"]["PLATESCRAPPLATESLOT"].InnerText.Trim();
                            break;
                        case "5068":
                        case "3533":
                            plateID = rptNode[i0]["array4"]["PLATESCRAPPLATEID"].InnerText.Trim();
                            break;
                    }
                }
                Invoke(eServiceName.JobService, "RemoveJobDataReportForSECS", new object[] { tid, plateID, eqp, "1", string.Empty, string.Empty, string.Empty, string.Empty });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_311_E_CEID311LotProcessingStart(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_311_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3045":
                            string lot = rptNode["array4"]["LOTSTARTLOTID"].InnerText.Trim();
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Lot processing start. LotID({0})", lot));
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_312_E_CEID312LotProcessEnd(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_312_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3046":
                                log = string.Format("{0} LotID({1})", log, rptNode["array4"]["LOTENDLOTID"].InnerText.Trim());
                            break;
                        case "3047":
                            if (rptNode["array4"]["LOTENDSTATE"].InnerText.Trim() == "0")
                                log = string.Format("{0} State(0:OK)", log);
                            else
                                log = string.Format("{0} State(1:NG)", log);
                            break;
                        case "3080":
                            log = string.Format("{0} CarryInNumber({1})", log, rptNode["array4"]["LOTENDCARRYINNUMBER"].InnerText.Trim());
                            break;
                        case "3081":
                            log = string.Format("{0} CarryOutNumber({1})", log, rptNode["array4"]["LOTENDCARRYOUTNUMBER"].InnerText.Trim());
                            break;
                        case "3082":
                            log = string.Format("{0} NormalProcessNumber({1})", log, rptNode["array4"]["LOTENDNORMALPROCESSNUMBER"].InnerText.Trim());
                            break;
                        case "3083":
                            log = string.Format("{0} AbnormalProcessNumber({1})", log, rptNode["array4"]["LOTENDABNORMALPROCESSNUMBER"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Lot processing end.{0}", log));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_401_E_CEID401JobLoaded(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_401_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string recipe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3048":
                            string state = rptNode[i0]["array4"]["JOBENTRYCATEGORY"].InnerText.Trim();
                            data = string.Format("{0} Category({1}:{2})", data, state, ConvertJobEntryCategory(state));
                            break;
                        case "3049":
                            data = string.Format("{0} LotID({1})", data, rptNode[i0]["array4"]["JOBENTRYLOTID"].InnerText.Trim());
                            break;
                        case "3050":
                            data = string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["JOBENTRYRECIPENAME"].InnerText.Trim());
                            break;
                        case "3051":
                            //if (eqp.File.CurrentRecipeID != rptNode[i0]["array4"]["JOBENTRYRECIPEID"].InnerText.Trim())
                            //{
                            //eqp.File.CurrentRecipeID = rptNode[i0]["array4"]["JOBENTRYRECIPEID"].InnerText.Trim();
                            //      ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            //      ObjectManager.EquipmentManager.RecordEquipmentHistory(eqp);
                            //}
                            data = string.Format("{0} RecipeID({1})", data, eqp.File.CurrentRecipeID);
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Job Loaded.{0}", data));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_402_E_CEID402JobCanceled(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_402_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string recipe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3053":
                            string state = rptNode[i0]["array4"]["JOBCANCELCATEGORY"].InnerText.Trim();
                            data = string.Format("{0} Category({1}:{2})", data, state, ConvertJobCancelCategory(state));
                            break;
                        case "3054":
                            data = string.Format("{0} LotID({1})", data, rptNode[i0]["array4"]["JOBCANCELLOTID"].InnerText.Trim());
                            break;
                        case "3055":
                            data = string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["JOBCANCELRECIPENAME"].InnerText.Trim());
                            break;
                        case "3056":
                            data = string.Format("{0} RecipeID({1})", data, rptNode[i0]["array4"]["JOBCANCELRECIPEID"].InnerText.Trim());
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Job Canceled.{0}", data));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_403_E_CEID403OperatorJobManipulation(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_403_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3058":
                            string state =rptNode["array4"]["JOBOPERATESTATE"].InnerText.Trim();
                            log = string.Format("{0} State({1})", log, state, ConvertJobOperateState(state));
                            break;
                        case "3059":
                            log = string.Format("{0} LotID({1})", log, rptNode["array4"]["JOBOPERATELOTID"].InnerText.Trim());
                            break;
                        case "3060":
                            log = string.Format("{0} RecipeName({1})", log, rptNode["array4"]["JOBOPERATERECIPENAME"].InnerText.Trim());
                            break;
                        case "3061":
                            log = string.Format("{0} RecipeID({1})", log, rptNode["array4"]["JOBOPERATERECIPEID"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Operator Job(Lot) manipulation.{0}", log));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_404_E_CEID404JobProcessingConditionEstablished(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_404_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3063":
                            log = string.Format("{0} LotID({1})", log, rptNode["array4"]["JOBCONDITIONLOTID"].InnerText.Trim());
                            break;
                        case "3064":
                            log = string.Format("{0} RecipeName({1})", log, rptNode["array4"]["JOBCONDITIONRECIPENAME"].InnerText.Trim());
                            break;
                        case "3065":
                            log = string.Format("{0} RecipeID({1})", log, rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim());
                            if (eqp.File.CurrentRecipeID != rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim())
                            {
                                  eqp.File.CurrentRecipeID = rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim();
                                  ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                  ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                            }
                            break;
                        case "3066":
                            log = string.Format("{0} MaskName({1})", log, rptNode["array4"]["JOBCONDITIONMASKNAME"].InnerText.Trim());
                            break;
                        case "3067":
                            log = string.Format("{0} CopyMaskID({1})", log, rptNode["array4"]["JOBCONDITIONCOPYMASKID"].InnerText.Trim());
                            break;
                        case "3068":
                            log = string.Format("{0} MaskSlot({1})", log, rptNode["array4"]["JOBCONDITIONMASKSLOT"].InnerText.Trim());
                            break;
                        case "3069":
                            {
                                string state = rptNode["array4"]["JOBCONDITIONALIGNMENTMETHOD"].InnerText.Trim();
                                log = string.Format("{0} AlignmentMethod({1}:{2})", log, state, ConvertJobConditionAlignmentMethod(state));
                            }
                            break;
                        case "3070":
                            log = string.Format("{0} ExposueControl({1})", log, rptNode["array4"]["JOBCONDITIONEXPOSUECONTROL"].InnerText.Trim());
                            break;
                        case "3071":
                            log = string.Format("{0} Dose({1})", log, rptNode["array4"]["JOBCONDITIONDOSE"].InnerText.Trim());
                            break;
                        case "3072":
                            log = string.Format("{0} ScanSpeed({1})", log, rptNode["array4"]["JOBCONDITIONSCANSPEED"].InnerText.Trim());
                            break;
                        case "3073":
                            log = string.Format("{0} Focus({1})", log, rptNode["array4"]["JOBCONDITIONFOCUS"].InnerText.Trim());
                            break;
                        case "3074":
                            {
                                string state = rptNode["array4"]["JOBCONDITIONEGAFIXMODE"].InnerText.Trim();
                                log = string.Format("{0} EGAFixMode({1}:{2})", log, state, ConvertJobConditionEGAFixMode(state));
                            }
                            break;
                        case "3075":
                            log = string.Format("{0} PlateShiftX({1})", log, rptNode["array4"]["JOBCONDITIONPLATESHIFTX"].InnerText.Trim());
                            break;
                        case "3076":
                            log = string.Format("{0} PlateShiftY({1})", log, rptNode["array4"]["JOBCONDITIONPLATESHIFTY"].InnerText.Trim());
                            break;
                        case "3077":
                            log = string.Format("{0} PlateScaleX({1})", log, rptNode["array4"]["JOBCONDITIONPLATESCALEX"].InnerText.Trim());
                            break;
                        case "3078":
                            log = string.Format("{0} PlateScaleY({1})", log, rptNode["array4"]["JOBCONDITIONPLATESCALEY"].InnerText.Trim());
                            break;
                        case "3079":
                            log = string.Format("{0} PlateOrthogonality({1})", log, rptNode["array4"]["JOBCONDITIONPLATEORTHOGONALITY"].InnerText.Trim());
                            break;
                        case "3080":
                            log = string.Format("{0} PlateRotation({1})", log, rptNode["array4"]["JOBCONDITIONPLATEROTATION"].InnerText.Trim());
                            break;
                        case "3081":
                            if (rptNode["array4"]["JOBCONDITIONILLMINATION"].InnerText.Trim() == "2")
                                log = string.Format("{0} Illmination(2:Execute)", log);
                            else
                                log = string.Format("{0} Illmination(1:Not executed)", log);
                            break;
                        case "3082":
                            if (rptNode["array4"]["JOBCONDITIONMASKROTATION"].InnerText.Trim() == "2")
                                log = string.Format("{0} MaskRotation(2:Execute)", log);
                            else
                                log = string.Format("{0} MaskRotation(1:Not executed)", log);
                            break;
                        case "3083":
                            if (rptNode["array4"]["JOBCONDITIONLENSFOCUS"].InnerText.Trim() == "2")
                                log = string.Format("{0} LensFocus(2:Execute)", log);
                            else
                                log = string.Format("{0} LensFocus(1:Not executed)", log);
                            break;
                        case "3084":
                            if(rptNode["array4"]["JOBCONDITIONLENSBASELINE"].InnerText.Trim() == "2")
                                log = string.Format("{0} LensBaseline(2:Execute)", log);
                            else
                                log = string.Format("{0} LensBaseline(1:Not executed)", log);
                            break;
                        case "3085":
                            if (rptNode["array4"]["JOBCONDITIONMASKSCALING"].InnerText.Trim() == "2")
                                log = string.Format("{0} MaskScaling(2:Execute)", log);
                            else
                                log = string.Format("{0} MaskScaling(1:Not executed)", log);
                            break;
                        #region [T3 Add Items]
                        //2015/9/23 add by Frank
                        case "3340":
                            log = string.Format("{0} JobConditionPSStraightnessPSX({1})", log, rptNode["array4"]["JOBCONDITIONPSSTRAIGHTNESSPSX"].InnerText.Trim());
                            break;
                        case "3341":
                            log = string.Format("{0} JobConditionPSStraightnessPSY({1})", log, rptNode["array4"]["JOBCONDITIONPSSTRAIGHTNESSPSY"].InnerText.Trim());
                            break;
                        case "3342":
                            log = string.Format("{0} JobConditionLensStitching({1})", log, rptNode["array4"]["JOBCONDITIONLENSSTITCHING"].InnerText.Trim());
                            break;
                        case "3343":
                            log = string.Format("{0} JobConditionLensResultCheck({1})", log, rptNode["array4"]["JOBCONDITIONLENSRESULTCHECK"].InnerText.Trim());
                            break;
                        case "3344":
                            log = string.Format("{0} JobConditionMaskShape({1})", log, rptNode["array4"]["JOBCONDITIONMASKSHAPE"].InnerText.Trim());
                            break;
                        case "3345":
                            log = string.Format("{0} JobConditionMSYStraightness({1})", log, rptNode["array4"]["JOBCONDITIONMSYSTRAIGHTNESS"].InnerText.Trim());
                            break;
                        #endregion
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Job(Lot) processing condition established.{0}", log));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1201_E_CEID1201RecipeControlChanged(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1201_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string ppevent = string.Empty;
                string recipeid = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3269":
                                string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["PPCHANGENAME"].InnerText.Trim());
                            break;
                        case "3270":
                            ppevent = rptNode[i0]["array4"]["PPCHANGESTATE"].InnerText.Trim();
                            string.Format("{0} Status({1}:{2})", data, ppevent, ConvertPPChangeState(ppevent));
                            break;
                        case "5061":
                        case "3561":  //3561 add for Nikon 68S by yang
                            recipeid = rptNode[i0]["array4"]["PPCHANGERECIPEID"].InnerText.Trim();
                            string.Format("{0} RecipeID({1})", data, recipeid);
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Recipe Control Changed.{0}", data));
                //20141119 cy:Request recipe parameter and report mes to check when change state is not 3:Delete.
                if (ppevent != "3")
                {
                    RecipeCheckInfo rci = new RecipeCheckInfo(eqpno, 0, 0, recipeid);
                    rci.TrxId = tid;
                    eRecipeCheckResult result = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterChangeRequest", new object[] { tid, rci, eqp.Data.NODEID, eqp.Data.LINEID });
                    if (result == eRecipeCheckResult.NG)
                    {
                        TS10F3_H_TerminalDisplaySingle(eqpno, eqp.Data.NODEID, "Recipe Rarameter Change MES Reply NG", tid, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1202_E_CEID1202MachineConstantChanged(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1202_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Machine constant changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1203_E_CEID1203SystemParameterChanged(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1203_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "System parameter changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1204_E_CEID1204MachineAdjustmentParameterChanged(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1204_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Machine agjustment parameter changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1301_E_CEID1301ConfirmationNoticeofTerminalServiceMessage(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1301_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3271":
                                log = string.Format("{0} TerminalID({1})", log, rptNode["array4"]["TERMINALID"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Confirmation notice of terminal service message.{0}", log));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F11_1302_E_CEID1302HostRequestCommandErrorNotice(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_1302_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (node == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                    return;
                }
                TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/23 Modify by Frank
                        case "3272":
                                log = string.Format("{0} ErrorCode({1})", log, rptNode["array4"]["ERRORCODE"].InnerText.Trim());
                            break;
                        case "3273":
                            log = string.Format("{0} ErrorDetail1({1})", log, rptNode["array4"]["ERRORDETAIL1"].InnerText.Trim());
                            break;
                        case "3274":
                            log = string.Format("{0} ErrorDetail2({1})", log, rptNode["array4"]["ERRORDETAIL2"].InnerText.Trim());
                            break;
                        case "3275":
                            log = string.Format("{0} ErrorDetail3({1})", log, rptNode["array4"]["ERRORDETAIL3"].InnerText.Trim());
                            break;
                        case "3276":
                            log = string.Format("{0} ErrorDetail4({1})", log, rptNode["array4"]["ERRORDETAIL4"].InnerText.Trim());
                            break;
                        case "3277":
                            log = string.Format("{0} ErrorDetail5({1})", log, rptNode["array4"]["ERRORDETAIL5"].InnerText.Trim());
                            break;
                        case "3278":
                            log = string.Format("{0} ErrorDetail6({1})", log, rptNode["array4"]["ERRORDETAIL6"].InnerText.Trim());
                            break;
                        case "3279":
                            log = string.Format("{0} ErrorText({1})", log, rptNode["array4"]["ERRORTEXT"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Host request command error notice.{0}", log));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1_E_CEID1EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1", string.Format("Clock:{0}", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CLOCK"].InnerText.Trim()) });
                        break;
                }
                //20150414 cy:Request回來的資料,不見得已經offline
                #region [Control Mode - CIM Mode]
                //if (eqp.File.CIMMode == eBitResult.ON)
                //{
                //    lock (eqp)
                //        eqp.File.CIMMode = eBitResult.OFF;
                //    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //    //20141023 cy:Report to OPI
                //    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                //}
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_2_E_CEID2EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_2_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_2", string.Format("Clock:{0}", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CLOCK"].InnerText.Trim()) });
                        break;
                }
                //20150414 cy:Request回來的資料,不見得已經offline
                #region [Control Mode - CIM Mode]
                //if (eqp.File.CIMMode == eBitResult.ON)
                //{
                //    lock (eqpno)
                //        eqp.File.CIMMode = eBitResult.OFF;
                //    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //    //20141023 cy:Report to OPI
                //    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                //}
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_3_E_CEID3EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_3_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                //20150414 cy:Request回來的資料,不見得已經online
                #region [Control Mode - CIM Mode]
                //if (eqp.File.CIMMode == eBitResult.OFF)
                //{
                //    lock (eqp)
                //        eqp.File.CIMMode = eBitResult.ON;
                //    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //    //20141023 cy:Report to OPI
                //    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                //}
                #endregion
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            #region [Process State - Equipemnt status]
                            string procState = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            lock (eqp)
                            {
                                eqp.File.PreStatus = eqp.File.Status;
                                eqp.File.Status = ConvertCsotEquipmentStatus(procState);
                                _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                            }
                            #endregion
                            break;
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskName = string.Empty;
                                string maskState = string.Empty;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while(maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    maskNode = maskNode.NextSibling;
                                }
                                HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                            }
                            #endregion
                            break;
                        case "1009":
                            #region [Mask Buffer Slot]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                        case "1010":
                            #region [Allocation of Mask Buffer]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_3", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }
                //// 收到狀態變化為online就對時
                //TS2F31_H_DateandTimeSetRequest(eqpno, agent, string.Empty, tid);
                //// 要求Current Recipe
                //TS1F3_H_SelectedEquipmentStatusRequest(eqpno, agent, 1019, string.Empty, tid);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_4_E_CEID4EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_4_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                //20150414 cy:Request回來的資料,不見得已經online
                #region [Control Mode - CIM Mode]
                //if (eqp.File.CIMMode == eBitResult.OFF)
                //{
                //    lock (eqp)
                //        eqp.File.CIMMode = eBitResult.ON;
                //    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //    //20141023 cy:Report to OPI
                //    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                //}
                #endregion
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            #region [Process State - Equipemnt status]
                            string procState = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();//rptNode[i0]["array4"]["PROCESSSTATE"].InnerText.Trim();
                            lock (eqp)
                            {
                                eqp.File.PreStatus = eqp.File.Status;
                                eqp.File.Status = ConvertCsotEquipmentStatus(procState);
                                _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                            }
                            #endregion
                            break;
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskName = string.Empty;
                                string maskState = string.Empty;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();//maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();//maskNodes[i1]["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();//maskNodes[i1]["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    maskNode = maskNode.NextSibling;
                                }
                                HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                            }
                            #endregion
                            break;
                        case "1009":
                            #region [Mask Buffer Slot]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                        case "1010":
                            #region [Allocation of Mask Buffer]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskState = string.Empty;
                                while (maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNode["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", eqp.Data.NODENO, tid, maskSlot, ConvertMaskAllocateState(maskState)));
                                    maskNode = maskNode.NextSibling;
                                }
                            }
                            #endregion
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_4", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }
                //// 收到狀態變化為online就對時
                //TS2F31_H_DateandTimeSetRequest(eqpno, agent, string.Empty, tid);
                //// 要求Current Recipe
                //TS1F3_H_SelectedEquipmentStatusRequest(eqpno, agent, 1019, string.Empty, tid);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_101_E_CEID101EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_101_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if(rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_101", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_101", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_102_E_CEID102EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_102_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_102", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_102", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_103_E_CEID103EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_103_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if(rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_103", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_103", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_104_E_CEID104EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_104_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if(rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_104", "No Data" });
                    return;
                }
                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_104", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_105_E_CEID105EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_105_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_105", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_105", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_106_E_CEID106EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_106_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_106", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_106", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_107_E_CEID107EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_107_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_107", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_107", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_108_E_CEID108EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_108_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_108", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_108", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_109_E_CEID109EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_109_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_109", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_109", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_110_E_CEID110EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_110_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_110", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_110", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_111_E_CEID111EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_111_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_111", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_111", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_112_E_CEID112EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_112_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_112", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_112", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_113_E_CEID113EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_113_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_113", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_113", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_114_E_CEID114EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_114_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_114", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_114", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_115_E_CEID115EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_115_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_115", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_115", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_116_E_CEID116EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_116_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes.Count == 0)
                {
                    if (rtn.Equals("OPI"))
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_116", "No Data" });
                    return;
                }

                string state = string.Empty;
                string alid = string.Empty;
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1007":
                            state = rptNode["array4"]["PROCESSSTATE"].InnerText.Trim();
                            break;
                        case "1002":
                            if (rptNode["array4"]["array5"].ChildNodes.Count > 0)
                            {
                                alid = rptNode["array4"]["array5"].FirstChild.InnerText.Trim();
                            }
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                lock (eqp)
                {
                    eqp.File.PreStatus = eqp.File.Status;
                    eqp.File.Status = ConvertCsotEquipmentStatus(state);
                    eqp.File.PreMesStatus = eqp.File.MESStatus;
                    eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                    eqp.File.CurrentAlarmCode = alid;
                    _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                }
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_116", string.Format("Equipment status({0})", eqp.File.Status) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_121_E_CEID121EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_121_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string warning = "No Data";
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3012":
                            warning = rptNode["array4"]["OCCURRENCENAME"].InnerText.Trim();
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Warning occurred name({0})", warning));
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_121", string.Format("Warning occurred name({0})", warning) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_122_E_CEID122EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_122_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string warning = "No Data";
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3013":
                            warning = rptNode["array4"]["ACKNOWLEDGENAME"].InnerText.Trim();
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Warning acknowledged name({0})", warning));
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_122", string.Format("Warning acknowledged name({0})", warning) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_201_E_CEID201EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_201_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string msg = "No Data";
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "1008":
                            #region [Mask Status]
                            if (rptNode["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNode maskNode = rptNode["array4"]["array5"].FirstChild;
                                string maskSlot = string.Empty;
                                string maskName = string.Empty;
                                string maskState = string.Empty;
                                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                                while(maskNode != null)
                                {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    msg = msg + string.Format("Mask slot-name-state({0}-{1}-{2}); ", maskSlot, maskName, maskState);
                                    maskNode = maskNode.NextSibling;
                                }
                                HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                            }
                            #endregion
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_201", msg });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_202_E_CEID202EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_202_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["MASKCARRYINMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask inserted. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_202", string.Format("Mask carry in slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_203_E_CEID203EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_203_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3015":
                            maskSlot = rptNode["array4"]["MASKCARRYOUTMASKSLOT"].InnerText.Trim();
                            break;
                        case "5064":
                            maskName = rptNode["array4"]["MASKCARRYOUTBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask removed. Mask slot({0})", maskSlot));
                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.DISMOUNT));
                HandleMaskStatus(eqp, masks, tid, false, string.Empty);

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_203", string.Format("Mask carry out slot({0}), name({1})", maskSlot, maskName) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_204_E_CEID204EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_204_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEOUTMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask loaded from case. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_204", string.Format("Mask loaded from case. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_205_E_CEID205EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_205_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASEINMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask unloaded to case. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_205", string.Format("Mask unloaded to case. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_206_E_CEID206EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_206_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["IDCHECKSTARTMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("ID check start. Mask slot({0})", slot));

                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_206", string.Format("ID check start. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_207_E_CEID207EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_207_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string checkState = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3019":
                            maskSlot = rptNode["array4"]["IDCHECKENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3020":
                            maskName = rptNode["array4"]["IDCHECKBARCODESTRING"].InnerText.Trim();
                            break;
                        case "3022":
                            checkState = rptNode["array4"]["IDCHECKSTATE"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("ID check end. Mask slot ({0})", maskSlot));
                if (checkState == "0")
                {
                    List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                    masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.MOUNT));
                    HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                }
                else
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",eqpno, true, tid,
                                string.Format("Mask ({0}) in slot ({1}) ID Check state is {2}:NG", maskName, maskSlot, checkState));
                }

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_207", string.Format("ID check end. Mask slot({0}) Check state({1})", maskSlot, (checkState == "0" ? "OK" : "NG")) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_208_E_CEID208EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_208_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3023":
                            maskSlot = rptNode[i0]["array4"]["PARTICLECHECKSTARTMASKSLOT"].InnerText.Trim();
                            break;
                        case "5062":
                            maskName = rptNode[i0]["array4"]["PARTICLECHECKSTARTBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Particle check start. Mask slot({0}). Mask name({1})", maskSlot, maskName));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_208", string.Format("Particle check start. Mask slot({0}). Mask name({1})", maskSlot, maskName) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_209_E_CEID209EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_209_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string checkState = string.Empty;
                string glass = string.Empty;
                string pellicle = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3024":
                            maskSlot = rptNode["array4"]["PARTICLECHECKENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3025":
                            glass = string.Format("{0} GlassA({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSA"].InnerText.Trim());
                            break;
                        case "3026":
                            glass = string.Format("{0} GlassB({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSB"].InnerText.Trim());
                            break;
                        case "3027":
                            glass = string.Format("{0} GlassC({1})", glass, rptNode["array4"]["PARTICLECHECKGLASSC"].InnerText.Trim());
                            break;
                        case "3028":
                            pellicle = string.Format("{0} PellicleA({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEA"].InnerText.Trim());
                            break;
                        case "3029":
                            pellicle = string.Format("{0} PellicleB({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEB"].InnerText.Trim());
                            break;
                        case "3030":
                            pellicle = string.Format("{0} PellicleC({1})", pellicle, rptNode["array4"]["PARTICLECHECKPELLICLEC"].InnerText.Trim());
                            break;
                        case "3031":
                            checkState = rptNode["array4"]["PARTICLECHECKSTATE"].InnerText.Trim();
                            break;
                        case "5063":
                            maskName = rptNode["array4"]["PARTICLECHECKENDBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                    string.Format("Particle check end. Mask slot({0}). Mask name({1}). Result({2})", maskSlot, maskName, (checkState == "0" ? "OK" : "NG")));
                if (checkState == "0")
                {
                    List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                    masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.PREPARE));
                    HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                }
                else
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Mask ({0}) in slot ({1}) Particle Check state is {2}:NG. For{3}{4}", maskName, maskSlot, checkState, glass, pellicle));
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_209", string.Format("Particle check end. Mask slot({0}). Mask name({1}). Result({2})", maskSlot, maskName, (checkState == "0" ? "OK" : "NG")) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_210_E_CEID210EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_210_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3032":
                            maskSlot = rptNode[i0]["array4"]["SETUPENDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3020":
                            maskName = rptNode[i0]["array4"]["IDCHECKBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask setup completed. Mask slot({0}). Mask name({1})", maskSlot, maskName));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_210", string.Format("Mask setup completed. Mask slot({0}). Mask name({1})", maskSlot, maskName) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_211_E_CEID211EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_211_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                while(rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        case "3033":
                            maskSlot = rptNode["array4"]["LOADMASKSLOT"].InnerText.Trim();
                            break;
                        case "5065":
                            maskName = rptNode["array4"]["LOADBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask load completed. Mask slot({0}). Mask name({1})", maskSlot, maskName));
                List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                masks.Add(Tuple.Create(maskSlot, maskName, eMaterialStatus.INUSE));
                HandleMaskStatus(eqp, masks, tid, false, string.Empty);

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_211", string.Format("Mask load completed. Mask slot({0}). Mask name({1})", maskSlot, maskName) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_212_E_CEID212EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_212_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3034":
                            maskSlot = rptNode[i0]["array4"]["UNLOADMASKSLOT"].InnerText.Trim();
                            break;
                        case "3020":
                            maskName = rptNode[i0]["array4"]["IDCHECKBARCODESTRING"].InnerText.Trim();
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask unload completed. Mask slot({0}). Mask name({1})", maskSlot, maskName));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_212", string.Format("Mask unload completed. Mask slot({0}). Mask name({1})", maskSlot, maskName) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_251_E_CEID251EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_251_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string msg = "No Data";
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskState = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "1009":
                            #region [Mask Buffer Slot]
                            if (rptNode[i0]["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNodeList maskNodes = rptNode[i0]["array4"]["array5"].ChildNodes;
                                for (int i1 = 0; i1 < maskNodes.Count; i1++)
                                {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                    msg = msg + string.Format("Mask slot-bufferstate({0}-{1}); ", maskSlot, ConvertMaskSlotState(maskState));
                                }
                            }
                            #endregion
                            break;
                        case "1010":
                            #region [Mask Allocation Slot]
                            if (rptNode[i0]["array4"]["array5"].Attributes["len"].InnerText.Trim() != "0")
                            {
                                XmlNodeList maskNodes = rptNode[i0]["array4"]["array5"].ChildNodes;
                                for (int i1 = 0; i1 < maskNodes.Count; i1++)
                                {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                    msg = msg + string.Format("Mask slot-allocation({0}-{1}); ", maskSlot, ConvertMaskAllocateState(maskState));
                                }
                            }
                            #endregion
                            break;
                    }
                }

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_251", msg });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_252_E_CEID252EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_252_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYINMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask case inserted. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_252", string.Format("Mask case inserted. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_253_E_CEID253EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_253_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["CASECARRYOUTMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Mask case removed. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_253", string.Format("Mask case removed. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_254_E_CEID254EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_254_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["USEMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Equipment mask use start. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_254", string.Format("Equipment mask use start. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_255_E_CEID255EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_255_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["UNUSEMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Equipment mask use end. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_255", string.Format("Equipment mask use end. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_256_E_CEID256EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_256_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string maskSlot = string.Empty;
                string maskName = string.Empty;
                string maskID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "3039":
                            maskSlot = rptNode[i0]["array4"]["MATASSIGNEDMASKSLOT"].InnerText.Trim();
                            break;
                        case "3040":
                            maskName = rptNode[i0]["array4"]["MATASSIGNEDMASKNAME"].InnerText.Trim();
                            break;
                        case "3041":
                            maskName = rptNode[i0]["array4"]["MATASSIGNEDCOPYMASKID"].InnerText.Trim();
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Register the mask in MAT. Mask slot ({0}), Mask name ({1}), Mask copy ID ({2})", maskSlot, maskName, maskID));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_256", string.Format("Register the mask in MAT. Mask slot ({0}), Mask name ({1}), Mask copy ID ({2})", maskSlot, maskName, maskID) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_257_E_CEID257EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_257_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"]["MATDEASSIGNEDMASKSLOT"].InnerText.Trim();
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Delete the mask from MAT. Mask slot({0})", slot));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_257", string.Format("Delete the mask from MAT. Mask slot({0})", slot) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_301_E_CEID301EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_301_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                

                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3039":
                            lotID = rptNode[i0]["array4"]["PLATELOADLOTID"].InnerText.Trim();
                            break;
                        case "3040":
                            plateSlot = rptNode[i0]["array4"]["PLATELOADPLATESLOT"].InnerText.Trim();
                            break;
                        case "3041":
                            plateID = rptNode[i0]["array4"]["PLATELOADPLATEID"].InnerText.Trim();
                            break;
                    }
                }
                Invoke(eServiceName.JobService, "ReceiveJobDataReportForSECS", new object[3] { tid, plateID, eqp });

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_301", string.Format("Last receipt job({0})", plateID) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_302_E_CEID302EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_302_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3042":
                            lotID = rptNode[i0]["array4"]["PLATEUNLOADLOTID"].InnerText.Trim();
                            break;
                        case "3043":
                            plateSlot = rptNode[i0]["array4"]["PLATEUNLOADPLATESLOT"].InnerText.Trim();
                            break;
                        case "3044":
                            plateID = rptNode[i0]["array4"]["PLATEUNLOADPLATEID"].InnerText.Trim();
                            break;
                    }
                }
                Invoke(eServiceName.JobService, "SendOutJobDataReportForSECS", new object[3] { tid, plateID, eqp });

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_302", string.Format("Last send job({0})", plateID) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_303_E_CEID303EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_303_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string lotID = string.Empty;
                string plateSlot = string.Empty;
                string plateID = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        case "5066":
                            lotID = rptNode[i0]["array4"]["PLATESCRAPLOTID"].InnerText.Trim();
                            break;
                        case "5067":
                            plateSlot = rptNode[i0]["array4"]["PLATESCRAPPLATESLOT"].InnerText.Trim();
                            break;
                        case "5068":
                            plateID = rptNode[i0]["array4"]["PLATESCRAPPLATEID"].InnerText.Trim();
                            break;
                    }
                }
                Invoke(eServiceName.JobService, "RemoveJobDataReportForSECS", new object[] { tid, plateID, eqp, "1", string.Empty, string.Empty });

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_303", string.Format("Last remove job({0})", plateID) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_311_E_CEID311EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_311_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }

                string msg = "No Data";
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3045":
                            string lot = rptNode["array4"]["LOTSTARTLOTID"].InnerText.Trim();
                            msg = string.Format("Lot processing start. LotID({0})", lot);
                            _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_311", msg });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_312_E_CEID312EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_312_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3046":
                            log = string.Format("{0} LotID({1})", log, rptNode["array4"]["LOTENDLOTID"].InnerText.Trim());
                            break;
                        case "3047":
                            if (rptNode["array4"]["LOTENDSTATE"].InnerText.Trim() == "0")
                                log = string.Format("{0} State(0:OK)", log);
                            else
                                log = string.Format("{0} State(1:NG)", log);
                            break;
                        case "3280":
                            log = string.Format("{0} CarryInNumber({1})", log, rptNode["array4"]["LOTENDCARRYINNUMBER"].InnerText.Trim());
                            break;
                        case "3281":
                            log = string.Format("{0} CarryOutNumber({1})", log, rptNode["array4"]["LOTENDCARRYOUTNUMBER"].InnerText.Trim());
                            break;
                        case "3282":
                            log = string.Format("{0} NormalProcessNumber({1})", log, rptNode["array4"]["LOTENDNORMALPROCESSNUMBER"].InnerText.Trim());
                            break;
                        case "3283":
                            log = string.Format("{0} AbnormalProcessNumber({1})", log, rptNode["array4"]["LOTENDABNORMALPROCESSNUMBER"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Lot processing end.{0}", log));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_312", string.Format("Lot processing end.{0}", log) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_401_E_CEID401EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_401_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string recipe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3048":
                            string state = rptNode[i0]["array4"]["JOBENTRYCATEGORY"].InnerText.Trim();
                            data = string.Format("{0} Category({1}:{2})", data, state, ConvertJobEntryCategory(state));
                            break;
                        case "3049":
                            data = string.Format("{0} LotID({1})", data, rptNode[i0]["array4"]["JOBENTRYLOTID"].InnerText.Trim());
                            break;
                        case "3050":
                            data = string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["JOBENTRYRECIPENAME"].InnerText.Trim());
                            break;
                        case "3051":
                            data = string.Format("{0} RecipeID({1})", data, eqp.File.CurrentRecipeID);
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Job Loaded.{0}", data));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_401", string.Format("Job Loaded.{0}", data) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_402_E_CEID402EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_402_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string recipe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3053":
                            string state = rptNode[i0]["array4"]["JOBCANCELCATEGORY"].InnerText.Trim();
                            data = string.Format("{0} Category({1}:{2})", data, state, ConvertJobCancelCategory(state));
                            break;
                        case "3054":
                            data = string.Format("{0} LotID({1})", data, rptNode[i0]["array4"]["JOBCANCELLOTID"].InnerText.Trim());
                            break;
                        case "3055":
                            data = string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["JOBCANCELRECIPENAME"].InnerText.Trim());
                            break;
                        case "3056":
                            data = string.Format("{0} RecipeID({1})", data, rptNode[i0]["array4"]["JOBCANCELRECIPEID"].InnerText.Trim());
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Job Canceled.{0}", data));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_402", string.Format("Job Canceled.{0}", data) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_403_E_CEID403EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_403_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3058":
                            string state = rptNode["array4"]["JOBOPERATESTATE"].InnerText.Trim();
                            log = string.Format("{0} State({1})", log, state, ConvertJobOperateState(state));
                            break;
                        case "3059":
                            log = string.Format("{0} LotID({1})", log, rptNode["array4"]["JOBOPERATELOTID"].InnerText.Trim());
                            break;
                        case "3060":
                            log = string.Format("{0} RecipeName({1})", log, rptNode["array4"]["JOBOPERATERECIPENAME"].InnerText.Trim());
                            break;
                        case "3061":
                            log = string.Format("{0} RecipeID({1})", log, rptNode["array4"]["JOBOPERATERECIPEID"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Operator Job(Lot) manipulation.{0}", log));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_403", string.Format("Operator Job(Lot) manipulation.{0}", log) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_404_E_CEID404EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_404_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                

                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3063":
                            log = string.Format("{0} LotID({1})", log, rptNode["array4"]["JOBCONDITIONLOTID"].InnerText.Trim());
                            break;
                        case "3064":
                            log = string.Format("{0} RecipeName({1})", log, rptNode["array4"]["JOBCONDITIONRECIPENAME"].InnerText.Trim());
                            break;
                        case "3065":
                            log = string.Format("{0} RecipeID({1})", log, rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim());
                            if (eqp.File.CurrentRecipeID != rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim())
                            {
                                  eqp.File.CurrentRecipeID = rptNode["array4"]["JOBCONDITIONRECIPEID"].InnerText.Trim();
                                  ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                  ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                            }
                            break;
                        case "3066":
                            log = string.Format("{0} MaskName({1})", log, rptNode["array4"]["JOBCONDITIONMASKNAME"].InnerText.Trim());
                            break;
                        case "3067":
                            log = string.Format("{0} CopyMaskID({1})", log, rptNode["array4"]["JOBCONDITIONCOPYMASKID"].InnerText.Trim());
                            break;
                        case "3068":
                            log = string.Format("{0} MaskSlot({1})", log, rptNode["array4"]["JOBCONDITIONMASKSLOT"].InnerText.Trim());
                            break;
                        case "3069":
                            {
                                string state = rptNode["array4"]["JOBCONDITIONALIGNMENTMETHOD"].InnerText.Trim();
                                log = string.Format("{0} AlignmentMethod({1}:{2})", log, state, ConvertJobConditionAlignmentMethod(state));
                            }
                            break;
                        case "3070":
                            log = string.Format("{0} ExposueControl({1})", log, rptNode["array4"]["JOBCONDITIONEXPOSUECONTROL"].InnerText.Trim());
                            break;
                        case "3071":
                            log = string.Format("{0} Dose({1})", log, rptNode["array4"]["JOBCONDITIONDOSE"].InnerText.Trim());
                            break;
                        case "3072":
                            log = string.Format("{0} ScanSpeed({1})", log, rptNode["array4"]["JOBCONDITIONSCANSPEED"].InnerText.Trim());
                            break;
                        case "3073":
                            log = string.Format("{0} Focus({1})", log, rptNode["array4"]["JOBCONDITIONFOCUS"].InnerText.Trim());
                            break;
                        case "3074":
                            {
                                string state = rptNode["array4"]["JOBCONDITIONEGAFIXMODE"].InnerText.Trim();
                                log = string.Format("{0} EGAFixMode({1}:{2})", log, state, ConvertJobConditionEGAFixMode(state));
                            }
                            break;
                        case "3075":
                            log = string.Format("{0} PlateShiftX({1})", log, rptNode["array4"]["JOBCONDITIONPLATESHIFTX"].InnerText.Trim());
                            break;
                        case "3076":
                            log = string.Format("{0} PlateShiftY({1})", log, rptNode["array4"]["JOBCONDITIONPLATESHIFTY"].InnerText.Trim());
                            break;
                        case "3077":
                            log = string.Format("{0} PlateScaleX({1})", log, rptNode["array4"]["JOBCONDITIONPLATESCALEX"].InnerText.Trim());
                            break;
                        case "3078":
                            log = string.Format("{0} PlateScaleY({1})", log, rptNode["array4"]["JOBCONDITIONPLATESCALEY"].InnerText.Trim());
                            break;
                        case "3079":
                            log = string.Format("{0} PlateOrthogonality({1})", log, rptNode["array4"]["JOBCONDITIONPLATEORTHOGONALITY"].InnerText.Trim());
                            break;
                        case "3080":
                            log = string.Format("{0} PlateRotation({1})", log, rptNode["array4"]["JOBCONDITIONPLATEROTATION"].InnerText.Trim());
                            break;
                        case "3081":
                            if (rptNode["array4"]["JOBCONDITIONILLMINATION"].InnerText.Trim() == "2")
                                log = string.Format("{0} Illmination(2:Execute)", log);
                            else
                                log = string.Format("{0} Illmination(1:Not executed)", log);
                            break;
                        case "3082":
                            if (rptNode["array4"]["JOBCONDITIONMASKROTATION"].InnerText.Trim() == "2")
                                log = string.Format("{0} MaskRotation(2:Execute)", log);
                            else
                                log = string.Format("{0} MaskRotation(1:Not executed)", log);
                            break;
                        case "3083":
                            if (rptNode["array4"]["JOBCONDITIONLENSFOCUS"].InnerText.Trim() == "2")
                                log = string.Format("{0} LensFocus(2:Execute)", log);
                            else
                                log = string.Format("{0} LensFocus(1:Not executed)", log);
                            break;
                        case "3084":
                            if (rptNode["array4"]["JOBCONDITIONLENSBASELINE"].InnerText.Trim() == "2")
                                log = string.Format("{0} LensBaseline(2:Execute)", log);
                            else
                                log = string.Format("{0} LensBaseline(1:Not executed)", log);
                            break;
                        case "3085":
                            if (rptNode["array4"]["JOBCONDITIONMASKSCALING"].InnerText.Trim() == "2")
                                log = string.Format("{0} MaskScaling(2:Execute)", log);
                            else
                                log = string.Format("{0} MaskScaling(1:Not executed)", log);
                            break;
                        #region [T3 Add Items]
                        //2015/9/24 add by Frank
                        case "3340":
                            log = string.Format("{0} JobConditionPSStraightnessPSX({1})", log, rptNode["array4"]["JOBCONDITIONPSSTRAIGHTNESSPSX"].InnerText.Trim());
                            break;
                        case "3341":
                            log = string.Format("{0} JobConditionPSStraightnessPSY({1})", log, rptNode["array4"]["JOBCONDITIONPSSTRAIGHTNESSPSY"].InnerText.Trim());
                            break;
                        case "3342":
                            log = string.Format("{0} JobConditionLensStitching({1})", log, rptNode["array4"]["JOBCONDITIONLENSSTITCHING"].InnerText.Trim());
                            break;
                        case "3343":
                            log = string.Format("{0} JobConditionLensResultCheck({1})", log, rptNode["array4"]["JOBCONDITIONLENSRESULTCHECK"].InnerText.Trim());
                            break;
                        case "3344":
                            log = string.Format("{0} JobConditionMaskShape({1})", log, rptNode["array4"]["JOBCONDITIONMASKSHAPE"].InnerText.Trim());
                            break;
                        case "3345":
                            log = string.Format("{0} JobConditionMSYStraightness({1})", log, rptNode["array4"]["JOBCONDITIONMSYSTRAIGHTNESS"].InnerText.Trim());
                            break;
                        #endregion

                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Job(Lot) processing condition established.{0}", log));

                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_404", string.Format("Job(Lot) processing condition established.{0}", log) });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1201_E_CEID1201EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1201_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNodeList rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                string data = string.Empty;
                string recipe = string.Empty;
                for (int i0 = 0; i0 < rptNode.Count; i0++)
                {
                    switch (rptNode[i0]["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3469":
                            string.Format("{0} RecipeName({1})", data, rptNode[i0]["array4"]["PPCHANGENAME"].InnerText.Trim());
                            break;
                        case "3470":
                            string.Format("{0} Status({1})", data, rptNode[i0]["array4"]["PPCHANGESTATE"].InnerText.Trim());
                            break;
                        case "5061":
                            string.Format("{0} RecipeID({1})", data, rptNode[i0]["array4"]["PPCHANGERECIPEID"].InnerText.Trim());
                            break;
                    }
                }
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Recipe Control Changed.{0}", data));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1201", string.Format("Recipe Control Changed.{0}", data) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1202_E_CEID1202EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1202_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1202", "Machine constant changed." });
                        break;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Machine constant changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1203_E_CEID1203EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1203_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1203", "System parameter changed." });
                        break;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "System parameter changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1204_E_CEID1204EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1204_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1204", "Machine agjustment parameter changed." });
                        break;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Machine agjustment parameter changed.");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1301_E_CEID1301EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1301_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3271":
                            log = string.Format("{0} TerminalID({1})", log, rptNode["array4"]["TERMINALID"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Confirmation notice of terminal service message.{0}", log));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1301", string.Format("Confirmation notice of terminal service message.{0}", log) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F16_1302_E_CEID1302EventReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F16_1302_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                
                XmlNode rptNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                string log = string.Empty;
                while (rptNode != null)
                {
                    switch (rptNode["RPTID"].InnerText.Trim())
                    {
                        //2015/9/24 modify by Frank
                        case "3272":
                            log = string.Format("{0} ErrorCode({1})", log, rptNode["array4"]["ERRORCODE"].InnerText.Trim());
                            break;
                        case "3273":
                            log = string.Format("{0} ErrorDetail1({1})", log, rptNode["array4"]["ERRORDETAIL1"].InnerText.Trim());
                            break;
                        case "3274":
                            log = string.Format("{0} ErrorDetail2({1})", log, rptNode["array4"]["ERRORDETAIL2"].InnerText.Trim());
                            break;
                        case "3275":
                            log = string.Format("{0} ErrorDetail3({1})", log, rptNode["array4"]["ERRORDETAIL3"].InnerText.Trim());
                            break;
                        case "3276":
                            log = string.Format("{0} ErrorDetail4({1})", log, rptNode["array4"]["ERRORDETAIL4"].InnerText.Trim());
                            break;
                        case "3277":
                            log = string.Format("{0} ErrorDetail5({1})", log, rptNode["array4"]["ERRORDETAIL5"].InnerText.Trim());
                            break;
                        case "3278":
                            log = string.Format("{0} ErrorDetail6({1})", log, rptNode["array4"]["ERRORDETAIL6"].InnerText.Trim());
                            break;
                        case "3279":
                            log = string.Format("{0} ErrorText({1})", log, rptNode["array4"]["ERRORTEXT"].InnerText.Trim());
                            break;
                    }
                    rptNode = rptNode.NextSibling;
                }

                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Host request command error notice.{0}", log));

                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F16_1302", string.Format("Host request command error notice.{0}", log) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F20_E_IndividualReportData(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F20_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtnid = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                XmlNode xArray1 = recvTrx["secs"]["message"]["body"]["array1"];
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        string.Format("Recieved {0} data. {1}", rtnid, xArray1.InnerXml));

                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F20", string.Format("Recieved {0} data. {1}", rtnid, xArray1.InnerXml) });
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        public void S6F24_E_RequestSpooledDataAcknowledge(XmlDocument recvTrx)
        {
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F24_E");
            #region Handle Logic
            try
            {
                string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                    return;
                }
                string rtnid = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                string rsda = recvTrx["secs"]["message"]["body"]["RSDA"].InnerText.Trim();

                string msg = string.Format("Request spooled data acknowledgement.({0}:{1})", rsda, ConvertRequestSpooledAck(rsda));
                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);

                switch (rtn)
                {
                    case "OPI":
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[4] { tid, eqp.Data.LINEID, "Reply S6F24", msg });
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            #endregion
        }
        #endregion

        #region Send by host-S6
        public void TS6F4_H_ProcessResultReportAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, byte ack)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F4_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F4_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["ACKC6"].InnerText = ack.ToString();
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS6F6_H_MultiblockGrant(string eqpno, string eqpid, string tid, string sysbytes, byte grant)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F6_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F6_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["GRANT"].InnerText = grant.ToString();
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS6F12_H_EventReportAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, byte ack)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F12_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F12_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                sendTrx["secs"]["message"]["body"]["ACKC6"].InnerText = ack.ToString();
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS6F15_H_EventReportRequest(string eqpno, string eqpid, uint ceid, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F15_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F15_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["CEID"].InnerText = ceid.ToString();
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = string.Format("S6F15_{0}_H", ceid.ToString());
                sendTrx["secs"]["message"]["return"].InnerText = tag;
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS6F19_H_IndividualReportRequest(string eqpno, string eqpid, uint rptid, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F19_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F19_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["RPTID"].InnerText = rptid.ToString();
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = string.Format("S6F19_{0}_H", rptid.ToString());
                sendTrx["secs"]["message"]["return"].InnerText = tag;
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TS6F23_H_RequestSpooledData(string eqpno, string eqpid, byte rsdc, string tag, string trxid)
        {
            try
            {
                //Get Agent Object
                IServerAgent agent = GetServerAgent(eqpid);
                if (agent == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not get agent object with name ({0})", eqpid));
                    return;
                }
                //Get Transaction Format
                XmlDocument sendTrx = agent.GetTransactionFormat("S6F23_H") as XmlDocument;
                if (sendTrx == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "Can not get transaction object with name (S6F23_H)");
                    return;
                }
                //Set Data
                sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                sendTrx["secs"]["message"].Attributes["tid"].InnerText = base.CreateTrxID();
                sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                sendTrx["secs"]["message"]["body"]["RSDC"].InnerText = rsdc.ToString();
                sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = string.Format("S6F23_{0}_H", rsdc.ToString());
                sendTrx["secs"]["message"]["return"].InnerText = tag;
                //Put to Queue
                xMessage msg = new xMessage();
                msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                msg.FromAgent = agent.Name;
                msg.ToAgent = agent.Name;
                msg.Data = sendTrx;
                PutMessage(msg);
            }
            catch (InvalidOperationException ioex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Sent form host-S6
        public void S6F4_H_ProcessResultReportAcknowledge(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F4_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F4_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S6F6_H_MultiblockGrant(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F6_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F6_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S6F12_H_EventReportAcknowledge(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F12_H T3-Timeout", false);
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F12_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S6F15_H_EventReportRequest(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F15_H T3-Timeout", false);
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                    string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                        return;
                    }
                    switch (rtn)
                    {
                        case "OPI":
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                new object[4] { tid, eqp.Data.LINEID, "Rqeuest S6F15", "T3 Timeout" });
                            break;
                    }
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F15_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S6F19_H_IndividualReportRequest(XmlDocument recvTrx, bool timeout)
        {
            try
            {
                if (timeout)
                {
                    _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F19_H T3-Timeout", false);
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                    string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                        return;
                    }
                    switch (rtn)
                    {
                        case "OPI":
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                new object[4] { tid, eqp.Data.LINEID, "Rqeuest S6F19", "T3 Timeout" });
                            break;
                    }
                    return;
                }
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F19_H", false);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void S6F23_H_RequestSpooledData(XmlDocument recvTrx, bool timeout)
        {
            if (timeout)
            {
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F23_H T3-Timeout");
                return;
            }
            _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F23_H");
        }
        #endregion
    }
}