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
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.IO;
using System.Collections;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.UIService
{
    public partial class UIService
    {
        enum eCVD_MIX_TYPE
        {
            UNKNOWN,
            TYPE1_LT_HT,
            TYPE2_IGZO_TFT,
            TYPE3_IGZO_MQC,
            TYPE4_TFT_MQC
        }

        enum eDRY_MIX_TYPE
        {
            UNKNOWN,
            TYPE1_TFT_ENG,
            TYPE2_TFT_IGZO,
            TYPE3_IGZO_MQC
        }

        /// <summary>
        /// OPI MessageSet: Offline Mode Cassette Data Send
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_OfflineModeCassetteDataSend(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            OfflineModeCassetteDataSend command = Spec.XMLtoMessage(xmlDoc) as OfflineModeCassetteDataSend;
            XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply") as XmlDocument;
            OfflineModeCassetteDataSendReply reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply;

            try
            {
                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                reply.BODY.PORTNO = command.BODY.PORTNO;
                reply.BODY.PORTID = command.BODY.PORTID;
                reply.BODY.CASSETTEID = command.BODY.CASSETTEID.Trim();

                reply.RETURN.RETURNCODE = "0000000";

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message=[{2}] from OPI. EQUIPMENTNO={3}, PORTNO={4}, CASSETTEID={5}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                #region Check Line Information
                Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                #region ServerName != LineName 的處理方式
                //Watson 20150301 Add If ServerName != Line ID
                if (command.BODY.LINENAME.Contains(eLineType.CELL.CBPMT))
                {
                    line = GetLineByLines(keyCELLPMTLINE.CBPMI);
                }
                #endregion

                if (line == null)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));

                    OfflineModeCassetteDataSendReply(command, "0010745", string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));
                    return;
                }

                if (line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        "BC Can Not Handle CST Data Download At EXCHANGE Mode[OFFLINE], Pls Change to Changer Mode");

                    OfflineModeCassetteDataSendReply(command, "0010748", "BC Can Not Handle CST Data Download At EXCHANGE Mode[OFFLINE], Pls Change to Changer Mode");
                    return;
                }
                #endregion

                #region Check Equipment Information
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                if (eqp == null)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment=[{0}] in EquipmentEntity.",
                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                    OfflineModeCassetteDataSendReply(command, "0010732", string.Format("Can't find Equipment=[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO));
                    return;
                }

                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment=[{0}] CIM Mode Off.",
                    command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                    OfflineModeCassetteDataSendReply(command, "0010733", string.Format("Equipment[{0}] CIM Mode Off", command.BODY.EQUIPMENTNO));
                    return;
                }
                #endregion

                #region Check Port Information
                Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                if (port == null)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port=[{0}] in PortEntity.",
                    command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                    OfflineModeCassetteDataSendReply(command, "0010731", string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTID));
                    return;
                }
                if (port.File.Type == ePortType.Unknown)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortType=[{2}] Invalid.",
                    command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Type.ToString()));
                    OfflineModeCassetteDataSendReply(command, "0010742", string.Format("Port[{0}] PortType[{1}] Invalid", command.BODY.PORTID, port.File.Type.ToString()));
                    return;
                }
                else if (port.File.Mode == ePortMode.Unknown)
                {
                    //CELL CCPCK 不會上報Port Mode
                    if (!port.Data.LINEID.Contains("CCPCK"))
                    {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortMode=[{2}] Invalid.",
                        command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Mode.ToString()));
                        OfflineModeCassetteDataSendReply(command, "0010743", string.Format("Port[{0}] PortMode[{1}] Invalid", command.BODY.PORTID, port.File.Mode.ToString()));
                        return;
                    }
                }
                else if (port.File.EnableMode != ePortEnableMode.Enabled)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortEnableMode=[{2}] Invalid.",
                    command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.EnableMode.ToString()));
                    OfflineModeCassetteDataSendReply(command, "0010746", string.Format("Port[{0}] PortEnableMode[{1}] Invalid", command.BODY.PORTID, port.File.EnableMode.ToString()));
                    return;

                }
                else if (port.File.Status != ePortStatus.LC)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortStatus=[{2}] Invalid.",
                    command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Status.ToString()));
                    OfflineModeCassetteDataSendReply(command, "0010734", string.Format("Port[{0}] PortStatus[{1}] Invalid", command.BODY.PORTID, port.File.Status));
                    return;
                }
                else if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA && port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[PORT={0}] [BCS <- OPI][{1}] Cassette Status=[{2}] is not available.",
                        command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.CassetteStatus.ToString()));
                    OfflineModeCassetteDataSendReply(command, "0010737", string.Format("Cassette Status=[{0}] is not available", port.File.CassetteStatus.ToString()));
                    return;
                }
                #endregion

                //Jun Modify 20150204
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                #region Check Cassette Information
                //Jun Modify 20150204
                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                    cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);

                if (cst == null)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette=[{0}] in CassetteEntity.",
                    command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID));
                    OfflineModeCassetteDataSendReply(command, "0010730", string.Format("Can't find Cassette[{0}] in CassetteEntity.", command.BODY.CASSETTEID.Trim()));
                    return;
                }
                if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
                {
                    if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                    { }
                    else
                    {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette SeqNo. Cassette SeqNo=[{2}] / Port Cassette SeqNo=[{3}].",
                        command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteSequenceNo, port.File.CassetteSequenceNo));
                        OfflineModeCassetteDataSendReply(command, "0010735", string.Format("Different Cassette SeqNo"));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(cst.CassetteID.Trim()) && !cst.CassetteID.Trim().Equals(command.BODY.CASSETTEID.Trim()))
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette ID. Cassette ID=[{2}] / OPI Cassette ID=[{0}].",
                        command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteID.Trim()));
                    OfflineModeCassetteDataSendReply(command, "0010736", string.Format("Different Cassette ID"));
                    return;
                }

                else if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Cassette have been Cancel.",
                        command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                    OfflineModeCassetteDataSendReply(command, "0010741", "Cassette have been Cancel");
                    return;
                }
                #endregion

                string errMsg = string.Empty;
                bool bRemap = command.BODY.REMAPFLAG == "Y" ? true : false;

                if (!ValidateCassetteCheckData(fabType, command, command.HEADER.TRANSACTIONID, ref line, ref eqp, ref port, ref cst, out errMsg))
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] ValidateCassetteCheckData fail. {5}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, errMsg));

                    OfflineModeCassetteDataSendReply(command, "0010744", string.Format("ValidateCassetteCheckData fail, {0}", errMsg));
                    return;
                }
                else
                {
                    XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                    XmlNodeList lotNodeList = body["LOTLIST"].ChildNodes;

                    MESDataIntoCstObject(body, ref cst);

                    lock (cst)
                    {
                        cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;
                        cst.CassetteID = command.BODY.CASSETTEID;

                        if (fabType == eFabType.CELL)
                            cst.LDCassetteSettingCode = command.BODY.CSTSETTINGCODE;
                    }
                    ObjectManager.CassetteManager.EnqueueSave(cst);

                    // Array BFG 機台 檢查SCRAPCUTFLAG Item 
                    if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG)
                    {
                        if (CheckTBBFG_SCRAPCUTFLAG(xmlDoc) == false)
                        {
                            string err = string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] There is no \"S\" or \"C\" value in each SCRAPCUTFLAG under all Product list of all Lot list",
                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                            OfflineModeCassetteDataSendReply(command, "0010747", string.Format("There is no \"S\" or \"C\" value in each SCRAPCUTFLAG under all Product list of all Lot list."));
                            return;
                        }
                    }
                    if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                    {
                        if (CheckGAPRecipe(xmlDoc, port, eqp, ref errMsg) == false)
                        {
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", errMsg);
                            OfflineModeCassetteDataSendReply(command, "0010744", string.Format("ValidateCassetteCheckData fail, {0}", errMsg));
                            return;
                        }
                    }
                    //儲存生成的Job
                    IList<Job> jobs = new List<Job>();
                    // Array Use : 計算Recipe Group Number
                    Dictionary<string, string> recipeGroup = new Dictionary<string, string>();
                    IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
                    IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

                    int i = 0;
                    foreach (OfflineModeCassetteDataSend.LOTDATAc lot in command.BODY.LOTLIST)
                    {
                        XmlNode lotNode = lotNodeList[i];

                        LOTc _lot = new LOTc();
                       // _lot.LOTNAME = lot.LOTNAME;   //modify by yang 2017/3/14,offline new一个lot出来
                        _lot.LOTNAME = "OFFLINE" + command.BODY.LINENAME + DateTime.Now.ToString("yyMMddHHmmss");
                        _lot.PROCESSOPERATIONNAME = lot.PROCESSOPERATIONNAME;
                        _lot.PRODUCTOWNER = lot.PRODUCTOWNER;
                        _lot.PRODUCTSPECNAME = lot.PRODUCTSPECNAME;
                        _lot.BCPRODUCTTYPE = lot.BCPRODUCTTYPE;
                        _lot.CFREWORKCOUNT = lot.CFREWORKCOUNT;
                        //_lot.PRODUCTSIZE = lot.PRODUCTSIZE;
                        _lot.LINERECIPENAME = lot.LINERECIPENAME;
                        _lot.PPID = lot.PPID;
                        _lot.PRDCARRIERSETCODE = lot.CSTSETTINGCODE;
                        //_lot.CFINLINEREWORKMAXCOUNT = lot.INLINE_REWORK_MAX_COUNT; // 20150727 Add by Frank For CF Photo Line 
                        _lot.TARGETCSTID_CF = lot.TARGETCSTID_CF;   // 20150727 Add by Frank For CF Photo Line

                        foreach (OfflineModeCassetteDataSend.PROCESSLINEc process in lot.PROCESSLINELIST)
                        {
                            PROCESSLINEc _process = new PROCESSLINEc();
                            _process.LINENAME = process.LINENAME;
                            _process.LINERECIPENAME = process.LINERECIPENAME;
                            _process.PPID = process.PPID;
                            _process.CARRIERSETCODE = process.CSTSETTINGCODE;

                            _lot.PROCESSLINELIST.Add(_process);
                        }

                        foreach (OfflineModeCassetteDataSend.STBPRODUCTSPECc stb in lot.STBPRODUCTSPECLIST)
                        {
                            STBPRODUCTSPECc _stb = new STBPRODUCTSPECc();
                            _stb.LINENAME = stb.LINENAME;
                            _stb.LINERECIPENAME = stb.LINERECIPENAME;
                            _stb.PPID = stb.PPID;
                            _stb.CARRIERSETCODE = stb.CSTSETTINGCODE;

                            _lot.STBPRODUCTSPECLIST.Add(_stb);
                        }

                        int j = 0;
                        XmlNodeList productList = lotNode["PRODUCTLIST"].ChildNodes;


                        foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                        {
                            XmlNode productNode = productList[j];

                            int iSlotNo = int.Parse(product.SLOTNO);

                            if (iSlotNo <= 0)
                            {
                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] OPI cassette data SlotNo=[{3}] is wrong.",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, product.SLOTNO));

                                OfflineModeCassetteDataSendReply(command, "0010739", string.Format("OPI cassette data SlotNo[{0}] is wrong", product.SLOTNO));
                                return;
                            }
                            //UPK Line需要過濾掉 2015/1/30  不是UPK的也要产生Job
                            else if (line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1)
                            {
                                if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Existence，不需要進行比對
                                {
                                    if (!port.File.ArrayJobExistenceSlot[iSlotNo - 1])
                                    {
                                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette SlotMap Mismatch. SlotNo=[{5}] isn't exist.",
                                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, product.SLOTNO));

                                        OfflineModeCassetteDataSendReply(command, "0010740", string.Format("Cassette SlotMap Mismatch"));
                                        return;
                                    }
                                }
                            }

                            #region Create Job
                            {
                                _lot.BCPRODUCTTYPE = product.PRODUCTTYPE;

                                PRODUCTc _product = new PRODUCTc();
                                _product.POSITION = product.SLOTNO;
                                _product.PROCESSFLAG = product.PROCESSFLAG;
                                _product.PRODUCTNAME = product.PRODUCTNAME;
                                _product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
                                _product.PPID = product.OPI_PPID;
                                _product.PRODUCTTYPE = product.PRODUCTTYPE;
                                _product.PRODUCTGRADE = product.PRODUCTGRADE;
                                _product.PRODUCTJUDGE = product.PRODUCTJUDGE;
                                _product.GROUPID = product.GROUPID;
                                //_product.TEMPERATUREFLAG = product.TEMPERATUREFLAG;
                                //MSE ARRAYPRODUCTSPECVER = OPI COAVERSION
                                _product.ARRAYPRODUCTSPECVER = product.COAVERSION;
                                _product.OWNERTYPE = product.OWNERTYPE;
                                _product.OWNERID = product.OWNERID;
                                _product.REVPROCESSOPERATIONNAME = product.REVPROCESSOPERATIONNAME;
                                //_product.SCRAPCUTFLAG = product.SCRAPCUTFLAG;
                                _product.TARGET_SLOTNO = product.TARGET_SLOTNO; // 20150727 Add by Frank For CF Photo Line

                                _lot.PRODUCTLIST.Add(_product);

                                //Create Job Data
                                Job job = null;

                                //Remap的時候，需清掉部分資訊
                                if (bRemap)
                                {
                                    job = ObjectManager.JobManager.GetJob(port.File.CassetteSequenceNo, iSlotNo.ToString());
                                    if (job != null)
                                    {
                                        if (productList[j][keyHost.PROCESSFLAG].InnerText.Equals("Y"))
                                        {
                                            job.TargetPortID = "0";
                                            job.ToSlotNo = "0";
                                            job.TrackingData = SpecialItemInitial("TrackingData", 32);
                                            job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                            job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
                                            job.EQPFlag = new string('0', 32);
                                            job.HoldInforList.Clear();
                                            job.JobProcessStartTime = DateTime.Now;
                                            if (job.QtimeList != null) job.QtimeList.Clear();
                                            job.DefectCodes.Clear();
                                            job.JobProcessFlows.Clear();
                                            job.MesCstBody.LOTLIST.Clear();
                                        }
                                    }
                                }

                                if (job == null)
                                {
                                    job = new Job(int.Parse(cst.CassetteSequenceNo), iSlotNo);
                                    job.TrackingData = SpecialItemInitial("TrackingData", 32);
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
                                }

                                job.CIMMode = eBitResult.ON;
                                //job.MesCstBody.LINEOPERMODE = command.BODY.LINEOPERMODE;
                                job.MesCstBody.PRODUCTQUANTITY = command.BODY.PRODUCTQUANTITY;
                                //job.MesCstBody.CLEANFLAG = command.BODY.CLEANFLAG;
                                job.MesCstBody.LOTLIST.Add(_lot);
                                job.MesProduct = _product;
                                job.GroupIndex = string.IsNullOrEmpty(product.GROUPID) ? "1" : product.GROUPID;
                                job.SamplingSlotFlag = product.PROCESSFLAG == "Y" ? "1" : "0";
                                job.RobotProcessFlag = product.PROCESSFLAG == "Y" ? keyCELLROBOTProcessFlag.WAIT_PROCESS : keyCELLROBOTProcessFlag.NO_PROCESS; //Watson Add 20150316 For Auto Abort Cassette.
                                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)  //Jun Modify 20150507 CBUAM不寫GlassID，寫到MaskID
                                    job.GlassChipMaskBlockID = product.PRODUCTNAME;
                                else
                                    job.CellSpecial.MASKID = product.PRODUCTNAME;

                                job.EQPJobID = product.PRODUCTNAME;
                                job.SourcePortID = port.Data.PORTID;
                                job.FromCstID = port.File.CassetteID;
                                int slotno = 0;
                                int.TryParse(product.SLOTNO, out slotno);
                                job.FromSlotNo = slotno.ToString();
                                //先預設給0
                                job.ToSlotNo = "0";
                                job.CurrentSlotNo = job.FromSlotNo; // add by bruce 20160412 for T2 Issue
                                job.CurrentEQPNo = eqp.Data.NODENO;
                                job.LineRecipeName = product.PRODUCTRECIPENAME;
                                job.JobGrade = product.PRODUCTGRADE;
                                job.JobJudge = product.PRODUCTJUDGE;
                                //job.JobJudge = "0"; //参照ValidateCassette 默认都给0  tom 20141202
                                job.INSPReservations = product.INSPRESERVATION;
                                int itype = int.Parse(product.PRODUCTTYPE);
                                job.JobType = (eJobType)itype;
                                int substrateType = int.Parse(product.SUBSTRATETYPE);
                                job.SubstrateType = (eSubstrateType)substrateType;
                                job.ProductType.Value = string.IsNullOrEmpty(lot.BCPRODUCTTYPE) ? 0 : int.Parse(lot.BCPRODUCTTYPE);
                                //job.CellSpecial.POLProductType = lot.BCPRODUCTTYPE_POL;
                                //job.CellSpecial.CUTProductType = lot.BCPRODUCTTYPE_CUT;
                                //job.CellSpecial.CUTCrossProductType = lot.BCPRODUCTTYPE_CUT_CROSS;
                                job.ProductID.Value = string.IsNullOrEmpty(lot.PRODUCTID) ? 0 : int.Parse(lot.PRODUCTID);
                                job.CellSpecial.ProductID = job.ProductID.Value.ToString();
                                //job.CellSpecial.POLProductID = lot.PRODUCTID_POL;
                                //job.CellSpecial.CUTProductID = lot.PRODUCTID_CUT;
                                //job.CellSpecial.CUTCrossProductID = lot.PRODUCTID_CUT_CROSS;
                                //job.CellSpecial.TurnAngle = product.TRUNANGLEFLAG;
                                //job.CellSpecial.PanelSize = product.PANELSIZE;
                                job.OXRInformation = product.OXR;
                                //job.CellSpecial.CrossLinePanelSize = product.PANELSIZE_CROSS;
                                //job.CellSpecial.CrossLinePanelSize = product.PANELSIZEFLAG_CROSS;
                                //job.CellSpecial.ReworkCount = product.REWORKCOUNT;
                                //job.CellSpecial.ArrayTTPEQVer = product.ARRAYTTPEQVERSION;
                                //job.CellSpecial.NodeStack = product.NODESTACK;
                                //job.CellSpecial.RepairResult = product.REPAIRRESULT;


                                //IDictionary<string, string> cutSub = ObjectManager.SubJobDataManager.GetSubItem("PanelSizeFlag");
                                //if (cutSub != null)
                                //{
                                //    if (cutSub.ContainsKey("PanelSizeFlag") && cutSub.ContainsKey("CrossLinePanelSizeFlag"))
                                //    {
                                //        cutSub["PanelSizeFlag"] = product.PANELSIZEFLAG;
                                //        //cutSub["CrossLinePanelSizeFlag"] = product.PANELSIZEFLAG_CROSS;
                                //    }
                                //    job.CellSpecial.PanelSizeFlag = ObjectManager.SubJobDataManager.Encode(cutSub, "PanelSizeFlag");
                                //}

                                job.TargetCSTID = product.TARGETCSTID;
                                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                {
                                    job.SamplingSlotFlag = "1"; //for curr and standby plan use
                                    /*
                                    // 資料來源由 PlanManager
                                    IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(line.File.CurrentPlanID, port.File.CassetteID.Trim());
                                    SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == job.GlassChipMaskBlockID.Trim());
                                    if (plan != null)
                                    {
                                        job.TargetCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                        // 跟登今確認, 在Changer Mode只要有Plan就是要抽, 即使MES 的原資料是不抽, 也要turn On
                                        if (!string.IsNullOrEmpty(job.TargetCSTID.Trim()))
                                        {
                                            job.SamplingSlotFlag = "1";
                                        }
                                        else
                                        {
                                            job.SamplingSlotFlag = "0";
                                        }
                                    }
                                    */
                                }
                                job.EQPFlag = product.EQPFLAG;
                                job.MesProduct = _product;

                                //CF Photo Line Unloader沒有CSTOperationMode By Jm.Pan
                                if (fabType == eFabType.CF && port.File.Type == ePortType.UnloadingPort &&
                                    (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS || line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB))
                                {
                                    Equipment eqps = ObjectManager.EquipmentManager.GetEQP("L2");
                                    job.CSTOperationMode = eqps.File.CSTOperationMode;

                                }
                                else
                                {
                                    job.CSTOperationMode = eqp.File.CSTOperationMode;
                                }
                                //job.ChipCount = 0; 20160518 Modify by Frank
                                if (string.IsNullOrEmpty(job.OXRInformation))
                                    job.ChipCount = 0;
                                else
                                    job.ChipCount = job.OXRInformation.Length;

                                #region PPID計算
                                string ppid = string.Empty;
                                string mesPPID = string.Empty;
                                string crossPPID = string.Empty;
                                string err = string.Empty;


                                if (fabType != eFabType.CELL)
                                {
                                    if (ObjectManager.JobManager.AnalysisMesPPID_AC(productNode, line, port, product.PROCESSTYPE, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
                                    {
                                        job.PPID = ppid;
                                        job.MES_PPID = mesPPID;
                                    }
                                    if (err.Trim() != string.Empty)
                                    {
                                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                        OfflineModeCassetteDataSendReply(command, "0010741", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                        return;
                                    }
                                }
                                else
                                {
                                    //20151225 cy:T3目前的CELL無特殊邏輯,只保留Normal的部份就好
                                    if (ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
                                    {
                                        job.PPID = ppid;
                                        job.MES_PPID = mesPPID;
                                    }
                                    if (err.Trim() != string.Empty)
                                    {
                                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                        OfflineModeCassetteDataSendReply(command, "0010742", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                        return;
                                    }
                                    #region Mark T2 CELL Normal/Cut rule
                                    //if (line.Data.JOBDATALINETYPE != eJobDataLineType.CELL.CBCUT)
                                    //{
                                    //      if (ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
                                    //      {
                                    //            job.PPID = ppid;
                                    //            job.MES_PPID = mesPPID;
                                    //      }
                                    //      if (err.Trim() != string.Empty)
                                    //      {
                                    //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    //                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                    //                    command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                    //            OfflineModeCassetteDataSendReply(command, "0010742", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                    //            return;
                                    //      }
                                    //}
                                    //else
                                    //{
                                    //      switch (line.Data.LINETYPE)
                                    //      {
                                    //            case eLineType.CELL.CBCUT_1:
                                    //                  if (ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_1(body, lotNode, productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out crossPPID, out mesPPID, out err))
                                    //                  {
                                    //                        job.PPID = ppid;
                                    //                        job.MES_PPID = mesPPID;
                                    //                        job.CellSpecial.CrossLinePPID = crossPPID;
                                    //                  }
                                    //                  if (err.Trim() != string.Empty)
                                    //                  {
                                    //                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    //                            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                    //                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                    //                        OfflineModeCassetteDataSendReply(command, "0010743", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                    //                        return;
                                    //                  }
                                    //                  break;
                                    //            case eLineType.CELL.CBCUT_2:
                                    //                  if (ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_2(body, lotNode, productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out crossPPID, out mesPPID, out err))
                                    //                  {
                                    //                        job.PPID = ppid;
                                    //                        job.MES_PPID = mesPPID;
                                    //                        job.CellSpecial.CrossLinePPID = crossPPID;
                                    //                  }
                                    //                  if (err.Trim() != string.Empty)
                                    //                  {
                                    //                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    //                            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                    //                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                    //                        OfflineModeCassetteDataSendReply(command, "0010744", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                    //                        return;
                                    //                  }
                                    //                  break;
                                    //            case eLineType.CELL.CBCUT_3:
                                    //                  if (ObjectManager.JobManager.AnalysisMesPPID_CELL_CUT_3(body, lotNode, productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
                                    //                  {
                                    //                        job.PPID = ppid;
                                    //                        job.MES_PPID = mesPPID;
                                    //                  }

                                    //                  if (err.Trim() != string.Empty)
                                    //                  {
                                    //                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    //                            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
                                    //                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
                                    //                        OfflineModeCassetteDataSendReply(command, "0010745", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
                                    //                        return;
                                    //                  }
                                    //                  break;
                                    //      }
                                    //}
                                    #endregion
                                }

                                #endregion

                                #region 各Shop Job Special Data

                                switch (fabType)
                                {
                                    case eFabType.ARRAY: M2P_SpecialDataBy_Array(lotNode, productNode, line, port, ref recipeGroup, ref job); break;
                                    case eFabType.CF: M2P_SpecialDataBy_CF(body, lotNode, productList[j], line, ref job); break;
                                    case eFabType.CELL: M2P_SpecialDataBy_CELL(lotNode, productList[j], j, line, port, ref job); break;
                                }

                                #endregion

                                job.ArraySpecial.ProcessType = product.PROCESSTYPE == "" ? "0" : product.PROCESSTYPE;
                                //job.CfSpecial.FlowPriorityInfo = product.FLOWPRIORITY == "" ? "0" : product.FLOWPRIORITY;
                                job.CfSpecial.FlowPriorityInfo = FlowPriority(product.FLOWPRIORITY);
                                job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID = product.PREINLINEID == "" ? "0" : product.PREINLINEID;
                                job.CfSpecial.COAversion = product.COAVERSION;
                                job.CfSpecial.ReworkMaxCount = lot.CFREWORKCOUNT;
                                job.CfSpecial.InlineReworkMaxCount = product.CFINLINEREWORKMAXCOUNT;    //20150925 Add by Frank For CF Photo Line
                                //// 20150812 Add by Frank
                                #region CStOprationMode = LTOL
                                if (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL)
                                {
                                    job.CfSpecial.TargetCSTID = port.File.CassetteID;
                                    job.CfSpecial.TargetSlotNo = iSlotNo.ToString();
                                }
                                #endregion
                                //job.CellSpecial.AbnormalCode = product.ABNORMALCODE;
                                //job.CellSpecial.RepairCount = product.REPAIRCOUNT == "" ? "0" : product.REPAIRCOUNT;
                                //job.CellSpecial.CuttingFlag = string.IsNullOrEmpty(product.CUTTINGFLAG) ? eCUTTING_FLAG.NOT_CUTTING : (eCUTTING_FLAG)int.Parse(product.CUTTINGFLAG);
                                job.CellSpecial.OwnerID = product.OWNERID;

                                //Watson Add 20150324
                                ObjectManager.QtimeManager.ValidateQTimeSetting(job);

                                #region Robot Create WIP   Return Failed Message and Cassette Command Failed!
                                //Watson Modify 20151106 For 新增錯誤訊息及重大錯誤不能再下貨成功!!
                                //20150817 add for [ Robot Offline Create RobotWIP ]
                                //轉拋給RobotService協助確認Create Robot WIP 是否有錯誤產生
                                string returnMsg = string.Empty;
                                object[] parameters = new object[] { eqp.Data.NODENO, job, returnMsg };
                                bool result = (bool)Invoke(eServiceName.RobotCoreService, "CreateJobRobotWIPInfo", parameters,
                                        new Type[] { typeof(string), typeof(Job), typeof(string).MakeByRefType() });
                                returnMsg = (string)parameters[2];

                                if ((!result) && (returnMsg != string.Empty))
                                {
                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette Robot Route Create Failed!! Reason=[{5}]",
                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, returnMsg));
                                    if (ParameterManager[eRobot_Check_ByPass.ROBOT_CHECK_ROUTE_BYPASS].GetBoolean())
                                    {
                                        OfflineModeCassetteDataSendReply(command, "0010739", string.Format("Cassette Job Robot Control Route Failed!! " + returnMsg));
                                        return;
                                    }
                                }
                                #endregion

                                jobs.Add(job);
                            }
                            #endregion
                            j++;
                        }
                        i++;
                    }

                    if (fabType == eFabType.ARRAY && recipeGroup.Count > 30) //add  cc.kuang 2016/03/09
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Recipe Group Count > 30, Pls Check the jobs on CST.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                        OfflineModeCassetteDataSendReply(command, "0010748", string.Format("Recipe Group Count > 30, Pls Check the jobs on CST."));
                        return;
                    }


                    if (command.BODY.PRODUCTQUANTITY != port.File.JobCountInCassette)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette Job Count Mismatch. Cassette Job Count=[{5}] OPI PRODUCTQUANTITY=[{6}].",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, port.File.JobCountInCassette, command.BODY.PRODUCTQUANTITY));

                        OfflineModeCassetteDataSendReply(command, "0010738", string.Format("Cassette Job Count Mismatch"));
                        return;
                    }

                    //將資料放進JobManager
                    ObjectManager.JobManager.AddJobs(jobs);
                    //ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), this.CreateTrxID());
                    ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), command.HEADER.TRANSACTIONID);

                    //Jun Add 20150114 Create Robot WIP In Offline
                    //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                    //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                    //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, jobs);

                    //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
                    port.File.RobotWaitProcCount = jobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;


                    //Download to PLC
                    //sy modify By T3 IO同CST  so Mark
                    //if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                    //{
                    //      Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    //          string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Invoke CassetteService DPCassetteMapDownlad.",
                    //          command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                    //      Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownload", new object[] { eqp, port, jobs });
                    //}
                    //else
                    //{
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Invoke CassetteService CassetteMapDownload.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                    Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, jobs, command.HEADER.TRANSACTIONID });
                    //}

                    #region Delete Old FTP file
                    ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                    string Path = @"D:\FileData\";
                    string subPath2 = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                    DateTime now = DateTime.Now;
                    int iOverTime = 10;
                    //for t2,t3 sync 2016/04/25 cc.kuang
                    if (para.ContainsKey("FileFormatPath") && para["FileFormatPath"].Value != null)
                    {
                        Path = para["FileFormatPath"].GetString();
                    }

                    if (para.ContainsKey("FileFormatOverTimer") && para["FileFormatOverTimer"].Value != null)
                    {
                        iOverTime = para["FileFormatOverTimer"].GetInteger();
                    }

                    if (Directory.Exists(Path + subPath2))
                    {
                        string[] fileEntries = Directory.GetFiles(Path + subPath2);

                        foreach (string fileName in fileEntries)
                        {
                            DateTime dt = Directory.GetLastWriteTime(fileName);
                            if (dt.AddDays(iOverTime).CompareTo(now) < 0)
                            {
                                if (File.Exists(fileName))
                                {
                                    File.Delete(fileName);
                                }
                            }
                        }
                    }
                    #endregion

                            #region Ftp File
                            if (port.File.Type != ePortType.UnloadingPort)
                            {
                                string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                                for (int k = 0; k < jobs.Count(); k++)
                                {
                                    if (fabType != eFabType.CELL)
                                        FileFormatManager.CreateFormatFile("ACShop", subPath, jobs[k], true);
                                    else
                                    {
                                        switch (line.Data.LINETYPE)
                                        {
                                            #region [T2]
                                            case eLineType.CELL.CBPIL:
                                            case eLineType.CELL.CBODF:
                                            case eLineType.CELL.CBHVA:
                                            case eLineType.CELL.CBPMT:
                                            case eLineType.CELL.CBGAP:
                                            case eLineType.CELL.CBUVA:
                                            case eLineType.CELL.CBMCL:
                                            case eLineType.CELL.CBATS:
                                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, jobs[k], true);
                                                break;

                                            case eLineType.CELL.CBLOI:// LOI 线要产生两种类型的File Data  20150313 Tom
                                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, jobs[k], true);
                                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
                                                break;
                                            case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                            case eLineType.CELL.CBCUT_2:
                                            case eLineType.CELL.CBCUT_3:
                                                if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                                    port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                                {
                                                    //break;
                                                }
                                                else if (port.Data.PORTID == "07" || port.Data.PORTID == "08" ||
                                                         port.Data.PORTID == "C07" || port.Data.PORTID == "C08" ||
                                                         port.Data.PORTID == "P01" || port.Data.PORTID == "P02" ||
                                                         port.Data.PORTID == "P03" || port.Data.PORTID == "P04" || port.Data.PORTID == "P06")
                                                {
                                                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, jobs[k], true);
                                                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
                                                }
                                                break;
                                            #endregion
                                            default://T3 使用default 集中管理 sy
                                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, jobs[k], port });
                                                //FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
                                                break;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Check MplcInterlock
                            //Check Mplcinterlock On By Port
                            if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort && port.File.Mode != ePortMode.Dummy)
                            {
                                string productType = "";
                                for (int j = 0; j < jobs.Count(); j++)
                                {
                                    productType = jobs[j].ProductType.Value.ToString();
                                    if (productType != "")
                                    {
                                        break;
                                    }
                                }
                                if (productType != "")
                                {
                                    string portNo = "P" + port.Data.PORTNO;
                                    Invoke(eServiceName.SubBlockService, "CheckMplcBlockByPort", new object[] { eqp.Data.NODENO, portNo, productType });
                                }
                            }
                            #endregion

                            #region CF Exposure Special Rule
                            if (port.File.Type == ePortType.LoadingPort)
                            {
                                if ((line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                                    (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) ||
                                    (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1))
                                {
                                    object[] _object = new object[]
                    {
                        command.HEADER.TRANSACTIONID,
                        command.BODY.LINENAME,
                        idCheckInfos
                    };
                                    Invoke(eServiceName.CFSpecialService, "AnalysisExposuePPID", _object);
                                }
                            }
                            #endregion

                        }

                        OfflineModeCassetteDataSendReply(command, "0000000", "");

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] UIService reply message=[{5}] to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Offline Mode Cassette Data Send for DPI
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_OfflineModeCassetteDataSend_DPI(XmlDocument xmlDoc)
            //{
            //    IServerAgent agent = GetServerAgent();
            //    OfflineModeCassetteDataSend_DPI command = Spec.XMLtoMessage(xmlDoc) as OfflineModeCassetteDataSend_DPI;
            //    XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply_DPI") as XmlDocument;
            //    OfflineModeCassetteDataSendReply_DPI reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply_DPI;

            //    try
            //    {
            //        reply.BODY.LINENAME = command.BODY.LINENAME;
            //        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

            //        reply.RETURN.RETURNCODE = "0000000";

            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message=[{2}] from OPI. EQUIPMENTNO={3}",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

            //        #region Check Line Information
            //        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

            //        if (line == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));

            //            OfflineModeCassetteDataSendReply_DPI(command, "0010745", string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));
            //            return;
            //        }
            //        #endregion

            //        #region Check Equipment Information
            //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
            //        if (eqp == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment=[{0}] in EquipmentEntity.",
            //            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_DPI(command, "0010732", string.Format("Can't find Equipment=[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO));
            //            return;
            //        }

            //        if (eqp.File.CIMMode == eBitResult.OFF)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment=[{0}] CIM Mode Off.",
            //            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_DPI(command, "0010733", string.Format("Equipment[{0}] CIM Mode Off", command.BODY.EQUIPMENTNO));
            //            return;
            //        }
            //        #endregion

            //        reply.BODY.PORTLIST.Clear();

            //        int index = 0;
            //        foreach (OfflineModeCassetteDataSend_DPI.PORTc _port in command.BODY.PORTLIST)
            //        {
            //            //reply.BODY.PORTNO = command.BODY.PORTNO;
            //            //reply.BODY.PORTID = command.BODY.PORTID;
            //            //reply.BODY.CASSETTEID = command.BODY.CASSETTEID.Trim();

            //            #region Check Port Information
            //            Port port = ObjectManager.PortManager.GetPort(_port.PORTID);
            //            if (port == null)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port=[{0}] in PortEntity.",
            //                _port.PORTID, command.HEADER.TRANSACTIONID));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010731", string.Format("Can't find Port[{0}] in PortEntity", _port.PORTID));
            //                return;
            //            }
            //            if (port.File.Type == ePortType.Unknown)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortType=[{2}] Invalid.",
            //                _port.PORTID, command.HEADER.TRANSACTIONID, port.File.Type.ToString()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010742", string.Format("Port[{0}] PortType[{1}] Invalid", _port.PORTID, port.File.Type.ToString()));
            //                return;
            //            }
            //            else if (port.File.Mode == ePortMode.Unknown)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortMode=[{2}] Invalid.",
            //                _port.PORTID, command.HEADER.TRANSACTIONID, port.File.Mode.ToString()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010743", string.Format("Port[{0}] PortMode[{1}] Invalid", _port.PORTID, port.File.Mode.ToString()));
            //                return;
            //            }
            //            else if (port.File.EnableMode != ePortEnableMode.Enabled)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortEnableMode=[{2}] Invalid.",
            //                _port.PORTID, command.HEADER.TRANSACTIONID, port.File.EnableMode.ToString()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010746", string.Format("Port[{0}] PortEnableMode[{1}] Invalid", _port.PORTID, port.File.EnableMode.ToString()));
            //                return;
            //            }
            //            else if (port.File.Status != ePortStatus.LC)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortStatus=[{2}] Invalid.",
            //                _port.PORTID, command.HEADER.TRANSACTIONID, port.File.Status.ToString()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010734", string.Format("Port[{0}] PortStatus[{1}] Invalid", _port.PORTID, port.File.Status));
            //                return;
            //            }
            //            else if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA && port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Cassette Status=[{2}] is not available.",
            //                    _port.PORTID, command.HEADER.TRANSACTIONID, port.File.CassetteStatus.ToString()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010737", string.Format("Cassette Status=[{0}] is not available", port.File.CassetteStatus.ToString()));
            //                return;
            //            }
            //            #endregion

            //            //Jun Modify 20150204
            //            eFabType fabType;
            //            Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

            //            #region Check Cassette Information
            //            //Jun Modify 20150204
            //            Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
            //            if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //                cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);

            //            if (cst == null)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette=[{0}] in CassetteEntity.",
            //                _port.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010730", string.Format("Can't find Cassette[{0}] in CassetteEntity.", _port.CASSETTEID.Trim()));
            //                return;
            //            }


            //            //if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
            //            //{
            //            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            //        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette SeqNo. Cassette SeqNo=[{2}] / Port Cassette SeqNo=[{3}].",
            //            //        _port.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteSequenceNo, port.File.CassetteSequenceNo));
            //            //        OfflineModeCassetteDataSendReply_DPI(command, "0010735", string.Format("Different Cassette SeqNo"));
            //            //        return;
            //            //}
            //            else if (!string.IsNullOrEmpty(cst.CassetteID.Trim()) && !cst.CassetteID.Trim().Equals(_port.CASSETTEID.Trim()))
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette ID. Cassette ID=[{2}] / OPI Cassette ID=[{0}].",
            //                    _port.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteID.Trim()));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010736", string.Format("Different Cassette ID"));
            //                return;
            //            }
            //            else if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Cassette have been Cancel.",
            //                    _port.CASSETTEID, command.HEADER.TRANSACTIONID));
            //                OfflineModeCassetteDataSendReply_DPI(command, "0010741", "Cassette have been Cancel");
            //                return;
            //            }
            //            #endregion

            //            string errMsg = string.Empty;

            //            //Jun Modify 20150204
            //            //eFabType fabType;
            //            //Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

            //            //bool bRemap = command.BODY.REMAPFLAG == "Y" ? true : false;

            //            if (!ValidateCassetteCheckData_DPI(fabType, command, command.HEADER.TRANSACTIONID, ref line, ref eqp, ref port, ref cst, out errMsg))
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] ValidateCassetteCheckData fail. {5}",
            //                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, _port.PORTNO, _port.CASSETTEID, errMsg));

            //                OfflineModeCassetteDataSendReply_DPI(command, "0010744", string.Format("ValidateCassetteCheckData fail, {0}", errMsg));
            //                return;
            //            }
            //            else
            //            {
            //                XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
            //                XmlNodeList portNodeList = body["PORTLIST"].ChildNodes;
            //                XmlNode portNode = portNodeList.Item(index);
            //                XmlNode lotNode = portNodeList.Item(index)["LOTDATA"];
            //                //XmlNodeList lotNodeList = portNode["LOTLIST"].ChildNodes;

            //                MESDataIntoCstObject_DPI(body, portNode, ref cst);

            //                lock (cst)
            //                {
            //                    cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;
            //                    cst.CassetteID = _port.CASSETTEID;

            //                    if (fabType == eFabType.CELL)
            //                        cst.LDCassetteSettingCode = _port.CSTSETTINGCODE;
            //                }
            //                ObjectManager.CassetteManager.EnqueueSave(cst);

            //                //儲存生成的Job
            //                IList<Job> jobs = new List<Job>();
            //                // Array Use : 計算Recipe Group Number
            //                Dictionary<string, string> recipeGroup = new Dictionary<string, string>();

            //                int i = 0;
            //                //foreach (OfflineModeCassetteDataSend_DPI.LOTDATAc lot in _port.LOTLIST)
            //                OfflineModeCassetteDataSend_DPI.LOTDATAc lot = _port.LOTDATA;
            //                {
            //                    //XmlNode lotNode =  lotNodeList[i];

            //                    LOTc _lot = new LOTc();
            //                    _lot.LOTNAME = lot.LOTNAME;
            //                    _lot.PROCESSOPERATIONNAME = lot.PROCESSOPERATIONNAME;
            //                    _lot.PRODUCTOWNER = lot.PRODUCTOWNER;
            //                    _lot.PRODUCTSPECNAME = lot.PRODUCTSPECNAME;
            //                    _lot.BCPRODUCTTYPE = lot.BCPRODUCTTYPE;
            //                    _lot.PRDCARRIERSETCODE = lot.CSTSETTINGCODE;

            //                    int j = 0;
            //                    XmlNodeList productList = lotNode["PRODUCTLIST"].ChildNodes;

            //                    foreach (OfflineModeCassetteDataSend_DPI.PRODUCTDATAc product in lot.PRODUCTLIST)
            //                    {
            //                        XmlNode productNode = productList[j];

            //                        int iSlotNo = int.Parse(product.SLOTNO);

            //                        if (iSlotNo <= 0)
            //                        {
            //                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                                string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] OPI cassette data SlotNo=[{3}] is wrong.",
            //                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, _port.PORTNO, _port.CASSETTEID, product.SLOTNO));

            //                            OfflineModeCassetteDataSendReply_DPI(command, "0010739", string.Format("OPI cassette data SlotNo[{0}] is wrong", product.SLOTNO));
            //                            return;
            //                        }

            //                        if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Existence，不需要進行比對
            //                        {
            //                            if (!port.File.ArrayJobExistenceSlot[iSlotNo - 1])
            //                            {
            //                                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette SlotMap Mismatch. SlotNo=[{5}] isn't exist.",
            //                                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, _port.PORTNO, _port.CASSETTEID, product.SLOTNO));

            //                                OfflineModeCassetteDataSendReply_DPI(command, "0010740", string.Format("Cassette SlotMap Mismatch"));
            //                                return;
            //                            }
            //                        }

            //                        #region Create Job
            //                        {
            //                            _lot.BCPRODUCTTYPE = product.PRODUCTTYPE;

            //                            PRODUCTc _product = new PRODUCTc();
            //                            _product.POSITION = product.SLOTNO;
            //                            _product.PROCESSFLAG = product.PROCESSFLAG;
            //                            _product.PRODUCTNAME = product.PRODUCTNAME;
            //                            _product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
            //                            _product.PPID = product.OPI_PPID;
            //                            _product.PRODUCTTYPE = product.PRODUCTTYPE;
            //                            _product.PRODUCTGRADE = product.PRODUCTGRADE;
            //                            _product.PRODUCTJUDGE = product.PRODUCTJUDGE;
            //                            _product.GROUPID = product.GROUPID;
            //                            //MES SUBPRODUCTGRADES = OPI OXR
            //                            _product.SUBPRODUCTGRADES = product.OXR;
            //                            //MSE ARRAYPRODUCTSPECVER = OPI COAVERSION
            //                            _product.OWNERTYPE = product.OWNERTYPE;
            //                            _product.OWNERID = product.OWNERID;
            //                            _product.REVPROCESSOPERATIONNAME = product.REVPROCESSOPERATIONNAME;

            //                            _lot.PRODUCTLIST.Add(_product);

            //                            //Create Job Data
            //                            Job job = null;

            //                            ////Remap的時候，需清掉部分資訊
            //                            //if (bRemap)
            //                            //{
            //                            //    job = ObjectManager.JobManager.GetJob(port.File.CassetteSequenceNo, iSlotNo.ToString());
            //                            //    if (job != null)
            //                            //    {
            //                            //        if (productList[j][keyHost.PROCESSFLAG].InnerText.Equals("Y"))
            //                            //        {
            //                            //            job.TargetPortID = "0";
            //                            //            job.ToSlotNo = "0";
            //                            //            job.TrackingData = SpecialItemInitial("TrackingData", 32);
            //                            //            job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
            //                            //            job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
            //                            //            job.EQPFlag = new string('0', 32);
            //                            //            job.HoldInforList.Clear();
            //                            //            job.JobProcessStartTime = DateTime.Now;
            //                            //            if (job.QtimeList != null) job.QtimeList.Clear();
            //                            //            job.DefectCodes.Clear();
            //                            //            job.JobProcessFlows.Clear();
            //                            //            job.MesCstBody.LOTLIST.Clear();
            //                            //        }
            //                            //    }
            //                            //}

            //                            if (job == null)
            //                            {
            //                                job = new Job(int.Parse(cst.CassetteSequenceNo), iSlotNo);
            //                                job.TrackingData = SpecialItemInitial("TrackingData", 32);
            //                                job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
            //                                job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);
            //                            }

            //                            job.CIMMode = eBitResult.ON;
            //                            job.MesCstBody.PRODUCTQUANTITY = _port.PRODUCTQUANTITY;
            //                            job.MesCstBody.LOTLIST.Add(_lot);
            //                            job.MesProduct = _product;
            //                            job.GroupIndex = string.IsNullOrEmpty(product.GROUPID) ? "1" : product.GROUPID;
            //                            job.SamplingSlotFlag = product.PROCESSFLAG == "Y" ? "1" : "0";
            //                            job.RobotProcessFlag = product.PROCESSFLAG == "Y" ? keyCELLROBOTProcessFlag.WAIT_PROCESS : keyCELLROBOTProcessFlag.NO_PROCESS; //Watson Add 20150316 For Auto Abort Cassette.
            //                            job.GlassChipMaskBlockID = product.PRODUCTNAME;
            //                            job.EQPJobID = product.PRODUCTNAME;
            //                            job.SourcePortID = port.Data.PORTID;
            //                            job.FromCstID = port.File.CassetteID;
            //                            job.FromSlotNo = product.SLOTNO;
            //                            //先預設給0
            //                            job.ToSlotNo = "0";
            //                            job.CurrentEQPNo = eqp.Data.NODENO;
            //                            job.LineRecipeName = product.PRODUCTRECIPENAME;
            //                            job.JobGrade = product.PRODUCTGRADE;
            //                            job.JobJudge = product.PRODUCTJUDGE;
            //                            int itype = int.Parse(product.PRODUCTTYPE);
            //                            job.JobType = (eJobType)itype;
            //                            int substrateType = int.Parse(product.SUBSTRATETYPE);
            //                            job.SubstrateType = (eSubstrateType)substrateType;
            //                            job.ProductType.Value = string.IsNullOrEmpty(lot.BCPRODUCTTYPE) ? 0 : int.Parse(lot.BCPRODUCTTYPE);
            //                            job.ProductID.Value = string.IsNullOrEmpty(lot.PRODUCTID) ? 0 : int.Parse(lot.PRODUCTID);
            //                            job.CellSpecial.ProductID = job.ProductID.Value.ToString();

            //                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
            //                            {
            //                                // 資料來源由 PlanManager
            //                                IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(line.File.CurrentPlanID, port.File.CassetteID.Trim());
            //                                SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == job.GlassChipMaskBlockID.Trim());
            //                                if (plan != null)
            //                                {
            //                                    job.TargetCSTID = plan.TARGET_CASSETTE_ID.Trim();
            //                                    // 跟登今確認, 在Changer Mode只要有Plan就是要抽, 即使MES 的原資料是不抽, 也要turn On
            //                                    if (!string.IsNullOrEmpty(job.TargetCSTID.Trim()))
            //                                        job.SamplingSlotFlag = "1";
            //                                    else
            //                                        job.SamplingSlotFlag = "0";
            //                                }
            //                            }
            //                            job.EQPFlag = product.EQPFLAG;
            //                            job.MesProduct = _product;
            //                            job.CSTOperationMode = eqp.File.CSTOperationMode;

            //                            string oxr = string.Empty;
            //                            job.ChipCount = product.OXR.Trim().Length;

            //                            for (i = 0; i < product.OXR.Length; i++)
            //                            {
            //                                switch (product.OXR.Substring(i, 1))
            //                                {
            //                                    case "1": oxr += "R"; break;
            //                                    case "2":
            //                                        //Cell OXR規則與其他Array/CF不同
            //                                        if (fabType == eFabType.CELL)
            //                                            oxr += "O";
            //                                        else
            //                                            oxr += "L";
            //                                        break;
            //                                    case "3": oxr += "O"; break;
            //                                    case "4": oxr += "F"; break;
            //                                    default: oxr += "X"; break;
            //                                }
            //                            }
            //                            job.OXRInformation = oxr;
            //                            job.OXRInformationRequestFlag = job.ChipCount > 56 ? "1" : "0";

            //                            #region PPID計算
            //                            string ppid = string.Empty;
            //                            string mesPPID = string.Empty;
            //                            string crossPPID = string.Empty;
            //                            string err = string.Empty;
            //                            IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
            //                            IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

            //                            if (ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
            //                            {
            //                                job.PPID = ppid;
            //                                job.MES_PPID = mesPPID;
            //                            }
            //                            if (err.Trim() != string.Empty)
            //                            {
            //                                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
            //                                        _port.CASSETTEID, command.HEADER.TRANSACTIONID, err));
            //                                OfflineModeCassetteDataSendReply_DPI(command, "0010742", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
            //                                return;
            //                            }
            //                            #endregion

            //                            M2P_SpecialDataBy_DPI(lotNode, productList[j], j, product.OXR, line, port, ref job);

            //                            job.CellSpecial.AbnormalCode = product.ABNORMALCODE;
            //                            job.CellSpecial.OwnerID = product.OWNERID;

            //                            //Watson Add 20150324
            //                            ObjectManager.QtimeManager.ValidateQTimeSetting(job);

            //                            jobs.Add(job);
            //                        }
            //                        #endregion
            //                        j++;
            //                    }
            //                    i++;
            //                }

            //                if (_port.PRODUCTQUANTITY != port.File.JobCountInCassette)
            //                {
            //                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette Job Count Mismatch. Cassette Job Count=[{5}] OPI PRODUCTQUANTITY=[{6}].",
            //                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, _port.PORTNO, _port.CASSETTEID, port.File.JobCountInCassette, _port.PRODUCTQUANTITY));

            //                    OfflineModeCassetteDataSendReply_DPI(command, "0010738", string.Format("Cassette Job Count Mismatch"));
            //                    return;
            //                }

            //                //將資料放進JobManager
            //                ObjectManager.JobManager.AddJobs(jobs);
            //                ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString());

            //                //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
            //                port.File.RobotWaitProcCount = jobs.Where(j => j.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

            //                //Download to PLC
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Invoke CassetteService DPCassetteMapDownlad.",
            //                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, _port.PORTNO, _port.CASSETTEID));

            //                Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownload", new object[] { eqp, port, jobs });

            //                #region Ftp File
            //                if (port.File.Type != ePortType.UnloadingPort)
            //                {
            //                    string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
            //                    for (int k = 0; k < jobs.Count(); k++)
            //                    {
            //                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
            //                    }
            //                }
            //                #endregion

            //                index++;
            //            }
            //            OfflineModeCassetteDataSendReply_DPI(command, "0000000", "");
            //        }

            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] UIService reply message=[{3}] to OPI.",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, reply.HEADER.MESSAGENAME));
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //    }
            //}

            ///// <summary>
            ///// OPI MessageSet: Offline Mode Cassette Data Send
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_OfflineModeCassetteDataSend_PRM(XmlDocument xmlDoc)
            //{
            //    IServerAgent agent = GetServerAgent();
            //    OfflineModeCassetteDataSend_PRM command = Spec.XMLtoMessage(xmlDoc) as OfflineModeCassetteDataSend_PRM;
            //    XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply_PRM") as XmlDocument;
            //    OfflineModeCassetteDataSendReply_PRM reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply_PRM;

            //    try
            //    {
            //        reply.BODY.LINENAME = command.BODY.LINENAME;
            //        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //        reply.BODY.PORTNO = command.BODY.PORTNO;
            //        reply.BODY.PORTID = command.BODY.PORTID;
            //        reply.BODY.CASSETTEID = command.BODY.CASSETTEID.Trim();

            //        reply.RETURN.RETURNCODE = "0000000";

            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message=[{2}] from OPI. EQUIPMENTNO={3}, PORTNO={4}, CASSETTEID={5}",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

            //        #region Check Line Information
            //        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
            //        if (line == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));

            //            OfflineModeCassetteDataSendReply_PRM(command, "0010745", string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, command.BODY.LINENAME));
            //            return;
            //        }
            //        #endregion

            //        #region Check Equipment Information
            //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
            //        if (eqp == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment=[{0}] in EquipmentEntity.",
            //            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010732", string.Format("Can't find Equipment=[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO));
            //            return;
            //        }

            //        if (eqp.File.CIMMode == eBitResult.OFF)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Equipment=[{0}] CIM Mode Off.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010733", string.Format("Equipment[{0}] CIM Mode Off", command.BODY.EQUIPMENTNO));
            //            return;
            //        }
            //        #endregion

            //        #region Check Port Information
            //        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
            //        if (port == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port=[{0}] in PortEntity.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010731", string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTID));
            //            return;
            //        }
            //        if (port.File.Type == ePortType.Unknown)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortType=[{2}] Invalid.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Type.ToString()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010742", string.Format("Port[{0}] PortType[{1}] Invalid", command.BODY.PORTID, port.File.Type.ToString()));
            //            return;
            //        }
            //        else if (port.File.Mode == ePortMode.Unknown)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortMode=[{2}] Invalid.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Mode.ToString()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010743", string.Format("Port[{0}] PortMode[{1}] Invalid", command.BODY.PORTID, port.File.Mode.ToString()));
            //            return;
            //        }
            //        else if (port.File.EnableMode != ePortEnableMode.Enabled)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortEnableMode=[{2}] Invalid.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.EnableMode.ToString()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010746", string.Format("Port[{0}] PortEnableMode[{1}] Invalid", command.BODY.PORTID, port.File.EnableMode.ToString()));
            //            return;
            //        }
            //        else if (port.File.Status != ePortStatus.LC)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port=[{0}] PortStatus=[{2}] Invalid.",
            //            command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.Status.ToString()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010734", string.Format("Port[{0}] PortStatus[{1}] Invalid", command.BODY.PORTID, port.File.Status));
            //            return;
            //        }
            //        else if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA && port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[PORT={0}] [BCS <- OPI][{1}] Cassette Status=[{2}] is not available.",
            //                command.BODY.PORTID, command.HEADER.TRANSACTIONID, port.File.CassetteStatus.ToString()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010737", string.Format("Cassette Status=[{0}] is not available", port.File.CassetteStatus.ToString()));
            //            return;
            //        }
            //        #endregion

            //        //Jun Modify 20150204
            //        eFabType fabType;
            //        Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

            //        #region Check Cassette Information
            //        //Jun Modify 20150204
            //        Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
            //        if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //            cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);

            //        if (cst == null)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette=[{0}] in CassetteEntity.",
            //            command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010730", string.Format("Can't find Cassette[{0}] in CassetteEntity.", command.BODY.CASSETTEID.Trim()));
            //            return;
            //        }
            //        if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
            //        {
            //            if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //            { }
            //            else
            //            {
            //                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette SeqNo. Cassette SeqNo=[{2}] / Port Cassette SeqNo=[{3}].",
            //                command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteSequenceNo, port.File.CassetteSequenceNo));
            //                OfflineModeCassetteDataSendReply_PRM(command, "0010735", string.Format("Different Cassette SeqNo"));
            //                return;
            //            }
            //        }
            //        else if (!string.IsNullOrEmpty(cst.CassetteID.Trim()) && !cst.CassetteID.Trim().Equals(command.BODY.CASSETTEID.Trim()))
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Different Cassette ID. Cassette ID=[{2}] / OPI Cassette ID=[{0}].",
            //                command.BODY.CASSETTEID.Trim(), command.HEADER.TRANSACTIONID, cst.CassetteID.Trim()));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010736", string.Format("Different Cassette ID"));
            //            return;
            //        }

            //        else if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Cassette have been Cancel.",
            //                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
            //            OfflineModeCassetteDataSendReply_PRM(command, "0010741", "Cassette have been Cancel");
            //            return;
            //        }
            //        #endregion

            //        string errMsg = string.Empty;

            //        //Jun Modify 20150204
            //        //eFabType fabType;
            //        //Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
            //        #region ValidateCassetteCheckData
            //        {
            //            if (ParameterManager[eREPORT_SWITCH.OFFLINE_SPECIAL_RULE].GetBoolean())
            //            {
            //                if (!ValidateCassetteCheckData_PRM(command, ref line, ref eqp, ref port, ref cst, out errMsg))
            //                {
            //                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] ValidateCassetteCheckData fail. {5}",
            //                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, errMsg));

            //                    OfflineModeCassetteDataSendReply_PRM(command, "0010744", string.Format("ValidateCassetteCheckData fail, {0}", errMsg));
            //                    return;
            //                }
            //            }
            //        }
            //        #endregion

            //        XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");

            //        #region MESDataIntoCstObject
            //        {
            //            lock (cst)
            //            {
            //                cst.PortID = command.BODY.PORTID;
            //                cst.PortNo = command.BODY.PORTNO;
            //                if (cst.OFFLINE_CstData == null) cst.OFFLINE_CstData = new OFFLINE_CstBody();
            //                cst.OFFLINE_CstData.LINENAME = command.BODY.LINENAME;
            //                cst.OFFLINE_CstData.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //                cst.OFFLINE_CstData.PORTNO = command.BODY.PORTNO;
            //                cst.OFFLINE_CstData.PORTID = command.BODY.PORTID;
            //                cst.OFFLINE_CstData.CASSETTEID = command.BODY.CASSETTEID;
            //                cst.OFFLINE_CstData.LINEOPERMODE = line.File.LineOperMode;
            //                cst.OFFLINE_CstData.PRODUCTQUANTITY = command.BODY.PRODUCTLIST.Count.ToString();
            //                cst.OFFLINE_CstData.CSTSETTINGCODE = port.File.CassetteSetCode;

            //                LOTDATAc offline_lot = new LOTDATAc();
            //                offline_lot.BCPRODUCTTYPE = "1";
            //                offline_lot.PRODUCTID = "1";
            //                offline_lot.LINERECIPENAME = command.BODY.PRODUCTLIST.Count > 0 ? command.BODY.PRODUCTLIST[0].PRODUCTRECIPENAME : string.Empty;
            //                offline_lot.PPID = command.BODY.PRODUCTLIST.Count > 0 ? command.BODY.PRODUCTLIST[0].OPI_PPID : string.Empty;
            //                offline_lot.CSTSETTINGCODE = port.File.CassetteSetCode;

            //                foreach (OfflineModeCassetteDataSend_PRM.PRODUCTDATAc opi_product in command.BODY.PRODUCTLIST)
            //                {
            //                    OFFLINEPRODUCTDATAc offline_product = new OFFLINEPRODUCTDATAc();
            //                    offline_product.SLOTNO = opi_product.SLOTNO;
            //                    offline_product.PROCESSFLAG = "Y";
            //                    offline_product.PRODUCTNAME = opi_product.PANELID;
            //                    offline_product.PRODUCTRECIPENAME = opi_product.PRODUCTRECIPENAME;
            //                    offline_product.OPI_PPID = opi_product.OPI_PPID;
            //                    offline_product.PRODUCTTYPE = "1";
            //                    offline_product.PRODUCTGRADE = "OK";
            //                    offline_product.PRODUCTJUDGE = "1";
            //                    offline_product.GROUPID = "1";
            //                    offline_product.OXR = "O";

            //                    IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
            //                    if (sub != null)
            //                    {
            //                        if (sub.ContainsKey("PanelSizeFlag"))
            //                            sub["PanelSizeFlag"] = opi_product.PANELSIZEFLAG;

            //                        offline_product.EQPFLAG = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
            //                    }

            //                    offline_product.SUBSTRATETYPE = eSubstrateType.Chip.ToString();
            //                    offline_product.PANELSIZE = opi_product.PANELSIZE;
            //                    offline_product.TRUNANGLE = "1";
            //                    offline_product.PANELSIZEFLAG = opi_product.PANELSIZEFLAG;

            //                    offline_lot.PRODUCTLIST.Add(offline_product);
            //                }
            //                cst.OFFLINE_CstData.LOTLIST.Add(offline_lot);
            //            }
            //        }
            //        #endregion

            //        lock (cst)
            //        {
            //            cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;
            //            cst.CassetteID = command.BODY.CASSETTEID;
            //            cst.LDCassetteSettingCode = port.File.CassetteSetCode;
            //        }
            //        ObjectManager.CassetteManager.EnqueueSave(cst);

            //        //儲存生成的Job
            //        IList<Job> jobs = new List<Job>();
            //        // Array Use : 計算Recipe Group Number
            //        Dictionary<string, string> recipeGroup = new Dictionary<string, string>();
            //        IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
            //        IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

            //        LOTc _lot = new LOTc();
            //        _lot.PRDCARRIERSETCODE = port.File.CassetteSetCode;
            //        _lot.BCPRODUCTTYPE = "1";
            //        _lot.BCPRODUCTID = "1";

            //        int j = 0;
            //        foreach (OfflineModeCassetteDataSend_PRM.PRODUCTDATAc product in command.BODY.PRODUCTLIST)
            //        {
            //            XmlNode productNode = xmlDoc["MESSAGE"]["BODY"]["PRODUCTLIST"].ChildNodes.Item(j);

            //            int iSlotNo = int.Parse(product.SLOTNO);

            //            if (iSlotNo <= 0)
            //            {
            //                Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                    string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] OPI cassette data SlotNo=[{3}] is wrong.",
            //                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, product.SLOTNO));

            //                OfflineModeCassetteDataSendReply_PRM(command, "0010739", string.Format("OPI cassette data SlotNo[{0}] is wrong", product.SLOTNO));
            //                return;
            //            }

            //            if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Existence，不需要進行比對
            //            {
            //                if (!port.File.ArrayJobExistenceSlot[iSlotNo - 1])
            //                {
            //                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette SlotMap Mismatch. SlotNo=[{5}] isn't exist.",
            //                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, product.SLOTNO));

            //                    OfflineModeCassetteDataSendReply_PRM(command, "0010740", string.Format("Cassette SlotMap Mismatch"));
            //                    return;
            //                }
            //            }

            //            #region Create Job
            //            {
            //                _lot.LINERECIPENAME = product.PRODUCTRECIPENAME;
            //                _lot.PPID = product.OPI_PPID;

            //                PRODUCTc _product = new PRODUCTc();
            //                _product.POSITION = product.SLOTNO;
            //                _product.PROCESSFLAG = "Y";
            //                _product.PRODUCTNAME = product.PANELID;
            //                _product.PRODUCTRECIPENAME = product.PRODUCTRECIPENAME;
            //                _product.PPID = product.OPI_PPID;
            //                _product.PRODUCTTYPE = "1";
            //                _product.PRODUCTGRADE = "OK";
            //                _product.PRODUCTJUDGE = "1";
            //                _product.GROUPID = "1";
            //                //MES SUBPRODUCTGRADES = OPI OXR
            //                _product.SUBPRODUCTGRADES = "O";
            //                //MSE ARRAYPRODUCTSPECVER = OPI COAVERSION
            //                _lot.PRODUCTLIST.Add(_product);

            //                //Create Job Data
            //                Job job = new Job(int.Parse(cst.CassetteSequenceNo), iSlotNo);
            //                job.TrackingData = SpecialItemInitial("TrackingData", 32);
            //                job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
            //                job.InspJudgedData = SpecialItemInitial("INSP.JudgedData", 32);

            //                job.CIMMode = eBitResult.ON;
            //                job.MesCstBody.LINEOPERMODE = line.File.LineOperMode;
            //                job.MesCstBody.PRODUCTQUANTITY = command.BODY.PRODUCTLIST.Count.ToString();
            //                job.MesCstBody.LOTLIST.Add(_lot);
            //                job.MesProduct = _product;
            //                job.GroupIndex = _product.GROUPID;
            //                job.SamplingSlotFlag = "1";
            //                job.RobotProcessFlag = keyCELLROBOTProcessFlag.WAIT_PROCESS;
            //                job.GlassChipMaskBlockID = product.PANELID;

            //                job.EQPJobID = product.PANELID;
            //                job.SourcePortID = port.Data.PORTID;
            //                job.FromCstID = port.File.CassetteID;
            //                int slotno = 0;
            //                int.TryParse(product.SLOTNO, out slotno);
            //                job.FromSlotNo = slotno.ToString();
            //                //先預設給0
            //                job.ToSlotNo = "0";
            //                job.CurrentEQPNo = eqp.Data.NODENO;
            //                job.LineRecipeName = product.PRODUCTRECIPENAME;
            //                job.JobGrade = _product.PRODUCTGRADE;
            //                job.JobJudge = _product.PRODUCTJUDGE;
            //                //job.JobJudge = "0"; //参照ValidateCassette 默认都给0  tom 20141202
            //                int itype = int.Parse(_product.PRODUCTTYPE);
            //                job.JobType = (eJobType)itype;
            //                job.SubstrateType = eSubstrateType.Chip;
            //                job.ProductType.Value = 1;
            //                job.ProductID.Value = 1;
            //                job.CellSpecial.ProductID = job.ProductID.Value.ToString();
            //                job.CellSpecial.TurnAngle = "1";
            //                job.CellSpecial.PanelSize = product.PANELSIZE;
            //                job.CellSpecial.PanelSizeFlag = product.PANELSIZEFLAG;
            //                job.CellSpecial.ControlMode = line.File.HostMode;
            //                job.CellSpecial.CassetteSettingCode = port.File.CassetteSetCode;
            //                job.CellSpecial.RunMode = line.File.CellLineOperMode;

            //                IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
            //                if (sub != null)
            //                {
            //                    if (sub.ContainsKey("PanelSizeFlag"))
            //                        sub["PanelSizeFlag"] = product.PANELSIZEFLAG;

            //                    job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
            //                }

            //                job.MesProduct = _product;
            //                job.CSTOperationMode = eqp.File.CSTOperationMode;

            //                string oxr = string.Empty;
            //                job.ChipCount = _product.SUBPRODUCTGRADES.Trim().Length;

            //                for (int k = 0; k < _product.SUBPRODUCTGRADES.Length; k++)
            //                {
            //                    switch (_product.SUBPRODUCTGRADES.Substring(k, 1))
            //                    {
            //                    case "1": oxr += "R"; break;
            //                    case "2":
            //                        //Cell OXR規則與其他Array/CF不同
            //                        if (fabType == eFabType.CELL)
            //                            oxr += "O";
            //                        else
            //                            oxr += "L";
            //                        break;
            //                    case "3": oxr += "O"; break;
            //                    case "4": oxr += "F"; break;
            //                    default: oxr += "X"; break;
            //                    }
            //                }
            //                job.OXRInformation = oxr;
            //                job.OXRInformationRequestFlag = job.ChipCount > 56 ? "1" : "0";

            //                #region PPID計算
            //                string ppid = string.Empty;
            //                string mesPPID = string.Empty;
            //                string crossPPID = string.Empty;
            //                string err = string.Empty;                       

            //                if (ObjectManager.JobManager.AnalysisMesPPID_CELLNormal(productNode, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesPPID, out err))
            //                {
            //                    job.PPID = ppid;
            //                    job.MES_PPID = mesPPID;
            //                }
            //                if (err.Trim() != string.Empty)
            //                {
            //                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                        string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] MES PPID Analysis Failed .REASON= [{2}] .",
            //                            command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID, err));
            //                    OfflineModeCassetteDataSendReply_PRM(command, "0010742", string.Format("MES PPID Analysis Failed, REASON =[{0}]", err));
            //                    return;
            //                }
            //                #endregion

            //                //Watson Add 20150324
            //                ObjectManager.QtimeManager.ValidateQTimeSetting(job);

            //                jobs.Add(job);
            //            }
            //            #endregion
            //            j++;
            //        }

            //        if (command.BODY.PRODUCTLIST.Count.ToString() != port.File.JobCountInCassette)
            //        {
            //            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Cassette Job Count Mismatch. Cassette Job Count=[{5}] OPI PRODUCTQUANTITY=[{6}].",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, port.File.JobCountInCassette, command.BODY.PRODUCTLIST.Count));

            //            OfflineModeCassetteDataSendReply_PRM(command, "0010738", string.Format("Cassette Job Count Mismatch"));
            //            return;
            //        }

            //        //將資料放進JobManager
            //        ObjectManager.JobManager.AddJobs(jobs);
            //        ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString());

            //        //Jun Add 20150114 Create Robot WIP In Offline
            //        //ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, jobs);

            //        //Watson Add 20150317 For CELL ROBOT AUTO ABORT CASSETTE.
            //        port.File.RobotWaitProcCount = jobs.Where(jb => jb.RobotProcessFlag == keyCELLROBOTProcessFlag.WAIT_PROCESS).ToList().Count;

            //        //Download to PLC
            //        if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Invoke CassetteService DPCassetteMapDownlad.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

            //            Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownload", new object[] { eqp, port, jobs });
            //        }
            //        else
            //        {
            //            Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //                string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] Invoke CassetteService CassetteMapDownload.",
            //                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

            //            Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, jobs, command.HEADER.TRANSACTIONID });
            //        }

            //        #region Ftp File
            //        if (port.File.Type != ePortType.UnloadingPort)
            //        {
            //            string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
            //            for (int k = 0; k < jobs.Count(); k++)
            //            {
            //                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
            //            }
            //        }
            //        #endregion

            //        OfflineModeCassetteDataSendReply_PRM(command, "0000000", "");

            //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("LINENAME=[{0}] [BCS <- OPI][{1}] EQUIPMENTNO=[{2}] PORTNO=[{3}] CASSETTEID=[{4}] UIService reply message=[{5}] to OPI.",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, reply.HEADER.MESSAGENAME));
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            //    }
            //}

            private void OfflineModeCassetteDataSendReply(OfflineModeCassetteDataSend command, string retCode, string retMsg)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply") as XmlDocument;
                        OfflineModeCassetteDataSendReply reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply;

                        reply.RETURN.RETURNCODE = retCode;
                        reply.RETURN.RETURNMESSAGE = retMsg;

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        xMessage msg = SendReplyToOPI(command, reply);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, MethodInfo.GetCurrentMethod().Name, ex.ToString()));
                  }
            }

            //private void OfflineModeCassetteDataSendReply_DPI(OfflineModeCassetteDataSend_DPI command, string retCode, string retMsg)
            //{
            //    try
            //    {
            //        IServerAgent agent = GetServerAgent();
            //        XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply_DPI") as XmlDocument;
            //        OfflineModeCassetteDataSendReply_DPI reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply_DPI;

            //        reply.RETURN.RETURNCODE = retCode;
            //        reply.RETURN.RETURNMESSAGE = retMsg;

            //        reply.BODY.LINENAME = command.BODY.LINENAME;
            //        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //        reply.BODY.PORTLIST.Clear();
            //        foreach (OfflineModeCassetteDataSend_DPI.PORTc port in command.BODY.PORTLIST)
            //        {
            //            reply.BODY.PORTLIST.Add(new OfflineModeCassetteDataSendReply_DPI.PORTc()
            //            {
            //                PORTNO = port.PORTNO,
            //                PORTID = port.PORTID,
            //                CASSETTEID = port.CASSETTEID.Trim()
            //            });
            //        }

            //        xMessage msg = SendReplyToOPI(command, reply);
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, MethodInfo.GetCurrentMethod().Name, ex.ToString()));
            //    }
            //}

            //private void OfflineModeCassetteDataSendReply_PRM(OfflineModeCassetteDataSend_PRM command, string retCode, string retMsg)
            //{
            //    try
            //    {
            //        IServerAgent agent = GetServerAgent();
            //        XmlDocument xml_doc = agent.GetTransactionFormat("OfflineModeCassetteDataSendReply_PRM") as XmlDocument;
            //        OfflineModeCassetteDataSendReply_PRM reply = Spec.XMLtoMessage(xml_doc) as OfflineModeCassetteDataSendReply_PRM;

            //        reply.RETURN.RETURNCODE = retCode;
            //        reply.RETURN.RETURNMESSAGE = retMsg;

            //        reply.BODY.LINENAME = command.BODY.LINENAME;
            //        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
            //        reply.BODY.PORTNO = command.BODY.PORTNO;
            //        reply.BODY.PORTID = command.BODY.PORTID;
            //        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

            //        xMessage msg = SendReplyToOPI(command, reply);
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message=[{2}] exception : {3} ",
            //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, MethodInfo.GetCurrentMethod().Name, ex.ToString()));
            //    }
            //}

            #region ================== OfflineModeCassetteDataSend Private Method ===================

            //依FAB確認資料
            private bool ValidateCassetteCheckData(eFabType fabType, OfflineModeCassetteDataSend data, string trxID, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            {
                  errMsg = string.Empty;
                  bool result = false;

                  if (ParameterManager[eREPORT_SWITCH.OFFLINE_PORT_MODE].GetBoolean() && line.Data.FABTYPE != eFabType.CELL.ToString())
                  {
                        #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
                        if (line.Data.LINETYPE != eLineType.ARRAY.CVD_AKT && line.Data.LINETYPE != eLineType.ARRAY.CVD_ULVAC && line.Data.LINETYPE != eLineType.ARRAY.DRY_YAC &&
                            line.Data.LINETYPE != eLineType.ARRAY.DRY_ICD && line.Data.LINETYPE != eLineType.ARRAY.DRY_TEL && port.File.Type == ePortType.LoadingPort)
                        {
                              //// 檢查 Port mode(EQP) 和 OPI 下來的 JobType(Job Data) 是否一致，若不一致要退 CST。
                              string jobType = string.Empty;
                              foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)// XmlNode n in lotNodeList)
                              {
                                    for (int i = 0; i < lot.PRODUCTLIST.Count; i++)
                                    {
                                          OfflineModeCassetteDataSend.PRODUCTDATAc product = lot.PRODUCTLIST[i];
                                          eJobType type = (eJobType)int.Parse(product.PRODUCTTYPE);
                                          bool checkng = false;

                                          switch (type)
                                          {
                                                case eJobType.TFT:
                                                      switch (port.File.Mode)
                                                      {
                                                            case ePortMode.CF:
                                                            case ePortMode.Dummy:
                                                            case ePortMode.MQC:
                                                            case ePortMode.ThicknessDummy:
                                                            case ePortMode.ThroughDummy:
                                                            case ePortMode.UVMask:
                                                                  checkng = true;
                                                                  break;
                                                      }
                                                      break;
                                                case eJobType.CF:
                                                      switch (port.File.Mode)
                                                      {
                                                            case ePortMode.TFT:
                                                            case ePortMode.HT:
                                                            case ePortMode.LT:
                                                            case ePortMode.Dummy:
                                                            case ePortMode.MQC:
                                                            case ePortMode.ThicknessDummy:
                                                            case ePortMode.ThroughDummy:
                                                            case ePortMode.UVMask:
                                                                  checkng = true;
                                                                  break;
                                                      }
                                                      break;
                                                case eJobType.DM:
                                                      if (port.File.Mode != ePortMode.Dummy &&
                                                          port.File.Mode != ePortMode.MQC)
                                                            checkng = true; break;
                                                case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                                                case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                                                case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
                                                default: checkng = true; break;
                                          }
                                          if (checkng)
                                          {
                                                errMsg = string.Format("Cassette MAP Transfer Error: OPI Job Type(PRODUCTTYPE)[{0}] mismatch with EQP Port Mode[{1}]. Equipment=[{2}], Port=[{3}]",
                                                    type, port.File.Mode, eqp.Data.NODENO, port.Data.PORTNO);

                                                return false;
                                          }
                                    }
                              }
                        }
                        #endregion
                  }
                  // No Check Speical Data
                  if (ParameterManager[eREPORT_SWITCH.OFFLINE_SPECIAL_RULE].GetBoolean() == false) return true;

                  switch (fabType)
                  {
                        case eFabType.ARRAY: result = ValidateCassetteCheckData_Array(data, ref line, ref eqp, ref port, ref cst, out errMsg); break;
                        case eFabType.CF: result = ValidateCassetteCheckData_CF(data, trxID, ref line, ref eqp, ref port, ref cst, out errMsg); break;
                        case eFabType.CELL: result = ValidateCassetteCheckData_CELL(data, ref line, ref eqp, ref port, ref cst, out errMsg); break;
                  }

                  return result;
            }

            ////依FAB確認資料 for DPI
            //private bool ValidateCassetteCheckData_DPI(eFabType fabType, OfflineModeCassetteDataSend_DPI data, string trxID, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            //{
            //    errMsg = string.Empty;
            //    // No Check Speical Data
            //    if (ParameterManager[eREPORT_SWITCH.OFFLINE_SPECIAL_RULE].GetBoolean() == false) return true;
            //    bool result = ValidateCassetteCheckData_DPI(data, ref line, ref eqp, ref port, ref cst, out errMsg);

            //    return result;
            //}

            //SCRAPCUTFLAG確認
            private bool CheckTBBFG_SCRAPCUTFLAG(XmlDocument xmlDoc)
            {
                  try
                  {
                        XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                        XmlNodeList lotNodeList = body["LOTLIST"].ChildNodes;

                        foreach (XmlNode lot in lotNodeList)
                        {
                              XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                              foreach (XmlNode product in productList)
                              {
                                    string scrap = product[keyHost.SCRAPCUTFLAG].InnerText.Trim();
                                    if (scrap.Equals("S") || scrap.Equals("C"))
                                    {
                                          return true;
                                    }
                              }
                        }
                        return false;
                  }
                  catch
                  {
                        return false;
                  }
            }

            //Array資料確認
            private bool ValidateCassetteCheckData_Array(OfflineModeCassetteDataSend data, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            {
                  errMsg = string.Empty;

                  try
                  {
                        string lineName = data.BODY.LINENAME;
                        int productQuantity;
                        int.TryParse(data.BODY.PRODUCTQUANTITY, out productQuantity);

                        if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT))
                        {
                              errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
                              return false;
                        }
                        else if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && line.Data.LINETYPE != eLineType.ARRAY.CAC_MYTEK && productQuantity.Equals(0))
                        {
                              errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                              return false;
                        }
                        else if (port.File.Type == ePortType.UnloadingPort && port.File.JobCountInCassette == port.Data.MAXCOUNT.ToString())
                        {
                              errMsg = "Cassette Data Transfer Error: Unloader Port loads CST , but CST have full glass.";
                              return false;
                        }
                        else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                        {
                              if (port.File.Type == ePortType.UnloadingPort)
                              {
                                    if (port.File.JobCountInCassette != "0")
                                    {
                                          errMsg = "Cassette Data Transfer Error: Force Clean Out Mode, Unloading Port can't loading partial full Cassette!";
                                          return false;
                                    }
                              }
                              else
                              {
                                    errMsg = "Cassette Data Transfer Error: Force Clean Out Mode, Loading/Both Port can't loading cassette.";
                                    return false;
                              }
                        }

                        if (port.File.Type == ePortType.UnloadingPort && port.File.JobCountInCassette.Equals("0")) return true;

                        switch (line.Data.LINETYPE)
                        {
                              case eLineType.ARRAY.CVD_AKT:
                              case eLineType.ARRAY.CVD_ULVAC:
                                    {
                                          break; //t3 CVD MES not use chamberrunmode and pass mix mode's glass flow check, 2015/10/14 cc.kuang
                                          bool ngResult = false;

                                          //mark by rebecca 2015-10-16 --刪除LINEOPERMODE
                                          //string LINEOPERATIONMODE = data.BODY.LINEOPERMODE;

                                          #region 檢查MES的LineOperationMode是否有在CVD的5個Chamber中做設定
                                          List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                                          Equipment cvd = eqps.FirstOrDefault(e => e.Data.USERUNMODE != null && e.Data.USERUNMODE.Equals("Y"));

                                          if (cvd == null)
                                          {
                                                errMsg = "Cassette Data Transfer Error: Check CVD Run Mode, can't find CVD Equipment Object.";
                                                return false;
                                          }

                                          IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(cvd.Data.NODENO).Where(u => u.Data.UNITTYPE != null
                                                  && u.Data.UNITTYPE.Equals("CHAMBER")).ToList<Unit>();

                                          //因為要比對Product層的資料, 所以要一層一層取得資料
                                          if (data.BODY.LOTLIST.Count == 0)
                                          {
                                                errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, Lot Data no Data.");
                                                return false;
                                          }

                                          if (data.BODY.LOTLIST[0].PRODUCTLIST.Count == 0)
                                          {
                                                errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, Product Data no Data.");
                                                return false;
                                          }

                                          OfflineModeCassetteDataSend.LOTDATAc lot = data.BODY.LOTLIST[0];
                                          string log = string.Empty;

                                          #region Check Port Mode
                                          switch (port.File.Mode)
                                          {
                                                case ePortMode.TFT:
                                                case ePortMode.CF:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (((eJobType)int.Parse(product.PRODUCTTYPE)).ToString() != port.File.Mode.ToString().ToUpper())
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] JOB_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      ((eJobType)int.Parse(product.PRODUCTTYPE)).ToString());
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Port Mode, PORT_MODE=[{0}], JOB_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                case ePortMode.HT:
                                                case ePortMode.LT:
                                                //foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                //{
                                                //    if (product.TEMPERATUREFLAG != port.File.Mode.ToString().ToUpper())
                                                //    {
                                                //        log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] TEMPERATUREFLAG=[{2}]", product.SLOTNO,
                                                //            product.PRODUCTNAME, product.TEMPERATUREFLAG);
                                                //        ngResult = true;
                                                //    }
                                                //}

                                                //if (ngResult)
                                                //{
                                                //    errMsg = string.Format("Cassette Data Transfer Error: Check CVD Port Mode, PORT_MODE=[{0}], TEMPERATUREFLAG is Invalid.{1}",
                                                //        port.File.Mode.ToString(), log);
                                                //    ngResult = true;
                                                //}
                                                //break;
                                                case ePortMode.IGZO:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (ConstantManager["OPI_CVD_PROCESSTYPE"][product.PROCESSTYPE].Value != port.File.Mode.ToString().ToUpper())
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] PROCESS_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      ConstantManager["OPI_CVD_PROCESSTYPE"][product.PROCESSTYPE].Value);
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Port Mode, PORT_MODE=[{0}], PROCESS_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                case ePortMode.MQC:
                                                case ePortMode.Dummy:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (product.OWNERTYPE != eMES_OWNER_TYPE.DUMMY)
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] OWNER_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      product.OWNERTYPE);
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Port Mode, PORT_MODE=[{0}], OWNER_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                default:
                                                      errMsg = string.Format("Cassette Data Transfer Error: Check Port Mode=[{0}] is Invalid.",
                                                          port.File.Mode.ToString());
                                                      ngResult = true;
                                                      break;
                                          }
                                          if (ngResult) return false;
                                          #endregion

                                          #region [Check flow type 是否一致]
                                          string flowtype = string.Empty;
                                          string clnRcp = "0";
                                          string cvdRcp = "0";
                                          for (int i = 0; i < data.BODY.LOTLIST.Count; i++)
                                          {
                                                for (int j = 0; j < data.BODY.LOTLIST[i].PRODUCTLIST.Count; j++)
                                                {
                                                      string ppid = ObjectManager.JobManager.ParsePPID(data.BODY.LOTLIST[i].PRODUCTLIST[j].OPI_PPID).Item1.PadRight(18, '0');
                                                      clnRcp = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                                      cvdRcp = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                                      if (string.IsNullOrEmpty(flowtype))
                                                      {
                                                            flowtype = clnRcp + cvdRcp;
                                                      }
                                                      else
                                                      {
                                                            if (!flowtype.Equals(clnRcp + cvdRcp))
                                                            {
                                                                  errMsg = string.Format("Cassette Data Transfer Error: glass flow type(PPID) mismatch with product data. Equipment=[{0}], Port=[{1}]",
                                                                      eqp.Data.NODENO, port.Data.PORTNO);
                                                                  /* modify 2016/08/09 cc.kuang
                                                                  return false;
                                                                  */
                                                            }
                                                      }
                                                }
                                          }
                                          // 只進清洗機的話就不用再往下檢查
                                          if (clnRcp.Equals("1") && cvdRcp.Equals("0")) return true;
                                          #endregion

                                          //mark by rebecca 2015-10-16 --刪除LINEOPERMODE
                                          //// 檢查MES的LineOperationMode是否有在CVD的5個Chamber中做設定
                                          //if (port.File.Type != ePortType.UnloadingPort)
                                          //{
                                          //    if (!units.Any(u => u.File.RunMode.Trim().Equals(LINEOPERATIONMODE)))
                                          //    {
                                          //        errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, can't find RUNMODE(LINEOPERATIONMODE)=[{0}] in CVD Equipment.", LINEOPERATIONMODE);
                                          //        return false;
                                          //    }
                                          //}

                                          if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                                          {
                                                if (port.File.Type != ePortType.BothPort)
                                                {
                                                      errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}] is not \"Both Port\" in MIX RUN MODE.",
                                                          lineName, port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                                      return false;
                                                }

                                                #region  Check Port Mode
                                                eCVD_MIX_TYPE type = AnalysisCVD_PortMode(port);

                                                /* t3 has not port mode 2015/10/05 cc.kuang
                                                switch (type)
                                                {
                                                    case eCVD_MIX_TYPE.TYPE1_LT_HT: break;
                                                    case eCVD_MIX_TYPE.TYPE2_IGZO_TFT:
                                                        if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.IGZO)
                                                        {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, (IGZO:TFT) Port Mode=[{0}] is Invalid.",
                                                                    port.File.Mode.ToString());
                                                            ngResult = true;
                                                        }
                                                        break;
                                                    case eCVD_MIX_TYPE.TYPE3_IGZO_MQC:
                                                        if (port.File.Mode != ePortMode.MQC && port.File.Mode != ePortMode.IGZO)
                                                        {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, (IGZO:MQC) Port Mode=[{0}] is Invalid.",
                                                                    port.File.Mode.ToString());
                                                            ngResult = true;
                                                        }
                                                        break;
                                                    case eCVD_MIX_TYPE.TYPE4_TFT_MQC:
                                                        if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.MQC)
                                                        {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, (TFT:MQC) Port Mode=[{0}] is Invalid.",
                                                                    port.File.Mode.ToString());
                                                            ngResult = true;
                                                        }
                                                        break;
                                                    default:
                                                        errMsg = string.Format("Cassette Data Transfer Error: Check CVD Run Mode, Port Mode Invalid.");
                                                        ngResult = true;
                                                        break;
                                                }
                                                */
                                                if (ngResult) return false;

                                                #endregion

                                                if (CheckMixRunOtherPortFlowType(cst.PortNo, flowtype, out errMsg) == false)
                                                {
                                                      return false;
                                                }
                                          }
                                          #endregion
                                    }
                                    break;

                              case eLineType.ARRAY.DRY_ICD:
                              case eLineType.ARRAY.DRY_YAC:
                              case eLineType.ARRAY.DRY_TEL:
                                    {
                                          bool ngResult = false;

                                          //mark by rebecca 2015-10-16 --刪除LINEOPERMODE
                                          //string LINEOPERATIONMODE = data.BODY.LINEOPERMODE.Trim().ToUpper();
                                          //if (LINEOPERATIONMODE == "NORN") { LINEOPERATIONMODE = "NORMAL"; } // add by bruce 2014/12/8 o mode report to mes

                                          #region 檢查MES的LineOperationMode是否有在DRY的3個Chamber中做設定
                                          List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                                          Equipment dry = eqps.FirstOrDefault(e => e.Data.USERUNMODE != null && e.Data.USERUNMODE.Equals("Y"));

                                          if (dry == null)
                                          {
                                                errMsg = "Cassette Data Transfer Error: Check DRY Run Mode, can't find DRY Equipment Object.";
                                                return false;
                                          }

                                          IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(dry.Data.NODENO);

                                          //因為要比對Product層的資料, 所以要一層一層取得資料
                                          if (data.BODY.LOTLIST.Count == 0)
                                          {
                                                errMsg = string.Format("Cassette Data Transfer Error: Check DRY Run Mode, MES Lot Data no Data.");
                                                return false;
                                          }

                                          if (data.BODY.LOTLIST[0].PRODUCTLIST.Count == 0)
                                          {
                                                errMsg = string.Format("Cassette Data Transfer Error: Check DRY Run Mode, MES Product Data no Data.");
                                                return false;
                                          }

                                          OfflineModeCassetteDataSend.LOTDATAc lot = data.BODY.LOTLIST[0];
                                          string log = string.Empty;

                                          #region Check Port Mode
                                          switch (port.File.Mode)
                                          {
                                                case ePortMode.TFT:
                                                case ePortMode.CF:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (((eJobType)int.Parse(product.PRODUCTTYPE)).ToString() != port.File.Mode.ToString().ToUpper())
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] JOB_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      ((eJobType)int.Parse(product.PRODUCTTYPE)).ToString());
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check DRY Port Mode, PORT_MODE=[{0}], JOB_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                case ePortMode.ENG:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (product.OWNERTYPE.ToUpper().Trim() != eMES_OWNER_TYPE.ENGINEER)
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] OWNER_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      product.OWNERTYPE.ToUpper());
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check DRY Port Mode, PORT_MODE=[{0}], OWNER_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                case ePortMode.IGZO:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (ConstantManager["OPI_DRY_PROCESSTYPE"][product.PROCESSTYPE].Value != port.File.Mode.ToString().ToUpper())
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] PROCESS_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      ConstantManager["OPI_DRY_PROCESSTYPE"][product.PROCESSTYPE].Value);
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check DRY Port Mode, PORT_MODE=[{0}], PROCESS_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                case ePortMode.MQC:
                                                case ePortMode.Dummy:
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (product.OWNERTYPE != eMES_OWNER_TYPE.DUMMY)
                                                            {
                                                                  log += string.Format("\r\n\t\t SLOT#{0} GLASS_ID=[{1}] OWNER_TYPE=[{2}]", product.SLOTNO, product.PRODUCTNAME,
                                                                      product.OWNERTYPE);
                                                                  ngResult = true;
                                                            }
                                                      }

                                                      if (ngResult)
                                                      {
                                                            errMsg = string.Format("Cassette Data Transfer Error: Check CVD Port Mode, PORT_MODE=[{0}], OWNER_TYPE is Invalid.{1}",
                                                                port.File.Mode.ToString(), log);
                                                            ngResult = true;
                                                      }
                                                      break;
                                                default:
                                                      errMsg = string.Format("Cassette Data Transfer Error: Check Port Mode=[{0}] is Invalid.",
                                                          port.File.Mode.ToString());
                                                      ngResult = true;
                                                      break;
                                          }
                                          if (ngResult) return false;
                                          #endregion

                                          #region [Check flow type 是否一致]
                                          string flowtype = string.Empty;
                                          string clnRcp = "0";
                                          string dryRcp = "0";
                                          for (int i = 0; i < data.BODY.LOTLIST.Count; i++)
                                          {
                                                for (int j = 0; j < data.BODY.LOTLIST[i].PRODUCTLIST.Count; j++)
                                                {
                                                      string ppid = ObjectManager.JobManager.ParsePPID(data.BODY.LOTLIST[i].PRODUCTLIST[j].OPI_PPID).Item1.PadRight(18, '0');
                                                      clnRcp = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                                      dryRcp = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                                      if (string.IsNullOrEmpty(flowtype))
                                                      {
                                                            flowtype = clnRcp + dryRcp;
                                                      }
                                                      else
                                                      {
                                                            if (!flowtype.Equals(clnRcp + dryRcp))
                                                            {
                                                                  errMsg = string.Format("Cassette Data Transfer Error: glass flow type(PPID) mismatch with product data. Equipment=[{0}], Port=[{1}]",
                                                                      eqp.Data.NODENO, port.Data.PORTNO);
                                                                /* modify 2016/08/09 cc.kuang  
                                                                return false;
                                                                 */
                                                            }
                                                      }
                                                }
                                          }
                                          // 只進清洗機的話就不用再往下檢查
                                          if (clnRcp.Equals("1") && dryRcp.Equals("0")) return true;
                                          #endregion

                                          if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                                          {
                                                if (port.File.Type != ePortType.BothPort)
                                                {
                                                      errMsg = string.Format("Cassette Data Transfer Error: Line Name=[{0}], Port Name =[{1}], CSTID=[{2}], Port Type=[{3}] is not \"Both Port\" in MIX RUN MODE.",
                                                          lineName, port.Data.PORTID, port.File.CassetteID, port.File.Type.ToString());
                                                      return false;
                                                }

                                                #region  Check
                                                int portModeCount = ObjectManager.PortManager.GetPorts().Select(p => p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().Count();

                                                if (portModeCount == 0 || portModeCount == 3)
                                                {
                                                      if (portModeCount == 0)
                                                            errMsg = "Cassette Data Transfer Error: Check DRY Run Mode, All the Port are \"Unknown\".";
                                                      else if (portModeCount == 3)
                                                            errMsg = "Cassette Data Transfer Error: Check DRY Run Mode, There are three different Port Mode.";

                                                      return false;
                                                }
                                                #endregion
                                                if (CheckMixRunOtherPortFlowType(cst.PortNo, flowtype, out errMsg) == false)
                                                {
                                                      return false;
                                                }
                                          }
                                          #endregion
                                    }
                                    break;
                              case eLineType.ARRAY.BFG_SHUZTUNG:

                                    break;
                        }

                        return true;
                  }
                  catch (Exception ex)
                  {
                        errMsg = ex.ToString();
                        return false;
                  }
            }

            //CF資料確認
            private bool ValidateCassetteCheckData_CF(OfflineModeCassetteDataSend data, string trxID, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            {
                  errMsg = string.Empty;

                  try
                  {
                        IList<Equipment> eqpList = new List<Equipment>();
                        int processCount = 0;

                        string cstid = data.BODY.CASSETTEID;
                        string portName = data.BODY.PORTID;
                        bool Result = false; //20150801
                        int productQuantity;
                        int.TryParse(data.BODY.PRODUCTQUANTITY, out productQuantity);
                        string lineName = data.BODY.LINENAME;

                        #region 取得 ProductQuantity 和 PlannedQuantity
                        lock (port) port.File.ProductQuantity = productQuantity.ToString();
                        ObjectManager.PortManager.EnqueueSave(port.File);
                        #endregion

                        #region Check Port (Common condition)
                        //防止Unloader上實卡匣 
                        if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
                        {
                              errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
                              return false;
                        }
                        //防止Unloader在非PartialFull時，上有玻璃的卡匣
                        if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0)
                        {
                              errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch portType=[{0}] PartialFullFlag=[{1}] ProductQuantity=[{2}]",
                                                      port.File.Type, port.File.PartialFullFlag, productQuantity);
                              return false;
                        }
                        //防止Loader/Both Port上空卡匣
                        if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0) && line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1)
                        {
                              errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                              return false;
                        }
                        //防止上實卡匣時，卡匣內的 Job ProcessFlag 都為 N
                        if (port.File.Type != ePortType.UnloadingPort && productQuantity > 0)
                        {
                              foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                              {
                                    if (lot.PRODUCTLIST.Where(p => p.PROCESSFLAG.Equals("Y")).Count() == 0)
                                    {
                                          errMsg = string.Format("Cassette Data Transfer Error: Can't found Job Process Flag = 'Y' in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                              eqp.Data.NODENO, port.Data.PORTNO);
                                          // Send CIM Message to EQP
                                          Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't found Job Process Flag = 'Y' in Cassette MAP Data.", "BCS" });
                                          return false;
                                    }
                              }
                        }
                        #endregion

                        #region Check Prot Mode According to Indexer Run Mode
                        //20160519 Add by Fank 
                        switch (line.File.IndexOperMode)
                        {
                            case eINDEXER_OPERATION_MODE.NORMAL_MODE:
                            case eINDEXER_OPERATION_MODE.MQC_MODE:
                                if (port.File.Type != ePortType.BothPort && line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1)  //ADD BY qiumin 20180119 check CF normal mode BYPASS upk
                                {
                                    errMsg = "Cassette Data Transfer Error: Normal/MQC Mode Need Both Port";
                                    return false;
                                }
                                break;
                            case eINDEXER_OPERATION_MODE.CHANGER_MODE:
                            case eINDEXER_OPERATION_MODE.SORTER_MODE:
                                if (port.File.Type == ePortType.BothPort)
                                {
                                    errMsg = "Cassette Data Transfer Error: Sorter/Changer Mode should use Loading/Unloading Port. Source CST --> Loading Port. Target CST --> Unloading Port";
                                    return false;
                                }
                                break;
                        }

                        #endregion

                        #region Check Port (Special condition by Line Type)
                        switch (line.Data.LINETYPE)
                        {
                              case eLineType.CF.FCREW_TYPE1:

                                    if (port.File.Type != ePortType.UnloadingPort)
                                    {
                                          #region Check CFREWORKCOUNT
                                          foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                          {
                                                string cfreworkcount = lot.CFREWORKCOUNT;
                                                int CFReworkCount = 0;
                                                int.TryParse(cfreworkcount, out CFReworkCount);
                                                if (CFReworkCount == 0)
                                                {
                                                      errMsg = string.Format("Cassette Data Transfer Error: Can't found CFREWORKCOUNT value in Cassette MAP Data. Equipment=[{0}], Port=[{1}]",
                                                          eqp.Data.NODENO, port.Data.PORTNO);
                                                      // Send CIM Message to EQP
                                                      Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't found CFREWORKCOUNT value in Cassette MAP Data.", "BCS" });
                                                      Result = false;
                                                      continue;
                                                }

                                                processCount += lot.PRODUCTLIST.Where(l => l.PROCESSFLAG.Equals("Y")).Count();
                                          }
                                          #endregion

                                          #region Check Process Count

                                          #region 取得目前機台最少可以Rework的片數
                                          int ReworkWashableCount = 0;
                                          IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                                          foreach (Equipment _eqp in eqps)
                                          {
                                                if (ReworkWashableCount == 0)
                                                {
                                                      ReworkWashableCount = _eqp.File.ReworkWashableCount;
                                                }
                                                else
                                                {
                                                      if (ReworkWashableCount > _eqp.File.ReworkWashableCount)
                                                      {
                                                            ReworkWashableCount = _eqp.File.ReworkWashableCount;
                                                      }
                                                }
                                          }
                                          #endregion

                                          #region 取得目前線上等待Rework的片數
                                          IList<Job> jobs = ObjectManager.JobManager.GetJobs().
                                              Where(j => (j.CfSpecial.TrackingData.Etching == "0" || j.CfSpecial.TrackingData.Stripper == "0") && j.MesProduct.PROCESSFLAG == "Y" && j.CurrentEQPNo == "L2").
                                              Distinct().ToList<Job>();
                                          #endregion

                                          #region 實際線上可 Rrwork 的片數
                                          int RealyReworkCount = ReworkWashableCount - jobs.Count;
                                          #endregion

                                          #region 取得 ValidateCassetteReply 內 PROCESSFLAG = 'Y' 的玻璃有幾片。
                                          int CassetteReworkCount = 0;
                                          foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                          {
                                                foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                {
                                                      string processflag = product.PROCESSFLAG;
                                                      if (!string.IsNullOrEmpty(processflag))
                                                      {
                                                            if (processflag.Equals("Y"))
                                                            {
                                                                  CassetteReworkCount += 1;
                                                            }
                                                      }
                                                }
                                          }
                                          #endregion

                                          #region 計算出可以Start的片數
                                          if (RealyReworkCount <= 0)
                                          {
                                                errMsg = "Cassette Data Transfer Error: Rework equipment can't do more Job.";
                                                return false;
                                          }
                                          else if (RealyReworkCount <= CassetteReworkCount)
                                          {
                                                lock (port)
                                                      port.File.StartByCount = RealyReworkCount.ToString();
                                                ObjectManager.PortManager.EnqueueSave(port.File);
                                          }
                                          else if (RealyReworkCount >= CassetteReworkCount)
                                          {
                                                lock (port)
                                                      port.File.StartByCount = CassetteReworkCount.ToString();
                                                ObjectManager.PortManager.EnqueueSave(port.File);
                                          }
                                          #endregion

                                          #endregion
                                    }
                                    break;

                              case eLineType.CF.FCREP_TYPE1:
                              case eLineType.CF.FCREP_TYPE2:
                              case eLineType.CF.FCREP_TYPE3:// 20160509 Add by Frank

                                    // Add by Kasim 20150509 Unloader Port 不做任何檢查
                                    if (port.File.Type == ePortType.UnloadingPort)
                                          return true;

                                    // 當所有的Repair機台的 Equipment Run Mode 都為Normal，此時來一個CST全都為 DR的玻璃時，要直接退。
                                    #region Check Equipment Run Mode
                                    int productCount = 0;
                                    int allProductCount = 0;
                                    eqpList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Where(e => e.Data.NODENO != "L2" && e.File.EquipmentRunMode != "0").ToList();
                                    if (eqpList.Select(e => e.File.EquipmentRunMode).Distinct().Count() == 1)
                                    {
                                          if (eqpList[0].File.EquipmentRunMode.ToUpper() == "NORMAL")
                                          {   //Add by Kasim 20150504
                                                productCount = 0;
                                                allProductCount = 0;
                                                foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                                {
                                                      allProductCount += lot.PRODUCTLIST.Count();
                                                      productCount += lot.PRODUCTLIST.Where(p => p.PRODUCTGRADE.Equals("DR")).Count();
                                                }

                                                if (productCount == allProductCount)
                                                {
                                                      errMsg = string.Format("Cassette Data Transfer Error: Can't process product judge(DR)'s Job. Equipment=[{0}], Port=[{1}]",
                                                                                              eqp.Data.NODENO, port.Data.PORTNO);
                                                      // Send CIM Message to EQP
                                                      Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, "Can't process product judge(DR)'s Job", "BCS" });
                                                      return false;
                                                }

                                          }
                                    }
                                    #endregion
                                    break;
                        }
                        #endregion

                        return true;
                  }
                  catch (Exception ex)
                  {
                        errMsg = ex.ToString();
                        return false;
                  }
            }

            //CELL資料確認
            private bool ValidateCassetteCheckData_CELL(OfflineModeCassetteDataSend data, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            {
                  errMsg = string.Empty;

                  try
                  {
                        string lineName = data.BODY.LINENAME;
                        string cstid = data.BODY.CASSETTEID;
                        string portName = data.BODY.PORTID;
                        int productQuantity;
                        int.TryParse(data.BODY.PRODUCTQUANTITY, out productQuantity);

                        #region Check Port (Common condition)
                        if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
                        {
                              errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
                              return false;
                        }
                        else if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
                        {
                              errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch portType=[{0}] PartialFullFlag=[{1}] ProductQuantity=[{2}]",
                                                      port.File.Type, port.File.PartialFullFlag, productQuantity);
                              return false;
                        }
                        else if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Count In Cassette，不需要進行比對
                        {
                            if (line.Data.LINETYPE == eLineType.CELL.CCCLN) //sy add for cst cleaner
                            {

                            }
                            else if ((port.File.Type == ePortType.LoadingPort /*|| port.File.Type == ePortType.BothPort*/) && productQuantity.Equals(0)) //防止Loader上空卡匣; 20160604 Pony提出BothPort可以上空卡 by JM.Pan
                            {
                                errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
                                return false;
                            }
                        }
                        #endregion

                        #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
                        //// 檢查 Port mode(EQP) 和 MES 下來的 ProductType(Job Data) 是否一致，若不一致要退 CST。
                        string jobType = string.Empty;
                        foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                        {
                              foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                              {
                                    eJobType productType = (eJobType)int.Parse(product.PRODUCTTYPE);

                                    if (productType == eJobType.Unknown)
                                    {
                                          errMsg = string.Format("Cassette MAP Transfer Error: Job Product Type Invalid. Equipment=[{0}], Port=[{1}]",
                                                  eqp.Data.NODENO, port.Data.PORTNO);
                                          return false;
                                    }
                                    else
                                    {
                                          bool checkng = false;
                                          //Jun Modify 20141208 Cell後段Unloader Port邏輯不同，所以移到Cell Special Check
                                          if (port.File.Type == ePortType.LoadingPort)
                                          {
                                                switch (productType)
                                                {
                                                      case eJobType.TFT: if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.MIX) checkng = true; break;
                                                      case eJobType.CF: if (port.File.Mode != ePortMode.CF && port.File.Mode != ePortMode.MIX) checkng = true; break;
                                                      case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                                                      case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                                                      case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                                                      case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
                                                      default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                                                }
                                          }
                                          else if (port.File.Type == ePortType.UnloadingPort)
                                          {
                                                switch (line.Data.LINETYPE)
                                                {
                                                      case eLineType.CELL.CBCUT_1:
                                                      case eLineType.CELL.CBCUT_2:
                                                      case eLineType.CELL.CBCUT_3:
                                                      case eLineType.CELL.CBSOR_1:
                                                      case eLineType.CELL.CBSOR_2:
                                                      case eLineType.CELL.CBLOI:
                                                            switch (productType)
                                                            {
                                                                  case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                                                                  case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                                                                  default: if (port.File.Mode == ePortMode.Dummy) checkng = true; break;
                                                            }
                                                            break;

                                                      default:
                                                            switch (productType)
                                                            {
                                                                  case eJobType.TFT: if (port.File.Mode != ePortMode.TFT) checkng = true; break;
                                                                  case eJobType.CF: if (port.File.Mode != ePortMode.CF) checkng = true; break;
                                                                  case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                                                                  case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
                                                                  case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
                                                                  case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
                                                                  default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
                                                            }
                                                            break;
                                                }
                                          }

                                          if (checkng)
                                          {
                                                //依據博章2015-03-26上午九點的mail, 將Product Type與Port Mode的判斷註解
                                                //--------------------------------
                                                //目前發現OPI在下Partial Full Cassette時，好像會卡Port Mode(TFT Mode),請看附圖
                                                //剛好這條Line Unload Port Mode不會有TFT Mode,只有OK/NG/MIX這三種，這樣會有問題
                                                //今天詢問褔杰的結果是，手動上”Cassette"時不要卡Port Mode ＆ Slot Information,讓機台自己檢查即可...
                                                //--------------------------------
                                                //errMsg = string.Format("Cassette MAP Transfer Error: Job Product Type[{0}] mismatch with EQP Port Mode[{1}]. Equipment=[{2}], Port=[{3}]",
                                                //    productType, port.File.Mode, eqp.Data.NODENO, port.Data.PORTNO);
                                                //return false;
                                          }
                                    }
                              }
                        }
                        #endregion

                        #region Check Port (Special condition by Line Type)
                        switch (line.Data.LINETYPE)
                        {
                              case eLineType.CELL.CBPOL_1:
                              case eLineType.CELL.CBPOL_2:
                              case eLineType.CELL.CBPOL_3:
                                    //if (port.File.Type == ePortType.UnloadingPort && port.File.PartialFullFlag == eParitalFull.PartialFull)
                                    //{
                                    //    foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                    //    {
                                    //        if (lot.PRODUCTLIST.Where(p => p.PRODUCTGRADE.Equals("P7")).Count() > 0) return false;
                                    //    }
                                    //}
                                    if (port.File.Type == ePortType.UnloadingPort)
                                    {
                                          //當POL ULD上Partial Cassette時, Job Grade只能上P7的等級
                                          bool is_partial_cassette = false;
                                          foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                          {
                                                if (lot.PRODUCTLIST.Count > 0)
                                                {
                                                      is_partial_cassette = true;
                                                      break;
                                                }
                                          }

                                          if (is_partial_cassette)
                                          {
                                                foreach (OfflineModeCassetteDataSend.LOTDATAc lot in data.BODY.LOTLIST)
                                                {
                                                      foreach (OfflineModeCassetteDataSend.PRODUCTDATAc product in lot.PRODUCTLIST)
                                                      {
                                                            if (product.PRODUCTGRADE != "P7")
                                                            {
                                                                  string.Format("Cassette Map Data Download Error: Partial Full Cassette Glass Count=[{0}], But Download Slot[{1}] GlassID[{2}] ProductGrade!=[P7].",
                                                                      productQuantity, product.SLOTNO, product.PRODUCTNAME);
                                                                  return false;
                                                            }
                                                      }
                                                }
                                          }
                                    }
                                    break;
                        }
                        #endregion

                        return true;
                  }
                  catch (Exception ex)
                  {
                        errMsg = ex.ToString();
                        return false;
                  }
            }

            ////CELL資料確認 for DPI
            //private bool ValidateCassetteCheckData_DPI(OfflineModeCassetteDataSend_DPI data, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            //{
            //    errMsg = string.Empty;

            //    try
            //    {
            //        string lineName = data.BODY.LINENAME;

            //        string cstid = port.File.CassetteID;
            //        string portName = port.Data.PORTID;
            //        int productQuantity;
            //        int.TryParse(data.BODY.PORTLIST.SingleOrDefault(p => p.PORTID == portName).PRODUCTQUANTITY, out productQuantity);

            //        #region Check Port (Common condition)
            //        if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
            //        {
            //            errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
            //            return false;
            //        }
            //        else if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
            //        {
            //            errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch portType=[{0}] PartialFullFlag=[{1}] ProductQuantity=[{2}]",
            //                                    port.File.Type, port.File.PartialFullFlag, productQuantity);
            //            return false;
            //        }
            //        else if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Count In Cassette，不需要進行比對
            //        {
            //            if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0)) //防止Loader/Both Port上空卡匣
            //            {
            //                errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
            //                return false;
            //            }
            //        }

            //        #endregion

            //        #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
            //        //// 檢查 Port mode(EQP) 和 MES 下來的 ProductType(Job Data) 是否一致，若不一致要退 CST。
            //        string jobType = string.Empty;
            //        //foreach (OfflineModeCassetteDataSend_DPI.LOTDATAc lot in data.BODY.PORTLIST.SingleOrDefault(p => p.PORTID == portName).LOTLIST)
            //        OfflineModeCassetteDataSend_DPI.LOTDATAc lot = data.BODY.PORTLIST.SingleOrDefault(p => p.PORTID == portName).LOTDATA;
            //        {
            //            foreach (OfflineModeCassetteDataSend_DPI.PRODUCTDATAc product in lot.PRODUCTLIST)
            //            {
            //                eJobType productType = (eJobType)int.Parse(product.PRODUCTTYPE);

            //                if (productType == eJobType.Unknown)
            //                {
            //                    errMsg = string.Format("Cassette MAP Transfer Error: Job Product Type Invalid. Equipment=[{0}], Port=[{1}]",
            //                            eqp.Data.NODENO, port.Data.PORTNO);
            //                    return false;
            //                }
            //                else
            //                {
            //                    bool checkng = false;
            //                    //Jun Modify 20141208 Cell後段Unloader Port邏輯不同，所以移到Cell Special Check
            //                    if (port.File.Type == ePortType.LoadingPort)
            //                    {
            //                        switch (productType)
            //                        {
            //                            case eJobType.TFT: if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.MIX) checkng = true; break;
            //                            case eJobType.CF: if (port.File.Mode != ePortMode.CF && port.File.Mode != ePortMode.MIX) checkng = true; break;
            //                            case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                            case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                            case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                            case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
            //                            default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                        }
            //                    }
            //                    else if (port.File.Type == ePortType.UnloadingPort)
            //                    {
            //                        switch (line.Data.LINETYPE)
            //                        {
            //                            case eLineType.CELL.CBCUT_1:
            //                            case eLineType.CELL.CBCUT_2:
            //                            case eLineType.CELL.CBCUT_3:
            //                            case eLineType.CELL.CBSOR_1:
            //                            case eLineType.CELL.CBSOR_2:
            //                            case eLineType.CELL.CBLOI:
            //                                switch (productType)
            //                                {
            //                                    case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                                    case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                                    default: if (port.File.Mode == ePortMode.Dummy) checkng = true; break;
            //                                }
            //                                break;

            //                            default:
            //                                switch (productType)
            //                                {
            //                                    case eJobType.TFT: if (port.File.Mode != ePortMode.TFT) checkng = true; break;
            //                                    case eJobType.CF: if (port.File.Mode != ePortMode.CF) checkng = true; break;
            //                                    case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                                    case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                                    case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                                    case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
            //                                    default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                                }
            //                                break;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        #endregion

            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        errMsg = ex.ToString();
            //        return false;
            //    }
            //}

            //private bool ValidateCassetteCheckData_PRM(OfflineModeCassetteDataSend_PRM data, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, out string errMsg)
            //{
            //    errMsg = string.Empty;

            //    try
            //    {
            //        string lineName = data.BODY.LINENAME;
            //        string cstid = data.BODY.CASSETTEID;
            //        string portName = data.BODY.PORTID;
            //        int productQuantity = data.BODY.PRODUCTLIST.Count;

            //        #region Check Port (Common condition)
            //        if (port.File.Type == ePortType.UnloadingPort && productQuantity.Equals(port.Data.MAXCOUNT)) //防止Unloader上實卡匣 
            //        {
            //            errMsg = "Cassette Data Transfer Error: Unloader Port loads full CST, unable to receive glass.";
            //            return false;
            //        }
            //        else if (port.File.Type == ePortType.UnloadingPort && eqp.File.PartialFullMode == eEnableDisable.Disable && productQuantity > 0) //防止Unloader在非PartialFull時，上有玻璃的卡匣
            //        {
            //            errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch portType=[{0}] PartialFullFlag=[{1}] ProductQuantity=[{2}]",
            //                                    port.File.Type, port.File.PartialFullFlag, productQuantity);
            //            return false;
            //        }
            //        else if (port.Data.MAPPINGENABLE == "TRUE")  //Jun Modify 20150309 Virtual Port不會上報Job Count In Cassette，不需要進行比對
            //        {
            //            if ((port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort) && productQuantity.Equals(0)) //防止Loader/Both Port上空卡匣
            //            {
            //                errMsg = string.Format("Cassette Data Transfer Error: {0} loads empty CST , unable to fetch glass.", port.File.Type.ToString());
            //                return false;
            //            }
            //        }
            //        #endregion

            //        #region Check JobType TODO：待確認 Port Mode 與 ProductType 的內容。
            //        //// 檢查 Port mode(EQP) 和 MES 下來的 ProductType(Job Data) 是否一致，若不一致要退 CST。
            //        string jobType = string.Empty;
            //        foreach (OfflineModeCassetteDataSend_PRM.PRODUCTDATAc product in data.BODY.PRODUCTLIST)
            //        {
            //            eJobType productType = (eJobType)int.Parse("1");//(eJobType)int.Parse(product.PRODUCTTYPE);

            //            if (productType == eJobType.Unknown)
            //            {
            //                errMsg = string.Format("Cassette MAP Transfer Error: Job Product Type Invalid. Equipment=[{0}], Port=[{1}]",
            //                        eqp.Data.NODENO, port.Data.PORTNO);
            //                return false;
            //            }
            //            else
            //            {
            //                bool checkng = false;
            //                //Jun Modify 20141208 Cell後段Unloader Port邏輯不同，所以移到Cell Special Check
            //                if (port.File.Type == ePortType.LoadingPort)
            //                {
            //                    switch (productType)
            //                    {
            //                    case eJobType.TFT: if (port.File.Mode != ePortMode.TFT && port.File.Mode != ePortMode.MIX) checkng = true; break;
            //                    case eJobType.CF: if (port.File.Mode != ePortMode.CF && port.File.Mode != ePortMode.MIX) checkng = true; break;
            //                    case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                    case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                    case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                    case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
            //                    default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                    }
            //                }
            //                else if (port.File.Type == ePortType.UnloadingPort)
            //                {
            //                    switch (line.Data.LINETYPE)
            //                    {
            //                    case eLineType.CELL.CBCUT_1:
            //                    case eLineType.CELL.CBCUT_2:
            //                    case eLineType.CELL.CBCUT_3:
            //                    case eLineType.CELL.CBSOR_1:
            //                    case eLineType.CELL.CBSOR_2:
            //                    case eLineType.CELL.CBLOI:
            //                        switch (productType)
            //                        {
            //                        case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                        case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                        default: if (port.File.Mode == ePortMode.Dummy) checkng = true; break;
            //                        }
            //                        break;

            //                    default:
            //                        switch (productType)
            //                        {
            //                        case eJobType.TFT: if (port.File.Mode != ePortMode.TFT) checkng = true; break;
            //                        case eJobType.CF: if (port.File.Mode != ePortMode.CF) checkng = true; break;
            //                        case eJobType.DM: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                        case eJobType.TR: if (port.File.Mode != ePortMode.ThroughDummy) checkng = true; break;
            //                        case eJobType.TK: if (port.File.Mode != ePortMode.ThicknessDummy) checkng = true; break;
            //                        case eJobType.UV: if (port.File.Mode != ePortMode.UVMask) checkng = true; break;
            //                        default: if (port.File.Mode != ePortMode.Dummy) checkng = true; break;
            //                        }
            //                        break;
            //                    }
            //                }

            //                if (checkng)
            //                {
            //                    //依據博章2015-03-26上午九點的mail, 將Product Type與Port Mode的判斷註解
            //                    //--------------------------------
            //                    //目前發現OPI在下Partial Full Cassette時，好像會卡Port Mode(TFT Mode),請看附圖
            //                    //剛好這條Line Unload Port Mode不會有TFT Mode,只有OK/NG/MIX這三種，這樣會有問題
            //                    //今天詢問褔杰的結果是，手動上”Cassette"時不要卡Port Mode ＆ Slot Information,讓機台自己檢查即可...
            //                    //--------------------------------
            //                    //errMsg = string.Format("Cassette MAP Transfer Error: Job Product Type[{0}] mismatch with EQP Port Mode[{1}]. Equipment=[{2}], Port=[{3}]",
            //                    //    productType, port.File.Mode, eqp.Data.NODENO, port.Data.PORTNO);
            //                    //return false;
            //                }
            //            }
            //        }
            //        #endregion
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        errMsg = ex.ToString();
            //        return false;
            //    }
            //}


            private eCVD_MIX_TYPE AnalysisCVD_PortMode(Port port)
            {
                  IList<ePortMode> modes = ObjectManager.PortManager.GetPorts().Select(p => p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().ToList<ePortMode>();
                  if (modes.Count > 2) return eCVD_MIX_TYPE.UNKNOWN;

                  switch (modes[0])
                  {
                        case ePortMode.HT:
                        case ePortMode.LT: return eCVD_MIX_TYPE.TYPE1_LT_HT;
                        case ePortMode.IGZO:
                              {
                                    if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                                    switch (modes[1])
                                    {
                                          case ePortMode.TFT: return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                                          case ePortMode.MQC: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                                          default: return eCVD_MIX_TYPE.UNKNOWN;
                                    }
                              }
                        case ePortMode.MQC:
                              {
                                    if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                                    switch (modes[1])
                                    {
                                          case ePortMode.IGZO: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                                          case ePortMode.TFT: return eCVD_MIX_TYPE.TYPE4_TFT_MQC;
                                          default: return eCVD_MIX_TYPE.UNKNOWN;
                                    }
                              }
                        case ePortMode.TFT:
                              {
                                    if (modes.Count == 1) return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                                    switch (modes[1])
                                    {
                                          case ePortMode.IGZO: return eCVD_MIX_TYPE.TYPE2_IGZO_TFT;
                                          case ePortMode.MQC: return eCVD_MIX_TYPE.TYPE3_IGZO_MQC;
                                          default: return eCVD_MIX_TYPE.UNKNOWN;
                                    }
                              }
                        default: return eCVD_MIX_TYPE.UNKNOWN;
                  }
            }

            private eDRY_MIX_TYPE AnalysisDRY_PortMode(Port port)
            {
                  IList<ePortMode> modes = ObjectManager.PortManager.GetPorts().Select(p =>
                      p.File.Mode).Where(p => p != ePortMode.Unknown).Distinct().ToList<ePortMode>();
                  if (modes.Count > 2) return eDRY_MIX_TYPE.UNKNOWN;

                  switch (modes[0])
                  {
                        case ePortMode.TFT:
                              {
                                    if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                                    switch (modes[1])
                                    {
                                          case ePortMode.ENG: return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                                          case ePortMode.IGZO: return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                                          default: return eDRY_MIX_TYPE.UNKNOWN;
                                    }
                              }
                        case ePortMode.ENG:
                              {
                                    if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                                    if (modes[1] == ePortMode.TFT)
                                          return eDRY_MIX_TYPE.TYPE1_TFT_ENG;
                                    else
                                          return eDRY_MIX_TYPE.UNKNOWN;
                              }
                        case ePortMode.IGZO:
                              {
                                    if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                                    switch (modes[1])
                                    {
                                          case ePortMode.TFT: return eDRY_MIX_TYPE.TYPE2_TFT_IGZO;
                                          case ePortMode.MQC: return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                                          default: return eDRY_MIX_TYPE.UNKNOWN;
                                    }
                              }
                        case ePortMode.MQC:
                              {
                                    if (modes.Count == 1) return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                                    if (modes[1] == ePortMode.IGZO)
                                          return eDRY_MIX_TYPE.TYPE3_IGZO_MQC;
                                    else
                                          return eDRY_MIX_TYPE.UNKNOWN;
                              }
                        default: return eDRY_MIX_TYPE.UNKNOWN;
                  }
            }

            //Array Job Special Data
            private void M2P_SpecialDataBy_Array(XmlNode lotNode, XmlNode productNode, Line line, Port port, ref Dictionary<string, string> recipeGroup, ref Job job)
            {
                  try
                  {
                        //AGINGENABLE: 1=ENABLE, 2=DISABLE
                        if (eLineType.ARRAY.TTP_VTEC == line.Data.LINETYPE)
                              job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, "Y");
                        else if (eLineType.ARRAY.BFG_SHUZTUNG == line.Data.LINETYPE)
                              job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, productNode[keyHost.SCRAPCUTFLAG].InnerText.Trim());
                        else if (eLineType.ARRAY.ELA_JSW == line.Data.LINETYPE)
                            job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, "");
                        else
                              job.ArraySpecial.GlassFlowType = GetArrayGlassFlowType(line, job.PPID, "");
                        //job.InspJudgedData = new string('0', 32);
                        //job.TrackingData = new string('0', 32);

                        //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.PROCESS_EQ)   // modify by bruce 2015/7/15 Array all line 都有此Item,不另外判斷
                        //{
                        #region  Recipe Group Number
                        
                        if (recipeGroup.Count == 0) //modify for IMP,.. use cc.kuang 2016/03/09
                        {
                            List<Port> lstPort;
                            lstPort = ObjectManager.PortManager.GetPorts();
                            foreach (Port pt in lstPort)
                            {
                                if (pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA || pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING
                                    || pt.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND
                                    || pt.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || pt.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                                {
                                    IList<Job> lstJob;
                                    lstJob = ObjectManager.JobManager.GetJobs(pt.File.CassetteSequenceNo);
                                    foreach (Job jb in lstJob)
                                    {
                                        if (jb.SamplingSlotFlag == "1")
                                        {
                                            if (jb.ArraySpecial.RecipeGroupNumber.Trim().Length == 0)
                                                continue;

                                            if (int.Parse(jb.ArraySpecial.RecipeGroupNumber.Trim()) <= 0 || int.Parse(jb.ArraySpecial.RecipeGroupNumber.Trim()) > 30)
                                                continue;

                                            if (!recipeGroup.ContainsKey(jb.PPID))
                                            {
                                                recipeGroup.Add(jb.PPID, jb.ArraySpecial.RecipeGroupNumber.Trim());
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (true)
                        {
                            #region  Recipe Group Number
                            if (!recipeGroup.ContainsKey(job.PPID))
                            {
                                //recipeGroup.Add(job.PPID, (recipeGroup.Count + 1).ToString());
                                int i;
                                for (i = 1; i < 31; i++)
                                {
                                    var item = recipeGroup.Where(k => k.Value.Trim().Equals(i.ToString())).Select(k => (KeyValuePair<string, string>?)k).FirstOrDefault();
                                    if (null == item)
                                        break;
                                }

                                if (i >= 31)
                                {
                                    recipeGroup.Add(job.PPID, "0"); //array over 30, set 0 and quit cst
                                }
                                else
                                {
                                    if (recipeGroup.Count == 0)
                                        recipeGroup.Add(job.PPID, "1");
                                    else
                                        recipeGroup.Add(job.PPID, i.ToString());
                                }
                            }
                            job.ArraySpecial.RecipeGroupNumber = recipeGroup[job.PPID];
                            #endregion
                        }

                        /*
                        if (!recipeGroup.ContainsKey(job.PPID))
                        {
                              recipeGroup.Add(job.PPID, (recipeGroup.Count + 1).ToString());
                        }
                        job.ArraySpecial.RecipeGroupNumber = recipeGroup[job.PPID];
                       */
                        #endregion
                        //}

                        // if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.ABFG) // modify by bruce 2015/7/15 改用line Type 判斷
                        if (line.Data.LINETYPE == eLineType.ARRAY.BFG_SHUZTUNG)
                        {
                              IDictionary<string, string> eqpData = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");

                              //t3 not use ScrapFlag 2015/10/15 cc.kuang
                              switch (productNode[keyHost.SCRAPCUTFLAG].InnerText.Trim())
                              {
                                    case "S":
                                          //if (eqpData.ContainsKey(eEQPFLAG.Array.ScrapFlag))
                                          {
                                                //eqpData[eEQPFLAG.Array.ScrapFlag] = "1";
                                                if (eqpData.ContainsKey(eEQPFLAG.Array.SmashFlag))
                                                      eqpData[eEQPFLAG.Array.SmashFlag] = "1";
                                          }
                                          break;
                                    case "C":
                                          //if (eqpData.ContainsKey(eEQPFLAG.Array.ScrapFlag))
                                          {
                                                //eqpData[eEQPFLAG.Array.ScrapFlag] = "1";
                                                if (eqpData.ContainsKey(eEQPFLAG.Array.CutFlag))
                                                      eqpData[eEQPFLAG.Array.CutFlag] = "1";
                                          }
                                          break;
                              }

                              job.EQPFlag = ObjectManager.SubJobDataManager.Encode(eqpData, "EQPFlag");
                        }
                        else
                        {
                              if (job.EQPFlag.Trim().Length == 0)
                                    job.EQPFlag = new string('0', 32);
                        }
                        #region  ELA1BY1Flag
                        if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                        {
                            Equipment ela;

                            ela = ObjectManager.EquipmentManager.GetEQP("L4");
                            string ela1_PPID = job.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                            ela = ObjectManager.EquipmentManager.GetEQP("L5");
                            string ela2_PPID = job.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                            string curByPassPPID = new string('0', ela.Data.RECIPELEN);
                            if (ela1_PPID != curByPassPPID && ela2_PPID == curByPassPPID)
                            {
                                job.ArraySpecial.ELA1BY1Flag = "L4";
                            }
                            else if (ela1_PPID == curByPassPPID && ela2_PPID != curByPassPPID)
                            {
                                job.ArraySpecial.ELA1BY1Flag = "L5";
                            }
                            else
                            {
                                job.ArraySpecial.ELA1BY1Flag = "L45";
                            }
                        }
                        #endregion
                        job.ArraySpecial.SourcePortNo = port.Data.PORTNO;

                        //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT) //modify by bruce 2015/7/15 Array T3 無 Sort line 
                        //{ 
                        //    job.ArraySpecial.SorterGrade = job.JobGrade;
                        //}
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            // 初始化 CF Job Special Data
            public void M2P_SpecialDataBy_CF(XmlNode body, XmlNode lotNode, XmlNode productNode, Line line, ref Job job)
            {
                  try
                  {
                        job.EQPReservations = SpecialItemInitial("EQPReservations", 6);
                        job.InspJudgedData = SpecialItemInitial("Insp.JudgedData", 16);
                        job.TrackingData = SpecialItemInitial("TrackingData", 16);
                        job.CFSpecialReserved = SpecialItemInitial("CFSpecialReserved", 16);

                        switch (line.Data.LINETYPE)
                        {
                              case "FCUPK_TYPE1":
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    break;

                              case "FCMPH_TYPE1":
                              case "FCSPH_TYPE1":
                              case "FCRPH_TYPE1":
                              case "FCGPH_TYPE1":
                              case "FCBPH_TYPE1":
                              case "FCOPH_TYPE1":
                                    job.InspJudgedData2 = SpecialItemInitial("Insp.JudgedData2", 16);
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 16);
                                    job.EQPFlag2 = SpecialItemInitial("EQPFlag2", 16);
                                    job.CFMarcoReserveFlag = SpecialItemInitial("MarcoReserveFlag", 16);
                                    job.CFProcessBackUp = SpecialItemInitial("ProcessBackUp", 8);
                                    break;

                              case "FCPSH_TYPE1":
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    break;

                              case "FCMSK_TYPE1":
                                    job.CFSpecialReserved = SpecialItemInitial("CFSpecialReserved", 16);
                                    break;

                              case "FCREW_TYPE1":
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    break;

                              case "FCSRT_TYPE1":
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    break;

                              case "FCREP_TYPE1":
                              case "FCREP_TYPE2":
                              case "FCREP_TYPE3":
                                    //if (job.EQPFlag == SpecialItemInitial("EQPFlag", 32))
                                    //{
                                    //    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    //}
                                    break;

                              case "FCMQC_TYPE1":
                              case "FCMQC_TYPE2":
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);

                                    //if (job.INSPReservations == SpecialItemInitial("INSPReservations", 6))
                                    //{
                                    //    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    //}
                                    break;

                              case "FCMAC_TYPE1":
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    break;

                              case "FCAOI_TYPE1":
                                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);
                                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            //Cell Job Special Data
            private void M2P_SpecialDataBy_CELL(XmlNode lotNode, XmlNode productNode, int i, Line line, Port port, ref Job job)
            {
                try
                {
                    //CELL 有跨超大線 PPID組成的問題, 當PRODUCTPROCESSTYPE為MMG時
                    /* 4線跨5線時:
                            第一組PPID:4線BCS本身的PPID為 拆LOT PPID的前兩個EQ(LD,CUT) + ProcessLineLst裡 LineName為4線的PPID, 排除前兩個EQ的PPID
                            第二組PPID:Process Line List 的5線PPID + STB Product Spec List的PPID
                    */

                    // 初始化參數內容
                    job.INSPReservations = SpecialItemInitial(eJOBDATA.INSPReservations, 6);
                    job.EQPReservations = SpecialItemInitial(eJOBDATA.EQPReservations, 6);
                    job.InspJudgedData = SpecialItemInitial(eJOBDATA.InspJudgedData, 32);
                    job.TrackingData = SpecialItemInitial(eJOBDATA.TrackingData, 32);
                    //job.EQPFlag = SpecialItemInitial(eJOBDATA.EQPFlag, 32); //20160114 sy 會蓋掉 資料 所以
                    //20151225 cy:指定ChipCount,正常看OXR的長度,PCS看BlockOX的長度,跟Online下貨一樣
                    job.ChipCount = (productNode["OXR"] == null ? 0 : (string.IsNullOrEmpty(productNode["OXR"].InnerText.Trim()) ? 0 : productNode["OXR"].InnerText.Trim().Length));
                    #region [CELL GlassOX]
                    if (job.ChipCount > 0)
                    {
                        string oxInfo = string.Empty;
                        for (int j = 0; j < productNode["OXR"].InnerText.Trim().Length; j++)
                        {
                            oxInfo += CellPanelOX(productNode["OXR"].InnerText.Trim().Substring(j, 1));
                        }
                        job.OXRInformation = oxInfo;
                        job.CellSpecial.PanelOXInformation = oxInfo;
                    }
                    
                    #endregion
                    #region [CELL BlockOX]
                    string blockOxInfo = string.Empty;
                    for (int k = 0; k < productNode["BLOCK_OX_INFO"].InnerText.Trim().Length; k++)
                    {
                        blockOxInfo += CellBlockOX(productNode["BLOCK_OX_INFO"].InnerText.Trim().Substring(k, 1));
                    }
                    job.CellSpecial.BlockOXInformation = blockOxInfo;
                    #endregion
                    //20151022 cy:Offline給的資料,不會有AbnormalCodeList
                    //// 取出 Abnormal Code List 內容
                    //XmlNodeList abnormalcodelist = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                    //string abnormalULD = string.Empty;  //Job Data
                    //string sealAbnormalFlag = string.Empty;  //Job Data
                    //string hvaChippigFlag = string.Empty;  //Job Data                
                    //string mgvFlag = string.Empty;  //Job Data
                    //string repairFlag = string.Empty;  //Job Data
                    //string ttpFlag = string.Empty;  //Job Data
                    //string cutGradeFlag = string.Empty;  //待確認
                    //string abnormalTFT = string.Empty;  //**File Service**
                    //string abnormalCF = string.Empty;  //**File Service**
                    //string abnormalLCD = string.Empty;  //**File Service**
                    //string deCreateFlag = string.Empty;  //**File Service**
                    //string fGradeFlag = string.Empty;  //**File Service**

                    //foreach (XmlNode abnormalcode in abnormalcodelist)
                    //{
                    //    string abnormalSeq = abnormalcode[keyHost.ABNORMALSEQ].InnerText.Trim();

                    //    switch (abnormalSeq)
                    //    {
                    //        //For Job Data Use
                    //        case "ABCODEULD": abnormalULD = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "SEALABNORMALFLAG": sealAbnormalFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "HVACHIPPINGFLAG": hvaChippigFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "MGVFLAG": mgvFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "REPAIRRESULTFLAG": repairFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "TTPFLAG": ttpFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;
                    //        case "CUTGradeFlag": cutGradeFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim(); break;

                    //        //For File Service Use
                    //        case "ABNORMALTFT":
                    //            abnormalTFT = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                    //            job.CellSpecial.AbnormalTFT = abnormalTFT;
                    //            break;
                    //        case "ABNORMALCF":
                    //            abnormalCF = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                    //            job.CellSpecial.AbnormalCF = abnormalCF;
                    //            break;
                    //        case "ABNORMALLCD":
                    //            abnormalLCD = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                    //            job.CellSpecial.AbnormalLCD = abnormalLCD;
                    //            break;
                    //        case "DECREASEFLAG":
                    //            deCreateFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                    //            job.CellSpecial.DeCreateFlag = deCreateFlag;
                    //            break;
                    //        case "FGRADERISKFLAG":
                    //            fGradeFlag = abnormalcode[keyHost.ABNORMALCODE].InnerText.Trim();
                    //            job.CellSpecial.FGradeFlag = fGradeFlag;
                    //            break;
                    //    }
                    //}
                    //double lotPnlSize = 0;
                    //if (double.TryParse(lotNode[keyHost.PRODUCTSIZE].InnerText, out lotPnlSize))
                    //    lotPnlSize = lotPnlSize * 100;
                    int networkNo = Convert.ToInt32(line.Data.LINEID.Substring(5, 1), 16);  //Jun Add 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                    XmlNodeList processList = lotNode[keyHost.PROCESSLINELIST].ChildNodes;
                    XmlNodeList stbList = lotNode[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                    IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
                    job.CellSpecial.TurnAngle = productNode["TRUNANGLEFLAG"].InnerText.Trim();
                    job.CellSpecial.PanelSize = productNode["PANELSIZE"].InnerText.Trim();
                    job.CellSpecial.ControlMode = line.File.HostMode;
                    job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                    job.CellSpecial.ProductOwner = Invoke(eServiceName.MESService, "CellProductOwner", new object[] { lotNode[keyHost.PRODUCTOWNER].InnerText }) as string;
                    job.CellSpecial.NetworkNo = networkNo.ToString();
                    //job.CellSpecial.BlockOXInformation = productNode["BLOCK_OX_INFO"].InnerText;
                    job.CellSpecial.PILiquidType = productNode["PI_LIQUID_TYPE"].InnerText.Trim();
                    job.CellSpecial.OperationID = productNode["OPERATION_ID"].InnerText;
                    job.CellSpecial.GlassThickness = string.IsNullOrEmpty(productNode["GLASS_THICKNESS"].InnerText) ? "0" : productNode["GLASS_THICKNESS"].InnerText;
                    job.CellSpecial.AssembleSeqNo = string.IsNullOrEmpty(productNode["ASSEMBLE_SEQNO"].InnerText) ? "0" : productNode["ASSEMBLE_SEQNO"].InnerText;
                    job.CellSpecial.UVMaskAlreadyUseCount = string.IsNullOrEmpty(productNode["UV_MASK_USE_COUNT"].InnerText) ? "0" : productNode["UV_MASK_USE_COUNT"].InnerText;
                    job.CellSpecial.CUTCassetteSettingCode = lotNode["CSTSETTINGCODE_CUT"].InnerText;
                    job.CellSpecial.CUTProductType = lotNode["BCPRODUCTTYPE_CUT"].InnerText;
                    job.CellSpecial.CUTProductID = lotNode["PRODUCTID_CUT"].InnerText;
                    job.CellSpecial.RejudgeCount = string.IsNullOrEmpty(productNode["REJUDGE_COUNT"].InnerText) ? "0" : productNode["REJUDGE_COUNT"].InnerText;
                    job.CellSpecial.VendorName = productNode["VENDER_NAME"].InnerText;
                    job.CellSpecial.BURCheckCount = string.IsNullOrEmpty(productNode["BUR_CHECK_COUNT"].InnerText) ? "0" : productNode["BUR_CHECK_COUNT"].InnerText;
                    //20151229 cy:因為每條Line都會有, 先指定成Lot層的, 有特殊指定的, 再重新賦值
                    job.CellSpecial.CassetteSettingCode = lotNode["CSTSETTINGCODE"].InnerText;
                    job.CellSpecial.MaxRwkCount = string.IsNullOrEmpty(productNode["MAX_REWORK_COUNT"].InnerText) ? "0" : productNode["MAX_REWORK_COUNT"].InnerText;
                    job.CellSpecial.CurrentRwkCount = string.IsNullOrEmpty(productNode["CURRENT_REWORK_COUNT"].InnerText) ? "0" : productNode["CURRENT_REWORK_COUNT"].InnerText;
                    job.CellSpecial.DotRepairCount = string.IsNullOrEmpty(productNode["DOT_REPAIR_COUNT"].InnerText) ? "0" : productNode["DOT_REPAIR_COUNT"].InnerText;
                    job.CellSpecial.LineRepairCount = string.IsNullOrEmpty(productNode["LINE_REPAIR_COUNT"].InnerText) ? "0" : productNode["LINE_REPAIR_COUNT"].InnerText;
                    job.CellSpecial.OQCBank = string.IsNullOrEmpty(productNode["OQC_BANK"].InnerText) ? "0" : productNode["OQC_BANK"].InnerText;
                    job.CellSpecial.BlockCount = (productNode["BLOCK_OX_INFO"] == null ? "0" : (string.IsNullOrEmpty(productNode["BLOCK_OX_INFO"].InnerText.Trim()) ? "0" : productNode["BLOCK_OX_INFO"].InnerText.Trim().Length.ToString()));
                    #region Job Data Line Special
                    switch (line.Data.JOBDATALINETYPE)
                    {
                        case eJobDataLineType.CELL.CCPIL:
                            //job.CellSpecial.ControlMode = line.File.HostMode;
                            //job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                            //job.CellSpecial.ProductOwner = Invoke(eServiceName.MESService, "CellProductOwner", new object[] { lotNode[keyHost.PRODUCTOWNER].InnerText }) as string;
                            //job.CellSpecial.NetworkNo = networkNo.ToString();
                            //job.CellSpecial.BlockOXInformation = productNode["BLOCK_OX_INFO"].InnerText;
                            //job.CellSpecial.PILiquidType = productNode["PI_LIQUID_TYPE"].InnerText.Trim();
                            //job.CellSpecial.OperationID = productNode["OPERATION_ID"].InnerText;
                            //job.CellSpecial.GlassThickness = productNode["GLASS_THICKNESS"].InnerText;
                            break;

                        case eJobDataLineType.CELL.CCODF:
                            //job.CellSpecial.BlockOXInformation = productNode["BLOCK_OX_INFO"].InnerText;//SpecialItemInitial(eJOBDATA.BlockOXInformation, 48);
                            //job.CellSpecial.ControlMode = line.File.HostMode;
                            //job.CellSpecial.GlassThickness = productNode["GLASS_THICKNESS"].InnerText;
                            //job.CellSpecial.OperationID = productNode["OPERATION_ID"].InnerText;
                            //job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                            //job.CellSpecial.AssembleSeqNo = productNode["ASSEMBLE_SEQNO"].InnerText;
                            //job.CellSpecial.ProductOwner = Invoke(eServiceName.MESService, "CellProductOwner", new object[] { lotNode[keyHost.PRODUCTOWNER].InnerText }) as string;
                            //job.CellSpecial.NetworkNo = networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                            //job.CellSpecial.UVMaskAlreadyUseCount = productNode["UV_MASK_USE_COUNT"].InnerText; //"0";
                            break;

                        case eJobDataLineType.CELL.CCCUT:
                            //job.CellSpecial.NetworkNo = networkNo.ToString();  //ParameterManager["NETWORKNO"].Value.ToString();  //Jun Modify 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                            //job.CellSpecial.CUTCassetteSettingCode = lotNode["CSTSETTINGCODE_CUT"].InnerText;
                            //job.CellSpecial.CUTProductType = lotNode["BCPRODUCTTYPE_CUT"].InnerText;
                            //job.CellSpecial.CUTProductID = lotNode["PRODUCTID_CUT"].InnerText;
                            //job.CellSpecial.RejudgeCount = productNode["REJUDGE_COUNT"].InnerText;
                            //job.CellSpecial.VendorName = productNode["VENDER_NAME"].InnerText;
                            //job.CellSpecial.BURCheckCount = productNode["BUR_CHECK_COUNT"].InnerText;
                            job.ChipCount = (productNode["BLOCK_OX_INFO"] == null ? 0 : (string.IsNullOrEmpty(productNode["BLOCK_OX_INFO"].InnerText.Trim()) ? 0 : productNode["BLOCK_OX_INFO"].InnerText.Trim().Length));

                            string disCardJudges = new string('0', 32);
                            //0:A 25:Z 23:X 不管有沒有給X 要下給機台都要有X
                            disCardJudges = disCardJudges.Substring(0, 23) + "1" + disCardJudges.Substring(23 + 1, disCardJudges.Length - 1 - 23);
                            job.CellSpecial.DisCardJudges = disCardJudges;
                            break;
                        case eJobDataLineType.CELL.CCPCS:
                            job.CellSpecial.CUTCassetteSettingCode2 = lotNode["CSTSETTINGCODE2"].InnerText;//20170718 huangjiayin add
                            job.CellSpecial.PCSCassetteSettingCodeList = lotNode["PCSCSTSETTINGCODELIST"].InnerText;//20170718 huangjiayin add
                            job.CellSpecial.PCSBlockSizeList=lotNode["PCSBLOCKSIZELIST"].InnerText;//20170718 huangjiayin add
                            job.CellSpecial.BlockSize1=lotNode["BLOCKSIZE1"].InnerText;//20170718 huangjiayin add
                            job.CellSpecial.BlockSize2=lotNode["BLOCKSIZE2"].InnerText;//20170718 huangjiayin add
                            job.ChipCount = (productNode["BLOCK_OX_INFO"] == null ? 0 : (string.IsNullOrEmpty(productNode["BLOCK_OX_INFO"].InnerText.Trim()) ? 0 : productNode["BLOCK_OX_INFO"].InnerText.Trim().Length));
                            break;

                        case eJobDataLineType.CELL.CCPTH:
                            //job.CellSpecial.GlassThickness = productNode["GLASS_THICKNESS"].InnerText;
                            //job.CellSpecial.OperationID = productNode["OPERATION_ID"].InnerText;
                            //job.CellSpecial.OwnerID = productNode[keyHost.OWNERID].InnerText;
                            //job.CellSpecial.ProductOwner = Invoke(eServiceName.MESService, "CellProductOwner", new object[] { lotNode[keyHost.PRODUCTOWNER].InnerText }) as string;
                            break;
                        case eJobDataLineType.CELL.CCSOR:
                            //string eqpSortFlag = string.Empty;
                            //for (int k = 0; k < job.EQPFlag.Substring(1, 5).Length; k++)
                            //{
                            //    eqpSortFlag += job.EQPFlag.Substring(1, 5).Substring(job.EQPFlag.Substring(1, 5).Length-1-k,1);
                            //}
                            //job.CellSpecial.SortFlagNo = Convert.ToInt32(eqpSortFlag, 2).ToString().PadLeft(2, '0');
                            break;
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }

            private string CellPanelOX(string OX)
            {
                try
                {
                    switch (OX.Trim())
                    {
                        case "0": return "O";
                        case "1": return "X";
                        case "2": return "A";
                        case "3": return "B";
                        case "4": return "C";
                        case "5": return "D";
                        case "6": return "E";
                        case "7": return "F";
                        case "8": return "G";
                        case "9": return "H";
                        case "10": return "I";
                        case "11": return "J";
                        case "12": return "K";
                        case "13": return "L";
                        case "14": return "M";
                        case "15": return "N";
                        case "O": return "O";
                        case "X": return "X";
                        case "A": return "A";
                        case "B": return "B";
                        case "C": return "C";
                        case "D": return "D";
                        case "E": return "E";
                        case "F": return "F";
                        case "G": return "G";
                        case "H": return "H";
                        case "I": return "I";
                        case "J": return "J";
                        case "K": return "K";
                        case "L": return "L";
                        case "M": return "M";
                        case "N": return "N";
                        default: return "O";
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    return "O";
                }
            }
            private string CellBlockOX(string OX)
            {
                try
                {
                    switch (OX.Trim())
                    {
                        case "0": return "O";
                        case "1": return "C";
                        case "2": return "A";
                        case "O": return "O";
                        case "C": return "C";
                        case "A": return "A";
                        default: return "O";
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    return "O";
                }
            }
            private void M2P_SpecialDataBy_DPI(XmlNode lotNode, XmlNode productNode, int i, string mesOXR, Line line, Port port, ref Job job)
            {
                  try
                  {
                        int networkNo = Convert.ToInt32(line.Data.LINEID.Substring(5, 1), 16);  //Jun Add 20150323 CSOT說直接抓Line ID倒數第三碼，不要使用Parameter參數
                        IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");

                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.CassetteSettingCode = lotNode["CSTSETTINGCODE"].InnerText;
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void MESDataIntoCstObject(XmlNode bodyNode, ref Cassette cst)
            {
                  try
                  {
                        lock (cst)
                        {
                              //cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText == null ? "" : bodyNode[keyHost.LINERECIPENAME].InnerText;
                              cst.PortID = bodyNode[keyHost.PORTID].InnerText == null ? "" : bodyNode[keyHost.PORTID].InnerText;
                              cst.PortNo = bodyNode[keyHost.PORTNO].InnerText == null ? "" : bodyNode[keyHost.PORTNO].InnerText;
                              if (cst.OFFLINE_CstData == null) cst.OFFLINE_CstData = new OFFLINE_CstBody();
                              cst.OFFLINE_CstData.LINENAME = bodyNode["LINENAME"].InnerText == null ? "" : bodyNode["LINENAME"].InnerText;
                              cst.OFFLINE_CstData.EQUIPMENTNO = bodyNode["EQUIPMENTNO"].InnerText == null ? "" : bodyNode["EQUIPMENTNO"].InnerText;
                              cst.OFFLINE_CstData.PORTNO = bodyNode["PORTNO"].InnerText == null ? "" : bodyNode["PORTNO"].InnerText;
                              cst.OFFLINE_CstData.PORTID = bodyNode[keyHost.PORTID].InnerText == null ? "" : bodyNode[keyHost.PORTID].InnerText;
                              cst.OFFLINE_CstData.CASSETTEID = bodyNode[keyHost.CASSETTEID].InnerText == null ? "" : bodyNode[keyHost.CASSETTEID].InnerText;
                              //cst.OFFLINE_CstData.LINEOPERMODE = bodyNode[keyHost.LINEOPERMODE].InnerText == null ? "" : bodyNode[keyHost.LINEOPERMODE].InnerText;
                              cst.OFFLINE_CstData.PRODUCTQUANTITY = bodyNode[keyHost.PRODUCTQUANTITY].InnerText == null ? "" : bodyNode[keyHost.PRODUCTQUANTITY].InnerText;
                              cst.OFFLINE_CstData.CLEANFLAG = string.Empty; // bodyNode[keyHost.CLEANFLAG].InnerText == null ? "" : bodyNode[keyHost.CLEANFLAG].InnerText;
                              cst.OFFLINE_CstData.REMAPFLAG = bodyNode["REMAPFLAG"].InnerText == null ? "" : bodyNode["REMAPFLAG"].InnerText;
                              cst.OFFLINE_CstData.CSTSETTINGCODE = bodyNode["CSTSETTINGCODE"].InnerText == null ? "" : bodyNode["CSTSETTINGCODE"].InnerText;

                              XmlNodeList lotList = bodyNode[keyHost.LOTLIST].ChildNodes;
                              foreach (XmlNode lot in lotList)
                              {
                                    LOTDATAc _lot = new LOTDATAc();
                                    _lot.LOTNAME = lot[keyHost.LOTNAME].InnerText == null ? "" : lot[keyHost.LOTNAME].InnerText;
                                    _lot.PROCESSOPERATIONNAME = lot[keyHost.PROCESSOPERATIONNAME].InnerText == null ? "" : lot[keyHost.PROCESSOPERATIONNAME].InnerText;
                                    _lot.PRODUCTOWNER = lot[keyHost.PRODUCTOWNER].InnerText == null ? "" : lot[keyHost.PRODUCTOWNER].InnerText;
                                    _lot.PRODUCTSPECNAME = lot[keyHost.PRODUCTSPECNAME].InnerText == null ? "" : lot[keyHost.PRODUCTSPECNAME].InnerText;
                                    _lot.BCPRODUCTTYPE = lot[keyHost.BCPRODUCTTYPE].InnerText == null ? "" : lot[keyHost.BCPRODUCTTYPE].InnerText;
                                    //_lot.BCPRODUCTTYPE_POL = lot["BCPRODUCTTYPE_POL"].InnerText == null ? "" : lot["BCPRODUCTTYPE_POL"].InnerText;
                                    _lot.BCPRODUCTTYPE_CUT = lot["BCPRODUCTTYPE_CUT"].InnerText == null ? "" : lot["BCPRODUCTTYPE_CUT"].InnerText;
                                    //_lot.BCPRODUCTTYPE_CUT_CROSS = lot["BCPRODUCTTYPE_CUT_CROSS"].InnerText == null ? "" : lot["BCPRODUCTTYPE_CUT_CROSS"].InnerText;
                                    _lot.PRODUCTID = lot["PRODUCTID"].InnerText == null ? "" : lot["PRODUCTID"].InnerText;
                                    //_lot.PRODUCTID_POL = lot["PRODUCTID_POL"].InnerText == null ? "" : lot["PRODUCTID_POL"].InnerText;
                                    _lot.PRODUCTID_CUT = lot["PRODUCTID_CUT"].InnerText == null ? "" : lot["PRODUCTID_CUT"].InnerText;
                                    //_lot.PRODUCTID_CUT_CROSS = lot["PRODUCTID_CUT_CROSS"].InnerText == null ? "" : lot["PRODUCTID_CUT_CROSS"].InnerText;
                                    _lot.CFREWORKCOUNT = lot[keyHost.CFREWORKCOUNT].InnerText == null ? "" : lot[keyHost.CFREWORKCOUNT].InnerText;
                                    //_lot.PRODUCTSIZE = lot[keyHost.PRODUCTSIZE].InnerText == null ? "" : lot[keyHost.PRODUCTSIZE].InnerText;
                                    _lot.LINERECIPENAME = lot[keyHost.LINERECIPENAME].InnerText == null ? "" : lot[keyHost.LINERECIPENAME].InnerText;
                                    _lot.PPID = lot[keyHost.PPID].InnerText == null ? "" : lot[keyHost.PPID].InnerText;
                                    //20151224 cy:OPI_CURRENTLINEPPID/OPI_CROSSLINEPPID已經不用了
                                    //_lot.OPI_CURRENTLINEPPID = lot["OPI_CURRENTLINEPPID"].InnerText == null ? "" : lot["OPI_CURRENTLINEPPID"].InnerText;
                                    //_lot.OPI_CROSSLINEPPID = lot["OPI_CROSSLINEPPID"].InnerText == null ? "" : lot["OPI_CROSSLINEPPID"].InnerText;
                                    _lot.CSTSETTINGCODE = lot["CSTSETTINGCODE"].InnerText == null ? "" : lot["CSTSETTINGCODE"].InnerText;
                                    //20151222 cy add for CUT
                                    _lot.CSTSETTINGCODE_CUT = lot["CSTSETTINGCODE_CUT"].InnerText == null ? "" : lot["CSTSETTINGCODE"].InnerText;

                                    XmlNodeList processLineList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                                    foreach (XmlNode process in processLineList)
                                    {
                                          OFFLINEPROCESSLINEc _process = new OFFLINEPROCESSLINEc();
                                          _process.LINENAME = process[keyHost.LINENAME].InnerText == null ? "" : process[keyHost.LINENAME].InnerText;
                                          _process.LINERECIPENAME = process[keyHost.LINERECIPENAME].InnerText == null ? "" : process[keyHost.LINERECIPENAME].InnerText;
                                          _process.PPID = process[keyHost.PPID].InnerText == null ? "" : process[keyHost.PPID].InnerText;
                                          _process.CSTSETTINGCODE = process["CSTSETTINGCODE"].InnerText == null ? "" : process["CSTSETTINGCODE"].InnerText;

                                          _lot.PROCESSLINELIST.Add(_process);
                                    }

                                    XmlNodeList stbProductSpecList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                                    foreach (XmlNode stb in stbProductSpecList)
                                    {
                                          OFFLINESTBPRODUCTSPECc _stb = new OFFLINESTBPRODUCTSPECc();
                                          _stb.LINENAME = stb[keyHost.LINENAME].InnerText == null ? "" : stb[keyHost.LINENAME].InnerText;
                                          _stb.LINERECIPENAME = stb[keyHost.LINERECIPENAME].InnerText == null ? "" : stb[keyHost.LINERECIPENAME].InnerText;
                                          _stb.PPID = stb[keyHost.PPID].InnerText == null ? "" : stb[keyHost.PPID].InnerText;
                                          _stb.CSTSETTINGCODE = stb["CSTSETTINGCODE"].InnerText == null ? "" : stb["CSTSETTINGCODE"].InnerText;

                                          _lot.STBPRODUCTSPECLIST.Add(_stb);
                                    }

                                    XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                                    foreach (XmlNode product in productList)
                                    {
                                          OFFLINEPRODUCTDATAc _product = new OFFLINEPRODUCTDATAc();
                                          _product.SLOTNO = product["SLOTNO"].InnerText == null ? "" : product["SLOTNO"].InnerText;
                                          _product.PROCESSFLAG = product[keyHost.PROCESSFLAG].InnerText == null ? "" : product[keyHost.PROCESSFLAG].InnerText;
                                          _product.PRODUCTNAME = product[keyHost.PRODUCTNAME].InnerText == null ? "" : product[keyHost.PRODUCTNAME].InnerText;
                                          _product.PRODUCTRECIPENAME = product[keyHost.PRODUCTRECIPENAME].InnerText == null ? "" : product[keyHost.PRODUCTRECIPENAME].InnerText;
                                          _product.OPI_PPID = product[keyHost.OPI_PPID].InnerText == null ? "" : product[keyHost.OPI_PPID].InnerText;
                                          _product.PRODUCTTYPE = product[keyHost.PRODUCTTYPE].InnerText == null ? "" : product[keyHost.PRODUCTTYPE].InnerText;
                                          _product.PRODUCTGRADE = product[keyHost.PRODUCTGRADE].InnerText == null ? "" : product[keyHost.PRODUCTGRADE].InnerText;
                                          _product.PRODUCTJUDGE = product[keyHost.PRODUCTJUDGE].InnerText == null ? "" : product[keyHost.PRODUCTJUDGE].InnerText;
                                          _product.GROUPID = product[keyHost.GROUPID].InnerText == null ? "" : product[keyHost.GROUPID].InnerText;
                                          //_product.OXR = product["OXR"].InnerText == null ? "" : product["OXR"].InnerText;
                                          //_product.TEMPERATUREFLAG = product[keyHost.TEMPERATUREFLAG].InnerText == null ? "" : product[keyHost.TEMPERATUREFLAG].InnerText;
                                          _product.PROCESSTYPE = product["PROCESSTYPE"].InnerText == null ? "" : product["PROCESSTYPE"].InnerText;
                                          _product.TARGETCSTID = product["TARGETCSTID"].InnerText == null ? "" : product["TARGETCSTID"].InnerText;
                                          _product.INSPRESERVATION = product["INSPRESERVATION"].InnerText == null ? "" : product["INSPRESERVATION"].InnerText;
                                          _product.PREINLINEID = product["PREINLINEID"].InnerText == null ? "" : product["PREINLINEID"].InnerText;
                                          _product.FLOWPRIORITY = product["FLOWPRIORITY"].InnerText == null ? "" : product["FLOWPRIORITY"].InnerText;
                                          _product.NETWORKNO = product["NETWORKNO"].InnerText == null ? "" : product["NETWORKNO"].InnerText;
                                          //_product.REPAIRCOUNT = product["REPAIRCOUNT"].InnerText == null ? "" : product["REPAIRCOUNT"].InnerText;
                                          //_product.ABNORMALCODE = product["ABNORMALCODE"].InnerText == null ? "" : product["ABNORMALCODE"].InnerText;
                                          //_product.CUTTINGFLAG = product["CUTTINGFLAG"].InnerText == null ? "" : product["CUTTINGFLAG"].InnerText;
                                          _product.OWNERTYPE = product[keyHost.OWNERTYPE].InnerText == null ? "" : product[keyHost.OWNERTYPE].InnerText;
                                          _product.OWNERID = product[keyHost.OWNERID].InnerText == null ? "" : product[keyHost.OWNERID].InnerText;
                                          _product.REVPROCESSOPERATIONNAME = product[keyHost.REVPROCESSOPERATIONNAME].InnerText == null ? "" : product[keyHost.REVPROCESSOPERATIONNAME].InnerText;
                                          _product.EQPFLAG = product["EQPFLAG"].InnerText == null ? "" : product["EQPFLAG"].InnerText;
                                          _product.SUBSTRATETYPE = product["SUBSTRATETYPE"].InnerText == null ? "" : product["SUBSTRATETYPE"].InnerText;
                                          //_product.AGINGENABLE = product[keyHost.AGINGENABLE].InnerText == null ? "" : product[keyHost.AGINGENABLE].InnerText;
                                          _product.COAVERSION = product["COAVERSION"].InnerText == null ? "" : product["COAVERSION"].InnerText;
                                          //_product.OXRFLAG = product["OXRFLAG"].InnerText == null ? "" : product["OXRFLAG"].InnerText;
                                          _product.SCRAPCUTFLAG = product["SCRAPCUTFLAG"].InnerText == null ? "" : product["SCRAPCUTFLAG"].InnerText;
                                          _product.PANELSIZE = product["PANELSIZE"].InnerText == null ? "" : product["PANELSIZE"].InnerText;
                                          //_product.PANELSIZE_CROSS = product["PANELSIZE_CROSS"].InnerText == null ? "" : product["PANELSIZE_CROSS"].InnerText;
                                          _product.TRUNANGLE = product["TRUNANGLEFLAG"].InnerText == null ? "" : product["TRUNANGLEFLAG"].InnerText;
                                          _product.PANELSIZEFLAG = product["BLOCK_SIZE"].InnerText == null ? "" : product["BLOCK_SIZE"].InnerText;
                                          //_product.PANELSIZEFLAG_CROSS = product["PANELSIZEFLAG_CROSS"].InnerText == null ? "" : product["PANELSIZEFLAG_CROSS"].InnerText;
                                          //_product.REWORKCOUNT = product["REWORKCOUNT"].InnerText == null ? "" : product["REWORKCOUNT"].InnerText;
                                          //_product.ARRAYTTPEQVERSION = product["ARRAYTTPEQVERSION"].InnerText == null ? "" : product["ARRAYTTPEQVERSION"].InnerText;
                                          //_product.NODESTACK = product["NODESTACK"].InnerText == null ? "" : product["NODESTACK"].InnerText;
                                          //_product.REPAIRRESULT = product["REPAIRRESULT"].InnerText == null ? "" : product["REPAIRRESULT"].InnerText;
                                          //20151222 cy add for CUT
                                          _product.REJUDGE_COUNT = product["REJUDGE_COUNT"].InnerText == null ? "" : product["REJUDGE_COUNT"].InnerText;
                                          _product.VENDOR_NAME = product["VENDER_NAME"].InnerText == null ? "" : product["VENDER_NAME"].InnerText;
                                          _product.BUR_CHECK_COUNT = product["BUR_CHECK_COUNT"].InnerText == null ? "" : product["BUR_CHECK_COUNT"].InnerText;

                                          _lot.PRODUCTLIST.Add(_product);
                                    }
                                    cst.OFFLINE_CstData.LOTLIST.Add(_lot);
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void MESDataIntoCstObject_DPI(XmlNode bodyNode, XmlNode portNode, ref Cassette cst)
            {
                  try
                  {
                        lock (cst)
                        {
                              //cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText == null ? "" : bodyNode[keyHost.LINERECIPENAME].InnerText;
                              cst.PortID = portNode[keyHost.PORTID].InnerText == null ? "" : portNode[keyHost.PORTID].InnerText;
                              cst.PortNo = portNode[keyHost.PORTNO].InnerText == null ? "" : portNode[keyHost.PORTNO].InnerText;
                              if (cst.OFFLINE_CstData == null) cst.OFFLINE_CstData = new OFFLINE_CstBody();
                              cst.OFFLINE_CstData.LINENAME = bodyNode["LINENAME"].InnerText == null ? "" : bodyNode["LINENAME"].InnerText;
                              cst.OFFLINE_CstData.EQUIPMENTNO = bodyNode["EQUIPMENTNO"].InnerText == null ? "" : bodyNode["EQUIPMENTNO"].InnerText;
                              cst.OFFLINE_CstData.PORTNO = portNode["PORTNO"].InnerText == null ? "" : portNode["PORTNO"].InnerText;
                              cst.OFFLINE_CstData.PORTID = portNode[keyHost.PORTID].InnerText == null ? "" : portNode[keyHost.PORTID].InnerText;
                              cst.OFFLINE_CstData.CASSETTEID = portNode[keyHost.CASSETTEID].InnerText == null ? "" : portNode[keyHost.CASSETTEID].InnerText;
                              cst.OFFLINE_CstData.PRODUCTQUANTITY = portNode[keyHost.PRODUCTQUANTITY].InnerText == null ? "" : portNode[keyHost.PRODUCTQUANTITY].InnerText;
                              cst.OFFLINE_CstData.CSTSETTINGCODE = portNode["CSTSETTINGCODE"].InnerText == null ? "" : portNode["CSTSETTINGCODE"].InnerText;

                              XmlNode lot = portNode["LOTDATA"];

                              //XmlNodeList lotList = portNode[keyHost.LOTLIST].ChildNodes;
                              //foreach (XmlNode lot in lotList)
                              if (lot != null)
                              {
                                    LOTDATAc _lot = new LOTDATAc();
                                    _lot.LOTNAME = lot[keyHost.LOTNAME].InnerText == null ? "" : lot[keyHost.LOTNAME].InnerText;
                                    _lot.PROCESSOPERATIONNAME = lot[keyHost.PROCESSOPERATIONNAME].InnerText == null ? "" : lot[keyHost.PROCESSOPERATIONNAME].InnerText;
                                    _lot.PRODUCTOWNER = lot[keyHost.PRODUCTOWNER].InnerText == null ? "" : lot[keyHost.PRODUCTOWNER].InnerText;
                                    _lot.PRODUCTSPECNAME = lot[keyHost.PRODUCTSPECNAME].InnerText == null ? "" : lot[keyHost.PRODUCTSPECNAME].InnerText;
                                    _lot.BCPRODUCTTYPE = lot[keyHost.BCPRODUCTTYPE].InnerText == null ? "" : lot[keyHost.BCPRODUCTTYPE].InnerText;
                                    _lot.PRODUCTID = lot["PRODUCTID"].InnerText == null ? "" : lot["PRODUCTID"].InnerText;
                                    _lot.CSTSETTINGCODE = lot["CSTSETTINGCODE"].InnerText == null ? "" : lot["CSTSETTINGCODE"].InnerText;

                                    XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                                    foreach (XmlNode product in productList)
                                    {
                                          OFFLINEPRODUCTDATAc _product = new OFFLINEPRODUCTDATAc();
                                          _product.SLOTNO = product["SLOTNO"].InnerText == null ? "" : product["SLOTNO"].InnerText;
                                          _product.PROCESSFLAG = product[keyHost.PROCESSFLAG].InnerText == null ? "" : product[keyHost.PROCESSFLAG].InnerText;
                                          _product.PRODUCTNAME = product[keyHost.PRODUCTNAME].InnerText == null ? "" : product[keyHost.PRODUCTNAME].InnerText;
                                          _product.PRODUCTRECIPENAME = product[keyHost.PRODUCTRECIPENAME].InnerText == null ? "" : product[keyHost.PRODUCTRECIPENAME].InnerText;
                                          _product.OPI_PPID = product["OPI_PPID"].InnerText == null ? "" : product["OPI_PPID"].InnerText;
                                          _product.PRODUCTTYPE = product[keyHost.PRODUCTTYPE].InnerText == null ? "" : product[keyHost.PRODUCTTYPE].InnerText;
                                          _product.PRODUCTGRADE = product[keyHost.PRODUCTGRADE].InnerText == null ? "" : product[keyHost.PRODUCTGRADE].InnerText;
                                          _product.PRODUCTJUDGE = product[keyHost.PRODUCTJUDGE].InnerText == null ? "" : product[keyHost.PRODUCTJUDGE].InnerText;
                                          _product.GROUPID = product[keyHost.GROUPID].InnerText == null ? "" : product[keyHost.GROUPID].InnerText;
                                          _product.OXR = product["OXR"].InnerText == null ? "" : product["OXR"].InnerText;
                                          _product.ABNORMALCODE = product["ABNORMALCODE"].InnerText == null ? "" : product["ABNORMALCODE"].InnerText;
                                          _product.OWNERTYPE = product[keyHost.OWNERTYPE].InnerText == null ? "" : product[keyHost.OWNERTYPE].InnerText;
                                          _product.OWNERID = product[keyHost.OWNERID].InnerText == null ? "" : product[keyHost.OWNERID].InnerText;
                                          _product.REVPROCESSOPERATIONNAME = product[keyHost.REVPROCESSOPERATIONNAME].InnerText == null ? "" : product[keyHost.REVPROCESSOPERATIONNAME].InnerText;
                                          _product.EQPFLAG = product["EQPFLAG"].InnerText == null ? "" : product["EQPFLAG"].InnerText;
                                          _product.SUBSTRATETYPE = product["SUBSTRATETYPE"].InnerText == null ? "" : product["SUBSTRATETYPE"].InnerText;

                                          _lot.PRODUCTLIST.Add(_product);
                                    }
                                    cst.OFFLINE_CstData.LOTLIST.Add(_lot);
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private string GetArrayGlassFlowType(Line line, string ppid, string lineSpecial)
            {
                  switch (line.Data.LINETYPE)
                  {
                        case eLineType.ARRAY.PHL_EDGEEXP:
                        case eLineType.ARRAY.PHL_TITLE:
                        case eLineType.ARRAY.ELA_JSW:
                            {   // Loader(2碼) + Cleaner(4碼) + EQ1(ELA 12碼) + EQ2(ELA 12碼) + Cleaner(4碼)
                                //  OO            OOOO          OOOOOOOOOOOO    OOOOOOOOOOOO      OOOO
                                //[1,1,1,1]=[clean1, EQ1, EQ2, clean2]
                                ppid = ppid.PadRight(34, '0');
                                int cleaner1 = ppid.Substring(2, 4).Equals(new string('0', 4)) ? 0 : 1;
                                int eq1 = ppid.Substring(6, 12).Equals(new string('0', 12)) ? 0 : 2;
                                int eq2 = ppid.Substring(18, 12).Equals(new string('0', 12)) ? 0 : 4;
                                int cleaner2 = ppid.Substring(30, 4).Equals(new string('0', 4)) ? 0 : 8;
                                return (cleaner1 + eq1 + eq2 + cleaner2).ToString();
                            }
                        case eLineType.ARRAY.CVD_ULVAC:
                        case eLineType.ARRAY.CVD_AKT:
                        case eLineType.ARRAY.DRY_YAC:
                        case eLineType.ARRAY.DRY_ICD:
                        case eLineType.ARRAY.DRY_TEL:
                              {   // Loader(2碼) + Cleaner(4碼) + EQ(CVD/DRY 12碼)
                                    //  OO            OOOO           OOOOOOOOOOOO 
                                    ppid = ppid.PadRight(18, '0');
                                    string cleaner = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    string eq = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    return Convert.ToInt32(eq + cleaner, 2).ToString();
                              }
                        case eLineType.ARRAY.MSP_ULVAC:
                        case eLineType.ARRAY.ITO_ULVAC:
                              {   // Loader(2碼) + Cleaner(4碼) + EQ(PVD 12碼) + Cleaner(4碼)
                                    //  OO            OOOO           OOOOOOOOOOOO       OOOO
                                    ppid = ppid.PadRight(22, '0');
                                    string cleaner = ppid.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    string eq = ppid.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    if (ppid.Substring(18, 4).Equals(new string('0', 4)))
                                    {
                                          return Convert.ToInt32(eq + cleaner, 2).ToString();
                                    }
                                    else
                                    {
                                          if (Convert.ToInt32(eq + cleaner, 2) == 2)
                                                return "5";
                                          else if (Convert.ToInt32(eq + cleaner, 2) == 3)
                                                return "4";
                                          else
                                                return "0";
                                    }
                              }
                        case eLineType.ARRAY.RTA_VIATRON:
                              {   // Loader(2碼) + RTA(12碼) + USC(12碼)
                                    ppid = ppid.PadRight(26, '0');
                                    string rta = ppid.Substring(2, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    string usc = ppid.Substring(14, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    return Convert.ToInt32(rta + usc, 2).ToString();
                              }
                        case eLineType.ARRAY.WEI_DMS:
                        case eLineType.ARRAY.WET_DMS:
                        case eLineType.ARRAY.STR_DMS:
                        case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                        case eLineType.ARRAY.IMP_NISSIN:
                        case eLineType.ARRAY.CHN_SEEC:
                        case eLineType.ARRAY.OVNSD_VIATRON:
                              {
                                    return "1";
                              }
                        case eLineType.ARRAY.OVNITO_CSUN:
                              {
                                    return "3";
                              }
                        case eLineType.ARRAY.OVNPL_YAC:
                              {
                                    return "4";
                              }
                        case eLineType.ARRAY.BFG_SHUZTUNG:
                              {
                                    if (lineSpecial.Trim().Equals("C"))
                                          return "1";
                                    else if (lineSpecial.Trim().Equals("S"))
                                          return "2";
                                    else if (lineSpecial.Trim().Equals("CS") || lineSpecial.Trim().Equals("SC"))
                                          return "3";
                                    else
                                          return "0";
                              }
                        case eLineType.ARRAY.TTP_VTEC:
                              {
                                    if (lineSpecial.Trim().Equals("Y"))
                                          return "1";
                                    else
                                          return "2";
                              }
                        case eLineType.ARRAY.CLS_MACAOH:
                        case eLineType.ARRAY.CLS_PROCDO:
                              {   // Loader(2碼) + L3(12碼) + L4(12碼)
                                    ppid = ppid.PadRight(26, '0');
                                    string L3 = ppid.Substring(2, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    string L4 = ppid.Substring(14, 12).Equals(new string('0', 12)) ? "0" : "1";
                                    return Convert.ToInt32(L4 + L3, 2).ToString();
                              }
                        default: return "1";
                  }
            }

            private string SpecialItemInitial(string itemName, int defaultLen)
            {
                  try
                  {
                        int len = ObjectManager.SubJobDataManager.GetItemLenth(itemName);
                        if (len != 0)
                              return new string('0', len);
                        else
                              return new string('0', defaultLen);
                  }
                  catch
                  {
                        return new string('0', defaultLen);
                  }
            }

            private string FlowPriority(string _flowpriority)
            {
                  try
                  {
                        int a1 = int.Parse(_flowpriority.Substring(0, 2));
                        int a2 = int.Parse(_flowpriority.Substring(2, 2));
                        int a3 = int.Parse(_flowpriority.Substring(4, 2));
                        int Sum1 = 0;
                        int Sum2 = 0;
                        int Sum3 = 0;
                        long Sum4 = 0;

                        Sum1 = a1;
                        Sum2 = a2 << 4; //往高位元Shift 4 bits
                        Sum3 = a3 << 8; //往高位元Shift 8 bits
                        Sum4 = Sum1 + Sum2 + Sum3 + Sum4;

                        return Sum4.ToString();
                  }
                  catch
                  {
                        return "0";
                  }
            }

            //Watson Add 20150301 For CELL SPECIALL SERVERNAME != LINENAME.
            //以近似名稱取得LINE ID.
            private Line GetLineByLines(string lineApproxname)
            {
                  try
                  {
                        foreach (Line line in ObjectManager.LineManager.GetLines())
                        {
                              if (line.Data.LINEID.Contains(lineApproxname))
                                    return line;
                        }
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineApproxname));
                        return null;
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineApproxname), ex);
                        return null;
                  }
            }

            private bool CheckMixRunOtherPortFlowType(string currPortNo, string currFlowType, out string err)
            {
                  err = string.Empty;
                  try
                  {
                        return true; // modify for not need check CVD/PVD/Dry 2016/08/09 cc.kuang
                        // 尋找非本次上Port的其他Port
                        List<Port> ports = ObjectManager.PortManager.GetPorts().Where(p => p.Data.PORTNO != currPortNo
                            && (p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND
                            || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING
                            || p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)).ToList();

                        foreach (Port p in ports)
                        {
                              Job job = ObjectManager.JobManager.GetJobs(p.File.CassetteSequenceNo).FirstOrDefault();
                              if (job != null)
                              {
                                    string flowtype = string.Empty;
                                    string clnRcp = "0";
                                    string cvdRcp = "0";
                                    clnRcp = job.PPID.Substring(2, 4).Equals(new string('0', 4)) ? "0" : "1";
                                    cvdRcp = job.PPID.Substring(6, 12).Equals(new string('0', 12)) ? "0" : "1";

                                    flowtype = clnRcp + cvdRcp;
                                    if (currFlowType != flowtype)
                                    {
                                          err = string.Format("Cassette Data Transfer Error: Mix Run Mode, Flow Type is different with Port#{0}", p.Data.PORTNO);
                                          return false;
                                    }
                              }
                        }

                        return true;
                  }
                  catch (Exception ex)
                  {
                        err = ex.ToString();
                        return false;
                  }
            }

            private bool CheckGAPRecipe(XmlDocument xmlDoc, Port port, Equipment eqp, ref string errMsg)
            {
                try
                {
                    XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");
                    XmlNodeList lotNodeList = body["LOTLIST"].ChildNodes;
                    foreach (XmlNode lot in lotNodeList)
                    {
                        #region [CCGAP]
                        if (port.File.Type != ePortType.UnloadingPort)
                        {
                            if (lotNodeList.Count > 0)
                            {
                                for (int i = 0; i < lotNodeList.Count; i++)
                                {
                                    XmlNodeList productList = lot["PRODUCTLIST"].ChildNodes;
                                    XmlNode product = productList[0];

                                    string eqPPID = string.Empty;
                                    // 如果OPI的部份有值時, 使用OPI的值
                                    if (string.IsNullOrEmpty(product[keyHost.OPI_PPID].InnerText.Trim()))
                                    {
                                        if (product[keyHost.PPID] != null)
                                            eqPPID = product[keyHost.PPID].InnerText;
                                    }
                                    else
                                        eqPPID = product[keyHost.OPI_PPID].InnerText;

                                    string[] productPPID = eqPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (port.File.PortAssignment == eCELLPortAssignment.UNKNOW )
                                    {
                                        errMsg = string.Format("PortAssignment Unknow!!!");
                                        return false;
                                    }
                                    switch (eqp.File.EquipmentRunMode)
                                    {                                        
                                        case "GAPANDSORTERMODE":
                                            #region [Check GAP]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GAP)
                                            {
                                                foreach (string pPID in productPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case "GMIANDSORTERMODE":
                                            #region [Check GMI]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GMI)
                                            {
                                                foreach (string pPID in productPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case "GAPANDGMIMODE":
                                            #region [Check GAP]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GAP)
                                            {
                                                foreach (string pPID in productPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            #region [Check GMI]
                                            if (port.File.PortAssignment == eCELLPortAssignment.GMI)
                                            {
                                                foreach (string pPID in productPPID)
                                                {
                                                    if (pPID.Split(':')[0] == "L4")
                                                    {
                                                        if (pPID.Split(':')[1] == "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GMI RECIPE is 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                    if (pPID.Split(':')[0] == "L3")
                                                    {
                                                        if (pPID.Split(':')[1] != "00")
                                                        {
                                                            errMsg = string.Format("RunMode=[{0}],PortAssignment=[{1}]GAP RECIPE is not 00,EQP PortAssignment Error", eqp.File.EquipmentRunMode, port.File.PortAssignment.ToString());
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        default:
                                            errMsg = string.Format("RunMode Unknow!!!");
                                        return false;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            #endregion
      }
}
