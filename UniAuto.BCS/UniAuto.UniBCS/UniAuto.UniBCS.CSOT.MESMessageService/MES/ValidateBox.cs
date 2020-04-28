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
        enum ePROCESSTYPE
        {
            DPK = 1,
            DPS = 2,
            PPK = 3
        }

        public class eRecipeType
        {
            public const string LOT = "LOT";
            public const string PROCESSLINE = "PROCESSLINE";
            public const string STB = "STB";
        }
        /// <summary>
        /// 6.148.	ValidateBoxRequest      MES MessageSet : Box Validate Request Report to MES DenseBox ID Request,Check
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="portid">Port ID(Box position)</param>
        /// <param name="processtype"> ‘DPK’ ‘DPS’ ‘PPK’</param>
        ///<param name="portmode"> [ Boxing] [Palletizing ]</param>
        /// <param name="boxlist">Box ID List</param>
        public void ValidateBoxRequest(string trxID, Port port, IList<string> boxlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));

                    #region Get and Remove Reply Key
                    string eqpNo = string.Empty;
                    string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                    object eqp = Repository.Remove(key);
                    if (eqp == null)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_DenseBoxDataRequestReply", trxID));
                        return;
                    }
                    eqpNo = eqp.ToString();

                    #endregion
                    base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", new object[] { eqpNo, eBitResult.ON, trxID, null });
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateBoxRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                //to do 
                //if (lineName.Contains(eLineType.CELL.CBPPK))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.PPK.ToString();
                //if (lineName.Contains(eLineType.CELL.CBDPS))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.DPS.ToString();
                //if (lineName.Contains(eLineType.CELL.CBDPK))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.DPK.ToString();

                bodyNode[keyHost.BOXQUANTITY].InnerText = boxlist.Count.ToString();
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;

                bodyNode[keyHost.PORTMODE].InnerText = port.File.OperMode.ToString(); //Palletizing

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
                    boxClone[keyHost.EMPTYBOXFLAG].InnerText = port.File.Empty?"Y":"";//add for T3 by sy
                    boxListNode.AppendChild(boxClone);

                    //boxNode = boxNode.Clone();
                    //boxNode.InnerText = boxid.Trim();
                    //boxListNode.AppendChild(boxNode);
                }
                    
                if (line.Data.LINETYPE == eLineType.CELL.CBDPI)
                {
                    #region MES ValidateBoxReply Timeout
                    string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout), trxID);
                    #endregion
                }
                //else if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                //{
                //    #region MES ValidateBoxReply Timeout
                //    string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                //    if (_timerManager.IsAliveTimer(timeoutName))
                //    {
                //        _timerManager.TerminateTimer(timeoutName);
                //    }
                //    _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                //        new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout), trxID);
                //    #endregion
                //}
                else
                {
                    #region MES ValidateCassetteReply Timeout For DenseBoxDataRequest
                    string timeoutName = string.Format("{0}_MES_ValidateBoxReply_DenseBoxDataRequest", port.Data.PORTID);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout_DenseBoxDataRequest), trxID);
                    #endregion
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, line.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Dense Box Port (與正常Cassette上port流程相同)
        /// <summary>
        /// 6.148.	ValidateBoxRequest
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        public void ValidateBoxRequest(string trxID, Port port)// string lineName, string portID, string boxid)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME={1}.", trxID, port.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateBoxRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = port.Data.LINEID;
                //to do 
                //Port port = ObjectManager.PortManager.GetPort(portID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;

                //if (lineName.Contains(eLineType.CELL.CBDPK))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.DPK.ToString();
                //if (lineName.Contains(eLineType.CELL.CBDPS))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.DPS.ToString();
                //if (lineName.Contains(eLineType.CELL.CBPPK))
                //    bodyNode[keyHost.PROCESSTYPE].InnerText = ePROCESSTYPE.PPK.ToString();

                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                //bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.PORTMODE].InnerText = "";

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];
                //Modify by marine for T3 MES 2015/9/14
                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                XmlNode boxClone = box.Clone();
                boxClone[keyHost.BOXNAME].InnerText = port.File.CassetteID;
                boxClone[keyHost.EMPTYBOXFLAG].InnerText = "";//add for T3 by sy
                boxListNode.AppendChild(boxClone);

                SendToMES(xml_doc);

                #region MES ValidateBoxReply Timeout
                // 要刪掉T9 TIMEOUT, REMOTE部份需等到 Process Start 之後再下
                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout), trxID);
                #endregion


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, port.Data.LINEID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        /// <summary>
        /// ValidateBoxRequest[CCPPK]
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="boxID"></param>
        /// <param name="nodeNO"></param>
        public void ValidateBoxRequest_CCPPK(string trxID,Port port, string boxID)
        {
            try
            {
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", port.Data.NODENO));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                if (line.File.HostMode == eHostMode.OFFLINE) return;
                #region[MES Data]
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateBoxRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = port.Data.LINEID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.PORTMODE].InnerText = "PACK"; 

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];

                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                    XmlNode boxClone = box.Clone();
                    boxClone[keyHost.BOXNAME].InnerText = boxID.Trim();
                    boxClone[keyHost.EMPTYBOXFLAG].InnerText = "";//add for T3 by sy
                    boxListNode.AppendChild(boxClone);
               
                SendToMES(xml_doc);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, port.Data.LINEID));
                #endregion
                #region Add Reply Key
                string key = keyBoxReplyPLCKey.PaperBoxDataRequestReply;
                string rep = port.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region MES ValidateBoxReply Timeout
                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout), trxID);
                #endregion                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ValidateBoxRequest_CCPPK(string trxID,Equipment eqp, string boxID)
        {
            try
            {
                #region[Get EQP & LINE]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                if (line.File.HostMode == eHostMode.OFFLINE) return;
                #region[MES Data]
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ValidateBoxRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = eqp.Data.LINEID;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.PORTNAME].InnerText = "";
                bodyNode[keyHost.PORTMODE].InnerText = "PACK";

                XmlNode boxListNode = bodyNode[keyHost.BOXLIST];

                XmlNode box = boxListNode[keyHost.BOX];
                boxListNode.RemoveAll();

                XmlNode boxClone = box.Clone();
                boxClone[keyHost.BOXNAME].InnerText = boxID.Trim();
                boxClone[keyHost.EMPTYBOXFLAG].InnerText = "";//add for T3 by sy
                boxListNode.AppendChild(boxClone);

                SendToMES(xml_doc);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME={1}.",
                        trxID, eqp.Data.LINEID));
                #endregion
                #region Add Reply Key
                string key = keyBoxReplyPLCKey.PaperBoxDataRequestReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region MES ValidateBoxReply Timeout
                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", eqp.Data.NODENO);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["VALIDATETIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ValidateBoxT9Timeout), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ValidateBoxT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES][{0}]  PortID={1}  Validate Box Reply MES Timeout.", trackKey, sArray[0]);

                //Case 1
                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Port port = ObjectManager.PortManager.GetPort(sArray[0]);
                if (port == null)//PPK 才有 port == null Case
                {
                    Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", new object[] { sArray[0], eBitResult.ON, trackKey, eReturnCode1.NG });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (port != null)
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//shihyang 20150814 add
                    {
                        Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", new object[] { port.Data.NODENO, eBitResult.ON, trackKey, eReturnCode1.NG });
                    }
                    else if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                    {
                        List<string> replylist = new List<string>();
                        replylist.Add("1");
                        for (int i = 0; i < 11 - 1; i++) { replylist.Add("0"); }
                        Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", new object[] { port.Data.NODENO, eBitResult.ON, trackKey, replylist });
                    }
                    else if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)//sy 2015 12 21 
                    //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151118 add 用DB port data 管控
                    {
                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                    }
                    else
                    {
                        Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //return;

                ////Case 2
                //timeoutName = string.Format("{0}_MES_ValidateBoxReply_DenseBoxDataRequest", sArray[0]);
                //if (_timerManager.IsAliveTimer(timeoutName))
                //{
                //    _timerManager.TerminateTimer(timeoutName);

                //    #region Get and Remove Reply Key
                //    string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                //    string eqpID = Repository.Remove(key).ToString();
                //    if (eqpID == null)
                //    {
                //        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                               string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxDataRequestReply", trackKey));
                //        return;
                //    }
                //    #endregion

                //    #region MES Data Validate Box Reply
                //    List<string> replylist = new List<string>();
                //    replylist.AddRange(new string[] { eReturnCode1.NG.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty });

                //    object[] _data = new object[4]
                //    { 
                //    eqpID,   // eqp.Data.NODENO,  /*0 eqpno*/
                //    eBitResult.ON,    /*1 ON*/
                //    trackKey,                          /*2 trx id */
                //    replylist
                //    };
                //    base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                //    #endregion

                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //}


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void LotIdCreateRequestT9Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_LotIdCreateRequestReply", sArray[0]));
                    return;
                }
                #endregion
                string timeoutName = string.Format("{0}_MES_LotIdCreateRequestReply", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                Invoke(eServiceName.CELLSpecialService, "LotIDCreateRequestReportReply", new object[] { eqpID, eBitResult.ON, sArray[0], eReturnCode1.NG, "", "0" });
                
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "LotIdCreateRequestT9Timeout");
               
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //Jun Add 20150107 因為Timerout發生會把TimeoutName刪除，所以需要另外寫一個Timeout給PPK使用
        private void ValidateBoxT9Timeout_DenseBoxDataRequest(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES][{0}]  PortID={1}  Validate Box Reply MES Timeout.", trackKey, sArray[0]);

                //Case 2
                string timeoutName = string.Format("{0}_MES_ValidateBoxReply_DenseBoxDataRequest", sArray[0]);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                #region Get and Remove Reply Key
                string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                string eqpID = Repository.Remove(key).ToString();
                if (eqpID == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP ID is Null.Don't Reply PLC_DenseBoxDataRequestReply", trackKey));
                    return;
                }
                #endregion

                #region MES Data Validate Box Reply
                List<string> replylist = new List<string>();
                string boxid1 = string.Empty, boxid2 = string.Empty, producttype = "0", grade1 = string.Empty, grade2 = string.Empty,
                    cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0";
                replylist.AddRange(new string[] { eReturnCode1.NG.ToString(), sArray[0], boxid1, boxid2, producttype, grade1, grade2, cstsetcode1, cstsetcode2, boxqty1, boxqty2 });

                object[] _data = new object[4]
                { 
                eqpID,   // eqp.Data.NODENO,  /*0 eqpno*/
                eBitResult.ON,    /*1 ON*/
                trackKey,                          /*2 trx id */
                replylist
                };
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                #endregion

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.149.	ValidateBoxReply
        /// </summary>
        /// <param name="xmlDoc"></param>
        private void MES_ValidateBoxReply_DenseBoxDataRequest(XmlDocument xmlDoc)
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
                        string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
                }

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                string boxid1 = string.Empty, boxid2 = string.Empty, producttype = "0", grade1 = string.Empty, grade2 = string.Empty,
                    cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0",
                    boxcapacity1 = string.Empty, boxtype1 = string.Empty, boxcapacity2 = string.Empty, boxtype2 = string.Empty;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply NG LINENAME={1},PORTNAME={2},CODE={3},MESSAGE={4}.",
                                        trxID, lineName, portid, returnCode, returnMessage));                    
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply OK LINENAME={1},PORTNAME={2},CODE={3},MESSAGE={4}.",
                                        trxID, lineName, portid, returnCode, returnMessage));
                    //to do?
                }

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                object eqpno = Repository.Remove(key);
                if (eqpno == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_DenseBoxDataRequestReply", trxID));
                    return;
                }
                eqpNo = eqpno.ToString();
                #endregion

                //Jun Add 20150106 先判斷result OK還NG
                if (valiresult != "Y")
                {
                    if (lineName.Contains("PPK"))
                    {
                        #region MES Data Validate Box Reply
                        //PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
                        object[] _dataNG = new object[4]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*2 trx id */
                        eReturnCode1.NG
                    };
                        base.Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", _dataNG);
                        #endregion
                    }
                    else
                    {
                        #region MES Data Validate Box Reply
                        List<string> replylistNG = new List<string>();
                        replylistNG.AddRange(new string[] { "1", portid, boxid1, boxid2, producttype, grade1, grade2, cstsetcode1, cstsetcode2, boxqty1, boxqty2 });

                        object[] _dataNG = new object[4]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*2 trx id */
                        replylistNG
                    };
                        base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _dataNG);
                        #endregion
                    }
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply NG LINENAME={1},CODE={2},MESSAGE={3}.",
                                    trxID, lineName, returnCode, returnMessage));
                    return;
                }


                string linerecipename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                string setcode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
                XmlNode boxlist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                Equipment eqp  = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Port port = ObjectManager.PortManager.GetPort(portid);
                if (line != null && port != null)
                {
                    if (line.File.HostMode == eHostMode.LOCAL)
                    {
                        lock (port) port.File.Mes_ValidateBoxReply = xmlDoc.InnerXml;

                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME=[{1})] [BCS <- MES][{0}] Local Mode, Check MES Data Complete, Wait UI Edit Data.",
                            lineName, trxID));
                        if (lineName.Contains("PPK"))
                        {
                            //TO DO
                        }
                        else 
                        { 
                            Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                        }
                        return;
                    }
                }

                int i = 0; //最多會有兩個
                foreach (XmlNode box in boxlist)
                {
                    Cassette cst = new Cassette();
                    if (i == 0)
                    {
                        boxid1 = box[keyHost.BOXNAME].InnerText;
                        grade1 = box[keyHost.BOXGRADE].InnerText;
                        cstsetcode1 = setcode;
                        boxqty1 = box[keyHost.PRODUCTQUANTITY].InnerText;
                        //Add for MES T3 2015/9/8
                        boxcapacity1 = box[keyHost.BOXCAPACITY].InnerText;
                        boxtype1 = box[keyHost.BOXTYPE].InnerText;
                        cst.CassetteID = boxid1;
                        //PPK只會有一個
                        cst.Grade = grade1;
                        if (line.Data.LINETYPE == eLineType.CELL.CCPPK) cst.eBoxType = boxtype1.ToUpper() == "INBOX" ? eBoxType.InBox : eBoxType.OutBox;
                        cst.Empty = port.File.Empty;
                    }
                    else
                    {
                        boxid2 = box[keyHost.BOXNAME].InnerText;
                        grade2 = box[keyHost.BOXGRADE].InnerText;
                        cstsetcode2 = setcode;
                        boxqty2 = box[keyHost.PRODUCTQUANTITY].InnerText;
                        //Add for MES T3 2015/9/8
                        boxcapacity2 = box[keyHost.BOXCAPACITY].InnerText;
                        boxtype2 = box[keyHost.BOXTYPE].InnerText;
                        cst.CassetteID = boxid2;
                        cst.Empty = port.File.Empty;
                    }
                    cst.PortID = portid;
                    cst.PortNo = portid;
                    cst.LineRecipeName = linerecipename;
                    ObjectManager.CassetteManager.CreateBoxforPacking(cst);

                    //20170407 huangjiayin: 再取一次CST，以免FileNmae缺失无法存入File：ProductType
                    cst = ObjectManager.CassetteManager.GetCassette(cst.CassetteID);

                    XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;
                    if (productList.Count == 0 && port.File.Empty) i++;//sy add 20160629 for QPP empty
                    
                    else if (productList.Count == 0)
                    {
                        List<string> items = new List<string>();

                        Job job = new Job(1, 0);
                        if (box[keyHost.OWNERTYPE].InnerText =="") //sy add 20160629 for QPP empty
                        {
                            //Product Type

                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            job.ProductType.SetItemData(items);
                        }
                        else if (box[keyHost.OWNERTYPE].InnerText.Substring(box[keyHost.OWNERTYPE].InnerText.Length - 1, 1) != "P"
                            && box[keyHost.OWNERID].InnerText != "RESD")
                        {

                                //Product Type
                                items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                                items.Add(box[keyHost.OWNERID].InnerText);
                                items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                                items.Add(box[keyHost.GROUPID].InnerText);
                                job.ProductType.SetItemData(items);

                        }
                        else
                        {
                            //Product Type
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            job.ProductType.SetItemData(items);
                        }
                        string err;
                        IList<Job> jobs = new List<Job>();
                        if (ObjectManager.JobManager.GetProductType(eFabType.CELL, box[keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                        {
                            producttype = job.ProductType.Value.ToString();
                        }
                    }
                    else if (productList.Count > 0)
                    {
                        List<string> items = new List<string>();

                        int slotNo;
                        int.TryParse(productList[0][keyHost.POSITION].InnerText, out slotNo);
                        Job job = new Job(1, slotNo);

                        if (productList[0][keyHost.OWNERTYPE].InnerText.Substring(productList[0][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                            && productList[0][keyHost.OWNERID].InnerText != "RESD")
                        {
                            //Product Type
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(productList[0][keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            items.Add(productList[0][keyHost.GROUPID].InnerText);
                            job.ProductType.SetItemData(items);
                        }
                        else
                        {
                            //Product Type
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(productList[0][keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                            job.ProductType.SetItemData(items);
                        }

                        string err;
                        IList<Job> jobs = new List<Job>();
                        if (ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[0][keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                        {
                            producttype = job.ProductType.Value.ToString();
                        }
                        i++;
                        //sy  add 20160118
                        cst.ProductType = producttype;
                        ObjectManager.CassetteManager.EnqueueSave(cst);
                    }
                }
                XmlDocument newXmlDoc = AddOPIItemInMESXml_BOX(xmlDoc);
                Handle_HostBoxDataRequest(newXmlDoc);
                //#region MES Data Validate Box Reply
                //List<string> replylist = new List<string>();
                //replylist.AddRange(new string[] { returnCode, portid, boxid1, boxid2, producttype, grade1, grade2, cstsetcode1, cstsetcode2, boxqty1, boxqty2 });

                //object[] _data = new object[4]
                //{ 
                //    eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                //    eBitResult.ON,    /*1 ON*/
                //    trxID,                          /*2 trx id */
                //    replylist
                //};
                //base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                //#endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MES_ValidateBoxReply_CCPPK(XmlDocument xmlDoc)
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
                        string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
                }

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                string boxid1 = string.Empty, boxid2 = string.Empty, producttype = "0", grade1 = string.Empty, grade2 = string.Empty,
                    cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0",
                    boxcapacity1 = string.Empty, boxtype1 = string.Empty, boxcapacity2 = string.Empty, boxtype2 = string.Empty;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply NG LINENAME={1},PORTNAME={2},CODE={3},MESSAGE={4}.",
                                        trxID, lineName, portid, returnCode, returnMessage));
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply OK LINENAME={1},PORTNAME={2},CODE={3},MESSAGE={4}.",
                                        trxID, lineName, portid, returnCode, returnMessage));
                    //to do?
                }

                #region Get and Remove Reply Key
                //MES Reply NO Mechine Name (PLC Write Key),BC Get Repository 自行從倉庫取得
                string eqpNo = string.Empty;
                string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                object eqpno = Repository.Remove(key);
                if (eqpno == null)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                           string.Format("[BCS <- MES][{0}] BC Get Repositor NG ,EQP No is Null.Don't Reply PLC_DenseBoxDataRequestReply", trxID));
                    return;
                }
                eqpNo = eqpno.ToString();
                #endregion

                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", eqpNo);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                //先判斷result OK還NG
                if (valiresult != "Y")
                {
                    #region MES Data Validate Box Reply
                    //PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
                    object[] _dataNG = new object[4]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*2 trx id */
                        eReturnCode1.NG
                    };
                    base.Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", _dataNG);
                    #endregion

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] MES_ValidateBoxReply NG LINENAME={1},CODE={2},MESSAGE={3}.",
                                    trxID, lineName, returnCode, returnMessage));
                    return;
                }


                string linerecipename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string boxqty = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                string setcode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
                XmlNode boxlist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Port port = ObjectManager.PortManager.GetPort(portid);

                int i = 0; //最多會有兩個
                foreach (XmlNode box in boxlist)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(box[keyHost.BOXNAME].InnerText);
                    if (cst == null) cst = new Cassette();
                    if (i == 0)
                    {
                        boxid1 = box[keyHost.BOXNAME].InnerText;
                        grade1 = box[keyHost.BOXGRADE].InnerText;
                        cstsetcode1 = setcode;
                        boxqty1 = box[keyHost.PRODUCTQUANTITY].InnerText;
                        //Add for MES T3 2015/9/8
                        boxcapacity1 = box[keyHost.BOXCAPACITY].InnerText;
                        boxtype1 = box[keyHost.BOXTYPE].InnerText;
                        cst.CassetteID = boxid1;
                        //PPK只會有一個
                        cst.Grade = grade1;
                        if (line.Data.LINETYPE == eLineType.CELL.CCPPK) cst.eBoxType = boxtype1.ToUpper() == "INBOX" ? eBoxType.InBox : eBoxType.OutBox;
                    }
                    //else
                    //{
                    //    boxid2 = box[keyHost.BOXNAME].InnerText;
                    //    grade2 = box[keyHost.BOXGRADE].InnerText;
                    //    cstsetcode2 = setcode;
                    //    boxqty2 = box[keyHost.PRODUCTQUANTITY].InnerText;
                    //    //Add for MES T3 2015/9/8
                    //    boxcapacity2 = box[keyHost.BOXCAPACITY].InnerText;
                    //    boxtype2 = box[keyHost.BOXTYPE].InnerText;
                    //    cst.CassetteID = boxid2;
                    //}
                    cst.PortID = portid;
                    cst.PortNo = portid;
                    cst.LineRecipeName = linerecipename;
                    ObjectManager.CassetteManager.CreateBoxforPacking(cst);

                    //20170407 huangjiayin: 再取一次CST，以免FileNmae缺失无法存入File：ProductType
                    cst = ObjectManager.CassetteManager.GetCassette(cst.CassetteID);

                    XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;
                    if (productList.Count == 0)
                    {
                        List<string> items = new List<string>();

                        Job job = new Job(1, 0);
                        if (box[keyHost.OWNERTYPE].InnerText.Substring(box[keyHost.OWNERTYPE].InnerText.Length - 1, 1) != "P"
                            && box[keyHost.OWNERID].InnerText != "RESD")
                        {
                            //Product Type
                            //if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//huangjiayin 20170703: PPK Use ShipProductSpecName
                           // { items.Add(box[keyHost.SHIPPRODUCTSPECNAME].InnerText); }
                            //else
                           // { items.Add(box[keyHost.PRODUCTSPECNAME].InnerText); }

                           // items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.OWNERTYPE].InnerText);
                            items.Add(box[keyHost.GROUPID].InnerText);
                            items.Add(box[keyHost.BOXGRADE].InnerText);//20171226 huangjiayin add
                            items.Add(box[keyHost.PROCESSOPERATIONNAME].InnerText);//20171226 huangjiayin add
                            job.ProductType.SetItemData(items);
                        }
                        else
                        {
                            //Product Type
                            //if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//huangjiayin 20170703: PPK Use ShipProductSpecName
                           // { items.Add(box[keyHost.SHIPPRODUCTSPECNAME].InnerText); }
                           // else
                            //{ items.Add(box[keyHost.PRODUCTSPECNAME].InnerText); }
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(box[keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.OWNERTYPE].InnerText);
                            items.Add(box[keyHost.BOXGRADE].InnerText);//20171226 huangjiayin add
                            items.Add(box[keyHost.PROCESSOPERATIONNAME].InnerText);//20171226 huangjiayin add
                            job.ProductType.SetItemData(items);
                        }
                        string err;
                        IList<Job> jobs = new List<Job>();
                        if (ObjectManager.JobManager.GetProductType(eFabType.CELL, box[keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                        {
                            producttype = job.ProductType.Value.ToString();
                        }
                    }
                    if (productList.Count > 0)
                    {
                        List<string> items = new List<string>();

                        int slotNo;
                        int.TryParse(productList[0][keyHost.POSITION].InnerText, out slotNo);
                        Job job = new Job(1, slotNo);

                        if (productList[0][keyHost.OWNERTYPE].InnerText.Substring(productList[0][keyHost.OWNERTYPE].InnerText.Length - 1, 1) != "P"
                            && productList[0][keyHost.OWNERID].InnerText != "RESD")
                        {
                            //Product Type
                           // if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//huangjiayin 20170703: PPK Use ShipProductSpecName
                           // { items.Add(box[keyHost.SHIPPRODUCTSPECNAME].InnerText); }
                           // else
                           // { items.Add(box[keyHost.PRODUCTSPECNAME].InnerText); }
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(productList[0][keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.OWNERTYPE].InnerText);
                            items.Add(productList[0][keyHost.GROUPID].InnerText);
                            items.Add(box[keyHost.BOXGRADE].InnerText);//20171226 huangjiayin add
                            items.Add(box[keyHost.PROCESSOPERATIONNAME].InnerText);//20171226 huangjiayin add
                            job.ProductType.SetItemData(items);
                        }
                        else
                        {
                            //Product Type
                           // if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//huangjiayin 20170703: PPK Use ShipProductSpecName
                           // { items.Add(box[keyHost.SHIPPRODUCTSPECNAME].InnerText); }
                           // else
                           // { items.Add(box[keyHost.PRODUCTSPECNAME].InnerText); }
                            items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                            items.Add(productList[0][keyHost.OWNERID].InnerText);
                            items.Add(box[keyHost.OWNERTYPE].InnerText);
                            items.Add(box[keyHost.BOXGRADE].InnerText);//20171226 huangjiayin add
                            items.Add(box[keyHost.PROCESSOPERATIONNAME].InnerText);//20171226 huangjiayin add
                            job.ProductType.SetItemData(items);
                        }

                        string err;
                        IList<Job> jobs = new List<Job>();
                        if (ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[0][keyHost.OWNERTYPE].InnerText, jobs, job, out err))
                        {
                            producttype = job.ProductType.Value.ToString();
                        }
                        i++;
                        //sy  add 20160118
                        cst.ProductType = producttype;
                        ObjectManager.CassetteManager.EnqueueSave(cst);
                    }
                }
                XmlDocument newXmlDoc = AddOPIItemInMESXml_BOX(xmlDoc);

                BoxProcessStarted(trxID, line, boxid1);
                #region MES Data Validate Box Reply
                //PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
                object[] _dataOK = new object[4]
                    { 
                        eqpNo,   // eqp.Data.NODENO,  /*0 eqpno*/
                        eBitResult.ON,    /*1 ON*/
                        trxID,                          /*2 trx id */
                        eReturnCode1.OK
                    };

                base.Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", _dataOK);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.149.	ValidateBoxReply        MES Reply Validate Box 中的某些資料仍會填入 Cassette.MesCstBody.LOTLIST[0] ，為與AC廠相同使用，不需要再寫多種方法
        /// Job.MesCstBody and Job.MesProduct
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_ValidateBoxReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);
                #region[Check LineName]
                if (!CheckMESLineID(lineName))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
                }
                #endregion 
                string portID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;

                #region[Get LINE]
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineName));
                #endregion

                string timeoutName = string.Format("{0}_MES_ValidateBoxReply_DenseBoxDataRequest", portID);
                if (line.Data.LINETYPE == eLineType.CELL.CCPPK)//MES 流程修改 20160530
                {
                    timeoutName = string.Format("{0}_MES_ValidateBoxReply", portID);
                    if (_timerManager.GetAliveTimer(timeoutName) != null)
                        _timerManager.TerminateTimer(timeoutName);
                    MES_ValidateBoxReply_CCPPK(xmlDoc);
                    return;
                }
                if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                {
                    if (_timerManager.GetAliveTimer(timeoutName) != null)
                        _timerManager.TerminateTimer(timeoutName);
                    MES_ValidateBoxReply_DenseBoxDataRequest(xmlDoc);
                    return;
                }
                                
                if (_timerManager.GetAliveTimer(timeoutName) != null)
                {
                    _timerManager.TerminateTimer(timeoutName);
                    MES_ValidateBoxReply_DenseBoxDataRequest(xmlDoc);
                    return;
                }
                
                XmlDocument newXmlDoc = AddOPIItemInMESXml_BOX(xmlDoc);

                if (lineName.Contains(eLineType.CELL.CBDPI))
                {
                    Handle_HostBoxDataDPI(newXmlDoc);
                    return;
                }

                Handle_HostBoxData(newXmlDoc);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Handle_HostBoxData(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);

            try
            {
                string err = string.Empty;

                // 檢查 Common Data
                if (!ValidateBoxCheckData_Common(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err))
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
                bool result = ValidateBoxCheckData_CELL(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err);

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
                MESDataIntoBoxObject(body, ref cst);
                //OPI使用
                lock (cst) cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;


                //Watson Add 20150209 Save PPID TO DB 不管Remote,Local
                ValidateBoxReply_Recipe_Save2DB(xmlDoc);

                if (line.File.HostMode == eHostMode.LOCAL)
                {
                    //Watson Add 20150413 For LOCAL MODE 
                    lock (port) port.File.Mes_ValidateBoxReply = xmlDoc.InnerXml;

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
                Decode_ValidateBoxData(xmlDoc, line, eqp, port, cst);
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
                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                    //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add 
                {                    
                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                }
                else
                {
                Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
                }
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Handle_HostBoxDataRequest(XmlDocument xmlDoc)
        {
            Line line = null; Equipment eqp = null; Port port = null; Cassette cst = null;
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);

            try
            {
                string err = string.Empty;

                // 檢查 Common Data
                if (!ValidateBoxCheckData_Request(xmlDoc, ref line, ref eqp, ref port, ref cst, trxID, out err))
                {
                    if (port == null)
                        throw new Exception(err);
                    else
                    {
                        // 刪掉T9 TIMEOUT, 錯誤的資料不用再計算
                        string timeoutName = string.Format("{0}_MES_ValidateBoxReply_DenseBoxDataRequest", port.Data.PORTID);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                            _timerManager.TerminateTimer(timeoutName);
                        }
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                }
                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);
                // 先將MES資料放入Box 
                MESDataIntoBoxObject(body, ref cst);
                //OPI使用
                lock (cst) cst.Mes_ValidateCassetteReply = xmlDoc.InnerXml;


                //Watson Add 20150209 Save PPID TO DB 不管Remote,Local
                ValidateBoxReply_Recipe_Save2DB(xmlDoc);

                Decode_ValidateBoxDataRequest(xmlDoc, line, eqp, port, cst);
            }
            catch (CassetteMapException ex)
            {
                if (cst != null && string.IsNullOrEmpty(cst.ReasonCode))
                {
                    cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                    cst.ReasonText = ERR_CST_MAP.BOX_DATA_TRANSER_ERROR_ABNORMAL_EXCEPTION_ERROR;
                }
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.Message);
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


         /// <summary>
        /// 解析資料到Job Data
        /// Local Mode 時, 組完資料直接Call這個Method
        /// </summary>
        public void Decode_ValidateBoxData(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst)
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

                IList<Job> mesJobs = new List<Job>();
                Dictionary<string, int> nodeStack = new Dictionary<string, int>();

                //Jun Add 20150205 For Cell Loader Cassette Setting Code
                cst.LDCassetteSettingCode = body[keyHost.CARRIERSETCODE].InnerText.Trim();

                foreach (XmlNode n in boxNodeList)
                {
                    string ppid = string.Empty;
                    string mesppid = string.Empty;

                    #region Create Slot Data
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                        Job job = new Job(int.Parse(port.File.CassetteSequenceNo), slotNo);
                        job.EQPJobID = productList[i][keyHost.PRODUCTNAME].InnerText.Trim();
                        job.SourcePortID = port.Data.PORTID;
                        job.TargetPortID = "0";
                        job.FromCstID = port.File.CassetteID;
                        job.ToCstID = string.Empty;
                        job.FromSlotNo = slotNo.ToString();
                        job.ToSlotNo = "0";
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
                        //job.CellSpecial.NodeStack = n[keyHost.NODESTACK].InnerText.Trim();
                        job.CellSpecial.NodeStack = ObjectManager.JobManager.M2P_GetCellNodeStack(n[keyHost.NODESTACK].InnerText, nodeStack);
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
                        job.OXRInformation = string.Empty;
                        job.ChipCount = 1;

                        job.CellSpecial.BOXULDFLAG = productList[i][keyHost.BOXULDFLAG].InnerText;
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        job.CellSpecial.RunMode = line.File.CellLineOperMode;
                        //sy add 20160829
                        #region calculation product type
                        List<string> items = new List<string>();
                        List<string> idItems = new List<string>();
                        switch (line.Data.LINETYPE)
                        {
                            #region [T3]
                            case eLineType.CELL.CCPCS://4/1/Owner Type is "E"
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E")
                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                job.ProductType.SetItemData(items);
                                //Product ID: ProductOwner,PFCD, GroupID
                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E")
                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                job.ProductID.SetItemData(idItems);
                                break;
                            case eLineType.CELL.CCQUP://4/2/Owner Type is "E" & Owner ID isn't "RESD"                                        
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                job.ProductType.SetItemData(items);
                                //Product ID: ProductOwner,PFCD, GroupID
                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                if (!(string.IsNullOrEmpty(productList[i][keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                }
                                job.ProductID.SetItemData(idItems);
                                break;
                                //20170718 huangjiayin: shipproduct for pck box in
                            case eLineType.CELL.CCPCK://4/2/Owner Type is "E" & Owner ID isn't "RESD"                                        
                                //Product Type: ProductOwner,OwnerID,PFCD, GroupID
                                items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.OWNERTYPE].InnerText);
                                items.Add(keyHost.OWNERID + "_" + n[keyHost.OWNERID].InnerText);
                                items.Add(keyHost.SHIPPRODUCTSPECNAME + "_" + n[keyHost.SHIPPRODUCTSPECNAME].InnerText);
                                items.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                items.Add(keyHost.PRODUCTGRADE + "_" + n[keyHost.BOXGRADE].InnerText);
                                if (!(string.IsNullOrEmpty(n[keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (n[keyHost.OWNERTYPE].InnerText.Substring(n[keyHost.OWNERTYPE].InnerText.Length - 1, 1) != "P"
                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                        items.Add(keyHost.GROUPID + "_" + n[keyHost.GROUPID].InnerText);
                                }
                                job.ProductType.SetItemData(items);
                                //Product ID: ProductOwner,PFCD, GroupID
                                idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.OWNERTYPE].InnerText);
                                //items.Add(keyHost.OWNERID + "_" + n[keyHost.OWNERID].InnerText);
                                idItems.Add(keyHost.SHIPPRODUCTSPECNAME + "_" + n[keyHost.SHIPPRODUCTSPECNAME].InnerText);
                                idItems.Add(keyHost.PROCESSOPERATIONNAME + "_" + n[keyHost.PROCESSOPERATIONNAME].InnerText);
                                idItems.Add(keyHost.PRODUCTGRADE + "_" + n[keyHost.BOXGRADE].InnerText);
                                if (!(string.IsNullOrEmpty(n[keyHost.OWNERTYPE].InnerText.Trim())))
                                {
                                    if (n[keyHost.OWNERTYPE].InnerText.Substring(n[keyHost.OWNERTYPE].InnerText.Length - 1, 1) != "P"
                                   && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                        idItems.Add(keyHost.GROUPID + "_" + n[keyHost.GROUPID].InnerText);
                                }
                                job.ProductID.SetItemData(idItems);
                                break;
                            #endregion
                            default:
                                #region [Other]
                                {
                                    //BEOL, HVA, GAP 
                                    //Product Type: 若OwnerType最后一码是E,但Owner ID不为RESD: ProductSpecName, Owner ID, ProductOwner, Group ID
                                    //                                              其它情况: ProductSpecName, Owner ID, ProductOwner.
                                    //Product ID  : 若OwnerType最后一码是E,但Owner ID不为RESD: ProductSpecName, ProductOwner, Group ID
                                    //                                              其它情况: ProductSpecName, ProductOwner.
                                    if (productList[i][keyHost.OWNERTYPE].InnerText.Substring(productList[i][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                                        && productList[i][keyHost.OWNERID].InnerText != "RESD")
                                    {
                                        //Product Type
                                        items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                        items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                        items.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                        job.ProductType.SetItemData(items);

                                        //Product ID
                                        idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                        idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                        idItems.Add(keyHost.GROUPID + "_" + productList[i][keyHost.GROUPID].InnerText);
                                        job.ProductID.SetItemData(idItems);
                                    }
                                    else
                                    {
                                        //Product Type
                                        items.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                        items.Add(keyHost.OWNERID + "_" + productList[i][keyHost.OWNERID].InnerText);
                                        items.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                        job.ProductType.SetItemData(items);

                                        //Product ID
                                        idItems.Add(keyHost.PRODUCTSPECNAME + "_" + n[keyHost.PRODUCTSPECNAME].InnerText);
                                        idItems.Add(keyHost.PRODUCTOWNER + "_" + n[keyHost.PRODUCTOWNER].InnerText);
                                        job.ProductID.SetItemData(idItems);
                                    }
                                }
                                #endregion
                                break;
                        }
                        if (!ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[i][keyHost.OWNERTYPE].InnerText, mesJobs, job, out err))
                        {
                            string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                            if (_timerManager.IsAliveTimer(timeoutName))
                            {
                                _timerManager.TerminateTimer(timeoutName);
                            }
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                        }
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            if (!ObjectManager.JobManager.GetProductID(port, mesJobs, job, out err))
                            {
                                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", port.Data.LINEID, port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                                throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                            }
                        }
                        #endregion

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
                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELLBOX(body, productList[i], line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
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
                        #endregion

                        #region 把MES Download的PPID重新組合後記錄
                        string recipe_Assembly_log = string.Format("ADJUST PPID OK, MES PPID =[{0}], EQP PPID(JOB DATA) =[{1}] ", job.MES_PPID, job.PPID);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                        #endregion

                        #region 將MES Data 存到Job Information
                        MESBoxDataIntoJobObject(body, n, ref job);
                        MESBoxProductDataIntoJobObject(productList[i], ref job);
                        #endregion

                        ObjectManager.QtimeManager.ValidateQTimeSetting(job);

                        mesJobs.Add(job);
                    }
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
                        string log = string.Format("LINE ID=[{0}] RECIPE ID Check", key);
                        string log2 = string.Empty;

                        for (int i = 0; i < recipeIDList.Count; i++)
                        {
                            if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                            {
                                log2 += string.Format(", EQNO={0} RESULT={1}", recipeIDList[i].EQPNo, recipeIDList[i].Result.ToString());
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

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, err });
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
                                string log = string.Format("LINE ID=[{0}] RECIPE PARAMETER CHECK", key);
                                string log2 = string.Empty;

                                for (int i = 0; i < recipeParaList.Count; i++)
                                {
                                    if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                        recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                    {
                                        log2 += string.Format(", EQNO={0} RESULT={1}", recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                    }
                                }
                                if (!string.IsNullOrEmpty(log2))
                                {
                                    err = string.IsNullOrEmpty(err) ? "[MES] CHECK RECIPE PARAMETER REPLY NG: " : err += ";";
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
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, err });
                    return;
                }

                //將資料放入JobManager
                if (mesJobs.Count() > 0)
                {
                    ObjectManager.JobManager.AddJobs(mesJobs);
                    ObjectManager.JobManager.RecordJobsHistory(mesJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Create.ToString(), trxID); //20121225 Tom  Add Job History

                    //if (line.Data.LINETYPE == eLineType.CELL.CBPMT || line.Data.LINETYPE == eLineType.CELL.CBPRM
                    //    || line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                    //    ObjectManager.RobotJobManager.CreateRobotJobsByBCSJobs(eqp.Data.NODENO, mesJobs);
                }
                // Download to PLC

                //T3 CELL IO 都開CST//shihyang 20151121 add 用DB port data 管控
                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add              
                {
                    Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { eqp, port, mesJobs, trxID });
                }
                else
                {//T2
                    Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteMapDownload", new object[] { eqp, port, mesJobs });
                }

                if (port.File.Type != ePortType.UnloadingPort)
                {
                    string subPath = string.Format(@"{0}\{1}", line.Data.LINEID, port.File.CassetteSequenceNo);
                    FileFormatManager.DeleteFileByTime("CELLShopFEOL", subPath);//sy add 20160824 修改 先將超過10天生成的刪除
                    for (int i = 0; i < mesJobs.Count(); i++)
                    {
                        switch (line.Data.LINETYPE)
                        {
                            #region [T2]
                            case eLineType.CELL.CBPIL:
                            case eLineType.CELL.CBODF:
                            case eLineType.CELL.CBHVA:
                            case eLineType.CELL.CBPMT:
                            case eLineType.CELL.CBGAP:
                                FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, mesJobs[i], true);
                                break;
                            #endregion
                            default://T3 使用default 集中管理
                                Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, mesJobs[i], port });
                                break;
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
                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                    //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add              
                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { port.Data.NODENO, port.Data.PORTNO });
                else
                    Invoke(eServiceName.DenseBoxCassetteService, "DPCassetteProcessCancel", new object[] { ex.EQPNo, ex.PortNo });
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Decode_ValidateBoxDataRequest(XmlDocument xmlDoc, Line line, Equipment eqp, Port port, Cassette cst)
        {
            string returnCode = GetMESReturnCode(xmlDoc);
            string trxID = GetTransactionID(xmlDoc);
            string lineName = GetLineName(xmlDoc);
            try
            {
                string err = string.Empty, boxid1 = string.Empty, boxid2 = string.Empty, producttype = "0", grade1 = string.Empty, grade2 = string.Empty,
                     cstsetcode1 = string.Empty, cstsetcode2 = string.Empty, boxqty1 = "0", boxqty2 = "0", 
                     boxcapacity1 = string.Empty, boxtype1 = string.Empty, boxcapacity2 = string.Empty, boxtype2 = string.Empty;

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

                IList<Job> mesJobs = new List<Job>();
                Dictionary<string, int> nodeStack = new Dictionary<string, int>();

                //Jun Add 20150205 For Cell Loader Cassette Setting Code
                cst.LDCassetteSettingCode = body[keyHost.CARRIERSETCODE].InnerText.Trim();

                foreach (XmlNode n in boxNodeList)
                {
                    string ppid = string.Empty;
                    string mesppid = string.Empty;

                    #region Create Slot Data
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                    if (productList.Count == 0)
                    {
                         #region 取得Slot Recipe Data
                        if (!ObjectManager.JobManager.AnalysisMesPPID_CELLOUTBOX(body, line, port, ref idCheckInfos, ref paraCheckInfos, out ppid, out mesppid, out err))
                        {
                            lock (cst)
                            {
                                cst.ReasonCode = reasonCode;
                                cst.ReasonText = ERR_CST_MAP.INVALID_PPID;
                            }
                            throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                        }

                        #endregion
                    }
                    for (int i = 0; i < productList.Count; i++)
                    {
                        int slotNo;
                        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);
                        
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

                        #endregion

                        #region 把MES Download的PPID重新組合後記錄
                        string recipe_Assembly_log = string.Format("ADJUST PPID OK, MES PPID =[{0}], EQP PPID(JOB DATA) =[{1}] ", mesppid, ppid);
                        NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", recipe_Assembly_log);
                        #endregion

                    }
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
                        string log = string.Format("LINE ID=[{0}] RECIPE ID Check", key);
                        string log2 = string.Empty;

                        for (int i = 0; i < recipeIDList.Count; i++)
                        {
                            if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                            {
                                log2 += string.Format(", EQNO={0} RESULT={1}", recipeIDList[i].EQPNo, recipeIDList[i].Result.ToString());
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
                                string log = string.Format("LINE ID=[{0}] RECIPE PARAMETER CHECK", key);
                                string log2 = string.Empty;

                                for (int i = 0; i < recipeParaList.Count; i++)
                                {
                                    if (recipeParaList[i].Result == eRecipeCheckResult.NG ||
                                        recipeParaList[i].Result == eRecipeCheckResult.TIMEOUT)
                                    {
                                        log2 += string.Format(", EQNO={0} RESULT={1}", recipeParaList[i].EQPNo, recipeParaList[i].Result.ToString());
                                    }
                                }
                                if (!string.IsNullOrEmpty(log2))
                                {
                                    err = string.IsNullOrEmpty(err) ? "[MES] CHECK RECIPE PARAMETER REPLY NG: " : err += ";";
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
                #region [box data]
                
                XmlNode boxlist = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                string setcode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERSETCODE].InnerText;
               int j = 0; //最多會有兩個
               foreach (XmlNode box in boxlist)
               {
                   if (j == 0)
                   {
                       boxid1 = box[keyHost.BOXNAME].InnerText;
                       grade1 = box[keyHost.BOXGRADE].InnerText;
                       boxqty1 = box[keyHost.PRODUCTQUANTITY].InnerText;
                       //Add for MES T3 2015/9/8
                       boxcapacity1 = box[keyHost.BOXCAPACITY].InnerText;
                       boxtype1 = box[keyHost.BOXTYPE].InnerText;
                   }
                   else
                   {
                       boxid2 = box[keyHost.BOXNAME].InnerText;
                       grade2 = box[keyHost.BOXGRADE].InnerText;
                       boxqty2 = box[keyHost.PRODUCTQUANTITY].InnerText;
                       //Add for MES T3 2015/9/8
                       boxcapacity2 = box[keyHost.BOXCAPACITY].InnerText;
                       boxtype2 = box[keyHost.BOXTYPE].InnerText;
                   }

                   XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;


                   if (productList.Count == 0 && port.File.Empty) j++;//sy add 20160629 for QPP empty
                   
                   else if (productList.Count > 0)
                   {
                       List<string> items = new List<string>();

                       int slotNo;
                       int.TryParse(productList[0][keyHost.POSITION].InnerText, out slotNo);
                       Job job = new Job(1, slotNo);

                       if (productList[0][keyHost.OWNERTYPE].InnerText.Substring(productList[0][keyHost.OWNERTYPE].InnerText.Length - 1, 1) == "E"
                           && productList[0][keyHost.OWNERID].InnerText != "RESD")
                       {
                           //Product Type
                           items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                           items.Add(productList[0][keyHost.OWNERID].InnerText);
                           items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                           items.Add(productList[0][keyHost.GROUPID].InnerText);
                           job.ProductType.SetItemData(items);
                       }
                       else
                       {
                           //Product Type
                           items.Add(box[keyHost.PRODUCTSPECNAME].InnerText);
                           items.Add(productList[0][keyHost.OWNERID].InnerText);
                           items.Add(box[keyHost.PRODUCTOWNER].InnerText);
                           job.ProductType.SetItemData(items);
                       }

                       string producttypeerr;
                       IList<Job> jobs = new List<Job>();
                       if (ObjectManager.JobManager.GetProductType(eFabType.CELL, productList[0][keyHost.OWNERTYPE].InnerText, jobs, job, out producttypeerr))
                       {
                           producttype = job.ProductType.Value.ToString();
                       }
                       j++;
                   }
               }

                #endregion
               if (lineName.Contains(keyCellLineType.PPK))
               {
                   #region MES Data Validate Box Reply
                   //PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
                   object[] _data = new object[4]
                { 
                    port.Data.NODENO,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    eReturnCode1.OK
                };
                   base.Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", _data);
                   #endregion
               }
               else
               {
                   #region MES Data Validate Box Reply
                   List<string> replylist = new List<string>();
                   replylist.AddRange(new string[] { returnCode, int.Parse(port.Data.PORTID).ToString(), boxid1, boxid2, producttype, grade1, grade2, setcode, setcode, boxqty1, boxqty2 });

                   object[] _data = new object[4]
                { 
                    port.Data.NODENO,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    replylist
                };
                   base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                   #endregion
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

                if (lineName.Contains("PPK"))
                {
                    //cst.eBoxType = boxtype1 == "INBOX" ? eBoxType.InBox : eBoxType.OutBox;
                    #region MES Data Validate Box Reply
                    //PaperBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
                    object[] _data = new object[4]
                { 
                    port.Data.NODENO,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    eReturnCode1.NG
                };
                    base.Invoke(eServiceName.PaperBoxService, "PaperBoxLineInReportReply", _data);
                    #endregion
                }
                else
                {
                #region MES Data Validate Box Reply
                List<string> replylist = new List<string>();
                //replylist.AddRange(new string[] { returnCode, portid, boxid1, boxid2, producttype, grade1, grade2, cstsetcode1, cstsetcode2, boxqty1, boxqty2 });
                replylist.AddRange(new string[] { "1", int.Parse(port.Data.PORTID).ToString(), "", "", "0", "", "", "", "", "0", "0" });

                object[] _data1 = new object[4]
                { 
                    port.Data.NODENO,   // eqp.Data.NODENO,  /*0 eqpno*/
                    eBitResult.ON,    /*1 ON*/
                    trxID,                          /*2 trx id */
                    replylist
                };
                base.Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data1);
                return;
                #endregion
                }
            }
            catch (Exception ex)
            {
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, "MES ValidateBoxReply - NG", 
                        "Box Data Transfer Error: Abnormal Exception Error" });

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        private bool ValidateBoxCheckData_Common(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst,
            string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);
                string boxid = string.Empty;
                if (body[keyHost.BOXLIST][keyHost.BOX]!=null)
                {
                    boxid = body[keyHost.BOXLIST][keyHost.BOX][keyHost.BOXNAME] == null ? "" : body[keyHost.BOXLIST][keyHost.BOX][keyHost.BOXNAME].InnerText; //只能捉第一個BOX LIST中的box來做比對
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
                //cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                //Watson Modify 20150326 For BOX 可能不是固定的Cassette Seq NO, 改以CASSETTE ID取得
                cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());
                if (cst == null)
                {
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    return false;
                }
                #endregion

                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found Line Object, Line Name={0}.", lineName);
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = ERR_CST_MAP.INVALID_LINE_DATA;
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
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found EQP Object, Line Name={0}, Equipment No={1}.", lineName, port.Data.NODENO);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.INVALID_EQUIPMENT_DATA;
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

                if (cst.CassetteSequenceNo != port.File.CassetteSequenceNo)
                {
                    errMsg = string.Format(" Cassette Data Transfer Error: Different Cassette SeqNo. Line Name={0} Cassette SeqNo={1}, Port_Cassette SeqNo={2}.",
                        lineName, port.File.CassetteSequenceNo, portName);
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.DIFFERENT_CST_SEQNO + errMsg;
                    }
                    return false;
                }

                if (!string.IsNullOrEmpty(cst.CassetteID.Trim()))
                {
                    if (!cst.CassetteID.Trim().Equals(boxid))
                    {
                        errMsg = string.Format(" Box Data Transfer Error: Different Box ID. Line Name={0} BC_BOXID={1}, MES_BOXID={2}.",
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

                #region Check Slot Mapping NOT USE

                //XmlNodeList boxNodeList = body[keyHost.BOXLIST].ChildNodes;
                //string mesMapData = string.Empty;
                //int productCount = 0;
                //foreach (XmlNode n in boxNodeList)
                //{
                //    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                //    productCount += productList.Count;

                //    for (int i = 0; i < productList.Count; i++)
                //    {
                //        int slotNo;
                //        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);

                //        if (slotNo <= 0)
                //        {
                //            errMsg = "Box Data Transfer Error: MES POSITION(SlotNo) Parsing Error";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                //        if (!port.File.ArrayJobExistenceSlot[slotNo - 1]) //不是For Arry是指陣列
                //        {
                //            errMsg = "Box Data Transfer Error: PLC Cassette SlotMap Mismatch";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                //    }
                //}
                //// 檢查MES所有玻璃資料的數量是否跟Port上的數量相同
                //if (int.Parse(port.File.JobCountInCassette) != productCount)
                //{
                //    errMsg = "Box Data Transfer Error: PLC Report Cassette SlotMap Mismatch";
                //    lock (cst)
                //    {
                //        cst.ReasonCode = reasonCode;
                //        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //    }
                //    return false;
                //}
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

        private bool ValidateBoxCheckData_CELL(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst,
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

                //Watson Modify 20150326 目前不需要檢查BOX SLOT
                #region Check Slot Mapping
                //XmlNodeList boxNodeList = body[keyHost.BOXLIST].ChildNodes;
                //string mesMapData = string.Empty;
                //int productCount = 0;
                //foreach (XmlNode n in boxNodeList)
                //{
                //    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                //    productCount += productList.Count;

                //    for (int i = 0; i < productList.Count; i++)
                //    {
                //        int slotNo;
                //        int.TryParse(productList[i][keyHost.POSITION].InnerText, out slotNo);

                //        if (slotNo <= 0)
                //        {
                //            errMsg = "Box Data Transfer Error: MES POSITION(SlotNo=[" + slotNo +"]) Parsing Error";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                //        if (!port.File.ArrayJobExistenceSlot[slotNo - 1]) //不是For Arry是指陣列
                //        {
                //            errMsg = "PORT [" + port.Data.PORTNO + "] BOX DATA [" + port.File.CassetteID + "] TRANSFER ERROR: PLC CASSETTE SLOTMAP AND MES VALIDATE BOX DATA MISMATCH, NO GLASS IN SLOTNO[" + slotNo + "] ";
                //            lock (cst)
                //            {
                //                cst.ReasonCode = reasonCode;
                //                cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //            }
                //            return false;
                //        }
                //    }
                //}
                //// 檢查MES所有玻璃資料的數量是否跟Port上的數量相同
                //if (int.Parse(port.File.JobCountInCassette) != productCount)
                //{
                //    errMsg = string.Format("Cassette Data Transfer Error: Cassette SlotMap Mismatch, Port JobCount=[{0}], MES Download JobCount=[{1}]", port.File.JobCountInCassette, productCount);
                //    lock (cst)
                //    {
                //        cst.ReasonCode = reasonCode;
                //        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH;
                //    }
                //    return false;
                //}
                #endregion

                int productQuantity;
                int.TryParse(body[keyHost.BOXLIST][keyHost.BOX][keyHost.PRODUCTQUANTITY].InnerText, out productQuantity);

                if ((port.File.Type == ePortType.LoadingPort && productQuantity.Equals(0)) ||
                    (port.File.Type == ePortType.UnloadingPort && port.File.PartialFullFlag != eParitalFull.PartialFull && productQuantity > 0))
                {                    
                    if (port.File.Type == ePortType.LoadingPort && productQuantity.Equals(0))
                        errMsg = " Box Data Transfer Error: Loading Port product Quantity Equals 0";
                    if (port.File.Type == ePortType.UnloadingPort && port.File.PartialFullFlag != eParitalFull.PartialFull && productQuantity > 0)
                        errMsg = " Box Data Transfer Error: Unloading Port not Partial Full But Quantity > 0";
                    lock (cst)
                    {
                        cst.ReasonCode = reasonCode;
                        cst.ReasonText = ERR_CST_MAP.SLOTMAP_MISMATCH + errMsg;
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

        private bool ValidateBoxCheckData_Request(XmlDocument xmlDoc, ref Line line, ref Equipment eqp, ref Port port, ref Cassette cst,
            string trxID, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string lineName = GetLineName(xmlDoc);
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);

                XmlNode body = GetMESBodyNode(xmlDoc);
                string boxid = string.Empty;
                if (body[keyHost.BOXLIST][keyHost.BOX] != null)
                {
                    boxid = body[keyHost.BOXLIST][keyHost.BOX][keyHost.BOXNAME] == null ? "" : body[keyHost.BOXLIST][keyHost.BOX][keyHost.BOXNAME].InnerText; //只能捉第一個BOX LIST中的box來做比對
                }
                string portName = body[keyHost.PORTNAME].InnerText;

                #region Check Port Object
                port = ObjectManager.PortManager.GetPort(portName);
                if (port == null)
                {
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found Port Object, Line Name={0}, Port Name={1}.", lineName, portName);
                    return false;
                }
                port.File.CassetteID = boxid;
                #endregion
 
                #region Check BOX
                if (!port.File.PortBoxID1.Trim().Equals(string.Empty))
                    cst = ObjectManager.CassetteManager.GetCassette(port.File.PortBoxID1.Trim());
                else
                    cst = ObjectManager.CassetteManager.GetCassette(port.File.PortBoxID2.Trim());                
                if (cst == null)
                {
                    errMsg = string.Format("BOX Data Transfer Error: Cannot found Cassette Object. Line Name=[{0}], Port Name=[{1}].", lineName, portName);
                    return false;
                }
                #endregion
                
                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    errMsg = string.Format(" BOX Data Transfer Error: Cannot found Line Object, Line Name={0}.", lineName);
                    lock (cst)
                    {
                        cst.ReasonCode = MES_ReasonCode.Loader_BC_Cancel_Data_Transfer_NG;
                        cst.ReasonText = ERR_CST_MAP.INVALID_LINE_DATA + errMsg;
                    }
                    return false;
                }
                #endregion

                string reasonCode = string.Empty;
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

                //#region Check Port Box
                //if (!string.IsNullOrEmpty(cst.CassetteID.Trim()))
                //{
                //    if (!cst.CassetteID.Trim().Equals(boxid))
                //    {
                //        errMsg = string.Format("Box Data Transfer Error: Different Box ID. Line Name={0} BC_BOXID={1}, MES_BOXID={2}.",
                //            lineName, cst.CassetteID, boxid);
                //        lock (cst)
                //        {
                //            cst.ReasonCode = reasonCode;
                //            cst.ReasonText = ERR_CST_MAP.INVALID_CSTID;
                //        }
                //        return false;
                //    }
                //}
                //else
                //{
                //    lock (cst) cst.CassetteID = boxid;
                //    ObjectManager.CassetteManager.EnqueueSave(cst);
                //}
                //#endregion

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

        /// <summary>
        /// ValidateBoxReply 重要資料仍填入 Cassette.MesCstBody.LOTLIST[0] 
        /// </summary>
        /// <param name="bodyNode">ValidateBoxReply Body層</param>
        /// <param name="cst">填入Cassette Entity</param>
        private void MESDataIntoBoxObject(XmlNode bodyNode, ref Cassette cst)
        {
            try
            {
                foreach (XmlNode boxNode in bodyNode[keyHost.BOXLIST])
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
                        product.PPID = boxNode[keyHost.PPID].InnerText;//MES 將PPID 提到BOX 層
                        //product.PPID = n[keyHost.PPID].InnerText;
                        product.FMAFLAG = n[keyHost.FMAFLAG].InnerText;
                        product.MHUFLAG = n[keyHost.MHUFLAG].InnerText;
                        product.PROCESSFLAG = n[keyHost.PROCESSFLAG].InnerText;

                        //product.BOXULDFLAG = n[keyHost.BOXULDFLAG].InnerText;

                        XmlNodeList abnormal = n[keyHost.ABNORMALCODELIST].ChildNodes;
                        foreach (XmlNode a in abnormal)
                        {
                            CODEc code = new CODEc();
                            code.ABNORMALCODE = a[keyHost.ABNORMALCODE].InnerText;
                            //MES pesc ABNORMALSEQ 修改ABNORMALVALUE by sy 20151118
                            code.ABNORMALVALUE = a[keyHost.ABNORMALVALUE].InnerText;
                            product.ABNORMALCODELIST.Add(code);
                        }
                        //Lot 層沒有PPID, 改填Product List層
                        //lot.PPID = n[keyHost.PPID].InnerText;
                        lot.PPID = boxNode[keyHost.PPID].InnerText;//MES 將PPID 提到BOX 層
                        lot.PRODUCTLIST.Add(product);
                    }
                    #endregion
                    cst.MES_CstData.LOTLIST.Add(lot);
                }
                ObjectManager.CassetteManager.EnqueueSave(cst);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 將MES Cst Data塞入Job Data裡的MesCstBody
        /// </summary>
        private void MESBoxDataIntoJobObject(XmlNode bodyNode, XmlNode boxNode, ref Job job)
        {
            try
            {
                job.MesCstBody.LINENAME = bodyNode[keyHost.LINENAME].InnerText;
                job.MesCstBody.PORTNAME = bodyNode[keyHost.PORTNAME].InnerText;
                job.MesCstBody.CARRIERNAME = bodyNode[keyHost.BOXLIST][keyHost.BOX][keyHost.BOXNAME].InnerText;
                job.MesCstBody.LINERECIPENAME = bodyNode[keyHost.LINERECIPENAME].InnerText;
                //job.MesCstBody.LINEOPERMODE = bodyNode[keyHost.LINENAME].InnerText;

                //job.MesCstBody.CLEANFLAGbool = bodyNode[keyHost.LINENAME].InnerText;
                job.MesCstBody.CARRIERSETCODE = bodyNode[keyHost.CARRIERSETCODE].InnerText;
                //job.MesCstBody.AOIBYPASSbool = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.EXPSAMPLINGbool = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.AUTOCLAVESAMPLINGbool = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.AUTOCLAVESKIPbool = bodyNode[keyHost.LINENAME].InnerText;
                job.MesCstBody.RECIPEPARAVALIDATIONFLAG = bodyNode[keyHost.RECIPEPARAVALIDATIONFLAG].InnerText;
                //job.MesCstBody.RECIPEPARANOCHECKLIST = bodyNode[keyHost.LINENAME].InnerText;
                job.MesCstBody.PRODUCTQUANTITY = bodyNode[keyHost.BOXLIST][keyHost.BOX][keyHost.PRODUCTQUANTITY].InnerText;
                //job.MesCstBody.PLANNEDPRODUCTSPECNAME = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.PLANNEDSOURCEPART = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.PLANNEDPROCESSOPERATIONNAME = bodyNode[keyHost.LINENAME].InnerText;
                //job.MesCstBody.PLANNEDQUANTITYint = 0;
                //job.MesCstBody.UPKOWNERTYPE = string.Empty;
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
                //lot.BCPRODUCTTYPE = boxNode[keyHost.PRODUCTOWNER].InnerText;
                //lot.BCPRODUCTID = boxNode[keyHost.PRODUCTOWNER].InnerText;

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

        /// <summary>
        /// 將MES Product Data塞入Job Data裡的MesProduct
        /// </summary>
        private void MESBoxProductDataIntoJobObject(XmlNode productNode, ref Job job)
        {
            try
            {
                job.MesProduct.POSITION = productNode[keyHost.POSITION].InnerText;
                job.MesProduct.PRODUCTNAME = productNode[keyHost.PRODUCTNAME].InnerText;
                job.MesProduct.PRODUCTJUDGE = productNode[keyHost.PRODUCTJUDGE].InnerText;
                job.MesProduct.PRODUCTGRADE = productNode[keyHost.PRODUCTGRADE].InnerText;
                job.MesProduct.GROUPID = productNode[keyHost.GROUPID].InnerText;
                job.MesProduct.PRODUCTTYPE = productNode[keyHost.PRODUCTTYPE].InnerText;
                job.MesProduct.ITOSIDEFLAG = productNode[keyHost.DUMUSEDCOUNT].InnerText;
                job.MesProduct.OWNERTYPE = productNode[keyHost.OWNERTYPE].InnerText;
                job.MesProduct.OWNERID = productNode[keyHost.OWNERID].InnerText;
                //job.MesProduct.PPID = productNode[keyHost.PPID].InnerText; //MES 將PPID 提到BOX 層
                job.MesProduct.FMAFLAG = productNode[keyHost.FMAFLAG].InnerText;
                job.MesProduct.MHUFLAG = productNode[keyHost.MHUFLAG].InnerText;
                job.MesProduct.PROCESSFLAG = productNode[keyHost.PROCESSFLAG].InnerText;
                //job.MesProduct.BOXULDFLAG = productNode[keyHost.BOXULDFLAG].InnerText;

                XmlNodeList abnormal = productNode[keyHost.ABNORMALCODELIST].ChildNodes;
                foreach (XmlNode a in abnormal)
                {
                    CODEc code = new CODEc();
                    code.ABNORMALCODE = a[keyHost.ABNORMALCODE].InnerText;
                    //MES pesc ABNORMALSEQ 修改ABNORMALVALUE by sy 20151118
                    code.ABNORMALVALUE = a[keyHost.ABNORMALVALUE].InnerText;
                    job.MesProduct.ABNORMALCODELIST.Add(code);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private XmlDocument AddOPIItemInMESXml_BOX(XmlDocument xmlDoc)
        {
            try
            {
                XmlNode body = GetMESBodyNode(xmlDoc);
                XmlNode lineRcp = body[keyHost.LINERECIPENAME];
                if (lineRcp == null)
                    return xmlDoc;
                XmlElement element = xmlDoc.CreateElement("OPI_LINERECIPENAME");
                body.InsertAfter(element, lineRcp);

                XmlNodeList boxList = body[keyHost.BOXLIST].ChildNodes;
                if (boxList == null)
                    return xmlDoc;

                #region OPI_PRDCARRIERSETCODE, OPI_CARRIERSETCODE
                {
                    #region BODY
                    {
                        XmlElement opi_carrierSetCode = xmlDoc.CreateElement("OPI_CARRIERSETCODE");
                        body.InsertAfter(opi_carrierSetCode, body[keyHost.CARRIERSETCODE]);
                    }
                    #endregion
                    #region BODY\BOXLIST\BOX
                    {
                        foreach (XmlNode box in boxList)
                        {
                            XmlElement opi_prdCarrierSetCode = xmlDoc.CreateElement("OPI_PRDCARRIERSETCODE");
                            box.InsertAfter(opi_prdCarrierSetCode, box[keyHost.PRDCARRIERSETCODE]);
                        }
                    }
                    #endregion
                }
                #endregion

                foreach (XmlNode box in boxList)
                {

                    XmlNodeList productList = box[keyHost.PRODUCTLIST].ChildNodes;
                    if (productList == null)
                        return xmlDoc;
                    foreach (XmlNode product in productList)
                    {
                        XmlNode productRcp = product[keyHost.PRODUCTRECIPENAME];
                        XmlElement elementPorduct = xmlDoc.CreateElement("OPI_PRODUCTRECIPENAME");
                        product.InsertAfter(elementPorduct, productRcp);

                        XmlNode productPPID = product[keyHost.PPID];
                        XmlElement elementPPID = xmlDoc.CreateElement(keyHost.OPI_PPID);
                        product.InsertAfter(elementPPID, productPPID);

                        XmlNode procFlag = product[keyHost.PROCESSFLAG];
                        XmlElement elementFlag = xmlDoc.CreateElement("OPI_PROCESSFLAG");
                        product.InsertAfter(elementFlag, procFlag);
                    }
                }
                return xmlDoc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region All Line Save to Recipe DB Rule Method
        private void ValidateBoxReply_Recipe_Save2DB(XmlDocument xmlDoc)
        {
            String recipeType = eRecipeType.LOT;  //Watson Add 20150413 For 內定為LOT
            string lineName = GetLineName(xmlDoc);
            try
            {
                string err = string.Empty;
                //Watson Add 取得BC IP的方法
                string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);
                //Line
                Line line = ObjectManager.LineManager.GetLine(lineName);
                //LINERECIPENAME
                string lineRecipeName = body[keyHost.LINERECIPENAME].InnerText.Trim();

                XmlNodeList boxNodeList = body[keyHost.BOXLIST].ChildNodes;
                Dictionary<string, string> PPID_Dic = new Dictionary<string, string>();
                foreach (XmlNode n in boxNodeList)
                {
                    #region Slot PPID
                    XmlNodeList productList = n[keyHost.PRODUCTLIST].ChildNodes;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        string ppid = productList[i][keyHost.PPID].InnerText;
                        if (!PPID_Dic.ContainsKey(ppid))
                        {
                            PPID_Dic.Add(ppid, string.Empty);
                            ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid, ip, recipeType);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ValidateCassetteReply_Recipe_Save2DB(XmlDocument xmlDoc, Port port)
        {
            String recipeType = eRecipeType.LOT;  //Watson Add 20150413 For 內定為LOT
            string lineName = GetLineName(xmlDoc);
            try
            {
                string errMsg = string.Empty;
                //watson add 20150209 For Get Computer IP
                string ip = Invoke(eAgentName.OPIAgent, "GetSocketSessionID", new object[] { }) as string;

                // 獲取MES Body層資料
                XmlNode body = GetMESBodyNode(xmlDoc);
                //Line
                Line line = ObjectManager.LineManager.GetLine(lineName);
                //LINERECIPENAME
                string lineRecipeName = body[keyHost.LINERECIPENAME].InnerText.Trim();

                XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;

                string ppid = string.Empty;
                Dictionary<string, string> PPID_Dic = new Dictionary<string, string>();
                #region PPID Save To DB for 拆解規則
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CBCUT_1:
                        #region CUT1
                        foreach (XmlNode lot in lotNodeList)
                        {
                            if (port.Data.NODENO == "L2")
                            {
                                #region L2 {本Line} 上Port PPID組成LOT層 PPID [L2,L3,0000(補滿至L8)]
                                ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 2, 2, false); //L2,L3,0000(補滿至L8)
                                lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.LOT;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion

                                #region L2 {本Line} 上Port PPID組成PorcessLine層 PPID [0000,L4~L8]
                                XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                                if (processList.Count == 0)
                                {
                                    errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                                    return;
                                }
                                ppid = Fill_OPI_PPID(processList[0][keyHost.PPID].InnerText, 4, 5, false); //0000,L4~L8
                                lineRecipeName = processList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.PROCESSLINE;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion

                                #region L2 {CrossLine} 上Port PPID組成PorcessLine層 PPID [0000,L4~L8]
                                if (processList.Count > 1)
                                {
                                    ppid = Fill_OPI_PPID(processList[1][keyHost.PPID].InnerText, 4, 5, false); //0000,L4~L8
                                    lineRecipeName = processList[1][keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.PROCESSLINE;

                                    RecipeCheckandSaveForCross(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType, "CBCUT_1");
                                }
                                #endregion
                            }

                            if (port.Data.NODENO == "L4")
                            {
                                #region L4 上Port PPID組成LOT層 PPID [0000,L4~L8)]
                                ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 4, 5, false); //0000,L4~L8
                                lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.PROCESSLINE;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion
                            }

                            //if (!PPID_Dic.ContainsKey(ppid))
                            //{
                            //    PPID_Dic.Add(ppid, string.Empty);
                            //    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid, ip, recipeType);
                            //}
                        }
                        #endregion
                        break;

                    case eLineType.CELL.CBCUT_2:
                        #region CUT2
                        foreach (XmlNode lot in lotNodeList)
                        {
                            if (port.Data.NODENO == "L2")
                            {
                                #region L2 {本Line} 上Port PPID組成LOT層 PPID [L2,L3,0000(補滿至L8)]
                                ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 2, 2, false); //L2,L3,0000(補滿至L8)
                                lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.LOT;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion

                                #region L2 {本Line} 上Port PPID組成PorcessLine層 PPID [0000,L4~L8]
                                XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                                if (processList.Count == 0)
                                {
                                    errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                                    return;
                                }
                                ppid = Fill_OPI_PPID(processList[0][keyHost.PPID].InnerText, 4, 5, true); //0000,L4~L8,0000(補滿至L15)
                                lineRecipeName = processList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.PROCESSLINE;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion

                                //#region L2 {CrossLine} 上Port PPID組成LOT層 PPID [L2,L3,0000(補滿至L8)]
                                //ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 2, 2, false); //L2,L3,0000(補滿至L8)
                                //lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                //recipeType = eRecipeType.LOT;

                                //RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                //#endregion

                                #region L2 {CrossLine} 上Port PPID組成PorcessLine層 PPID [0000,L4~L9]
                                if (processList.Count > 1)
                                {
                                    ppid = Fill_OPI_PPID(processList[1][keyHost.PPID].InnerText, 4, 6, true); //0000,L4~L9,0000(補滿至L15)
                                    lineRecipeName = processList[1][keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.PROCESSLINE;

                                    RecipeCheckandSaveForCross(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType, "CBCUT_3");
                                }
                                #endregion

                                #region L2 {CrossLine} 上Port PPID組成STB層 PPID [0000(補滿至L9),L10~L15]
                                XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                                if (stbList.Count == 0)
                                {
                                    errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                                    return;
                                }
                                ppid = Fill_OPI_PPID(stbList[0][keyHost.PPID].InnerText, 10, 6, true); //0000,L10~L15
                                lineRecipeName = stbList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.STB;

                                RecipeCheckandSaveForCross(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType, "CBCUT_3");
                                #endregion
                            }

                            if (port.Data.NODENO == "L4")
                            {
                                #region L4 上Port PPID組成LOT層 PPID [0000,L4~L8)]
                                ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 4, 5, false); //0000,L4~L8
                                lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                recipeType = eRecipeType.PROCESSLINE;

                                RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                #endregion
                            }

                            //if (!PPID_Dic.ContainsKey(ppid))
                            //{
                            //    PPID_Dic.Add(ppid, string.Empty);
                            //    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid,ip,recipeType);
                            //}
                        }
                        #endregion
                        break;

                    case eLineType.CELL.CBCUT_3:
                        #region CUT3
                        foreach (XmlNode lot in lotNodeList)
                        {
                            if (string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim()))
                            {
                                if (port.Data.NODENO == "L2")
                                {
                                    #region L2 上Port PPID組成LOT層 PPID [L2,L3,0000(補滿至L9)]
                                    ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 2, 2, false); //L2,L3,0000(補滿至L8)
                                    lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText;
                                    recipeType = eRecipeType.LOT;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion

                                    #region L2 上Port PPID組成PorcessLine層 PPID [0000,L4~L9,0000(補滿至L15)]
                                    XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                                    if (processList.Count == 0)
                                    {
                                        errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                                        return;
                                    }
                                    ppid = Fill_OPI_PPID(processList[0][keyHost.PPID].InnerText, 4, 6, true); //0000,L4~L9,0000(補滿至L15)
                                    lineRecipeName = processList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.PROCESSLINE;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion

                                    #region L2 上Port PPID組成STB層 PPID [0000(補滿至L9),L10~L15]
                                    XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                                    if (stbList.Count == 0)
                                    {
                                        errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                                        return;
                                    }
                                    ppid = Fill_OPI_PPID(stbList[0][keyHost.PPID].InnerText, 10, 6, true); //0000,L10~L15
                                    lineRecipeName = stbList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.STB;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion
                                }

                                if (port.Data.NODENO == "L4")
                                {
                                    #region L4 上Port PPID組成LOT層 PPID [0000,L4~L9,0000(補滿至L15)]
                                    ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 4, 6, true); //0000,L4~L9,0000(補滿至L15)
                                    lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.PROCESSLINE;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion

                                    #region L4 上Port PPID組成STB層 PPID [0000(補滿至L9),L10~L15]
                                    XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                                    if (stbList.Count == 0)
                                    {
                                        errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                                        return;
                                    }
                                    ppid = Fill_OPI_PPID(stbList[0][keyHost.PPID].InnerText, 10, 6, true); //0000,L10~L15
                                    lineRecipeName = stbList[0][keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.STB;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion
                                }

                                if (port.Data.NODENO == "L9")
                                {
                                    #region L9 上Port PPID組成LOT層 PPID [0000(補滿至L6),L7~L9,0000(補滿至15)]
                                    ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 7, 3, true); //0000(補滿至L6),L7~L9,0000(補滿至15)
                                    lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.PROCESSLINE;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion
                                }

                                if (port.Data.NODENO == "L11" || port.Data.NODENO == "L13" || port.Data.NODENO == "L14")
                                {
                                    #region L11 上Port PPID組成LOT層 PPID [0000(補滿至L9),L10~L15]
                                    ppid = Fill_OPI_PPID(lot[keyHost.PPID].InnerText, 10, 6, true); //0000(補滿至L9),L10~L15
                                    lineRecipeName = lot[keyHost.LINERECIPENAME].InnerText.Trim();
                                    recipeType = eRecipeType.STB;

                                    RecipeCheckandSave(PPID_Dic, line, lineRecipeName, ppid, ip, recipeType);
                                    #endregion
                                }

                                //if (!PPID_Dic.ContainsKey(ppid))
                                //{
                                //    PPID_Dic.Add(ppid, string.Empty);
                                //    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid,ip,recipeType);
                                //}
                            }
                        }
                        #endregion
                        break;
                    default:
                        #region Arry ,CF CELL Normal Line
                        foreach (XmlNode lot in lotNodeList)
                        {
                            XmlNodeList productList = lot[keyHost.PRODUCTLIST].ChildNodes;
                            for (int i = 0; i < productList.Count; i++)
                            {
                                ppid = productList[i][keyHost.OPI_PPID].InnerText.Trim();
                                lineRecipeName = productList[i][keyHost.OPI_PRODUCTRECIPENAME].InnerText.Trim();

                                if (string.IsNullOrEmpty(ppid))
                                {
                                    ppid = productList[i][keyHost.PPID].InnerText.Trim();
                                    lineRecipeName = productList[i][keyHost.PRODUCTRECIPENAME].InnerText.Trim();
                                }


                                switch (line.Data.LINETYPE)
                                {
                                    //watson Modify 20150514 For Array Photo Line 跳號不補零
                                    case eLineType.ARRAY.PHL_TITLE:
                                    case eLineType.ARRAY.PHL_EDGEEXP:
                                    case eLineType.ARRAY.ITO_ULVAC:
                                    case eLineType.ARRAY.MSP_ULVAC:
                                        ppid = Fill_OPI_PPID_ITOPVD(ppid); //watson Modify 20150527
                                        break;
                                    case eLineType.CF.FCBPH_TYPE1:
                                    case eLineType.CF.FCGPH_TYPE1:
                                    case eLineType.CF.FCSPH_TYPE1:
                                    case eLineType.CF.FCOPH_TYPE1:
                                    case eLineType.CF.FCRPH_TYPE1:
                                    case eLineType.CF.FCMPH_TYPE1:
                                    case eLineType.CELL.CCPIL_2://20180313 HuangJiayin add
                                    case eLineType.CELL.CCODF_2: // 20161108 sy add 
                                    case eLineType.CELL.CCCUT_3: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_4: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_6: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_7: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_8: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_9: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_10: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_11: // 20161108 sy add
                                    case eLineType.CELL.CCCUT_12: // 20161108 sy add
                                        ppid = Fill_OPI_PPID_AC(ppid, 2, 20); //watson Modify 20150514 For Array Photo Line 跳號不補零
                                        break;
                                    case eLineType.ARRAY.CAC_MYTEK:
                                    case eLineType.ARRAY.BFG_SHUZTUNG: break;
                                    default:
                                        ppid = Fill_OPI_PPID(ppid, 2, 20, false);
                                        break;
                                }

                                if (string.IsNullOrEmpty(lineRecipeName) == false && PPID_Dic.ContainsKey(ppid) == false)
                                {
                                    PPID_Dic.Add(ppid, string.Empty);
                                    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid, ip, eRecipeType.LOT);
                                }
                            }
                        }
                        #endregion
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        //會補齊"00" 下給機台PPID
        private string Fill_OPI_PPID(string mesPPID, int startindex, int length, bool boolCrossLine)
        {
            Dictionary<string, string> tempRecipeDic = Full_MES_PPID(mesPPID);
            string subeqpPPID = string.Empty;
            int ss = 0;
            int maxnodelist = 0;

            if (boolCrossLine)
                maxnodelist = 15;
            else
                maxnodelist = ObjectManager.EquipmentManager.GetEQPs().Count + 1;  //L2 Start

            for (int i = 2; i <= maxnodelist; i++) //L2 ~
            {
                string nodeno = "L" + i.ToString();
                if (nodeno == "L" + startindex.ToString())
                {
                    //開始
                    if (tempRecipeDic.ContainsKey(nodeno))
                        subeqpPPID += tempRecipeDic[nodeno] + ";";
                    else
                        subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                    ss = 1; //start
                }
                else
                {
                    if (ss > 0) // start
                    {
                        if (ss < length)
                        {
                            //開始捉需要的
                            if (tempRecipeDic.ContainsKey(nodeno))
                                subeqpPPID += tempRecipeDic[nodeno] + ";";
                            else
                                subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                            ss++;
                            continue;
                        }
                    }
                    subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                }
            }
            if (subeqpPPID.Substring(subeqpPPID.Length - 1, 1) == ";")
            {
                return subeqpPPID.Substring(0, subeqpPPID.Length - 1);
            }
            return subeqpPPID;
        }

        //跳號不補00 Array Photo CF Photo (Watson modify 20150514 For Array Photo Line 跳號不補零)
        private string Fill_OPI_PPID_AC(string mesPPID, int startindex, int length)
        {
            Dictionary<string, string> tempRecipeDic = Full_MES_PPID(mesPPID);
            string subeqpPPID = string.Empty;
            int ss = 0;
            int maxnodelist = 0;

            maxnodelist = 20;  //L2 Start

            for (int i = 2; i <= maxnodelist; i++) //L2 ~
            {
                string nodeno = "L" + i.ToString();

                //沒有機台就不補
                if (ObjectManager.EquipmentManager.GetEQP(nodeno) == null)
                    continue;
                if (nodeno == "L" + startindex.ToString())
                {
                    //開始
                    if (tempRecipeDic.ContainsKey(nodeno))
                        subeqpPPID += tempRecipeDic[nodeno] + ";";
                    else
                        subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                    ss = 1; //start
                }
                else
                {
                    if (ss > 0) // start
                    {
                        if (ss < length)
                        {
                            //開始捉需要的
                            if (tempRecipeDic.ContainsKey(nodeno))
                                subeqpPPID += tempRecipeDic[nodeno] + ";";
                            else
                                subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                            ss++;
                            continue;
                        }
                    }
                    subeqpPPID += nodeno + ":" + new string('0', 2) + ";";
                }
            }
            if (subeqpPPID.Substring(subeqpPPID.Length - 1, 1) == ";")
            {
                return subeqpPPID.Substring(0, subeqpPPID.Length - 1);
            }
            return subeqpPPID;
        }


        //把MES PPID補齊(MES可能是沒有NodeNO，一律補上存入opi db.
        private Dictionary<string, string> Full_MES_PPID(string mesPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                string[] recipeIDs = mesPPID.Trim().Split(';');
                Dictionary<string, string> tempRecipeDic = new Dictionary<string, string>();
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {

                        NLogManager.Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES DOWNLOAD PPID IS OLD VERSION PPID FORMAT=[{0}], NEW VERSION PPID FORMAT=[{1}]", mesPPID.Trim(), "L2:AA;L3:BB;;L4:CC;L5:DD...."));
                        int j = 0;
                        foreach (string recipeid in recipeIDs)
                        {
                            tempRecipeDic.Add("L" + (j + 2).ToString(), "L" + (j + 2).ToString() + ":" + recipeid);  //L2:AA;L3:BB;L4:CC;L5:DD;L6:EE;L7:FF;L8:GG
                            j++;
                        }
                        break;
                    }
                    //add for CLN 2nd run, MES "L3" download 8码recipe--add by Yang 2016/8/12
                    if (eqreci[0] == "L3" && eqreci[1].Length == 8)
                    {
                        tempRecipeDic.Add("L3", "L3:" + eqreci[1].Substring(0, 4));
                    }

                    if (!tempRecipeDic.ContainsKey(eqreci[0]))
                        tempRecipeDic.Add(eqreci[0], recipeIDs[i]);
                    else
                    {
                        tempRecipeDic.Add("L3_2", "L3_2:" + eqreci[1].Substring(4, 4));  //For Array ITO 、PVD
                        string err = string.Format("EQUIPMETNO=[{0}], EQP_PPID=[{1}] DUPLICATION FOR CLN DOUBLE RUN", eqreci[0], recipeIDs[i]);
                        NLogManager.Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    }
                }
                return tempRecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }


        //Array Photo 非正常順序，無法補
        private string Fill_OPI_PPID_ITOPVD(string mesPPID)
        {
            Dictionary<string, string> tempRecipeDic = Full_MES_PPID(mesPPID);
            string subeqpPPID = string.Empty;
            int ss = 0;
            //補ppid len
            foreach (string nodeNo in tempRecipeDic.Keys)
            {
                subeqpPPID += tempRecipeDic[nodeNo] + ";";
            }
            if (tempRecipeDic.ContainsKey("L3_2"))
                subeqpPPID += tempRecipeDic["L3_2"] + ";";
            return subeqpPPID;
        }

        private void RecipeCheckandSave(Dictionary<string, string> PPID_Dic, Line line, string lineRecipeName, string ppid, string ip, string recipeType)
        {
            try
            {
                if (!PPID_Dic.ContainsKey(ppid))
                {
                    PPID_Dic.Add(ppid, string.Empty);
                    ObjectManager.RecipeManager.SaveRecipeObject(line, lineRecipeName, ppid, ip, recipeType);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RecipeCheckandSaveForCross(Dictionary<string, string> PPID_Dic, Line line, string lineRecipeName, string ppid, string ip, string recipeType, string saveLineType)
        {
            try
            {
                if (!PPID_Dic.ContainsKey(ppid))
                {
                    PPID_Dic.Add(ppid, string.Empty);
                    ObjectManager.RecipeManager.SaveRecipeObjectForCross(line, lineRecipeName, ppid, ip, recipeType, saveLineType);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //private string Fill_OPI_PPID_ITOMQC(string mesPPID, int startindex, int length)
        //{
        //    string[] recipeIDs = mesPPID.Trim().Split(';');

        //    int maxlen = 0;
        //    int maxidex = 0;
        //    maxlen = ObjectManager.EquipmentManager.GetEQPs().Max(e => e.Data.RECIPELEN);
        //    maxidex = ObjectManager.EquipmentManager.GetEQPs().Max(e => e.Data.RECIPEIDX);

        //    int recipemaxlen = maxidex + maxlen;
        //    string maxppidstring = new string('0', recipemaxlen);
        //    string fill_mes_ppid = string.Empty;
        //    //補ppid len
        //    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
        //    {
        //        fill_mes_ppid = fill_mes_ppid.Substring(0, eqp.Data.RECIPEIDX+3) + ;
        //    }

        //    //補local no

        //    return fill_mes_ppid;
        //}
        #endregion
    }
}