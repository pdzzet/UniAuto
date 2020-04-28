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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        /// <summary>
        /// 6.27.	ChangePlanAborted      MES MessageSet : BC requests MES to abort the current Change Plan 
        /// Add by marine for MES 2015/7/11
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="curPlanName"></param>
        /// <param name="reasoncode"></param>
        /// <param name="description"></param>
        /// <param name="nodelist"></param>
        public void ChangePlanAborted(string trxID, string lineName, string curPlanName, string reasonCode, string description, string carrierName)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangePlanAborted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CURRENTPLANNAME].InnerText = curPlanName;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.DESCRIPTION].InnerText = description;

                XmlNode reasonCarrilerList = bodyNode[keyHost.REASONCARRILERLIST];

                //Modify by marine for T3 MES 2015/9/11
                XmlNode reasonCarriler = reasonCarrilerList[keyHost.REASONCARRILER];
                reasonCarrilerList.RemoveAll();

                XmlNode reasonCarrilerClone = reasonCarriler.Clone();
                reasonCarrilerClone[keyHost.CARRIERNAME].InnerText = carrierName;
                reasonCarrilerList.AppendChild(reasonCarrilerClone);

                SendToMES(xml_doc);

                lock (line.File)//add by hujunpeng 20190522 for auto change DCR status
                {
                    if (line.File.PlanVCR.ContainsKey(curPlanName))
                        line.File.PlanVCR.Remove(curPlanName);
                    ObjectManager.LineManager.EnqueueSave(line.File);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] ChangePlanAborted OK LINENAME=[{1}],CARRIERNAME=[{2}],CURRENTPLANNAME=[{3}].",
                    trxID, lineName, carrierName, curPlanName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.28.	ChangePlanCanceled      MES Message : BC requests MES to cancel the current Change Plan
        /// Add by marine for MES 2015/7/11
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="curPlanName"></param>
        /// <param name="reasoncode"></param>
        /// <param name="description"></param>
        /// <param name="nodelist"></param>
        public void ChangePlanCanceled(string trxID, string lineName, string curPlanName, string reasonCode, string description, string carrierName)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangePlanCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CURRENTPLANNAME].InnerText = curPlanName;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.DESCRIPTION].InnerText = description;

                XmlNode reasonCarrilerList = bodyNode[keyHost.REASONCARRILERLIST];
                //Modify by marine for T3 MES 2015/9/11
                XmlNode reasonCarriler = reasonCarrilerList[keyHost.REASONCARRILER];
                reasonCarrilerList.RemoveAll();

                XmlNode reasonCarrilerClone = reasonCarriler.Clone();
                reasonCarrilerClone[keyHost.CARRIERNAME].InnerText = carrierName;
                reasonCarrilerList.AppendChild(reasonCarrilerClone);

                SendToMES(xml_doc);
                lock (line.File)//add by hujunpeng 20190522 for auto change DCR status
                {
                    if (line.File.PlanVCR.ContainsKey(curPlanName))
                        line.File.PlanVCR.Remove(curPlanName);
                    ObjectManager.LineManager.EnqueueSave(line.File);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] ChangePlanCancled OK LINENAME=[{1}],CARRIERNAME=[{2}],CURRENTPLANNAME=[{3}].",
                    trxID, lineName, carrierName, curPlanName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.29.	ChangePlanRequest        MES MessageSet : BC requests MES to provide Glass Change Plan
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="cSTID">provide this value when BC requests for Change Plan associated with a specific source cassette, otherwise leave it blank.</param>
        /// <param name="curPlanName">provide current Change Plan ID when current Change Plan has been completed and requests for the next Change Plan, otherwise leave it blank.</param>
        public void ChangePlanRequest(string trxID, string lineName, string cSTID, string curPlanName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] ChangePlanRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangePlanRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CARRIERNAME].InnerText = cSTID;
                bodyNode[keyHost.CURRENTPLANNAME].InnerText = curPlanName;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] ChangePlanRequest OK LINENAME=[{1}],CARRIERNAME=[{2}],CURRENTPLANNAME=[{3}].",
                    trxID, lineName, cSTID, curPlanName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.30.	ChangePlanReply         MES MessagetSet : Change Plan Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ChangePlanReply(XmlDocument xmlDoc)
        {
            try
            {
                Line line;
                List<string> sourceList = new List<string>();
                List<string> targetList = new List<string>();
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);
                string planID = string.Empty;
                string VCRFLAG = string.Empty;//add by hujunpeng 20190522 for auto change DCR status
                string changeplantype;
                eReturnCode1 plcRetCode = eReturnCode1.OK;

                line = ObjectManager.LineManager.GetLine(lineName);
                planID = body[keyHost.NEXTPLANNAME].InnerText.Trim();
                VCRFLAG = body[keyHost.VCRFLAG].InnerText.Trim() == null ? "N" : body[keyHost.VCRFLAG].InnerText.Trim();//add by hujunpeng 20190522 for auto change DCR status 
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[LINENAME={1}] [{0}] " + " current planID=[{2}],current VCRFLAG=[{3}]",
                     trxID, lineName, planID,VCRFLAG));
                changeplantype = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CHANGEPLANTYPE].InnerText;

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] MES Reply LineName=[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                    plcRetCode = eReturnCode1.NG;
                }
                else if (line == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES][{1}] CAN'T FIND LINENAME.", lineName, trxID));
                    plcRetCode = eReturnCode1.NG;
                }
                else if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] ChangePlanReply NG LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, planID, returnCode, returnMessage));
                    plcRetCode = eReturnCode1.NG;
                }
                else if (planID == null || planID.Length == 0)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] ChangePlanReply NEXTPLANNAME NG LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, planID, returnCode, returnMessage));
                    plcRetCode = eReturnCode1.NG;
                }

                IList<SLOTPLAN> plans = new List<SLOTPLAN>();
                XmlNodeList sourceNodeList = body[keyHost.SOURCELIST].ChildNodes;
                XmlNodeList targetNodeList = body[keyHost.TARGETLIST].ChildNodes;
                foreach (XmlNode source in sourceNodeList)
                {
                    sourceList.Add(source[keyHost.CARRIERNAME].InnerText.Trim());
                    XmlNodeList productNodeList = source[keyHost.PRODUCTLIST].ChildNodes;
                    foreach (XmlNode product in productNodeList)
                    {
                        targetList.Add(product[keyHost.PAIRCARRIERNAME].InnerText);
                    }
                }

                if (plcRetCode == eReturnCode1.OK)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}][BCS <- MES][{0}] ChangePlanReply OK LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                            trxID, lineName, planID, returnCode, returnMessage));
                    //for T3 MES Spec 1.12
                    foreach (XmlNode target in targetNodeList)
                    {
                        string targetCstID = target[keyHost.CARRIERNAME].InnerText.Trim();
                        string targetHoldState = target[keyHost.HOLDSTATE].InnerText;
                        if (targetHoldState.Trim() == "Y")
                        {
                            plcRetCode = eReturnCode1.NG;
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("NEXTPLANNAME =[{0}], TARGET CASSETTEID =[{1}] HOLDSTATE =[Y]",
                                        planID, targetCstID));
                            break;
                        }
                    }
                    /*string targetCstID = targetNodeList[0].InnerText;
                    string targetHoldState = targetNodeList[1].InnerText;
                    if (targetHoldState.Trim() == "Y")
                    {
                        plcRetCode = eReturnCode1.NG;
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("NEXTPLANNAME =[{0}], TARGET CASSETTEID =[{1}] HOLDSTATE =[Y]",
                                    planID, targetCstID));
                    }*/

                    foreach (XmlNode source in sourceNodeList)
                    {
                        if (plcRetCode == eReturnCode1.NG)
                            break;
                        string sourceCstID = source[keyHost.CARRIERNAME].InnerText.Trim();
                        string sourceHoldState = source[keyHost.HOLDSTATE].InnerText;
                        if (sourceHoldState.Trim() == "Y")
                        {
                            plcRetCode = eReturnCode1.NG;
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("NEXTPLANNAME =[{0}], SOURCE CASSETTEID =[{1}] HOLDSTATE =[Y]",
                                        planID, sourceCstID));
                            break;
                        }

                        XmlNodeList productNodeList = source[keyHost.PRODUCTLIST].ChildNodes;
                        foreach (XmlNode product in productNodeList)
                        {
                            SLOTPLAN plan = new SLOTPLAN();
                            plan.SLOTNO = 0;
                            plan.HAVE_BEEN_USED = false;
                            plan.PRODUCT_NAME = product[keyHost.PRODUCTNAME].InnerText;
                            plan.SOURCE_CASSETTE_ID = sourceCstID;
                            plan.TARGET_CASSETTE_ID = product[keyHost.PAIRCARRIERNAME].InnerText;
                            if (changeplantype.Trim().Equals("MANUAL"))
                            {
                                if (product[keyHost.PAIRCARRIERSLOT].InnerText.Trim().Length == 0)
                                {
                                    plcRetCode = eReturnCode1.NG;
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("NEXTPLANNAME =[{0}], MANUAL MODE BUT TARGET SLOT NOT DEFINE!",
                                        planID));
                                    break;
                                }
                                else
                                {
                                    plan.TARGET_SLOTNO = product[keyHost.PAIRCARRIERSLOT].InnerText;
                                }
                            }
                            else
                            {
                                plan.TARGET_SLOTNO = "000";
                            }

                            plans.Add(plan);
                        }
                    }

                    if (plans.Count == 0)
                    {
                        plcRetCode = eReturnCode1.NG;
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] ChangePlanReply Item Length = 0, LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, planID, returnCode, returnMessage));
                    }
                    else
                    {
                        //取得source CST & target CST, check total count <= L2's port count
                        List<string> lsSourceCassette = (from slot in plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID).ToList<string>();
                        List<string> lsTargetCassette = (from slot in plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID).ToList<string>();
                        if ((lsSourceCassette.Count + lsTargetCassette.Count) > ObjectManager.PortManager.GetPorts(ObjectManager.EquipmentManager.GetEQP("L2").Data.NODEID).Count)
                        {
                            plcRetCode = eReturnCode1.NG;
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES][{0}] ChangePlanReply Cassettes > L2's Total Ports, LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                            trxID, lineName, planID, returnCode, returnMessage));
                        }
                    }

                    if (plcRetCode != eReturnCode1.NG)
                    {
                        string existPlanID = string.Empty;
                        string exitStandbyPlanID = string.Empty;
                        IList<SLOTPLAN> existCruuPlan = ObjectManager.PlanManager.GetProductPlans(out existPlanID);
                        IList<SLOTPLAN> existStandbyPlan = ObjectManager.PlanManager.GetProductPlansStandby(out exitStandbyPlanID);
                        object[] obj = new object[]
                        {
                            line,
                            plans,
                            false,
                            trxID
                        };

                        if (planID.Trim() == existPlanID.Trim()) //don't add the duplicate plan & don't cancel the duplicate plan 2016/05/06 cc.kuang
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] Plan ID is Same with Current Plan, LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, planID, returnCode, returnMessage));
                        }
                        else if (existCruuPlan == null || existCruuPlan.Count == 0)
                        {
                            obj[2] = true;
                            ObjectManager.PlanManager.AddOnlinePlans(planID, plans);
                            line.File.CurrentPlanID = planID;
                            line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            Invoke(eServiceName.PortService, "CassetteBondToPort", obj);
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                        else if (existStandbyPlan == null || existStandbyPlan.Count == 0)
                        {
                            ObjectManager.PlanManager.AddOnlinePlansStandby(planID, plans);
                            Invoke(eServiceName.PortService, "CassetteBondToPort", obj);
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                        else
                        {
                            //plcRetCode = eReturnCode1.NG;
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] CrrrPlan & StandbyPlan are Exist, LINENAME =[{1}],NEXTPLANNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, planID, returnCode, returnMessage));
                        }
                    }
                }

                if (plcRetCode != eReturnCode1.OK)
                {
                    object[] obj = new object[]
                        {
                            sourceList,
                            targetList,
                            1,
                            trxID
                        };
                    Invoke(eServiceName.PortService, "CassettePlanCancel", obj);
                }
                else//add by hujunpeng 20190522 for auto change DCR status
                {
                    lock (line.File)
                    {
                        line.File.PlanVCR = new Dictionary<string, string>();
                        line.File.PlanVCR.Add(planID, VCRFLAG);
                        ObjectManager.LineManager.EnqueueSave(line.File);
                    }
                }
               

                /* t3 changer not reply to indexer cc.kuang 2015/07/30
                object[] obj = new object[]
                {
                    plcRetCode,
                    lineName,
                    planID,
                    sourceList,
                    targetList,
                    trxID
                };
                Invoke(eServiceName.EquipmentService, "ChangerPlanRequestReportReply_On", obj);
                */
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.31.	ChangePlanStarted       MES MessageSet : Change Plan Start Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="planName">CURRENTPLANNAME is Change Plan ID  Unique 6 characters</param>
        public void ChangePlanStarted(string trxID, string lineName, string planName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangePlanStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CURRENTPLANNAME].InnerText = planName;

                SendToMES(xml_doc);               
                
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
