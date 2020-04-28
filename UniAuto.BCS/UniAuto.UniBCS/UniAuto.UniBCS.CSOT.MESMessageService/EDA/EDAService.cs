using System;
using System.Collections;
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
using UniAuto.UniBCS.EDASpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public class EDAService : AbstractService
    {
        private bool _run = false;
        private int seqNo = 0;
        //EDA  EDCGLASSRUNEND 需要使用不同的TargetSubject  20150227 Tom
        //Change Subject Name to T3 
        private string _runEndTargetSubject = "WHCSOT.G6C.BC.PRD.EDC.SubEQP"; 
        public string RunEndTargetSubject
        {
            get { return _runEndTargetSubject; }
            set { _runEndTargetSubject = value; }
        }

        #region Ini
        /// <summary>
        /// Service 初始化方法
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            bool ret = false;
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                _run = true;
                ret = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                Destory();
                ret = false;
            }
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
            return ret;
        }

        #endregion

        #region Destory
        public void Destory()
        {
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            try
            {
                if (_run)
                {
                    _run = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
        }
        #endregion



        private IServerAgent GetServerAgent()
        {
            return GetServerAgent("EDAAgent");
        }

        private IServerAgent GetServerAgent2()
        {
            return GetServerAgent("EDAAgent2");
        }

        /// <summary>
        /// Agent回報Service, 訊息已經送出, Service決定是否加入T3
        /// </summary>
        /// <param name="sendDt"></param>
        /// <param name="xml"></param>
        /// <param name="t3TimeoutSecond"></param>
        public void EDA_MessageSend(DateTime sendDt, string xml, int t3TimeoutSecond)
        {
            try
            {
                //Message msg = Spec.XMLtoMessage(xml);
                //if (msg.WaitReply != string.Empty && t3TimeoutSecond > 0)
                //{
                //    T3Message t3msg = new T3Message(sendDt, msg, t3TimeoutSecond);
                //    NLogManager.Logger.LogTrxWrite(LogName, string.Format("{0}=[{1}] Add to WaitForT3", t3msg.Msg.HEADER.MESSAGENAME, t3msg.Msg.HEADER.TRANSACTIONID));
                //    lock (_waitForT3)
                //    {
                //        _waitForT3.Add(t3msg.Msg.HEADER.TRANSACTIONID, t3msg);
                //    }
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region Tibcio RV
        /// <summary>
        /// Agent回報Service, Tibco Open成功
        /// </summary>
        public void EDA_TibcoOpen()
        {
            //尚未決定Service要做何處理
            try
            {
                Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                string trxID = CreateTrxID();
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LineName={0}] [BCS -> MES] [{1}] MES Tibrv Open.", line.Data.LINEID, trxID));
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EDAReport(string trxID, string lineName, string eqpName, string eqpStartTime, string eqpEndTime, string eqpReportTime, Job job, Dictionary<string, List<string>> edaLis)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null) throw new Exception(string.Format("Can't find line Name =[{0}] in LineEntity!", lineName));

                switch (line.Data.FABTYPE)
                {
                    case "ARRAY":
                        ARRAYEDCGLASSSEND(trxID, lineName, eqpName, eqpStartTime, eqpEndTime, eqpReportTime, job, edaLis);
                        break;
                    case "CF":
                        CFEDCGLASSSEND(trxID, lineName, eqpName, eqpStartTime, eqpEndTime, eqpReportTime, job, edaLis);
                        break;
                    case "CELL":
                        CELLEDCCOMPONENTSEND(trxID, lineName, eqpName, eqpStartTime, eqpEndTime, eqpReportTime, job, edaLis);
                        break;
                    case "MODULE": //2015-9-16 Add Module Message Tom
                        MODEDCPANELEND(trxID, lineName, eqpName, eqpStartTime, eqpEndTime, eqpReportTime, job, edaLis);
                        break;
                    default :
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


        #region 1.1.	ARRAYEDCGLASSSEND
        /// <summary>
        /// EDA MessageSet  : ARRAYEDCGLASSSEND to EDA
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        private void ARRAYEDCGLASSSEND(string trxID, string lineName, string eqpName, string eqpStartTime, string eqpEndTime, string eqpReportTime, Job job, Dictionary<string, List<string>> edaLis)
        {
            try
            {
                Thread.Sleep(1000);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpName);

                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                    return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("ARRAYEDCGLASSSEND") as XmlDocument;
                xml_doc["message"]["message_id"].InnerText = "ARRAYEDCGLASSSEND";
                //xml_doc["message"]["systembyte"].InnerText = string.Format("{0}-" + GetSeqNo() + "-{1}-ARRAYPDSGLASSSEND-{2}", "A", eqpName, DateTime.Now.ToString("yyyyMMddHHmmssfff")); //A-000-ALSP01-ARRAYPDSGLASSSEND-20080301121212789
                xml_doc["message"]["timestamp"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //xml_doc["message"]["oriented_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDSITE);
                //xml_doc["message"]["oriented_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDFACTORYNAME);
                //xml_doc["message"]["current_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTSITE);
                //xml_doc["message"]["current_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTFACTORYNAME);
                xml_doc["message"]["lot_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LOTNAME);
                xml_doc["message"]["glass_id"].InnerText = (job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID);
                xml_doc["message"]["product_group"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP);
                xml_doc["message"]["line_id"].InnerText = lineName;
                xml_doc["message"]["eqp_id"].InnerText = eqpName;
                xml_doc["message"]["cst_id"].InnerText = (job.FromCstID == null ? "" : job.FromCstID);
                xml_doc["message"]["unit_id"].InnerText = GetProcessUnit(job, eqpName); //for t3 add Unit_id 2015-9-16 tom
                xml_doc["message"]["slot_no"].InnerText = (job.FromSlotNo == null ? "" : job.FromSlotNo);
                xml_doc["message"]["operator_id"].InnerText = "";
                if (eqp.Data.LINEID.Contains("TCCVD"))
                {
                    xml_doc["message"]["recipe_id"].InnerText = (job.ArraySpecial.CurrentRecipeID == null ? "" : job.ArraySpecial.CurrentRecipeID);  //Modify 20150921 by bruce 上報機台Current Recipe //modify by hujunpeng 20190416 for CVD700 PROCESSDATA REPORT debug
                }
                else xml_doc["message"]["recipe_id"].InnerText =(eqp.File.CurrentRecipeID==null ? "" : eqp.File.CurrentRecipeID);
                //xml_doc["message"]["recipe"].InnerText = (eqp == null ? "" : eqp.File.CurrentRecipeID);  //Jun Modify 20150401 上報機台Current Recipe
                xml_doc["message"]["fab_area"].InnerText = "Array";
                xml_doc["message"]["owner_id"].InnerText = (job.MesProduct == null ? "" : job.MesProduct.OWNERID);
                xml_doc["message"]["eqp_start_time"].InnerText = dateFormat("20" + eqpStartTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
                xml_doc["message"]["eqp_end_time"].InnerText = dateFormat("20" + eqpEndTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
                //xml_doc["message"]["track_out_time"].InnerText = eqpReportTime;

                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    xml_doc["message"]["pfcd"].InnerText = job.MesCstBody.PLANNEDPRODUCTSPECNAME;
                    xml_doc["message"]["process_id"].InnerText = job.MesCstBody.PLANNEDPROCESSOPERATIONNAME;
                }
                else
                {
                    xml_doc["message"]["pfcd"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME);
                    xml_doc["message"]["process_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME);
                }

                XmlNode paraListNode = xml_doc["message"]["param_list"];
                XmlNode paraNodeClone = paraListNode["param"].Clone();
                paraListNode.RemoveAll();

                foreach (var key in edaLis)
                {
                    foreach (string prarValue in edaLis[key.Key])
                    {
                        XmlNode paraNode = paraNodeClone.Clone();

                        string[] p = prarValue.Split(';');

                        if (p.Length == 2)
                        {
                            if (edaLis[key.Key].Count > 1)
                            {
                                //by bruce 2015/9/21 EDA Spec. no this Item
                                //paraNode["param_group"].InnerText = key.Key.ToString();
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }
                            else
                            {
                                // paraNode["param_group"].InnerText = "NA";
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }

                            paraListNode.AppendChild(paraNode);
                        }
                    }
                }

                // remove all unit data for avoid resend to same EQ node will have much err unit history 2016/04/05 cc.kuang
                // job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Clear(); keep for store EDCGLASSRUNEND use
                // job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows.Clear(); keep for store EDCGLASSRUNEND use 2016/05/12 cc.kuang

                xMessage msg = new xMessage();
                msg.FromAgent = "EDAAgent";
                msg.ToAgent = "EDAAgent";
                msg.Data = xml_doc.OuterXml;
                msg.TransactionID = trxID;
                PutMessage(msg);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> EDA][{0}] " + MethodBase.GetCurrentMethod().Name + " OK.",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region 1.2.	CFEDCGLASSSEND
        /// <summary>
        /// EDA MessageSet  : CFEDCGLASSSEND to EDA
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        private void CFEDCGLASSSEND(string trxID, string lineName, string eqpName, string eqpStartTime, string eqpEndTime, string eqpReportTime, Job job, Dictionary<string, List<string>> edaLis)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpName);
                
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                    return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CFEDCGLASSSEND") as XmlDocument;
                xml_doc["message"]["message_id"].InnerText = "CFEDCGLASSSEND";
                //xml_doc["message"]["systembyte"].InnerText = string.Format("{0}-" + GetSeqNo() + "-{1}-CFPDSGLASSSEND-{2}", "F", eqpName, DateTime.Now.ToString("yyyyMMddHHmmssfff")); //F-000-ALSP01-CFPDSGLASSSEND-20080301121212789
                xml_doc["message"]["timestamp"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
              //  xml_doc["message"]["oriented_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDSITE);
               // xml_doc["message"]["oriented_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDFACTORYNAME);
               // xml_doc["message"]["current_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTSITE);
               // xml_doc["message"]["current_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTFACTORYNAME);
                xml_doc["message"]["lot_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LOTNAME);
                xml_doc["message"]["glass_id"].InnerText = (job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID);
                xml_doc["message"]["product_group"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP);
                xml_doc["message"]["line_id"].InnerText = lineName;
                xml_doc["message"]["eqp_id"].InnerText = eqpName;
                xml_doc["message"]["cst_id"].InnerText = (job.FromCstID == null ? "" : job.FromCstID);
                xml_doc["message"]["unit_id"].InnerText = GetProcessUnit(job, eqpName); //for t3 add Unit_id 2015-9-16 tom
                xml_doc["message"]["slot_no"].InnerText = (job.FromSlotNo == null ? "" : job.FromSlotNo);
                xml_doc["message"]["operator_id"].InnerText = "";
                xml_doc["message"]["recipe_id"].InnerText = (eqp == null ? "" : eqp.File.CurrentRecipeID);  //Jun Modify 20150401 上報機台Current Recipe
                xml_doc["message"]["fab_area"].InnerText = (line.File.UPKEquipmentRunMode == eUPKEquipmentRunMode.TFT ? "ARRAY" : "CF");;
                xml_doc["message"]["owner_id"].InnerText = (job.MesProduct == null ? "" : job.MesProduct.OWNERID);
                xml_doc["message"]["eqp_start_time"].InnerText = dateFormat("20" + eqpStartTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
                xml_doc["message"]["eqp_end_time"].InnerText = dateFormat("20" + eqpEndTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
               // xml_doc["message"]["track_out_time"].InnerText = eqpReportTime;

                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    xml_doc["message"]["pfcd"].InnerText = job.MesCstBody.PLANNEDPRODUCTSPECNAME;
                    xml_doc["message"]["process_id"].InnerText = job.MesCstBody.PLANNEDPROCESSOPERATIONNAME;
                }
                else
                {
                    xml_doc["message"]["pfcd"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME);
                    xml_doc["message"]["process_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME);
                }

                XmlNode paraListNode = xml_doc["message"]["param_list"];
                XmlNode paraNodeClone = paraListNode["param"].Clone();
                paraListNode.RemoveAll();

                foreach (var key in edaLis)
                {
                    foreach (string prarValue in edaLis[key.Key])
                    {
                        XmlNode paraNode = paraNodeClone.Clone();

                        string[] p = prarValue.Split(';');

                        if (p.Length == 2)
                        {
                            if (edaLis[key.Key].Count > 1)
                            {
                                //paraNode["param_group"].InnerText = key.Key.ToString();
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }
                            else
                            {
                                //paraNode["param_group"].InnerText = "NA";
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }

                            paraListNode.AppendChild(paraNode);
                        }
                    }
                }

                xMessage msg = new xMessage();
                msg.FromAgent = "EDAAgent";
                msg.ToAgent = "EDAAgent";
                msg.Data = xml_doc.OuterXml;
                msg.TransactionID = trxID;
                PutMessage(msg);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> EDA][{0}] " + MethodBase.GetCurrentMethod().Name + " OK .",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region 1.3.	CELLEDCCOMPONENTSEND
        /// <summary>
        /// EDA MessageSet  : CELLEDCCOMPONENTSEND to EDA
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        private void CELLEDCCOMPONENTSEND(string trxID, string lineName, string eqpName, string eqpStartTime, string eqpEndTime, string eqpReportTime, Job job, Dictionary<string, List<string>> edaLis)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpName);

                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                    return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("CELLEDCCOMPONENTSEND") as XmlDocument;
                xml_doc["message"]["message_id"].InnerText = "CELLEDCCOMPONENTSEND";
                //xml_doc["message"]["systembyte"].InnerText = string.Format("{0}-" + GetSeqNo() + "-{1}-CELLPDSGLASSSEND-{2}", "C", eqpName, DateTime.Now.ToString("yyyyMMddHHmmssfff")); //C-000-ALSP01-CELLEDCCOMPONENTSEND-20080301121212789
                xml_doc["message"]["timestamp"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //xml_doc["message"]["oriented_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDSITE);
                //xml_doc["message"]["oriented_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDFACTORYNAME);
                //xml_doc["message"]["current_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTSITE);
                //xml_doc["message"]["current_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTFACTORYNAME);
                xml_doc["message"]["component_id"].InnerText = (job.GlassChipMaskBlockID == null ? "DEFAULT" : job.GlassChipMaskBlockID);
                if (line.Data.LINETYPE == eLineType.CELL.CCCLN)//huangjiayin  20170710
                {
                    xml_doc["message"]["process_id"].InnerText ="9527";
                    xml_doc["message"]["pfcd"].InnerText ="DEFAULT";
                    xml_doc["message"]["product_group"].InnerText = "DEFAULT";
                }
                else
                {
                    xml_doc["message"]["process_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "1024" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME);
                    xml_doc["message"]["pfcd"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "DEFAULT" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME);
                    xml_doc["message"]["product_group"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "DEFAULT" : job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP);
                }
                xml_doc["message"]["line_id"].InnerText = lineName;
                xml_doc["message"]["eqp_id"].InnerText = eqpName;
                xml_doc["message"]["cst_id"].InnerText = (job.FromCstID == null ? "" : job.FromCstID);
                xml_doc["message"]["unit_id"].InnerText = GetProcessUnit(job, eqpName); //for t3 add Unit_id 2015-9-16 tom
                xml_doc["message"]["slot_no"].InnerText = (job.FromSlotNo == null ? "" : job.FromSlotNo);
                string jobType = "";
                string subStrateType = "";
                if (job != null)
                {
                    switch (job.JobType)
                    {
                        case eJobType.TFT: jobType = "T";
                            break;
                        case eJobType.CF: jobType = "C";
                            break;
                        default: jobType = "D";
                            break; 
                    }
                    switch (job.SubstrateType)
                    {
                        case eSubstrateType.Chip: subStrateType = "PANEL";
                            break;
                        case eSubstrateType.Block: subStrateType = "BLOCK";
                            break;
                        case eSubstrateType.Glass: subStrateType = "GLASS";
                            break;
                        default:
                            subStrateType = "GLASS";
                            break;
                    }
                }
                xml_doc["message"]["productspectype"].InnerText = jobType;//(job == null ? "" : job.JobType.ToString());
                xml_doc["message"]["substrate_type"].InnerText = subStrateType;//(job == null ? "" : job.SubstrateType.ToString());
                xml_doc["message"]["operator_id"].InnerText = "";
                xml_doc["message"]["recipe_id"].InnerText = (eqp == null ? "" : eqp.File.CurrentRecipeID);  //Jun Modify 20150401 上報機台Current Recipe
                xml_doc["message"]["fab_area"].InnerText = "Cell";
                xml_doc["message"]["owner_id"].InnerText = (job.MesProduct == null ? "DEFAULT" : job.MesProduct.OWNERID);
                if (string.IsNullOrEmpty(xml_doc["message"]["owner_id"].InnerText)) xml_doc["message"]["owner_id"].InnerText = "DEFAULT";
                xml_doc["message"]["eqp_start_time"].InnerText = dateFormat("20" + eqpStartTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
                xml_doc["message"]["eqp_end_time"].InnerText = dateFormat("20" + eqpEndTime);   //20yyMMddHHmmss -> yyyy-MM-dd HH:mm:ss
               // xml_doc["message"]["track_out_time"].InnerText = eqpReportTime;

                XmlNode paraListNode = xml_doc["message"]["param_list"];
                XmlNode paraNodeClone = paraListNode["param"].Clone();
                paraListNode.RemoveAll();

                foreach (var key in edaLis)
                {
                    foreach (string prarValue in edaLis[key.Key])
                    {
                        XmlNode paraNode = paraNodeClone.Clone();

                        string[] p = prarValue.Split(';');

                        if (p.Length == 2)
                        {
                            if (edaLis[key.Key].Count > 1)
                            {
                                //paraNode["param_group"].InnerText = key.Key.ToString();
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }
                            else
                            {
                                //paraNode["param_group"].InnerText = "NA";
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }

                            paraListNode.AppendChild(paraNode);
                        }
                    }
                }

                xMessage msg = new xMessage();
                msg.FromAgent = "EDAAgent";
                msg.ToAgent = "EDAAgent";
                msg.Data = xml_doc.OuterXml;
                msg.TransactionID = trxID;
                PutMessage(msg);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME {1}] [BCS -> EDA][{0}] " + MethodBase.GetCurrentMethod().Name + " OK .",
                    trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion


        #region 1.4.	MODEDCPANELEND
        /// <summary>
        /// EDA MessageSet  : MODEDCPANELEND to EDA
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="job">JOB(WIP)</param>
        private void MODEDCPANELEND(string trxID, string lineName, string eqpName, string eqpStartTime, string eqpEndTime, string eqpReportTime, Job job, Dictionary<string, List<string>> edaLis)
        {
            try
            {

                Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpName);

                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                    return;
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("MODEDCPANELEND") as XmlDocument;
                xml_doc["message"]["message_id"].InnerText = "MODEDCPANELEND";
                xml_doc["message"]["timestamp"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                xml_doc["message"]["process_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME);
                xml_doc["message"]["pfcd"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME);
                xml_doc["message"]["product_group"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP);
                xml_doc["message"]["line_id"].InnerText = lineName;
                xml_doc["message"]["eqp_id"].InnerText = eqpName;
                xml_doc["message"]["cst_id"].InnerText = (job.FromCstID == null ? "" : job.FromCstID);
                xml_doc["message"]["unit_id"].InnerText = GetProcessUnit(job, eqpName); //for t3 add Unit_id 2015-9-16 tom
                xml_doc["message"]["slot_no"].InnerText = (job.FromSlotNo == null ? "" : job.FromSlotNo);
                xml_doc["message"]["operator_id"].InnerText = "";
                xml_doc["message"]["recipe"].InnerText = (eqp == null ? "" : eqp.File.CurrentRecipeID);
                xml_doc["message"]["fab_area"].InnerText = "Module";
                xml_doc["message"]["owner_id"].InnerText = (job.MesProduct == null ? "" : job.MesProduct.OWNERID);
                xml_doc["message"]["eqp_start_time"].InnerText = dateFormat("20" + eqpStartTime);   
                xml_doc["message"]["eqp_end_time"].InnerText = dateFormat("20" + eqpEndTime);

                XmlNode paraListNode = xml_doc["message"]["param_list"];
                XmlNode paraNodeClone = paraListNode["param"].Clone();
                paraListNode.RemoveAll();

                foreach (var key in edaLis)
                {
                    foreach (string prarValue in edaLis[key.Key])
                    {
                        XmlNode paraNode = paraNodeClone.Clone();

                        string[] p = prarValue.Split(';');

                        if (p.Length == 2)
                        {
                            if (edaLis[key.Key].Count > 1)
                            {
                                paraNode["param_group"].InnerText = key.Key.ToString();
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }
                            else
                            {
                                paraNode["param_group"].InnerText = "NA";
                                paraNode["param_name"].InnerText = p[0].ToString().Replace(" ", "_"); //modify 2016/03/14 cc.kuang
                                paraNode["param_value"].InnerText = p[1].ToString();
                                if (paraNode["param_value"].InnerText.Trim().Length == 0) //modify 2016/03/14 cc.kuang
                                    paraNode["param_value"].InnerText = "NA";
                            }

                            paraListNode.AppendChild(paraNode);
                        }
                    }
                }
                xMessage msg = new xMessage();
                msg.FromAgent = "EDAAgent";
                msg.ToAgent = "EDAAgent";
                msg.Data = xml_doc.OuterXml;
                msg.TransactionID = trxID;
                PutMessage(msg);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME {1}] [BCS -> EDA][{0}] " + MethodBase.GetCurrentMethod().Name + " OK .",
                    trxID, lineName));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region 1.5.	EDCGLASSRUNEND
        /// <summary>
        /// EDA MessageSet  : EDCGLASSRUNEND to EDA
        /// </summary>
        /// <param name="trxID">Transaction ID (yyyyMMddHHmmssffff)</param>
        /// <param name="lineName">Line ID</param>
        /// <param name="eqpName">EQP ID</param>
        /// <param name="job">JOB(WIP)</param>
        public void EDCGLASSRUNEND(string trxID, string lineName, string eqpName, Job job)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line.File.HostMode == eHostMode.OFFLINE)
                    return;
                IServerAgent agent = GetServerAgent2();
                XmlDocument xml_doc = agent.GetTransactionFormat("EDCGLASSRUNEND") as XmlDocument;
                xml_doc["message"]["message_id"].InnerText = "EDCGLASSRUNEND";
                xml_doc["message"]["timestamp"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //xml_doc["message"]["oriented_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDSITE);  //<!--Glass第一次投入時的Site -->
                //xml_doc["message"]["oriented_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].ORIENTEDFACTORYNAME); ; //<!-- Glass第一次投入时的厂别-->
                //xml_doc["message"]["current_site"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTSITE); ; //<!--当前Glass所在的Site -->
                //xml_doc["message"]["current_factory_name"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].CURRENTFACTORYNAME); //<!-- 当前Glass所在的厂别-->
                xml_doc["message"]["line_id"].InnerText = (lineName ==null ? "" : lineName);
                xml_doc["message"]["glass_id"].InnerText = (job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID);
                xml_doc["message"]["product_group"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECGROUP);
                xml_doc["message"]["lot_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LOTNAME);
                xml_doc["message"]["start_time"].InnerText = job.JobProcessStartTime.ToString("yyyy-MM-dd HH:mm:ss");
                xml_doc["message"]["end_time"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //Store Time is End Time
                xml_doc["message"]["cst_id"].InnerText = (job.MesCstBody.CARRIERNAME == null ? "" : job.MesCstBody.CARRIERNAME);
                
               
                xml_doc["message"]["slot_id"].InnerText = (job.MesProduct.POSITION == null ? "" : job.MesProduct.POSITION);
                xml_doc["message"]["owner_id"].InnerText = (job.MesProduct == null ? "" : job.MesProduct.OWNERID);

                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                   line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    xml_doc["message"]["pfcd"].InnerText = job.MesCstBody.PLANNEDPRODUCTSPECNAME;
                    xml_doc["message"]["process_id"].InnerText = job.MesCstBody.PLANNEDPROCESSOPERATIONNAME;
                }
                else
                {
                    xml_doc["message"]["pfcd"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME);
                    xml_doc["message"]["process_id"].InnerText = (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME);
                }

                XmlNode eqpListNode = xml_doc["message"]["eqp_list"];
                XmlNode eqpNodeClone = eqpListNode["eqp"].Clone();
                eqpListNode.RemoveAll();

                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(lineName))
                {
                    if (eqp.Data.NODEATTRIBUTE == "LU" || eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UD")
                        continue;
                    XmlNode eqpNode = eqpNodeClone.Clone();
                    eqpNode["eqp_id"].InnerText = eqp.Data.NODEID; //<eqp_id>STP01</eqp_id>
                    //eqpNode["eqp_status"].InnerText = eqp.File.MESStatus.ToString(); //<eqp_status>RUN</eqp_status>

                    try {
                        //if (line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP || line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE )
                        //    eqpNode["recipe_id"].InnerText = job.PPID;
                        //else
                            eqpNode["recipe_id"].InnerText = job.PPID.Substring(eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN);  //<recipe_id>RecipeAAA</ recipe_id>
                    } catch (System.ArgumentOutOfRangeException) {
                        eqpNode["recipe_id"].InnerText = (job.PPID == null ? "" : job.PPID);
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("Substring Job PPID Length Error Index=[0],length=[1]", eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN));
                    }
                    //eqpNode["recipe_id"].InnerText = (job.PPID == null ? "" : job.PPID); ; //<recipe_id>RecipeAAA</ recipe_id>
                    if (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                    {
                        if ((job.JobProcessFlows[eqp.Data.NODEID].StartTime == DateTime.MinValue) || (job.JobProcessFlows[eqp.Data.NODEID].EndTime == DateTime.MinValue))
                        {
                            continue;
                        }
                        eqpNode["eqp_slot_id"].InnerText = job.JobProcessFlows[eqp.Data.NODEID].SlotNO == null ? "" : job.JobProcessFlows[eqp.Data.NODEID].SlotNO; //<eqp_slot_id>01</eqp_slot_id >/*For buffer or oven*/
                        eqpNode["start_time"].InnerText = job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyyy-MM-dd HH:mm:ss"); ;//<start_time>2007-09-03 19:24:47</start_time>

                        eqpNode["end_time"].InnerText = job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyyy-MM-dd HH:mm:ss"); ;//<end_time>2007-09-03 19:34:47</end_time>

                    }
                    else
                    {
                        continue;
                    }

                    #region Add Sub Eqp
                    XmlNode unitListNode = eqpNode["sub_eqp_list"];
                    XmlNode unitNodeClone = unitListNode["sub_eqp"].Clone();

                    unitListNode.RemoveAll();

                    //#region CVD DRY 
                    //if (line.Data.LINETYPE == eLineType.ARRAY.CVD && line.Data.LINETYPE == eLineType.ARRAY.DRY_ADP && line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL)
                    {
                        List<ProcessFlow> unitProcessFlows = new List<ProcessFlow>();
                        SortUnitProcessFlow(unitProcessFlows, job.JobProcessFlows[eqp.Data.NODEID]);

                        foreach (ProcessFlow flow in unitProcessFlows)
                        {
                            Unit unit = ObjectManager.UnitManager.GetUnit(flow.MachineName);
                            if (unit != null)
                            {
                                XmlNode unitNode = unitNodeClone.Clone();
                                unitNode["sub_eqp_id"].InnerText = unit.Data.UNITID;//<sub_eqp_id>CH01</sub_eqp_id>
                                //unitNode["sub_eqp_status"].InnerText = unit.File.MESStatus.ToString();//<sub_eqp_status>RUN</sub_eqp_status>
                                unitNode["recipe_id"].InnerText = (job.PPID == null ? "" : job.PPID); ;//<recipe_id>RecipeAAA1</ recipe_id>

                                if ((flow.StartTime == DateTime.MinValue) || (flow.EndTime == DateTime.MinValue))//日期格式不正确的不上报
                                {
                                    continue;
                                }
                                unitNode["sub_eqp_slot_id"].InnerText = string.IsNullOrEmpty(flow.SlotNO) == true ? "" : flow.SlotNO;//<sub_eqp_slot_id>01</ sub_eqp_slot_id >/*For buffer or oven*/                       
                                unitNode["start_time"].InnerText = flow.StartTime.ToString("yyyy-MM-dd HH:mm:ss"); ;//<start_time>2007-09-03 19:24:47</start_time>
                                unitNode["end_time"].InnerText = flow.EndTime.ToString("yyyy-MM-dd HH:mm:ss"); ;//<end_time>2007-09-03 19:34:47</end_time>
                                unitListNode.AppendChild(unitNode);

                            }

                        }
                    }
                    #endregion
                    eqpListNode.AppendChild(eqpNode);
                  
                }

                xMessage msg = new xMessage();
                //msg.FromAgent = "EDAAgent";
                //msg.ToAgent = "EDAAgent";
                msg.FromAgent = "EDAAgent2";
                msg.ToAgent = "EDAAgent2";
                msg.Data = xml_doc.OuterXml;
                msg.TransactionID = trxID;
                //msg.UseField.Add("TargetSubject", RunEndTargetSubject);  //EDA  EDCGLASSRUNEND 需要使用不同的TargetSubject  20150227 Tom 
                PutMessage(msg);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME {1}] [BCS -> EDA][{0}] " + MethodBase.GetCurrentMethod().Name + " OK .",
                                trxID, lineName));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion



        #region Public Fucntion For EDA
        private void SortUnitProcessFlow(List<ProcessFlow> flows,ProcessFlow nodeProcessFlow)
        {
            try
            {
                //modify 2016/03/14 cc.kuang
                /*
                if(nodeProcessFlow.UnitProcessFlows!=null && nodeProcessFlow.UnitProcessFlows.Count>0)
                {
                    flows.AddRange(nodeProcessFlow.UnitProcessFlows.Values.ToList());
                }
                */
                if(nodeProcessFlow.ExtendUnitProcessFlows!=null&& nodeProcessFlow.ExtendUnitProcessFlows.Count>0)
                {
                    flows.AddRange(nodeProcessFlow.ExtendUnitProcessFlows);
                }
                flows.Sort((left, right) =>
                {
                    if (left.StartTime > right.StartTime)
                        return 1;
                    else if (left.StartTime == right.StartTime)
                        return 0;
                    else
                        return -1;
                });
            
            }
            catch (System.Exception ex)
            {
            	NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }


        //yyyyMMddHHmmss
        private static string dateFormat(string value)
        {
            if (value.Length != "yyyyMMddHHmmss".Length)
            {
                return "";
                //throw new FormatException(String.Format("The strlen of arg must be {0}", "yyyyMMddHHmmss".Length), null);
            }

            return string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                value.Substring(0, 4),
                value.Substring(4, 2),
                value.Substring(6, 2),
                value.Substring(8, 2),
                value.Substring(10, 2),
                value.Substring(12, 2));
        }

        object _syncSeqNo = new object();
        private string GetSeqNo()
        {
            try
            {
                lock (_syncSeqNo)
                {
                    seqNo++;
                    if (seqNo >= 1000)
                        seqNo = 0;
                }
                return seqNo.ToString().PadLeft(3, '0');
            }
            catch (Exception ex)
            {                
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return 0.ToString().PadLeft(3, '0');
            }
        }

        /// <summary>
        /// 获取Job 所经过的当前Equipment的Unit id 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="nodeID"> current Node id</param>
        /// <returns></returns>
        private string GetProcessUnit(Job job, string nodeID) {
            try
            {
                List<string> unitNames=new List<string>();
                StringBuilder sb = new StringBuilder();
                List<ProcessFlow> processFlows=new List<ProcessFlow>();
                if (job != null) {
                    if (job.JobProcessFlows != null) {

                        if (job.JobProcessFlows.ContainsKey(nodeID)) {

                            SortUnitProcessFlow(processFlows, job.JobProcessFlows[nodeID]);

                            foreach (ProcessFlow flow in processFlows){

                                //if (unitNames.Contains(flow.MachineName)==false) { //mark for EDC unit list report can use repeat unit 2016/04/08 cc.kuang
                                    unitNames.Add(flow.MachineName);
                                    sb.AppendFormat("{0},", flow.MachineName);
                                //}
                            }
                            if (sb.Length > 0) {
                                return sb.ToString(0, sb.Length - 1);
                            }
                        }
                    }
                }
                return "";
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "";
            }
        }
        
        #endregion


    }
}
