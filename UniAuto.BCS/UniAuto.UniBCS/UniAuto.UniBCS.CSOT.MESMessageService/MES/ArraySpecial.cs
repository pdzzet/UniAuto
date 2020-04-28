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
        /// 6.32.	ChangePVDMaterialLife        MES<-BC Reports when  consume PVD  material.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">MachineName</param>
        /// <param name="productName">ProductName</param>
        /// <param name="chamberList">ChamberList</param>
        public void ChangePVDMaterialLife(string trxID, string lineName, string machineName, string productName, IList<ChangePVDMaterialLife.CHAMBERc> chamberList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].",
                        //trxID, lineName));
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangePVDMaterialLife") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = productName;

                XmlNode chamberListNode = bodyNode[keyHost.CHAMBERLIST];
                XmlNode chamberNode = chamberListNode[keyHost.CHAMBER].Clone();

                chamberListNode.RemoveAll();

                foreach (ChangePVDMaterialLife.CHAMBERc chamber in chamberList)
                {
                    XmlNode chambernode = chamberNode.Clone();
                    chambernode[keyHost.MATERIALTYPE].InnerText = chamber.MATERIALTYPE;
                    chambernode[keyHost.CHAMBERID].InnerText = chamber.CHAMBERID;
                    chambernode[keyHost.QUANTITY].InnerText = chamber.QUANTITY;
                    chamberListNode.AppendChild(chambernode);
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

        /// <summary>
        ///    6.180    CIMModeChangeReport     BC->MES Reports when CIMMode Change
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="CIMMode"></param>
        public void CIMModeChangeReport(string trxID,string lineName,string machineName,eBitResult CIMMode)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp.File.PreCIMMode == CIMMode)
                    return; //CIMMode无变化,就不要上报（SECS机台需要判断）
                /*
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].",
                        //trxID, lineName));
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                 */  //MES说OFFLINE也要上报（避免丢失CIMMode,影响判断）

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CIMModeChangeReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.CIMMODE].InnerText = CIMMode == eBitResult.ON ? "Y" : "N";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                if (ParameterManager["MESSYNCREPORT"].GetBoolean())// 判断是否需要同步上报MES
                    SendToQueue(xml_doc);
                else
                    SendToMES(xml_doc);

                lock (eqp.File)
                    eqp.File.PreCIMMode = CIMMode;

                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                  trxID, lineName));
            }
            catch(Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
