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
        /// <summary>
        /// 6.36.	CheckLocalPPIDRequest       MES MessageSet :  Online local, if OP changed PPID , then BC must send to MES, and wait for MES reply.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="localrecipe">Loacal Recipe Name</param>
        /// <param name="nodelist">所有機台iList</param>
        public void CheckLocalPPIDRequest(string trxID, string lineName, string localrecipe, IList<Equipment> nodelist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] CheckLocalPPIDRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckLocalPPIDRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LOTNAME].InnerText = "";
                bodyNode[keyHost.LOCALRECIPENAME].InnerText = localrecipe;

                //to do 
                XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
                eqpListNode.RemoveAll();

                foreach (Equipment eqp in nodelist)
                {
                    XmlNode eqpnode = eqpCloneNode.Clone();
                    eqpnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    eqpnode[keyHost.LOCALNAME].InnerText = eqp.Data.NODENO;
                    eqpnode[keyHost.RECIPEID].InnerText = eqp.File.CurrentRecipeID;
                    eqpListNode.AppendChild(eqpnode);
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

        /// <summary>
        /// 6.37.	CheckLocalPPIDReply     MES MessagetSet : Check Local PPID Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CheckLocalPPIDReply(XmlDocument xmlDoc)
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
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}]  CheckLocalPPIDReply NG LINENAME =[{1}],MACHINENAME =[{2}],PAUSECOMMAND={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}] CheckLocalPPIDReply OK LINENAME =[{1}],CODE={2},MESSAGE={3}.",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        

        /// <summary>
        /// If this PPID existed at MES, then check BC's RecipeID vs MES's RecipeID.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="localRecipeName">Representative Recipe identification of Line that is created or modified on BC</param>
        /// <param name="valiResult">‘Y’ – Validation result is OK    ;‘N’ – Validation result is NG</param>
        public void CheckLocalPPIDReply(string trxID, string lineName, string localRecipeName, string valiResult)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckLocalPPIDReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LOCALRECIPENAME].InnerText = localRecipeName;
                bodyNode[keyHost.VALIRESULT].InnerText = valiResult;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.38.	CheckRecipeParameter         MES MessageSet : Upon MES request of EQ recipe parameter validation, BC reports EQ recipe parameters.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="port">Port Object</param>
        /// <param name="recipeCheckInfos">IDictionary<string, IList<RecipeCheckInfo>></param>
        public void CheckRecipeParameter(string trxID, Port port, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] CheckRecipeParameter Send MES but OFF LINE LINENAME =[{1}].", trxID, port.Data.LINEID));
                     string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, port.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckRecipeParameter") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                XmlNode lineList = bodyNode[keyHost.LINELIST];

                XmlNode lineNodeClone = lineList[keyHost.LINE].Clone();
                lineList.RemoveAll();

                foreach (string key in recipeCheckInfos.Keys)
                {
                    XmlNode lineNode = lineNodeClone.Clone();

                    IList<RecipeCheckInfo> rcis = recipeCheckInfos[key];
                    lineNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(port.Data.LINEID);
                    if (key.Trim().Equals(port.Data.LINEID.Trim()))
                    {
                        lineNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                    }
                    else
                    {
                        lineNode[keyHost.PORTNAME].InnerText = "0";
                    }
                    lineNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID.Trim();

                    XmlNode timeoutList = lineNode[keyHost.TIMEOUTEQPLIST];
                    XmlNode noCheckList = lineNode[keyHost.RECIPEPARANOCHECKLIST];
                    XmlNode cimoffList = lineNode[keyHost.CIMOFFEQPLIST];
                    XmlNode lineRecipeNameList = lineNode[keyHost.LINERECIPENAMELIST];

                    XmlNode itemClone = timeoutList[keyHost.MACHINENAME].Clone();
                    XmlNode lineRecipeClone = lineRecipeNameList[keyHost.LINERECIPE].Clone();

                    timeoutList.RemoveAll();
                    noCheckList.RemoveAll();
                    cimoffList.RemoveAll();
                    lineRecipeNameList.RemoveAll();

                    // Key: Recipe Name  Value: RecipeCheckInfo 重新整理後再報出
                    Dictionary<string, List<RecipeCheckInfo>> okRecipeInfo = new Dictionary<string, List<RecipeCheckInfo>>();

                    for (int i = 0; i < rcis.Count(); i++)
                    {
                        if (rcis[i].Result != eRecipeCheckResult.OK)
                        {
                            XmlNode item = itemClone.Clone();
                            if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rcis[i].EQPNo == "L2"))
                            {
                                item.InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                            }
                            else
                                item.InnerText = rcis[i].EqpID;

                            switch (rcis[i].Result)
                            {
                                case eRecipeCheckResult.TIMEOUT:
                                    {
                                        timeoutList.AppendChild(item);
                                    }
                                    break;
                                case eRecipeCheckResult.NOCHECK:
                                    {
                                        noCheckList.AppendChild(item);
                                    }
                                    break;
                                case eRecipeCheckResult.CIMOFF:
                                    {
                                        cimoffList.AppendChild(item);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // 找Collection是否有相同的RecipeName
                            if (okRecipeInfo.ContainsKey(rcis[i].LineRecipeName.Trim()))
                            {
                                List<RecipeCheckInfo> obj = okRecipeInfo[rcis[i].LineRecipeName.Trim()];
                                obj.Add(rcis[i]);
                            }
                            else
                            {
                                okRecipeInfo.Add(rcis[i].LineRecipeName.Trim(), new List<RecipeCheckInfo>() { rcis[i] });
                            }
                        }
                    }

                    if (okRecipeInfo.Count() > 0)
                    {
                        foreach (string key2 in okRecipeInfo.Keys)
                        {
                            List<RecipeCheckInfo> tmp = okRecipeInfo[key2];
                            XmlNode lineRecipe = lineRecipeClone.Clone();
                            lineRecipe[keyHost.LINERECIPENAME].InnerText = key2;
                            XmlNode machineList = lineRecipe[keyHost.MACHINELIST];
                            XmlNode machineNodeClone = machineList[keyHost.MACHINE].Clone();

                            machineList.RemoveAll();

                            for (int i = 0; i < tmp.Count(); i++)
                            {
                                XmlNode machine = machineNodeClone.Clone();
                                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (tmp[i].EQPNo == "L2"))
                                {
                                    machine[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                                }
                                else
                                    machine[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(tmp[i].EQPNo);
                                machine[keyHost.RECIPEID].InnerText = tmp[i].RecipeID;

                                XmlNode recipeparaList = machine[keyHost.RECIPEPARALIST];
                                XmlNode paraNodeClone = recipeparaList[keyHost.PARA].Clone();

                                recipeparaList.RemoveAll();
                                foreach (string key3 in tmp[i].Parameters.Keys)
                                {
                                    XmlNode para = paraNodeClone.Clone();
                                    para[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID ,key3); //Watson add 20141118 For new spec
                                    para[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, tmp[i].EQPNo , key3); //配合RecipeService GetRecipeParameter Decode Rule
                                    para[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, key3);
                                    #region ODF Line Special Reply Unit Recipe
                                    if (line.Data.LINEID.Contains(eLineType.CELL.CBODF))
                                    {
                                        if (para[keyHost.PARANAMEORIENTED].InnerText == eCELLODFLineRecipeParaReply.Unit_Recipe_Info)
                                        {

                                            int u = 1; //Unit No
                                            foreach (char c in tmp[i].Parameters[key3])
                                            {
                                                Unit unit = null;
                                                if (c == '1') // - 1：Recipe No Exist or Time Out
                                                    unit = ObjectManager.UnitManager.GetUnit(tmp[i].EQPNo, u.ToString());
                                                if (unit != null)
                                                {
                                                    XmlNode unitnode = itemClone.Clone();
                                                    unitnode.InnerText = unit.Data.UNITID;
                                                    timeoutList.AppendChild(unitnode);
                                                }
                                                u++;
                                            }
                                        }
                                    }
                                    #endregion
                                    para[keyHost.PARAVALUE].InnerText = tmp[i].Parameters[key3];
                                    recipeparaList.AppendChild(para);
                                }
                                machineList.AppendChild(machine);
                            }
                            lineRecipeNameList.AppendChild(lineRecipe);
                        }
                    }

                    lineList.AppendChild(lineNode);
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

        public void CheckRecipeParameter(string trxID, Line line, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] CheckRecipeParameter Send MES but OFF LINE LINENAME =[{1}].", trxID, port.Data.LINEID));
                     string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckRecipeParameter") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                XmlNode lineList = bodyNode[keyHost.LINELIST];

                XmlNode lineNodeClone = lineList[keyHost.LINE].Clone();
                lineList.RemoveAll();

                foreach (string key in recipeCheckInfos.Keys)
                {
                    XmlNode lineNode = lineNodeClone.Clone();

                    IList<RecipeCheckInfo> rcis = recipeCheckInfos[key];
                    lineNode[keyHost.LINENAME].InnerText = line.Data.NEXTLINEID;                    
                    lineNode[keyHost.PORTNAME].InnerText = "0";
                    lineNode[keyHost.CARRIERNAME].InnerText = "";
                    XmlNode timeoutList = lineNode[keyHost.TIMEOUTEQPLIST];
                    XmlNode noCheckList = lineNode[keyHost.RECIPEPARANOCHECKLIST];
                    XmlNode cimoffList = lineNode[keyHost.CIMOFFEQPLIST];
                    XmlNode lineRecipeNameList = lineNode[keyHost.LINERECIPENAMELIST];
                    XmlNode itemClone = timeoutList[keyHost.MACHINENAME].Clone();
                    XmlNode lineRecipeClone = lineRecipeNameList[keyHost.LINERECIPE].Clone();
                    timeoutList.RemoveAll();
                    noCheckList.RemoveAll();
                    cimoffList.RemoveAll();
                    lineRecipeNameList.RemoveAll();

                    // Key: Recipe Name  Value: RecipeCheckInfo 重新整理後再報出
                    Dictionary<string, List<RecipeCheckInfo>> okRecipeInfo = new Dictionary<string, List<RecipeCheckInfo>>();

                    for (int i = 0; i < rcis.Count(); i++)
                    {
                        if (rcis[i].Result != eRecipeCheckResult.OK)
                        {
                            XmlNode item = itemClone.Clone();
                            item.InnerText = rcis[i].EqpID;

                            switch (rcis[i].Result)
                            {
                                case eRecipeCheckResult.TIMEOUT:
                                    {
                                        timeoutList.AppendChild(item);
                                    }
                                    break;
                                case eRecipeCheckResult.NOCHECK:
                                    {
                                        noCheckList.AppendChild(item);
                                    }
                                    break;
                                case eRecipeCheckResult.CIMOFF:
                                    {
                                        cimoffList.AppendChild(item);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // 找Collection是否有相同的RecipeName
                            if (okRecipeInfo.ContainsKey(rcis[i].LineRecipeName.Trim()))
                            {
                                List<RecipeCheckInfo> obj = okRecipeInfo[rcis[i].LineRecipeName.Trim()];
                                obj.Add(rcis[i]);
                            }
                            else
                            {
                                okRecipeInfo.Add(rcis[i].LineRecipeName.Trim(), new List<RecipeCheckInfo>() { rcis[i] });
                            }
                        }
                    }

                    if (okRecipeInfo.Count() > 0)
                    {
                        foreach (string key2 in okRecipeInfo.Keys)
                        {
                            List<RecipeCheckInfo> tmp = okRecipeInfo[key2];
                            XmlNode lineRecipe = lineRecipeClone.Clone();
                            lineRecipe[keyHost.LINERECIPENAME].InnerText = key2;
                            XmlNode machineList = lineRecipe[keyHost.MACHINELIST];
                            XmlNode machineNodeClone = machineList[keyHost.MACHINE].Clone();

                            machineList.RemoveAll();

                            for (int i = 0; i < tmp.Count(); i++)
                            {
                                XmlNode machine = machineNodeClone.Clone();
                                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (tmp[i].EQPNo == "L2"))
                                {
                                    machine[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                                }
                                else
                                    machine[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(tmp[i].EQPNo);
                                machine[keyHost.RECIPEID].InnerText = tmp[i].RecipeID;

                                XmlNode recipeparaList = machine[keyHost.RECIPEPARALIST];
                                XmlNode paraNodeClone = recipeparaList[keyHost.PARA].Clone();

                                recipeparaList.RemoveAll();
                                foreach (string key3 in tmp[i].Parameters.Keys)
                                {
                                    XmlNode para = paraNodeClone.Clone();
                                    para[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, key3); 
                                    para[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, tmp[i].EQPNo, key3); 
                                    para[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, key3);                                    
                                    para[keyHost.PARAVALUE].InnerText = tmp[i].Parameters[key3];
                                    recipeparaList.AppendChild(para);
                                }
                                machineList.AppendChild(machine);
                            }
                            lineRecipeNameList.AppendChild(lineRecipe);
                        }
                    }

                    lineList.AppendChild(lineNode);
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
        /// 6.39.	CheckRecipeParameterReply       MES MessagetSet : Check Recipe Parameter Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CheckRecipeParameterReply(XmlDocument xmlDoc)
        {
            try
            {

                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                //XmlNode linenodelist = xmlDoc[keyHost.LINELIST];
                //foreach (XmlNode linenode in linenodelist)

                string lineName = xmlDoc.SelectSingleNode("//MESSAGE/BODY/LINELIST/LINE/LINENAME").InnerText; ; //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                //string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINELIST][keyHost.LINE][keyHost.LINERECIPENAME].InnerText;
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

                    //MES 回复NG后需要将结果显示到OPI上。 20150705 Tom
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName,
                        "Recipe Parameter check NG", string.Format("Cassette =[{0}] Parameter Validation NG from MES CODE=[{1}] ,MESSAGE=[{2}], please confirm", cstid,returnCode,returnMessage) });
                    #region[Get LINE]
                    Line line = ObjectManager.LineManager.GetLine(lineName);
                    if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineName));
                    #endregion
                    //To Do Dowload CIM Message To current Equipemnt 20150704 Tom  
                    if (port != null)
                    {
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//20161101 sy add By CELL 是不同IO 
                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, port.Data.NODENO, string.Format("Cassette =[{0}] Parameter Validation NG from MES CODE=[{1}] ,MESSAGE=[{2}], please confirm", cstid, returnCode, returnMessage), "BCS","0" });
                        else
                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, port.Data.NODENO, string.Format("Cassette =[{0}] Parameter Validation NG from MES CODE=[{1}] ,MESSAGE=[{2}], please confirm", cstid, returnCode, returnMessage), "BCS" });

                    }
                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName,
                    //    "Recipe Parameter check NG", string.Format("Cassette =[{0}] Parameter Validation NG from MES, please confirm", cstid) });
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

        /// <summary>
        /// 6.123.	PreCheckRecipeParameterRequest      MES MessageSet :PreCheckRecipeParameterRequest
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_PreCheckRecipeParameterRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                string inboxname = GetMESINBOXName(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string ppid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PPID].InnerText;

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_PreCheckRecipeParameterRequest NG LINENAME=[{1}],LINERECIPENAME=[{2}],PPID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, ppid, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_PreCheckRecipeParameterRequest OK LINENAME=[{1}],LINERECIPENAME=[{2}],PPID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, ppid, returnCode, returnMessage));

                }

                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null)
                {
                    string rtnmsg = string.Format("Line Name[{0}] IS ERROR!!", lineName);
                    PreCheckRecipeParameterReplyNG(trxID, lineName, lineRecipeName, ppid, "1", rtnmsg, inboxname);
                    return;
                }

                foreach (Port port in ObjectManager.PortManager.GetPorts())
                {
                    string rtnmsg = string.Empty;
                    if (port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                    {
                        rtnmsg = string.Format("PORT ID {0} IS BUSY, CASSETTE STATUS REMAP", port.Data.PORTID);
                    }
                    if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                    {
                        rtnmsg = string.Format("PORT ID {0} IS BUSY, CASSETTE STATUS IS WAIT FOR CASSETTE DATA", port.Data.PORTID);
                    }
                    if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND)
                    {
                        rtnmsg = string.Format("PORT ID {0} IS BUSY, CASSETTE STATUS IS WAIT FOR COMMAND", port.Data.PORTID);
                    }
                    if (rtnmsg != string.Empty)
                    {

                        PreCheckRecipeParameterReplyNG(trxID, lineName, lineRecipeName, ppid, "1", rtnmsg, inboxname);

                        string err = string.Format("PRECHECKRECIPEPARAMETERREPLY NG LINERECIPENAME =[{0}], PPID =[{1}], ERROR ={2}", lineRecipeName, ppid, rtnmsg);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trxID, line.Data.LINEID, "[PreCheckRecipeParameterReplyNG] " + err });

                        return;
                    }
                }

                IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
                //Watson Add 20141217 修改為MES PPID為L2:AA;L3:BB;L4:CC;L5:DD......
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

                foreach (Equipment eqp in EQPList)//.GetEQPs())  //Jun Modify 20141223 By Line 取得EQPs
                {
                    if (eqp.Data.RECIPEPARAVALIDATIONENABLED != "Y")
                    {
                        nocheckeqp.Add(eqp.Data.NODEID);
                        continue;
                    }

                    if (eqp.Data.RECIPELEN != 0)
                    {
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
                                    string.Format("PRECHECKRECIPEPARAMETERREPLY Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} != Recipe Len={3}.",
                                    line.Data.LINEID, eqp.Data.NODENO, eqpPPID[eqp.Data.NODENO], eqp.Data.RECIPELEN));

                                string err = string.Format("PRECHECKRECIPEPARAMETERREPLY NG LINERECIPENAME =[{0}], PPID =[{1}], MES PPID LENGTH <> EQP PPID LENGTH=[{2}]", lineRecipeName, ppid, eqp.Data.RECIPELEN);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { trxID, line.Data.LINEID, "[PreCheckRecipeParameterReplyNG] " + err });

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
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("PreCheckRecipeParameterRquest Error: DP RECIPEID Length = 0, Line Name={0} Equipment No={1} PPID={2} <> Recipe Len={3}.",
                                line.Data.LINEID, eqp.Data.NODENO, eqpPPID[eqp.Data.NODENO], eqp.Data.RECIPELEN));
                    }
                }

                //RecipeRegisterValidationCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfos,IList<string> noCheckEqp)

                Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommandFromBCS", new object[] { trxID, recipeCheckInfos, nocheckeqp });

                PreCheckRecipeParameterReply(trxID, lineName, lineRecipeName, ppid, recipeCheckInfos, inboxname);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.124.	PreCheckRecipeParameterReply        MES MessageSet : MES Request Recipe Check Reply to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        ///  <param name="linerecipe">Line Recipe</param>
        /// <param name="pPID">PPID</param>
        /// <param name="timeoutEQPList">Recipe Para Check Time out EQP List</param>
        /// <param name="cimOffEQPList">Recipe Para Check CIM OFF EQP List</param>
        public void PreCheckRecipeParameterReply(string trxID, string lineName, string linerecipe, string pPID, IList<RecipeCheckInfo> recipeCheckInfos, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PreCheckRecipeParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                SetINBOXNAME(xml_doc, inboxname);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.LINERECIPENAME].InnerText = linerecipe;
                bodyNode[keyHost.PPID].InnerText = pPID;

                XmlNode timeoutList = bodyNode[keyHost.TIMEOUTEQPLIST];
                XmlNode timeoutnode = timeoutList[keyHost.MACHINENAME].Clone();
                timeoutList.RemoveAll();

                XmlNode cimoffList = bodyNode[keyHost.CIMOFFEQPLIST];
                XmlNode cimoffnode = cimoffList[keyHost.MACHINENAME].Clone();
                cimoffList.RemoveAll();

                XmlNode recipeListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.PARA].Clone();
                recipeListNode.RemoveAll();

                string errmsg = string.Empty;
                XmlNode tempnode;
                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    if (rci.Result == eRecipeCheckResult.TIMEOUT)
                    {
                        tempnode = timeoutnode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                        {
                            tempnode.InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        }
                        else
                            tempnode.InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        timeoutList.AppendChild(tempnode);
                        SETReturnCodeMsg(xml_doc, "1", ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo) + " Machine Reply Timeout!!");
                        errmsg += string.Format(" [{0}][{1}] TIMEOUT ,", rci.EQPNo, rci.EqpID);
                    }

                    if (rci.Result == eRecipeCheckResult.CIMOFF)
                    {
                        tempnode = cimoffnode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                        {
                            tempnode.InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        }
                        else
                            tempnode.InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        cimoffList.AppendChild(tempnode);
                        SETReturnCodeMsg(xml_doc, "1", ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo) + " Machine OffLine (CIM Off)!!");

                        errmsg += string.Format(" [{0}][{1}] OFFLINE(CIM OFF) ,", rci.EQPNo, rci.EqpID);
                    }

                    int ii = 0;
                    if (rci.Result == eRecipeCheckResult.OK)
                    {
                        foreach (string para in rci.Parameters.Keys)
                        {
                            tempnode = recipeCloneNode.Clone();
                            if (rci.RecipeID.Equals("00"))
                            {
                                tempnode[keyHost.PARANAME].InnerText = "";
                                tempnode[keyHost.PARANAMEORIENTED].InnerText = "M"; //Watson add 20141213 For new spec 
                            }
                            else
                            {
                                tempnode[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, para); //Watson add 20141213 For new spec 
                                tempnode[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, rci.EQPNo, para); //配合RecipeService GetRecipeParameter Decode Rule
                                tempnode[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, para);
                            }
                            #region ODF Line Special Reply Unit Recipe
                            if (line.Data.LINEID.Contains(eLineType.CELL.CBODF))
                            {
                                if (tempnode[keyHost.PARANAMEORIENTED].InnerText == eCELLODFLineRecipeParaReply.Unit_Recipe_Info)
                                {

                                    int u = 1; //Unit No
                                    foreach (char c in rci.Parameters[para])
                                    {
                                        Unit unit = null;
                                        if (c == '1') // - 1：Recipe No Exist or Time Out
                                            unit = ObjectManager.UnitManager.GetUnit(rci.EQPNo, u.ToString());
                                        if (unit != null)
                                        {
                                            XmlNode unitnode = timeoutnode.Clone();
                                            unitnode.InnerText = unit.Data.UNITID;
                                            timeoutList.AppendChild(unitnode);
                                        }
                                        u++;
                                    }
                                }
                            }
                            #endregion
                            if (int.TryParse(rci.Parameters[para], out ii))
                                tempnode[keyHost.VALUETYPE].InnerText = "NUMBER";
                            else
                                tempnode[keyHost.VALUETYPE].InnerText = "TEXT";
                            tempnode[keyHost.PARAVALUE].InnerText = rci.Parameters[para];
                            recipeListNode.AppendChild(tempnode);
                        }
                    }
                    else
                    {
                        //CSOT 登京、勝平說，TIME OUT, CIM OFF 不用再回覆NG CODE與message.
                        //SETReturnCodeMsg(xml_doc, "2", ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo) + " Machine Result NG!!");
                        //errmsg += string.Format(" [{0}][{1}] REPLY_NG ,", rci.EQPNo, rci.EqpID);
                    }
                }

                SendToMES(xml_doc);

                if (errmsg == string.Empty)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                }
                else
                {
                    string err = string.Format("PRECHECKRECIPEPARAMETERREPLY NG ,ERROR REASON={0} ", errmsg);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trxID, line.Data.LINEID, "[RecipeParameterReplyNG] " + err });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.124.	PreCheckRecipeParameterReply        MES MessageSet : Port IS Busy Reply NG
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        ///  <param name="linerecipe">Line Recipe</param>
        /// <param name="pPID">PPID</param>
        /// <param name="timeoutEQPList">Recipe Para Check Time out EQP List</param>
        /// <param name="cimOffEQPList">Recipe Para Check CIM OFF EQP List</param>
        public void PreCheckRecipeParameterReplyNG(string trxID, string lineName, string linerecipe, string pPID, string rtnCode, string rtnmsg, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PreCheckRecipeParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                SetINBOXNAME(xml_doc, inboxname);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.LINERECIPENAME].InnerText = linerecipe;
                bodyNode[keyHost.PPID].InnerText = pPID;

                XmlNode timeoutList = bodyNode[keyHost.TIMEOUTEQPLIST];
                XmlNode timeoutnode = timeoutList[keyHost.MACHINENAME].Clone();
                timeoutList.RemoveAll();

                XmlNode cimoffList = bodyNode[keyHost.CIMOFFEQPLIST];
                XmlNode cimoffnode = cimoffList[keyHost.MACHINENAME].Clone();
                cimoffList.RemoveAll();

                XmlNode recipeListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.PARA].Clone();
                recipeListNode.RemoveAll();

                SETReturnCodeMsg(xml_doc, rtnCode, rtnmsg);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.134.	RecipeParameterChangeRequest        MES MessageSet :  When EQ create/change recipe Parameter report to MES to validate this create/change.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="machineName">Equipment No</param>
        /// <param name="recipeID">Recipe ID</param>
        /// <param name="parameters"></param>
        /// 
        public void RecipeParameterChangeRequest(string trxID, string lineName, string machienName, string recipeID, IDictionary<string, string> parameters)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeParameterChangeRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machienName);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                if ((lineName.Contains(keyCELLPMTLINE.CBPTI) && (eqp.Data.NODENO == "L2")))
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();

                bodyNode[keyHost.RECIPEID].InnerText = recipeID;
                //20141124 Delete by Edison：Follow new Spec
                //bodyNode[keyHost.RECIPENOPARAMETERNAME].InnerText = machienName + "_RECIPE_ID";

                XmlNode paraListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode paraCloneNode = paraListNode[keyHost.PARA].Clone();
                paraListNode.RemoveAll();

                foreach (string key in parameters.Keys)
                {
                    XmlNode paranode = paraCloneNode.Clone();
                    paranode[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, key);
                    paranode[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, eqp.Data.NODENO, key); //配合RecipeService GetRecipeParameter Decode Rule
                    paranode[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, key);
                    paranode[keyHost.PARAVALUE].InnerText = parameters[key];
                    paraListNode.AppendChild(paranode);
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

        /// <summary>
        /// 6.135.	RecipeParameterChangeReply      MES MessagetSet : Recipe Parameter Change Reply to RecipeService
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_RecipeParameterChangeReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                    return;
                }

                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string recipeid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RECIPEID].InnerText;
                string valiResult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES]=[{1}] RecipeParameterChangeReply .",
                                    trxID, lineName));

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeParameterChangeReply NG LINENAME=[{1}],MACHINENAME=[{2}],RECIPEID=[{3}],VALIRESULT=[{4}],CODE=[{5}],MESSAGE=[{6}].",
                                         trxID, lineName, machineName, recipeid, valiResult, returnCode, returnMessage));
                    //to do?
                    valiResult = "N";
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeParameterChangeReply OK LINENAME=[{1}],MACHINENAME=[{2}],RECIPEID=[{3}],VALIRESULT=[{4}],CODE=[{5}],MESSAGE=[{6}].",
                                         trxID, lineName, machineName, recipeid, valiResult, returnCode, returnMessage));
                    //to do?
                }

                Invoke(eServiceName.RecipeService, "RecipeParameterMESReply", new object[]{
                    trxID,
                    lineName,
                    machineName,
                    valiResult,
                });

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }

        /// <summary>
        /// 6.136.	RecipeParameterRequest      MES MessageSet :MES Request Recipe Parameter Registered to BC
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
                string ppid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PPID].InnerText;

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
                        RecipeParameterReplyNG(trxID, line.Data.LINEID, lineRecipeName, ppid, "1", rtnmsg, inboxname);

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
                        if (eqp.Data.NODEATTRIBUTE != "VIRTUAL")//虚拟机台则无需做Recipe Parameter Request 20150704 Tom
                            nocheckeqp.Add(eqp.Data.NODEID);
                        //nocheckeqp.Add(eqp.Data.NODEID);
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

                RecipeParameterReply(trxID, line.Data.LINEID, lineRecipeName, ppid, recipeCheckInfos, inboxName);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.137.	RecipeParameterReply        MES MessageSet : Recipe Parameter Registered Repy to MES Request
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="pPID">Line Recipe</param>
        /// <param name="recipeCheckInfos">Recipe Para registered All EQP List(Time out,CIM Off,OK,NG)</param>
        public void RecipeParameterReply(string trxID, string lineName, string lineRecipeName, string ppid, IList<RecipeCheckInfo> recipeCheckInfos, string inboxname)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeParameterReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                SetINBOXNAME(xml_doc, inboxname);


                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINERECIPENAME].InnerText = lineRecipeName;
                bodyNode[keyHost.PPID].InnerText = ppid;

                XmlNode timeoutList = bodyNode[keyHost.TIMEOUTEQPLIST];
                XmlNode timeoutnode = timeoutList[keyHost.MACHINENAME].Clone();
                timeoutList.RemoveAll();

                XmlNode cimoffList = bodyNode[keyHost.CIMOFFEQPLIST];
                XmlNode cimoffnode = cimoffList[keyHost.MACHINENAME].Clone();
                cimoffList.RemoveAll();

                XmlNode recipeListNode = bodyNode[keyHost.RECIPEPARALIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.PARA].Clone();
                recipeListNode.RemoveAll();
                string errmsg = string.Empty;
                XmlNode tempnode;
                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    if (rci.Result == eRecipeCheckResult.TIMEOUT)
                    {
                        tempnode = timeoutnode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode.InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode.InnerText = rci.EqpID;
                        timeoutList.AppendChild(tempnode);
                        //CSOT 登京、勝平說，TIME OUT, CIM OFF 不用再回覆NG CODE與message.
                        //SETReturnCodeMsg(xml_doc, "1", rci.EqpID + " Machine Time Out !!");
                        //errmsg += string.Format(" [{0}][{1}] TIMEOUT ,", rci.EQPNo,rci.EqpID);
                    }

                    if (rci.Result == eRecipeCheckResult.CIMOFF)
                    {
                        tempnode = cimoffnode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode.InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode.InnerText = rci.EqpID;
                        cimoffList.AppendChild(tempnode);
                        //CSOT 登京、勝平說，TIME OUT, CIM OFF 不用再回覆NG CODE與message.
                        //SETReturnCodeMsg(xml_doc, "1", rci.EqpID + " Machine Offline(CIM Off)!!");
                        //errmsg += string.Format(" [{0}][{1}] OFFLINE(CIM OFF) ,", rci.EQPNo,rci.EqpID);
                    }

                    int ii = 0;
                    if (rci.Result == eRecipeCheckResult.OK)
                    {
                        foreach (string para in rci.Parameters.Keys)
                        {
                            tempnode = recipeCloneNode.Clone();
                            tempnode[keyHost.TRACELEVEL].InnerText = GetPARANAME_TRACELEVEL(line.Data.LINEID, para); //Watson Add 20141118 For MES Spec
                            tempnode[keyHost.PARANAME].InnerText = GetPARANAME(line.Data.LINEID, rci.EQPNo, para); //配合RecipeService GetRecipeParameter Decode Rule
                            tempnode[keyHost.PARANAMEORIENTED].InnerText = GetPARANAMEORIENTED(line.Data.LINEID, para);
                            #region ODF Line Special Reply Unit Recipe
                            if (line.Data.LINEID.Contains(eLineType.CELL.CBODF))
                            {
                                if (tempnode[keyHost.PARANAMEORIENTED].InnerText == eCELLODFLineRecipeParaReply.Unit_Recipe_Info)
                                {

                                    int u = 1; //Unit No
                                    foreach (char c in rci.Parameters[para])
                                    {
                                        Unit unit = null;
                                        if (c == '1') // - 1：Recipe No Exist or Time Out
                                            unit = ObjectManager.UnitManager.GetUnit(rci.EQPNo, u.ToString());
                                        if (unit != null)
                                        {
                                            XmlNode unitnode = timeoutnode.Clone();
                                            unitnode.InnerText = ObjectManager.UnitManager.GetUnitID(unit.Data.NODENO, unit.Data.UNITID);
                                            timeoutList.AppendChild(unitnode);
                                        }
                                        u++;
                                    }
                                }
                            }
                            #endregion
                            if (int.TryParse(rci.Parameters[para], out ii))
                                tempnode[keyHost.VALUETYPE].InnerText = "NUMBER";
                            else
                                tempnode[keyHost.VALUETYPE].InnerText = "TEXT";
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
        /// 6.137.	RecipeParameterReply        MES MessageSet : if Port Is ON.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineID</param>
        /// <param name="pPID">Line Recipe</param>
        /// <param name="recipeCheckInfos">Recipe Para registered All EQP List(Time out,CIM Off,OK,NG)</param>
        public void RecipeParameterReplyNG(string trxID, string lineName, string lineRecipeName, string ppid, string rtnCode, string rtnmsg, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        // string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
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
                bodyNode[keyHost.PPID].InnerText = ppid;

                XmlNode timeoutList = bodyNode[keyHost.TIMEOUTEQPLIST];
                XmlNode timeoutnode = timeoutList[keyHost.MACHINENAME].Clone();
                timeoutList.RemoveAll();

                XmlNode cimoffList = bodyNode[keyHost.CIMOFFEQPLIST];
                XmlNode cimoffnode = cimoffList[keyHost.MACHINENAME].Clone();
                cimoffList.RemoveAll();

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
        /// 6.138.	RecipeRegisterRequest       MES MessageSet :MES  Recipe Register Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_RecipeRegisterRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (!CheckMESLineID(line.Data.LINEID))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, line.Data.LINEID));
                }

                //to Do
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string checkeqpFlag = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CHECKEQPFLAG].InnerText;

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RecipeRegisterRequest NG LINENAME=[{1}],LINERECIPENAME=[{2}],CHECKEQPFLAG=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                        trxID, line.Data.LINEID, lineRecipeName, checkeqpFlag, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RecipeRegisterRequest OK LINENAME=[{1}],LINERECIPENAME=[{2}],CHECKEQPFLAG=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                        trxID, line.Data.LINEID, lineRecipeName, checkeqpFlag, returnCode, returnMessage));
                }


                IList<RecipeCheckInfo> recipelist = new List<RecipeCheckInfo>();

                if (line == null)
                {
                    RecipeRegisterRequestReplyNG(trxID, line.Data.LINEID, lineRecipeName);

                    string err = string.Format("RECIPE REGISTER REQUEST REPLY NG LINEID FAILED =[{0}]", line.Data.LINEID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trxID, line.Data.LINEID, "[" + MethodBase.GetCurrentMethod().Name + "] " + err });
                    return;
                }

                RECIPE recipe = ObjectManager.RecipeManager.FindOne(line.Data.LINETYPE, lineRecipeName, line.File.HostMode.ToString());
                if (recipe != null)
                {
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))  //Jun Modify 20141223 By Line取得EQPs
                    {
                        RecipeCheckInfo recipecheckinfo = new RecipeCheckInfo();
                        recipecheckinfo.RecipeID = recipe.PPID.Substring(eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN);
                        recipecheckinfo.TrxId = trxID;
                        recipecheckinfo.EQPNo = eqp.Data.NODENO;
                        recipelist.Add(recipecheckinfo);
                    }

                    IList<string> nocheckeqp = new List<string>();
                    // (string trxID, IList<RecipeCheckInfo> recipeCheckInfos,IList<string> noCheckEqp)
                    Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromBCS", new object[] { trxID, recipelist, nocheckeqp });
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RecipeRegisterRequest NG ,LINERECIPENAME=[{2}],CHECKEQPFLAG=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                            trxID, line.Data.LINEID, lineRecipeName, checkeqpFlag, returnCode, returnMessage));
                    RecipeRegisterRequestReplyNG(trxID, line.Data.LINEID, lineRecipeName);

                    string err = string.Format("RECIPE REGISTER REQUEST REPLY NG LINERECIPENAME =[{0}] Find No PPID.", lineRecipeName);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trxID, line.Data.LINEID, "[" + MethodBase.GetCurrentMethod().Name + "] " + err });

                    return;
                }

                //目前還沒有spec
                //RecipeRegisterRequestReply(trxID, lineName, lineRecipeName, recipeCheckInfos);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.139.	RecipeRegisterRequestReply      目前還沒有specMES MessageSet : Reports all recipe register result of specified Recipe registered in EQP.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="ppid">PPID</param>
        /// <param name="registerEQP">EQP List</param>
        /// <param name="recipe"></param>
        public void RecipeRegisterRequestReply(string trxID, string lineName, string ppid, IList<Equipment> registerEQP, IList<Equipment> nodelist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but LINENAME=[{1}] is error!!", trxID,
                        ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    return;
                }

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeRegisterRequestReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.LINERECIPENAME].InnerText = ppid;
                bodyNode[keyHost.BCREGISTERRESULT].InnerText = "OK";

                XmlNode eqregListNode = bodyNode[keyHost.EQREGISTERRESULTLIST];
                XmlNode eqregCloneNode = eqregListNode[keyHost.EQREGISTERRESULT].Clone();
                eqregListNode.RemoveAll();

                foreach (Equipment eqp in registerEQP)
                {
                    XmlNode eqregNode = eqregCloneNode.Clone();
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                        eqregNode[keyHost.EQUIPMENTID].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    else
                        eqregNode[keyHost.EQUIPMENTID].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    eqregNode[keyHost.RECIPEID].InnerText = eqp.File.CurrentRecipeID;
                    eqregNode[keyHost.RETURNTEXT].InnerText = "OK"; //不知道要填什麼
                    eqregListNode.AppendChild(eqregNode);
                }

                XmlNode recipeListNode = bodyNode[keyHost.RECIPENUMBERLIST];
                XmlNode recipeCloneNode = recipeListNode[keyHost.RECIPENUMBER].Clone();
                recipeListNode.RemoveAll();
                foreach (Equipment eqp in nodelist)
                {
                    XmlNode recipeNode = recipeCloneNode.Clone();
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (eqp.Data.NODENO == "L2"))
                        recipeNode[keyHost.EQUIPMENTID].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    else
                        recipeNode[keyHost.EQUIPMENTID].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    recipeNode[keyHost.RECIPENUMBER].InnerText = eqp.File.CurrentRecipeID;
                    recipeNode[keyHost.LOCALNAME].InnerText = eqp.Data.NODENO;
                    recipeListNode.AppendChild(recipeNode);
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

        /// <summary>
        /// 6.139.	RecipeRegisterRequestReply
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="linerecipename"></param>
        public void RecipeRegisterRequestReplyNG(string trxID, string lineName, string linerecipename)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but LINENAME=[{1}] is error!!", trxID,
                        ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    return;
                }

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, 
                        //ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeRegisterRequestReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.LINERECIPENAME].InnerText = linerecipename;
                bodyNode[keyHost.BCREGISTERRESULT].InnerText = "NG";

                XmlNode eqregListNode = bodyNode[keyHost.EQREGISTERRESULTLIST];
                //XmlNode eqregCloneNode = eqregListNode[keyHost.EQREGISTERRESULT].Clone();
                eqregListNode.RemoveAll();


                XmlNode recipeListNode = bodyNode[keyHost.RECIPENUMBERLIST];
                //XmlNode recipeCloneNode = recipeListNode[keyHost.RECIPENUMBER].Clone();
                recipeListNode.RemoveAll();


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

        /// <summary>
        /// 6.140.	RecipeIDRegisterCheckRequest        MES MessagetSet : Recipe ID Register Check to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_RecipeIDRegisterCheckRequest(XmlDocument xmlDoc)
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
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string actiontype = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ACTIONTYPE].InnerText;
                string inBoxName = GetMESINBOXName(xmlDoc);

                XmlNode eqpList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINELIST];

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG LINENAME=[{1}],LINERECIPENAME={2},CODE=[{3}],MESSAGE=[{4}].",
                                         trxID, lineName, lineRecipeName, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest OK LINENAME=[{1}],LINERECIPENAME={2},CODE=[{3}],MESSAGE=[{4}].",
                                         trxID, lineName, lineRecipeName, returnCode, returnMessage));

                }

                Line line = ObjectManager.LineManager.GetLine(lineName);

                IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
                IList<RecipeCheckInfo> recipeNoCheckInfos = new List<RecipeCheckInfo>();

                if (eqpList != null)
                {
                    foreach (XmlNode eqpNode in eqpList)
                    {

                        if (eqpNode == null)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MACHINE is Null LINENAME=[{1}],CODE=[{3}],MESSAGE=[{4}].",
                                trxID, lineName, returnCode, returnMessage));
                            continue;
                        }

                        string localNo = eqpNode[keyHost.LOCALNAME].InnerText;
                        string recipeid = eqpNode[keyHost.RECIPEID].InnerText;
                        string eventcomment = eqpNode[keyHost.EVENTCOMMENT].InnerText;
                        RecipeCheckInfo recipe = new RecipeCheckInfo();

                        string meseqpID = eqpNode[keyHost.MACHINENAME].InnerText;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(meseqpID);

                        #region Watson Modify 20150311 For CELL PMT Line
                        if (eqp == null)
                        {
                            if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                            {
                                if (meseqpID == ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString())
                                    eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                            }
                        }
                        #endregion

                        if (eqp == null)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MACHINENAME(EQP ID) is Null ,LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, meseqpID, returnCode, returnMessage));
                            recipe.Result = eRecipeCheckResult.NOMACHINE; //Watson Modify 20150309
                            recipe.MESVALICODEText = "Machine QTY is different, BC does not have " + meseqpID;   //Watson Modify 20150203 For MES 
                            //asir 2014/11/20
                            recipe.EqpID = meseqpID;//eqp.Data.NODEID;
                            recipe.EqpName = ""; //eqp.Data.NODEID;
                            recipe.EQPNo = ""; //eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        if (eqp.Data.NODENO != localNo)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG LOCALNAME =[{5}] <> BC Local Eqpuipment No =[{6}] ,LINENAME=[{1}],LOCALNAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, localNo, returnCode, returnMessage, localNo, eqp.Data.NODENO));
                            recipe.Result = eRecipeCheckResult.MACHINENG;
                            recipe.MESVALICODEText = "Machine local number is " + eqp.Data.NODENO + " at BC";
                            //asir 2014/11/20
                            recipe.EqpID = eqp.Data.NODEID;
                            recipe.EqpName = eqp.Data.NODEID;
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        if (eqp.Data.RECIPELEN != recipeid.Length)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MES Recipe Length =[{5}] <> BC Setting Recipe Length  =[{6}],LINENAME=[{1}],LOCALNAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, localNo, returnCode, returnMessage, recipeid.Length, eqp.Data.RECIPELEN));
                            recipe.Result = eRecipeCheckResult.RECIPELENNG;
                            recipe.MESVALICODEText = "Machine recipe ID length is " + recipeid.Length + ", but BC length is " + eqp.Data.RECIPELEN;
                            //asir 2014/11/20
                            recipe.EqpID = eqp.Data.NODEID;
                            recipe.EqpName = eqp.Data.NODEID;
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        recipe.EQPNo = localNo;
                        recipe.EqpID = eqp.Data.NODEID;
                        recipe.EqpName = eqp.Data.NODEID;
                        recipe.EQPNo = eqp.Data.NODENO;
                        recipe.RecipeID = recipeid;
                        recipe.EventComment = eventcomment;
                        recipe.TrxId = trxID;
                        recipe.Parameters = new Dictionary<string, string>();

                        //因為在Function : RecipeSerivce中 RecipeRegisterValidationCommandFromBCS 
                        //就算是CIM OFF也會回覆OK(為了ValidateCassette 可以順利進行)
                        //但是在RecipeIDRegisterCheckRequest (MES->BC) BC需要回覆CIM OFF是NG
                        //所以在Recipe下給機台時就先check EQP CIM Mode，
                        //如果是OFF 就不再進行Register

                        if (eqp.File.CIMMode == eBitResult.ON)
                        {
                            recipeCheckInfos.Add(recipe);
                        }
                        else
                        {
                            recipe.Result = eRecipeCheckResult.CIMOFF;
                            recipe.MESVALICODEText = "Machine is  offline";
                            recipeNoCheckInfos.Add(recipe);
                        }
                    }
                }
                IList<string> nocheckeqp = new List<string>();
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                {
                    // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150331
                    //此处需要增加判断Virtual的机台MES不会Download Recipe ID Register Check 20150704 Tom
                    if (eqp.Data.NODEATTRIBUTE == "CV5" || eqp.Data.NODEATTRIBUTE == "VIRTUAL")
                        continue;

                    RecipeCheckInfo ret = recipeCheckInfos.FirstOrDefault(w => w.EQPNo == eqp.Data.NODENO);
                    if (ret == null)
                    {
                        nocheckeqp.Add(eqp.Data.NODEID);

                        #region Watson Add 20150326 For MES寶哥新需求
                        RecipeCheckInfo retno = recipeNoCheckInfos.FirstOrDefault(w => w.EQPNo == eqp.Data.NODENO);
                        if (retno == null)
                        {
                            RecipeCheckInfo recipe = new RecipeCheckInfo();
                            recipe.EqpID = eqp.Data.NODEID;
                            recipe.EqpName = eqp.Data.NODEID;
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.RecipeID = string.Empty;
                            recipe.EventComment = string.Empty;
                            recipe.TrxId = trxID;
                            recipe.Result = eRecipeCheckResult.MACHINENG;
                            recipe.MESVALICODEText = string.Format("MES NOT DOWNLOAD [{0}] [{1}]", eqp.Data.NODENO, eqp.Data.NODEID);
                            recipeNoCheckInfos.Add(recipe);
                        }
                        #endregion
                    }
                }
                //RecipeRegisterValidationCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfos,IList<string> noCheckEqp)
                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromBCS", new object[] { trxID, recipeCheckInfos, nocheckeqp });



                RecipeIDRegisterCheckReply(trxID, lineName, lineRecipeName, actiontype, inBoxName, recipeCheckInfos, recipeNoCheckInfos);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }

        /// <summary>
        /// 6.140.	RecipeIDRegisterCheckRequest
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_RecipeIDRegisterCheckRequest2(XmlDocument xmlDoc)
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
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string actiontype = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ACTIONTYPE].InnerText;
                string inBoxName = GetMESINBOXName(xmlDoc);

                XmlNode eqpList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINELIST];

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG LINENAME=[{1}],LINERECIPENAME={2},CODE=[{3}],MESSAGE=[{4}].",
                                         trxID, lineName, lineRecipeName, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest OK LINENAME=[{1}],LINERECIPENAME={2},CODE=[{3}],MESSAGE=[{4}].",
                                         trxID, lineName, lineRecipeName, returnCode, returnMessage));

                }

                Line line = ObjectManager.LineManager.GetLine(lineName);

                IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
                IList<RecipeCheckInfo> recipeNoCheckInfos = new List<RecipeCheckInfo>();

                if (eqpList != null)
                {
                    foreach (XmlNode eqpNode in eqpList)
                    {

                        if (eqpNode == null)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MACHINE is Null LINENAME=[{1}],CODE=[{3}],MESSAGE=[{4}].",
                                trxID, lineName, returnCode, returnMessage));
                            continue;
                        }

                        string localNo = eqpNode[keyHost.LOCALNAME].InnerText;
                        string recipeid = eqpNode[keyHost.RECIPEID].InnerText;
                        string eventcomment = eqpNode[keyHost.EVENTCOMMENT].InnerText;
                        RecipeCheckInfo recipe = new RecipeCheckInfo();


                        string meseqpID = eqpNode[keyHost.MACHINENAME].InnerText;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(meseqpID);

                        #region Watson Modify 20150311 For CELL PMT Line
                        if (eqp == null)
                        {
                            if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                            {
                                if (meseqpID == ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString())
                                    eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                            }
                        }
                        #endregion

                        if (eqp == null)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MACHINENAME(EQP ID) is Null ,LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, meseqpID, returnCode, returnMessage));
                            recipe.Result = eRecipeCheckResult.NOMACHINE; //Watson Modify 20150309
                            recipe.MESVALICODEText = "Machine QTY is different, BC does not have " + meseqpID;   //Watson Modify 20150203 For MES 
                            //asir 2014/11/20
                            recipe.EqpID = meseqpID;//eqp.Data.NODEID;
                            recipe.EqpName = ""; //eqp.Data.NODEID;
                            recipe.EQPNo = ""; //eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        if (eqp.Data.NODENO != localNo)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG LOCALNAME =[{5}] <> BC Local Eqpuipment No =[{6}] ,LINENAME=[{1}],LOCALNAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, localNo, returnCode, returnMessage, localNo, eqp.Data.NODENO));
                            recipe.Result = eRecipeCheckResult.MACHINENG;
                            recipe.MESVALICODEText = "Machine local number is " + eqp.Data.NODENO + " at BC";
                            //asir 2014/11/20
                            recipe.EqpID = eqp.Data.NODEID;
                            recipe.EqpName = eqp.Data.NODEID;
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        if (eqp.Data.RECIPELEN != recipeid.Length)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_RecipeIDRegisterCheckRequest NG MES Recipe Length =[{5}] <> BC Setting Recipe Length  =[{6}],LINENAME=[{1}],LOCALNAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                    trxID, lineName, localNo, returnCode, returnMessage, recipeid.Length, eqp.Data.RECIPELEN));
                            recipe.Result = eRecipeCheckResult.RECIPELENNG;
                            recipe.MESVALICODEText = "Machine recipe ID length is " + recipeid.Length + ", but BC length is " + eqp.Data.RECIPELEN;
                            //asir 2014/11/20
                            recipe.EqpID = eqp.Data.NODEID;
                            recipe.EqpName = eqp.Data.NODEID;
                            recipe.EQPNo = eqp.Data.NODENO;
                            recipe.RecipeID = recipeid;
                            recipe.EventComment = eventcomment;
                            recipe.TrxId = trxID;
                            recipeNoCheckInfos.Add(recipe);
                            continue;
                        }

                        recipe.EQPNo = localNo;
                        recipe.EqpID = eqp.Data.NODEID;
                        recipe.EqpName = eqp.Data.NODEID;
                        recipe.EQPNo = eqp.Data.NODENO;
                        recipe.RecipeID = recipeid;
                        recipe.EventComment = eventcomment;
                        recipe.TrxId = trxID;
                        recipe.Parameters = new Dictionary<string, string>();

                        //因為在Function : RecipeSerivce中 RecipeRegisterValidationCommandFromBCS 
                        //就算是CIM OFF也會回覆OK(為了ValidateCassette 可以順利進行)
                        //但是在RecipeIDRegisterCheckRequest (MES->BC) BC需要回覆CIM OFF是NG
                        //所以在Recipe下給機台時就先check EQP CIM Mode，
                        //如果是OFF 就不再進行Register

                        if (eqp.File.CIMMode == eBitResult.ON)
                        {
                            recipeCheckInfos.Add(recipe);
                        }
                        else
                        {
                            recipe.Result = eRecipeCheckResult.CIMOFF;
                            recipe.MESVALICODEText = "Machine is  offline";
                            recipeNoCheckInfos.Add(recipe);
                        }
                    }
                }
                IList<string> nocheckeqp = new List<string>();
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                {
                    RecipeCheckInfo ret = recipeCheckInfos.FirstOrDefault(w => w.EQPNo == eqp.Data.NODENO);
                    if (ret == null)
                        nocheckeqp.Add(eqp.Data.NODEID);
                }
                //RecipeRegisterValidationCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfos,IList<string> noCheckEqp)
                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommandFromBCS", new object[] { trxID, recipeCheckInfos, nocheckeqp });

                RecipeIDRegisterCheckReply(trxID, lineName, lineRecipeName, actiontype, inBoxName, recipeCheckInfos, recipeNoCheckInfos);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }

        /// <summary>
        /// 6.141.	RecipeIDRegisterCheckReply      MES MessageSet :  RecipeID Register Check Reply to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodelist">所有Register 的機台</param
        public void RecipeIDRegisterCheckReply(string trxID, string lineName, string lineRecipeName, string mesActionType, string inBoxName, IList<RecipeCheckInfo> recipeCheckInfos, IList<RecipeCheckInfo> NoCheckInfos)
        {
            try
            {

                Line line = ObjectManager.LineManager.GetLine(lineName);
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        // string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                //           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    return;
                //}
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RecipeIDRegisterCheckReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫      
                SetINBOXNAME(xml_doc, inBoxName);

                XmlNode headerNode = xml_doc[keyHost.MESSAGE][keyHost.HEADER];

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINERECIPENAME].InnerText = lineRecipeName;

                XmlNode eqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode eqpCloneNode = eqpListNode[keyHost.MACHINE].Clone();
                eqpListNode.RemoveAll();

                string errmsg = string.Empty;
                XmlNode tempnode;
                #region Recipe Check Reply Node

                bodyNode[keyHost.ACTIONTYPE].InnerText = mesActionType;

                bodyNode[keyHost.VALIRESULT].InnerText = "Y";   //先給'Y'，下面有判斷式有錯誤會回覆'N'

                bodyNode[keyHost.VALICODE].InnerText = ""; // MES 給什麼就要回什麼

                // Add by Kasim 20150403 濾掉虛擬機台 L19_CV#5               
                int lineEQPCount = 0;
                if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1)
                {
                    foreach (Equipment e in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                    {
                        if (e.Data.NODEATTRIBUTE != "CV5")
                            lineEQPCount += 1;
                    }
                }
                else
                {
                    lineEQPCount = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Count;
                }

                #region Watson Modify 20150311 For CELL PMT Line
                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    lineEQPCount = 2;
                }
                #endregion

                if (lineEQPCount != recipeCheckInfos.Count + NoCheckInfos.Count)  //Jun Modify 20141223 By Line取得EQPs
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] RecipeIDRegisterCheckReply NG EQP QTY is =[{2}] ,MES MACHINE QTY is =[{3}]",
                             trxID, lineName, lineEQPCount, recipeCheckInfos.Count));
                    bodyNode[keyHost.VALIRESULT].InnerText = "N"; 
                    bodyNode[keyHost.VALICODE].InnerText = "Machine QTY is different, BC has MACHINE QTY " + lineEQPCount;
                }


                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);


                    if (rci.Result == eRecipeCheckResult.TIMEOUT)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);

                        // bodyNode[keyHost.VALIRESULT].InnerText = "N";  //Watson Modify 20150326 與MES王 確認 timeout不能改變此值
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "T"; //RecipeCheck TimeOut
                        tempnode[keyHost.VALICODE].InnerText = string.Format("[{0}][{1}] MACHINE DID NOT REPONSE REQUEST", rci.EQPNo, rci.EqpID);

                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }

                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] TIMEOUT ,", rci.EQPNo, rci.EqpID);
                    }

                    if (rci.Result == eRecipeCheckResult.OK)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "Y";
                        tempnode[keyHost.VALICODE].InnerText = string.Empty;
                        eqpListNode.AppendChild(tempnode);

                    }

                    if (rci.Result == eRecipeCheckResult.NG)
                    {
                        //bodyNode[keyHost.VALIRESULT].InnerText = "N"; //Watson Modify 20150326 與MES王 確認 timeout不能改變此值
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "N";
                        tempnode[keyHost.VALICODE].InnerText = string.Format("[{0}][{1}] MACHINE REPLY NG", rci.EQPNo, rci.EqpID);
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] REPLY NG ,", rci.EQPNo, rci.EqpID);
                    }

                    if (rci.Result == eRecipeCheckResult.MACHINENG)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";  //Watson Add 20150204 For MES Mail Issue
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "M";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] LOCAL NAME ERROR ,", rci.EQPNo, rci.EqpID);
                    }

                    if (rci.Result == eRecipeCheckResult.RECIPELENNG)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";   //Watson Add 20150204 For MES Mail Issue
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "L";// Recipe Length Wrong
                        tempnode[keyHost.VALICODE].InnerText = string.Format(" [{0}][{1}] RECIPE LENGTH ERROR", rci.EQPNo, rci.EqpID);
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] RECIPE LENGTH ERROR ,", rci.EQPNo, rci.EqpID);
                    }
                    //Watson Add 20150309 For No 機台
                    if (rci.Result == eRecipeCheckResult.NOMACHINE)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "N"; //Watson Add 20150309
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] MACHINENAME ERROR ,", rci.EQPNo, rci.EqpID);
                    }
                    if (rci.Result == eRecipeCheckResult.ZERO)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "Y";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] RECIPE IS ZERO ,", rci.EQPNo, rci.EqpID);
                    }
                    if (rci.Result == eRecipeCheckResult.NOCHECK)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "B";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] NO CHECK ,", rci.EQPNo, rci.EqpID);
                    }
                }

                #endregion

                #region No Check Node
                foreach (RecipeCheckInfo rci in NoCheckInfos)
                {

                    if (rci.Result == eRecipeCheckResult.CIMOFF)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        //asir 2014/11/20 Change"O"To"F"
                        tempnode[keyHost.VALIRESULT].InnerText = "F";// CIM OFF
                        tempnode[keyHost.VALICODE].InnerText = string.Format(" [{0}][{1}] CIM OFF ,", rci.EQPNo, rci.EqpID);
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] CIM OFF ,", rci.EQPNo, rci.EqpID);
                    }
                    //asir 2014/11/20 Add
                    if (rci.Result == eRecipeCheckResult.NG)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "N";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] REPLY NG ,", rci.EQPNo, rci.EqpID);
                    }
                    //asir 2014/11/20 Add
                    if (rci.Result == eRecipeCheckResult.MACHINENG)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";  //Watson Add 20150204 For MES Mail Issue
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "M";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                            bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] LOCAL NAME ERROR ,", rci.EQPNo, rci.EqpID);
                    }
                    //asir 2014/11/20 Add
                    if (rci.Result == eRecipeCheckResult.RECIPELENNG)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";  //Watson Add 20150204 For MES Mail Issue
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "L";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] RECIPE LENGTH ERROR ,", rci.EQPNo, rci.EqpID);
                    }
                    //Watson Add 20150310 For No 機台
                    if (rci.Result == eRecipeCheckResult.NOMACHINE)
                    {
                        bodyNode[keyHost.VALIRESULT].InnerText = "N";
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "N";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] MACHINENAME ERROR ,", rci.EQPNo, rci.EqpID);
                    }
                    if (rci.Result == eRecipeCheckResult.ZERO)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "Y";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] RECIPE IS ZERO ,", rci.EQPNo, rci.EqpID);
                    }
                    if (rci.Result == eRecipeCheckResult.NOCHECK)
                    {
                        tempnode = eqpCloneNode.Clone();
                        if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)) && (rci.EQPNo == "L2"))
                            tempnode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        else
                            tempnode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(rci.EQPNo);
                        tempnode[keyHost.LOCALNAME].InnerText = rci.EQPNo;
                        tempnode[keyHost.RECIPEID].InnerText = rci.RecipeID;
                        tempnode[keyHost.EVENTCOMMENT].InnerText = rci.EventComment;
                        tempnode[keyHost.VALIRESULT].InnerText = "B";
                        tempnode[keyHost.VALICODE].InnerText = rci.MESVALICODEText;
                        if (bodyNode[keyHost.VALICODE].InnerText.Trim() == string.Empty)
                            bodyNode[keyHost.VALICODE].InnerText = tempnode[keyHost.VALICODE].InnerText;
                        else
                        {
                            if (tempnode[keyHost.VALICODE].InnerText.Trim() != string.Empty)
                                bodyNode[keyHost.VALICODE].InnerText += ";" + tempnode[keyHost.VALICODE].InnerText;
                        }
                        eqpListNode.AppendChild(tempnode);
                        errmsg += string.Format(" [{0}][{1}] NO CHECK ,", rci.EQPNo, rci.EqpID);
                    }
                }
                #endregion
                bodyNode[keyHost.EVENTUSER].InnerText = "";
                
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    bodyNode[keyHost.VALIRESULT].InnerText = "O";
                }

                SendToMES(xml_doc);

                if (errmsg == string.Empty)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, lineName));

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " NG LINENAME=[{1}]. REASON={2}",
                        trxID, lineName, errmsg));

                    string err = string.Format("RECIPEIDREGISTERCHECKREPLY NG ,ERROR REASON={0} ", errmsg);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trxID, line.Data.LINEID, "[RecipeIDRegisterCheckReply] " + err });
                }


                #region Watson Add 20150326 For 登京OPI Download Recipe，存入DB
                if (bodyNode[keyHost.VALIRESULT].InnerText == "Y")
                {
                    string ppid = string.Empty;
                    foreach (RecipeCheckInfo rci in recipeCheckInfos)
                    {
                        ppid += rci.EQPNo + ":" + rci.RecipeID + ";";
                    }
                    foreach (RecipeCheckInfo rci in NoCheckInfos)
                    {
                        ppid += rci.EQPNo + ":" + rci.RecipeID + ";";
                    }
                    string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;
                    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid, ip, eRecipeType.LOT);
                }
                #endregion



            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
