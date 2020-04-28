using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.MesSpec;
using System.Collections;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
 

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {

        ///// <summary>
        ///// MES MessageSet : Check Facility Request to MES 
        ///// </summary>
        ///// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        ///// <param name="lineName">Line ID</param>
        //public void CheckFacilityReport(string trxID, string lineName, IList<CheckFacilityReport.MACHINEc> machineList)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("CheckFacilityReport") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫

        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
        //        bodyNode[keyHost.LINENAME].InnerText = lineName;

        //        XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
        //        XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
        //        eqpListNode.RemoveAll();

        //        foreach (CheckFacilityReport.MACHINEc machine in machineList)
        //        {
        //            XmlNode eqpNode = eqpCloneNode.Clone();
        //            eqpNode[keyHost.MACHINENAME].InnerText = machine.MACHINENAME;

        //            XmlNode paraNodeList = eqpNode[keyHost.FACILITYPARALIST];
        //            XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
        //            paraNodeList.RemoveAll();

        //            foreach (var para in machine.FACILITYPARALIST)
        //            {
        //                XmlNode paraNode = paraNodeClone.Clone();
        //                paraNode[keyHost.PARANAME].InnerText = para.PARANAME;
        //                paraNode[keyHost.VALUETYPE].InnerText = para.VALUETYPE;

        //                paraNodeList.AppendChild(paraNode);
        //            }
        //            eqpListNode.AppendChild(eqpNode);
        //        }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 6.51.	GlassChangeMACOJudge        MES MessageSet : Glass Change MACRO Judge
        /// </summary>
        public void GlassChangeMACOJudge(string trxID, string lineName, Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("GlassChangeMACOJudge") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                XmlNode originalProductList = bodyNode[keyHost.ORIGINALPRODUCTLIST];
                XmlNode originalProductNode = originalProductList[keyHost.ORIGINALPRODUCT].Clone();
                originalProductList.RemoveAll();

                originalProductNode[keyHost.ORIGINALPRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();

                XmlNode productList = originalProductNode[keyHost.PRODUCTLIST];
                XmlNode productNode = productList[keyHost.PRODUCT].Clone();
                productList.RemoveAll();

                foreach (DefectCode dc in job.DefectCodes)
                {
                    XmlNode obj = productNode.Clone();
                    int chip = int.Parse(dc.ChipPostion);
                    obj[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID + chip.ToString("X2");
                    if (chip.Equals(0)) continue;
                    obj[keyHost.MACOJUDGE].InnerText = job.OXRInformation.Substring(chip - 1, 1);
                    obj[keyHost.MURACODES].InnerText = dc.DefectCodes;
                    obj[keyHost.EVENTCOMMENT].InnerText = "REPORT By EQ";
                    productList.AppendChild(obj);
                }
                originalProductList.AppendChild(originalProductNode);
                bodyNode[keyHost.EVENTUSER].InnerText = lineName;
                bodyNode[keyHost.EVENTCOMMENT].InnerText = "REPORT By EQ";
                bodyNode[keyHost.LANGUAGE].InnerText = "ENGLISH";

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> MES][{1}] SEND MES TRX \"{2}\".", lineName, trxID, MethodBase.GetCurrentMethod().Name));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

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
            string productSpecName, string productSpecVer, string processOperationName, string lineRecipeName, IList<ProductProcessData.ITEMc> itemList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return; 

                //WATSON mODIFY 20150320 FOR PMT SPECIAL RULE 改在下面再判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].",
                //        //trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}
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

                foreach (ProductProcessData.ITEMc item in itemList)
                {
                    XmlNode itemnode = itemNode.Clone();
                    itemnode[keyHost.ITEMNAME].InnerText = item.ITEMNAME;

                    //If item doesn’t have any site, BC should report SITENAME with ‘DEFAULT’.
                    if (item.SITELIST.Count == 0)
                    {
                        XmlNode sitelistnode = siteListNode.Clone();
                        itemnode.RemoveChild(itemnode[keyHost.SITELIST]);
                        XmlNode sitenode = siteNode.Clone();
                        sitenode[keyHost.SITENAME].InnerText = "DEFAULT";
                        sitelistnode.AppendChild(sitenode);
                        itemnode.AppendChild(sitelistnode);
                    }
                    else
                    {
                        //SITELIST
                        XmlNode sitelistnode = siteListNode.Clone();
                        itemnode.RemoveChild(itemnode[keyHost.SITELIST]);
                        foreach (ProductProcessData.SITEc site in item.SITELIST)
                        {
                            XmlNode sitenode = siteNode.Clone();
                            sitenode[keyHost.SITENAME].InnerText = site.SITENAME;
                            sitenode[keyHost.SITEVALUE].InnerText = site.SITEVALUE;
                            sitelistnode.AppendChild(sitenode);
                        }
                        itemnode.AppendChild(sitelistnode);
                    }

                    itemListNode.AppendChild(itemnode);
                }

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                }
                else
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                            trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                        bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        if (unitName.Trim() != string.Empty)
                            bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        SendToMES(xml_doc);

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                                trxID, bodyNode[keyHost.LINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion



            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// MES MessagetSet : Glass Process Start Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_GlassProcessStartReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                //EX: string machineName = xmlDoc["MESSAGE"]["BODY"]["MACHINENAME"].InnerText;

                if (returnCode != "0")
                {
                    //Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[BCS <- MES]=[{0}] " + GetMESMessageName(xmlDoc) + "  OK LINENAME =[{1}],MACHINENAME =[{2}],PAUSECOMMAND={3},CODE={4},MESSAGE={5}.",
                    //                    trxID, lineName, , returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    //Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[BCS <- MES]=[{0}] " + GetMESMessageName(xmlDoc) + "  OK LINENAME =[{1}],MACHINENAME =[{2}],PAUSECOMMAND={3},CODE={4},MESSAGE={5}.",
                    //                    trxID, lineName, machineName, pausecmd, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }

        /// <summary>
        /// MES MessagetSet : Glass Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_GlassProcessEndReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                //EX: string machineName = xmlDoc["MESSAGE"]["BODY"]["MACHINENAME"].InnerText;

                if (returnCode != "0")
                {
                    //Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[BCS <- MES]=[{0}] " + GetMESMessageName(xmlDoc) + "  OK LINENAME =[{1}],MACHINENAME =[{2}],PAUSECOMMAND={3},CODE={4},MESSAGE={5}.",
                    //                    trxID, lineName, , returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    //Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[BCS <- MES]=[{0}] " + GetMESMessageName(xmlDoc) + "  OK LINENAME =[{1}],MACHINENAME =[{2}],PAUSECOMMAND={3},CODE={4},MESSAGE={5}.",
                    //                    trxID, lineName, machineName, pausecmd, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
