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
        /// 6.25.	ChamberRunModeChanged   MES MessageSet : Reports when EQP operation mode has been changed. This message applies for EQP that has more than one specific operation 
        ///                                                     modes that defined in equipment operation scenario.When  chamber mode  has  been  changed , BC will  report  this  message 
        /// Add by marine for MES 2015/7/14
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="linename"></param>
        /// <param name="eqp"></param>
        /// <param name="unit"></param>
        public void ChamberRunModeChanged(String trxID, string lineName, string lineOperMode, string machineName, List<ChamberRunModeChanged.CHAMBERc> chamberList)
        {
            try 
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if(line.File.HostMode==eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ChamberRunModeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.LINEOPERMODE].InnerText = lineOperMode;

                XmlNode machineNodeClone = bodyNode[keyHost.MACHINE];
                machineNodeClone[keyHost.MACHINENAME].InnerText = machineName;
                XmlNode chamberListNode = machineNodeClone[keyHost.CHAMBERLIST];
                XmlNode chamberNodeClone = chamberListNode[keyHost.CHAMBER].Clone();

                machineNodeClone.RemoveChild(chamberListNode);
                chamberListNode.RemoveAll();

                //XmlNode machineNode = machineNodeClone.Clone();
                //machineNode[keyHost.MACHINENAME].InnerText = machineName;
                //bodyNode.RemoveChild(machineNodeClone);
                //machineNodeClone.RemoveAll();

                foreach (ChamberRunModeChanged.CHAMBERc chamber in chamberList)
                {
                    XmlNode chamberNode = chamberNodeClone.Clone();
                    chamberNode[keyHost.CHAMBERNAME].InnerText = chamber.CHAMBERNAME;
                    chamberNode[keyHost.CHAMBERRUNMODE].InnerText = chamber.CHAMBERRUNMODE;

                    chamberListNode.AppendChild(chamberNode);
                }
                machineNodeClone.AppendChild(chamberListNode);
                bodyNode.AppendChild(machineNodeClone);


                //XmlNode timeNodeClone = bodyNode[keyHost.TIMESTAMP];
                //XmlNode timeNode = timeNodeClone.Clone();
                //xml_doc[keyHost.MESSAGE].RemoveChild(timeNode);
                ////bodyNode.RemoveChild(timeNode);
                //bodyNode.AppendChild(timeNode);


                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

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
        ///  6.68.	MachineControlStateChanged      MES MessageSet : Reports BC control state has been changed
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="controlStatus">control state ‘OFFLINE’ ‘REMOTE’ ‘LOCAL’ </param>
        public void MachineControlStateChanged(string trxID, string lineName, string controlStatus)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

                bodyNode[keyHost.CONTROLSTATENAME].InnerText = controlStatus.ToString();
                Line line = ObjectManager.LineManager.GetLine(lineName);
                //20160718 add by Frank For Check "ARRAY Dummy Request" Special Rule
                if (line.File.DummyRequestFlag && controlStatus == "REMOTE")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS][{1}] DUMMY REQUEST BIT (ON) SO CAN NOT CHANG CONTROL MODE TO {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, controlStatus));
                    return;
                }
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);
                SendToOEE(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, controlStatus));

                // To Do 判断ControlState 是否是Offline ，如果不是Offline 则需要与MES 做资料同步

                if (line == null) throw new Exception(string.Format("Can't find Line ID =[{0}] in LineEntity!", lineName));

                line.File.PreHostMode = line.File.HostMode;

                switch (controlStatus)
                {
                    case "LOCAL":
                        line.File.HostMode = eHostMode.LOCAL;
                        break;
                    case "REMOTE":
                        line.File.HostMode = eHostMode.REMOTE;
                        break;
                    case "OFFLINE":
                        line.File.HostMode = eHostMode.OFFLINE;
                        break;
                    default:
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, controlStatus));
                        break;
                }

                if (line.File.HostMode != line.File.PreHostMode)
                {
                    #region OPI Service
                    string timeId = string.Format("{0}_OPI_LineModeChangeRequest", line.Data.LINEID);
                    if (Timermanager.IsAliveTimer(timeId))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                        Timermanager.TerminateTimer(timeId);

                    Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                    #endregion

                    ObjectManager.LineManager.EnqueueSave(line.File);
                    ObjectManager.LineManager.RecordLineHistory(trxID, line); //记录Line History
                }

                if (controlStatus != "OFFLINE")
                {
                    if (line.File.PreHostMode == eHostMode.OFFLINE)
                    {
                        Thread.Sleep(2000);
                        MachineDataUpdate(trxID, line);
                        Thread.Sleep(1500);
                        PortDataUpdate(trxID, line);
                        foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID)) //Jun Modify 20141223 By Line取得EQPs
                        {
                            //20150119 cy : 增加TBPHL Line Mask
                            if (!line.Data.LINEID.Contains("TCPHL"))
                                MaskStateChanged_OnLine(trxID, eqp);  //if Mask Machine is mounted Report else not report
                            else
                                MaskStateChanged_OnLine_TCPHL(trxID, eqp);

                            //if (!line.Data.LINEID.Contains("CCPOL"))//MES spec 1.21 remove shihyang
                                MaterialStateChanged_OnLine(this.CreateTrxID(), eqp);
                            //else
                            //    POLStateChanged_OnLine(this.CreateTrxID(), eqp);
                        }
                        //MES say 不需要 20141115
                        //QtimeSetChanged_OnLine(this.CreateTrxID(), line.Data.LINEID);

                        //add by yang 2017/4/24 for ChangerPlan ,if host mode->online,clear all plan
                        if(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        {
                            ObjectManager.PlanManager.RemoveChangePlan();
                            ObjectManager.PlanManager.RemoveChangePlanStandby();
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
        ///  6.68.	MachineControlStateChanged      MES MessageSet : Reports BC control state has been changed
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="controlStatus">control state ‘OFFLINE’ ‘REMOTE’ ‘LOCAL’ </param>
        public void MachineControlStateChanged_FirstRun(string trxID, string lineName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("Can't find Line ID =[{0}] in LineEntity!", lineName));

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(lineName);

                bodyNode[keyHost.CONTROLSTATENAME].InnerText = line.File.HostMode.ToString();
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);
                SendToOEE(xml_doc);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={0}] [BCS -> MES][{1}] Control Mode Change To {2}", ObjectManager.LineManager.GetLineID(lineName), trxID, line.File.HostMode.ToString()));



                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    Thread.Sleep(2000);
                    MachineDataUpdate(trxID, line);
                    Thread.Sleep(1500);
                    PortDataUpdate(trxID, line);
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID)) //Jun Modify 20141223 By Line取得EQPs
                    {
                        //20150119 cy : 增加TBPHL Line Mask
                        if (!line.Data.LINEID.Contains("TCPHL"))
                            MaskStateChanged_OnLine(trxID, eqp);  //if Mask Machine is mounted Report else not report
                        else
                            MaskStateChanged_OnLine_TCPHL(trxID, eqp);

                        //if (!line.Data.LINEID.Contains("CCPOL"))//MES spec 1.21 remove shihyang
                            MaterialStateChanged_OnLine(this.CreateTrxID(), eqp);
                        //else
                        //    POLStateChanged_OnLine(this.CreateTrxID(), eqp);
                    }

                    //t3 MES Msg Add 2016/01/08 cc.kuang
                    Thread.Sleep(1000);
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                        IndexerOperModeChanged(UtilityMethod.GetAgentTrackKey(), line.Data.LINEID ,"GLASS_CHANGER");
                    else
                        IndexerOperModeChanged(UtilityMethod.GetAgentTrackKey(), line.Data.LINEID , "NORMAL");
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.69.	MachineModeChanged(delete)          MES MessageSet : Reports when EQP operation mode has been changed.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="operMode">EquipmentEntity 有變動的機台</param>
        public void MachineModeChanged(string trxID, string lineName, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(lineName)));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineModeChanged") as XmlDocument;
                GetTransactionID(xml_doc);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                // Changer Line 沒有轉換的觸發時機點(ex. RunModeChange, IndexerOperationModeChange..)
                if (line.Data.LINETYPE == eLineType.ARRAY.CHN_SEEC)
                    bodyNode[keyHost.LINEOPERMODE].InnerText = eMES_LINEOPERMODE.CHANGER;
                else
                    bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode;

                XmlNode machineNodeList = bodyNode[keyHost.MACHINELIST];
                XmlNode machineClone = machineNodeList[keyHost.MACHINE].Clone();
                machineNodeList.RemoveAll();

                XmlNode machine = machineClone.Clone();
                machine[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                machine[keyHost.LINEOPERMODE].InnerText = eqp.File.EquipmentRunMode;

                XmlNode unitNodeList = machine[keyHost.UNITLIST];
                XmlNode unitClone = unitNodeList[keyHost.UNIT].Clone();
                unitNodeList.RemoveAll();

                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                foreach (Unit unit in units)
                {
                    XmlNode uNode = unitClone.Clone();
                    uNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unit.Data.UNITID);
                    uNode[keyHost.LINEOPERMODE].InnerText = unit.File.RunMode;
                    unitNodeList.AppendChild(uNode);
                }

                machineNodeList.AppendChild(machine);

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
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
        ///  6.70.	MachineModeChangeRequest        MES MessageSet : BC requests MES to validate changing machine operation mode is valid or not in Boxing equipment.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void MachineModeChangeRequest(string trxID, string lineName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={0}] [BCS -> MES][{1}] {2} Send MES but OFF LINE LINENAME=[{1}].",
                        //lineName, trxID, MethodBase.GetCurrentMethod().Name));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineModeChangeRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                // Changer Line 沒有轉換的觸發時機點(ex. RunModeChange, IndexerOperationModeChange..)
                if (line.Data.LINETYPE == eLineType.ARRAY.CHN_SEEC)
                {
                    if (line.File.LineOperMode == eMES_LINEOPERMODE.CHANGER || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE) //add for Exchange Mode, cc.kuang 2016/02/23
                        bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode;
                    else
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", line.File.LineOperMode + " Mode Not Define in Array Changer Line");
                    //bodyNode[keyHost.LINEOPERMODE].InnerText = eMES_LINEOPERMODE.CHANGER;
                }
                else // add by bruce 2014/12/09 report line mix run mode
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                    {
                        bodyNode[keyHost.LINEOPERMODE].InnerText = "MIX"; //"MIXEDRUNMODE";
                    }
                    else
                    {
                        //2015/9/7 Add by Frank For UPK Line ULD EQP Run Mode
                        #region [UPK Line ULD EQP Run Mode]
                        if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                        {
                            List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineName);
                            foreach (Equipment eqp in eqps)
                            {
                                if ((eqp.Data.NODEATTRIBUTE == "UD")&&(eqp.File.EquipmentRunMode == "RE-CLEAN"))
                                {
                                    if (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.TFT)
                                        bodyNode[keyHost.LINEOPERMODE].InnerText = "TFTRECLEAN";
                                    else
                                        bodyNode[keyHost.LINEOPERMODE].InnerText = "CFRECLEAN";
                                }
                                else
                                    bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                            }
                        }
                        #endregion

                        else
                            bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                    }
                }

                XmlNode machineNodeList = bodyNode[keyHost.MACHINELIST];
                XmlNode machineClone = machineNodeList[keyHost.MACHINE].Clone();

                machineNodeList.RemoveAll();

                if (fabType != eFabType.CELL)
                {
                    if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        switch (line.Data.LINETYPE)
                        {                                                       
                            case eLineType.ARRAY.CVD_AKT:
                            case eLineType.ARRAY.CVD_ULVAC:
                            case eLineType.ARRAY.DRY_ICD:
                            case eLineType.ARRAY.DRY_YAC:
                            case eLineType.ARRAY.DRY_TEL:
                            case eLineType.ARRAY.ITO_ULVAC:
                            case eLineType.ARRAY.MSP_ULVAC: 
                            case eLineType.ARRAY.ELA_JSW:
                            case eLineType.CF.FCUPK_TYPE1:
                            case eLineType.CF.FCMAC_TYPE1:
                            case eLineType.CF.FCREP_TYPE1:
                            case eLineType.CF.FCREP_TYPE2:
                            case eLineType.CF.FCREP_TYPE3:
                                {
                                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineName);

                                    foreach (Equipment eqp in eqps)
                                    {
                                        XmlNode machine = machineClone.Clone();
                                        machine[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                                        //if (bodyNode[keyHost.LINEOPERMODE].InnerText == "TFTRECLEAN" || bodyNode[keyHost.LINEOPERMODE].InnerText == "CFRECLEAN")
                                        //{
                                        //    machine[keyHost.LINEOPERMODE].InnerText = bodyNode[keyHost.LINEOPERMODE].InnerText;
                                        //}
                                        //else
                                        //{
                                            machine[keyHost.LINEOPERMODE].InnerText = eqp.File.EquipmentRunMode.ToUpper();
                                        //}
                                        XmlNode unitNodeList = machine[keyHost.UNITLIST];
                                        XmlNode unitClone = unitNodeList[keyHost.UNIT].Clone();

                                        unitNodeList.RemoveAll();

                                        if (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                                            line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                                            line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                                            line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                                            line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL ||
                                            line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC ||
                                            line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC ||
                                            line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                                        {
                                            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);

                                            foreach (Unit unit in units)
                                            {
                                                XmlNode uNode = unitClone.Clone();
                                                uNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unit.Data.UNITID);
                                                if (unit.Data.UNITTYPE == "CHAMBER")
                                                {
                                                    if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC)
                                                        uNode[keyHost.LINEOPERMODE].InnerText = unit.File.ChamberRunMode.Trim();
                                                    else
                                                        uNode[keyHost.LINEOPERMODE].InnerText = unit.File.RunMode.ToUpper();
                                                }
                                                unitNodeList.AppendChild(uNode);
                                            }
                                        }
                                        machineNodeList.AppendChild(machine);
                                    }
                                }
                                break;
                        }
                    }
                }

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

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
        /// 6.71.	MachineModeChangeReply      MES MessagetSet : Machine Status Change Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MachineModeChangeReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string lineopermode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINEOPERMODE].InnerText;
                string timestamp = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.TIMESTAMP].InnerText;
                string result = ""; //xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                //machine list
                XmlNodeList machineList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINELIST].ChildNodes;
                foreach (XmlNode node in machineList)
                {
                    string machineName = node["MACHINENAME"].InnerText.Trim();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqp.Data.NODENO));

                    //check timer
                    string timerId = string.Format("SecsEquipmentModeChange_{0}_{1}_MachineModeChangeRequest", eqp.Data.NODENO, trxID);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachineModeChangeReply  LINENAME=[{1}],LINEOPERMODE=[{2}],VALIRESULT=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                    trxID, lineName, lineopermode, result, returnCode, returnMessage));

                        //get timer by key
                        UserTimer timer = Timermanager.GetAliveTimer(timerId);
                        if (timer != null)
                        {
                            Tuple<string, string, string, List<ChamberRunModeChanged.CHAMBERc>> tuple = timer.State as Tuple<string, string, string, List<ChamberRunModeChanged.CHAMBERc>>;
                            if (tuple != null)
                            {
                                string systembyte = tuple.Item1;
                                string header = tuple.Item2;
                                string lineopmode = tuple.Item3;
                                List<ChamberRunModeChanged.CHAMBERc> cbmodes = tuple.Item4;
                                Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge",
                                    new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxID, systembyte, (byte)(returnCode != "0" ? 1 : 0) });
                                if (returnCode != "0")
                                {
                                      Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "MES Reply EquipmentModeChange NG", trxID, string.Empty });
                                }
                                else
                                {
                                      //20151216 cy:如果有Chamber,發ChamberRunModeChange
                                      if (cbmodes != null && cbmodes.Count > 0)
                                      {
                                            ChamberRunModeChanged(base.CreateTrxID(), eqp.Data.LINEID, lineopermode, eqp.Data.NODEID, cbmodes);
                                      }
                                }
                            }
                        }
                        Timermanager.TerminateTimer(timerId);
                        continue;
                    }
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachineModeChangeReply NG LINENAME=[{1}],LINEOPERMODE=[{2}],VALIRESULT=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), lineopermode, result, returnCode, returnMessage));
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachineModeChangeReply  OK LINENAME=[{1}],LINEOPERMODE=[{2}],VALIRESULT=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), lineopermode, result, returnCode, returnMessage));
                    //to do?

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.72.	MachinePauseCommandRequest(delete)      MES MessagetSet : Machine Pause Command Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MachinePauseCommandRequest(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string mespausecmd = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PAUSECOMMAND].InnerText;
                //string unitName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.UNITNAME].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachinePauseCommandRequest  NG LINENAME=[{1}],MACHINENAME=[{2}],PAUSECOMMAND=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, lineName, machineName, mespausecmd, returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachinePauseCommandRequest  OK LINENAME=[{1}],MACHINENAME=[{2}],PAUSECOMMAND=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                        trxID, lineName, machineName, mespausecmd, returnCode, returnMessage));

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);

                    if (eqp == null)
                    {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachinePauseCommandRequest  EQP is Null, LINENAME=[{1}],MACHINENAME=[{2}],PAUSECOMMAND=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                                trxID, lineName, machineName, mespausecmd, returnCode, returnMessage));
                    }

                    //Unit unit = ObjectManager.UnitManager.GetUnit(unitname);
                    //if (unit == null)
                    //{
                    //    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachinePauseCommandRequest  Unit is null, LINENAME=[{1}],MACHINENAME=[{2}],PAUSECOMMAND=[{3}],CODE=[{4}],MESSAGE=[{5}].",
                    //            trxID, lineName, machineName, pausecmd, returnCode, returnMessage));
                    //}
                    //string unitno = unit.Data.UnitNo;
                    string unitno = "0";
                    //"1：Pause
                    //2：Resume"

                    string pausecmd = "2";
                    if (mespausecmd == "Y")
                        pausecmd = "1";
                    //eqp.File.CIMMode = eBitResult.ON;
                    //ProcessPauseCommand
                    //MES_ProcessPauseCommand(string trxid, string eqpNo, string processPause, string unitNo)
                    Invoke(eServiceName.EquipmentService, "MES_ProcessPauseCommand", new object[] { eqp.Data.NODENO, pausecmd, unitno, trxID });
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.73.	MachinePauseCommandReply(delete)        MES MessageSet :  EQP Execute Pause Command Result Reply to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodeno">Node No</param>
        /// <param name="mespausecmd">MES Pause command Y: Pause N: release pause</param>
        /// <param name="exeresult">“Y”: EQ execute ok;  “N”: EQ execute failure;</param>
        public void MachinePauseCommandReply(string trxID, Equipment eqp, string mespausecmd, string exeresult)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachinePauseCommandReply Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINENAME));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachinePauseCommandReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.PAUSECOMMAND].InnerText = mespausecmd; //Y: Pause N: release pause
                bodyNode[keyHost.EXERESULT].InnerText = exeresult; //“Y”: EQ execute ok;  “N”: EQ execute failure;

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}],EXERESULT =[{2}].",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID), exeresult));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.74.	MachineSampleRateChangeReport       取消MES MessageSet : BC report the sampling frequency, the need to record in MES. CFM click on EQ,
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodeno">Node No</param>
        /// <param name="samplingrate">SAMPLE RATE</param>
        //public void MachineSampleRateChangeReport(string trxID, string lineName,Equipment eqp,string samplingrate)
        //{
        //    try
        //    {
        //        Line line = ObjectManager.LineManager.GetLine(lineName);
        //        if (line.File.HostMode == eHostMode.OFFLINE)
        //        {
        //            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineSampleRateChangeReport Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
        //            return;
        //        }
        //        IServerAgent agent = GetServerAgent();
        //        XmlDocument xml_doc = agent.GetTransactionFormat("MachineSampleRateChangeReport") as XmlDocument;
        //        SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
        //        XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

        //        bodyNode[keyHost.LINENAME].InnerText = lineName;
        //        //to do 
        //        bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
        //        bodyNode[keyHost.SAMPLERATE].InnerText = samplingrate;

        //        SendToMES(xml_doc);

        //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
        //            trxID, lineName));
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        ///  6.75.	MachineStateChanged     MES MessageSet : Reports when EQP states has been changed.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="EQPID">Node ID</param>
        /// <param name="eqpStatus">‘IDLE’,‘RUN’,‘DOWN’ </param>
        /// <param name="alarmID">eqpStatus is Down report</param>
        /// <param name="alarmtext">eqpStatus is Down report</param>
        /// <param name="alarmTime">eqpStatus is Down report</param>
        /// <param name="materialflag">Material Status change is True</param>
        public void MachineStateChanged(string trxID, Equipment eqp, string alarmID, string alarmtext, string alarmTime)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different.
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineStateChanged Send MES but OFF LINE LINENAME=[{1}].",
                //            //trxID, eqp.Data.LINEID));
                //            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(eqp.Data.LINEID);
                if (eqp.Data.NODEATTRIBUTE == "ATTACHMENT")    // add by bruce 20160217 Array 附屬設備固定用 NODENO = LA
                    bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                else
                    bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                bodyNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus;
                bodyNode[keyHost.MATERIALCHANGEFLAG].InnerText = line.File.Array_Material_Change ? "Y" : "N";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                //Modify By James at 2014/09/28
                if (eqp.File.MESStatus == "DOWN")
                {
                    bodyNode[keyHost.ALARMCODE].InnerText = alarmID;
                }
                else
                {
                    bodyNode[keyHost.ALARMCODE].InnerText = string.Empty;
                }
                bodyNode[keyHost.ALARMTEXT].InnerText = alarmtext;
                bodyNode[keyHost.ALARMTIMESTAMP].InnerText = alarmTime;
                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";

                //Jun Add 20141211 For CIM,MES需求再修改一次
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2)
                    {
                        List<Cassette> cstList = ObjectManager.CassetteManager.GetCassettes().Where(c => c.CrossLineFlag == "Y").ToList<Cassette>();
                        if (cstList != null)
                        {
                            foreach (Cassette cst in cstList)
                            {
                                List<Port> portList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteID == cst.CassetteID).ToList<Port>();
                                if (portList != null)
                                {
                                    if (portList.Count > 0)
                                    {
                                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = "Y";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    if (ParameterManager["MESSYNCREPORT"].GetBoolean()) //上报MES 是否
                        SendToQueue(xml_doc); //先放到Queue中
                    else
                        SendToMES(xml_doc);

                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                    ObjectManager.LineManager.GetLineID(eqp.Data.LINEID), trxID, ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), eqp.File.Status.ToString()));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherLine = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherLine.File.HostMode != eHostMode.OFFLINE)
                    {
                        if (ParameterManager["MESSYNCREPORT"].GetBoolean()) //上报MES 是否
                            SendToQueue(xml_doc); //先放到Queue中
                        else
                            SendToMES(xml_doc);
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        bodyNode[keyHost.LINENAME].InnerText, trxID, bodyNode[keyHost.MACHINENAME].InnerText, eqp.File.Status.ToString()));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }

                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.75.	MachineStateChanged     
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqp"></param>
        public void MachineStateChanged_WaitCassetteState(string trxID, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different.
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineStateChanged Send MES but OFF LINE LINENAME=[{1}].",
                //            //trxID, eqp.Data.LINEID));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(eqp.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.WaitCassetteStatus.ToString();

                bodyNode[keyHost.MATERIALCHANGEFLAG].InnerText = line.File.Array_Material_Change ? "Y" : "N";
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                //Modify By James at 2014/09/28
                bodyNode[keyHost.ALARMCODE].InnerText = string.Empty;
                bodyNode[keyHost.ALARMTEXT].InnerText = string.Empty;
                bodyNode[keyHost.ALARMTIMESTAMP].InnerText = string.Empty;
                bodyNode[keyHost.CROSSLINEFLAG].InnerText = "N";

                //Jun Add 20141211 For CIM,MES需求再修改一次
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2)
                    {
                        List<Cassette> cstList = ObjectManager.CassetteManager.GetCassettes().Where(c => c.CrossLineFlag == "Y").ToList<Cassette>();
                        if (cstList != null)
                        {
                            foreach (Cassette cst in cstList)
                            {
                                List<Port> portList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteID == cst.CassetteID).ToList<Port>();
                                if (portList != null)
                                {
                                    if (portList.Count > 0)
                                    {
                                        bodyNode[keyHost.CROSSLINEFLAG].InnerText = "Y";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                //SendToMES(xml_doc);
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToQueue(xml_doc);
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO), eqp.File.Status.ToString()));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                }

                #region Watson Add 20150319 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToQueue(xml_doc);
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> MES] [{1}] Send MachineStateChange MachineName=[{2}],EQP State =[{3}].",
                        bodyNode[keyHost.LINENAME].InnerText, trxID, bodyNode[keyHost.MACHINENAME].InnerText, eqp.File.Status.ToString()));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.82.	MachineSiteChangeRequest
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="job"></param>
        public void MachineSiteChangeRequest(string trxID, string lineName, string newSite, string machineEnable, string user)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineSiteChangeRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineSiteChangeRequest") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //To do?
                //Add for T3 MES
                bodyNode[keyHost.NEWSITE].InnerText = newSite;
                bodyNode[keyHost.MACHINEENABLE].InnerText = machineEnable;
                bodyNode[keyHost.EVENTUSER].InnerText = user;

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
        /// 6.83.	MachineSiteChangeReply
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_MachineSiteChangeReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //to Do
                string newsite = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.NEWSITE].InnerText;
                string machineenable = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINEENABLE].InnerText;
                string eventusesr = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.EVENTUSER].InnerText;
                string valiresult = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.VALIRESULT].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachineSiteChangeReply NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                    //to do?

                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] MachineSiteChangeReply OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
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
        /// 6.94.	MachineControlStateChangeRequest        MES MessagetSet : EQP Control Status Change Request to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MachineControlStateChangeRequest(XmlDocument xmlDoc)
        {
            try
            {
                string err = string.Empty;
                string lineName = GetLineName(xmlDoc);
                string trxID = GetTransactionID(xmlDoc);
                string inboxname = GetMESINBOXName(xmlDoc);
                string controlState = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CONTROLSTATENAME].InnerText;

                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line != null)
                {
                    #region LineService CheckControlCondition
                    string strResult = Invoke("LineService", "CheckControlCondition",
                            new object[] { trxID, line.Data.LINEID, controlState, err },
                            new Type[] { typeof(string), typeof(string), typeof(string), typeof(string).MakeByRefType() }).ToString();
                    //err 目前不會用到
                    #endregion

                    MachineControlStateChangeReply(trxID, line.Data.LINEID, controlState, strResult, inboxname);

                    if (strResult == "Y")
                    {
                        if (line.File.HostMode == eHostMode.OFFLINE) //OFFLINE -> ONLINE or LOCAL
                        {
                            MachineControlStateChanged(trxID, line.Data.LINEID, controlState);
                        }
                        else //LOCAL -> REMOTE or REMOTE -> LOCAL
                        {
                            MachineControlStateChanged(trxID, line.Data.LINEID, controlState);
                        }
                    }

                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[LINENAME={0}] [BCS <-MES ] [{1}] MES Change Control State =[{2}].",
                          ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, controlState));

                    #region OPI Service
                    Invoke(eServiceName.UIService, "LineStatusReport", new object[] { trxID, line });
                    #endregion
                }
                else
                {
                    MachineControlStateChangeReply(trxID, lineName, controlState, "N", inboxname); //BC cannot change to online
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[LINENAME={0}] [BCS <-MES ] [{1}] MES Change Control State =[{2}], mismatch Line =[{3}].",
                          ServerName, trxID, controlState, lineName));
                          //ServerName, trxID, controlState, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                }


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>
        ///  6.95.	MachineControlStateChangeReply      MES MessageSet : EQP Control Status Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="ctlStatus">‘OFFLINE’ ‘REMOTE’‘LOCAL’ </param>
        /// <param name="ackResult">If BC accepts the request ackResult is ‘Y’),</param>
        public void MachineControlStateChangeReply(string trxID, string lineName, string ctlStatus, string ackResult, string inboxname)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineControlStateChangeReply") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                SetINBOXNAME(xml_doc, inboxname);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.CONTROLSTATENAME].InnerText = ctlStatus;
                bodyNode[keyHost.ACKNOWLEDGE].InnerText = ackResult;

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
        ///  6.96.	MachineDataUpdate       MES MessageSet : Online Sequence Report All EQP Data
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        public void MachineDataUpdate(string trxID, Line line)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] MachineDataUpdate Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineDataUpdate") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                //2015/9/7 Add by Frank For UPK Line ULD EQP Run Mode
                #region [UPK Line ULD EQP Run Mode]
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                {
                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                    foreach (Equipment eqp in eqps)
                    {
                        if ((eqp.Data.NODEATTRIBUTE == "UD") && (eqp.File.EquipmentRunMode == "RE-CLEAN"))
                        {
                            if (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.TFT)
                                bodyNode[keyHost.LINEOPERMODE].InnerText = "TFTRECLEAN";
                            else
                                bodyNode[keyHost.LINEOPERMODE].InnerText = "CFRECLEAN";
                        }
                        else
                            bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();
                    }
                }
                #endregion
                else
                    bodyNode[keyHost.LINEOPERMODE].InnerText = line.File.LineOperMode.ToUpper();

                bodyNode[keyHost.LINESTATENAME].InnerText = line.File.Status.ToString();
                bodyNode[keyHost.MATERIALCHANGEFLAG].InnerText = "N";

                bodyNode[keyHost.PARTIALFULLMODE].InnerText = "NORMAL"; //‘NORMAL’ – Not Partial Full mode.    
                //watson add 20141118 For MES New Spec
                bodyNode[keyHost.CFSHORTCUTMODE].InnerText = (line.File.CFShortCutMode == eShortCutMode.Enable ? "ENABLE" : "DISABLE");

                if (ObjectManager.EquipmentManager.GetEQPs().Any(e => e.File.PartialFullMode == eEnableDisable.Enable))
                    bodyNode[keyHost.PARTIALFULLMODE].InnerText = "PARTIAL";

                XmlNode specEqpListNode = bodyNode[keyHost.MACHINELIST];
                XmlNode specEqpNodeClone = specEqpListNode[keyHost.MACHINE].Clone();
                XmlNode specUnitListNode = specEqpNodeClone[keyHost.UNITLIST];
                XmlNode specUnitNodeClone = specUnitListNode[keyHost.UNIT].Clone();
                specUnitListNode.RemoveAll();
                specEqpNodeClone.RemoveChild(specUnitListNode);  //Watson Modify 20141128 For MES要求，沒有unit也要有UnitList
                specEqpListNode.RemoveAll();

                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))
                {
                    #region Watson Add 20150313 For PMT Line PTI100的特殊上報規則
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                    {
                        Equipment eqp2 = ObjectManager.EquipmentManager.GetEQP("L2");
                        XmlNode eqpNode2 = specEqpNodeClone.Clone();
                        eqpNode2[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        eqpNode2[keyHost.MACHINESTATENAME].InnerText = eqp2.File.MESStatus;
                        eqpNode2[keyHost.MATERIALCHANGEFLAG].InnerText = "N";

                        if (eqp2.File.CurrentAlarmCode.Trim() != string.Empty)
                        {
                            HappeningAlarm alarm = ObjectManager.AlarmManager.GetAlarm(eqp2.Data.NODENO, eqp2.File.CurrentAlarmCode);
                            if (alarm != null)
                            {
                                eqpNode2[keyHost.ALARMCODE].InnerText = alarm.Alarm.ALARMCODE;
                                eqpNode2[keyHost.ALARMTEXT].InnerText = alarm.Alarm.ALARMTEXT;
                                eqpNode2[keyHost.ALARMTIMESTAMP].InnerText = alarm.OccurDateTime.ToString();
                            }
                        }

                        eqpNode2[keyHost.LINEOPERMODE].InnerText = eqp2.File.EquipmentRunMode;
                        eqpNode2[keyHost.BCSERIALNO].InnerText = eqp2.Data.NODENO;
                        eqpNode2[keyHost.BCRECIPEIDLENGTH].InnerText = eqp2.Data.RECIPELEN.ToString();

                        XmlNode uniListNode2 = specUnitListNode.Clone();
                        XmlNode unitNode2 = null;
                        foreach (Unit unit2 in ObjectManager.UnitManager.GetUnitsByEQPNo(eqp2.Data.NODENO))
                        {
                            unitNode2 = specUnitNodeClone.Clone();
                            HappeningAlarm alarm = ObjectManager.AlarmManager.GetAlarm(eqp2.Data.NODENO, unit2.Data.UNITNO, eqp2.File.CurrentAlarmCode);
                            if (alarm != null)
                            {
                                unitNode2[keyHost.ALARMCODE].InnerText = alarm.Alarm.ALARMCODE;
                                unitNode2[keyHost.ALARMTEXT].InnerText = alarm.Alarm.ALARMTEXT;
                                unitNode2[keyHost.ALARMTIMESTAMP].InnerText = alarm.OccurDateTime.ToString();
                            }
                            unitNode2[keyHost.UNITNAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                            unitNode2[keyHost.UNITSTATENAME].InnerText = unit2.File.MESStatus;
                            unitNode2[keyHost.MATERIALCHANGEFLAG].InnerText = "N";
                        }
                        uniListNode2.AppendChild(unitNode2);

                        eqpNode2.AppendChild(uniListNode2);
                        specEqpListNode.AppendChild(eqpNode2);
                    }
                    #endregion

                    if (eqp.Data.NODEATTRIBUTE != "CV5")
                    {
                        if (line.Data.FABTYPE == "CF") //20160304 modify by Frank
                        {
                            if(eqp.Data.NODEATTRIBUTE == "LD")
                                bodyNode[keyHost.CSTOPERMODE].InnerText = eqp.File.CSTOperationMode.ToString();
                        }
                        else
                            bodyNode[keyHost.CSTOPERMODE].InnerText = eqp.File.CSTOperationMode.ToString();
                        XmlNode eqpNode = specEqpNodeClone.Clone();
                        eqpNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                        eqpNode[keyHost.MACHINESTATENAME].InnerText = eqp.File.MESStatus;
                        eqpNode[keyHost.MATERIALCHANGEFLAG].InnerText = "N";

                        if (eqp.File.CurrentAlarmCode.Trim() != string.Empty)
                        {
                            HappeningAlarm alarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, eqp.File.CurrentAlarmCode);
                            if (alarm != null)
                            {
                                eqpNode[keyHost.ALARMCODE].InnerText = alarm.Alarm.ALARMCODE;
                                eqpNode[keyHost.ALARMTEXT].InnerText = alarm.Alarm.ALARMTEXT;
                                eqpNode[keyHost.ALARMTIMESTAMP].InnerText = alarm.OccurDateTime.ToString();
                            }
                        }

                        eqpNode[keyHost.LINEOPERMODE].InnerText = eqp.File.EquipmentRunMode;  
                        eqpNode[keyHost.BCSERIALNO].InnerText = eqp.Data.NODENO;
                        eqpNode[keyHost.BCRECIPEIDLENGTH].InnerText = eqp.Data.RECIPELEN.ToString();

                        XmlNode uniListNode = specUnitListNode.Clone();
                        XmlNode unitNode = null;
                        foreach (Unit unit in ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO))
                        {
                            if (unit.Data.UNITATTRIBUTE == "VIRTUAL") // Virtual 的UNIT 无需上报MES
                                continue;
                            unitNode = specUnitNodeClone.Clone();
                            HappeningAlarm alarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, unit.Data.UNITNO, eqp.File.CurrentAlarmCode);
                            if (alarm != null)
                            {
                                unitNode[keyHost.ALARMCODE].InnerText = alarm.Alarm.ALARMCODE;
                                unitNode[keyHost.ALARMTEXT].InnerText = alarm.Alarm.ALARMTEXT;
                                unitNode[keyHost.ALARMTIMESTAMP].InnerText = alarm.OccurDateTime.ToString();
                            }
                            unitNode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnitID(eqp.Data.NODENO, unit.Data.UNITID);
                            unitNode[keyHost.UNITSTATENAME].InnerText = unit.File.MESStatus;
                            unitNode[keyHost.MATERIALCHANGEFLAG].InnerText = "N";

                            unitNode[keyHost.CHAMBERRUNMODE].InnerText = unit.File.ChamberRunMode;   //Modify by marine for MES 2015/7/10

                            switch (line.Data.LINETYPE) //for HSMS chamber mode keep in RunMode
                            {
                                case eLineType.ARRAY.CVD_ULVAC:
                                case eLineType.ARRAY.CVD_AKT:
                                case eLineType.ARRAY.DRY_YAC:
                                case eLineType.ARRAY.DRY_ICD:
                                case eLineType.ARRAY.DRY_TEL:
                                case eLineType.ARRAY.MSP_ULVAC:
                                case eLineType.ARRAY.ITO_ULVAC:
                                    if (unit.Data.UNITTYPE == "CHAMBER")
                                    {
                                        if (unit.File.ChamberRunMode.Trim().Length > 0)
                                            unitNode[keyHost.CHAMBERRUNMODE].InnerText = unit.File.ChamberRunMode;
                                        else
                                            unitNode[keyHost.CHAMBERRUNMODE].InnerText = unit.File.RunMode;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (line.Data.LINETYPE)
                            {
                                case eLineType.ARRAY.CVD_ULVAC:
                                case eLineType.ARRAY.CVD_AKT:
                                case eLineType.ARRAY.DRY_YAC:
                                case eLineType.ARRAY.DRY_ICD:
                                case eLineType.ARRAY.DRY_TEL:
                                    if (unit.Data.UNITTYPE == "CHAMBER") unitNode[keyHost.LINEOPERMODE].InnerText = unit.File.RunMode.ToUpper();
                                    break;
                                default:
                                    unitNode[keyHost.LINEOPERMODE].InnerText = unit.File.RunMode.ToUpper();   //eqp.File.EquipmentOperationMode.ToString(); Watson modify 20141120
                                    break;
                            }
                            uniListNode.AppendChild(unitNode);
                        }
                        eqpNode.AppendChild(uniListNode);
                        specEqpListNode.AppendChild(eqpNode);
                    }
                }
                //bodyNode.AppendChild(specEqpListNode);
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
        /// 6.99.	MachineInspectionOverRatio      Inspection EQ by pass glass more than Ratio,BC report to MES,for CFM Alarm.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="machineName">Equipment identification or Equipment number</param>
        /// <param name="overResult">Over Result</param>
        /// <param name="timeStamp">Time Stamp</param>
        public void MachineInspectionOverRatio(string trxID, string lineName, string machineName, string overResult)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MachineInspectionOverRatio Send MES but OFF LINE LINENAME =[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                // CF Photo Line 機台　L19_CV5　不上報給　MES.  Add Kasim 20150318 
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (line.Data.FABTYPE == eFabType.CF.ToString() && eqp.Data.NODEATTRIBUTE == "CV5") return;

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineInspectionOverRatio") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.OVERRESULT].InnerText = overResult;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

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
        /// 6.100.	MachineLoginReport      User Login machine,BC report to mes for CFM.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="machineName">Equipment identification or Equipment number</param>
        /// <param name="loginUserID">SPEC無提供說明</param>
        /// <param name="timeStamp">‘yyyy-MM-dd HH:mm:ss.fff’ – TIMESTAMP format.</param>
        public void MachineLoginReport(string trxID, string lineName, string machineName, string loginUserID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);

                //Watson Modify 20150319 For PMT LINE 2 line HOST MODE Different. 改成送出前判斷
                //if (line.File.HostMode == eHostMode.OFFLINE)
                //{
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MachineLoginReport Send MES but OFF LINE LINENAME =[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                //    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                //    //return;
                //}

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MachineLoginReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.LOGINUSERID].InnerText = loginUserID;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                //to do 
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    SendToMES(xml_doc);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                        trxID, bodyNode[keyHost.LINENAME].InnerText));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                }
                #region Watson Add 20150313 For PMT Line Send 2 Time Loader Status.
                if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (eqp.Data.NODENO == "L2"))
                {
                    bodyNode[keyHost.LINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString();
                    bodyNode[keyHost.MACHINENAME].InnerText = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                    Line otherline = ObjectManager.LineManager.GetLine(ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString());
                    if (otherline.File.HostMode != eHostMode.OFFLINE)
                    {
                        SendToMES(xml_doc);

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                            trxID, bodyNode[keyHost.LINENAME].InnerText));
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, bodyNode[keyHost.LINENAME].InnerText, trxID, MethodBase.GetCurrentMethod().Name));
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.113.	PortAccessModeChanged       MES MessageSet : All or 2 以上 Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="Port">Port IList, 也可以傳單一port</param>
        public void PortAccessModeChanged(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortAccessModeChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = _port.Data.PORTID;
                    if (_port.File.TransferMode == ePortTransferMode.Unknown)
                    {
                        portNode[keyHost.PORTACCESSMODE].InnerText = "";
                    }
                    portNode[keyHost.PORTACCESSMODE].InnerText = ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value;
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLine(line.Data.LINEID).Data.LINEID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.113.	PortAccessModeChanged       MES MessageSet :  Port Acess Mode(MGV/AGV) Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="Port">Port</param>
        public void PortAccessModeChanged(string trxID, string lineName, Port _port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortAccessModeChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                XmlNode portNode = portCloneNode.Clone();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
                portListNode.AppendChild(portNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortAccessModeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.114.	PortCarrierSetCodeChanged       MES MessageSet :  Reports Port Carrier Setting Code to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="port">Port Entity</param>
        public void PortCarrierSetCodeChanged(string trxID, string lineName, IList<Port> portList)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortCarrierSetCodeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortCarrierSetCodeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                //to do 
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                //單一port
                foreach (Port _port in portList)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.CARRIERSETCODE].InnerText = _port.File.CassetteSetCode;
                    portListNode.AppendChild(portNode);
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
        /// 6.115.	PortDataUpdate      MES MessageSet : On line Seqence Port Data Update Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="node">Node Entity</param>
        public void PortDataUpdate(string trxID, Line line)
        {
            try
            {
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortDataUpdate Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortDataUpdate") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                #region CELL PMT line 要報全部的port
                //Watson Add 20141223 For 雙Line邏輯
                if (ServerName.Contains(eLineType.CELL.CBPMT))
                {
                    foreach (Port _port in ObjectManager.PortManager.GetPorts())
                    {

                        XmlNode portNode = portCloneNode.Clone();
                        portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                        portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID;

                        portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
                        //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade
                        if (CheckCellLine(line, _port))
                            portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                        else
                            portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][(int)_port.File.Mode].Value;

                        portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)_port.File.Type].Value;

                        ///PMT Line Special 做法
                        if (_port.Data.LINEID == line.Data.LINEID)
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)_port.File.EnableMode].Value;
                        else
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)ePortEnableMode.Disabled].Value;

                        portNode[keyHost.PORTTRANSFERSTATE].InnerText = ConstantManager["MES_PORTTRANSFERSTATE"][(int)_port.File.Status].Value;
                        portNode[keyHost.PORTOPERMODE].InnerText = ConstantManager["MES_PORTOPERMODE"][(int)_port.File.OperMode].Value;
                        portNode[keyHost.CARRIERSETCODE].InnerText = _port.File.CassetteSetCode;

                        portListNode.AppendChild(portNode);
                    }
                }
                #endregion
                else
                {
                    foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))  //Jun Modify 20141222 By Line 取得EQP List
                    {
                        foreach (Port _port in ObjectManager.PortManager.GetPorts(eqp.Data.NODEID))
                        {
                            XmlNode portNode = portCloneNode.Clone();
                            portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                            portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID;

                            portNode[keyHost.PORTACCESSMODE].InnerText = _port.File.TransferMode != ePortTransferMode.Unknown ? ConstantManager["MES_PORTACCESSMODE"][((int)_port.File.TransferMode).ToString()].Value : string.Empty;
                            //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade
                            if (CheckCellLine(line, _port))
                                portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                            else
                                portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][(int)_port.File.Mode].Value;

                            portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][(int)_port.File.Type].Value;
                            portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)_port.File.EnableMode].Value;
                            portNode[keyHost.PORTTRANSFERSTATE].InnerText = ConstantManager["MES_PORTTRANSFERSTATE"][(int)_port.File.Status].Value;
                            portNode[keyHost.PORTOPERMODE].InnerText = ConstantManager["MES_PORTOPERMODE"][(int)_port.File.OperMode].Value;
                            portNode[keyHost.CARRIERSETCODE].InnerText = _port.File.CassetteSetCode;
                            if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))       //zhuxingxing add 20160819  MES 需求
                            {
                                if (_port.File.Mode == ePortMode.ReJudge) portNode[keyHost.PORTUSETYPE].InnerText = "FP";
                                if (_port.File.Mode == ePortMode.Rework) portNode[keyHost.PORTUSETYPE].InnerText = "CG";
                            }

                            portListNode.AppendChild(portNode);
                        }
                    }
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortDataUpdate OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.116.	PortEnableChanged       MES MessageSet : All or 以上 Port Enable Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="Port">Port IList, 也可以傳單一port</param>
        public void PortEnableChanged(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortEnableChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][((int)_port.File.EnableMode).ToString()].Value;
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.116.	PortEnableChanged       MES MessageSet : Port Enable Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="Port">Port</param>
        public void PortEnableChanged(string trxID, string lineName, Port _port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortEnableChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                XmlNode portNode = portCloneNode.Clone();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][((int)_port.File.EnableMode).ToString()].Value;
                portListNode.AppendChild(portNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged OK LINENAME=[{1}].",
                        trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.116.	PortEnableChanged
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="dicPort"></param>
        public void PortEnableChangedByPMT(string trxID, string lineName, Dictionary<Port, ePortEnableMode> dicPort)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortEnableChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in dicPort.Keys)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTENABLEFLAG].InnerText = ConstantManager["MES_PORTENABLEFLAG"][(int)dicPort[_port]].Value;
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortEnableChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.117.	PortOperModeChanged     MES MessageSet  : Port Oper Mode Change Report to MES.
        /// between Packing Mode and Unpacking Mode in Palletizing equipment.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="portlist">Port List</param>
        public void PortOperModeChanged(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortOperModeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortOperModeChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTOPERMODE].InnerText = _port.File.OperMode.ToString();
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortOperModeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.118.	PortOperModeChangeRequest       MES MessageSet  : All or 2 以上 Port Oper Mode Change Request to MES.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port IList, 也可以傳單一port</param>
        public void PortOperModeChangeRequest(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortOperModeChangeRequest Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortOperModeChangeRequest") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTOPERMODE].InnerText = ConstantManager["MES_PORTOPERMODE"][(int)_port.File.OperMode].Value;
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortOperModeChangeRequest OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.119.	PortOperModeChangeReply     MES MessageSet :MES sends validation result of changing port operation mode to BC in Palletizing equipment.
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_PortOperModeChangeReply(XmlDocument xmlDoc)
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
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxID, lineName));
                }

                //Watson modify 20141229 只是記Log
                //string port1id = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTLIST][keyHost.PORT][keyHost.PORTNAME].InnerText;
                //string port1mode = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTLIST][keyHost.PORT][keyHost.PORTOPERMODE].InnerText;

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] PortOperModeChangeReply  NG LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES][{0}] PortOperModeChangeReply  OK LINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                        trxID, lineName, returnCode, returnMessage));

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.120.	PortTransferStateChanged        MES MessageSet : Port (All or 2 以上)Transfer States Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port IList, 也可以傳單一port</param>
        public void PortTransferStateChanged(string trxID, string lineName, IList<Port> portlist)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTransferStateChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode uldportListNode = bodyNode[keyHost.ULDPORTLIST];

                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                XmlNode uldportCloneNode = uldportListNode[keyHost.PORT].Clone();

                portListNode.RemoveAll();
                uldportListNode.RemoveAll();

                foreach (Port _port in portlist)
                {
                    //if (_port.File.Type == ePortType.LoadingPort)  //Port Type ="Loader"
                    //{
                    XmlNode portNode = portCloneNode.Clone();
                    //20141125 Add by Edison:"UN" Don't Report
                    if (_port.File.Status == ePortStatus.UN)
                    {
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but PORT STATUS IS (UN),PortId=[{2}].", trxID,
                            ObjectManager.LineManager.GetLineID(line.Data.LINEID), _port.Data.PORTID));
                        continue;
                    }
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTTRANSFERSTATE].InnerText = _port.File.Status.ToString();
                    //20150203 modify by edison:防住机台LR时上报CSTID不为空
                    if (_port.File.Status == ePortStatus.LR)
                    {
                        if (line.File.LineOperMode == eMES_LINEOPERMODE.CHANGER)
                            portNode[keyHost.CARRIERNAME].InnerText = _port.File.PlannedCassetteID;
                        else
                            portNode[keyHost.CARRIERNAME].InnerText = "";
                    }
                    else
                    {
                        portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();
                    }

                    // 20141218 Add Sort Rule
                    if (_port.File.Type == ePortType.UnloadingPort)
                    {
                        if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                        {
                            portNode[keyHost.PRODUCTGRADE].InnerText = _port.File.MappingGrade;

                            Job job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                                j.TargetPortID == "0" && j.JobGrade.Trim().Equals(_port.File.MappingGrade.Trim()) &&
                                j.ProductType.Value.Equals(int.Parse(_port.File.ProductType)));

                            if (job != null)
                            {
                                portNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();
                            }
                        }
                    }
                    //portNode[keyHost.PRODUCTNAME].InnerText = string.Empty;
                    //portNode[keyHost.PRODUCTGRADE].InnerText = string.Empty;
                    portListNode.AppendChild(portNode);
                    //}
                    //else
                    //{
                    //TODO: 等待MES 新的SPEC , 因為結構有改變
                    //    XmlNode uldportNode = uldportCloneNode.Clone();
                    //    uldportNode[keyHost.PORTNAME].InnerText = _port.Data.PORTID;
                    //    uldportNode[keyHost.PRODUCTNAME].InnerText = string.Empty;
                    //    uldportListNode.AppendChild(uldportNode);
                    //}
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                bodyNode.RemoveChild(uldportListNode);
                SendToOEE(xml_doc);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.120.	PortTransferStateChanged        MES MessageSet : One Port Transfer States Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port</param>
        public void PortTransferStateChanged(string trxID, string lineName, Port _port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                //20141125 Add by Edison:"UN" Don't Report
                if (_port.File.Status == ePortStatus.UN)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged Send MES but PORT STATUS IS (UN),PortId=[{2}].", trxID, lineName, _port.Data.PORTID));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTransferStateChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];

                XmlNode portNode = portListNode[keyHost.PORT].Clone();

                XmlNode uldportListNode = portListNode[keyHost.ULDPORTLIST];

                portListNode.RemoveAll();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                portNode[keyHost.PORTTRANSFERSTATE].InnerText = _port.File.Status.ToString();
                //20150203 modify by edison:防住机台LR时上报CSTID不为空
                if (_port.File.Status == ePortStatus.LR)
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        if (line.File.PlanStatus == ePLAN_STATUS.NO_PLAN)
                            portNode[keyHost.CARRIERNAME].InnerText = "";
                        else
                            portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();
                    }
                    else
                        portNode[keyHost.CARRIERNAME].InnerText = "";
                }
                else
                    portNode[keyHost.CARRIERNAME].InnerText = _port.File.CassetteID.Trim();

                // 20141218 Add Sort Rule
                if (_port.File.Type == ePortType.UnloadingPort && _port.File.Status == ePortStatus.LR)
                {
                    if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                    {
                        portNode[keyHost.PRODUCTGRADE].InnerText = _port.File.MappingGrade.Trim();

                        Job job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                            j.TargetPortID == "0" && j.JobGrade.Trim().Equals(_port.File.MappingGrade.Trim()) &&
                            j.ProductType.Value.Equals(int.Parse(_port.File.ProductType)));

                        if (job != null)
                        {
                            portNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();
                        }
                    }
                }

                portListNode.AppendChild(portNode);


                XmlNode uldportNode = uldportListNode[keyHost.PORT].Clone();

                uldportListNode.RemoveAll();

                //// Array CF Shop in Sort Line Rule - Unloader Port List
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                    _port.File.Type == ePortType.LoadingPort && _port.File.Status == ePortStatus.LR)
                {
                    List<Port> uldPort = ObjectManager.PortManager.GetPorts(_port.Data.NODEID).Where(p =>
                        p.File.Type == ePortType.UnloadingPort).ToList<Port>();

                    Job job = null;
                    for (int i = 0; i < uldPort.Count(); i++)
                    {
                        job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j =>
                            j.CurrentEQPNo == uldPort[i].Data.NODENO && j.TargetPortID == uldPort[i].Data.PORTID);
                        if (job != null)
                        {
                            uldportNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                            uldportNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                            uldportListNode.AppendChild(uldportNode);
                            break;
                        }
                    }
                }
                //Watson add 20141121 For MES 要求
                portListNode.AppendChild(uldportListNode);
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTransferStateChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                portListNode.RemoveChild(uldportListNode); // Send to OEE  20150401 Tom
                SendToOEE(xml_doc);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.121.	PortTypeChanged     MES MessageSet : All or 2 以上 Port Type Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port IList</param>
        public void PortTypeChanged(string trxID, string lineName, IList<Port> port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTypeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in port)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][((int)_port.File.Type).ToString()].Value;
                    portListNode.AppendChild(portNode);
                }

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.121.	PortTypeChanged     MES MessageSet : One Port Type Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="port">Port</param>
        public void PortTypeChanged(string trxID, string lineName, Port port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortTypeChanged") as XmlDocument;

                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                XmlNode portNode = portCloneNode.Clone();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID);
                portNode[keyHost.PORTTYPE].InnerText = ConstantManager["MES_PORTTYPE"][((int)port.File.Type).ToString()].Value;
                portListNode.AppendChild(portNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortTypeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.122.	PortUseTypeChanged      MES MessageSet  :All or 2 以上Port Use Type Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port IList</param>
        public void PortUseTypeChanged(string trxID, string lineName, IList<Port> port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortUseTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortUseTypeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                foreach (Port _port in port)
                {
                    XmlNode portNode = portCloneNode.Clone();
                    portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                    //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade      //sy edit
                    if (CheckCellDispatchLine(line, _port))//by geade                    
                        portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                    else
                    {
                        portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][((int)_port.File.Mode).ToString()].Value;

                        //CF UPK的LD是屬於DENSE, 當PortMode=TFT或CF的時候要加上DENSE上報給MES, 而ULD是屬於CASSETTE, 則要加上CASSETTE
                        if (line.Data.LINETYPE == "FCUPK_TYPE1")
                        {
                            if (portNode[keyHost.PORTUSETYPE].InnerText == "TFT" || portNode[keyHost.PORTUSETYPE].InnerText == "CF")
                            {
                                if (_port.Data.PORTATTRIBUTE == "DENSE")
                                {
                                    portNode[keyHost.PORTUSETYPE].InnerText = portNode[keyHost.PORTUSETYPE].InnerText + "DENSE";
                                }
                                else
                                {
                                    portNode[keyHost.PORTUSETYPE].InnerText = portNode[keyHost.PORTUSETYPE].InnerText + "CASSETTE";
                                }
                            }
                        }
                        if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy add 20160701 MES 需求
                        {
                            if (_port.File.Mode == ePortMode.ReJudge)portNode[keyHost.PORTUSETYPE].InnerText ="FP";
                            if (_port.File.Mode == ePortMode.Rework)portNode[keyHost.PORTUSETYPE].InnerText ="CG";
                        }
                    }                       
                    portListNode.AppendChild(portNode);
                }
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortUseTypeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 6.122.	PortUseTypeChanged      MES MessageSet  :One Port Use Type Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="Port">Port</param>
        public void PortUseTypeChanged(string trxID, string lineName, Port _port)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortUseTypeChanged Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("PortUseTypeChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode portListNode = bodyNode[keyHost.PORTLIST];
                XmlNode portCloneNode = portListNode[keyHost.PORT].Clone();
                portListNode.RemoveAll();

                XmlNode portNode = portCloneNode.Clone();
                portNode[keyHost.PORTNAME].InnerText = ObjectManager.PortManager.GetPortID(_port.Data.NODENO, _port.Data.PORTID);
                //Jun Add 20141210 Cell的CUT LOI SOR的Unloader，上報的Port Ues Type是Port Grade      //sy edit
                if (CheckCellDispatchLine(line, _port))//by geade                    
                    portNode[keyHost.PORTUSETYPE].InnerText = _port.File.UseGrade;
                else
                {
                    portNode[keyHost.PORTUSETYPE].InnerText = ConstantManager["MES_PORTUSETYPE"][((int)_port.File.Mode).ToString()].Value;
                }
                portListNode.AppendChild(portNode);
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] PortUseTypeChanged OK LINENAME=[{1}].",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 收到MES MachineStateChanged 的回复后直接发送下一笔记录
        /// </summary>
        /// <param name="document"></param>
        public void MES_MachineStateChanged(XmlDocument document)
        {
            try {
                lock (_timeSync) { //MES 回复后可以直接上报下一笔
                    _waitForMachineStateChangeReply = false;
                }
                //if (_equipmentStateQueue.Count > 0)
                //{
                //    xMessage msg = null;
                //    bool done = _equipmentStateQueue.TryDequeue(out msg);
                //    if (done)
                //    {
                //        lock (_timeSync)
                //            _eqpStateLastReportTime = DateTime.Now;
                //        PutMessage(msg);
                //    }
                //}
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
