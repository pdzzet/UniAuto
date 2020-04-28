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

        public void Handle_HostBoxDataDPI(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null; Cassette cst2 = null;
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);

            try
            {
                string err = string.Empty;

                // 檢查 Common Data
                if (!ValidateBoxCheckData_CommonDPI(xmlDoc, ref line, ref eqp, ref port, ref cst, ref cst2, trxID, out err))
                {
                    if (port == null)
                        throw new Exception(err);
                    else
                    {
                        // 刪掉T9 TIMEOUT, 錯誤的資料不用再計算
                        string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }

                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    err = string.Format("Box Data Transfer Error: Box have been Cancel. Line Name={0}, Port Name={1} BOXID={2}.",
                        lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = err;
                    return;
                }

                #region 檢查資料
                bool result = ValidateBoxCheckData_DPI(xmlDoc, ref line, ref eqp, ref port, ref cst,trxID, out err);

                if (!result)
                {
                    string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                }
                #endregion

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);
                // 先將MES資料放入Box 
                XmlNodeList boxlist = body[keyHost.BOXLIST].ChildNodes;

                if (boxlist.Count >0)
                    MESDataIntoBoxObject_DPI(body, boxlist[0], ref cst);

                if (boxlist.Count > 1)
                    MESDataIntoBoxObject_DPI(body, boxlist[1], ref cst2);
                else
                {
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] BOX2 NO DATA!!.",lineName, trxID));
                }

                //OPI使用
                lock (cst) cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;

                //Watson Add 20150209 Save PPID TO DB 不管Remote,Local
                ValidateBoxReply_Recipe_Save2DB(xmlDoc);

                if (line.File.HostMode == eHostMode.LOCAL)
                {
                    //Watson Add 20150413 For LOCAL MODE 
                    lock (port) port.File.Mes_ValidateBoxReply = xmlDoc.InnerXml;
                    Port otherport = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, port.Data.PORTNO);
                    lock (otherport) otherport.File.Mes_ValidateBoxReply = xmlDoc.InnerXml;

                    // 刪掉T9 TIMEOUT, 錯誤的資料不用再計算
                    string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
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

                Decode_ValidateBoxDataDPI(xmlDoc, line, eqp, port, cst,cst2);

            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = ERR_CST_MAP.BOX_DATA_TRANSER_ERROR_ABNORMAL_EXCEPTION_ERROR;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, ex.Message });
                Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private bool ValidateBoxCheckData_CommonDPI(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst, ref Cassette cst2,
            string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNodeList boxlist = body[keyHost.BOXLIST].ChildNodes;
                string boxid = boxlist[0][keyHost.BOXNAME].InnerText; //只能捉第一個BOX LIST中的box來做比對
                string boxid2 = string.Empty;
                if (boxlist.Count >= 2)
                {
                    boxid2 = boxlist[1][keyHost.BOXNAME].InnerText; //只能捉第二個BOX LIST中的box來做比對
                }
               string portName = body[keyHost.PORTNAME].InnerText;


                #region Check Port Object
                port = ObjectManager.PortManager.GetPort(portName);
                if (port == null)
                {
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found Port Object, Line Name={0}, Port Name={1}.", lineName, portName);
                    return false;
                }
                #endregion

                #region Check BOX
                cst = ObjectManager.CassetteManager.GetCassette(boxid.Trim());
                if (cst == null)
                {
                    errMsg = string.Format("BOX1 Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    return false;
                }
                cst.CassetteControlCommand = eCstControlCmd.None;
                if (boxid2.Trim() != string.Empty) //Watson 20150605 Add 2 Box judge.
                {
                    cst2 = ObjectManager.CassetteManager.GetCassette(boxid2.Trim());
                    if (cst2 == null)
                    {
                        errMsg = string.Format("BOX2 Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                        return false;
                    }
                    cst2.CassetteControlCommand = eCstControlCmd.None;
                }
                #endregion

                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    errMsg = string.Format(" BOX Data Transfer Error: Cannot found Line Object, Line Name=[{0}].", lineName);
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = ERR_CST_MAP.INVALID_LINE_DATA + errMsg;
                    }
                    return false;
                }
                #endregion

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                #region Check Equipment Object
                eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                if (eqp == null)
                {
                    errMsg = string.Format(" BOX Data Transfer Error: Cannot found EQP Object, Line Name={0}, Equipment No={1}.", lineName, port.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_EQUIPMENT_DATA + errMsg;
                    }
                    return false;
                }

               
                #endregion

                #region Check Return Code
                if (!returnCode.Equals("0"))
                {
                    errMsg = string.Format("[LineName={0}] [BCS <- MES] [{1}] ValidateBoxReply, RETURNCODE={2}, RETURNMESSAGE={3}.",
                        lineName, trxID, returnCode, returnMessage);
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
                        string.Format("Reply Sucess. ReturnCode={0}", returnCode));
                }
                #endregion

                #region Check CIM Mode
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    errMsg = string.Format("Box Data Transfer Error: CIM Mode Off. EQPID={0}", eqp.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.CIM_MODE_OFF;
                    }
                    return false;
                }
                #endregion

                #region Check Port Box
                if (port.File.Status != ePortStatus.LC)
                {
                    errMsg = string.Format(" Box Data Transfer Error: Line Name={0} Port Name ={1} CSTID={2} Port Status={3} is Invalid.",
                        lineName, portName, port.File.CassetteID, port.File.Status.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_PORT_STATUS + errMsg;
                    }
                    return false;
                }

                //if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
                //{
                //    errMsg = string.Format("Box1 Data Transfer Error: Different Cassette SeqNo. Line Name={0} Cassette SeqNo={1}, Port_Cassette SeqNo={2}.",
                //        lineName, port.File.CassetteSequenceNo, portName);
                //    lock (cst)
                //    {
                //        cst.ReasonCode = reasonCode;
                //        cst.ReasonText = ERR_CST_MAP.DIFFERENT_CST_SEQNO;
                //    }
                //    return false;
                //}

                if (!string.IsNullOrEmpty(cst.CassetteID.Trim()))
                {
                    if (!cst.CassetteID.Trim().Equals(boxid))
                    {
                        errMsg = string.Format(" Box1 Data Transfer Error: Different Box ID. Line Name={0} BC_BOXID={1}, MES_BOXID={2}.",
                            lineName, cst.CassetteID, boxid);
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.INVALID_CSTID + errMsg;
                        }
                        return false;
                    }
                }
                else
                {
                    lock (cst) cst.CassetteID = boxid;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                if (cst2 != null) //Watson 20150605 Add 2 Box judge.
                {
                    if (!string.IsNullOrEmpty(cst2.CassetteID.Trim()))
                    {
                        if (!cst2.CassetteID.Trim().Equals(boxid2))
                        {
                            errMsg = string.Format(" Box2 Data Transfer Error: Different Box ID. Line Name={0} BC_BOXID={1}, MES_BOXID={2}.",
                                lineName, cst2.CassetteID, boxid2);
                            lock (cst)
                            {
                                cst2.ReasonCode = reasonCode;
                                cst2.ReasonText = ERR_CST_MAP.INVALID_CSTID + errMsg;
                            }
                            return false;
                        }
                    }
                    else
                    {
                        lock (cst) cst2.CassetteID = boxid2;
                        ObjectManager.CassetteManager.EnqueueSave(cst2);
                    }
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    errMsg = string.Format(" Box Data Transfer Error: Line Name={0} PORTID={1}, BOXID={2}, CSTSTATUS={3}, Cassette Status is not available.",
                        lineName, port.Data.PORTID, cst.CassetteID, port.File.CassetteStatus.ToString());
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.UNEXPECTED_MES_MESSAGE + errMsg;
                    }
                    return false;
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
                        cst.ReasonText = ERR_CST_MAP.BOX_DATA_TRANSER_ERROR_ABNORMAL_EXCEPTION_ERROR;
                    }
                }
                return false;
            }
        }

        private bool ValidateBoxCheckData_DPI(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst,
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

                //Watson Modify 20150326 目前不需要檢查BOX的SLOT
                #region Check Slot Mapping
                //XmlNodeList boxNodeList = body[keyHost.BOXLIST].ChildNodes;
                //string mesMapData = string.Empty;
                //XmlNodeList n = body[keyHost.BOXLIST].ChildNodes;
                //int portno = int.Parse(port.Data.PORTNO);
                //Cassette box = new Cassette();
                //for (int i = 0; i < n.Count; i++)
                //{
                //    box = ObjectManager.CassetteManager.GetCassette(n[i][keyHost.BOXNAME].InnerText.Trim());
                //    Port boxport = GetPortByValidateBox(box.CassetteID);
                //    int productCount = 0;
                //    XmlNodeList productList = n[i][keyHost.PRODUCTLIST].ChildNodes;
                //    productCount += productList.Count;
                //    if (boxport == null)
                //    {
                //        errMsg = string.Format("Box Data Transfer Error: Different Box ID. Line Name={0} ,MES_BOXID={1}. NO MATCH BOX DATA IN PORT",
                //            lineName, box.CassetteID);
                //        lock (cst)
                //        {
                //            cst.ReasonCode = reasonCode;
                //            cst.ReasonText = ERR_CST_MAP.INVALID_CSTID;
                //        }
                //        return false;
                //    }
                //    if (boxport.File.Type == ePortType.UnloadingPort)
                //        reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                //    else
                //        reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                //    for (int j = 0; j < productList.Count; j++)
                //    {
                //        int slotNo;
                //        int.TryParse(productList[j][keyHost.POSITION].InnerText, out slotNo);

                //        if (slotNo <= 0)
                //        {
                //            errMsg = "Port [" + box.PortNo + "] Box Data  [" + box.CassetteID + "]Transfer Error: MES POSITION(SlotNo) Parsing Error";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                        
                //        if (!boxport.File.ArrayJobExistenceSlot[slotNo - 1]) //不是For Arry是指陣列
                //        {
                //            errMsg = "PORT [" + box.PortNo + "] BOX DATA [" + box.CassetteID + "] TRANSFER ERROR: PLC CASSETTE SLOTMAP AND MES VALIDATE BOX DATA MISMATCH, NO GLASS IN SLOTNO[" + slotNo +"] ";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                //    }
                //    // 檢查MES所有玻璃資料的數量是否跟Port上的數量相同
                //    if (int.Parse(boxport.File.JobCountInCassette) != productCount)
                //    {
                //        errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}], MES Download JobCount=[{1}]", port.File.JobCountInCassette, productCount);
                //        lock (cst)
                //        {
                //            cst.ReasonCode = reasonCode;
                //            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //        }
                //        return false;
                //    }

                //    int productQuantity;
                //    int.TryParse(n[i][keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);

                //    if ((boxport.File.Type == ePortType.LoadingPort && productQuantity.Equals(0)) ||
                //        (boxport.File.Type == ePortType.UnloadingPort && boxport.File.PartialFullFlag != eParitalFull.PartialFull && productQuantity > 0))
                //    {
                //        errMsg = "Port [" + boxport.Data.PORTNO + "] Box Data [" + boxport.File.CassetteID + "] Transfer Error: Box SlotMap Mismatch";
                //        lock (cst)
                //        {
                //            cst.ReasonCode = reasonCode;
                //            cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //        }
                //        return false;
                //    }

                //}
                #endregion
                return true;

            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        private Port GetPortByValidateBox(string boxid)
        {
            foreach (Port port in ObjectManager.PortManager.GetPorts())
            {
                if (int.Parse(port.Data.PORTNO) < 3)
                {
                    if (port.File.CassetteID.Trim() == boxid)
                        return port;
                }
            }
            return null;
        }

        /// <summary>
        /// ValidateBoxReply 重要資料仍填入 Cassette.MesCstBody.LOTLIST[0] 
        /// </summary>
        /// <param name="bodyNode">ValidateBoxReply Body層</param>
        /// <param name="cst">填入Cassette Entity</param>
        private void MESDataIntoBoxObject_DPI(XmlNode bodyNode, XmlNode boxNode,ref Cassette cst)
        {
            try
            {
                    cst.MES_CstData.LINENAME = bodyNode[keyHost.LINENAME].InnerText;
                    cst.MES_CstData.PORTNAME = bodyNode[keyHost.PORTNAME].InnerText;
                    cst.MES_CstData.CARRIERNAME = boxNode[keyHost.BOXNAME].InnerText;
                    cst.MES_CstData.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;
                    cst.MES_CstData.CARRIERSETCODE = bodyNode[keyHost.CARRIERSETCODE].InnerText;
                    cst.MES_CstData.RECIPEPARAVALIDATIONFLAG = bodyNode[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText;
                    cst.MES_CstData.PRODUCTQUANTITY = boxNode[keyHost.PRODUCTQUANTITY].InnerText;
                    cst.LineRecipeName = bodyNode[keyHost.LINERECIPENAME].InnerText;

                    XmlNodeList recipeNoCheckList = bodyNode[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                    for (int i = 0; i < recipeNoCheckList.Count; i++)
                    {
                        cst.MES_CstData.RECIPEPARANOCHECKLIST.Add(recipeNoCheckList[i].InnerText.Trim());
                    }
                    XmlNodeList boxList = bodyNode[keyHost.BOXLIST].ChildNodes;

                    LOTc lot = new LOTc();
                    lot.LOTNAME = boxNode[keyHost.BOXNAME].InnerText;
                    lot.PRODUCTSPECNAME = boxNode[keyHost.PRODUCTSPECNAME].InnerText;

                    lot.PROCESSOPERATIONNAME = boxNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                    lot.PRODUCTOWNER = boxNode[keyHost.PRODUCTOWNER].InnerText;
                    lot.PRDCARRIERSETCODE = boxNode[keyHost.PRDCARRIERSETCODE].InnerText;

                    lot.PRODUCTSIZETYPE = boxNode[keyHost.PRODUCTSIZETYPE].InnerText;
                    lot.PRODUCTSIZE = boxNode[keyHost.PRODUCTSIZE].InnerText;
                    lot.BCPRODUCTTYPE = boxNode[keyHost.PRODUCTOWNER].InnerText;
                    lot.BCPRODUCTID = boxNode[keyHost.PRODUCTOWNER].InnerText;

                    lot.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;

                    lot.NODESTACK = boxNode[keyHost.NODESTACK].InnerText;
                    lot.PRODUCTSPECGROUP = boxNode[keyHost.PRODUCTSPECGROUP].InnerText;
                    lot.PRODUCTGCPTYPE = boxNode[keyHost.PRODUCTGCPTYPE].InnerText;

                    lot.ORIENTEDSITE = boxNode[keyHost.ORIENTEDSITE].InnerText;
                    lot.ORIENTEDFACTORYNAME = boxNode[keyHost.ORIENTEDFACTORYNAME].InnerText;
                    lot.CURRENTSITE = boxNode[keyHost.CURRENTSITE].InnerText;
                    lot.CURRENTFACTORYNAME = boxNode[keyHost.CURRENTFACTORYNAME].InnerText;

                    #region 在Product Data 加入 Product List
                    XmlNodeList productList = boxNode[keyHost.PRODUCTLIST].ChildNodes;
                    foreach (XmlNode n in productList)
                    {
                        PRODUCTc product = new PRODUCTc();
                        product.POSITION = n[keyHost.POSITION].InnerText;
                        product.PRODUCTNAME = n[keyHost.PRODUCTNAME].InnerText;
                        product.PRODUCTJUDGE = n[keyHost.PRODUCTJUDGE].InnerText;
                        product.PRODUCTGRADE = n[keyHost.PRODUCTGRADE].InnerText;
                        product.GROUPID = n[keyHost.GROUPID].InnerText;
                        product.PRODUCTTYPE = n[keyHost.PRODUCTTYPE].InnerText;
                        product.ITOSIDEFLAG = n[keyHost.DUMUSEDCOUNT].InnerText;
                        product.OWNERTYPE = n[keyHost.OWNERTYPE].InnerText;
                        product.OWNERID = n[keyHost.OWNERID].InnerText;
                        product.PPID = n[keyHost.PPID].InnerText;
                        product.FMAFLAG = n[keyHost.FMAFLAG].InnerText;
                        product.MHUFLAG = n[keyHost.MHUFLAG].InnerText;
                        product.PROCESSFLAG = n[keyHost.PROCESSFLAG].InnerText;

                        //product.BOXULDFLAG = n[keyHost.BOXULDFLAG].InnerText;

                        XmlNodeList abnormal = n[keyHost.ABNORMALCODELIST].ChildNodes;
                        foreach (XmlNode a in abnormal)
                        {
                            CODEc code = new CODEc();
                            code.ABNORMALCODE = a[keyHost.ABNORMALCODE].InnerText;
                            code.ABNORMALSEQ = a[keyHost.ABNORMALSEQ].InnerText;
                            product.ABNORMALCODELIST.Add(code);
                        }
                        //Lot 層沒有PPID, 改填Product List層
                        lot.PPID = n[keyHost.PPID].InnerText;

                        lot.PRODUCTLIST.Add(product);
                    }
                    #endregion
                cst.MES_CstData.LOTLIST.Add(lot);
                ObjectManager.CassetteManager.EnqueueSave(cst);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        private void MESBoxDataIntoJobObjectDPI(XmlNode bodyNode, XmlNode boxNode, ref Job job)
        {
            try
            {
                job.MesCstBody.LINENAME = bodyNode[keyHost.LINENAME].InnerText;
                job.MesCstBody.PORTNAME = bodyNode[keyHost.PORTNAME].InnerText;
                job.MesCstBody.CARRIERNAME = boxNode[keyHost.BOXNAME].InnerText;
                job.MesCstBody.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;

                job.MesCstBody.CARRIERSETCODE = bodyNode[keyHost.CARRIERSETCODE].InnerText;

                job.MesCstBody.RECIPEPARAVALIDATIONFLAG = bodyNode[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText;

                job.MesCstBody.PRODUCTQUANTITY = bodyNode[keyHost.BOXLIST][keyHost.BOX][keyHost.PRODUCTQUANTITY].InnerText;
                int panelcount = 0;
                if (int.TryParse(job.MesCstBody.PRODUCTQUANTITY, out panelcount))
                {
                    job.MesCstBody.SELECTEDPOSITIONMAP = new string('1', panelcount).PadRight(96, '0');
                }

                LOTc lot = new LOTc();
                lot.LOTNAME = boxNode[keyHost.BOXNAME].InnerText;
                lot.PRODUCTSPECNAME = boxNode[keyHost.PRODUCTSPECNAME].InnerText;

                lot.PROCESSOPERATIONNAME = boxNode[keyHost.PROCESSOPERATIONNAME].InnerText;
                lot.PRODUCTOWNER = boxNode[keyHost.PRODUCTOWNER].InnerText;
                lot.PRDCARRIERSETCODE = boxNode[keyHost.PRDCARRIERSETCODE].InnerText;

                lot.PRODUCTSIZETYPE = boxNode[keyHost.PRODUCTSIZETYPE].InnerText;
                lot.PRODUCTSIZE = boxNode[keyHost.PRODUCTSIZE].InnerText;
  
                lot.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;

                lot.NODESTACK = boxNode[keyHost.NODESTACK].InnerText;
                lot.PRODUCTSPECGROUP = boxNode[keyHost.PRODUCTSPECGROUP].InnerText;
                lot.PRODUCTGCPTYPE = boxNode[keyHost.PRODUCTGCPTYPE].InnerText;

                lot.ORIENTEDSITE = boxNode[keyHost.ORIENTEDSITE].InnerText;
                lot.ORIENTEDFACTORYNAME = boxNode[keyHost.ORIENTEDFACTORYNAME].InnerText;
                lot.CURRENTSITE = boxNode[keyHost.CURRENTSITE].InnerText;
                lot.CURRENTFACTORYNAME = boxNode[keyHost.CURRENTFACTORYNAME].InnerText;

                job.MesCstBody.LOTLIST.Add(lot);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Decode_ValidateBoxDataDPI(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst,Cassette cst2)
        {
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);
            try
            {
                string err = string.Empty;

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);

                string reasonCode = string.Empty;
                if (port.File.Type == ePortType.UnloadingPort)
                    reasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                else
                    reasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;

                XmlNodeList boxNodeList = body[keyHost.BOXLIST].ChildNodes;
                List<string> lstNoCehckEQ = new List<string>();
                // Array Use : 計算Recipe Group Number
                Dictionary<string, string> recipeGroup = new Dictionary<string, string>();

                // 從MES Data 取出 Recipe Parameter 是否Check部份
                bool recipePara = body[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText.Equals("Y");

                IDictionary<string, IList<RecipeCheckInfo>> recipeIDCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IDictionary<string, IList<RecipeCheckInfo>> recipeParaCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
                IList<RecipeCheckInfo> paraCheckInfos = new List<RecipeCheckInfo>();

                Dictionary<int, IList<Job>> mesboxJobs = new Dictionary<int, IList<Job>>();
                int boxn = 0;
                foreach (XmlNode n in boxNodeList)
                {
                    //portno = int.Parse(port.Data.PORTNO) + boxn;
                    //Port boxport = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portno.ToString().PadLeft(2, '0'));
                    string ppid = string.Empty;
                    string mesppid = string.Empty;
                    IList<Job> mesJobs = new List<Job>();
                    #region Create Slot Data
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;                    
                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                        Job job;
                        if (boxn == 0)
                        {
                            job = new Job(int.Parse(cst.CassetteSequenceNo), slotNo);
                            job.FromCstID = cst.CassetteID;
                            job.SourcePortID = cst.PortID;
                        }
                        else
                        {
                            if (cst2 != null) //Watson 20150605 Add 2 Box judge.
                            {
                                job = new Job(int.Parse(cst2.CassetteSequenceNo), slotNo);
                                job.FromCstID = cst2.CassetteID;
                                job.SourcePortID = cst2.PortID;
                            }
                            else
                                continue;
                        }

                        job.EQPJobID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();
                        
                        job.TargetPortID = "0";

                        job.ToCstID = string.Empty;
                        job.FromSlotNo = slotNo.ToString();
                        job.ToSlotNo = "0";
                        job.CurrentSlotNo = job.FromSlotNo; //add by bruce 20160412 for T2 Issue
                        job.CurrentEQPNo = eqp.Data.NODENO;

                        //job.LineRecipeName = body[keyHost.LINERECIPENAME].InnerText.Trim();
                        // Watson Modify 20150420 For LOCAL MODE 有OPI的部份使用OPI的
                        if (string.IsNullOrEmpty(body["OPI_LINERECIPENAME"].InnerText.Trim()))
                            job.LineRecipeName = body[keyHost.LINERECIPENAME].InnerText.Trim();
                        else
                            job.LineRecipeName = body["OPI_LINERECIPENAME"].InnerText.Trim();
                        
                        job.GroupIndex = ObjectManager.JobManager.M2P_GetCellGroupIndex(line, productList[i][keyHost.GROUPID].InnerText);

                        job.SubstrateType = ObjectManager.JobManager.M2P_GetSubstrateType(n[keyHost.PRODUCTGCPTYPE].InnerText);
                        job.CIMMode = eBitResult.ON;
                        job.JobType = ObjectManager.JobManager.M2P_GetJobType(productList[i][keyHost.PRODUCTTYPE].InnerText);
                        job.JobJudge = "0"; // 胡福杰說不管MES Download 什麼, 預設都給0
                        job.SamplingSlotFlag = productList[i][keyHost.PROCESSFLAG].InnerText.Equals("Y") ? "1" : "0";
                        job.FirstRunFlag = "0";
                        job.JobGrade = productList[i][keyHost.PRODUCTGRADE].InnerText.Trim();
                        job.GlassChipMaskBlockID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();
                        job.CSTOperationMode = eqp.File.CSTOperationMode; //watson add 20150130 For 補充
                        //要確認
                        job.CellSpecial.CassetteSettingCode = string.IsNullOrEmpty(n[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim()) ? n[keyHost.PRDCARRIERSETCODE].InnerText.Trim() : n[keyHost.OPI_PRDCARRIERSETCODE].InnerText.Trim();
                        job.CellSpecial.NodeStack = n[keyHost.NODESTACK].InnerText.Trim();
                        #region ProductSize
                        double pnlsize = 0;
                        if (double.TryParse(n[keyHost.PRODUCTSIZE].InnerText.Trim(), out pnlsize))
                            pnlsize = pnlsize * 100;
                        job.CellSpecial.PanelSize = pnlsize.ToString();
                        #endregion
                        //job.ProductType = n[keyHost.BCPRODUCTTYPE].InnerText;
                        //job.CellSpecial.ProductID = n[keyHost.BCPRODUCTID].InnerText;

                        job.CellSpecial.ProductID = "1";
                        job.ProductType.Value = 1;

                        //HOT Code BOX不會有此資訊
                        job.OXRInformationRequestFlag = "0";
                        job.ChipCount = 1;

                        //job.CellSpecial.BOXULDFLAG = productList[i][keyHost.BOXULDFLAG].InnerText;
                        job.CellSpecial.ControlMode = line.File.HostMode;


                        #region EQP FLAG:
                        IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
                        //Jun Modify 20141230 判斷Sub Job Data 是否為Null
                        if (sub != null)
                        {
                            if (sub.ContainsKey("PanelSizeFlag"))
                                sub["PanelSizeFlag"] = CellPanelSizeType(n[keyHost.PRODUCTSIZETYPE].InnerText.Trim());

                            if (sub.ContainsKey("SortFlag"))
                                sub["SortFlag"] = productList[i][keyHost.BOXULDFLAG].InnerText.Trim() == "Y" ? "1" : "0";

                            job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        }
                        #endregion

                        #region 取得Slot Recipe Data
                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELLBOX(body,productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                        {
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                            }
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                        }
                        job.PPID = ppid;
                        job.MES_PPID = mesppid;
                        cst.PPID = mesppid;
                        if (cst2 != null) //Watson 20150605 Add 2 Box judge.
                            cst2.PPID = mesppid;
                        #endregion

                        #region 把MES Download的PPID重新組合後記錄
                        string recipe_Assembly_log = string.Format("Adjust PPID OK, MES PPID =[{0}], EQP PPID(Job Data) =[{1}] ", job.MES_PPID, job.PPID);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                        #endregion

                        #region 將MES Data 存到Job Information
                        MESBoxDataIntoJobObjectDPI(body, n, ref job);

                        MESBoxProductDataIntoJobObject(productList[i], ref job);
                        job.MES_PPID = body[keyHost.BOXLIST][keyHost.BOX][keyHost.PPID].InnerText;//MES 將PPID 提到BOX 層
                        #endregion

                        mesJobs.Add(job);
                    }
                    mesboxJobs.Add(boxn+1, mesJobs);
                    boxn++;
                    #endregion
                }

                #region Check Recipe ID
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
                        string log = string.Format("Line Name[{0}] Recipe ID Check", key);
                        string log2 = string.Empty;

                        for (int i = 0; i < recipeIDList.Count; i++)
                        {
                            if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                            {
                                log2 += string.Format(", EQNo={0} Result={1}", recipeIDList[i].EQPNo, recipeIDList[i].Result.ToString());
                                _NGResult = true;
                            }
                        }
                        if (!string.IsNullOrEmpty(log2))
                        {
                            err = string.IsNullOrEmpty(err) ? "" : err += ";";
                            err += log + log2;
                        }
                    }
                    if (_NGResult)
                    {
                        lock (cst)
                        {
                            cst.ReasonCode = reasonCode;
                            cst.ReasonText = ERR_CST_MAP.RECIPEID_VALIDATION_NG;
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }
                #endregion

                //Recipe Check 的時間可能大於Timeout時間, 要檢查BOX是否已經被下過Cancel了
                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    err = string.Format("Box Data Transfer Error: Box have been Cancel. Line Name={0}, Port Name={1} BoxID={2}.",
                        lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = err;
                    return;
                }

                #region Check Parameter
                if (recipePara)
                {
                    XmlNodeList recipeNoCheckList = body[keyHost.RECIPEPARANOCHECKLIST].ChildNodes;
                    for (int i = 0; i < recipeNoCheckList.Count; i++)
                    {
                        lstNoCehckEQ.Add(recipeNoCheckList[i].InnerText.Trim());
                    }

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
                            port.Data.PORTID,
                            port.File.CassetteID,
                        };
                        eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "RecipeParameterRequestCommand", obj);

                        //檢查完畢, 返回值
                        if (retCode != eRecipeCheckResult.OK)
                        {
                            foreach (string key in recipeParaCheckData.Keys)
                            {
                                IList<RecipeCheckInfo> recipeParaList = recipeIDCheckData[key];
                                string log = string.Format("Line Name[{0}] Recipe Parameter Check", key);
                                string log2 = string.Empty;

                                for (int i = 0; i < recipeParaList.Count; i++)
                                {
                                    if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                        recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                    {
                                        log2 += string.Format(", EQNo={0} Result={1}", recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                    }
                                }
                                if (!string.IsNullOrEmpty(log2))
                                {
                                    err = string.IsNullOrEmpty(err) ? "[MES] Check Recipe Parameter Reply NG: " : err += ";";
                                    err += log + log2;
                                }
                            }
                            if (string.IsNullOrEmpty(cst.ReasonCode))
                            {
                                lock (cst)
                                {
                                    cst.ReasonCode = reasonCode;
                                    cst.ReasonText = ERR_CST_MAP.RECIPE_PARAMATER_VALIDATION_NG;
                                }
                            }
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                        }
                    }
                }
                #endregion
                //Recipe Check 的時間可能大於Timeout時間, 要檢查BOX是否已經被下過Cancel了
                if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                {
                    err = string.Format("Box Data Transfer Error: Box have been Cancel. Line Name={0}, Port Name={1} BoxID={2}.",
                        lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = err;
                    return;
                }

                if (!mesboxJobs.ContainsKey(1))
                {
                    err = string.Format("Box Data Transfer Error: Box have been Cancel. Line Name={0}, Port Name={1} BoxID={2}.",
                      lineName, port.Data.PORTID, port.File.CassetteID);
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    cst.ReasonCode = MES_ReasonCode.Unloader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = err;
                    return;
                }


                //將資料放入JobManager
                if (mesboxJobs[1].Count() > 0) 
                {
                    ObjectManager.JobManager.AddJobs(mesboxJobs[1]);
                    ObjectManager.JobManager.RecordJobsHistory(mesboxJobs[1], eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20121225 Tom  Add Job History
                }

                Port otherport = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, port.Data.PORTNO);

                if (mesboxJobs.Count() > 1) //Watson 20150605 Add 2 Box judge.
                {
                    ObjectManager.JobManager.AddJobs(mesboxJobs[2]);
                    ObjectManager.JobManager.RecordJobsHistory(mesboxJobs[2], eqp.Data.NODEID, eqp.Data.NODENO, otherport.Data.PORTNO, eJobEvent.Create.ToString(), trxID); 
                }
                // Download to PLC
                Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownloadByDPI", new object[] { eqp, port, mesboxJobs[1] });

                if (mesboxJobs.Count > 1) //Watson 20150605 Add 2 Box judge.
                {             
                    Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownloadByDPI", new object[] { eqp, otherport, mesboxJobs[2] });
                }

                if (port.File.Type != ePortType.UnloadingPort)
                {
                    string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                    for (int i = 0; i < mesboxJobs[1].Count(); i++)
                    {
                          FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesboxJobs[1][i], true);
                    }
                    if (mesboxJobs.Count > 1)   //Watson 20150605 Add 2 Box judge.
                    {
                        for (int i = 0; i < mesboxJobs[2].Count(); i++)
                        {
                            FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, mesboxJobs[2][i], true);
                        }
                    }
                }
            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = ERR_CST_MAP.BOX_DATA_TRANSER_ERROR_ABNORMAL_EXCEPTION_ERROR;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, ex.Message });
                Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
