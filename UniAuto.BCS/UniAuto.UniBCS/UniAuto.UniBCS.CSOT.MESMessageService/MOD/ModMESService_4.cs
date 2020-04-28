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
    public partial class ModMESService : AbstractService
    {
        //MaterialConsumableReport
        //1.MATERIALLIST內容是否正确?
        //2.CONSUMEDQTY取值是否正确?
        public void MaterialConsumableReport(string trxID, string lineName, Equipment eqp, string productName, string carrierName, IList<MaterialConsumableRequest.MATERIALc> materialData )
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaterialConsumableReport") as XmlDocument;
                
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.CARRIERNAME].InnerText = carrierName;
                
                XmlNode materialList = xml_doc[keyHost.MATERIALLIST];

                foreach (MaterialConsumableRequest.MATERIALc materialInfo in materialData)
                {
                    XmlNode material = xml_doc[keyHost.MATERIAL];
                    material[keyHost.MATERIALTYPE].InnerText = materialInfo.MATERIALTYPE;
                    material[keyHost.MATERIALNAME].InnerText = materialInfo.MATERIALNAME;
                    material[keyHost.CONSUMEDQTY].InnerText = materialInfo.QUANTITY;
                }

                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] "+ MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PanelInformationRequest
        public void PanelInformationRequest(string trxID, string lineName, Equipment eqp, string productName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelInformationRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                
                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                #region MES Panel Information Reply Timeout
                string timeoutName = string.Format("{0}_MES_PanelInformationReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PanelInformationReply_Timeout), trxID);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //PanelInformationReply
        //Update Panel Information和Download to PLC 未完成
        public void MES_PanelInformationReply(XmlDocument xmlDoc)
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

                #region kill Timeout
                string timeoutName = string.Format("{0}_MES_PanelInformationReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion
                
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                string productSpecName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string productRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTRECIPENAME].InnerText;
                string processOperationName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PROCESSOPERATIONNAME].InnerText;
                string productOwner = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTOWNER].InnerText;

                //Update Panel Information  未完成

                // Download to PLC 未完成
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName.Trim());
                if (eqp == null) throw new Exception(string.Format("CAN NOT FIND EQUIPMENT_ID=[{0}] IN EQUIPMENT OBJECT!", machineName));
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("CAN NOT FIND LINE_ID=[{0}] IN LINE OBJECT!", lineName));
                
                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] PanelInforamtionReply NG LINENAME =[{1}],MACHINENAME =[{2}],PRODUCTNAME =[{3}],CODE =[{4}],MESSAGE =[{5}].",
                                        trxID, lineName, machineName, productName, returnCode, returnMessage));
                    Invoke(eServiceName.JobService, "PanelInforamtionReply", new object[] { eqp.Data.NODENO, eBitResult.ON, trxID });
                    return;
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

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  PanelInformation MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_PanelInformationReply", sArray[0]);
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

        //PanelTestResultReport
        //SPEC裡面寫MES->BC,需確認?
        //1.TESTRESULT未完成
        public void PanelTestResultReport(string trxID, string lineName, Equipment eqp,string unitID, string productName, string testResult)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Unit unit = ObjectManager.UnitManager.GetUnit(unitID);

                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PanelTestResultReport") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unitID);
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;
                bodyNode[keyHost.TESTRESULT].InnerText = testResult;
                
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //WorkOrderReport 未完成
        //名稱為Report但訊息方向為MES->BC 待確認? 目前先依SPEC
        //update WorkOrder 未完成
        public void MES_WorkOrderReport(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Download LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string workOrderName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WORKORDERNAME].InnerText;
                string planQuantity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PLANQUANTITY].InnerText;
                string productSpecName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string ownerType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OWNERTYPE].InnerText;
                string ownerId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OWNERID].InnerText;

                //update WorkOrder 未完成
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //WorkOrderStateChanged
        public void WorkOrderStateChanged(string trxID, string lineName, Equipment eqp, string workOrderName, string workOrderState)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("WorkOrderStateChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.WORKORDERNAME].InnerText = workOrderName;
                bodyNode[keyHost.WORKORDERSTATE].InnerText = workOrderState;

                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                #region MES Work Order State Changed Reply Timeout
                string timeoutName = string.Format("{0}_MES_WorkOrderStateChangedReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(WorkOrderStateChangedReply_Timeout), trxID);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //WorkOrderStateChangedReply
        //未完成
        public void MES_WorkOrderStateChangedReply(XmlDocument xmlDoc)
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

                #region kill Timeout
                string timeoutName = string.Format("{0}_WorkOrderStateChangedReply", trxID);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                #endregion


                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string workOrderName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.WORKORDERNAME].InnerText;
                string planQuantity = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PLANQUANTITY].InnerText;
                string productSpecName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTSPECNAME].InnerText;
                string ownerType = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OWNERTYPE].InnerText;
                string ownerId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.OWNERID].InnerText;
                string resultCode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.RESULTCODE].InnerText;

                //update WorkOrder 未完成
                
                

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void WorkOrderStateChangedReply_Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string err = string.Empty;

                err = string.Format("[BCS -> MES]=[{0}]  EQP ID={1}  WorkOrderStateChanged MES Reply Timeout.", trackKey, sArray[0]);

                string timeoutName = string.Format("{0}_MES_WorkOrderStateChangedReply", sArray[0]);
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


        //BoxProcessCanceled
        public void BoxProcessCanceled(string trxID, string lineName, Equipment eqp, string portID, string boxName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessCanceled") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = portID;
                bodyNode[keyHost.BOXNAME].InnerText = boxName;

                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //BoxProcessEnd
        //BOXQUANTITY POSITION PRODUCTNAME未完成
        public void BoxProcessEnd(string trxID, Port port, Cassette cst)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (CheckOffline(line, trxID, MethodBase.GetCurrentMethod().Name))
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return; 
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("BoxProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                
                XmlNode boxNode = bodyNode[keyHost.BOX];

                boxNode[keyHost.BOXNAME].InnerText = cst.BoxName;
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = cst.Jobs.Count.ToString();

                XmlNode productListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode productNode = bodyNode[keyHost.PRODUCT];

                //BOXQUANTITY POSITION PRODUCTNAME未完成
                productNode[keyHost.POSITION].InnerText = "";
                productNode[keyHost.PRODUCTNAME].InnerText = "";

                SendToMES(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //OutBoxProcessEnd
        //未完成
        public void OutBoxProcessEnd(string trxID, string lineID, string portID, Cassette cst)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("OutBoxProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName; ;
                bodyNode[keyHost.BOXQUANTITY].InnerText = "1";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                XmlNode boxNode = bodyNode[keyHost.BOX];
                boxNode.RemoveAll();//把原格式中的BOX移除
                
                boxNode[keyHost.BOXNAME].InnerText = cst.CassetteID.Trim();
                boxNode[keyHost.PRODUCTQUANTITY].InnerText = "";

                XmlNode productListNode = boxNode[keyHost.PRODUCTLIST];
                XmlNode productNode = productListNode[keyHost.PRODUCT];
                
                productNode[keyHost.POSITION].InnerText = "";
                productNode[keyHost.PRODUCTNAME].InnerText = "";

                boxNode.AppendChild(productListNode);
                productListNode.AppendChild(productNode);

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

        //OutBoxProcessEndReply
        //未完成
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

                string boxName = xmldoc[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXNAME].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_OutBoxProcessEndReply  NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                            trxID, lineName, returnCode, returnMessage));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}]  MES_OutBoxProcessEndReply  OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
