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
        /// 6.158.	ValidateMaskPrepareRequest      MES MessageSet : EQP reads mask ID Report to MES 
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="maskid">Validate Mask</param>
        /// <param name="eqpID">EQP ID</param>
        /// <param name="maxcount">Mask count</param>
        /// <param name="jobid">any  one job in the current machine</param>
        public void ValidateMaskPrepareRequest(string trxID, string lineName, string eqpID, string maskid, string jobid)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateMaskPrepareRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.MASKNAME].InnerText = maskid;
                //bodyNode[keyHost.MAXUSECOUNT].InnerText = maxcount;
                bodyNode[keyHost.PRODUCTNAME].InnerText = jobid;

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

        /// <summary>
        /// 6.159.	ValidateMaskPrepareReply        MES MessageSet :MES  Reply Mask ID OK/NG to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ValidateMaskPrepareReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                string unitNo = string.Empty; //目前 MES 回覆回來的訊息中，沒有包含 UnitID，故不Download給機台
                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string machineName = xmlDoc["MESSAGE"]["BODY"]["MACHINENAME"].InnerText;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqp.Data.NODENO));

                string timeId1 = string.Format("{0}_{1}_MaterialVerificationRequest", eqp.Data.NODENO, trxID);
                if (Timermanager.IsAliveTimer(timeId1))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                {
                    UserTimer timer = Timermanager.GetAliveTimer(timeId1);
                    unitNo = timer.State.ToString();
                    Timermanager.TerminateTimer(timeId1);
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES]=[{4}] MES_ValidateMaskPrepareReply not found BCS Send Message LINENAME=[{0}],MACHINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                     lineName, machineName, returnCode, returnMessage, trxID));
                    Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxID, eBitResult.ON, eReturnCode1.NG, eqp.Data.NODENO, unitNo });
                    return;
                }

                if (returnCode != "0")
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], ReturnCode =[{3}], ReturnMessage =[{4}].", ServerName, trxID, lineName, returnCode, returnMessage));
                    Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxID, eBitResult.ON, eReturnCode1.NG, eqp.Data.NODENO, unitNo });
                    return;
                }

                string maskName = xmlDoc["MESSAGE"]["BODY"]["MASKNAME"].InnerText;
                string valiResult = xmlDoc["MESSAGE"]["BODY"]["VALIRESULT"].InnerText;
                string command = "2";
                if (valiResult == "Y")
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_ValidateMaskPrepareReply OK LINENAME=[{1}],MACHINENAME=[{2}],MASKNAME=[{3}],VALIRESULT=[{4}],CODE=[{5}],MESSAGE=[{6}].",
                                         trxID, lineName, machineName, maskName, valiResult, returnCode, returnMessage));
                    //Send OK To Equipment
                    command = "1";//OK
                    //object keepData=Repository.Get("L9_MaskStateChanged");
                    //if (keepData!=null)
                    //{
                    //    Invoke(eServiceName.MESService, "MaskStateChanged", (object[])keepData);
                    //    string timerID = string.Format("{0}_{1}_MaskStateChanged",eqp.Data.NODENO, trxID);
                    //    if (this.Timermanager.IsAliveTimer(timerID))
                    //    {
                    //        Timermanager.TerminateTimer(timerID);
                    //    }
                    //    Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqp.Data.NODENO);
                    // }                   
                }
                else
                {
                    //Send NG to Equipment 
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_ValidateMaskPrepareReply NG LINENAME=[{1}],MACHINENAME=[{2}],MASKNAME=[{3}],VALIRESULT=[{4}],CODE=[{5}],MESSAGE=[{6}].",
                                         trxID, lineName, machineName, maskName, valiResult, returnCode, returnMessage));

                }

                Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxID, eBitResult.ON, (eReturnCode1)int.Parse(command), eqp.Data.NODENO, unitNo });

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.160.	ValidateMaskRequest     MES MessageSet : Mask Validate Request Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="eqpID">EQP HOST ID (Mask position)</param>
        /// <param name="masklist">Total Mask list</param>
        public void ValidateMaskRequest(string trxID, string lineName, string eqpID, IList masklist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));

                    #region Reply PLC
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpID);
                    if (eqp == null)
                    {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] ValidateMaskRequest  EQP is Null, LINENAME=[{1}],MACHINENAME=[{2}] MES IS OFFLINE BUT CAN NOT REPLY EQP [MaterialAuthorityRequestReply] BIT.",
                                trxID, lineName, eqpID));
                        return;
                    }
                    //MaterialAuthorityRequestReply(string eqpNo, eBitResult value, string trackKey, string rtnCode)
                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.CELLSpecialService, "MaterialAuthorityRequestReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, ((int)eReturnCode1.OK).ToString() });
                    else
                        Invoke(eServiceName.CELLSpecialService, "MaterialAuthorityRequestReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, ((int)eReturnCode1.NG).ToString() });
                    #endregion

                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateMaskRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;

                XmlNode maskListNode = bodyNode[keyHost.MASKLIST];
                XmlNode maskCloneNode = maskListNode[keyHost.MASK].Clone();
                maskListNode.RemoveAll();

                foreach (string maskid in masklist)
                {
                    XmlNode boxNode = maskCloneNode.Clone();
                    boxNode[keyHost.MASKNAME].InnerText = maskid.Trim();
                    maskListNode.AppendChild(boxNode);
                }

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

        /// <summary>
        /// 6.161.	ValidateMaskReply       MES MessageSet :MES Reply Mask Validate Data to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ValidateMaskReply(XmlDocument xmlDoc)
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

                //to Do
                string machinename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;

                XmlNode masklist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST];
                string mask, result = string.Empty;
                foreach (XmlNode masknode in masklist)
                {
                    mask = masknode[keyHost.MASKNAME].InnerText;
                    result = masknode[keyHost.VALIRESULT].InnerText;
                }

                eReturnCode1 plcrtncode = eReturnCode1.Unknown;
                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_ValidatePalletReply NG LINENAME={1},MACHINENAME={2},CODE={3},MESSAGE={4}.",
                                         trxID, lineName, machinename, returnCode, returnMessage));
                    plcrtncode = eReturnCode1.NG;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_ValidatePalletReply NG LINENAME={1},MACHINENAME={2},CODE={3},MESSAGE={4}.",
                                         trxID, lineName, machinename, returnCode, returnMessage));
                    plcrtncode = eReturnCode1.OK;

                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machinename);
                if (eqp == null) throw new Exception(string.Format("Can't find EQP ID=[{0}] in EquipmentEntity!", machinename));

                #region Reply PLC
                //MaterialAuthorityRequestReply(string eqpNo, eBitResult value, string trackKey, string rtnCode)
                object[] _data = new object[4]
                { 
                    eqp.Data.NODENO,
                    eBitResult.ON,
                    trxID,
                    ((int)plcrtncode).ToString() //1：OK  2：NG
                };
                Invoke(eServiceName.CELLSpecialService, "MaterialAuthorityRequestReply", _data);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.162.	ValidateMaskByCarrierRequest
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="portid"></param>
        /// <param name="maskCstid"></param>
        public void ValidateMaskByCarrierRequest_HVA2(string trxID, string lineName, string portid, string maskCstid)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] ValidateMaskByCarrierRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    //Watson Add 20150115 Add 回覆內容較多且複雜，OFFLINE只能回覆NG
                    #region Get and Remove Reply Key
                    string eqpNo = string.Empty;
                    string key = keyBoxReplyPLCKey.ValidateMaskByCarrierReply;
                    object eqp = Repository.Remove(key);
                    if (eqp == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply MES_ValidateMaskByCarrierReply_HVA2", trxID));
                        return;
                    }
                    eqpNo = eqp.ToString();
                    #endregion
                    string timerID = string.Format("{0}_{1}_{2}_PUMMaterialStateChanged", portid, maskCstid, trxID);
                    if (this.Timermanager.IsAliveTimer(timerID))
                        Timermanager.TerminateTimer(timerID);
                    Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply_PUM", new object[] { eqpNo, portid, string.Empty, new List<MaterialEntity>(), eBitResult.OFF, eReturnCode1.NG, trxID });

                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateMaskByCarrierRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PORTNAME].InnerText = portid;
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                bodyNode[keyHost.MASKCARRIERNAME].InnerText = maskCstid;

                #region MES ValidateMaskByCarrierReply Timeout For HVA2 PUM Material Status Change
                string timeoutName = string.Format("{0}_MES_ValidateMaskByCarrierReply_HVA2", maskCstid.Trim());
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateMaskT9Timeout), trxID);
                #endregion

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 6.162.	ValidateMaskByCarrierRequest
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        public void ValidateMaskByCarrierRequest(string trxID, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] ValidateMaskByCarrierRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateMaskByCarrierRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = port.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                bodyNode[keyHost.MASKCARRIERNAME].InnerText = port.File.CassetteID;

                #region MES ValidateMaskByCarrierReply Timeout For Mask Cleaner
                string timeoutName = string.Format("{0}_{1}_MES_ValidateMaskByCarrierReply", port.Data.LINEID, port.Data.PORTID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateMaskT9Timeout), trxID);
                #endregion

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void ValidateMaskT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  LINEID=[{1}], PortID=[{2}]  Validate Mask Reply MES Timeout.", trackKey, sArray[0],sArray[1]);

                //Case 1
                string timeoutName = string.Format("{0}_{1}_MES_ValidateMaskByCarrierReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0],sArray[1]);
                if (port != null)
                {
                    //TODO: 送到OPI
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
  
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.163.	ValidateMaskByCarrierReply
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_ValidateMaskByCarrierReply_HVA2(XmlDocument xmlDoc)
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

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                eReturnCode1 returncode;
                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_ValidateMaskByCarrierReply NG LINENAME=[{1}],LINERECIPENAME=[{2}],PORTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, portid, returnCode, returnMessage));
                    returncode = eReturnCode1.NG;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_ValidateMaskByCarrierReply OK LINENAME=[{1}],LINERECIPENAME=[{2}],PORTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                         trxID, lineName, lineRecipeName, portid, returnCode, returnMessage));
                    returncode = eReturnCode1.OK;

                }
                string result = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;
                string maskid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKCARRIERNAME].InnerText;

                XmlNode maskListNode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST];
                List<MaterialEntity> materialList = new List<MaterialEntity>();
                foreach (XmlNode masknode in maskListNode)
                {
                    MaterialEntity material = new MaterialEntity();
                    if (masknode[keyHost.MASKNAME] != null)
                    {
                        material.MaterialID = masknode[keyHost.MASKNAME].InnerText;
                        material.MaterialPosition = masknode[keyHost.MASKPOSITION].InnerText;
                        material.MaterialType = masknode[keyHost.MASKDETAILTYPE].InnerText;
                        material.UseCount = masknode[keyHost.MASKUSECOUNT].InnerText;
                        material.CellValidateResult = result;  
                        string masklimitusecount = masknode[keyHost.MASKLIMITUSECOUNT].InnerText;
                        materialList.Add(material);
                    }
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));

                string timerID = string.Format("{0}_{1}_{2}_PUMMaterialStateChanged", portid, maskid, trxID);
                if (this.Timermanager.IsAliveTimer(timerID))
                {
                    Timermanager.TerminateTimer(timerID);
                }

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.ValidateMaskByCarrierReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply MES_ValidateMaskByCarrierReply_HVA2", trxID));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion
                //(string eqpNo, string unitNo, string materialID,List<MaterialEntity> materialList, eBitResult value, eReturnCode1 returncode, string trackKey)
                Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply_PUM", new object[] { eqpNo, portid, maskid, materialList, eBitResult.ON, returncode, trxID });


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.163.	ValidateMaskByCarrierReply
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_ValidateMaskByCarrierReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                string maskCstid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKCARRIERNAME].InnerText;

                string timeoutName = string.Format("{0}_MES_ValidateMaskByCarrierReply_HVA2", maskCstid);
                if (_timerManager.GetAliveTimer(timeoutName) != null)
                {
                    _timerManager.TerminateTimer(timeoutName);
                    MES_ValidateMaskByCarrierReply_HVA2(xmlDoc);
                    return;
                }

                Handle_HostMaskData(xmlDoc);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Handle_HostMaskData(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);

            try
            {
                string err = string.Empty;

                // 檢查 Common Data
                if (!ValidateMaskCheckData_Common(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err))
                {
                    if (port == null)
                        throw new Exception(err);
                    else
                    {
                        // 刪掉T9 TIMEOUT, 錯誤的資料不用再計算
                        string timeoutName = string.Format("{0}_MES_ValidateMaskByCarrierReply", port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }

               XmlNode body = GetMESBodyNode(xmlDoc);

               if (body[keyHost.VALIRESULT].InnerText == "N")
               {
                   err = string.Format("MES REPLY VALIRESULT NG , CASSETTE DATA TRANSFER ERROR: CASSETTE HAVE BEEN CANCEL. LINE_NAME=[{0}], PORT_NAME=[{1}], CSTID=[{2}).",
                       lineName, port.Data.PORTID, port.File.CassetteID);
                   NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                   cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                   cst.ReasonText = MES_ReasonText.MES_Validate_Reply_NG + "_" + err;

                   MaskProcessEndAbort(trxID, port, new List<Job>(),cst.ReasonCode, cst.ReasonText);
                   return;
               }

                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    err = string.Format("Cassette Data Transfer Error: Cassette have been Cancel. Line Name={0}, Port Name={1} MASKCARRIERNAME={2}.",
                        lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_Cassette_Have_Been_Cancel;
                    return;
                }

                #region 檢查資料 mask不需要
                //bool result = ValidateMaskCheckData(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err);

                //if (!result)
                //{
                //    string timeoutName = string.Format("{0}_MES_ValidateMaskByCarrierReply", port.Data.PORTID);
                //    if (_timerManager.IsAliveTimer(timeoutName))
                //    {
                //        _timerManager.TerminateTimer(timeoutName);
                //    }
                //    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                //}
                #endregion

                // 先將MES資料放入Box 
                MESDataIntoMaskObject(body, ref cst);
                //OPI使用
                lock (cst) cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;

                if (line.File.HostMode == eHostMode.LOCAL)
                {
                    // 刪掉T9 TIMEOUT, 錯誤的資料不用再計算
                    string timeoutName = string.Format("{0}_MES_ValidateMaskByCarrierReply", port.Data.PORTID);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    // 通知OPI 可供OP修改資料 
                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                    ObjectManager.PortManager.EnqueueSave(port.File);
                    
                    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { trxID, port });

                    NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                     string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] Local Mode, Check MES Data Complete, Wait UI Edit Data.",
                     lineName, trxID));
                    return;
                }

                Decode_ValidateMaskData(xmlDoc, line, eqp, port, cst);
            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, ex.Message });
                Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Cassette Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 解析資料到Job Data
        /// Local Mode 時, 組完資料直接Call這個Method
        /// </summary>
        public void Decode_ValidateMaskData(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst)
        {
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);
            try
            {
                string err = string.Empty;

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                //XmlNodeList lotNodeList = body[keyHost.MASKLIST].ChildNodes;
                IList<Job> mesJobs = new List<Job>();
                //foreach (XmlNode n in lotNodeList)
                //{
                    #region Create Slot Data
                    XmlNodeList productList = body[keyHost.MASKLIST].ChildNodes;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.MASKPOSITION].InnerText, out slotNo);
                        Job job = new Job(int.Parse(port.File.CassetteSequenceNo), slotNo);
                        job.EQPJobID = productList[i][keyHost.MASKNAME].InnerText.Trim();
                        //job.GlassChipMaskCutID = productList[i][keyHost.MASKNAME].InnerText.Trim();
                        job.SourcePortID = port.Data.PORTID;
                        job.TargetPortID = "0";
                        job.FromCstID = port.File.CassetteID;
                        job.ToCstID = string.Empty;
                        job.FromSlotNo = slotNo.ToString();
                        job.ToSlotNo = "0";
                        job.CurrentSlotNo = job.FromSlotNo; // add by bruce 20160412 for T2 Issue
                        job.CurrentEQPNo = eqp.Data.NODENO;

                        job.LineRecipeName = body[keyHost.LINERECIPENAME].InnerText.Trim();
                        job.CIMMode = eBitResult.ON;
                        job.FirstRunFlag = "0";
                        //Jun Modify 20150507 CBUAM不寫GlassID Item，寫到MaskID Item
                        job.CellSpecial.MASKID = productList[i][keyHost.MASKNAME].InnerText.Trim();

                        #region 把MES Download的PPID重新組合後記錄
                        foreach (Equipment eq in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                        {
                            string eqpno = eq.Data.NODENO;

                            job.PPID += new string('0', eq.Data.RECIPELEN);
                        }
                        job.MES_PPID = string.Empty;
                        string recipe_Assembly_log = string.Format("Adjust PPID OK, MES PPID =[{0}], EQP PPID(Job Data) =[{1}] ", job.MES_PPID, job.PPID);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                        #endregion

                        #region Job Special Data MASK USE COUNT 利用現成的UVMASKUSECOUNT來上報資料
                        job.CellSpecial.UVMaskUseCount = productList[i][keyHost.MASKUSECOUNT].InnerText.Trim();
                        #endregion

                        #region 將MES Data 存到Job Information
                        MESCstDataIntoMaskObject(body, ref job, ref cst);
                        MESProductDataIntoMaskObject(productList[i], ref job);
                        #endregion
                        ObjectManager.QtimeManager.ValidateQTimeSetting(job);

                        mesJobs.Add(job);
                    }
                    #endregion

                //}

                //將資料放入JobManager
                if (mesJobs.Count() > 0)
                {
                    ObjectManager.JobManager.AddJobs(mesJobs);
                    ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID,eqp.Data.NODENO, port.Data.PORTNO ,"Create", trxID); //20141225 Tom  Add Job History
                }

                // Download to PLC
                Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs,trxID});
            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, ex.Message });
                Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                    new object[] { trxID, lineName, "MES ValidateCassetteReply - NG", 
                        "Cassette Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private bool ValidateMaskCheckData_Common(XmlDocument xmlDoc, ref Line line, ref Equipment eqp,
            ref Port port, ref Cassette cst, string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);

                #region Check Port Object
                string portName = body[keyHost.PORTNAME].InnerText;
                port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portName);
                if (port == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Port Object, Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    return false;
                }
                #endregion

                string portid = body[keyHost.PORTNAME].InnerText;
                string cstid = body[keyHost.MASKCARRIERNAME].InnerText;

                #region Check Cassette
                cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    return false;
                }
                #endregion

                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Line Object, Line Name=[{0}].", lineName);
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = ERR_CST_MAP.INVALID_LINE_DATA + "_" + errMsg; 
                    }
                    return false;
                }
                #endregion
                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                #region Check Return Code
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    if (!returnCode.Equals("0"))
                    {
                        errMsg = string.Format("[LINENAME={0}] [BCS <- MES] [{1}] ValidateMaskByCarrierReply NG, PORTID=[{2}], CSTID=[{3}], RETURNCODE=[{4}], RETURNMESSAGE=[{5}).",
                            lineName, trxID, portid, cstid, returnCode, returnMessage);

                        lock (cst)
                        {
                            cst.ReasonText = returnMessage;
                            if (port.File.Type == ePortType.UnloadingPort)
                            {
                                cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Validation_NG_From_MES;
                            }
                            else
                            {
                                cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Validation_NG_From_MES;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Reply Sucess. ReturnCode=[{0}]", returnCode));
                    }
                }
                #endregion

                #region Check Equipment Object
                eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                if (eqp == null)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Cannot found EQP Object, Line Name=[{0}], Equipment No=[{1}].", lineName, port.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_EQUIPMENT_DATA + "_" + errMsg; 
                    }
                    return false;
                }
                #endregion

                #region Check CIM Mode
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: CIM Mode Off. EQPID=[{0}]", eqp.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.CIM_MODE_OFF;
                    }
                    return false;
                }
                #endregion

                #region Check Port Cassette
                if (port.File.Type == ePortType.Unknown)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.Type.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + "_" + errMsg; 
                    }
                    return false;
                }

                //Mask Cleaner 不會上報Port Mode
                //if (port.File.Mode == ePortMode.Unknown)
                //{
                //    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Mode=[{3}] is Invalid.",
                //        lineName, portName, port.File.CassetteID, port.File.Mode.ToString());
                //    lock (cst)
                //    {
                //        cst.ReasonCode = reasonCode;
                //        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS;
                //    }
                //    return false;
                //}

                if (port.File.EnableMode != ePortEnableMode.Enabled)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Enable Mode=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.EnableMode.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + "_" + errMsg; 
                    }
                    return false;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Status=[{3}] is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.Status.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + "_" + errMsg; 
                    }
                    return false;
                }

                if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Different Cassette SeqNo. Line Name=[{0}], Cassette SeqNo=[{1}], Port_Cassette SeqNo=[{2}].",
                        lineName, port.File.CassetteSequenceNo, portName);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.DIFFERENT_CST_SEQNO + "_" + errMsg; 
                    }
                    return false;
                }

                if (!string.IsNullOrEmpty(cst.CassetteID.Trim()))
                {
                    if (!cst.CassetteID.Trim().Equals(cstid))
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Different Cassette ID. Line Name=[{0}], BC_CSTID=[{1}], MES_CSTID=[{2}].",
                            lineName, cst.CassetteID, cstid);
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.INVALID_CSTID + "_" + errMsg; 
                        }
                        return false;
                    }
                }
                else
                {
                    lock (cst) cst.CassetteID = cstid;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], PORTID=[{1}], CSTID=[{2}], CSTSTATUS=[{3}], Cassette Status is not available.",
                        lineName, port.Data.PORTID, cst.CassetteID, port.File.CassetteStatus.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.UNEXPECTED_MES_MESSAGE + "_" + errMsg;
                    }
                    return false;
                }
                #endregion

                #region Check Slot Mapping
                string mesMapData = string.Empty;
                int productCount = 0;
                if (port.Data.MAPPINGENABLE == "TRUE")
                {
                    XmlNodeList productList = body[keyHost.MASKLIST].ChildNodes;
                    productCount += productList.Count;

                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.MASKPOSITION].InnerText, out slotNo);

                        if (slotNo <= 0)
                        {
                            errMsg = "Cassette Data Transfer Error: SlotNo Parsing Error";
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg;
                            }
                            return false;
                        }

                        if (!port.File.ArrayJobExistenceSlot[slotNo - 1])
                        {
                            errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch Port Slot=[{0}] EQP JobExistenceSlot status=[{1}] No GLASS. MES Download Data has Glass. ", slotNo, port.File.ArrayJobExistenceSlot[slotNo - 1]);
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg; ;
                            }
                            return false;
                        }
                    }
                    // 檢查MES所有玻璃資料的數量是否跟Port上的數量相同
                    if (int.Parse(port.File.JobCountInCassette) != productCount)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}],MES Download JobCount=[{1}]", port.File.JobCountInCassette, productCount);
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg; 
                        }
                        return false;
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = MES_ReasonText.Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error;
                    }
                }
                return false;
            }
        }

        private bool ValidateMaskCheckData(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst,
            string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                XmlNode body = GetMESBodyNode(xmlDoc);

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                int productQuantity = body[keyHost.MASKLIST].ChildNodes.Count;

                if ((port.File.Type == ePortType.LoadingPort && productQuantity.Equals(0)) ||
                    (port.File.Type == ePortType.UnloadingPort && port.File.PartialFullFlag != eParitalFull.PartialFull && productQuantity > 0))
                {
                    errMsg = "Mask Data Transfer Error: Mask Cassette SlotMap Mismatch";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + "_" + errMsg; 
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// ValidateBoxReply 重要資料仍填入 Cassette.MesCstBody.LOTLIST[0] 
        /// </summary>
        /// <param name="bodyNode">ValidateBoxReply Body層</param>
        /// <param name="cst">填入Cassette Entity</param>
        private void MESDataIntoMaskObject(XmlNode bodyNode, ref Cassette cst)
        {
            try
            {
                cst.MES_CstData.LINENAME = bodyNode[keyHost.LINENAME].InnerText;
                cst.MES_CstData.PORTNAME = bodyNode[keyHost.PORTNAME].InnerText;
                cst.MES_CstData.CARRIERNAME = bodyNode[keyHost.MASKCARRIERNAME].InnerText;
                cst.MES_CstData.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;
                cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;

                    #region 在Product Data 加入 Product List
                    XmlNodeList productList = bodyNode[keyHost.MASKLIST].ChildNodes;
                    foreach (XmlNode n in productList)
                    {
                        PRODUCTc product = new PRODUCTc();
                        product.POSITION = n[keyHost.MASKPOSITION].InnerText;
                        product.PRODUCTNAME = n[keyHost.MASKNAME].InnerText;
                    }
                    #endregion
                    ObjectManager.CassetteManager.EnqueueSave(cst);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MESCstDataIntoMaskObject(XmlNode bodyNode,  ref Job job, ref Cassette cst)
        {
            try
            {
                if (string.IsNullOrEmpty(bodyNode[keyHost.OPI_LINERECIPENAME].InnerText.Trim()))
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;
                else
                    lock (cst) cst.LineRecipeName = bodyNode[keyHost.OPI_LINERECIPENAME].InnerText;

                ObjectManager.JobManager.ConvertXMLToObject(job.MesCstBody, bodyNode);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 將MES Product Data塞入Job Data裡的MesProduct
        /// </summary>
        private void MESProductDataIntoMaskObject(XmlNode productNode, ref Job job)
        {
            try
            {
                job.MesProduct.POSITION = productNode[keyHost.MASKPOSITION].InnerText;
                job.MesProduct.PRODUCTNAME = productNode[keyHost.MASKNAME].InnerText;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}

