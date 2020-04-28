using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MesSpec;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using System.Xml;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public partial class CFSpecialService : AbstractService
    {
        private const string PermissionRequestTimeout = "PermissionRequestTimeout";
        private const string JobDataRequestReportReplyTimeOut = "JobDataRequestReportReplyTimeOut";
        private const string ShortCutModeChangeReportTimeout = "ShortCutModeChangeReportTimeout";
        private const string GlassOutResultCommandTimeout = "GlassOutResultCommandTimeout";
        private const string GlassInCheckRequestTimeout = "GlassInCheckRequestTimeout";         

        #region [9.2.3 Short Cut Mode Change Report]
        public void ShortCutModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                {
                    ShortCutModeChangeReportUpdate(inputData, "ShortCutModeChangeReport");
                    return;
                }

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eShortCutMode cfShortCutMode;
                cfShortCutMode = (eShortCutMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    ShortCutModeChangeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                #endregion

                #region [Reply EQ]
                ShortCutModeChangeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [更新Line資訊]
                //save data in progarm
                lock (line) line.File.CFShortCutMode = cfShortCutMode;
                //save progarm data in file
                ObjectManager.LineManager.EnqueueSave(line.File);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] SHORT CUT MODE[{2}]=[{3}].", eqpNo, inputData.TrackKey, (int)cfShortCutMode, cfShortCutMode.ToString()));
                #endregion

                #region [Report MES]
                Invoke(eServiceName.MESService, "CFShortCutModeChangeRequest", new object[] { inputData.TrackKey, line.Data.LINEID, cfShortCutMode == eShortCutMode.Enable ? "Y" : "N" });
                #endregion

                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });//OPI新增ShortCutMode顯示 add by marine

                ObjectManager.LineManager.RecordLineHistory(inputData.TrackKey, line);//Line His 新增儲存ShortCutMode add by jm.pan

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ShortCutModeChangeReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_ShortCutModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, ShortCutModeChangeReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(ShortCutModeChangeReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT=[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ShortCutModeChangeReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SHORT CUT MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                ShortCutModeChangeReportReply(trackKey, eBitResult.OFF, sArray[0]);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ShortCutModeChangeReportUpdate(Trx inputData, string log)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eShortCutMode cfShortCutMode;
                cfShortCutMode = (eShortCutMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                if (eqp.Data.NODENO != "L21") return;
               
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {3} SET SHORTCUTMODE{2} START.", eqp.Data.NODENO, inputData.TrackKey, "", log));

                eShortCutMode oldValue = line.File.CFShortCutMode;

                lock (line) line.File.CFShortCutMode = cfShortCutMode;
                //save progarm data in file
                ObjectManager.LineManager.EnqueueSave(line.File);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPORT, SHORT CUT MODE =[{2}].", eqpNo, inputData.TrackKey, cfShortCutMode));

                if (oldValue != cfShortCutMode)
                {
                    #region [Report MES]
                    Invoke(eServiceName.MESService, "CFShortCutModeChangeRequest", new object[] { inputData.TrackKey, line.Data.LINEID, cfShortCutMode == eShortCutMode.Enable ? "Y" : "N" });
                    #endregion
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {3} SET SHORTCUTMODE{2} END.", eqp.Data.NODENO, inputData.TrackKey, "", log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        #endregion

        #region [9.2.4 Permission Request Report]
        public void PermissionRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string cstseq = inputData.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
                string jobseq = inputData.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [取得Job資訊]
                Job job = ObjectManager.JobManager.GetJob(cstseq, jobseq);
                if (job == null)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PERMISSION REQUEST REPORT, CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}], JOB NOT IN WIP.",
                        eqpNo, inputData.TrackKey, cstseq, jobseq));
                    return;
                }
                string productname = job.MesProduct.PRODUCTNAME;
                string hostproductname = job.MesProduct.PRODUCTNAME;   //VCR READ GLASS ID
                string processlinename = line.Data.NEXTLINEID;
                string totalpitch = job.CfSpecial.EQPFlag2.TotalPitchOfflineInspectionFlag;
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] PERMISSION REQUEST REPORT BIT (OFF), CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstseq, jobseq));
                    PermissionRequestReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo, eReturnCode1.Unknown);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] PERMISSION REQUEST REPORT BIT (ON), CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstseq, jobseq));                   
                }
                #endregion

                #region [Reply EQ]
                ObjectManager.JobManager.EnqueueSave(job);
                if (line.File.CFShortCutMode != eShortCutMode.Enable && job.JobJudge != "1"
                    && job.CfSpecial.EQPFlag2.TotalPitchOfflineInspectionFlag != "1"
                    && job.FirstRunFlag != "1" && job.JobType != eJobType.DM && job.SamplingSlotFlag == "1")
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS][{1}] CAN NOT DO CF SHORTCUT PERMIT REQUEST, CFSHORTCUTMODE=[{2}], JOBJUDGE=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, line.File.CFShortCutMode, job.JobJudge));
                    //PermissionRequestReportReply(inputData.TrackKey, eBitResult.ON, eqpNo, eReturnCode1.NG);

                    // 更新 Job WIP
                    lock (job)
                    {
                        job.CfSpecial.PermitFlag = "N";
                    }
                    ObjectManager.JobManager.EnqueueSave(job);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS][{1}] CF SHORTCUT PERMIT REQUEST NOT REPORT MES, CFSORTCUTMODE[{2}], JOBJUDGE[{3}]",
                       inputData.Metadata.NodeNo, inputData.TrackKey, line.File.CFShortCutMode, job.JobJudge));  
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] DO CF SHORTCUT PERMIT REQUEST, CFSHORTCUTMODE=[{2}], JOBJUDGE=[{3}]",
                          inputData.Metadata.NodeNo, inputData.TrackKey, line.File.CFShortCutMode, job.JobJudge));
                    //PermissionRequestReportReply(inputData.TrackKey, eBitResult.ON, eqpNo, eReturnCode1.OK);

                    #region [Report MES]
                    object[] _data = new object[6]
                    { 
                        inputData.TrackKey, /*0   TrackKey*/
                        eqp.Data.LINEID,    /*1   LineName*/  //這邊要用本 Line，還是用次 Line ?
                        productname,        /*2   ProductName*/  //這邊要用本 Line，還是用次 Line ?
                        hostproductname,    /*3   HostProductName*/
                        processlinename,    /*4   ProcessLineName*/
                        totalpitch,         /*5   EQPFlag2.TotalPitchOfflineInspectionFlag*/
                    };
                    //呼叫MES方法
                    Invoke(eServiceName.MESService, "CFShortCutPermitRequest", _data);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CF SHORTCUT PERMIT REQUEST REPORT MES, SHORTCUTGLASSFLAG=[{2}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, job.CfSpecial.EQPFlag2.ShortCutGlassFlag_ForCV06));

                    #region [Create Timer]
                    string timerID = string.Format("{0}_{1}_PermissionRequestReport", eqp.Data.NODENO, inputData.TrackKey);
                    if (this.Timermanager.IsAliveTimer(timerID))
                    {
                        Timermanager.TerminateTimer(timerID);
                    }
                    Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(PermissionRequestReportTimeoutForMES), eqpNo);
                    #endregion

                    #endregion
                }
                #endregion


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PermissionRequestReportReply(string trxID, eBitResult bitResut, string eqpNo, eReturnCode1 returncode)
        {
            try
            {
                string trxName = string.Format("{0}_PermissionRequestReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                if (bitResut == eBitResult.ON)
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returncode).ToString();
                else
                    outputdata.EventGroups[0].Events[0].IsDisable = true;

                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, PermissionRequestTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PermissionRequestReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, PERMISSION REQUEST REPORT REPLY SET BIT =[{2}], PERMISSION REQUEST REPORT REPLY RESULT =[{3}]",
                    eqpNo, trxID, bitResut, returncode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PermissionRequestReportTimeoutForMES(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                PermissionRequestReportReply(trackKey, eBitResult.OFF, sArray[0], eReturnCode1.NG);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] BC REPLY, PERMISSION REQUEST REPORT REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PermissionRequestReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                PermissionRequestReportReply(trackKey, eBitResult.OFF, sArray[0], eReturnCode1.NG);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, PERMISSION REQUEST REPORT REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.5 Glass Out Result Command]
        public void GlassOutResultCommand(string trxID, Job job, ePermitFlag permitFlag, string samplingValue)
        {
            try
            {
                //trxID = this.CreateTrxID();

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                string equipmentNodeNo = string.Empty;
                IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment equipment in equipments)
                {
                    if (equipment.Data.NODEATTRIBUTE.ToUpper() == "BF")
                        equipmentNodeNo = equipment.Data.NODENO;
                }

                Equipment eqp_CV07 = ObjectManager.EquipmentManager.GetEQP("L23");
                if (eqp_CV07 == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", "L23"));
                #endregion

                #region [寫入PLCAgent Data]
                string trxName = string.Format("{0}_GlassOutResultCommand", equipmentNodeNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)//Check Trx
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN NOT FOUND TRX {0}", trxName));
                    return;
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                eGlassOutResult glassOutResult = new eGlassOutResult();

                #region Recipe Parameter Check
                bool recipeParameterCheck = false;
                if (permitFlag == ePermitFlag.Y)
                {                    
                    bool recipeParameterSkipCheck = ParameterManager["SHORTCUT_RECIPE_PARAMETER_SKIP_CHECK"].GetBoolean();
                    if (job.CfSpecial.CFShortCutrecipeParameterRequestResult == eRecipeCheckResult.OK || recipeParameterSkipCheck)
                    { recipeParameterCheck = true; }
                    else
                    { recipeParameterCheck = false; }
                }
                #endregion

                #region Recipe ID Check
                bool recipeIDCheck = false;
                if (permitFlag == ePermitFlag.Y)
                {                    
                    bool recipeIDSkipCheck = ParameterManager["SHORTCUT_RECIPE_ID_SKIP_CHECK"].GetBoolean();
                    if (job.CfSpecial.CfShortCutRecipeIDCheckResult == eRecipeCheckResult.OK || recipeIDSkipCheck)
                    { recipeIDCheck = true; }
                    else
                    { recipeIDCheck = false; }
                }
                #endregion

                switch (permitFlag.ToString())
                {
                    case "Y":
                        if (job.JobJudge == "1")//JobJudge=OK
                        {
                            if (recipeIDCheck &&
                                recipeParameterCheck &&
                                eqp_CV07.File.CV07Status == eBitResult.ON &&
                                eqp_CV07.File.NextLineBCStatus == eBitResult.ON &&
                                NextLineStatusCkeck(eqp_CV07) &&
                                line.File.CFShortCutMode == eShortCutMode.Enable)
                            {
                                if (job.CfSpecial.NextLineJobData != null)
                                {
                                    glassOutResult = eGlassOutResult.OK_ToShortCut;
                                    samplingValue = string.Empty;
                                    #region [當玻璃確定要做 Short Cut 時，把 Job Data 傳給下條 Line]
                                    Invoke(eServiceName.ActiveSocketService, "JobShortCutPermit", new object[] { job, job.CfSpecial.NextLineJobData });
                                    #endregion
                                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LINE[{2}] DOWNLOAD NEW JOB DATA TO NEXT LINE[{3}].",
                                        equipmentNodeNo, trxID, line.Data.LINEID, line.Data.NEXTLINEID));
                                }
                                else
                                {
                                    glassOutResult = eGlassOutResult.NG;
                                    samplingValue = string.Empty;
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{0}] LINE[{1}] CAN'T DOWNLOAD NEW JOB DATA TO NEXT LINE[{2}].",
                                        equipmentNodeNo, trxID, line.Data.LINEID, line.Data.NEXTLINEID));
                                }
                            }
                            else
                            {
                                glassOutResult = eGlassOutResult.NG;
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LINE[{2}] JOB CAN'T GO TO NEXT LINE[{3}]" +
                                " CF Short Cut Mode [{4}], Next Line BC Status [{5}], CV07 Status [{6}]."
                                , equipmentNodeNo, trxID, line.Data.LINEID, line.Data.NEXTLINEID, line.File.CFShortCutMode.ToString(), eqp_CV07.File.NextLineBCStatus.ToString(), eqp_CV07.File.CV07Status.ToString()));
                            }
                        }
                        else
                        {
                            glassOutResult = eGlassOutResult.NG;
                        }

                        break;
                    case "N":
                        if (job.JobJudge == "3")//JobJudge=RW
                        {
                            glassOutResult = eGlassOutResult.NG;//JobJudge=RW,回NG
                        }
                        else
                        {
                            glassOutResult = eGlassOutResult.OK_ToUnloader;
                        }
                        samplingValue = job.CfSpecial.SamplingValue;
                        break;
                    case "F":
                        glassOutResult = eGlassOutResult.OK_ToUnloader;
                        samplingValue = job.CfSpecial.SamplingValue;     // SamplingValue 此值為空機台則將此片用做填卡匣，非空則不能混卡匣。
                        break;
                }
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)glassOutResult).ToString();
                outputdata.EventGroups[0].Events[0].Items[1].Value = job.CassetteSequenceNo;
                outputdata.EventGroups[0].Events[0].Items[2].Value = job.JobSequenceNo;
                outputdata.EventGroups[0].Events[0].Items[3].Value = samplingValue;
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [{2}], RESULT [{3}], PERMITFLAG [{4}], SAMPLING VALUE [{5}], CASSETTESEQUENCENO [{6}], JOBSEQUENCENO [{7}].",
                    equipmentNodeNo, trxID, eBitResult.ON, glassOutResult, permitFlag, samplingValue, job.CassetteSequenceNo, job.JobSequenceNo));
                #endregion

                #region[Time Out]
                string timeoutName = string.Format("{0}_{1}", equipmentNodeNo, GlassOutResultCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(GlassOutResultCommandTimeoutForEQP), trxID);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassOutResultCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT [ON]. GLASS OUT RESULT COMMAND REPLY [{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, returnCode));
                }
                #endregion

                #region [Reply EQ]
                string trxName = string.Format("{0}_GlassOutResultCommand", eqp.Data.NODENO);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = "0";
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqp.Data.NODENO, GlassOutResultCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT [OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassOutResultCommandTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string trxName = string.Format("{0}_GlassOutResultCommand", sArray[0]);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = "0";
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, GLASS OUT RESULT COMMAND TIMEOUT SET BIT (OFF).",
                    sArray[1], sArray[0]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public bool NextLineStatusCkeck(Equipment eqp)
        {
            try
            {
                if (eqp.File.CV01Status == eEQPStatus.STOP || eqp.File.CleanerStatus == eEQPStatus.STOP || eqp.File.CV02Status == eEQPStatus.STOP)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.SETUP || eqp.File.CleanerStatus == eEQPStatus.SETUP || eqp.File.CV02Status == eEQPStatus.SETUP)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.PAUSE || eqp.File.CleanerStatus == eEQPStatus.PAUSE || eqp.File.CV02Status == eEQPStatus.PAUSE)
                { return false; }
                if (eqp.File.CV01Status == eEQPStatus.NOUNIT || eqp.File.CleanerStatus == eEQPStatus.NOUNIT || eqp.File.CV02Status == eEQPStatus.NOUNIT)
                { return false; }
                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
 
        }
        #endregion

        #region [9.2.6 Glass In Check Request]
        public void GlassInCheckRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                string cassetteSeqNo = string.Empty;
                string jobSeqNo = string.Empty;
                string jobID = string.Empty;
                string err = string.Empty;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                cassetteSeqNo = inputData.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
                jobSeqNo = inputData.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
                jobID = inputData.EventGroups[0].Events[0].Items["Glass/Chip/MaskID/BlockID"].Value;
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [取得 Job WIP]
                Job job = new Job();
                job = ObjectManager.JobManager.GetJob(cassetteSeqNo, jobSeqNo);
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] GLASS IN CHECK REQUEST BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassInCheckRequestReply(inputData.TrackKey, job, eBitResult.OFF, eReturnCode1.Unknown, "", eqpNo);
                    return;
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] GLASS IN CHECK REQUEST BIT (ON)", inputData.Metadata.NodeNo, inputData.TrackKey));
                #endregion

                #region [Reply EQ]  判斷後回覆OK/NG給機台
                if (job != null)
                {
                    if (job.CfSpecial.CfShortCutRecipeIDCheckResult == eRecipeCheckResult.OK && job.CfSpecial.CfShortCutWIPCheck)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECIPE ID CHECK OK, RETURN CODE =[{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, eReturnCode1.OK));
                        GlassInCheckRequestReply(inputData.TrackKey, job, eBitResult.ON, eReturnCode1.OK, "", eqpNo);
                    }
                    else
                    {
                        if (job.CfSpecial.CfShortCutRecipeIDCheckResult != eRecipeCheckResult.OK)
                        {
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECIPE ID CHECK NG, RETURN CODE =[{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, eReturnCode1.NG));
                        }

                        if (!job.CfSpecial.CfShortCutWIPCheck)
                        {
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB WIP DUPLICATE., RETURN CODE =[{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, eReturnCode1.NG));
                        }
                        GlassInCheckRequestReply(inputData.TrackKey, job, eBitResult.ON, eReturnCode1.NG, "", eqpNo);
                        return;
                    }
                }
                else
                {
                    GlassInCheckRequestReply(inputData.TrackKey, job, eBitResult.ON, eReturnCode1.NG, "", eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT (ON), RETURN CODE =[{2}] CAN'T FOUND JOB WIP. CASSETTE SEQUENCE NO=[{3}], JOB SEQUENCE NO=[{4}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, eReturnCode1.NG, cassetteSeqNo, jobSeqNo));
                    return;
                }
                #endregion

                #region [Report MES]
                object[] _data = new object[3]
                { 
                    inputData.TrackKey,      /*0  TrackKey*/
                    eqp.Data.LINEID,         /*1  LineName*/
                    job                      /*2  Job*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "GlassProcessStarted", _data);
                #endregion

                #region [Delete Up Line Job WIP]
                Invoke(eServiceName.PassiveSocketService, "JOBShortCutGlassIn", new object[] { eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID });
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> BCS][{1}] DELETE JOB WIP.", line.Data.LINEID, inputData.TrackKey, eReturnCode1.OK));
                #endregion

                #region Check MPLC Interlock
                //if (job.JobType != eJobType.DM)
                //{
                //    if (line != null && line.Data.FABTYPE.ToUpper() != "CF")
                //        Invoke(eServiceName.SubBlockService, "CheckMplcBlock", new object[] { inputData.Metadata.NodeNo, "0", "RECEIVE" });
                //}
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassInCheckRequestReply(string trxID, Job job, eBitResult bitResut, eReturnCode1 returnCode, string newJobData, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_GlassInCheckRequestReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (bitResut == eBitResult.ON)
                {
                    //寫入Return Code
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString();

                    if (returnCode == eReturnCode1.OK)
                    {
                        //寫入Job Data
                        Event eVent = outputdata.EventGroups[0].Events[1];

                        eVent.Items["CassetteSequenceNo"].Value = job.CassetteSequenceNo;   //INT
                        eVent.Items["JobSequenceNo"].Value = job.JobSequenceNo;   //INT
                        eVent.Items["GroupIndex"].Value = job.GroupIndex;   //INT
                        eVent.Items["ProductType"].Value = job.ProductType.Value.ToString();   //INT
                        eVent.Items["CSTOperationMode"].Value = ((int)job.CSTOperationMode).ToString();   //INT
                        eVent.Items["SubstrateType"].Value = ((int)job.SubstrateType).ToString();   //INT
                        eVent.Items["CIMMode"].Value = ((int)job.CIMMode).ToString();   ////INT
                        eVent.Items["JobType"].Value = ((int)job.JobType).ToString();   ////INT
                        eVent.Items["JobJudge"].Value = job.JobJudge;   ////INT
                        eVent.Items["SamplingSlotFlag"].Value = job.SamplingSlotFlag;   ////INT
                        eVent.Items["FirstRunFlag"].Value = job.FirstRunFlag;   ////INT
                        eVent.Items["JobGrade"].Value = job.JobGrade;   //ASCII
                        eVent.Items["Glass/Chip/MaskID/BlockID"].Value = job.GlassChipMaskBlockID;   //ASCII
                        eVent.Items["PPID"].Value = job.PPID;   //ASCII

                        #region CF Special Job Data
                        eVent.Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                        eVent.Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                        eVent.Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT

                        eVent.Items[eJOBDATA.InspJudgedData1].Value = job.InspJudgedData;   //BIN
                        eVent.Items[eJOBDATA.InspJudgedData2].Value = job.InspJudgedData2;  //BIN
                        eVent.Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                        eVent.Items[eJOBDATA.CFSpecialReserved].Value = job.CFSpecialReserved;  //BIN
                        eVent.Items[eJOBDATA.EQPFlag1].Value = job.EQPFlag; //BIN
                        eVent.Items[eJOBDATA.EQPFlag2].Value = job.EQPFlag2;    //BIN
                        eVent.Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT
                        eVent.Items[eJOBDATA.COAVersion].Value = job.CfSpecial.COAversion;   //ASCII.ArrayPhotoPre_InlineID.Trim().ToUpper()].Value;   //INT
                        eVent.Items["TargetCassetteID"].Value = job.CfSpecial.TargetCSTID;
                        eVent.Items["ProcessBackUp"].Value = job.CFProcessBackUp;
                        eVent.Items["TargetPortNo"].Value = job.CfSpecial.TargetPortNo;
                        eVent.Items["TargetSlotNo"].Value = job.CfSpecial.TargetSlotNo;
                        if (job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID != string.Empty)
                            eVent.Items["ArrayPhotoPre-InlineID"].Value =
                            ConstantManager["ARRAY_PHOTO_PRE_INLINE_ID"][job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID.Trim().ToUpper()].Value;
                        eVent.Items["OvenHPSlotNumber"].Value = job.CfSpecial.OvenHPSlotNumber;
                        eVent.Items["InlineReworkMaxCount"].Value = job.CfSpecial.InlineReworkMaxCount;
                        eVent.Items["InlineReworkRealCount"].Value = job.CfSpecial.InlineReworkRealCount;
                        eVent.Items["MarcoReserveFlag"].Value = job.CFMarcoReserveFlag;
                        eVent.Items["SamplingValue"].Value = job.CfSpecial.SamplingValue;

                        #endregion
                    }
                    else
                    {
                        outputdata.EventGroups[0].Events[1].IsDisable = true;
                    }
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].IsDisable = true;
                }
                //寫入bit
                outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)bitResut).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, GlassInCheckRequestTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(GlassInCheckRequestTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS IN CHECK REQUEST REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassInCheckRequestTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string trxName = string.Format("{0}_GlassInCheckRequestReportReply", sArray[0]);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                //寫入bit
                outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.OFF).ToString();
                //寫入Return Code
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.Unknown).ToString();
                //寫入Job Data
                outputdata.EventGroups[0].Events[1].IsDisable = true;
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, GLASS IN CHECK REQUEST REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.19 Short Cut Status Report]
        public void ShortCutStatusReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eEQPStatus cv01rstatus = new eEQPStatus();
                eEQPStatus cleanerstatus = new eEQPStatus();
                eEQPStatus cv02status = new eEQPStatus();
                int cv01producttype = 0;
                int cleanerproducttype = 0;
                int cv02producttype = 0;
                cv01rstatus = (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                cv01producttype = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value.ToString());
                cleanerstatus = (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                cleanerproducttype = int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value.ToString());
                cv02status = (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                cv02producttype = int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value.ToString());
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                //save data in progarm
                lock (eqp) eqp.File.CV01Status = cv01rstatus;
                lock (eqp) eqp.File.CV01ProductType = cv01producttype;
                lock (eqp) eqp.File.CleanerStatus = cleanerstatus;
                lock (eqp) eqp.File.CleanerProductType = cleanerproducttype;
                lock (eqp) eqp.File.CV02Status = cv02status;
                lock (eqp) eqp.File.CV02ProductType = cv02producttype;

                //save progarm data in file
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPORT NEXT LINE INFORMATION, LOADER STATUS[{2}], LOADER PRODUCTTYPE[{3}], PRECLEANER STATUS[{4}], PRECLEANER PRODUCTTYPE[{5}].",
                    eqpNo, inputData.TrackKey, cv01rstatus, cv01producttype, cleanerstatus, cleanerproducttype));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}
