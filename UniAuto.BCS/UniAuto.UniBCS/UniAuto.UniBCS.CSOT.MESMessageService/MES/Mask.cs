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
        //CELL MCL 以JOB 的型式上報
        /// <summary>
        /// 6.78.	MaskProcessEnd
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="cst"></param>
        /// <param name="jobList"></param>
        public void MaskProcessEnd_MCL(string trxID, Port port, Cassette cst, IList<Job> jobList)
        {
            try
            {

                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.MACHINENAME].InnerText = port.Data.NODEID;
                bodyNode[keyHost.MASKCARRIERNAME].InnerText = cst.CassetteID;

                switch (port.File.CompletedCassetteData)
                {
                    case eCompletedCassetteData.BCForcedToCancel:
                    case eCompletedCassetteData.EQAutoCancel:
                    case eCompletedCassetteData.OperatorForcedToCancel:
                    case eCompletedCassetteData.BCForcedToAbort:
                    case eCompletedCassetteData.EQAutoAbort:
                    case eCompletedCassetteData.OperatorForcedToAbort:
                        bodyNode[keyHost.ABORTFLAG].InnerText = "Y";
                        break;
                    case eCompletedCassetteData.NormalComplete:
                        bodyNode[keyHost.ABORTFLAG].InnerText = "N";
                        break;
                    default:
                        break;
                }

                XmlNode jobListNode = bodyNode[keyHost.MASKLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.MASK].Clone();

                jobListNode.RemoveAll();//把原格式中的MASKLIST移除

                foreach (Job job in jobList)
                {

                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.MASKPOSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.MASKNAME].InnerText = job.CellSpecial.MASKID;

                    jobNode[keyHost.MASKUSECOUNT].InnerText = job.CellSpecial.UVMaskUseCount;

                    if (job.EQPFlag != "0")
                    {
                        IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                        if (subItem == null)
                        {
                            NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}).", job.CassetteSequenceNo, job.JobSequenceNo));
                            jobNode[keyHost.CLEANRESULT].InnerText = "N";
                        }
                        else
                        {
                            if (subItem.ContainsKey("CleanCompleteFlag"))
                                jobNode[keyHost.CLEANRESULT].InnerText = subItem["CleanCompleteFlag"] == "1" ? "Y" : "N";
                            else
                                jobNode[keyHost.CLEANRESULT].InnerText = "N";
                        }
                    }
                    else
                        jobNode[keyHost.CLEANRESULT].InnerText = "N";

                    jobNode[keyHost.UNITNAME].InnerText = string.Empty;
                    jobNode[keyHost.HEADID].InnerText = string.Empty;

                    jobListNode.AppendChild(jobNode);
                }

                bodyNode.AppendChild(jobListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}], LINERECIPENAME=[{3}], HOSTLINERECIPENAME=[{4}].",
                         trxID, line.Data.LINEID, port.Data.PORTID, cst.LineRecipeName, cst.MES_CstData.LINERECIPENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //CELL UVA以Material 的型式上報
        //Asir 2015/01/23 （1）Add a Parameter MaterialID，
        /// <summary>
        /// 6.78.	MaskProcessEnd
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineID"></param>
        /// <param name="eqpID"></param>
        /// <param name="MaterialID"></param>
        /// <param name="maskList"></param>
        public void MaskProcessEnd_UVA(string trxID, string lineID, string eqpID, string MaterialID, List<MaterialEntity> maskList)
        {
            try
            {

                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                //Asir 2015/01/23 
                bodyNode[keyHost.MASKCARRIERNAME].InnerText = MaterialID;

                bodyNode[keyHost.ABORTFLAG].InnerText = "N";


                XmlNode jobListNode = bodyNode[keyHost.MASKLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.MASK].Clone();

                jobListNode.RemoveAll();//把原格式中的MASKLIST移除
                foreach (MaterialEntity mask in maskList)
                {

                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.MASKPOSITION].InnerText = mask.MaterialPosition;
                    jobNode[keyHost.MASKNAME].InnerText = mask.MaterialID;

                    jobNode[keyHost.MASKUSECOUNT].InnerText = mask.UseCount;
                    jobNode[keyHost.CLEANRESULT].InnerText = string.Empty;
                    jobNode[keyHost.UNITNAME].InnerText = mask.UnitNo;
                    jobNode[keyHost.HEADID].InnerText = mask.HEADID;

                    jobListNode.AppendChild(jobNode);
                }
                bodyNode.AppendChild(jobListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd OK LINENAME=[{1}], PORTNAME =[{2}].",
                         trxID, line.Data.LINEID, string.Empty));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.78.	MaskProcessEnd
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="port"></param>
        /// <param name="jobList"></param>
        /// <param name="reasoncode"></param>
        /// <param name="reasontxt"></param>
        public void MaskProcessEndAbort(string trxID, Port port, IList<Job> jobList, string reasoncode, string reasontxt)
        {
            try
            {

                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd Send MES but OFF LINE LINENAME=[{1}].", trxID, line.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskProcessEnd") as XmlDocument;

                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.MACHINENAME].InnerText = port.Data.NODEID;
                bodyNode[keyHost.MASKCARRIERNAME].InnerText = port.File.CassetteID.Trim();

                bodyNode[keyHost.ABORTFLAG].InnerText = "Y";

                XmlNode jobListNode = bodyNode[keyHost.MASKLIST];
                XmlNode jobNodeClone = jobListNode[keyHost.MASK].Clone();

                jobListNode.RemoveAll();//把原格式中的MASKLIST移除
                foreach (Job job in jobList)
                {

                    XmlNode jobNode = jobNodeClone.Clone();
                    jobNode[keyHost.MASKPOSITION].InnerText = job.ToSlotNo;
                    jobNode[keyHost.MASKNAME].InnerText = job.GlassChipMaskBlockID;

                    jobNode[keyHost.MASKUSECOUNT].InnerText = job.CellSpecial.UVMaskUseCount;

                    if (job.EQPFlag != "0")
                    {
                        IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                        if (subItem == null)
                        {
                            NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}).", job.CassetteSequenceNo, job.JobSequenceNo));
                            jobNode[keyHost.CLEANRESULT].InnerText = "N";
                        }
                        else
                        {
                            if (subItem.ContainsKey("CleanCompleteFlag"))
                                jobNode[keyHost.CLEANRESULT].InnerText = subItem["CleanCompleteFlag"] == "1" ? "Y" : "N";
                            else
                                jobNode[keyHost.CLEANRESULT].InnerText = "N";
                        }
                    }
                    else
                        jobNode[keyHost.CLEANRESULT].InnerText = "N";
                    jobNode[keyHost.UNITNAME].InnerText = string.Empty;
                    jobNode[keyHost.HEADID].InnerText = string.Empty;

                    jobListNode.AppendChild(jobNode);
                }

                bodyNode.AppendChild(jobListNode);

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES]=[{0}] MaskProcessEnd NG LINENAME=[{1}], PORTNAME =[{2}]. ReasonCode=[{3}],ReasonText=[{4}]",
                         trxID, line.Data.LINEID, port.Data.PORTID, reasoncode, reasontxt));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.79.	MaskStateChanged        MES<-BC Reports when Mask state has been changed. Refer to ‘MES Standard for CSOT G8.5 LCD’ document for Mask state definition.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">MachineName</param>
        /// <param name="machineRecipeName">MachineRecipeName</param>
        /// <param name="eventUse">EventUse</param>
        /// <param name="maskList">MaskList</param>
        public void MaskStateChanged(string trxID, string lineName, string machineName, string machineRecipeName, string eventUse, IList<MaskStateChanged.MASKc> maskList, string requestKey)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].",
                        //trxID, lineName));
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    #region 根据OFFLINEREPLYEQP设定回复机台
                    string unitNo = "0";
                    int command = 1;

                    if (!ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                    {
                        command = 2;
                    }
                    if (requestKey == "B")
                    {
                        Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxID, eBitResult.ON, (eReturnCode1)command, eqp.Data.NODENO, unitNo });
                        return;
                    }
                    else if (requestKey == "MaterialStatusChangeReport")
                    {
                        MaterialEntity material = new MaterialEntity();
                        if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                        {
                            string headNo = "0";
                            if (maskList.Count > 0) headNo = maskList[0].MASKPOSITION;
                            Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, headNo, material, eBitResult.ON, (eReturnCode1)command, trxID, false });
                        }
                        else
                            Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, "0", material, eBitResult.ON, (eReturnCode1)command, trxID, false });
                        return;
                    }
                    else if (requestKey == "SecsMaskStatusChangeReport")
                    {
                        //Nothing need to do. BY CY
                        //For secs, it will reply at event rise when offline.
                    }
                    return;
                    #endregion
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                bodyNode[keyHost.MACHINERECIPENAME].InnerText = machineRecipeName;
                //bodyNode[keyHost.EVENTUSE].InnerText = eventUse;//spec是EVENTUSE,要問一下

                XmlNode maskListNode = bodyNode[keyHost.MASKLIST];
                XmlNode maskNodeClone = maskListNode[keyHost.MASK].Clone();
                maskListNode.RemoveAll();

                foreach (MaskStateChanged.MASKc mask in maskList)
                {
                    XmlNode maskNode = maskNodeClone.Clone();
                    maskNode[keyHost.MASKPOSITION].InnerText = mask.MASKPOSITION;
                    maskNode[keyHost.MASKNAME].InnerText = mask.MASKNAME;
                    maskNode[keyHost.MASKSTATE].InnerText = mask.MASKSTATE;
                    maskNode[keyHost.MASKUSECOUNT].InnerText = mask.MASKUSECOUNT;
                    maskNode[keyHost.UNITNAME].InnerText = mask.UNITNAME;
                    maskNode[keyHost.CLEANRESULT].InnerText = mask.CLEANRESULT;
                    maskNode[keyHost.REASONCODE].InnerText = mask.REASONCODE;
                    maskNode[keyHost.HEADID].InnerText = mask.HEADID;
                    maskListNode.AppendChild(maskNode);
                }

                if (maskList.Count > 0) //modify 2016/03/18 cc.kuang
                    SendToMES(xml_doc);

                foreach (MaskStateChanged.MASKc mask in maskList)
                {
                    if (mask.MASKSTATE == eMaterialStatus.DISMOUNT.ToString())
                    {
                        List<MaterialEntity> materialEntity = ObjectManager.MaterialManager.GetMaterials();
                        if (materialEntity != null)
                        {
                            foreach (MaterialEntity material in materialEntity)
                            {
                                if (material.MaterialID == mask.MASKNAME)
                                {
                                    ObjectManager.MaterialManager.DeleteMaterial(material);
                                    break;
                                }
                            }
                        }
                    }
                }
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
        ///  6.79.	MaskStateChanged        MES MessageSet : Mask States change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        public void MaskStateChanged_OnLine(string trxID, Equipment eqp)
        {
            try
            {
                int iMaskCount = 0; //add 2016/03/18 cc.kuang
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.MACHINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);

                XmlNode masklistnode = bodyNode[keyHost.MASKLIST];
                XmlNode masknodeclone = masklistnode[keyHost.MASK].Clone();
                masklistnode.RemoveAll();

                List<MaterialEntity> materialList = ObjectManager.MaterialManager.GetMasks();   //20160816 Modify by Frank Mask需要用GetMasks否則會找不到
                if (materialList != null)
                {
                    foreach (MaterialEntity material in materialList)
                    {
                        if (material.EQType != eMaterialEQtype.MaskEQ || material.NodeNo != eqp.Data.NODENO)
                            continue;  //return;  //Jun Modify 20141208 Material List是By Line取得的，所以需要判斷所有的Material 不能只判斷一個不符合就跳出
                        XmlNode masknode = masknodeclone.Clone();
                        masknode[keyHost.MASKPOSITION].InnerText = material.MaterialSlotNo;
                        masknode[keyHost.MASKNAME].InnerText = material.MaterialID;
                        masknode[keyHost.MASKSTATE].InnerText = material.MaterialStatus.ToString();
                        masknode[keyHost.MASKUSECOUNT].InnerText = material.MaterialValue;
                        //masknode[keyHost.UNITNAME].InnerText = material.UnitNo;
                        masknode[keyHost.UNITNAME].InnerText = ObjectManager.UnitManager.GetUnit(material.NodeNo, material.UnitNo).Data.UNITID; // BC ONLINE GET mask unit name add by qiumin 20170804
                        masknode[keyHost.CLEANRESULT].InnerText = "";
                        masknode[keyHost.REASONCODE].InnerText = "";
                        masknode[keyHost.HEADID].InnerText = material.HEADID;
                        masklistnode.AppendChild(masknode);
                        bodyNode[keyHost.OPERATOR].InnerText = ""; //填入什麼？
                        iMaskCount ++;

                        #region ON Line TimeID Regesiter
                        string timeId = string.Format("{0}_{1}_MaskStateChanged_ONLINE", eqp.Data.NODENO, trxID); //Watson 20150106 Add For On Line Timer

                        if (Timermanager.IsAliveTimer(timeId))
                        {
                            Timermanager.TerminateTimer(timeId);
                        }
                        Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskStateChangedReplyTimeout), eqp.Data.NODENO);
                        #endregion

                        if (material.MaterialStatus != eMaterialStatus.DISMOUNT) //report one by one 2016/03/23 cc.kuang
                        {
                            SetTransactionID(xml_doc, UtilityMethod.GetAgentTrackKey());
                            SendToMES(xml_doc);
                        }

                        if (material.MaterialStatus == eMaterialStatus.DISMOUNT)
                        {
                            ObjectManager.MaterialManager.DeleteMaterial(material);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));

                    }

                    //if (iMaskCount > 0)
                        //SendToMES(xml_doc); //modify by bruce 201602022 統一收集完上報
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

        /// <summary>
        /// 6.79.	MaskStateChanged
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqp"></param>
        public void MaskStateChanged_OnLine_TCPHL(string trxID, Equipment eqp)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = line.Data.LINEID;
                bodyNode[keyHost.MACHINERECIPENAME].InnerText = eqp.File.CurrentRecipeID;
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;

                XmlNode masklistnode = bodyNode[keyHost.MASKLIST];
                XmlNode masknodeclone = masklistnode[keyHost.MASK].Clone();
                masklistnode.RemoveAll();

                List<MaterialEntity> maskList = ObjectManager.MaterialManager.GetMasks();
                if (maskList != null)
                {
                    foreach (MaterialEntity material in maskList)
                    {
                        masklistnode.RemoveAll(); //report one by one 2016/03/23 cc.kuang
                        //20150417 cy:mask name為空時,不上報給MES
                        if (material.EQType != eMaterialEQtype.MaskEQ || material.NodeNo != eqp.Data.NODENO || string.IsNullOrEmpty(material.MaterialID))
                            continue;
                        XmlNode masknode = masknodeclone.Clone();
                        masknode[keyHost.MASKPOSITION].InnerText = material.MaterialSlotNo;
                        masknode[keyHost.MASKNAME].InnerText = material.MaterialID;
                        masknode[keyHost.MASKSTATE].InnerText = material.MaterialStatus.ToString();
                        masknode[keyHost.MASKUSECOUNT].InnerText = material.MaterialValue;
                        masknode[keyHost.UNITNAME].InnerText = material.UnitNo;
                        masknode[keyHost.CLEANRESULT].InnerText = "";
                        masknode[keyHost.REASONCODE].InnerText = "";
                        masknode[keyHost.HEADID].InnerText = material.HEADID;
                        masklistnode.AppendChild(masknode);
                        bodyNode[keyHost.OPERATOR].InnerText = ""; //填入什麼？

                        #region ON Line TimeID Regesiter
                        string timeId = string.Format("{0}_{1}_MaskStateChanged_ONLINE", eqp.Data.NODENO, trxID); //Watson 20150106 Add For On Line Timer

                        if (Timermanager.IsAliveTimer(timeId))
                        {
                            Timermanager.TerminateTimer(timeId);
                        }
                        Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskStateChangedReplyTimeout), eqp.Data.NODENO);
                        #endregion

                        if (material.MaterialStatus != eMaterialStatus.DISMOUNT) // report one by one & dismount not report when online switch 2016/03/23
                        {
                            SetTransactionID(xml_doc, UtilityMethod.GetAgentTrackKey());
                            SendToMES(xml_doc);
                        }
                        if (material.MaterialStatus == eMaterialStatus.DISMOUNT)
                        {
                            //ObjectManager.MaterialManager.DeleteMaterial(material);
                            ObjectManager.MaterialManager.DeleteMask(material);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME=[{1}].",
                                trxID, line.Data.LINEID));

                    }
                    //SendToMES(xml_doc); //modify for report MES many times again bug 2016/01/28 cc.kuang
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

        /// <summary>
        /// 6.80.	MaskStateChangedReply       MES Reply "MaskStateChangedReply"
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void MES_MaskStateChangedReply(XmlDocument xmlDoc)
        {
            try
            {
                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineId = GetLineName(xmlDoc);
                string trxId = GetTransactionID(xmlDoc);
                string machineName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;

                Line line = ObjectManager.LineManager.GetLine(lineId);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqp.Data.NODENO));
                //string lineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                string unitId = ""; //modify for MES download maybe loss MASKLIST,...
                if (xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST] != null && xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST][keyHost.MASK] !=null &&
                    xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST][keyHost.MASK][keyHost.UNITNAME] != null)
                {
                    unitId = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST][keyHost.MASK][keyHost.UNITNAME].InnerText;
                }
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
                string command = string.Empty;

                if (!CheckMESLineID(line.Data.LINEID))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}].", ServerName, trxId, line.Data.LINEID));
                }
                //有可能有多个Transaction call MaskStateChanged,需要知道Reply给对应的Transaction
                string timeId1 = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, unitNo, trxId);
                string timeId2 = string.Format("B_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, unitNo, trxId);
                string timeId3 = string.Format("SecsMaskStatusChangeReport_{0}_{1}_MaskStateChanged", eqp.Data.NODENO, trxId);
                string timeId4 = string.Format("{0}_{1}_MaskStateChanged", eqp.Data.NODENO, trxId);
                string timeId5 = string.Format("{0}_{1}_MaskStateChanged_ONLINE", eqp.Data.NODENO, trxId); //Watson 20150106 Add For On Line Timer
                string timeId6 = string.Format("{0}_MASKStatusReportReplyTimeout", eqp.Data.NODENO); //Kasim 20150312 Add For MES Reply
                string replyKey = "";
                if (Timermanager.IsAliveTimer(timeId6))
                {
                    Timermanager.TerminateTimer(timeId6);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxId, line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage));
                    return;
                }
                else if (Timermanager.IsAliveTimer(timeId5))//Watson add 20150106
                {
                    Timermanager.TerminateTimer(timeId5);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                            trxId, line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage));
                    return;
                }
                else if (Timermanager.IsAliveTimer(timeId1))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                {
                    replyKey = "MaterialStatusChangeReport";
                    Timermanager.TerminateTimer(timeId1);
                }
                else if (Timermanager.IsAliveTimer(timeId2))
                {
                    replyKey = "B";
                    Timermanager.TerminateTimer(timeId2);
                }
                else if (Timermanager.IsAliveTimer(timeId4))
                {
                    Timermanager.TerminateTimer(timeId4);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                         trxId, line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage));
                    return;
                }
                else if (Timermanager.IsAliveTimer(timeId3))
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply LINENAME=[{1}],MACHINENAME=[{2}],CODE=[{3}],MESSAGE=[{4}].",
                                         trxId, line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage));

                    //get timer by key
                    UserTimer timer = Timermanager.GetAliveTimer(timeId3);
                    if (timer != null)
                    {
                        Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                        if (tuple != null)
                        {
                            string systembyte = tuple.Item1;
                            switch (eqp.Data.NODEATTRIBUTE)
                            {
                                case "NIKON":
                                    Invoke(eServiceName.NikonSECSService, "TS6F12_H_EventReportAcknowledge",
                                        new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)(returnCode != "0" ? 1 : 0) });
                                    if (returnCode != "0")
                                    {
                                        Invoke(eServiceName.NikonSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "MES Reply MaskStateChange NG", trxId, string.Empty });
                                    }
                                    break;
                                default:
                                    Invoke(eServiceName.CSOTSECSService, "TS6F12_H_EventReportAcknowledge",
                                        new object[] { eqp.Data.NODENO, eqp.Data.NODEID, trxId, systembyte, (byte)(returnCode != "0" ? 1 : 0) });
                                    if (returnCode != "0")
                                    {
                                        Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO, eqp.Data.NODEID, "MES Reply MaskStateChange NG", trxId, string.Empty });
                                    }
                                    break;
                            }
                        }
                    }
                    Timermanager.TerminateTimer(timeId3);
                    return;
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS <- MES][{4}] MES_MaskStateChangedReply not found BCS Send Message LINENAME=[{0}],MACHINENAME=[{1}],CODE=[{2}],MESSAGE=[{3}].",
                                     line.Data.LINEID, eqp.Data.NODEID, returnCode, returnMessage, trxId));
                    return;
                }
                XmlNodeList maskList = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MASKLIST].ChildNodes;
                List<MaskStateChanged.MASKc> masklist = new List<MaskStateChanged.MASKc>();
                MaskStateChanged.MASKc mask = new MaskStateChanged.MASKc();
                foreach (XmlNode node in maskList)
                {
                    mask = new MaskStateChanged.MASKc();
                    mask.MASKPOSITION = node[keyHost.MASKPOSITION].InnerText;
                    mask.MASKNAME = node[keyHost.MASKNAME].InnerText;
                    mask.MASKSTATE = node[keyHost.MASKSTATE].InnerText;
                    mask.MASKUSECOUNT = node[keyHost.MASKUSECOUNT].InnerText;
                    mask.UNITNAME = node[keyHost.UNITNAME].InnerText;
                    masklist.Add(mask);
                }

                if (returnCode != "0")
                {
                    //Send NG to Equipment 
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply NG LINENAME=[{1}],MACHINENAME=[{2}],MASKPOSITION=[{3}],MASKNAME=[{4}],MASKSTATE=[{5}],CODE=[{6}],MESSAGE=[{7}].",
                                         trxId, line.Data.LINEID, eqp.Data.NODEID, mask.MASKPOSITION, mask.MASKNAME, mask.MASKSTATE, returnCode, returnMessage));

                    command = "2";
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[LINENAME={1}] [BCS <- MES][{0}] MES_MaskStateChangedReply OK LINENAME=[{1}],MACHINENAME=[{2}],MASKPOSITION=[{3}],MASKNAME=[{4}],MASKSTATE=[{5}],CODE=[{6}],MESSAGE=[{7}].",
                                         trxId, line.Data.LINEID, eqp.Data.NODEID, mask.MASKPOSITION, mask.MASKNAME, mask.MASKSTATE, returnCode, returnMessage));
                    //Send OK To Equipment
                    command = "1";//OK
                }

                if (replyKey == "B")
                {
                    Invoke(eServiceName.CFSpecialService, "MaterialVerificationRequestReply", new object[] { trxId, eBitResult.ON, (eReturnCode1)int.Parse(command), eqp.Data.NODENO, unitNo });

                }
                else if (replyKey == "MaterialStatusChangeReport")
                {
                    MaterialEntity material = new MaterialEntity();
                    Invoke(eServiceName.MaterialService, "MaterialStatusChangeReportReply", new object[] { eqp.Data.NODENO, unitNo, mask.MASKPOSITION, material, eBitResult.ON, (eReturnCode1)int.Parse(command), trxId, false });
                }

            }
            catch (Exception ex)
            {
                //string timeId1 = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, unitNo, trxId);
                //string timeId2 = string.Format("B_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, unitNo, trxId);
                //string timeId3 = string.Format("SecsMaskStatusChangeReport_{0}_{1}_MaskStateChanged", eqp.Data.NODENO, trxId);
                //if (Timermanager.IsAliveTimer(timeId1))//没有找到对应的TimeID，有可能已经Timeout ,或者直接没有发送给
                //    Timermanager.TerminateTimer(timeId1);
                //Timermanager.TerminateTimer(timeId2);
                //Timermanager.TerminateTimer(timeId3);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  6.84.	MaskLocationChanged     MES MessageSet : Mask Location Change Report to MES
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="eQPID"></param>
        /// <param name="maskid"></param>
        /// <param name="location"></param>
        public void MaskLocationChanged(string trxID, string lineName, string eQPID, string maskState, string maskid, string maskAction, string maskPosition, string user)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eQPID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME=[{1}].", trxID, lineName));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskLocationChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);

                bodyNode[keyHost.MACHINENAME].InnerText = ObjectManager.EquipmentManager.GetEQPID(eqp.Data.NODENO);
                XmlNode maskListNode = bodyNode[keyHost.MASKLIST];
                XmlNode maskNodeClone = maskListNode[keyHost.MASK];
                maskListNode.RemoveAll();
                //目前只會有一個Mask
                //for (int i = 0; i < job.MesAbnormal.CODE.Count; i++)
                //{
                XmlNode maskNode = maskNodeClone.Clone();
                maskNode[keyHost.MASKSTATE].InnerText = maskState; //Add by marine for T3 MES 20150818
                maskNode[keyHost.MASKNAME].InnerText = maskid;
                maskNode[keyHost.MASKACTION].InnerText = maskAction; //Add by marine for T3 MES 20150818
                maskNode[keyHost.MASKPOSITION].InnerText = maskPosition; //Add by marine for T3 MES 20150818
                maskListNode.AppendChild(maskNode);
                //}

                bodyNode[keyHost.EVENTUSER].InnerText = user;  //Add by marine for T3 MES 20150818
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();  //Add by marine for T3 MES 20150818

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
        /// 6.85.	MaskLocationChangedReply        MES MessagetSet : Mask Location Change Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_MaskLocationChangedReply(XmlDocument xmlDoc)
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
                string eqpid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.MACHINENAME].InnerText;
                string eventuser = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.EVENTUSER].InnerText;  //Add by marine for T3 MES 2015/8/18
                string timestamp = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.TIMESTAMP].InnerText;  //Add by marine for T3 MES 2015/8/18

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MaskLocationChangedReply NG LINENAME =[{1}],MACHINENAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, eqpid, returnCode, returnMessage));
                    //to do?
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES]=[{0}] MaskLocationChangedReply  OK LINENAME =[{1}],MACHINENAME =[{2}],CODE =[{3}],MESSAGE =[{4}].",
                                        trxID, lineName, eqpid, returnCode, returnMessage));
                    //to do?
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.86.	MaskUsedCountReport     MES<-BC Report every half hour by BC.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">MachineName</param>
        /// <param name="maskList">MaskList</param>
        public void MaskUsedCountReport(string trxID, string lineName, string machineName, IList<MaskUsedCountReport.MASKc> maskList)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("MaskUsedCountReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;

                XmlNode maskListNode = bodyNode[keyHost.MASKLIST];
                XmlNode maskNode = maskListNode[keyHost.MASK].Clone();

                maskListNode.RemoveAll();

                foreach (MaskUsedCountReport.MASKc mask in maskList)
                {
                    XmlNode masknode = maskNode.Clone();
                    masknode[keyHost.MASKNAME].InnerText = mask.MASKNAME;
                    masknode[keyHost.MASKUSECOUNT].InnerText = mask.MASKUSECOUNT;
                    maskListNode.AppendChild(masknode);
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
        /// 6.168.	UVMaskUseCount      MES MessageSet : Reports when UV Mask has been used.
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="eqpID">EQP ID</param>
        /// <param name="maskID">UV Mask ID</param>
        /// <param name="useQty">use count</param>
        public void UVMaskUseCount(string trxID, string lineName, string eqpID, string maskID, string useQty)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("UVMaskUseCount") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.MASKNAME].InnerText = maskID;
                bodyNode[keyHost.MASKUSECOUNT].InnerText = useQty;

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
    }
}
