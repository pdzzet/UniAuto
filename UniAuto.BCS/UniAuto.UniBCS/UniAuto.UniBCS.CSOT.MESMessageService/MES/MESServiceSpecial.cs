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
        /// 6.40.	ChangeTankReport        MES MessageSet :Change Tank Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodeno">Node ID</param>
        /// <param name="targetdata">Tank</param>
        public void ChangeTankReport(string trxID, string lineName, string eqpId, string tankNo)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangeTankReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpId;
                bodyNode[keyHost.MATERIALTYPE].InnerText = "TANK";

                XmlNode materListNode = bodyNode[keyHost.TANKLIST];
                XmlNode prodCloneNode = materListNode[keyHost.TANK].Clone();
                materListNode.RemoveAll();

                //一次只有一片
                XmlNode materNode = prodCloneNode.Clone();
                materNode[keyHost.TANKNAME].InnerText = string.Empty; //TANKNAME,CHAMBERID  are  both  empty.
                materNode[keyHost.NEWTANKNAME].InnerText = tankNo;
                materNode[keyHost.CHAMBERID].InnerText = string.Empty; //TANKNAME,CHAMBERID  are  both  empty.
                materListNode.AppendChild(materNode);

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
        /// 6.44.	DefectCodeReportByGlass      MES MessageSet : Defect Code Report By Glass
        /// </summary>
        public void DefectCodeReportByGlass(string trxID, string lineName, Job job)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("DefectCodeReportByGlass") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                XmlNode originalProductList = bodyNode[keyHost.ORIGINALPRODUCTLIST];
                XmlNode originalProductNode = originalProductList[keyHost.ORIGINALPRODUCT].Clone();
                originalProductList.RemoveAll();

                originalProductNode[keyHost.ORIGINALPRODUCTNAME].InnerText = job.GlassChipMaskBlockID.Trim();

                XmlNode productList = originalProductNode[keyHost.PRODUCTLIST];
                XmlNode productNode = productList[keyHost.PRODUCT].Clone();
                productList.RemoveAll();

                foreach (DefectCode dc in job.DefectCodes)
                {
                    XmlNode obj = productNode.Clone();
                    int chip = int.Parse(dc.ChipPostion);
                    //Check Chip > 99 记录Log  2016-2-24 Tom.bian
                    if (chip > 99) {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("CST_SETNO=[{0}] JOB_SEQNO=[{1}] CHIP_POSITION=[{2}] >99 .",
                                                    job.CassetteSequenceNo, job.JobSequenceNo, chip, job.ChipCount));
                        continue;
                    }
                    obj[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID + chip.ToString("X2");
                    if (chip == 0 || chip > job.ChipCount)
                    {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("CST_SETNO=[{0}] JOB_SEQNO=[{1}] CHIP_POSITION=[{2}] > CHIP_COUNT[{3}] OR CHIP_POSITION==0.",
                             job.CassetteSequenceNo, job.JobSequenceNo, chip, job.ChipCount));
                        continue;
                    }
                    obj[keyHost.MACOJUDGE].InnerText = job.OXRInformation.Substring(chip - 1, 1);
                    obj[keyHost.MURACODES].InnerText = dc.DefectCodes;
                    obj[keyHost.EVENTCOMMENT].InnerText = "REPORT By EQ";
                    productList.AppendChild(obj);
                }
                originalProductList.AppendChild(originalProductNode);
                bodyNode[keyHost.EVENTUSER].InnerText = lineName;
                bodyNode[keyHost.EVENTCOMMENT].InnerText = "REPORT By EQ";
                bodyNode[keyHost.LANGUAGE].InnerText = "ENGLISH";

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> MES][{1}] SEND MES TRX \"{2}\".", lineName, trxID, MethodBase.GetCurrentMethod().Name));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Watson Add 20150126 For MES Spec
        public void ProductInspectionDataReport(string trxID, Equipment eqp,Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductInspectionDataReport Send MES but OFF LINE LINENAME =[{1}].", trxID, eqp.Data.LINEID));
                            string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ProductInspectionDataReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = eqp.Data.LINEID;
                bodyNode[keyHost.MACHINENAME].InnerText = eqp.Data.NODEID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;
                bodyNode[keyHost.INSPECTIONRESULT].InnerText = job.JobJudge =="1"?"Y":"N";
                bodyNode[keyHost.INSPECTIONTIME].InnerText = GetTIMESTAMP();
                foreach (DefectCode defectcode in job.DefectCodes)
                    bodyNode[keyHost.DEFECTCODELIST].InnerText += defectcode.DefectCodes;

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] ProductInspectionDataReport OK LINENAME =[{1}].",
                    trxID, eqp.Data.LINEID));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MaskStateChangedReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, MES_MaskStateChangedReplyTimeout Set Bit (OFF).", sArray[0], trackKey));
                MaterialEntity material = new MaterialEntity();

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.33.	ChangeTargetLife        MES MessageSet :Change Target Life to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="nodeno">Node No</param>
        /// <param name="targetdata">Target Life List</param>
        public void ChangeTargetLife(string trxID, string lineName, string eqpName, IList<ChangeTargetLife.CHAMBERc> chamberData)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("ChangeTargetLife") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpName;
                bodyNode[keyHost.PRODUCTNAME].InnerText = string.Empty;
                bodyNode[keyHost.MATERIALTYPE].InnerText = "TARGET"; //HOT CODE Ref MES SPEC

                XmlNode chamListNode = bodyNode[keyHost.CHAMBERLIST];
                XmlNode chamCloneNode = chamListNode[keyHost.CHAMBER].Clone();
                chamListNode.RemoveAll();

                foreach (ChangeTargetLife.CHAMBERc chamber in chamberData)
                {
                    XmlNode chamNode = chamCloneNode.Clone();
                    chamNode[keyHost.AVERAGE].InnerText = chamber.AVERAGE;
                    chamNode[keyHost.CHAMBERID].InnerText = chamber.CHAMBERID;
                    chamNode[keyHost.QUANTITY].InnerText = chamber.QUANTITY;
                    chamListNode.AppendChild(chamNode);
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
        /// 6.166.	VCRReadReport       MES MessageSet : VCR Read Report to MES
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="eqpID">EQP ID (VCR Postion)</param>
        /// <param name="job">VCR Read Glass/Panel ID update job.VCRJobID</param>
        /// <param name="vcrreadflag">VCR Read Flag ‘OK’ ;‘NG’; ‘PASS’; ‘UNMATCH’</param>
        public void VCRReadReport(string trxID, string lineName, string eqpID, Job job, eVCR_EVENT_RESULT vcrreadflag)
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
                XmlDocument xml_doc = agent.GetTransactionFormat("VCRReadReport") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                //to do 
                bodyNode[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                bodyNode[keyHost.MACHINENAME].InnerText = eqpID;
                bodyNode[keyHost.PRODUCTNAME].InnerText = job.VCRJobID;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.VCRREADFLAG].InnerText = ObjectManager.JobManager.P2M_GetVCRResult(vcrreadflag);

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
        /// 6.167.	VCRStateChanged     MES<-BC Reports when VCR states has been changed,and MES will not report to OEE.
        /// </summary>
        /// <param name="trxID">TransactionID(yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">LineName</param>
        /// <param name="machineName">MachineName</param>
        /// <param name="vcrName">VCRName</param>
        /// <param name="vcrStateName">VCRStateName</param>
        /// <param name="timeStamp">TimeStamp</param>
        public void VCRStateChanged(string trxID, string lineName, string machineName, string vcrName, string vcrStateName)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}].",
                        //trxID, lineName));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("VCRStateChanged") as XmlDocument;
                SetTransactionID(xml_doc, trxID);  //調用傳入的TrxID 重要不能沒寫
                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = lineName;
                bodyNode[keyHost.MACHINENAME].InnerText = machineName;
                bodyNode[keyHost.VCRNAME].InnerText = machineName + "_" + vcrName;
                bodyNode[keyHost.VCRSTATENAME].InnerText = vcrStateName;
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                SendToMES(xml_doc);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}].",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }




    }
}
