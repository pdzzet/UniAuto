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
        /// 6.4.	AssembleComplete
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="eQPID"></param>
        /// <param name="tFTPanelID"></param>
        /// <param name="tFTCSTID"></param>
        /// <param name="cFPanelID"></param>
        /// <param name="cFCSTID"></param>
        public void AssembleComplete(string trxID, string lineName, string eQPID, string tFTPanelID, string tFTCSTID,string tFTOX, string cFPanelID, string cFCSTID,string cFOX)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] AssembleComplete Send But OFF LINE LINENAME=[{1}].",trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("AssembleComplete") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                XmlNode mainNode = bodyNode[keyHost.MAINPRODUCT];
                XmlNode pairNode = bodyNode[keyHost.PAIRPRODUCT];
                

                mainNode[keyHost.CARRIERNAME].InnerText = tFTCSTID;
                mainNode[keyHost.PRODUCTNAME].InnerText = tFTPanelID;
                mainNode[keyHost.SUBPRODUCTGRADES].InnerText = tFTOX;//20170616 by huangjiayin

                pairNode[keyHost.CARRIERNAME].InnerText = cFCSTID;
                pairNode[keyHost.PRODUCTNAME].InnerText = cFPanelID;
                pairNode[keyHost.SUBPRODUCTGRADES].InnerText = cFOX;//20170616 by huangjiayin

                bodyNode.AppendChild(mainNode);
                bodyNode.AppendChild(pairNode);

                SendToMES(xml_doc);

                #region MES_AssembleCompleteReply Timeout
                string timeoutName = string.Format("{0}_MES_AssembleCompleteReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(AssembleCompleteReply_Timeout), trxID);
                #endregion



                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] AssembleComplete OK LINENAME=[{1}],TFTOX=[{2}],CFOX=[{3}].",
                         trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID),tFTOX,cFOX));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>
        /// 6.138.	AssembleCompleteReply        MES MessagetSet :AssembleCompleteReply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_AssembleCompleteReply(XmlDocument xmlDoc)
        {
            try
            {
                XmlNode bodyNode = xmlDoc[keyHost.MESSAGE][keyHost.BODY];

                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                string tFTJobID = string.Empty;
                string tFTOX = string.Empty;
                string eqpid=string.Empty;

                Job assembledJob;           
                Equipment eqp;

                //kill timer
                string timeoutName = string.Format("{0}_MES_AssembleCompleteReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_AssembleCompleteReply NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                }
                else
                {
                    tFTJobID = bodyNode[keyHost.MAINPRODUCT][keyHost.PRODUCTNAME].InnerText.Trim();
                    tFTOX = bodyNode[keyHost.MAINPRODUCT][keyHost.SUBPRODUCTGRADES].InnerText.Trim();
                    eqpid=bodyNode[keyHost.MACHINENAME].InnerText.Trim();

                    assembledJob = ObjectManager.JobManager.GetJob(tFTJobID);
                    eqp=ObjectManager.EquipmentManager.GetEQPByID(eqpid);
                    

                    if (string.IsNullOrEmpty(tFTJobID)) throw new Exception(string.Format("assembledJob by MES reply JobID is empty"));
                    if (string.IsNullOrEmpty(tFTOX)) throw new Exception(string.Format("assembledJob by MES reply JobID=[{0}],OXR is empty", tFTJobID));
                    if (assembledJob == null) throw new Exception(string.Format("Can not find assembledJob by MES reply JobID=[{0}]", tFTJobID));
                    if (eqp == null) throw new Exception(string.Format("Can not find Machine by MES reply MachineName=[{0}]", eqpid));
                    if (assembledJob.OXRInformation.Length != tFTOX.Length) throw new Exception(string.Format("OXR Invalid Length error! BC OXR=[{2}][{0}], BUT MES OXR=[{3}][{1}]", assembledJob.OXRInformation, tFTOX, assembledJob.OXRInformation.Length,tFTOX.Length));


                    //Delay 0.5sec...
                    Thread.Sleep(500);
                    assembledJob.OXRInformation = tFTOX;
                    ObjectManager.JobManager.EnqueueSave(assembledJob);
                    ObjectManager.JobManager.RecordJobHistory(assembledJob, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.Assembly + "_OXUpdate",trxID);


                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_AssembleCompleteReply OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}],OXRInfo Assembled=[{4}].",
                                        trxID, lineName, returnCode, returnMessage,tFTOX));

                }


            
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }



        private void AssembleCompleteReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  AssembleComplete MES Reply Timeout.", trackKey);
                string timeoutName = string.Format("{0}_MES_AssembleCompleteReply", trackKey);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);



                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }




        /// <summary>
        ///  6.6.	BoxLabelInformationRequest      MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxID"></param>
        public void BoxLabelInformationRequest(string trxID, string lineName, string eqpNo, string eqpID, string boxID, string boxType, List<Cassette> subBoxList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BOXLABELINFOMATIONREQUEST SEND MES BUT OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    //DenseBoxLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string denseBoxID, string modelName, string modelVersion, string caseID, int qty, string weekCode)
                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", new object[] { eqpNo, eBitResult.ON, trxID, eReturnCode1.OK, boxID, string.Empty, string.Empty, 0 });
                    else
                        Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", new object[] { eqpNo, eBitResult.ON, trxID, eReturnCode1.NG, boxID, string.Empty, string.Empty, 0 });
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxLabelInformationRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.BOXNAME].InnerText = boxID;

                //Add by marine for T3 MES 2015/8/19
                bodyNode[keyHost.BOXTYPE].InnerText = boxType;
                XmlNode subBoxListNode = bodyNode[keyHost.SUBBOXLIST];

                //Add by marine for T3 MES 2015/9/11
                XmlNode subBox = subBoxListNode[keyHost.SUBBOX];
                subBoxListNode.RemoveAll();
                XmlNode subBoxClone = subBox.Clone();

                foreach (Cassette cst in subBoxList)
                {
                    subBoxClone[keyHost.SUBBOXNAME].InnerText = cst.SubBoxID;
                    subBoxClone[keyHost.BOXTYPE].InnerText = cst.eBoxType.ToString();
                }
                subBoxListNode.AppendChild(subBoxClone);
                //XmlNode subBoxList = bodyNode[keyHost.SUBBOXLIST];
                //subBoxList[keyHost.SUBBOXNAME].InnerText = subBoxID;

                SendToMES(xml_doc);

                #region MES BoxLabelInformationReply Timeout
                string timeoutName = string.Format("{0}_MES_BoxLabelInformationReply", eqpID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(BoxLabelInformationReply_Timeout), trxID);
                #endregion


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxLabelInformationRequest OK LINENAME=[{1}], BOXNAME=[{2}].",
                         trxID, lineName, boxID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.7.	BoxLabelInformationReply        MES MessagetSet : BOX Label Information Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_BoxLabelInformationReply(XmlDocument xmlDoc)
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
                //string boxname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                //string modelname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME].InnerText;
                //string modelver = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION].InnerText;
                string eqpid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                //Add for T3 MES by marine 2015/8/19
                //string quantity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY].InnerText;
                //string bomVersion = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION].InnerText;
                //MES T3 SPEC 1.34 結構大改 sy add 20160120 //MES_reply NG format不全 防呆 sy edit 20160516
                string shipIdName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.NAME].InnerText;
                string shipIdValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.VALUE].InnerText;
                string quantityName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY][keyHost.NAME].InnerText;
                string quantityValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY][keyHost.VALUE].InnerText;
                string bomVersionName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.NAME].InnerText;
                string bomVersionValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.VALUE].InnerText;
                string modelName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.NAME].InnerText;
                string modelValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.VALUE].InnerText;
                string modelVersionName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.NAME].InnerText;
                string modelVersionValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.VALUE].InnerText;
                string environmenFlagName = string.Empty;
                string environmenFlagValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ENVIRONMENTFLAG] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ENVIRONMENTFLAG].InnerText;
                string partIdName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PARTID][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PARTID][keyHost.NAME].InnerText;
                string partIdValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PARTID][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PARTID][keyHost.VALUE].InnerText;
                string carrierNameName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.NAME].InnerText;
                string carrierNameValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.VALUE].InnerText;
                string countryName = string.Empty;
                string countryValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.COUNTRY] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.COUNTRY].InnerText;
                string noteName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.NAME].InnerText;
                string noteValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.VALUE].InnerText;
                string wtName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.NAME].InnerText;
                string wtValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.VALUE].InnerText;
                eReturnCode1 plcreturncode;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpid);
                if (eqp == null)                    throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqpid));                

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_BoxLabelInformationReply NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                    plcreturncode = eReturnCode1.NG;

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MES_BoxLabelInformationReply OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?
                    plcreturncode = eReturnCode1.OK; //OK

                }

                string timeoutName = string.Format("{0}_MES_BoxLabelInformationReply", eqpid);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                string timeoutName1 = string.Format("{0}_MES_PaperBoxLabelInformationReply", eqpid);
                if (_timerManager.IsAliveTimer(timeoutName1))
                {
                    _timerManager.TerminateTimer(timeoutName1);
                }

                //寫入PLC
                //(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string denseBoxID, 
                //string modelName, string modelVersion, string caseID, int qty, string weekCode)
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK || line.Data.LINETYPE == eLineType.CELL.CCPCK || line.Data.LINETYPE == eLineType.CELL.CCOVP)//sy modify 20160926
                {
                    #region [PaperBoxLabelInformationRequestReply]
                    object[] _data = new object[26]
                { 
                    eqp.Data.NODENO ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trxID,                           /*2 trackKey */
                    plcreturncode ,         /*3 returnCode*/
                    shipIdName,                    /*4  SHIPID Name */
                    shipIdValue,              /*5 SHIPID Value */
                    quantityName,                    /*6  QUANTITY */
                    quantityValue,              /*7 QUANTITY*/
                    bomVersionName,                    /*8  BOMVERSION */
                    bomVersionValue,              /*9 BOMVERSION*/
                    modelName,                    /*10  MODELNAME --*/
                    modelValue,              /*11 MODELNAME*/
                    modelVersionName,                    /*12  MODELVERSION */
                    modelVersionValue,              /*13 MODELVERSION*/
                    environmenFlagName,                    /*14  ENVIRONMENTFLAG */
                    environmenFlagValue,              /*15 ENVIRONMENTFLAG*/
                    partIdName,                    /*16 PARTID */
                    partIdValue,              /*17 PARTID*/
                    carrierNameName,                    /*18  CARRIERNAME */
                    carrierNameValue,              /*19 CARRIERNAME*/
                    noteName,                    /*20  NOTE */
                    noteValue,              /*21 NOTE*/
                    countryName,                    /*22  COUNTRY */
                    countryValue,              /*23 COUNTRY*/
                    wtName,                    /*24  WT */
                    wtValue              /*25 WT*/
                };
                    Invoke(eServiceName.PaperBoxService, "PaperBoxLabelInformationRequestReply", _data);
                    #endregion
                    #region [PaperBoxLabelInformationRequestReply] old mark
                    //    object[] _data = new object[8]
                    //{ 
                    //    eqp.Data.NODENO ,  /*0 eqpNo*/
                    //    eBitResult.ON ,         /*1 eBitResult*/
                    //    trxID,                           /*2 trackKey */
                    //    plcreturncode ,         /*3 returnCode*/
                    //    boxname,                    /*4 paperBoxID */
                    //    modelname,              /*5 modelName*/
                    //    modelver ,                  /*6 modelVersion*/
                    //    "1"                                /*7 cartonQuantity*/
                    //};
                    //    Invoke(eServiceName.PaperBoxService, "PaperBoxLabelInformationRequestReply", _data);
                    #endregion
                }
                else if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                {
                    #region [LabelInformationforBoxRequestReply]
                    object[] _data = new object[26]
                { 
                    eqp.Data.NODENO ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trxID,                           /*2 trackKey */
                    plcreturncode ,         /*3 returnCode*/
                    shipIdName,                    /*4  SHIPID Name */
                    shipIdValue,              /*5 SHIPID Value */
                    quantityName,                    /*6  QUANTITY */
                    quantityValue,              /*7 QUANTITY*/
                    bomVersionName,                    /*8  BOMVERSION */
                    bomVersionValue,              /*9 BOMVERSION*/
                    modelName,                    /*10  MODELNAME --*/
                    modelValue,              /*11 MODELNAME*/
                    modelVersionName,                    /*12  MODELVERSION */
                    modelVersionValue,              /*13 MODELVERSION*/
                    environmenFlagName,                    /*14  ENVIRONMENTFLAG */
                    environmenFlagValue,              /*15 ENVIRONMENTFLAG*/
                    partIdName,                    /*16 PARTID */
                    partIdValue,              /*17 PARTID*/
                    carrierNameName,                    /*18  CARRIERNAME */
                    carrierNameValue,              /*19 CARRIERNAME*/
                    noteName,                    /*20  NOTE */
                    noteValue,              /*21 NOTE*/
                    countryName,                    /*22  COUNTRY */
                    countryValue,              /*23 COUNTRY*/
                    wtName,                    /*24  WT */
                    wtValue              /*25 WT*/
                };
                    //(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string paperBoxID,
                    //    string modelName ,string modelVersion ,string cartonQuantity)
                    Invoke(eServiceName.DenseBoxService, "LabelInformationforBoxRequestReply", _data);
                    #endregion
                }
                else
                {
                    //T3 沒用到 結構大改 Mark
                    #region [DenseBoxLabelInformationRequestReply]
                    //    object[] _data = new object[8]
                    //{ 
                    //    eqp.Data.NODENO ,  /*0 eqpNo*/
                    //    eBitResult.ON ,         /*1 eBitResult*/
                    //    trxID,                           /*2 trackKey */
                    //    plcreturncode ,         /*3 returnCode*/
                    //    boxname,                    /*4 denseBoxID */
                    //    modelname,              /*5 modelName*/
                    //    modelver ,                  /*6 modelVersion*/
                    //    //casid,                          /*7 caseID */
                    //    1 ,                                 /*8 qty*/
                    //    //weekcode,                  /*9 weekCode */
                    //    //bomVersion                /*10 bomVersion*/
                    //};
                    //    Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", _data);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void BoxLabelInformationReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  MACHINENAME={1}  Box Label Information MES Reply Timeout.", trackKey, sArray[0]);
                string timeoutName = string.Format("{0}_MES_BoxLabelInformationReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(sArray[0]);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", sArray[0]));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                {
                    #region [LabelInformationforBoxRequestReply]
                    object[] _data = new object[26]
                { 
                    eqp.Data.NODENO ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trackKey,                           /*2 trackKey */
                    eReturnCode1.NG ,         /*3 returnCode*/
                    "",                    /*4  SHIPID Name */
                    "",              /*5 SHIPID Value */
                    "",                    /*6  QUANTITY */
                    "",              /*7 QUANTITY*/
                    "",                    /*8  BOMVERSION */
                    "",              /*9 BOMVERSION*/
                    "",                    /*10  MODELNAME --*/
                    "",              /*11 MODELNAME*/
                    "",                    /*12  MODELVERSION */
                    "",              /*13 MODELVERSION*/
                    "",                    /*14  ENVIRONMENTFLAG */
                    "",              /*15 ENVIRONMENTFLAG*/
                    "",                    /*16 PARTID */
                    "",              /*17 PARTID*/
                    "",                    /*18  CARRIERNAME */
                    "",              /*19 CARRIERNAME*/
                    "",                    /*20  NOTE */
                    "",              /*21 NOTE*/
                    "",                    /*22  COUNTRY */
                    "",              /*23 COUNTRY*/
                    "",                    /*24  WT */
                    ""              /*25 WT*/
                };
                    Invoke(eServiceName.DenseBoxService, "LabelInformationforBoxRequestReply", _data);
                    #endregion
                }
                else
                {
                    object[] _data = new object[11]
                { 
                    eqp.Data.NODENO ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trackKey,                           /*2 trackKey */
                    eReturnCode1.NG ,         /*3 returnCode*/
                    string.Empty,                    /*4 denseBoxID */
                    string.Empty,              /*5 modelName*/
                    string.Empty ,                  /*6 modelVersion*/
                    string.Empty,                          /*7 caseID */
                    0 ,                                 /*8 qty*/
                    string.Empty,                  /*9 weekCode */
                    string.Empty                   /*10 bomVersion*/
                };
                    Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", _data);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.8.	BoxProcessCanceled      MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="bOXIDList"></param>
        /// <param name="portID"></param>
        /// <param name="reasonCode"></param>
        /// <param name="reasonText"></param>
        public void BoxProcessCanceled(string trxID, string lineID, string portID, string boxID, string reasonCode, string reasonText)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.REASONTEXT].InnerText = reasonText;
                //只有一個Box
                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Add by marine for T3 MES 2015/9/11
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                XmlNode boxClone = box.Clone();
                boxClone[keyHost.BOXNAME].InnerText = boxID;
                boxListNode.AppendChild(boxClone);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled OK LINENAME=[{1}], PORTNAME=[{2}], REASONCODE=[{3}], REASONTEXT=[{4}].",
                         trxID, line.Data.LINEID, portID, reasonCode, reasonText));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void BoxProcessCanceled_PPK(string trxID, string lineID, string portID, List<Cassette> boxList, string reasonCode, string reasonText)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.REASONTEXT].InnerText = reasonText;

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodes = boxListNode[keyHost.BOX];
                XmlNode boxNode = boxNodes[keyHost.BOXNAME];
                boxNodes.RemoveAll();
                foreach (Cassette cst in boxList)
                {
                    XmlNode boxClone = boxNode.Clone();
                    boxClone.InnerText = cst.CassetteID;
                    boxNodes.AppendChild(boxClone);
                }
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled OK LINENAME=[{1}], PORTNAME=[{2}], REASONCODE=[{3}], REASONTEXT=[{4}].",
                         trxID, line.Data.LINEID, portID, reasonCode, reasonText));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void BoxProcessCanceled_QPP(string trxID, string lineID, string portID, List<string> boxList, string reasonCode, string reasonText)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.REASONTEXT].InnerText = reasonText;

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify for T3 Spec 1.21 shihyang
                XmlNode boxNodes = boxListNode[keyHost.BOX];
                //XmlNode boxNode = boxNodes[keyHost.BOXNAME];
                boxListNode.RemoveAll();
                foreach (string box in boxList)
                {
                    XmlNode boxClone = boxNodes.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = box;
                    boxListNode.AppendChild(boxClone);
                }
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessCanceled OK LINENAME=[{1}], PORTNAME=[{2}], REASONCODE=[{3}], REASONTEXT=[{4}].",
                         trxID, line.Data.LINEID, portID, reasonCode, reasonText));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void BoxProcessAborted(string trxID, Port port, string reasonCode, string reasonTxt)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID); //可能會有兩條以上的Line由BC 控管
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessAborted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                if (line.Data.LINETYPE == eLineType.CELL.CCPCK)
                {
                    bodyNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                    bodyNode[keyHost.CARRIERNAME].InnerText = "";
                }
                else
                {
                    bodyNode[keyHost.BOXNAME].InnerText = cst.BoxName.Trim();//Modify by sy for T3 MES 2015/11/23
                    if (cst.BoxName.Trim() == string.Empty && port.File.Type == ePortType.UnloadingPort)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("BoxName is Empty ,it forget to reqest MES LotIDRequestReport"));
                    }
                    bodyNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID.Trim();
                }
                bodyNode[keyHost.REASONCODE].InnerText = reasonCode;
                bodyNode[keyHost.REASONTEXT].InnerText = reasonTxt;

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

        public void BoxLineOutReport(string trxID, string lineID, string portID, string boxID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxLineOutReport") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.BOXNAME].InnerText = boxID;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxLineOutReport OK LINENAME=[{1}], PORTNAME =[{2}], OXID=[{3}].",
                         trxID, line.Data.LINEID, portID, boxID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.9.	BoxLineOutRequest   MES MessageSet : 	BC reports this event instead of ‘BoxProcessEnd’ when Box is moved out of  Line for Sampling.
        /// Add by marine 2015/8/12 for T3 MES
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="portID"></param>
        /// <param name="cst"></param>
        /// <param name="jobList"></param>
        public void BoxLineOutRequest(string trxID, string lineID, string portID, Cassette cst, IList<Job> jobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxLineOutRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";

                if (port == null)
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                else
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                boxNodeClone.RemoveChild(jobListNode);
                boxListNode.RemoveAll();
                jobListNode.RemoveAll();

                XmlNode boxNode = boxNodeClone.Clone();
                boxNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count.ToString();

                foreach (Job job in jobList)
                {
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                    if (job.MesCstBody.LOTLIST.Count != 0)
                    {
                        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                    }

                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.PRODUCTNAME].InnerText = job.EQPJobID;
                    jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                    #region
                    XmlNode abromalListNode = jobNode[keyHost.ABNORMALCODELIST];
                    XmlNode codeNode = abromalListNode[keyHost.CODE];
                    abromalListNode.RemoveAll();
                    #endregion
                    //foreach (CODEc code in job.MesProduct.ABNORMALCODELIST)
                    //{
                    //    XmlNode abnorNode = codeNode.Clone();
                    //    abnorNode[keyHost.ABNORMALSEQ].InnerText = code.ABNORMALSEQ;
                    //    abnorNode[keyHost.ABNORMALCODE].InnerText = code.ABNORMALCODE;
                    //    jobNode.AppendChild(abnorNode);
                    //}

                    jobNode[keyHost.BOXULDFLAG].InnerText = job.CellSpecial.BOXULDFLAG;
                    if (port == null)
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = "N";
                    else
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";
                    jobNode[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;

                    jobListNode.AppendChild(jobNode);
                }
                boxNode.AppendChild(jobListNode);
                boxListNode.AppendChild(boxNode);

                bodyNode.AppendChild(boxListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxLineOutRequest OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, portID, cst.LineRecipeName, cst.MES_CstData.LINERECIPENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.10.	BoxLineOutReply     MES MessageSet : MES acknowledge the Line out of the Box
        /// Add by marine 2015/8/12 for T3 MES
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_BoxLineOutReply(XmlDocument xmlDoc)
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
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                string desc = string.Empty;

                XmlNode boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                List<string> boxlist = new List<string>();
                foreach (XmlNode boxnode in boxlistnode)
                {
                    if (boxnode != null)
                    {
                        string boxname = boxnode.InnerText;
                        boxlist.Add(boxname);
                    }
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxLineOutReply  NG LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxID, lineName, boxqty, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxLineOutReply  OK LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, boxqty, returnCode, returnMessage));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 6.11.	BoxIdCreateRequest   MES MessageSet : BC requests first panel into a Tray. MES should create a BoxId based on panel information
        /// Add by marine 2015/8/13 for T3 MES
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="productName"></param>
        public void BoxIdCreateRequest(string trxID, string lineName, string productName, string boxType)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxIdCreateRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                //Add by marine for T3 MES 2015/9/16
                bodyNode[keyHost.BOXTYPE].InnerText = boxType;//機台不會知道 都給空值//PPK 用OUTBOX
                bodyNode[keyHost.RUNMODE].InnerText = line.File.LineOperMode;//Add by sy for T3 MES 1.44 20160614
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxIdCreateRequest OK LINENAME=[{1}], productName=[{2}].",
                         trxID, lineName, productName));
                #region MES BoxIdCreateRequest Timeout
                string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(LotIdCreateRequestT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.12.	BoxIdCreateReply    MES MessageSet : MES replies Runcard information after a Box creation
        /// Add by marine 2015/8/13 for T3 MES
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_BoxIdCreateReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));

                eReturnCode1 rtcode = returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG;
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                    return;
                }
                #endregion
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqpID));
                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_BoxIdCreateRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion

                string lotName = string.Empty; string productName = string.Empty; string capacity = "0"; string boxType = string.Empty; string runMode = string.Empty;

                #region [Reply NG]
                if (rtcode == eReturnCode1.NG)//MES reply NG 時 很多格式都不會給。sy edit 20160516
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxIdCreateReply NG LINENAME=[{1}],LOTNAME=[{2}],PRODUCTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                        trxID, lineName, lotName, productName, returnCode, returnMessage));
                    Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, lotName, "0" });
                    return;
                }
                #endregion
                lotName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTNAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTNAME].InnerText;
                productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;                
                capacity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CAPACITY] == null ? "0" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CAPACITY].InnerText;
                boxType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
                runMode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RUNMODE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RUNMODE].InnerText;//Add by sy for T3 MES 1.44 20160614
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK)
                {
                    #region [UpDate CST]
                    #region Get and Remove Reply Key boxID
                    string boxIDkey = keyBoxReplyPLCKey.PaperBoxReply;
                    string boxID = Repository.Remove(boxIDkey).ToString();
                    if (boxID == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,BOX NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                        return;
                    }
                    #endregion
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(boxID);
                    if (cst == null)
                    {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN NOT FIND BOX=[{1}] IN CST OBJECT!", eqpID, boxID));
                        return;
                    }
                    lock (cst)
                    {
                        cst.SubBoxID = lotName;
                    }
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                    //BoxProcessStarted(trxID, line, lotName);//[MES 20160530 修改邏輯]
                    #endregion
                }
                else if (line.Data.LINETYPE == eLineType.CELL.CCPCK)
                {
                    #region [NEW CST]
                    #region Get and Remove Reply Key Port
                    string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                    string portNo = Repository.Remove(Portkey).ToString();
                    if (portNo == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,PORT NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                        return;
                    }
                    #endregion
                    if (portNo.PadLeft(2, '0') != "00")
                    {
                        Cassette cst = new Cassette();
                        cst.CassetteID = lotName;
                        cst.BoxName = lotName;
                        cst.PortID = portNo.PadLeft(2, '0');
                        cst.PortNo = portNo.PadLeft(2, '0');
                        ObjectManager.CassetteManager.CreateCassette(cst);
                    }
                    #endregion
                }
                else
                {
                    #region [UpDate CST]
                    #region Get and Remove Reply Key Port
                    string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                    string portNo = Repository.Remove(Portkey).ToString();
                    if (portNo == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,PORT NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                        return;
                    }
                    #endregion
                    if (portNo.PadLeft(2, '0') != "00")
                    {
                        Port port = ObjectManager.PortManager.GetPort(lineName, eqpID, portNo.PadLeft(2, '0'));
                        if (port == null)
                        {
                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("EQUIPMENT=[{0}] CAN NOT FIND PORT_NO=[{1}] IN CST OBJECT!", eqpID, portNo));
                            return;
                        }
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (cst == null)
                        {
                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("EQUIPMENT=[{0}] CAN NOT FIND Cassette=[{1}] IN CST OBJECT!", eqpID, port.File.CassetteID));
                            return;
                        }
                        cst.BoxName = lotName;
                        if (cst.IsBoxed)//原用於ppk boxfinsh,這邊用於store 時check 不需回EQP LotIDCreateRequestReportReply
                        {
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxIdCreateReply OK LINENAME=[{1}],LOTNAME=[{2}],PRODUCTNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                trxID, lineName, lotName, productName, returnCode, returnMessage));
                            return;
                        }
                        cst.IsBoxed = true;
                    }
                    #endregion
                }
                Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, lotName, capacity });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CheckPairBoxRequest(string trxID, string lineName, string nodeId, string boxId1, string boxId2)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckPairBoxRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = nodeId;
                bodyNode[keyHost.BOXNAME].InnerText = boxId1;
                bodyNode[keyHost.PAIRBOXNAME].InnerText = boxId2;
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] CheckPairBoxRequest OK LINENAME=[{1}], MACHINENAME=[{2}], BOXID1=[{3}], BOXID2=[{4}]",
                         trxID, lineName, nodeId, boxId1, boxId2));
                #region MES CheckPairBoxRequest Timeout
                string timeoutName = string.Format("{0}_MES_CheckPairBoxReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CheckPairBoxRequestT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MES_CheckPairBoxReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));

                eReturnCode1 rtcode = returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG;                
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineName));
                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_CheckPairBoxReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion

                string nodeId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string boxName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                string pairBoxName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PAIRBOXNAME] == null ? "0" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PAIRBOXNAME].InnerText;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(nodeId);
                if (eqp == null) throw new Exception(string.Format("CAN NOT FIND EQUIPMENT_ID=[{0}] IN EQUIPMENT OBJECT!", nodeId));

                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply + "_PortNo";
                string portNo = Repository.Remove(key).ToString();
                if (portNo == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,portNo is Null.Don't Reply PLC_DenseBoxIDCheckReportReply", trxID));
                    return;
                }
                #endregion
                #region [Reply NG]
                if (rtcode == eReturnCode1.NG)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_CheckPairBoxReply NG LINENAME=[{1}],MACHINENAME=[{2}],BOXNAME=[{3}],PAIRBOXNAME=[{4}].",
                        trxID, lineName, nodeId, boxName, pairBoxName));
                    Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, "2", portNo });
                    return;
                }
                #endregion                
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_CheckPairBoxReply OK LINENAME=[{1}],MACHINENAME=[{2}],BOXNAME=[{3}],PAIRBOXNAME=[{4}].",
                        trxID, lineName, nodeId, boxName, pairBoxName));
                Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, "1", portNo });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CheckPairBoxRequestT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_CheckPairBoxReply", sArray[0]));
                    return;
                }
                #endregion
                string timeoutName = string.Format("{0}_MES_CheckPairBoxReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", new object[] { eqpID, eBitResult.ON, sArray[0], "2", "0" });

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "CheckPairBoxRequestT9Timeout");

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///  6.11.	BoxProcessEnd       MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxQty">Box Quility</param>
        /// <param name="pPID">PPID</param>
        /// <param name="boxList">BOX ID List</param>
        /// <param name="panelList">Panel List單一BOX List</param>
        public void BoxProcessEnd(string trxID, string lineID, string portID, Cassette cst, IList<Job> jobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                //bodyNode[keyHost.PORTMODE].InnerText = "Boxing";

                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                if (port == null)
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                else
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                //XmlNode abnorListNode = jobNodeClone[keyHost.ABNORMALCODELIST];
                //XmlNode abnorNodeClone = abnorListNode[keyHost.CODE].Clone();
                //abnorListNode.RemoveAll();

                //jobNodeClone.RemoveChild(abnorListNode); //把複製格式中的ABNORMALCODELIST移除
                boxNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                boxListNode.RemoveAll();//把原格式中的BOXLIST移除
                jobListNode.RemoveAll();//把原格式中的BOXLIST移除
                //int x = 0;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                XmlNode boxNode = boxNodeClone.Clone();
                if (line.Data.LINETYPE == eLineType.CELL.CCPCK || line.Data.LINETYPE == eLineType.CELL.CCOVP)
                {
                    boxNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                    boxNode[keyHost.CARRIERNAME].InnerText = "";
                }
                else
                {
                    boxNode[keyHost.BOXNAME].InnerText = cst.BoxName.Trim();//Modify by sy for T3 MES 2015/11/23
                    if (cst.BoxName.Trim() == string.Empty && port.File.Type == ePortType.UnloadingPort)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("BoxName is Empty ,it forget to reqest MES LotIDRequestReport"));
                    }
                    //Add by marine for T3 MES 2015/9/16
                    boxNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID.Trim();
                }
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count.ToString();
                //XmlNode newjobListNode = jobListNode.Clone();
                foreach (Job job in jobList)
                {
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                    if (job.MesCstBody.LOTLIST.Count != 0)
                    {
                        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME; //Watson modify 20141122 For 帶JOB層的
                        //bodyNode[keyHost.HOSTPPID].InnerText = job.MesCstBody.LOTLIST[0].PPID == null ? job.MesProduct.PPID : job.MES_PPID; //Jun Modify 20150326
                    }
                    //Watson add 20150128 For New MES Spec
                    //bodyNode[keyHost.PPID].InnerText = cst.PPID;

                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                    P2M_DecodeEQPFlagBy_CELLBOX(jobNode, line, job);
                    //jobNode[keyHost.SHORTCUTFLAG].InnerText = job.MesProduct.SHORTCUTFLAG;

                    //TODO: CSOT 胡福杰說此部份會再整理一份詳細的文件, 等文件出來再寫這塊
                    #region [Abnormal Code List]
                    XmlNode abromalListNode = jobNode[keyHost.ABNORMALCODELIST];
                    XmlNode codeNode = abromalListNode[keyHost.CODE];
                    abromalListNode.RemoveAll();
                    P2M_DecodeAbnormalCodeFlagBy_CELL(abromalListNode, codeNode, jobNode, line, job);
                    #endregion
                    //foreach (CODEc code in job.MesProduct.ABNORMALCODELIST)
                    //{
                    //    XmlNode abnorNode = abnorNodeClone.Clone();
                    //    abnorNode[keyHost.ABNORMALSEQ].InnerText = code.ABNORMALSEQ;
                    //    abnorNode[keyHost.ABNORMALCODE].InnerText = code.ABNORMALCODE;
                    //    jobNode.AppendChild(abnorNode);
                    //}

                    jobNode[keyHost.BOXULDFLAG].InnerText = job.CellSpecial.BOXULDFLAG;

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
                    if (port == null)
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = "N";
                    else
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                    //Watson Add 20150128 For New MES Spec
                    jobNode[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                    //jobNode[keyHost.PPID].InnerText = job.MES_PPID;
                    //jobNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                    string blockCutSampleFlag = string.Empty;
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                    {
                        if (port.Data.NODENO.Trim() == "L3" || port.Data.NODENO.Trim() == "L03")
                            blockCutSampleFlag = "BP";
                        else
                            blockCutSampleFlag = "DP";
                    }
                    jobNode[keyHost.BLOCKCUTSAMPLEFLAG].InnerText = blockCutSampleFlag;
                    //jobNode[keyHost.OQCNGRESULT].InnerText = ""; //Add for T3 MES by marine 2015/8/19//EQPFLAG 給值 這邊MARK
                    //jobNode[keyHost.CELLSAMPLEFLAG].InnerText = "";  //Add for T3 MES by marine 2015/8/19//EQPFLAG 給值 這邊MARK

                    jobListNode.AppendChild(jobNode);
                }
                boxNode.AppendChild(jobListNode);
                boxListNode.AppendChild(boxNode);

                bodyNode.AppendChild(boxListNode);

                SendToMES(xml_doc);

                string err;
                if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err, "BOX_"))
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, portID, cst.LineRecipeName, cst.MES_CstData.LINERECIPENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///  6.11.	BoxProcessEnd       MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxQty">Box Quility</param>
        /// <param name="pPID">PPID</param>
        /// <param name="boxList">BOX ID List</param>
        /// <param name="panelList">Panel List單一BOX List</param>
        public void BoxProcessEnd_PPK(string trxID, string lineID, List<Cassette> boxList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                #region [OFFLINE]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                #endregion
                //Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = "";//portID
                //bodyNode[keyHost.PORTMODE].InnerText = "Boxing";

                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                //if (port == null)
                //    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                //else
                //    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                //XmlNode abnorListNode = jobNodeClone[keyHost.ABNORMALCODELIST];
                //XmlNode abnorNodeClone = abnorListNode[keyHost.CODE].Clone();
                //abnorListNode.RemoveAll();

                //jobNodeClone.RemoveChild(abnorListNode); //把複製格式中的ABNORMALCODELIST移除
                boxNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                boxListNode.RemoveAll();//把原格式中的BOXLIST移除
                jobListNode.RemoveAll();//把原格式中的BOXLIST移除//QPP PPK 不報BOX 內部資料


                foreach (Cassette cst in boxList)
                {
                    XmlNode boxClone = boxNodeClone.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = cst.CassetteID;
                    boxClone[keyHost.CARRIERNAME].InnerText = cst.CassetteID;
                    boxClone[keyHost.PRODUCTQUANTITY].InnerText = "";
                    boxListNode.AppendChild(boxClone);
                }

                boxNodeClone.AppendChild(jobListNode);
                bodyNode.AppendChild(boxListNode);

                SendToMES(xml_doc);

                //string err;
                //if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err, "BOX_"))
                //{
                //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                //}

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd OK LINENAME=[{1}]",
                         trxID, line.Data.LINEID));
                foreach (var box in boxList)
                {
                    if (box.eBoxType != eBoxType.InBox)//InBox 到BoxendReply OK 才移除 sy add 20151209
                    {
                        ObjectManager.CassetteManager.DeleteBox(box.CassetteID);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///6.11.	BoxProcessEnd        MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxQty">Box Quility</param>
        /// <param name="pPID">PPID</param>
        /// <param name="boxList">BOX ID List</param>
        /// <param name="panelList">Panel List兩個BOX List</param>
        public void BoxProcessEndByDPI(string trxID, Port port, Port port2, IList<Job> jobs, IList<Job> jobs2, IList<string> boxIDlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;

                bodyNode[keyHost.BOXQUANTITY].InnerText = boxIDlist.Count.ToString();
                if (port == null)
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                else
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                if (boxIDlist.Count > 0)
                    bodyNode.RemoveChild(boxListNode);//把原格式中的BOXLIST移除，不然會有多一個

                boxNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                boxListNode.RemoveAll();//把原格式中的BOXLIST移除
                jobListNode.RemoveAll();//把原格式中的BOXLIST移除
                Port boxport;
                //int x = 0;
                IList<Job> jobList = new List<Job>();
                int boxint = 1;
                foreach (string boxid in boxIDlist)
                {
                    XmlNode boxNode = boxNodeClone.Clone();
                    //XmlNode boxlistnode = boxListNode.Clone();
                    boxNode[keyHost.BOXNAME].InnerText = boxid.Trim();
                    if (boxint == 1)
                    {
                        boxport = port;
                        jobList = jobs;
                    }
                    else
                    {
                        boxport = port2;
                        jobList = jobs2;
                    }
                    boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count.ToString();
                    XmlNode joblistnode = jobListNode.Clone();
                    foreach (Job job in jobList)
                    {
                        bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                        if (job.MesCstBody.LOTLIST.Count != 0)
                        {
                            bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                            //bodyNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                        }

                        //Watson add 20150128 For New MES Spec
                        //bodyNode[keyHost.PPID].InnerText = job.MES_PPID;


                        XmlNode jobNode = jobNodeClone.Clone();
                        jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                        jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                        jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                        P2M_DecodeEQPFlagBy_CELLBOX(jobNode, line, job);

                        #region [Abnormal Code List]
                        XmlNode abromalListNode = jobNode[keyHost.ABNORMALCODELIST];
                        XmlNode codeNode = abromalListNode[keyHost.CODE];
                        abromalListNode.RemoveAll();
                        P2M_DecodeAbnormalCodeFlagBy_CELL(abromalListNode, codeNode, jobNode, line, job);
                        #endregion

                        jobNode[keyHost.BOXULDFLAG].InnerText = job.CellSpecial.BOXULDFLAG;
                        //‘OK’ ‘NG’ ‘BYPASS’ 
                        switch (GetProductJudge(line, eFabType.CELL, job))
                        {
                            case "G":
                                jobNode[keyHost.DPIPROCESSFLAG].InnerText = "OK";
                                break;
                            case "N":
                                jobNode[keyHost.DPIPROCESSFLAG].InnerText = "NG";
                                break;
                            default:
                                jobNode[keyHost.DPIPROCESSFLAG].InnerText = "BYPASS";
                                break;
                        }
                        //Watson Add 20150128 For New MES Spec
                        jobNode[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                        //jobNode[keyHost.PPID].InnerText = job.MES_PPID;
                        //jobNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                        jobNode[keyHost.BLOCKCUTSAMPLEFLAG].InnerText = "";
                        jobNode[keyHost.OQCNGRESULT].InnerText = "";
                        jobNode[keyHost.CELLSAMPLEFLAG].InnerText = "";

                        joblistnode.AppendChild(jobNode);

                    }
                    boxNode.AppendChild(joblistnode);
                    boxListNode.AppendChild(boxNode);
                    boxint++;
                    bodyNode.AppendChild(boxListNode);
                }

                SendToMES(xml_doc);

                string err;
                if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err, "BOX_"))
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                }


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, port.Data.PORTID, bodyNode[keyHost.LINERECIPENAME].InnerText, bodyNode[keyHost.HOSTLINERECIPENAME].InnerText));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.11.	BoxProcessEnd
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="port2"></param>
        /// <param name="jobList"></param>
        /// <param name="jobList2"></param>
        /// <param name="boxIDlist"></param>
        public void BoxProcessEndByDPIRemove(string trxID, Port port, Port port2, IList<Job> jobList, IList<Job> jobList2, IList<string> boxIDlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;

                bodyNode[keyHost.BOXQUANTITY].InnerText = "2";
                if (port == null)
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                else
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                bodyNode.RemoveChild(boxListNode);//把原格式中的BOXLIST移除，不然會有多一個

                boxNodeClone.RemoveChild(jobListNode);//把複製格式中的PRODUCTLIST移除
                boxListNode.RemoveAll();//把原格式中的BOXLIST移除
                jobListNode.RemoveAll();//把原格式中的BOXLIST移除
                Port boxport;
                IList<Job> jobListtemp = new List<Job>();
                int boxint = 1;
                foreach (string boxid in boxIDlist)
                {
                    XmlNode boxNode = boxNodeClone.Clone();
                    //XmlNode boxlistnode = boxListNode.Clone();
                    boxNode[keyHost.BOXNAME].InnerText = boxid.Trim();
                    if (boxint == 1)
                    {
                        boxport = port;
                        jobListtemp = jobList;
                    }
                    else
                    {
                        boxport = port2;
                        jobListtemp = jobList2;
                    }
                    boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobListtemp.Count.ToString();
                    XmlNode joblistnode = jobListNode.Clone();
                    foreach (Job job in jobListtemp)
                    {
                        bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                        if (job.MesCstBody.LOTLIST.Count != 0)
                        {
                            bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                            //bodyNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                        }

                        //Watson add 20150128 For New MES Spec
                        //bodyNode[keyHost.PPID].InnerText = job.MES_PPID;


                        XmlNode jobNode = jobNodeClone.Clone();
                        jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                        jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                        jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                        P2M_DecodeEQPFlagBy_CELLBOX(jobNode, line, job);

                        #region [Abnormal Code List]
                        XmlNode abromalListNode = jobNode[keyHost.ABNORMALCODELIST];
                        XmlNode codeNode = abromalListNode[keyHost.CODE];
                        abromalListNode.RemoveAll();
                        P2M_DecodeAbnormalCodeFlagBy_CELL(abromalListNode, codeNode, jobNode, line, job);
                        #endregion

                        jobNode[keyHost.BOXULDFLAG].InnerText = job.CellSpecial.BOXULDFLAG;
                        //‘OK’ ‘NG’ ‘BYPASS’ 
                        switch (GetProductJudge(line, eFabType.CELL, job))
                        {
                            case "G":
                                if (boxport.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                                    jobNode[keyHost.DPIPROCESSFLAG].InnerText = "OK";
                                else
                                    jobNode[keyHost.DPIPROCESSFLAG].InnerText = "BYPASS";
                                break;
                            case "N":
                                if (boxport.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                                    jobNode[keyHost.DPIPROCESSFLAG].InnerText = "NG";
                                else
                                    jobNode[keyHost.DPIPROCESSFLAG].InnerText = "BYPASS";
                                break;
                            case "":
                                jobNode[keyHost.DPIPROCESSFLAG].InnerText = "BYPASS";
                                break;
                        }
                        //Watson Add 20150128 For New MES Spec
                        jobNode[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                        //jobNode[keyHost.PPID].InnerText = job.MES_PPID;
                        //jobNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                        jobNode[keyHost.BLOCKCUTSAMPLEFLAG].InnerText = "";
                        jobNode[keyHost.OQCNGRESULT].InnerText = "";
                        jobNode[keyHost.CELLSAMPLEFLAG].InnerText = "";

                        joblistnode.AppendChild(jobNode);

                    }
                    boxNode.AppendChild(joblistnode);
                    boxListNode.AppendChild(boxNode);
                    boxint++;
                    bodyNode.AppendChild(boxListNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, port.Data.PORTID, bodyNode[keyHost.LINERECIPENAME].InnerText, bodyNode[keyHost.HOSTLINERECIPENAME].InnerText));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.12.	BoxProcessEndReply       MES MessagetSet : Box Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_BoxProcessEndReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                #region[CCPPK] //shihyang add 20150818 //20161102 sy modify 不會使用
                //if (lineName.Contains(eLineType.CELL.CCPPK))
                //{
                //    MES_BoxProcessEndReply_CCPPK(xmlDoc);
                //    return;
                //}
                #endregion
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);

                //Modify for T3 MES by marine 2015/8/19
                string boxName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                string boxType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //string desc = string.Empty;
                List<string> boxlist = new List<string>();

                //Modify by marine for T3 MES 2015/9/11
                XmlNodeList boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SUBBOXLIST].ChildNodes;

                foreach (XmlNode boxnode in boxlistnode)
                {
                    if (boxnode != null)
                    {
                        string subbox = boxnode.InnerText;
                        boxlist.Add(subbox);
                    }
                    //if (boxnode != null)
                    //{
                    //    string boxname = boxnode[keyHost.BOXNAME].InnerText;
                    //    string boxtype = boxnode[keyHost.BOXTYPE].InnerText;
                    //    boxlist.Add(boxname);
                    //    boxlist.Add(boxtype);
                    //}
                }

                //Port port = ObjectManager.PortManager.GetPort(portId);
                //if (port == null)
                //{
                //    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply  OK LINENAME=[{1}] But PORTNAME[{2}] IS ERROR!!" + string.Format(eLOG_CONSTANT.CAN_NOT_FIND_PORT, portId),
                //               trxID, lineName, portId, returnCode, returnMessage));
                //    return;
                //}

                //if (!ObjectManager.CassetteManager.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB(portId, boxlist[0], trxID, returnCode == "0" ? "OK" : "NG", returnCode, returnMessage, out desc, "BOX_"))
                //    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //       string.Format("[BCS <- MES]=[{0}] BOXProcessEndReply.UpdateBOXPorcessEndMesReplyToExecuteXmlAndDB  NG LINENAME={1},PORTNAME={2},BOXNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                //                   trxID, lineName, portId, boxlist[0], returnCode, returnMessage, desc));

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply  NG LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxID, lineName, boxqty, returnCode, returnMessage));

                    //if (!ObjectManager.CassetteManager.FileMoveToIncompleteCST(portId, boxlist[0], trxID, out desc, "BOX_"))
                    //    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[BCS <- MES]=[{0}] BoxProcessEndReply.FileMoveToIncompleteCST  NG LINENAME={1},PORTNAME={2},BOXNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                    //                    trxID, lineName, portId, boxlist[0], returnCode, returnMessage, desc));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply  OK LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, boxqty, returnCode, returnMessage));

                    //if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portId).File.Type != ePortType.LoadingPort)
                    //{
                    //    if (!ObjectManager.CassetteManager.FileMoveToCompleteCST(portId, boxlist[0], trxID, out desc, "BOX_"))
                    //        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //            string.Format("[BCS <- MES][{0}] BoxProcessEndReply.FileMoveToIncompleteCST  NG LINENAME={1},PORTNAME={2},BOXNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                    //                        trxID, lineName, portId, boxlist[0], returnCode, returnMessage, desc));
                    //}
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MES_BoxProcessEndReply_CCPPK(XmlDocument xmlDoc)//shihyang add 20150817
        {
            //try
            //{
            //    string lineName = GetLineName(xmlDoc);
            //    string returnCode = GetMESReturnCode(xmlDoc);
            //    string returnMessage = GetMESReturnMessage(xmlDoc);
            //    string trxID = GetTransactionID(xmlDoc);

            //    #region[Check LineName]
            //    if (!CheckMESLineID(lineName))
            //    {
            //        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
            //    }
            //    #endregion

            //    string paperBoxID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
            //    string paperBoxType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
            //    string boxQuantity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;

            //    XmlNode subBoxlist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SUBBOXLIST];
            //    string boxID = subBoxlist[keyHost.SUBBOX][keyHost.BOXNAME].InnerText;
            //    string boxType = subBoxlist[keyHost.SUBBOX][keyHost.BOXTYPE].InnerText;
            //    Line line = ObjectManager.LineManager.GetLine(lineName);

            //    if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineName));
            //    #region[Delete Timer]
            //    string timeoutName = string.Format("{0}_MES_BoxProcessEndReply", boxID);
            //    if (_timerManager.IsAliveTimer(timeoutName))
            //    {
            //        _timerManager.TerminateTimer(timeoutName);
            //    }
            //    #endregion

            //    //檢察boxID是否正確且找的到(InBox)
            //    Cassette cst = ObjectManager.CassetteManager.GetCassette(boxID);
            //    if (cst == null) throw new Exception(string.Format("CAN NOT FIND CST=[{0}] IN CST OBJECT!", boxID));
            //    if (cst.eBoxType == eBoxType.OutBox) return;
            //    ////建立new CST 資料(OutBox)              
            //    Cassette newCst = new Cassette();
            //    lock (newCst)
            //    {
            //        //    newCst.PortID = "0";
            //        //    newCst.CassetteSequenceNo = "0";
            //        newCst.CassetteID = paperBoxID;
            //        newCst.eBoxType = eBoxType.OutBox;
            //        newCst.SubBoxID = boxID;
            //        newCst.LDCassetteSettingCode = cst.LDCassetteSettingCode;
            //        newCst.LineRecipeName = cst.LineRecipeName;
            //    }
            //    ObjectManager.CassetteManager.CreateBox(newCst);
            //    //ObjectManager.CassetteManager.GetCassette(newCst.CassetteID);
            //    //ObjectManager.CassetteManager.DeleteBox(boxID);//將舊的資料delete

            //    //建立new CST 資料(OutBox)             
            //    //lock (cst)
            //    //{
            //    //    cst.CassetteID = paperBoxID;
            //    //    cst.eBoxType = eBoxType.OutBox;
            //    //    cst.SubBoxID = boxID;
            //    //}
            //    //ObjectManager.CassetteManager.(cst);
            //    if (!(line.File.HostMode == eHostMode.REMOTE)) return;

            //    #region [Get and Remove Reply Key]
            //    //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
            //    string eqpNo = string.Empty;
            //    string key = keyBoxReplyPLCKey.BoxProcessEndReply;
            //    object eqp = Repository.Remove(key);
            //    if (eqp == null)
            //    {
            //        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PaperBoxDataRequestReply", trxID));
            //        return;
            //    }
            //    eqpNo = eqp.ToString();
            //    #endregion
            //    #region [MES Data Reply]
            //    if (!returnCode.Equals("0"))
            //    {
            //        Invoke(eServiceName.PaperBoxService, "BoxProcessFinishReportReply", new object[] { eqpNo, eBitResult.ON, trxID, eReturnCode1.NG, paperBoxID, paperBoxType, boxID, boxType });
            //        #region [Log]
            //        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //                string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply NG LINENAME=[{1}],PaperBoxID=[{2}],BoxID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
            //                    trxID, lineName, paperBoxID, boxID, returnCode, returnMessage));
            //        #endregion
            //        return;
            //    }
            //    Invoke(eServiceName.PaperBoxService, "BoxProcessFinishReportReply", new object[] { eqpNo, eBitResult.ON, trxID, eReturnCode1.OK, paperBoxID, paperBoxType, boxID, boxType });
            //    #region [Log]
            //    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply OK  LINENAME=[{1}],PaperBoxID=[{2}],BoxID=[{3}],CODE=[{4}],MESSAGE=[{5}].",
            //                trxID, lineName, paperBoxID, boxID, returnCode, returnMessage));
            //    #endregion
            //    #endregion
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //}
        }

        /// <summary>
        /// 6.14.	BoxProcessAbnormalEndReply       MES MessagetSet : Box Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_BoxProcessAbnormalEndReply(XmlDocument xmlDoc)
        {
            try
            {
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                #region[CCPPK] //shihyang add 20150818
                if (lineName.Contains(eLineType.CELL.CCPPK))
                {
                    //MES_BoxProcessEndReply_CCPPK(xmlDoc);
                    return;
                }
                #endregion
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);

                //Modify for T3 MES by marine 2015/8/19
                string boxName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                string boxType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //string desc = string.Empty;
                List<string> boxlist = new List<string>();

                //Modify by marine for T3 MES 2015/9/11
                XmlNodeList boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SUBBOXLIST].ChildNodes;

                foreach (XmlNode boxnode in boxlistnode)
                {
                    if (boxnode != null)
                    {
                        string subbox = boxnode.InnerText;
                        boxlist.Add(subbox);
                    }
                }
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply  NG LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxID, lineName, boxqty, returnCode, returnMessage));

                       }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_BoxProcessEndReply  OK LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, boxqty, returnCode, returnMessage));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void BoxProcessAbnormalEnd(string trxID, string lineID, string portID, Cassette cst, IList<Job> jobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessAbnormalEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                if (port == null)
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = "N";
                else
                    bodyNode[keyHost.SAMPLEFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                XmlNode jobListNode = boxNodeClone[keyHost.PRODUCTLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.PRODUCT].Clone();

                boxNodeClone.RemoveChild(jobListNode);
                boxListNode.RemoveAll();
                jobListNode.RemoveAll();
                //int x = 0;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                XmlNode boxNode = boxNodeClone.Clone();
                if (line.Data.LINETYPE == eLineType.CELL.CCPCK)
                {
                    boxNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                    boxNode[keyHost.CARRIERNAME].InnerText = "";
                }
                else
                {
                    boxNode[keyHost.BOXNAME].InnerText = cst.BoxName.Trim();
                    if (cst.BoxName.Trim() == string.Empty && port.File.Type == ePortType.UnloadingPort)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("BoxName is Empty ,it forget to reqest MES LotIDRequestReport"));
                    }
                    boxNode[keyHost.CARRIERNAME].InnerText = cst.CassetteID.Trim();
                }
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count.ToString();
                foreach (Job job in jobList)
                {
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                    if (job.MesCstBody.LOTLIST.Count != 0)
                    {
                        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME; 
                    }
                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.POSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                    jobNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                    P2M_DecodeEQPFlagBy_CELLBOX(jobNode, line, job);
                    #region [Abnormal Code List]
                    XmlNode abromalListNode = jobNode[keyHost.ABNORMALCODELIST];
                    XmlNode codeNode = abromalListNode[keyHost.CODE];
                    abromalListNode.RemoveAll();
                    P2M_DecodeAbnormalCodeFlagBy_CELL(abromalListNode, codeNode, jobNode, line, job);
                    #endregion
                    jobNode[keyHost.BOXULDFLAG].InnerText = job.CellSpecial.BOXULDFLAG;
                    if (port == null)
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = "N";
                    else
                        jobNode[keyHost.DPIPROCESSFLAG].InnerText = port.File.DPISampligFlag == eBitResult.ON ? "Y" : "N";

                    //Watson Add 20150128 For New MES Spec
                    jobNode[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                    //jobNode[keyHost.PPID].InnerText = job.MES_PPID;
                    //jobNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;
                    string blockCutSampleFlag = string.Empty;
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                    {
                        if (port.Data.NODENO.Trim() == "L3" || port.Data.NODENO.Trim() == "L03")
                            blockCutSampleFlag = "BP";
                        else
                            blockCutSampleFlag = "DP";
                    }
                    jobNode[keyHost.BLOCKCUTSAMPLEFLAG].InnerText = blockCutSampleFlag;
                    //jobNode[keyHost.OQCNGRESULT].InnerText = ""; //Add for T3 MES by marine 2015/8/19//EQPFLAG 給值 這邊MARK
                    //jobNode[keyHost.CELLSAMPLEFLAG].InnerText = "";  //Add for T3 MES by marine 2015/8/19//EQPFLAG 給值 這邊MARK

                    jobListNode.AppendChild(jobNode);
                }
                boxNode.AppendChild(jobListNode);
                boxListNode.AppendChild(boxNode);

                bodyNode.AppendChild(boxListNode);

                SendToMES(xml_doc);

                string err;
                if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err, "BOX_"))
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, portID, cst.LineRecipeName, cst.MES_CstData.LINERECIPENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.13.	BoxProcessLineRequest       MES MessageSet :Box loads to PPK manual port for DPK/DPS unpacking,
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">dpk/dps Line ID</param>
        /// <param name="ppklinename">ppk Line ID</param>
        /// <param name="boxid">Box ID</param>
        /// <param name="portID">Target Port ID</param>
        public void BoxProcessLineRequest(string trxID, string lineName, string ppklinename, string boxid, string portID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));


                    #region PLC Write Dense Box ID Check Report Reply
                    #region Get and Remove Reply Key
                    string eqpNo = string.Empty;
                    string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply;
                    object eqp = Repository.Remove(key);
                    if (eqp == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply MES_BoxProcessLineReply", trxID));
                        return;
                    }
                    eqpNo = eqp.ToString();
                    #endregion

                    #region MES BoxLabelInformationReply Timeout
                    string timeoutName1 = string.Format("{0}_{1}_MES_BoxProcessLineReply", lineName, portID);
                    if (_timerManager.IsAliveTimer(timeoutName1))
                    {
                        _timerManager.TerminateTimer(timeoutName1);
                    }
                    #endregion

                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "1", "0" });
                    else
                        Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "2", "0" });
                    #endregion
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessLineRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.BOXNAME].InnerText = boxid.Trim();

                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.PROCESSLINE].InnerText = ppklinename;

                #region MES BoxProcessLineReply Timeout
                string timeoutName = string.Format("{0}_{1}_MES_BoxProcessLineReply", lineName, portID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(BoxProcessLineReply_Timeout), trxID);
                #endregion

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.14.	BoxProcessLineReply     MES MessageSet :MES  Reply Box loads to PPK manual port for DPK/DPS unpacking
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_BoxProcessLineReply(XmlDocument xmlDoc)
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
                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string boxid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                string processline = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PROCESSLINE].InnerText;
                string setcode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
                string result = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS <- MES]=[{0}] MES_BoxProcessLineReply NG LINENAME={1},PORTNAME={2},BOXNAME={3},CODE={4},MESSAGE={5}.",
                            trxID, lineName, portid, boxid, returnCode, returnMessage));

                    //to do?
                    result = "N";
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_BoxProcessLineReply OK LINENAME={1},PORTNAME={2},BOXNAME={3},CODE={4},MESSAGE={5}.",
                                         trxID, lineName, portid, boxid, returnCode, returnMessage));
                    //to do?
                }

                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid.PadLeft(2, '0'));
                if (port == null) throw new Exception(string.Format("Can't find Port ID =[{0}] in PortEntity!", portid.PadLeft(2, '0')));


                #region PLC Write Dense Box ID Check Report Reply
                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply MES_BoxProcessLineReply", trxID));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion

                object[] _data = new object[5]
                { 
                    eqpNo,   
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    result == "Y"?"1":"2", //1：OK  2：NG  3：etc NG…
                    port.Data.PORTNO
                };

                #region MES BoxLabelInformationReply Timeout
                string timeoutName = string.Format("{0}_{1}_MES_BoxProcessLineReply", lineName, portid);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", _data);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void BoxProcessLineReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  LINEID=[{1}], PORTID=[{2}]  BoxProcessLineReply MES Reply Timeout.", trackKey, sArray[0], sArray[1]);

                string timeoutName = string.Format("{0}_{1}_MES_BoxProcessLineReply", sArray[0], sArray[1]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0], sArray[1]);
                if (port == null) throw new Exception(string.Format("Can't find LINEID=[{0}],PORT ID =[{0}] in PortEntity!", sArray[0], sArray[1]));

                #region PLC Write Dense Box ID Check Report Reply
                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply MES_BoxProcessLineReply", trackKey));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion

                object[] _data = new object[5]
                { 
                    eqpNo,   
                    eBitResult.ON,    /*1 ON*/
                    trackKey,                          /*2 trx id */
                    "2", //2：NG
                    port.Data.PORTNO
                };
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxIDCheckReportReply", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.15.	BoxProcessStarted        MES MessageSet : BC reports Box processing has been started to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxQty"></param>
        /// <param name="pPID"></param>
        /// <param name="bOXIDList"></param>
        public void BoxProcessStarted(string trxID, Port port, Cassette cst)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;

                //Watson add 20150128 For New MES Spec
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LINERECIPENAME;
                //Modify by marine for T3 MES 2015/8/20
                //bodyNode[keyHost.PPID].InnerText = cst.PPID;
                //bodyNode[keyHost.HOSTPPID].InnerText = cst.MES_CstData.LOTLIST.Count == 0 ? cst.PPID : cst.MES_CstData.LOTLIST[0].PPID;

                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Add by marine for T3 MES 2015/9/11
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                XmlNode boxClone = box.Clone();
                boxClone[keyHost.BOXNAME].InnerText = cst.CassetteID;
                boxListNode.AppendChild(boxClone);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}],LINERECIPENAME=[{2}],BOXNAME=[{3}].",
                    trxID, line.Data.LINEID, cst.LineRecipeName, cst.CassetteID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// BoxProcessStarted[CCPPK]
        /// </summary>//shihyang add 20150817
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="cst"></param>
        public void BoxProcessStarted(string trxID, Line line, string boxID)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = "";
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                XmlNode boxClone = box.Clone();
                boxClone[keyHost.BOXNAME].InnerText = boxID;
                boxListNode.AppendChild(boxClone);

                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.15.	BoxProcessStarted
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="line"></param>
        /// <param name="boxList"></param>
        public void BoxProcessStarted_PPK(string trxID, Line line, List<Cassette> boxList)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = boxList[0].LineRecipeName;
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = boxList[0].LineRecipeName;
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxList.Count.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify for T3 Spec 1.21 shihyang
                XmlNode boxNodes = boxListNode[keyHost.BOX];
                //XmlNode boxNode = boxNodes[keyHost.BOXNAME];
                boxListNode.RemoveAll();
                foreach (Cassette cst in boxList)
                {
                    XmlNode boxClone = boxNodes.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = cst.CassetteID;
                    boxListNode.AppendChild(boxClone);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.15.	BoxProcessStarted
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="line"></param>
        /// <param name="cst"></param>
        /// <param name="boxidlist"></param>
        public void BoxProcessStartedDPI(string trxID, Line line, Cassette cst, List<string> boxidlist)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxidlist.Count.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LINERECIPENAME;

                //Modify for T3 Spec 
                //bodyNode[keyHost.PPID].InnerText = cst.PPID;
                //bodyNode[keyHost.HOSTPPID].InnerText = cst.MES_CstData.LOTLIST.Count == 0 ? cst.PPID : cst.MES_CstData.LOTLIST[0].PPID;

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNode = boxListNode[keyHost.BOXNAME];
                boxListNode.RemoveAll();
                foreach (string boxid in boxidlist)
                {
                    XmlNode boxClone = boxNode.Clone();

                    boxClone.InnerText = boxid;
                    boxListNode.AppendChild(boxClone);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxProcessStarted OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///6.16.	BoxTargetPortChanged         MES MessageSet : when box goes to target ULD Port,  adjust Box's position Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="boxid">Box ID</param>
        /// <param name="portID">Target Port ID</param>
        public void BoxTargetPortChanged(string trxID, string lineName, string boxid, string portID)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxTargetPortChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.BOXNAME].InnerText = boxid;
                bodyNode[keyHost.PORTNAME].InnerText = portID;

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
        ///  6.34.	CheckBoxNameRequest     MES MessageSet : When Palletizing EQ reads box name in UNPACK mode to MES .
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="bOXID">Box ID</param>
        /// <param name="PalletteID">Pallette ID</param>
        public void CheckBoxNameRequest(string trxID, string lineName, string bOXID, string carrID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] CheckBoxNameRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    #region Get and Remove Reply Key
                    string key = keyBoxReplyPLCKey.BoxIDCheckRequestReply;
                    string eqpID = Repository.Remove(key).ToString();
                    if (eqpID == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                        return;
                    }
                    #endregion
                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.DenseBoxService, "BoxIDCheckRequestReply", new object[] { eqpID, eBitResult.ON, trxID, eReturnCode1.OK, "", "", "" });
                    else
                        Invoke(eServiceName.DenseBoxService, "BoxIDCheckRequestReply", new object[] { eqpID, eBitResult.ON, trxID, eReturnCode1.NG, "", "", "" });
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CheckBoxNameRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CARRIERNAME].InnerText = carrID;
                bodyNode[keyHost.BOXNAME].InnerText = bOXID;

                SendToMES(xml_doc);

                #region MES BoxIdCreateRequest Timeout
                string timeoutName = string.Format("{0}_MES_CheckBoxNameRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CheckBoxNameRequestT9Timeout), trxID);
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] CheckBoxNameRequest OK LINENAME=[{1}],BOXNAME=[{2}],PALLETNAME=[{3}].",
                    trxID, lineName, bOXID, carrID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.35.	CheckBoxNameReply       MES MessagetSet : MES sends EQ box name validation result
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CheckBoxNameReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do//MES_reply NG format不全 防呆 sy edit 20160516
                string carrID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;
                string boxname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;
                //Add for T3 MES for marine 2015/8/21
                string boxtype = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXTYPE].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;
                string valicode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALICODE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALICODE].InnerText;
                string boxSettingCode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXSETTINGCODE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXSETTINGCODE].InnerText;
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.BoxIDCheckRequestReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                    return;
                }
                #endregion
                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_CheckBoxNameRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                if (returnCode != "0" || valiresult != "Y")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}]  MES_CheckBoxNameReply NG LINENAME=[{1}],PALLETNAME=[{2}],BOXNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                            trxID, lineName, carrID, boxname, returnCode, returnMessage));
                    //(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string boxId, string boxtype, string palletID)
                    Invoke(eServiceName.DenseBoxService, "BoxIDCheckRequestReply", new object[7] { eqpID, eBitResult.ON, trxID, eReturnCode1.NG, "", "", "" });
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}]  MES_CheckBoxNameReply  OK LINENAME=[{1}],PALLETNAME=[{2}],BOXNAME=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, lineName, carrID, boxname, returnCode, returnMessage));
                    //BoxIDCheckRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string boxId, string boxtype, string palletID)
                    Invoke(eServiceName.DenseBoxService, "BoxIDCheckRequestReply", new object[7] { eqpID, eBitResult.ON, trxID, eReturnCode1.OK, boxname, boxtype, carrID });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.43.	CutComplete         MES MessageSet : Reports when Glass is cut to panels to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="cSTID">Cassette ID</param>
        /// <param name="Job">Job Entity</param>
        public void CutComplete(string trxID, string lineName, string cSTID, Job job, List<Job> subJobList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] CutComplete Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CutComplete") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CARRIERNAME].InnerText = cSTID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                //Add by marine for T3 MES 2015/8/21
                XmlNode subProductList = bodyNode[keyHost.SUBPRODUCTLIST];
                XmlNode subProductClone = subProductList[keyHost.SUBPRODUCT].Clone();
                subProductList.RemoveAll();
                if (line.Data.LINETYPE.Trim() == eLineType.CELL.CCPCS)//sy add for MES Test(20151204)  Cut need report
                {
                    foreach (Job subJob in subJobList)
                    {
                        XmlNode subProduct = subProductClone.Clone();
                        subProduct[keyHost.SUBPRODUCTPOSITION].InnerText = subJob.MesProduct.SUBPRODUCTPOSITION;
                        subProduct[keyHost.SUBPRODUCTNAME].InnerText = subJob.SubProductName;
                        subProduct[keyHost.SUBPRODUCTSPECNAME].InnerText = subJob.MesProduct.SUBPRODUCTSPECNAEM;
                        subProduct[keyHost.SUBPRODUCTSIZE].InnerText = subJob.MesProduct.SUBPRODUCTSIZE;
                        subProductList.AppendChild(subProduct);
                    }
                }
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] CutComplete OK LINENAME=[{1}],CARRIERNAME=[{2}].",
                    trxID, lineName, cSTID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.57.	LineLinkChanged
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="eqpid"></param>
        /// <param name="proresult"></param>
        public void LineLinkChanged(string trxID, string lineName, string eqpid, eCELL_TCVDispatchRule proresult)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LineLinkChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.MACHINENAME].InnerText = eqpid;
                //POL->MaxCut changed to dispatch to POL
                //NRP-> MaxCut changed to only run NRP
                //CUT-> MaxCut changed to not dispatch to POL
                bodyNode[keyHost.LINKPROCESSRESULT].InnerText = proresult.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();



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
        /// 6.97.	MaxCutGlassProcessEnd       MES MessageSet : TKOUT glass that is not unloaded from a port
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        public void MaxCutGlassProcessEnd(string trxID, string lineName, Equipment eqp, Job job)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("MaxCutGlassProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;

                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = "1"; //只有一片
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                bodyNode[keyHost.PPID].InnerText = job.MES_PPID; //ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.MES_PPID).Trim(); 
                bodyNode[keyHost.HOSTPPID].InnerText = job.MesCstBody.LOTLIST[0].PPID;


                XmlNode joblist = bodyNode[keyHost.PRODUCTLIST];
                XmlNode jobnodeclone = joblist[keyHost.PRODUCT].Clone();
                joblist.RemoveAll();

                XmlNode jobnode = jobnodeclone.Clone();

                jobnode[keyHost.POSITION].InnerText = job.FromSlotNo;
                jobnode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                jobnode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                jobnode[keyHost.DENSEBOXID].InnerText = job.MesProduct.DENSEBOXID;//
                jobnode[keyHost.PRODUCTJUDGE].InnerText = ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                jobnode[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;
                jobnode[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation;
                jobnode[keyHost.PAIRPRODUCTNAME].InnerText = "";  //job.MesProduct.PPID;
                jobnode[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                jobnode[keyHost.PRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                jobnode[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                jobnode[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesProduct.REVPROCESSOPERATIONNAME;
                jobnode[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;

                XmlNode abnorlist = jobnode[keyHost.ABNORMALCODELIST];
                XmlNode abnornodeclone = abnorlist[keyHost.CODE].Clone();
                abnorlist.RemoveAll();
                foreach (CODEc code in job.MesProduct.ABNORMALCODELIST)
                {
                    XmlNode codenode = abnornodeclone.Clone();
                    codenode[keyHost.ABNORMALSEQ].InnerText = code.ABNORMALSEQ;
                    codenode[keyHost.ABNORMALCODE].InnerText = code.ABNORMALCODE;
                    abnorlist.AppendChild(codenode); // Add by marine 2015/8/25
                }
                if (job.HoldInforList.Count() > 0)
                {
                    jobnode[keyHost.HOLDFLAG].InnerText = "Y";
                    string holdmachine = string.Empty;
                    string holdoperater = string.Empty;
                    for (int i = 0; i < job.HoldInforList.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(holdmachine)) holdmachine += ";";
                        holdmachine += job.HoldInforList[i].NodeID;
                        if (!string.IsNullOrEmpty(holdoperater)) holdoperater += ";";
                        holdoperater += job.HoldInforList[i].OperatorID;
                    }
                    jobnode[keyHost.HOLDMACHINE].InnerText = holdmachine;
                    jobnode[keyHost.HOLDOPERATOR].InnerText = holdoperater;
                }
                //目前不使用
                XmlNode psheighList = jobnode[keyHost.PSHEIGHTLIST];
                //Modify by marine for T3 MES 2015/9/11
                XmlNode psheight = psheighList[keyHost.PSHEIGHT];
                psheighList.RemoveAll();

                XmlNode psheightClone = psheight.Clone();
                psheightClone[keyHost.SITEVALUE].InnerText = "";
                psheighList.AppendChild(psheightClone);

                //  <PSHEIGHTLIST>
                //        <SITEVALUE></SITEVALUE>
                //</PSHEIGHTLIST>
                jobnode[keyHost.DUMUSEDCOUNT].InnerText = job.MesProduct.DUMUSEDCOUNT;
                jobnode[keyHost.CFTYPE1REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE1REPAIRCOUNT;
                jobnode[keyHost.CFTYPE2REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE2REPAIRCOUNT;
                jobnode[keyHost.CARBONREPAIRCOUNT].InnerText = job.MesProduct.CARBONREPAIRCOUNT;
                jobnode[keyHost.LASERREPAIRCOUNT].InnerText = ""; // MES會計算, 不用填
                jobnode[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, job.CellSpecial.CuttingFlag);
                //jobnode[keyHost.GMURAFLAG].InnerText = job.CellSpecial.CGMOFlag.ToString();
                //jobnode[keyHost.QTAPFLAG].InnerText = job.FirstRunFlag == "1" ? "Y" : "N";
                //TODO: ODF Special   SAMPLEFLAG
                if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                {
                    // CELL SPECIAL
                }
                else
                {
                    jobnode[keyHost.SAMPLEFLAG].InnerText = "N";
                }
                jobnode[keyHost.MASKNAME].InnerText = "";
                if (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                    line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                    line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                    line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                    line.Data.LINETYPE==eLineType.ARRAY.DRY_TEL)
                {
                    jobnode[keyHost.CHAMBERNAME].InnerText = job.ChamberName;
                }

                //TODO: 等CSOT提供各Shop或各LINE 判斷已做過的依據
                jobnode[keyHost.PROCESSRESULT].InnerText = "Y";

                //TODO: CF ITO Line 用法： BC 解析tracking data，sputter#1 上報1A,Sputter#2上報1B. 在Lot process end時上報給MES.
                jobnode[keyHost.ITOSIDEFLAG].InnerText = job.MesProduct.ITOSIDEFLAG;

                XmlNode materialList = jobnode[keyHost.MATERIALLIST];
                XmlNode materialClone = materialList[keyHost.MATERIAL].Clone();
                materialList.RemoveAll();
                //   <MATERIALLIST>
                //    <MATERIAL>
                //        <MATERIALTYPE></MATERIALTYPE>
                //        <MATERIALNAME></MATERIALNAME>
                //    </MATERIAL>
                //</MATERIALLIST>
                Port sourcePort = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, job.SourcePortID);
                if (sourcePort != null)
                {
                    switch (sourcePort.Data.PORTATTRIBUTE.Trim())
                    {
                        case keyCELLPORTAtt.DENSE:
                        case keyCELLPORTAtt.BOX: jobnode[keyHost.SOURCEDURABLETYPE].InnerText = "B"; break;
                        default: jobnode[keyHost.SOURCEDURABLETYPE].InnerText = "C"; break;
                    }
                }
                else
                {
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                         string.Format("CSTSEQ =[{0}], SLOTNO =[{1}], GLASSID =[{2}], can't find source port, port id =[{3}]",
                         job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID, job.SourcePortID));
                    jobnode[keyHost.SOURCEDURABLETYPE].InnerText = "C";
                }

                jobnode[keyHost.SAMPLETYPE].InnerText = "";
                jobnode[keyHost.USEDCOUNT].InnerText = job.CellSpecial.UVMaskAlreadyUseCount;

                jobnode[keyHost.ABNORMALENG].InnerText = "";

                jobnode[keyHost.GAPSAMPLEFLAG].InnerText = ""; //Report by ODF line for LC Drop  machine abnormal case ,  if glass has this flag, then it must do GAP sampling.
                //jobnode[keyHost.CENGFLAG].InnerText ="";
                //jobnode[keyHost.CQLTFLAG].InnerText ="";
                //jobnode[keyHost.FMAFLAG].InnerText =job.MesProduct.FMAFLAG;
                //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                jobnode[keyHost.PPID].InnerText = job.MES_PPID; //ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.MES_PPID).Trim(); 
                jobnode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;

                P2M_DecodeProductFlagBy_CELL(jobnode, line, job);

                joblist.AppendChild(jobnode);

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
        /// 6.98.	MaxCutGlassProcessEndReply      MES MessageSet : TKOUT glass that is not unloaded from a port
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MaxCutGlassProcessEndReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string jobcount = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY].InnerText;
                XmlNode joblist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTLIST][keyHost.PRODUCT];


                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MaxCutGlassProcessEndReply NG LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MaxCutGlassProcessEndReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, machineName, returnCode, returnMessage));
                    //to do?
                }



            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.101.	PanelInformationRequest     MES MessageSet : BC requests MES to validate Panel on  Mport
        /// Add by marine for T3 MES 2015/8/26
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eQPID"></param>
        /// <param name="panelID"></param>
        /// <param name="recipeParaCheck"></param>
        public void PanelInformationRequest(string trxID, string lineName, string eqpID, string panelID, string commandNo, string recipeParaCheck)//sy modify 20160911
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
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelInformationRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = panelID;
                bodyNode[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText = recipeParaCheck;

                SendToMES(xml_doc);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpID.Trim());
                if (eqp == null) throw new Exception(string.Format("CAN NOT FIND EQUIPMENT_ID=[{0}] IN EQUIPMENT OBJECT!", eqpID));

                #region MES PanelInformationRequest Timeout
                string timeoutName = string.Format("{0}_MES_PanelInformationReply#{1}", eqp.Data.NODENO, commandNo);//sy modify 20160911
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PanelInformationReply_Timeout), trxID);
                #endregion

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
        /// 6.102.	PanelInformationReply       MES MessageSet : MES sends cassette validation result and infomation upon Panel validation request from BC
        /// Add by marine for T3 MES 2015/8/26
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_PanelInformationReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineID = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);

                //sy modify 20160911
                #region keyCommandNo
                string keyCommandNo = string.Empty;
                int maxCommandNo = 2;
                string commandNo = string.Empty;
                string commandNoGlassId = string.Empty;

                for (int comNo = 0; comNo < maxCommandNo+1; comNo++)
                {
                    keyCommandNo = keyBoxReplyPLCKey.PanelRequestReplyCommandNo + "#" + comNo.ToString().PadLeft(2, '0');
                    if (Repository.Get(keyCommandNo) != null)
                        commandNoGlassId = Repository.Get(keyCommandNo).ToString();
                    else
                        commandNoGlassId = string.Empty;
                    if (!string.IsNullOrEmpty(commandNoGlassId))
                    {
                        if (commandNoGlassId.Split('#')[1] == body[keyHost.PRODUCTNAME].InnerText.Trim())
                        {
                            commandNo = commandNoGlassId.Split('#')[0];
                            Repository.Remove(keyCommandNo);                   
                            break;
                        }
                    }
                }

                //string keyCommandNo = keyBoxReplyPLCKey.PanelRequestReplyCommandNo;     //add by zhuxingxing 20160906 
                //string commandNo = Repository.Get(keyCommandNo).ToString();
                #endregion

                if (!CheckMESLineID(lineID))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineID));
                }
                
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;

                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName.Trim());
                if (eqp == null) throw new Exception(string.Format("CAN NOT FIND EQUIPMENT_ID=[{0}] IN EQUIPMENT OBJECT!", machineName));
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line == null) throw new Exception(string.Format("CAN NOT FIND LINE_ID=[{0}] IN LINE OBJECT!", lineID));

                //sy modify 20160911
                #region Kill MES PanelInformationRequest Timeout
                string timeoutName = string.Format("{0}_MES_PanelInformationReply#{1}", eqp.Data.NODENO, commandNo);//sy modify 20160911
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PanelInforamtionReply NG LINENAME =[{1}],MACHINENAME =[{2}],PRODUCTNAME =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                        trxID, lineID, machineName, productName, returnCode, returnMessage));
                    //Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID }
                    //20171120 by huangjiayin POL OHV SpecialEvent
                    if (machineName.Contains("CCOHV"))
                    {
                        Invoke(eServiceName.VCRService, "VCRMismatchJobDataRequestReply", new object[] {commandNo, line, eqp.Data.NODENO, eBitResult.ON, eReturnCode1.NG, null, trxID});
                    }
                    else
                    {
                        Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, commandNo });  //add commandNo by zhuxingxing 20160906
                    }

                    //20161214 add by huangjiayin:Mes Reply NG Send CIM Message
                    string cim_msg = "MES Reply NG:" + returnCode + "_" + returnMessage;
                    cim_msg = cim_msg.Trim();
                    if (line.Data.LINETYPE == eLineType.CELL.CCRWK || line.Data.LINETYPE == eLineType.CELL.CCPCS) return;//20170411 huangjiayin: 师令说，RWK不需要BC发消息
                    while (cim_msg.Length > 80)
                    {
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg.Substring(0,80), "", "1" });
                        cim_msg = cim_msg.Substring(80, cim_msg.Length - 80);
 
                    }
                    if (cim_msg.Length > 0)
                    {
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg, "", "1" });
                    }
                    return;
                }

                //20170425:huangjiayin add pcs line function: only update blockoxinformation for pcs job data request:3
                if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                {
                    string pcs_jobid = body[keyHost.PRODUCTNAME].InnerText.Trim();
                    Job pcs_job= ObjectManager.JobManager.GetBlockJob(pcs_jobid);

                    switch (body[keyHost.PRODUCTOX].InnerText.Trim())
                    {
                        case "O":
                        case "A":
                        case "C":
                        case "X":
                                    pcs_job.CellSpecial.BlockOXInformation = body[keyHost.PRODUCTOX].InnerText.Trim().PadRight(12,'O');
                                    Invoke(eServiceName.JobService, "JobDataRequestReportReplyForPanelInformation", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, pcs_job, commandNo });
                                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PanelInforamtionReply  OK LINENAME =[{1}],MACHINENAME =[{2}],PRODUCTNAME =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                        trxID, lineID, machineName, productName, returnCode, returnMessage));
                            break;

                        default:
                            Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, commandNo });
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PanelInforamtionReply  LINENAME =[{1}],MACHINENAME =[{2}],PRODUCTNAME =[{3}],CODE =[{4}],BUT PRODUCTOX =[{5}] is not in <O,X,A,C>",
                            trxID, lineID, machineName, productName, returnCode, body[keyHost.PRODUCTOX].InnerText.Trim()));
                            break;
                    }

                    return;
 
                }



                string lotname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTNAME].InnerText;  //MES SEPC 1.19, MES Reply NG時不會給lotname所以放在判斷完OK/NG之後
                string productSpecName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string lineRecipeNmae = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string processOperationName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PROCESSOPERATIONNAME].InnerText;
                //string productOwner = string.Empty;

                string productOwner = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OWNERID].InnerText;//MES 1.37 remove 20160224 by sy 閰波說OWNERID相同
                string productGroup = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECGROUP].InnerText;//MES 1.37 add 20160224 by sy
                string productGrade = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTGRADE].InnerText;
                string recipeParaValidationFlag = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RECIPEPARAVALIDATIONFLAG].InnerText;
                string[] pnlJPSCode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SUBPRODUCTJPSCODE].InnerText.Split(';');


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PanelInforamtionReply  OK LINENAME =[{1}],MACHINENAME =[{2}],PRODUCTNAME =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                        trxID, lineID, machineName, productName, returnCode, returnMessage));


                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                // 從MES Data 取出 Recipe Parameter 是否Check部份
                bool recipePara = body[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText.Equals("Y");

                IDictionary<string, IList<RecipeCheckInfo>> recipeIDCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IDictionary<string, IList<RecipeCheckInfo>> recipeParaCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
                IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

                List<string> lstNoCehckEQ = new List<string>();
                IList<Job> mesJobs = new List<Job>();
                IList<Job> JobsforCut = new List<Job>();//sy add for Cut 前資料也要比較
                Dictionary<string, int> nodeStack = new Dictionary<string, int>();

                string ppid = string.Empty;
                string mesppid = string.Empty;
                #region [Create New Job]
                int cstNo = 50001;//sy add 20160824 修改 cstNo & slotNo 產生規則
                int slotNo = 1;//sy modify 20160912 將雙通道區分開來 Reply & Reply#01 用 1 , Reply#02 用 2 //因相同cstNo 砍殘帳時怕會誤砍 (砍資料夾)所以還是用cstNo來區分
                for (int i = cstNo; i<= 65500; i++) // Modify By Yangzhenteng For panel Information Request(50001--65534全部占用则替换掉的最久资料)
                {
                    switch (commandNo)
                    {
                        case "":
                        case "01":
                            if (i % 2 == 0) i++;
                            break;
                        case "02":
                            if (i % 2 != 0) i++;
                            break;
                    }
                    if (ObjectManager.JobManager.GetJob(i.ToString(), slotNo.ToString()) != null)
                    {
                        if (i < 65500)
                        { }
                        else
                        {
                            IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                            lock (jobs)
                            {
                                List<Job> _jobs = (from j in jobs where (int.Parse(j.CassetteSequenceNo) > 50000) orderby j.LastUpdateTime ascending select j).ToList();
                                cstNo = int.Parse(_jobs[0].CassetteSequenceNo.Trim());
                            }
                        }
                    }
                    else
                    {
                        if (i < 65500)
                        {
                            cstNo = i;
                            break;
                        }
                        else
                        {
                            throw new Exception(string.Format("CAN NOT Create NEW JOB CST SEQ NO =[{0}]!", cstNo));
                        }
                    }
                }               
                Job job = new Job(cstNo, slotNo);
                job.CassetteSequenceNo = cstNo.ToString();
                job.JobSequenceNo = slotNo.ToString();
                job.TargetPortID = "0";
                job.ToSlotNo = "0";
                job.TrackingData = SpecialItemInitial("TrackingData", 32);
                job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
                job.EQPJobID = body[keyHost.PRODUCTNAME].InnerText.Trim();
                job.FromCstID = lotname;
                job.SubstrateType = eSubstrateType.Chip;
                job.ToCstID = string.Empty;
                job.FromSlotNo = slotNo.ToString();
                job.CurrentEQPNo = eqp.Data.NODENO;
                job.CSTOperationMode = eqp.File.CSTOperationMode;
                job.LineRecipeName = lineRecipeNmae;
                job.MesProduct.PRODUCTRECIPENAME = lineRecipeNmae;
                job.MesProduct.GROUPID = body[keyHost.GROUPID].InnerText.Trim();//20171017 huangjiayin add
                job.MesProduct.OWNERTYPE = body[keyHost.OWNERTYPE].InnerText.Trim();
                job.GroupIndex = ObjectManager.JobManager.M2P_GetCellGroupIndex(line, body[keyHost.GROUPID].InnerText);//-----MES
                job.CIMMode = eBitResult.ON;
                job.JobType = ObjectManager.JobManager.M2P_GetJobType(body[keyHost.PRODUCTTYPE].InnerText);//-----MES
                job.JobJudge = "0";
                job.SamplingSlotFlag = "1";
                job.FirstRunFlag = "0";
                job.JobGrade = body[keyHost.PRODUCTGRADE].InnerText.Trim();
                job.MesProduct.PRODUCTGRADE = body[keyHost.PRODUCTGRADE].InnerText.Trim();//20161125 ADD BY huangjiayin
                job.GlassChipMaskBlockID = body[keyHost.PRODUCTNAME].InnerText.Trim();
                job.MesProduct.PRODUCTNAME = body[keyHost.PRODUCTNAME].InnerText.Trim();
                job.ChipCount = 1;
                job.OXRInformation = body[keyHost.SUBPRODUCTGRADES].InnerText;//-----MES
                job.PPID = body[keyHost.PPID].InnerText;//-----MES
                job.MES_PPID = body[keyHost.PPID].InnerText;//-----MES
                job.MesProduct.ARRAYPRODUCTNAME = body[keyHost.ARRAYPRODUCTNAME]==null ? "": body[keyHost.ARRAYPRODUCTNAME].InnerText;  //add item by zhuxingxing 20160819
                job.MesProduct.CFPRODUCTNAME = body[keyHost.CFPRODUCTNAME] == null ? "" : body[keyHost.CFPRODUCTNAME].InnerText;        //add MES item by zhuxingxing 20160819
                job.MesProduct.ARRAYLOTNAME = body[keyHost.ARRAYLOTNAME] == null ? "" : body[keyHost.ARRAYLOTNAME].InnerText;          //add MES item by zhuxingxing 20160819
                job.MesProduct.PROCESSFLAG = "Y"; //20161128 sy add
                job.MesProduct.MESPROCESSFLAG = "Y";//20161128 sy add
                job.MesProduct.SUBPRODUCTGRADES = body[keyHost.SUBPRODUCTGRADES].InnerText; //20170110 MengHui add
                job.CellSpecial.TFTIdLastChar = string.IsNullOrEmpty(job.MesProduct.ARRAYPRODUCTNAME) ? "B" : job.MesProduct.ARRAYPRODUCTNAME.Substring(job.MesProduct.ARRAYPRODUCTNAME.Length - 1, 1);

                
                
                #region
                //add item DisCardJudge by Scrap
                if (body[keyHost.DISCARDJUDGES] != null)
                {
                    if (body[keyHost.DISCARDJUDGES].InnerText != "X" & body[keyHost.DISCARDJUDGES].InnerText != "")//預設同機台預設值 就不報
                    {
                        string disCardJudges = new string('0', 32);
                        //0:A 25:Z 23:X 不管有沒有給X 要下給機台都要有X
                        disCardJudges = disCardJudges.Substring(0, 23) + "1" + disCardJudges.Substring(23 + 1, disCardJudges.Length - 1 - 23);
                        foreach (string disCardJudge in body[keyHost.DISCARDJUDGES].InnerText.Split(';'))
                        {
                            disCardJudgesUpdate(ref disCardJudges, disCardJudge);//updata EX: 00000110001110011
                        }
                        job.CellSpecial.DisCardJudges = disCardJudges;
                    }
                }
                //add MES item by zhuxingxing 20160819
                #endregion

                #region [EDC 需求資料]
                job.MesCstBody.LOTLIST.Add(new LOTc());
                job.MesCstBody.LOTLIST[0].LOTNAME = body[keyHost.LOTNAME].InnerText;//-----MES
                job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = body[keyHost.PRODUCTSPECNAME].InnerText;//-----MES
                job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME = body[keyHost.PROCESSOPERATIONNAME].InnerText;//-----MES 
                job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP = body[keyHost.PRODUCTSPECGROUP].InnerText;//-----MES 
                job.MesProduct.OWNERID = body[keyHost.OWNERID].InnerText;//-----MES
                #endregion
                #endregion

                job.MesCstBody.LOTLIST[0].GRADERANKGROUP = body[keyHost.GRADERANKGROUP].InnerText;//Add by Menghui 20161207 for Cell Filedata

                #region calculation product type
                List<string> items = new List<string>();
                List<string> idItems = new List<string>();
                switch (fabType)
                {
                    case eFabType.ARRAY:
                        break;
                    case eFabType.CF:
                        break;
                    #region [CELL]
                    case eFabType.CELL://MES SPEC 沒有這個PRODUCTOWNER 閰波 說用OWNERID
                        switch (line.Data.LINETYPE)
                        {
                            #region [RWK]
                            case eLineType.CELL.CCRWK://5/2
                                //T3 PI,  Product Type: ProductOwner,Oper ID, OwnerID,PFCD, GroupID
                                items.Add(keyHost.PRODUCTOWNER + "_" + body[keyHost.OWNERTYPE].InnerText);//20161206 huangjiayin: 取OwnerType..
                                items.Add(keyHost.PROCESSOPERATIONNAME + "_" + body[keyHost.PROCESSOPERATIONNAME].InnerText);
                                items.Add(keyHost.OWNERID + "_" + body[keyHost.OWNERID].InnerText);//------MES
                                items.Add(keyHost.PRODUCTSPECNAME + "_" + body[keyHost.PRODUCTSPECNAME].InnerText);//20161206 huangjiayin: add productspecname
                                //items.Add(keyHost.PRODUCTPROCESSTYPE + "_" + body[keyHost.PRODUCTPROCESSTYPE].InnerText);//------MES 20161206 huangjiayin: no need this item.
                                //if (body[keyHost.PRODUCTOWNER].InnerText == "E" && body[keyHost.OWNERID].InnerText == "RESD")//------MES
                                //    items.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);//------MES
                                if (!(string.IsNullOrEmpty(body[keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (body[keyHost.OWNERTYPE].InnerText.Substring(body[keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                   && body[keyHost.OWNERID].InnerText != "RESD")
                                        items.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);
                                }   
                                job.ProductType.SetItemData(items);
                                //T3 PI,  Product ID: ProductOwner,Oper ID, PFCD, GroupID
                                idItems.Add(keyHost.PRODUCTOWNER + "_" + body[keyHost.OWNERTYPE].InnerText);//20161206 huangjiayin: 取OwnerType..
                                idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + body[keyHost.PROCESSOPERATIONNAME].InnerText);
                                //idItems.Add(keyHost.PRODUCTPROCESSTYPE + "_" + body[keyHost.PRODUCTPROCESSTYPE].InnerText);//------MES 20161206 huangjiayin: no need this item.
                                //if (body[keyHost.PRODUCTOWNER].InnerText == "E" && body[keyHost.OWNERID].InnerText == "RESD")//------MES
                                //    idItems.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);//------MES
                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + body[keyHost.PRODUCTSPECNAME].InnerText);//20161206 huangjiayin: add productspecname

                                if (!(string.IsNullOrEmpty(body[keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (body[keyHost.OWNERTYPE].InnerText.Substring(body[keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                   && body[keyHost.OWNERID].InnerText != "RESD")
                                        idItems.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);//20161206 huangjiayin: fix items-->idItems
                                }   
                                job.ProductID.SetItemData(idItems);
                                break;
                            #endregion
                            default:
                                
                                    //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                    items.Add(keyHost.PRODUCTOWNER + "_" + body[keyHost.OWNERTYPE].InnerText);//20161206 huangjiayin:OwnerID-->OwnerType
                                    items.Add(keyHost.OWNERID + "_" + body[keyHost.OWNERID].InnerText);
                                    items.Add(keyHost.PRODUCTSPECNAME + "_" + body[keyHost.PRODUCTSPECNAME].InnerText);
                                    if (!(string.IsNullOrEmpty(body[keyHost.OWNERTYPE].InnerText.Trim())))
                                    {
                                        if (body[keyHost.OWNERTYPE].InnerText.Substring(body[keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                            && body[keyHost.OWNERID].InnerText != "RESD")
                                            items.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);
                                    }
                                    job.ProductType.SetItemData(items);
                                    //Product ID: ProductOwner,PFCD, GroupID
                                    idItems.Add(keyHost.PRODUCTOWNER + "_" + body[keyHost.OWNERTYPE].InnerText);//20161206 huangjiayin:OwnerID-->OwnerType
                                    //idItems.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                    idItems.Add(keyHost.PRODUCTSPECNAME + "_" + body[keyHost.PRODUCTSPECNAME].InnerText);
                                    if (!(string.IsNullOrEmpty(body[keyHost.OWNERTYPE].InnerText.Trim())))
                                    {
                                        if (body[keyHost.OWNERTYPE].InnerText.Substring(body[keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                            && body[keyHost.OWNERID].InnerText != "RESD")
                                            idItems.Add(keyHost.GROUPID + "_" + body[keyHost.GROUPID].InnerText);
                                    }
                                    job.ProductID.SetItemData(idItems);

                                break;
                        }
                        break;
                    #endregion
                    default:
                        break;
                }


                if (!ObjectManager.JobManager.GetProductType(fabType, body[keyHost.OWNERTYPE].InnerText, mesJobs, job, out err))//------MES
                {
                    throw new Exception(string.Format("CAN NOT FIND ProductType LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                }
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    Port port = null;
                    if (!ObjectManager.JobManager.GetProductID(port, mesJobs, job, out err))
                    {
                        throw new Exception(string.Format("CAN NOT FIND ProductID LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                    }
                }

                #endregion

                #region CELL Recipe Data
                if (fabType == eFabType.CELL)
                {
                    if (!ObjectManager.JobManager.AnalysisMesPPID_CELLPanel(body, line, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                    {
                        throw new Exception(string.Format("CAN NOT FIND Recipe LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                    }
                    job.PPID = ppid;
                    job.MES_PPID = mesppid;
                }
                #endregion

                if (ParameterManager[eCELL_SWITCH.PANEL_RECIPE_ID_CHECK].GetBoolean())//sy add 20160906
                {
                    #region Check Recipe ID
                    if (((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_ID].GetBoolean()) ||
                        (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_ID].GetBoolean())) &&
                        ((line.File.PanelInformationReplyLastPPID != mesppid) ||//PPID发生变化的Check
                        (!line.File.PanelInformationReplyLastRecipeIDCheckResult) ||//上一次CheckNG的Check
                        (line.File.PanelInformationReplyLastTime.AddHours(6) < DateTime.Now)))//超过6hr未投入的Check
                    {

                        if (idCheckInfos.Count() > 0)
                        {
                            if (recipeIDCheckData.ContainsKey(line.Data.LINEID))
                            {
                                List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;
                                for (int i = 0; i < idCheckInfos.Count(); i++)
                                {
                                    // 過濾掉重覆的部份
                                    if (rci.Any(r => r.EQPNo == idCheckInfos[i].EQPNo && r.RecipeID == idCheckInfos[i].RecipeID)) continue;
                                    rci.AddRange(idCheckInfos);
                                }
                            }
                            else
                            {
                                recipeIDCheckData.Add(line.Data.LINEID, idCheckInfos);
                            }
                        }

                        // Check Recipe ID 
                        if (recipeIDCheckData.Count() > 0)
                        {
                            Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxID, recipeIDCheckData, new List<string>() });

                            //檢查完畢, 返回值
                            bool _NGResult = false;
                            foreach (string key in recipeIDCheckData.Keys)
                            {
                                IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                string log = string.Format("Line Name=[{0}) Recipe ID Check", key);
                                string log2 = string.Empty;

                                for (int i = 0; i < recipeIDList.Count; i++)
                                {
                                    if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                        recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                    {
                                        log2 += string.Format(", EQID=[{0}({1})] RecipeID=[{2}] Result=[{3}]", recipeIDList[i].EqpID, recipeIDList[i].EQPNo, recipeIDList[i].RecipeID, recipeIDList[i].Result.ToString());
                                        _NGResult = true;
                                    }
                                }
                                if (!string.IsNullOrEmpty(log2))
                                {
                                    err = string.IsNullOrEmpty(err) ? "" : err += ";";
                                    err += log + log2;
                                }
                            }
                            line.File.PanelInformationReplyLastPPID = mesppid;
                            line.File.PanelInformationReplyLastRecipeIDCheckResult = !_NGResult;
                            line.File.PanelInformationReplyLastTime = DateTime.Now;
                            ObjectManager.LineManager.EnqueueSave(line.File);
                            if (_NGResult)
                            {
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("CAN NOT FIND Recipe ID LINE_ID=[{0}] IN LINE OBJECT! ERROR LOG = {1}", lineID, err));//sy add 20160906
                                //20171120 by huangjiayin POL OHV SpecialEvent
                                if (machineName.Contains("CCOHV"))
                                {
                                    Invoke(eServiceName.VCRService, "VCRMismatchJobDataRequestReply", new object[] { line, eqp.Data.NODENO, eBitResult.ON, eReturnCode1.NG, null, trxID });
                                }
                                else
                                {
                                    Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, commandNo });
                                }

                                if (line.Data.LINETYPE == eLineType.CELL.CCRWK || line.Data.LINETYPE == eLineType.CELL.CCPCS) return;//20170411 huangjiayin: 师令说，RWK不需要BC发消息
                                //20161215 add by huangjiayin: fix err length if>80 to 80
                                string cim_msg = err.Trim();
                                while (cim_msg.Length > 80)
                                {
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg.Substring(0, 80), "", "1" });
                                    cim_msg = cim_msg.Substring(80, cim_msg.Length - 80);

                                }
                                if (cim_msg.Length > 0)
                                {
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg, "", "1" });//sy add 20160906
                                }

                                return;
                                //throw new Exception(string.Format("CAN NOT FIND Recipe ID LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                            }

                        }
                    }
                    line.File.PanelInformationReplyLastTime = DateTime.Now;
                    ObjectManager.LineManager.EnqueueSave(line.File);

                    #endregion
                }
                if (ParameterManager[eCELL_SWITCH.PANEL_RECIPE_PARA_CHECK].GetBoolean())//sy add 20160906
                {
                    #region Check Parameter
                    if (recipePara)
                    {
                        if ((line.File.HostMode == eHostMode.REMOTE && ParameterManager[eREPORT_SWITCH.REMOTE_RECIPE_PARAMETER].GetBoolean()) ||
                            (line.File.HostMode == eHostMode.LOCAL && ParameterManager[eREPORT_SWITCH.LOCAL_RECIPE_PARAMETER].GetBoolean()))
                        {
                            XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;//----MES
                            for (int i = 0; i < recipeNoCheckList.Count; i++)
                            {
                                lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                            }
                            //Watson 20141126 DB Setting Recipe Parameter Check Enable/Disable 在RecipeService :RecipeParameterRequestCommand裏加入

                            if (paraCheckInfos.Count() > 0)
                            {
                                if (recipeParaCheckData.ContainsKey(line.Data.LINEID))
                                {
                                    List<RecipeCheckInfo> rcp = recipeParaCheckData[line.Data.LINEID] as List<RecipeCheckInfo>;

                                    for (int i = 0; i < paraCheckInfos.Count(); i++)
                                    {
                                        // 過濾掉重覆的部份
                                        if (rcp.Any(r => r.EQPNo == paraCheckInfos[i].EQPNo && r.RecipeID == paraCheckInfos[i].RecipeID)) continue;
                                        rcp.AddRange(paraCheckInfos);
                                    }
                                    rcp.AddRange(paraCheckInfos);
                                }
                                else
                                {
                                    recipeParaCheckData.Add(line.Data.LINEID, paraCheckInfos);
                                }
                            }

                            if (recipeParaCheckData.Count > 0)
                            {
                                object[] obj = new object[]
                            {
                                trxID,
                                recipeParaCheckData,
                                lstNoCehckEQ,
                                line,
                                //port.Data.PORTID,
                                //port.File.CassetteID,
                            };
                                eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                                //檢查完畢, 返回值
                                if (retCode != eRecipeCheckResult.OK)
                                {
                                    err = "[MES] Check Recipe Parameter Reply NG: ";
                                    foreach (string key in recipeParaCheckData.Keys)
                                    {
                                        IList<RecipeCheckInfo> recipeParaList = recipeParaCheckData[key];
                                        string log = string.Format("Line Name=[{0}) Recipe Parameter Check", key);
                                        string log2 = string.Empty;

                                        for (int i = 0; i < recipeParaList.Count; i++)
                                        {
                                            if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                                recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                            {
                                                log2 += string.Format(", EQID=[{0}({1})], Result=[{2}]", recipeParaList[i].EqpID, recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(log2))
                                        {
                                            //err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                            err += log + log2;
                                        }
                                    }
                                    //Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",

                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("CAN NOT FIND Recipe ID LINE_ID=[{0}] IN LINE OBJECT! ERROR LOG = {1}", lineID, err));//sy add 20160906
                                    //20171120 by huangjiayin POL OHV SpecialEvent
                                    if (machineName.Contains("CCOHV"))
                                    {
                                        Invoke(eServiceName.VCRService, "VCRMismatchJobDataRequestReply", new object[] { line, eqp.Data.NODENO, eBitResult.ON, eReturnCode1.NG, null, trxID });
                                    }
                                    else
                                    {
                                        Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, commandNo });
                                    }
                                    //    string.Format("MES Recipe Para Replay NG LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                                    if (line.Data.LINETYPE == eLineType.CELL.CCRWK || line.Data.LINETYPE == eLineType.CELL.CCPCS) return;//20170411 huangjiayin: 师令说，RWK不需要BC发消息
                                    //20161215 add by huangjiayin: fix err length if>80 to 80
                                    string cim_msg = err.Trim();
                                    while (cim_msg.Length > 80)
                                    {
                                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg.Substring(0, 80), "", "1" });
                                        cim_msg = cim_msg.Substring(80, cim_msg.Length - 80);
                                    }
                                    if (cim_msg.Length > 0)
                                    {
                                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, cim_msg, "", "1" });//sy add 20160906
                                    }
                                    return;
                                    //throw new Exception(string.Format("CAN NOT FIND Recipe Para LINE_ID=[{0}] IN LINE OBJECT!", lineID));
                                }
                            }
                        }
                    }
                    #endregion
                }
                M2P_PanelInformationSpecialDataBy_CELL(body, line, ref  job);

                if (pnlJPSCode.Length > 0 && !string.IsNullOrEmpty(pnlJPSCode[0].ToString())) job.CellSpecial.AbnormalTFT = pnlJPSCode[0].ToString();
                if (pnlJPSCode.Length > 1) job.CellSpecial.LcdQtapLotGroupID = pnlJPSCode[1].ToString();

                #region Create FTP File
                string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, cstNo);
                FileFormatManager.DeleteFileByTime("CELLShopFEOL", subPath);//sy add 20160824 修改 先將超過10天生成的刪除
                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, job });                 
                #endregion


                if (mesJobs.Count() > 0)
                    ObjectManager.JobManager.AddJobs(mesJobs);

                PanelProcessStarted(trxID, line, job, productSpecName, productOwner);
                //Add BY Yangzhenteng20180809 For BEOL CUT BCN
                #region[For CUT BCN/NLS/NRD Panel Recovery] 
                if (returnCode == "0" && line.Data.LINEID.Contains("CCCUT"))
                {
                    if ((eqp.Data.NODEID.Contains("CCBCN") && eqp.Data.NODENO == "L5") ||
                        ((eqp.Data.NODEID.Contains("CCTST") && eqp.Data.NODENO == "L7") && (line.Data.LINEID.Contains("CCCUTC00") || line.Data.LINEID.Contains("CCCUTD00")))||
                        ((eqp.Data.NODEID.Contains("CCTLD") && eqp.Data.NODENO == "L9") && (line.Data.LINEID.Contains("CCCUTK00") || line.Data.LINEID.Contains("CCCUT100") || line.Data.LINEID.Contains("CCCUT200") || line.Data.LINEID.Contains("CCCUT800") || line.Data.LINEID.Contains("CCCUTB00"))))
                    {
                        string _unitno = string.Empty;
                        string _portno = string.Empty;
                        string _slotno = string.Empty;
                        ObjectManager.JobManager.EnqueueSave(job);
                        ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, _unitno, _portno, _slotno, eJobEvent.Recovery.ToString(), trxID);
                        Thread.Sleep(100);
                        object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[] { trxID, eqp.Data.LINEID, eqp, job, "" });
                    }                    
                }
                else if (returnCode == "0" && line.Data.LINEID.Contains("CCNLS"))
                {
                    if (eqp.Data.NODEID.Contains("CCNLS") && eqp.Data.NODENO == "L2")
                    {
                        string _unitno = string.Empty;
                        string _portno = string.Empty;
                        string _slotno = string.Empty;
                        ObjectManager.JobManager.EnqueueSave(job);
                        ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, _unitno, _portno, _slotno, eJobEvent.Recovery.ToString(), trxID);
                        Thread.Sleep(50);
                        object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[] { trxID, eqp.Data.LINEID, eqp, job, "" });
                    }            
                }
                else if (returnCode == "0" && line.Data.LINEID.Contains("CCNRD"))
                {
                    if (eqp.Data.NODEID.Contains("CCNRD") && eqp.Data.NODENO == "L2")
                    {
                        string _unitno = string.Empty;
                        string _portno = string.Empty;
                        string _slotno = string.Empty;
                        ObjectManager.JobManager.EnqueueSave(job);
                        ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, _unitno, _portno, _slotno, eJobEvent.Recovery.ToString(), trxID);
                        Thread.Sleep(50);
                        object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[] { trxID, eqp.Data.LINEID, eqp, job, "" });
                    }                   
                }
                else if (returnCode == "0" && line.Data.LINEID.Contains("CCPOL"))
                {
                    if (eqp.Data.NODEID.Contains("CCLOI") && eqp.Data.NODENO == "L7")
                    {
                        string _unitno = string.Empty;
                        string _portno = string.Empty;
                        string _slotno = string.Empty;
                        ObjectManager.JobManager.EnqueueSave(job);
                        ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, _unitno, _portno, _slotno, eJobEvent.Recovery.ToString(), trxID);
                        Thread.Sleep(50);
                        object retVal = base.Invoke(eServiceName.MESService, "ProductUnscrapped", new object[] { trxID, eqp.Data.LINEID, eqp, job, "" });
                    }
                }
                else
                { }
                #endregion
                //Invoke(eServiceName.JobService, "JobDataRequestReportReplyForPanelInformation", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, job });
                if (machineName.Contains("CCOHV")&&!line.Data.LINEID.Contains("CCPOL400"))
                {
                    Invoke(eServiceName.VCRService, "VCRMismatchJobDataRequestReply", new object[] {commandNo, line, eqp.Data.NODENO, eBitResult.ON, eReturnCode1.OK, job, trxID });
                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                else
                {
                    Invoke(eServiceName.JobService, "JobDataRequestReportReplyForPanelInformation", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID, job, commandNo });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PanelInformationReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                //string keyCommandNo = keyBoxReplyPLCKey.PanelRequestReplyCommandNo;     //add by zhuxingxing 20160906 //sy modify 20160911
                string commandNo = tmp.Split('#')[1];//sy modify 20160911


                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  MACHINENAME={1}  Panel Information MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_PanelInformationReply#{1}", sArray[0], commandNo);//sy modify 20160911                 
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                Invoke(eServiceName.JobService, "JobDataRequestReportReply", new object[] { sArray[0], eBitResult.ON, trackKey, commandNo });

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void M2P_PanelInformationSpecialDataBy_CELL(XmlNode body, Line line, ref Job job)
        {
            job.INSPReservations = new string('0', 6);
            job.EQPReservations = new string('0', 6);
            job.InspJudgedData = new string('0', 32);
            job.TrackingData = new string('0', 32);
            job.EQPFlag = new string('0', 32);

            XmlNodeList abnormalcodelist = body[keyHost.ABNORMALCODELIST].ChildNodes;
            string beveledFlag = string.Empty;  //Job Data
            string chippigFlag = string.Empty;  //Job Data
            string cutSlimReworkFlag = string.Empty;  //Job Data
            string CellCutRejudgeCount = string.Empty;  //Job Data
            string CFSideResidueFlag = string.Empty;  //Job Data
            string lineReworkCount = string.Empty;//Job Data
            string LOIFlag = string.Empty;//Job Data
            string mgvFlag = string.Empty;  //Job Data
            string pitype = string.Empty;  //Job Data
            string pointReworkCount = string.Empty;//Job Data
            string ribMarkFlag = string.Empty;  //Job Data
            string sealAbnormalFlag = string.Empty;  //Job Data
            string turnAngle = string.Empty;
            string rwTimeflag = string.Empty;    // add rwtimeFlag JobData by zhuxingxing  20160825
            string abnormalTFT = string.Empty;//20171124 huangjiayin
            #region [ABNORMALVALUE]
            foreach (XmlNode abnormalcode in abnormalcodelist)
            {
                string abnormalVal = abnormalcode[keyHost.ABNORMALVALUE].InnerText.Trim();
                switch (abnormalVal)
                {
                    case "BEVELEFLAG": beveledFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "CUTSLIMREWORKFLAG": cutSlimReworkFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "CHIPPINGFLAG": chippigFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "CELLCUTREJUDGECOUNT": CellCutRejudgeCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "CFSIDERESIDUEFLAG": CFSideResidueFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "GLASSCHANGEANGLE": turnAngle = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "LOIFLAG": LOIFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "LINEREWORKCOUNT": lineReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "POINTREWORKCOUNT": pointReworkCount = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "RIBMARKFLAG": ribMarkFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "SEALABNORMALFLAG": sealAbnormalFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    case "POLREWORKLIFETIME": rwTimeflag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;      // add by zhuxingxing 取AbnormalCode中的RWtime值;     
                    case "ABNORMALTFT": abnormalTFT = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break; //20171124 huangjiayin add
                }
            }
            #endregion

            int networkNo = 1;
            IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
            job.CellSpecial.ProductID = job.ProductID.Value.ToString();
            job.CellSpecial.CassetteSettingCode = body[keyHost.PRDCARRIERSETCODE].InnerText; ;
            job.CellSpecial.ControlMode = line.File.HostMode;
            job.CellSpecial.AbnormalTFT = abnormalTFT;//20171124 huangjiayin add
            switch (line.Data.JOBDATALINETYPE)
            {
                #region [CCCUT]
                case eJobDataLineType.CELL.CCCUT:
                    job.CellSpecial.ControlMode = line.File.HostMode;
                    if (turnAngle == "") //MES閻波說如果MES給的值是空的話turnAngle就固定給"2" by tom.su 20160613
                    {
                        job.CellSpecial.TurnAngle = "2";
                    }
                    else
                    {
                        job.CellSpecial.TurnAngle = CellTurnAngle(turnAngle);
                    }
                    
                    ////job.CellSpecial.CutLayout = ??//Panel cutcomplete no need
                    ////job.CellSpecial.CutPoint = ??//Panel cutcomplete no need
                    ////job.CellSpecial.CutSubProductSpecs = ??//Panel cutcomplete no need
                    job.CellSpecial.PanelOXInformation = job.OXRInformation;

                    double lotPnlSize = 0;
                    if (double.TryParse(body[keyHost.PRODUCTSIZE].InnerText, out lotPnlSize))//Panel cutcomplete no need 
                    {
                        lotPnlSize = lotPnlSize * 100;
                    }
                    job.CellSpecial.PanelSize = lotPnlSize.ToString();//Panel cutcomplete MES down
                    job.CellSpecial.DefectCode = body[keyHost.PRODUCTDEFECTCODE].InnerText;
                    job.CellSpecial.RejudgeCount = CellCutRejudgeCount == string.Empty ? "0" : CellCutRejudgeCount;
                    job.CellSpecial.VendorName = body[keyHost.VENDORNAME].InnerText;

                    if (sub == null) return;
                    if (sub.ContainsKey("SealErrorFlag")) sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";//SealErrorFlag: From BlockCut to CUT
                    if (sub.ContainsKey("ChippigFlag")) sub["ChippigFlag"] = chippigFlag.Trim() == "" ? "0" : "1";//ChippingFlag: From BlockCut to CUT
                    if (sub.ContainsKey("BeveledFlag")) sub["BeveledFlag"] = beveledFlag.Trim() == "" ? "0" : "1";//BeveledFlag: From CUT to CUT
                    if (sub.ContainsKey("CFSideResidueFlag")) sub["CFSideResidueFlag"] = CFSideResidueFlag.Trim() == "" ? "0" : "1";//CFSideResidueFlag: From CUT 
                    if (sub.ContainsKey("RibMarkFlag")) sub["RibMarkFlag"] = ribMarkFlag.Trim() == "" ? "0" : "1";//RibMarkFlag: From CUT
                    if (sub.ContainsKey("LOIFlag")) sub["LOIFlag"] = LOIFlag.Trim() == "" ? "0" : "1";//LOIFlag: From CUT to (CUT and RWT)
                    if (sub.ContainsKey("CutSlimReworkFlag")) sub["CutSlimReworkFlag"] = cutSlimReworkFlag.Trim() == "" ? "0" : "1";//CutSlimReworkFlag: From CUT to CUT
                    job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                    break;
                #endregion
                #region [CCRWK]
                case eJobDataLineType.CELL.CCRWK:
                    job.CellSpecial.ControlMode = line.File.HostMode;
                    // add by zhuxingxing 20160825 判断PolRWLifeTime 的值是否为空;
                    if (sub == null)
                    {
                        return;
                    }
                    else 
                    {
                        if (sub.ContainsKey("POLREWORKLIFETIME"))
                        {
                            if (rwTimeflag == "" || rwTimeflag == "0")
                            {
                                sub["POLREWORKLIFETIME"] = "0";
                                job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                            }
                            else
                            {
                                sub["POLREWORKLIFETIME"] = "1";
                                job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                            }
                            job.CellSpecial.RwLiftTime = rwTimeflag;
                        }
                    }
                    //end 
                    break;
                #endregion
            }
        }

        public void PanelProcessStarted(string trxID, Line line, Job job, string productSpecName, string productOwner)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                //bodyNode[keyHost.PORTNAME].InnerText = string.Empty;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = productSpecName;
                bodyNode[keyHost.PRODUCTOWNER].InnerText = productOwner;
                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.LineRecipeName;
                bodyNode[keyHost.PPID].InnerText = job.PPID;
                bodyNode[keyHost.HOSTPPID].InnerText = job.MES_PPID;


                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] PanelProcessStarted OK LINENAME=[{1}].",
                    trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.102.	OutBoxProcessEnd        MES MessageSet : BC reports when processing of Lot in EQP was completed and all substrates of the InBox have been stored in to OutBox for moving out of the EQP.
        /// Add by marine for T3 MES 2015/9/16
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="portID"></param>
        /// <param name="cst"></param>
        /// <param name="jobList"></param>
        public void OutBoxProcessEnd(string trxID, string lineID, string portID, Cassette cst)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] OutBoxProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                Port port = ObjectManager.PortManager.GetPortByLineIDPortID(lineID, portID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("OutBoxProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;

                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                XmlNode boxNodeClone = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();//把原格式中的BOXLIST移除

                XmlNode boxNode = boxNodeClone.Clone();
                boxNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                //boxNode[keyHost.PRODUCTQUANTITY].InnerText = jobList.Count.ToString();
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = "";
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName; ;
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LINERECIPENAME;

                bodyNode[keyHost.OUTBOXNAME].InnerText = cst.SubBoxID;

                boxListNode.AppendChild(boxNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] OutBoxProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                            trxID, line.Data.LINEID, portID, cst.LineRecipeName, cst.MES_CstData.LINERECIPENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.103.	OutBoxProcessEndReply   MES MessageSet : MES acknowledge the processing end of the OutBox
        /// Add by marine for T3 MES 2015/9/22
        /// </summary>
        /// <param name="xmldoc"></param>
        public void MES_OutBoxProcessEndReply(XmlDocument xmldoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmldoc);
                string returnMessage = GetMESReturnMessage(xmldoc);
                string lineName = GetLineName(xmldoc);
                string trxID = GetTransactionID(xmldoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string portName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string lineRecipeName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string hostLineRecipeName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.HOSTLINERECIPENAME].InnerText;
                string outBoxName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OUTBOXNAME].InnerText;
                string boxQuantity = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;

                List<string> boxlist = new List<string>();

                XmlNodeList boxListNode = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST].ChildNodes;

                foreach (XmlNode boxnode in boxListNode)
                {
                    if (boxnode != null)
                    {
                        string box = boxnode.InnerText;
                        boxlist.Add(box);
                    }
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_OutBoxProcessEndReply  NG LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxID, lineName, boxQuantity, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_OutBoxProcessEndReply  OK LINENAME=[{1}],BOXQUANTITY=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                        trxID, lineName, boxQuantity, returnCode, returnMessage));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.104.	PalletIdCreateRequest       MES MessageSet : BC requests first Box into a Pallet. MES should create a PalletId based on Box information
        /// Add by marine for t3 MES 2015/9/15
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="boxName"></param>
        public void PalletIdCreateRequest(string trxID, string lineName, string boxName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xmldoc = agent.GetTransactionFormat("PalletIdCreateRequest") as XmlDocument;
                SetTransactionID(xmldoc, trxID);
                XmlNode bodyNode = xmldoc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.BOXNAME].InnerText = boxName;

                SendToMES(xmldoc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                    trxID, lineName));
                #region MES PalletIdCreateRequest Timeout
                string timeoutName = string.Format("{0}_MES_PalletIdCreateRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(LotIdCreateRequestT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.105.	PalletIdCreateReply     MES MessageSet : MES replies PalletId information
        /// Add by marine for T3 MES 2015/9/16
        /// </summary>
        /// <param name="xmldoc"></param>
        public void MES_PalletIdCreateReply(XmlDocument xmldoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmldoc);
                string returnMessage = GetMESReturnMessage(xmldoc);
                string lineName = GetLineName(xmldoc);
                string trxID = GetTransactionID(xmldoc);

                if (!CheckMESLineID(lineName))
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));

                eReturnCode1 rtcode = returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG;
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxDataRequestReply", trxID));
                    return;
                }
                #endregion
                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_PalletIdCreateRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                string lotName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME] == null ? "" : xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;
                #region [Reply NG]
                if (rtcode == eReturnCode1.NG)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PalletIdCreateReply NG LINENAME =[{1}],PALLETNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, lotName, returnCode, returnMessage));
                    Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, lotName, "0" });
                    return;
                }
                #endregion

                #region Get and Remove Reply Key PalletNo
                string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                string PalletNo = Repository.Remove(Portkey).ToString();
                if (PalletNo == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,PORT NO is Null.Don't Reply PLC_LotIDCreateRequestReportReply", trxID));
                    return;
                }
                #endregion
                //[MES 20160530 修改邏輯 同QPP] //20160606 因現場Pallet 沒有ID 與MES 提出不同 修正回來 
                if (lineName.Contains(keyCellLineType.PPK))
                {
                    Pallet pallet = new Pallet(new PalletEntityFile());
                    pallet.File.PalletMode = ePalletMode.PACK;
                    pallet.File.PalletID = lotName.Trim();//PPK 只有用PalletName//sy end 在不上報
                    pallet.File.PalletNo = PalletNo;
                    pallet.File.NodeNo = eqpID;
                    pallet.File.PalletName = lotName;
                    ObjectManager.PalletManager.AddPallet(pallet);
                }
                else
                {
                    #region [UpDate Pallet]
                    if (PalletNo.PadLeft(2, '0') != "00")
                    {
                        Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(PalletNo.PadLeft(2, '0'));
                        if (pallet == null)
                        {
                            Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("EQUIPMENT=[{0}] CAN NOT FIND PalletNo=[{1}] IN Pallet OBJECT!", eqpID, PalletNo));
                            return;
                        }
                        pallet.File.PalletName = lotName;
                        ObjectManager.PalletManager.EnqueueSave(pallet.File);
                    }
                    #endregion
                }

                string capacity = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CAPACITY].InnerText;
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PalletIdCreateReply  OK LINENAME =[{1}],PALLETNAME =[{2}],CAPACITY =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                    trxID, lineName, lotName, capacity, returnCode, returnMessage));

                Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxID, rtcode, lotName, capacity });
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.103.	PalletLabelInformationRequest       MES MessageSet : Pallet Label Info Request to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        public void PalletLabelInformationRequest(string trxID, string lineName, string palletteID, string eqpNoforTimeout)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));


                    #region Get and Remove Reply Key
                    string eqpNo = string.Empty;
                    string key = keyBoxReplyPLCKey.PalletLabelInformationRequestReply;
                    object eqp = Repository.Remove(key);
                    if (eqp == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletLabelInformationRequestReply", trxID));
                        return;
                    }
                    eqpNo = eqp.ToString();
                    #endregion

                    string timeoutName1 = string.Format("{0}_MES_PalletLabelInformationReply", eqpNo);
                    if (_timerManager.IsAliveTimer(timeoutName1))
                        _timerManager.TerminateTimer(timeoutName1);

                    #region PLC Write  Data Pallet Label Information Request Reply
                    //PalletLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey,object reply)
                    // returnCode(INT) ; palletID ; modelName ; modelVersion; cartonQuantity(INT) = 0 ;
                    //"returnCode:Always 1:OK  or 2:NG"

                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.PalletService, "PalletLabelInformationRequestReply", new object[]
                        { 
                            eqpNo,eBitResult.ON,trxID,
                            string.Format("{0},{1},{2},{3},{4}",((int)eReturnCode1.OK).ToString() ,palletteID,string.Empty,string.Empty,"0")
                        });
                    else
                        Invoke(eServiceName.PalletService, "PalletLabelInformationRequestReply", new object[]
                        { 
                            eqpNo,eBitResult.ON,trxID,
                            string.Format("{0},{1},{2},{3},{4}",((int)eReturnCode1.NG).ToString() ,palletteID,string.Empty,string.Empty,"0")
                        });
                    #endregion
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PalletLabelInformationRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PALLETNAME].InnerText = palletteID;

                SendToMES(xml_doc);

                #region MES Pallet Label Infomation Request Timeout
                string timeoutName = string.Format("{0}_MES_PalletLabelInformationReply", eqpNoforTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PalletLabelInformationReply_Timeout), trxID);
                #endregion

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
        /// 6.104.	PalletLabelInformationReply     MES MessagetSet : Pallete Label Infomation Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_PalletLabelInformationReply(XmlDocument xmlDoc)
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
                //string palletid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;
                //string modelname = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME].InnerText;
                //string modelver = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION].InnerText;

                //string bomversion = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION].InnerText;

                //string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                //string panelqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY].InnerText;
                //string shipid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID].InnerText;
                //string weekcode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WEEKCODE].InnerText;
                //MES T3 SPEC 1.34 結構大改 sy add 20160120
                string shipIdName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.NAME].InnerText;
                string shipIdValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SHIPID][keyHost.VALUE].InnerText;
                string bomVersionName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.NAME].InnerText;
                string bomVersionValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION][keyHost.VALUE].InnerText;
                string modelName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.NAME].InnerText;
                string modelValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME][keyHost.VALUE].InnerText;
                string modelVersionName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.NAME].InnerText;
                string modelVersionValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION][keyHost.VALUE].InnerText;
                string boxQuantityName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY][keyHost.NAME].InnerText;
                string boxQuantityValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY][keyHost.VALUE].InnerText;
                string productQuantityName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY][keyHost.NAME].InnerText;
                string productQuantityValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTQUANTITY][keyHost.VALUE].InnerText;
                string weekCodeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WEEKCODE][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WEEKCODE][keyHost.NAME].InnerText;
                string weekCodeValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WEEKCODE][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WEEKCODE][keyHost.VALUE].InnerText;
                string environmenFlagName = string.Empty;
                string environmenFlagValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ENVIRONMENTFLAG] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ENVIRONMENTFLAG].InnerText;
                string noteName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.NAME].InnerText;
                string noteValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NOTE][keyHost.VALUE].InnerText;
                string countryName = string.Empty;
                string countryValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.COUNTRY] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.COUNTRY].InnerText;
                string carrierNameName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.NAME].InnerText;
                string carrierNameValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME][keyHost.VALUE].InnerText;
                string wtName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.NAME] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.NAME].InnerText;
                string wtValue = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.VALUE] == null ? "" : xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WT][keyHost.VALUE].InnerText;
                int plcReplyCode = 0;
                eReturnCode1 plcReturnCode = eReturnCode1.Unknown;
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PalletLabelInformationReply NG LINENAME =[{1}],BOXQUANTITY =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, boxQuantityValue, returnCode, returnMessage));
                    //to do?
                    plcReplyCode = 2;  //"1：OK ;2：NG"
                    plcReturnCode = eReturnCode1.NG;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PalletLabelInformationReply  OK LINENAME =[{1}],BOXQUANTITY =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, boxQuantityValue, returnCode, returnMessage));
                    //to do?
                    plcReplyCode = 1;  //"1：OK ;2：NG"
                    plcReturnCode = eReturnCode1.OK;
                }

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.PalletLabelInformationRequestReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletLabelInformationRequestReply", trxID));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion

                string timeoutName = string.Format("{0}_MES_PalletLabelInformationReply", eqpNo);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK || line.Data.LINETYPE == eLineType.CELL.CCQPP)
                {
                    #region [LabelInformationforPalletRequestReply]
                    object[] _data = new object[28]
                { 
                    eqpNo ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trxID,                           /*2 trackKey */
                    plcReturnCode,         /*3 returnCode*/
                    shipIdName,                    /*4  SHIPID Name */
                    shipIdValue,              /*5 SHIPID Value */
                    bomVersionName,                    /*6  BOMVERSION */
                    bomVersionValue,              /*7 BOMVERSION*/
                    modelName,                    /*8  MODELNAME */
                    modelValue,              /*9 MODELNAME*/
                    modelVersionName,                    /*10  MODELVERSION --*/
                    modelVersionValue,              /*11 MODELVERSION*/
                    boxQuantityName,                    /*12  BOXQUANTITY */
                    boxQuantityValue,              /*13 BOXQUANTITY*/
                    productQuantityName,                    /*14  PRODUCTQUANTITY */
                    productQuantityValue,              /*15 PRODUCTQUANTITY*/
                    weekCodeName,                    /*16 WEEKCODE */
                    weekCodeValue,              /*17 WEEKCODE*/
                    environmenFlagName,                    /*18  ENVIRONMENTFLAG */
                    environmenFlagValue,              /*19 ENVIRONMENTFLAG*/
                    noteName,                    /*20  NOTE */
                    noteValue,              /*21 NOTE*/
                    countryName,                    /*22  COUNTRY */
                    countryValue,              /*23 COUNTRY*/
                    carrierNameName,                    /*24  CARRIERNAME */
                    carrierNameValue,             /*25 CARRIERNAME*/
                    wtName,                    /*26  WT */
                    wtValue              /*27 WT*/
                };
                    Invoke(eServiceName.PalletService, "LabelInformationforPalletRequestReply", _data);
                    #endregion
                }
                else
                {
                    //T3 沒用到 結構大改 Mark
                    #region PLC Write  Data Pallet Label Information Request Reply
                    //    object[] _data = new object[4]
                    //{ 
                    //    eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                    //    eBitResult.ON,    /*1 ON*/
                    //    trxID,                          /*trx id */
                    //    string.Format("{0},{1},{2},{3},{4}",plcReplyCode.ToString() ,"",modelname,modelver,boxqty)
                    //                              //returnCode;palletID;modelName;modelVersion;cartonQuantity
                    //};
                    //    //Send MES Data
                    //    //PalletLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey,object reply)
                    //    base.Invoke(eServiceName.PalletService, "PalletLabelInformationRequestReply", _data);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PalletLabelInformationReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  EQPID={1}  Pallet Label Information MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_PalletLabelInformationReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                #region Get and Remove Reply Key
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.PalletLabelInformationRequestReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletLabelInformationRequestReply", trackKey));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion

                object[] _data = new object[4]
                { 
                    eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trackKey,                          /*trx id */
                    string.Format("{0},{1},{2},{3},{4}",eReturnCode2.NG.ToString()  ,string.Empty,string.Empty,string.Empty,string.Empty)
                };
                base.Invoke(eServiceName.PalletService, "PalletLabelInformationRequestReply", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.105.	PalletProcessCanceled       MES MessageSet : Pallet Process Cancel Report to MES (DPPK)
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        /// <param name="portID">Port ID</param>
        public void PalletProcessCanceled(string trxID, string lineName, string palletteID, string portID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PalletProcessCanceled") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PALLETNAME].InnerText = palletteID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;

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
        ///  6.106.	PalletProcessEnd        MES MessageSet : Pallete Process End Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        /// <param name="boxQty">Box Count</param>
        /// <param name="pPID">Line Recipe ID</param>
        /// <param name="boxList">Box List</param>
        public void PalletProcessEnd(string trxID, string lineName, string palletteID, string carrierName, string pPID, List<string> boxList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PalletProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //bodyNode[keyHost.PORTMODE].InnerText = "PACK";  //Hot code: [ Boxing]  Used in both PACK, UNPACK mode.[ Palletizing ] Used in PACK mode only.
                bodyNode[keyHost.LINERECIPENAME].InnerText = pPID;
                Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletteID);//Add by sy for T3 MES 2015/11/25
                if (pallet == null)
                {
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format(" CAN NOT FIND Pallet_ID=[{0}] IN Pallet OBJECT!", palletteID));
                    bodyNode[keyHost.PALLETNAME].InnerText = string.Empty;
                }
                else
                {
                    bodyNode[keyHost.PALLETNAME].InnerText = pallet.File.PalletName;
                }
                //Add by marine for T3 MES 2015/9/16
                //[MES 20160530 修改邏輯 同QPP] //20160606 因現場Pallet 沒有ID 與MES 提出不同 修正回來 
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//sy add for PPK 
                    bodyNode[keyHost.CARRIERNAME].InnerText = "";
                else
                    bodyNode[keyHost.CARRIERNAME].InnerText = palletteID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxList.Count.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify for T3 Spec 1.21 shihyang
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();
                foreach (string boxid in boxList)
                {
                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxid.Trim();
                    if (ObjectManager.CassetteManager.GetCassette(boxid.Trim()) == null)
                    {
                        boxClone[keyHost.EMPTYBOXFLAG].InnerText = "";//add for T3 by sy
                    }
                    else
                    {
                        boxClone[keyHost.EMPTYBOXFLAG].InnerText = ObjectManager.CassetteManager.GetCassette(boxid.Trim()).Empty ? "Y" : string.Empty;
                    }                    
                    boxListNode.AppendChild(boxClone);
                }
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
        ///  6.106.	PalletProcessEnd        MES MessageSet : Pallete Process End Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        /// <param name="boxQty">Box Count</param>
        /// <param name="pPID">Line Recipe ID</param>
        /// <param name="boxList">Box List</param>
        //public void PalletProcessEnd(string trxID, string lineName, string palletteID, string carrierName, string boxQty, string pPID, List<string> boxList)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
        //                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("PalletProcessEnd") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);

        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        //bodyNode[keyHost.PORTMODE].InnerText = "Palletizing";  //Hot code: [ Boxing]  Used in both PACK, UNPACK mode.[ Palletizing ] Used in PACK mode only.
        //        bodyNode[keyHost.LINERECIPENAME].InnerText = pPID;
        //        bodyNode[keyHost.PALLETNAME].InnerText = palletteID;
        //        //Add by marine for T3 MES 2015/9/16
        //        bodyNode[keyHost.CARRIERNAME].InnerText = carrierName;
        //        bodyNode[keyHost.BOXQUANTITY].InnerText = boxQty;
        //        bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

        //        XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
        //        XmlNode boxNode = boxListNode[keyHost.BOXNAME].Clone();
        //        boxListNode.RemoveAll();
        //        foreach (string boxid in boxList)
        //        {
        //            boxNode = boxNode.Clone();
        //            boxNode.InnerText = boxid.Trim();
        //            boxListNode.AppendChild(boxNode);
        //        }

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 6.107.	PalletProcessEndReply       MES MessagetSet : Pallet Process  End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_PalletProcessEndReply(XmlDocument xmlDoc)
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
                        string.Format("[LineName={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}] mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string palletid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;

                //Modify by marine for T3 MES 2015/9/14
                List<string> boxlist = new List<string>();
                XmlNode boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];

                foreach (XmlNode boxnode in boxlistnode)
                {
                    if (boxnode != null)
                    {
                        string box = boxnode.InnerText;
                        boxlist.Add(box);
                    }
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}] PalletProcessEndReply NG LINENAME={1},PALLETNAME={2},BOXQUANTITY={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, palletid, boxqty, returnCode, returnMessage));
                    //to do?
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES]=[{0}] PalletProcessEndReply  OK LINENAME={1},PALLETNAME={2},BOXQUANTITY={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, palletid, boxqty, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///  6.108.	PalletProcessStarted        MES MessageSet : BC reports Pallet processing has been started.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        /// <param name="boxQty">Box Count</param>
        /// <param name="pPID">PPID</param>
        /// <param name="boxList">Box List</param>
        public void PalletProcessStarted(string trxID, string lineName, string palletteID, string pPID, List<string> boxList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PalletProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = pPID;
                bodyNode[keyHost.PALLETNAME].InnerText = palletteID;//pallette name
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxList.Count.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify by marine for T3 MES 2015/9/14
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                //XmlNode boxNode = boxListNode[keyHost.BOXNAME].Clone();
                //boxListNode.RemoveAll();

                foreach (string boxid in boxList)
                {
                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxid.Trim();
                    boxListNode.AppendChild(boxClone);

                    //boxNode = boxNode.Clone();
                    //boxNode.InnerText = boxid.Trim();
                    //boxListNode.AppendChild(boxNode);
                }

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
        ///  6.108.	PalletProcessStarted        MES MessageSet : BC reports Pallet processing has been started.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="palletteID">Pallette ID</param>
        /// <param name="boxQty">Box Count</param>
        /// <param name="pPID">PPID</param>
        /// <param name="boxList">Box List</param>
        public void PalletProcessStarted(string trxID, string lineName, string palletteName, string boxQty, string pPID, List<string> boxList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PalletProcessStarted") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.LINERECIPENAME].InnerText = pPID;
                bodyNode[keyHost.PALLETNAME].InnerText = palletteName;
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxQty;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify by marine for T3 MES 2015/9/14
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                //XmlNode boxNode = boxListNode[keyHost.BOXNAME].Clone();
                //boxListNode.RemoveAll();

                foreach (string boxid in boxList)
                {
                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxid.Trim();
                    boxListNode.AppendChild(boxClone);

                    //boxNode = boxNode.Clone();
                    //boxNode.InnerText = boxid.Trim();
                    //boxListNode.AppendChild(boxNode);
                }

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
        /// 6.109.	PanleReportByGlass      MES MessageSet : Reports when panel need report grade by Glass
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="originalProductList"></param>
        public void PanelReportByGlass(string trxID, string lineName, IList<PanelReportByGlass.ORIGINALPRODUCTc> originalProductList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] PanelReportByGlass Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelReportByGlass") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;

                XmlNode orgProdListNode = bodyNode[keyHost.ORIGINALPRODUCTLIST];
                XmlNode orgProdCloneNodeClone = orgProdListNode[keyHost.ORIGINALPRODUCT].Clone();
                orgProdListNode.RemoveAll();

                foreach (var orgp in originalProductList)
                {
                    XmlNode orgProdCloneNode = orgProdCloneNodeClone.Clone();
                    orgProdCloneNode[keyHost.ORIGINALPRODUCTNAME].InnerText = orgp.ORIGINALPRODUCTNAME;

                    XmlNode prodNodeList = orgProdCloneNode[keyHost.PRODUCTLIST];
                    XmlNode prodNodeClone = prodNodeList[keyHost.PRODUCT].Clone();
                    prodNodeList.RemoveAll();

                    foreach (var para in orgp.PRODUCTLIST)
                    {
                        XmlNode prodNode = prodNodeClone.Clone();
                        prodNode[keyHost.PRODUCTNAME].InnerText = para.PRODUCTNAME;
                        prodNode[keyHost.MACOJUDGE].InnerText = para.MACOJUDGE;
                        prodNode[keyHost.MURACODES].InnerText = para.MURACODES;
                        prodNode[keyHost.EVENTCOMMENT].InnerText = para.EVENTCOMMENT;

                        prodNodeList.AppendChild(prodNode);
                    }

                    orgProdListNode.AppendChild(orgProdCloneNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] PanelReportByGlass OK LINENAME =[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.110.	POLAttachComplete       MES MessageSet : Reports when POL is attached to a panel
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="eQPID"></param>
        /// <param name="panelID"></param>
        /// <param name="hostPanelID"></param>
        public void POLAttachComplete(string trxID, string lineName, string machineName, string productName, string hostProductName, IList<POLAttachComplete.MATERIALc> materialData)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("POLAttachComplete") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = hostProductName;

                XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                materListNode.RemoveAll();

                foreach (POLAttachComplete.MATERIALc material in materialData)
                {
                    XmlNode materNode = prodCloneNode.Clone();
                    materNode[keyHost.CARTNAME].InnerText = material.CARTNAME;
                    materNode[keyHost.MATERIALNAME].InnerText = material.MATERIALNAME;
                    materNode[keyHost.MATERIALTYPE].InnerText = material.MATERIALTYPE;
                    materNode[keyHost.PARTNO].InnerText = material.PARTNO;
                    //需配合MES SPEC 4.02 Watson Add 20150327 For New SPEC
                    //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_3)
                    //    materNode[keyHost.ISRTP].InnerText = "Y";
                    //else
                    //    materNode[keyHost.ISRTP].InnerText = "N";
                    materNode[keyHost.ISRTP].InnerText = material.ISRTP == "1" ? "Y" : "N";  //Jun Add 20150330 For MES New Spec

                    materNode[keyHost.POSITION].InnerText = material.POSITION;
                    materListNode.AppendChild(materNode);
                }

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

        private void POLStateChangedMESTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] MES_POLStateChangedReply Timeout.", sArray[0], sArray[1]));
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        /// 6.111.	POLStateChanged     MES MessageSet : Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="machineName">Equipment identification or Equipment number. It must be unique in the whole system.</param>
        /// <param name="lineRecipeName">Representative Recipe identification of Line that is defined in MES. It must be unique in the whole system</param>
        /// <param name="materialType">Identifier of raw material type</param>
        /// <param name="cartName">SPEC無提供說明</param>
        /// <param name="partNo">Spec name of Polarizer</param>
        /// <param name="materialList"></param>
        public void POLStateChanged(string trxID, string lineName, string machineName, string lineRecipeName, eMaterialStatus materialStatus, string materialType, string polMaterialType, string cartName, string partNo, IList<POLStateChanged.MATERIALc> materialList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] POLStateChanged Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("POLStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp == null)
                    bodyNode[keyHost.RECIPEID].InnerText = lineRecipeName;
                else
                    bodyNode[keyHost.RECIPEID].InnerText = eqp.File.CurrentRecipeID;

                bodyNode[keyHost.MATERIALSTATE].InnerText = materialStatus.ToString();
                bodyNode[keyHost.MATERIALTYPE].InnerText = materialType;
                bodyNode[keyHost.CARTNAME].InnerText = cartName.Trim();  //20150420 For CSOT MES 陳忠 不能有空格要trim
                bodyNode[keyHost.PARTNO].InnerText = partNo;
                //需配合MES SPEC 4.02 Watson Add 20150327 For New SPEC
                //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_3)
                //    bodyNode[keyHost.ISRTP].InnerText = "Y";
                //else
                //    bodyNode[keyHost.ISRTP].InnerText = "N"; 
                bodyNode[keyHost.ISRTP].InnerText = polMaterialType == "1" ? "Y" : "N";  //Jun Add 20150330 For MES New Spec

                XmlNode mtlListNode = bodyNode[keyHost.MATERIALLIST];
                XmlNode mtlCloneNodeClone = mtlListNode[keyHost.MATERIAL].Clone();
                mtlListNode.RemoveAll();

                foreach (var mtl in materialList)
                {
                    XmlNode mtlCloneNode = mtlCloneNodeClone.Clone();
                    mtlCloneNode[keyHost.POSITION].InnerText = mtl.POSITION;

                    mtlCloneNode[keyHost.MATERIALNAME].InnerText = mtl.MATERIALNAME;

                    mtlCloneNode[keyHost.COUNT].InnerText = mtl.COUNT;
                    //mtlCloneNode[keyHost.LIFEQTIME].InnerText = mtl.LIFEQTIME;//MES Spec3.78版移除LIFEQTIME
                    if (mtl.MATERIALNAME.Trim() != string.Empty)   //Watson Modify 20150306 no material name no list. MES 王鵬say
                        mtlListNode.AppendChild(mtlCloneNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] POLStateChanged OK LINENAME =[{1}].",
                    trxID, lineName));

                //Jun Add 20150115 如果上報過Dismount後，就Remove
                if (materialStatus == eMaterialStatus.DISMOUNT)
                {
                    List<MaterialEntity> materialEntity = ObjectManager.MaterialManager.GetMaterials();
                    if (materialEntity != null)
                    {
                        foreach (MaterialEntity material in materialEntity)
                        {
                            if (material.MaterialID == cartName)
                            {
                                ObjectManager.MaterialManager.DeleteMaterial(material);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.111.	POLStateChanged
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqp"></param>
        public void POLStateChanged_OnLine(string trxID, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] POLStateChanged Send MES but OFF LINE LINENAME =[{1}].", trxID, eqp.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                List<MaterialEntity> materialList = ObjectManager.MaterialManager.GetMaterials();
                if (materialList != null)
                {
                    foreach (var materialE in materialList)
                    {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("POLStateChanged") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = eqp.Data.LINEID;
                        bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                        //2014/01/20 Asir Modify keyHost.LINERECIPENAME change to keyHost.RECIPEID
                        // bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        bodyNode[keyHost.RECIPEID].InnerText = eqp.File.CurrentRecipeID;
                        if (materialE.NodeNo != eqp.Data.NODENO)
                            return;

                        bodyNode[keyHost.MATERIALSTATE].InnerText = materialE.MaterialStatus.ToString();
                        bodyNode[keyHost.MATERIALTYPE].InnerText = materialE.MaterialType;
                        bodyNode[keyHost.CARTNAME].InnerText = materialE.MaterialID;
                        bodyNode[keyHost.PARTNO].InnerText = materialE.PartNo;
                        //需配合MES SPEC 4.02 Watson Add 20150327 For New SPEC
                        //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_3)
                        //    bodyNode[keyHost.ISRTP].InnerText = "Y";
                        //else
                        //    bodyNode[keyHost.ISRTP].InnerText = "N";
                        bodyNode[keyHost.ISRTP].InnerText = materialE.PolMaterialType == "1" ? "Y" : "N";  //Jun Add 20150330 For MES New Spec

                        XmlNode mtlListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode mtlCloneNodeClone = mtlListNode[keyHost.MATERIAL].Clone();
                        mtlListNode.RemoveAll();

                        foreach (string materialC in materialE.CellPOLMaterial)
                        {
                            XmlNode mtlCloneNode = mtlCloneNodeClone.Clone();

                            string[] tmpVaule = materialC.Split(',');
                            if (tmpVaule.Length == 3)
                            {
                                mtlCloneNode[keyHost.POSITION].InnerText = tmpVaule[0];
                                mtlCloneNode[keyHost.MATERIALNAME].InnerText = tmpVaule[1];
                                mtlCloneNode[keyHost.COUNT].InnerText = tmpVaule[2];
                            }
                            mtlListNode.AppendChild(mtlCloneNode);
                        }

                        //watson 20150120 Add 避免online時找不到timeid，造成錯誤log
                        string timeId5 = string.Format("{0}_{1}_POLStateChanged_ONLINE", eqp.Data.NODENO, trxID);
                        if (Timermanager.IsAliveTimer(timeId5))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                            Timermanager.TerminateTimer(timeId5);
                        Timermanager.CreateTimer(timeId5, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(POLStateChangedMESTimeout), eqp.Data.NODENO);


                        SendToMES(xml_doc);
                    }
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] POLStateChanged OK LINENAME =[{1}].",
                    trxID, eqp.Data.LINEID));

                //Jun Add 20150115 如果上報過Dismount後，就Remove
                for (int i = materialList.Count; i > 0; i--)
                {
                    if (materialList[i - 1].MaterialStatus == eMaterialStatus.DISMOUNT)
                    {
                        ObjectManager.MaterialManager.DeleteMaterial(materialList[i - 1]);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.112.	POLStateChangedReply        MES MessagetSet : Polizar Status Change Reply to BC for POL Cartridge mount.
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_POLStateChangedReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                string command = string.Empty;

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string unitNo = string.Empty;
                string key = keyBoxReplyPLCKey.POLStateChangedReply;
                object unit = Repository.Remove(key);

                if (unit == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,Unit No is Null.Don't Reply MES_POLStateChangedReply", trxID));
                }

                #endregion
                unitNo = unit.ToString();


                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqp.Data.NODENO));

                string timeId = string.Format("{0}_{1}_{2}_POLStateChanged", eqp.Data.NODENO, unitNo, trxID);
                if (Timermanager.IsAliveTimer(timeId))
                    Timermanager.TerminateTimer(timeId);
                List<POLStateChanged.MATERIALc> polList = new List<POLStateChanged.MATERIALc>();
                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_POLStateChangedReply NG LINENAME =[{1}],MACHINENAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                         trxID, lineName, machineName, returnCode, returnMessage));

                    command = "2";

                    Invoke(eServiceName.MaterialService, "PolStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, string.Empty, string.Empty, string.Empty, polList, eBitResult.ON, (eReturnCode1)int.Parse(command), trxID, false });
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_POLStateChangedReply OK LINENAME =[{1}],MACHINENAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                         trxID, lineName, machineName, returnCode, returnMessage));
                    //Send OK To Equipment
                    command = "1";//OK
                }


                string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RECIPEID].InnerText;
                //string unitId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST][keyHost.MATERIAL][keyHost.UNITID].InnerText;

                string materstatus = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALSTATE].InnerText;
                string matertype = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALTYPE].InnerText;
                string cartid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARTNAME].InnerText;
                string partno = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PARTNO].InnerText;
                string isRTP = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.ISRTP].InnerText;
                XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;

                int i = 0;
                foreach (XmlNode node in materialList)
                {
                    POLStateChanged.MATERIALc data = new POLStateChanged.MATERIALc();
                    data.POSITION = i.ToString();
                    data.MATERIALNAME = node[keyHost.MATERIALNAME].InnerText;
                    //Jun Modify 20150518 如果RTPFlag = Y時，Count需要乘以10
                    double count;
                    if (isRTP == "Y")
                    {
                        if (double.TryParse(node[keyHost.COUNT].InnerText, out count))
                            data.COUNT = (Math.Floor(count * 10).ToString());
                        else
                            data.COUNT = "0";
                    }
                    else
                    {
                        if (double.TryParse(node[keyHost.COUNT].InnerText, out count))
                            data.COUNT = (Math.Floor(count)).ToString();
                        else
                            data.COUNT = "0";
                    }
                    polList.Add(data);
                    i++;
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_POLStateChangedReply OK LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALSTATE =[{4}],CARTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                                        trxID, lineName, machineName, lineRecipeName, materstatus, cartid, returnCode, returnMessage));
                //Send OK To Equipment
                command = "1";//OK

                //watson 20150120 Add
                string timeId5 = string.Format("{0}_{1}_POLStateChanged_ONLINE", eqp.Data.NODENO, trxID);
                if (Timermanager.IsAliveTimer(timeId5))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                {
                    Timermanager.TerminateTimer(timeId5);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_POLStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                           trxID, lineName, machineName, returnCode, returnMessage));
                    return;
                }

                Invoke(eServiceName.MaterialService, "PolStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, materstatus, cartid, partno, polList, eBitResult.ON, (eReturnCode1)int.Parse(command), trxID, false });

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
        private bool CheckCellDispatchLine(Line line, Port port)
        {
            try
            {
                if (line.Data.FABTYPE != eFabType.CELL.ToString()) return false;
                if (port.File.Type != ePortType.UnloadingPort) return false;
                if (port.File.UseGrade.Trim() == string.Empty) return false;
                if (port.File.Mode != ePortMode.ByGrade) return false;
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        /// <summary>
        /// 6.142.	RuncardIdCreateRequest      MES MessageSet : BC requests first panel into a Tray. MES should RuncardId based on panel information
        /// Add by marine for T3 MES 2015/9/7
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="productName"></param>
        public void RuncardIdCreateRequest(string trxID, string lineName, string productName)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("RuncardIdCreateRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                #region MES RuncardIdCreateRequest Timeout
                string timeoutName = string.Format("{0}_MES_RuncardIdCreateRequestReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(LotIdCreateRequestT9Timeout), trxID);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.143.	RuncardIdCreateReply        MES MeaasgeSet : MES replies Runcard information after a RuncardId creation
        /// Add by marine for T3 MES 2015/9/7
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_RuncardIdCreateReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineId = GetLineName(xmlDoc);
                string trxId = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, lineId));
                }
                eReturnCode1 rtcode = returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG;
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxDataRequestReply", trxId));
                    return;
                }
                #endregion
                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_RuncardIdCreateRequestReply", trxId);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                string lotName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTNAME].InnerText;
                string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                #region [Reply NG]
                if (rtcode == eReturnCode1.NG)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RuncardIdCreateReply NG LINENAME =[{1}],PRODUCTNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                         trxId, lineId, productName, returnCode, returnMessage));

                    Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxId, rtcode, lotName, "0" });
                    return;
                }
                #endregion
                string capacity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CAPACITY].InnerText;

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RuncardIdCreateReply OK LINENAME =[{1}],PRODUCTNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                     trxId, lineId, productName, returnCode, returnMessage));

                Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, trxId, rtcode, lotName, capacity });

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.144.	RuncardLableInformationRequest      MES MessageSet : BC requests label information of Runcard
        /// Add by marine for T3 MES 2015/9/8
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="lotID"></param>
        public void RuncardLableInformationRequest(string trxID, string lineName, string lotID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RuncardLableInformationRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LOTNAME].InnerText = lotID;

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
        /// 6.145.	RuncardLableInformationReply        MES MessageSet : MES replies label information of Runcard
        /// Add by marine for T3 MES 2015/9/8
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_RuncardLableInformationReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineId = GetLineName(xmlDoc);
                string trxId = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineId))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, lineId));
                }

                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string lotName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTNAME].InnerText;
                string quantity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.QUANTITY].InnerText;
                string bomVersion = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOMVERSION].InnerText;
                string modelName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELNAME].InnerText;
                string modelVersion = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MODELVERSION].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RuncardLableInformationReply NG LINENAME =[{1}],LOTNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                         trxId, lineId, lotName, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_RuncardLableInformationReply OK LINENAME =[{1}],LOTNAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                         trxId, lineId, lotName, returnCode, returnMessage));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 6.151.	ValidateBoxWeightReply      MES MessageSet : Box Validate Request Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="boxtotalweight">The total weight of boxes on a stage of Palletizing equipment.</param>
        /// <param name="boxlist">total box id list</param>
        public void ValidateBoxWeightRequest(string trxID, string lineName, string boxtotalweight, IList boxlist, string eqpNoforTimeout)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        // string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    #region MES Data Validate Box Weight Reply
                    #region Get and Remove Reply Key
                    string eqpNo = string.Empty;
                    string key = keyBoxReplyPLCKey.DenseBoxWeightCheckReportReply;
                    object eqp_portno = Repository.Remove(key);
                    if (eqp_portno == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxWeightCheckReportReply", trxID));

                        return;
                    }
                    string[] sArray = eqp_portno.ToString().Split(';');
                    eqpNo = sArray[0];
                    string portNo = sArray[1];
                    #endregion

                    string timeoutName1 = string.Format("{0}_MES_ValidateBoxWeightReply", eqpNo);
                    if (_timerManager.IsAliveTimer(timeoutName1))
                        _timerManager.TerminateTimer(timeoutName1);

                    if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        Invoke(eServiceName.DenseBoxService, "DenseBoxWeightCheckReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "1", portNo });
                    else
                        Invoke(eServiceName.DenseBoxService, "DenseBoxWeightCheckReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "2", portNo });
                    #endregion

                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateBoxWeightRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxlist.Count.ToString();
                bodyNode[keyHost.BOXWEIGHT].InnerText = boxtotalweight;

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];

                //Modify by marine for T3 MES 2015/9/14
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                //XmlNode boxNode = boxListNode[keyHost.BOXNAME].Clone();
                //boxListNode.RemoveAll();

                foreach (string boxid in boxlist)
                {
                    if (boxid.Trim() == string.Empty)
                        continue;
                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxid.Trim();
                    boxListNode.AppendChild(boxClone);

                    //boxNode = boxNode.Clone();
                    //boxNode.InnerText = boxid.Trim();
                    //boxListNode.AppendChild(boxNode);
                }

                SendToMES(xml_doc);

                #region MES Validate Box Weight Reply Timeout
                string timeoutName = string.Format("{0}_MES_ValidateBoxWeightReply", eqpNoforTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateBoxWeightReply_Timeout), trxID);
                #endregion

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
        /// 6.151.	ValidateBoxWeightReply      MES MessageSet :MES Reply Box Weight Validate Data to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ValidateBoxWeightReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName  =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string result = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;
                if (returnCode != "0")
                {
                    //Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES]=[{0}] MES_ValidateBoxWeightReply NG LINENAME={1},VALIRESULT={2},CODE={3},MESSAGE={4}.",
                                                             trxID, lineName, result, returnCode, returnMessage));
                    result = "N";  //可能會不給值
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_ValidateBoxWeightReply OK LINENAME={1},VALIRESULT={2},CODE={3},MESSAGE={4}.",
                                         trxID, lineName, result, returnCode, returnMessage));
                    //to do?
                }

                XmlNode boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                List<string> boxlist = new List<string>();
                foreach (XmlNode boxnode in boxlistnode)
                {
                    //Modify by marine for T3 MES 2015/9/14
                    if (boxnode[keyHost.BOXNAME] != null)
                    {
                        string boxname = boxnode[keyHost.BOXNAME].InnerText;
                        boxlist.Add(boxname);
                    }
                }

                #region MES Data Validate Box Weight Reply
                List<string> replylist = new List<string>();
                replylist.AddRange(new string[] { returnCode });

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key) and PortNo,BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxWeightCheckReportReply;
                object eqp_portno = Repository.Remove(key);
                if (eqp_portno == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxWeightCheckReportReply", trxID));

                    return;
                }
                string[] sArray = eqp_portno.ToString().Split(';');
                eqpNo = sArray[0];
                string portNo = sArray[1];
                #endregion

                string timeoutName = string.Format("{0}_MES_ValidateBoxWeightReply", eqpNo);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                object[] _data = new object[5]
                { 
                    eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    result == "Y"?"1":"2", //1：OK  2：NG  3：etc NG…
                    portNo
                };
                //DenseBoxWeightCheckReportReply(string eqpNo, eBitResult value, string trackKey, string rtncode)
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxWeightCheckReportReply", _data);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ValidateBoxWeightReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  Validate Box Weight MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_ValidateBoxWeightReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                #region MES Data Validate Box Weight Reply
                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxWeightCheckReportReply;
                //Jun Modify 20141230 修正Key節構
                object eqp_portno = Repository.Remove(key);
                if (eqp_portno == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxWeightCheckReportReply", trackKey));

                    return;
                }
                string[] keyArray = eqp_portno.ToString().Split(';');
                eqpNo = keyArray[0];
                string portNo = keyArray[1];
                #endregion

                object[] _data = new object[5]
                { 
                    eqpNo,
                    eBitResult.ON,    /*1 ON*/
                    trackKey,                          /*2 trx id */
                    "3", //eReturnCode1.NG.ToString() Jun Modify 20150106 直接ToString會寫入NG 而不是數字
                    portNo
                };
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxWeightCheckReportReply", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.164.	ValidatePalletRequest       MES MessageSet : Pallete Validate Request Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="portID">Port ID (Box postion)</param>
        /// <param name="portmode"> [ Boxing] [Palletizing ]</param>
        /// <param name="palletID">Pallete ID(Box position)</param>
        /// <param name="boxlist">Box List</param>
        public void ValidatePalletRequest(string trxID, string lineName, string portID, string portmode, string palletID, IList boxlist, string eqpNoforTimeout)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));

                    #region Watson Modify 20150409 目前由OPI可以下，所以不需要判斷
                    //#region Get and Remove Reply Key
                    //string eqpNo = string.Empty;
                    //string key = keyBoxReplyPLCKey.PalletDataRequestReportReply;
                    //object eqp = Repository.Remove(key);
                    //if (eqp == null)
                    //{
                    //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletDataRequestReportReply", trxID));
                    //    return;
                    //}
                    //eqpNo = eqp.ToString();
                    //#endregion
                    //string timeoutName1 = string.Format("{0}_MES_ValidatePalletReply", eqpNo);
                    //if (_timerManager.IsAliveTimer(timeoutName1))
                    //    _timerManager.TerminateTimer(timeoutName1);

                    //#region PLC
                    //if (ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                    //    Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "1", null });
                    //else
                    //    Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", new object[] { eqpNo, eBitResult.ON, trxID, "2", null });
                    //#endregion
                    #endregion
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidatePalletRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.PALLETNAME].InnerText = palletID;
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.PORTMODE].InnerText = portmode;
                bodyNode[keyHost.BOXQUANTITY].InnerText = boxlist.Count.ToString();

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify by marine for T3 MES 2015/9/15
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                //XmlNode boxNode = boxListNode[keyHost.BOXNAME];
                //boxListNode.RemoveAll();
                foreach (string boxid in boxlist)
                {
                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxid.Trim();
                    boxListNode.AppendChild(boxClone);
                }

                SendToMES(xml_doc);

                #region MES Validate Pallet Reply Timeout
                string timeoutName = string.Format("{0}_MES_ValidatePalletReply", eqpNoforTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidatePalletReply_Timeout), trxID);
                #endregion

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
        /// 6.165.	ValidatePalletReply     MES MessageSet :MES Reply Pallete Validate Data to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_ValidatePalletReply(XmlDocument xmlDoc)
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
                        string.Format("[LineName={0}] [BCS <- MES] =[{1}] MES Reply LineName ={2} mismatch {0}.", ServerName, trxID, lineName));
                }

                //to Do
                //20170911 huangjiayin add: MES Reply NG , May Loss Some Items
                string lineRecipeName = string.Empty;
                string palletid = string.Empty;
                string boxqty = string.Empty;
                string portid = string.Empty;

                foreach (XmlElement xel in xmlDoc[keyHost.MESSAGE][keyHost.BODY].ChildNodes)
                {
                    switch (xel.Name)
                    {
                        case keyHost.LINERECIPENAME:
                            lineRecipeName = xel.InnerText;
                            break;
                        case keyHost.PALLETNAME:
                            palletid = xel.InnerText;
                            break;
                        case keyHost.BOXQUANTITY:
                            boxqty = xel.InnerText;
                            break;
                        case keyHost.PORTNAME:
                            portid = xel.InnerText.PadLeft(2, '0');
                            break;
                    }
                }

                /*
                lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                palletid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;
                boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText.PadLeft(2, '0');
                */
                
                if (returnCode != "0")
                {
                    //Send NG to Equipment ?
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_ValidatePalletReply NG LINENAME={1},LINERECIPENAME={2},PALLETNAME={3},CODE={4},MESSAGE={5}.",
                                         trxID, lineName, lineRecipeName, palletid, returnCode, returnMessage));
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES]=[{0}] MES_ValidatePalletReply OK LINENAME={1},LINERECIPENAME={2},PALLETNAME={3},CODE={4},MESSAGE={5}.",
                                         trxID, lineName, lineRecipeName, palletid, returnCode, returnMessage));
                    //to do?
                }
                foreach (Pallet p in ObjectManager.PalletManager.GetPallets())//20161226 sy modify
                {
                    if (p.File.PalletID ==palletid  && p.File.PalletNo != portid )
                    {
                        p.File.PalletID = "";
                        ObjectManager.PalletManager.EnqueueSave(p.File);      
                    }
                }                
                Pallet pallet = ObjectManager.PalletManager.GetPalletByID(palletid);

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.PalletDataRequestReportReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletDataRequestReportReply", trxID));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion
                // ReturnCode;
                // PalletID;
                // PalletNo;
                // DenseBoxCount
                string timeoutName = string.Format("{0}_MES_ValidatePalletReply", eqpNo);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (pallet == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] Can't find Pallet ID=[{1}] in PalletEntity", trxID, palletid));
                    PalletEntityFile newfile = new PalletEntityFile();
                    pallet = new Pallet(newfile);
                    #region PLC Data Write
                    //string eqpNo, eBitResult value, string trackKey, string returncode, Pallet pallet
                    object[] _data = new object[5]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*trx id */
                        "2",
                        pallet
                    };
                    //Send MES Data
                    base.Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", _data);
                    #endregion
                }
                else
                {
                    Line line = ObjectManager.LineManager.GetLine(lineName);
                    if (line != null)
                    {
                        if (line.File.HostMode == eHostMode.LOCAL)
                        {
                            lock (pallet) pallet.File.Mes_ValidatePalletReply = xmlDoc.InnerXml;

                            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] Local Mode, Check MES Data Complete, Wait UI Edit Data.",
                                lineName, trxID));

                            Invoke(eServiceName.UIService, "PalletStatusReport", new object[] { line.Data.LINEID, pallet });
                            return;
                        }
                    }

                    //pallet.File.PalletID = palletid;
                    //pallet.File.PalletNo = portid;
                    pallet.File.DenseBoxList = new List<string>();

                    XmlNode boxlistnode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                    List<string> boxlist = new List<string>();
                    int pos = 1;
                    int palletNo = 0;
                    int.TryParse(portid, out palletNo);
                    foreach (XmlNode boxnode in boxlistnode)
                    {
                        if (boxnode[keyHost.BOXNAME] != null)
                        {
                            string boxname = boxnode[keyHost.BOXNAME].InnerText;
                            boxlist.Add(boxname);
                            pallet.File.DenseBoxList.Add(boxname);

                            //Jun Add 20150106 Create Box Object
                            Cassette cst = new Cassette();
                            cst.PortNo = palletid;
                            cst.PortID = palletid;
                            cst.CassetteSequenceNo = (palletNo * 100 + pos).ToString();
                            cst.CassetteID = boxname;
                            cst.LineRecipeName = lineRecipeName;
                            ObjectManager.CassetteManager.CreateBoxforPacking(cst);//20161102 sy modify 只會下空PALLET
                        }
                        pos++;
                    }
                    #region PLC Data Write
                    //string eqpNo, eBitResult value, string trackKey, string returncode, Pallet pallet
                    object[] _data = new object[5]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*trx id */
                        returnCode != "0" ? "2" : "1",
                        pallet
                    };
                    //Send MES Data
                    base.Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", _data);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ValidatePalletReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  Validate Pallet MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_ValidatePalletReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.PalletDataRequestReportReply;
                object eqp = Repository.Remove(key);
                if (eqp == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES]=[{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_PalletDataRequestReportReply", trackKey));
                    return;
                }
                eqpNo = eqp.ToString();
                #endregion
                #region MES Data Send Product IN
                object[] _data = new object[5]
                { 
                    eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trackKey,                          /*trx id */
                    eReturnCode1.NG.ToString(),
                    null
                };
                base.Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///  6.XXXXX	PaperBoxLabelInformationRequest      
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="boxID"></param>
        public void PaperBoxLabelInformationRequest(string trxID, string lineName, string eqpNo, string eqpID, string boxID, string boxType, string subBoxID, string subBoxType)
        {
            try
            {
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                if (line.File.HostMode == eHostMode.OFFLINE) return;
                #region[MES Data]
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxLabelInformationRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.BOXNAME].InnerText = boxID;
                bodyNode[keyHost.BOXTYPE].InnerText = boxType;
                XmlNode subBoxList = bodyNode[keyHost.SUBBOXLIST];
                subBoxList[keyHost.SUBBOX][keyHost.SUBBOXNAME].InnerText = subBoxID;
                subBoxList[keyHost.SUBBOX][keyHost.BOXTYPE].InnerText = subBoxType;
                SendToMES(xml_doc);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] BoxLabelInformationRequest OK LINENAME=[{1}], BOXNAME=[{2}].BOXTYPE=[{3}].SUBBOXNAME=[{4}].SUBBOXTYPE=[{5}].",
                         trxID, lineName, boxID, boxType, subBoxID, subBoxType));
                #endregion
                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.BoxLabelInformationReply;
                string rep = eqpNo;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region MES BoxLabelInformationReply Timeout
                string timeoutName = string.Format("{0}_MES_PaperBoxLabelInformationReply", eqpID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PaperBoxLabelInformationReply_Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PaperBoxLabelInformationReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  MACHINENAME={1}  PaperBox Label Information MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_PaperBoxLabelInformationReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(sArray[0]);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", sArray[0]));
                #region [PaperBoxLabelInformationRequestReply]
                object[] _data = new object[26]
                { 
                    eqp.Data.NODENO ,  /*0 eqpNo*/
                    eBitResult.ON ,         /*1 eBitResult*/
                    trackKey,                           /*2 trackKey */
                    eReturnCode1.NG ,         /*3 returnCode*/
                    "",                    /*4  SHIPID Name */
                    "",              /*5 SHIPID Value */
                    "",                    /*6  QUANTITY */
                    "",              /*7 QUANTITY*/
                    "",                    /*8  BOMVERSION */
                    "",              /*9 BOMVERSION*/
                    "",                    /*10  MODELNAME --*/
                    "",              /*11 MODELNAME*/
                    "",                    /*12  MODELVERSION */
                    "",              /*13 MODELVERSION*/
                    "",                    /*14  ENVIRONMENTFLAG */
                    "",              /*15 ENVIRONMENTFLAG*/
                    "",                    /*16 PARTID */
                    "",              /*17 PARTID*/
                    "",                    /*18  CARRIERNAME */
                    "",              /*19 CARRIERNAME*/
                    "",                    /*20  NOTE */
                    "",              /*21 NOTE*/
                    "",                    /*22  COUNTRY */
                    "",              /*23 COUNTRY*/
                    "",                    /*24  WT */
                    ""              /*25 WT*/
                };
                Invoke(eServiceName.PaperBoxService, "PaperBoxLabelInformationRequestReply", _data);
                #endregion
                #region [PaperBoxLabelInformationRequestReply old mark]
                //object[] _data = new object[8]
                //{ 
                //    eqp.Data.NODENO ,  /*0 eqpNo*/
                //    eBitResult.ON ,         /*1 eBitResult*/
                //    trackKey,                           /*2 trackKey */
                //    eReturnCode1.NG ,         /*3 returnCode*/
                //    string.Empty,                    /*4 paperBoxID */
                //    string.Empty,              /*5 modelName*/
                //    string.Empty ,                  /*6 modelVersion*/
                //    string.Empty                                /*7 cartonQuantity*/
                //};
                //Invoke(eServiceName.PaperBoxService, "PaperBoxLabelInformationRequestReply", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void P2M_DecodeEQPFlagBy_CELLBOX(XmlNode product, Line line, Job job)
        {
            try
            {
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                if (subItem == null)
                {
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}].", job.CassetteSequenceNo, job.JobSequenceNo));
                    return;
                }

                if (subItem.ContainsKey("CuttingFlag"))
                {
                    eCUTTING_FLAG CUTTING_FLAG;
                    Enum.TryParse<eCUTTING_FLAG>(subItem["CuttingFlag"], out CUTTING_FLAG);
                    product[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, CUTTING_FLAG);
                }
                else
                    product[keyHost.SHORTCUTFLAG].InnerText = "N";

                if (subItem.ContainsKey("ShellFlag"))//MES 1.21 ADD PIL 用
                    product[keyHost.CELLSAMPLEFLAG].InnerText = subItem["ShellFlag"] == "1" ? "SHELL" : "";
                else if (subItem.ContainsKey("BubbleSampleFlag"))//MES 1.21 ADD PCS 用
                    product[keyHost.CELLSAMPLEFLAG].InnerText = subItem["BubbleSampleFlag"] == "1" ? "BUBBLE" : "";
                else
                    product[keyHost.CELLSAMPLEFLAG].InnerText = "";

                if (subItem.ContainsKey("OQCNGFlag"))//MES 1.21 ADD PCK 用
                    product[keyHost.OQCNGRESULT].InnerText = subItem["OQCNGFlag"] == "1" ? "Y" : "N";
                else
                    product[keyHost.OQCNGRESULT].InnerText = "";

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ProductInForPallet(string trxID, string lineName, Cassette Box, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl, string processtime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(currentEQPID);

                Unit unit = null;
                if (unitID != "")
                    unit = ObjectManager.UnitManager.GetUnit(unitID);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductIn") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                if (unitID != "")
                    bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;
                if (portid != "")
                    bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);
                else
                    bodyNode[keyHost.PORTNAME].InnerText = string.Empty;
                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();
                bodyNode[keyHost.LOTNAME].InnerText = Box.CassetteID;//------------
                //bodyNode[keyHost.CARRIERNAME].InnerText = job.ToCstID; //沒有就報空//////////////-----------
                //Watson modidyf 2011215 For MES 王鵬
                //if (traceLvl == eMESTraceLevel.M)
                //    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                //else
                //bodyNode[keyHost.POSITION].InnerText = job.ToSlotNo.Equals("0") ? "" : job.ToSlotNo;///------------

                bodyNode[keyHost.PRODUCTNAME].InnerText = "";//------
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = "";//--
                bodyNode[keyHost.LINERECIPENAME].InnerText = "";//---
                //if (job.MesCstBody.LOTLIST.Count > 0)
                //    bodyNode[keyHost.HOSTRECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME;//--
                //bodyNode[keyHost.CURRENTRECIPEID].InnerText = job.PPID;

                //Watson Modify 20141124 job.PPID 是寫給 機台的，不能上報MES AABBCCDDEEFF，所以需要重新組AA;BB;CC;DD;EE;FF;GG
                //if (job.MES_PPID != null)
                //    //Jun Modify 20141203 job.MES_PPID=AA;BB;CC;DD 不需要增加;
                //    bodyNode[keyHost.BCRECIPEID].InnerText = job.MES_PPID; //------
                //else
                //    bodyNode[keyHost.BCRECIPEID].InnerText = ObjectManager.JobManager.Covert_PPID_To_MES_FORMAT(job.PPID).Trim();//-----
                //bodyNode[keyHost.HOSTRECIPEID].InnerText = job.MesProduct.PPID;//----

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.PROCESSINGTIME].InnerText = processtime; //是Port 就不為空

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";  //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText.Trim());

                if (otherline.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, otherline.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductIn", eAgentName.MESAgent) == false)
                {
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                            trxID, bodyNode[keyHost.LINENAME].InnerText));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductIn to MES.",
                       trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProductOutForPallet(string trxID, string lineName, Cassette Box, string currentEQPID, string portid, string unitID, eMESTraceLevel traceLvl)
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
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductOut") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                if (unitID != "")
                    bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                else
                    bodyNode[keyHost.UNITNAME].InnerText = string.Empty;
                if (portid != "")
                    bodyNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(eqp.Data.NODENO, portid);
                else
                    bodyNode[keyHost.PORTNAME].InnerText = string.Empty;
                bodyNode[keyHost.TRACELEVEL].InnerText = traceLvl.ToString();
                //if (job.MesCstBody.LOTLIST.Count > 0)
                bodyNode[keyHost.LOTNAME].InnerText = Box.CassetteID;//-------
                //bodyNode[keyHost.CARRIERNAME].InnerText = Box.CassetteID;//-------
                //if (traceLvl == eMESTraceLevel.M)
                //    bodyNode[keyHost.POSITION].InnerText = string.Empty;
                //else
                //    bodyNode[keyHost.POSITION].InnerText = job.FromSlotNo;
                //if (job.GlassChipMaskBlockID == string.Empty)
                //{
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                //}
                //else
                //    bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = "";//-----
                bodyNode[keyHost.PRODUCTJUDGE].InnerText = "";//-----
                bodyNode[keyHost.PRODUCTGRADE].InnerText = "";//-----

                bodyNode[keyHost.SUBPRODUCTGRADES].InnerText = "";  //watson modify 20141122 For 應該是要報機台即時報
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N"; //‘N’ – no  cross  line.. Watson Add 20150209 For MES SPEC

                //Watson Add 20150210 For MES Spec 3.85
                //if (job.LastGlassFlag == "1")
                //    bodyNode[keyHost.LASTGLASSFLAG].InnerText = "Y";
                //else
                bodyNode[keyHost.LASTGLASSFLAG].InnerText = "N";

                Line otherline = ObjectManager.LineManager.GetLine(bodyNode[keyHost.LINENAME].InnerText);

                if (otherline.File.HostMode == eHostMode.OFFLINE) //Offline 无需上报资料 20150325 Tom 
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                if (ObjectManager.LineManager.CheckSkipReportByID(lineName, currentEQPID, unitID, "ProductOut", eAgentName.MESAgent) == false)
                {
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                        trxID, bodyNode[keyHost.LINENAME].InnerText));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Equipment =[{2}],Unit =[{3}]Don't Report ProductOut to MES.",
                      trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID)));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CheckBoxNameRequestT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.BoxIDCheckRequestReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxDataRequestReply", sArray[0]));
                    return;
                }
                #endregion
                string timeoutName = string.Format("{0}_MES_CheckBoxNameRequestReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Invoke(eServiceName.DenseBoxService, "BoxIDCheckRequestReply", new object[] { eqpID, eBitResult.ON, sArray[0], eReturnCode1.NG, "", "", "" });
                //CheckBoxNameRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string boxId, string boxtype, string palletID)
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "CheckBoxNameRequestT9Timeout");

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //Add By Yangzhenteng For PCK Line BoxID Check;
        public void BoxIDRequest(string trxID, Port Port, string lineid, IList PanelIDList, string EQPID) 
            {
                try
                {
                    Line line = ObjectManager.LineManager.GetLine(lineid);
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, Port.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                        return;
                    }
                    IServerAgent agent = GetServerAgent();
                    XmlDocument xml_doc = agent.GetTransactionFormat("BoxIDRequest") as XmlDocument;
                    SetTransactionID(xml_doc, trxID);
                    XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                    bodyNode[keyHost.LINENAME].InnerText = lineid;
                    bodyNode[keyHost.PORTID].InnerText = Port.Data.PORTID.ToString();
                    bodyNode[keyHost.PANELQUANTITY].InnerText = PanelIDList.Count.ToString();
                    XmlNode _PanelIDListNode = bodyNode[keyHost.PANELIDLIST];
                    XmlNode _PanelID = _PanelIDListNode[keyHost.PANEL];
                    _PanelIDListNode.RemoveAll();
                    foreach (string panelID in PanelIDList)
                    {
                        if (panelID.Trim() == string.Empty) continue;
                        XmlNode panelIDClone = _PanelID.Clone();
                        panelIDClone[keyHost.PRODUCTNAME].InnerText = panelID.Trim();
                        _PanelIDListNode.AppendChild(panelIDClone);
                    }
                    SendToMES(xml_doc);
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME=[{1})] [BCS -> MES][{0}] , PORT_ID=[{2}] PanelIDList=[{3}]",
                              trxID, lineid, Port.Data.PORTNO, PanelIDList));
                    #region Mes_BoxIDRequestReply_Timeout  
                    string timeoutName = string.Format("{0}_{1}_BoxIDRequestReply_Timeout", lineid ,Port.Data.PORTID);                
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(BoxIDRequestReply_Timeout), trxID);
                    #endregion
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }
        public void MES_BoxIDRequestReply(XmlDocument XMLDoc)
            {
                try
                {
                    string returnCode = GetMESReturnCode(XMLDoc);
                    string returnMessage = GetMESReturnMessage(XMLDoc);
                    string LineName = GetLineName(XMLDoc);
                    string Portid = GetPortID(XMLDoc);
                    string trxID = GetTransactionID(XMLDoc);
                    Port PORT = ObjectManager.PortManager.GetPort(Portid);
                    string timeoutName = string.Format("{0}_{1}_BoxIDRequestReply_Timeout", PORT.Data.LINEID,PORT.Data.PORTID);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    if (!CheckMESLineID(LineName))
                    {
                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, LineName));
                    }
                    if (returnCode != "0")
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                   string.Format("[BCS <- MES]=[{0}] MES_BOXID Request Reply NG LINENAME={1},CODE={2},MESSAGE={3}.",
                                                                 trxID, LineName, returnCode, returnMessage));
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), PORT.Data.LINEID, 
                               string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, returnMessage) });
                        return;
                    }
                    else
                    {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("[BCS <- MES]=[{0}] MES_BOXID Request Reply OK LINENAME={1},CODE={2},MESSAGE={3}.",
                                             trxID, LineName, returnCode, returnMessage));
                    }
                    XmlNode portidnode = XMLDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTID];
                    XmlNodeList boxlistnode = XMLDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST].ChildNodes;
                    List<string> boxidlist = new List<string>();
                    foreach (XmlNode boxnode in boxlistnode)
                    {
                        string box = boxnode[keyHost.BOXID].InnerText; 
                        if (!string.IsNullOrEmpty(box))
                        {
                            boxidlist.Add(box);
                        }
                        if (string.IsNullOrEmpty(box))
                        {
                            string EMessage = string.Empty;
                            EMessage = string.Format("MES Reply BoxID IS Empty");
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), PORT.Data.LINEID, 
                            string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, EMessage) });
                            return;
                        }
                    }
                    PORT.File.BoxIDList  =  boxidlist;
                    lock (PORT.File)
                    {
                        ObjectManager.PortManager.EnqueueSave(PORT.File);
                    }

                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }
        private void BoxIDRequestReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    UserTimer timer = subjet as UserTimer;
                    string tmp = timer.TimerId;
                    string trackKey = timer.State.ToString();
                    string[] sArray = tmp.Split('_');
                    string err = string.Empty;
                    Port port = ObjectManager.PortManager.GetPortByLineIDPortID(sArray[0], sArray[1]);
                    err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1} BoxID Request MES Reply Timeout.", trackKey, port.Data.NODEID);
                    string timeoutName = string.Format("{0}_{1}_BoxIDRequestReply_Timeout", sArray[0], sArray[1]);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    if (!string.IsNullOrEmpty(err))
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);                 
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), port.Data.LINEID, 
                               string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
        //For Cell CUT BUR,Add By Yangzhenteng20180420
        public void PanelRejudgeReport(string trxID, string lineid, string EQPID, string GlassID, string PortName, string SlotNo, string SideNo)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineid);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelRejudgeReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineid;
                bodyNode[keyHost.MACHINENAME].InnerText = EQPID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = GlassID;
                bodyNode[keyHost.PORTNAME].InnerText = PortName;
                bodyNode[keyHost.POSITION].InnerText = SlotNo;
                bodyNode[keyHost.SIDE].InnerText = SideNo;
                SendToMES(xml_doc);
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}][BCS -> MES][{1}],MachineName ={2},ProductName=[{3}],PortID=[{4}],Position=[{5}],Side=[{6}]",
                       lineid, trxID, EQPID, GlassID, PortName, SlotNo, SideNo));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void MES_PanelRejudgeResultRequest(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);
                string Inboxname = GetMESINBOXName(xmlDoc);
                Line L = ObjectManager.LineManager.GetLine(lineName);
                if (L.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, L.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }
                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", lineName, trxID, lineName));
                }
                string LineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINENAME].InnerText;
                string MachineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string PortName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                PortName = PortName.PadLeft(2, '0');
                string ProductName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                string ProductSpecName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string JudgeResult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.JUDGERESULT].InnerText;
                string SlotNO = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.POSITION].InnerText;
                string SideNo = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.SIDE].InnerText;
                string ReasonCode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DEFECTCODE].InnerText;
                Port P = ObjectManager.PortManager.GetPort(PortName);
                string portno = P.Data.PORTNO;
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_PanelRejudgeResultRequest NG LINENAME=[{1}],RETURNCODE=[{2}],MESSAGE=[{3}].",
                           trxID, lineName, returnCode, returnMessage));
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "[MES_PanelRejudgeResultRequest] MES REPLY CurrentDateTime Return Code <> '0' , BCS AND MES REMOTE FAILED(DISONNECT)!!" });
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_PanelRejudgeResultRequest OK,RETURNCODE=[{2}],MESSAGE=[{3}].",
                           trxID, lineName, returnCode, returnMessage));
                    Invoke(eServiceName.CELLSpecialService, "RejudgeCommandCheck", new object[] { MachineName, ProductName, portno, SlotNO, SideNo, JudgeResult, trxID, ProductSpecName, Inboxname, ReasonCode });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PanelRejudgeResultReply(string trackkey, string lineName, string eqpNo, string PortName, string GlassID, string SlotNO, string SideNO, string Judgeresult, string Returncode, string ProductSpecName, string INBoxName)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can't Find Equipment No =[{0}] in EquipmentEntity!", eqpNo));
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                if (line.File.HostMode == eHostMode.OFFLINE) return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelRejudgeResultReply") as XmlDocument;
                SetTransactionID(xml_doc, trackkey);
                XmlNode header = xml_doc[keyHost.MESSAGE][keyHost.HEADER];
                header[keyHost.INBOXNAME].InnerText = INBoxName;
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                bodyNode[keyHost.PORTNAME].InnerText = PortName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = GlassID;
                bodyNode[keyHost.PRODUCTSPECNAME].InnerText = ProductSpecName;
                bodyNode[keyHost.POSITION].InnerText = SlotNO;
                bodyNode[keyHost.SIDE].InnerText = SideNO;
                bodyNode[keyHost.JUDGERESULT].InnerText = Judgeresult;
                bodyNode[keyHost.RETURNCODE].InnerText = Returncode;
                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] PanelRejudgeResultReply OK, LINENAME=[{1}], MACHINENAME=[{2}].PORTNAME=[{3}].PRODUCTNAME=[{4}].PRODUCTSPECNAME=[{5}].SLOTNO=[{6}].SIDE=[{7}].JUDGERESULT=[{8}].RETURNCODE=[{9}].",
                       trackkey, lineName, eqp.Data.NODEID, PortName, GlassID, ProductSpecName, SlotNO, SideNO, Judgeresult, Returncode));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        //Add By Yangzhenteng20180803
        public void MachienPanelRejudgeFetchOutReport(string trxID, string lineid, string EQPID, string GlassID, string PortName, string SlotNo, string SideNo)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineid);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachienPanelRejudgeFetchOutReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineid;
                bodyNode[keyHost.MACHINENAME].InnerText = EQPID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = GlassID;
                bodyNode[keyHost.PORTNAME].InnerText = PortName;
                bodyNode[keyHost.POSITION].InnerText = SlotNo;
                bodyNode[keyHost.SIDE].InnerText = SideNo;
                SendToMES(xml_doc);
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}][BCS -> MES][{1}],MachineName ={2},ProductName=[{3}],PortID=[{4}],Position=[{5}],Side=[{6}]",
                       lineid, trxID, EQPID, GlassID, PortName, SlotNo, SideNo));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}