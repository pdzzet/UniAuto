using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public class OEEService : AbstractService
    {
        public override bool Init()
        {
            return true;
        }

        public void OEE_TibcoOpen()
        {
            LogInfo(MethodBase.GetCurrentMethod().Name + "()", "OEE Tibrv open success.");
        }

        /// <summary>
        /// LotProcessStarted
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="port">Port Entity</param>
        ///  <param name="cst">Cassette Entity</param>
        /// <param name="jobs">job Entity List</param>
        public void LotProcessStarted(string trxID, Port port, Cassette cst)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                XmlDocument xml_doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("LotProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
                //Add by marine for T3 2015/9/24
                bodyNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][((int)port.File.Type).ToString()].Value;
                //Jun Add 20150602 For POL Unloader AutoClave Report LotProcessStart
                if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3 && cst.CellBoxProcessed == eboxReport.Processing)
                    bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
                else
                    bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
                lotListNode.RemoveAll();
                if (cst.MES_CstData.LOTLIST.Count > 0)//sy add 防止T3 空 tray 只有 LOTLIST沒有PRODUCTLIST 會報Exception
                {
                    if (cst.MES_CstData.LOTLIST[0].PRODUCTLIST.Count > 0)
                    {
                        for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                        {
                            XmlNode lotnode = lotCloneNode.Clone();
                            Job job = null;
                            lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                            lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].PROCESSOPERATIONNAME;
                            lotnode[keyHost.PRODUCTQUANTITY].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count.ToString();
                            lotnode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTOWNER;
                            lotnode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;
                            //Add by marine for T3 2015/9/24
                            string ownerid = "";
                            if (cst.MES_CstData.LOTLIST.Count > 0)
                            {
                                ownerid = cst.MES_CstData.LOTLIST[0].PRODUCTLIST[0].OWNERID;
                            }
                            foreach (LOTc lotInfo in cst.MES_CstData.LOTLIST)
                            {
                                if (lotnode[keyHost.LOTNAME].InnerText == lotInfo.LOTNAME)
                                {
                                    //lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.OWNERTYPE, lotInfo.PRODUCTLIST[0].OWNERTYPE));
                                    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                                    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                                    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                                    lotnode[keyHost.OWNERTYPE].InnerText = lotInfo.PRODUCTLIST[0].OWNERTYPE.Length > 0 ? lotInfo.PRODUCTLIST[0].OWNERTYPE.Substring(lotInfo.PRODUCTLIST[0].OWNERTYPE.Length - 1, 1) : "";

                                    lotnode[keyHost.LOTINFO].InnerText = ownerid;
                                    break;
                                }
                            }

                            if (cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count > 0)
                                job = ObjectManager.JobManager.GetJob(cst.MES_CstData.LOTLIST[i].PRODUCTLIST[0].PRODUCTNAME.Trim());

                            if (job != null)
                            {
                                lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                                //lotnode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                                lotnode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName; //2015/10/15 cc.kuang
                            }
                            else
                            {
                                lotnode[keyHost.PRODUCTRECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                                lotnode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                            }

                            lotListNode.AppendChild(lotnode);
                        }
                    }
                }
                //Watson Add 20141212 For MES Spec 
                //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
                cst.IsOffLineProcessStarted = false;
                lock (cst)
                {
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                SendToOEE(xml_doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send LotProcessStarted to OEE PortName({2}).", line.Data.LINEID, trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// ProductIn
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        /// <param name="currentEQPID">Process EQPID</param>
        /// <param name="unitID">Process UNITID</param>
        /// <param name="portid">Port ID</param>
        /// <param name="traceLvl">Process Level ‘M’ – Machine ,‘U’ – Unit ,‘P’ – Port</param>
        /// <param name="processtime">traceLvl='P' is empty,</param>
        public void ProductIn(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, string processtime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                Unit unit = null;
                if (unitID != "")
                    unit = ObjectManager.UnitManager.GetUnit(unitID);

                //Watson Modify 20150320 For PMT Line 2 Send 改成在送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                XmlDocument xml_doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("ProductIn") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                //Watson Modify 20150313 For PMT Line Send Different Data To MES.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                else
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                if (unitID != "")
                {
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                        bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    else
                        bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                }
                else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;

                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);

                #region Watson Add 20150313 For PMT Line Store、Receive 做法.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    Port port = ObjectManager.PortManager.GetPort(portid);
                    if (port != null)
                    {
                        if (port.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                        {
                            bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                            bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        }
                    }
                }
                #endregion

                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();
                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                //Watson modidyf 2011215 For MES 王鵬
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = job.ToSlotNo.Equals("0") ? "" : job.ToSlotNo;

                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID == string.Empty ? job.MesProduct.PRODUCTNAME : job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";  //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                //Modify for T3 OEE by marine 2015/9/23
                bodyNode[keyHost.ORIENTEDSITE].InnerText = "t3";
                bodyNode[keyHost.ORAIENTEDFACTORYNAME].InnerText = line.Data.FABTYPE;
                bodyNode[keyHost.CURRENTSITE].InnerText = "t3";
                bodyNode[keyHost.CURRENTFACTORYNAME].InnerText = line.Data.FABTYPE;

                if (job.MesCstBody.LOTLIST.Count > 0)
                {
                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                    bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                }

                string ownerType = job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "";
                if (line.Data.LINETYPE.Contains("FCUPK_TYPE"))
                {
                    ownerType = job.CfSpecial.UPKOWNERTYPE;
                }
                bodyNode[keyHost.OWNERTYPE].InnerText = ownerType;

                bodyNode[keyHost.PRODUCTINFO].InnerText = job.MesProduct.OWNERID;

                //Jun Add 20141128 For Spec 修改
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                    {
                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                        double pnlsize = 0;
                        if (double.TryParse(job.CellSpecial.PanelSize, out pnlsize))
                            pnlsize = pnlsize / 100;
                        bodyNode[keyHost.GLASSSIZE].InnerText = pnlsize.ToString();
                    }
                }

                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText.Trim());

                if (otherline.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, otherline.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                SendToOEE(xml_doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send ProductIn to OEE .", lineName, trxID));
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// ProductInoutTotal
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="line"></param>
        /// <param name="port"></param>
        /// <param name="job"></param>
        public void ProductInOutTotal(string trxid, string lineName, string machineName, string portName, Job job, eProductInOutTotalFlag cutFlag = eProductInOutTotalFlag.NORMAL)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName); //Offline  无需上报OEE  20150317 Tom
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}).", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                //Check 是否无需上报OEE  20150407 Tom
                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, machineName, "", "ProductInOutTotal", eAgentName.OEEAgent) == true)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[LINENAME={1}] [BCS -> OEE][{0}] Equipment =[{2}],Port =[{3}]Don't Report ProductInOutTotal to OEE.",
                     trxid, lineName, machineName, portName));

                    return;
                }


                XmlDocument doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("ProductInOutTotal") as XmlDocument;
                if (doc == null)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] ProductInOutTotal transaction is Not found.", lineName, trxid));
                    return;
                }
                SetTransactionID(doc, trxid);

                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                if (job.MesCstBody.LOTLIST.Count > 0)
                {
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;  //此处需要使用Line Recipe Name 20150515 tom //job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                    bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                }
                //根据福杰介绍 此处要截取最后一码 20150325 Tom
                //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                bodyNode[keyHost.OWNERTYPE].InnerText = (job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "");
                bodyNode[keyHost.PRODUCTINFO].InnerText = job.MesProduct.OWNERID;

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";
                //Jun Add 20141128 For Spec 修改
                if (lineName.Contains(keyCellLineType.CUT))
                {
                    bodyNode[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                    double pnlsize = 0;
                    if (double.TryParse(job.CellSpecial.PanelSize, out pnlsize))
                        pnlsize = pnlsize / 100;
                    bodyNode[keyHost.GLASSSIZE].InnerText = pnlsize.ToString();
                }

                if (line.Data.LINETYPE.Contains("FCUPK_TYPE"))
                {
                    bodyNode[keyHost.OWNERTYPE].InnerText = job.CfSpecial.UPKOWNERTYPE;
                }

                //-----------20141224 Tom Add For OEE 1.12
                bodyNode[keyHost.ORIENTEDSITE].InnerText = "t3";
                bodyNode[keyHost.ORAIENTEDFACTORYNAME].InnerText = line.Data.FABTYPE;
                bodyNode[keyHost.CURRENTSITE].InnerText = "t3";
                bodyNode[keyHost.CURRENTFACTORYNAME].InnerText = line.Data.FABTYPE;
                //------------20141224------------------
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                //StartTime使用 出CST的时间 20150407 Tom
                bodyNode["STARTTIMESTAMP"].InnerText = job.JobProcessStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");//DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                //for t3 Mark 20150917 tom
                // bodyNode[keyHost.PORTNAME].InnerText = portName;
                // bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                //TimeStamp使用当前时间 就可以了（进入CST的时间）
                // bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                XmlNode machineNodeList = bodyNode[keyHost.MACHINELIST];
                XmlNode machineNode = machineNodeList[keyHost.MACHINE];
                machineNodeList.RemoveAll();

                SerializableDictionary<string, ProcessFlow> jobProcessFlows = null;
                if (job.JobProcessFlowsForOEE == null || job.JobProcessFlowsForOEE.Count == 0)
                    jobProcessFlows = job.JobProcessFlows;
                else
                    jobProcessFlows = job.JobProcessFlowsForOEE;


                if (jobProcessFlows != null)
                {
                    foreach (string key in jobProcessFlows.Keys)
                    {
                        XmlNode node = machineNode.Clone();
                        node[keyHost.MACHINENAME].InnerText = jobProcessFlows[key].MachineName;
                        node[keyHost.PORTNAME].InnerText = portName;
                        node[keyHost.TRACELEVEL].InnerText = "M";
                        node[keyHost.POSITION].InnerText = string.IsNullOrEmpty(jobProcessFlows[key].SlotNO) == true ? "" : jobProcessFlows[key].SlotNO;//job.ToSlotNo;
                        node["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        node["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(jobProcessFlows[key].MachineName);
                        if (eqp != null)
                        {

                            if (eqp.Data.NODEATTRIBUTE == "LU")
                            {
                                //LU 机台Startime 为最后一次Receive Time  ,EndTime 为为第一次Sent Out Time
                                //modify 2015/10/16 cc.kuang
                                //node["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                //node["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                node["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                node["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"); 
                            }
                            else if (eqp.Data.NODEATTRIBUTE == "LD")
                            {
                                //LD 机台Start time 为第一次Receive time (没有Receive) 
                                node["STARTTIMESTAMP"].InnerText = "";
                                node["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else if (eqp.Data.NODEATTRIBUTE == "UD")
                            {
                                node["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                node["ENDTIMESTAMP"].InnerText = "";
                            }

                        }

                        XmlNode subMachineNodelist = node["SUBMACHINELIST"];
                        XmlNode subMachineNode = subMachineNodelist["SUBMACHINE"];
                        subMachineNodelist.RemoveAll();
                        if (jobProcessFlows[key].UnitProcessFlows.Count > 0)
                        {
                            foreach (string subKey in jobProcessFlows[key].UnitProcessFlows.Keys)
                            {
                                //日期格式不正确时就不上班OEE  20150424 Tom
                                if (jobProcessFlows[key].UnitProcessFlows[subKey].StartTime == DateTime.MinValue || jobProcessFlows[key].UnitProcessFlows[subKey].EndTime == DateTime.MinValue)
                                {
                                    continue;
                                }
                                XmlNode subUnit = subMachineNode.Clone();
                                subUnit[keyHost.MACHINENAME].InnerText = jobProcessFlows[key].MachineName;
                                subUnit[keyHost.UNITNAME].InnerText = jobProcessFlows[key].UnitProcessFlows[subKey].MachineName;
                                subUnit[keyHost.PORTNAME].InnerText = portName;
                                subUnit[keyHost.TRACELEVEL].InnerText = "U";
                                subUnit[keyHost.POSITION].InnerText = string.IsNullOrEmpty(jobProcessFlows[key].UnitProcessFlows[subKey].SlotNO) == true ? "" : jobProcessFlows[key].UnitProcessFlows[subKey].SlotNO;//job.ToSlotNo;
                                if (eqp.Data.NODEATTRIBUTE == "LU")
                                {
                                    subUnit["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].UnitProcessFlows[subKey].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    subUnit["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].UnitProcessFlows[subKey].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                }
                                else
                                {
                                    subUnit["STARTTIMESTAMP"].InnerText = jobProcessFlows[key].UnitProcessFlows[subKey].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    subUnit["ENDTIMESTAMP"].InnerText = jobProcessFlows[key].UnitProcessFlows[subKey].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                }
                                subMachineNodelist.AppendChild(subUnit);
                            }
                        }
                        machineNodeList.AppendChild(node);
                    }
                }

                SendToOEE(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send ProductInOutTotal to OEE machineName({2}),PortName({3}).", lineName, trxid, machineName, portName));
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Product In Out Total M t3 新增Function 在每次Send
        /// 	1.Reports when occurs PRODUCT OUT in the EQP(Machine)
	    ///2.This reports included PRODUCT IN(STARTTIME), OUT(ENDTIME) the Time and product the content for EQP(Machine) and each SUB EQP(Unit)
	    ///3.If use this message then no need more previous the PRODUCT IN, PRODUCT OUT, PRODUCT IN-OUT-TOTAL the message. Therefore can be reduced for the message transaction.
	    ///4.When job remove,should report ProductInOutTotalM message,include start time(product in ) andend time(remove time)
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="lineName"></param>
        /// <param name="currentEQPID"></param>
        /// <param name="portName"></param>
        /// <param name="job"></param>
        /// <param name="jobevent"></param>
        //public void ProductInOutTotalM(string trxid, string lineName, string currentEQPID, string portName, Job job) { // 2016/04/13 cc.kuang for t3 OEE requirement
        public void ProductInOutTotalM(string trxid, string lineName, string currentEQPID, string portName, Job job)
        {
            ProductInOutTotalM(trxid, lineName, currentEQPID, portName, job, eJobEvent.SendOut);
        }
        public void ProductInOutTotalM(string trxid, string lineName, string currentEQPID, string portName, Job job, eJobEvent jobevent = eJobEvent.SendOut)
        { 
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);
                if (line.File.HostMode == eHostMode.OFFLINE) {
                    //mark by yang,2017/3/15 not record in ERROR.txt
                  //  base.Logger.LogErrorWrite(base.LogName, base.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   //     string.Format("[LINENAME={0}] [BCS -> MES][{1}] MES OFF-LINE, SKIP \"{2}\" REPORT.", line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                    base.Logger.LogInfoWrite(base.LogName, base.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> MES][{1}] MES OFF-LINE, SKIP \"{2}\" REPORT.", line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                } else
                {
                    XmlDocument transactionFormat = base.GetServerAgent("OEEAgent").GetTransactionFormat("ProductInOutTotalM") as XmlDocument;
                    if (transactionFormat == null) {
                        base.LogError(MethodBase.GetCurrentMethod().Name + "()", 
                            string.Format("[LINENAME={0}] [BCS -> OEE][{1}] ProductInOutTotalM transaction is Not found.", lineName, trxid));
                    } else {
                        this.SetTransactionID(transactionFormat, trxid);
                        XmlNode node = transactionFormat["MESSAGE"]["BODY"];
                        node["LINENAME"].InnerText = lineName;
                        node["MACHINENAME"].InnerText = currentEQPID;
                        node["PORTNAME"].InnerText = portName;
                        //Bstl 测试时要求LD、UD、LU 在上报是要使用”P“ 20151223  Tom
                        if (eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD" || eqp.Data.NODEATTRIBUTE == "LU")
                        {
                            if (jobevent == eJobEvent.Store) //just store report P for t3 OEE requirement 2016/04/13 cc.kuang
                                node["TRACELEVEL"].InnerText = "P";
                            else
                                node["TRACELEVEL"].InnerText = "M";
                        }
                        else
                        {
                            node["TRACELEVEL"].InnerText = "M";
                        }


                        #region CF Short Cut Special Rule
                        if (lineName == "FCMPH100" || lineName == "FCRPH100" || lineName == "FCGPH100" || lineName == "FCBPH100" || lineName == "FCOPH100")
                        {
                            // CV06 Send to CV07，模擬此line的Unloader上報。
                            if (eqp.Data.NODEATTRIBUTE == "BF" && job.CfSpecial.PermitFlag == "Y")
                            {
                                Equipment ULD = ObjectManager.EquipmentManager.GetEQP("L22");
                                node["MACHINENAME"].InnerText = ULD.Data.NODEID;
                                node["TRACELEVEL"].InnerText = "P";
                            }
                            // CV07 Send Out，模擬Next line的Loader上報。
                            if (eqp.Data.NODEATTRIBUTE == "CV07" && job.CfSpecial.PermitFlag == "Y")
                            {
                                node["LINENAME"].InnerText = line.Data.NEXTLINEID;
                                Equipment LD = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.NEXTLINEID).FirstOrDefault(ld => ld.Data.NODEATTRIBUTE == "LD");
                                node["MACHINENAME"].InnerText = LD.Data.NODEID;
                                node["TRACELEVEL"].InnerText = "M";
                            }
                        }
                        #endregion

                        if (job.MesCstBody.LOTLIST.Count > 0) {
                            node["LOTNAME"].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                            node["LINERECIPENAME"].InnerText = job.LineRecipeName;
                            node["PRODUCTSPECNAME"].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                            node["PROCESSOPERATIONNAME"].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                            node["PRODUCTOWNER"].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                        }
                        node["OWNERTYPE"].InnerText = (job.MesProduct.OWNERTYPE.Length > 0) ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "";
                        node["PRODUCTINFO"].InnerText = job.MesProduct.OWNERID;
                        node["CROSSLINEFLAG"].InnerText = "N";
                        if (lineName.Contains("CUT")) {
                            node["CROSSLINEFLAG"].InnerText = job.CellSpecial.CrossLineFlag;
                            double result = 0.0;
                            if (double.TryParse(job.CellSpecial.PanelSize, out result)) {
                                result /= 100.0;
                            }
                            node["GLASSSIZE"].InnerText = result.ToString();
                        }
                        if (line.Data.LINETYPE.Contains("FCUPK_TYPE")) {
                            node["OWNERTYPE"].InnerText = job.CfSpecial.UPKOWNERTYPE;
                        }
                        node["ORIENTEDSITE"].InnerText = "t3";
                        node["ORAIENTEDFACTORYNAME"].InnerText = line.Data.FABTYPE;
                        node["CURRENTSITE"].InnerText = "t3";
                        node["CURRENTFACTORYNAME"].InnerText = line.Data.FABTYPE;
                        node["PRODUCTNAME"].InnerText = job.GlassChipMaskBlockID;
                        node["HOSTPRODUCTNAME"].InnerText = job.MesProduct.PRODUCTNAME;
                        node["PRODUCTGRADE"].InnerText = job.JobGrade;
                        ProcessFlow flow = job.JobProcessFlowsForOEE[currentEQPID];
                        if (flow != null) {
                            node["STARTTIMESTAMP"].InnerText = flow.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                        node["ENDTIMESTAMP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        XmlNode node2 = node["SUBMACHINELIST"];
                        XmlNode node3 = node2["SUBMACHINE"];
                        node2.RemoveAll();
                        //modify 2016/03/14 cc.kuang
                        /*
                        if (flow.UnitProcessFlows.Count > 0) {
                            foreach (string str in flow.UnitProcessFlows.Keys) {
                                if (!(flow.UnitProcessFlows[str].StartTime == DateTime.MinValue) && !(flow.UnitProcessFlows[str].EndTime == DateTime.MinValue)) {
                                    XmlNode newChild = node3.Clone();
                                    newChild["UNITNAME"].InnerText = flow.UnitProcessFlows[str].MachineName;
                                    newChild["PORTNAME"].InnerText = portName;
                                    newChild["TRACELEVEL"].InnerText = "U";
                                    newChild["POSITION"].InnerText = string.IsNullOrEmpty(flow.UnitProcessFlows[str].SlotNO) ? "" : flow.UnitProcessFlows[str].SlotNO;
                                    newChild["STARTTIMESTAMP"].InnerText = flow.UnitProcessFlows[str].StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    newChild["ENDTIMESTAMP"].InnerText = flow.UnitProcessFlows[str].EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    node2.AppendChild(newChild);
                                }
                            }
                        }
                        */
                        if (flow.ExtendUnitProcessFlows.Count > 0) {
                            foreach (ProcessFlow flow2 in flow.ExtendUnitProcessFlows) {
                                if (!(flow2.StartTime == DateTime.MinValue) && !(flow2.EndTime == DateTime.MinValue)) {
                                    if (flow2.StartTime > flow2.EndTime) //for t3 garbage data not report to OEE 2016/01/20 cc.kuang
                                    {
                                        base.LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send ProductInOutTotalM to OEE UnitName({2}) StartTime > EndTime).", lineName, trxid, flow2.MachineName));
                                        continue;
                                    }
                                    XmlNode node5 = node3.Clone();
                                    node5["UNITNAME"].InnerText = flow2.MachineName;
                                    node5["PORTNAME"].InnerText = portName;
                                    node5["TRACELEVEL"].InnerText = "U";
                                    node5["POSITION"].InnerText = string.IsNullOrEmpty(flow2.SlotNO) ? "" : flow2.SlotNO;
                                    node5["STARTTIMESTAMP"].InnerText = flow2.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    node5["ENDTIMESTAMP"].InnerText = flow2.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    node2.AppendChild(node5);
                                }
                            }
                        }
                        this.SendToOEE(transactionFormat);
                        //job.JobProcessFlowsForOEE.Remove(currentEQPID); //for t3 remove unit history data 2016/01/20 cc.kuang
                        //modify for SECS S6F3_03 glass history resave unit history 2016/05/11 cc.kuang
                        if (job.JobProcessFlowsForOEE.ContainsKey(currentEQPID))
                            job.JobProcessFlowsForOEE[currentEQPID].ExtendUnitProcessFlows.Clear();

                        base.LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send ProductInOutTotalM to OEE machineName({2})).", lineName, trxid, currentEQPID));
                    }
                }
            } catch (Exception exception) {
                base.LogError(MethodBase.GetCurrentMethod().Name + "()", exception);
            }
        }

        /// <summary>
        /// ProductOut
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">job entity(WIP)</param>
        /// <param name="currentEQPID">假如job位於機台中 就報，不是就填空</param>
        /// <param name="portid">假如job位於Port 就報，不是就填空</param>
        /// <param name="unitID">假如job位於Unit 就報，不是就填空</param>
        /// <param name="traceLvl">Process Level ‘M’ – Machine ,‘U’ – Unit ,‘P’ – Port</param>
        public void ProductOut(string trxID, string lineName, Job job, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                Unit unit = null;
                if (unitID != "")
                    unit = ObjectManager.UnitManager.GetUnit(unitID);

                //Watson Add 20150319 For PMT Line Special Rule 在送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                XmlDocument xml_doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("ProductOut") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                else
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);


                if (unitID != "")
                {
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                        bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    else
                        bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                }
                else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);

                #region Watson Add 20150313 For PMT Line Fetch Out、Send 做法.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    Port port = ObjectManager.PortManager.GetPort(portid);
                    if (port != null)
                    {
                        if (port.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                        {
                            bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                            bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        }
                    }
                }
                #endregion

                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();

                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                //Watson modidyf 2011215 For MES 王
                if (traceLvl == eMESTraceLevel.M)
                    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                else
                    bodyNode[keyHost.POSITION].InnerText = job.FromSlotNo;
                if (job.GlassChipMaskBlockID == string.Empty)
                {
                    bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                }
                else
                    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;

                bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N"; //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                //Modify for T3 OEE by marine 2015/9/23
                bodyNode[keyHost.ORIENTEDSITE].InnerText = "t3";
                bodyNode[keyHost.ORAIENTEDFACTORYNAME].InnerText = line.Data.FABTYPE;
                bodyNode[keyHost.CURRENTSITE].InnerText = "t3";
                bodyNode[keyHost.CURRENTFACTORYNAME].InnerText = line.Data.FABTYPE;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                if (job.MesCstBody.LOTLIST.Count > 0)
                {
                    bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                    bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                    bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                }

                string ownerType = job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "";
                if (line.Data.LINETYPE.Contains("FCUPK_TYPE"))
                {
                    ownerType = job.CfSpecial.UPKOWNERTYPE;
                }
                bodyNode[keyHost.OWNERTYPE].InnerText = ownerType;

                bodyNode[keyHost.PRODUCTINFO].InnerText = job.MesProduct.OWNERID;

                //Jun Add 20141128 For Spec 修改
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                    {
                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                        double pnlsize = 0;
                        if (double.TryParse(job.CellSpecial.PanelSize, out pnlsize))
                            pnlsize = pnlsize / 100;
                        bodyNode[keyHost.GLASSSIZE].InnerText = pnlsize.ToString();
                    }
                }

                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText);

                if (otherline.File.HostMode == eHostMode.OFFLINE) //Offline 无需上报资料 20150325 Tom 
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                SendToOEE(xml_doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send ProductOut to OEE .", lineName, trxID));
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LotProcessEnd(string trxid, Line line, Port port, IList<Job> jobs)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                //Line line = ObjectManager.LineManager.GetLine(lineName);
                XmlDocument doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("LotProcessEnd") as XmlDocument;
                if (doc == null)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] LotProcessEnd transaction is Not found.", line.Data.LINEID, trxid));
                    return;
                }
                SetTransactionID(doc, trxid);

                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                bodyNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)port.File.Type].Value;
                XmlNode productNodeList = bodyNode[keyHost.PRODUCTLIST];
                XmlNode productNode = productNodeList[keyHost.PRODUCT];
                productNodeList.RemoveAll();
                foreach (Job job in jobs)
                {
                    XmlNode jobNode = productNode.Clone();
                    jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    jobNode[keyHost.PRODUCTJUDGE].InnerText = ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                    jobNode[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;
                    jobNode[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation;
                    if (line.Data.LINETYPE == "CBODF")
                        jobNode[keyHost.PAIRPRODUCTNAME].InnerText = job.MesProduct.CFPRODUCTNAME;
                    jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    if (job.MesCstBody.LOTLIST.Count > 0)
                    {
                        jobNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                        jobNode[keyHost.LINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                        jobNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                        jobNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                        jobNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                    }
                    jobNode[keyHost.PRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                    //jobNode[keyHost.OWNERTYPE].InnerText = job.MesProduct.OWNERTYPE;
                    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                    jobNode[keyHost.OWNERTYPE].InnerText = (job.MesProduct.OWNERTYPE.Length > 0 ? job.MesProduct.OWNERTYPE.Substring(job.MesProduct.OWNERTYPE.Length - 1, 1) : "");


                    jobNode[keyHost.LOTINFO].InnerText = job.MesProduct.OWNERID;

                    SetAbnormalCodelList(line.Data.LINETYPE, jobNode[keyHost.ABNORMALCODELIST], job);
                    string holdFlag = "N";
                    if (job.HoldInforList.Count > 0)
                    {
                        holdFlag = "Y";
                    }
                    jobNode[keyHost.HOLDFLAG].InnerText = holdFlag;
                    XmlNode psHeightListNode = jobNode[keyHost.PSHEIGHTLIST];
                    XmlNode psHeightCloneNode = psHeightListNode[keyHost.SITEVALUE].Clone();
                    psHeightListNode.RemoveAll();
                    //PS Height dont use
                    //TO DO ProductResult
                    bool _ttp = false;
                    jobNode[keyHost.PROCESSRESULT].InnerText = GetPROCESSRESULT(line, port, ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo)), job, out _ttp);

                    productNodeList.AppendChild(jobNode);
                }

                SendToOEE(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send LotProcessEnd to OEE PortName({2}).", line.Data.LINEID, trxid, line.Data.LINEID));
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SetAbnormalCodelList(string lineType, XmlNode parentNode, Job job)
        {
            XmlNode codeCloneNode = parentNode[keyHost.CODE];
            parentNode.RemoveAll();
            foreach (CODEc code in job.MesProduct.ABNORMALCODELIST)
            {
                XmlNode codeNode = codeCloneNode.Clone();
                codeNode[keyHost.ABNORMALSEQ].InnerText = code.ABNORMALSEQ;
                codeNode[keyHost.ABNORMALCODE].InnerText = code.ABNORMALCODE;
                parentNode.AppendChild(codeNode);
            }

        }

        public void EnergyReport(string trxid, string lineName, Equipment eqp, Dictionary<string, EnergyMeter> energyMeters)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxid, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                //20150422 OEE Spec修改 Tom
                XmlDocument doc = GetServerAgent(eAgentName.OEEAgent).GetTransactionFormat("ENERGYREPORT") as XmlDocument;
                if (doc == null)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] EnergyReport transaction is Not found.", lineName, trxid));
                    return;
                }
                SetTransactionID(doc, trxid);

                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                bodyNode[keyHost.TIMESTAMP].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                bodyNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus;
                bodyNode[keyHost.RECIPEID].InnerText = eqp.File.CurrentRecipeID;

                //XmlNode MeterList = bodyNode["METERLIST"];
                //XmlNode MeterNode = MeterList["METER"];
                //MeterList.RemoveAll();
                XmlNode ParameterList = bodyNode["PARAMETERLIST"];
                XmlNode ParameterNode = ParameterList["PARAMETER"];
                ParameterList.RemoveAll();
                foreach (EnergyMeter energyMeter in energyMeters.Values)
                {

                    //XmlNode ParameterList = node["ParameterList"];
                    //XmlNode ParameterNode = ParameterList["Parameter"];
                    //ParameterList.RemoveAll();
                    XmlNode subNode = ParameterNode.Clone();
                    subNode["UNITID"].InnerText = energyMeter.UnitID;
                    subNode["METERNO"].InnerText = energyMeter.MeterNo;
                    subNode["REFRESH"].InnerText = energyMeter.Refresh;

                    if (energyMeter.EnergyParam.Count > 0)
                    {
                        switch (energyMeter.EnergyType)
                        {
                            case "Liquid":
                                subNode["ENERGYTYPE"].InnerText = "WATER";
                                break;
                            case "Electricity":
                                subNode["ENERGYTYPE"].InnerText = "ELECTRIC";
                                break;
                            case "Gas":
                                subNode["ENERGYTYPE"].InnerText = "GAS";
                                break;
                            case "N2":
                                subNode["ENERGYTYPE"].InnerText = "N2";
                                break;
                            case "Stripper":
                                subNode["ENERGYTYPE"].InnerText = "STRIPPER";
                                break;
                        }
                        foreach (string key in energyMeter.EnergyParam.Keys)    // modify by brcue 20160222 依機台上報組數固定將 CURRENT, PRESSURE, QUANTITY 參數加入
                        {
                            subNode[key].InnerText = energyMeter.EnergyParam[key];
                        }
                        ParameterList.AppendChild(subNode);
                    }

                }

                SendToOEE(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> OEE][{1}] send EnergyReport to OEE machineName({2})).", lineName, trxid, eqp.Data.NODEID));
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendToOEE(XmlDocument doc)
        {
            xMessage msg = new xMessage();
            msg.ToAgent = eAgentName.OEEAgent;
            msg.Data = doc.OuterXml;
            PutMessage(msg);
        }

        private void SetTransactionID(XmlDocument aDoc, string transactionId)
        {
            aDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText = transactionId;
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
                                //case eLineType.ARRAY.SRT:
                                case eLineType.ARRAY.CHN_SEEC:
                                    if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";
                                    return "N";
                                //case eLineType.ARRAY.PHOTO:
                                case eLineType.ARRAY.PHL_TITLE:
                                case eLineType.ARRAY.PHL_EDGEEXP:
                                    if (job.SamplingSlotFlag.Equals("1") && !job.TrackingData.Equals(new string('0', 32))) return "Y";
                                    if (job.ArraySpecial.PhotoIsProcessed) return "Y";
                                    return "N";
                                case eLineType.ARRAY.WET_DMS:
                                case eLineType.ARRAY.WEI_DMS:
                                case eLineType.ARRAY.STR_DMS:
                                case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                    trackData = job.TrackingData.Substring(0, 2);
                                    trackData = UtilityMethod.Reverse(trackData);

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
                                            if (trackings.ElementAt(0).Value.Equals("1"))
                                            {
                                                TTPFlag = true;
                                                return "Y";
                                            }
                                        }
                                        return "N";
                                    }
                                default:
                                    {
                                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                        {
                                            if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";
                                        }
                                        else
                                        {
                                            //TrackingData
                                            IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                            foreach (string v in trackings.Values)
                                            {
                                                if (v.Equals("1") || v.Equals("2")) return "Y";
                                            }
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
                              + 7.UPK Line 的 Unloader 機台
                                  (1)Equipment Run Mode為Normal時一律都報Y.
                                  (2)Equipment Run Mode為Re-Clean時由TrackingData來判斷.                  
                             */

                            switch (line.Data.LINETYPE)
                            {
                                case eLineType.CF.FCMPH_TYPE1:
                                case eLineType.CF.FCRPH_TYPE1:
                                case eLineType.CF.FCGPH_TYPE1:
                                case eLineType.CF.FCBPH_TYPE1:
                                case eLineType.CF.FCOPH_TYPE1:
                                case eLineType.CF.FCSPH_TYPE1:
                                    #region Photo Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                    {
                                                        if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }

                                                    switch (job.JobJudge)
                                                    {
                                                        case "1": //OK
                                                        case "5": //RP
                                                            if (job.CfSpecial.EQPFlag2.ExposureProcessFlag.Equals("1"))
                                                                return "Y";
                                                            else
                                                                return "N";
                                                        case "2": //NG
                                                        case "3": //RW
                                                            return "Y";
                                                    }
                                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("CAN'T FIND JOB JUDGE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                                    return "N";

                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCREW_TYPE1:
                                    #region Rework Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                    {
                                                        if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }

                                                    if (int.Parse(job.CfSpecial.ReworkRealCount) > 1 && int.Parse(job.CfSpecial.ReworkRealCount) < int.Parse(job.CfSpecial.ReworkMaxCount)) return "M";
                                                    else if (int.Parse(job.CfSpecial.ReworkRealCount) == int.Parse(job.CfSpecial.ReworkRealCount)) return "Y";
                                                    else if (job.CfSpecial.ReworkRealCount.Equals("0")) return "N";

                                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("CHECK REWORK COUNT,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}],ReworkMaxCount:[{6}],ReworkRealCount:[{7}]",
                                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag, job.CfSpecial.ReworkMaxCount, job.CfSpecial.ReworkRealCount));
                                                    return "N";

                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCSRT_TYPE1:
                                    #region Sorter Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                    {
                                                        if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                //// 20150417 邏輯跟Array一樣
                                //if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";

                                //// 以下寫的有問題
                                ////if (port.File.Type == ePortType.LoadingPort)
                                ////{
                                ////    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                ////}
                                ////else
                                ////{
                                ////    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && port.File.Type == ePortType.UnloadingPort) return "Y";
                                ////}

                                    #endregion
                                case eLineType.CF.FCREP_TYPE1:
                                case eLineType.CF.FCREP_TYPE2:
                                case eLineType.CF.FCREP_TYPE3:// 20160509 Add by Frank
                                case eLineType.CF.FCMQC_TYPE1:
                                case eLineType.CF.FCMQC_TYPE2:
                                case eLineType.CF.FCMAC_TYPE1:
                                    #region REP,MQC,FIP Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                            {
                                                switch (port.File.Type)
                                                {
                                                    case ePortType.LoadingPort:
                                                        return "N";
                                                    case ePortType.UnloadingPort:
                                                        return "Y";
                                                }
                                            }
                                            else
                                            {
                                                IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                switch (port.File.Type)
                                                {
                                                    case ePortType.LoadingPort:
                                                    case ePortType.BothPort:
                                                        foreach (string v in trackings.Values)
                                                        {
                                                            if (v.Equals("1")) return "Y";
                                                        }
                                                        return "N";
                                                    case ePortType.UnloadingPort:
                                                        if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                        {
                                                            if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                            {
                                                                //Add by Kasim in 20150526 
                                                                return "N";
                                                            }
                                                        }
                                                        foreach (string v in trackings.Values)
                                                        {
                                                            if (v.Equals("1")) return "Y";
                                                        }
                                                        return "N";
                                                }
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCUPK_TYPE1:
                                    #region Unpacker Rule
                                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                                    string ULDRunmode = string.Empty;
                                    foreach (Equipment eqp in eqps)
                                    {
                                        if (eqp.Data.NODEATTRIBUTE == "UD")
                                            ULDRunmode = eqp.File.EquipmentRunMode;
                                    }
                                    switch (port.File.Type)
                                    {
                                        case ePortType.LoadingPort:
                                            return "N";
                                        case ePortType.UnloadingPort:
                                            return "Y";
                                        case ePortType.BothPort:
                                            //2015/9/21 add by Frank
                                            if (ULDRunmode == "RE-CLEAN")
                                            {
                                                if (job.TrackingData.Substring(0, 1).Equals(1))
                                                    return "Y";
                                                else
                                                    return "N";
                                            }
                                            return "N";
                                    }

                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                default:
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T LINE TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
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
                                //case eLineType.CELL.CBPMT:
                                //    if (job.JobProcessFlows.Count > 1) //Jun Modify 20150402 不需判斷SamplingSlotFlag或ProcessFlag  && job.SamplingSlotFlag == "1")
                                //        return "Y";
                                //    else
                                //        return "N"; //Watson Add 20141225 For MES Spec

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

                                case eLineType.CELL.CBGAP:
                                case eLineType.CELL.CBPMT:
                                case eLineType.CELL.CBDPS:
                                case eLineType.CELL.CBDPI:
                                case eLineType.CELL.CBATS:
                                case eLineType.CELL.CBLOI:
                                case eLineType.CELL.CBSOR_1:
                                case eLineType.CELL.CBSOR_2:
                                    if (job.CreateTime != job.JobProcessStartTime)  //Jun Modify 20150402 && job.SamplingSlotFlag == "1")
                                        return "Y";
                                    else
                                        return "N";

                                default:
                                    if (job.JobProcessFlows.Count > 1)  //Jun Modify 20150402 && job.SamplingSlotFlag == "1")
                                        return "Y";
                                    else
                                        return "N";
                            }
                        }
                    default: return "N";
                }
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                return "N";
            }
        }
    }
}
