using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Core;
using System.Xml;
using System.Reflection;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.MISC;


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class ModMESService : AbstractService
    {
        /// <summary>
        /// Check is Offline 
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="trxID">TrxID</param>
        /// <param name="methodName"> message name </param>
        /// <returns></returns>
        private bool CheckOffline(Line line,string trxID,string methodName) {
            if (line.File.HostMode == eHostMode.OFFLINE) {
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, methodName));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check Mes Message Return Node 
        /// when ReturnCode is "0" method return true otherwise return false;  
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="lineName"></param>
        /// <param name="trxId"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private bool CheckMESReturnCode(XmlDocument doc, string lineName, string trxId, string methodName) {
            try {
                string retCode = doc[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText;
                if (retCode != "0") {
                    string retMsg = doc[keyHost.MACHINE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText;
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} mes reply NG RetCode =[{3}] RetMsg =[{4}].",
                                       lineName, trxId, methodName, retCode, retMsg));

                    return false;
                }
                return true;
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("Method {0} has Exception {1}", methodName, ex.ToString()));
                return true;
            }
        }

        /// <summary>
        /// Get product Judge
        /// </summary>
        /// <param name="fabType"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public string GetProductJudge(string fabType, Job job) {
            try {

                return ConstantManager[string.Format("{0}_JOBJUDGE_MES", fabType.ToString())][job.JobJudge].Value;

            } catch (Exception ex) {
                LogError(MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }
        //ProductProcessData
        /// <summary>
        /// 6.129.	ProductProcessData      MES<-BC BC reports Process Data of Glass or Panel. This ‘Process Data’ includes only EDC items that are subject to Real-time SPC rule check. Detailed EDC items will be specified separately after customer requirement collection on Real-time SPC items.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">MachineName</param>
        /// <param name="unitName">UnitName</param>
        /// <param name="lotName">LotName</param>
        /// <param name="carrierName">CarrierName</param>
        /// <param name="productName">ProductName</param>
        /// <param name="productSpecName">ProductSpecName</param>
        /// <param name="productSpecVer">ProductSpecVer</param>
        /// <param name="processOperationName">ProcessOperationName</param>
        /// <param name="lineRecipeName">LineRecipeName</param>
        /// <param name="itemList">ItemList</param>
        public void ProductProcessData(string trxID, string lineName, string machineName, string unitName, string lotName, string carrierName, string productName,
            string productSpecName, string productSpecVer, string processOperationName, string lineRecipeName, IList<ProductProcessData.ITEMc> itemList) {

            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductProcessData") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitName);
                bodyNode[keyHost.LOTNAME].InnerText = lotName;
                bodyNode[keyHost.CARRIERNAME].InnerText = carrierName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = productSpecName;
                bodyNode[keyHost.PRODUCTSPECVER].InnerText = productSpecVer;
                bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = processOperationName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = lineRecipeName;

                XmlNode itemListNode = bodyNode[keyHost.ITEMLIST];
                XmlNode itemNode = itemListNode[keyHost.ITEM].Clone();

                XmlNode siteListNode = itemNode[keyHost.SITELIST].Clone();
                XmlNode siteNode = siteListNode[keyHost.SITE].Clone();
                siteListNode.RemoveAll();

                itemListNode.RemoveAll();

                foreach (ProductProcessData.ITEMc item in itemList) {
                    XmlNode itemnode = itemNode.Clone();
                    itemnode[keyHost.ITEMNAME].InnerText = item.ITEMNAME;

                    //If item doesn’t have any site, BC should report SITENAME with ‘DEFAULT’.
                    if (item.SITELIST.Count == 0) {
                        XmlNode sitelistnode = siteListNode.Clone();
                        itemnode.RemoveChild(itemnode[keyHost.SITELIST]);
                        XmlNode sitenode = siteNode.Clone();
                        sitenode[keyHost.SITENAME].InnerText = "DEFAULT";
                        sitelistnode.AppendChild(sitenode);
                        itemnode.AppendChild(sitelistnode);
                    } else {
                        //SITELIST
                        XmlNode sitelistnode = siteListNode.Clone();
                        itemnode.RemoveChild(itemnode[keyHost.SITELIST]);
                        foreach (ProductProcessData.SITEc site in item.SITELIST) {
                            XmlNode sitenode = siteNode.Clone();
                            sitenode[keyHost.SITENAME].InnerText = site.SITENAME;
                            sitenode[keyHost.SITEVALUE].InnerText = site.SITEVALUE;
                            sitelistnode.AppendChild(sitenode);
                        }
                        itemnode.AppendChild(sitelistnode);
                    }

                    itemListNode.AppendChild(itemnode);
                }
                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            } catch (Exception ex) {
                LogError(MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


       /// <summary>
       /// ProductLineOut
       /// </summary>
       /// <param name="trxID"></param>
       /// <param name="lineName"></param>
       /// <param name="job"></param>
       /// <param name="currentEQPID"></param>
       /// <param name="portID"></param>
       /// <param name="unitID"></param>
        public void ProductLineOut(string trxID, string lineName, Job job, string currentEQPID, string portID, string unitID) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("ProductLineOut") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portID);

            
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                if (job.MesCstBody.LOTLIST.Count > 0)
                    bodyNode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;

                //Modify 2015/7/13
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                if (fabType == eFabType.CF) {
                    bodyNode[keyHost.INLINEINFLAG].InnerText = "N";
                }
                if (fabType == eFabType.ARRAY || fabType == eFabType.CELL) {
                    bodyNode[keyHost.INLINEINFLAG].InnerText = "Y";
                } else {
                    //
                    bodyNode[keyHost.INLINEINFLAG].InnerText = "N";
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}] Report ProductLineOut to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
            } catch (Exception ex) {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Pallet Change Report
        /// Pallet id change report
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="palletName"></param>
        /// <param name="agingPalletName"></param>
        /// <param name="eventUser"></param>
        public void PalletChangeReport(string trxID, string lineName, string machineName, string palletName, string agingPalletName, string eventUser) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument xml_doc = GetServerAgent().GetTransactionFormat("PalletChangeReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MESSAGENAME].InnerText = machineName;
                bodyNode[keyHost.PALLETNAME].InnerText = palletName;
                bodyNode[keyHost.AGINGPALLETNAME].InnerText = agingPalletName;
                bodyNode[keyHost.EVENTUSER].InnerText = eventUser;

                SendToMES(xml_doc);

                LogInfo( MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],PalletName =[{3}],AgingPalletName=[{4}] Report PalletChangeReport to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), machineName, palletName,agingPalletName));

            } catch (System.Exception ex) {
                LogError(MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PanelAccumulationReport
        /// <summary>
        /// Panel AccumulationRepot
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="LineName"></param>
        /// <param name="machineName"></param>
        /// <param name="accumulationStatus">1:Start  2:Finish</param>
        public void PanelAccumulationReport(string trxID,string LineName,string machineName,string accumulationStatus) {
            try {
                Line line = ObjectManager.LineManager.GetLine(LineName);
                if (CheckOffline(line, trxID, MethodInfo.GetCurrentMethod().Name)) {//Off line Don't Report
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelAccumulationReport") as XmlDocument;
                SetTransactionID(doc, trxID);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = LineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.ACCUMULATIONSTATUS].InnerText = accumulationStatus;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(doc);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],AccumulationStatus =[{3}] Report PanelAccumulationReport to MES.",
                   trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), machineName,accumulationStatus=="1"?"Start":"Finish" ));


            } catch (System.Exception ex) {
                LogError(MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PanelProcessEnd
        public void PanelProcessEnd(string trxID, string lineName, string machineName, Job job) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name)) {// Offline don't Report
                    return;
                }
                Equipment eqp =ObjectManager.EquipmentManager.GetEQPByID(machineName);
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelProcessEnd") as XmlDocument;
                SetTransactionID(doc, trxID);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;


                XmlNode materialList = bodyNode[keyHost.MATERIALLIST];
                XmlNode materialClone = materialList[keyHost.MATERIAL].Clone();
                materialList.RemoveAll();
                bodyNode[keyHost.CASSETTESEQUENCENO].InnerText = job.CassetteSequenceNo;
                bodyNode[keyHost.JOBSEQUENCENO].InnerText = job.JobSequenceNo;
                bodyNode[keyHost.PPID].InnerText = job.MES_PPID;
                bodyNode[keyHost.CIMMODE].InnerText = ((int)job.CIMMode).ToString();
                bodyNode[keyHost.JOBJUDGE].InnerText = GetProductJudge(line.Data.FABTYPE, job); //在Constants.xml定义了MODULE_JOBJUDGE_MES
                bodyNode[keyHost.CHECKWORKORDERFLAG].InnerText = "";
                bodyNode[keyHost.LSRCFLAG].InnerText = ""; //OK ,Cutting Ok ,NG  Cutting NG,NC Not Cutting before
                bodyNode[keyHost.CLEANBYPASFLAG].InnerText = "";//0:Clean 1:Clean by pass
                bodyNode[keyHost.OCHNGFLAG].InnerText = "";//0 ok,1 OLB Check NG 
                bodyNode[keyHost.PCBNGFLAG].InnerText = "";//0 ok 1 panel check NG
                bodyNode[keyHost.OCHSHIFTDISABLEFLAG].InnerText = "";
                bodyNode[keyHost.OCHAKKCOMDISABLEFLAG].InnerText = "";
                bodyNode[keyHost.OCHBURRDISABLEFLAG].InnerText = "";
                bodyNode[keyHost.OCHSHIFTSAMPLINGPASSFLAG].InnerText = "";
                bodyNode[keyHost.OCHAKKCOMSAMPLINGPASSFLAG].InnerText = "";
                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],PanelID=[{3}], Report PanelProcessEnd to MES.",
                     trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), machineName,job.GlassChipMaskCutID));

            } catch (System.Exception ex) {
                LogError(MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        //PanelProcessEndReply
        public void MES_PanelProcessEndReply(XmlDocument doc) {
            try {
                string trx = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
                string lineName=doc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINENAME].InnerText;
                string machineName = doc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string productName = doc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", 
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} Equipment =[{3}],ProductName =[{4}] Receive PanelProcessEndReply from MES",
                                       lineName,trx,MethodBase.GetCurrentMethod().Name,machineName,productName ));
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PanelProcessStarted
        /// <summary>
        /// panel process Started 
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="job"></param>
        public void PanelProcessStarted(string trxId,string lineName,string machineName, Job job) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelProcessStared") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                bodyNode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;

                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],ProductID =[{4}]. report PanelprocessStarted to MES.",
                        lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, job.GlassChipMaskBlockID));


            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PanelProcessCanceled
        /// <summary>
        /// PanelProcess Cancelel
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="job"></param>
        public void PanelProcessCanceled(string trxID, string lineName, string machineName, Job job) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelProcessCanceled") as XmlDocument;
                SetTransactionID(doc, trxID);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],ProductID =[{4}]. report PanelProcessCanceled to MES.",
                      lineName, trxID, MethodBase.GetCurrentMethod().Name, machineName, job.GlassChipMaskBlockID));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PanelProductIn

        /// <summary>
        /// PanelProductIn
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="unitName"></param>
        /// <param name="portName"></param>
        /// <param name="traceLevel">M,machine,P port,U unit</param>
        /// <param name="position"></param>
        /// <param name="job"></param>
        /// <param name="processingTime"> bc processingtime wiht nonoe empty value</param>
        public void PanelProductIn(string trxId, string lineName, string machineName, string unitName, string portName,
                                   string traceLevel, string position, Job job, string processingTime) {
                try {
                    Line line = ObjectManager.LineManager.GetLine(lineName);
                    if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                        return;
                    }
                    XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelProductIn") as XmlDocument;
                    SetTransactionID(doc, trxId);
                    XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                    bodyNode[keyHost.LINENAME].InnerText = lineName;
                    bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                    bodyNode[keyHost.UNITNAME].InnerText = unitName;
                    bodyNode[keyHost.PORTNAME].InnerText = portName;
                    bodyNode[keyHost.TRACELEVEL].InnerText = traceLevel;
                    bodyNode[keyHost.POSITION].InnerText = position;
                    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                    bodyNode[keyHost.PROCESSINGTIME].InnerText = processingTime;

                    SendToMES(doc);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],ProductName =[{4}]. report PanelProductIn to MES.",
                            lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, job.GlassChipMaskBlockID));


                } catch (System.Exception ex) {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                }
        }
        //PanelProductOut
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="unitName"></param>
        /// <param name="portName"></param>
        /// <param name="traceLevel">M :Machine,P:port,U:Unit</param>
        /// <param name="position"></param>
        /// <param name="job"></param>
        public void PanelProductOut(string trxId, string lineName, string machineName, string unitName, string portName,
                                    string traceLevel,string position, Job job) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PanelProductOut") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.UNITNAME].InnerText = unitName;
                bodyNode[keyHost.PORTNAME].InnerText = portName;
                bodyNode[keyHost.TRACELEVEL].InnerText = traceLevel;
                bodyNode[keyHost.POSITION].InnerText = position;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;
                bodyNode[keyHost.PALLETNAME].InnerText = "";//Job Entity 需要增加一个Pallet Name
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PROCESSINGTIME].InnerText = "";
                XmlNode materialList = bodyNode[keyHost.MATERIALLIST];
                XmlNode materialClone = materialList[keyHost.MATERIAL].Clone();
                materialList.RemoveAll();

                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],ProductName =[{4}]. report PanelProductOut to MES.",
                          lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, job.GlassChipMaskBlockID));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PuncherMountRequest
        /// <summary>
        /// PuncherMountRequest
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="puncherName"></param>
        public void PuncherMountRequest(string trxId, string lineName, string machineName, string puncherName) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PuncherMountRequest") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.PUNCHERNAME].InnerText = puncherName;
                //to do Create Timer
                string timerId = string.Format("{0}_{1}_PuncherMountRequest", lineName, machineName);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId,false,ParameterManager["MESTIMEOUT"].GetInteger(),PuncherMountReplyTimeout,trxId);
                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],PuncherName =[{4}]. report PuncherMountRequest to MES.",
                          lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, puncherName));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name+"()",ex);
            }
        }

        private void PuncherMountReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e){
            try {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                if (Timermanager.IsAliveTimer(tmp)) {
                    Timermanager.TerminateTimer(tmp);
                }
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} Equipment =[{3}].  PuncherMountRply timeout.",
                        sArray[0], trackKey, MethodBase.GetCurrentMethod().Name, sArray[1]));


                
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PuncherMountReply
        /// <summary>
        /// PuncherMountReply 
        /// 需要与MES讨论如果两个Return Code 怎么处理
        /// </summary>
        /// <param name="doc"></param>
        public void MES_PuncherMountReply(XmlDocument doc) {
            try {
                string trxId = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
                string retCode = doc[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText;
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                string lineName = bodyNode[keyHost.LINENAME].InnerText;
                string machineName = bodyNode[keyHost.MACHINENAME].InnerText;
                string timerId = string.Format("{0}_{1}_PuncherMountReply", lineName, machineName);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                if (retCode != "0") {
                    string retMsg = doc[keyHost.MACHINE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText;
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} mes reply NG RetCode =[{3}] RetMsg =[{4}].",
                                    lineName, trxId, MethodBase.GetCurrentMethod().Name, retCode, retMsg));
                    //TODO  MES Reply NG Handler
                    return;
                }
               
                string returnCode = bodyNode[keyHost.RETURNCODE].InnerText;
                string puncherName = bodyNode[keyHost.PUNCHERNAME].InnerText;
                string punncherMaxCount = bodyNode[keyHost.PUNCHERMAXIMUMCOUNT].InnerText;
                string puncherCurrentCount = bodyNode[keyHost.PUNCHERCURRENTCOUNT].InnerText;
                
                

                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} Equipment =[{3}],PuncherName =[{4}],MaxCount =[{5}],CurrentCount =[{6}]. receive PuncherMountReply from MES.",
                        lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, puncherName,punncherMaxCount,puncherCurrentCount));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PuncherUnmountReport
        /// <summary>
        /// PuncherUnmountReport
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="puncherName"></param>
        /// <param name="currentCount"></param>
        public void PuncherUnmountReport(string trxId, string lineName, string machineName, string puncherName, string currentCount) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PuncherUnmountReport") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.PUNCHERNAME].InnerText = puncherName;
                bodyNode[keyHost.PUNCHERCURRENTCOUNT].InnerText = currentCount;
                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Equipment =[{3}],PuncherName =[{4}],CurrentCount =[{5}]. Report PuncherUnmountReport to MES.",
                        lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, puncherName, currentCount));


            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PuncherCountReport
        /// <summary>
        /// PuncherCountReport
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="punchers"></param>
        public void PuncherCountReport(string trxId, string lineName, List<Puncher> punchers) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PuncherCountReport") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                XmlNode puncherNodeList = bodyNode[keyHost.PUNCHERLIST];
                XmlNode puncherNode = puncherNodeList["PUNCHER"];
                puncherNodeList.RemoveAll();
                foreach (Puncher puncher in punchers) {
                    XmlNode temp = puncherNode.Clone();
                    temp[keyHost.MACHINENAME].InnerText = puncher.EquipmentID;
                    temp[keyHost.PUNCHERNAME].InnerText = puncher.PuncherName;
                    temp[keyHost.PUNCHERCURRENTCOUNT].InnerText = puncher.CurrentCount.ToString();
                    puncherNodeList.AppendChild(temp);
                }
                SendToMES(doc);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} . Report PuncherCountReport to MES.",
                        lineName, trxId, MethodBase.GetCurrentMethod().Name));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PalletIdCreateRequest
        /// <summary>
        /// PalletIdCreateReqeuest 
        /// 记录MES Time out ，Timer ID =LineName _TrxID_PalletIdCreateReqeust, timer State 为BoxName
        /// 
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="boxName"></param>
        public void PalletIdCreateRequest(string trxId, string lineName, string boxName) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("PalletIdCreateRequest") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.BOXNAME].InnerText = boxName;
                SendToMES(doc);

                string timerId = string.Format("{0}_{1}_PalletIdCreateReply", lineName, trxId);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), PalletIdCreateReplyTimeout, boxName);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} BoxName =[{3}]. report PalletIdCreateRequest to MES.",
                          lineName, trxId, MethodBase.GetCurrentMethod().Name, boxName));
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PalletIdCreateReplyTimeout(object subject, System.Timers.ElapsedEventArgs e) {
            try {
                UserTimer userTimer = subject as UserTimer;
                string tmp = userTimer.TimerId;
                string boxName = userTimer.State.ToString();
                if (Timermanager.IsAliveTimer(tmp)) {
                    Timermanager.TerminateTimer(tmp);
                }
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} BoxName =[{3}].  PalleIdCreateReply timeout.",
                        sArray[0], sArray[1], MethodBase.GetCurrentMethod().Name, boxName));
                //TODO Timeout Handler

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //PalletIdCreateReply
        /// <summary>
        /// PalletIdCreateReply
        /// </summary>
        /// <param name="doc"></param>
        public void MES_PalletIdCreateReply(XmlDocument doc) {
            try {
                string trxId = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                string lineName = bodyNode[keyHost.LINENAME].InnerText;
                string retCode = doc[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText;
                string timerId = string.Format("{}_{}_PalletIdCreateReply", lineName, trxId);

                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                if (retCode != "0") {
                    string retMsg = doc[keyHost.MACHINE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText;
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} mes reply NG RetCode =[{3}] RetMsg =[{4}].",
                                    lineName, trxId, MethodBase.GetCurrentMethod().Name, retCode, retMsg));
                    //TODO  MES Reply NG Handler
                    return;
                }
                string palletName = bodyNode[keyHost.PALLETNAME].InnerText;
                string capacity = bodyNode[keyHost.CAPACITY].InnerText;
                
                LogInfo(MethodBase.GetCurrentMethod().Name+"()",string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} PalletName =[{3}],Capacity =[{4}]. Receive PalleIdCreateReply from Mes ",
                        lineName,trxId,MethodBase.GetCurrentMethod().Name,palletName,capacity));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //RuncardIdCreateRequest
        /// <summary>
        /// RuncardIdCreateReply
        /// 侦测MES timeout TImer ID=LineName_ProductName_RuncardIdCreateReply, timer State 为TrxID
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="productName"></param>
        public void RuncardIdCreateRequest(string trxId, string lineName, string productName) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("RuncardIdCreateRequest") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                SendToMES(doc);

                string timerId = string.Format("{0}_{1}_RuncardIdCreateReply", lineName, productName);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), RuncardIdCreateReplyTimeout, trxId);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} ProductName =[{3}]. report PalletIdCreateRequest to MES.",
                          lineName, trxId, MethodBase.GetCurrentMethod().Name, productName));
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// RuncardIdCreateReply Timeout handler Function
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RuncardIdCreateReplyTimeout(object subject, System.Timers.ElapsedEventArgs e) {
            try {
                UserTimer userTimer = subject as UserTimer;
                string tmp = userTimer.TimerId;
                string trxId = userTimer.State.ToString();
                if (Timermanager.IsAliveTimer(tmp)) {
                    Timermanager.TerminateTimer(tmp);
                }
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} PRODUCTNAME =[{3}]. RuncardIdCreateReply timeout.",
                        sArray[0], trxId, MethodBase.GetCurrentMethod().Name, sArray[1]));
                //TODO Timeout Handler

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //RuncardIdCreateReply
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void MES_RuncardIdCreateReply(XmlDocument doc) {
            try {
                string trxId = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
                string returnCode=doc[keyHost.MACHINE][keyHost.RETURN][keyHost.RETURNCODE].InnerText;
                 XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                string lineName = bodyNode[keyHost.LINENAME].InnerText;
                string productName = bodyNode[keyHost.PRODUCTNAME].InnerText;
                string timerId = string.Format("{0}_{1}_RuncardIdCreateReply", lineName, productName);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                if(returnCode!="0"){
                    string retMsg=doc[keyHost.MACHINE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText;
                     LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} mes reply NG RetCode =[{3}] RetMsg =[{4}].",
                                        lineName, trxId, MethodBase.GetCurrentMethod().Name, returnCode, retMsg));
                    //TODO MES Reply NG 
                     return;
                }
                string runcardName = bodyNode[keyHost.RUNCARDNAME].InnerText;
                string capacity = bodyNode[keyHost.CAPACITY].InnerText;
               
                //TODO  MES Reply RuncarId Create
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} ProductName =[{3}],Capacity =[{4}],RuncardName =[{5}]. Receive RuncardIdCreateReply from Mes.",
                                        lineName, trxId, MethodBase.GetCurrentMethod().Name, productName, capacity, runcardName));
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //ModBoxInformationRequest
        /// <summary>
        /// ModBoxInformationRequest,参照ValidationBoxRequest 的写法 需要侦测t9 timeout
        /// Timerid=lineName_MachineName_PortName_trxID_ModBoxyInforamtionReply
        /// timer  state =BoxName
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="portName"></param>
        /// <param name="boxName"></param>
        public void ModBoxInformationRequest(string trxId, string lineName, string machineName, string portName, string boxName) {
            try {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                    return;
                }
                XmlDocument doc = GetServerAgent().GetTransactionFormat("ModBoxInformationRequest") as XmlDocument;
                SetTransactionID(doc, trxId);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.PORTNAME].InnerText = portName;
                bodyNode[keyHost.BOXNAME].InnerText = boxName;
                SendToMES(doc);
                // 要刪掉T9 TIMEOUT, REMOTE部份需等到 Process Start 之後再下
                string timeoutName = string.Format("{0}_{1}_{2}_{3}_ModBoxInformationReply",lineName,machineName, portName,trxId);
                if (_timerManager.IsAliveTimer(timeoutName)) {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ModBoxInformationReplyTimeout), boxName);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()", 
                        string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} BoxName =[{3}] PortName =[{4}]. report ModBoxInformationRequest to MES.",
                          lineName, trxId, MethodBase.GetCurrentMethod().Name, boxName,portName));

            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ModBoxInformationReplyTimeout(object subject,System.Timers.ElapsedEventArgs e) {
            try {
                UserTimer userTimer = subject as UserTimer;
                string tmp = userTimer.TimerId;
                string boxName = userTimer.State.ToString();
                if (Timermanager.IsAliveTimer(tmp)) {
                    Timermanager.TerminateTimer(tmp);
                }
                string[] sArray = tmp.Split('_');
                string lineName=sArray[0];
                string machineName=sArray[1];
                string portName=sArray[2];
                string trxId=sArray[3];
                LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} ,MachineName =[{3}],PortName =[{4}] ,BoxName =[{5}]. ModBoxInformationReply timeout.",
                       lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName,portName,boxName));
                //TODO  MES reply Time out 


            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //ModBoxInformationReply
        public void MES_ModBoxInformationReply(XmlDocument doc) {
            try {
                string trxId = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
                
                XmlNode bodyNode=doc[keyHost.MESSAGE][keyHost.BODY];
                string lineName = bodyNode[keyHost.LINENAME].InnerText;
                string machineName = bodyNode[keyHost.MACHINENAME].InnerText;
                string portName = bodyNode[keyHost.PORTNAME].InnerText;
                if (CheckMESReturnCode(doc, lineName, trxId, MethodBase.GetCurrentMethod().Name)==false) {
                    //TODO MES Reply NG 
                    return;
                }
                string lineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;
                string recipeParaValidationFlag = bodyNode[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText;
                string carrierSetCode = bodyNode[keyHost.CARRIERSETCODE].InnerText;
                string valiResult = bodyNode[keyHost.VALIRESULT].InnerText;
                if (valiResult != "Y") {
                    //TODO MES Reply NG
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} mes Validation Result NG .",
                                       lineName, trxId, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Line line = null;
                Equipment eqp = null;
                Port port = null;
                string errMsg="";
                    
              

                if(line!=null&& eqp!=null && port!=null){
                    if (line.File.HostMode == eHostMode.LOCAL) {
                        lock (port) port.File.Mes_ValidateBoxReply = doc.InnerXml;
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] Local Mode, Check MES Data Complete, Wait UI Edit Data.",
                            lineName, trxId));
                        //TODO  Box Local Mode
                    }
                }


            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //ModChangerVCRReadRequest
        /// <summary>
        /// ModChangerVCRReadRequest, 侦测MES TIme 
        /// TIme ID=LineName_MachineName_ProductName_trxID_odChangerVCRReadReply
        /// Timer State =boxName 
        /// </summary>
        /// <param name="trxId"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="boxName"></param>
        /// <param name="productName"></param>
        /// <param name="vcrResult"></param>
        public void ModChangerVCRReadRequest(string trxId, string lineName, string machineName, string boxName, string productName, eVCR_EVENT_RESULT vcrResult) {
           try{
               Line line = ObjectManager.LineManager.GetLine(lineName);
               if (CheckOffline(line, trxId, MethodBase.GetCurrentMethod().Name)) {
                   return;
               }
               XmlDocument doc = GetServerAgent().GetTransactionFormat("ModBoxInformationRequest") as XmlDocument;
               SetTransactionID(doc, trxId);
               XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
               bodyNode[keyHost.LINENAME].InnerText = lineName;
               bodyNode[keyHost.MACHINENAME].InnerText = machineName;
               bodyNode[keyHost.BOXNAME].InnerText = boxName;
               bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
               bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
               bodyNode[keyHost.VCRREADFLAG].InnerText = ((int)vcrResult).ToString();
               SendToMES(doc);

               string timeoutName = string.Format("{0}_{1}_{2}_{3}_ModChangerVCRReadReply", lineName, machineName, productName,trxId);
               if (_timerManager.IsAliveTimer(timeoutName)) {
                   _timerManager.TerminateTimer(timeoutName);
               }
               _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                   new System.Timers.ElapsedEventHandler(ModChangerVCRReadReplyTimeout), boxName);
               LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} MachineName =[{3}],ProductName =[{4}] ,BoxName =[{5}]. Report ModChangerVCRReadRequest to MES.",
                     lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, productName, boxName));


           }catch (System.Exception ex){
               LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
           }
        }

        /// <summary>
        /// ModChangerVCRReadReply Timeout
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void ModChangerVCRReadReplyTimeout(object subject, System.Timers.ElapsedEventArgs e) {
            try{
                UserTimer userTimer = subject as UserTimer;
                string boxName = userTimer.State.ToString();
                string timerId = userTimer.TimerId;
                string[] sArray = timerId.Split('_');
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} MachineName =[{3}],ProductName =[{4}] ,BoxName =[{5}]. ModChangerVCRReadReply timeout.",
                       sArray[0], sArray[3], MethodBase.GetCurrentMethod().Name, sArray[1], sArray[2], boxName));
                //TODO Time out

            }catch (System.Exception ex){
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //ModChangerVCRReadReply
        public void MES_ModChangerVCRReadReply(XmlDocument doc) {
            try{
                string trxId = GetTransactionID(doc);
                XmlNode bodyNode = doc[keyHost.MESSAGE][keyHost.BODY];
                string lineName = bodyNode[keyHost.LINENAME].InnerText;
                string machineName = bodyNode[keyHost.MACHINENAME].InnerText;
                string productName = bodyNode[keyHost.PRODUCTNAME].InnerText;
                string timerId = string.Format("{0}_{1}_{2}_{3}_ModChangerVCRReadReply", lineName, machineName, productName, trxId);
                if (Timermanager.IsAliveTimer(timerId)) {
                    Timermanager.TerminateTimer(timerId);
                }
                string productSpecName = bodyNode[keyHost.PRODUCTSPECNAME].InnerText;
                string productRecipeName = bodyNode[keyHost.PRODUCTRECIPENAME].InnerText;
                string processOperationName = bodyNode[keyHost.PROCESSOPERATIONNAME].InnerText;

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} MachineName =[{3}],ProductName =[{4}] . receive ModChangerVCRReadReply from MES.",
                     lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, productName));

                Job job = ObjectManager.JobManager.GetJob(productName);
                if (job == null) {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[LINENAME={0}] [BCS <- MES][{1}] {2} Can't find the ProductName is {3} Job.",
                     lineName, trxId, MethodBase.GetCurrentMethod().Name, machineName, productName));
                    return;
                }
                //TODO FInd Job Hndlers
                


            }catch (System.Exception ex){
                LogError(MethodBase.GetCurrentMethod().Name+"()", ex);
            }
        }
        //ModValidateBoxRequest
        //ModValidateBoxReply
        //ModValidateCassetteRequest
        //ModValidateCassetteReply
        //ModValidatePalletRequest
        //ModValidatePalletReply
        //ModPalletProcessEnd
        //ModPalletProcessEndReply
        //ModVCRReadReport
        //ModVCRReadRequest
        //ModVCRReadReply
        //ValidateBLRequest
        //ValidateBLReply
        //ValidatePanelRequest
        //ValidatePanelReply
        //ValidateAssembleRequest
        //ValidateAssembleReply
        //AssemblyRequest
        //AssemblyReply
        //WorkStationProcessReport
        //OLBCheckReport
        //AOIInspectionReport
        //MaterialChangedReport
        //TrayMountRequest
        //TrayMountReply

        public override bool Init()
        {
            return true;
        }
    }
}
