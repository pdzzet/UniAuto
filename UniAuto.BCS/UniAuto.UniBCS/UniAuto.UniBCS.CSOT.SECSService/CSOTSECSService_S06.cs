using System;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.MISC;
using System.Text.RegularExpressions;
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      class eVCRTYPE
      {
            public const string A = "A";
            public const string B = "B";
      }
      public partial class CSOTSECSService
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
            public void S6F3_01_E_GlassProcessDataInformationReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_01_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        int cstseq = 0;
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check if controlmode offline,drop
                        //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                        //if (line.File.HostMode == eHostMode.OFFLINE) {
                        //    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        //        string.Format("MES offline,drop EQP({0}), Glass({1}), S6F3_01 SUBCD({2}), GlassProcessDataInformationReport!", eqp.Data.NODEID, glassid, subcd));
                        //    return;
                        //}

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job in JobEntity by GlassID({0})!", glassid));
                              job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                              if (job == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Can not find Job in JobEntity by CSTSEQ({0}) and SLOT({1})!", cstseq.ToString(), slot.ToString()));
                                    return;
                              }
                        }
                        //add by hujunpeng 20190416
                        job.ArraySpecial.CurrentRecipeID = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPID"].InnerText.Trim();
                        //20150417 cy:將Job內的PhotoIsProcessed設為true,但退port時判斷是否經過SECS機台(for array)
                        job.ArraySpecial.PhotoIsProcessed = true;

                        //get node GlassProcessDataInformationReport
                        if (!this._GlassProcessDataInformationReports.ContainsKey(eqpno))
                        {
                              bool done = this._GlassProcessDataInformationReports.TryAdd(eqpno, new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node GlassProcessDataInformationReport!");
                                    return;
                              }
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeGlassProcessDataInformationReport = this._GlassProcessDataInformationReports[eqpno];

                        //get glass GlassProcessDataInformationReport
                        if (!NodeGlassProcessDataInformationReport.ContainsKey(glassid))
                        {
                              bool done = NodeGlassProcessDataInformationReport.TryAdd(glassid, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Fail to add glass GlassProcessDataInformationReport GlassID({0})!", glassid));
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> GlassProcessDataInformationReport = NodeGlassProcessDataInformationReport[glassid];

                        //new add
                        GlassProcessDataInformationReport.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_01_E_GlassProcessDataInformationReport(eqp, job, "final");
                        }
                        return;
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            private void ReportS6F3_01_E_GlassProcessDataInformationReport(Equipment eqp, Job job, string reason)
            {
                  try
                  {
                        //get node GlassProcessDataInformationReport
                        string eqpno = eqp.Data.NODENO;
                        if (!this._GlassProcessDataInformationReports.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Fail to get node GlassProcessDataInformationReport EQPNo({0})!", eqpno));
                              return;
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeGlassProcessDataInformationReport = this._GlassProcessDataInformationReports[eqpno];

                        //remove glass from NodeGlassProcessDataInformationReport
                        string glassid = job.GlassChipMaskBlockID;
                        List<Tuple<DateTime, XmlDocument>> GlassProcessDataInformationReport = null;
                        NodeGlassProcessDataInformationReport.TryRemove(glassid, out GlassProcessDataInformationReport);
                        if (GlassProcessDataInformationReport == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Fail to get glass GlassProcessDataInformationReport EQPNo({0}), Glass({1})!", eqpno, glassid));
                              return;
                        }
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        //<array1 name="List" type="L" len="6">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <OPERATIONID name="OPERATIONID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="9">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <UNIQUEID name="UNIQUEID" type="A" len="6" fixlen="False" />
                        //    <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //    <CARRIERID name="CARRIERID" type="A" len="10" fixlen="False" />
                        //    <PROCESS_RESULT name="PROCESS_RESULT" type="A" len="1" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <array4 name="List" type="L" len="3">
                        //        <PDCNAME name="PDCNAME" type="A" len="30" fixlen="False" />
                        //        <PDCTYPE name="PDCTYPE" type="A" len="3" fixlen="False" />
                        //        <PDCVALUE name="PDCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //20150325 cy:增加for eda名稱會不同
                        //20150408 cy:修改寫法,可檢查名稱重覆
                        //IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();
                        Dictionary<string, Tuple<string, string, string, string, string>> pdata = new Dictionary<string, Tuple<string, string, string, string, string>>();
                        IList<ChangeTargetLife.CHAMBERc> ctlChambers = new List<ChangeTargetLife.CHAMBERc>();
                        string tid = string.Empty;
                        HoldInfo hold = null;
                        //subcd 1~99 list
                        foreach (var v in GlassProcessDataInformationReport)
                        {
                              XmlDocument recvTrx = v.Item2;
                              //20150608 cy:增加非Normal Process Result上報Hold
                              string result = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PROCESS_RESULT"].InnerText.Trim();
                              if (!result.Equals("1"))
                              {
                                    hold = new HoldInfo()
                                    {
                                          NodeNo = eqp.Data.NODENO,
                                          NodeID = eqp.Data.NODEID,
                                          UnitNo = "0",
                                          UnitID = string.Empty,
                                          HoldReason = ConvertProcessResult(result),
                                          OperatorID = eqp.Data.NODEID,
                                    };
                              }

                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                              string len = xNode.Attributes["len"].InnerText.Trim();
                              int loop = 0;
                              int.TryParse(len, out loop);
                              for (int i = 0; i < loop; i++)
                              {
                                    string pdcname = xNode.ChildNodes[i]["PDCNAME"].InnerText.Trim();
                                    //20150323 cy:針對name,避開特殊符號
                                    pdcname = Regex.Replace(pdcname, @"[/| |?|/|<|>|'|\-|,]", "_"); //pdcname.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                                    string pdctype = xNode.ChildNodes[i]["PDCTYPE"].InnerText.Trim();
                                    string pdcvalue = xNode.ChildNodes[i]["PDCVALUE"].InnerText.Trim();

                                    if (pdata.ContainsKey(pdcname))
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  string.Format("Glass process data item duplicate. PDCName({0})", pdcname));
                                          pdata.Remove(pdcname);
                                    }
                                    pdata.Add(pdcname, new Tuple<string, string, string, string, string>(pdcname, "NA", "DEFAULT", pdctype, pdcvalue));
                                    //ProductProcessData.ITEMc itemC = new ProductProcessData.ITEMc();
                                    //itemC.ITEMNAME = pdcname;
                                    //itemC.SITELIST.Add(new ProductProcessData.SITEc() { SITENAME = "DEFAULT", SITEVALUE = pdcvalue });
                                    //mesdic.Add(itemC);

                                    //20150310 cy 增加特定欄位,上報特定功能
                                    //20150603 cy 增加DNS特定欄位上報
                                    #region For ChangeTargetLife
                                    if (line.Data.FABTYPE.ToUpper() == "ARRAY")
                                    {
                                          switch (line.Data.LINETYPE)
                                          {
                                                /* t3 not use cc.kuang 2015/07/03
                                                case eLineType.ARRAY.PVD:
                                                      string key = string.Format("{0}_AKT_PROCESS_DATA_TARGETLIFE", line.Data.LINETYPE);

                                                      ConstantData _constantdata = ConstantManager[key];

                                                      if (_constantdata != null && _constantdata[pdcname.Trim()].Value.ToUpper() == "TRUE")
                                                      {
                                                            ChangeTargetLife.CHAMBERc obj = new ChangeTargetLife.CHAMBERc()
                                                            {
                                                                  CHAMBERID = job.ChamberName,
                                                                  QUANTITY = pdcvalue
                                                            };

                                                            ctlChambers.Add(obj);
                                                      }
                                                      break;
                                                */
                                                case eLineType.ARRAY.PHL_TITLE:
                                                case eLineType.ARRAY.PHL_EDGEEXP:
                                                      if (eqp.Data.NODEATTRIBUTE.Equals("DNS"))
                                                      {
                                                            if (pdcname.Trim().Equals("SBUseHPNum"))
                                                                  job.ArraySpecial.DNS_SB_HP_NUM = pdcvalue;
                                                            if (pdcname.Trim().Equals("SBUseCPNum"))
                                                                  job.ArraySpecial.DNS_SB_CP_NUM = pdcvalue;
                                                            if (pdcname.Trim().Equals("HBUseHPNum"))
                                                                  job.ArraySpecial.DNS_HB_HP_NUM = pdcvalue;
                                                            if (pdcname.Trim().Equals("LCDRPrcCh"))
                                                                  job.ArraySpecial.DNS_VCD_NUM = pdcvalue;
                                                            if (pdcname.Trim().Equals("LCRegBcd"))  //add by bruce 2015/12/29
                                                                job.ArraySpecial.DNS_MATERIALID = pdcvalue;
                                                            if (pdcname.Trim().Equals("LCCTDpQt"))  // add by bruce 2015/12/29
                                                                job.ArraySpecial.DNS_MATERIAL_CONSUMEABLE = pdcvalue;
                                                            if (pdcname.Trim().Equals("LCCTCtPrsAve"))  // add by qiumin 20171227
                                                                job.ArraySpecial.DNS_LCCTCtPrsAve = pdcvalue;
                                                      }
                                                      break;
                                          }

                                    }
                                    #endregion
                              }
                        }
                        //20150608 cy:增加非Normal Process Result上報Hold
                        if (hold != null)
                        {
                              ObjectManager.JobManager.HoldEventRecord(job, hold);
                        }
                        //this._common.ProcessDataReport(eqp, job, mesdic, tid, true);
                        this._common.ProcessDataReport(eqp, job, pdata, tid);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report GlassProcessDataInformationReport EQPNo({0}), Glass({1}), reason({2})!", eqpno, glassid, reason));

                        #region 上報ChangeTargetLife(如果需要)
                        if (ctlChambers.Count > 0)
                              Invoke(eServiceName.MESService, "ChangeTargetLife", new object[] { tid, line.Data.LINEID, eqp.Data.NODEID, ctlChambers });
                        #endregion

                      #region 上報 MaterialConsumableReport
                        MaterialEntity material = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, job.ArraySpecial.DNS_MATERIALID);
                        if (material != null)
                        {
                            material.MaterialValue = job.ArraySpecial.DNS_MATERIAL_CONSUMEABLE;
                            List<MaterialEntity> materialList = new List<MaterialEntity>();
                            materialList.Add(material);
                            object[] _dataPrepare = new object[6]
                            { 
                            tid, /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp,                /*2 Equipment*/
                            job.GlassChipMaskBlockID,            /*3 ProductName*/
                            "",                 /*4 MaterialDurableName*/
                            materialList,       /*5 materilst*/ 
                            };
                            Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                        }
                        else
                        {
                            NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Fail to get material Entity!! Material ID({0})!", job.ArraySpecial.DNS_MATERIALID));
                        }
                      #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_02_E_LotProcessDataInformationReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_02_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        int cstseq = 0;
                        if (!int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq))
                        {
                              _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Can not convert cassette sequince value. CSTSEQ({0})", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim()));
                              return;
                        }

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                          "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check if controlmode offline,drop
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("MES offline,drop EQP({0}), CST({1}), S6F3_02 SUBCD({2}), LotProcessDataInformationReport!", eqp.Data.NODEID, cstseq, subcd));
                              return;
                        }

                        //get node LotProcessDataInformationReport
                        if (!this._LotProcessDataInformationReports.ContainsKey(eqpno))
                        {
                              bool done = this._LotProcessDataInformationReports.TryAdd(eqpno, new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node LotProcessDataInformationReport!");
                                    return;
                              }
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeLotProcessDataInformationReport = this._LotProcessDataInformationReports[eqpno];

                        //get cst LotProcessDataInformationReport
                        if (!NodeLotProcessDataInformationReport.ContainsKey(cstseq.ToString()))
                        {
                              bool done = NodeLotProcessDataInformationReport.TryAdd(cstseq.ToString(), new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Fail to add cst LotProcessDataInformationReport CST({0})!", cstseq));
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> LotProcessDataInformationReport = NodeLotProcessDataInformationReport[cstseq.ToString()];

                        //new add
                        LotProcessDataInformationReport.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_02_E_LotProcessDataInformationReport(eqp, cstseq.ToString(), "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_02_E_LotProcessDataInformationReport(Equipment eqp, string cstseq, string reason)
            {
                  try
                  {
                        //get node LotProcessDataInformationReport
                        string eqpno = eqp.Data.NODENO;
                        if (!this._LotProcessDataInformationReports.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node LotProcessDataInformationReport EQPNo({0})!", eqpno));
                              return;
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeLotProcessDataInformationReport = this._LotProcessDataInformationReports[eqpno];

                        //remove cst from LotProcessDataInformationReport
                        List<Tuple<DateTime, XmlDocument>> LotProcessDataInformationReport = null;
                        NodeLotProcessDataInformationReport.TryRemove(cstseq, out LotProcessDataInformationReport);
                        if (LotProcessDataInformationReport == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get cst LotProcessDataInformationReport EQPNo({0}), CST({1})!", eqpno, cstseq));
                              return;
                        }

                        //<array1 name="List" type="L" len="6">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <OPERATIONID name="OPERATIONID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="6">
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <UNIQUEID name="UNIQUEID" type="A" len="6" fixlen="False" />
                        //    <LOTSTARTTIME name="LOTSTARTTIME" type="A" len="14" fixlen="False" />
                        //    <LOTENDTIME name="LOTENDTIME" type="A" len="14" fixlen="False" />
                        //    <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <array4 name="List" type="L" len="3">
                        //        <PDCNAME name="PDCNAME" type="A" len="30" fixlen="False" />
                        //        <PDCTYPE name="PDCTYPE" type="A" len="3" fixlen="False" />
                        //        <PDCVALUE name="PDCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();
                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in LotProcessDataInformationReport)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                              string len = xNode.Attributes["len"].InnerText.Trim();
                              int loop = 0;
                              int.TryParse(len, out loop);
                              for (int i = 0; i < loop; i++)
                              {
                                    string pdcname = xNode.ChildNodes[i]["PDCNAME"].InnerText.Trim();
                                    //20150323 cy:針對name,避開特殊符號
                                    pdcname = pdcname.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                                    string pdctype = xNode.ChildNodes[i]["PDCTYPE"].InnerText.Trim();
                                    string pdcvalue = xNode.ChildNodes[i]["PDCVALUE"].InnerText.Trim();

                                    ProductProcessData.ITEMc itemC = new ProductProcessData.ITEMc();
                                    itemC.SITELIST.Add(new ProductProcessData.SITEc() { SITENAME = pdcname, SITEVALUE = pdcvalue });
                                    mesdic.Add(itemC);
                              }
                        }
                        this._common.LotProcessDataReport(eqp, cstseq, mesdic, tid);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report LotProcessDataInformationReport EQPNo({0}), CST({1}), reason({2})!", eqpno, cstseq, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_03_E_GlassHistoryReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_03_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //<array1 name="List" type="L" len="2">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="5">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <GSTARTIME name="GSTARTIME" type="A" len="14" fixlen="False" />
                        //    <GPRENTIME name="GPRENTIME" type="A" len="14" fixlen="False" />
                        //    <GPROCTIME name="GPROCTIME" type="A" len="4" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <array4 name="List" type="L" len="5">
                        //        <UNITID name="UNITID" type="A" len="16" fixlen="False" />
                        //        <PROCESS_RESULT name="PROCESS_RESULT" type="A" len="1" fixlen="False" />
                        //        <UGSTARTIME name="UGSTARTIME" type="A" len="14" fixlen="False" />
                        //        <UGPRENTIME name="UGPRENTIME" type="A" len="14" fixlen="False" />
                        //        <UGPROCTIME name="UGPROCTIME" type="A" len="4" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check if controlmode offline,drop
                        //20150306 cy:此功能尚不知要怎麼報,所以先不卡offline
                        //if (line.File.HostMode == eHostMode.OFFLINE) {
                        //    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        //        string.Format("MES offline,drop EQP({0}), Glass({1}), S6F3_03 GlassHistoryReport!", eqp.Data.NODEID, glassid));
                        //    return;
                        //}

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job GlassID({0}) in JobEntity!", glassid));
                              return;
                        }

                        string gstarttime = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GSTARTIME"].InnerText.Trim();
                        string gprentime = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GPRENTIME"].InnerText.Trim();
                        string gproctime = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GPROCTIME"].InnerText.Trim();
                        //20150417 cy:收到這個eventy,把之前send/receive產生的資料都刪除,以此evnet的資料為主
                        lock (job)
                        {

                              if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                              {
                                    ProcessFlow pcf = new ProcessFlow();
                                    pcf.MachineName = eqp.Data.NODEID;
                                    pcf.StartTime = _common.DateTime14ToDateTime(gstarttime);
                                    pcf.EndTime = _common.DateTime14ToDateTime(gprentime);
                                    job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                              }
                              else
                              {
                                    job.JobProcessFlows[eqp.Data.NODEID].StartTime = _common.DateTime14ToDateTime(gstarttime);
                                    job.JobProcessFlows[eqp.Data.NODEID].EndTime = _common.DateTime14ToDateTime(gprentime);
                              }

                              //add for OEE pcf 2016/06/08 cc.kuang
                              if (!job.JobProcessFlowsForOEE.ContainsKey(eqp.Data.NODEID))
                              {
                                  ProcessFlow pcf = new ProcessFlow();
                                  pcf.MachineName = eqp.Data.NODEID;
                                  pcf.StartTime = _common.DateTime14ToDateTime(gstarttime);
                                  pcf.EndTime = _common.DateTime14ToDateTime(gprentime);
                                  job.JobProcessFlowsForOEE.Add(eqp.Data.NODEID, pcf);
                              }
                              else
                              {
                                  job.JobProcessFlowsForOEE[eqp.Data.NODEID].StartTime = _common.DateTime14ToDateTime(gstarttime);
                                  job.JobProcessFlowsForOEE[eqp.Data.NODEID].EndTime = _common.DateTime14ToDateTime(gprentime);
                              }

                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                              string len = xNode.Attributes["len"].InnerText.Trim();
                              int loop = 0;
                              int.TryParse(len, out loop);
                              //units
                              //20150417 cy:收到這個eventy,把之前send/receive產生的資料都刪除,以此evnet的資料為主
                              if (loop > 0)
                              {
                                  job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Clear();
                                  //modify for EDA, OEE unit list 2016/03/14 cc.kuang
                                  job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Clear();
                                  job.JobProcessFlowsForOEE[eqp.Data.NODEID].UnitProcessFlows.Clear();
                                  job.JobProcessFlowsForOEE[eqp.Data.NODEID].ExtendUnitProcessFlows.Clear();
                              }

                              for (int i = 0; i < loop; i++)
                              {
                                    string unitid = xNode.ChildNodes[i]["UNITID"].InnerText.Trim();
                                    Unit unit = ObjectManager.UnitManager.GetUnit(unitid);
                                    if (unit == null)
                                    {
                                          continue;
                                    }
                                    else if (unit.Data.UNITATTRIBUTE == "VIRTUAL")
                                          continue;
                                    string processresult = xNode.ChildNodes[i]["PROCESS_RESULT"].InnerText.Trim();
                                    string ugstarttime = xNode.ChildNodes[i]["UGSTARTIME"].InnerText.Trim();
                                    string ugprentime = xNode.ChildNodes[i]["UGPRENTIME"].InnerText.Trim();
                                    string ugproctime = xNode.ChildNodes[i]["UGPROCTIME"].InnerText.Trim();
                                    //20150401 cy:針對重複進出的unit,放到extens list去
                                    ProcessFlow pcf = new ProcessFlow();
                                    pcf.MachineName = unit.Data.UNITID;//unit.Data.NODEID; 此处应使用UNITID  20150405 Tom
                                    pcf.StartTime = _common.DateTime14ToDateTime(ugstarttime);
                                    pcf.EndTime = _common.DateTime14ToDateTime(ugprentime);
                                    if (!job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                                    {
                                          //ProcessFlow pcf = new ProcessFlow();
                                          //pcf.MachineName = unit.Data.NODEID;
                                          job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, pcf);
                                    }

                                    //modify for EDA, OEE unit list 2016/03/14 cc.kuang
                                    //else 
                                          //job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Add(pcf);
                                    job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Add(pcf);
                                    job.JobProcessFlowsForOEE[eqp.Data.NODEID].ExtendUnitProcessFlows.Add(pcf);

                                    //job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(ugstarttime);
                                    //job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(ugprentime);
                              }
                        }

                        ObjectManager.JobManager.EnqueueSave(job);
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }



            public void S6F3_04_E_APCImportantDataInformationReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_04_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //機台不是RUN或IDLE,不上報
                        if (eqp.File.Status != eEQPStatus.RUN && eqp.File.Status != eEQPStatus.IDLE)
                        {
                              _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Skip APC Important Data because equipment status({0}) is not RUN or IDLE", eqp.File.Status.ToString()));
                              return;
                        }

                        //get node APCImportantDataInformationReport
                        if (!this._APCImportantDataInformationReports.ContainsKey(eqpno))
                        {
                              bool done = this._APCImportantDataInformationReports.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node APCImportantDataInformationReport!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCImportantDataInformationReport = this._APCImportantDataInformationReports[eqpno];

                        //new add
                        NodeAPCImportantDataInformationReport.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_04_E_APCImportantDataInformationReport(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_04_E_APCImportantDataInformationReport(Equipment eqp, string reason)
            {
                  try
                  {
                        //*APCImportantDataRepository Format=> List<Tuple<dcname,dctype,dcvalue>>
                        // List<Tuple<string, string, string>> apcimportantDataRepository = new List<Tuple<string, string, string>>();  // wucc modify 20150806
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcimportantDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //get node APCImportantDataInformationReport
                        string eqpno = eqp.Data.NODENO;
                        if (!this._APCImportantDataInformationReports.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node APCImportantDataInformationReport EQPNo({0})!", eqpno));
                              return;
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCImportantDataInformationReport = this._APCImportantDataInformationReports[eqpno];

                        //wucc add  20150806
                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //wucc modify 格式變更如上  20150806
                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in NodeAPCImportantDataInformationReport)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();

                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    apcimportantDataRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          //add item
                                          ///20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    }
                              }
                        }
                        NodeAPCImportantDataInformationReport.Clear();

                        //*APCImportantDataRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantData'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.APCImportanEnable)
                              Repository.Add(key, apcimportantDataRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report APCImportantDataInformationReport EQPNo({0}), reason({1})!", eqpno, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_05_E_APCNormalDataInformationReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_05_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //mark by yang 2017/6/14
                        //機台不是RUN或IDLE,不上報
                        //if (eqp.File.Status != eEQPStatus.RUN && eqp.File.Status != eEQPStatus.IDLE)
                        //{
                        //      _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        //            string.Format("Skip APC Normal Data because equipment status({0}) is not RUN or IDLE", eqp.File.Status.ToString()));
                        //      return;
                        //}

                        //get node APCNormalDataInformationReport
                        if (!this._APCNormalDataInformationReports.ContainsKey(eqpno))
                        {
                              bool done = this._APCNormalDataInformationReports.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node APCNormalDataInformationReport!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCNormalDataInformationReport = this._APCNormalDataInformationReports[eqpno];

                        //new add
                        NodeAPCNormalDataInformationReport.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_05_E_APCNormalDataInformationReport(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_05_E_APCNormalDataInformationReport(Equipment eqp, string reason)
            {
                  try
                  {
                        //*APCNormalDataRepository Format=> List<Tuple<dcname,dctype,dcvalue>>
                        // List<Tuple<string, string, string>> apcnormalDataRepository = new List<Tuple<string, string, string>>();// wucc modify 20150806
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcnormalDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //get node APCNormalDataInformationReport
                        string eqpno = eqp.Data.NODENO;
                        if (!this._APCNormalDataInformationReports.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node APCNormalDataInformationReport EQPNo({0})!", eqpno));
                              return;
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCNormalDataInformationReport = this._APCNormalDataInformationReports[eqpno];

                        //wucc add  20150806
                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List" type="L" len="3">
                        //        <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //        <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //        <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //wucc modify 格式變更如上  20150806

                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in NodeAPCNormalDataInformationReport)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();

                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    apcnormalDataRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          //add item
                                          ///20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    }
                              }
                        }
                        NodeAPCNormalDataInformationReport.Clear();

                        //*APCNormalDataRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalData'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.APCNormalEnable)
                              Repository.Add(key, apcnormalDataRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report APCNormalDataInformationReport EQPNo({0}), reason({1})!", eqpno, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_06_E_GlassProcessDataInformationReportfortestEQ(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_06_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        int cstseq = 0;
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check if controlmode offline,drop
                        //20150306 cy:不檢查Offline,交給相對應的Service去檢查,這樣才能把資料寫到BC的DB
                        //if (line.File.HostMode == eHostMode.OFFLINE) {
                        //    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        //        string.Format("MES offline,drop EQP({0}), Glass({1}), S6F3_06 SUBCD({2}), GlassProcessDataInformationReportfortestEQ!", eqp.Data.NODEID, glassid, subcd));
                        //    return;
                        //}

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job in JobEntity by GlassID({0})!", glassid));
                              job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                              if (job == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Can not find Job in JobEntity by CSTSEQ({0}) and SLOT({1})!", cstseq.ToString(), slot.ToString()));
                                    return;
                              }
                        }

                        //20150417 cy:將Job內的PhotoIsProcessed設為true,但退port時判斷是否經過SECS機台(for array)
                        job.ArraySpecial.PhotoIsProcessed = true;

                        //get node GlassProcessDataInformationReportfortestEQ
                        if (!this._GlassProcessDataInformationReportfortestEQs.ContainsKey(eqpno))
                        {
                              bool done = this._GlassProcessDataInformationReportfortestEQs.TryAdd(eqpno, new ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node GlassProcessDataInformationReportfortestEQ!");
                                    return;
                              }
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeGlassProcessDataInformationReportfortestEQ = this._GlassProcessDataInformationReportfortestEQs[eqpno];

                        //get glass GlassProcessDataInformationReportfortestEQ
                        if (!NodeGlassProcessDataInformationReportfortestEQ.ContainsKey(glassid))
                        {
                              bool done = NodeGlassProcessDataInformationReportfortestEQ.TryAdd(glassid, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Fail to add glass GlassProcessDataInformationReportfortestEQ Glass({0})!", glassid));
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> GlassProcessDataInformationReportfortestEQ = NodeGlassProcessDataInformationReportfortestEQ[glassid];

                        //new add
                        GlassProcessDataInformationReportfortestEQ.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_06_E_GlassProcessDataInformationReportfortestEQ(eqp, job, "final");
                        }
                        return;

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            private void ReportS6F3_06_E_GlassProcessDataInformationReportfortestEQ(Equipment eqp, Job job, string reason)
            {
                  try
                  {
                        //get node GlassProcessDataInformationReportfortestEQ
                        string eqpno = eqp.Data.NODENO;
                        if (!this._GlassProcessDataInformationReportfortestEQs.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node GlassProcessDataInformationReportfortestEQ EQPNo({0})!", eqpno));
                              return;
                        }
                        ConcurrentDictionary<string, List<Tuple<DateTime, XmlDocument>>> NodeGlassProcessDataInformationReportfortestEQ = this._GlassProcessDataInformationReportfortestEQs[eqpno];

                        //remove glass from GlassProcessDataInformationReportfortestEQ
                        string glassid = job.GlassChipMaskBlockID;
                        List<Tuple<DateTime, XmlDocument>> GlassProcessDataInformationReportfortestEQ = null;
                        NodeGlassProcessDataInformationReportfortestEQ.TryRemove(glassid, out GlassProcessDataInformationReportfortestEQ);
                        if (GlassProcessDataInformationReportfortestEQ == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get glass GlassProcessDataInformationReportfortestEQ EQPNo({0}), Glass({1})!", eqpno, glassid));
                              return;
                        }

                        //<array1 name="List" type="L" len="7">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <REPORTFLAG name="REPORTFLAG" type="A" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <OPERATIONID name="OPERATIONID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="9">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <UNIQUEID name="UNIQUEID" type="A" len="6" fixlen="False" />
                        //    <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //    <CARRIERID name="CARRIERID" type="A" len="10" fixlen="False" />
                        //    <PROCESS_RESULT name="PROCESS_RESULT" type="A" len="1" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <array4 name="List" type="L" len="2">
                        //        <GROUPNAME name="GROUPNAME" type="A" len="16" fixlen="False" />
                        //        <array5 name="List" type="L" len="?">
                        //          <array6 name="List" type="L" len="2">
                        //            <SITENAME name="SITENAME" type="A" len="4" fixlen="False" />
                        //            <array7 name="List" type="L" len="3">
                        //              <PDCNAME name="PDCNAME" type="A" len="30" fixlen="False" />
                        //              <PDCTYPE name="PDCTYPE" type="A" len="3" fixlen="False" />
                        //              <PDCVALUE name="PDCVALUE" type="A" len="16" fixlen="False" />
                        //            </array7>
                        //          </array6>
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //20150408 cy:修改寫法,可檢查名稱重覆
                        //IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();
                        Dictionary<string, Tuple<string, string, string, string, string>> pdata = new Dictionary<string, Tuple<string, string, string, string, string>>();
                        string tid = string.Empty;

                        //subcd 1~99 list
                        foreach (var v in GlassProcessDataInformationReportfortestEQ)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              //ProductProcessData.ITEMc itemC = null;
                              XmlNode xNode3 = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                              string len3 = xNode3.Attributes["len"].InnerText.Trim();
                              int loop3 = 0;
                              int.TryParse(len3, out loop3);
                              for (int i = 0; i < loop3; i++)
                              {
                                    string groupname = xNode3.ChildNodes[i]["GROUPNAME"].InnerText.Trim();
                                    //itemC = new ProductProcessData.ITEMc();
                                    //itemC.ITEMNAME = groupname;
                                    string len5 = xNode3.ChildNodes[i]["array5"].Attributes["len"].InnerText.Trim();
                                    int loop5 = 0;
                                    int.TryParse(len5, out loop5);
                                    for (int j = 0; j < loop5; j++)
                                    {
                                          XmlNode xNode5 = xNode3.ChildNodes[i]["array5"];
                                          string sitename = xNode5.ChildNodes[j]["SITENAME"].InnerText.Trim();
                                          string pdcname = xNode5.ChildNodes[j]["array7"]["PDCNAME"].InnerText.Trim();
                                          //20150323 cy:針對name,避開特殊符號
                                          pdcname = Regex.Replace(pdcname, @"[/| |?|/|<|>|'|\-|,]", "_"); // pdcname.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                                          string pdctype = xNode5.ChildNodes[j]["array7"]["PDCTYPE"].InnerText.Trim();
                                          string pdcvalue = xNode5.ChildNodes[j]["array7"]["PDCVALUE"].InnerText.Trim();
                                          string paname = pdcname;
                                          //20150325 cy:針對此transaction, sitename改為由"groupname_sitename_pdcname"組成
                                          //20150402 cy:登京交待,groupname或site若為空,則不加入組合.
                                          //20150421 cy:MES要求,直接上報pdcname,不做組合.
                                          //if (!string.IsNullOrEmpty(groupname) && !string.IsNullOrEmpty(sitename))
                                          //    paname = string.Format("{0}_{1}_{2}", groupname, sitename, pdcname);
                                          //else if (!string.IsNullOrEmpty(groupname) && string.IsNullOrEmpty(sitename))
                                          //    paname = string.Format("{0}_{1}}", groupname, pdcname);
                                          //else if (string.IsNullOrEmpty(groupname) && !string.IsNullOrEmpty(sitename))
                                          //    paname = string.Format("{0}_{1}", sitename, pdcname);
                                          //else
                                          //    paname = pdcname;

                                          if (pdata.ContainsKey(paname))
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    string.Format("Glass process data item duplicate. GroupName({0}), SiteName({1}), PDCName({2})", groupname, sitename, pdcname));
                                                pdata.Remove(paname);
                                          }
                                          //20150428 cy:登京要求groupname若機台報空,則填NA
                                          pdata.Add(paname, new Tuple<string, string, string, string, string>(pdcname, string.IsNullOrEmpty(groupname) ? "NA" : groupname, sitename, pdctype, pdcvalue));
                                    }
                                    //mesdic.Add(itemC);
                              }
                        }
                        //this._common.ProcessDataReport(eqp, job, mesdic, tid, false);
                        _common.ProcessDataReport(eqp, job, pdata, tid);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Report GlassProcessDataInformationReportfortestEQ EQPNo({0}), Glass({1}), reason({2})!", eqpno, glassid, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_07_E_GlassDefectCodeInformationReportfortestEQ(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_07_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //<array1 name="List" type="L" len="6">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <REPORTFLAG name="REPORTFLAG" type="A" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <OPERATIONID name="OPERATIONID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="9">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <UNIQUEID name="UNIQUEID" type="A" len="6" fixlen="False" />
                        //    <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //    <CARRIERID name="CARRIERID" type="A" len="10" fixlen="False" />
                        //    <PROCESS_RESULT name="PROCESS_RESULT" type="A" len="1" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <array4 name="List" type="L" len="4">
                        //        <PANELID name="PANELID" type="A" len="20" fixlen="False" />
                        //        <PANELPOSITION name="PANELPOSITION" type="A" len="32" fixlen="False" />
                        //        <PANEL_RESULT name="PANEL_RESULT" type="A" len="10" fixlen="False" />
                        //        <array5 name="List" type="L" len="?">
                        //          <array6 name="List" type="L" len="3">
                        //            <DEFECTCODEPOSITION name="DEFECTCODEPOSITION" type="A" len="32" fixlen="False" />
                        //            <DEFECTCODE name="DEFECTCODE" type="A" len="16" fixlen="False" />
                        //            <DEFECTCODE_RESULT name="DEFECTCODE_RESULT" type="A" len="10" fixlen="False" />
                        //          </array6>
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        int cstseq = 0;
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check if controlmode offline,drop
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("MES offline,drop EQP({0}), Glass({1}) S6F3_07 GlassDefectCodeInformationReportfortestEQ!", eqp.Data.NODEID, glassid));
                              return;
                        }

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                          string.Format("Can not find Job in JobEntity by GlassID({0})!", glassid));
                              job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                              if (job == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Can not find Job in JobEntity by CSTSEQ({0}) and SLOT({1})!", cstseq.ToString(), slot.ToString()));
                                    return;
                              }
                        }

                        XmlNode xNode3 = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                        string len3 = xNode3.Attributes["len"].InnerText.Trim();
                        int loop3 = 0;
                        int.TryParse(len3, out loop3);
                        //pnls
                        for (int i = 0; i < loop3; i++)
                        {
                              string pnlid = xNode3.ChildNodes[i]["PANELID"].InnerText.Trim();
                              string pnlpos = xNode3.ChildNodes[i]["PANELPOSITION"].InnerText.Trim();
                              string pnlresult = xNode3.ChildNodes[i]["PANEL_RESULT"].InnerText.Trim();
                              string len5 = xNode3.ChildNodes[i]["array5"].Attributes["len"].InnerText.Trim();
                              int loop5 = 0;
                              int.TryParse(len5, out loop5);
                              //defects
                              for (int j = 0; j < loop5; j++)
                              {
                                    XmlNode xNode5 = xNode3.ChildNodes[i]["array5"];
                                    string dpos = xNode5.ChildNodes[j]["DEFECTCODEPOSITION"].InnerText.Trim();
                                    string dcode = xNode5.ChildNodes[j]["DEFECTCODE"].InnerText.Trim();
                                    string dresult = xNode5.ChildNodes[j]["DEFECTCODE_RESULT"].InnerText.Trim();

                              }
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_08_E_SpecialDataInformationReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_08_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //get node SpecialDataInformationReport
                        if (!this._SpecialDataInformationReports.ContainsKey(eqpno))
                        {
                              bool done = this._SpecialDataInformationReports.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node SpecialDataInformationReport!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeSpecialDataInformationReport = this._SpecialDataInformationReports[eqpno];

                        //new add
                        NodeSpecialDataInformationReport.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_08_E_SpecialDataInformationReport(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_08_E_SpecialDataInformationReport(Equipment eqp, string reason)
            {
                  try
                  {
                        //*SpecialDataRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> specialDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //get node SpecialDataInformationReport
                        string eqpno = eqp.Data.NODENO;
                        if (!this._SpecialDataInformationReports.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node SpecialDataInformationReport EQPNo({0})!", eqpno));
                              return;
                        }
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        List<Tuple<DateTime, XmlDocument>> NodeSpecialDataInformationReport = this._SpecialDataInformationReports[eqpno];

                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List" type="L" len="3">
                        //        <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //        <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //        <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>

                        string tid = string.Empty;
                        IList<ChangeMaterialLifeReport.MATERIALc> cmlMaterials = new List<ChangeMaterialLifeReport.MATERIALc>();
                        //subcd 1~99 list
                        foreach (var v in NodeSpecialDataInformationReport)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                                    //add subeqp
                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    specialDataRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          //add item
                                          //20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                          //20150310 cy 增加特定欄位,上報特定功能
                                          #region For ChangeTargetLife
                                          if (line.Data.FABTYPE.ToUpper() == "ARRAY")
                                          {
                                                if (line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD||line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL)
                                                {
                                                      string mkey = string.Format("{0}_SPECIAL_DATA_MATERIALLIFE", line.Data.LINETYPE);

                                                      ConstantData _constantdata = ConstantManager[mkey];

                                                      if (_constantdata != null && _constantdata[dcname.Trim()].Value.ToUpper() == "TRUE")
                                                      {
                                                            cmlMaterials.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                            {
                                                                  CHAMBERID = string.Empty,
                                                                  MATERIALNAME = dcname.Trim(),
                                                                  MATERIALTYPE = "DRYPOLE",
                                                                  QUANTITY = dcvalue.Trim(),
                                                            });
                                                      }
                                                }
                                          }
                                          #endregion
                                    }
                              }
                        }
                        NodeSpecialDataInformationReport.Clear();

                        //*SpecialDataRepository key=>lineid+'_'+nodeid+'_SecsSpecialData'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.SpecialDataEnable)
                              Repository.Add(key, specialDataRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report SpecialDataInformationReport EQPNo({0}), reason({1})!", eqpno, reason));

                        #region 上報ChangeMaterialLifeReport(如果需要)
                        if (cmlMaterials.Count > 0)
                        {
                              Invoke(eServiceName.MESService, "ChangeMaterialLife", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, string.Empty, cmlMaterials });
                        }
                        #endregion

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_09_E_APCImportantDataInformationReportforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_09_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //機台不是RUN或IDLE,不上報
                        if (eqp.File.Status != eEQPStatus.RUN && eqp.File.Status != eEQPStatus.IDLE)
                        {
                              _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Skip APC Important Data because equipment status({0}) is not RUN or IDLE", eqp.File.Status.ToString()));
                              return;
                        }

                        //get node APCImportantDataInformationReportforID
                        if (!this._APCImportantDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              bool done = this._APCImportantDataInformationReportforIDs.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node APCImportantDataInformationReportforID!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCImportantDataInformationReportforID = this._APCImportantDataInformationReportforIDs[eqpno];

                        //new add
                        NodeAPCImportantDataInformationReportforID.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_09_E_APCImportantDataInformationReportforID(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_09_E_APCImportantDataInformationReportforID(Equipment eqp, string reason)
            {
                  try
                  {
                        //*APCImportantData4IDRepository Format=> List<Tuple<dcname,dctype,dcvalue>>
                        //List<Tuple<string, string, string>> apcimportantData4IDRepository = new List<Tuple<string, string, string>>();// wucc modify 20150806
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcimportantData4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //get node APCImportantDataInformationReportforID
                        string eqpno = eqp.Data.NODENO;
                        if (!this._APCImportantDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node APCImportantDataInformationReportforID EQPNo({0})!", eqpno));
                              return;
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCImportantDataInformationReportforID = this._APCImportantDataInformationReportforIDs[eqpno];

                        //wucc add  20150806
                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List" type="L" len="3">
                        //        <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //        <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //        <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //wucc modify 格式變更如上  20150806
                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in NodeAPCImportantDataInformationReportforID)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                            //  IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                              IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqpno);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();

                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    apcimportantData4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          string dcname = GetDCNAMEForAPC(dataFormats, dcid);
                                          //add item
                                          ///20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})",dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    }
                              }
                        }
                        NodeAPCImportantDataInformationReportforID.Clear();

                        //*APCImportantData4IDRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantData4ID'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.APCImportanEnableForID)
                              Repository.Add(key, apcimportantData4IDRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report APCImportantDataInformationReportforID EQPNo({0}), reason({1})!", eqpno, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_10_E_APCNormalDataInformationReportforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_10_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //mark by yang 2017/6/14 
                        //機台不是RUN或IDLE,不上報
                        //if (eqp.File.Status != eEQPStatus.RUN && eqp.File.Status != eEQPStatus.IDLE)
                        //{
                        //      _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                        //            string.Format("Skip APC Normal Data because equipment status({0}) is not RUN or IDLE", eqp.File.Status.ToString()));
                        //      return;
                        //}

                        //get node APCNormalDataInformationReportforID
                        if (!this._APCNormalDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              bool done = this._APCNormalDataInformationReportforIDs.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node APCNormalDataInformationReportforID!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCNormalDataInformationReportforID = this._APCNormalDataInformationReportforIDs[eqpno];

                        //new add
                        NodeAPCNormalDataInformationReportforID.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_10_E_APCNormalDataInformationReportforID(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_10_E_APCNormalDataInformationReportforID(Equipment eqp, string reason)
            {
                  try
                  {
                        //*APCNormalData4IDRepository Format=> List<Tuple<dcname,dctype,dcvalue>>
                        //  List<Tuple<string, string, string>> apcnormalData4IDRepository = new List<Tuple<string, string, string>>();// wucc modify 20150806
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcnormalData4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //get node APCNormalDataInformationReportforID
                        string eqpno = eqp.Data.NODENO;
                        if (!this._APCNormalDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node APCNormalDataInformationReportforID EQPNo({0})!", eqpno));
                              return;
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeAPCNormalDataInformationReportforID = this._APCNormalDataInformationReportforIDs[eqpno];

                        //wucc add  20150806
                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List" type="L" len="3">
                        //        <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //        <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //        <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //wucc modify 格式變更如上  20150806
                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in NodeAPCNormalDataInformationReportforID)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                            //  IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                              IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqpno);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();

                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    apcnormalData4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          string dcname = GetDCNAMEForAPC(dataFormats, dcid);
                                          //add item
                                          ///20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})",dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    }
                              }
                        }
                        NodeAPCNormalDataInformationReportforID.Clear();

                        //*APCNormalData4IDRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalData4ID'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.APCNormalEnableForID)
                              Repository.Add(key, apcnormalData4IDRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report APCNormalDataInformationReportforID EQPNo({0}), reason({1})!", eqpno, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F3_11_E_SpecialDataInformationReportforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F3_11_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS6F4_H_DiscreteVariableDataAcknowledge(eqpno, agent, tid, sysbytes);

                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //get node SpecialDataInformationReportforID
                        if (!this._SpecialDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              bool done = this._SpecialDataInformationReportforIDs.TryAdd(eqpno, new List<Tuple<DateTime, XmlDocument>>());
                              if (!done)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Fail to add node SpecialDataInformationReportforID!");
                                    return;
                              }
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeSpecialDataInformationReportforID = this._SpecialDataInformationReportforIDs[eqpno];

                        //new add
                        NodeSpecialDataInformationReportforID.Add(Tuple.Create(DateTime.Now, recvTrx));

                        //check if report now
                        if (subcd == "99")
                        {
                              ReportS6F3_11_E_SpecialDataInformationReportforID(eqp, "final");
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ReportS6F3_11_E_SpecialDataInformationReportforID(Equipment eqp, string reason)
            {
                  try
                  {
                        //*specialData4IDRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();


                        //get node SpecialDataInformationReportforID
                        string eqpno = eqp.Data.NODENO;
                        if (!this._SpecialDataInformationReportforIDs.ContainsKey(eqpno))
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("fail to get node SpecialDataInformationReportforID EQPNo({0})!", eqpno));
                              return;
                        }
                        List<Tuple<DateTime, XmlDocument>> NodeSpecialDataInformationReportforID = this._SpecialDataInformationReportforIDs[eqpno];

                        //<array1 name="List" type="L" len="5">
                        //  <DVID name="DVID" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="3">
                        //      <DCID name="DCID" type="A" len="30" fixlen="False" />
                        //      <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //      <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>

                        string tid = string.Empty;
                        //subcd 1~99 list
                        foreach (var v in NodeSpecialDataInformationReportforID)
                        {
                              XmlDocument recvTrx = v.Item2;
                              tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len2 = xNode2.Attributes["len"].InnerText.Trim();
                              int loop2 = 0;
                              int.TryParse(len2, out loop2);
                             // IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                              IList<EnergyVisualizationData> dataFormats = ObjectManager.EnergyVisualizationManager.GetEnergyVisualizationProfile(eqpno);
                              for (int i = 0; i < loop2; i++)
                              {
                                    string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                                    //add subeqp
                                    List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                                    specialData4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                                    XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                                    string len4 = xNode4.Attributes["len"].InnerText.Trim();
                                    int loop4 = 0;
                                    int.TryParse(len4, out loop4);
                                    for (int j = 0; j < loop4; j++)
                                    {
                                          string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                          string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                          string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                          string dcname = GetDCNAMEForEnergy(dataFormats, dcid);
                                          //add item
                                          //20150604 cy:增加檢查資料不能重覆
                                          int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                          if (dupIndex > -1)
                                          {
                                                _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})", dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                                itemList.RemoveAt(dupIndex);
                                          }
                                          //itemList.Add(Tuple.Create(dcid, dctype, dcvalue));
                                          itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    }
                              }
                        }
                        NodeSpecialDataInformationReportforID.Clear();

                        //*SpecialData4IDRepository key=>lineid+'_'+nodeid+'_SecsSpecialData4ID'
                        //20160310 cy: 格式已統一,所以統一放同一個倉庫, 讓APCDataService取
                        string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.SpecialDataEnableForID)
                              Repository.Add(key, specialData4IDRepository);
                        else
                              Repository.Remove(key);

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Report SpecialDataInformationReportforID EQPNo({0}), reason({1})!", eqpno, reason));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F11_01_E_ControlModeChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_01_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string timerIdOfl = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                        string timerIdOnl = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);

                        //Chang Node CIM ON
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        if (eqpid != eqp.Data.NODEID)
                        {
                              string msg = string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent);
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              //20150317 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                              #region check opi operate and reply message
                              if (_timerManager.IsAliveTimer(timerIdOnl))
                              {
                                    _timerManager.TerminateTimer(timerIdOnl);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] Online request Fail. {1}", eqpno, msg) });
                              }
                              if (_timerManager.IsAliveTimer(timerIdOfl))
                              {
                                    _timerManager.TerminateTimer(timerIdOfl);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] Offline request Fail. {1}", eqpno, msg) });
                              }
                              #endregion
                              return;
                        }
                        //replay secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                        string mode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CTRLMODE"].InnerText.Trim();
                        string handleMsg = string.Empty;
                        if (HandleControlMode(eqp, mode, out handleMsg))
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);
                        else
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);

                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //20150616 cy:增加record to history的功能
                        ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);

                        //20141023 cy:Report to OPI
                        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });

                        //20161204 yang:CIM Mode要报给MES
                        if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                            Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqpid, eqp.File.CIMMode });

                        //20150317 cy:存在online/offline request計時器,表示為OPI操作,要回覆訊息.
                        #region check opi operate and reply message
                        if (_timerManager.IsAliveTimer(timerIdOnl))
                        {
                              _timerManager.TerminateTimer(timerIdOnl);
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, handleMsg) });
                        }
                        if (_timerManager.IsAliveTimer(timerIdOfl))
                        {
                              _timerManager.TerminateTimer(timerIdOfl);
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[4] { tid, string.Empty, "Receive S6F11", string.Format("[EQUIPMENT={0}] {1}", eqpno, handleMsg) });
                        }
                        #endregion

                        //add by yang 20161125 机台切到cim on,就建立通讯(energy & apc) 
                        if (eqp.File.CIMMode==eBitResult.ON && eqp.Data.REPORTMODE.Contains("HSMS_PLC") && eqp.Data.APCREPORT.Equals("Y"))
                        {
                            //有存值就用档案里的值,没有就给default
                            string apcinterval = (eqp.File.APCNormalIntervalMS == 0) ? "1000" : eqp.File.APCNormalIntervalMS.ToString();
                            string apcintervalForID = (eqp.File.APCNormalIntervalMSForID == 0) ? "1000" : eqp.File.APCNormalIntervalMSForID.ToString();
                            string energyinterval = (eqp.File.SpecialDataIntervalMS == 0) ? "3600000" : eqp.File.SpecialDataIntervalMS.ToString();
                            string energyintervalForID = (eqp.File.SpecialDataIntervalMSForID == 0) ? "3600000" : eqp.File.SpecialDataIntervalMSForID.ToString();
                            string trxid = base.CreateTrxID();

                            //这边不用帮判断
                            #region[DataSetCommandforID]
                           // if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_10") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S1F5", "10", "0", energyintervalForID, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_07") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S1F5", "07", "0", apcintervalForID, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_08") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S1F5", "08", "0", apcintervalForID, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_09") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S6F3", "09", "0", apcintervalForID, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_10") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S6F3", "10", "0", apcintervalForID, "AutoRequest", trxid);
                         //   if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_11") != null)
                                TS2F21_H_DataSetCommandforID(eqpno, eqpid, "S6F3", "11", "0", energyintervalForID, "AutoRequest", trxid);
                            #endregion

                            #region[DataSetCommand]
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_02") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S1F5", "02", "0", apcinterval, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_04") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S1F5", "04", "0", apcinterval, "AutoRequest", trxid);
                         //   if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S1F5_06") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S1F5", "06", "0", energyinterval, "AutoRequest", trxid);
                         //   if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_04") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S6F3", "04", "0", apcinterval, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_05") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S6F3", "05", "0", apcinterval, "AutoRequest", trxid);
                          //  if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "S6F3_08") != null)
                                TS2F19_H_DataSetCommand(eqpno, eqpid, "S6F3", "08", "0", energyinterval, "AutoRequest", trxid);
                            #endregion
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }

            public bool EquipmentRunModeCheck_Common(Line line, Equipment eqp, List<Tuple<Unit, string, string>> unitRpt, string eqpmode, out string descript, out string smsg)
            {
                  descript = string.Empty;
                  smsg = string.Empty;
                  try
                  {
                        //Equipment Run Mode是MIX, Line IndexOperaMode必需也要MIX
                        if (eqpmode == "MIX" && line.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE)
                        {
                              smsg = "INDEXER RUN MODE NOT MIX";
                              descript = "Equipment run mode check error: Change rum mode to MIX but Indexer run mode NOT MIX";
                              _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqp.Data.NODENO, true, "", descript); //just log not match log cc.kuang 2016/03/02
                              //return false; 
                        }
                        //Unit有不同Mode,必需報MIX,否則報NORN
                        int subModeCount = unitRpt.Select(m => m.Item2).Distinct().Count();
                        if (subModeCount == 1)
                        {
                              if (!eqpmode.Equals("NORN"))
                              {
                                    smsg = "EQP MODE NOT NORN";
                                    descript = "Equipment run mode check error: Units have same mode but main equipment is not NORN mode.";
                                    return false;
                              }
                        }
                        else
                        {
                              if (!eqpmode.Equals("MIX"))
                              {
                                    smsg = "EQP MODE NOT MIX";
                                    descript = "Equipment run mode check error: Units have different mode but main equipment is not MIX mode.";
                                    return false;
                              }
                        }
                        return true;
                  }
                  catch (Exception ex)
                  {
                        throw ex;
                  }
            }
            public void S6F11_02_E_EquipmentModeChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_02_E");
                  #region Handle Logic
                  try
                  {
                        string timerId = string.Empty;
                        string timerIdBC = string.Empty;

                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="3">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //    <array3 name="List (number of sub equipments)" type="L" len="?">
                        //      <array4 name="List" type="L" len="2">
                        //        <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //        <SUBEQPMODE name="SUBEQPMODE" type="A" len="4" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body

                        //t3 只檢查Submode全一樣, EqMode就Normal, 否則就MIX  20150923 cy
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string newOrgMode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQPMODE"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get rpt units	
                        List<ChamberRunModeChanged.CHAMBERc> cbmode = new List<ChamberRunModeChanged.CHAMBERc>();
                        //濾掉不是chamber的unit CHAMBER
                        List<Tuple<Unit, string, string>> unitRpt = new List<Tuple<Unit, string, string>>();
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              string subeqpid = xNode.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              string subeqporgmode = xNode.ChildNodes[i]["SUBEQPMODE"].InnerText.Trim();
                              string subeqpmode = ConstantManager["CSOT_SECS_RUNMODE"][subeqporgmode].Value;
                              Unit unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                              if (unit == null)
                              {
                                    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Can not find Unit ID({0}) in UnitEntity!", subeqpid));
                                    continue;
                              }
                              if (unit.Data.UNITTYPE == null || !unit.Data.UNITTYPE.Equals("CHAMBER"))
                                    continue;
                              if (unit.Data.UNITATTRIBUTE == "VIRTUAL")
                                    continue;
                              unitRpt.Add(Tuple.Create(unit, subeqporgmode, subeqpmode));
                        }

                        //20150207 cy:要排除PASS跟APAS，因為在這2種mode下，Unit Mode也是可能會不同，所以在這2種mode下，不用檢查Unit Mode
                        if (newOrgMode != "PASS" && newOrgMode != "APAS" && unitRpt.Count > 0)
                        {
                              string lmsg = string.Empty;
                              string smsg = string.Empty;
                              if (!EquipmentRunModeCheck_Common(line, eqp, unitRpt, newOrgMode, out lmsg, out smsg))
                              {
                                    //reply secondary
                                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                                    smsg = "Run Mode Check NG: " + smsg;
                                    //send cim msg
                                    TS10F3_H_TerminalDisplaySingle(eqpno, eqpid, smsg, tid, string.Empty);
                                    //Invoke(eServiceName.EvisorService, "BC_System_Alarm",
                                    //            new object[] { eqp.Data.LINEID, string.Format("{0} {1}", eqp.Data.NODEID, smsg) });
                                    Invoke(eServiceName.EvisorService, "AppAlarmReport",
                                                  new object[] { eqp.Data.LINEID,"ALARM", string.Format("{0} {1}", eqp.Data.NODEID, smsg) });
                                    //check if in onlinescenario process(timer exists)
                                    timerId = string.Format("{0}_{1}", eqpno, "SecsOnlineScenario");
                                    timerIdBC = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                                    if (_timerManager.IsAliveTimer(timerId) || _timerManager.IsAliveTimer(timerIdBC))
                                    {
                                          //terminate onlinescenario						
                                          _timerManager.TerminateTimer(timerId);
                                          _timerManager.TerminateTimer(timerIdBC);
                                          //request offline
                                          TS1F15_H_OfflineRequest(eqpno, agent, string.Empty, tid);
                                          _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Equipment run mode check ng and request offline");
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                          new object[4] { tid, eqp.Data.LINEID, "Run Mode Check NG", smsg });

                                    _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, lmsg);

                                    return;
                              }
                        }

                        #region [ update mode ]
                        //get & set eqp mode
                        string oldMode = eqp.File.EquipmentRunMode;
                        lock (eqp)
                        {
                              eqp.File.EquipmentRunMode = ConstantManager["CSOT_SECS_RUNMODE"][newOrgMode].Value;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //20150616 cy:增加record to history的功能
                        if (oldMode != eqp.File.EquipmentRunMode)
                            ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);

                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("EquipmentMode Change from ({0}) to ({1}).", oldMode, eqp.File.EquipmentRunMode));

                        //get & set current unit mode
                        for (int i = 0; i < unitRpt.Count; i++)
                        {
                              Unit unit = unitRpt[i].Item1;

                              string subeqpMode = unitRpt[i].Item3;
                              string oldsubeqpmode = unit.File.RunMode;
                              lock (unit)
                              {
                                    unit.File.RunMode = subeqpMode;
                              }
                              ObjectManager.UnitManager.EnqueueSave(unit.File);

                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("SUBEQUIPMENT({0}), current SUBEQPMODE({1}).", unit.Data.UNITID, subeqpMode));

                              ChamberRunModeChanged.CHAMBERc cb = new ChamberRunModeChanged.CHAMBERc();
                              cb.CHAMBERNAME = unit.Data.UNITID;
                              cb.CHAMBERRUNMODE = subeqpMode;
                              cbmode.Add(cb);
                        }

                        //20141023 cy:Report to OPI
                        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });

                        //呼叫LineService.CheckLineRunMode
                        Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { eqp.Data.LINEID });
                        Invoke(eServiceName.UIService, "LineStatusReport", new object[] { tid, line });
                        #endregion

                        #region [ force clean out ]
                        //check if already triggered
                        if (oldMode != ConstantManager["CSOT_SECS_RUNMODE"][newOrgMode].Value)
                        {
                              switch (eqp.Data.NODEATTRIBUTE)
                              {
                                    case "DNS":
                                          //1.DNS發mode為"Abnormal Pass Mode"時，BC要ON Abnormal Force Clean out。
                                          //2.DNS發mode為"Pass Mode"時，BC要ON Force Clean out。
                                          switch (newOrgMode)
                                          {
                                                case "PASS":
                                                      if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                                      {
                                                            Invoke(eServiceName.ArraySpecialService, "ForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                                      }
                                                      break;
                                                case "APAS":
                                                      if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                                      {
                                                            Invoke(eServiceName.ArraySpecialService, "AbnormalForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                                      }
                                                      break;
                                          }
                                          break;
                              }
                        }
                        #endregion

                        #region [ MachineModeChangeRequest ChamberRunModeChanged ]
                        if (eqp.Data.NODEATTRIBUTE == "CVD" || eqp.Data.NODEATTRIBUTE == "DRY")
                        {
                              if (line.File.HostMode == eHostMode.OFFLINE)
                              {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} SEND MES, BUT OFF LINE LINENAME({1}).",
                                            eqp.Data.LINEID, tid, MethodBase.GetCurrentMethod().Name));
                                    if (ParameterManager["OFFLINEREPLYEQP"].GetBoolean())
                                    {
                                          TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack
                                          //結束BCS主動要求online的部份
                                          timerIdBC = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                                          if (_timerManager.IsAliveTimer(timerIdBC))
                                                _timerManager.TerminateTimer(timerIdBC);
                                    }
                                    else
                                    {
                                          TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack NG
                                          string msg = string.Format("Host Offline and BCS setting is reply NG automatically");
                                          TS10F3_H_TerminalDisplaySingle(eqpno, eqpid, string.Format("Equipment run Mode change err - Host Offline"), tid, string.Empty);
                                          //Invoke(eServiceName.EvisorService, "BC_System_Alarm",
                                          //      new object[] { eqp.Data.LINEID, string.Format("{0} {1}", eqp.Data.NODEID, msg) });
                                          Invoke(eServiceName.EvisorService, "AppAlarmReport",
                                                  new object[] { eqp.Data.LINEID,"ALARM", string.Format("{0} {1}", eqp.Data.NODEID, msg) });  
                                        //check if in onlinescenario process(timer exists)
                                          timerId = string.Format("{0}_{1}", eqpno, "SecsOnlineScenario");  //由機台端切的
                                          timerIdBC = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                                          if (_timerManager.IsAliveTimer(timerId) || _timerManager.IsAliveTimer(timerIdBC))
                                          {
                                                //terminate onlinescenario						
                                                _timerManager.TerminateTimer(timerId);
                                                _timerManager.TerminateTimer(timerIdBC);
                                                //request offline
                                                TS1F15_H_OfflineRequest(eqpno, agent, string.Empty, tid);
                                                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    string.Format("Equipment {0} mode changed but Host Offline and request EQP offline", newOrgMode));
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                      new object[4] { tid, eqp.Data.LINEID, "EQP Mode Check Error", msg + " Can not online." });
                                          }
                                          else
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                            new object[4] { tid, eqp.Data.LINEID, "EQP Mode Check Error", msg });
                                    }
                                    return;
                              }
                              //CVD、DRY發的需上報MES MachineModeChangeRequest
                              Invoke(eServiceName.MESService, "MachineModeChangeRequest", new object[] { tid, eqp.Data.LINEID });

                              //wait mes ack
                              timerId = string.Format("SecsEquipmentModeChange_{0}_{1}_MachineModeChangeRequest", eqp.Data.NODENO, tid);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }
                              //create wait timer
                              _timerManager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                                  new System.Timers.ElapsedEventHandler(MES_MachineModeChangeReplyTimeOut),
                                    Tuple.Create(sysbytes, string.Empty, line.File.LineOperMode, cbmode));

                              //20151216 cy: MES要求,不能同時報,要等MachineModeChangeReply後再報
                              //ChamberRunModeChanged
                              //if (cbmode.Count > 0)
                              //{
                              //      Invoke(eServiceName.MESService, "ChamberRunModeChanged",
                              //            new object[] { tid, eqp.Data.LINEID, line.File.LineOperMode, eqp.Data.NODEID, cbmode });
                              //}
                        }
                        else
                        {
                              //reply secondary
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok
                        }
                        #endregion

                        //結束BCS主動要求online的部份
                        timerIdBC = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                        if (_timerManager.IsAliveTimer(timerIdBC))
                              _timerManager.TerminateTimer(timerIdBC);

                        
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            private void Add2SetsID(string eqpno, string trid,
                string func, string subfunc, string enable, string frequence,
                List<Tuple<string, string, string, string, List<string>>> sets)
            {
                  List<SECSVARIABLEDATA> data = ObjectManager.EquipmentManager.GetVariableData(eqpno, trid);
                  if (data == null)
                  {
                        return;
                  }
                  if (data.Count <= 0)
                  {
                        return;
                  }
                  List<string> names = new List<string>();
                  foreach (SECSVARIABLEDATA sv in data.OrderBy(s=>s.ITEMID))  //modify by yang 2016/12/02 send list order by SVID
                  {
                        if (sv.ITEMSET == "1")
                        {
                              if (!string.IsNullOrEmpty(sv.ITEMID.Trim()))
                                    names.Add(sv.ITEMID);
                        }
                  }
                  sets.Add(Tuple.Create(func, subfunc, enable, frequence, names));
            }
            private void Add2SetsName(string eqpno, string trid,
                string func, string subfunc, string enable, string frequence,
                List<Tuple<string, string, string, string, List<string>>> sets)
            {
                  List<SECSVARIABLEDATA> data = ObjectManager.EquipmentManager.GetVariableData(eqpno, trid);
                  if (data == null)
                  {
                        return;
                  }
                  if (data.Count <= 0)
                  {
                        return;
                  }
                  List<string> names = new List<string>();
                  foreach (SECSVARIABLEDATA sv in data.OrderBy(s => s.ITEMNAME))  //modify by yang 2016/12/02 send list order by ItemName
                  {
                        if (sv.ITEMSET == "1")
                        {
                              names.Add(sv.ITEMNAME);
                        }
                  }
                  sets.Add(Tuple.Create(func, subfunc, enable, frequence, names));
            }
            //timeout handler
            private void MES_MachineModeChangeReplyTimeOut(object subject, System.Timers.ElapsedEventArgs e)
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

                  Tuple<string, string, string, List<ChamberRunModeChanged.CHAMBERc>> tuple = timer.State as Tuple<string, string, string, List<ChamberRunModeChanged.CHAMBERc>>;
                  if (tuple != null)
                  {
                        string systembyte = tuple.Item1;

                        //mes timeout,ack eq ng
                        TS6F12_H_EventReportAcknowledge(eqpno, eqp.Data.NODEID, tid, systembyte, 1); //ack ng

                        TS10F3_H_TerminalDisplaySingle(eqpno, eqp.Data.NODEID, "MES MachineModeChangeReply TimeOut.", tid, string.Empty);
                  }

                  Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                               new object[4] { tid, eqp.Data.LINEID, "EQP Mode Check Error", "MES MachineModeChangeReply TimeOut." });
            }
            public void S6F11_03_E_EquipmentStatusChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_03_E");
                  #region Handle Logic
                  //TODO:Logic handle.
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        //Get Node Object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                        //20150615 cy:避免非同步資料被更新,將整個eqp做Lock.
                        lock (eqp)
                        {
                              #region [Check control mode]
                              string mode = recvTrx["secs"]["message"]["body"]["array1"]["CTRLMODE"].InnerText.Trim();
                              string handleMsg = string.Empty;
                              if (HandleControlMode(eqp, mode, out handleMsg))
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);
                              else
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);

                              #endregion
                              //20150319 cy:改成只針對非CVD,DRY,DNS機台才做更新
                              if (eqp.Data.NODEATTRIBUTE != "CVD" && eqp.Data.NODEATTRIBUTE != "DRY" && eqp.Data.NODEATTRIBUTE != "DNS")
                              {
                                    #region [Check equipment mode]
                                    string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                                    if (eqp.File.EquipmentRunMode != ConstantManager["CSOT_SECS_RUNMODE"][eqpmode].Value)
                                    {
                                          string oldMode = eqp.File.EquipmentRunMode;
                                          eqp.File.EquipmentRunMode = ConstantManager["CSOT_SECS_RUNMODE"][eqpmode].Value;

                                          _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              string.Format("EquipmentMode Change from ({0}) to ({1}).", oldMode, eqp.File.EquipmentRunMode));

                                          //呼叫LineService.CheckLineRunMode
                                          Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { eqp.Data.LINEID });
                                          Invoke(eServiceName.UIService, "LineStatusReport", new object[] { tid, line });

                                          //if (eqp.Data.NODEATTRIBUTE == "CVD" || eqp.Data.NODEATTRIBUTE == "DRY")
                                          //{
                                          //    //CVD、DRY發的需上報MES MachineModeChangeRequest
                                          //    Invoke(eServiceName.MESService, "MachineModeChangeRequest", new object[] { tid, eqp.Data.LINEID });
                                          //}
                                          //else if (eqp.Data.NODEATTRIBUTE == "DNS")
                                          //{
                                          //    //1.DNS發mode為"Abnormal Pass Mode"時，BC要ON Abnormal Force Clean out。
                                          //    //2.DNS發mode為"Pass Mode"時，BC要ON Force Clean out。
                                          //    switch (eqpmode)
                                          //    {
                                          //        case "PASS":
                                          //            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                          //            {
                                          //                Invoke(eServiceName.ArraySpecialService, "ForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                          //            }
                                          //            break;
                                          //        case "APAS":
                                          //            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                          //            {
                                          //                Invoke(eServiceName.ArraySpecialService, "AbnormalForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                          //            }
                                          //            break;
                                          //    }
                                          //}
                                    }
                                    #endregion
                              }
                              #region [Set equipment status:邏輯在HandleEquipmentStatus處理]
                              string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                              //GOTO [ MachineStateChanged ]
                              #endregion
                              #region [Force Clean Out (Recovery)]
                              string recovery = recvTrx["secs"]["message"]["body"]["array1"]["RECOVERY"].InnerText.Trim();
                              _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Equipment Recovery({0}:{1})", recovery, ConvertRecoveryMode(recovery)));
                              #endregion
                              #region [Set Iniline Mode:更新變數,記Log]
                              string inlineMode = recvTrx["secs"]["message"]["body"]["array1"]["INLINE"].InnerText.Trim();
                              eBitResult preUpInlineMode = eqp.File.UpstreamInlineMode;
                              eBitResult preDoInlineMode = eqp.File.DownstreamInlineMode;
                              switch (inlineMode)
                              {
                                    case "0":
                                          eqp.File.UpstreamInlineMode = eBitResult.OFF;
                                          eqp.File.DownstreamInlineMode = eBitResult.OFF;
                                          break;
                                    case "1":
                                          eqp.File.UpstreamInlineMode = eBitResult.ON;
                                          eqp.File.DownstreamInlineMode = eBitResult.ON;
                                          break;
                                    case "2":
                                          eqp.File.UpstreamInlineMode = eBitResult.OFF;
                                          eqp.File.DownstreamInlineMode = eBitResult.ON;
                                          break;
                                    case "3":
                                          eqp.File.UpstreamInlineMode = eBitResult.ON;
                                          eqp.File.DownstreamInlineMode = eBitResult.OFF;
                                          break;
                              }
                              if (preUpInlineMode != eqp.File.UpstreamInlineMode || preDoInlineMode != eqp.File.DownstreamInlineMode)
                              {
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Upstream Inline Mode({0}->{1}), Downstream Inline Mode({2}->{3}).",
                                                preUpInlineMode, eqp.File.UpstreamInlineMode.ToString(), preDoInlineMode, eqp.File.DownstreamInlineMode.ToString()));
                              }
                              #endregion
                              #region [PPID:記Log]
                              string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();
                              if (eqp.File.CurrentRecipeID != ppid)
                              {
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                           string.Format("Current Recipe ID change from ({0}) to ({1})", eqp.File.CurrentRecipeID, ppid));
                                    eqp.File.CurrentRecipeID = ppid;
                              }
                              #endregion
                              #region [Glass Count:更新變數]
                              int glassCount = 0;
                              if (!int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["GLASS_COUNT"].InnerText.Trim(), out glassCount))
                              {
                                    _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Can not convert glass count value. GLASS_COUNT({0})", recvTrx["secs"]["message"]["body"]["array1"]["GLASS_COUNT"].InnerText.Trim()));
                              }
                              //20150526 cy:由於DNS在這的Count不準,所以不更新,以CEID 15的為主
                              if (eqp.Data.NODEATTRIBUTE != "DNS")
                              {
                                    eqp.File.TotalTFTJobCount = glassCount;
                              }
                              #endregion
                              #region [Alarm:存在值就記Log]
                              string alarmID = recvTrx["secs"]["message"]["body"]["array1"]["ALID"].InnerText.Trim();
                              string alarmTX = recvTrx["secs"]["message"]["body"]["array1"]["ALTX"].InnerText.Trim();
                              if (!string.IsNullOrEmpty(alarmID) || !string.IsNullOrEmpty(alarmTX))
                              {
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("AlarmID({0}), AlarmText({1}).", alarmID, alarmTX));
                              }
                              #endregion

                              //20141227 cy modify:直接存檔報變化
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                              #region 處理SubEquipment
                              if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                              {
                                    XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                                    Unit unit = null;
                                    string unitid = string.Empty;
                                    while (xNode != null)
                                    {

                                          unitid = xNode["SUBEQUIPMENTID"].InnerText.Trim();
                                          unit = ObjectManager.UnitManager.GetUnit(unitid);
                                          if (unit == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Can not find Unit ID({0}) in UnitEntity!", unitid));
                                                xNode = xNode.NextSibling;
                                                continue;
                                          }
                                          else if (unit.Data.UNITATTRIBUTE == "VIRTUAL")
                                          {
                                                xNode = xNode.NextSibling;
                                                continue;
                                          }

                                          #region [Set sub-equipment status]
                                          string subEqpst = xNode["EQPST"].InnerText.Trim();
                                          #endregion
                                          #region [Set SubEquipment PPID]
                                          ppid = xNode["PPID"].InnerText.Trim();
                                          #endregion
                                          #region [SubEquipment Glass Count]
                                          glassCount = 0;
                                          if (!int.TryParse(xNode["GLASS_COUNT"].InnerText.Trim(), out glassCount))
                                          {
                                                _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    string.Format("Can not convert unit glass count value. UnitID({0}), GLASS_COUNT({1})", unit.Data.UNITID, xNode["GLASS_COUNT"].InnerText.Trim()));
                                          }
                                          unit.File.TFTProductCount = glassCount;
                                          #endregion
                                          #region [SubEquipment Alarm]
                                          string subAlarmID = xNode["ALID"].InnerText.Trim();
                                          string subAlarmTX = xNode["ALTX"].InnerText.Trim();
                                          #endregion
                                          recovery = xNode["RECOVERY"].InnerText.Trim();
                                          lock (unit)
                                          {
                                                unit.File.PreStatus = unit.File.Status;
                                                unit.File.Status = ConvertCsotStatus(subEqpst);
                                                unit.File.PreMesStatus = unit.File.MESStatus;
                                                unit.File.MESStatus = _common.ConvertMesStatus(unit.File.Status);
                                                unit.File.CurrentAlarmCode = subAlarmID;
                                          }
                                          ObjectManager.UnitManager.EnqueueSave(unit.File);

                                          xNode = xNode.NextSibling;
                                    }
                                    xNode = null;
                              }
                              #endregion

                              #region [ MachineStateChanged ]
                              //20150414 cy:直接叫用HandleEquipmentStatus就好,這裏不看eqp是否變化,避免unit有變化卻沒上報
                              eqp.File.PreStatus = eqp.File.Status;
                              eqp.File.Status = ConvertCsotStatus(eqpst);
                              eqp.File.PreMesStatus = eqp.File.MESStatus;
                              eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                              //eqp.File.CurrentRecipeID = ppid;
                              eqp.File.CurrentAlarmCode = alarmID;
                              _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                              #endregion
                        }

                        #region [ online data Set (S2F19 or S2F21) ]
                        //check if in onlinescenario process(timer exists)
                        string timerId = string.Format("{0}_{1}", eqpno, "SecsOnlineScenario");
                        if (_timerManager.IsAliveTimer(timerId))
                        {
                              //terminate onlinescenario						
                              _timerManager.TerminateTimer(timerId);

                              //Send S2F19/S2F21
                              #region [ dataset ]
                              //20150211 cy:修改間隔時間設定
                              //20150603 cy:修改若時間設定為0就不發送S6F3的,不做預設時間
                              //if (eqp.Data.NODEATTRIBUTE.Equals("DRY"))   //DRY ICD要用S2F21,
                              if (line.Data.LINETYPE.Contains("DRY_ICD"))   // add by qiumin 20180226 ,DRY TEL USE s2f19
                              {
                                    TS2F23_H_DataItemMappingTableRequest(eqpno, agent, "ONLINE", tid);
                                    //t3 DRY_ICD支援S2F21的
                                    List<Tuple<string, string, string, string, List<string>>> s2f21Sets = new List<Tuple<string, string, string, string, List<string>>>();
                                    //ObjectManager.EquipmentManager.ReloadSECSVariableDataByEqpNo(eqpno);
                                    Add2SetsID(eqpno, "S1F5_07", "S1F5", "07", "0", eqp.File.APCImportanIntervalMSForID.ToString(), s2f21Sets);
                                    Add2SetsID(eqpno, "S1F5_08", "S1F5", "08", "0", eqp.File.APCNormalIntervalMSForID.ToString(), s2f21Sets);
                                    Add2SetsID(eqpno, "S1F5_10", "S1F5", "10", "0", eqp.File.SpecialDataIntervalMSForID.ToString(), s2f21Sets);
                                    if (eqp.File.APCNormalIntervalMSForID != 0)
                                          Add2SetsID(eqpno, "S6F3_09", "S6F3", "09", "0", eqp.File.APCImportanIntervalMSForID.ToString(), s2f21Sets); ;
                                    if (eqp.File.APCNormalIntervalMSForID != 0)
                                          Add2SetsID(eqpno, "S6F3_10", "S6F3", "10", "0", eqp.File.APCNormalIntervalMSForID.ToString(), s2f21Sets);
                                    if (eqp.File.SpecialDataIntervalMSForID != 0)
                                          Add2SetsID(eqpno, "S6F3_11", "S6F3", "11", "0", eqp.File.SpecialDataIntervalMSForID.ToString(), s2f21Sets);
                                    if (s2f21Sets.Count > 0)
                                    {
                                          TS2F21_H_DataSetCommandforID(eqpno, agent, s2f21Sets, string.Empty, tid);
                                    }
                              }
                              else
                              {
                                    List<Tuple<string, string, string, string, List<string>>> s2f19Sets = new List<Tuple<string, string, string, string, List<string>>>();
                                   // ObjectManager.EquipmentManager.ReloadSECSVariableDataByEqpNo(eqpno); 
                                  //yang modify 20161130 机台cim mode change 去send
                                    Add2SetsName(eqpno, "S1F5_02", "S1F5", "02", "0", eqp.File.APCImportanIntervalMS.ToString(), s2f19Sets);
                                    Add2SetsName(eqpno, "S1F5_04", "S1F5", "04", "0", eqp.File.APCNormalIntervalMS.ToString(), s2f19Sets);
                                    Add2SetsName(eqpno, "S1F5_06", "S1F5", "06", "0", eqp.File.SpecialDataIntervalMS.ToString(), s2f19Sets);
                                    if (eqp.File.APCImportanIntervalMS != 0)
                                          Add2SetsName(eqpno, "S6F3_04", "S6F3", "04", "0", eqp.File.APCImportanIntervalMS.ToString(), s2f19Sets);
                                    if (eqp.File.APCNormalIntervalMS != 0)
                                          Add2SetsName(eqpno, "S6F3_05", "S6F3", "05", "0", eqp.File.APCNormalIntervalMS.ToString(), s2f19Sets);
                                    if (eqp.File.SpecialDataIntervalMS != 0)
                                          Add2SetsName(eqpno, "S6F3_08", "S6F3", "08", "0", eqp.File.SpecialDataIntervalMS.ToString(), s2f19Sets);
                                    if (s2f19Sets.Count > 0)
                                    {
                                          TS2F19_H_DataSetCommand(eqpno, agent, s2f19Sets, string.Empty, tid);
                                    }
                              }
                              
                              #endregion
                        }
                        #endregion

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_04_E_MaskStatusChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_04_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="3">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List (number of material slots)" type="L" len="?">
                        //    <array3 name="List" type="L" len="3">
                        //      <MATERIALSL name="MATERIALSL" type="A" len="2" fixlen="False" />
                        //      <MATERIALID name="MATERIALID" type="A" len="30" fixlen="False" />
                        //      <MATERIALST name="MATERIALST" type="A" len="1" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              string materialsl = xNode.ChildNodes[i]["MATERIALSL"].InnerText.Trim();
                              string materialid = xNode.ChildNodes[i]["MATERIALID"].InnerText.Trim();
                              string materialst = xNode.ChildNodes[i]["MATERIALST"].InnerText.Trim();
                              if (materialst == "R")
                              {
                                    continue;
                              }
                              //20141220 cy modify:canon報"*",記log, 發warning到OPI
                              if (eqp.Data.NODEATTRIBUTE == "CANON" && materialid == "*".PadRight(30, '*'))
                              {
                                    string msg = string.Format("Equipment({0}) mask id read fale. MaskSlot({1}),MaskID({2}),MaskState({3})", eqp.Data.NODEID, materialsl, materialid, materialst);
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Receive S6F11_04", msg });
                                    continue;
                              }
                              masks.Add(Tuple.Create(materialsl, materialid, ConvertCsotMaterialStatus(materialst)));
                        }
                        //if (eqp.Data.NODEATTRIBUTE == "CANON")
                        //    HandleMaskStatusForCanon(eqp, masks, tid, true, sysbytes);
                        //else
                        HandleMaskStatus(eqp, masks, tid, true, sysbytes);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_05_E_ProcessProgramChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_05_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="4">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //    <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQPST"].InnerText.Trim();
                        string newppid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //set eq new recipe
                        string oldppid = eqp.File.CurrentRecipeID;
                        eqp.File.CurrentRecipeID = newppid;
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("PPID Change from ({0}) to ({1}).", oldppid, newppid));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_06_E_ProcessProgramModifyorCreateorDelete(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_06_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //<array1 name="List" type="L" len="4">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <PPEVENT name="PPEVENT" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (number of changed)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //      <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body				
                        string ppevent = recvTrx["secs"]["message"]["body"]["array1"]["PPEVENT"].InnerText.Trim(); //C:Create M:Modify D:Delete
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        if (loop == 0)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Recipe Modify Report ModifyFlag({0}), without RecipeID.", ppevent));
                              return;
                        }

                        List<string> ppid = new List<string>();
                        for (int i = 0; i < loop; i++)
                        {
                              ppid.Add(xNode.ChildNodes[i]["PPID"].InnerText.Trim());
                              //string time = xNode.ChildNodes[i]["TIME"].InnerText.Trim();
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Recipe Modify Report RecipeID({0}), ModifyFlag({1}).", string.Join(",", ppid.ToArray()), ppevent));
                        //20141119 cy:Request recipe parameter and report mes to check when change state is not 3:Delete.
                        if (ppevent != "D")
                        {
                              for (int i = 0; i < loop; i++)
                              {
                                    RecipeCheckInfo rci = new RecipeCheckInfo(eqpno, 0, 0, ppid[i]);
                                    rci.TrxId = tid;
                                    eRecipeCheckResult result = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterChangeRequest", new object[] { tid, rci, eqp.Data.NODEID, eqp.Data.LINEID });
                                    if (result == eRecipeCheckResult.NG)
                                    {
                                          TS10F3_H_TerminalDisplaySingle(eqpno, eqpid, "Recipe Rarameter Change MES Reply NG", tid, string.Empty);
                                    }
                              }
                        }

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_07_E_GlassEraseReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_07_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="3">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="6">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <GLSPOSITION name="GLSPOSITION" type="A" len="16" fixlen="False" />
                        //    <REASONCODE name="REASONCODE" type="A" len="3" fixlen="False" />
                        //    <OPERATORID name="OPERATORID" type="A" len="10" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string cstseq = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim();
                        string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim();
                        string glsposition = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLSPOSITION"].InnerText.Trim();
                        //001: Causes of Equipment Remove
                        //002: Causes of manual Remove
                        //003: Causes of Equipment Broken
                        //004: Causes of Manual Broken
                        string reasoncode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["REASONCODE"].InnerText.Trim();
                        string operatorid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OPERATORID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok				

                        Invoke(eServiceName.JobService, "RemoveJobDataReportForSECS", new object[] { tid, glassid, eqp, "1", reasoncode, operatorid, cstseq, slot });
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_08_E_JOBINEQPReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_08_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="3">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok
                        // add by HUJUNPENG 20190123 for array SECS eqp process time start
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if(job!=null)
                        {
                            if (job.JobType == eJobType.TFT && eqp.Data.NODENO != "L2")
                            {
                                lock (job)
                                {
                                    job.ArraySpecial.JobProcessStartedTime = DateTime.Now;
                                }
                                ObjectManager.JobManager.EnqueueSave(job);
                            }
                        }                        
                        //
                        Invoke(eServiceName.JobService, "ReceiveJobDataReportForSECS", new object[] { tid, glassid, eqp, string.Empty });
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_09_E_JOBOUTEQPReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_09_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="3">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        Invoke(eServiceName.JobService, "SendOutJobDataReportForSECS", new object[] { tid, glassid, eqp, string.Empty });
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_10_E_JobProcessStartReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_10_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="4">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //    <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPID"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);

                        //20141215 cy add:job data內記錄使用的mask id
                        #region Update Mask ID in Job
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job ID({0}) in JobEntity", glassid));
                              return;
                        }
                        job.ArraySpecial.ExposureMaskID = eqp.InUseMaskID;
                        ObjectManager.JobManager.EnqueueSave(job);
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_11_E_JobProcessEndReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_11_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="4">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //    <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string ppid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["PPID"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_12_E_CummyRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_12_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="1">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_14_E_GlassDataRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_14_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //   <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //   <array2 name="List" type="L" len="3">
                        //     <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //     <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //     <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //   </array2>
                        // </array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        int cstseq = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job in JobEntity by CSTSEQ({0}), SLOT({1})!", cstseq.ToString(), slot.ToString()));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //download glass data
                        TS2F115_H_GlassDataDownload(eqpno, eqpid, job, string.Empty, tid);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_15_E_GlassPositionReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_15_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //get eqp object
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="3">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <GLASS_COUNT name="GLASS_COUNT" type="A" len="4" fixlen="False" />
                        //    <array3 name="List (number of total glass or postion)" type="L" len="?">
                        //      <array4 name="List" type="L" len="3">
                        //        <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //        <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //        <GLSPOSITION name="GLSPOSITION" type="A" len="16" fixlen="False" />
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body				
                        int glasscount = 0;
                        if (!int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASS_COUNT"].InnerText.Trim(), out glasscount))
                        {
                              _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Can not convert glass count value. GLASS_COUNT({0})", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASS_COUNT"].InnerText.Trim()));
                        }
                        //20150616 cy:增加record to history的功能
                        if (eqp.File.TotalTFTJobCount != glasscount)
                        {
                              eqp.File.TotalTFTJobCount = glasscount;
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                        }
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        List<Tuple<string, string, string>> gps = new List<Tuple<string, string, string>>();
                        for (int i = 0; i < loop; i++)
                        {
                              int cstseq = 0;
                              int slot = 0;
                              int.TryParse(xNode.ChildNodes[i]["CSTSEQ"].InnerText.Trim(), out cstseq);
                              int.TryParse(xNode.ChildNodes[i]["SLOT"].InnerText.Trim(), out slot);
                              gps.Add(Tuple.Create(xNode.ChildNodes[i]["GLSPOSITION"].InnerText.Trim(),
                                  cstseq.ToString(), slot.ToString()));
                        }

                        //add or update
                        string key = string.Format("{0}_PositionInfo", eqpno);
                        //20141107 cy:不管有沒有都更新,因為by glass count上報的,沒glass不能留殘帳
                        //if (gps.Count > 0) {
                        Repository.Add(key, gps);
                        //}				
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_16_E_GlassRecoveredReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_16_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        //<array1 name="List" type="L" len="3">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="6">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <GLSPOSITION name="GLSPOSITION" type="A" len="16" fixlen="False" />
                        //    <REASONCODE name="REASONCODE" type="A" len="3" fixlen="False" />
                        //    <OPERATORID name="OPERATORID" type="A" len="10" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string cstseq = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim();
                        string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim();
                        string glsposition = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLSPOSITION"].InnerText.Trim();
                        string reasoncode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["REASONCODE"].InnerText.Trim();
                        string operatorid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OPERATORID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        Invoke(eServiceName.JobService, "RemoveJobDataReportForSECS", new object[] { tid, glassid, eqp, "2", reasoncode, operatorid, cstseq, slot }); //wucc 20150728 補加入 cstseq,slot
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_18_E_GlassDataEdit(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_18_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="5">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <OLDCSTSEQ name="OLDCSTSEQ" type="A" len="5" fixlen="False" />
                        //    <OLDSLOT name="OLDSLOT" type="A" len="3" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string newcstseq = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim();
                        string newslot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim();
                        string oldcstseq = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OLDCSTSEQ"].InnerText.Trim();
                        string oldslot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OLDSLOT"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //check new cstseq
                        int cstseq;
                        int.TryParse(newcstseq, out cstseq);
                        if (cstseq == 0)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Invalid New CSTSEQ({0})!", newcstseq));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //check new slot
                        int slot;
                        int.TryParse(newslot, out slot);
                        if (slot == 0)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Invalid New SLOT({0})!", newslot));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get new job
                        Job newjob = ObjectManager.JobManager.GetJob(newcstseq, newslot);
                        //get old job
                        Job oldjob = ObjectManager.JobManager.GetJob(oldcstseq, oldslot);
                        if (oldjob != null)
                        {
                              //check if new not exists,change identity
                              if (newjob == null)
                              {
                                    oldjob.JobKey = newcstseq + newslot;
                                    oldjob.CassetteSequenceNo = newcstseq;
                                    oldjob.JobSequenceNo = newslot;
                                    ObjectManager.JobManager.AddJob(oldjob);
                                    ObjectManager.JobManager.EnqueueSave(oldjob);
                              }
                        }
                        else
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job OLDCSTSEQ({0}),OLDSLOT({1}) in JobEntity!", oldcstseq, oldslot));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("GlassDataEdit from Old CSTSEQ-SLOT ({0}-{1}) to New ({2}-{3})", oldcstseq, oldslot, newcstseq, newslot));

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_19_E_JobInSubEQReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_19_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //   <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //   <array2 name="List" type="L" len="5">
                        //     <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //     <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //     <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //     <INPUT_SLOT name="INPUT_SLOT" type="A" len="3" fixlen="False" />
                        //     <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //   </array2>
                        // </array1>
                        //body
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string subeqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SUBEQUIPMENTID"].InnerText.Trim();
                        string inputslot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["INPUT_SLOT"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get unit object
                        Unit unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                        if (unit == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Unit ID ({0}) in UnitEntity!", subeqpid));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        Invoke(eServiceName.JobService, "ReceiveJobDataReportForSECS", new object[] { tid, glassid, eqp, subeqpid });
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F11_20_E_JobOutSubEQReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_20_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="5">
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <INPUT_SLOT name="INPUT_SLOT" type="A" len="3" fixlen="False" />
                        //    <TIME name="TIME" type="A" len="14" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string subeqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SUBEQUIPMENTID"].InnerText.Trim();
                        string outputslot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OUTPUT_SLOT"].InnerText.Trim();
                        string time = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TIME"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get unit object
                        Unit unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                        if (unit == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Unit ID({0}) in UnitEntity!", subeqpid));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        Invoke(eServiceName.JobService, "SendOutJobDataReportForSECS", new object[] { tid, glassid, eqp, subeqpid });
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F11_21_E_JobHoldRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_21_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="5">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <SLOT name="INPUT_SLOT" type="A" len="3" fixlen="False" />
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <HOLDRESON name="HOLDRESON" type="A" len="80" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        string subeqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SUBEQUIPMENTID"].InnerText.Trim();
                        string slot = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim();
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string holdreason = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["HOLDRESON"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get unit object
                        Unit unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                        if (unit == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Unit ID({0}) in UnitEntity!", subeqpid));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(glassid);
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job GlassID({0}) in JobEntity!", glassid));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //add hold & update job
                        HoldInfo hold = new HoldInfo()
                        {
                              NodeNo = eqp.Data.NODENO,
                              NodeID = eqp.Data.NODEID,
                              UnitNo = unit.Data.UNITNO,
                              UnitID = unit.Data.UNITID,
                              HoldReason = holdreason,
                              OperatorID = eqp.Data.NODEID,
                        };
                        ObjectManager.JobManager.HoldEventRecord(job, hold);

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_22_E_OXRInformationUpdateReport(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_22_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="2">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <array2 name="List" type="L" len="5">
                        //    <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <OXR name="OXR" type="A" len="400" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                        int cstseq = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);
                        string glassId = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                        string oxr = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OXR"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job CSTSEQ({0}), SLOT({1}) in JobEntity!", cstseq.ToString(), slot.ToString()));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        //reply secondary
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                        //update job oxr	
                        lock (job)
                        {
                              job.OXRInformation = oxr; //ObjectManager.JobManager.M2P_AC_OXRInformation(oxr,job.ChipCount);				
                        }
                        ObjectManager.JobManager.EnqueueSave(job);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S6F11_23_E_MaterialStatusChange(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_23_E");
                  #region Handle Logic
                  try
                  {
                        string header = recvTrx["secs"]["message"]["header"].InnerText.Trim();
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="3">
                        //  <CEID name="CEID" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List (number of material type)" type="L" len="?">
                        //    <array3 name="List" type="L" len="5">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <MATERIALID name="MATERIALID" type="A" len="30" fixlen="False" />
                        //      <MATERIALST name="MATERIALST" type="A" len="1" fixlen="False" />
                        //      <MATERIALCT name="MATERIALCT" type="A" len="20" fixlen="False" />
                        //      <MATERIALSITE name="MATERIALSITE" type="A" len="20" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L2");//add by qiumin 20180711 PHL PR check
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                              return;
                        }

                        List<Tuple<Unit, string, string, eMaterialStatus, string, string>> materials = new List<Tuple<Unit, string, string, eMaterialStatus, string, string>>();
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              string subeqpid = xNode.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              Unit unit = null;
                              if (subeqpid != eqpid)
                              {
                                    unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                              }
                              string materialid = xNode.ChildNodes[i]["MATERIALID"].InnerText.Trim();
                              string materialst = xNode.ChildNodes[i]["MATERIALST"].InnerText.Trim();
                              string materialct = xNode.ChildNodes[i]["MATERIALCT"].InnerText.Trim();
                              string materialsite = xNode.ChildNodes[i]["MATERIALSITE"].InnerText.Trim();
                              if (materialst == "R")
                              {
                                    continue;
                              }
                              //add by qiumin 20180711 PHL PR check start
                              #region PHL PR check start
                              if ( materialst=="I"&&line.Data.LINEID.Contains("TCPHL"))  
                            {
                                if (string.IsNullOrEmpty (eqp.File.LastMaterialID))
                                {
                                    lock (eqp)
                                        eqp.File.LastMaterialID=materialid ;
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                }
                                if (eqp.File.LastMaterialID.Substring(0, 8) != materialid.Substring(0, 8))
                                {
                                    lock (eqp1)
                                        eqp1.File.PhlDelayCount = ParameterManager["MATERIALTYPECHANGEDELAYCOUNT"].GetInteger();
                                        eqp1.File.PhlHoldCount = ParameterManager["MATERIALTYPECHANGEHOLDCOUNT"].GetInteger();
                                        eqp1.File.PhlHoldReason = 1;
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                }
                                else
                                {
                                    if (eqp.File.LastMaterialID.Substring(13, 6) != materialid.Substring(13, 6))
                                    {
                                        if(eqp.File.LastMaterialID.Substring(1,2)=="42")
                                        {
                                            lock (eqp1)
                                                eqp1.File.PhlDelayCount = ParameterManager["PRMATERIALBATCHCHANGEDELAYCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldCount = ParameterManager["PRMATERIALBATCHCHANGEHOLDCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldReason = 2;
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                        }
                                        else if (eqp.File.LastMaterialID.Substring(1, 2) == "31")
                                        {
                                            lock (eqp1)
                                                eqp1.File.PhlDelayCount = ParameterManager["PLNMATERIALBATCHCHANGEDELAYCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldCount = ParameterManager["PLNMATERIALBATCHCHANGEHOLDCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldReason =3;
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                        }
                                        else
                                        {
                                            lock (eqp1)
                                                eqp1.File.PhlDelayCount = ParameterManager["PLNMATERIALBATCHCHANGEDELAYCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldCount = ParameterManager["PLNMATERIALBATCHCHANGEHOLDCOUNT"].GetInteger();
                                                eqp1.File.PhlHoldReason = 4;
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                                        }
                                    }
                                }
                                
                                lock (eqp)
                                    eqp.File.LastMaterialID = materialid;
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                            }
                              #endregion
                              materials.Add(Tuple.Create(unit, string.Empty, materialid, ConvertCsotMaterialStatus(materialst), materialct, materialsite));
                        }
                        HandleMaterialStatus(eqp, materials, tid, false, true, sysbytes, header);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            //2015/9/30 Add by Frank
            public void S6F11_24_E_MaterialConsumableReport(XmlDocument recvTrx)
            {
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_24_E");
                #region Handle Logic
                try
                {
                    string header = recvTrx["secs"]["message"]["header"].InnerText.Trim();
                    //get basic
                    string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                    string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                    //get eqp object
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Can not find Equipment Number in EquipmentEntity!");
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                    if (eqpid != eqp.Data.NODEID)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                    }
                    //get line object
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }

                    #region Materila Entity Collection
                    XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                    string len = xNode.Attributes["len"].InnerText.Trim();
                    int loop = 0;
                    int.TryParse(len, out loop);
                    List<MaterialEntity> materialList = new List<MaterialEntity>();
                    for (int i = 0; i < loop; i++)
                    {
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqpno;
                        materialE.MaterialType = xNode.ChildNodes[i]["MATERIALTYPE"].InnerText.Trim();
                        materialE.MaterialID = xNode.ChildNodes[i]["MATERIALID"].InnerText.Trim();
                        materialE.MaterialValue = xNode.ChildNodes[i]["QUANTITY"].InnerText.Trim();                        

                        materialList.Add(materialE);
                        ObjectManager.MaterialManager.AddMaterial(materialE);
                    }
                    #endregion

                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                    #region [MES Data]
                    if (materialList != null)
                    {
                        string glassId = "";
                        glassId = ObjectManager.JobManager.GetJobIDbyEQPNO(eqpno);
                        object[] _dataPrepare = new object[6]
                        { 
                            tid,                /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp,                /*2 Equipment*/
                            glassId,            /*3 ProductName*/
                            "",                 /*4 MaterialDurableName*/
                            materialList,       /*5 materilst*/ 
                        };
                        Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                    }
                                      
                    #endregion
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
                #endregion
            }
            //2015/9/30 Add by Frank
            public void S6F11_25_E_VCRStatusReport(XmlDocument recvTrx)
            {
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_25_E");
                #region Handle Logic
                try
                {
                    string header = recvTrx["secs"]["message"]["header"].InnerText.Trim();
                    //get basic
                    string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                    string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                    //get eqp object
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Can not find Equipment Number in EquipmentEntity!");
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                    if (eqpid != eqp.Data.NODEID)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                    }
                    //get line object
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //2015/9/30 add by Frank Check VCR Object
                    if (eqp.Data.VCRCOUNT <= 0)
                    {
                        _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("[EQUIPMENT={0}] Not found VCR Object.", eqpno));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                    }

                   
                    XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                    string len = xNode.Attributes["len"].InnerText.Trim();
                    int loop = 0;
                    int.TryParse(len, out loop);
                    //2015/9/30 add by Frank Check 機台上報VCR數量是否與設定VCR數量相同 
                    if(loop != eqp.Data.VCRCOUNT)
                    {
                        _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name +"()", eqpno, true, tid,
                            string.Format("[EQUIPMENT={0}] The number of VCR missmatch.", eqpno));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;                        
                    }
                    for (int i = 0; i < loop; i++)
                    {
                        string vcrno = xNode.ChildNodes[i]["VCRNO"].InnerText.Trim();
                        eBitResult vcrMode = (eBitResult)int.Parse(xNode.ChildNodes[i]["VCRSTATUS"].InnerText.Trim());
                        eBitResult oldVcrMode = eqp.File.VcrMode[int.Parse(vcrno) - 1];
                        if (oldVcrMode != vcrMode)
                        {
                            lock (eqp)
                                eqp.File.VcrMode[int.Parse(vcrno) - 1] = vcrMode;

                            #region MES Report VCRStateChanged
                            object[] _data = new object[5]
                            { 
                                tid,                /*0 TrackKey*/
                                eqp.Data.LINEID,    /*1 LineName*/
                                eqp.Data.NODEID,    /*2 EQPID*/
                                "VCR0" +  vcrno,                
                                vcrMode == eBitResult.ON ? "ENABLE":"DISABLE"
                            };
                            Invoke(eServiceName.MESService, "VCRStateChanged", _data);
                            #endregion
                        }
                    }
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok
                    
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
                #endregion
            }
            //2015/10/1 Add by Frank
            public void S6F11_26_E_VCRStatusReport(XmlDocument recvTrx)
            {
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_26_E");
                #region Handle Logic
                try
                {
                    string header = recvTrx["secs"]["message"]["header"].InnerText.Trim();
                    //get basic
                    string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                    string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                    string vcrno = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["VCRNO"].InnerText.Trim();
                    int cstseq = 0;
                    int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"].InnerText.Trim(), out cstseq);
                    int slot = 0;
                    int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"].InnerText.Trim(), out slot);
                    string glassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"].InnerText.Trim();
                    string readglassid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["READGLASSID"].InnerText.Trim();
                    string vcrresult = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["VCRRESULT"].InnerText.Trim();
                    string operatorid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OPERATORID"].InnerText.Trim();


                    //get eqp object
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Can not find Equipment Number in EquipmentEntity!");
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                    if (eqpid != eqp.Data.NODEID)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                    }
                    //get line object
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //Get Job
                    Job job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                    if (job == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Can not find Job CSTSEQ({0}), SLOT({1}) in JobEntity!", cstseq.ToString(), slot.ToString()));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return; 
                    }
                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                    job.VCRJobID = glassid;
                    job.VCR_Result = (eVCR_EVENT_RESULT)(int.Parse(vcrresult));

                    //CF & CELl Updata Glass ID in VCR B Type
                    if (line.Data.FABTYPE == eFabType.CF.ToString() || line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        if (eqp.Data.VCRTYPE == eVCRTYPE.B.ToString())
                        {
                            if (job.VCR_Result != eVCR_EVENT_RESULT.READING_FAIL_PASS)
                                Invoke(eServiceName.VCRService, "VCRMismatch_TypeB", new object[] {job, glassid});
                        }
                    }

                    if (job.VCRJobID.Trim() != job.MesProduct.PRODUCTNAME.Trim())
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE =[{2}], VCR GLASS ID{3} AND MES PRODUCTNAME{4} IS DIFFERENT!!",
                                eqp.Data.NODENO, tid, eqp.File.CIMMode, glassid.Trim(), job.MesProduct.PRODUCTNAME.Trim()));
                    }

                    ObjectManager.JobManager.EnqueueSave(job);

                    #region MES Report VCRReadReport
                    object[] _data = new object[5]
                    { 
                        tid,                /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp.Data.NODEID,    /*2 EQPID*/
                        job,                
                        job.VCR_Result,
                    };
                    Invoke(eServiceName.MESService, "VCRReadReport", _data);
                    #endregion

                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.VCR_Report.ToString(), tid, vcrno, string.Empty);
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
                #endregion
            }
            //2015/10/1 Add by Frank
            public void S6F11_27_E_OperatorLoginLogoutReport(XmlDocument recvTrx)
            {
                _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F11_27_E");
                #region Handle Logic
                try
                {
                    string header = recvTrx["secs"]["message"]["header"].InnerText.Trim();
                    //get basic
                    string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                    string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                    string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                    string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                    string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["EQUIPMENTID"].InnerText.Trim();
                    string touchpanelid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["TID"].InnerText.Trim();
                    string mode = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["MODE"].InnerText.Trim();
                    string operatorid = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["OPERATORID"].InnerText.Trim();


                    //get eqp object
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                    if (eqp == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        "Can not find Equipment Number in EquipmentEntity!");
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }
                    //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                    if (eqpid != eqp.Data.NODEID)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1);
                        return;
                    }
                    //get line object
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null)
                    {
                        _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                        TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 1); //ack ng
                        return;
                    }

                    TS6F12_H_EventReportAcknowledge(eqpno, agent, tid, sysbytes, 0); //ack ok

                    #region MES MachineLoginReport 只有login 才要報
                    if (mode != ((int)eLogInOutMode.Login).ToString())
                        return;

                    object[] _data = new object[4]
                    { 
                        tid,                /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp.Data.NODEID,    /*2 machineName */
                        operatorid
                    };
                    //Send MES Data
                    Invoke(eServiceName.MESService, "MachineLoginReport", _data);
                    #endregion

                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
                #endregion
            }

            public void S6F31_E_OXRInformationDownloadRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F31_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //<array1 name="List" type="L" len="4">
                        //   <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //   <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //   <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //   <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        // </array1>
                        //body
                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        int cstseq = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["CSTSEQ"].InnerText.Trim(), out cstseq);
                        int slot = 0;
                        int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["SLOT"].InnerText.Trim(), out slot);
                        string glassid = recvTrx["secs"]["message"]["body"]["array1"]["GLASSID"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              TS6F32_H_OXRInformationDownloadData(eqpno, agent, tid, sysbytes, "2", cstseq.ToString(), slot.ToString(), glassid, string.Empty); //ack ng
                              return;
                        }
                        //20141218 cy:檢查收到的equipment id跟設定的node no是否相同.
                        if (eqpid != eqp.Data.NODEID)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                  string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent));
                              TS6F32_H_OXRInformationDownloadData(eqpno, agent, tid, sysbytes, "2", cstseq.ToString(), slot.ToString(), glassid, string.Empty); //ack ng
                              return;
                        }
                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              TS6F32_H_OXRInformationDownloadData(eqpno, agent, tid, sysbytes, "2", cstseq.ToString(), slot.ToString(), glassid, string.Empty); //ack ng
                              return;
                        }

                        //get job object
                        Job job = ObjectManager.JobManager.GetJob(cstseq.ToString(), slot.ToString());
                        if (job == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Job CSTSEQ({0}), SLOT({1}) in JobEntity!", cstseq.ToString(), slot.ToString()));
                              TS6F32_H_OXRInformationDownloadData(eqpno, agent, tid, sysbytes, "2", cstseq.ToString(), slot.ToString(), glassid, string.Empty); //ack ng
                              return;
                        }

                        //convert oxr
                        string oxr = job.OXRInformation; //ObjectManager.JobManager.P2M_AC_OXRInformation(job.OXRInformation, job.ChipCount);

                        //reply secondary
                        TS6F32_H_OXRInformationDownloadData(eqpno, agent, tid, sysbytes, "1", cstseq.ToString(), slot.ToString(), glassid, oxr); //ack ok
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Send by host-S6
            public void TS6F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S6F0_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S6F0_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
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
            public void TS6F4_H_DiscreteVariableDataAcknowledge(string eqpno, string eqpid, string tid, string sysbytes)
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
                        sendTrx["secs"]["message"]["body"]["ACKC6"].InnerText = "0";
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
                        sendTrx["secs"]["message"]["body"]["ACK6"].InnerText = ack.ToString();
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
            //ret //1 = OK,2 = NG.
            public void TS6F32_H_OXRInformationDownloadData(string eqpno, string eqpid, string tid, string sysbytes, string ret, string cstseq, string slot, string glassId, string oxr)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S6F32_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S6F32_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;

                        //<array1 name="List" type="L" len="2">
                        //  <RETURN name="RETURN" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //    <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //    <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //    <OXR name="OXR" type="A" len="400" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["RETURN"], ret); //1 = OK,2 = NG.
                        if (ret == "2")
                        {
                              //If when RETURN is 2, The host sends a zero-length list to the equipment.
                              XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                              xNode.Attributes["len"].InnerText = "0";
                              xNode.RemoveChild(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"]);
                              xNode.RemoveChild(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"]);
                              xNode.RemoveChild(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"]);
                              xNode.RemoveChild(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["OXR"]);
                        }
                        else
                        {
                              XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                              xNode.Attributes["len"].InnerText = "4";
                              _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["CSTSEQ"], cstseq);
                              _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["SLOT"], slot);
                              _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["GLASSID"], glassId);
                              _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["OXR"], oxr);
                        }

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
            public void S6F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F0_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F0_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S6F4_H_DiscreteVariableDataAcknowledge(XmlDocument recvTrx, bool timeout)
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
            public void S6F32_H_OXRInformationDownloadData(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F32_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S6F32_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
      }
}