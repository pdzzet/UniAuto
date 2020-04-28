using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
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
using System.Collections.Concurrent;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class ModMESService : AbstractService
    {
        private ConcurrentQueue<xMessage> _equipmentStateQueue = new ConcurrentQueue<xMessage>();
        private DateTime _eqpStateLastReportTime = DateTime.Now;
        private object _timeSync = new object();
        private ConcurrentQueue<xMessage> _processEndQueue = new ConcurrentQueue<xMessage>();
        private DateTime _processEndLastReportTime = DateTime.Now;
        private System.Timers.Timer _queueTimer = new System.Timers.Timer(500);
        private bool _waitForMachineStateChangeReply = false;

        //CheckRecipeParameter
        /// <summary>
        /// CheckRecipeParameter         MES MessageSet : Upon MES request of EQ recipe parameter validation, BC reports EQ recipe parameters.
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="portid"></param>
        /// <param name="recipeCheckInfos"></param>
        public void CheckRecipeParameter(string trxID, Port port, string portid, string carrierNmae, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckRecipeParameter") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portid;
                bodyNode[keyHost.CARRIERNAME].InnerText = carrierNmae;
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                XmlNode recipeparaList = bodyNode[keyHost.RECIPEPARALIST];

                foreach (string key in recipeCheckInfos.Keys)
                {
                    IList<RecipeCheckInfo> rcis = recipeCheckInfos[key];

                    // Key: Recipe Name  Value: RecipeCheckInfo 重新整理後再報出
                    Dictionary<string, List<RecipeCheckInfo>> okRecipeInfo = new Dictionary<string, List<RecipeCheckInfo>>();

                    if (okRecipeInfo.Count() > 0)
                    {
                        foreach (string key2 in okRecipeInfo.Keys)
                        {
                            List<RecipeCheckInfo> tmp = okRecipeInfo[key2];


                            for (int i = 0; i < tmp.Count(); i++)
                            {
                                XmlNode paraNodeClone = recipeparaList[keyHost.PARA].Clone();

                                recipeparaList.RemoveAll();
                                foreach (string key3 in tmp[i].Parameters.Keys)
                                {
                                    XmlNode para = paraNodeClone.Clone();
                                    para[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, key3); //Watson add 20141118 For new spec
                                    para[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, tmp[i].EQPNo, key3); //配合RecipeService GetRecipeParameter Decode Rule
                                    para[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, key3);
                                    para[keyHost.PARAVALUE].InnerText = tmp[i].Parameters[key3];
                                    recipeparaList.AppendChild(para);
                                }
                            }
                        }
                    }

                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] CheckRecipeParameter OK LINENAME =[{1}],CARRIERNAME =[{2}].",
                    trxID, port.Data.LINEID, port.File.CassetteID));
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CheckRecipeParameter(string trxID, Line line, string portid, string carrierNmae, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos)
        {
            try
            {
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckRecipeParameter") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portid;
                bodyNode[keyHost.CARRIERNAME].InnerText = carrierNmae;
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";

                XmlNode recipeparaList = bodyNode[keyHost.RECIPEPARALIST];

                foreach (string key in recipeCheckInfos.Keys)
                {
                    IList<RecipeCheckInfo> rcis = recipeCheckInfos[key];

                    // Key: Recipe Name  Value: RecipeCheckInfo 重新整理後再報出
                    Dictionary<string, List<RecipeCheckInfo>> okRecipeInfo = new Dictionary<string, List<RecipeCheckInfo>>();

                    if (okRecipeInfo.Count() > 0)
                    {
                        foreach (string key2 in okRecipeInfo.Keys)
                        {
                            List<RecipeCheckInfo> tmp = okRecipeInfo[key2];


                            for (int i = 0; i < tmp.Count(); i++)
                            {
                                XmlNode paraNodeClone = recipeparaList[keyHost.PARA].Clone();

                                recipeparaList.RemoveAll();
                                foreach (string key3 in tmp[i].Parameters.Keys)
                                {
                                    XmlNode para = paraNodeClone.Clone();
                                    para[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, key3); //Watson add 20141118 For new spec
                                    para[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, tmp[i].EQPNo, key3); //配合RecipeService GetRecipeParameter Decode Rule
                                    para[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, key3);
                                    para[keyHost.PARAVALUE].InnerText = tmp[i].Parameters[key3];
                                    recipeparaList.AppendChild(para);
                                }
                            }
                        }
                    }

                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] CheckRecipeParameter OK LINENAME =[{1}].",
                    trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// MES 多了TRACELEVEL 要上報，只能從ParaName的結構去判斷是不是unit或machine，此案不會有port
        /// </summary>
        /// <param name="paraName">Recipe Parameter Name</param>
        /// <returns>‘M’ – Machine ;‘U’ – Unit </returns>
        public string GetPARANAME_TRACELEVEL(string lineName, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName); //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                int p = 0;
                p = paraName.IndexOf('@');
                if (p > 0)
                    return "U";  //檢查到@表示有unitid，直接傳回@後面的值(name)
                else
                    return "M";
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return "M";
            }
        }

        /// <summary>
        /// Recipe ParaName Rule is EQID@UNITID^PARANAME
        /// </summary>
        /// <param name="paraName">EQID@UNITID^PARANAME</param>
        /// <returns>PARANAME</returns>
        public string GetPARANAME(string lineName, string eqpNO, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName);  //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    if (lineName.Contains(keyCELLPMTLINE.CBPTI) && (eqpNO == "L2"))
                    {
                        if (paraName.Length > 8)
                        {
                            string pmtEQPID = paraName.Substring(0, 8);
                            paraName = paraName.Replace(pmtEQPID, ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString());
                        }
                    }
                }

                return (paraName.Replace('@', '_').Replace('^', '_'));
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return paraName;
            }
        }

        /// <summary>
        /// //配合RecipeService GetRecipeParameter Decode Rule (EQID@UNITID^PARANAME or EQID@PARANAME)
        /// </summary>
        /// <param name="paraName">EQPID_UNITID_ParaName or EQPID_ParaName</param>
        /// <returns>ParaName</returns>
        public string GetPARANAMEORIENTED(string lineName, string paraName)
        {
            try
            {
                if (paraName == "") return paraName;

                Line line = ObjectManager.LineManager.GetLine(lineName); //Watson Modify 20150302 不可以用servername去取得
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.CF:
                            paraName = "F" + paraName.Substring(1).ToString(); break;
                        case eUPKEquipmentRunMode.TFT:
                            paraName = "T" + paraName.Substring(1).ToString(); break;
                    }
                }

                int p = 0;
                p = paraName.IndexOf('@');
                if (p > 0)
                    return (paraName.Substring(p + 1, paraName.Length - p - 1));  //檢查到@表示有unitid，直接傳回@後面的值(name)
                p = paraName.IndexOf('^');  //查到^表示只有eqpid，傳回^後面的值(name)
                return (paraName.Substring(p + 1, paraName.Length - p - 1));
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return paraName;
            }
        }

        //CheckRecipeParameterReply
        /// <summary>
        /// CheckRecipeParameterReply       MES MessagetSet : Check Recipe Parameter Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CheckRecipeParameterReply(XmlDocument xmlDoc)
        {
            try
            {

                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                string lineName = xmlDoc.SelectSingleNode("//MESSAGE/BODY/LINELIST/LINE/LINENAME").InnerText;
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINELIST][keyHost.LINE][keyHost.PORTNAME].InnerText;
                string cstid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINELIST][keyHost.LINE][keyHost.CARRIERNAME].InnerText;
                string result = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                if (returnCode != "0" || result != "Y")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES] =[{0}] MES_CheckRecipeParameterReply NG  LINENAME =[{1}],PORTNAME =[{2}],CARRIERNAME =[{3}] Validation NG.",
                                         trxID, lineName, portid, cstid));

                    Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid);

                    Cassette cst = ObjectManager.CassetteManager.GetCassette(cstid);
                    if (cst != null)
                    {
                        lock (cst)
                        {
                            cst.ReasonText = returnMessage;
                            if (port == null)
                            {
                                cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                            }
                            else
                            {
                                if (port.File.Type == ePortType.UnloadingPort)
                                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Validation_NG_From_MES;
                                else
                                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                            }
                        }
                    }

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName,
                        "Recipe Parameter check NG", string.Format("Cassette =[{0}] Parameter Validation NG from MES, please confirm", cstid) });
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_CheckRecipeParameterReply OK LINENAME =[{1}],PORTNAME =[{2}],CARRIERNAME =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                         trxID, lineName, portid, cstid, returnCode, returnMessage));
                }
                Invoke(eServiceName.RecipeService, "RecipeParameterMESReply", new object[] { trxID, lineName, portid, cstid, result });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //FacilityParameterReply
        /// <summary>
        ///  FacilityParameterReply      MES MessageSet : Reports all facility parameter items registered in EQP to MES
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
                    eqpCloneNode[keyHost.MACHINESTATENAME].InnerText = machine.MACHINESTATENAME;

                    XmlNode paraNodeList = eqpCloneNode[keyHost.FACILITYPARALIST];
                    XmlNode paraNodeClone = paraNodeList[keyHost.PARA].Clone();
                    paraNodeList.RemoveAll();

                    foreach (var para in machine.FACILITYPARALIST)
                    {
                        XmlNode paraNode = paraNodeClone.Clone();
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

        //FacilityCheckReport
        /// <summary>
        ///  FacilityCheckReport     MES MessageSet : BC reports to MES facility parameter check result,
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

                    eqpCloneNode[keyHost.MACHINESTATENAME].InnerText = machine.MACHINESTATENAME;
                    //eqpCloneNode[keyHost.RECIPEID].InnerText = machine.RECIPEID;
                    eqpCloneNode[keyHost.VALIRESULT].InnerText = "";

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

        //LineStateChanged
        /// <summary>
        /// LineStateChanged        MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="linestatus"></param>
        /// <param name="materialflag"></param>
        public void LineStateChanged(string trxID, string lineName, string linestatus)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });   //modify by bruce 2016/01/19 Offline 也需更新line Status
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.LINESTATENAME].InnerText = linestatus;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);

                #region OPI Service
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LineStateChanged OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcessAbnormalEnd
        /// <summary>
        /// LotProcessAbnormalEnd       MES MessageSet : BC reports this event instead of ‘LotProcessEnd’ when Lot process is ended abnormally. 
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="cst"></param>
        /// <param name="jobList"></param>
        public void LotProcessAbnormalEnd(string trxID, Port port, Cassette cst, IList<Job> jobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessAbnormalEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count().ToString();

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotNodeClone = lotListNode[keyHost.LOT];
                XmlNode jobListNode = lotNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                lotNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                lotListNode.RemoveAll();//把原格式中的LOTLIST移除
                jobListNode.RemoveAll();//把原格式中的PRODUCTLIST移除
                //int x = 0;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                XmlNode lotNode = lotNodeClone.Clone();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    if (cst.MES_CstData.LOTLIST.Count != 0)
                    {
                        lotNode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                        lotNode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;
                        lotNode[keyHost.PRODUCTSPECVER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECVER;
                        lotNode[keyHost.PROCESSFLOWNAME].InnerText = "";
                        lotNode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                        lotNode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                    }

                    foreach (Job job in jobList)
                    {
                        bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                        XmlNode jobNode = jobNodeClone.Clone();
                        jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                        jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                        jobNode[keyHost.PRODUCTJUDGE].InnerText = GetProductJudge(line, fabType, job);
                        jobNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
                        jobNode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;

                        //Port sourcePort = ObjectManager.PortManager.GetPortByLineIDPortID(lineID,job.SourcePortID);
                        //if (sourcePort != null)
                        //{
                        //    switch (sourcePort.Data.PORTATTRIBUTE.Trim())
                        //    {
                        //        case keyCELLPORTAtt.DENSE:
                        //        case keyCELLPORTAtt.BOX: jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "B"; break;
                        //        default: jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "C"; break;
                        //    }
                        //}
                        //else
                        //{
                        //    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //         string.Format("CSTSEQ =[{0}], SLOTNO =[{1}], GLASSID =[{2}], can't find source port, port id =[{3}]",
                        //         job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID, job.SourcePortID));
                        //    jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "C";
                        //}
                        //jobNode[keyHost.PROCESSFLAG].InnerText = job.MesProduct.PROCESSFLAG;
                        //‘OK’ ‘NG’ ‘BYPASS’ 

                        jobListNode.AppendChild(jobNode);
                    }
                    lotNode.AppendChild(jobListNode);
                    lotListNode.AppendChild(lotNode);
                }

                bodyNode.AppendChild(lotListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAbnormalEnd OK LINENAME =[{1}).",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }

            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public string GetProductJudge(Line line, eFabType fabType, Job job)
        {
            try
            {
                // Array不用報, CF報在ProductGrade裡, 只有CELL需要報
                if (fabType == eFabType.CELL)
                {
                    return ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }

        public string GetProductGrade(Line line, eFabType fabType, Job job)
        {
            try
            {
                if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1 ||
                    fabType == eFabType.ARRAY || fabType == eFabType.CELL)
                {
                    return job.JobGrade;
                }
                else
                {
                    return ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }

        //LotProcdssAbort
        /// <summary>
        ///  LotProcessAbort       MES MessageSet : MES notice BC to abort one carrier.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="port">Port Entity</param>
        public void LotProcessAbort(string trxID, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessAbort") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAborted OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcdssAborted
        /// <summary>
        ///  LotProcessAborted       MES MessageSet : BC reports processing of Lot has been canceled.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="port">Port Entity</param>
        public void LotProcessAborted(string trxID, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessAborted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID; ;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAborted OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcessCanceled
        /// <summary>
        ///  LotProcessCanceled      MES MessageSet : BC reports processing of Lot has been canceled.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="port">Port Entity</param>
        /// <param name="reasonCode">Cancel Reason code</param>
        /// <param name="reasonTxt">Cancel Reason Description</param>
        public void LotProcessCanceled(string trxID, Port port, string reasonCode, string reasonDescription)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);//可能會有兩條以上的Line由BC 控管
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.REASONDESCRIPTION].InnerText = reasonDescription;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcessEnd
        /// <summary>
        /// LotProcessEnd           BC reports when last panel of Lot/CST was fetched out.
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="cst"></param>
        /// <param name="jobList"></param>
        public void lotProcessEnd(string trxID, Port port, Cassette cst, IList<Job> jobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("lotProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count().ToString();

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotNodeClone = lotListNode[keyHost.LOT];
                XmlNode jobListNode = lotNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                lotNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                lotListNode.RemoveAll();//把原格式中的LOTLIST移除
                jobListNode.RemoveAll();//把原格式中的PRODUCTLIST移除
                //int x = 0;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                XmlNode lotNode = lotNodeClone.Clone();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    if (cst.MES_CstData.LOTLIST.Count != 0)
                    {
                        lotNode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                        lotNode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;
                        lotNode[keyHost.PRODUCTSPECVER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECVER;
                        lotNode[keyHost.PROCESSFLOWNAME].InnerText = "";
                        lotNode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                        lotNode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                    }

                    foreach (Job job in jobList)
                    {
                        bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                        XmlNode jobNode = jobNodeClone.Clone();
                        jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                        jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                        jobNode[keyHost.PRODUCTJUDGE].InnerText = GetProductJudge(line, fabType, job);
                        jobNode[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
                        jobNode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;

                        //Port sourcePort = ObjectManager.PortManager.GetPortByLineIDPortID(lineID,job.SourcePortID);
                        //if (sourcePort != null)
                        //{
                        //    switch (sourcePort.Data.PORTATTRIBUTE.Trim())
                        //    {
                        //        case keyCELLPORTAtt.DENSE:
                        //        case keyCELLPORTAtt.BOX: jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "B"; break;
                        //        default: jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "C"; break;
                        //    }
                        //}
                        //else
                        //{
                        //    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        //         string.Format("CSTSEQ =[{0}], SLOTNO =[{1}], GLASSID =[{2}], can't find source port, port id =[{3}]",
                        //         job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID, job.SourcePortID));
                        //    jobNode[keyHost.SOURCEDURABLETYPE].InnerText = "C";
                        //}
                        //jobNode[keyHost.PROCESSFLAG].InnerText = job.MesProduct.PROCESSFLAG;
                        //‘OK’ ‘NG’ ‘BYPASS’ 

                        jobListNode.AppendChild(jobNode);
                    }
                    lotNode.AppendChild(jobListNode);
                    lotListNode.AppendChild(lotNode);
                }

                bodyNode.AppendChild(lotListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] lotProcessEnd OK LINENAME =[{1}).",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }

            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcessEndReply
        /// <summary>
        /// LotProcessEndReply      MES MessagetSet : Lot Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_LotProcessEndReply(XmlDocument xmlDoc)
        {
            try
            {
                #region Delay Send LotProcessEnd
                try
                {
                    if (_processEndQueue.Count > 0)
                    {
                        xMessage msg = null;
                        bool done = _processEndQueue.TryDequeue(out msg);
                        if (done)
                        {
                            lock (_timeSync)
                                _processEndLastReportTime = DateTime.Now;
                            PutMessage(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                #endregion

                string returnCode = GetMESReturnCode(xmlDoc).Trim();
                string returnMessage = GetMESReturnMessage(xmlDoc).Trim();
                string lineName = GetLineName(xmlDoc).Trim(); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc).Trim();

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
                }

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText.Trim();
                string cstid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText.Trim();

                string desc = string.Empty;


                if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                {
                    if (!ObjectManager.CassetteManager.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB(portid, cstid, trxID, returnCode == "0" ? "OK" : "NG", returnCode, returnMessage, out desc))
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES][{0}] LotProcessEndReply.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                       trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessEndReply  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToIncompleteCST(portid, cstid, trxID, out desc))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS <- MES][{0}] LotProcessEndReply.FileMoveToIncompleteCST  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                            trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessEndReply  OK LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToCompleteCST(portid, cstid, trxID, out desc))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("[BCS <- MES][{0}] LotProcessEndReply.FileMoveToCompleteCST NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                           trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }

                //object[] _data = new object[5]
                //{ 
                //    trxID,                           /*1 trackKey */
                //    lineName,
                //    cstid,
                //    portid,
                //    doublerunflag
                //};

                string timeoutName = string.Format("{0}_MES_LotProcessEndReply", lineName);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                //Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", _data);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //LotProcessStarted
        /// <summary>
        /// LotProcessStarted       MES MessageSet : BC reports Lot processing has been started.
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
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
                //Jun Add 20150602 For POL Unloader AutoClave Report LotProcessStart
                if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3 && cst.CellBoxProcessed == eboxReport.Processing)
                    bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
                else
                    bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode lotListNode = bodyNode[keyHost.LOTLIST];
                XmlNode lotCloneNode = lotListNode[keyHost.LOT].Clone();
                lotListNode.RemoveAll();

                for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
                {
                    XmlNode lotnode = lotCloneNode.Clone();
                    Job job = null;
                    lotnode[keyHost.LOTNAME].InnerText = cst.MES_CstData.LOTLIST[i].LOTNAME;
                    lotnode[keyHost.PROCESSOPERATIONNAME].InnerText = cst.MES_CstData.LOTLIST[i].PROCESSOPERATIONNAME;
                    lotnode[keyHost.PRODUCTQUANTITY].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count.ToString();
                    lotnode[keyHost.PRODUCTOWNER].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTOWNER;
                    lotnode[keyHost.PRODUCTSPECNAME].InnerText = cst.MES_CstData.LOTLIST[i].PRODUCTSPECNAME;

                    if (cst.MES_CstData.LOTLIST[i].PRODUCTLIST.Count > 0)
                        job = ObjectManager.JobManager.GetJob(cst.MES_CstData.LOTLIST[i].PRODUCTLIST[0].PRODUCTNAME.Trim());

                    if (job != null)
                    {
                        lotnode[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                    }
                    else
                    {
                        lotnode[keyHost.PRODUCTRECIPENAME].InnerText = cst.MES_CstData.LOTLIST[i].LINERECIPENAME;
                    }

                    lotListNode.AppendChild(lotnode);
                }

                SendToMES(xml_doc);

                //Watson Add 20141212 For MES Spec 
                //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
                cst.IsOffLineProcessStarted = false;
                lock (cst)
                {
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));

                #region OEE
                if (cst.MES_CstData.LOTLIST.Count > 0)//sy add 防止T3 空 tray 只有 LOTLIST沒有PRODUCTLIST 會報Exception
                {
                    if (cst.MES_CstData.LOTLIST[0].PRODUCTLIST.Count > 0)
                    {
                        //没有呼叫OEEService？ --James.Yan at 2015/01/24
                        string ownerid = "";
                        if (cst.MES_CstData.LOTLIST.Count > 0)
                        {
                            ownerid = cst.MES_CstData.LOTLIST[0].PRODUCTLIST[0].OWNERID;
                        }

                        bodyNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.PORTTYPE, ConstantManager["MES_PORTTYPE"][((int)port.File.Type).ToString()].Value));


                        foreach (XmlNode lotNode in lotListNode)
                        {
                            string lotName = lotNode[keyHost.LOTNAME].InnerText;

                            foreach (LOTc lotInfo in cst.MES_CstData.LOTLIST)
                            {
                                if (lotName == lotInfo.LOTNAME)
                                {
                                    //lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.OWNERTYPE, lotInfo.PRODUCTLIST[0].OWNERTYPE));
                                    //根据福杰介绍 此处要截取最后一码 20150325 Watson
                                    //<OWNERTYPE>OwnerE</OWNERTYPE>  这个节点的内容之前只是 P 或 E 或 M Interface 文档也是这样规定的，没有OwnerE，请帮忙check ,谢谢！
                                    //登京mail 2015/03/25 下午：T1的逻辑是MES Download后,若值不为空,则BC截取最后一码.
                                    lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.OWNERTYPE, lotInfo.PRODUCTLIST[0].OWNERTYPE.Length > 0 ? lotInfo.PRODUCTLIST[0].OWNERTYPE.Substring(lotInfo.PRODUCTLIST[0].OWNERTYPE.Length - 1, 1) : ""));


                                    lotNode.AppendChild(this.CreateXmlNode(xml_doc, keyHost.LOTINFO, ownerid));
                                    break;
                                }
                            }
                        }
                    }
                }
                //lotListNode.RemoveChild(lotListNode[keyHost.LINERECIPENAME]);
                //lotListNode.RemoveChild(lotListNode[keyHost.HOSTLINERECIPENAME]);
                //lotListNode.RemoveChild(lotListNode[keyHost.PPID]);
                //lotListNode.RemoveChild(lotListNode[keyHost.HOSTPPID]);

                //SendToOEE(xml_doc);
                Invoke(eServiceName.OEEService, "LotProcessStarted", new object[3] { trxID, port, cst }); //2015/10/15 cc.kuang
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MachineDataUpdate
        /// <summary>
        ///  MachineDataUpdate       MES MessageSet : Online Sequence Report All EQP Data
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        public void MachineDataUpdate(string trxID, Line line)
        {
            try
            {
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineDataUpdate") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                //2015/9/7 Add by Frank For UPK Line ULD EQP Run Mode
                #region [UPK Line ULD EQP Run Mode]
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                {
                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                    foreach (Equipment eqp in eqps)
                    {
                        if ((eqp.Data.NODEATTRIBUTE == "UD") && (eqp.File.EquipmentRunMode == "RE-CLEAN"))
                        {
                            if (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.TFT)
                                bodyNode[keyHost.LINEOPERMODE].InnerText = "TFTRECLEAN";
                            else
                                bodyNode[keyHost.LINEOPERMODE].InnerText = "CFRECLEAN";
                        }
                        else
                            bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                    }
                }
                #endregion
                else
                    bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();

                bodyNode[keyHost.LINESTATENAME].InnerText = line.File.Status.ToString();

                XmlNode specEqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode specEqpNodeClone = specEqpListNode[keyHost.MACHINE].Clone();
                XmlNode specUnitListNode = specEqpNodeClone[keyHost.UNITLIST];
                XmlNode specUnitNodeClone = specUnitListNode[keyHost.UNIT].Clone();
                specUnitListNode.RemoveAll();
                specEqpNodeClone.RemoveChild(specUnitListNode);  //Watson Modify 20141128 For MES要求，沒有unit也要有UnitList
                specEqpListNode.RemoveAll();

                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                {
                    #region Watson Add 20150313 For PMT Line PTI100的特殊上報規則
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                    {
                        Equipment eqp2 = ObjectManager.EquipmentManager.GetEQP("L2");
                        XmlNode eqpNode2 = specEqpNodeClone.Clone();
                        eqpNode2[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        eqpNode2[keyHost.MACHINESTATENAME].InnerText = eqp2.File.MESStatus;

                        XmlNode uniListNode2 = specUnitListNode.Clone();
                        XmlNode unitNode2 = null;
                        foreach (Unit unit2 in ObjectManager.UnitManager.GetUnitsByEQPNo(eqp2.Data.NODENO))
                        {
                            unitNode2 = specUnitNodeClone.Clone();
                            unitNode2[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                            unitNode2[keyHost.UNITSTATENAME].InnerText = unit2.File.MESStatus;
                        }
                        uniListNode2.AppendChild(unitNode2);

                        eqpNode2.AppendChild(uniListNode2);
                        specEqpListNode.AppendChild(eqpNode2);
                    }
                    #endregion

                    if (eqp.Data.NODEATTRIBUTE != "CV5")
                    {
                        XmlNode eqpNode = specEqpNodeClone.Clone();
                        eqpNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        eqpNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus;

                        XmlNode uniListNode = specUnitListNode.Clone();
                        XmlNode unitNode = null;
                        foreach (Unit unit in ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO))
                        {
                            if (unit.Data.UNITATTRIBUTE == "VIRTUAL") // Virtual 的UNIT 无需上报MES
                                continue;
                            unitNode = specUnitNodeClone.Clone();
                            unitNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unit.Data.UNITID);
                            unitNode[keyHost.UNITSTATENAME].InnerText = unit.File.MESStatus;

                            uniListNode.AppendChild(unitNode);
                        }
                        eqpNode.AppendChild(uniListNode);
                        specEqpListNode.AppendChild(eqpNode);
                    }
                }
                //bodyNode.AppendChild(specEqpListNode);
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MachineModeChangeRequest
        /// <summary>
        /// MachineModeChangeRequest        MES MessageSet : BC requests MES to validate changing machine operation mode is valid or not.
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        public void MachineModeChangeRequest(string trxID, string lineName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineModeChangeRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                // Changer Line 沒有轉換的觸發時機點(ex. RunModeChange, IndexerOperationModeChange..)
                if (line.Data.LINETYPE == eLineType.ARRAY.CHN_SEEC)
                {
                    bodyNode[keyHost.LINEOPERMODE].InnerText = eMES_LINEOPERMODE.CHANGER;
                }
                else // add by bruce 2014/12/09 report line mix run mode
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                    {
                        bodyNode[keyHost.LINEOPERMODE].InnerText = "MIX"; //"MIXEDRUNMODE";
                    }
                    else
                    {
                        //2015/9/7 Add by Frank For UPK Line ULD EQP Run Mode
                        #region [UPK Line ULD EQP Run Mode]
                        if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                        {
                            List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineName);
                            foreach (Equipment eqp in eqps)
                            {
                                if ((eqp.Data.NODEATTRIBUTE == "UD") && (eqp.File.EquipmentRunMode == "RE-CLEAN"))
                                {
                                    if (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.TFT)
                                        bodyNode[keyHost.LINEOPERMODE].InnerText = "TFTRECLEAN";
                                    else
                                        bodyNode[keyHost.LINEOPERMODE].InnerText = "CFRECLEAN";
                                }
                                else
                                    bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                            }
                        }
                        #endregion

                        else
                            bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                    }
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MachineModeChangeReply
        /// <summary>
        ///  MachineModeChangeReply      MES MessagetSet : Machine Status Change Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MachineModeChangeReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string lineopermode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINEOPERMODE].InnerText;
                string timestamp = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.TIMESTAMP].InnerText;
                string result = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MachineStateChanged
        /// <summary>
        ///  MachineStateChanged     MES MessageSet : Reports when EQP states has been changed.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="EQPID">Node ID</param>
        /// <param name="eqpStatus">‘IDLE’,‘RUN’,‘DOWN’ </param>
        /// <param name="alarmID">eqpStatus is Down report</param>
        /// <param name="alarmtext">eqpStatus is Down report</param>
        /// <param name="alarmTime">eqpStatus is Down report</param>
        /// <param name="materialflag">Material Status change is True</param>
        public void MachineStateChanged(string trxID, Equipment eqp, string alarmID, string alarmtext, string alarmTime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different.
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineStateChanged Send MES but OFF LINE LINENAME=[{1}].",
                //            //trxID, eqp.Data.LINEID));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(eqp.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus;

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    if (ParameterManager["MESSYNCREPORT"].GetBoolean()) //上报MES 是否
                        SendToQueue(xml_doc); //先放到Queue中
                    else
                        SendToMES(xml_doc);

                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                    ObjectManager.LineManager.GetLineID(eqp.Data.LINEID), trxID, ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), eqp.File.Status.ToString()));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherLine = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherLine.File.HostMode != eHostMode.OFFLINE)
                    {
                        if (ParameterManager["MESSYNCREPORT"].GetBoolean()) //上报MES 是否
                            SendToQueue(xml_doc); //先放到Queue中
                        else
                            SendToMES(xml_doc);
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        bodyNode[keyHost.LINENAME].InnerText, trxID, bodyNode[keyHost.MACHINENAME].InnerText, eqp.File.Status.ToString()));
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

        //MachineStateChanged
        /// <summary>
        /// MachineStateChanged         MES MessageSet : Reports when EQP states has been changed.
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqp"></param>
        public void MachineStateChanged_WaitCassetteState(string trxID, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different.
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineStateChanged Send MES but OFF LINE LINENAME=[{1}].",
                //            //trxID, eqp.Data.LINEID));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(eqp.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.WaitCassetteStatus.ToString();

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                //SendToMES(xml_doc);
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToQueue(xml_doc);
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), eqp.File.Status.ToString()));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150319 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToQueue(xml_doc);
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        bodyNode[keyHost.LINENAME].InnerText, trxID, bodyNode[keyHost.MACHINENAME].InnerText, eqp.File.Status.ToString()));
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

        //MaterialMount
        /// <summary>
        /// MaterialMount       MES MessageSet : Reports when material state has been changed
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eqp"></param>
        /// <param name="lineRecipeName"></param>
        /// <param name="materialMode"></param>
        /// <param name="glassID"></param>
        /// <param name="POLType"></param>
        /// <param name="materilst"></param>
        public void MaterialMount(string trxID, Equipment eqp, string glassID, string materialDurableName, string polType, List<MaterialEntity> materialList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line.Data.LINEID.Contains("TCPHL") && eqp.Data.NODEATTRIBUTE == "DNS") //cc.kuang 2015/09/24
                    eqp.File.MaterialChange = false;

                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialMount") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                materialListNode.RemoveAll();

                foreach (MaterialEntity material in materialList)
                {
                    XmlNode materNode = materNodeClone.Clone();
                    materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                    materNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                    materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;

                    materNode[keyHost.CARRIERNAME].InnerText = "";
                    materNode[keyHost.CREATEDCOUNT].InnerText = "";

                    materialListNode.AppendChild(materNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodInfo.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                    trxID, ObjectManager.LineManager.GetLine(line.Data.LINEID).Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MaterialMountReply
        /// <summary>
        /// MaterialMountReply     MES MessageSet : Reports when material state has been changed,mes reply to bc.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_MaterialMountReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineId = GetLineName(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineId);
                if (line == null) throw new Exception(string.Format("Can't find LINEID=[{0}] in LineEntity!", lineId));
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp == null) throw new Exception(string.Format("Can't find MACHINENAME =[{0}] in EquipmentEntity!", machineName));
                string trxId = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName=[{2}], mismatch=[{0}].", ServerName, trxId, lineId));
                }

                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                MaterialEntity material = new MaterialEntity();

                XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;
                foreach (XmlNode node in materialList)
                {
                    material.MaterialID = node[keyHost.MATERIALNAME].InnerText;
                    //returnCode
                }
                eFabType fabType;//shihyang  20151026 移到這
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                //20151111 cy:之前只mark else的部份, 會造成只有MES reply NG時,才會回覆機台NG, 這樣若MES reply OK, BCS會因為Timeout而NG, 機台永遠無法Mount.
                //if (returnCode != "0")
                //{
                // to do BC reply ng, eq can't change material status !!
                //20151112 cy:傳入的值整個有問題
                //List<MaterialEntity> materialEntity = ObjectManager.MaterialManager.GetMaterials();
                //object[] _data = new object[8]
                //  {
                //      eqp.Data.NODENO, //eqpNo
                //      material.UnitNo, //unitNo
                //      "",              //headNo
                //      materialEntity,  //material  //20151112 cy:MaterialStatusChangeReportReply要傳入的是MaterialEntity,而不是List<MaterialEntity>
                //      "",              //eBitResult //20151112 cy:要傳ON
                //      returnCode,      //eReturnCode1   //20151112 cy:要用Enum
                //      trxId,           //trackKey
                //      ""              //flgDisable  //20151112 cy:若是Bit ON,要給false, OFF給true
                //  };
                object[] _data = new object[8]
                          {
                              eqp.Data.NODENO, //eqpNo
                              material.UnitNo, //unitNo
                              "",              //headNo
                              material,  //material  //20151112 cy:MaterialStatusChangeReportReply要傳入的是MaterialEntity,而不是List<MaterialEntity>
                              eBitResult.ON,              //eBitResult //20151112 cy:要傳ON
                              returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG,      //eReturnCode1   //20151112 cy:要用Enum
                              trxId,           //trackKey
                              false              //flgDisable  //20151112 cy:若是Bit ON,要給false, OFF給true
                          };

                //20151222 cy:先判斷是否有SECS的Message要回應,沒有的話,再回應PLC的部份
                string timerId1 = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialMount", eqp.Data.NODENO, trxId);
                if (Timermanager.IsAliveTimer(timerId1))
                {
                    UserTimer timer = Timermanager.GetAliveTimer(timerId1);
                    if (timer != null)
                    {
                        Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                        if (tuple != null)
                        {
                            string systembyte = tuple.Item1;
                            string header = tuple.Item2;
                            if (lineId.Contains("TCPHL") && eqp.Data.NODEATTRIBUTE == "DNS") //cc.kuang 2015/09/24
                            {
                                bool checkFlag = true;
                                string cimMessage = string.Empty;
                                //就算MES沒回Material List,還是要回應機台 20151006 cy
                                //if (materialList.Count > 0)
                                if (!checkFlag)
                                {
                                    eqp.File.MaterialChange = false;
                                    Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)1 });
                                    Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, cimMessage, trxId, string.Empty });
                                    Invoke(eServiceName.CSOTSECSService, "TS10F5_H_TerminalDisplaySingleforDNSLC",
                                    new object[] { eqp.Data.NODENO, eqp.Data.NODEID, header, string.Empty, trxId });
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [MES -> BCS] " + MethodInfo.GetCurrentMethod().Name + " {1}",
                                            lineId, cimMessage));
                                }
                                else
                                {
                                    eqp.File.MaterialChange = true;
                                    Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)0 });
                                }
                            }
                        }
                    }
                    Timermanager.TerminateTimer(timerId1);
                }
                else
                    Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", _data);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialMountReply NG LINENAME =[{1}],MACHINENAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                     trxId, lineId, machineName, returnCode, returnMessage));
                //}
                //else
                //{
                //    List<MaterialEntity> materialEntity = ObjectManager.MaterialManager.GetMaterials();

                //    //report MES
                //    object[] _data = new object[7]
                //    { 
                //        trxId,  /*0 TrackKey*/
                //        eqp.Data.LINEID,    /*1 LineName*/
                //        eqp.Data.NODEID,    /*2 EQPID*/
                //        "",          /*3 LINERECIPENAME*/
                //        "",            /*4 MATERIALMODE*/ 
                //        "",            /*5 panelID*/
                //        materialEntity,          /*6 materlist*/
                //    };

                //    //呼叫MES方法
                //    Invoke(eServiceName.MESService, "MaterialStateChanged", _data);

                //    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialMountReply OK LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALMODE =[{4}],PRODUCTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                //                         trxId, lineId, machineName, lineRecipeName, materialMode, productName, returnCode, returnMessage));
                //}
                //Invoke BC Function MATERIALWARNINGTIME FOR CF Coater Equipment PR


                //20151222 cy:改拉到上面做判斷,避免應該走SECS的,還多呼叫PLC的回覆
                #region Array Secs
                //if (fabType == eFabType.ARRAY)
                //{
                //      //Array Use for check PR life time
                //      string timerId = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialMount", eqp.Data.NODENO, trxId);
                //      if (Timermanager.IsAliveTimer(timerId))
                //      {
                //            UserTimer timer = Timermanager.GetAliveTimer(timerId);
                //            if (timer != null)
                //            {
                //                  Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                //                  if (tuple != null)
                //                  {
                //                        string systembyte = tuple.Item1;
                //                        string header = tuple.Item2;
                //                        if (lineId.Contains("TCPHL") && eqp.Data.NODEATTRIBUTE == "DNS") //cc.kuang 2015/09/24
                //                        {
                //                              bool checkFlag = true;
                //                              string cimMessage = string.Empty;
                //                              DateTime LifeQTime;
                //                              double warringtime;
                //                              foreach (XmlNode node in materialList)
                //                              {
                //                                    if (!node[keyHost.VALIRESULT].InnerText.Trim().Equals("Y"))
                //                                    {
                //                                          checkFlag = false;
                //                                          cimMessage = "MES Mount Reply VALIRESULT Value <> Y, Material ID = " + node[keyHost.MATERIALNAME].InnerText;
                //                                          break;
                //                                    }

                //                                    if (node[keyHost.MATERIALWARNINGTIME].InnerText.Trim().Length > 0 && node[keyHost.LIFEQTIME].InnerText.Trim().Length > 0)
                //                                    {
                //                                          LifeQTime = DateTime.ParseExact(node[keyHost.LIFEQTIME].InnerText.Trim().Substring(0, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                //                                          warringtime = double.Parse(node[keyHost.MATERIALWARNINGTIME].InnerText.Trim());
                //                                          if (LifeQTime.Subtract(DateTime.Now).TotalMinutes < warringtime)
                //                                          {
                //                                                checkFlag = false;
                //                                                cimMessage = "MES Mount Reply LIFEQTIME Check is Over Warring Time, Material ID = " + node[keyHost.MATERIALNAME].InnerText;
                //                                                break;
                //                                          }
                //                                    }
                //                              }
                //                              //就算MES沒回Material List,還是要回應機台 20151006 cy
                //                              //if (materialList.Count > 0)
                //                              {
                //                                    if (!checkFlag)
                //                                    {
                //                                          eqp.File.MaterialChange = false;
                //                                          Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)1 });
                //                                          Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, cimMessage, trxId, string.Empty });
                //                                          Invoke(eServiceName.CSOTSECSService, "TS10F5_H_TerminalDisplaySingleforDNSLC",
                //                                          new object[] { eqp.Data.NODENO, eqp.Data.NODEID, header, string.Empty, trxId });
                //                                          Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                //                                          string.Format("[LINENAME={0}] [MES -> BCS] " + MethodInfo.GetCurrentMethod().Name + " {1}",
                //                                                  lineId, cimMessage));
                //                                    }
                //                                    else
                //                                    {
                //                                          eqp.File.MaterialChange = true;
                //                                          Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)0 });
                //                                    }
                //                              }
                //                        }
                //                  }
                //            }
                //            Timermanager.TerminateTimer(timerId);
                //      }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MaterialDismountReport
        /// <summary>
        /// MaterialDismountReport      MES MessageSet : Reports when material state has been changed to DISMOUNT.
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eqp"></param>
        /// <param name="lineRecipeName"></param>
        /// <param name="materialMode"></param>
        /// <param name="glassID"></param>
        /// <param name="materilst"></param>
        public void MaterialDismountReport(string trxID, Equipment eqp, string glassID, string materialDurableName, List<MaterialEntity> materialList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialDismountReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                materialListNode.RemoveAll();

                foreach (MaterialEntity material in materialList)
                {
                    XmlNode materNode = materNodeClone.Clone();
                    materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                    materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                    materNode[keyHost.MATERIALCONSUMEDQUANTITY].InnerText = "";

                    materialListNode.AppendChild(materNode);
                }
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, ObjectManager.LineManager.GetLine(line.Data.LINEID).Data.LINEID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MaterialStateChanged
        /// <summary>
        ///  MaterialStateChanged        MES MessageSet : On Line Material Status Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="eqp">Node</param>
        public void MaterialStateChanged_OnLine(string trxID, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) return;

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                //Watson modify 20141124 For MES Spec (原來竟然寫MASK)
                XmlNode maskListNode = bodyNode[keyHost.MATERIALLIST];  //Watson modify 20141124 For MES Spec
                XmlNode maskNodeClone = maskListNode[keyHost.MATERIAL];  //Watson modify 20141124 For MES Spec
                maskListNode.RemoveAll();

                List<MaterialEntity> materialList = ObjectManager.MaterialManager.GetMaterials();
                if ((materialList != null) && (materialList.Count > 0))  //Watson Modify 20150327 For Maybe Material  != null, But Material Count == 0
                {
                    foreach (MaterialEntity material in materialList)
                    {

                        ////Watson modify 20141124 For MES Spec (原來竟然寫MASK)
                        //XmlNode maskListNode = bodyNode[keyHost.MATERIALLIST];  //Watson modify 20141124 For MES Spec
                        //XmlNode maskNodeClone = maskListNode[keyHost.MATERIAL];  //Watson modify 20141124 For MES Spec

                        //目前只會有一個Mask
                        //for (int i = 0; i < job.MesAbnormal.CODE.Count; i++)
                        //{
                        XmlNode maskNode = maskNodeClone.Clone();
                        maskNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                        maskNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;//不知道報什麼 //Jun Modify 20141202
                        maskListNode.AppendChild(maskNode);

                        #region ON Line TimeID Regesiter
                        string timeId = string.Format("{0}_{1}_MaterialStateChanged_ONLINE", eqp.Data.NODENO, trxID);  //watson 20150106 Add

                        if (Timermanager.IsAliveTimer(timeId))
                        {
                            Timermanager.TerminateTimer(timeId);
                        }
                        Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStateChangedReplyTimeout), eqp.Data.NODENO);
                        #endregion

                        if (material.MaterialStatus == eMaterialStatus.DISMOUNT)
                        {
                            ObjectManager.MaterialManager.DeleteMaterial(material);
                        }

                        // }
                    }

                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                    trxID, ObjectManager.LineManager.GetLine(line.Data.LINEID).Data.LINEID));
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MaterialStateChangedReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                if (_timerManager.IsAliveTimer(tmp))
                {
                    _timerManager.TerminateTimer(tmp);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, MES_MaterialStateChangedReplyTimeout Set Bit (OFF).", sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  MaterialStateChanged        MES MessageSet : Material State Changed to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="eQPID"></param>
        /// <param name="pPID"></param>
        /// <param name="materialmode"></param>
        /// <param name="panelID"></param>
        /// <param name="materialID"></param>
        /// <param name="materialStatus"></param>
        /// <param name="type"></param>
        public void MaterialStateChanged(string trxID, string lineName, string eQPID, string pPID, string materialmode, string panelID,
             string materialID, eMaterialStatus materialStatus, string materialWeight, string type, string useCount, string lifeQtime, string groupID, string unitID, string headID, string requestKey)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);
                if (eqp == null) throw new Exception(string.Format("Can't find MACHINENAME =[{0}] in EquipmentEntity!", eQPID));

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    #region 根据OFFLINEREPLYEQP回复机台
                    string unitNo = "";
                    #region get unitno
                    if (unitID != "")
                    {
                        Unit unit = ObjectManager.UnitManager.GetUnit(unitID);
                        if (unit != null)
                        {
                            unitNo = unit.Data.UNITNO;
                        }
                    }
                    else
                    {
                        unitNo = "0";
                    }
                    #endregion

                    int command = 1;
                    if (!ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                    {
                        command = 2;
                    }
                    if (requestKey == "MaterialVerificationRequest")
                    {
                        Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxID, eBitResult.ON, (eReturnCode1)(command), eqp.Data.NODENO, unitNo });
                        return;
                    }
                    else if (requestKey == "MaterialStatusChangeReport")
                    {
                        MaterialEntity material = new MaterialEntity();
                        Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, "0", material, eBitResult.ON, (eReturnCode1)(command), trxID, false });
                    }
                    return;
                    #endregion
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;

                bodyNode[keyHost.MACHINENAME].InnerText = eQPID;

                XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                materListNode.RemoveAll();

                //一次只有一片
                string _materialStatus = string.Empty;
                XmlNode materNode = prodCloneNode.Clone();
                if (materialStatus == eMaterialStatus.NONE)
                    _materialStatus = "EMPTY";
                else
                    _materialStatus = materialStatus.ToString();
                materNode[keyHost.MATERIALSTATE].InnerText = _materialStatus;
                materListNode.AppendChild(materNode);

                SendToMES(xml_doc);
                if (materialStatus == eMaterialStatus.DISMOUNT)
                {
                    List<MaterialEntity> materialEntity = ObjectManager.MaterialManager.GetMaterials();
                    if (materialEntity != null)
                    {
                        foreach (MaterialEntity material in materialEntity)
                        {
                            if (material.MaterialID == materialID)
                            {
                                ObjectManager.MaterialManager.DeleteMaterial(material);
                                break;
                            }
                        }
                    }
                }
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> MES] [{4}] Send MES trx MaterialStateChanged,LineName =[{0}],MachineName =[{1}], MaterialName =[{2}], MaterialState =[{3}].", lineName, eQPID, materialID, materialStatus.ToString(), trxID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// MaterialStateChanged 
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="eQPID"></param>
        /// <param name="linerecipeName"></param>
        /// <param name="materialmode"></param>
        /// <param name="panelID"></param>
        /// <param name="materlist"></param>
        public void MaterialStateChanged(string trxID, string lineName, string eQPID, string linerecipeName, string materialmode, string panelID,
           List<MaterialEntity> materlist)
        {
            try
            {

                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eQPID;

                XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                materListNode.RemoveAll();

                //一次只有一片
                foreach (MaterialEntity mater in materlist)
                {
                    XmlNode materNode = prodCloneNode.Clone();
                    materNode[keyHost.MATERIALSTATE].InnerText = mater.MaterialStatus.ToString();
                    materNode[keyHost.MATERIALTYPE].InnerText = mater.MaterialType;
                    materListNode.AppendChild(materNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                     string.Format("[LINENAME={0}] [BCS -> MES] [{3}] Send MES trx MaterialStateChanged,LineName =[{0}],MachineName =[{1}], PanelID =[{2}].", lineName, eQPID, panelID, trxID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PortDataUpdate
        /// <summary>
        ///  PortDataUpdate      MES MessageSet : On line Seqence Port Data Update Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="node">Node Entity</param>
        public void PortDataUpdate(string trxID, Line line)
        {
            try
            {
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortDataUpdate") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                #region CELL PMT line 要報全部的port
                //Watson Add 20141223 For 雙Line邏輯
                if (ServerName.Contains(eLineType.CELL.CBPMT))
                {
                    foreach (Port _port in ObjectManager.PortManager.GetPorts())
                    {

                        XmlNode portNode = portCloneNode.Clone();
                        portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                        portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID;

                        portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
                        //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade
                        if (CheckCellLine(line, _port))
                            portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                        else
                            portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][(int)_port.File.Mode].Value;

                        portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)_port.File.Type].Value;

                        ///PMT Line Special 做法
                        if (_port.Data.LINEID == line.Data.LINEID)
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)_port.File.EnableMode].Value;
                        else
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)ePortEnableMode.Disabled].Value;

                        portNode[keyHost.PORTTRANSFERSTATE].InnerText = ConstantManager["MES_PORTTRANSFERSTATE"][(int)_port.File.Status].Value;
                        portNode[keyHost.PORTOPERMODE].InnerText = ConstantManager["MES_PORTOPERMODE"][(int)_port.File.OperMode].Value;

                        portListNode.AppendChild(portNode);
                    }
                }
                #endregion
                else
                {
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))  //Jun Modify 20141222 By Line 取得EQP List
                    {
                        foreach (Port _port in ObjectManager.PortManager.GetPorts(eqp.Data.NODEID))
                        {
                            XmlNode portNode = portCloneNode.Clone();
                            portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                            portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID;

                            portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
                            //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade
                            if (CheckCellLine(line, _port))
                                portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                            else
                                portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][(int)_port.File.Mode].Value;

                            portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)_port.File.Type].Value;
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)_port.File.EnableMode].Value;
                            portNode[keyHost.PORTTRANSFERSTATE].InnerText = ConstantManager["MES_PORTTRANSFERSTATE"][(int)_port.File.Status].Value;
                            portNode[keyHost.PORTOPERMODE].InnerText = ConstantManager["MES_PORTOPERMODE"][(int)_port.File.OperMode].Value;

                            portListNode.AppendChild(portNode);
                        }
                    }
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortDataUpdate OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private bool CheckCellLine(Line line, Port port)
        {
            try
            {
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 ||
                        line.Data.LINETYPE == eLineType.CELL.CBCUT_2 ||
                        line.Data.LINETYPE == eLineType.CELL.CBCUT_3 ||
                        line.Data.LINETYPE == eLineType.CELL.CBSOR_1 ||
                        line.Data.LINETYPE == eLineType.CELL.CBSOR_2 ||
                        line.Data.LINETYPE == eLineType.CELL.CBLOI)
                    {
                        if (port.File.Type == ePortType.UnloadingPort)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        //PortTransferStateChanged
        /// <summary>
        /// PortTransferStateChanged        MES MessageSet : Port (All or 2 以上)Transfer States Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port IList, 也可以傳單一port</param>
        public void PortTransferStateChanged(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTransferStateChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();

                portListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    //if (_port.File.Type == ePortType.LoadingPort)  //Port Type ="Loader"
                    //{
                    XmlNode portNode = portCloneNode.Clone();
                    //20141125 Add by Edison:"UN" Don't Report
                    if (_port.File.Status == ePortStatus.UN)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but PORT STATUS IS (UN),PortId=[{2}].", trxID,
                            ObjectManager.LineManager.GetLineID(line.Data.LINEID), _port.Data.PORTID));
                        continue;
                    }
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTTRANSFERSTATE].InnerText = _port.File.Status.ToString();
                    //20150203 modify by edison:防住机台LR时上报CSTID不为空
                    if (_port.File.Status == ePortStatus.LR)
                    {
                        portNode[keyHost.CARRIERNAME].InnerText = "";
                    }
                    else
                    {
                        portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();
                    }

                    portNode[keyHost.CARRIERSETCODE].InnerText = _port.File.CassetteSetCode;

                    // 20141218 Add Sort Rule
                    //if (_port.File.Type == ePortType.UnloadingPort)
                    //{
                    //    if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                    //    {
                    //        portNode[keyHost.PRODUCTGRADE].InnerText = _port.File.MappingGrade;

                    //        Job job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                    //            j.TargetPortID == "0" && j.JobGrade.Trim().Equals(_port.File.MappingGrade.Trim()) &&
                    //            j.ProductType.Value.Equals(int.Parse(_port.File.ProductType)));

                    //        if (job != null)
                    //        {
                    //            portNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();
                    //        }
                    //    }
                    //}
                    //portNode[keyHost.PRODUCTNAME].InnerText = string.Empty;
                    //portNode[keyHost.PRODUCTGRADE].InnerText = string.Empty;
                    portListNode.AppendChild(portNode);
                    //}
                    //else
                    //{
                    //TODO: 等待MES 新的SPEC , 因為結構有改變
                    //    XmlNode uldportNode = uldportCloneNode.Clone();
                    //    uldportNode[keyHost.PORTNAME].InnerText = _port.Data.PORTID;
                    //    uldportNode[keyHost.PRODUCTNAME].InnerText = string.Empty;
                    //    uldportListNode.AppendChild(uldportNode);
                    //}
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                //bodyNode.RemoveChild(uldportListNode);
                SendToOEE(xml_doc);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// PortTransferStateChanged        MES MessageSet : One Port Transfer States Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port</param>
        public void PortTransferStateChanged(string trxID, string lineName, Port _port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                //20141125 Add by Edison:"UN" Don't Report
                if (_port.File.Status == ePortStatus.UN)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but PORT STATUS IS (UN),PortId=[{2}].", trxID, lineName, _port.Data.PORTID));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTransferStateChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portNode = portListNode[keyHost.PORT].Clone();

                portListNode.RemoveAll();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                portNode[keyHost.PORTTRANSFERSTATE].InnerText = _port.File.Status.ToString();
                //20150203 modify by edison:防住机台LR时上报CSTID不为空
                if (_port.File.Status == ePortStatus.LR)
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        if (line.File.PlanStatus == ePLAN_STATUS.NO_PLAN)
                            portNode[keyHost.CARRIERNAME].InnerText = "";
                        else
                            portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();
                    }
                    else
                        portNode[keyHost.CARRIERNAME].InnerText = "";
                }
                else
                    portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();

                portNode[keyHost.CARRIERSETCODE].InnerText = _port.File.CassetteSetCode;

                // 20141218 Add Sort Rule
                //if (_port.File.Type == ePortType.UnloadingPort && _port.File.Status == ePortStatus.LR)
                //{
                //    if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                //    {
                //        portNode[keyHost.PRODUCTGRADE].InnerText = _port.File.MappingGrade.Trim();

                //        Job job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                //            j.TargetPortID == "0" && j.JobGrade.Trim().Equals(_port.File.MappingGrade.Trim()) &&
                //            j.ProductType.Value.Equals(int.Parse(_port.File.ProductType)));

                //        if (job != null)
                //        {
                //            portNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();
                //        }
                //    }
                //}

                portListNode.AppendChild(portNode);

                //// Array CF Shop in Sort Line Rule - Unloader Port List
                //if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                //    _port.File.Type == ePortType.LoadingPort && _port.File.Status == ePortStatus.LR)
                //{
                //    List<Port> uldPort = ObjectManager.PortManager.GetPorts(_port.Data.NODEID).Where(p =>
                //        p.File.Type == ePortType.UnloadingPort).ToList<Port>();

                //    Job job = null;
                //    for (int i = 0; i < uldPort.Count(); i++)
                //    {
                //        job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                //            j.CurrentEQPNo == uldPort[i].Data.NODENO && j.TargetPortID == uldPort[i].Data.PORTID);
                //        if (job != null)
                //        {
                //            uldportNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                //            uldportNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                //            uldportListNode.AppendChild(uldportNode);
                //            break;
                //        }
                //    }
                //}
                //Watson add 20141121 For MES 要求
                //portListNode.AppendChild(uldportListNode);
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                //portListNode.RemoveChild(uldportListNode); // Send to OEE  20150401 Tom
                SendToOEE(xml_doc);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendToOEE(XmlDocument xml)
        {
            xMessage msg = new xMessage();
            msg.ToAgent = eAgentName.OEEAgent;
            msg.Data = xml.OuterXml;
            PutMessage(msg);
        }

        //RecipeParameterRequest
        /// <summary>
        /// RecipeParameterRequest      MES MessageSet :MES Request Recipe Parameter Registered to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_RecipeParameterRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                string inboxName = GetMESINBOXName(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (!CheckMESLineID(line.Data.LINEID))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string ppid = "";

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RecipeParameterRequest NG LINENAME=[{1}],LINERECIPENAME=[{2}],PPID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, ppid, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RecipeParameterRequest OK LINENAME=[{1}],LINERECIPENAME=[{2}],PPID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, ppid, returnCode, returnMessage));

                }

                IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();

                foreach (Port port in ObjectManager.PortManager.GetPortsByLine(line.Data.LINEID))  //Watson Modify 20141223 For 雙Line 邏輯
                {
                    string rtnmsg = string.Empty;
                    //CSOT 登京：避免正在做Parameter 收集，但MES仍繼續詢問，也會造成bc負擔
                    if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                    {
                        rtnmsg = string.Format("Port ID {0} is Busy, Cassette Status ReMap", ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                    }
                    if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                    {
                        rtnmsg = string.Format("Port ID {0} is Busy, Cassette Status is Wait For Cassette Data", ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                    }
                    if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND)
                    {
                        rtnmsg = string.Format("Port ID {0} is Busy, Cassette Status is Wait For Command", ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                    }
                    if (rtnmsg != string.Empty)
                    {
                        string inboxname = string.Empty;
                        RecipeParameterReplyNG(trxID, line.Data.LINEID, lineRecipeName, "1", rtnmsg, inboxname);

                        string err = string.Format("RECIPEPARAMETERREPLY NG LINERECIPENAME =[{0}], PPID =[{1}], ERROR :{2}", lineRecipeName, ppid, rtnmsg);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trxID, line.Data.LINEID, "[RecipeParameterReplyNG] " + err });

                        return;
                    }
                }

                //Jun Add 20141204 修改為MES PPID為L2:AA;L3:BB;L4:CC;L5:DD......
                Dictionary<string, string> eqpPPID = new Dictionary<string, string>();
                string fullPPID = string.Empty;
                eqpPPID = ObjectManager.JobManager.AddVirtualEQP_PPID(line, ppid, out fullPPID);

                //預設全部機台都要報
                IList<string> nocheckeqp = new List<string>();
                #region Watson Add PMT Line Special
                IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    EQPList = ObjectManager.EquipmentManager.GetEQPs();
                }

                #endregion
                foreach (Equipment eqp in EQPList)  //Jun Modify 20141223 By Line取得EQPs
                {
                    if (eqp.Data.RECIPEPARAVALIDATIONENABLED == "N")
                    {
                        nocheckeqp.Add(eqp.Data.NODEID);
                        continue;
                    }

                    if (eqp.Data.RECIPELEN != 0)
                    {
                        //Jun Add 20141204 修改為MES PPID為L2:AA;L3:BB;L4:CC;L5:DD......
                        if (eqpPPID.ContainsKey(eqp.Data.NODENO))
                        {
                            RecipeCheckInfo recipe = new RecipeCheckInfo();
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.PortNo = 0;      //不給值不可以
                            recipe.Parameters = new Dictionary<string, string>();
                            recipe.TrxId = trxID;

                            if (eqpPPID[eqp.Data.NODENO].Length != eqp.Data.RECIPELEN)
                            {
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("RecipeParameterRequest Error: MES PPID LENGTH ERROR, LINE NAME={0} EQP_NO=[{1}] PPID=[{2}] != RECIPE_LEN=[{3}].",
                                    line.Data.LINEID, eqp.Data.NODENO, eqpPPID[eqp.Data.NODENO], eqp.Data.RECIPELEN));

                                string err = string.Format("RECIPEPARAMETERREPLY NG LINERECIPENAME =[{0}], EQP_NO=[{1}], PPID =[{2}], MES PPID LENGTH <> EQP PPID LENGTH=[{3}]", lineRecipeName, eqp.Data.NODENO, ppid, eqp.Data.RECIPELEN);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { trxID, line.Data.LINEID, "[RecipeParameterReplyNG] " + err });

                                return;
                            }
                            #region Watson Add 20150312 For PMT Line HOT Code
                            if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                            {
                                if (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                                {
                                    if (eqp.Data.NODENO != "L4")
                                    {
                                        recipe.RecipeID = eqpPPID[eqp.Data.NODENO];
                                        recipeCheckInfos.Add(recipe);
                                    }
                                }
                                else
                                {
                                    if (eqp.Data.NODENO != "L3")
                                    {
                                        recipe.RecipeID = eqpPPID[eqp.Data.NODENO];
                                        recipeCheckInfos.Add(recipe);
                                    }
                                }
                            }
                            #endregion
                            else
                            {
                                recipe.RecipeID = eqpPPID[eqp.Data.NODENO];
                                recipeCheckInfos.Add(recipe);
                            }
                        }
                    }
                }
                //RecipeRegisterValidationCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfos,IList<string> noCheckEqp)

                Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandFromBCS", new object[] { trxID, recipeCheckInfos, nocheckeqp });

                RecipeParameterReply(trxID, line.Data.LINEID, lineRecipeName, recipeCheckInfos, inboxName);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //RecipeParameterReply
        /// <summary>
        /// RecipeParameterReply        MES MessageSet : Recipe Parameter Registered Repy to MES Request
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="pPID">Line Recipe</param>
        /// <param name="recipeCheckInfos">Recipe Para registered All EQP List(Time out,CIM Off,OK,NG)</param>
        public void RecipeParameterReply(string trxID, string lineName, string lineRecipeName, IList<RecipeCheckInfo> recipeCheckInfos, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                SetINBOXNAME(xml_doc, inboxname);


                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINERECIPENAME].InnerText = lineRecipeName;

                XmlNode recipeListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.PARA].Clone();
                recipeListNode.RemoveAll();
                string errmsg = string.Empty;
                XmlNode tempnode;
                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    if (rci.Result == eRecipeCheckResult.OK)
                    {
                        foreach (string para in rci.Parameters.Keys)
                        {
                            tempnode = recipeCloneNode.Clone();
                            tempnode[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, rci.EQPNo, para); //配合RecipeService GetRecipeParameter Decode Rule
                            tempnode[keyHost.PARAVALUE].InnerText = rci.Parameters[para];
                            recipeListNode.AppendChild(tempnode);
                        }
                    }
                    else
                    {
                        //CSOT 登京、勝平說，TIME OUT, CIM OFF 不用再回覆NG CODE與message.
                        //SETReturnCodeMsg(xml_doc, "2", "Recipe Parameter Reply NG!!");
                        //errmsg += string.Format(" [{0}][{1}] REPLY_NG ,", rci.EQPNo,rci.EqpID);

                    }
                }

                SendToMES(xml_doc);

                if (errmsg == string.Empty)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " NG LINENAME=[{1}], REASON={2}",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), errmsg));

                    string err = string.Format("RECIPEPARAMETERREPLY NG ,ERROR REASON={0} ", errmsg);
                    //CSOT 登京、勝平說，TIME OUT, CIM OFF 不用再回覆NG CODE與message.

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                    //    new object[] { trxID, line.Data.LINEID, "[RecipeParameterReplyNG] " + err });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// RecipeParameterReply        MES MessageSet : if Port Is ON.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="pPID">Line Recipe</param>
        /// <param name="recipeCheckInfos">Recipe Para registered All EQP List(Time out,CIM Off,OK,NG)</param>
        public void RecipeParameterReplyNG(string trxID, string lineName, string lineRecipeName, string rtnCode, string rtnmsg, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                //Watson Add 20141202 For MES Spec
                SetINBOXNAME(xml_doc, inboxname);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINERECIPENAME].InnerText = lineRecipeName;

                XmlNode recipeListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.PARA].Clone();
                recipeListNode.RemoveAll();

                SETReturnCodeMsg(xml_doc, rtnCode, rtnmsg);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), rtnCode, rtnmsg));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 設定送出的Message Set 裏需要Return code, Return Message
        /// Mes回傳時會帶回，可以此區別事件的差別
        /// </summary>
        /// <param name="xml">Message Xml</param>
        /// <param name="returnCode">RETURN\RETURNCODE</param>
        /// <param name="returnMsg">RETURN\RETURNMESSAGE</param>
        private void SETReturnCodeMsg(XmlDocument xml, string returnCode, string returnMsg)
        {
            try
            {
                if (returnCode != string.Empty)
                {
                    xml[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNCODE].InnerText = returnCode;
                }
                if (returnMsg != string.Empty)
                {
                    xml[keyHost.MESSAGE][keyHost.RETURN][keyHost.RETURNMESSAGE].InnerText = returnMsg;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //UnitStateChanged
        /// <summary>
        /// UnitStateChanged        MES MessageSet : Unit Status change Event Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="unit">Unit Class</param>
        /// <param name="materialChangeflag">is Material Change?</param>
        /// <param name="alarm">Alarm occurs if Unit Status is Down</param>
        public void UnitStateChanged(string trxID, Unit unit, string alarmID, string alarmText, string alarmTime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(unit.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different. 改成程式送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, unit.Data.LINEID));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    
                //    //return;   
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("UnitStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(unit.Data.NODENO, unit.Data.UNITID);
                bodyNode[keyHost.UNITSTATENAME].InnerText = unit.File.MESStatus.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, bodyNode[keyHost.LINENAME].InnerText));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (unit.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
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

        private XmlNode CreateXmlNode(XmlDocument xml, string name, string value)
        {
            XmlNode xmlNode = xml.CreateElement(name);

            if (value != null)
                xmlNode.InnerText = value;

            return xmlNode;
        }

        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                xMessage message = null;
                bool hasSend = false;
                #region MachineStateChanged Delay
                if (_equipmentStateQueue.Count > 0)
                {
                    if (_waitForMachineStateChangeReply == false)
                    { //BCS 没有等待MES回复可以直接上报
                        hasSend = true;
                    }
                    else
                    {//BCS 在等待MES回复需要Check 有没有Time out
                        TimeSpan ts = DateTime.Now.Subtract(_eqpStateLastReportTime);//MES  Time ou
                        if (ts.TotalMilliseconds > ParameterManager["MESTIMEOUT"].GetInteger())
                        {
                            hasSend = true;
                        }
                    }
                    if (hasSend)
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("MES Synchronization MachineStateChanged Queue count=({0}).", _equipmentStateQueue.Count));
                        bool done1 = _equipmentStateQueue.TryDequeue(out message);

                        if (done1)
                        {
                            lock (_timeSync)
                            {
                                _eqpStateLastReportTime = DateTime.Now;
                                _waitForMachineStateChangeReply = true;
                            }
                            PutMessage(message);
                        }
                    }
                }
                #endregion
                #region ProcessEnd
                if (_processEndQueue.Count > 0)
                {
                    TimeSpan ts = DateTime.Now.Subtract(_processEndLastReportTime);//MES  Time out
                    if (ts.TotalMilliseconds > ParameterManager["MESTIMEOUT"].GetInteger())
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("MES Synchronization LotProcessEnd Queue count=({0}).", _equipmentStateQueue.Count));

                        bool done1 = _processEndQueue.TryDequeue(out message);
                        if (done1)
                        {
                            lock (_timeSync)
                                _processEndLastReportTime = DateTime.Now;
                            PutMessage(message);
                        }
                    }
                }
                #endregion
            }
            catch
            {

            }
        }

        /// <summary>
        /// 需要先Queue住，等MES回复或Time out 在发送
        /// </summary>
        /// <param name="doc"></param>
        public void SendToQueue(XmlDocument doc)
        {
            xMessage msg = new xMessage();
            msg.Name = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.MESSAGENAME].InnerText;
            msg.TransactionID = doc[keyHost.MESSAGE][keyHost.HEADER][keyHost.TRANSACTIONID].InnerText;
            msg.ToAgent = eAgentName.MESAgent;
            msg.Data = doc.OuterXml;
            if (msg.Name == "MachineStateChanged")
            {
                _equipmentStateQueue.Enqueue(msg);
            }
            else if ((msg.Name == "LotProcessEnd") || (msg.Name == "LotProcessAbnormalEnd"))
            {
                _processEndQueue.Enqueue(msg);
            }

        }


    }
}
