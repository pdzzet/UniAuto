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
        /// 6.45.	FacilityCriteriaSend        MES MessagetSet : Daily Check frequency Download Facility Criteria Send to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_FacilityCriteriaSend(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //TODO:判斷邏輯需修改
                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string qtime = xmlDoc["MESSAGE"]["BODY"]["QTIME"].InnerText;

                //Add by marine for MES 2015/7/16
                XmlNodeList machineList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINELIST].ChildNodes;
                foreach (XmlNode node in machineList)
                {
                    string machineName = node[keyHost.MACHINENAME].InnerText;

                    XmlNodeList facilityParaList = node[keyHost.FACILITYPARALIST].ChildNodes;
                    foreach (XmlNode facilitynode in facilityParaList)
                    {
                        string paraName = facilitynode[keyHost.PARANAME].InnerText;
                        string valueType = facilitynode[keyHost.VALUETYPE].InnerText;
                        XmlNodeList limitList = facilitynode[keyHost.LIMITLIST].ChildNodes;
                        foreach (XmlNode limitlistnode in limitList)
                        {
                            string machineStateName = limitlistnode[keyHost.MACHINESTATENAME].InnerText;
                            string upperLimit = limitlistnode[keyHost.UPPERLIMIT].InnerText;
                            string loeerLimit = limitlistnode[keyHost.LOWERLIMIT].InnerText;
                        }
                    }
                }


                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityCriteriaSend NG LINENAME =[{1}],QTIME={2},CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, qtime, returnCode, returnMessage));
                    //to do?
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityCriteriaSend OK LINENAME =[{1}],QTIME={2},CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, qtime, returnCode, returnMessage));
                }

                double intQ;
                if (double.TryParse(qtime, out intQ))
                {
                    if (line == null) throw new Exception(string.Format("Can't find Line =[{0}] in LineEntity!", lineName));

                    line.File.DailyCheckIntervalS = intQ * 3600;

                    ObjectManager.LineManager.EnqueueSave(line.File);
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityCriteriaSend QTIME Format is not Digital, LINENAME =[{1}],QTIME={2},CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, qtime, returnCode, returnMessage));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.46.	FacilityParameterRequest        MES MessagetSet : Facility Parameter Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_FacilityParameterRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                //Watson Add 2014117 For MES 要求
                string inboxName = GetMESINBOXName(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //TODO:判斷邏輯需修改
                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }


                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityParameterRequest NG LINENAME =[{1}],CODE={2},MESSAGE={3}.",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityParameterRequest OK LINENAME =[{1}],CODE={2},MESSAGE={3}.",
                                        trxID, lineName, returnCode, returnMessage));
                }

                Invoke("DailyCheckService", "DailyCheckForceCheck", new object[] { line, lineName, inboxName });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.47.	FacilityParameterReply      MES MessageSet : Reports all facility parameter items registered in EQP to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void FacilityParameterReply(string trxID, string lineName, IList<FacilityParameterReply.MACHINEc> machineList, string mesINBOXName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //Watson Modify 20150320 PMT Special Rule
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityParameterReply Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("FacilityParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                //Watson Add 20141117 For MES 要求
                SetINBOXNAME(xml_doc, mesINBOXName);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

                XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode eqpCloneNodeClone = eqpListNode[keyHost.MACHINE].Clone();
                eqpListNode.RemoveAll();

                //Jun Modify 20141203 DailyCheckData Table沒有資料時 Reply NG 給MES
                if (machineList == null || machineList.Count == 0)
                {
                    xml_doc[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText = "1";
                    xml_doc[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText = string.Format("Can't Get LineName=[{0}] Facility Parameter!", lineName);
                    SendToMES(xml_doc);
                    return;
                }

                Equipment eqp = null;
                foreach (var machine in machineList)
                {
                    XmlNode eqpCloneNode = eqpCloneNodeClone.Clone();
                    eqp = ObjectManager.EquipmentManager.GetEQPByID(machine.MACHINENAME);
                    eqpCloneNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    #region Watson Add 20150314 For CELL PMT Type PTI Line Report DailyCheck Data
                    if (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                    {
                        if (eqp.Data.NODENO == "L2")
                        {
                            eqpCloneNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString(); ;
                        }
                    }
                    #endregion
                    eqpCloneNode[keyHost.MACHINESTATENAME].InnerText = machine.MACHINESTATENAME;

                    XmlNode paraNodeList = eqpCloneNode[keyHost.FACILITYPARALIST];
                    XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
                    paraNodeList.RemoveAll();

                    foreach (var para in machine.FACILITYPARALIST)
                    {
                        XmlNode paraNode = paraNodeClone.Clone();
                        paraNode[keyHost.RECIPEID].InnerText = para.RECIPEID;
                        paraNode[keyHost.PARANAME].InnerText = para.PARANAME;
                        paraNode[keyHost.PARAVALUE].InnerText = para.PARAVALUE;
                        paraNode[keyHost.VALUETYPE].InnerText = para.VALUETYPE;

                        paraNodeList.AppendChild(paraNode);
                    }

                    eqpListNode.AppendChild(eqpCloneNode);
                }

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityParameterReply OK LINENAME =[{1}].",
                        trxID, lineName));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        /// <summary>
        ///  6.48.	FacilityCheckReport     MES MessageSet : BC reports to MES facility parameter check result,
        ///  According to daily check specified by frequency.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="nodelist">Node List</param>
        public void FacilityCheckReport(string trxID, string lineName, IList<FacilityCheckReport.MACHINEc> machineList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                //Watson Modify 20150320 PMT Special Rule
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityCheckReport Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("FacilityCheckReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode eqpCloneNodeClone = eqpListNode[keyHost.MACHINE].Clone();
                eqpListNode.RemoveAll();

                foreach (var machine in machineList)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machine.MACHINENAME);
                    XmlNode eqpCloneNode = eqpCloneNodeClone.Clone();
                    eqpCloneNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    #region Watson Add 20150314 For CELL PMT Type PTI Line Report DailyCheck Data
                    if (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                    {
                        if (eqp.Data.NODENO == "L2")
                        {
                            eqpCloneNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString(); ;
                        }
                    }
                    #endregion

                    eqpCloneNode[keyHost.MACHINESTATENAME].InnerText = machine.MACHINESTATENAME;
                    //eqpCloneNode[keyHost.RECIPEID].InnerText = machine.RECIPEID;

                    XmlNode paraNodeList = eqpCloneNode[keyHost.FACILITYPARALIST];
                    XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
                    paraNodeList.RemoveAll();

                    foreach (var para in machine.FACILITYPARALIST)
                    {
                        XmlNode paraNode = paraNodeClone.Clone();
                        paraNode[keyHost.RECIPEID].InnerText = para.RECIPEID;
                        paraNode[keyHost.PARANAME].InnerText = para.PARANAME;
                        paraNode[keyHost.PARAVALUE].InnerText = para.PARAVALUE;
                        paraNode[keyHost.VALUETYPE].InnerText = para.VALUETYPE;

                        paraNodeList.AppendChild(paraNode);
                    }

                    eqpListNode.AppendChild(eqpCloneNode);
                }

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityCheckReport OK LINENAME =[{1}].",
                        trxID, lineName));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.49.	FacilityCheckRequest        MES MessagetSet : Facility Check Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_FacilityCheckRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityCheckRequest OK LINENAME =[{1}],CODE={2},MESSAGE={3}.",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] FacilityCheckRequest  OK LINENAME =[{1}],CODE={2},MESSAGE={3}.",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MOdify for T3 MES 2015/8/20
        ///// <summary>
        ///// 6.50.	FacilityCheckReply       MES MessageSet : BC reports to MES facility parameter check result as a reply for FacityCheckRequest
        ///// </summary>
        ///// <param name="trxID">yyyyMMddHHmmssffff</param>
        ///// <param name="lineName">LineID</param>
        ///// <param name="valiresult"></param>
        //public void FacilityCheckReply(string trxID, string lineName, IList<Equipment> nodelist)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityCheckReport Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
        //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("FacilityCheckReply") as XmlDocument;
        //        GetTransactionID(xml_doc);

        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
        //        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

        //        XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
        //        XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
        //        eqpListNode.RemoveAll();

        //        foreach (Equipment eqp in nodelist)
        //        {
        //            XmlNode eqpNode = eqpCloneNode.Clone();
        //            eqpNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
        //            eqpNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus.ToString();
        //            eqpNode[keyHost.VALIRESULT].InnerText = "";
        //        }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityCheckReply OK LINENAME =[{1}].",
        //            trxID, lineName));

        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 6.52.	FacilityCheckReply  MES MessageSet : BC reports to MES facility parameter check result as a reply for FacityCheckRequest
        /// Add by marine for T3 MES 2015/8/20
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="machineList"></param>
        public void FacilityCheckReply(string trxID, string lineName, IList<FacilityCheckReply.MACHINEc> machineList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("FacilityCheckReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode eqpCloneNodeClone = eqpListNode[keyHost.MACHINE].Clone();
                eqpListNode.RemoveAll();

                foreach (var machine in machineList)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machine.MACHINENAME);
                    XmlNode eqpCloneNode = eqpCloneNodeClone.Clone();
                    eqpCloneNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    eqpCloneNode[keyHost.MACHINESTATENAME].InnerText = machine.MACHINESTATENAME;

                    XmlNode paraNodeList = eqpCloneNode[keyHost.FACILITYPARALIST];
                    XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
                    paraNodeList.RemoveAll();

                    foreach (var para in machine.FACILITYPARALIST)
                    {
                        XmlNode paraNode = paraNodeClone.Clone();
                        paraNode[keyHost.RECIPEID].InnerText = para.RECIPEID;
                        paraNode[keyHost.PARANAME].InnerText = para.PARANAME;
                        paraNode[keyHost.PARAVALUE].InnerText = para.PARAVALUE;
                        paraNode[keyHost.VALUETYPE].InnerText = para.VALUETYPE;

                        paraNodeList.AppendChild(paraNode);
                    }

                    eqpListNode.AppendChild(eqpCloneNode);
                }

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] FacilityCheckReport OK LINENAME =[{1}].",
                        trxID, lineName));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


    }
}
