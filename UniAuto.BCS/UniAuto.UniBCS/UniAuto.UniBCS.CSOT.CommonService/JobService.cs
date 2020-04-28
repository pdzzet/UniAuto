using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.MISC;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class JobService : AbstractService
    {
        private const string SendOutTimeout = "SendOutTimeout";
        private const string ReceiveTimeout = "ReceiveTimeout";
        private const string StoreTimeout = "StoreTimeout";
        private const string FetchTimeout = "FetchTimeout";
        private const string ProcCompTimeout = "ProcCompTimeout"; //CELL Specieal Q time Trigger
        private const string RemoveTimeout = "RemoveTimeout";
        private const string JobEditTimeout = "JobDataEditReportTimeout";
        private const string FixCassetteSeqForSECS = "65535";

        private enum eLastFlag
        {
            NotLastGlass = 0,
            LastGlass = 1
        }

        public class keyEQPEvent
        {
            public const string SendOutJobDataReport = "SendOutJobDataReport";
            public const string ReceiveJobDataReport = "ReceiveJobDataReport";
            public const string FetchOutJobDataReport = "FetchOutJobDataReport";
            public const string StoreJobDataReport = "StoreJobDataReport";
            public const string RemoveJobDataReport = "RemoveJobDataReport";
            public const string JobDataEditReport = "JobDataEditReport";
        }

        public override bool Init()
        {
            return true;
        }

        public void SendOutJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                //Watson Add 20150413 For CSOT 要求記錄Event Name 
                //string[] eventName = inputData.Name.Split(new char[] { '_' });

                string commandNo = inputData.Name.Split(new char[] { '#' })[1];

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] REPORT#{2}.",
                        inputData.Metadata.NodeNo, inputData.TrackKey, commandNo));
                    SendOutJobDataReportReply(inputData.Metadata.NodeNo, commandNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #region [拆出PLCAgent Data]  Word
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] REPORT#{3} CST_SEQNO=[{4}] JOB_SEQNO=[{5}] GLASS_ID=[{6}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, commandNo, casseqno, jobseqno, glsID));

                SendOutJobDataReportReply(inputData.Metadata.NodeNo, commandNo, eBitResult.ON, inputData.TrackKey);

                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                #region   //20170721 modify by qiumin forDRY TACTTIME,20180502 by qiumin for ELA TACTTIME
                if (job != null && (line.Data.LINENAME.Contains("DRY") || line.Data.LINENAME.Contains("ELA")) && (eqp.Data.NODENO.Contains("L4") || eqp.Data.NODENO.Contains("L5")))
               
                {
                    lock (eqp.File)
                    {
                        eqp.File.LastSendOutGlassTime = DateTime.Now;
                    }
                    

                }
                    #endregion

                if (job == null)
                {
                    if ((casseqno == "0") || (jobseqno == "0"))
                    {

                        //if (eventName.Length > 1)
                        //    RecordJobHistoryForErrorData(inputData, string.Empty, string.Empty, string.Empty, eventName[1]);
                        //else
                        RecordJobHistoryForErrorData(inputData, string.Empty, string.Empty, string.Empty, eJobEvent.SendOut.ToString() + "#" + commandNo, inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] SENDOUTJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, casseqno, jobseqno, glsID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { line.Data.LINEID, err });
                    }
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    //job.GlassChipMaskBlockID = glsID; //Watson Modify 20150421 不得更新WIP ，HISTORY
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, AND BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            eqp.Data.NODENO, casseqno, jobseqno));
                }

                if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                {
                    #region Check MPLCInterlock
                    Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { inputData.Metadata.NodeNo, "", "SEND" });
                    #endregion
                }

                #region[VCRLossCheck]
                //20180625 BY HUANGJIAYIN
                if ((line.Data.JOBDATALINETYPE == eJobDataLineType.CELL.CCCUT && eqp.Data.NODENO == "L2")
                    || (line.Data.JOBDATALINETYPE == eJobDataLineType.CELL.CCPOL && eqp.Data.NODENO == "L3")
                    || (line.Data.JOBDATALINETYPE == eJobDataLineType.CELL.CCQUP && eqp.Data.NODENO == "L2"))
                {
                    if (ParameterManager.ContainsKey("VCRLossCheckFlag"))
                    {
                        if (ParameterManager["VCRLossCheckFlag"].GetBoolean()&&int.Parse(job.CassetteSequenceNo)<50000)
                        {
                            Invoke(eServiceName.VCRService, "VCRReadLossCheck", new object[] { line, job, eqp, inputData.TrackKey });
                        }
                    }
                }
                #endregion


                #region Endtime
                //if (eqp.Data.NODEATTRIBUTE != "LD" && eqp.Data.NODEATTRIBUTE != "UD" && eqp.Data.NODEATTRIBUTE != "LU")
                #region Job Process Flow
                lock (job.JobProcessFlows)
                {
                    if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                    {
                        ProcessFlow pcf = new ProcessFlow();
                        pcf.MachineName = eqp.Data.NODEID;
                        pcf.EndTime = DateTime.Now;
                        job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                    }
                    else
                    {
                        if (eqp.Data.NODEATTRIBUTE != "LU") //非Load
                            job.JobProcessFlows[eqp.Data.NODEID].EndTime = DateTime.Now;
                    }
                }
                #endregion
                //记录专门For OEE 不能使用同一个ProcessFlow 20150527 Tom
                #region 记录专门For OEE 不能使用同一个ProcessFlow For OEE
                lock (job.JobProcessFlowsForOEE)
                {
                    if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                    {
                        ProcessFlow pcf = new ProcessFlow();
                        pcf.MachineName = eqp.Data.NODEID;
                        pcf.EndTime = DateTime.Now;
                        job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                    }
                    else
                    {
                        //if (eqp.Data.NODEATTRIBUTE != "LU") //非Load
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                    }

                }
                #endregion

                #endregion

                #region Qtime 確認
                if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.SendOutEvent, eqp.File.CurrentRecipeID))
                {
                    //to do
                    //EX:job.EQPFlag.QtimeFlag = true;
                }
                #endregion

                UpdateJobDatabyLineType(job, inputData);

                #region Report Defect Code, 要在報Send out 給MES之前
                if (eqp.File.DefectReport)
                {
                    lock (eqp) eqp.File.DefectReport = false;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    switch (line.Data.LINETYPE)
                    {
                        case eLineType.ARRAY.MAC_CONTREL: Invoke(eServiceName.MESService, "GlassChangeMACOJudge",
                            new object[] { inputData.TrackKey, line.Data.LINEID, job }); break;
                        //Add AOH 20150529 tom
                        case eLineType.ARRAY.AOH_HBT:
                        case eLineType.ARRAY.AOH_ORBOTECH:
                        case eLineType.ARRAY.FLR_CHARM:
                        case eLineType.CF.FCPSH_TYPE1:
                        case eLineType.CF.FCREP_TYPE1:
                        case eLineType.CF.FCREP_TYPE2:
                        case eLineType.CF.FCREP_TYPE3:// 20160509 Add by Frank
                            Invoke(eServiceName.MESService, "DefectCodeReportByGlass", new object[] { inputData.TrackKey, line.Data.LINEID, job }); break;
                        default:
                            // T3 defect都走file sy mark 20160111
                            //if (line.Data.FABTYPE == eFabType.CELL.ToString())  //By EQP Select 做在function裏
                            //    CELLDefectCodeReport(inputData, line, eqp, job);
                            break;
                    }
                }
                #endregion

                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                {
                    #region MES Data Send Product Out
                    /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
                    /// <param name="lineName">Line ID</param>
                    /// <param name="job">JOB(WIP)</param>
                    /// <param name="currentEQPID">Process EQPID</param>
                    /// <param name="unitID">Process UNITID</param>
                    /// <param name="traceLvl">Process Level ‘M’ – Machine ,‘U’ – Unit ,‘P’ – Port</param>
                    /// <param name="processtime">traceLvl='P' is empty,</param>
                    object[] _data = new object[7]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,               /*2 WIP Data */
                    eqp.Data.NODEID,    /*3 EQP ID */
                    "",               /*4 PortID */
                    "",               /*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                };
                    #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                    {
                        if (commandNo == "01")
                        {
                            _data[1] = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                        }
                    }
                    #endregion
                    //Send MES Data
                    object retVal = base.Invoke(eServiceName.MESService, "ProductOut", _data);
                    #endregion
                    //Send APC Data// sy add 20161003 APC 修改 不同於MES
                    #region [APC Data Send Product Out]
                    if (ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO) != null)//20161003 sy add profile 沒資料就不用 上報 by 勝杰
                    {
                        object[] _data_APC = new object[8]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,               /*2 WIP Data */
                    eqp.Data.NODEID,    /*3 EQP ID */
                    "",               /*4 PortID */
                    eqp.Data.NODEID, /*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                    0,/*7 Position */
                };
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        { }
                        else
                        {
                            base.Invoke(eServiceName.APCService, "ProductOut", _data_APC);
                        }
                    }
                    #endregion
                }

                #region Save History
                //增加PathNO 记录到HISTORY
                //if (eventName.Length >1)
                //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eventName[1]);
                //else
                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.SendOut.ToString() + "#" + commandNo, inputData.TrackKey);
                #endregion

                #region Short Cut - Recipe Parameter Request
                if (!job.CfSpecial.CFShortCutRecipeIDCheckFlag)
                {
                    CFShortCutRecipeIDCheck(inputData.TrackKey, line, eqp, job);
                }

                if (!job.CfSpecial.CFShortCutRecipeParameterCheckFlag)
                {
                    CFShortCutRecipeParameterCheck(inputData.TrackKey, line, eqp, job);
                }
                #endregion

                #region Cell Speciall Check Report
                CELL_SendReceiveEvent_Report_Check(inputData, eqp, job);
                #endregion

                #region Array Photo Special
                switch (line.Data.LINETYPE)
                {
                    case eLineType.ARRAY.PHL_TITLE:
                    case eLineType.ARRAY.PHL_EDGEEXP:
                        {
                            if (eqp.Data.NODENO == "L5" || eqp.Data.NODENO == "L6")
                            {
                                lock (job) job.ArraySpecial.PhotoIsProcessed = true;
                            }
                        }
                        break;
                }
                #endregion

                #region CF Photo Line Inline Rework Special Rule
                //20160920 Add by 邱敏 CV#08 Sned Out時，將原先的OXR改成MES Download的原始值
                if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1)
                {
                    if (eqp.Data.NODENO == "L24")
                    {
                        /*
                        string oxrInfo = string.Empty;
                        for (int i = 0; i < job.MesProduct.SUBPRODUCTGRADES.Length; i++)
                        {
                            oxrInfo += ConstantManager["PLC_OXRINFO_AC"][job.MesProduct.SUBPRODUCTGRADES.Substring(i, 1)].Value;
                        }
                        */
                        lock (job)
                            job.OXRInformation = job.MesProduct.SUBPRODUCTGRADES; //modify 20161004
                    }
                }
                #endregion

                //t3 新增 OEE Message  在机台Sent out 后上报 机台经过的当前机台的Unit 的时间 20151211 Tom
                if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                {
                    if (!job.OEESendFlag) //if indexer has report, not need report again when send 2016/01/25 cc.kuang
                        Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job });
                }
                else
                {
                    Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job });
                }
                job.OEESendFlag = true;
                
                //20150421 tom  Cut Line L2 Report ProductInOutTotal
                #region Cut Line L2 Report ProductInOutTotal
                //switch (line.Data.LINETYPE)
                //{
                //    case eLineType.CELL.CBCUT_1:
                //    case eLineType.CELL.CBCUT_2:
                //    case eLineType.CELL.CBCUT_3:
                //        {
                //            if (eqp.Data.NODENO == "L2")
                //            {
                //                Invoke(eServiceName.OEEService, "ProductInOutTotal", new object[] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, "", job, eProductInOutTotalFlag.CUTTING });
                //            }
                //        }
                //        break;
                //}
                #endregion

                //20170626 by huangjiayin  PDR Line Pre-Fetch Special Delay Time
                #region PDR Line Pre-Fetch Special Delay Time
                if (line.Data.LINETYPE == eLineType.CELL.CCPDR)
                {
                    if (eqp.Data.NODENO=="L2"&& commandNo!="03" )
                    {
                        string timeoutname = "PDRPrefetchTimeout";
                        if (!_timerManager.IsAliveTimer(timeoutname))
                        {
                            //timer not alive, send ok glass start timer
                            if (int.Parse(job.CellSpecial.CurrentRwkCount) < int.Parse(job.CellSpecial.MaxRwkCount))
                            {
                                _timerManager.CreateTimer(timeoutname, false, ParameterManager["PDR_PREFETCH_TIMER"].GetInteger() * 1000, new System.Timers.ElapsedEventHandler(PDRPrefetchTimeout), inputData.TrackKey);
                                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS ->RCS][{1}] PDRPrefetchTimer Started[{2}sec], Can not do pre_fetch command!", "CCPDR10B", inputData.TrackKey, ParameterManager["PDR_PREFETCH_TIMER"].GetInteger()));
                            }

                        }
                        else
                        {   //timer is alive, send ng glass terminate timer
                            //for pdr#2 one pdr in ok glass, another pdr in ng glass,terminate timer
                            if (int.Parse(job.CellSpecial.CurrentRwkCount) >=int.Parse(job.CellSpecial.MaxRwkCount))
                            {
                                _timerManager.TerminateTimer(timeoutname);
                                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS ->RCS][{1}] PDRPrefetchTimer terminated by NG glass", "CCPDR10B", inputData.TrackKey));
                            }
                        }

                     }
 
                }

                #endregion

                //add by qiumin 20180711 for Array PHL PR.No check
                #region for Array PHL PR.No check
                if (eqp.Data.NODEID.Contains("TCPHL") && (eqp.Data.NODENO=="L2"))
                {
                    if (string.IsNullOrEmpty(eqp.File.LastReceiveGlassCstSqNo))
                    {
                        lock (eqp)
                            eqp.File.LastReceiveGlassCstSqNo = job.CassetteSequenceNo;
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    }
                    if (eqp.File.PhlDelayCount>0)
                    {
                        if (job.CassetteSequenceNo != eqp.File.LastReceiveGlassCstSqNo)
                        {
                            lock (eqp)
                                eqp.File.PhlDelayCount--;
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS ->BCS] PHL Delay Count[{1}]", eqp.Data.NODENO, eqp.File.PhlDelayCount.ToString()));
                        }
                    }
                    else if (eqp.File.PhlHoldCount > 0)
                    {
                        if (job.CassetteSequenceNo != eqp.File.LastReceiveGlassCstSqNo)
                        {
                            lock (eqp)
                                eqp.File.PhlHoldCount --;
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS ->BCS] PHL Hold Count[{1}]", eqp.Data.NODENO, eqp.File.PhlHoldCount.ToString()));
                        }
                        HoldInfo _hd = new HoldInfo();
                        _hd.NodeID = line.Data.LINEID;
                        _hd.OperatorID = "BCAuto";
                        switch(eqp.File.PhlHoldReason)
                        {
                            case 1:
                                _hd.HoldReason = "Material Type have changed，please check the Material Type and measure the CD";
                                break;
                            case 2:
                                _hd.HoldReason = "PR Material Lot ID have changed，please measure the CD";
                                break;
                            case 3:
                                _hd.HoldReason = "PLN Material Lot ID have changed，please measure the CD";
                                break;
                            default:
                                _hd.HoldReason = "Material Type is not match,please check the BARCODE";
                                break;
                        }                        
                        job.HoldInforList.Add(_hd);
                    }
                    lock (eqp)
                        eqp.File.LastReceiveGlassCstSqNo = job.CassetteSequenceNo;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                }
                #endregion

                //add by hujunpeng 20190723 for PI JOB COUNT MONITOR
                #region PI JOB COUNT MONITOR
                if (line.Data.LINEID=="CCPIL100"||line.Data.LINEID=="CCPIL200")
                {
                    lock (eqp.File)
                    {
                        if (eqp.Data.NODENO == "L2" && job.MesProduct.OWNERTYPE.ToUpper() == "OWNERP" && (job.JobType == eJobType.TFT || job.JobType == eJobType.CF))
                        {
                            string PRODGROUP = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME + job.MesProduct.GROUPID;
                            //eqp.File.PIJobCount = new Dictionary<string, Tuple<int, int>>();
                            if (eqp.File.PIJobCount.Count <= 0)
                            {
                                if (job.JobType == eJobType.TFT)
                                {
                                    Tuple<int, int> jobcount = new Tuple<int, int>(1, 0);
                                    eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                }
                                else
                                {
                                    Tuple<int, int> jobcount = new Tuple<int, int>(0, 1);
                                    eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                }
                            }
                            else
                            {
                                if (!eqp.File.PIJobCount.ContainsKey(PRODGROUP))
                                {
                                    if (job.JobType == eJobType.TFT)
                                    {
                                        Tuple<int, int> jobcount = new Tuple<int, int>(1, 0);
                                        eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                    }
                                    else
                                    {
                                        Tuple<int, int> jobcount = new Tuple<int, int>(0, 1);
                                        eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                    }
                                }
                                else
                                {
                                    if (job.JobType == eJobType.TFT)
                                    {
                                        int tftcount = eqp.File.PIJobCount[PRODGROUP].Item1;
                                        tftcount++;
                                        Tuple<int, int> jobcount = new Tuple<int, int>(tftcount, eqp.File.PIJobCount[PRODGROUP].Item2);
                                        eqp.File.PIJobCount.Remove(PRODGROUP);
                                        eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                    }
                                    else
                                    {
                                        int cfcount = eqp.File.PIJobCount[PRODGROUP].Item2;
                                        cfcount++;
                                        Tuple<int, int> jobcount = new Tuple<int, int>(eqp.File.PIJobCount[PRODGROUP].Item1, cfcount);
                                        eqp.File.PIJobCount.Remove(PRODGROUP);
                                        eqp.File.PIJobCount.Add(PRODGROUP, jobcount);
                                    }
                                }
                                if (eqp.File.PIJobCount.Count == 4)
                                {
                                    string key = eqp.File.PIJobCount.Keys.First();
                                    eqp.File.PIJobCount.Remove(key);
                                }
                            }
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        }
                        Invoke(eServiceName.UIService, "PIJobCountReport", new object[] { inputData.TrackKey, eqp.File.PIJobCount });
                    }
                }
                

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ReceiveJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L4");
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Watson Add 20150413 For CSOT 要求記錄Event Name 
                //string[] eventName = inputData.Name.Split(new char[] { '_' });

                string commandNo = inputData.Name.Split(new char[] { '#' })[1];

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] REPORT#{2}.",
                        inputData.Metadata.NodeNo, inputData.TrackKey, commandNo));
                    ReceiveJobDataReply(inputData.Metadata.NodeNo, commandNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string casseqno = inputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;
                string producttype = inputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] REPORT#{3} CST_SEQNO=[{4}] JOB_SEQNO=[{5}] GLASS_ID=[{6}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, commandNo, casseqno, jobseqno, glsID));

                ReceiveJobDataReply(inputData.Metadata.NodeNo, commandNo, eBitResult.ON, inputData.TrackKey);

                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                bool _timeout = eqp.File.TactTimeOut;
                lock (eqp.File)
                {
                    eqp.File.FinalReceiveGlassID = glsID;
                    if (job != null && ObjectManager.LineManager.GetLines()[0] != null && ObjectManager.LineManager.GetLines()[0].Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                    {
                        //區分是不是本線Job來決定要不要Update
                        int ELALine;   //modify for ELA all line by yang 2017/5/23
                        if (int.Parse(job.CassetteSequenceNo) <= 4000) 
                        { ELALine = 1; }
                        else
                        //{ ELALine = "TCELA200"; }
                        { ELALine = 0; }
                        int linenu;
                        if (int.TryParse(eqp.Data.LINEID.Substring(5, 1), out linenu))
                        {
                            if (job.ArraySpecial.ProcessType.Trim() == "0" && linenu % 2 == ELALine) // just keep prod for robot control fetch glass from cst 2016/01/06 cc.kuang
                            {
                                eqp.File.FinalReceiveGlassTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);
                            }
                        }
                    }
                    else
                    {
                        eqp.File.FinalReceiveGlassTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now); //2015/11/11 cc.kuang
                    }
                    eqp.File.TactTimeOut = false;
                }

                

                if (job == null)
                {
                    if ((casseqno == "0") || (jobseqno == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, string.Empty, string.Empty, string.Empty, eJobEvent.Receive.ToString() + "#" + commandNo, inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] RECEIVEJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, casseqno, jobseqno, glsID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { eqp.Data.LINEID, err });
                    }
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    //job.GlassChipMaskBlockID = glsID; //Watson Modify 20150421 不得更新WIP ，HISTORY
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, AND BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            eqp.Data.NODENO, casseqno, jobseqno));
                }

                eqp.File.FinalReceiveGlassProcessType = job.ArraySpecial.ProcessType;

                lock (job)//JobProcessStartedTime,CurrentEQPNo同步更新，lock。20190809，Dengrongxiao
                {
                    if ((job.JobType == eJobType.TFT || job.JobType == eJobType.CF) && eqp.Data.NODENO != "L2")
                    {
                        job.ArraySpecial.JobProcessStartedTime = DateTime.Now;
                    }
                    job.CurrentEQPNo = eqp.Data.NODENO;
                    ObjectManager.JobManager.EnqueueSave(job);
                }

                #region Qtime 確認
                if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.ReceiveEvent, eqp.File.CurrentRecipeID))
                {
                    //to do
                    //EX:job.EQPFlag.QtimeFlag = true;
                }
                #endregion

                #region Starttime
                //if (eqp.Data.NODEATTRIBUTE != "LD" && eqp.Data.NODEATTRIBUTE != "LU" && eqp.Data.NODEATTRIBUTE != "UD")
                #region JobProcess Flow Start Time
                lock (job.JobProcessFlows)
                {
                    if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                    {
                        ProcessFlow pcf = new ProcessFlow();
                        pcf.MachineName = eqp.Data.NODEID;
                        pcf.StartTime = DateTime.Now;
                        job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                    }
                    else
                    {
                        if (eqp.Data.NODEATTRIBUTE != "LD") // Load 机台要第一次Receive 时间
                            job.JobProcessFlows[eqp.Data.NODEID].StartTime = DateTime.Now;
                    }
                }
                #endregion

                #region JobProcessFlow For OEE 20150527 Tom
                lock (job.JobProcessFlowsForOEE)
                {
                    if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                    {
                        ProcessFlow pcf = new ProcessFlow();
                        pcf.MachineName = eqp.Data.NODEID;
                        pcf.StartTime = DateTime.Now;
                        job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                    }
                    else
                    {
                        //if (eqp.Data.NODEATTRIBUTE != "LD") // Load 机台要第一次Receive 时间
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].StartTime = DateTime.Now;
                    }
                    job.OEERecvFlag = true; //for store report OEE productinoutM use 2016/01/25 cc.kuang 
                }
                #endregion
                #endregion

                UpdateJobDatabyLineType(job, inputData);
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (_timeout && (line.Data.FABTYPE == eFabType.ARRAY.ToString()))
                {
                    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                }

                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                {
                    #region [MES Data Send]
                    object[] _data = new object[8]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    "",/*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                   ""/*7 Process Time  */
                };
                    #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                    {
                        if (commandNo == "01")
                        {
                            _data[1] = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                        }
                    }
                    #endregion
                    //Send MES Data
                    object retVal = base.Invoke(eServiceName.MESService, "ProductIn", _data);
                    #endregion
                    // sy add 20161003 APC 修改 不同於MES
                    #region [APC Data Send]
                    if (ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO) != null)//20161003 sy add profile 沒資料就不用 上報 by 勝杰
                    {
                        object[] _data_APC = new object[9]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    eqp.Data.NODEID,/*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                    0,/*7 Position */
                   ""/*8 Process Time  */
                };
                        //Send APC Data
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        { }
                        else
                        {
                            base.Invoke(eServiceName.APCService, "ProductIn", _data_APC);
                        }
                    }
                    #endregion
                }
                if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                {
                    #region Check MPLCInterlock
                    Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { inputData.Metadata.NodeNo, "", "RECEIVE" });
                    #endregion
                }
                // add by qiumin 20171222 for array eqp process time start //修改到上面代码，JobProcessStartedTime,CurrentEQPNo同步更新，lock。20190809，Dengrongxiao
                //if ((job.JobType == eJobType.TFT||job.JobType==eJobType.CF) && eqp.Data.NODENO != "L2")
                //{
                //    lock (job)
                //    {
                //        job.ArraySpecial.JobProcessStartedTime = DateTime.Now;
                //    }
                //    ObjectManager.JobManager.EnqueueSave(job);
                //}

                ////add by hujunpeng 20181231 for cf (1300站点THK~DEV LOAD）需要增加Q-time管控功能
                //if(eqp.Data.LINEID=="FCBPH100"||eqp.Data.LINEID=="FCMPH100"||eqp.Data.LINEID=="FCRPH100"||eqp.Data.LINEID=="FCGPH100"||eqp.Data.LINEID=="FCSPH100")
                //{
                //    if (eqp.Data.NODENAME.Contains("CF Thickness") && job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "1300")
                //    {
                //        lock (job)
                //        {
                //            job.CfSpecial.ThicknessReceiveJobTime = DateTime.Now;
                //            ObjectManager.JobManager.EnqueueSave(job);
                //        }
                //    }
                //    if (eqp.Data.NODENAME.Contains("CF Developer") && job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "1300")
                //    {
                //        lock (job)
                //        {
                //            job.CfSpecial.DeveloperReceiveJobTime = DateTime.Now;
                //            ObjectManager.JobManager.EnqueueSave(job);
                //        }
                //        if (ParameterManager.ContainsKey("THKTODEVTIMEMONITOR"))
                //        {
                //            if ((job.CfSpecial.DeveloperReceiveJobTime - job.CfSpecial.ThicknessReceiveJobTime).Minutes > ParameterManager["THKTODEVTIMEMONITOR"].GetInteger())
                //            {
                //                HoldInfo _hd = new HoldInfo();
                //                _hd.NodeID = eqp.Data.NODEID;
                //                _hd.OperatorID = "BCAuto";
                //                _hd.HoldReason = "job from THK to DEV is timeout .";
                //                job.HoldInforList.Add(_hd);
                //            }
                //        }
                //    }
                //}                
                #region Cell Speciall Check Report
                CELL_SendReceiveEvent_Report_Check(inputData, eqp, job);
                #endregion

                #region Save History
                //Save PathNo  20150406 tom
                //if (eventName.Length > 1)
                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Receive.ToString() + "#" + commandNo, inputData.TrackKey);
                //else
                //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Receive.ToString());
                #endregion

                #region[PICMaterialSpecial]
                //20171128 by huangjiayin
                if ((job.JobType == eJobType.TFT || job.JobType == eJobType.CF)&&
                    eqp.Data.NODEID.Contains("CCPIC")
                    )
                {
                    if (string.IsNullOrEmpty(eqp.File.PIMaterialTCProductType))
                    {
                        eqp.File.PIMaterialTCProductType = job.ProductType.Value.ToString();
                    }
                    else if (eqp.File.PIMaterialTCProductType != job.ProductType.Value.ToString())
                    {
                        eqp.File.PIMaterialTCProductType = job.ProductType.Value.ToString();
                        //Add By Yangzhenteng For PIL OPI Display20180905
                        if (!Repository.ContainsKey("PIMaterialWeightRequestCommand"))
                        {
                            Repository.Add("PIMaterialWeightRequestCommand", "PIMaterialWeightRequestCommand");
                        }
                        Invoke(eServiceName.MaterialService, "PIMaterialWeightRequestCommand", new object[]{eqp.Data.NODENO,eBitResult.ON,inputData.TrackKey});
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PIC Receive New Product , invoke PIMaterialWeightRequestCommand.", eqp.Data.NODENO,
                            inputData.TrackKey));
                    }

 
                }
                #endregion
                #region cf producttype 更换时模拟上报MES
                //20180524 add by hujunpeng
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                lock (eqp.File)
                {
                    if (fabType == eFabType.CF)
                    {
                        if (eqp.Data.NODEATTRIBUTE == "COATER")
                        {
                            if (!string.IsNullOrEmpty(eqp.File.LastProductType))
                            {
                                if (eqp.File.LastProductType != producttype)
                                {
                                    //string mystr = "Server=.;user=sa;pwd=itc123!@#;database=UNIBCS_t3";
                                    //SqlConnection mycon = new SqlConnection(mystr);
                                    //try
                                    //{
                                    //    mycon.Open();
                                    //    string sql = "select [NODENAME] from SBRM_NODE where [SERVERNAME]='" + eqp.Data.SERVERNAME + "'and [NODENO]='L6'";
                                    //    string sql1 = "select [NODENAME] from SBRM_NODE where [SERVERNAME]='" + eqp1.Data.SERVERNAME + "'and [NODENO]='L4'";

                                    //    SqlDataAdapter myda = new SqlDataAdapter(sql, mycon);
                                    //    SqlDataAdapter myda1 = new SqlDataAdapter(sql1, mycon);
                                    //    DataSet myds = new DataSet();
                                    //    DataSet myds1 = new DataSet();
                                    //    myda.Fill(myds, "L6");
                                    //    myda1.Fill(myds1, "L4");

                                    //    DataRow row = myds.Tables[0].Rows[0];
                                    //    DataRow row1 = myds1.Tables[0].Rows[0];
                                    //    if (row[0] != null || row1[0] != null)
                                    //    {
                                    //        if (row[0].ToString().Count() != 30)
                                    //        {
                                    //            Logger.LogErrorWrite("JobService", "JobService", "ReceiveJobDataReport", "PRID1长度不是30码");
                                    //            row[0] = "000000000000000000000000000000";
                                    //        }
                                    //        if (row1[0].ToString().Count() != 30)
                                    //        {
                                    //            Logger.LogErrorWrite("JobService", "JobService", "ReceiveJobDataReport", "PRID2长度不是30码");
                                    //            row1[0] = "000000000000000000000000000000";
                                    //        }
                                    if (eqp.File.PrID1.Substring(24, 1) == "I" || eqp.File.PrID2.Substring(24, 1) == "I")
                                    {
                                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                                        MaterialEntity materialE = new MaterialEntity();
                                        materialE.EQType = eMaterialEQtype.Normal;
                                        materialE.NodeNo = eqp.Data.NODENO;
                                        materialE.OperatorID = string.Empty;
                                        materialE.MaterialStatus = eMaterialStatus.INUSE;
                                        if (eqp.File.PrID1.Substring(24, 1) == "I")
                                        {
                                            materialE.MaterialID = eqp.File.PrID1.Substring(0, 24);
                                            materialE.MaterialSlotNo = "1";
                                            materialE.MaterialValue = eqp.File.PrID1.Substring(25, 5);
                                        }
                                        if (eqp.File.PrID2.Substring(24, 1) == "I")
                                        {
                                            materialE.MaterialID = eqp.File.PrID2.Substring(0, 24);
                                            materialE.MaterialSlotNo = "2";
                                            materialE.MaterialValue = eqp.File.PrID2.Substring(25, 5);
                                        }
                                        materialE.UnitNo = "0";
                                        materialE.MaterialPosition = string.Empty;
                                        materialE.MaterialType = "PR";  //2015/8/28 add by Frank CF固定填PR
                                        materialList.Add(materialE);
                                        Invoke(eServiceName.MESService, "MaterialDismountReport", new object[5] { inputData.TrackKey, eqp, glsID, string.Empty, materialList });
                                        Thread.Sleep(2000);
                                        Invoke(eServiceName.MESService, "MaterialMount", new object[6] { inputData.TrackKey, eqp, glsID, string.Empty, string.Empty, materialList });
                                    }



                                    //catch (Exception ex)
                                    //{ Logger.LogErrorWrite("JobService", "JobService", "ReceiveJobDataReport", "PRID1长度不是30码", ex); }
                                    //finally
                                    //{ mycon.Close(); }


                                }
                            }
                            eqp.File.LastProductType = producttype;
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void StoreJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];//huangjiayin add for #02

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                Port port = null;
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Watson Add 20150413 For CSOT 要求記錄Event Name 
                //string[] eventName = inputData.Name.Split(new char[] { '_' });

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    StoreJobDataReportReply(commandNo,inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                string log = string.Empty;

                #region [拆出PLCAgent Data]
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;

                string type = inputData.EventGroups[0].Events[1].Items[0].Value;
                log += string.Format(" UNIT_OR_PORT=[{0}]({1})", type, type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"));
                string unitNo = inputData.EventGroups[0].Events[1].Items[1].Value;
                log += string.Format(" UNIT=[{0}]", unitNo);
                string portNo = inputData.EventGroups[0].Events[1].Items[2].Value;
                log += string.Format(" PORT=[{0}]", portNo);
                string slotNo = inputData.EventGroups[0].Events[1].Items[3].Value;
                log += string.Format(" SLOT_NO=[{0}]", slotNo);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}]{6}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID, log));

                StoreJobDataReportReply(commandNo,inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);


                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                if (job == null)
                {
                    if ((casseqno == "0") || (jobseqno == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, unitNo, portNo.PadRight(2, '0'), slotNo, eJobEvent.Store.ToString(), inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] STOREJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, casseqno, jobseqno, glsID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { eqp.Data.LINEID, err });
                    }
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    //job.GlassChipMaskBlockID = glsID; //Watson Modify 20150421 不得更新WIP ，HISTORY
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadRight(2, '0'), slotNo, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, AND BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            eqp.Data.NODENO, casseqno, jobseqno));
                }

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                Unit unit = null;
                job.CurrentSlotNo = slotNo; // add by bruce 20160412 for T2 Issue
                if (type == "1")
                {
                    if (unitNo != "0")
                    {
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);

                        string err = string.Format("[EQUIPMENT={0}] UnitorPort TYPE=[{1}] BUT UNITNO=[{2}] .{3}",
                                 eqp.Data.NODENO, type, unitNo, string.Format(eLOG_CONSTANT.CAN_NOT_FIND_UNIT, unitNo));
                        if (unit == null) throw new Exception(err);
                        else
                        {
                            if (!string.IsNullOrEmpty(unit.Data.UNITTYPE) && unit.Data.UNITTYPE.ToUpper() == "CHAMBER" && unit.Data.UNITATTRIBUTE.ToUpper() == "NORMAL") 
                                job.ChamberName = unit.Data.UNITID;
                        }
                    }

                    #region Qtime is up確認
                    if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, unit != null ? unit.Data.UNITID : "", eQtimeEventType.StoreEvent, eqp.File.CurrentRecipeID))
                    {
                        //to do
                        //EX:job.EQPFlag.QtimeFlag = true;
                    }
                    #endregion

                    #region Starttime
                    #region Job Process Flow
                    lock (job.JobProcessFlows)
                    {
                        if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit Start Time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.StartTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            #region Update Start Time [已經存過被修改]
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                if (job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime.Now;
                                }
                                else
                                {
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                            }
                            #endregion
                        }

                        //20170809 huangjiayin: move extendjobprocessflows here
                        //for the last fetch out event may be after process data report...

                        #region[EDCUNITINFO]
                        if (unit != null)
                        {
                            ProcessFlow upcf = new ProcessFlow();
                            upcf.MachineName = unit.Data.UNITID;
                            upcf.StartTime = DateTime.Now;
                            upcf.SlotNO = slotNo;
                            job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Add(upcf);
                        }
                        #endregion

                    }
                    #endregion
                    #region Job Process Flow for OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit Start Time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.StartTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            #region Update Start Time [已經存過被修改]
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                if (job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime.Now;
                                }
                                else
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                            }
                            #endregion
                        }
                    }

                    #endregion
                    #endregion

                    #region Update WIP (Unit)需更新什麼資訊，可自行加入);
                    job.CurrentEQPNo = eqp.Data.NODENO;
                    job.CurrentUNITNo = unit.Data.UNITNO;
                    UpdateJobDatabyLineType(job, inputData);
                    #endregion

                    if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    {
                        #region MES APC Data Send Product IN
                        object[] _data = new object[8]
                    { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                     unit !=null ? unit.Data.UNITID:"",/*5 Unit ID */
                    eMESTraceLevel.U,/*6 Trace Level */
                   ""/*7 Process Time */  //DateTime.Now - job.StartDateTime
                    };
                        //Send MES Data
                        object retVal = base.Invoke(eServiceName.MESService, "ProductIn", _data);
                        //Send APC Data // sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                        //base.Invoke(eServiceName.APCService, "ProductIn", _data);
                        #endregion
                    }

                }
                else
                {
                    port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));


                    string err1 = string.Format("[EQUIPMENT={0}] UnitorPort TYPE=[{1}] BUT PORTNO=[{2}] .{3}",
                    eqp.Data.NODENO, type, unitNo, string.Format(eLOG_CONSTANT.CAN_NOT_FIND_PORT, portNo));

                    if (port == null) throw new Exception(err1);

                    #region Qtime is up確認
                    if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.StoreEvent, eqp.File.CurrentRecipeID))
                    {
                        //to do
                        //CELL Send InLine QTime Over Command Block

                    }
                    #endregion

                    #region Endtime
                    //
                    lock (job.JobProcessFlowsForOEE) {
                        if (job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID)) {
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                        } else {    //job.JobProcessFlows.Remove(eqp.Data.NODEID);
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.EndTime = DateTime.Now; //沒有只能代填現在時間
                            pcf.SlotNO = slotNo;
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                        }
                    }
                    #endregion

                    #region Update WIP 需更新什麼資訊，可自行加入
                    job.TargetPortID = port.Data.PORTID;
                    job.ToSlotNo = slotNo;
                    job.ToCstID = port.File.CassetteID;
                    job.CurrentEQPNo = eqp.Data.NODENO;

                    // 2015.02.05
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        if (job.HoldInforList.Count.Equals(0))
                        {
                            HoldInfo hold = new HoldInfo()
                            {
                                NodeNo = eqp.Data.NODENO,
                                NodeID = eqp.Data.LINEID,
                                UnitNo = "0",
                                UnitID = "",
                                HoldReason = "FORCE_CLEAN_OUT_MODE",
                                OperatorID = "",
                            };
                            ObjectManager.JobManager.HoldEventRecord(job, hold);
                        }
                    }

                    UpdateJobDatabyLineType(job, inputData);
                    #endregion

                    #region Change Mode時要主動更新 SlotPlan是否已經做過了
                    if (line != null)
                    {
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        {
                            ObjectManager.PlanManager.HaveBeenUsePlanByProductName(line.File.CurrentPlanID, job.GlassChipMaskBlockID.Trim());
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                    }
                    #endregion

                    #region Sorter Mode更新Source Cassette內相關資訊
                    if (line != null)
                    {
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE)
                        {
                            UpdateSourcePortData(job);
                            port.File.StoreInDateTime = DateTime.Now;
                        }
                    }
                    #endregion

                    if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    {
                        #region MES APC Data Send Product IN
                        double processtime = Math.Round((DateTime.Now - job.JobProcessStartTime).TotalSeconds);
                        object[] _data = new object[8]
                    { 
                        inputData.TrackKey,  /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        job,/*2 WIP Data */
                        eqp.Data.NODEID, /*3 EQP ID */
                        port.Data.PORTID, /*4 PortID */
                        "",/*5 Unit ID */
                        eMESTraceLevel.P,/*6 Trace Level */
                       processtime.ToString()/*7 Process Time */ //DateTime.Now - job.StartDateTime   //PROCESSINGTIME : If TRACELEVEL is ‘P’, BC should report PROCESSINGTIME with none empty value.
                    };
                        //Send MES Data
                        object retVal = base.Invoke(eServiceName.MESService, "ProductIn", _data);
                        //Send APC Data // sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                        //base.Invoke(eServiceName.APCService, "ProductIn", _data);
                        #endregion

                        #region EDCGLASSRUNEND Send to EDA
                        //EDCGLASSRUNEND(string trxID, string lineName, string eqpName,Job job)
                        object[] _edadata = new object[4]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp.Data.NODEID,
                            job,/*2 WIP Data */
                        };
                        //Send EDA Data
                        Invoke(eServiceName.EDAService, "EDCGLASSRUNEND", _edadata);
                        #endregion

                        #region Send ProductInOutTotalM
                        //t3 新增的OEE  Message 在Store  Port 的时候需要上报ProductInOutTotalM  
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        {
                            // EQPRTC, not report productinouttotoalM  2016/11/18 yang 
                            //EQP RTC flow: cst->eq1(->cst->)eq2->cst  ( )中sendout & store not report
                            if (job.RobotWIP.EQPRTCFlag)  
                            { }


                            else if (job.OEERecvFlag ||( line.File.LineOperMode == eMES_LINEOPERMODE.CHANGER))
                            {
                                job.OEERecvFlag = false; //if report productinouttotoalM, reset OEERecvFlag for avoid RTC report again 2016/01/25 cc.kuang
                                //Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, port.Data.LINEID, port.Data.NODEID, port.Data.PORTID, job });

                                        Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, port.Data.LINEID, port.Data.NODEID, port.Data.PORTID, job, eJobEvent.Store }); //use for t3 OEE trcelevel=P rule(only store use P) 2016/04/14 cc.kuang
                            }
                            else if(line.File.LineOperMode == eMES_LINEOPERMODE.CELL_MSORT)
                            {
                                //20171221 Cell SORTMODE也要上报
                                        Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, port.Data.LINEID, port.Data.NODEID, port.Data.PORTID, job });

                            }

                        }
                        else
                        {
                            Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, port.Data.LINEID, port.Data.NODEID, port.Data.PORTID, job });
                        }
                        //if (port.File.Type != ePortType.LoadingPort)//只要不是Load Port 都需要上报
                        //{
                        //    //void ProductInOutTotal(string trxid,string lineName,string machineName,string portName,Job job)

                        //    eProductInOutTotalFlag flag = eProductInOutTotalFlag.NORMAL;
                        //    //甄别出
                        //    switch (line.Data.LINETYPE)
                        //    {
                        //        case eLineType.CELL.CBCUT_1:
                        //        case eLineType.CELL.CBCUT_2:
                        //        case eLineType.CELL.CBCUT_3:
                        //            flag = eProductInOutTotalFlag.UNLOAD;
                        //            break;
                        //    }

                        //    Invoke(eServiceName.OEEService, "ProductInOutTotal", new object[]{
                        //                                                            inputData.TrackKey,
                        //                                                            port.Data.LINEID,
                        //                                                            port.Data.NODEID,
                        //                                                            port.Data.PORTID,
                        //                                                            job,
                        //                                                            flag});
                        //}
                        #endregion
                    }
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)//sy add for pcs BoxIdcreate check again
                    {
                        BoxIDRequestCheck(inputData, line, eqp, job, port);
                    }
                    //20160330 sy add Update port.File.ProductType // 20160330 modify all shop
                    if (port.File.Type == ePortType.UnloadingPort)
                    {
                        port.File.ProductType = job.ProductType.Value.ToString();
                    }
                    ObjectManager.PortManager.EnqueueSave(port.File);
                }

                if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                {
                    #region Check MPLCInterlock
                    if (line != null && line.Data.FABTYPE.ToUpper() != "ARRAY")
                        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { inputData.Metadata.NodeNo, unitNo, "STORE" });
                    #endregion
                }
                #region   [CHANGER_MODE] add by box.zhai 2017/3/2 Store && Arm has no glass 后Check Plan Status Ready
                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);

                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                {
                    Thread.Sleep(200);   //mark by yang
                    if (curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.NoExist && curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.NoExist)
                    {
                        Invoke(eServiceName.CassetteService, "PlanReadyStatusCheckForOnline", new object[4] { line, eqp, inputData.TrackKey,"STORE" });
                    }
                }
                #endregion

                CELL_Robot_FetchStoreEvent_AutoAbortCST(inputData, eqp, port, job);

                //if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)  //add by bruce 2015/09/30 Array ELA 跨line 使用
                //{
                //    if (eqp.Data.NODENO == "L2" && unitNo == "2") //Indexer Turn Table
                //    {
                //        if (job.ArrayELACrossLineID =="")
                //            job.ArrayELACrossLineID = line.Data.LINEID;

                        //object[] _objJob = new object[1] { job };
                        //Invoke(eServiceName.PassiveSocketService, "ArrayShortCut", _objJob);

                        //ARRAY_ShortCutCreateWIP(line, eqp, unitNo, job, inputData.TrackKey);

                        //add by bruce 2015/10/8
                        //string unitId = string.Empty;

                        //if (unit != null)
                        //{
                        //    unitId = unit.Data.UNITID;
                        //}
                        
                        //if (job.ArrayELACrossLineID == line.Data.LINEID)
                        //{
                        //    job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                        //    ObjectManager.JobManager.EnqueueSave(job);
                        //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Recovery.ToString());
                        //    object retVal = base.Invoke(eServiceName.MESService, "ProductLineOut", new object[]{
                        //    inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "", unitId});
                        //}
                        //else
                        //{
                        //    job.RemoveFlag = false;  //Regeistor回來了
                        //    ObjectManager.JobManager.EnqueueSave(job);
                        //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Remove.ToString());
                        //    object retVal = base.Invoke(eServiceName.MESService, "ProductLineIn", new object[]{
                        //    inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID,"", unitId});
                        //}
                        //Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //    string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, inputData.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));

                    //}
                //}
                
                #region Save History
                //if (eventName.Length > 1)
                //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eventName[1]);
                //else
                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Store.ToString(), inputData.TrackKey);
                //20160105 cy:如果Store是Unit, 會出錯, 提到上面去做
                //ObjectManager.PortManager.EnqueueSave(port.File);
                #endregion

                #region Short Cut Special Rule
                //若PermitFlag為N時，當CV#06上報Store，BC直接Reply NG。
                if (eqp.Data.NODEATTRIBUTE == "BF")     
                {
                    if(job.CfSpecial.PermitFlag == "N")
                    {
                        Invoke(eServiceName.CFSpecialService, "GlassOutResultCommand", new object[] { inputData.TrackKey, job, ePermitFlag.N, job.CfSpecial.SamplingValue });
                    }
                }
                #endregion

                #region FEOL 重要Unit Process Time监控 
                //add by hujunpeng 20181119
                if ( eqp.Data.NODENO != "L2")
                {
                    lock (job)
                    {
                        job.CellSpecial.UnitProcessStartTime = DateTime.Now;
                    }
                    ObjectManager.JobManager.EnqueueSave(job);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("job [{0}] unit process start time [{1}] ", job.GlassChipMaskBlockID, job.CellSpecial.UnitProcessStartTime.ToString()));
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void FetchOutJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];//huangjiayin add for #02

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    FetchOutJobDataReportReply(commandNo,inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                string log = string.Empty;

                //Watson Add 20150413 For CSOT 要求記錄Event Name 
                //string[] eventName = inputData.Name.Split(new char[] { '_' });
                #region [拆出PLCAgent Data]
                string casseqno = inputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;
                string type = inputData.EventGroups[0].Events[1].Items[ePLC.FetchOutJobData_UnitOrPort].Value;
                log += string.Format(" UNIT_OR_PORT=[{0}]({1})", type, type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"));
                string unitNo = inputData.EventGroups[0].Events[1].Items[ePLC.FetchOutJobData_UnitNo].Value;
                log += string.Format(" UNIT=[{0}]", unitNo);
                string portNo = inputData.EventGroups[0].Events[1].Items[ePLC.FetchOutJobData_PortNo].Value;
                log += string.Format(" PORT=[{0}]", portNo);
                string slotNo = inputData.EventGroups[0].Events[1].Items[ePLC.FetchOutJobData_SlotNo].Value;
                log += string.Format(" SLOT_NO=[{0}]", slotNo);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}]{6}.",
                   eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID, log));
                #region[FOR CUT BUR]
                //Add By yanzhenteng20180802 For CUT BUR Check
                if (portNo != "0" && eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR))
                {
                    Port _port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));//Add By yangzhenteng20180717
                    if ((_port.Data.PORTID == "09") || (_port.Data.PORTID == "10") || (_port.Data.PORTID == "13") || (_port.Data.PORTID == "14"))
                    {
                        string _glassid = glsID.Trim();
                        Invoke(eServiceName.MESService, "MachienPanelRejudgeFetchOutReport", new object[] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, _glassid, _port.Data.PORTID, slotNo, "" });
                    }
                }
                #endregion
                FetchOutJobDataReportReply(commandNo,inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);


                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                if (job == null)
                {
                    if ((casseqno == "0") || (jobseqno == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, unitNo, portNo.PadRight(2, '0'), slotNo, eJobEvent.FetchOut.ToString(), inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] FETCHJOBEVENT DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, casseqno, jobseqno, glsID);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { line.Data.LINEID, err });
                    }
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    //job.GlassChipMaskBlockID = glsID; //Watson Modify 20150421 不得更新WIP ，HISTORY
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadRight(2, '0'), slotNo, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, AND BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            eqp.Data.NODENO, casseqno, jobseqno));
                }

                Unit unit = null;
                job.CurrentSlotNo = "0";    // add by bruce 20160412 fot T2 Issue
                if (type == "1")
                {
                    if (unitNo != "0")
                    {
                        unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);

                        string err = string.Format("[EQUIPMENT={0}] UnitorPort TYPE=[{1}] BUT UNITNO=[{2}] .{3}",
                        eqp.Data.NODENO, type, unitNo, string.Format(eLOG_CONSTANT.CAN_NOT_FIND_UNIT, unitNo));

                        if (unit == null) throw new Exception(err);
                        else
                        {
                            if (!string.IsNullOrEmpty(unit.Data.UNITTYPE) && unit.Data.UNITTYPE.ToUpper() == "CHAMBER" && unit.Data.UNITATTRIBUTE.ToUpper() == "NORMAL")
                                job.ChamberName = unit.Data.UNITID;

                            job.CurrentUNITNo = string.Empty;
                        }
                    }                    

                    #region Qtime 確認
                    if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, unit != null ? unit.Data.UNITID : "", eQtimeEventType.FetchOutEvent, eqp.File.CurrentRecipeID))
                    {
                        //to do
                        //EX:job.EQPFlag.QtimeFlag = true;
                    }
                    #endregion

                    #region Endtime
                    #region JobProcessFlow
                    lock (job.JobProcessFlows)
                    {
                        if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit End time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.EndTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.EndTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            if (unit != null)
                            {
                                #region Update Unit End time
                                ProcessFlow upcf = new ProcessFlow();
                                if (job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime.Now;
                                }
                                else
                                {
                                    upcf.MachineName = unit.Data.UNITID;
                                    upcf.EndTime = DateTime.Now;
                                    upcf.SlotNO = slotNo;
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                                #endregion
                            }
                        }

                        //for EDC unit list use 2016/03/14 cc.kuang
                        //20170809 huangjiayin modify....
                        //fetchout only update end time
                        if (unit != null)
                        {

                                if (job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Count > 0)
                                {
                                    for(int i=0;i<job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Count ;i++)
                                    {
                                        ProcessFlow pf=job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows[i];
                                        if (pf.MachineName == unit.Data.UNITID && pf.EndTime == DateTime.MinValue)
                                        {
                                            job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows[i].EndTime = DateTime.Now;
 
                                        }
                                    }
                                }
                            }
                        }

                    #endregion
                    #region JobProcessFlow For OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit End time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.EndTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.EndTime = DateTime.Now;
                                upcf.SlotNO = slotNo;
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            if (unit != null)
                            {
                                #region Update Unit End time
                                ProcessFlow upcf = new ProcessFlow();
                                if (job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime.Now;
                                }
                                else
                                {
                                    upcf.MachineName = unit.Data.UNITID;
                                    upcf.EndTime = DateTime.Now;
                                    upcf.SlotNO = slotNo;
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                                #endregion
                            }
                        }

                        //for OEE unit list use 2016/03/14 cc.kuang
                        if (unit != null)
                        {
                            ProcessFlow upcf = new ProcessFlow();
                            upcf.MachineName = unit.Data.UNITID;
                            upcf.StartTime = job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime;
                            upcf.EndTime = job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime;
                            upcf.SlotNO = job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].SlotNO;
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].ExtendUnitProcessFlows.Add(upcf);
                        }
                    }
                    #endregion
                    #endregion

                    UpdateJobDatabyLineType(job, inputData);

                    if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    {
                        #region MES APC Data Send
                        object[] _data = new object[7]
                    { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    unit !=null ? unit.Data.UNITID:"",/*5 Unit ID */
                    eMESTraceLevel.U,/*6 Trace Level */
                    };
                        //Send MES Data
                        object retVal = base.Invoke(eServiceName.MESService, "ProductOut", _data);
                        //Send APC Data // sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                        //base.Invoke(eServiceName.APCService, "ProductOut", _data);
                        #endregion
                    }
                }
                else
                {
                    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));

                    string err1 = string.Format("[EQUIPMENT={0}] UnitorPort TYPE=[{1}] BUT PORTNO=[{2}] .{3}",
                    eqp.Data.NODENO, type, unitNo, string.Format(eLOG_CONSTANT.CAN_NOT_FIND_PORT, portNo));

                    if (port == null) throw new Exception(err1);

                    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR))
                    {
                        //sy add 20160804 BUR Port loader request empty CST & job store in port and fetch out from port 
                    }
                    else if (job.FromCstID.Trim() != port.File.CassetteID.Trim())
                    {                        
                        string err = "Job From Cassette ID <> EQ Report Cassette ID.";
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] FETCHOUTJOBDATAREPORT CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}] NG ,{6}.",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID, err));

                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, line.Data.LINEID, "Fetch Out Job Data Report - NG", 
                        err });
                    }

                    #region ChangerPlan's status = Start Check 2010/10/21 cc.kuang 
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        IList<SLOTPLAN> currPlans;
                        string PlanID;

                        currPlans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
                        if (line.File.CurrentPlanID.Trim().Length > 0 && line.File.PlanStatus == ePLAN_STATUS.READY)
                        {
                            if (currPlans.FirstOrDefault(slot => slot.SOURCE_CASSETTE_ID.Trim() == port.File.CassetteID.Trim()) != null)
                            {
                                line.File.PlanStatus = ePLAN_STATUS.START;
                                ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                if (line.File.HostMode != eHostMode.OFFLINE)
                                {                                     
                                    Invoke(eServiceName.MESService, "ChangePlanStarted", new object[] { inputData.TrackKey, line.Data.LINEID, PlanID });
                                    Thread.Sleep(1000); //move delay time 2016/05/06 cc.kuang
                                }
                                // mark by box.zhai 2017/03/01 Online只有用Source CST去要Plan的Case
                                //if (line.File.HostMode != eHostMode.OFFLINE) //check standby plan, if has any plan nulock port, request standby plan cc.kuang 2015/09/21
                                //{
                                //    IList<SLOTPLAN> Plans;
                                //    string standbyPlanID;
                                //    Plans = ObjectManager.PlanManager.GetProductPlansStandby(out standbyPlanID);
                                //    if (standbyPlanID.Trim().Length == 0)
                                //    {
                                //        if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                //            p => p.File.PlannedCassetteID.Trim().Length == 0) != null)
                                //        {
                                //            Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                                //            new object[] { inputData.TrackKey, line.Data.LINEID, "", PlanID });
                                //        }
                                //    }
                                //}

                                Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                            }
                        }
                    }
                    #endregion

                    #region Qtime 確認
                    if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.FetchOutEvent, eqp.File.CurrentRecipeID))
                    {
                        //to do
                        //EX:job.EQPFlag.QtimeFlag = true;
                    }
                    #endregion

                    #region Starttime
                    //出CST无需记录 JobProcessFlows 20150407 tom 
                    //t3 新增 在Fetch  from Port 后需要记录 Start Time 作为Load 机台的开始时间  20151211 Tom
                    lock (job.JobProcessFlowsForOEE) {
                        ProcessFlow pcf = null;
                        if (job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID)) {
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].StartTime = DateTime.Now;
                        } else {
                            pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.StartTime = DateTime.Now; //沒有只能代填現在時間
                            pcf.SlotNO = slotNo;
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                        }   //job.JobProcessFlows.Remove(eqp.Data.NODEID);
                    }
                    #endregion

                    #region [APC LastGlass]
                    // 20161019 sy modify APC LastGlass
                    if (port.File.Type == ePortType.BothPort)
                    {
                        if (job.SamplingSlotFlag != "1")
                        {
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [ERROR [Job = {0}] Sampling Slot Flag != 1 ]", job.JobKey));
                        }
                        else
                        {
                            if (job.CreateTime == job.JobProcessStartTime)//第一次 取才計算
                            {
                                port.File.SamplingCount--;
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }
                            if (port.File.SamplingCount == 0)
                            {
                                job.APCLastGlassFlag = true;
                                ObjectManager.JobManager.EnqueueSave(job);
                                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", job.JobKey));
                            }
                        }
                    }
                    #endregion

                    #region Update WIP (Port) 需更新什麼資訊，可自行加入
                    job.FromCstID = port.File.CassetteID;
                    job.FromSlotNo = slotNo;
                    job.JobProcessStartTime = DateTime.Now; ///ProductIN的Process Time.只有出cassette 開始計算，不得修改
                    UpdateJobDatabyLineType(job, inputData);
                    #endregion

                    #region 同步T2 for 補報cst Lot Process Started to MES
                    //add by bruce 20160411
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                    if (cst != null && cst.IsProcessed == false)
                    {
                        lock (cst) cst.IsProcessed = true;

                        object[] _data = new object[3]
                            { 
                                inputData.TrackKey,               /*0  TrackKey*/
                                port,                             /*1  Port*/
                                cst,                              /*2  Cassette*/
                            };
                        //luojun 20170306 新增CELL LD port漏报inprocessing，BC收到fetchout后，补报的逻辑
                        //分为nornal(cst)/tray 上报LOTPROCESSSTART,，dense/box 上报BOXPROCESSSTART，pallet暂时不考虑
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            switch(port.Data.PORTATTRIBUTE)
                        {                                                 
                            case keyCELLPORTAtt.DENSE:
                            case keyCELLPORTAtt.BOX:
                                 Invoke(eServiceName.MESService, "BoxProcessStarted", _data);
                            break;
                            case keyCELLPORTAtt.NORMAL:
                            case keyCELLPORTAtt.TRAY:
                            default:
                                 Invoke(eServiceName.MESService, "LotProcessStarted", _data);
                                 Invoke(eServiceName.APCService, "LotProcessStart", _data);
                            break;                          
                        }
                        }
                        else
                        {
                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "LotProcessStarted", _data);
                        Invoke(eServiceName.APCService, "LotProcessStart", _data);
                        }

                        ObjectManager.CassetteManager.EnqueueSave(cst);
                    }
                    #endregion

                    if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    {
                        #region MES APC Data Send
                        object[] _data = new object[7]
                    { 
                        inputData.TrackKey,  /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        job,/*2 WIP Data */
                        eqp.Data.NODEID, /*3 EQP ID */
                        port.Data.PORTID, /*4 PortID */
                        "",/*5 Unit ID */
                        eMESTraceLevel.P,/*6 Trace Level */
                    };
                        //Send MES Data
                        object retVal = base.Invoke(eServiceName.MESService, "ProductOut", _data);
                        //Send APC Data// sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                        //base.Invoke(eServiceName.APCService, "ProductOut", _data);
                        #endregion
                    }

                    CELL_Robot_FetchStoreEvent_AutoAbortCST(inputData, eqp, port, job);
                }

                if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                {
                    #region Check MPLCInterlock
                    Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { inputData.Metadata.NodeNo, unitNo, "FETCH" });
                    #endregion
                }

                //#region Report CFShortCutGlassProcessEnd to MES

                //if (line.File.CFShortCutMode == eShortCutMode.Enable &&
                //    job.JobJudge == "1" && job.CfSpecial.PermitFlag == "Y" &&
                //    job.CfSpecial.CFShortCutrecipeParameterRequestResult == eRecipeCheckResult.OK
                //    && !job.CfSpecial.CFShortCutTrackOut)
                //{
                //    switch (line.Data.LINETYPE)
                //    {
                //        case eLineType.CF.FCMPH_TYPE1:
                //        case eLineType.CF.FCRPH_TYPE1:
                //        case eLineType.CF.FCGPH_TYPE1:
                //        case eLineType.CF.FCBPH_TYPE1:
                //            if (unit != null && unit.Data.UNITATTRIBUTE == "BUFFER")
                //            {
                //                Invoke(eServiceName.MESService, "CFShortCutGlassProcessEnd", new object[] { inputData.TrackKey, line, job });
                //            }
                //            break;
                //    }
                //}
                //#endregion

                //if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)  //add by bruce 2015/09/30 Array ELA 跨line 使用
                //{
                //    if (eqp.Data.NODENO == "L2" && unitNo == "2") //Indexer Turn Table
                //    {
                //        ARRAY_ShortCutCreateWIP(line, eqp, unitNo, job, inputData.TrackKey);

                //        //add by bruce 2015/10/8
                //        string unitId = string.Empty;
                //        if (unit != null)
                //        {
                //            unitId = unit.Data.UNITID;
                //        }
                        
                //        if (job.ArrayELACrossLineID == line.Data.LINEID)
                //        {
                //            job.RemoveFlag = false;
                //            ObjectManager.JobManager.EnqueueSave(job);
                //            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Remove.ToString());
                //            object retVal = base.Invoke(eServiceName.MESService, "ProductLineIn", new object[]{
                //            inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "", unitId});
                //        }
                //        else
                //        {
                //            job.RemoveFlag = true;
                //            ObjectManager.JobManager.EnqueueSave(job);
                //            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Recovery.ToString());
                //            object retVal = base.Invoke(eServiceName.MESService, "ProductLineOut", new object[]{
                //            inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "", unitId});
                //        }

                //    }
                //}

                //add by qiumin 20180106 CheckUpkCstBuffer 
                if (line.Data.LINEID.Contains("UPK") && eqp.Data.NODENO == "L2")
                {
                    Invoke(eServiceName.CFSpecialService, "CheckUpkCstBuffer", new object[] { inputData.TrackKey, eqp });
                }
                #region Save History
                //if (eventName.Length > 1)
                //    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eventName[1]);
                //else
                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.FetchOut.ToString(), inputData.TrackKey);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RemoveJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    RemoveJobReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                string log = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L2");
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                //Job Data Block
                #region [拆出PLCAgent Data]
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;
                #endregion

                //CF job data remove 时需check 上报EQ与job current EQ 是相同的，只有相同才能执行后续逻辑，modify by qiumin 20170207
                #region [对比上报EQ与current EQ]
                if (line.Data.FABTYPE == eFabType.CF.ToString())
                {
                    string eqpnoC = ObjectManager.JobManager.GetJob(casseqno, jobseqno).CurrentEQPNo;
                    if (eqpnoC != inputData.Metadata.NodeNo) throw new Exception(string.Format(eLOG_CONSTANT.JOB_REMOVE_REPORT_MISMATCH, inputData.Metadata.NodeNo,eqpnoC ));
                    
                }
                #endregion

                #region [拆出PLCAgent Data]  Word 2
                string type = inputData.EventGroups[0].Events[1].Items[0].Value;
                log += string.Format(" UNIT_OR_PORT=[{0}]({1})", type, type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"));
                string unitNo = inputData.EventGroups[0].Events[1].Items[1].Value;
                log += string.Format(" UNIT=[{0}]", unitNo);
                string portNo = inputData.EventGroups[0].Events[1].Items[2].Value;
                log += string.Format(" PORT=[{0}]", portNo);
                string slotNo = inputData.EventGroups[0].Events[1].Items[3].Value;
                log += string.Format(" SLOT_NO=[{0}]", slotNo);
                string removedFlag = inputData.EventGroups[0].Events[1].Items[4].Value;
                switch (removedFlag)
                {
                    case "1": log += " REMOVE_FLAG=[1](NORMAL REMOVE)"; break;
                    case "2": log += " REMOVE_FLAG=[2](RECOVERY)"; break;
                    case "3": log += " REMOVE_FLAG=[3](DELETE DIRTY DATA)"; break;
                    case "4": log += " REMOVE_FLAG=[4](MANUAL PORT OUT FOR CF)"; break;
                    default: log += string.Format(" REMOVE_FLAG=[{0}](UNKNOWN)", removedFlag); break;
                }

                string operID = string.Empty;
                string manualOutFlag = string.Empty;
                string reasonCode = string.Empty;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())//CELL IO 與AC 不同 sy 20160317 modify
                {
                    operID = inputData.EventGroups[0].Events[1].Items["OperatorID"].Value;
                    log += string.Format(" OPERATORID=[{0}]", operID);
                    manualOutFlag = inputData.EventGroups[0].Events[1].Items["ManualOutFlag"].Value;
                    log += string.Format(" MANUAL_OUT_FLAG=[{0}]", manualOutFlag);
                    reasonCode = inputData.EventGroups[0].Events[1].Items["ReasonCode"].Value;
                    log += string.Format(" REASON_CODE=[{0}]", reasonCode);
                }
                else if (line.Data.FABTYPE != eFabType.CF.ToString())
                {
                    operID = inputData.EventGroups[0].Events[1].Items[5].Value;
                    log += string.Format(" OPERATORID=[{0}]", operID);
                    manualOutFlag = inputData.EventGroups[0].Events[1].Items[6].Value;
                    log += string.Format(" MANUAL_OUT_FLAG=[{0}]", manualOutFlag);
                    reasonCode = inputData.EventGroups[0].Events[1].Items[7].Value;
                    log += string.Format(" REASON_CODE=[{0}]", reasonCode);
                }
                else
                {
                    operID = inputData.EventGroups[0].Events[1].Items[5].Value;
                    log += string.Format(" OPERATORID=[{0}]", operID);
                    manualOutFlag = inputData.EventGroups[0].Events[1].Items[6].Value;
                    log += string.Format(" MANUAL_OUT_FLAG=[{0}]", manualOutFlag);
                    reasonCode = inputData.EventGroups[0].Events[1].Items[7].Value;
                    log += string.Format(" REASON_CODE=[{0}]", reasonCode);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}]{6}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID, log));

                if (inputData.Tag != null && inputData.Tag.ToString() == eqp.Data.NODENO + "_RemoveJobDataListReport")//sy add 201608010 不是由PLC觸發 不用回寫PLC
                { 
                }
                else
                    RemoveJobReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);
                if (job != null)
                {
                    //Add By yanzhenteng20180802 For CUT BUR Check
                    if (portNo != "0" && eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR))
                    {
                        Port _port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));//Add By yangzhenteng20180717
                        if ((_port.Data.PORTID == "09") || (_port.Data.PORTID == "10") || (_port.Data.PORTID == "13") || (_port.Data.PORTID == "14"))
                        {
                            string _glassid = glsID.Trim();
                            Invoke(eServiceName.MESService, "MachienPanelRejudgeFetchOutReport", new object[] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, _glassid, _port.Data.PORTID, slotNo, "" });
                        }
                    }
                }
                if (job == null)
                {
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            eqp.Data.NODENO, casseqno, jobseqno));
                    #region JobRemoveRecoveryCommnad

                    if (removedFlag == "1" || removedFlag == "2")
                    {
                        JobRemoveRecoveryCommand(inputData.TrackKey, eqp.Data.NODENO, casseqno, jobseqno, "0", (eJobCommand)int.Parse(removedFlag));
                    }
                    return;
                    #endregion
                }

                #region JobRemoveRecoveryCommnad
                if (removedFlag == "1" || removedFlag == "2")
                {
                    JobRemoveRecoveryCommand(inputData.TrackKey, eqp.Data.NODENO, casseqno, jobseqno, job.LastGlassFlag, (eJobCommand)int.Parse(removedFlag));
                }
                #endregion

                #region CF Manual Port Special
                //2015/8/28 add by Frank 
                if (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS || line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB)
                {
                    if (manualOutFlag == "2")
                    {
                        switch (removedFlag)
                        {
                            case "2":
                                {
                                    job.RemoveFlag = false;  //Regeistor回來了
                                    ObjectManager.JobManager.EnqueueSave(job);
                                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Remove.ToString(), inputData.TrackKey);
                                    //Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                                    //Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                                    //Send MES Data
                                    object retVal = base.Invoke(eServiceName.MESService, "ProductLineIn", new object[]{
                                    inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "", ""});
                                }
                                break;
                            case "4":
                                {
                                    job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                                    ObjectManager.JobManager.EnqueueSave(job);
                                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Recovery.ToString(), inputData.TrackKey);
                                    //Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                                    //Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                                    // Send MES Data
                                    object retVal = base.Invoke(eServiceName.MESService, "ProductLineOut", new object[]{
                                    inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "", "" });
                                }
                                break;
                        }
                        return;
                    }


                }
                #endregion

                #region [CELL Special Updata job data]
                //sy add 20160718 CUT MDI no send out ,store,fetchout
                if (line.Data.LINETYPE == eLineType.CELL.CCCUT_5 && eqp.Data.NODENO == "L3" && removedFlag =="1")
                {
                    UpdateJobDatabyLineType(job, inputData);
                }
                #endregion

                #region Remove Special Rule
                switch (removedFlag)
                {
                    case "1":
                        {
                            job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                            job.RemoveReason = string.Format("OPERATORID=[{0}] ; NORMAL REMOVE BY EQ=({1})[{2}]", operID, eqp.Data.NODENO, eqp.Data.NODEID);
                            ObjectManager.JobManager.EnqueueSave(job);

                            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Remove.ToString(), inputData.TrackKey);

                            //Send MES Data
                            object retVal = base.Invoke(eServiceName.MESService, "ProductScrapped", new object[]{
                                inputData.TrackKey, eqp.Data.LINEID, eqp, job, reasonCode });

                            #region LastGlassCommand
                            Job theNewLastGlass;
                            if (GetLastGlass(job, eJobCommand.JOBREMOVE, out theNewLastGlass))
                            {
                                if (theNewLastGlass != null)
                                    SetLastGlassCommand(inputData.TrackKey, eqp.Data.NODENO, theNewLastGlass);
                            }
                            #endregion

                            // Check Recipe Group End Flag  2015.1.15 Michael for Array, 
                            RecipeGroupEndCheck(line, job, inputData.TrackKey);
                            // t3 新增 OEE Message  在Remove 时需要上报 机台的所经的Unit 的Start End Time  20151211 Tom
                            #region 记录专门For OEE 不能使用同一个ProcessFlow For OEE
                            lock (job.JobProcessFlowsForOEE)
                            {
                                if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                                {
                                    ProcessFlow pcf = new ProcessFlow();
                                    pcf.MachineName = eqp.Data.NODEID;
                                    pcf.EndTime = DateTime.Now;
                                    job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                                }
                                else
                                {
                                    //if (eqp.Data.NODEATTRIBUTE != "LU") //非Load
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                                }
                            }
                            #endregion

                            if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG) //2016/06/24 cc.kuang
                                Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job, eJobEvent.Store });
                            else
                                Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job });

                            // 20161019 sy modify APC LastGlass
                            #region [APC LastGlass]
                            if (job.APCLastGlassFlag)
                            {
                                string jobSeqNo = string.Empty;
                                DateTime maxTime = new DateTime();
                                foreach (Job aPCJob in ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo))
                                {
                                    if (!aPCJob.RemoveFlag && aPCJob.JobProcessStartTime > maxTime)
                                    {
                                        maxTime = aPCJob.JobProcessStartTime;
                                        jobSeqNo = aPCJob.JobSequenceNo;
                                    }
                                }
                                if (!string.IsNullOrEmpty(jobSeqNo))
                                {
                                    Job aPCLastJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, jobSeqNo);
                                    if (aPCLastJob != null)
                                    {
                                        aPCLastJob.APCLastGlassFlag = true;
                                        ObjectManager.JobManager.EnqueueSave(aPCLastJob);
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", aPCLastJob.JobKey));
                                    }
                                }
                            }
                            #endregion

                            //add by hujunpeng 20190723 for PI JOB COUNT MONITOR
                            #region PI JOB COUNT MONITOR
                            if (line.Data.LINEID == "CCPIL100" || line.Data.LINEID == "CCPIL200")
                            {
                                lock (eqp1.File)
                                {
                                    if (job.MesProduct.OWNERTYPE.ToUpper() == "OWNERP" && (job.JobType == eJobType.TFT || job.JobType == eJobType.CF))
                                    {
                                        string PRODGROUP = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME + job.MesProduct.GROUPID;
                                        //eqp.File.PIJobCount = new Dictionary<string, Tuple<int, int>>();
                                        if (eqp1.File.PIJobCount.Count <= 0)
                                        {
                                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PI Job Count is 0!");
                                        }
                                        else
                                        {
                                            if (!eqp1.File.PIJobCount.ContainsKey(PRODGROUP))
                                            {
                                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PI Job Count is not contains!");
                                            }
                                            else
                                            {
                                                if (job.JobType == eJobType.TFT)
                                                {
                                                    int tftcount = eqp1.File.PIJobCount[PRODGROUP].Item1;
                                                    tftcount--;
                                                    Tuple<int, int> jobcount = new Tuple<int, int>(tftcount, eqp1.File.PIJobCount[PRODGROUP].Item2);
                                                    eqp1.File.PIJobCount.Remove(PRODGROUP);
                                                    eqp1.File.PIJobCount.Add(PRODGROUP, jobcount);
                                                }
                                                else
                                                {
                                                    int cfcount = eqp1.File.PIJobCount[PRODGROUP].Item2;
                                                    cfcount--;
                                                    Tuple<int, int> jobcount = new Tuple<int, int>(eqp1.File.PIJobCount[PRODGROUP].Item1, cfcount);
                                                    eqp1.File.PIJobCount.Remove(PRODGROUP);
                                                    eqp1.File.PIJobCount.Add(PRODGROUP, jobcount);
                                                }
                                            }
                                        }
                                        ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                    }
                                    Invoke(eServiceName.UIService, "PIJobCountReport", new object[] { inputData.TrackKey, eqp1.File.PIJobCount });
                                }
                            }
                            

                            #endregion
                        }
                        break;
                    case "2":
                        {
                            job.RemoveFlag = false;  //Regeistor回來了
                            job.RemoveReason = string.Format("OPERATORID=[{0}] ; RECOVERY BY EQ=({1})[{2}]", operID, eqp.Data.NODENO, eqp.Data.NODEID);
                            ObjectManager.JobManager.EnqueueSave(job);

                            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Recovery.ToString(), inputData.TrackKey);

                            // Send MES Data
                            //string trxID, string lineName, Equipment eqp,Job job, string recoveReasonCode)
                            object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[]{
                                inputData.TrackKey, eqp.Data.LINEID, eqp, job, reasonCode });

                            //for t3 OEE 2016/06/24 cc.kuang
                            lock (job.JobProcessFlowsForOEE)
                            {
                                if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                                {
                                    ProcessFlow pcf = new ProcessFlow();
                                    pcf.MachineName = eqp.Data.NODEID;
                                    pcf.StartTime = DateTime.Now;
                                    job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                                }
                                else
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].StartTime = DateTime.Now;
                                }
                                job.OEERecvFlag = true; 
                            }


                            // 20161019 sy modify APC LastGlass
                            #region [APC LastGlass]
                            if (job.APCLastGlassFlag)
                            {
                                string jobSeqNo = string.Empty;
                                DateTime maxTime = new DateTime();
                                job.APCLastGlassFlag = false;//先清除 重新 在判斷
                                foreach (Job aPCJob in ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo))
                                {
                                    #region [Clear APCLastGlassFlag]
                                    if (aPCJob.APCLastGlassFlag)
                                    {
                                        aPCJob.APCLastGlassFlag = false;
                                        ObjectManager.JobManager.EnqueueSave(aPCJob);
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [CLEAR [Job = {0}] APC Last Glass Flag]]", aPCJob.JobKey));
                                    }
                                    #endregion
                                    if (!aPCJob.RemoveFlag && aPCJob.JobProcessStartTime > maxTime)
                                    {
                                        maxTime = aPCJob.JobProcessStartTime;
                                        jobSeqNo = aPCJob.JobSequenceNo;
                                    }
                                }
                                if (!string.IsNullOrEmpty(jobSeqNo))
                                {
                                    Job aPCLastJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, jobSeqNo);
                                    if (aPCLastJob != null)
                                    {
                                        aPCLastJob.APCLastGlassFlag = true;
                                        ObjectManager.JobManager.EnqueueSave(aPCLastJob);
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", aPCLastJob.JobKey));
                                    }
                                }
                            }
                            #endregion

                            //add by hujunpeng 20190723 for PI JOB COUNT MONITOR
                            #region PI JOB COUNT MONITOR
                            if (line.Data.LINEID == "CCPIL100" || line.Data.LINEID == "CCPIL200")
                            {
                                lock (eqp1.File)
                                {
                                    if (job.MesProduct.OWNERTYPE.ToUpper() == "OWNERP" && (job.JobType == eJobType.TFT || job.JobType == eJobType.CF))
                                    {
                                        string PRODGROUP = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME + job.MesProduct.GROUPID;
                                        //eqp.File.PIJobCount = new Dictionary<string, Tuple<int, int>>();
                                        if (eqp1.File.PIJobCount.Count <= 0)
                                        {
                                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PI Job Count is 0!");
                                        }
                                        else
                                        {
                                            if (!eqp1.File.PIJobCount.ContainsKey(PRODGROUP))
                                            {
                                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PI Job Count is not contains!");
                                            }
                                            else
                                            {
                                                if (job.JobType == eJobType.TFT)
                                                {
                                                    int tftcount = eqp1.File.PIJobCount[PRODGROUP].Item1;
                                                    tftcount++;
                                                    Tuple<int, int> jobcount = new Tuple<int, int>(tftcount, eqp1.File.PIJobCount[PRODGROUP].Item2);
                                                    eqp1.File.PIJobCount.Remove(PRODGROUP);
                                                    eqp1.File.PIJobCount.Add(PRODGROUP, jobcount);
                                                }
                                                else
                                                {
                                                    int cfcount = eqp1.File.PIJobCount[PRODGROUP].Item2;
                                                    cfcount++;
                                                    Tuple<int, int> jobcount = new Tuple<int, int>(eqp1.File.PIJobCount[PRODGROUP].Item1, cfcount);
                                                    eqp1.File.PIJobCount.Remove(PRODGROUP);
                                                    eqp1.File.PIJobCount.Add(PRODGROUP, jobcount);
                                                }
                                            }
                                        }
                                        ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                    }
                                    Invoke(eServiceName.UIService, "PIJobCountReport", new object[] { inputData.TrackKey, eqp1.File.PIJobCount });
                                }
                            }
                            

                            #endregion
                        }
                        break;
                    case "3":
                        job.RemoveReason = string.Format("OPERATORID=[{0}] ; DELETE BY EQ=({1})[{2}]", operID, eqp.Data.NODENO, eqp.Data.NODEID);
                        ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Delete.ToString(), inputData.TrackKey);
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            //20180308 cell delete special
                            string cl_lineid = line.Data.LINEID;
                            string cl_eqpid = eqp.Data.NODEID;
                            Unit cl_unit=ObjectManager.UnitManager.GetUnit(cl_eqpid, unitNo);
                            string cl_unitid = cl_unit != null ? cl_unit.Data.UNITID : string.Empty;
                            string cl_productname = job.GlassChipMaskBlockID.Trim();
                            string cl_processoperationname=job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                            string cl_userid = operID.Trim();
                            string cl_transactionstarttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            Invoke(eServiceName.MESService, "ProductDeleteReport", new object[]{
                                inputData.TrackKey,
                                cl_lineid,
                                cl_eqpid,
                                cl_unitid,
                                cl_productname,
                                cl_processoperationname,
                                cl_userid,
                                cl_transactionstarttime
                            });
 
                        }
                        break;
                    //Mdofify By Yangzhenteng 20190812 For POL Panel Borrowed,用Remove上报,打给MES Borrowed;
                    case "5":
                        {
                            job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                            job.RemoveReason = string.Format("OPERATORID=[{0}] ; NORMAL REMOVE BY EQ=({1})[{2}]", operID, eqp.Data.NODENO, eqp.Data.NODEID);
                            string _ReasoonCode = "BORROWED";
                            ObjectManager.JobManager.EnqueueSave(job);
                            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo.PadLeft(2, '0'), slotNo, eJobEvent.Borrowed.ToString(), inputData.TrackKey);
                            //Send MES Data
                            object retVal = base.Invoke(eServiceName.MESService, "ProductScrapped", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp, job, _ReasoonCode });
                            #region LastGlassCommand
                            Job theNewLastGlass;
                            if (GetLastGlass(job, eJobCommand.JOBREMOVE, out theNewLastGlass))
                            {
                                if (theNewLastGlass != null)
                                    SetLastGlassCommand(inputData.TrackKey, eqp.Data.NODENO, theNewLastGlass);
                            }
                            #endregion
                            //Check Recipe Group End Flag  2015.1.15 Michael for Array, 
                            RecipeGroupEndCheck(line, job, inputData.TrackKey);
                            //T3 新增 OEE Message  在Remove 时需要上报 机台的所经的Unit 的Start End Time  20151211 Tom
                            #region 记录专门For OEE 不能使用同一个ProcessFlow For OEE
                            lock (job.JobProcessFlowsForOEE)
                            {
                                if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                                {
                                    ProcessFlow pcf = new ProcessFlow();
                                    pcf.MachineName = eqp.Data.NODEID;
                                    pcf.EndTime = DateTime.Now;
                                    job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                                }
                                else
                                {
                                    //if (eqp.Data.NODEATTRIBUTE != "LU") //非Load
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                                }
                            }
                            #endregion
                            if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG) //2016/06/24 cc.kuang
                            {
                                Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job, eJobEvent.Store });
                            }                                
                            else
                            {
                                Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, "", job });
                            }                                
                            // 20161019 sy modify APC LastGlass
                            #region [APC LastGlass]
                            if (job.APCLastGlassFlag)
                            {
                                string jobSeqNo = string.Empty;
                                DateTime maxTime = new DateTime();
                                foreach (Job aPCJob in ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo))
                                {
                                    if (!aPCJob.RemoveFlag && aPCJob.JobProcessStartTime > maxTime)
                                    {
                                        maxTime = aPCJob.JobProcessStartTime;
                                        jobSeqNo = aPCJob.JobSequenceNo;
                                    }
                                }
                                if (!string.IsNullOrEmpty(jobSeqNo))
                                {
                                    Job aPCLastJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, jobSeqNo);
                                    if (aPCLastJob != null)
                                    {
                                        aPCLastJob.APCLastGlassFlag = true;
                                        ObjectManager.JobManager.EnqueueSave(aPCLastJob);
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", aPCLastJob.JobKey));
                                    }
                                }
                            }
                            #endregion
                        }
                        break;
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RemoveRecoveryJobDataForUI(string trxID, string cstSeqNo, string jobSeqNo, string glsID, string removeFlag, string reason)
        {
            try
            {
                string log = string.Empty;
                Equipment eqp = null;

                switch (removeFlag)
                {
                    case "1": log += string.Format("REMOVE_FLAG=[1](NORMAL REMOVE) REASON_CODE=[{0}]", reason); break;
                    case "2": log += string.Format("REMOVE_FLAG=[2](RECOVERY) REASON_CODE=[{0}]", reason); break;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[JOBID={0}] [BCS <- EQP][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{0}]{4}.",
                    glsID, trxID, cstSeqNo, jobSeqNo, log));

                Job job = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo);
                if (job == null)
                {
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("JOBID=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                            glsID, cstSeqNo, jobSeqNo));

                    if (removeFlag == "1" || removeFlag == "2")
                    {
                        JobRemoveRecoveryCommand(trxID, "OPI", cstSeqNo, jobSeqNo, "0", (eJobCommand)int.Parse(removeFlag));
                    }
                    return;
                }

                #region JobRemoveRecoveryCommnad
                if (removeFlag == "1" || removeFlag == "2")
                {
                    JobRemoveRecoveryCommand(trxID, job.CurrentEQPNo, cstSeqNo, jobSeqNo, job.LastGlassFlag, (eJobCommand)int.Parse(removeFlag));
                }
                #endregion

                #region Remove Special Rule
                eqp = ObjectManager.EquipmentManager.GetEQP(job.CurrentEQPNo);
                switch (removeFlag)
                {
                    case "1":
                        {
                            job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                            job.RemoveReason = reason;
                            ObjectManager.JobManager.EnqueueSave(job);

                            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, job.CurrentEQPNo, "OPI", "OPI", string.Empty, eJobEvent.Remove.ToString(), trxID);

                            //OPI不需上報MES
                            //object retVal = base.Invoke(eServiceName.MESService, "ProductScrapped", new object[]{
                            //    inputData.TrackKey, eqp.Data.LINEID, eqp, job, reasonCode });

                            #region LastGlassCommand
                            Job theNewLastGlass;
                            if (GetLastGlass(job, eJobCommand.JOBREMOVE, out theNewLastGlass))
                            {
                                if (theNewLastGlass != null)
                                    SetLastGlassCommand(trxID, eqp.Data.NODENO, theNewLastGlass);
                            }
                            #endregion

                            Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                            // Check Recipe Group End Flag  2015.1.15 Michael for Array, 
                            RecipeGroupEndCheck(line, job, trxID);
                        }
                        break;
                    case "2":
                        {
                            job.RemoveFlag = false;  //Regeistor回來了
                            job.RemoveReason = "RECOVERY BY OPI";
                            ObjectManager.JobManager.EnqueueSave(job);

                            ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, "OPI", "OPI", string.Empty, eJobEvent.Recovery.ToString(), trxID);

                            //OPI不需上報MES
                            //string trxID, string lineName, Equipment eqp,Job job, string recoveReasonCode)
                            //object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[]{
                            //    inputData.TrackKey, eqp.Data.LINEID, eqp, job, reasonCode });
                        }
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDataEditReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;


                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    //Jun Modify 20150107 Bit Off Reply Method寫錯
                    JobDataEditReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Job Data Block
                #region [拆出PLCAgent Data]
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;

                string operID = inputData.EventGroups[0].Events[1].Items[0].Value;
                #endregion
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}] OPERATOR_ID=[{6}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID, operID));

                JobDataEditReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);
                if (job == null)
                {
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    job.GlassChipMaskBlockID = glsID;
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                }

                UpdateJobDatabyLineType(job, inputData);
                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Edit.ToString(), inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SendOutJobDataReportForSECS(string trxid, string jobid, Equipment eqp, string subeqpid)
        {
            try
            {
                #region Check Parameter
                //20150131 cy modify:job id空白,產生新的job
                if (string.IsNullOrEmpty(jobid))
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "THE PARAMETER OF JOB_ID IS EMPTY!");
                    //return;
                }
                if (eqp == null)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "THE PARAMETER OF EQUIPMENT OBJECT IS NULL!");
                    return;
                }
                #endregion
                Job job = ObjectManager.JobManager.GetJob(jobid);
                if (job == null)
                {
                    string jobseq = string.Empty;
                    if (!GetJobSeqForSECS(out jobseq))
                    {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Can not find Job ID({2}) in JobEntity and create new job fail. Can not fine useful JobSequenceNo. CassettSequenceNo=[{3}]",
                                           eqp.Data.NODENO, trxid, jobid, FixCassetteSeqForSECS));
                        return;
                    }
                    //Create a new job
                    job = NewJob(eqp.Data.LINEID, FixCassetteSeqForSECS, jobseq, null); // wucc modify  NewJob() assign 給 job 20150715
                    job.GlassChipMaskBlockID = jobid;

                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), trxid);
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Send Out Job Data New Cassette Sequence No=[{4}],Job Sequence No=[{5}],Glass ID=[{6}]",
                                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, FixCassetteSeqForSECS, jobseq, job.GlassChipMaskBlockID));
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Send Out Job Data Cassette Sequence No=[{4}],Job Sequence No=[{5}]",
                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo));

                Unit unit = null;
                if (!string.IsNullOrEmpty(subeqpid))
                {
                    unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                }

                //20150417 cy:將Job內的PhotoIsProcessed設為true,但退port時判斷是否經過SECS機台(for array)
                job.ArraySpecial.PhotoIsProcessed = true;

                #region MES Data Send
                object[] _data = new object[7]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    unit==null?"":unit.Data.UNITID,/*5 Unit ID */
                    unit==null?eMESTraceLevel.M:eMESTraceLevel.U,/*6 Trace Level M’ – Machine ,‘U’ – Unit ,‘P’ – Port*/
                };
                Invoke(eServiceName.MESService, "ProductOut", _data);
                #endregion
                // sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                #region APC Data Send
                if (unit == null)
                {
                    if (ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO) != null)//20161003 sy add profile 沒資料就不用 上報 by 勝杰
                    {
                        object[] _data_APC = new object[8]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    eqp.Data.NODEID,/*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                    0,/*7 Position */
                };
                        //20190410 huangjiayin modify for fa special
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        { }
                        else
                        {
                            base.Invoke(eServiceName.APCService, "ProductOut", _data_APC); //20161212 sy modify
                        }
                    }
                }
                #endregion

                if (unit == null)
                {
                    if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                    {
                        // Check MPLCInterlock
                        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { eqp.Data.NODENO, "", "SEND" });
                    }
                    // Qtime 確認
                    if (QtimeEventJudge(trxid, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODENO, "", eQtimeEventType.SendOutEvent, eqp.File.CurrentRecipeID))
                    {
                        //TODO:Qtime確認
                        //EX:job.EQPFlag.QtimeFlag = true;
                    }

                    #region Endtime
                    #region JobProcessFlow
                    lock (job.JobProcessFlows)
                    {
                        string jkey = string.Format("{0}", eqp.Data.NODEID);
                        if (!job.JobProcessFlows.ContainsKey(jkey))
                        {
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = jkey;
                            pcf.EndTime = DateTime.Now;
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                        }
                        else
                        {
                            job.JobProcessFlows[eqp.Data.NODEID].EndTime = DateTime.Now;
                        }
                    }
                    #endregion

                    #region JobProcessFlow For OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        string jkey = string.Format("{0}", eqp.Data.NODEID);
                        if (!job.JobProcessFlowsForOEE.ContainsKey(jkey))
                        {
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = jkey;
                            pcf.EndTime = DateTime.Now;
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                        }
                        else
                        {
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                        }
                    }
                    #endregion
                    #endregion

                    //Save History
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.SendOut.ToString(), trxid);
                    ObjectManager.JobManager.EnqueueSave(job);

                    // t3 新增 OEE Message 在Sent out 时需要上报该机台的Unit 的Start End time 20151211 Tom
                    Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] { trxid,eqp.Data.LINEID,eqp.Data.NODEID,"",job});
                }
                else
                {
                    if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                    {
                        // Check MPLCInterlock
                        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { eqp.Data.NODENO, unit.Data.UNITNO, "FETCH" });
                    }

                    #region Endtime Watson Add 20141125
                    lock (job.JobProcessFlows)
                    {
                        if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit End time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.EndTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.EndTime = DateTime.Now;
                                upcf.SlotNO = "";
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            if (unit != null)
                            {
                                #region Update Unit End time
                                ProcessFlow upcf = new ProcessFlow();
                                if (job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    if (job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime == DateTime.MinValue) //if has not set endtime, set now for avoid modify s6f3_03 extendUnitProcessFlows 2016/06/08 cc.kuang
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime.Now;
                                }
                                else
                                {
                                    upcf.MachineName = unit.Data.UNITID;
                                    upcf.EndTime = DateTime.Now;
                                    upcf.SlotNO = "";
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                                #endregion
                            }
                        }
                    }


                    #endregion

                    #region JobProcessFlow For OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit End time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.EndTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.EndTime = DateTime.Now;
                                upcf.SlotNO = "";
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            if (unit != null)
                            {
                                #region Update Unit End time
                                ProcessFlow upcf = new ProcessFlow();
                                if (job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    if (job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime == DateTime.MinValue) //if has not set endtime, set now for avoid modify s6f3_03 extendUnitProcessFlows 2016/06/08 cc.kuang
                                        job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime.Now;
                                }
                                else
                                {
                                    upcf.MachineName = unit.Data.UNITID;
                                    upcf.EndTime = DateTime.Now;
                                    upcf.SlotNO = "";
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    job.CurrentUNITNo = string.Empty;
                    //Save History
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unit.Data.UNITNO, string.Empty, string.Empty, eJobEvent.FetchOut.ToString(), trxid);
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ReceiveJobDataReportForSECS(string trxid, string jobid, Equipment eqp, string subeqpid)
        {
            try
            {
                #region Check Parameter
                //20150131 cy modify:job id空白,產生新的job
                if (string.IsNullOrEmpty(jobid))
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "The parameter of job ID is empty");
                    //return;
                }
                if (eqp == null)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "The parameter of Equipment object is null");
                    return;
                }
                #endregion
                Job job = ObjectManager.JobManager.GetJob(jobid);
                if (job == null)
                {
                    string jobseq = string.Empty;
                    if (!GetJobSeqForSECS(out jobseq))
                    {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Can not find Job ID({2}) in JobEntity and create new job fail. Can not fine useful JobSequenceNo. CassettSequenceNo=[{3}]",
                                           eqp.Data.NODENO, trxid, jobid, FixCassetteSeqForSECS));
                        return;
                    }
                    //Create a new job
                    job = NewJob(eqp.Data.LINEID, FixCassetteSeqForSECS, jobseq, null); // wucc modify  NewJob() assign 給 job 20150715
                    job.GlassChipMaskBlockID = jobid; job.CIMMode = eqp.File.CIMMode;
                    job.CurrentEQPNo = eqp.Data.NODENO;
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), trxid);
                    job = ObjectManager.JobManager.GetJob(FixCassetteSeqForSECS, jobseq);
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Receive Job Data New Cassette Sequence No=[{4}],Job Sequence No=[{5}],Glass ID=[{6}]",
                                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, FixCassetteSeqForSECS, jobseq, job.GlassChipMaskBlockID));
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Receive Job Data Cassette Sequence No=[{4}],Job Sequence No=[{5}]",
                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo));
                //更新Job目前位置
                job.CurrentEQPNo = eqp.Data.NODENO;
                Unit unit = null;
                if (!string.IsNullOrEmpty(subeqpid))
                {
                    unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                }
                if (unit != null)
                {
                    //20150310 cy:改判斷UnitType為Chamber
                    //if (eqp.Data.NODEATTRIBUTE == "CVD" || eqp.Data.NODEATTRIBUTE == "DRY") {
                    //    job.ChamberName = unit.Data.UNITID;
                    //}
                    if (!string.IsNullOrEmpty(unit.Data.UNITTYPE) && unit.Data.UNITTYPE.ToUpper() == "CHAMBER" && unit.Data.UNITATTRIBUTE.ToUpper() == "NORMAL")
                        job.ChamberName = unit.Data.UNITID;


                }

                //20150417 cy:將Job內的PhotoIsProcessed設為true,但退port時判斷是否經過SECS機台(for array)
                job.ArraySpecial.PhotoIsProcessed = true;

                #region MES Data Send
                object[] _data = new object[8]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    unit==null?"":unit.Data.UNITID,/*5 Unit ID */
                    unit==null?eMESTraceLevel.M:eMESTraceLevel.U,/*6 Trace Level M’ – Machine ,‘U’ – Unit ,‘P’ – Port*/
                    "120",/*7 Process Time *///DateTime.Now - job.StartDateTime
                };
                Invoke(eServiceName.MESService, "ProductIn", _data);
                #endregion
                // sy add 20161003 APC 修改 不同於MES  && Store FetchOut 不需上報
                #region [APC Data Send]
                if (unit == null)
                {
                    if (ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqp.Data.NODENO) != null)//20161003 sy add profile 沒資料就不用 上報 by 勝杰
                    {
                        object[] _data_APC = new object[9]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    job,/*2 WIP Data */
                    eqp.Data.NODEID, /*3 EQP ID */
                    "", /*4 PortID */
                    eqp.Data.NODEID,/*5 Unit ID */
                    eMESTraceLevel.M,/*6 Trace Level */
                    0,/*7 Position */
                   "120"/*8 Process Time  */
                };
                        //Send APC Data  
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        { }
                        else
                        {
                            Invoke(eServiceName.APCService, "ProductIn", _data_APC);
                        }
                    }
                }
                #endregion

                if (unit == null)
                {
                    if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                    {
                        //Check MPLCInterlock
                        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { eqp.Data.NODENO, "", "RECEIVE" });
                    }

                    //Qtime 確認
                    if (QtimeEventJudge(trxid, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODENO, "", eQtimeEventType.ReceiveEvent, eqp.File.CurrentRecipeID))
                    {
                        //TODO:Qtime確認
                        //EX:job.EQPFlag.QtimeFlag = true;
                    }

                    #region Starttime
                    #region Job Process Flow
                    lock (job.JobProcessFlows)
                    {
                        if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                        {
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID;
                            pcf.StartTime = DateTime.Now;
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                        }
                        else
                        {
                            job.JobProcessFlows[eqp.Data.NODEID].StartTime = DateTime.Now;
                        }
                    }
                    #endregion

                    #region Job Process Flow for OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                        {
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID;
                            pcf.StartTime = DateTime.Now;
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                        }
                        else
                        {
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].StartTime = DateTime.Now;
                        }
                    }
                    #endregion
                    #endregion

                    //Save History
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Receive.ToString(), trxid);
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                else
                {
                    if (job.JobType == eJobType.TFT || job.JobType == eJobType.CF) //if (job.JobType != eJobType.DM) sy modify 20160626
                    {
                        //Check MPLCInterlock
                        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { eqp.Data.NODENO, unit.Data.UNITNO, "STORE" });
                    }

                    #region Starttime Watson Add 20141125
                    lock (job.JobProcessFlows)
                    {
                        if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit Start Time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.StartTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = "";
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            #region Update Unit Start Time [已經存過被修改]
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = "";
                                if (job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime.Now;
                                }
                                else
                                {
                                    job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                            }
                            #endregion
                        }
                    }

                    #region Job Process Flow for OEE
                    lock (job.JobProcessFlowsForOEE)
                    {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                        {
                            #region Add Unit Start Time
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID; //找不到就直接填入
                            pcf.StartTime = DateTime.Now; //沒有只能代填現在時間
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = "";
                                pcf.UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                            }
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                            #endregion
                        }
                        else
                        {
                            #region Update Unit Start Time [已經存過被修改]
                            if (unit != null)
                            {
                                ProcessFlow upcf = new ProcessFlow();
                                upcf.MachineName = unit.Data.UNITID;
                                upcf.StartTime = DateTime.Now;
                                upcf.SlotNO = "";
                                if (job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime.Now;
                                }
                                else
                                {
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, upcf);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                    #endregion
                    job.CurrentUNITNo = unit.Data.UNITNO;
                    //Save History
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unit.Data.UNITNO, string.Empty, string.Empty, eJobEvent.Store.ToString(), trxid);
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                //// add by qiumin 20180604 for array SECS eqp process time start 移动到S6F11 08
                //if (job.JobType == eJobType.TFT && eqp.Data.NODENO != "L2")
                //{
                //    lock (job)
                //    {
                //        job.ArraySpecial.JobProcessStartedTime = DateTime.Now;
                //    }
                //    ObjectManager.JobManager.EnqueueSave(job);
                //}
                ////

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="jobid"></param>
        /// <param name="eqp"></param>
        /// <param name="removeFlag">1:Remove 2:Recovery 3:Delete Dirty</param>
        public void RemoveJobDataReportForSECS(string trxid, string jobid, Equipment eqp, string removeFlag, string removeReasonCode, string operatorID, string cstseq, string slot)
        {
            //20150417 cy:remove時,可傳入cstseq+slot
            try
            {
                #region Check Parameter
                //20150131 cy modify:job id空白,產生新的job
                if ((string.IsNullOrEmpty(cstseq) || string.IsNullOrEmpty(slot)) & string.IsNullOrEmpty(jobid))
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "The parameter of job ID is empty");
                    //return;
                }
                if (eqp == null)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "The parameter of Equipment object is null");
                    return;
                }
                #endregion
                Job job = null;
                if (!string.IsNullOrEmpty(cstseq) && !string.IsNullOrEmpty(slot))
                    job = ObjectManager.JobManager.GetJob(cstseq, slot);
                else
                    job = ObjectManager.JobManager.GetJob(jobid);
                if (job == null)
                {
                    string jobseq = string.Empty;
                    if (!GetJobSeqForSECS(out jobseq))
                    {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Can not find Job ID({2}) in JobEntity and create new job fail. Can not fine useful JobSequenceNo. CassettSequenceNo=[{3}]",
                                           eqp.Data.NODENO, trxid, jobid, FixCassetteSeqForSECS));
                        return;
                    }
                    //Create a new job
                    job = NewJob(eqp.Data.LINEID, FixCassetteSeqForSECS, jobseq, null); // wucc modify  NewJob() assign 給 job 20150715
                    job.GlassChipMaskBlockID = jobid; job.CIMMode = eqp.File.CIMMode;
                    job.CurrentEQPNo = eqp.Data.NODENO;
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), trxid);

                    job = ObjectManager.JobManager.GetJob(FixCassetteSeqForSECS, jobseq);
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Remove Job Data New Cassette Sequence No=[{4}],Job Sequence No=[{5}],Glass ID=[{6}]",
                                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, FixCassetteSeqForSECS, jobseq, job.GlassChipMaskBlockID));
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], Remove Job Data Cassette Sequence No=[{4}],Job Sequence No=[{5}]",
                    eqp.Data.NODENO, trxid, eqp.File.CIMMode, eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo));

                if (removeFlag == "3")
                {
                    job.RemoveReason = string.Format("OPERATORID=[{0}] ; DELETE BY EQ=({1})[{2}]", operatorID, eqp.Data.NODENO, eqp.Data.NODEID);
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Delete.ToString(), trxid);
                    return;
                }

                bool isLastGlass = false;

                if (removeFlag == "2")
                {
                    job.RemoveFlag = false;   //Regeistor回來了
                    job.RemoveReason = string.Format("OPERATORID=[{0}] ; RECOVERY BY EQ=({1})[{2}]", operatorID, eqp.Data.NODENO, eqp.Data.NODEID);
                    ObjectManager.JobManager.EnqueueSave(job);

                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Recovery.ToString(), trxid);

                    #region LastGlassCommand
                    Job theOldLastGlass;
                    if (GetLastGlass(job, eJobCommand.JOBRECOVERY, out theOldLastGlass))
                    {
                        if (theOldLastGlass != null)
                        {
                            isLastGlass = true;
                            SetLastGlassCommand(trxid, eqp.Data.NODENO, theOldLastGlass); //也要報給PLC機台
                        }
                    }
                    #endregion
                    #region Send MES Data
                    object[] _data = new object[5]
                    { 
                        trxid,  /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp, /*2 EQP ID */
                        job,/*3 WIP Data */
                        removeReasonCode,/*4 Reason Code */
                    };
                    //string trxID, string lineName, Equipment eqp,Job job, string recoveReasonCode)
                    object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", _data);
                    #endregion
                    JobRemoveRecoveryCommand(trxid, eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo,
                                            isLastGlass ? ((int)eLastFlag.LastGlass).ToString() : ((int)eLastFlag.NotLastGlass).ToString(), eJobCommand.JOBRECOVERY);
                }
                if (removeFlag == "1")
                {
                    job.RemoveFlag = true;
                    job.RemoveReason = string.Format("OPERATORID=[{0}] ; NORMAL REMOVE BY EQ=({1})[{2}]", operatorID, eqp.Data.NODENO, eqp.Data.NODEID);
                    ObjectManager.JobManager.EnqueueSave(job);

                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Remove.ToString(), trxid);

                    #region LastGlassCommand
                    Job theNewLastGlass;
                    if (GetLastGlass(job, eJobCommand.JOBREMOVE, out theNewLastGlass))
                    {
                        if (theNewLastGlass != null)
                        {
                            isLastGlass = true;
                            SetLastGlassCommand(trxid, eqp.Data.NODENO, theNewLastGlass); //也要報給PLC機台
                        }
                    }
                    #endregion
                    #region MES Data Send
                    object[] _data = new object[5]
                    { 
                        trxid,  /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp, /*2 EQP ID */
                        job,/*3 WIP Data */
                        removeReasonCode,/*4 Reason Code */
                    };
                    Invoke(eServiceName.MESService, "ProductScrapped", _data);
                    #endregion
                    JobRemoveRecoveryCommand(trxid, eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo,
                                            isLastGlass ? ((int)eLastFlag.LastGlass).ToString() : ((int)eLastFlag.NotLastGlass).ToString(), eJobCommand.JOBREMOVE);

                    // t3 新增 OEE Message  在Remove 时需要上报 机台的所经的Unit 的Start End Time  20151211 Tom
                    #region 记录专门For OEE 不能使用同一个ProcessFlow For OEE
                    lock (job.JobProcessFlowsForOEE) {
                        if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID)) {
                            ProcessFlow pcf = new ProcessFlow();
                            pcf.MachineName = eqp.Data.NODEID;
                            pcf.EndTime = DateTime.Now;
                            job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                        } else {
                            //if (eqp.Data.NODEATTRIBUTE != "LU") //非Load
                            job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = DateTime.Now;
                        }
                    }
                    #endregion
                    Invoke(eServiceName.OEEService, "ProductInOutTotalM", new object[] {trxid, eqp.Data.LINEID, eqp.Data.NODEID, "", job });


                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void JobEachPositionBlock(Trx inputData)
        {
            try
            {
                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                //if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                ////Job Data Block
                //#region [拆出PLCAgent Data]  Word 1
                //for(int i =0;i<inputData.EventGroups.Count;i++)
                //{
                //    for (int j=0;j<inputData.EventGroups[i].Events.Count;j++)
                //    {
                //        string casseqno = inputData.EventGroups[i].Events[i].Items[0].Value;
                //        string jobseqno = inputData.EventGroups[i].Events[i].Items[1].Value;
                //        Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                //        if (job == null)
                //        {
                //            NewJob(casseqno, jobseqno);
                //            job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);
                //        }

                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node =[{3}],Job Each Position Data Cassette Sequence No =[{4}],Job Sequence No =[{5}]",
                //        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, casseqno, jobseqno));
                //    }
                //}
                //#endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //目前無用
        public void ProductTypeBlock(Trx inputData)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Job Data Block
                #region [拆出PLCAgent Data]  Word 1
                eqp.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value); //
                string log = string.Empty;
                const string PRODUCTTYPE = "ProductType";
                foreach (Unit unit in ObjectManager.UnitManager.GetUnits())
                {
                    if ((inputData.EventGroups[0].Events[0].Items["Unit#" + unit.Data.UNITNO.PadLeft(2, '0') + PRODUCTTYPE] == null))
                        continue;
                    unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items["Unit#" + unit.Data.UNITNO.PadLeft(2, '0') + PRODUCTTYPE].Value);
                    log += string.Format(" UNIT#{0}=[{1}]", unit.Data.UNITNO, unit.File.ProductType);
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PRODUCT_TYPE: EQP=[{2}]{3}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.ProductType, log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// CF、 ARRAY 、CELL 一律呼叫
        /// </summary>
        /// <param name="job">Job Entity</param>
        /// <param name="inputData">PLC Trx Data</param>
        public void UpdateJobDatabyLineType(Job job, Trx inputData)
        {
            try
            {
                Event eVent = inputData.EventGroups[0].Events[0];

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                #region[FOR POL Get EQP Check List Add By Yangzhenteng20190616]
                 ParameterManager _parameterManager = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                 bool SpecialEQPJobGradeUpdate = _parameterManager.ContainsKey("SPECIALEQPJOBGRADEUPDATE") ? _parameterManager["SPECIALEQPJOBGRADEUPDATE"].GetBoolean() : false;
                 List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                 string[] Cell_POL_UpdateJobGrade_EqpList = _parameterManager[eArrayPPIDByPass.CELLPOLUPDATEJOBGRADEEQPLIST].GetString().Split(',');
                 eqps = ObjectManager.EquipmentManager.GetEQPsByEQPNOList(Cell_POL_UpdateJobGrade_EqpList.ToList());
                #endregion

                #region 三廠Common部份統一更新
                lock (job)
                {
                    job.GroupIndex = eVent.Items[eJOBDATA.GroupIndex].Value;
                    if (inputData.Name.Contains(keyEQPEvent.JobDataEditReport))//防止最後一筆 更新 WIP，再上CST後，會更新到ProductTypeManange sy Modify 20160307
                    {
                        job.ProductType.Value = int.Parse(eVent.Items[eJOBDATA.ProductType].Value);
                    }
                    job.CSTOperationMode = (eCSTOperationMode)int.Parse(eVent.Items[eJOBDATA.CSTOperationMode].Value);
                    job.SubstrateType = (eSubstrateType)int.Parse(eVent.Items[eJOBDATA.SubstrateType].Value);
                    job.CIMMode = (eBitResult)int.Parse(eVent.Items[eJOBDATA.CIMMode].Value);
                    job.JobType = (eJobType)int.Parse(eVent.Items[eJOBDATA.JobType].Value);
                    job.JobJudge = eVent.Items[eJOBDATA.JobJudge].Value.ToString();
                    job.SamplingSlotFlag = eVent.Items[eJOBDATA.SamplingSlotFlag].Value.ToString();
                    //job.OXRInformationRequestFlag = eVent.Items[eJOBDATA.OXRInformationRequestFlag].Value.ToString(); // modify by bruce 2015/7/7 目前全廠不使用 
                    job.FirstRunFlag = eVent.Items[eJOBDATA.FirstRunFlag].Value.ToString();
                    //20180323 for t3 cell: 不具备改等级功能的line，不能updateJobGrade
                    //Modify By Yangzhenteng20180821 For T3 Cell:CUT LIne BUR机台拿掉Job Grade更新;
                    if (line.Data.LINETYPE == eLineType.CELL.CCSOR || line.Data.LINETYPE == eLineType.CELL.CCCHN||(line.Data.LINEID.Contains("CCCUT")&&eqp.Data.NODEID.Contains("CCBUR")))                      
                    { }
                    //Add By Yangzhenteng20190616
                    else if (line.Data.LINEID.Contains("CCPOL"))
                    {
                        if (SpecialEQPJobGradeUpdate && eqps.Count()!=0)
                        {
                            foreach (Equipment eq in eqps)
                            {
                                if (eq.Data.NODENO == inputData.Metadata.NodeNo)
                                {
                                    job.JobGrade = eVent.Items[eJOBDATA.JobGrade].Value.Trim();
                                }
                                else
                                { }
                            }
                        }
                        else
                        {
                            job.JobGrade = eVent.Items[eJOBDATA.JobGrade].Value.Trim();//此处需要做trim tom 2015-04-06
                        }
                                    
                    }
                    else
                    {
                        job.JobGrade = eVent.Items[eJOBDATA.JobGrade].Value.Trim();//此处需要做trim tom 2015-04-06
                    }
                    //Watson Modify 20150418 For CSOT 登京、福杰、俊成、懷莘討論，OXINFO與GLASS ID在流片時不能置換!!
                    //job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/CUTID"].Value.ToString().Trim(); //Watson Modify 20141125 For阿昌                   
                    job.PPID = eVent.Items[eJOBDATA.PPID].Value.ToString();

                    //Add Current EQPNo
                    //job.CurrentEQPNo = eqp.Data.NODENO;
                }
                #endregion
                #region [Glass Id]
                lock (job)
                {                    
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        //sy 20160810 move to UpdateJobDataby_CELL
                    }                    
                    else if (job.GlassChipMaskBlockID.Trim() != eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim())
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], EQP JOBDATA GlassChipMaskBlockID=[{6}] MISMATCH MES PRODUCTNAME=[{7}],  Cassette Sequence No=[{4}],Job Sequence No =[{5}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim(), job.GlassChipMaskBlockID.Trim()));
                    }
                }
                #region CF BM Line Titler Update GlassID
                // 20150111 add by Frank
                lock (job)
                {
                    if (line.Data.LINETYPE == "FCMPH_TYPE1")
                        if (eqp.Data.NODEATTRIBUTE == "TITLER")
                            job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();

                }
                #endregion
                #endregion
                eFabType fabtype;

                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabtype);

                switch (fabtype)
                {
                    case eFabType.ARRAY:
                        UpdateJobDataby_Array(line, job, eVent);
                        break;
                    case eFabType.CF:
                        UpdateJobDataby_CF(line, job, eVent);
                        break;
                    case eFabType.CELL:
                        UpdateJobDataby_CELL(line,eqp, job, eVent, inputData);
                        break;                        
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // add by bruce 20160411 for t2 Issue
                Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { Workbench.ServerName,
                    string.Format("Method:[UpdateJobDatabyLineType] Error:[{0}].", ex.ToString()) });
            }
        }

        // Array Job Data Update
        private void UpdateJobDataby_Array(Line line, Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.ArraySpecial.GlassFlowType = eVent.Items[eJOBDATA.GlassFlowType].Value;
                    job.ArraySpecial.ProcessType = eVent.Items[eJOBDATA.ProcessType].Value;
                    job.LastGlassFlag = eVent.Items[eJOBDATA.LastGlassFlag].Value;
                    job.ArraySpecial.RtcFlag = eVent.Items[eJOBDATA.RTCFlag].Value;

                    if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.PROCESS_EQ)
                    {
                        job.ArraySpecial.MainEQInFlag = eVent.Items[eJOBDATA.MainEQInFlag].Value;
                        job.ArraySpecial.RecipeGroupNumber = eVent.Items[eJOBDATA.RecipeGroupNumber].Value;
                    }

                    job.InspJudgedData = eVent.Items[eJOBDATA.InspJudgedData].Value;
                    job.TrackingData = eVent.Items[eJOBDATA.TrackingData].Value;
                    job.EQPFlag = eVent.Items[eJOBDATA.EQPFlag].Value;
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);

                    if (line.Data.JOBDATALINETYPE != eJobDataLineType.ARRAY.ABFG)
                    {
                        job.ArraySpecial.SourcePortNo = eVent.Items[eJOBDATA.SourcePortNo].Value;
                        job.ArraySpecial.TargetPortNo = eVent.Items[eJOBDATA.TargetPortNo].Value;
                    }

                    if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.TEST_1 ||
                        line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT)
                    {
                        job.TargetCSTID = eVent.Items[eJOBDATA.TargetCSTID].Value;
                    }

                    if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT)
                    {
                        job.ArraySpecial.SorterGrade = eVent.Items[eJOBDATA.SorterGrade].Value;
                    }

                    //if (eVent.Items["OXRInformation"].Value.Trim() != string.Empty)
                    //{
                    //    string oxrInfomation = string.Empty;
                    //    int count = 0;
                    //    for (int i = 0; i < job.ChipCount; i++)
                    //    {
                    //        if (i >= 56) break;
                    //        count ++;
                    //        oxrInfomation += ConstantManager["PLC_OXRINFO2_AC"][eVent.Items["OXRInformation"].Value.Substring(i * 4, 4)].Value;
                    //    }
                    //    job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, count);
                    //}

                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //CELL Special ProcessCompleteReport  Event Trigger Qtime
        public void ProcessCompleteReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    ProcessCompleteReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glassid = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PROCESS COMPLETE: CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glassid));

                #region Update WIP
                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                if (job == null)
                {
                    if ((casseqno == "0") || (jobseqno == "0"))
                    {
                        RecordJobHistoryForErrorData(inputData, string.Empty, string.Empty, string.Empty, eJobEvent.Store.ToString(), inputData.TrackKey);
                        string err = string.Format("[EQUIPMENT={0}] PROCESSCOMPLETE DATA ERROR!! CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASS_ID=[{3}].",
                        eqp.Data.NODENO, casseqno, jobseqno, glassid);
                        //object retVal = base.Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { eqp.Data.LINEID, err });
                        object retVal = base.Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { eqp.Data.LINEID,"WARNING", err });
                    }

                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, null);
                    //job.GlassChipMaskBlockID = glassid; //Watson Modify 20150421 不得更新WIP ，HISTORY
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, AND BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                        eqp.Data.NODENO, casseqno, jobseqno));
                }

                ProcessCompleteReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                #region Qtime 確認
                if (QtimeEventJudge(inputData.TrackKey, eqp.Data.LINEID, job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, "", eQtimeEventType.ProcCompEvent, eqp.File.CurrentRecipeID))
                {
                    //to do
                    //EX:job.EQPFlag.QtimeFlag = true;
                }

                if (job.GlassChipMaskBlockID.Trim() != glassid.Trim())
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB GlASSID[{3}] <> WIP GLASSID[{4}] IN CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!! EQP REPORT FAILED!!",
                        eqp.Data.NODENO, casseqno, jobseqno, glassid.Trim(), job.GlassChipMaskBlockID.Trim()));
                }
                else
                    ObjectManager.JobManager.EnqueueSave(job);
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region CF job Data Update
        private void UpdateJobDataby_CF(Line line, Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.INSPReservations = eVent.Items[eJOBDATA.INSPReservations].Value;
                    job.EQPReservations = eVent.Items[eJOBDATA.EQPReservations].Value;
                    job.LastGlassFlag = eVent.Items[eJOBDATA.LastGlassFlag].Value;

                    if (eVent.Items.AllKeys.Contains(eJOBDATA.InspJudgedData))
                        job.InspJudgedData = eVent.Items[eJOBDATA.InspJudgedData].Value;
                    else
                    {
                        job.InspJudgedData = eVent.Items[eJOBDATA.InspJudgedData1].Value;
                        job.InspJudgedData2 = eVent.Items[eJOBDATA.InspJudgedData2].Value;
                    }

                    job.TrackingData = eVent.Items[eJOBDATA.TrackingData].Value;
                    job.CFSpecialReserved = eVent.Items[eJOBDATA.CFSpecialReserved].Value;

                    if (eVent.Items.AllKeys.Contains(eJOBDATA.EQPFlag))
                        job.EQPFlag = eVent.Items[eJOBDATA.EQPFlag].Value;
                    else
                    {
                        job.EQPFlag = eVent.Items[eJOBDATA.EQPFlag1].Value;
                        job.EQPFlag2 = eVent.Items[eJOBDATA.EQPFlag2].Value;
                    }
                    //if (eVent.Items.AllKeys.Contains(eJOBDATA.MarcoReserveFlag))
                    //{
                    //    job.CFMarcoReserveFlag = eVent.Items[eJOBDATA.MarcoReserveFlag].Value;                       
                    //}

                    IDictionary<string, string> eQPFlagSubItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");
                    IDictionary<string, string> eQPFlag2SubItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag2, "EQPFlag2");
                    IDictionary<string, string> trackingDataSubItem = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                    IDictionary<string, string> inspJudgedDataSubItem = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData, "Insp.JudgedData");
                    IDictionary<string, string> inspJudgedData2SubItem = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData2, "Insp.JudgedData2");
                    IDictionary<string, string> cfSpecialReservedSubItem = ObjectManager.SubJobDataManager.Decode(job.CFSpecialReserved, "CFSpecialReserved");
                    IDictionary<string, string> eqpReservations = ObjectManager.SubJobDataManager.Decode(job.EQPReservations, "EQPReservations");
                    IDictionary<string, string> inspReservations = ObjectManager.SubJobDataManager.Decode(job.INSPReservations, "INSPReservations");
                    

                    switch (line.Data.JOBDATALINETYPE)
                    {
                        case eJobDataLineType.CF.PHOTO_BMPS:
                        case eJobDataLineType.CF.PHOTO_GRB:

                            job.CFMarcoReserveFlag = eVent.Items[eJOBDATA.MarcoReserveFlag].Value;
                            job.CFProcessBackUp = eVent.Items[eJOBDATA.ProcessBackUp].Value;
                            IDictionary<string, string> marcoReserveFlagSubItem = ObjectManager.SubJobDataManager.Decode(job.CFMarcoReserveFlag, "MarcoReserveFlag");
                            IDictionary<string, string> processBackUpSubItem = ObjectManager.SubJobDataManager.Decode(job.CFProcessBackUp, "ProcessBackUp");

                            #region Insp.JudgedData1
                            job.CfSpecial.InspJudgedData1.THK = inspJudgedDataSubItem["THK(Thickness)"];
                            job.CfSpecial.InspJudgedData1.AOI = inspJudgedDataSubItem["AOI"];
                            job.CfSpecial.InspJudgedData1.Macro = inspJudgedDataSubItem["Macro"];
                            job.CfSpecial.InspJudgedData1.TotalPitch_BM = inspJudgedDataSubItem["TotalPitch(BM)"];
                            #endregion

                            #region Insp.JudgedData2
                            job.CfSpecial.InspJudgedData2.CD = inspJudgedData2SubItem["CD"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.CoaterVCD01 = trackingDataSubItem["CoaterVCD#01"];
                            job.CfSpecial.TrackingData.CoaterVCD02 = trackingDataSubItem["CoaterVCD#02"];
                            job.CfSpecial.TrackingData.CV03HighTurnTable = trackingDataSubItem["CV#03HighTurnTable"];
                            job.CfSpecial.TrackingData.CV03LowTurnTable = trackingDataSubItem["CV#03LowTurnTable"];
                            if (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS)
                            {
                                job.CfSpecial.TrackingData.BMPS_Exposure = trackingDataSubItem["BMPSExposure"];
                                job.CfSpecial.TrackingData.BMPS_Exposure2 = trackingDataSubItem["BMPSExposure2"];
                            }
                            else
                            {
                                job.CfSpecial.TrackingData.BMPS_Exposure = trackingDataSubItem["RGBExposureCP#01"];
                                job.CfSpecial.TrackingData.BMPS_Exposure2 = trackingDataSubItem["RGBExposureCP#02"];
                            }
                            job.CfSpecial.TrackingData.Photo_OvenHP01 = trackingDataSubItem["OvenHP#01"];
                            job.CfSpecial.TrackingData.Photo_OvenHP02 = trackingDataSubItem["OvenHP#02"];
                            job.CfSpecial.TrackingData.Photo_CD = trackingDataSubItem["CD"];
                            job.CfSpecial.TrackingData.Macro = trackingDataSubItem["Macro"];
                            job.CfSpecial.TrackingData.TotalPitch = trackingDataSubItem["TotalPitch(BM)"];

                            #endregion

                            #region CF Special Reserved
                            job.CfSpecial.CFSpecialReserved.CPSlotNumber = cfSpecialReservedSubItem["CPSlotNumber"];
                            job.CfSpecial.CFSpecialReserved.HPSlotNumber = cfSpecialReservedSubItem["HPSlotNumber"];
                            job.CfSpecial.CFSpecialReserved.THKMuraJudgedData = cfSpecialReservedSubItem["THKMuraJudgedData"];
                            #endregion

                            #region EQP Flag1
                            job.CfSpecial.EQPFlag1.CleanerTurnModeFlag = eQPFlagSubItem["CleanerTurnModeFlag"];
                            job.CfSpecial.EQPFlag1.CoaterTurnModeFlag = eQPFlagSubItem["CoaterTurnModeFlag"];
                            job.CfSpecial.EQPFlag1.DeveloperTurnModeFlag = eQPFlagSubItem["DeveloperTurnModeFlag"];
                            job.CfSpecial.EQPFlag1.OvenTurnModeFlag = eQPFlagSubItem["OvenTurnModeFlag"];
                            job.CfSpecial.EQPFlag1.TurnTable01_CV01 = eQPFlagSubItem["TurnTable#01(CV#01)"];
                            job.CfSpecial.EQPFlag1.TurnTable02_CV02 = eQPFlagSubItem["TurnTable#02(CV#02)"];
                            job.CfSpecial.EQPFlag1.TurnTable03_CV03HighTurnTable = eQPFlagSubItem["TurnTable#03(CV#03HighT)"];
                            job.CfSpecial.EQPFlag1.TurnTable04_CV03LowTurnTable = eQPFlagSubItem["TurnTable#04(CV#03LowT)"];
                            job.CfSpecial.EQPFlag1.TurnTable05_CV05 = eQPFlagSubItem["TurnTable#05(CV#05)"];
                            job.CfSpecial.EQPFlag1.TurnTable06_CV06 = eQPFlagSubItem["TurnTable#06(CV#06)"];
                            job.CfSpecial.EQPFlag1.TurnTableInformation_CV03 = eQPFlagSubItem["TurnTableInformation(CV#03)"];
                            job.CfSpecial.EQPFlag1.THKBypassFlag = eQPFlagSubItem["THKBypassFlag"];
                            job.CfSpecial.EQPFlag1.ShortCutPermissionFlag_CV06 = eQPFlagSubItem["ShortCutPermissionFlag(ForCV#06)"];
                            #endregion

                            #region EQP Flag2
                            job.CfSpecial.EQPFlag2.ExposureQTimeOverRWFlag = eQPFlag2SubItem["ExposureQTimeOverRWFlag"];
                            job.CfSpecial.EQPFlag2.ExposureQTimeOverNGFlag = eQPFlag2SubItem["ExposureQTimeOverNGFlag"];
                            job.CfSpecial.EQPFlag2.DeveloperQTimeOverNGFlag = eQPFlag2SubItem["DeveloperQTimeOverNGFlag"];
                            job.CfSpecial.EQPFlag2.HPCPOverBakeFlag = eQPFlag2SubItem["HPCPOverBakeFlag"];
                            job.CfSpecial.EQPFlag2.OvenOverBakeFlag = eQPFlag2SubItem["OvenOverBakeFlag"];
                            job.CfSpecial.EQPFlag2.ExposureNGFlag = eQPFlag2SubItem["ExposureNGFlag"];
                            job.CfSpecial.EQPFlag2.ExposureProcessFlag = eQPFlag2SubItem["ExposureProcessFlag"];
                            job.CfSpecial.EQPFlag2.TitlerBypassFlag = eQPFlag2SubItem["TitlerBypassFlag"];
                            job.CfSpecial.EQPFlag2.AOIBypassFlag = eQPFlag2SubItem["AOIBypassFlag"];
                            job.CfSpecial.EQPFlag2.AOI_CDOL_InspectionFlag = eQPFlag2SubItem["AOICDOLInspectionFlag"];
                            job.CfSpecial.EQPFlag2.ShortCutGlassFlag_ForCV06 = eQPFlag2SubItem["ShortCutGlassFlag(ForCV#06)"];
                            job.CfSpecial.EQPFlag2.ShortCutForcedSamplingFlag_ForCV06 = eQPFlag2SubItem["ShortCutForcedSamplingFlag(ForCV#06)"];
                            job.CfSpecial.EQPFlag2.ShortCutFailFlag = eQPFlag2SubItem["ShortCutFailFlag"];
                            job.CfSpecial.EQPFlag2.MP1 = eQPFlag2SubItem["MP1"];
                            job.CfSpecial.EQPFlag2.MP2 = eQPFlag2SubItem["MP2"];
                            job.CfSpecial.EQPFlag2.TotalPitchOfflineInspectionFlag = eQPFlag2SubItem["TotalPitchOfflineInspectionFlag"];
                            #endregion

                            job.CfSpecial.CoaterCSPNo = eVent.Items["CoaterCSPNo"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.AbnormalCode.CSPNUMBER = eVent.Items["CoaterCSPNo"].Value;
                            job.CfSpecial.OvenHPSlotNumber = eVent.Items["OvenHPSlotNumber"].Value;
                            job.CfSpecial.InlineReworkRealCount = eVent.Items["InlineReworkRealCount"].Value;

                            #region MarcoReserveFlag
                            job.CfSpecial.CFMarcoReserve.BM = marcoReserveFlagSubItem["BM"];
                            job.CfSpecial.CFMarcoReserve.R = marcoReserveFlagSubItem["R"];
                            job.CfSpecial.CFMarcoReserve.G = marcoReserveFlagSubItem["G"];
                            job.CfSpecial.CFMarcoReserve.B = marcoReserveFlagSubItem["B"];
                            job.CfSpecial.CFMarcoReserve.OC = marcoReserveFlagSubItem["OC"];
                            job.CfSpecial.CFMarcoReserve.PS = marcoReserveFlagSubItem["PS"];
                            #endregion

                            #region ProcessBackUp
                            job.CfSpecial.CFProcessBackUp.BM = processBackUpSubItem["BackUpBMProcessFlag"];
                            job.CfSpecial.CFProcessBackUp.R = processBackUpSubItem["BackUpRProcessFlag"];
                            job.CfSpecial.CFProcessBackUp.G = processBackUpSubItem["BackUpGProcessFlag"];
                            job.CfSpecial.CFProcessBackUp.B = processBackUpSubItem["BackUpBProcessFlag"];
                            job.CfSpecial.CFProcessBackUp.OC = processBackUpSubItem["BackUpOCProcessFlag"];
                            job.CfSpecial.CFProcessBackUp.PS = processBackUpSubItem["BackUpPSProcessFlag"];
                            #endregion

                            job.CfSpecial.SamplingValue = eVent.Items["SamplingValue"].Value;
                            break;
                        case eJobDataLineType.CF.REWORK:

                            #region EQP Flag
                            job.CfSpecial.EQPFlag1.EtchingProcessAbnormalFlag = eQPFlagSubItem["EtchingProcessAbnormalFlag"];
                            job.CfSpecial.EQPFlag1.StripperProcessAbnormalFlag = eQPFlagSubItem["StripperProcessAbnormalFlag"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.Etching = trackingDataSubItem["Etching"];
                            job.CfSpecial.TrackingData.Stripper = trackingDataSubItem["Stripper"];
                            #endregion

                            job.CfSpecial.ReworkRealCount = eVent.Items["ReworkRealCount"].Value;
                            job.CfSpecial.VCRMismatchFlag = eVent.Items["VCRMismatchFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            job.CellSpecial.MaxRwkCount = eVent.Items[eJOBDATA.MaxRwkCount].Value.ToString();
                            job.CellSpecial.CurrentRwkCount = eVent.Items[eJOBDATA.CurrentRwkCount].Value.ToString();
                            break;
                        case eJobDataLineType.CF.UNPACK:

                            break;

                        case eJobDataLineType.CF.MASK:

                            #region Tracking Data
                            job.CfSpecial.TrackingData.EUV = trackingDataSubItem["EUV"];
                            job.CfSpecial.TrackingData.MaskInspection = trackingDataSubItem["MaskInspection"];
                            job.CfSpecial.TrackingData.MaskCleaner = trackingDataSubItem["MaskCleaner"];
                            job.CfSpecial.TrackingData.MaskAOI = trackingDataSubItem["MaskAOI"];
                            #endregion

                            #region CF Special Reserved
                            job.CfSpecial.CFSpecialReserved.StockNumber = cfSpecialReservedSubItem["StockNumber"];
                            job.CfSpecial.CFSpecialReserved.StockSlotNumber = cfSpecialReservedSubItem["StockSlotNumber"];
                            #endregion

                            break;

                        case eJobDataLineType.CF.REPAIR:

                            #region EQP Flag
                            job.CfSpecial.EQPFlag1.InkRepairGlass = eQPFlagSubItem["InkRepairGlass"];
                            job.CfSpecial.EQPFlag1.RepairGlass = eQPFlagSubItem["RepairGlass"];//2015/03/06 by chia-cheng
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.Repair01 = trackingDataSubItem["Repair#01"];
                            job.CfSpecial.TrackingData.Repair02 = trackingDataSubItem["Repair#02"];
                            job.CfSpecial.TrackingData.Repair03 = trackingDataSubItem["Repair#03"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.Repair01 = inspJudgedDataSubItem["Repair#01"];
                            job.CfSpecial.InspJudgedData1.InkRepair02 = inspJudgedDataSubItem["Ink/Repair#02"];
                            job.CfSpecial.InspJudgedData1.InkRepair03 = inspJudgedDataSubItem["Ink/Repair#03"];
                            #endregion

                            #region EQP Reservations
                            job.CfSpecial.EQPReservations.Repair01 = eqpReservations["Repair#01"];
                            job.CfSpecial.EQPReservations.InkRepair02 = eqpReservations["Ink/Repair#02"];
                            job.CfSpecial.EQPReservations.InkRepair03 = eqpReservations["Ink/Repair#02"];
                            #endregion

                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            break;

                        case eJobDataLineType.CF.MQC_2:

                            #region INSP Reservations
                            job.CfSpecial.InspReservations.MQCCD = inspReservations["CD"];
                            job.CfSpecial.InspReservations.MCPD = inspReservations["MCPD"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.CD = inspJudgedDataSubItem["CD"];
                            job.CfSpecial.InspJudgedData1.MCPD = inspJudgedDataSubItem["MCPD"];
                            #endregion
                            
                            #region Tracking Data
                            job.CfSpecial.TrackingData.CD = trackingDataSubItem["CD"];
                            job.CfSpecial.TrackingData.MCPD=trackingDataSubItem["MCPD"];
                            #endregion
                           
                            job.CfSpecial.FlowPriorityInfo = eVent.Items["FlowPriorityInfo"].Value;
                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            break;

                        case eJobDataLineType.CF.MQC_1:

                            #region INSP Reservations
                            job.CfSpecial.InspReservations.TTP = inspReservations["TTP"];
                            job.CfSpecial.InspReservations.MCPD = inspReservations["MCPD"];
                            job.CfSpecial.InspReservations.SP = inspReservations["SP"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.TTP = inspJudgedDataSubItem["TTP"];
                            job.CfSpecial.InspJudgedData1.MCPD = inspJudgedDataSubItem["MCPD"];
                            job.CfSpecial.InspJudgedData1.SP = inspJudgedDataSubItem["SP"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.TTP = trackingDataSubItem["TTP"];
                            job.CfSpecial.TrackingData.MCPD=trackingDataSubItem["MCPD"];
                            job.CfSpecial.TrackingData.SP = trackingDataSubItem["SP"];
                            #endregion

                            job.CfSpecial.FlowPriorityInfo = eVent.Items["FlowPriorityInfo"].Value;
                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            break;

                        case eJobDataLineType.CF.FCMAC:

                            #region INSP Reservations
                            job.CfSpecial.InspReservations.Macro01 = inspReservations["Macro#01"];
                            job.CfSpecial.InspReservations.Macro02 = inspReservations["Macro#02"];
                            job.CfSpecial.InspReservations.Macro03 = inspReservations["Macro#03"];
                            #endregion

                            #region EQP Flag
                            job.CfSpecial.EQPFlag1.FIPGlass = eQPFlagSubItem["FIPGlass"];
                            job.CfSpecial.EQPFlag1.MQCGlass = eQPFlagSubItem["MQCGlass"];
                            job.CfSpecial.EQPFlag1.BMacroGlass = eQPFlagSubItem["BMacroGlass"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.Macro01 = trackingDataSubItem["Macro#01"];
                            job.CfSpecial.TrackingData.Macro02 = trackingDataSubItem["Macro#02"];
                            job.CfSpecial.TrackingData.Macro03 = trackingDataSubItem["Macro#03"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.Macro01 = inspJudgedDataSubItem["Macro#01"];
                            job.CfSpecial.InspJudgedData1.Macro02 = inspJudgedDataSubItem["Macro#02"];
                            job.CfSpecial.InspJudgedData1.Macro03 = inspJudgedDataSubItem["Macro#03"];
                            #endregion

                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            break;

                        case eJobDataLineType.CF.FCSRT:

                            job.CfSpecial.VCRMismatchFlag = eVent.Items["VCRMismatchFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            break;

                        case eJobDataLineType.CF.FCPSH:

                            #region INSP Reservations
                            job.CfSpecial.InspReservations.PSH=inspReservations["PSH"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.IJPSH1 = inspJudgedDataSubItem["PSH1"];
                            job.CfSpecial.InspJudgedData1.IJPSH2 = inspJudgedDataSubItem["PSH2"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.TDPSH1 = trackingDataSubItem["PSH1"];
                            job.CfSpecial.TrackingData.TDPSH2 = trackingDataSubItem["PSH2"];
                            #endregion

                            #region CF Special Reserved
                            job.CfSpecial.CFSpecialReserved.ForcePSHbit = cfSpecialReservedSubItem["ForcePSHbit"];
                            #endregion

                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            job.CfSpecial.TargetCSTID = eVent.Items["TargetCassetteID"].Value;
                            job.CfSpecial.TargetPortNo = eVent.Items["TargetPortNo"].Value;
                            job.CfSpecial.TargetSlotNo = eVent.Items["TargetSlotNo"].Value;
                            job.CfSpecial.SourcePortNo = eVent.Items["SourcePortNo"].Value;
                            break;
                            
                        case eJobDataLineType.CF.FCAOI:

                            #region INSP Reservations
                            job.CfSpecial.InspReservations.AOI = inspReservations["AOI"];
                            #endregion

                            #region Insp.JudgedData
                            job.CfSpecial.InspJudgedData1.AOI= inspJudgedDataSubItem["AOI"];
                            #endregion

                            #region Tracking Data
                            job.CfSpecial.TrackingData.AOI = trackingDataSubItem["AOI"];
                            #endregion

                            job.CfSpecial.RTCFlag = eVent.Items["RTCFlag"].Value;
                            job.CfSpecial.LoaderBufferingFlag = eVent.Items["LoaderBufferingFlag"].Value;
                            break;
                    }

                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Cell Job Data Update 
        #region [T2]
        //private void UpdateJobDataby_Cell_PIL(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString());
        //            job.CellSpecial.TurnAngle = eVent.Items["TurnAngle"].Value.ToString();
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
        //            job.CellSpecial.PPOSlotNo = eVent.Items["PPOSlotNo"].Value.ToString();
        //            job.CellSpecial.ReworkCount = eVent.Items["ReworkCount"].Value.ToString();
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //private void UpdateJobDataby_Cell_ODF(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
        //            job.CellSpecial.CFCassetteSeqNo = eVent.Items["CFCasetteSeqNo"].Value.ToString();
        //            job.CellSpecial.CFJobSeqNo = eVent.Items["CFJobSeqno"].Value.ToString();
        //            job.CellSpecial.ODFBoxChamberOpenTime01 = eVent.Items["ODFBoxChamberOpenTime#01"].Value.ToString();
        //            job.CellSpecial.ODFBoxChamberOpenTime02 = eVent.Items["ODFBoxChamberOpenTime#02"].Value.ToString();
        //            job.CellSpecial.ODFBoxChamberOpenTime03 = eVent.Items["ODFBoxChamberOpenTime#03"].Value.ToString();
        //            job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
        //            job.CellSpecial.RepairCount = eVent.Items["RepairCount"].Value.ToString();
        //            job.CellSpecial.TurnAngle = eVent.Items["TurnAngle"].Value.ToString();
        //            job.CellSpecial.UVMaskAlreadyUseCount = eVent.Items["UVMaskAlreadyUseCount"].Value.ToString();
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        private void UpdateJobDataby_Cell_HVA(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
                    job.CellSpecial.TurnAngle = eVent.Items["TurnAngle"].Value.ToString();
                    job.CellSpecial.ReturnModeTurnAngle = eVent.Items["ReturnModeTurnAngle"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //private void UpdateJobDataby_Cell_CUT(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
        //            job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
        //            job.CellSpecial.TurnAngle = eVent.Items["TurnAngle"].Value.ToString();
        //            job.CellSpecial.CrossLineCassetteSettingCode = eVent.Items["CrossLineCassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.PanelSizeFlag = eVent.Items["PanelSizeFlag"].Value.ToString();
        //            job.CellSpecial.MMGFlag = eVent.Items["MMGFlag"].Value.ToString();
        //            job.CellSpecial.CrossLinePanelSize = eVent.Items["CrossLinePanelSize"].Value.ToString();
        //            job.CellSpecial.CUTProductID = eVent.Items["CUTProductID"].Value.ToString();
        //            job.CellSpecial.CUTCrossProductID = eVent.Items["CUTCrossProductID"].Value.ToString();
        //            job.CellSpecial.CUTProductType = eVent.Items["CUTProductType"].Value.ToString();
        //            job.CellSpecial.CUTCrossProductType = eVent.Items["CUTCrossProductType"].Value.ToString();
        //            job.CellSpecial.POLProductType = eVent.Items["POLProductType"].Value.ToString();
        //            job.CellSpecial.POLProductID = eVent.Items["POLProductID"].Value.ToString();
        //            job.CellSpecial.CrossLinePPID = eVent.Items["CrossLinePPID"].Value.ToString();
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //private void UpdateJobDataby_Cell_POL(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
        //            //job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString(); Watson modify 20141209 For New IO
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        private void UpdateJobDataby_Cell_DPK(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_PMT(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
                    job.CellSpecial.RunMode = eVent.Items["RunMode"].Value.ToString();
                    job.CellSpecial.NodeStack = eVent.Items["NodeStack"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //private void UpdateJobDataby_Cell_GAP(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
        //            job.CellSpecial.NodeStack = eVent.Items["NodeStack"].Value.ToString();
        //            job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        private void UpdateJobDataby_Cell_PIS(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_PRM(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                    job.CellSpecial.RunMode = eVent.Items["RunMode"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_GMO(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_LOI(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                    job.CellSpecial.RepairResult = eVent.Items["RepairResult"].Value.ToString();
                    job.CellSpecial.RunMode = eVent.Items["RunMode"].Value.ToString();
                    //job.CellSpecial.VirtualPortEnableMode = eVent.Items["VirtualPortEnableMode"].Value.ToString();  Jun Modify 20150107 For New IO
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_NRP(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                    job.CellSpecial.RepairResult = eVent.Items["RepairResult"].Value.ToString();
                    job.CellSpecial.RepairCount = eVent.Items["RepairCount"].Value.ToString();
                    job.CellSpecial.RunMode = eVent.Items["RunMode"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_OLS(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //private void UpdateJobDataby_Cell_SOR(Job job, Event eVent)
        //{
        //    try
        //    {
        //        lock (job)
        //        {
        //            job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

        //            if (job.CellSpecial == null)
        //                job.CellSpecial = new JobCellSpecial();

        //            job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
        //            job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
        //            job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
        //            //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
        //            //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
        //            //{
        //            string oxrInfomation = string.Empty;
        //            int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
        //            //for (int i = 0; i < chipcount; i++)
        //            //{
        //            //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
        //            //}
        //            //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
        //            //}

        //            job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
        //            job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
        //            job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
        //            job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
        //            job.CellSpecial.RunMode = eVent.Items["RunMode"].Value.ToString();
        //            job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
        //            job.CellSpecial.NodeStack = eVent.Items["NodeStack"].Value.ToString();
        //        }
        //        ObjectManager.JobManager.EnqueueSave(job);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        private void UpdateJobDataby_Cell_DPS(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_ATS(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.NetworkNo = eVent.Items["NetworkNo"].Value.ToString();
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                    job.CellSpecial.PanelSize = eVent.Items["PanelSize"].Value.ToString();
                    job.CellSpecial.NodeStack = eVent.Items["NodeStack"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_DPI(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.AbnormalCode = eVent.Items["AbnormalCode"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_UVA(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {
                    job.LastGlassFlag = eVent.Items["LastGlassFlag"].Value.ToString();

                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.InspJudgedData = eVent.Items["Insp.JudgedData"].Value.ToString();
                    job.TrackingData = eVent.Items["TrackingData"].Value.ToString();
                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                    //job.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                    //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                    //{
                    string oxrInfomation = string.Empty;
                    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                    //for (int i = 0; i < chipcount; i++)
                    //{
                    //    oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                    //}
                    //job.OXRInformation = oxrInfomation + job.OXRInformation.Remove(0, chipcount);
                    //}

                    job.CellSpecial.ControlMode = (eHostMode)int.Parse(eVent.Items["ControlMode"].Value.ToString()); ;
                    job.CellSpecial.ProductID = eVent.Items["ProductID"].Value.ToString();
                    job.CellSpecial.CassetteSettingCode = eVent.Items["CassetteSettingCode"].Value.ToString();
                    job.CellSpecial.OwnerID = eVent.Items["OwnerID"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UpdateJobDataby_Cell_MCL(Job job, Event eVent)
        {
            try
            {
                lock (job)
                {


                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();
                    job.CellSpecial.MASKID = eVent.Items["MaskID"].Value.ToString();

                    job.EQPFlag = eVent.Items["EQPFlag"].Value.ToString();
                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region T3
        private void UpdateJobDataby_CELL(Line line, Equipment eqp, Job job, Event eVent, Trx inputData)
        {
            try
            {
                lock (job)
                {
                    #region [Glass Id]
                    if (inputData.Name.Contains(keyEQPEvent.JobDataEditReport))//20160131 sy by line/JobDataEditReport 來更新GlassID 
                    {
                        job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
                    }
                    else if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))//20160104 sy by line/EQP CUT PCS SendoutEvent 來更新GlassID 不判斷GlassID不同LOG 移到外面
                    {
                        if (eqp.Data.NODENO.Contains("L3") && (eqp.Data.NODENAME.Contains(keyCellLineType.CUT) || eqp.Data.NODENAME.Contains(keyCellLineType.PCS)))
                        {
                            job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
                        }
                        if (eqp.Data.NODENO.Contains("L4") && line.Data.LINETYPE == eLineType.CELL.CCCUT_5)//sy add CUT_5 MDI RemoveJobDataReport 會Update job 但GLASSID 不會update ,由CutLDUpdate 20160810
                        {
                            job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
                        }
                    }
                    else if (inputData.Name.Contains(keyEQPEvent.RemoveJobDataReport))//sy add 20160718 CUT MDI no send out ,store,fetchout ,remark20160810 
                    {
                        //if (eqp.Data.NODENO.Contains("L3") && eqp.Data.NODENAME.Contains(keyCellLineType.CUT) && line.Data.LINETYPE == eLineType.CELL.CCCUT_5)
                        //{
                        //    job.GlassChipMaskBlockID = eVent.Items["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
                        //}
                    }
                    else if (job.GlassChipMaskBlockID.Trim() != eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim())
                    { 
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}],Node=[{3}], EQP JOBDATA GlassChipMaskBlockID=[{6}] MISMATCH MES PRODUCTNAME=[{7}],  Cassette Sequence No=[{4}],Job Sequence No =[{5}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim(), job.GlassChipMaskBlockID.Trim()));
                    }

                    #endregion
                    #region [By Event]
                    //CUT PCS 須由機台UPDate
                    if (inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport) && eqp.Data.NODENO.Contains("L3") &&
                    (eqp.Data.NODENAME.Contains(keyCellLineType.CUT) || eqp.Data.NODENAME.Contains(keyCellLineType.PCS)))
                    {
                        job.ProductType.Value = int.Parse(eVent.Items[eJOBDATA.ProductType].Value);
                        job.CellSpecial.ProductID = eVent.Items[eJOBDATA.ProductID].Value.ToString();
                        job.ProductID.Value = int.Parse(eVent.Items[eJOBDATA.ProductID].Value);
                    }
                    //防止最後一筆 更新 WIP，再上CST後，會更新到ProductTypeManange sy Modify 20160307
                    if (inputData.Name.Contains(keyEQPEvent.JobDataEditReport))
                    {
                        job.CellSpecial.ProductID = eVent.Items[eJOBDATA.ProductID].Value.ToString();
                        job.ProductID.Value = int.Parse(eVent.Items[eJOBDATA.ProductID].Value);
                    }
                    //sy add 20160718 CUT MDI no send out ,store,fetchout
                    if (inputData.Name.Contains(keyEQPEvent.RemoveJobDataReport))
                    {
                        if (eqp.Data.NODENO.Contains("L3") && eqp.Data.NODENAME.Contains(keyCellLineType.CUT) && line.Data.LINETYPE == eLineType.CELL.CCCUT_5)
                        {
                            job.ProductType.Value = int.Parse(eVent.Items[eJOBDATA.ProductType].Value);
                            job.CellSpecial.ProductID = eVent.Items[eJOBDATA.ProductID].Value.ToString();
                            job.ProductID.Value = int.Parse(eVent.Items[eJOBDATA.ProductID].Value);
                        }
                    }
                    #endregion
                    if (job.CellSpecial == null)
                        job.CellSpecial = new JobCellSpecial();

                    job.INSPReservations = eVent.Items[eJOBDATA.INSPReservations].Value.ToString();
                    job.EQPReservations = eVent.Items[eJOBDATA.EQPReservations].Value.ToString();
                    job.LastGlassFlag = eVent.Items[eJOBDATA.LastGlassFlag].Value.ToString();
                    job.InspJudgedData = eVent.Items[eJOBDATA.InspJudgedData].Value.ToString();
                    job.TrackingData = eVent.Items[eJOBDATA.TrackingData].Value.ToString();
                    job.EQPFlag = eVent.Items[eJOBDATA.EQPFlag].Value.ToString();
                    job.ChipCount = int.Parse(eVent.Items[eJOBDATA.ChipCount].Value);

                    job.CellSpecial.CassetteSettingCode = eVent.Items[eJOBDATA.CassetteSettingCode].Value.ToString();
                    switch (line.Data.JOBDATALINETYPE)
                    {
                        case eJobDataLineType.CELL.CCPIL:
                            job.CellSpecial.BlockOXInformation = ObjectManager.JobManager.P2M_CELL_BlockOXR_OX(eVent.Items[eJOBDATA.BlockOXInformation].Value.ToString());
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.GlassThickness = eVent.Items[eJOBDATA.GlassThickness].Value.ToString();
                            job.CellSpecial.OperationID = eVent.Items[eJOBDATA.OperationID].Value.ToString();
                            job.CellSpecial.OwnerID = eVent.Items[eJOBDATA.OwnerID].Value.ToString();
                            job.CellSpecial.ProductOwner = eVent.Items[eJOBDATA.ProductOwner].Value.ToString();
                            job.CellSpecial.PILiquidType = eVent.Items[eJOBDATA.PILiquidType].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCODF:
                            job.CellSpecial.BlockOXInformation = ObjectManager.JobManager.P2M_CELL_BlockOXR_OX(eVent.Items[eJOBDATA.BlockOXInformation].Value.ToString());
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.GlassThickness = eVent.Items[eJOBDATA.GlassThickness].Value.ToString();
                            job.CellSpecial.OperationID = eVent.Items[eJOBDATA.OperationID].Value.ToString();
                            job.CellSpecial.OwnerID = eVent.Items[eJOBDATA.OwnerID].Value.ToString();
                            job.CellSpecial.ProductOwner = eVent.Items[eJOBDATA.ProductOwner].Value.ToString();
                            job.CellSpecial.AssembleSeqNo = eVent.Items[eJOBDATA.AssembleSeqNo].Value.ToString();
                            job.CellSpecial.UVMaskAlreadyUseCount = eVent.Items[eJOBDATA.UVMaskAlreadyUseCount].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCPCS:
                            job.CellSpecial.BlockOXInformation = ObjectManager.JobManager.P2M_CELL_BlockOXR_OX(eVent.Items[eJOBDATA.BlockOXInformation].Value.ToString());
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.BlockSize = eVent.Items[eJOBDATA.BlockSize].Value.ToString();
                            //job.CellSpecial.CUTProductID = eVent.Items[eJOBDATA.PCSProductID].Value.ToString();
                            //job.CellSpecial.CUTProductType = eVent.Items[eJOBDATA.PCSProductType].Value.ToString();
                           // job.CellSpecial.CUTProductID2 = eVent.Items[eJOBDATA.PCSProductID2].Value.ToString();
                           // job.CellSpecial.CUTProductType2 = eVent.Items[eJOBDATA.PCSProductType2].Value.ToString();
                            job.CellSpecial.PCSCassetteSettingCodeList = eVent.Items[eJOBDATA.PCSCassetteSettingCodeList].Value.ToString();
                            //20170725 huangjiayin add: pcsblocksizelist
                            job.CellSpecial.PCSBlockSizeList = eVent.Items[eJOBDATA.PCSBlockSizeList].Value.ToString();
                            job.CellSpecial.BlockSize1 = eVent.Items[eJOBDATA.BlockSize1].Value.ToString();
                            job.CellSpecial.BlockSize2 = eVent.Items[eJOBDATA.BlockSize2].Value.ToString();
                            job.CellSpecial.CUTCassetteSettingCode = eVent.Items[eJOBDATA.PCSCassetteSettingCode].Value.ToString();
                            job.CellSpecial.CUTCassetteSettingCode2 = eVent.Items[eJOBDATA.PCSCassetteSettingCode2].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCCUT:
                            if (inputData.Name.Contains(keyEQPEvent.RemoveJobDataReport))
                            { //sy modify CUT_5 MDI RemoveJobDataReport 會Update job 但是每片job PanelOXInformation不同 不updata 20160810
                            }
                            else
                            {
                                job.CellSpecial.PanelOXInformation = ObjectManager.JobManager.P2M_CELL_PanelInt2OX(eVent.Items[eJOBDATA.PanelOXInformation].Value.ToString());
                            }
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.PanelSize = eVent.Items[eJOBDATA.PanelSize].Value.ToString();
                            job.CellSpecial.CUTProductID = eVent.Items[eJOBDATA.CUTProductID].Value.ToString();
                            job.CellSpecial.CUTProductType = eVent.Items[eJOBDATA.CUTProductType].Value.ToString();
                            job.CellSpecial.DefectCode = eVent.Items[eJOBDATA.DefectCode].Value.ToString();
                            job.CellSpecial.RejudgeCount = eVent.Items[eJOBDATA.RejudgeCount].Value.ToString();
                            job.CellSpecial.VendorName = eVent.Items[eJOBDATA.VendorName].Value.ToString();
                            job.CellSpecial.BURCheckCount = eVent.Items[eJOBDATA.BURCheckCount].Value.ToString();
                            job.CellSpecial.CUTCassetteSettingCode = eVent.Items[eJOBDATA.CUTCassetteSettingCode].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCPCK:
                            job.CellSpecial.PanelGroup = eVent.Items[eJOBDATA.PanelGroup].Value.ToString();
                            job.CellSpecial.OQCBank = eVent.Items[eJOBDATA.OQCBank].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCPDR:
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.GlassThickness = eVent.Items[eJOBDATA.GlassThickness].Value.ToString();
                            job.CellSpecial.OperationID = eVent.Items[eJOBDATA.OperationID].Value.ToString();
                            job.CellSpecial.OwnerID = eVent.Items[eJOBDATA.OwnerID].Value.ToString();
                            job.CellSpecial.ProductOwner = eVent.Items[eJOBDATA.ProductOwner].Value.ToString();
                            job.CellSpecial.MaxRwkCount = eVent.Items[eJOBDATA.MaxRwkCount].Value.ToString();
                            job.CellSpecial.CurrentRwkCount = eVent.Items[eJOBDATA.CurrentRwkCount].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCTAM:
                        case eJobDataLineType.CELL.CCPTH:
                        case eJobDataLineType.CELL.CCGAP:
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.GlassThickness = eVent.Items[eJOBDATA.GlassThickness].Value.ToString();
                            job.CellSpecial.OperationID = eVent.Items[eJOBDATA.OperationID].Value.ToString();
                            job.CellSpecial.OwnerID = eVent.Items[eJOBDATA.OwnerID].Value.ToString();
                            job.CellSpecial.ProductOwner = eVent.Items[eJOBDATA.ProductOwner].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCRWT:
                        case eJobDataLineType.CELL.CCCRP:
                            job.CellSpecial.DotRepairCount = eVent.Items[eJOBDATA.DotRepairCount].Value.ToString();
                            job.CellSpecial.LineRepairCount = eVent.Items[eJOBDATA.LineRepairCount].Value.ToString();
                            job.CellSpecial.DefectCode = eVent.Items[eJOBDATA.DefectCode].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCPOL:
                            job.CellSpecial.DefectCode = eVent.Items[eJOBDATA.MainDefectCode].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCSOR:
                            //job.CellSpecial.SortFlagNo = eVent.Items[eJOBDATA.SortFlagNo].Value.ToString();
                            break;
                        case eJobDataLineType.CELL.CCRWK:
                        case eJobDataLineType.CELL.CCQUP:
                        //case eJobDataLineType.CELL.CCQPP:
                        //case eJobDataLineType.CELL.CCPPK:
                        case eJobDataLineType.CELL.CCCHN:
                        case eJobDataLineType.CELL.CCQSR:
                            break;
                            // add by huangjiayin for t3 notch
                        case eJobDataLineType.CELL.CCNLS:
                        case eJobDataLineType.CELL.CCNRD:
                            job.CellSpecial.TurnAngle = eVent.Items[eJOBDATA.TurnAngle].Value.ToString();
                            job.CellSpecial.PanelSize = eVent.Items[eJOBDATA.PanelSize].Value.ToString();
                            job.CellSpecial.PanelOXInformation = ObjectManager.JobManager.P2M_CELL_PanelInt2OX(eVent.Items[eJOBDATA.PanelOXInformation].Value.ToString());
                            break;
                        default:
                            return; ;
                    }

                }
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region Reply PLC Data and Time out
        private void SendOutJobDataReportReply(string eqpNo, string commandNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_SendOutJobDataReportReply#" + commandNo) as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + SendOutTimeout + "_" + commandNo))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + SendOutTimeout + "_" + commandNo);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + SendOutTimeout + "_" + commandNo, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(SendOutJobDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] REPLY#{2}, SET BIT=[{3}].",
                    eqpNo, trackKey, commandNo, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void SendOutJobDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SEND OUT JOB DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                SendOutJobDataReportReply(sArray[0], sArray[2], eBitResult.OFF, trackKey); //EX: sArray[2] :#1;sArray[2] :#2....
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReceiveJobDataReply(string eqpNo, string commandNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_ReceiveJobDataReportReply#" + commandNo) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ReceiveTimeout + "_" + commandNo))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ReceiveTimeout + "_" + commandNo);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ReceiveTimeout + "_" + commandNo, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ReceiveJobDataReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] REPLY#{2}, SET BIT=[{3}].",
                    eqpNo, trackKey, commandNo, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ReceiveJobDataReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECEIVE JOB DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                ReceiveJobDataReply(sArray[0], sArray[2], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void StoreJobDataReportReply(string commandNo,string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata;
                 if (string.IsNullOrEmpty(commandNo))
                outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_StoreJobDataReportReply") as Trx;
                 else
                     outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_StoreJobDataReportReply#" + commandNo) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);


                    if (_timerManager.IsAliveTimer(eqpNo + "_" + StoreTimeout+"_"+commandNo))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + StoreTimeout + "_" + commandNo);
                    }



                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + StoreTimeout + "_" + commandNo, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(StoreJobDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void StoreJobDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] STORE JOB DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                StoreJobDataReportReply(sArray[2], sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void FetchOutJobDataReportReply(string commandNo,string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata;
                 if (string.IsNullOrEmpty(commandNo))
                outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_FetchOutJobDataReportReply") as Trx;
                 else
                     outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_FetchOutJobDataReportReply#"+commandNo) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + FetchTimeout + "_" + commandNo))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + FetchTimeout + "_" + commandNo);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + FetchTimeout + "_" + commandNo, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(FetchOutJobDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void FetchOutJobDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FETCH OUT JOB DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                FetchOutJobDataReportReply(sArray[2],sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //CELL Special Event Trigger Qtime
        private void ProcessCompleteReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_ProcessCompleteReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ProcCompTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ProcCompTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ProcCompTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ProcessCompleteReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ProcessCompleteReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PROCESS COMPLETE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                ProcessCompleteReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RemoveJobReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RemoveJobReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + RemoveTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + RemoveTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + RemoveTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RemoveJobReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,Remove Job Reply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RemoveJobReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Remove Job Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                RemoveJobReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void JobDataEditReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_JobDataEditReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + JobEditTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + JobEditTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + JobEditTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(JobDataEditReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void JobDataEditReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB DATA EDIT REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                JobDataEditReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        ////Job Remove Recovery Command Block
        //private void JobRemoveRecoveryCommand(string eqpNo,string cstseqno,string  jobseqno, eBitResult value, eJobCommand jobcmd, eLastFlag lastGlassFlag ,string trackKey)
        //{
        //    try
        //    {
        //        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_JobDataEditReportReply") as Trx;
        //        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
        //        outputdata.EventGroups[0].Events[1].Items[0].Value = jobcmd.ToString();
        //        outputdata.EventGroups[0].Events[1].Items[1].Value = cstseqno.ToString();
        //        outputdata.EventGroups[0].Events[1].Items[2].Value = jobseqno.ToString();
        //        outputdata.EventGroups[0].Events[1].Items[3].Value = lastGlassFlag.ToString();
        //        outputdata.TrackKey = trackKey;
        //        SendPLCData(outputdata);

        //        if (_timerManager.IsAliveTimer(eqpNo + "_" + JobEditTimeout))
        //        {
        //            _timerManager.TerminateTimer(eqpNo + "_" + JobEditTimeout);
        //        }

        //        if (value.Equals(eBitResult.ON))
        //        {
        //            _timerManager.CreateTimer(eqpNo + "_" + JobEditTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(JobDataEditReportReplyTimeout), trackKey);
        //        }
        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,Job Data Edit Reply Set Bit [{2}].",
        //            eqpNo, trackKey, value.ToString()));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}


        //20170626 by huangjiayin  PDR Line Pre-Fetch Special Delay Time
        private void PDRPrefetchTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
               

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS ->RCS][{1}] PDRPrefetchTimer Terminated, Can do pre_fetch command", "CCPDR10B", trackKey));

                if (_timerManager.IsAliveTimer("PDRPrefetchTimeout"))
                {
                    _timerManager.TerminateTimer("PDRPrefetchTimeout");
                }


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

        #region Q time 管理

        public bool QtimeEventJudge(string trxid, string lineID, string cstseqno, string jobseqno, string nodeID, string unitID, string startorEndEvent, string startRecipeID)
        {
            try
            {
                if ((cstseqno == "0") || (jobseqno == "0"))
                {
                    string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] NOT IN WIP!!", cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return false; //無符合
                }
                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                if (job == null)
                {
                    string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] NOT IN WIP!!", cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return false; //無符合
                }


                if (job.QtimeList == null)
                    job.QtimeList = new List<Qtimec>();
                //可能未經ValidateCassetteReply DownLod，但DB可能有設定 還是可以Reload.
                //如果已經ValidateCassetteReply DownLod，則已經Load過DB的設定了
                //避免Abnormal 流片或是測試，一律Reload DB的設定
                if (job.QtimeList.Count <= 0)
                    ObjectManager.QtimeManager.ValidateDBQTime(job);

                //Step 1 :Start Q time is match?
                foreach (Qtimec qtime in job.QtimeList)
                {
                    if (qtime.QTIMEVALUE == "0") //Watson Add 20150520 For CSOT鄒揚要求，設為零不在計算之內
                        continue;
                    if (!qtime.ENABLED)
                        continue;
                    if (qtime.LINENAME != lineID)
                        continue;
                    if (qtime.OVERQTIMEFLAG) //過時不用再判斷
                        continue;
                    if (qtime.STARTQTIMEFLAG) //開始計時的不用再判斷
                        continue;
                    if (qtime.STARTMACHINE == nodeID)
                    {
                        if ((qtime.RECIPEID.Trim() == string.Empty) || (qtime.RECIPEID == startRecipeID))
                        {
                            if (qtime.STARTEVENT == startorEndEvent)
                            {
                                if ((qtime.STARTUNITS.Trim() == string.Empty) || (qtime.STARTUNITS.Trim() == "0"))
                                {
                                    qtime.STARTQTIMEFLAG = true;
                                    qtime.STARTQTIME = DateTime.Now;
                                    lock (job)
                                    {
                                        ObjectManager.JobManager.EnqueueSave(job);
                                    }
                                    string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] GLASSID=[{2}] Qtime Start!!", cstseqno, jobseqno, job.GlassChipMaskBlockID);
                                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                }
                                else
                                {
                                    if (qtime.STARTUNITS == unitID)
                                    {
                                        qtime.STARTQTIMEFLAG = true;
                                        qtime.STARTQTIME = DateTime.Now;
                                        lock (job)
                                        {
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }
                                        string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] GLASSID=[{2}] Qtime Start!!", cstseqno, jobseqno, job.GlassChipMaskBlockID);
                                        Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] GLASSID=[{2}] Qtime Recipe Mismatch!! EQP Current Recipe=[{3}],Q time Setting Recipe=[{4}]!!", cstseqno, jobseqno, job.GlassChipMaskBlockID, startRecipeID.Trim(), qtime.RECIPEID.Trim());
                            Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                        }
                    }
                }


                //玻璃符合MES or DB Setting或者早已計時中，那麼玻璃的時間到了嗎？
                if (QtimeEventTimeOut(lineID, cstseqno, jobseqno, nodeID, unitID, startorEndEvent))
                {
                    //CELL Special CCPIL or ODF InLineOverQtime
                    CELLPIInLineQtimeOver(trxid, lineID, cstseqno, jobseqno, nodeID);
                    CELLODFInLineQtimeOver(trxid, lineID, cstseqno, jobseqno, nodeID);
                    return true; //符合且時間到
                }
                else
                {
                    if (ObjectManager.LineManager.GetLine(lineID).Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS ||
                        ObjectManager.LineManager.GetLine(lineID).Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB)
                    {
                        CFRWQtimeEventTimeOut(lineID, cstseqno, jobseqno, nodeID, unitID, startorEndEvent);
                        CFQtimeEventTimeOut(lineID, cstseqno, jobseqno, nodeID, unitID, startorEndEvent);//add by hujunpeng 20190130
                    }
                }
                return false; //無符合
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
        }

        private bool QtimeEventTimeOut(string lineID, string cstseqno, string jobseqno, string nodeID, string unitID, string CurrentEvent)
        {
            try
            {
                string jobno = string.Format("{0}_{1}", cstseqno, jobseqno);
                bool OverFlag = false;
                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                Line line = ObjectManager.LineManager.GetLine(lineID); // add by  bruce 20160406
                if (line == null) throw new Exception(string.Format("Can't find line ID[{0}] in EquipmentEntity!", lineID));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(nodeID);
                

                if (job == null)
                {
                    string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return false; //無符合
                }

                //Step 1 :MES Q Time is match?
                foreach (Qtimec qtime in job.QtimeList)
                {
                    if (qtime.QTIMEVALUE == "0") //Watson Add 20150520 For CSOT鄒揚要求，設為零不在計算之內
                        continue;
                    if (!qtime.ENABLED)
                        continue;
                    if (qtime.LINENAME != lineID)
                        continue;
                    if (qtime.OVERQTIMEFLAG) //過時不用再判斷
                    {
                        OverFlag = true;
                        continue;
                    }
                    if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
                        continue;
                    if (qtime.ENDMACHINE == nodeID)
                    {
                        if (qtime.ENDEVENT == CurrentEvent)
                        {
                            if ((qtime.ENDUNITS.Trim() == string.Empty) || ((qtime.ENDUNITS.Trim() == "0")))
                            {
                                TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                int setvalue = 0;
                                if (!int.TryParse(qtime.QTIMEVALUE, out setvalue))
                                {
                                    string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}]  GlassID =[{3}],  Q Time IS NOT NUMBER Setting[{4}] Error!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.QTIMEVALUE);
                                    Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    continue;
                                }

                                if (qtimesec.TotalSeconds < setvalue)
                                    continue;
                                qtime.ENDQTIME = DateTime.Now;
                                qtime.OVERQTIMEFLAG = true;
                                OverFlag = true;
                                lock (job)
                                {
                                    ObjectManager.JobManager.EnqueueSave(job);
                                }
                                //string info = string.Format("Cassette seq No[{0}] Job Seq No[{1}] Q time setting [{2}], Start Time [{3}]", cstseqno, jobseqno,qtime.QTIMEVALUE,qtime.STARTQTIME);
                                //string info = string.Format("CASSETTE_SEQ_NO=[{7}] JOB_SEQ_NO=[{8}] GLASSID =[{9}], QTIME VALUE=[{0}] sec,START EQUIPMENT ID=[{1}] =[{2})] =[{3}]-> END=[{4}] =[{5}] =[{6}]" ,qtime.QTIMEVALUE,qtime.STARTMACHINE,qtime.STARTEVENT,qtime.STARTQTIME,qtime.ENDMACHINE,qtime.ENDEVENT,qtime.ENDQTIME,job.CassetteSequenceNo,job.JobSequenceNo,job.GlassChipMaskBlockID);


                                string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                info += string.Format("QTIME VALUE=[{0}] sec, START EQUIPMENT_ID=[{1}] , EVENT=[{2}] , START_TIME=[{3}]", qtime.QTIMEVALUE, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                info += string.Format("-> END EQUIPMENT_ID=[{0}] , EVENT=[{1}] , END_TIME=[{2}] ", qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME);

                                if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) // add by  bruce 20160406 for T2 當Qtime Timeout時, 只有Array需要去Hold, CF及CELL不用去Hold
                                {
                                    #region Add Job HoldReason
                                    HoldInfo hold = new HoldInfo()
                                    {
                                        NodeNo = eqp.Data.NODENO,
                                        NodeID = qtime.ENDMACHINE,
                                        UnitNo = "",
                                        UnitID = qtime.ENDUNITS,
                                        HoldReason = string.Format("QTIME VALUE=[{0}],START =[{1}] =[{2}] =[{3}]-> END=[{4}] =[{5}] =[{6}]", qtime.QTIMEVALUE, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME, qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME),
                                        OperatorID = ""
                                    };
                                    #endregion
                                    ObjectManager.JobManager.HoldEventRecord(job, hold);
                                }
                                Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                continue;

                            }
                            else
                            {
                                if (qtime.ENDUNITS == unitID)
                                {
                                    TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                    int setvalue = 0;
                                    if (!int.TryParse(qtime.QTIMEVALUE, out setvalue))
                                    {
                                        string err = string.Format("Cassette seq No[{0}] Job Seq No[{1}] Q Time type falied Setting[{2}] Error!!", cstseqno, jobseqno, qtime.QTIMEVALUE);
                                        Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                        continue;
                                    }

                                    if (qtimesec.TotalSeconds < setvalue)
                                        continue;
                                    qtime.ENDQTIME = DateTime.Now;
                                    qtime.OVERQTIMEFLAG = true;
                                    OverFlag = true;
                                    lock (job)
                                    {
                                        ObjectManager.JobManager.EnqueueSave(job);
                                    }
                                    //string info = string.Format("Cassette seq No[{0}] Job Seq No[{1}] Q time setting [{2}], Start Time [{3}]", cstseqno, jobseqno, qtime.QTIMEVALUE, qtime.STARTQTIME);
                                    //string info = string.Format("CASSETTE_SEQ_NO=[{7}] JOB_SEQ_NO=[{8}] GLASSID =[{9}], QTIME VALUE=[{0}],START =[{1}] =[{2}] =[{3}]-> END=[{4}] =[{5}] =[{6}]", qtime.QTIMEVALUE, qtime.STARTUNITS, qtime.STARTEVENT, qtime.STARTQTIME, qtime.ENDUNITS, qtime.ENDEVENT, qtime.ENDQTIME, job.CassetteSequenceNo, job.JobSequenceNo,job.GlassChipMaskBlockID);
                                    string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                    info += string.Format("QTIME VALUE=[{0}] sec, START  EQUIPMENT_ID=[{1}] ,EVENT=[{2}] , START_TIME=[{3}]", qtime.QTIMEVALUE, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                    info += string.Format("-> END EQUIPMENT_ID=[{0}] AND UNIT_ID=[{1}],  EVENT=[{2}] , END_TIME=[{3}] ", qtime.ENDMACHINE, qtime.ENDUNITS, qtime.ENDEVENT, qtime.ENDQTIME);

                                    if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) // add by  bruce 20160406 for T2 當Qtime Timeout時, 只有Array需要去Hold, CF及CELL不用去Hold
                                    {
                                        #region Add Job HoldReason
                                        HoldInfo hold = new HoldInfo()
                                        {
                                            NodeNo = eqp.Data.NODENO,
                                            NodeID = qtime.ENDMACHINE,
                                            UnitNo = "",
                                            UnitID = qtime.ENDUNITS,
                                            HoldReason = string.Format("QTIME VALUE=[{0}],START =[{1}) =[{2}) =[{3})-> END=[{4}) =[{5}) =[{6}]", qtime.QTIMEVALUE, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME, qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME),
                                            OperatorID = ""
                                        };
                                        #endregion
                                    }
                                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (OverFlag)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private bool CFRWQtimeEventTimeOut(string lineID, string cstseqno, string jobseqno, string nodeID, string unitID, string CurrentEvent)
        {
            try
            {
                string jobno = string.Format("{0}_{1}", cstseqno, jobseqno);

                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(nodeID);

                if (job == null)
                {
                    string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return false; //無符合
                }


                //Step 1 :MES Q Time is match?
                foreach (Qtimec qtime in job.QtimeList)
                {
                    if (qtime.CFRWQTIME == "0")  //Watson Add 20150520 For CSOT鄒揚要求，設為零不在計算之內
                        continue;
                    if (!qtime.ENABLED)
                        continue;
                    if (qtime.LINENAME != lineID)
                        continue;
                    if (qtime.OVERQTIMEFLAG) //過時不用再判斷
                        continue;
                    if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
                        continue;
                    if (qtime.ENDMACHINE == nodeID)
                    {
                        if (qtime.ENDEVENT == CurrentEvent)
                        {
                            if ((qtime.ENDUNITS.Trim() == string.Empty) || (qtime.ENDUNITS.Trim() == "0"))
                            {
                                TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                int setvalue = 0;
                                if (!int.TryParse(qtime.CFRWQTIME, out setvalue))
                                {
                                    string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q TIME type falied Setting[{2}] Error!!", cstseqno, jobseqno, qtime.QTIMEVALUE);
                                    Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    return false;
                                }

                                if (qtimesec.TotalSeconds < setvalue)
                                    return false;

                                qtime.CFRWQTIMEFLAG = true;
                                lock (job)
                                {
                                    ObjectManager.JobManager.EnqueueSave(job);
                                }
                                //string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q time SETTING [{2}], START TIME [{3}]", cstseqno, jobseqno, qtime.QTIMEVALUE, qtime.STARTQTIME);
                                string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                info += string.Format("CFRWQTIME VALUE=[{0}] SEC, START EQUIPMENT_ID=[{1}] , EVENT=[{2}] , START_TIME=[{3}]", qtime.CFRWQTIME, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                info += string.Format("-> END EQUIPMENT_ID=[{0}] , EVENT=[{1}] , END_TIME=[{2}] ", qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME);

                                Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                return true;
                            }
                            else
                            {
                                if (qtime.ENDUNITS == unitID)
                                {
                                    TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                    int setvalue = 0;
                                    if (!int.TryParse(qtime.CFRWQTIME, out setvalue))
                                    {
                                        string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q Time type falied Setting[{2}] Error!!", cstseqno, jobseqno, qtime.QTIMEVALUE);
                                        Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                        return false;
                                    }

                                    if (qtimesec.TotalSeconds < setvalue)
                                        return false;

                                    qtime.CFRWQTIMEFLAG = true;
                                    lock (job)
                                    {
                                        ObjectManager.JobManager.EnqueueSave(job);
                                    }
                                    //string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q TIME SETTING [{2}], START TIME [{3}]", cstseqno, jobseqno, qtime.QTIMEVALUE, qtime.STARTQTIME);
                                    string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                    info += string.Format("CFRWQTIME VALUE=[{0}] SEC, START EQUIPMENT_ID=[{1}] EVENT=[{2}] , START_TIME=[{3}]", qtime.CFRWQTIME, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                    info += string.Format("-> END EQUIPMENT_ID=[{0}] AND UNIT_ID=[{1}] , EVENT=[{2}] , END_TIME=[{3}] ", qtime.ENDMACHINE, qtime.ENDUNITS, qtime.ENDEVENT, qtime.ENDQTIME);

                                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private bool CFQtimeEventTimeOut(string lineID, string cstseqno, string jobseqno, string nodeID, string unitID, string CurrentEvent)//add by hujunpeng 20190130
        {
            try
            {
                string jobno = string.Format("{0}_{1}", cstseqno, jobseqno);

                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                Line line = ObjectManager.LineManager.GetLine(lineID); // add by  bruce 20160406
                if (line == null) throw new Exception(string.Format("Can't find line ID[{0}] in EquipmentEntity!", lineID));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(nodeID);

                if (job == null)
                {
                    string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return false; //無符合
                }


                //Step 1 :MES Q Time is match?
                foreach (Qtimec qtime in job.QtimeList)
                {
                    int remark = 0;
                    if (!int.TryParse(qtime.REMARK, out remark) || string.IsNullOrEmpty(qtime.REMARK))//不能转化为整数或者为空不在計算之內
                        continue;
                    if (remark == 0)
                        continue;
                    if (!qtime.ENABLED)
                        continue;
                    if (qtime.LINENAME != lineID)
                        continue;
                    if (qtime.CFQTIMEFLAG)//過時不用再判斷
                        continue;
                    if (qtime.OVERQTIMEFLAG) //過時不用再判斷
                        continue;
                    if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
                        continue;
                    if (qtime.ENDMACHINE == nodeID)
                    {
                        if (qtime.ENDEVENT == CurrentEvent)
                        {
                            if ((qtime.ENDUNITS.Trim() == string.Empty) || (qtime.ENDUNITS.Trim() == "0"))
                            {
                                TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                int setvalue = 0;
                                if (!int.TryParse(qtime.REMARK, out setvalue))
                                {
                                    string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF REMARK Q TIME type falied Setting[{2}] Error!!", cstseqno, jobseqno, qtime.REMARK);
                                    Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    return false;
                                }

                                if (qtimesec.TotalSeconds < setvalue)
                                    return false;

                                qtime.CFQTIMEFLAG = true;
                                lock (job)
                                {
                                    ObjectManager.JobManager.EnqueueSave(job);
                                }
                                //string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q time SETTING [{2}], START TIME [{3}]", cstseqno, jobseqno, qtime.QTIMEVALUE, qtime.STARTQTIME);
                                string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                info += string.Format("CFREMARKQTIME VALUE=[{0}] SEC, START EQUIPMENT_ID=[{1}] , EVENT=[{2}] , START_TIME=[{3}]", qtime.REMARK, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                info += string.Format("-> END EQUIPMENT_ID=[{0}] , EVENT=[{1}] , END_TIME=[{2}] ", qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME);
                                Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                if (line.Data.FABTYPE == eFabType.CF.ToString()) // add by  hujunpeng 20190130 for T3 當Qtime.REMARK Timeout時, CF需要Hold
                                {
                                    #region Add Job HoldReason
                                    HoldInfo hold = new HoldInfo()
                                    {
                                        NodeNo = eqp.Data.NODENO,
                                        NodeID = qtime.ENDMACHINE,
                                        UnitNo = "",
                                        UnitID = "",
                                        HoldReason = string.Format("QTIME VALUE=[{0}],START =[{1}) =[{2}) =[{3})-> END=[{4}) =[{5}) =[{6}]", qtime.REMARK, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME, qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME),
                                        OperatorID = "BCAUTO"
                                    };
                                    #endregion
                                }
                                return true;
                            }
                            else
                            {
                                if (qtime.ENDUNITS == unitID)
                                {
                                    TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
                                    int setvalue = 0;
                                    if (!int.TryParse(qtime.REMARK, out setvalue))
                                    {
                                        string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF REMARK Q Time type falied Setting[{2}] Error!!", cstseqno, jobseqno, qtime.REMARK);
                                        Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                        return false;
                                    }

                                    if (qtimesec.TotalSeconds < setvalue)
                                        return false;

                                    qtime.CFQTIMEFLAG = true;
                                    lock (job)
                                    {
                                        ObjectManager.JobManager.EnqueueSave(job);
                                    }
                                    //string info = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}] CF RW Q TIME SETTING [{2}], START TIME [{3}]", cstseqno, jobseqno, qtime.QTIMEVALUE, qtime.STARTQTIME);
                                    string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID =[{3}],", eqp.Data.NODENO, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID);
                                    info += string.Format("CFREMARKQTIME VALUE=[{0}] SEC, START EQUIPMENT_ID=[{1}] EVENT=[{2}] , START_TIME=[{3}]", qtime.REMARK, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME);
                                    info += string.Format("-> END EQUIPMENT_ID=[{0}] AND UNIT_ID=[{1}] , EVENT=[{2}] , END_TIME=[{3}] ", qtime.ENDMACHINE, qtime.ENDUNITS, qtime.ENDEVENT, qtime.ENDQTIME);
                                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                                    if (line.Data.FABTYPE == eFabType.CF.ToString()) // add by  hujunpeng 20190130 for T3 當Qtime.REMARK Timeout時, CF需要Hold
                                    {
                                        #region Add Job HoldReason
                                        HoldInfo hold = new HoldInfo()
                                        {
                                            NodeNo = eqp.Data.NODENO,
                                            NodeID = qtime.ENDMACHINE,
                                            UnitNo = "",
                                            UnitID = qtime.ENDUNITS,
                                            HoldReason = string.Format("QTIME VALUE=[{0}],START =[{1}) =[{2}) =[{3})-> END=[{4}) =[{5}) =[{6}]", qtime.REMARK, qtime.STARTMACHINE, qtime.STARTEVENT, qtime.STARTQTIME, qtime.ENDMACHINE, qtime.ENDEVENT, qtime.ENDQTIME),
                                            OperatorID = "BCAUTO"
                                        };
                                        #endregion
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        //是否符合CELL INLINEQTIMEOVERCMD 下給機台的條件？
        #region [T2 USE]
        //private void CELLPIInLineQtimeOver(string trxid, string lineID, string cstseqno, string jobseqno, string endNodeID)
        //{
        //    try
        //    {
        //        eCELLInLineOverQtimePI piovertime = new eCELLInLineOverQtimePI();

        //        Line line = ObjectManager.LineManager.GetLine(lineID);
        //        if (line == null) throw new Exception(string.Format("Can't find line ID[{0}] in EquipmentEntity!", lineID));

        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(endNodeID);
        //        if (eqp == null) throw new Exception(string.Format("Can't find Equipment ID[{0}] in EquipmentEntity!", endNodeID));

        //        Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

        //        if (job == null)
        //        {
        //            string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
        //            Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
        //            return; //無符合
        //        }

        //        if (line.Data.LINETYPE != eLineType.CELL.CBPIL)
        //            return;
        //        foreach (Qtimec qtime in job.QtimeList)
        //        {
        //            if (!qtime.ENABLED)
        //                continue;
        //            if (qtime.LINENAME != lineID)
        //                continue;
        //            if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
        //                continue;
        //            if (qtime.ENDMACHINE.Contains(keyCELLMachingName.CBPPO) || qtime.ENDMACHINE.Contains(keyCELLMachingName.CBPMO))
        //            {
        //                if (qtime.OVERQTIMEFLAG)
        //                {
        //                    if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CBPIP))
        //                        piovertime.PIP2PPO = true;
        //                    if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CBPPO))
        //                        piovertime.PPO2PMO = true;
        //                }
        //            }
        //        }

        //        object[] _data = new object[6]
        //        { 
        //            eqp.Data.NODENO,
        //            eBitResult.ON,
        //            trxid,  /*0 TrackKey*/
        //            cstseqno,
        //            jobseqno,
        //            piovertime
        //        };
        //        if ((piovertime.PIP2PPO) || (piovertime.PPO2PMO))
        //            Invoke(eServiceName.CELLSpecialService, "InLineQTimeOverCommand_PI", _data);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Print(ex.Message);
        //    }
        //}
        #endregion

        private void CELLPIInLineQtimeOver(string trxid, string lineID, string cstseqno, string jobseqno, string endNodeID)
        {
            try
            {
                eCELLInLineOverQtimePI piovertime = new eCELLInLineOverQtimePI();

                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line == null) throw new Exception(string.Format("Can't find line ID[{0}] in EquipmentEntity!", lineID));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(endNodeID);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment ID[{0}] in EquipmentEntity!", endNodeID));

                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                if (job == null)
                {
                    string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                    return; //無符合
                }

                if (!line.Data.LINENAME.Contains("CCPIL"))
                    return;
                foreach (Qtimec qtime in job.QtimeList)
                {
                    if (!qtime.ENABLED)
                        continue;
                    if (qtime.LINENAME != lineID)
                        continue;
                    if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
                        continue;
                    if (qtime.ENDMACHINE.Contains(keyCELLMachingName.CCPMO) || qtime.ENDMACHINE.Contains(keyCELLMachingName.CCPPO)
                        || qtime.ENDMACHINE.Contains(keyCELLMachingName.CCPPA) || qtime.ENDMACHINE.Contains(keyCELLMachingName.CCPAO))
                    {
                        if (qtime.OVERQTIMEFLAG)
                        {
                            if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCPPO))
                                piovertime.PPO2PMO = true;
                            if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCPIC))
                                piovertime.PIC2PPO = true;
                            if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCPPO))
                                piovertime.PPO2PPA = true;
                            if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCPMO))
                                piovertime.PMO2PPA = true;
                            if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCPAO))
                                piovertime.PPA2PAO = true;
                            if ((piovertime.PPO2PMO) || (piovertime.PIC2PPO) || (piovertime.PPO2PPA) || (piovertime.PMO2PPA) || (piovertime.PPA2PAO))
                                Invoke(eServiceName.MESService, "QtimeOverReport", new object[10] { trxid, lineID, job.GlassChipMaskBlockID, 
                                          qtime.QTIMEID, qtime.STARTMACHINE, "", qtime.STARTQTIME.ToString("yyyyMMddHHmmss"), qtime.ENDMACHINE, "", qtime.ENDQTIME.ToString("yyyyMMddHHmmss") });
                        }
                    }
                }
                object[] _data = new object[6]
                { 
                    eqp.Data.NODENO,
                    eBitResult.ON,
                    trxid,  /*0 TrackKey*/
                    cstseqno,
                    jobseqno,
                    piovertime
                };
                if ((piovertime.PPO2PMO) || (piovertime.PIC2PPO) || (piovertime.PPO2PPA) || (piovertime.PMO2PPA) || (piovertime.PPA2PAO))
                    Invoke(eServiceName.CELLSpecialService, "InLineQTimeOverCommand_PI", _data);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void CELLODFInLineQtimeOver(string trxid, string lineID, string cstseqno, string jobseqno, string endNodeID)
        {
              try
              {
                    eCELLInLineOverQtimeODF odfovertime = new eCELLInLineOverQtimeODF();

                    Line line = ObjectManager.LineManager.GetLine(lineID);
                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE ID[{0}] IN EQUIPMENTENTITY!", lineID));

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(endNodeID);
                    if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT ID[{0}] IN EQUIPMENTENTITY!", endNodeID));

                    Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                    if (job == null)
                    {
                          string info = string.Format("EQUIPMENT[{0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] NOT IN WIP!!", eqp.Data.NODENO, cstseqno, jobseqno);
                          Log.NLogManager.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                          return; //無符合
                    }


                    if (!line.Data.LINEID.Contains(keyCellLineType.ODF))//sy add 20160907
                          return;
                    foreach (Qtimec qtime in job.QtimeList)
                    {
                          if (!qtime.ENABLED)
                                continue;
                          if (qtime.LINENAME != lineID)
                                continue;
                          if (!qtime.STARTQTIMEFLAG) //開始的才需要判斷
                                continue;
                          if (qtime.ENDMACHINE == endNodeID)
                          {
                              if (qtime.OVERQTIMEFLAG)
                              {
                                  if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCBOO))
                                      odfovertime.BOO2VAC = true;
                                  if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCVPO))
                                      odfovertime.VPO2VAC = true;
                                  if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCLCD))
                                      odfovertime.LCD2VAC = true;
                                  if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCVAC))
                                      odfovertime.VAC2SUV = true;
                                  if (qtime.STARTMACHINE.Contains(keyCELLMachingName.CCSUV))
                                      odfovertime.SUV2SMO = true;
                                  if ((odfovertime.BOO2VAC) || (odfovertime.VPO2VAC) || (odfovertime.LCD2VAC) || (odfovertime.VAC2SUV) || (odfovertime.SUV2SMO))
                                      Invoke(eServiceName.MESService, "QtimeOverReport", new object[10] { trxid, lineID, job.GlassChipMaskBlockID, 
                                          qtime.QTIMEID, qtime.STARTMACHINE, "", qtime.STARTQTIME.ToString("yyyyMMddHHmmss"), qtime.ENDMACHINE, "", qtime.ENDQTIME.ToString("yyyyMMddHHmmss") });
                              }
                          }
                          else
                              return;
                    }

                    object[] _data = new object[6]
                { 
                    eqp.Data.NODENO,
                    eBitResult.ON,
                    trxid,  /*0 TrackKey*/
                    cstseqno,
                    jobseqno,
                    odfovertime
                };
                    if ((odfovertime.BOO2VAC) || (odfovertime.VPO2VAC) || (odfovertime.LCD2VAC) || (odfovertime.VAC2SUV) || (odfovertime.SUV2SMO))
                          Invoke(eServiceName.CELLSpecialService, "InLineQTimeOverCommand_ODF", _data);
                    
              }
              catch (Exception ex)
              {
                    Debug.Print(ex.Message);
              }
        }

        #endregion

        #region Find Last glass of Cassette
        private bool GetLastGlass(Job reMoveCovejob, eJobCommand jobcmd, out Job theNewLastJob)
        {
            try
            {
                theNewLastJob = null;
                Dictionary<int, Job> lastSecondSlotdic = new Dictionary<int, Job>();

                if (reMoveCovejob.LastGlassFlag != "1")
                    return false;

                //Step.1: Save All Sampling Slot Glass
                foreach (Job job in ObjectManager.JobManager.GetJobs(reMoveCovejob.CassetteSequenceNo))
                {
                    int slotno = 0;
                    if (job.RemoveFlag) //移走不用再加入考慮
                        continue;
                    if (job.SamplingSlotFlag != "1")  //沒有抽的片 不需要考慮
                        continue;
                    if (!int.TryParse(job.FromSlotNo, out slotno))//找不到slotno
                        continue;
                    if (job.GlassChipMaskBlockID == reMoveCovejob.GlassChipMaskBlockID)
                        continue;

                    if (!lastSecondSlotdic.ContainsKey(slotno))
                        lastSecondSlotdic.Add(slotno, job);
                }

                //Step.2: Find out the New Last Glass
                if (jobcmd == eJobCommand.JOBREMOVE)
                {
                    theNewLastJob = (from d in lastSecondSlotdic orderby d.Key ascending select d.Value).Last();
                    return true;
                }
                else
                {
                    int removSlotno, lastSlotno;
                    if (int.TryParse(reMoveCovejob.FromSlotNo, out removSlotno))//找不到slotno
                    {
                        theNewLastJob = lastSecondSlotdic.Values.FirstOrDefault(w => w.LastGlassFlag == "1"); //現存在CST的最後一片
                        if (theNewLastJob == null)
                            return false;
                        if (int.TryParse(theNewLastJob.FromSlotNo, out lastSlotno))//找不到slotno
                        {
                            if (removSlotno > lastSlotno) //蓋回的是最大的
                            {
                                //Cancel Last Glass 
                                theNewLastJob = reMoveCovejob;
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                theNewLastJob = null;
                return false;
            }
        }
        #endregion

        private void RecipeGroupEndCheck(Line line, Job removeJob, string trackKey)
        {
            try
            {
                //if (line.Data.JOBDATALINETYPE != eJobDataLineType.ARRAY.PROCESS_EQ) return;

                IDictionary<string, string> eqpflag = ObjectManager.SubJobDataManager.Decode(removeJob.EQPFlag, eJOBDATA.EQPFlag);
                if (!eqpflag.ContainsKey(eEQPFLAG.Array.RecipeGroupEndFlag)) return;

                if (eqpflag[eEQPFLAG.Array.RecipeGroupEndFlag] != "1") return;

                List<Job> jobs = (from j in ObjectManager.JobManager.GetJobs()
                                  where j.CassetteSequenceNo == removeJob.CassetteSequenceNo &&
                                      j.JobSequenceNo != removeJob.JobSequenceNo &&
                                      j.RemoveFlag == false && j.SamplingSlotFlag.Equals("1") &&
                                      j.ArraySpecial.RecipeGroupNumber == removeJob.ArraySpecial.RecipeGroupNumber &&
                                      j.CreateTime != j.JobProcessStartTime
                                  orderby j.JobProcessStartTime descending
                                  select j).ToList<Job>();

                if (jobs.Count() == 0) return;

                Invoke(eServiceName.ArraySpecialService, "SetRecipeGroupEndCommand", new object[] { jobs.First(), trackKey });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        public Job NewJob(string lineID, string casseqno, string jobseqno, Trx inputData)
        {
            try
            {

                int c, j;
                if (!int.TryParse(casseqno, out c)) throw new Exception(string.Format("Create Job Failed Cassette Sequence No isn't number!!", casseqno));

                if (c == 0) throw new Exception(string.Format("Create Job Failed Cassette Sequence No (0)!!", casseqno));

                if (!int.TryParse(jobseqno, out j)) throw new Exception(string.Format("Create Job Failed Job Sequence No isn't number!!", jobseqno));

                if (j == 0) throw new Exception(string.Format("Create Job Failed Job Sequence No (0)!!", jobseqno));

                //#region Robot Control WIP Route (Watson Add 20151105)
                ////如果Robot Control 是我們負責的，就不能繼續傳片下去，可能發生該存放在哪一個Slot？或怎麼產生Robot Step？
                ////任意複製或產生新帳料，可能會佔住原正常玻璃 Slot位置或是玻璃在Robot上不知何去何從等打結的問題。
                ////如果Robot Control不是我們負責，可以繼續傳片下去，以避免玻璃停在機台內的情形。
                //if (ObjectManager.RobotManager.GetRobots().Count > 0)//sy20160517 modify only write log
                //{
                //    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Robot Control Route is Null!! The Job Cassette Seq No[{0}] Job Seq No[{1}]  is not IN BC WIP. Don't Continute Transfer.", casseqno, jobseqno));
                ////    throw new Exception(string.Format("Robot Control Route is Null!! The Job Cassette Seq No[{0}] Job Seq No[{1}]  is not IN BC WIP. Don't Continute Transfer.", casseqno, jobseqno));
                //}
                //#endregion

                Job job = new Job(c, j);

                //SECS 不會有值
                if (inputData == null)
                {
                    ObjectManager.JobManager.NewJobCreateMESDataEmpty(job);
                    ObjectManager.JobManager.AddJob(job);
                    return job;
                }

                Line line = ObjectManager.LineManager.GetLine(lineID);

                if (line == null) throw new Exception(string.Format("Create Job Failed Line[{0}] IS NOT IN LINEENTITY!!", lineID));


                //Watson Add 20150416 此值不會跟著job event作update，不會隨機台上報而改變，
                //但是NEW的JOB沒有值，需要相信機台上報的
                Event eVent = inputData.EventGroups[0].Events[0];
                //Jun Add 20150429 New Job 需要更新Glass ID by CSOT


                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                job.CurrentEQPNo = eqp.Data.NODENO;

                job.GlassChipMaskBlockID = eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim();
                job.ChipCount = int.Parse(eVent.Items[eJOBDATA.ChipCount].Value);

                string oxrInfomation = string.Empty;
                eFabType fabtype;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabtype);

                switch (fabtype)
                {
                    case eFabType.ARRAY:
                        //modify by bruce 2015/7/7 AC 廠 目前Job Data 沒有開 OXR Information 
                        //#region Array 
                        //if (eVent.Items[eJOBDATA.OXRInformation].Value.Trim() != string.Empty)
                        //{

                        //    int count = 0;
                        //    for (int i = 0; i < job.ChipCount; i++)
                        //    {
                        //        if (i >= 56) break;
                        //        count++;
                        //        oxrInfomation += ConstantManager["PLC_OXRINFO2_AC"][eVent.Items["OXRInformation"].Value.Substring(i * 4, 4)].Value;
                        //    }
                        //    job.OXRInformation = oxrInfomation;
                        //}
                        //#endregion
                        break;
                    case eFabType.CF:

                        //modify by bruce 2015/7/7 AC 廠 目前Job Data 沒有開 OXR Information
                        //#region CF
                        //if (eVent.Items["OXRInformation"].Value.Length >= job.ChipCount * 4 && job.ChipCount != 0)
                        //{
                        //    for (int i = 0; i < job.ChipCount; i++)
                        //    {
                        //        oxrInfomation += ConstantManager["PLC_OXRINFO2_AC"][eVent.Items["OXRInformation"].Value.Substring(i * 4, 4)].Value;
                        //    }
                        //    job.OXRInformation = oxrInfomation;
                        //}
                        //else if (eVent.Items["OXRInformation"].Value.Length < job.ChipCount * 4)
                        //{
                        //    this.Logger.LogDebugWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("OXRInformation total length [{0}] is not enough.",
                        //        eVent.Items["OXRInformation"].Value.Length));
                        //}
                        //#endregion
                        break;
                    case eFabType.CELL:
                        #region CELL
                        //if (eVent.Items["OXRInformation"].Value.Length == job.ChipCount * 2 && job.ChipCount != 0)
                        //{
                        //    int chipcount = job.ChipCount > 56 ? 56 : job.ChipCount;
                        //    for (int i = 0; i < chipcount; i++)
                        //    {
                        //        oxrInfomation += ConstantManager["PLC_OXRINFO2_CELL"][eVent.Items["OXRInformation"].Value.Substring(i * 2, 2)].Value.ToString();
                        //    }
                        //    job.OXRInformation = oxrInfomation;
                        //}
                        #endregion
                        break;
                    default:
                        break;
                }
                ObjectManager.JobManager.NewJobCreateMESDataEmpty(job);
                ObjectManager.JobManager.AddJob(job); //與駿龍確認過 相信機台
                return job;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Job History

        //Watson Add 20150310 For CassetteSeqNO =0 or JobSeqNo = 0 錯誤資料存入DB, 但不更新WIP
        private void RecordJobHistoryForErrorData(Trx inputData, string unitNo, string portNo, string slotNo, string eventname, string transactionID)
        {
            try
            {
                // Save DB
                if (unitNo == "0") unitNo = string.Empty;
                if (portNo == "00") portNo = string.Empty;

                Event eVent = inputData.EventGroups[0].Events[0];

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Job errjob = new Job(); //是錯誤資料要存入JOB HISTORY ,所以新增一個JOB，但不存檔也不會是WIP，存完即丟
                errjob.CassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                errjob.JobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                errjob.GroupIndex = eVent.Items[eJOBDATA.GroupIndex].Value;
                //errjob.ProductType = // 無法直接捉取值eVent.Items["ProductType"].Value;
                errjob.CSTOperationMode = (eCSTOperationMode)int.Parse(eVent.Items[eJOBDATA.CSTOperationMode].Value);
                errjob.SubstrateType = (eSubstrateType)int.Parse(eVent.Items[eJOBDATA.SubstrateType].Value);
                errjob.CIMMode = (eBitResult)int.Parse(eVent.Items[eJOBDATA.CIMMode].Value);
                errjob.JobType = (eJobType)int.Parse(eVent.Items[eJOBDATA.JobType].Value);
                errjob.JobJudge = eVent.Items[eJOBDATA.JobJudge].Value;
                errjob.SamplingSlotFlag = eVent.Items[eJOBDATA.SamplingSlotFlag].Value;
                //errjob.OXRInformationRequestFlag = eVent.Items["OXRInformationRequestFlag"].Value;
                errjob.FirstRunFlag = eVent.Items[eJOBDATA.FirstRunFlag].Value;
                errjob.JobGrade = eVent.Items[eJOBDATA.JobGrade].Value;
                errjob.GlassChipMaskBlockID = eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim();
                errjob.PPID = eVent.Items[eJOBDATA.PPID].Value.ToString();
                //Watson 20150310 Memo 各廠不一，不敢填值，若有字串不符或位置不符，怕會error.
                //=======================================
                errjob.INSPReservations = string.Empty;
                errjob.LastGlassFlag = string.Empty;
                errjob.InspJudgedData = string.Empty;
                errjob.TrackingData = string.Empty;
                errjob.EQPFlag = string.Empty;
                //errjob.OXRInformation = eVent.Items["OXRInformation"].Value;  //modify by bruce 2015/7/14 T3 Job data 已無OXR Info
                errjob.ChipCount = int.Parse(eVent.Items["ChipCount"].Value);
                //========================================
                errjob.FromCstID = string.Empty;
                errjob.ToCstID = string.Empty;


                ObjectManager.JobManager.RecordJobHistory(errjob, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, portNo, slotNo, eventname, transactionID);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //private void RecordJobHistoryForErrorData2(Trx inputData, string unitNo, string portNo, string slotNo, string eventname)
        //{
        //    try
        //    {
        //        // Save DB
        //        if (unitNo == "0") unitNo = string.Empty;
        //        if (portNo == "00") portNo = string.Empty;

        //        Event eVent = inputData.EventGroups[0].Events[0];

        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
        //        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

        //        JOBHISTORY his = new JOBHISTORY()
        //        {
        //            UPDATETIME = DateTime.Now,
        //            EVENTNAME = eventname,
        //            CASSETTESEQNO = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value),
        //            JOBSEQNO = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value),
        //            GROUPINDEX = int.Parse(eVent.Items["GroupIndex"].Value),
        //            PRODUCTTYPE = int.Parse(eVent.Items["ProductType"].Value),
        //            CSTOPERATIONMODE = ((eCSTOperationMode)int.Parse(eVent.Items["CSTOperationMode"].Value)).ToString(),
        //            SUBSTRATETYPE = ((eSubstrateType)int.Parse(eVent.Items["SubstrateType"].Value)).ToString(),
        //            CIMMODE = ((eBitResult)int.Parse(eVent.Items["CIMMode"].Value)).ToString(),
        //            JOBTYPE = ((eJobType)int.Parse(eVent.Items["JobType"].Value)).ToString(),
        //            JOBJUDGE = eVent.Items["JobJudge"].Value.ToString(),
        //            SAMPLINGSLOTFLAG = eVent.Items["SamplingSlotFlag"].Value.ToString(),
        //            OXRINFORMATIONREQUESTFLAG = eVent.Items["OXRInformationRequestFlag"].Value.ToString(),
        //            FIRSTRUNFLAG = eVent.Items["FirstRunFlag"].Value.ToString(),
        //            JOBGRADE = eVent.Items["JobGrade"].Value.ToString(),
        //            JOBID = eVent.Items["Glass/Chip/MaskID/CUTID"].Value.ToString().Trim(),
        //            PPID = eVent.Items["PPID"].Value.ToString(),
        //            //Watson 20150310 Memo 各廠不一，不敢填值，若有字串不符或位置不符，怕會error.
        //            //=======================================
        //            INSPRESERVATIONS = "",
        //            LASTGLASSFLAG = "",
        //            INSPJUDGEDDATA = "",
        //            TRACKINGDATA = "",
        //            EQPFLAG = "",
        //            OXRINFORMATION = eVent.Items["OXRInformation"].Value,
        //            CHIPCOUNT = int.Parse(eVent.Items["ChipCount"].Value),
        //            //========================================
        //            NODENO = eqp.Data.NODENO,
        //            UNITNO = unitNo,
        //            PORTNO = portNo,
        //            SLOTNO = slotNo,
        //            NODEID = eqp.Data.NODEID,
        //            SOURCECASSETTEID = "",
        //            CURRENTCASSETTEID = ""
        //        };
        //        ObjectManager.JobManager.InsertDB(his);

        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //public void RecordJobHistory2(Job job, string nodeId, string nodeNo, string unitNo, string portNo, string slotNo, string eventname, string pathNo = "")
        //{
        //    try
        //    {
        //        // Save DB
        //        if (unitNo == "0") unitNo = string.Empty;
        //        if (portNo == "00") portNo = string.Empty;
        //        int i = 0;
        //        JOBHISTORY his = new JOBHISTORY()
        //        {
        //            UPDATETIME = DateTime.Now,
        //            EVENTNAME = eventname,
        //            CASSETTESEQNO = int.TryParse(job.CassetteSequenceNo, out i) == true ? i : 0,
        //            JOBSEQNO = int.TryParse(job.JobSequenceNo, out i) == true ? i : 0,
        //            JOBID = job.GlassChipMaskBlockID,
        //            GROUPINDEX = int.TryParse(job.GroupIndex, out i) == true ? i : 0,
        //            PRODUCTTYPE = job.ProductType.Value,
        //            CSTOPERATIONMODE = job.CSTOperationMode.ToString(),
        //            SUBSTRATETYPE = job.SubstrateType.ToString(),
        //            CIMMODE = job.CIMMode.ToString(),
        //            JOBTYPE = job.JobType.ToString(),
        //            JOBJUDGE = job.JobJudge.ToString(),
        //            SAMPLINGSLOTFLAG = job.SamplingSlotFlag,
        //            OXRINFORMATIONREQUESTFLAG = job.OXRInformationRequestFlag,
        //            FIRSTRUNFLAG = job.FirstRunFlag,
        //            JOBGRADE = job.JobGrade,
        //            PPID = job.PPID,
        //            INSPRESERVATIONS = job.INSPReservations,
        //            LASTGLASSFLAG = job.LastGlassFlag,
        //            INSPJUDGEDDATA = job.InspJudgedData,
        //            TRACKINGDATA = job.TrackingData,
        //            EQPFLAG = job.EQPFlag,
        //            OXRINFORMATION = job.OXRInformation,
        //            CHIPCOUNT = job.ChipCount,
        //            NODENO = nodeNo,//Watson Modify 20150209 以機台上報的資料為準
        //            UNITNO = unitNo,//Watson Modify 20150209 以機台上報的資料為準
        //            PORTNO = portNo, //Watson Modify 20150209 以機台上報的資料為準
        //            SLOTNO = slotNo, //Watson Modify 20150209 以機台上報的資料為準
        //            NODEID = nodeId, //Watson Modify 20150209 以機台上報的資料為準
        //            SOURCECASSETTEID = job.FromCstID, //
        //            CURRENTCASSETTEID = job.ToCstID,
        //            PATHNO = pathNo
        //        };
        //        ObjectManager.JobManager.InsertDB(his);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        #endregion

        private bool GetJobSeqForSECS(out string seq)
        {
            try
            {
                seq = "0";
                List<Job> jobs = ObjectManager.JobManager.GetJobs() as List<Job>;
                var filtJobs = jobs.Where(j => j.CassetteSequenceNo == FixCassetteSeqForSECS);
                if (filtJobs == null)
                {
                    seq = "1";
                    return true;
                }
                StringBuilder sb = new StringBuilder(new string('0', 65535));
                lock (filtJobs)
                {
                    foreach (var j in filtJobs)
                        sb.Replace("0", "1", int.Parse(j.JobSequenceNo) - 1, 1);

                    if (sb.ToString().IndexOf("0") == -1)
                        return false;
                    else
                        seq = (sb.ToString().IndexOf("0") + 1).ToString();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateSourcePortData(Job job)
        {
            try
            {
                Port souPort = ObjectManager.PortManager.GetPort(job.SourcePortID);
                if (souPort != null)
                {
                    foreach (Sorter s in souPort.File.SorterJobGrade)
                    {
                        if (s.SorterGrade.Equals(job.JobGrade) && s.ProductType.Equals(job.ProductType.Value))
                            if (s.GradeCount > 0)
                            {
                                s.GradeCount--;
                                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                         string.Format("IN SORTER MODE, JOB_GRADE=[{0}] AND JOB_COUNT=[{1}] GLASS IN SOURCE PORT_NO[{2}].",
                                                         job.JobGrade, s.GradeCount, souPort.Data.PORTNO));
                            }
                    }
                }
                
                ObjectManager.PortManager.EnqueueSave(souPort.File);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
