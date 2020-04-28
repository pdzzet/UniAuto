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
            public string MATERIALPOSITION1;
            /// <summary>
            /// 6.5.	AutoDecreaseMaterialQuantity        MES MessageSet : Reports when glass consume material.
            /// </summary>
            /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
            /// <param name="lineName">Line ID</param>
            /// <param name="eqpid">EQP ID</param>
            /// <param name="materialid">Material ID</param>
            /// <param name="materialtype">Material type</param>
            /// <param name="pnlID">Panel/Glass ID</param>
            /// <param name="materialqty">Decrease Material Qty</param>
            public void AutoDecreaseMaterialQuantity(string trxID, string lineName, string eqpid, string materialid, string materialtype, string pnlID, string materialqty)
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
                        XmlDocument xml_doc = agent.GetTransactionFormat("AutoDecreaseMaterialQuantity") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                        bodyNode[keyHost.LINENAME].InnerText = lineName;
                        bodyNode[keyHost.MACHINENAME].InnerText = eqpid;
                        bodyNode[keyHost.MATERIALNAME].InnerText = materialid;
                        bodyNode[keyHost.MATERIALTYPE].InnerText = materialtype;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = pnlID;
                        bodyNode[keyHost.QUANTITY].InnerText = materialqty;

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
            /// 6.26.	ChangeMaterialLifeReport
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineName"></param>
            /// <param name="machineName"></param>
            /// <param name="productName"></param>
            /// <param name="materialList"></param>
            public void ChangeMaterialLife(string trxID, string lineName, string machineName, string productName, IList<ChangeMaterialLifeReport.MATERIALc> materialList)
            {
                  try
                  {
                        Line line = ObjectManager.LineManager.GetLine(lineName);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] ChangeMaterialLife Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                              string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("ChangeMaterialLifeReport") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = lineName;
                        bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = productName;

                        XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                        materListNode.RemoveAll();

                        foreach (ChangeMaterialLifeReport.MATERIALc material in materialList)
                        {
                              XmlNode materNode = prodCloneNode.Clone();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MATERIALTYPE;
                              materNode[keyHost.MATERIALNAME].InnerText = material.MATERIALNAME;
                              materNode[keyHost.CHAMBERID].InnerText = material.CHAMBERID;
                              materNode[keyHost.QUANTITY].InnerText = material.QUANTITY;
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

            private void MaterialStatusChangeMESTimeout(object subject, System.Timers.ElapsedEventArgs e)
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
                          string.Format("[EQUIPMENT={0}] [BCS <- MES]=[{1}] MES Reply Material Status Change  Timeout.", sArray[0], sArray[1]));

                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                  }
            }

            /// <summary>
            /// 6.87.	MaterialConsumableReport        MES MessageSet : Reports when glass consume Material.
            /// </summary>
            /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
            /// <param name="lineName">Line ID</param>
            /// <param name="nodeno">Node No</param>
            /// <param name="casseqno">Consum Material List</param>
            /// <param name="jobseqno">Node No</param>
            /// <param name="materilst">Consum Material List</param>
            public void MaterialConsumableRequest(string trxID, string lineName, Equipment eqp, string glassID, string materialDurableName, IList<MaterialEntity> materilst)
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
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialConsumableReport") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                        bodyNode[keyHost.LINENAME].InnerText = lineName;
                        bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = glassID;
                        bodyNode[keyHost.MATERIALDURABLENAME].InnerText = materialDurableName;  //Add for MES 2015/7/15

                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        foreach (MaterialEntity material in materilst)
                        {
                              XmlNode materNode = materNodeClone.Clone();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                              materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                              materNode[keyHost.QUANTITY].InnerText = material.MaterialValue;
                              materialListNode.AppendChild(materNode);
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

            ///// <summary>
            /////  重载
            ///// </summary>
            ///// <param name="trxID"></param>
            ///// <param name="eqp"></param>
            ///// <param name="materialmode"></param>
            ///// <param name="glassID"></param>
            ///// <param name="materialList"></param>
            //public void MaterialMount(string trxID, Equipment eqp, string glassID, List<MaterialEntity> materialList)
            //{
            //         MaterialMount( trxID,  eqp,  glassID,"", "", materialList) ;
            //}
            /// <summary>
            /// 6.88.	MaterialMount       MES MessageSet : Reports when material state has been changed
            /// Add by marine for MES 2015/7/8
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

                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }

                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialMount") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                        bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) //2016/01/26 cc.kuang
                        {
                            bodyNode[keyHost.LINERECIPENAME].InnerText = line.File.LineRecipeName;
                        }
                        bodyNode[keyHost.PRODUCTNAME].InnerText = glassID;
                        bodyNode[keyHost.MATERIALDURABLENAME].InnerText = materialDurableName;
                        bodyNode[keyHost.POLTYPE].InnerText = polType;

                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        foreach (MaterialEntity material in materialList)
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, material.UnitNo);
                              //20151109 cy:找不到就丟Exception會造成後面都不做了,機台也有可能報上來的UnitNo是0
                              //if (unit == null) throw new Exception(string.Format("Can't find UNIT =[{0}] in UnitEntity!", unit));
                              bodyNode[keyHost.MATERIALMODE].InnerText = material.eMaterialMode.ToString(); //Modify 2015/7/13
                              XmlNode materNode = materNodeClone.Clone();
                              materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                              materNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                              materNode[keyHost.USEDCOUNT].InnerText = material.UseCount;//20160902 Modified by zhangwei
                              materNode[keyHost.GROUPID].InnerText = material.GroupId;
                              materNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialValue+"000"; //20160927 Modified by zhangwei
                              //20151109 cy:找不到就丟Exception會造成後面都不做了,機台也有可能報上來的UnitNo是0
                              materNode[keyHost.UNITID].InnerText = unit == null ? string.Empty : unit.Data.UNITID; //unit.Data.UNITID;
                              materNode[keyHost.MATERIALSITE].InnerText = material.Site;//add by hujunpeng 20190223
                              materialListNode.AppendChild(materNode);
                              MATERIALPOSITION1 = material.MaterialPosition;
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
            public void MaterialMount_CELL_PAM(string trxID, Equipment eqp, string glassID, string materialDurableName, string polType, List<MaterialEntity> materialList)
            {
                  try
                  {
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line.Data.LINEID.Contains("TCPHL") && eqp.Data.NODEATTRIBUTE == "DNS") //cc.kuang 2015/09/24
                              eqp.File.MaterialChange = false;

                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }

                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialMount") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                        bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        bodyNode[keyHost.MATERIALMODE].InnerText = eMaterialMode.NORMAL.ToString();
                        bodyNode[keyHost.PRODUCTNAME].InnerText = glassID;
                        bodyNode[keyHost.MATERIALDURABLENAME].InnerText = materialDurableName;
                        bodyNode[keyHost.POLTYPE].InnerText = polType;

                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        //20151111 cy:CELL POL/RWK 不需產生List內容
                        //foreach (MaterialEntity material in materialList)
                        //{
                        //      Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, material.UnitNo);
                        //      //20151109 cy:找不到就丟Exception會造成後面都不做了,機台也有可能報上來的UnitNo是0
                        //      //if (unit == null) throw new Exception(string.Format("Can't find UNIT =[{0}] in UnitEntity!", unit));
                        //      XmlNode materNode = materNodeClone.Clone();
                        //      materNode[keyHost.MATERIALNAME].InnerText = string.Empty; //material.MaterialID;
                        //      materNode[keyHost.MATERIALSTATE].InnerText = string.Empty; //material.MaterialStatus.ToString();
                        //      materNode[keyHost.MATERIALTYPE].InnerText = string.Empty; //material.MaterialType;
                        //      materNode[keyHost.USEDCOUNT].InnerText = string.Empty; //material.MaterialValue;
                        //      materNode[keyHost.GROUPID].InnerText = string.Empty; //material.GroupId;
                        //      materNode[keyHost.MATERIALWEIGHT].InnerText = string.Empty; //material.MaterialWeight;
                        //      //20151109 cy:找不到就丟Exception會造成後面都不做了,機台也有可能報上來的UnitNo是0
                        //      materNode[keyHost.UNITID].InnerText = string.Empty; //unit == null ? string.Empty : unit.Data.UNITID; //unit.Data.UNITID;
                        //      materialListNode.AppendChild(materNode);
                        //}

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

            /// <summary>
            /// 6.89.	MaterialMountReply     MES MessageSet : Reports when material state has been changed,mes reply to bc.
            /// Add by marine for MES 2015/7/9
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

                        //20151112 cy:Cell Special,另外處理
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                              MES_MaterialMountReply_CELL(xmlDoc, line, eqp);
                              return;
                        }

                        string returnCode = GetMESReturnCode(xmlDoc);
                        string returnMessage = GetMESReturnMessage(xmlDoc);
                        MaterialEntity material = new MaterialEntity();
                        
                        string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;

                        string materialMode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALMODE].InnerText;
                        string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                        string materialDurablename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALDURABLENAME].InnerText;
                        string cartrigeLifeTime = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARTRIGELIFETIME].InnerText;
                        
                        
                        XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;
                        foreach (XmlNode node in materialList)
                        {
                            material.MaterialType = node[keyHost.MATERIALTYPE].InnerText;
                            material.MaterialID = node[keyHost.MATERIALNAME].InnerText;
                            material.MaterialState = node[keyHost.MATERIALSTATE].InnerText;
                            material.UseCount = node[keyHost.USEDCOUNT].InnerText;
                            material.MaterialAliveTime = node[keyHost.LIFEQTIME].InnerText;
                            material.GroupId = node[keyHost.GROUPID].InnerText;
                            Unit unit = ObjectManager.UnitManager.GetUnit(node[keyHost.UNITID].InnerText.Trim());
                            if (unit == null)
                                material.UnitNo = "0";
                            else
                                material.UnitNo = unit.Data.UNITNO;
                            material.ValiResult = node[keyHost.VALIRESULT].InnerText;
                            material.MaterialPosition = node[keyHost.MATERIALPOSITION].InnerText;
                            material.MaterialCount = node[keyHost.MATERIALCOUNT].InnerText;
                            material.MaterialAbnormalCode = node[keyHost.MATERIALABNORMALCODE].InnerText;
                            material.MaterialWarningTime = node[keyHost.MATERIALWARNINGTIME].InnerText;
                            if (node.InnerXml.Contains("WARMCOUNT"))
                            {
                                material.WarmCount = node[keyHost.WARMCOUNT].InnerText;
                                if (string.IsNullOrEmpty(material.WarmCount)) material.WarmCount = "0";
                                lock (eqp.File)
                                {
                                    switch (MATERIALPOSITION1)
                                    {

                                        case "1":
                                        case "3":
                                            eqp.File.Slot1WarmCountFlag = material.WarmCount;
                                            break;
                                        case "2":
                                        case "4":
                                            eqp.File.Slot2WarmCountFlag = material.WarmCount;
                                            break;
                                        default:
                                            break;
                                    }
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                }
                            }//modify by hujunpeng 2018/6/1
                            if ((line.Data.FABTYPE == "CF") && (material.MaterialType == "PR") && (material.MaterialState == "INUSE") && (material.MaterialID[0] == 'N'))// ADD by qiumin 20180307
                            {
                                try
                                {
                                    material.MaterialBatchSame = node[keyHost.MATERIALBATCHSAME].InnerText;
                                    if (material.MaterialBatchSame[0] == 'N')
                                    { Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", trxId }); }
                                }
                                catch
                                {


                                    continue;
                                }

                            }
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
                                                DateTime LifeQTime;
                                                double warringtime;
                                                foreach (XmlNode node in materialList)
                                                {
                                                      if (!node[keyHost.VALIRESULT].InnerText.Trim().Equals("Y"))
                                                      {
                                                            checkFlag = false;
                                                            cimMessage = "MES Mount Reply VALIRESULT Value <> Y, Material ID = " + node[keyHost.MATERIALNAME].InnerText;
                                                            break;
                                                      }

                                                      if (node[keyHost.MATERIALWARNINGTIME].InnerText.Trim().Length > 0 && node[keyHost.LIFEQTIME].InnerText.Trim().Length > 0)
                                                      {
                                                            LifeQTime = DateTime.ParseExact(node[keyHost.LIFEQTIME].InnerText.Trim().Substring(0, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                                            warringtime = double.Parse(node[keyHost.MATERIALWARNINGTIME].InnerText.Trim());
                                                            if (LifeQTime.Subtract(DateTime.Now).TotalMinutes < warringtime)
                                                            {
                                                                  checkFlag = false;
                                                                  cimMessage = "MES Mount Reply LIFEQTIME Check is Over Warring Time, Material ID = " + node[keyHost.MATERIALNAME].InnerText;
                                                                  break;
                                                            }
                                                      }
                                                }
                                                //就算MES沒回Material List,還是要回應機台 20151006 cy
                                                //if (materialList.Count > 0)
                                                {
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
                              }
                              Timermanager.TerminateTimer(timerId1);
                        }
                        else
                              Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", _data);

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialMountReply NG LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALMODE =[{4}],PRODUCTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                                             trxId, lineId, machineName, lineRecipeName, materialMode, productName, returnCode, returnMessage));
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
            public void MES_MaterialMountReply_CELL(XmlDocument xmlDoc, Line line, Equipment eqp)
            {
                try
                {
                    string returnCode = GetMESReturnCode(xmlDoc);
                    string returnMessage = GetMESReturnMessage(xmlDoc);
                    string trxId = GetTransactionID(xmlDoc);
                    string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                    string materialMode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALMODE].InnerText;
                    string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                    string materialDurablename = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALDURABLENAME].InnerText;
                    string cartrigeLifeTime = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARTRIGELIFETIME].InnerText;

                    XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;

                    string timeId8 = string.Format("MaterialStatusChangeReport_CELL_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, trxId);

                    if (Timermanager.IsAliveTimer(timeId8))
                    {
                        string unitNo = "0", operatorID = string.Empty, feedingPortID = string.Empty;
                        MaterialEntity material = null;
                        UserTimer timer8 = Timermanager.GetAliveTimer(timeId8);
                        Timermanager.TerminateTimer(timeId8);

                        if (line.Data.LINETYPE.Contains(keyCellLineType.POL) || line.Data.LINETYPE == eLineType.CELL.CCRWK)//sy modify  20160705
                        {
                            #region PAM lot material
                            //透過TimerManager的State取得UnitNO
                            if (timer8 != null)
                            {
                                Tuple<string, string, string, string> tuple = timer8.State as Tuple<string, string, string, string>;
                                if (tuple != null)
                                {
                                    unitNo = tuple.Item2;
                                    operatorID = tuple.Item3;
                                    feedingPortID = tuple.Item4;
                                }
                            }
                            material = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, materialDurablename);
                            //找不到,新增一筆,MES回的若是OK,才真的加進去並記History
                            if (material == null)
                            {
                                material = new MaterialEntity();
                                material.NodeNo = eqp.Data.NODENO;
                                material.OperatorID = operatorID;
                                material.MaterialID = materialDurablename;
                                material.UnitNo = unitNo;
                                material.MaterialStatus = eMaterialStatus.MOUNT;
                                material.MaterialRecipeID = lineRecipeName;
                                material.MaterialType = feedingPortID.Trim();
                                if (returnCode == "0")
                                {
                                    ObjectManager.MaterialManager.AddMaterial(material);
                                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, material, string.Empty, trxId);
                                }
                            }
                            else
                            {
                                material.MaterialStatus = eMaterialStatus.MOUNT;//sy add 20160216
                                //已經存在但MES回NG,記History NG
                                if (returnCode != "0")
                                {
                                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, material, string.Empty, "MES REPLY NG");
                                }
                            }
                            material.CellPAMMateril.Clear();
                            MaterialEntity subMaterial;
                            foreach (XmlNode node in materialList)
                            {
                                subMaterial = new MaterialEntity();
                                subMaterial.NodeNo = eqp.Data.NODENO;
                                subMaterial.MaterialType = node[keyHost.MATERIALTYPE] == null ? string.Empty : node[keyHost.MATERIALTYPE].InnerText;
                                subMaterial.MaterialID = node[keyHost.MATERIALNAME] == null ? string.Empty : node[keyHost.MATERIALNAME].InnerText;
                                subMaterial.MaterialState = node[keyHost.MATERIALSTATE] == null ? string.Empty : node[keyHost.MATERIALSTATE].InnerText;
                                subMaterial.UseCount = node[keyHost.USEDCOUNT] == null ? "0" : node[keyHost.USEDCOUNT].InnerText;
                                subMaterial.MaterialAliveTime = node[keyHost.LIFEQTIME] == null ? "0" : node[keyHost.LIFEQTIME].InnerText;
                                subMaterial.GroupId = node[keyHost.GROUPID] == null ? string.Empty : node[keyHost.GROUPID].InnerText;
                                if (node[keyHost.UNITID] == null)
                                    subMaterial.UnitNo = "0";
                                else
                                {
                                    Unit unit = ObjectManager.UnitManager.GetUnit(node[keyHost.UNITID].InnerText.Trim());
                                    if (unit == null)
                                        subMaterial.UnitNo = "0";
                                    else
                                        subMaterial.UnitNo = unit.Data.UNITNO;
                                }
                                subMaterial.ValiResult = node[keyHost.VALIRESULT] == null ? "N" : node[keyHost.VALIRESULT].InnerText;
                                subMaterial.MaterialPosition = node[keyHost.MATERIALPOSITION] == null ? string.Empty : node[keyHost.MATERIALPOSITION].InnerText;
                                subMaterial.MaterialCount = node[keyHost.MATERIALCOUNT] == null ? string.Empty : node[keyHost.MATERIALCOUNT].InnerText;
                                subMaterial.MaterialAbnormalCode = node[keyHost.MATERIALABNORMALCODE] == null ? string.Empty : node[keyHost.MATERIALABNORMALCODE].InnerText;
                                subMaterial.MaterialWarningTime = node[keyHost.MATERIALWARNINGTIME] == null ? string.Empty : node[keyHost.MATERIALWARNINGTIME].InnerText;
                                subMaterial.MaterialPort = node[keyHost.MATERIALPOSITION] == null ? string.Empty : node[keyHost.MATERIALPOSITION].InnerText;//用來當dismount key
                                material.CellPAMMateril.Add(subMaterial);
                            }
                            Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply_CELL_PAM",
                                  new object[] { eqp.Data.NODENO, unitNo, string.Empty, material, eBitResult.ON, returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG, trxId, false });
                            #endregion
                        }
                        else
                        {
                            #region [Other]
                            //TODO:假設MES一定會回一筆,只用List的第1筆
                            XmlNode xNode = materialList[0];
                            material = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, xNode[keyHost.MATERIALNAME].InnerText.Trim());
                            if (material == null)
                            {
                                material = new MaterialEntity();
                                material.NodeNo = eqp.Data.NODENO;
                                material.OperatorID = operatorID;
                                material.MaterialID = xNode[keyHost.MATERIALNAME].InnerText.Trim();
                                if (xNode[keyHost.UNITID] == null)
                                    material.UnitNo = "0";
                                else
                                {
                                    Unit unit = ObjectManager.UnitManager.GetUnit(xNode[keyHost.UNITID].InnerText.Trim());
                                    if (unit == null)
                                        material.UnitNo = "0";
                                    else
                                        material.UnitNo = unit.Data.UNITNO;
                                }
                                material.MaterialStatus = eMaterialStatus.MOUNT;
                                material.MaterialRecipeID = lineRecipeName;
                                material.MaterialType = xNode[keyHost.MATERIALTYPE] == null ? string.Empty : xNode[keyHost.MATERIALTYPE].InnerText;
                                material.MaterialSpecName = xNode[keyHost.MATERIALSPECNAME] == null ? string.Empty : xNode[keyHost.MATERIALSPECNAME].InnerText;
                                if (returnCode == "0")
                                {
                                    ObjectManager.MaterialManager.AddMaterial(material);
                                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, material, string.Empty, trxId);
                                }
                            }
                            else
                            {
                                //已經存在但MES回NG,記History NG
                                if (returnCode != "0")
                                {
                                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, material, string.Empty, "MES REPLY NG");
                                }
                            }
                            material.UseCount = xNode[keyHost.USEDCOUNT] == null ? "0" : xNode[keyHost.USEDCOUNT].InnerText;
                            material.MaterialAliveTime = xNode[keyHost.LIFEQTIME] == null ? "0" : xNode[keyHost.LIFEQTIME].InnerText;
                            material.ValiResult = xNode[keyHost.VALIRESULT] == null ? "N" : xNode[keyHost.VALIRESULT].InnerText;
                            material.MaterialType = xNode[keyHost.MATERIALTYPE] == null ? string.Empty : xNode[keyHost.MATERIALTYPE].InnerText;
                            material.MaterialSpecName = xNode[keyHost.MATERIALSPECNAME] == null ? string.Empty : xNode[keyHost.MATERIALSPECNAME].InnerText;
                            Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply",
                                  new object[] { eqp.Data.NODENO, unitNo, string.Empty, material, eBitResult.ON, returnCode == "0" ? eReturnCode1.OK : eReturnCode1.NG, trxId, false });
                            #endregion
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                               trxId, line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage));
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }

            /// <summary>
            /// 6.90.	MaterialDismountReport      MES MessageSet : Reports when material state has been changed to DISMOUNT.
            /// Add by marine for MES 2015/7/9
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
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }

                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialDismountReport") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                        bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) //2016/01/26 cc.kuang
                        {
                            bodyNode[keyHost.LINERECIPENAME].InnerText = line.File.LineRecipeName;
                        }
                        bodyNode[keyHost.PRODUCTNAME].InnerText = glassID;
                        bodyNode[keyHost.MATERIALDURABLENAME].InnerText = materialDurableName;
                        
                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        foreach (MaterialEntity material in materialList)
                        {
                              bodyNode[keyHost.MATERIALMODE].InnerText = material.eMaterialMode.ToString(); //Modify 2015/7/13 
                              XmlNode materNode = materNodeClone.Clone();
                              materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                              materNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                              materNode[keyHost.USEDCOUNT].InnerText = material.UseCount;//20160902 Modified by zhangwei
                              materNode[keyHost.GROUPID].InnerText = material.GroupId;
                              materNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialValue+"000";  //20160927 Modified by zhangwei
                              materNode[keyHost.MATERIALPOSITION].InnerText = material.MaterialPosition;
                              materNode[keyHost.MATERIALCOUNT].InnerText = material.MaterialCount;
                              materNode[keyHost.MATERIALABNORMALCODE].InnerText = material.MaterialAbnormalCode;
                              materNode[keyHost.USEDTIME].InnerText = material.UsedTime;

                              //20170301 huangjiayin add for apr special
                              #region APR-PI
                              try
                              {
                                  if (line.Data.LINEID.Contains("CCPIL") && material.MaterialType.Trim() == "APR")
                                  {

                                      materNode[keyHost.CONSUMMATERIALID].InnerText =
                                          string.Compare(material.MaterialSpecName.Split('_')[0], material.InUseTime) == 1 ? material.MaterialSpecName.Split('_')[1]
                                          : string.Empty;
                                  }
                                  else
                                  {
                                      //materNode[keyHost.CONSUMMATERIALID].InnerText = string.Empty;

                                  }
                              }
                              catch (Exception ex)
                              {
                                  materNode[keyHost.CONSUMMATERIALID].InnerText = string.Empty;
                              }
                              #endregion


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
            public void MaterialDismountReport_CELL_PAM(string trxID, Equipment eqp, string glassID, string materialDurableName, List<MaterialEntity> materialList)
            {
                  try
                  {
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }

                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialDismountReport") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                        bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = glassID;
                        bodyNode[keyHost.MATERIALDURABLENAME].InnerText = materialDurableName;
                        //20151113 cy:修改Material Mode邏輯,material list內,存在一筆Count > 0,視為Abnromal 
                        bodyNode[keyHost.MATERIALMODE].InnerText = eMaterialMode.NORMAL.ToString();
                        if (materialList != null && materialList.Count > 0 && materialList.Any(m => m.MaterialCount != "0"))
                              bodyNode[keyHost.MATERIALMODE].InnerText = eMaterialMode.ABNORMAL.ToString();

                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        foreach (MaterialEntity material in materialList)
                        {
                              if (material.MaterialPosition == "0") break;//濾掉多餘的
                              XmlNode materNode = materNodeClone.Clone();
                              materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                              materNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                              materNode[keyHost.USEDCOUNT].InnerText = material.MaterialValue;
                              materNode[keyHost.GROUPID].InnerText = material.GroupId;
                              materNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialWeight;
                              materNode[keyHost.MATERIALPOSITION].InnerText = material.MaterialPosition;
                              materNode[keyHost.MATERIALCOUNT].InnerText = material.MaterialCount;
                              materNode[keyHost.MATERIALABNORMALCODE].InnerText = material.MaterialAbnormalCode;
                              materNode[keyHost.USEDTIME].InnerText = material.UsedTime;
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

            /// <summary>
            /// 6.91.	MaterialWeightReport     MES MessageSet : Reports when material load a weighing machine
            /// Add by marine for T3 MES 2015/8/24
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineID"></param>
            /// <param name="eqp"></param>
            /// <param name="lineRecipeName"></param>
            /// <param name="materialMode"></param>
            /// <param name="glassID"></param>
            /// <param name="materilst"></param>
            public void MaterialWeightReport(string trxID, Equipment eqp, string materialMode, string panelID, List<MaterialEntity> materialList)
            {
                  try
                  {
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }

                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialWeightReport") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);

                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                        bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                        bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        bodyNode[keyHost.MATERIALMODE].InnerText = materialMode;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = panelID;

                        XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                        materialListNode.RemoveAll();

                        foreach (MaterialEntity material in materialList)
                        {
                              XmlNode materNode = materNodeClone.Clone();
                              materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                              materNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                              materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                              materNode[keyHost.USEDCOUNT].InnerText = material.MaterialValue;
                              materNode[keyHost.GROUPID].InnerText = material.GroupId;
                              materNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialWeight;
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

            /// <summary>
            /// 6.184.	MaterialConsumeChangeReport    MES MessageSet : Reports when PIC Receive T\C Product Type Change
            /// Add by marine for T3 MES 2017/11/28
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="eqp"></param>
            /// <param name="materialMode"></param>
            /// <param name="panelID"></param>
            /// <param name="materilst"></param>
            public void MaterialConsumeChangeReport(string trxID, Equipment eqp, string materialMode, string panelID, List<MaterialEntity> materialList)
            {
                try
                {
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                        return;
                    }

                    IServerAgent agent = GetServerAgent();
                    XmlDocument xml_doc = agent.GetTransactionFormat("MaterialConsumeChangeReport") as XmlDocument;
                    SetTransactionID(xml_doc, trxID);

                    XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                    bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                    bodyNode[keyHost.MATERIALMODE].InnerText = materialMode;
                    bodyNode[keyHost.PRODUCTNAME].InnerText = panelID;

                    XmlNode materialListNode = bodyNode[keyHost.MATERIALLIST];
                    XmlNode materNodeClone = materialListNode[keyHost.MATERIAL].Clone();
                    materialListNode.RemoveAll();

                    foreach (MaterialEntity material in materialList)
                    {
                        XmlNode materNode = materNodeClone.Clone();
                        if (string.IsNullOrEmpty(material.MaterialID) && (line.Data.LINEID == "CCPIL100" || line.Data.LINEID == "CCPIL200")) break;
                        materNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                        materNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;
                        materNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialWeight;
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


            /// <summary>
            ///  6.92.	MaterialStateChanged        MES MessageSet : On Line Material Status Change Report to MES
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

                        bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = string.Empty; //material Status 不能得知PanelID??

                        //Watson modify 20141124 For MES Spec (原來竟然寫MASK)
                        XmlNode maskListNode = bodyNode[keyHost.MATERIALLIST];  //Watson modify 20141124 For MES Spec
                        XmlNode maskNodeClone = maskListNode[keyHost.MATERIAL];  //Watson modify 20141124 For MES Spec
                        maskListNode.RemoveAll();

                        List<MaterialEntity> materialList = ObjectManager.MaterialManager.GetMaterials();
                        if ((materialList != null) && (materialList.Count > 0))  //Watson Modify 20150327 For Maybe Material  != null, But Material Count == 0
                        {
                              foreach (MaterialEntity material in materialList)
                              {
                                    if (material.NodeNo != eqp.Data.NODENO ||
                                        (line.Data.FABTYPE == "CELL" && material.EQType == eMaterialEQtype.MaskEQ))
                                          return;

                                    if (material.MaterialStatus == eMaterialStatus.DISMOUNT)
                                    {
                                          if (material.MaterialValue == "0")
                                                bodyNode[keyHost.MATERIALMODE].InnerText = "Normal";
                                          else
                                                bodyNode[keyHost.MATERIALMODE].InnerText = "Abnormal";
                                    }
                                    else
                                          bodyNode[keyHost.MATERIALMODE].InnerText = string.Empty;  //MES 未說明

                                    ////Watson modify 20141124 For MES Spec (原來竟然寫MASK)
                                    //XmlNode maskListNode = bodyNode[keyHost.MATERIALLIST];  //Watson modify 20141124 For MES Spec
                                    //XmlNode maskNodeClone = maskListNode[keyHost.MATERIAL];  //Watson modify 20141124 For MES Spec

                                    //目前只會有一個Mask
                                    //for (int i = 0; i < job.MesAbnormal.CODE.Count; i++)
                                    //{
                                    XmlNode maskNode = maskNodeClone.Clone();
                                    maskNode[keyHost.MATERIALNAME].InnerText = material.MaterialID;
                                    maskNode[keyHost.MATERIALSTATE].InnerText = material.MaterialStatus.ToString();
                                    maskNode[keyHost.MATERIALTYPE].InnerText = material.MaterialType;//不知道報什麼 //Jun Modify 20141202
                                    maskNode[keyHost.MATERIALWEIGHT].InnerText = material.MaterialWeight;//Add for T3 MES 2015/7/29
                                    maskNode[keyHost.USEDCOUNT].InnerText = material.MaterialValue;
                                    maskNode[keyHost.GROUPID].InnerText = material.GroupId; //Jun Modify 20141202
                                    maskNode[keyHost.UNITID].InnerText = "";  //Watson Modify 20150120 For MES Spec 不需要報UnitNo    
                                    //ObjectManager.UnitManager.GetUnit(material.NodeNo, material.UnitNo) == null ? string.Empty : ObjectManager.UnitManager.GetUnit(material.NodeNo, material.UnitNo).Data.UNITID;
                                    maskNode[keyHost.HEADID].InnerText = material.HEADID;
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
            ///  6.92.	MaterialStateChanged        MES MessageSet : Material State Changed to MES
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

                        if (eqp == null)
                        { bodyNode[keyHost.LINERECIPENAME].InnerText = ""; }
                        else
                        {
                              if (line.Data.FABTYPE == eFabType.CF.ToString())
                                    bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                              else
                                    //Jun Modify 20150529 一般情況使用機台上報的RecipeID上報 Offline->Online使用CurrentRecipeID
                                    bodyNode[keyHost.LINERECIPENAME].InnerText = pPID;  //eqp.File.CurrentRecipeID;  
                        }

                        if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) //2016/01/26 cc.kuang
                        {
                            bodyNode[keyHost.LINERECIPENAME].InnerText = line.File.LineRecipeName;
                        }

                        bodyNode[keyHost.MACHINENAME].InnerText = eQPID;
                        bodyNode[keyHost.MATERIALMODE].InnerText = materialmode;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = panelID;

                        XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                        materListNode.RemoveAll();

                        //一次只有一片
                        string _materialStatus = string.Empty;
                        XmlNode materNode = prodCloneNode.Clone();
                        materNode[keyHost.MATERIALNAME].InnerText = materialID;
                        if (materialStatus == eMaterialStatus.NONE)
                              _materialStatus = "EMPTY";
                        else
                              _materialStatus = materialStatus.ToString();
                        materNode[keyHost.MATERIALSTATE].InnerText = _materialStatus;
                        materNode[keyHost.MATERIALTYPE].InnerText = type;
                        materNode[keyHost.MATERIALWEIGHT].InnerText = materialWeight; //Add for T3 MES 2015/7/29
                        materNode[keyHost.USEDCOUNT].InnerText = useCount;
                        //materNode[keyHost.MASKUSECOUNT].InnerText = lifeQtime;
                        materNode[keyHost.GROUPID].InnerText = groupID;
                        materNode[keyHost.UNITID].InnerText = unitID;  //Jun Modify 20150226 需要上報UnitID  //Watson Modify 20150120 For MES Spec 不需要報UnitNo;
                        materNode[keyHost.HEADID].InnerText = headID;
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
            /// 6.92.	MaterialStateChanged 
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
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                                      string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                              return;
                        }
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("MaterialStateChanged") as XmlDocument;
                        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);
                        if (eqp == null)
                              bodyNode[keyHost.LINERECIPENAME].InnerText = "";
                        else
                              bodyNode[keyHost.LINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;

                        if (line.Data.FABTYPE == eFabType.ARRAY.ToString()) //2016/01/26 cc.kuang
                        {
                            bodyNode[keyHost.LINERECIPENAME].InnerText = line.File.LineRecipeName;
                        }

                        bodyNode[keyHost.LINENAME].InnerText = lineName;
                        bodyNode[keyHost.MACHINENAME].InnerText = eQPID;
                        bodyNode[keyHost.MATERIALMODE].InnerText = materialmode;
                        bodyNode[keyHost.PRODUCTNAME].InnerText = panelID;

                        XmlNode materListNode = bodyNode[keyHost.MATERIALLIST];
                        XmlNode prodCloneNode = materListNode[keyHost.MATERIAL].Clone();
                        materListNode.RemoveAll();

                        //一次只有一片
                        foreach (MaterialEntity mater in materlist)
                        {
                              XmlNode materNode = prodCloneNode.Clone();
                              materNode[keyHost.MATERIALNAME].InnerText = mater.MaterialID;
                              materNode[keyHost.MATERIALSTATE].InnerText = mater.MaterialStatus.ToString();
                              materNode[keyHost.MATERIALTYPE].InnerText = mater.MaterialType;
                              materNode[keyHost.USEDCOUNT].InnerText = mater.UseCount;
                              materNode[keyHost.MATERIALWEIGHT].InnerText = mater.MaterialWeight;//Add for T3 MES 2015/7/29// 20160902 Modified by zhangwei
                              //materNode[keyHost.LIFEQTIME].InnerText = lifeQtime;
                              materNode[keyHost.GROUPID].InnerText = mater.GroupId;
                              if (mater.UnitNo != "0")
                              {
                                    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, mater.UnitNo);
                                    if (unit != null)
                                    {
                                          materNode[keyHost.UNITID].InnerText = unit.Data.UNITID;  //Jun Modify 20150226 需要上報UnitID  //Watson Modify 20150120 For MES Spec 不需要報UnitNo
                                    }
                              }
                              materNode[keyHost.HEADID].InnerText = mater.HEADID;
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

            /// <summary>
            /// 6.93.	MaterialStateChangedReply       MES MessagetSet : Material Status Change Reply to BC
            /// </summary>
            /// <param name="xmlDoc">Reply XML Document</param>
            public void MES_MaterialStateChangedReply(XmlDocument xmlDoc)
            {
                  try
                  {
                        string returnCode = GetMESReturnCode(xmlDoc);
                        string returnMessage = GetMESReturnMessage(xmlDoc);
                        string lineId = GetLineName(xmlDoc);
                        string trxId = GetTransactionID(xmlDoc);
                        MaterialEntity material = new MaterialEntity();
                        string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                        if (eqp == null) throw new Exception(string.Format("Can't find MACHINENAME =[{0}] in EquipmentEntity!", machineName));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;   //Watson Modify 20150327 NEW SPEC
                        string unitId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST][keyHost.MATERIAL][keyHost.UNITID].InnerText;
                        string unitNo = "";
                        #region get unitno
                        if (unitId != "")
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(unitId);
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
                        string materialMode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALMODE].InnerText;
                        string productName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PRODUCTNAME].InnerText;
                        string command = string.Empty;

                        if (!CheckMESLineID(lineId))
                        {
                              Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, lineId));
                        }
                        string timeId1 = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, trxId);
                        string timeId2 = string.Format("MaterialVerificationRequest_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, trxId);
                        string timeId3 = string.Format("SecsMaterialStatusChangeReport_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, trxId);
                        string timeId5 = string.Format("{0}_{1}_MaterialStateChanged_ONLINE", eqp.Data.NODENO, trxId);  //watson 20150106 Add
                        string timeId6 = string.Format("MaterialVerificationRequest_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, trxId);  //Kasim 20150323 Add
                        string timeId7 = string.Format("MaterialStatusChangeReport_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, trxId);  //Kasim 20150323 Add
                        string timeId8 = string.Format("MaterialStatusChangeReport_CELL_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, trxId);
                        string replyKey = "";
                        if (Timermanager.IsAliveTimer(timeId8))
                        {
                              UserTimer timer = Timermanager.GetAliveTimer(timeId8);
                              if (returnCode == "0")
                                    command = "1";
                              else
                                    command = "2";
                              Timermanager.TerminateTimer(timeId8);
                              
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                 string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                     trxId, lineId, machineName, (eReturnCode1)int.Parse(command), returnMessage));

                              if (line.Data.LINETYPE.Contains(keyCellLineType.POL) || line.Data.LINETYPE == eLineType.CELL.CCRWK)//sy modify  20160705
                              {
                                  Tuple<string, string, string, string> tuple = timer.State as Tuple<string, string, string, string>;
                                  Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply_CELL_PAM",
                                        new object[] { eqp.Data.NODENO, tuple.Item2, "", material, eBitResult.ON, (eReturnCode1)int.Parse(command), trxId, true });  //因為只會寫OK/NG,所以把Disable ON起來
                              }
                              else
                              {
                                  Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                                  Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply",
                                        new object[] { eqp.Data.NODENO, tuple.Item2, "", material, eBitResult.ON, (eReturnCode1)int.Parse(command), trxId, false });
                              }
                              return;
                        }
                        else if (Timermanager.IsAliveTimer(timeId7))
                        {
                              UserTimer timer = Timermanager.GetAliveTimer(timeId7);
                              unitNo = timer.State.ToString();
                              if (returnCode == "0")
                                    command = "1";
                              else
                                    command = "2";
                              Timermanager.TerminateTimer(timeId7);
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                 string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                     trxId, lineId, machineName, (eReturnCode1)int.Parse(command), returnMessage));
                              Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, "", material, eBitResult.ON, (eReturnCode1)int.Parse(command), trxId, false });
                              return;
                        }
                        else if (Timermanager.IsAliveTimer(timeId6))
                        {
                              UserTimer timer = Timermanager.GetAliveTimer(timeId6);
                              unitNo = timer.State.ToString();
                              if (returnCode == "0")
                                    command = "1";
                              else
                                    command = "2";
                              Timermanager.TerminateTimer(timeId6);
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                 string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                     trxId, lineId, machineName, (eReturnCode1)int.Parse(command), returnMessage));
                              Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxId, eBitResult.ON, (eReturnCode1)int.Parse(command), eqp.Data.NODENO, unitNo });
                              return;
                        }
                        else if (Timermanager.IsAliveTimer(timeId5))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                        {
                              Timermanager.TerminateTimer(timeId5);
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                 string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                     trxId, lineId, machineName, returnCode, returnMessage));
                              return;
                        }
                        else if (Timermanager.IsAliveTimer(timeId1))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                        {
                              replyKey = "MaterialStatusChangeReport";
                              Timermanager.TerminateTimer(timeId1);
                        }
                        else if (Timermanager.IsAliveTimer(timeId2))
                        {
                              replyKey = "MaterialVerificationRequest";
                              Timermanager.TerminateTimer(timeId2);
                        }
                        else if (Timermanager.IsAliveTimer(timeId3))
                        {
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALMODE =[{4}],PRODUCTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                                           trxId, lineId, machineName, lineRecipeName, materialMode, productName, returnCode, returnMessage));

                              //get timer by key
                              UserTimer timer = Timermanager.GetAliveTimer(timeId3);
                              if (timer != null)
                              {
                                    Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                                    if (tuple != null)
                                    {
                                          string systembyte = tuple.Item1;
                                          string header = tuple.Item2;
                                          Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge",
                                              new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)(returnCode != "0" ? 1 : 0) });
                                          if (returnCode != "0")
                                          {
                                                Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "MES Reply MaterialStateChange NG", trxId, string.Empty });
                                                if (eqp.Data.NODEATTRIBUTE == "DNS")
                                                {
                                                      Invoke(eServiceName.CSOTSECSService, "TS10F5_H_TerminalDisplaySingleforDNSLC",
                                                          new object[] { eqp.Data.NODENO, eqp.Data.NODEID, header, string.Empty, trxId });
                                                }
                                          }
                                    }
                              }
                              Timermanager.TerminateTimer(timeId3);
                              return;
                        }
                        else
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                 string.Format("[LINENAME={0}][BCS <- MES][{4}] MES_MaterialStateChangedReply not found BCS Send Message LINENAME =[{0}],MACHINENAME =[{1}],CODE =[{2}],MESSAGE =[{3}].",
                                               lineId, machineName, returnCode, returnMessage, trxId));
                              return;
                        }
                        XmlNodeList materialList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MATERIALLIST].ChildNodes;
                        foreach (XmlNode node in materialList)
                        {
                              material.TankNo = node[keyHost.HEADID].InnerText;
                              material.UseCount = node[keyHost.USEDCOUNT].InnerText;
                              material.MaterialAliveTime = node[keyHost.LIFEQTIME].InnerText;
                              material.MaterialID = node[keyHost.MATERIALNAME].InnerText;
                              material.MaterialType = node[keyHost.MATERIALTYPE].InnerText;
                              switch (material.MaterialType)
                              {
                                    case "TANK":
                                          material.MaterialType = "1";
                                          break;
                                    case "HEAD":
                                          material.MaterialType = "2";
                                          break;
                                    case "SEAL-AU":
                                          material.MaterialType = "1";
                                          break;
                                    case "SEAL-GF":
                                          material.MaterialType = "2";
                                          break;
                                    default:
                                          material.MaterialType = "0";
                                          break;
                              }
                              material.GroupId = node[keyHost.GROUPID].InnerText;
                        }
                        material.UnitNo = unitNo;

                        if (returnCode != "0")
                        {
                              //Send NG to Equipment 
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                     string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply NG LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALMODE =[{4}],PRODUCTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                                                   trxId, lineId, machineName, lineRecipeName, materialMode, productName, returnCode, returnMessage));

                              command = "2";
                        }
                        else
                        {
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaterialStateChangedReply OK LINENAME =[{1}],MACHINENAME =[{2}],RECIPEID =[{3}],MATERIALMODE =[{4}],PRODUCTNAME =[{5}],CODE =[{6}],MESSAGE =[{7}].",
                                                   trxId, lineId, machineName, lineRecipeName, materialMode, productName, returnCode, returnMessage));
                              //Send OK To Equipment
                              command = "1";//OK
                        }

                        if (replyKey == "MaterialVerificationRequest")
                        {
                              Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxId, eBitResult.ON, (eReturnCode1)int.Parse(command), eqp.Data.NODENO, unitNo });

                        }
                        else if (replyKey == "MaterialStatusChangeReport")
                        {
                              Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, "0", material, eBitResult.ON, (eReturnCode1)int.Parse(command), trxId, false });
                        }

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }






      }
}
