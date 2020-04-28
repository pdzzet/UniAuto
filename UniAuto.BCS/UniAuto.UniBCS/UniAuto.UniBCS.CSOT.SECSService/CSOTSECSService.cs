using System;
using System.Reflection;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MesSpec;
using System.IO;
using UniAuto.UniBCS.MISC;
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class CSOTSECSService : AbstractService
      {
            private Random _rnd;
            private CommonSECSService _common;
            private Thread _serviceThread;
            private bool _isRuning;
            private int _CSOTSECSDATATIMEOUT;
            private DateTime _DailyCheckLastDT = DateTime.Now;
            private DateTime _APCDataIMPLastDT = DateTime.Now;
            private DateTime _APCDataNORLastDT = DateTime.Now;
            private DateTime _SpecialDataLastDT = DateTime.Now;

            private DateTime _APCDataIMPLastDTForID = DateTime.Now;  // wucc add 20150806
            private DateTime _APCDataNORLastDTForID = DateTime.Now;  // wucc add 20150806
            private DateTime _SpecialDataLastDTForID = DateTime.Now; // wucc add 20150806

            //for s7f73 by node,trxid
            //{ {Node,{{trxid,recipecheckinfo},...}},...}
            private ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeCheckInfo>> _RecipeCheckInfos = new ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeCheckInfo>>();
            //for s7f25 by node,trxid
            //{ {Node,{{trxid,recipecheckinfo},...}},...}
            private ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeCheckInfo>> _RecipeParameters = new ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeCheckInfo>>();

            //for s6f3_01 by node,glassid
            //{ {Node,{{glassid,{{timestamp,xmldoc},...}},...},...}		
            private ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>> _GlassProcessDataInformationReports = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>>();

            //for s6f3_02 by node,cstseq
            //{ {Node,{{cstseq,{{timestamp,xmldoc},...}},...},...}		
            private ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>> _LotProcessDataInformationReports = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>>();

            //for s6f3_04 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _APCImportantDataInformationReports = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();
            //for s6f3_05 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _APCNormalDataInformationReports = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();

            //for s6f3_06 by node,glassid
            //{ {Node,{{glassid,{{timestamp,xmldoc},...}},...},...}		
            private ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>> _GlassProcessDataInformationReportfortestEQs = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>>();

            //for s6f3_08 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _SpecialDataInformationReports = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();
            //for s6f3_09 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _APCImportantDataInformationReportforIDs = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();
            //for s6f3_10 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _APCNormalDataInformationReportforIDs = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();
            //for s6f3_11 by node
            //{ {Node,{{timestamp,xmldoc},...}},...}		
            private ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> _SpecialDataInformationReportforIDs = new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>();

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
                  _isRuning = true;
                  _serviceThread = new Thread(new ThreadStart(ThreadProc));
                  _serviceThread.IsBackground = true;
                  _serviceThread.Start();
                  _CSOTSECSDATATIMEOUT = ParameterManager["CSOTSECSDATATIMEOUT"].GetInteger();
                  ret = true;
                  NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
                  return ret;
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
                              eqp.HsmsConnStatus = "DISCONNECTED";

                              //20150918 t3:針對report mode是HSMS_開頭的,就去更新CIM Mode. (Modify by CY)
                              switch (eqp.Data.REPORTMODE)
                              {
                                    case "HSMS_PLC":
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          break;
                                    default:
                                          //SECS 断线后如果是CIM On 的状态则需要通知UI 和Evisor 
                                          if (eqp.File.CIMMode == eBitResult.ON)
                                          {
                                                string trxKey = CreateTrxID();
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxKey, eqp.Data.NODENO, string.Format("{0} EQP CIM On but HSMS Disconnect !", eqp.Data.NODENO) });
                                                //Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { eqp.Data.LINEID, string.Format("{0} EQP CIM On but HSMS Disconnect !", eqp.Data.NODENO) });
                                                Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { eqp.Data.LINEID, "ALARM",string.Format("{0} EQP CIM On but HSMS Disconnect !", eqp.Data.NODENO) });
                                          }
                                          break;
                              }
                              eqp.SecsCommunicated = false;

                              //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                              Invoke(eServiceName.LineService, "CheckLineState", new object[] { base.CreateTrxID(), eqp.Data.LINEID });
                             
                        }
                        else if (!eqp.HsmsConnected && connected)
                        {
                              NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[EQUIPMENT={0}] HSMS State Change From [0 (Disconnected) ] -> [1 (Connected) ]", eqpno));
                              eqp.HsmsConnStatus = "CONNECTED";
                              if (!eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                  Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { eqp.Data.LINEID, "NORMAL", string.Format("{0} EQP CIM On and HSMS Connect Recover OK!", eqp.Data.NODENO) }); //add 2016/06/30 cc.kuang
                        }
                        eqp.HsmsConnected = connected;
                        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { base.CreateTrxID(), eqp });// wucc add 20150714
                        //20161204 yang:CIM Mode要报给MES
                        if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                            Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { base.CreateTrxID(), eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                        //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                        Invoke(eServiceName.LineService, "CheckLineState", new object[] { base.CreateTrxID(), eqp.Data.LINEID });
                  }

                  if (!connected)
                  {
                        //斷線要清
                        if (!this._RecipeCheckInfos.ContainsKey(eqpno))
                        {
                              return;
                        }
                        this._RecipeCheckInfos[eqpno].Clear();
                        if (!this._RecipeParameters.ContainsKey(eqpno))
                        {
                              return;
                        }
                        this._RecipeParameters[eqpno].Clear();
                  }
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

                              //20150610 cy:連線後就主動要求Online (登京已確認)
                              //不管跟誰家,都先去做通訊建立
                              TS1F13_H_EstablishCommunicationsRequest(eqp.Data.NODENO, eqp.Data.NODEID, "AutoOnlineRequest", string.Empty);
                        }
                        eqp.HsmsSelected = selected;
                  }
            }

            private void ThreadProc()
            {
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

                              //20150603 cy:增加判斷主動去取APC資料
                              #region [ Daliy check/APC ]
                              List<Line> lines = ObjectManager.LineManager.GetLines();
                              if (lines != null)
                              {
                                    bool reqDailyCheck = false;
                                    foreach (Line line in lines)
                                    {
                                          DateTime now = DateTime.Now;
                                          if (line.File.DailyCheckIntervalS != 0)
                                                reqDailyCheck = now.Subtract(this._DailyCheckLastDT).TotalSeconds >= line.File.DailyCheckIntervalS;

                                          List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                                          //eqp
                                          foreach (Equipment eqp in eqps)
                                          {
                                                //send s1f5_05/s1f5_09
                                                //t3 除了DailyCheck, 餘改用S6F3, 讓機台自己發
                                                if (eqp.Data.REPORTMODE == "HSMS_CSOT" || eqp.Data.REPORTMODE == "HSMS_PLC")
                                                {
                                                      //20150625 cy:offline時不發
                                                      if (eqp.File.HSMSControlMode == "OFF-LINE") continue;

                                                      ////20150714 cy:除SpecialData外, 其餘若機台不是Run或IDLE就不要求
                                                      //if (eqp.File.SpecialDataEnableReq && eqp.File.SpecialDataIntervalMS != 0 && now.Subtract(this._SpecialDataLastDT).TotalMilliseconds >= eqp.File.SpecialDataIntervalMS)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "06", string.Empty, string.Empty);
                                                      //      this._SpecialDataLastDT = now;
                                                      //}
                                                      //if (eqp.File.SpecialDataEnableReqForID && eqp.File.SpecialDataIntervalMSForID != 0 && now.Subtract(this._SpecialDataLastDTForID).TotalMilliseconds >= eqp.File.SpecialDataIntervalMSForID)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "10", string.Empty, string.Empty);
                                                      //      this._SpecialDataLastDTForID = now;
                                                      //}

                                                      if (eqp.File.Status != eEQPStatus.RUN && eqp.File.Status != eEQPStatus.IDLE)
                                                      {
                                                            //20150714 cy:機台不是Run或IDLE,不去要求資料,同時要清掉倉庫,避免取到舊資料
                                                            string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                                                            Repository.Remove(key);
                                                            //key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                            //Repository.Remove(key);
                                                            //key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                            //Repository.Remove(key);
                                                            continue;
                                                      }

                                                      if (reqDailyCheck)
                                                      {
                                                            //t3 DRY會by ID發
                                                            if (eqp.Data.NODEATTRIBUTE == "DRY")
                                                                  TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "09", string.Empty, string.Empty);
                                                            else
                                                                  TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "05", string.Empty, string.Empty);
                                                            this._DailyCheckLastDT = now; //update last
                                                      }
                                                      //if (eqp.File.APCImportanEnableReq && eqp.File.APCImportanIntervalMS != 0 && now.Subtract(this._APCDataIMPLastDT).TotalMilliseconds >= eqp.File.APCImportanIntervalMS)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "02", string.Empty, string.Empty);
                                                      //      this._APCDataIMPLastDT = now;
                                                      //}
                                                      //if (eqp.File.APCNormalEnableReq && eqp.File.APCNormalIntervalMS != 0 && now.Subtract(this._APCDataNORLastDT).TotalMilliseconds >= eqp.File.APCNormalIntervalMS)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "04", string.Empty, string.Empty);
                                                      //      this._APCDataNORLastDT = now;
                                                      //}

                                                      //// 設定 S2F21 Data Set Command for ID 固定發 7,8,10      wucc add 20150806
                                                      //if (eqp.File.APCImportanEnableReqForID && eqp.File.APCImportanIntervalMSForID != 0 && now.Subtract(this._APCDataIMPLastDTForID).TotalMilliseconds >= eqp.File.APCImportanIntervalMSForID)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "07", string.Empty, string.Empty);
                                                      //      this._APCDataIMPLastDTForID = now;
                                                      //}
                                                      //if (eqp.File.APCNormalEnableReqForID && eqp.File.APCNormalIntervalMSForID != 0 && now.Subtract(this._APCDataNORLastDTForID).TotalMilliseconds >= eqp.File.APCNormalIntervalMSForID)
                                                      //{
                                                      //      TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "08", string.Empty, string.Empty);
                                                      //      this._APCDataNORLastDTForID = now;
                                                      //}
                                                      
                                                }
                                          }
                                    }
                              }
                              #endregion

                              ProcS6F3_01();
                              ProcS6F3_02();
                              ProcS6F3_04();
                              ProcS6F3_05();
                              ProcS6F3_06();
                              ProcS6F3_08();
                              ProcS6F3_09();
                              ProcS6F3_10();
                              ProcS6F3_11();
                        }
                        catch (Exception ex)
                        {
                              Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        }
                  }
            }
            private void ProcS6F3_01()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._GlassProcessDataInformationReports)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              //nodeglasses
                              foreach (var g in n.Value.ToArray())
                              {
                                    List<Tuple<DateTime, XmlDocument>> tmp = null;

                                    if (g.Value.Count == 0)
                                    {
                                          continue;
                                    }

                                    //check expired
                                    if (DateTime.Now.Subtract(g.Value[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                                    {
                                          continue;
                                    }

                                    //get job object
                                    string glassid = g.Key;
                                    Job job = ObjectManager.JobManager.GetJob(glassid);
                                    if (job == null)
                                    {
                                          n.Value.TryRemove(glassid, out tmp);
                                          continue;
                                    }

                                    //report now
                                    ReportS6F3_01_E_GlassProcessDataInformationReport(eqp, job, "expired");
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_02()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._LotProcessDataInformationReports)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              //nodeglasses
                              foreach (var g in n.Value.ToArray())
                              {
                                    //List<Tuple<DateTime, XmlDocument>> tmp = null;

                                    if (g.Value.Count == 0)
                                    {
                                          continue;
                                    }

                                    //check expired
                                    if (DateTime.Now.Subtract(g.Value[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                                    {
                                          continue;
                                    }

                                    //get cst object
                                    string cstseq = g.Key;

                                    //report now
                                    ReportS6F3_02_E_LotProcessDataInformationReport(eqp, cstseq, "expired");
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_04()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._APCImportantDataInformationReports)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_04_E_APCImportantDataInformationReport(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_05()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._APCNormalDataInformationReports)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_05_E_APCNormalDataInformationReport(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_06()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._GlassProcessDataInformationReportfortestEQs)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              //nodeglasses
                              foreach (var g in n.Value.ToArray())
                              {
                                    List<Tuple<DateTime, XmlDocument>> tmp = null;

                                    if (g.Value.Count == 0)
                                    {
                                          continue;
                                    }

                                    //check expired
                                    if (DateTime.Now.Subtract(g.Value[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                                    {
                                          continue;
                                    }

                                    //get job object
                                    string glassid = g.Key;
                                    Job job = ObjectManager.JobManager.GetJob(glassid);
                                    if (job == null)
                                    {
                                          n.Value.TryRemove(glassid, out tmp);
                                          continue;
                                    }

                                    //report now
                                    ReportS6F3_06_E_GlassProcessDataInformationReportfortestEQ(eqp, job, "expired");
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_08()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._SpecialDataInformationReports)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_08_E_SpecialDataInformationReport(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_09()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._APCImportantDataInformationReportforIDs)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_09_E_APCImportantDataInformationReportforID(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_10()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._APCNormalDataInformationReportforIDs)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_10_E_APCNormalDataInformationReportforID(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ProcS6F3_11()
            {
                  try
                  {
                        //nodes
                        foreach (var n in this._SpecialDataInformationReportforIDs)
                        {
                              //get eqp object
                              string eqpno = n.Key;
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              //get line object
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                              List<Tuple<DateTime, XmlDocument>> tmp = n.Value;

                              if (tmp.Count == 0)
                              {
                                    continue;
                              }

                              //check expired
                              if (DateTime.Now.Subtract(tmp[0].Item1).TotalMilliseconds < this._CSOTSECSDATATIMEOUT)
                              {
                                    continue;
                              }

                              //report now
                              ReportS6F3_11_E_SpecialDataInformationReportforID(eqp, "expired");
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private eEQPStatus ConvertCsotStatus(string state)
            {
                  eEQPStatus rtn = eEQPStatus.SETUP;
                  switch (state)
                  {
                        case "T":
                              rtn = eEQPStatus.SETUP;
                              break;
                        case "I":
                              rtn = eEQPStatus.IDLE;
                              break;
                        case "R":
                              rtn = eEQPStatus.RUN;
                              break;
                        case "S":
                              rtn = eEQPStatus.STOP;
                              break;
                        case "P":
                              rtn = eEQPStatus.PAUSE;
                              break;
                  }
                  return rtn;
            }

            private eMaterialStatus ConvertCsotMaterialStatus(string state)
            {
                  eMaterialStatus rtn = eMaterialStatus.NONE;
                  switch (state)
                  {
                        case "U":
                              rtn = eMaterialStatus.DISMOUNT;
                              break;
                        case "M":
                              rtn = eMaterialStatus.MOUNT;
                              break;
                        case "I":
                              rtn = eMaterialStatus.INUSE;
                              break;
                        case "P":
                              rtn = eMaterialStatus.PREPARE;
                              break;
                  }
                  return rtn;
            }

            private string ConvertRecoveryMode(string state)
            {
                  string rtn = string.Empty;
                  switch (state)
                  {
                        case "0":
                              rtn = "Normal Mode";
                              break;
                        case "1":
                              rtn = "Recovery Mode";
                              break;
                        case "2":
                              rtn = "Abnormal Recovery Mode";
                              break;
                  }
                  return rtn;
            }

        private string ConvertProcessResult(string result)
        {
            string rtn = string.Empty;
            switch (result)
            {
                case "0":
                    rtn = "Not Processed"; 
                    break;
                case "1":
                    rtn = "Normal Processed";
                    break;
                case "2":
                    rtn = "Abnormal Processed";
                    break;
                case "3":
                    rtn = "Process Skip";
                    break;
            }
            return rtn;
        }

            private void HandleMaterialStatus(Equipment eqp,
                List<Tuple<Unit, string, string, eMaterialStatus, string, string>> materials,
                string tid, bool allsync, bool waitack, string systembyte, string header)
            {
                  if (materials != null && materials.Count <= 0)
                        return;

                  List<MaterialEntity> materialRptList = new List<MaterialEntity>();
                  List<MaterialEntity> mountMaterials = new List<MaterialEntity>();
                  List<MaterialEntity> dismountMaterials = new List<MaterialEntity>();
                  foreach (var material in materials)
                  {
                        Unit unit = material.Item1;
                        string slot = material.Item2;
                        string name = material.Item3;
                        eMaterialStatus status = material.Item4;
                        string amount = material.Item5;
                        string site = material.Item6;

                        eMaterialStatus oldstatus = eMaterialStatus.NONE; //init default
                        //get entity by key
                        string key = string.Format("{0}_{1}_{2}_{3}", eqp.Data.NODENO, unit == null ? "0" : unit.Data.UNITNO, slot, name);
                        MaterialEntity materialEntity = ObjectManager.MaterialManager.GetMaterialByKey(key);
                        //check if entity not exists,create
                        if (materialEntity == null)
                        {
                              materialEntity = new MaterialEntity();
                              materialEntity.MaterialType = _common.GetMaterialType(eqp); //20150116 cy:增加取得material type
                              materialEntity.EQType = eMaterialEQtype.Normal;
                              materialEntity.MaterialID = name;
                              materialEntity.MaterialSlotNo = slot;
                              materialEntity.NodeNo = eqp.Data.NODENO;
                              materialEntity.UnitNo = unit == null ? "0" : unit.Data.UNITNO;
                              materialEntity.MaterialPort = slot; //port為key值,避免有問題,所以讓port = slot

                              ObjectManager.MaterialManager.AddMaterial(materialEntity);
                        }
                        else
                        {
                              oldstatus = materialEntity.MaterialStatus;
                        }
                        materialEntity.MaterialStatus = status;
                        materialEntity.Site = site;//add by hujunpeng 20190223
                        if (!string.IsNullOrEmpty(amount))
                        {
                              materialEntity.MaterialWeight = amount;
                              int val;
                              int.TryParse(amount, out val);
                              //20141125 modify by Edison:MaterialValueINT->MaterialValue
                              materialEntity.MaterialWeight = val.ToString();
                              //20160822 modify by Yang:MaterialValue->MaterialWeight,MES看Weight决定用量
                        }
                        NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("MaterialStateChanged MaterialId({0}), from old({1}) to new({2})",
                                materialEntity.MaterialID, oldstatus.ToString(), materialEntity.MaterialStatus.ToString()));

                        if (status == eMaterialStatus.DISMOUNT)
                        {
                              ObjectManager.MaterialManager.UnMountMaterial(materialEntity);
                              //DismountMaterials.Add(materialEntity); // add by bruce 2015/09/25
                        }

                        //check if status change
                        //modify by Yang 20160919 不用帮忙过滤掉重复的status
                       // if (oldstatus != status)
                      //  {
                              //report material status rule change for t3. 20151006 cy.
                              if (status == eMaterialStatus.MOUNT)
                                    mountMaterials.Add(materialEntity);
                              else if (status == eMaterialStatus.DISMOUNT)
                                    dismountMaterials.Add(materialEntity);
                              else
                                    materialRptList.Add(materialEntity); //add to rpt list
                      //   }

                        //if (status == eMaterialStatus.MOUNT)
                        //{
                        //      mountMaterials.Add(materialEntity); //add to rpt MES mount check cc.kuang 2015/09/24
                        //}

                        ObjectManager.MaterialManager.EnqueueSave(materialEntity);
                  }

                  //check if need sycn all
                  if (allsync)
                  {
                        foreach (var material in ObjectManager.MaterialManager.GetMaterials())
                        {
                              if (!materials.Exists(m => m.Item3 == material.MaterialID
                                                     && (m.Item1 == null ? "0" : m.Item1.Data.UNITNO) == material.UnitNo
                                                     && eqp.Data.NODENO == material.NodeNo
                                                     ))
                              {
                                    //remove garbage
                                    ObjectManager.MaterialManager.UnMountMaterial(material);
                              }
                        }
                  }

                  //20151221 cy:改由下面上報MES時檢查
                  //check need rpt
                  //if (materialRptList.Count == 0)
                  //{
                  //      if (waitack)
                  //      {
                  //            TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack 
                  //      }
                  //      return; //no need report MES
                  //}

                  //20151221 cy:增加其它List物件到History
                  //20150420 cy:Add recoder to material history
                  #region Add recoder to material history
                  try
                  {
                        foreach (MaterialEntity m in mountMaterials)
                        {
                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, m.UnitNo, m, string.Empty, tid);
                        }
                        foreach (MaterialEntity m in dismountMaterials)
                        {
                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, m.UnitNo, m, string.Empty, tid);
                        }
                        foreach (MaterialEntity m in materialRptList)
                        {
                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, m.UnitNo, m, string.Empty, tid);
                        }
                  }
                  catch { }
                  #endregion


                  //20150122 cy:與MES offlien時,根據設定回覆OK,NG
                  Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                  if (line.File.HostMode == eHostMode.OFFLINE)
                  {
                        if (waitack)
                        {
                              if (ParameterManager["OFFLINEREPLYEQP"].GetBoolean())
                              {
                                    TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack 
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        "Host offline and BCS reply material status change result by setting. (OK)");
                              }
                              else
                              {
                                    TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 1); //ack 
                                    TS10F3_H_TerminalDisplaySingle(eqp.Data.NODENO, eqp.Data.NODEID, "Host Offline and BCS Reply MaterialStateChange NG", tid, string.Empty);
                                    if (eqp.Data.NODEATTRIBUTE == "DNS")
                                          TS10F5_H_TerminalDisplaySingleforDNSLC(eqp.Data.NODENO, eqp.Data.NODEID, header, string.Empty, tid);
                                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        "Host offline and BCS reply material status change result by setting. (NG)");
                              }
                        }
                        return;
                  }

                  //report MES
                  //t3 rule:
                  //Mount->上報MaterialMount, 若reply OK,上報MaterialStateChanged
                  //Dismount->上報MaterialDismountReport
                  //其它->MaterialStateChanged
                  if (mountMaterials.Count > 0)
                  {
                        object[] _data = new object[6]
                        　{
                             　tid,  /*0 TrackKey*/
                             　eqp,                 /*1 EQP*/
                        　　　　"",         /*2 GlassID*/
                        　　　　"",                  /*3 MaterialDurableName*/
                        　　　　"",                  /*4 PolType*/
                        　　　　mountMaterials         /*5 MaterialList*/
                    　　　};
                        Invoke(eServiceName.MESService, "MaterialMount", _data);
                        if (waitack)
                        {
                              string timerId = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialMount", eqp.Data.NODENO, tid);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }
                              //create wait timer
                              _timerManager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                                  new System.Timers.ElapsedEventHandler(MES_MaterialStateChangedReplyTimeOut), Tuple.Create(systembyte, header));
                        }
                  }
                  if (dismountMaterials.Count > 0)
                  {
                        object[] _data = new object[5]
                                        { 
                                            tid,  /*0 TrackKey*/
                                            eqp,                 /*1 EQP*/
                                            "",         /*2 GlassID*/
                                            "",                  /*3 MaterialDurableName*/
                                            dismountMaterials         /*5 MaterialList*/
                                        };
                        Invoke(eServiceName.MESService, "MaterialDismountReport", _data);
                  }
                  if (materialRptList.Count > 0)
                  {
                        object[] _data = new object[7]
			              { 
				              tid,  /*0 TrackKey*/
				              eqp.Data.LINEID,    /*1 LineName*/
				              eqp.Data.NODEID,    /*2 EQPID*/
				              "",          /*3 LINERECIPENAME*/
				              "",            /*4 MATERIALMODE*/ 
				              "",            /*5 panelID*/
				              materialRptList,          /*6 materlist*/
			              };
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                  }
                  //t3只有當報Mount才能wait,所以如果沒報mount,則直接回OK
                  if(waitack && mountMaterials.Count == 0)
                        TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack 

                  ////check if need wait ack
                  //if (waitack)
                  //{
                  //      string timerId = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, tid);
                  //      if (_timerManager.IsAliveTimer(timerId))
                  //      {
                  //            _timerManager.TerminateTimer(timerId); //remove old
                  //      }
                  //      //create wait timer
                  //      _timerManager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                  //          new System.Timers.ElapsedEventHandler(MES_MaterialStateChangedReplyTimeOut), Tuple.Create(systembyte, header));
                  //}

                  ////cc.kuang add for TCPHL line PR mount check
                  //if (line.Data.LINEID.Contains("TCPHL") && eqp.Data.NODENO.Equals("L3"))
                  //{
                  //    if (mountMaterials.Count > 0)
                  //    {
                  //        object[] _data２ = new object[6]
                  //       {
                  //            tid,  /*0 TrackKey*/
                  //            eqp,                 /*1 EQP*/
                  //          "",         /*2 GlassID*/
                  //          "",                  /*3 MaterialDurableName*/
                  //          "",                  /*4 PolType*/
                  //          mountMaterials         /*5 MaterialList*/
                  //     };
                  //        Invoke(eServiceName.MESService, "MaterialMount", _data２);
                  //        string timerId = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialMount", eqp.Data.NODENO, tid);
                  //        if (_timerManager.IsAliveTimer(timerId))
                  //        {
                  //            _timerManager.TerminateTimer(timerId); //remove old
                  //        }
                  //        //create wait timer
                  //        _timerManager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                  //            new System.Timers.ElapsedEventHandler(MES_MaterialStateChangedReplyTimeOut), Tuple.Create(systembyte, header));
                  //    }
                  //    else
                  //    {
                  //        foreach (var material in materials)   //add by bruce 2015/09/25 需上報 Material DisMount
                  //        {
                  //            Unit unit = material.Item1;
                  //            string slot = material.Item2;
                  //            string name = material.Item3;
                  //            eMaterialStatus status = material.Item4;
                  //            string amount = material.Item5;
                  //            string site = material.Item6;
                  //            if (status == eMaterialStatus.DISMOUNT)
                  //            {
                  //                object[] _data2 = new object[5]
                  //                      { 
                  //                          tid,  /*0 TrackKey*/
                  //                          eqp,                 /*1 EQP*/
                  //                          "",         /*2 GlassID*/
                  //                          "",                  /*3 MaterialDurableName*/
                  //                          dismountMaterials         /*5 MaterialList*/
                  //                      };
                  //                Invoke(eServiceName.MESService, "MaterialDismountReport", _data2);
                  //            }
                  //        }
                  //    }
                  //}
            }
            //timeout handler
            private void MES_MaterialStateChangedReplyTimeOut(object subject, System.Timers.ElapsedEventArgs e)
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
                            string.Format("Invalid TimerId=({0}) error", timer.TimerId));
                        return;
                  }
                  string eqpno = arr[1];
                  string tid = arr[2];
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                  if (eqp == null)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can not find Equipment No({0}) in EquipmentEntity! TrxId({1})", eqpno, tid));
                        return;
                  }

                  Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                  if (tuple != null)
                  {
                        string systembyte = tuple.Item1;
                        string header = tuple.Item2;
                        //mes timeout,ack eq ng
                        TS6F12_H_EventReportAcknowledge(eqpno, eqp.Data.NODEID, tid, systembyte, 1); //ack ng

                        if (eqp.Data.NODEATTRIBUTE == "DNS")
                        {
                              Invoke(eServiceName.CSOTSECSService, "TS10F5_H_TerminalDisplaySingleforDNSLC",
                                  new object[] { eqp.Data.NODENO, eqp.Data.NODEID, header, string.Empty, tid });
                        }
                  }
            }


            //private void HandleMaskStatus(Equipment eqp,List<Tuple<string ,string , eMaterialStatus>> masks, 
            //    string tid,bool waitack,string systembyte) 
            //{
            //    if (masks != null && masks.Count <= 0)
            //        return;

            //    IList<MaskStateChanged.MASKc> maskRptList=new List<MaskStateChanged.MASKc>();
            //    foreach (var mask in masks) 
            //    {
            //        string slot = mask.Item1;
            //        string newname = mask.Item2;
            //        eMaterialStatus newstatus = mask.Item3;

            //        string oldname = string.Empty; //init default
            //        eMaterialStatus oldstatus = eMaterialStatus.NONE; //init default				
            //        //get entity by slot or name
            //        MaterialEntity maskEntity = null;
            //        if (string.IsNullOrEmpty(slot)) {
            //            if (string.IsNullOrEmpty(newname)) {
            //                throw new Exception("The parameter of mask slot and name is empty. TrxID=" + tid);
            //            } else {
            //                maskEntity = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, newname);
            //            }
            //        } else {
            //            maskEntity = ObjectManager.MaterialManager.GetMaterialBySlot(eqp.Data.NODENO, slot);
            //        }
            //        //check if enity not exists,create
            //        if (maskEntity == null) {
            //            maskEntity = new MaterialEntity();
            //            maskEntity.NodeNo = eqp.Data.NODENO;
            //            maskEntity.UnitNo = "0";
            //            maskEntity.MaterialPort = slot; //port為key值,避免有問題,所以讓port = slot
            //            maskEntity.MaterialID = newname;
            //            maskEntity.MaterialSlotNo = slot;
            //            maskEntity.MaterialStatus = newstatus;
            //            ObjectManager.MaterialManager.AddMaterial(maskEntity);
            //        } else {
            //            oldstatus = maskEntity.MaterialStatus;
            //            oldname = maskEntity.MaterialID;
            //        }

            //        if (oldstatus != newstatus) 
            //        {
            //            if (oldname == newname) {
            //                //oldstatus<>newstatus && oldname=namenew
            //                //check new rpt
            //                if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
            //                {
            //                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
            //                    item.MASKPOSITION = maskEntity.MaterialSlotNo;
            //                    item.MASKNAME = newname;
            //                    item.MASKSTATE = newstatus.ToString();
            //                    maskRptList.Add(item);
            //                }
            //            } else {
            //                //oldstatus<>newstatus && oldname<>newname 
            //                //check old unmount
            //                if (!string.IsNullOrEmpty(oldname) && oldstatus != eMaterialStatus.NONE && oldstatus != eMaterialStatus.DISMOUNT)
            //                {
            //                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
            //                    item.MASKPOSITION = maskEntity.MaterialSlotNo;
            //                    item.MASKNAME = oldname;
            //                    item.MASKSTATE = eMaterialStatus.DISMOUNT.ToString();
            //                    maskRptList.Add(item);
            //                }
            //                //check new rpt
            //                if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
            //                {
            //                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
            //                    item.MASKPOSITION = maskEntity.MaterialSlotNo;
            //                    item.MASKNAME = newname;
            //                    item.MASKSTATE = newstatus.ToString();
            //                    maskRptList.Add(item);
            //                }
            //            }
            //        } else {					
            //            if (oldname == newname) {
            //                //oldstatus==newstatus && oldname==newname
            //                //same name & same status,skip
            //            } else {
            //                //oldstatus==newstatus && oldname<>newname
            //                //check old unmount
            //                if (!string.IsNullOrEmpty(oldname) && oldstatus != eMaterialStatus.NONE && oldstatus != eMaterialStatus.DISMOUNT)
            //                {
            //                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
            //                    item.MASKPOSITION = maskEntity.MaterialSlotNo;
            //                    item.MASKNAME = oldname;
            //                    item.MASKSTATE = eMaterialStatus.DISMOUNT.ToString();
            //                    maskRptList.Add(item);                   
            //                }
            //                //check new rpt
            //                if (!string.IsNullOrEmpty(newname) && newstatus != eMaterialStatus.NONE)
            //                {
            //                    MaskStateChanged.MASKc item = new MaskStateChanged.MASKc();
            //                    item.MASKPOSITION = maskEntity.MaterialSlotNo;
            //                    item.MASKNAME = newname;
            //                    item.MASKSTATE = newstatus.ToString();
            //                    maskRptList.Add(item);
            //                }
            //            }
            //        }
            //        //update name/status
            //        maskEntity.MaterialID = newname;
            //        maskEntity.MaterialStatus = newstatus;
            //        NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("Set Mask ({0}) in slot ({1}) as ({2}).", maskEntity.MaterialID, maskEntity.MaterialSlotNo, maskEntity.MaterialStatus.ToString()));
            //        //20141215 cy add:標記機台使用中的mask id
            //        if (maskEntity.MaterialStatus == eMaterialStatus.INUSE)
            //            eqp.InUseMaskID = maskEntity.MaterialID;
            //        //20141218 cy:Dismount時要叫用Manager以刪除集合
            //        else if (maskEntity.MaterialStatus == eMaterialStatus.DISMOUNT)
            //            ObjectManager.MaterialManager.UnMountMaterial(maskEntity);

            //        ObjectManager.MaterialManager.EnqueueSave(maskEntity);
            //    }

            //    //check need rpt
            //    if (maskRptList.Count==0) {
            //        if (waitack) {
            //            TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 0); //ack ng
            //        }
            //        return; //no need rpt mes
            //    }

            //    //呼叫MES方法
            //    object[] _data = new object[6]
            //    { 
            //        tid,  /*0 TrackKey*/
            //        eqp.Data.LINEID,    /*1 LineName*/
            //        eqp.Data.NODEID,    /*2 EQPID*/
            //        "", /*3 machineRecipeName*/
            //        "", /*4 eventUse*/
            //        maskRptList /*5 masklist*/
            //    };			
            //    Invoke(eServiceName.MESService, "MaskStateChanged", _data);

            //    //check if need wait ack
            //    if (waitack) {
            //        string timerId = string.Format("SecsMaskStatusChangeReport_{0}_{1}_MaskStateChanged", eqp.Data.NODENO, tid);
            //        if (_timerManager.IsAliveTimer(timerId)) {
            //            _timerManager.TerminateTimer(timerId); //remove old
            //        }
            //        //create wait timer
            //        _timerManager.CreateTimer(timerId, false, 30000,
            //            new System.Timers.ElapsedEventHandler(MES_MaskStateChangedReplyTimeOut), Tuple.Create(systembyte, string.Empty));
            //    }
            //}

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
                                    maskEntity.MaterialType = _common.GetMaterialType(eqp); //20150116 cy:增加取得material type
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
                              maskEntity.MaterialType = _common.GetMaterialType(eqp); //20150116 cy:增加取得material type
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
                                        //maskRptList.Add(item); if (newstatus != eMaterialStatus.DISMOUNT) //modify for avoid dismount other line's mount mask 2016/03/24
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

                  //20150420 cy:Add recoder to material history
                  #region Add recoder to material history
                  try
                  {
                        foreach (MaskStateChanged.MASKc m in maskRptList)
                        {
                              MaterialEntity me = new MaterialEntity();
                              me.MaterialType = _common.GetMaterialType(eqp); //20150116 cy:增加取得material type
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
                                    string.Format("Can not find Line ID({0}) in LineEntity! TrxID ({1})", eqp.Data.LINEID, tid));
                        TS6F12_H_EventReportAcknowledge(eqp.Data.NODENO, eqp.Data.NODEID, tid, systembyte, 1); //ack ng
                        return;
                  }
                  else if (line != null && line.File.HostMode == eHostMode.OFFLINE)
                  {
                        if (waitack)
                        {
                              if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
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
				eqp.File.CurrentRecipeID, /*3 machineRecipeName*/
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
                            string.Format("Invalid TimerId=({0}) error", timer.TimerId));
                        return;
                  }
                  string eqpno = arr[1];
                  string tid = arr[2];
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                  if (eqp == null)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can not find Equipment No({0}) in EquipmentEntity! TrxId({1})", eqpno, tid));
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
                  TS10F3_H_TerminalDisplaySingle(eqpno, eqp.Data.NODEID, "Host check mask status timeout.", tid, string.Empty);
            }

            private bool HandleControlMode(Equipment eqp, string CTRLMODE, out string msg)
            {
                  msg = string.Empty;
                  try
                  {
                        switch (CTRLMODE)
                        {
                              case "1": //Offline
                                    if (eqp.File.HSMSControlMode != "OFF-LINE")
                                    {
                                          string oldCtrlMode = eqp.File.HSMSControlMode;
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          msg = string.Format("Control mode changed from ({0}) to ({1}).", oldCtrlMode, eqp.File.HSMSControlMode);
                                    }
                                    else
                                          msg = string.Format("Control mode is ({0}).", eqp.File.HSMSControlMode);
                                    break;
                              case "2": //Online
                                    if (eqp.File.HSMSControlMode != "ON-LINE")
                                    {
                                          string oldCtrlMode = eqp.File.HSMSControlMode;
                                          eqp.File.HSMSControlMode = "ON-LINE";
                                          msg = string.Format("Control mode changed from ({0}) to ({1}).", oldCtrlMode, eqp.File.HSMSControlMode);
                                    }
                                    else
                                          msg = string.Format("Control mode is ({0}).", eqp.File.HSMSControlMode);
                                    break;
                              default:
                                    msg = string.Format("Unknow control mode({0})!", CTRLMODE);
                                    return false;
                        }

                        //20150918 t3:針對report mode是HSMS_開頭的,就去更新CIM Mode. (Modify by CY)
                        switch (eqp.Data.REPORTMODE)
                        {
                              case "HSMS_PLC":
                                    if (CTRLMODE == "1")
                                          eqp.File.CIMMode = eBitResult.OFF;
                                    else
                                          eqp.File.CIMMode = eBitResult.ON;
                                    msg = string.Format("{0} CIM Mode({1}).", msg, eqp.File.CIMMode.ToString());
                                    break;
                              default:
                                    if ((eqp.File.CIMMode == eBitResult.ON) && CTRLMODE == "1")
                                    {
                                          string trxKey = CreateTrxID();
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxKey, eqp.Data.NODENO, string.Format("{0} EQP CIM On but HSMS Control Mode Change to Offline !", eqp.Data.NODENO) });
                                          //Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { eqp.Data.LINEID, string.Format("{0} EQP CIM On but HSMS Control Mode Change To Offline !", eqp.Data.NODENO) });
                                          Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { eqp.Data.LINEID,"ALARM", string.Format("{0} EQP CIM On but HSMS Control Mode Change To Offline !", eqp.Data.NODENO) });
                                    }
                                    break;
                        }
                        return true;
                  }
                  catch (Exception ex)
                  {
                        msg = "HandleControlMode:" + ex;
                        return false;
                  }
            }

            private string GetDCNAME(IList<DailyCheckData> dataFormats, string dcid)
            {
                  try
                  {
                        string dcname = dcid;
                        if (dataFormats != null && dataFormats.Count > 0)
                        {
                              DailyCheckData dc = dataFormats.FirstOrDefault(d => d.Data.SVID == dcid);
                              if (dc != null)
                                    dcname = dc.Data.PARAMETERNAME;
                        }
                        return dcname;
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        return dcid;
                  }
            }

          // add by box.zhai 添加  传入APC Data ID获取Name
            private string GetDCNAMEForAPC(IList<APCDataReport> dataFormats, string dcid)
            {
                try
                {
                    string dcname = dcid;
                    if (dataFormats != null && dataFormats.Count > 0)
                    {
                        APCDataReport dc = dataFormats.FirstOrDefault(d => d.Data.SVID == dcid);
                        if (dc != null)
                            dcname = dc.Data.PARAMETERNAME;
                    }
                    return dcname;
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    return dcid;
                }
            }

          //add by yang for Energy get parameter name via dcid
          private string GetDCNAMEForEnergy(IList<EnergyVisualizationData> dataFormats,string dcid)
            {
                try
                {
                    string dcname = dcid;
                    if (dataFormats != null && dataFormats.Count > 0)
                    {
                        EnergyVisualizationData dc = dataFormats.FirstOrDefault(d => d.Data.SVID == dcid);
                        if (dc != null)
                            dcname = dc.Data.PARAMETERNAME;
                    }
                    return dcname;
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    return dcid;
                }
            }
      }
}
