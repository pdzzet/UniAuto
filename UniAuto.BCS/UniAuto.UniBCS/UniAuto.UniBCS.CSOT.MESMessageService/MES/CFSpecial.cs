
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
        /// 6.19.	CFShortCutPermitRequest     MES MessageSet : CF Short Cut Permit Request to MES
        /// </summary>
        /// <param name="trxID">Transaction ID</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="productName">Glass ID or Panel ID</param>
        /// <param name="hostProductName">BC reports Product Identifier that was received from MES within cassette data</param>
        /// <param name="processLineName">SPEC無提供說明</param>
        public void CFShortCutPermitRequest(string trxID, string lineName, string productName, string hostProductName, string processLineName, string totalPitch)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("CFShortCutPermitRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = hostProductName;
                bodyNode[keyHost.PROCESSLINENAME].InnerText = processLineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                //Add for MES 2015/7/27
                Job job = ObjectManager.JobManager.GetJob(productName);
                if (job == null)
                {
                    
                }
                else
                {   //不同cst的第一片玻璃，SAMPLINGCHECKFLAG要上報"Y，同CST內的其他玻璃，SAMPLINGCHECKFLAG上報"N"。
                    if (job.CfSpecial.PermitFlag == string.Empty)
                        bodyNode[keyHost.SAMPLINGCHECKFLAG].InnerText = "Y";
                    else
                        bodyNode[keyHost.SAMPLINGCHECKFLAG].InnerText = "N";
                }
                bodyNode[keyHost.INSPECTIONFLAG].InnerText = totalPitch == "1" ? "Y" : "N";

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
        ///6.20.	CFShortCutPermitReply        MES MessageSet :MES  CF Short Cut Permit Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CFShortCutPermitReply(XmlDocument xmlDoc)
        {
            try
            {
                string jobID = string.Empty;
                string trxID = GetTransactionID(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name    
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply NG, ReturnMessage:=[{2}].", ServerName, trxID, returnMessage));
                    Invoke(eServiceName.CFSpecialService, "PermissionRequestReportReply", new object[] { trxID, eBitResult.ON, "L21", eReturnCode1.NG });                    
                    return;
                }
                else
                {
                    string permitFlag = GetMESPermitFlag(xmlDoc);
                    if (!CheckMESLineID(lineName))
                    {
                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- MES] =[{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                        Invoke(eServiceName.CFSpecialService, "PermissionRequestReportReply", new object[] { trxID, eBitResult.ON, "L21", eReturnCode1.NG});
                    }

                    // 獲取MES Body層資料
                    Job job = new Job();
                    XmlNode body = GetMESBodyNode(xmlDoc);
                    XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                    IList<Job> jobs = new List<Job>();
                    foreach (XmlNode n in lotNodeList)
                    {
                        #region Get Job Glass ID
                        XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                        for (int i = 0; i < productList.Count; i++)
                        {
                            jobID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();
                            job = ObjectManager.JobManager.GetJob(jobID);
                            if (job == null)
                            {
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] Can't Get Job WIP, LINENAME =[{1}],PERMITFLAG =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                                    trxID, lineName, permitFlag, returnCode, returnMessage));
                                Invoke(eServiceName.CFSpecialService, "PermissionRequestReportReply", new object[] { trxID, eBitResult.ON, "L21", eReturnCode1.NG });
                                return;
                            }
                            else
                            {
                                // 更新 Job WIP
                                if (permitFlag != string.Empty)
                                {
                                    lock (job)
                                        job.CfSpecial.PermitFlag = permitFlag;
                                }
                                lock (job)
                                {
                                    job.CfSpecial.NextLineJobData = xmlDoc.InnerXml;
                                    if (job.CfSpecial.EQPFlag2.ExposureProcessFlag == "1")
                                        job.CfSpecial.ExposureDoperation = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LOTLIST][keyHost.LOT][keyHost.PROCESSOPERATIONNAME].InnerText.Trim();
                                }
                                ObjectManager.JobManager.EnqueueSave(job);
                            }
                        }
                        //若MES Reply PERMITFLAG有值的話，不論Y/N，同CST內的PermitFlag都需更新成一樣的。
                        if (permitFlag != string.Empty)
                        {
                            jobs = ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo);
                            foreach (Job j in jobs)
                            {
                                lock (j)
                                    j.CfSpecial.PermitFlag = job.CfSpecial.PermitFlag;
                            }
                        }
                        #endregion
                    }


                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES]=[{1}] CF Short Cut Permit Reply,PRODUCTNAME =[{2}],PERMITFLAG =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                        lineName, trxID, jobID, job.CfSpecial.PermitFlag, returnCode, returnMessage));
                    if(job.CfSpecial.PermitFlag == "Y")
                        Invoke(eServiceName.CFSpecialService, "PermissionRequestReportReply", new object[] { trxID, eBitResult.ON, "L21", eReturnCode1.OK });
                    else
                        Invoke(eServiceName.CFSpecialService, "PermissionRequestReportReply", new object[] { trxID, eBitResult.ON, "L21", eReturnCode1.NG });
                
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.21.	CFShortCutGlassProcessEnd       MES<-BC With  short  cut  mode, TKOUT glass that is not unloaded from a port.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="job">Job Entity</param>
        public void CFShortCutGlassProcessEnd(string trxID, Line line, Job job)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].",
                        //trxID, line.Data.LINENAME));
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CFShortCutGlassProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;               

                IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment eqp in equipments)
                {
                    if (eqp.Data.NODEATTRIBUTE.ToUpper().Trim() == "CV07")
                    {
                        bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;

                        if (eqp.File.NextLineBCStatus == eBitResult.ON &&
                            eqp.File.CV07Status == eBitResult.ON &&
                            NextLineStatusCkeck(eqp))
                        {                            
                            bodyNode[keyHost.PERMITFLAG].InnerText = job.CfSpecial.PermitFlag;
                        }
                        else
                        {
                            bodyNode[keyHost.PERMITFLAG].InnerText = "F";
                            lock (job)
                                job.CfSpecial.PermitFlag = "F";
                        }
                    }
                }

                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = "1";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                bodyNode[keyHost.CFSHORTCUTMODE].InnerText = (line.File.CFShortCutMode == eShortCutMode.Enable ? "ENABLE" : "DISABLE");

                bodyNode[keyHost.PPID].InnerText = job.PPID;
                bodyNode[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;

                XmlNode glsListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode glsNode = glsListNode[keyHost.PRODUCT].Clone();
                glsListNode.RemoveAll();

                bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;   //Watson modify 20141122 For 帶JOB層的
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME; //Watson modify 20141122 For 帶JOB層的

                XmlNode product = glsNode.Clone();

                product[keyHost.POSITION].InnerText = job.FromSlotNo;
                product[keyHost.PRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;

                product[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                product[keyHost.DENSEBOXID].InnerText = job.MesProduct.DENSEBOXID;
                product[keyHost.PRODUCTJUDGE].InnerText = job.JobJudge;
                product[keyHost.PRODUCTGRADE].InnerText = job.JobGrade;
                product[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation;

                product[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                product[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                product[keyHost.HOSTPRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;

                product[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                product[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                product[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                product[keyHost.VCRREADFLAG].InnerText = ObjectManager.JobManager.P2M_GetVCRResult(job.VCR_Result);

                #region [Abnormal Code List]
                XmlNode code;
                bool _ttp = false;
                string _side = string.Empty;

                XmlNode abromalListNode = product[keyHost.ABNORMALCODELIST];
                XmlNode codeNode = abromalListNode[keyHost.CODE];
                abromalListNode.RemoveAll();

                IList<Unit> Units = ObjectManager.UnitManager.GetUnits();

                #region OVENSIDE
                _side = string.Empty;
                if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.OVENSIDE))
                {
                    if (job.CfSpecial.TrackingData.Photo_OvenHP01 == "1") { _side = "OVENHP1"; }
                    if (job.CfSpecial.TrackingData.Photo_OvenHP02 == "1") { _side = "OVENHP2"; }
                    foreach (Unit unit in Units)
                    {
                        if (unit.Data.UNITATTRIBUTE == _side)
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                            abromalListNode.AppendChild(code);
                            lock (job)
                            { job.CfSpecial.AbnormalCode.OVENSIDE = unit.Data.UNITID; }
                            ObjectManager.JobManager.EnqueueSave(job);
                        }
                    }
                }
                else
                {
                    code = codeNode.Clone();
                    code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.OVENSIDE;
                    abromalListNode.AppendChild(code);
                }
                #endregion

                #region VCDSIDE
                _side = string.Empty;
                if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.VCDSIDE))
                {

                    if (job.CfSpecial.TrackingData.CoaterVCD01 == "1") { _side = "VCD1"; }
                    if (job.CfSpecial.TrackingData.CoaterVCD02 == "1") { _side = "VCD2"; }
                    foreach (Unit unit in Units)
                    {
                        if (unit.Data.UNITATTRIBUTE == _side)
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                            code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                            abromalListNode.AppendChild(code);
                            lock (job)
                            { job.CfSpecial.AbnormalCode.VCDSIDE = unit.Data.UNITID; }
                            ObjectManager.JobManager.EnqueueSave(job);
                        }
                    }
                }
                else
                {
                    code = codeNode.Clone();
                    code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.VCDSIDE;
                    abromalListNode.AppendChild(code);
                }
                #endregion

                #region COA2MASKEQPID
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKEQPID";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID;
                abromalListNode.AppendChild(code);
                #endregion

                #region COA2MASKNAME
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKNAME";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.COA2MASKNAME;
                abromalListNode.AppendChild(code);
                #endregion

                #region PRLOT
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "PRLOT";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.PRLOT;
                abromalListNode.AppendChild(code);
                #endregion

                #region CSPNUMBER
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "CSPNUMBER";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.CSPNUMBER;
                abromalListNode.AppendChild(code);
                #endregion

                #region HPCHAMBER
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "HPCHAMBER";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.HPCHAMBER;
                abromalListNode.AppendChild(code);
                #endregion

                #region CPCHAMBER
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "CPCHAMBER";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.CPCHAMBER;
                abromalListNode.AppendChild(code);
                #endregion

                #region DISPENSESPEED
                code = codeNode.Clone();
                code[keyHost.ABNORMALVALUE].InnerText = "DISPENSESPEED";
                code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.DISPENSESPEED;
                abromalListNode.AppendChild(code);
                #endregion
                switch (line.Data.JOBDATALINETYPE)
                {
                    case eJobDataLineType.CF.PHOTO_BMPS:
                        #region TTPFlag
                        if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 && _ttp)
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "TTPFlag";
                            code[keyHost.ABNORMALCODE].InnerText = "Y";
                            abromalListNode.AppendChild(code);
                        }
                        else if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1)
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "TTPFlag";
                            code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.TTPFLAG;
                            abromalListNode.AppendChild(code);
                        }
                        #endregion

                        #region ALNSIDE
                        _side = string.Empty;
                        if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                        {
                            if (job.CfSpecial.TrackingData.BMPS_Exposure == "1") { _side = "CP1"; }
                            if (job.CfSpecial.TrackingData.BMPS_Exposure2 == "1") { _side = "CP2"; }
                            foreach (Unit unit in Units)
                            {
                                if (unit.Data.UNITATTRIBUTE == _side)
                                {
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                    code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                    abromalListNode.AppendChild(code);
                                    lock (job)
                                    { job.CfSpecial.AbnormalCode.ALNSIDE = unit.Data.UNITID; }
                                    ObjectManager.JobManager.EnqueueSave(job);
                                }
                            }
                        }
                        else
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                            code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.ALNSIDE;
                            abromalListNode.AppendChild(code);
                        }
                        #endregion                        
                        break;
                    case eJobDataLineType.CF.PHOTO_GRB:
                        #region ALNSIDE
                        _side = string.Empty;
                        if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                        {
                            if (job.CfSpecial.TrackingData.RGB_ExposureCP01 == "1") { _side = "CP1"; }
                            if (job.CfSpecial.TrackingData.RGB_ExposureCP02 == "1") { _side = "CP2"; }
                            foreach (Unit unit in Units)
                            {
                                if (unit.Data.UNITATTRIBUTE == _side)
                                {
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                    code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                    abromalListNode.AppendChild(code);
                                    lock (job)
                                    { job.CfSpecial.AbnormalCode.ALNSIDE = unit.Data.UNITID; }
                                    ObjectManager.JobManager.EnqueueSave(job);
                                }
                            }
                        }
                        else
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                            code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.ALNSIDE;
                            abromalListNode.AppendChild(code);
                        }
                        #endregion                        
                        break;
                }
                #endregion

                if (job.HoldInforList.Count() > 0)
                {
                    product[keyHost.HOLDFLAG].InnerText = "Y";
                    string holdmachine = string.Empty;
                    string holdoperater = string.Empty;
                    for (int i = 0; i < job.HoldInforList.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(holdmachine)) holdmachine += ";";
                        holdmachine += job.HoldInforList[i].NodeID;
                        if (!string.IsNullOrEmpty(holdoperater)) holdoperater += ";";
                        holdoperater += job.HoldInforList[i].OperatorID;
                    }
                    product[keyHost.HOLDMACHINE].InnerText = holdmachine;
                    product[keyHost.HOLDOPERATOR].InnerText = holdoperater;
                }

                product[keyHost.DUMUSEDCOUNT].InnerText = job.MesProduct.DUMUSEDCOUNT;
                product[keyHost.CFTYPE1REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE1REPAIRCOUNT;
                product[keyHost.CFTYPE2REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE2REPAIRCOUNT;
                product[keyHost.CARBONREPAIRCOUNT].InnerText = job.MesProduct.CARBONREPAIRCOUNT;
                product[keyHost.LASERREPAIRCOUNT].InnerText = ""; // MES會計算, 不用填
                product[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, job.CellSpecial.CuttingFlag);
                product[keyHost.SAMPLEFLAG].InnerText = "N";

                XmlNode materialList = product[keyHost.MATERIALLIST];
                XmlNode materialClone = materialList[keyHost.MATERIAL].Clone();
                materialList.RemoveAll();

                product[keyHost.SOURCEDURABLETYPE].InnerText = "C";

                //Download by MES CFShortCutGlassProcessEndReply, and report by CF Shortcut CST ULD TKOUT, 
                //then MES will execute auto sampling by this value.
                product[keyHost.SAMPLETYPE].InnerText = "";

                product[keyHost.ABNORMALENG].InnerText = "";

                product[keyHost.PROCESSFLAG].InnerText = job.MesProduct.PROCESSFLAG;

                //Watson Modify For MES Spec 20141125
                //‘OffLine’ - equipment is in Off-line mode.
                //‘OnLineRemote’ - equipment is in Online Remote mode.
                //‘OnLineLocal’ - equipment is in Online Local mode. 
                if (line.File.HostMode == eHostMode.LOCAL)
                    product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OnLineLocal";
                if (line.File.HostMode == eHostMode.OFFLINE)
                    product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OffLine";
                if (line.File.HostMode == eHostMode.REMOTE)
                    product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OnLineRemote";

                product[keyHost.PPID].InnerText = job.PPID;
                product[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;

                //CF Photo Line 上報ALIGNER側別 UnitID.
                product[keyHost.CHAMBERNAME].InnerText = job.CfSpecial.AbnormalCode.ALNSIDE;

                if (job.CfSpecial.EQPFlag2.ExposureProcessFlag.Equals("1"))
                {
                    //CF Photo Line Aligner - 此片玻璃若是有做曝光的話，填Y，反之填N.
                    product[keyHost.PROCESSRESULT].InnerText = "Y";
                    //CF Photo Line Aligner - 此片玻璃若是有做曝光的話，則把PROCESSOPERATIONNAME轉填到EXPOSUREDOPERATION.
                    product[keyHost.EXPOSUREDOPERATION].InnerText = job.CfSpecial.ExposureDoperation;
                }
                else
                {
                    product[keyHost.PROCESSRESULT].InnerText = "N";
                }

                //CF Photo Line Aligner - 從機台上報Process Data中的 Mask ID 轉填過來.
                product[keyHost.MASKNAME].InnerText = job.CfSpecial.MaskID;
                
                //Add for MES 2015/7/27
                product[keyHost.INLINERWCOUNT].InnerText = job.CfSpecial.ReworkRealCount.ToString();
                product[keyHost.ALIGNERNAME].InnerText = job.MesProduct.ALIGNERNAME;
                product[keyHost.MACROFLAG].InnerText = job.MesProduct.MACROFLAG;
                product[keyHost.INSPECTIONFLAG].InnerText = string.Empty;
                product[keyHost.RECYCLINGFLAG].InnerText = string.Empty;
                product[keyHost.GROUPID].InnerText = string.Empty;

                glsListNode.AppendChild(product);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                    trxID, line.Data.LINEID));

                #region Short Cut Specila Rule
                //如果PermitFlag等於F，BC直接回復設備
                if (job.CfSpecial.PermitFlag == "F")
                    Invoke(eServiceName.CFSpecialService, "GlassOutResultCommand", new object[] { trxID, job, ePermitFlag.F, job.CfSpecial.SamplingValue });
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public bool NextLineStatusCkeck(Equipment eqp)
        {
            try
            {
                if (eqp.File.CV01Status == eEQPStatus.STOP || eqp.File.CleanerStatus == eEQPStatus.STOP || eqp.File.CV02Status == eEQPStatus.STOP)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.SETUP || eqp.File.CleanerStatus == eEQPStatus.SETUP || eqp.File.CV02Status == eEQPStatus.SETUP)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.PAUSE || eqp.File.CleanerStatus == eEQPStatus.PAUSE || eqp.File.CV02Status == eEQPStatus.PAUSE)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.NOUNIT || eqp.File.CleanerStatus == eEQPStatus.NOUNIT || eqp.File.CV02Status == eEQPStatus.NOUNIT)
                { return false; }
                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary>
        ///6.22.	CFShortCutGlassProcessEndReply       MES MessagetSet : CFShort CutGlass Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CFShortCutGlassProcessEndReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={2}] [BCS <- MES] [{0}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string mesJobId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;

                Job job = new Job();
                
                IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                foreach (Job js in jobs)
                {
                    if (js.EQPJobID == mesJobId)
                        job = js;
                }
                if (job == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutGlassProcessEndReply  PRODUCTNAME(Glass ID) is Null ,LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                            trxID, lineName, returnCode, returnMessage));
                }

                lock (job)
                {
                    job.CfSpecial.CFShortCutTrackOut = true;
                }

                //更新 Job WIP
                ObjectManager.JobManager.EnqueueSave(job);

                //XmlNode joblist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTLIST];
                //if (joblist != null)
                //{
                //    foreach (XmlNode jobnode in joblist)
                //    {
                //        if (jobnode == null)
                //        {
                //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutGlassProcessEndReply  PRODUCT is Null LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                //                trxID, lineName, returnCode, returnMessage));
                //            continue;
                //        }

                //        ePermitFlag permitFlag;
                //        Enum.TryParse<ePermitFlag>(jobnode[keyHost.PERMITFLAG].InnerText, out permitFlag);
                //        string mesJobId = jobnode[keyHost.PRODUCTNAME].InnerText;
                //        string sampleType = jobnode[keyHost.SAMPLETYPE].InnerText;

                //        //Job job = ObjectManager.JobManager.GetJob(mesJobId);
                //        Job job = new Job();
                //        IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                //        foreach (Job js in jobs)
                //        {
                //            if (js.EQPJobID == mesJobId)
                //                job = js;
                //        }
                //        if (job == null)
                //        {
                //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutGlassProcessEndReply  PRODUCTNAME(Glass ID) is Null ,LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                //                    trxID, lineName, returnCode, returnMessage));
                //            continue;
                //        }

                //        lock (job)
                //        {
                //            job.CfSpecial.PermitFlag = permitFlag.ToString();
                //            job.CfSpecial.SamplingValue = sampleType;
                //            job.CfSpecial.CFShortCutTrackOut = true;
                //        }
                //        //更新 Job WIP
                //        ObjectManager.JobManager.EnqueueSave(job);

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutGlassProcessEndReply  NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutGlassProcessEndReply  OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));

                    //下命令給機台
                    Invoke(eServiceName.CFSpecialService, "GlassOutResultCommand", new object[] { trxID, job, ePermitFlag.Y, "0000" });
                }
                        //目前MES只會Download一片Job Data
                        //return;
                    //}
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.23.	CFShortCutModeChangeRequest     MES<-BC Request when CF ShortCut Mode has been changed.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">CFShortCutMode(Y/N)</param>
        public void CFShortCutModeChangeRequest(string trxID, string lineName, string cfshortcutmode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] CFShortCutModeChangeRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CFShortCutModeChangeRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.CFSHORTCUTMODE].InnerText = cfshortcutmode;

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
        /// 6.24.	CFShortCutModeChangeReply       MES MessagetSet : CF ShortCut Mode Change Reply
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_CFShortCutModeChangeReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                string eqpNo = string.Empty;

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                #region 取得機台資訊
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment eqp in eqps)
                {
                    if (eqp.Data.NODENO.Equals("L21"))//CV#06是L21
                    {
                        eqpNo = eqp.Data.NODENO;
                    }
                }
                #endregion

                string cfshortcutmode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CFSHORTCUTMODE].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                if (valiresult != "Y")
                {
                    // Send CIM Message to EQP
                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqpNo, "CF ShortCut Mode Change MES Reply NG", "MES" });
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS <- MES][{0}] CFShortCutModeChangeReply OK LINENAME=[{1}],CFSHORTCUTMODE=[{2}],VALIRESULT=[{3}].",
                    trxID, lineName, cfshortcutmode, valiresult));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public bool CheckNextLineStatus(Equipment eqp, Job job)
        {
            try
            {
                if (eqp.File.CV01Status == eEQPStatus.IDLE && eqp.File.CV02Status == eEQPStatus.IDLE && eqp.File.CleanerStatus == eEQPStatus.IDLE)
                    return true;
                else if (eqp.File.CV01Status == eEQPStatus.RUN || eqp.File.CV02Status == eEQPStatus.RUN || eqp.File.CleanerStatus == eEQPStatus.RUN)
                {
                    if (job.ProductType.Value.ToString() == eqp.File.CV01ProductType.ToString())
                        if (job.ProductType.Value.ToString() == eqp.File.CV02ProductType.ToString())
                            if (job.ProductType.Value.ToString() == eqp.File.CleanerProductType.ToString())
                                return true;
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            } 
        }

    }
}
