using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Log;
using System;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UniAuto.UniBCS.MesSpec;
using System.Threading;
using System.Linq;
using UniAuto.UniBCS.MISC;
using System.Threading.Tasks;
using UniAuto.UniBCS.Core;
using System.Net;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class NikonSECSService : AbstractService
    {
        private Random _rnd;
        private uint _dataid = 0; //0~4294967295
        private CommonSECSService _common;
        //private string _mdln;
        //private string _softrev;
        private List<Tuple<uint, List<uint>>> _defineReport;
        private List<Tuple<uint, List<uint>>> _linkEvent;
        private List<uint> _eventReport;
        Thread _reportProcessDataThread;
        bool _isRuning;
        //ConcurrentDictionary<'TRID', List<Tuple<'ItemID', 'ItemName', 'ItemType'>>>
        private ConcurrentDictionary<string, List<Tuple<string, string, string>>> _traceData;

        /// <summary>
        /// Service 初始化方法
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            bool ret = false;
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            _rnd = new Random();
            _common = new CommonSECSService(LogName, this);
            //_mdln = string.Empty;
            //_softrev = string.Empty;
            _defineReport = new List<Tuple<uint, List<uint>>>();
            _linkEvent = new List<Tuple<uint, List<uint>>>();
              //20160201 cy:T3沒256,257
              //20161117 yang:T3 Nikon更新spec（68S）,没1302
              //20170606 yang:by line维护
              //68S的更新

            if (ConstantManager.ContainsKey("NIKON68SLINES") && ConstantManager["NIKON68SLINES"].Values.ContainsKey(Dns.GetHostName()))
            {
                _eventReport = new List<uint>(){ 1,2,3,4,
                                              101,102,103,104,105,106,107,108,109,110,
                                              111,112,113,114,115,116,121,122,
                                              201,202,203,206,207,208,
                                              211,212,251,252,
                                              301,302,303,311,312,401,403,404,
                                              1201,1202,1203,1204,1301};
            }
            else
            {
                _eventReport = new List<uint>() { 1,2,3,4,
                                              101,102,103,104,105,106,107,108,109,110,
                                              111,112,113,114,115,116,121,122,
                                              201,202,203,204,205,206,207,208,209,210,
                                              211,212,251,252,253,254,255,
                                              301,302,303,311,312,401,402,403,404,
                                              1201,1202,1203,1204,1301,1302};
            }
            _traceData = new ConcurrentDictionary<string, List<Tuple<string, string, string>>>();

            _isRuning = true;
            _reportProcessDataThread = new Thread(new ThreadStart(ThreadProcessData));
            _reportProcessDataThread.IsBackground = true;
            _reportProcessDataThread.Start();
            ret = true;
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
            return ret;
        }

        public void Distroy()
        {
            _isRuning = false;
        }

        public void ConnectStatusChanged(bool connected, string eqpno)
        {
            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
            if (eqp == null)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not find Equipment No({0}) in EquipmentEntity!", eqpno));
                return;
            }
            if (eqp.HsmsConnected != connected)
            {
                if (eqp.HsmsConnected && !connected)
                {
                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] HSMS State Change From [1 (Connected) ] -> [0 (Disconnected) ]", eqpno));
                    eqp.SecsCommunicated = false;
                    eqp.HsmsConnStatus = "DISCONNECTED";

                    //20141217 CY:針對CIM MODE沒PLC的機台，Disconnect時，視為CIM OFF，待Online後才切為CIM ON
                    eqp.File.CIMMode = eBitResult.OFF;

                    //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                    Invoke(eServiceName.LineService, "CheckLineState", new object[] { base.CreateTrxID(), eqp.Data.LINEID });
                }
                else if (!eqp.HsmsConnected && connected)
                {
                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] HSMS State Change From [0 (Disconnected) ] -> [1 (Connected) ]", eqpno));
                    eqp.HsmsConnStatus = "CONNECTED";
                }
                eqp.HsmsConnected = connected;
                //20150611 cy:增加主動上報給OPI
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { base.CreateTrxID(), eqp });
                //20161204 yang:CIM Mode要报给MES
                Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { base.CreateTrxID(), eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });

                //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                Invoke(eServiceName.LineService, "CheckLineState", new object[] { base.CreateTrxID(), eqp.Data.LINEID });
            }
            //斷線清除RecipeCheckQueue,讓Recipe Service自己timeout就好
            if (!connected)
            {
                #region RecipeRegisterCheck
                {
                    ConcurrentDictionary<string, RecipeInCheck> checkings = _common.GetCheckingRecipeIDs(eqpno);
                    if (checkings != null && checkings.Count > 0)
                        _common.RemoveCheckingRecipeID(eqpno);
                }
                #endregion
                #region RecipeParameterCheck
                {
                    ConcurrentDictionary<string, RecipeInCheck> checkings = _common.GetCheckingRecipeParameters(eqpno);
                    if (checkings != null && checkings.Count > 0)
                        _common.RemoveCheckingRecipeParameter(eqpno);
                }
                #endregion
            }

            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { base.CreateTrxID(), eqp });
            //20161204 yang:CIM Mode要报给MES
            Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { base.CreateTrxID(), eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
        }

        public void SelectStatusChanged(bool selected, string eqpno)
        {
            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
            if (eqp == null)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not find Equipment No({0}) in EquipmentEntity!", eqpno));
                return;
            }
            if (eqp.HsmsSelected != selected)
            {
                if (!eqp.HsmsSelected && selected)
                {
                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] HSMS State Change From [NOT-SELECTED] -> [SELECTED]", eqpno));
                    //當HSMS發生斷線後再重新連線時，若此時為CIM On，也就是之斷線前的control mode是online，則發S1F3去要回contorl mode
                    //20141217 cy: 改為斷線重連後，若斷線前的control mode是online，則發S1F3去要回contorl mode
                    //20150611 cy:連線後就主動要求Online (登京已確認)
                    //if (eqp.File.CIMMode == eBitResult.ON)
                    //if (eqp.File.HSMSControlMode != "OFF-LINE")
                    //{
                        if (!eqp.SecsCommunicated)
                        {
                              TS1F13_H_EstablishCommunicationsRequest(eqp.Data.NODENO, eqp.Data.NODEID, "AutoOnlineRequest", string.Empty);
                        }

                      //20160128 cy:改到通訊建立發
                        //Task.Factory.StartNew(() =>
                        //{
                        //    while (true)
                        //    {
                        //        if (eqp.SecsCommunicated)
                        //        {
                        //            //TS1F3_H_SelectedEquipmentStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, (uint)1004, "ConnectedCheckContorl", string.Empty);
                        //              TS1F17_H_RequestOnLine(eqp.Data.NODENO, eqp.Data.NODEID, "AutoOnlineRequest", string.Empty);
                        //            break;
                        //        }
                        //    }
                        //});
                    //}
                }
                eqp.HsmsSelected = selected;
            }
        }


        private uint GetDataID()
        {
            if (_dataid == 4294967295)
                _dataid = 0;

            return _dataid++;
        }

        private void ThreadProcessData()
        {
            List<Equipment> eqps = null;
            while (_isRuning)
            {
                Thread.Sleep(300);
                try
                {
                    //20141218 cy:避免程式還沒initial完
                    if (Workbench.State != eWorkbenchState.RUN)
                    {
                        continue;
                    }

                    if (eqps == null)
                        eqps = ObjectManager.EquipmentManager.GetEQPs();
                    else
                    {
                        foreach (Equipment eqp in eqps)
                        {
                            HandleProcessData(eqp);
                            HandleLotProcessData(eqp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
        }

        private void HandleProcessData(Equipment eqp)
        {
            try
            {
                ConcurrentDictionary<string, ProcessDataSpool> jobs = _common.GetRawProcessData(eqp.Data.NODENO);
                if (jobs == null || jobs.Count == 0) return;
                lock (jobs)
                {
                    bool reportJob = false;
                    string reportRsn = string.Empty;
                    List<Tuple<string, string>> removeJobs = new List<Tuple<string, string>>();
                    List<string> trxids = null;
                    string lastTrxID = string.Empty;
                    foreach (string jobid in jobs.Keys)
                    {
                        Job job = ObjectManager.JobManager.GetJob(jobid);
                        trxids = new List<string>();
                        lastTrxID = string.Empty;
                        if (job == null)
                        {
                            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] Can not find JobID({1}) in JobEntity!", eqp.Data.NODENO, jobid));
                            removeJobs.Add(Tuple.Create(jobid, "WIP not exist"));
                            continue;
                        }
                        reportJob = false;
                        if (jobs[jobid].ProcessDataRaw.ContainsKey("99"))
                        {
                            reportRsn = "SUBCD 99 received";
                            removeJobs.Add(Tuple.Create(jobid, reportRsn));
                            reportJob = true;
                        }
                        else if (new TimeSpan(DateTime.Now.Ticks - jobs[jobid].StartTime.Ticks).TotalMilliseconds >= ParameterManager["NIKONSECSDATATIMEOUT"].GetInteger())
                        {
                            reportRsn = "Data collection timeout";
                            removeJobs.Add(Tuple.Create(jobid, reportRsn));
                            reportJob = true;
                        }
                        else if (!eqp.HsmsConnected)
                        {
                            reportRsn = "Disconnect with equipment";
                            removeJobs.Add(Tuple.Create(jobid, reportRsn));
                            reportJob = true;
                        }
                        if (reportJob)
                        {
                            IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();
                            IList<ChangeMaterialLifeReport.MATERIALc> cmlMaterials = new List<ChangeMaterialLifeReport.MATERIALc>();
                            foreach (KeyValuePair<string, XmlDocument> kvp in jobs[jobid].ProcessDataRaw)
                            {
                                //20150310 cy:將ChangeMaterialLifeReport的判斷由S6F3_1_99_E_CEID1SUBCD99ProcessResultReport移到這邊
                                AnalyzeProcessData(kvp.Value, job, ref mesdic, ref cmlMaterials);
                                lastTrxID = kvp.Value["secs"]["message"].Attributes["tid"].InnerText.Trim();
                                trxids.Add(lastTrxID);
                            }
                            //20150402 cy:叫用新法
                            _common.ProcessDataReport(eqp, job, mesdic, lastTrxID, true);
                            NLogManager.Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [{3}] Report job process data for {2}. JobID({1}) TrxIDs({4}]", eqp.Data.NODENO, jobid, reportRsn, lastTrxID, string.Join(",", trxids.ToArray())));

                            #region 上報ChangeMaterialLifeReport(如果需要)
                            if (cmlMaterials.Count > 0)
                            {
                                Invoke(eServiceName.MESService, "ChangeMaterialLife", new object[] { lastTrxID, eqp.Data.LINEID, eqp.Data.NODEID, jobid, cmlMaterials });
                            }
                            #endregion
                            break; //一次只報一筆
                        }
                    }
                    if (removeJobs.Count > 0)
                    {
                        foreach (Tuple<string, string> v in removeJobs)
                        {
                            _common.RemoveRawProcessData(eqp.Data.NODENO, v.Item1);
                            NLogManager.Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] Remove job({1}) process data spool for {2}.", eqp.Data.NODENO, v.Item1, v.Item2));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AnalyzeProcessData(XmlDocument message, ref IList<ProductProcessData.ITEMc> mesdic)
        {
            try
            {
                string name = message["secs"]["message"].Attributes["name"].InnerText.Trim();
                string tid = message["secs"]["message"].Attributes["tid"].InnerText.Trim();
                switch (name)
                {
                    case "S6F3_1_0_E":
                        #region S6F3 CDIE 1 SUBCD 0
                        {
                            if (mesdic.Count == 0) //若已組過SUBCD99的資料,不再組一次
                            {
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        XmlNode xArray4 = xArray3["array4"].FirstChild;
                                        while (xArray4 != null)
                                        {
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4.InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xArray4.Name.Trim(),
                                                SITELIST = site
                                            });
                                            xArray4 = xArray4.NextSibling;
                                        }
                                    }
                                    else
                                    {
                                        List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                        site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                        mesdic.Add(new ProductProcessData.ITEMc()
                                        {
                                            ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                            SITELIST = site
                                        });
                                    }
                                    xArray3 = xArray3.NextSibling;
                                }
                            }
                        }
                        #endregion
                        break;
                    case "S6F3_1_99_E":
                        {
                            if (mesdic.Count == 0) //未組過SUBCD 0,全組
                            {
                                #region 未組過SUBCD 0,全組
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        if (xArray3["array4"].FirstChild.Name.Contains("DV"))
                                        {
                                            XmlNode xArray4 = xArray3.FirstChild;
                                            while (xArray4 != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVNAME"].InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray4["DVVALUE"].InnerText.Trim(),
                                                    SITELIST = site
                                                });
                                                xArray4 = xArray4.NextSibling;
                                            }
                                        }
                                        else
                                        {
                                            XmlNode xNode = xArray3["array4"].FirstChild;
                                            while (xNode != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xNode.InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xNode.Name.Trim(),
                                                    SITELIST = site
                                                });

                                                xNode = xNode.NextSibling;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                        site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                        mesdic.Add(new ProductProcessData.ITEMc()
                                        {
                                            ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                            SITELIST = site
                                        });
                                    }
                                    xArray3 = xArray3.NextSibling;
                                } 
                                #endregion
                            }
                            else //已組過資料,則只組最後400個
                            {
                                #region 已組過資料,則只組最後400個
                                //20150306 cy:改由最後的child找起
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].LastChild;//.FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        if (xArray3["array4"].FirstChild.Name.Contains("DV"))
                                        {
                                            XmlNode xArray4 = xArray3.FirstChild;
                                            while (xArray4 != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVNAME"].InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray4["DVVALUE"].InnerText.Trim(),
                                                    SITELIST = site
                                                });
                                                xArray4 = xArray4.NextSibling;
                                            }
                                        }

                                    }

                                    xArray3 = xArray3.NextSibling;
                                } 
                                #endregion
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AnalyzeProcessData(XmlDocument message, Job job, ref IList<ProductProcessData.ITEMc> mesdic, ref IList<ChangeMaterialLifeReport.MATERIALc> mtldic)
        {
            try
            {
                string name = message["secs"]["message"].Attributes["name"].InnerText.Trim();
                string tid = message["secs"]["message"].Attributes["tid"].InnerText.Trim();
                string mkey = "NIKON_PROCESS_DATA_MATERIALLIFE";
                ConstantData _constantdata = ConstantManager[mkey];
                switch (name)
                {
                    case "S6F3_1_0_E":
                        #region S6F3 CDIE 1 SUBCD 0
                        {
                            if (mesdic.Count == 0) //若已組過SUBCD99的資料,不再組一次
                            {
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        XmlNode xArray4 = xArray3["array4"].FirstChild;
                                        while (xArray4 != null)
                                        {
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4.InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xArray4.Name.Trim(),
                                                SITELIST = site
                                            });
                                            #region handle ChangeMaterialLifeReport.MATERIALc
                                            if (_constantdata != null && _constantdata[xArray4.Name.Trim()].Value.ToUpper() == "TRUE")
                                            {
                                                mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                {
                                                    CHAMBERID = job.ChamberName,
                                                    MATERIALNAME = xArray4.Name.Trim(),
                                                    MATERIALTYPE = "lamp",
                                                    QUANTITY = xArray4.InnerText.Trim(),
                                                });
                                            }
                                            #endregion
                                            xArray4 = xArray4.NextSibling;
                                        }
                                    }
                                    else
                                    {
                                        List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                        site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                        mesdic.Add(new ProductProcessData.ITEMc()
                                        {
                                            ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                            SITELIST = site
                                        });
                                        #region handle ChangeMaterialLifeReport.MATERIALc
                                        if (_constantdata != null && _constantdata[xArray3["DCNAME"].InnerText.Trim()].Value.ToUpper() == "TRUE")
                                        {
                                            mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                            {
                                                CHAMBERID = job.ChamberName,
                                                MATERIALNAME = xArray3["DCNAME"].InnerText.Trim(),
                                                MATERIALTYPE = "lamp",
                                                QUANTITY = xArray3["DCVALUE"].InnerText.Trim(),
                                            });
                                        }
                                        #endregion
                                        //20151019 cy:若DCNAME有RETICLE_ID,以此為Use Mask Id上報
                                        if (xArray3["DCNAME"].InnerText.Trim().Equals("RETICLE_ID"))
                                        {
                                              job.ArraySpecial.ExposureMaskID = job.CfSpecial.MaskID = xArray3["DCNAME"].InnerText.Trim();
                                        }
                                    }
                                    xArray3 = xArray3.NextSibling;
                                }
                            }
                        }
                        #endregion
                        break;
                    case "S6F3_1_99_E":
                        {
                            if (mesdic.Count == 0) //未組過SUBCD 0,全組
                            {
                                #region 未組過SUBCD 0,全組
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        if (xArray3["array4"].FirstChild.Name.Contains("DV"))
                                        {
                                            XmlNode xArray4 = xArray3.FirstChild;
                                            while (xArray4 != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVVALUE"].InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray4["DVNAME"].InnerText.Trim(),
                                                    SITELIST = site
                                                });
                                                #region handle ChangeMaterialLifeReport.MATERIALc
                                                if (_constantdata != null && _constantdata[xArray4["DVNAME"].InnerText.Trim()].Value.ToUpper() == "TRUE")
                                                {
                                                    mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                    {
                                                        CHAMBERID = job.ChamberName,
                                                        MATERIALNAME = xArray4["DVNAME"].InnerText.Trim(),
                                                        MATERIALTYPE = "lamp",
                                                        QUANTITY = xArray4["DVVALUE"].InnerText.Trim(),
                                                    });
                                                }
                                                #endregion
                                                xArray4 = xArray4.NextSibling;
                                            }
                                        }
                                        else
                                        {
                                            XmlNode xNode = xArray3["array4"].FirstChild;
                                            while (xNode != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xNode.InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xNode.Name.Trim(),
                                                    SITELIST = site
                                                });
                                                #region handle ChangeMaterialLifeReport.MATERIALc
                                                if (_constantdata != null && _constantdata[xNode.Name.Trim()].Value.ToUpper() == "TRUE")
                                                {
                                                    mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                    {
                                                        CHAMBERID = job.ChamberName,
                                                        MATERIALNAME = xNode.Name.Trim(),
                                                        MATERIALTYPE = "lamp",
                                                        QUANTITY = xNode.InnerText.Trim(),
                                                    });
                                                }
                                                #endregion
                                                xNode = xNode.NextSibling;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                        site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                        mesdic.Add(new ProductProcessData.ITEMc()
                                        {
                                            ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                            SITELIST = site
                                        });
                                        #region handle ChangeMaterialLifeReport.MATERIALc
                                        if (_constantdata != null && _constantdata[xArray3["DCNAME"].InnerText.Trim()].Value.ToUpper() == "TRUE")
                                        {
                                            mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                            {
                                                CHAMBERID = job.ChamberName,
                                                MATERIALNAME = xArray3["DCNAME"].InnerText.Trim(),
                                                MATERIALTYPE = "lamp",
                                                QUANTITY = xArray3["DCVALUE"].InnerText.Trim(),
                                            });
                                        }
                                        #endregion
                                        //20151019 cy:若DCNAME有RETICLE_ID,以此為Use Mask Id上報
                                        if (xArray3["DCNAME"].InnerText.Trim().Equals("RETICLE_ID"))
                                        {
                                              job.ArraySpecial.ExposureMaskID = job.CfSpecial.MaskID = xArray3["DCNAME"].InnerText.Trim();
                                        }
                                    }
                                    xArray3 = xArray3.NextSibling;
                                }
                                #endregion
                            }
                            else //已組過資料,則只組最後400個
                            {
                                #region 已組過資料,則只組最後400個
                                //20150306 cy:改由最後的child找起
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].LastChild;//.FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        if (xArray3["array4"].FirstChild.Name.Contains("DV"))
                                        {
                                            XmlNode xArray4 = xArray3.FirstChild;
                                            while (xArray4 != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVVALUE"].InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray4["DVNAME"].InnerText.Trim(),
                                                    SITELIST = site
                                                });
                                                #region handle ChangeMaterialLifeReport.MATERIALc
                                                if (_constantdata != null && _constantdata[xArray4["DVNAME"].InnerText.Trim()].Value.ToUpper() == "TRUE")
                                                {
                                                    mtldic.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                    {
                                                        CHAMBERID = job.ChamberName,
                                                        MATERIALNAME = xArray4["DVNAME"].InnerText.Trim(),
                                                        MATERIALTYPE = "lamp",
                                                        QUANTITY = xArray4["DVVALUE"].InnerText.Trim(),
                                                    });
                                                }
                                                #endregion
                                                xArray4 = xArray4.NextSibling;
                                            }
                                        }

                                    }

                                    xArray3 = xArray3.NextSibling;
                                }
                                #endregion
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HandleLotProcessData(Equipment eqp)
        {
            try
            {
                ConcurrentDictionary<string, ProcessDataSpool> lots = _common.GetRawLotProcessData(eqp.Data.NODENO);
                if (lots == null || lots.Count == 0) return;
                lock (lots)
                {
                    bool report = false;
                    string reportRsn = string.Empty;
                    List<Tuple<string, string>> removeLots = new List<Tuple<string, string>>();
                    List<string> trxids = null;
                    string lastTrxID = string.Empty;
                    foreach (string lot in lots.Keys)
                    {
                        trxids = new List<string>();
                        lastTrxID = string.Empty;
                        report = false;
                        if (lots[lot].ProcessDataRaw.ContainsKey("99"))
                        {
                            reportRsn = "SUBCD 99 received";
                            removeLots.Add(Tuple.Create(lot, reportRsn));
                            report = true;
                        }
                        else if (new TimeSpan(DateTime.Now.Ticks - lots[lot].StartTime.Ticks).TotalMilliseconds >= ParameterManager["NIKONSECSDATATIMEOUT"].GetInteger())
                        {
                            reportRsn = "Data collection timeout";
                            removeLots.Add(Tuple.Create(lot, reportRsn));
                            report = true;
                        }
                        else if (!eqp.HsmsConnected)
                        {
                            reportRsn = "Disconnect with equipment";
                            removeLots.Add(Tuple.Create(lot, reportRsn));
                            report = true;
                        }
                        if (report)
                        {
                            IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();

                            foreach (KeyValuePair<string, XmlDocument> kvp in lots[lot].ProcessDataRaw)
                            {
                                AnalyzeLotProcessData(kvp.Value, ref mesdic);
                                lastTrxID = kvp.Value["secs"]["message"].Attributes["tid"].InnerText.Trim();
                                trxids.Add(lastTrxID);
                            }
                            _common.LotProcessDataReport(eqp, lot, mesdic, lastTrxID);
                            NLogManager.Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [{3}] Report lot process data for {2}. CassetteSeq({1}) TrxIDs({4}]", eqp.Data.NODENO, lot, reportRsn, lastTrxID, string.Join(",", trxids.ToArray())));
                            break; //一次只報一筆
                        }
                    }
                    if (removeLots.Count > 0)
                    {
                        foreach (Tuple<string, string> v in removeLots)
                        {
                            _common.RemoveRawLotProcessData(eqp.Data.NODENO, v.Item1);
                            NLogManager.Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] Remove CassettSeq({1}) lot process data spool for {2}.", eqp.Data.NODENO, v.Item1, v.Item2));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AnalyzeLotProcessData(XmlDocument message, ref IList<ProductProcessData.ITEMc> mesdic)
        {
            try
            {
                string name = message["secs"]["message"].Attributes["name"].InnerText.Trim();
                string tid = message["secs"]["message"].Attributes["tid"].InnerText.Trim();
                int count = 0;
                switch (name)
                {
                    case "S6F3_2_0_E":
                        #region S6F3 CDIE 2 SUBCD 0
                        {
                            if (mesdic.Count == 0) //若已組過SUBCD99的資料,不再組一次
                            {
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "DCVALUE")
                                    {
                                        #region <array3 name="List" type="L" len="?"><DCVALUE name="DCVALUE" type="A" len="16" fixlen="True" />
                                        XmlNode xDCVALUE = xArray3.FirstChild;
                                        count = 0;
                                        while (xDCVALUE != null)
                                        {
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xDCVALUE.InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xDCVALUE.Name.Trim() + "_" + count++.ToString(),
                                                SITELIST = site
                                            });
                                            xDCVALUE = xDCVALUE.NextSibling;
                                        } 
                                        #endregion
                                    }
                                    else
                                    {
                                        if (xArray3["array4"] != null)
                                        {
                                            #region <array4 name="List (number of slots)" type="L" len="?"><DCVALUE name="DCVALUE" type="A" len="16" fixlen="True" />
                                            XmlNode xDCVALUE = xArray3["array4"].FirstChild;
                                            count = 0;
                                            while (xDCVALUE != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xDCVALUE.InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray3["DCNAME"].InnerText.Trim() + "_" + count++.ToString(),
                                                    SITELIST = site
                                                });
                                                xDCVALUE = xDCVALUE.NextSibling;
                                            } 
                                            #endregion
                                        }
                                        else
                                        {
                                            #region <DCNAME name="DCNAME" type="A" len="9" fixlen="True" /><DCVALUE name="DCVALUE" type="A" len="2" fixlen="True" />
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                                SITELIST = site
                                            }); 
                                            #endregion
                                        }
                                    }
                                    xArray3 = xArray3.NextSibling;
                                }
                            }
                        }
                        #endregion
                        break;
                    case "S6F3_2_99_E":
                        {
                            if (mesdic.Count == 0) //未組過SUBCD 0,全組
                            {
                                #region 未組過SUBCD 0,全組
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        XmlNode xArray4 = xArray3.FirstChild;
                                        while (xArray4 != null)
                                        {
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVNAME"].InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xArray4["DVVALUE"].InnerText.Trim(),
                                                SITELIST = site
                                            });
                                            xArray4 = xArray4.NextSibling;
                                        }
                                    }
                                    else
                                    {
                                        if (xArray3.FirstChild.Name == "DCVALUE")
                                        {
                                            #region <array3 name="List" type="L" len="?"><DCVALUE name="DCVALUE" type="A" len="16" fixlen="True" />
                                            XmlNode xDCVALUE = xArray3.FirstChild;
                                            count = 0;
                                            while (xDCVALUE != null)
                                            {
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xDCVALUE.InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xDCVALUE.Name.Trim() + "_" + count++.ToString(),
                                                    SITELIST = site
                                                });
                                                xDCVALUE = xDCVALUE.NextSibling;
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            if (xArray3["array4"] != null)
                                            {
                                                #region <array4 name="List (number of slots)" type="L" len="?"><DCVALUE name="DCVALUE" type="A" len="16" fixlen="True" />
                                                XmlNode xDCVALUE = xArray3["array4"].FirstChild;
                                                count = 0;
                                                while (xDCVALUE != null)
                                                {
                                                    List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                    site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xDCVALUE.InnerText.Trim() });
                                                    mesdic.Add(new ProductProcessData.ITEMc()
                                                    {
                                                        ITEMNAME = xArray3["DCNAME"].InnerText.Trim() + "_" + count++.ToString(),
                                                        SITELIST = site
                                                    });
                                                    xDCVALUE = xDCVALUE.NextSibling;
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region <DCNAME name="DCNAME" type="A" len="9" fixlen="True" /><DCVALUE name="DCVALUE" type="A" len="2" fixlen="True" />
                                                List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                                site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray3["DCVALUE"].InnerText.Trim() });
                                                mesdic.Add(new ProductProcessData.ITEMc()
                                                {
                                                    ITEMNAME = xArray3["DCNAME"].InnerText.Trim(),
                                                    SITELIST = site
                                                });
                                                #endregion
                                            }
                                        }
                                    }
                                    xArray3 = xArray3.NextSibling;
                                }
                                #endregion
                            }
                            else //已組過資料,則只組最後400個
                            {
                                #region 已組過資料,則只組最後400個
                                XmlNode xArray3 = message["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                while (xArray3 != null)
                                {
                                    if (xArray3.FirstChild.Name == "array4")
                                    {
                                        XmlNode xArray4 = xArray3.FirstChild;
                                        while (xArray4 != null)
                                        {
                                            List<ProductProcessData.SITEc> site = new List<ProductProcessData.SITEc>();
                                            site.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = xArray4["DVNAME"].InnerText.Trim() });
                                            mesdic.Add(new ProductProcessData.ITEMc()
                                            {
                                                ITEMNAME = xArray4["DVVALUE"].InnerText.Trim(),
                                                SITELIST = site
                                            });
                                            xArray4 = xArray4.NextSibling;
                                        }
                                    }

                                    xArray3 = xArray3.NextSibling;
                                }
                                #endregion
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HandleMaskStatus(Equipment eqp, List<Tuple<string, string, eMaterialStatus>> masks,
            string tid, bool waitack, string systembyte)
        {
            if (masks != null && masks.Count <= 0)
                return;

            IList<MaskStateChanged.MASKc> maskRptList = new List<MaskStateChanged.MASKc>();
            IList<MaskStateChanged.MASKc> maskTransList = new List<MaskStateChanged.MASKc>();
            //先以Slot管理物件，機台報的不存在就新增
            foreach (var mask in masks)
            {
                string slot = mask.Item1;
                if (!string.IsNullOrEmpty(slot))
                {
                    MaterialEntity maskEntity = ObjectManager.MaterialManager.GetMaskBySlot(eqp.Data.NODENO, slot, _common.GetMaterialType(eqp));
                    if (maskEntity == null)
                    {
                        maskEntity = new MaterialEntity();
                        maskEntity.MaterialType = _common.GetMaterialType(eqp);
                        maskEntity.EQType = eMaterialEQtype.MaskEQ;
                        maskEntity.NodeNo = eqp.Data.NODENO;
                        maskEntity.UnitNo = "0";
                        maskEntity.MaterialID = string.Empty;
                        maskEntity.MaterialSlotNo = slot;
                        maskEntity.MaterialStatus = eMaterialStatus.NONE;
                        maskEntity.MaterialPort = slot; //port為key值,避免有問題,所以讓port = slot
                        
                        ObjectManager.MaterialManager.AddMask(maskEntity);
                        //Add時就會做一次Save了, 所以不再叫用一次.
                        //ObjectManager.MaterialManager.EnqueueSave(maskEntity);
                    }
                }
            }
            //再以Name管理物件，將相同Name不同Slot的Mask，移到相同Slot
            foreach (var mask in masks)
            {
                string slot = mask.Item1;
                string name = mask.Item2;
                MaterialEntity maskEntitySlot = ObjectManager.MaterialManager.GetMaskBySlot(eqp.Data.NODENO, slot, _common.GetMaterialType(eqp));
                if (!string.IsNullOrEmpty(name))
                {
                    List<MaterialEntity> maskEntitysName = ObjectManager.MaterialManager.GetMasksByName(eqp.Data.NODENO, name);
                    //用name找不到，表示本來就不存在這筆資料，則不做事，等一下比對時再決定是否上報
                    if (maskEntitysName != null && maskEntitysName.Count > 0)
                    {
                        //foreach (MaterialEntity maskEntityName in maskEntitysName)
                        for (int i = 0; i < maskEntitysName.Count; i++)
                        {
                            if (maskEntitysName[i].MaterialSlotNo != maskEntitySlot.MaterialSlotNo)
                            {
                                //不在同Slot表示是移動，把資料移動對應的Slot
                                //若這個Slot有資料，且與目前不同，應就是殘帳，放到回收桶
                                if (!string.IsNullOrEmpty(maskEntitySlot.MaterialID.Trim()) &&
                                    maskEntitySlot.MaterialID != maskEntitysName[i].MaterialID &&
                                    maskEntitySlot.MaterialStatus != eMaterialStatus.DISMOUNT)
                                {
                                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                                    item.MASKPOSITION = maskEntitySlot.MaterialSlotNo;
                                    item.MASKNAME = maskEntitySlot.MaterialID;
                                    item.MASKSTATE = maskEntitySlot.MaterialStatus.ToString();
                                    maskTransList.Add(item);
                                }
                                //目的slot有資料且與來源不同,更新目的slot,否則不改.
                                if (maskEntitySlot.MaterialID != maskEntitysName[i].MaterialID)
                                {
                                    maskEntitySlot.MaterialID = maskEntitysName[i].MaterialID;
                                    maskEntitySlot.MaterialStatus = maskEntitysName[i].MaterialStatus;
                                    ObjectManager.MaterialManager.EnqueueSave(maskEntitySlot);
                                }
                                maskEntitysName[i].MaterialID = string.Empty;
                                maskEntitysName[i].MaterialStatus = eMaterialStatus.NONE;
                                ObjectManager.MaterialManager.EnqueueSave(maskEntitysName[i]);
                            }
                        }

                    }
                }

            }
            //物件管理後，直接比對
            foreach (var mask in masks)
            {
                string slot = mask.Item1;
                string newname = mask.Item2;
                eMaterialStatus newstatus = mask.Item3;

                string oldname = string.Empty; //init default
                eMaterialStatus oldstatus = eMaterialStatus.NONE; //init default				
                //get entity by slot or name
                MaterialEntity maskEntity = null;
                if (string.IsNullOrEmpty(slot))
                {
                    if (string.IsNullOrEmpty(newname))
                    {
                        throw new Exception("The parameter of mask slot and name is empty. TrxID=" + tid);
                    }
                    else
                    {
                        maskEntity = ObjectManager.MaterialManager.GetMaskByName(eqp.Data.NODENO, newname);
                    }
                }
                else
                {
                    maskEntity = ObjectManager.MaterialManager.GetMaskBySlot(eqp.Data.NODENO, slot, _common.GetMaterialType(eqp));
                }
                //check if enity not exists,create
                if (maskEntity == null)
                {
                    maskEntity = new MaterialEntity();
                    maskEntity.MaterialType = _common.GetMaterialType(eqp);
                    maskEntity.EQType = eMaterialEQtype.MaskEQ;
                    maskEntity.NodeNo = eqp.Data.NODENO;
                    maskEntity.UnitNo = "0";
                    maskEntity.MaterialID = newname;
                    maskEntity.MaterialSlotNo = slot;
                    maskEntity.MaterialPort = slot; //port為key值,避免有問題,所以讓port = slot
                    maskEntity.MaterialStatus = newstatus;
                    
                    ObjectManager.MaterialManager.AddMask(maskEntity);
                }
                else
                {
                    oldstatus = maskEntity.MaterialStatus;
                    oldname = maskEntity.MaterialID;
                }

                if (oldstatus != newstatus)
                {
                    if (oldname == newname)
                    {
                        //oldstatus<>newstatus && oldname=namenew
                        //check new rpt
                        if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
                        {
                            MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                            item.MASKPOSITION = maskEntity.MaterialSlotNo;
                            item.MASKNAME = newname;
                            item.MASKSTATE = newstatus.ToString();
                            maskRptList.Add(item);
                        }
                    }
                    else
                    {
                        //oldstatus<>newstatus && oldname<>newname 
                        //check old unmount
                        if (!string.IsNullOrEmpty(oldname) && oldstatus != eMaterialStatus.NONE && oldstatus != eMaterialStatus.DISMOUNT)
                        {
                            MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                            item.MASKPOSITION = maskEntity.MaterialSlotNo;
                            item.MASKNAME = oldname;
                            item.MASKSTATE = eMaterialStatus.DISMOUNT.ToString();
                            //maskRptList.Add(item); modify for avoid dismount other line's mount mask 2016/03/24
                        }
                        //check new rpt
                        if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
                        {
                            MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                            item.MASKPOSITION = maskEntity.MaterialSlotNo;
                            item.MASKNAME = newname;
                            item.MASKSTATE = newstatus.ToString();
                            if (newstatus != eMaterialStatus.DISMOUNT) //modify for avoid dismount other line's mount mask 2016/03/24
                                maskRptList.Add(item);
                        }
                    }
                }
                else
                {
                    if (oldname == newname)
                    {
                        //oldstatus==newstatus && oldname==newname
                        //same name & same status,skip
                    }
                    else
                    {
                        //oldstatus==newstatus && oldname<>newname
                        //check old unmount
                        if (!string.IsNullOrEmpty(oldname) && oldstatus != eMaterialStatus.NONE && oldstatus != eMaterialStatus.DISMOUNT)
                        {
                            MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                            item.MASKPOSITION = maskEntity.MaterialSlotNo;
                            item.MASKNAME = oldname;
                            item.MASKSTATE = eMaterialStatus.DISMOUNT.ToString();
                            //maskRptList.Add(item); modify for avoid dismount other line's mount mask 2016/03/24
                        }
                        //check new rpt
                        if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
                        {
                            MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
                            item.MASKPOSITION = maskEntity.MaterialSlotNo;
                            item.MASKNAME = newname;
                            item.MASKSTATE = newstatus.ToString();
                            if (newstatus != eMaterialStatus.DISMOUNT) //modify for avoid dismount other line's mount mask 2016/03/24
                                maskRptList.Add(item);
                        }
                    }
                }
                //update name/status
                maskEntity.MaterialID = newname;
                maskEntity.MaterialStatus = newstatus;
                NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("Set Mask ({0}) in slot ({1}) as ({2}).", maskEntity.MaterialID, maskEntity.MaterialSlotNo, maskEntity.MaterialStatus.ToString()));

                //把回收桶內相同的刪除
                if (maskTransList.Count > 0)
                {
                    for (int i0 = 0; i0 < maskTransList.Count; i0++)
                    {
                        if (maskTransList[i0].MASKNAME == newname)
                        {
                            maskTransList.RemoveAt(i0);
                            break;
                        }
                    }
                }

                //20141215 cy add:標記機台使用中的mask id
                if (maskEntity.MaterialStatus == eMaterialStatus.INUSE)
                    eqp.InUseMaskID = maskEntity.MaterialID;
                //20141218 cy:Dismount時要叫用Manager以刪除集合
                //else if (maskEntity.MaterialStatus == eMaterialStatus.DISMOUNT)
                //    ObjectManager.MaterialManager.UnMountMaterial(maskEntity);

                ObjectManager.MaterialManager.EnqueueSave(maskEntity);
            }

            //回收桶內若還有資料，dismount掉
            if (maskTransList.Count > 0)
            {
                for (int i0 = 0; i0 < maskTransList.Count; i0++)
                {
                    maskTransList[i0].MASKSTATE = eMaterialStatus.DISMOUNT.ToString();
                    //maskRptList.Add(maskTransList[i0]); modify for avoid dismount other line's mount mask 2016/03/24
                }
            }

            //check need rpt
            if (maskRptList.Count == 0)
            {
                if (waitack)
                {
                    TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack OK
                }
                return; //no need rpt mes
            }

            //20150421 cy:Add recoder to material history
            #region Add recoder to material history
            try
            {
                foreach (MaskStateChanged.MASKc m in maskRptList)
                {
                    MaterialEntity me = new MaterialEntity();
                    me.MaterialType = _common.GetMaterialType(eqp); 
                    me.EQType = eMaterialEQtype.MaskEQ;
                    me.NodeNo = eqp.Data.NODENO;
                    me.UnitNo = "0";
                    me.MaterialID = m.MASKNAME;
                    me.MaterialSlotNo = m.MASKPOSITION;
                    me.MaterialPort = m.MASKPOSITION; //port為key值,避免有問題,所以讓port = slot
                    me.MaterialStatus = (eMaterialStatus)Enum.Parse(typeof(eMaterialStatus), m.MASKSTATE);

                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, "0", me, string.Empty, tid);
                }
            }
            catch { }
            #endregion

            //check host on-line
            Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
            if (line == null)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Can not find Line ID({0}) in LineEntity! TrxID ({1}]", eqp.Data.LINEID, tid));
                TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 1); //ack ng
                return;
            }
            else if (line != null && line.File.HostMode == eHostMode.OFFLINE)
            {
                if (waitack)
                {
                    if (ParameterManager["OFFLINEREPLYEQP"].GetBoolean())
                    {
                        TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack 
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            "Host offline and BCS reply mask status change result by setting. (OK)");
                    }
                    else
                    {
                        TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 1); //ack 
                        TS10F3_H_TerminalDisplaySingle(eqp.Data.NODENO, eqp.Data.NODEID, "Host offline and BCS reply mask status change result by setting. (NG)", tid, string.Empty);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            "Host offline and BCS reply mask status change result by setting. (NG)");
                    }
                }
                return;
            }

            //呼叫MES方法
            object[] _data = new object[7]
            { 
                tid,  /*0 TrackKey*/
                eqp.Data.LINEID,    /*1 LineName*/
                eqp.Data.NODEID,    /*2 EQPID*/
				"", /*3 machineRecipeName*/
				"", /*4 eventUse*/
				maskRptList, /*5 masklist*/
                "SecsMaskStatusChangeReport" /*6 request key*/
            };
            Invoke(eServiceName.MESService, "MaskStateChanged", _data);

            //check if need wait ack
            if (waitack)
            {
                string timerId = string.Format("SecsMaskStatusChangeReport_{0}_{1}_MaskStateChanged", eqp.Data.NODENO, tid);
                if (_timerManager.IsAliveTimer(timerId))
                {
                    _timerManager.TerminateTimer(timerId); //remove old
                }
                //create wait timer
                _timerManager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(MES_MaskStateChangedReplyTimeOut), Tuple.Create(systembyte, string.Empty));
            }
        }

        //timeout handler
        private void MES_MaskStateChangedReplyTimeOut(object subject, System.Timers.ElapsedEventArgs e)
        {
            UserTimer timer = subject as UserTimer;
            if (timer == null)
            {
                return;
            }
            string[] arr = timer.TimerId.Split('_');
            if (arr.Length < 3)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("Invalid TimerId({0}).", timer.TimerId));
                return;
            }
            string eqpno = arr[1];
            string tid = arr[2];
            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
            if (eqp == null)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can not find Equipment No({0}) in EquipmentEntity! TrxId({1}]", eqpno, tid));
                return;
            }

            Tuple<string, string> tuple = timer.State as Tuple<string, string>;
            if (tuple != null)
            {
                string systembyte = tuple.Item1;

                //mes timeout,ack eq ng
                TS6F12_H_EventReportAcknowledge(eqpno, eqp.Data.NODEID, tid, systembyte, 1); //ack ng
            }
            //Send message display
            TS10F3_H_TerminalDisplaySingle(eqp.Data.NODENO, eqp.Data.NODEID, "Host check mask status timeout.", tid, string.Empty);
        }

        #region [Convert Data]
        private eMaterialStatus ConvertCsotMaterialStatus(string state, bool emptyName)
        {
            eMaterialStatus rtn = eMaterialStatus.NONE;
            //rtn = (eMaterialStatus)Enum.Parse(typeof(eMaterialStatus), ConstantManager["NIKONSECS_MASKSTATE"][state].Value);
            switch (state)
            {
                case "0":
                    if (!emptyName)
                        rtn = eMaterialStatus.DISMOUNT;
                    else
                        rtn = eMaterialStatus.NONE;
                    break;
                case "1":
                    if (!emptyName)
                        rtn = eMaterialStatus.MOUNT;
                    else
                        rtn = eMaterialStatus.NONE;
                    break;
                case "6":
                    rtn = eMaterialStatus.INUSE;
                    break;
                case "2":
                case "3":
                case "4":
                case "5":
                    rtn = eMaterialStatus.PREPARE;
                    break;
            }
            return rtn;
        }

        private eEQPStatus ConvertCsotEquipmentStatus(string state)
        {
            eEQPStatus rtn = eEQPStatus.NOUNIT;
            rtn = (eEQPStatus)Enum.Parse(typeof(eEQPStatus), ConstantManager["NIKONSECS_PROCESSSTATE"][state].Value);
            //switch (state)
            //{
            //    case "0":
            //    case "2":
            //        rtn = eEQPStatus.SETUP;
            //        break;
            //    case "1":
            //    case "9":
            //        rtn = eEQPStatus.STOP;
            //        break;
            //    case "3":
            //        rtn = eEQPStatus.IDLE;
            //        break;
            //    case "4":
            //    case "5":
            //    case "8":
            //        rtn = eEQPStatus.RUN;
            //        break;
            //    case "6":
            //    case "7":
            //        rtn = eEQPStatus.PAUSE;
            //        break;
            //}
            return rtn;
        }

        private string ConvertDescriptionACKC10(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Accepted for display";
                    break;
                case "1":
                    rtn = "Message will not be display";
                    break;
                case "2":
                    rtn = "Terminal not available";
                    break;
            }
            return rtn;
        }

        private string ConvertAlarmCategories(string val)
        {
            string rtn = "Other";
            switch (val)
            {
                case "0":
                    rtn = "Not used";
                    break;
                case "1":
                    rtn = "Personal safety";
                    break;
                case "2":
                    rtn = "Equipment safety";
                    break;
                case "3":
                    rtn = "Parameter control warning";
                    break;
                case "4":
                    rtn = "Parameter control error";
                    break;
                case "5":
                    rtn = "Irrecoverable error";
                    break;
                case "6":
                    rtn = "Equipment status warning";
                    break;
                case "7":
                    rtn = "Attention flags";
                    break;
                case "8":
                    rtn = "Data integrity";
                    break;
                case "64":
                    rtn = "Recoverable error";
                    break;
            }
            return rtn;
        }

        private string ConvertMaskSlotState(string state)
        {
            string rtn = "Unknow";
            switch (state)
            {
                case "0":
                    rtn = "Slot Empty";
                    break;
                case "1":
                    rtn = "Slot Occupied with Case";
                    break;
            }
            return rtn;
        }

        private string ConvertMaskAllocateState(string state)
        {
            string rtn = "Unknow";
            switch (state)
            {
                case "0":
                    rtn = "Not Allocated";
                    break;
                case "1":
                    rtn = "Allocated to Equipment";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionDRACK(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Accepted";
                    break;
                case "1":
                    rtn = "Denied, space insufficient";
                    break;
                case "2":
                    rtn = "Invalid format";
                    break;
                case "3":
                    rtn = "Denied, at least one RPTID is already defined";
                    break;
                case "4":
                    rtn = "Denied, at least one VID does not exist";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionLRACK(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Accepted";
                    break;
                case "1":
                    rtn = "Denied, space insufficient";
                    break;
                case "2":
                    rtn = "Invalid format";
                    break;
                case "3":
                    rtn = "Denied, at least one CEID is already defined";
                    break;
                case "4":
                    rtn = "Denied, at least one CEID does not exist";
                    break;
                case "5":
                    rtn = "Denied, at least one RPTID does not exist";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionERACK(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Accepted";
                    break;
                case "1":
                    rtn = "Denied, at least one CEID does not exist";
                    break;
            }
            return rtn;
        }

        private string ConvertPPChangeState(string val)
        {
            string rtn = "Unknow";
            switch (val)
            {
                case "1":
                    rtn = "Created";
                    break;
                case "2":
                    rtn = "Edited";
                    break;
                case "3":
                    rtn = "Deleted";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionHCACK(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Acknowledged. Command has been executed";
                    break;
                case "1":
                    rtn = "Command does not exist";
                    break;
                case "2":
                    rtn = "Command cannot be executed now";
                    break;
                case "3":
                    rtn = "At least one parameter is invalid";
                    break;
                case "4":
                    rtn = "Acknowledged. Command is executed and the completion is to be advised by the event";
                    break;
                case "5":
                    rtn = "Denied. Already in requested status";
                    break;
                case "6":
                    rtn = "The specified object does not exit";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionCPACK(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "1":
                    rtn = "Parameter name does not exist";
                    break;
                case "2":
                    rtn = "Illegal value was specified for the CPVAL";
                    break;
                case "3":
                    rtn = "Illegal format was specified for the CPVAL";
                    break;
            }
            return rtn;
        }

        private string ConvertJobEntryCategory(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Entered by an operator";
                    break;
                case "1":
                    rtn = "Reservation notice of the job reservation system";
                    break;
                case "2":
                    rtn = "Entry notice of CIM communication";
                    break;
            }
            return rtn;
        }

        private string ConvertJobCancelCategory(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Canceled by an operator";
                    break;
                case "1":
                    rtn = "Cancellation notice of the job reservation system";
                    break;
                case "2":
                    rtn = "Cancellation notice of CIM communication";
                    break;
            }
            return rtn;
        }

        private string ConvertJobOperateState(string state)
        {
            string rtn = "Unknow";
            switch (state)
            {
                case "1":
                    rtn = "Changed";
                    break;
                case "2":
                    rtn = "Inserted";
                    break;
                case "3":
                    rtn = "Replaced";
                    break;
            }
            return rtn;
        }

        private string ConvertJobConditionAlignmentMethod(string state)
        {
            string rtn = "Unknow";
            switch (state)
            {
                case "1":
                    rtn = "1st";
                    break;
                case "2":
                    rtn = "EGA";
                    break;
                case "3":
                    rtn = "c-EGA";
                    break;
            }
            return rtn;
        }

        private string ConvertJobConditionEGAFixMode(string state)
        {
            string rtn = "Unknow";
            switch (state)
            {
                case "1":
                    rtn = "No";
                    break;
                case "2":
                    rtn = "Orthogonality";
                    break;
                case "5":
                    rtn = "Scaling and Orthogonality";
                    break;
            }
            return rtn;
        }

        private string ConvertDescriptionEAC(string val)
        {
            string rtn = "Reserved";
            switch (val)
            {
                case "0":
                    rtn = "Accepted";
                    break;
                case "1":
                    rtn = "Denied, at least one constant does not exist";
                    break;
                case "2":
                    rtn = "Denied, busy";
                    break;
                case "3":
                    rtn = "Denied, at least one constant is outside the range";
                    break;
            }
            return rtn;
        }

        private string ConvertRequestSpooledAck(string val)
        {
            string rtn = "Unknow";
            switch (val)
            {
                case "0":
                    rtn = "OK";
                    break;
                case "1":
                    rtn = "Denied,busy,retry";
                    break;
                case "2":
                    rtn = "Denied,spooled data does not exist";
                    break;
            }
            return rtn;
        }

        private string ConvertResetSpoolingRspAck(string val)
        {
            string rtn = "Unknow";
            switch (val)
            {
                case "0":
                    rtn = "Acknowledge, spooling setup accepted";
                    break;
                case "1":
                    rtn = "Spooling setup rejected";
                    break;
            }
            return rtn;
        }

        private string ConvertResetSpoolingStrAck(string val)
        {
            string rtn = "Unknow";
            switch (val)
            {
                case "1":
                    rtn = "Spooling not allowed for stream";
                    break;
                case "2":
                    rtn = "Stream unknown";
                    break;
                case "3":
                    rtn = "Unknown function specified for this stream";
                    break;
                case "4":
                    rtn = "Secondary function specified for this stream is not spooled";
                    break;
            }
            return rtn;
        }
        #endregion
    }
}
