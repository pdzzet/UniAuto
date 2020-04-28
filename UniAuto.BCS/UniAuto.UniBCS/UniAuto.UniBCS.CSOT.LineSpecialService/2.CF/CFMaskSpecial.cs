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
        private const string MASKStartReportReplyTimeout = "MASKStartReportReplyTimeout";
        private const string MASKEndReportReplyTimeout = "MASKEndReportReplyTimeout";
        private const string MASKStatusReportReplyTimeout = "MASKStatusReportReplyTimeout";
        private const string MaskIDRequestReportTimeout = "MaskIDRequestReportTimeout";

        #region [9.2.10 MASK Start Report]
        public void MaskStartReport(Trx inputData)
        {
            try
            {

                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string cassetteSeqNo = inputData[0][0]["CassetteSequenceNo"].Value;
                string jobSeqNo = inputData[0][0]["JobSequenceNo"].Value;
                string maskID = inputData[0][0]["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
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
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK START REPORT BIT ({2}), MASK ID =[{3}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, bitResult.ToString(), maskID));
                    MaskStartReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                #endregion

                #region [Reply EQ]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK START REPORT BIT ({2}), MASK ID =[{3}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, bitResult.ToString(), maskID));
                MaskStartReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Get Mask Job WIP]
                Job job = new Job();
                job = ObjectManager.JobManager.GetJob(maskID);
                #endregion

                #region [Create/Update Mask Job WIP]
                if (job == null)
                {
                    Job newJob = new Job(int.Parse(cassetteSeqNo), int.Parse(jobSeqNo));
                    newJob.GlassChipMaskBlockID = maskID;
                    newJob.GroupIndex = inputData[0][0]["GroupIndex"].Value;
                    newJob.ProductType.Value = int.Parse(inputData[0][0]["ProductType"].Value);
                    newJob.CSTOperationMode = (eCSTOperationMode)int.Parse(inputData[0][0]["CSTOperationMode"].Value.ToString());
                    newJob.SubstrateType = (eSubstrateType)int.Parse(inputData[0][0]["SubstrateType"].Value.ToString());
                    newJob.CIMMode = (eBitResult)int.Parse(inputData[0][0]["CIMMode"].Value.ToString());
                    newJob.JobType = (eJobType)int.Parse(inputData[0][0]["JobType"].Value.ToString());
                    newJob.JobJudge = inputData[0][0]["JobJudge"].Value.ToString();
                    newJob.SamplingSlotFlag = inputData[0][0]["SamplingSlotFlag"].Value.ToString();
                    //newJob.OXRInformationRequestFlag = inputData[0][0]["OXRInformationRequestFlag"].Value.ToString();
                    newJob.FirstRunFlag = inputData[0][0]["FirstRunFlag"].Value.ToString();
                    newJob.JobGrade = inputData[0][0]["JobGrade"].Value.ToString();
                    newJob.PPID = inputData[0][0]["PPID"].Value.ToString();
                    newJob.LastGlassFlag = inputData[0][0]["LastGlassFlag"].Value.ToString();
                    newJob.EQPFlag = inputData[0][0]["EQPFlag"].Value.ToString();
                    newJob.TrackingData = inputData[0][0]["TrackingData"].Value.ToString();
                    newJob.InspJudgedData = inputData[0][0]["Insp.JudgedData"].Value.ToString();
                    newJob.CFSpecialReserved = inputData[0][0]["CFSpecialReserved"].Value.ToString();
                    //newJob.OXRInformation = inputData[0][0]["OXRInformation"].Value.ToString();
                    newJob.ChipCount = int.Parse(inputData[0][0]["ChipCount"].Value.ToString());
                    newJob.CfSpecial.COAversion = inputData[0][0]["COAVersion"].Value.ToString();
                    ObjectManager.JobManager.AddJob(newJob);
                }
                else
                {
                    lock (job)
                    {
                        job.GlassChipMaskBlockID = maskID;
                        job.GroupIndex = inputData[0][0]["GroupIndex"].Value;
                        job.ProductType.Value = int.Parse(inputData[0][0]["ProductType"].Value);
                        job.CSTOperationMode = (eCSTOperationMode)int.Parse(inputData[0][0]["CSTOperationMode"].Value.ToString());
                        job.SubstrateType = (eSubstrateType)int.Parse(inputData[0][0]["SubstrateType"].Value.ToString());
                        job.CIMMode = (eBitResult)int.Parse(inputData[0][0]["CIMMode"].Value.ToString());
                        job.JobType = (eJobType)int.Parse(inputData[0][0]["JobType"].Value.ToString());
                        job.JobJudge = inputData[0][0]["JobJudge"].Value.ToString();
                        job.SamplingSlotFlag = inputData[0][0]["SamplingSlotFlag"].Value.ToString();
                        //job.OXRInformationRequestFlag = inputData[0][0]["OXRInformationRequestFlag"].Value.ToString();
                        job.FirstRunFlag = inputData[0][0]["FirstRunFlag"].Value.ToString();
                        job.JobGrade = inputData[0][0]["JobGrade"].Value.ToString();
                        job.PPID = inputData[0][0]["PPID"].Value.ToString();
                        job.LastGlassFlag = inputData[0][0]["LastGlassFlag"].Value.ToString();
                        job.EQPFlag = inputData[0][0]["EQPFlag"].Value.ToString();
                        job.TrackingData = inputData[0][0]["TrackingData"].Value.ToString();
                        job.InspJudgedData = inputData[0][0]["Insp.JudgedData"].Value.ToString();
                        job.CFSpecialReserved = inputData[0][0]["CFSpecialReserved"].Value.ToString();
                        //job.OXRInformation = inputData[0][0]["OXRInformation"].Value.ToString();
                        job.ChipCount = int.Parse(inputData[0][0]["ChipCount"].Value.ToString());
                        job.CfSpecial.COAversion = inputData[0][0]["COAVersion"].Value.ToString();
                    }
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaskStartReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_MASKStartReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0][0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, MASKStartReportReplyTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(MaskStartReportReplyTimeoutForEQP), trxID);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MASK START REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 紀錄EQP Clear Bit time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaskStartReportReplyTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, MASK START REPORT REPLY TIMEOUT.",
                    sArray[0], trackKey));

                MaskStartReportReply(trackKey, eBitResult.OFF, sArray[0].ToString());

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.11 MASK End Report]
        public void MaskEndReport(Trx inputData)
        {
            try
            {

                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string cassetteSeqNo = inputData[0][0]["CassetteSequenceNo"].Value;
                string jobSeqNo = inputData[0][0]["JobSequenceNo"].Value;
                string maskID = inputData[0][0]["Glass/Chip/MaskID/BlockID"].Value.ToString().Trim();
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
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK END REPORT BIT (OFF), MASK ID =[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, maskID));
                    MaskEndReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                #endregion

                #region [Reply EQ]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK END REPORT BIT ({2}), MASK ID =[{3}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, bitResult.ToString(), maskID));
                MaskEndReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Get Mask Job WIP]
                Job job = new Job();
                job = ObjectManager.JobManager.GetJob(maskID);
                #endregion

                #region [Delete Mask Job WIP]
                if (job != null)
                {
                    ObjectManager.JobManager.DeleteJob(job);
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaskEndReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_MASKEndReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0][0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, MASKEndReportReplyTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(MaskEndReportReplyTimeoutForEQP), trxID);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MASK END REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 紀錄EQP Clear Bit time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaskEndReportReplyTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, MASK END REPORT REPLY TIMEOUT.",
                    sArray[0], trackKey));

                MaskEndReportReply(trackKey, eBitResult.OFF, sArray[0].ToString());

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.12 MASK Status Report]
        public void MaskStatusReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string maskID = inputData[0][0][0].Value.ToString().Trim(); ;
                eMask_Status maskStatus = (eMask_Status)int.Parse(inputData[0][0][1].Value);
                int _stockno = 0;
                int _stockslotno = 0;
                int stockNo = int.TryParse(inputData[0][0][2].Value, out _stockno) == true ? _stockno : 0;
                int stockSlotNo = int.TryParse(inputData[0][0].Items[3].Value, out _stockslotno) == true ? _stockslotno : 0;
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
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK STATUS REPORT BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    MaskStatusReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK STATUS REPORT BIT (ON) MASK ID = [{2}], MASK STATUS = [{3}], STOCK NO = [{4}], STOCKSLOT = [{5}].", 
                        inputData.Metadata.NodeNo, inputData.TrackKey, maskID, maskStatus.ToString(), stockNo, stockSlotNo));
                }
                #endregion

                #region [Reply EQ]
                MaskStatusReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [取得 Mask Job WIP]
                Job job = new Job();

                job = ObjectManager.JobManager.GetJob(maskID);
                if (job == null)
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CAN'T FINE MASK ID [{2}] IN WIP.", 
                        inputData.Metadata.NodeNo, inputData.TrackKey, maskID));
                    return;
                }
                #endregion

                #region [MES Item - CLEANRESULT Logic]
                string cleanResult = "N";
                if (job.CfSpecial.TrackingData.MaskCleaner == "1")
                {
                    cleanResult = "Y";
                }
                #endregion

                #region [Report MES]
                IList<MaskStateChanged.MASKc> maskList = new List<MaskStateChanged.MASKc>();
                MaskStateChanged.MASKc msc = new MaskStateChanged.MASKc();
                if (stockNo == 0 || stockSlotNo == 0)
                {
                    msc.MASKPOSITION = string.Empty;
                }
                else
                {
                    msc.MASKPOSITION = stockNo.ToString().PadLeft(2, '0').Trim() + stockSlotNo.ToString().PadLeft(2, '0').Trim();
                }
                msc.MASKNAME = maskID;
                msc.MASKSTATE = maskStatus.ToString();
                msc.MASKUSECOUNT = string.Empty;
                msc.UNITNAME = string.Empty; //保留不使用
                msc.CLEANRESULT = cleanResult;
                msc.REASONCODE = string.Empty;//CF不使用
                msc.HEADID = string.Empty;//CF不使用                
                maskList.Add(msc);
                object[] _data = new object[7]
                { 
                    inputData.TrackKey,   /*0  TrackKey*/
                    eqp.Data.LINEID,      /*1  LineName*/
                    eqp.Data.NODEID,      /*2  EQPID*/
                    string.Empty,         /*3  machineRecipeName*/
                    string.Empty,         /*4  eventUse*/
                    maskList,             /*5  maskList*/
                    ""                    /*6  requestKey*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "MaskStateChanged", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaskStatusReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_MASKStatusReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0][0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, MASKStatusReportReplyTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(MaskEndReportReplyTimeoutForEQP), trxID);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MASK STATUS REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 紀錄EQP Clear Bit time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaskStatusReportReplyTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, MASK STATUS REPORT REPLY TIMEOUT.",
                    sArray[0], trackKey));

                MaskEndReportReply(trackKey, eBitResult.OFF, sArray[0].ToString());

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.20 Mask ID Request Report]
        public void MaskIDRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string cstseqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
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
                Job job = ObjectManager.JobManager.GetJob(cstseqNo, jobseqNo);
                if (job == null)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK ID REQUEST REPORT, CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}], JOB NOT IN WIP.",
                        eqpNo, inputData.TrackKey, cstseqNo, jobseqNo));

                    MaskIDRequestReportReply(inputData.TrackKey, eBitResult.ON, eqpNo, null);

                    return;
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MASK ID REQUEST REPORT BIT (OFF), CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstseqNo, jobseqNo));
                    MaskIDRequestReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo, job);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MASK ID REQUEST REPORT BIT (ON), CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstseqNo, jobseqNo));
                }
                #endregion

                #region [Reply EQ]
                MaskIDRequestReportReply(inputData.TrackKey, eBitResult.ON, eqpNo, job);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaskIDRequestReportReply(string trxID, eBitResult bitResut, string eqpNo, Job job)
        {
            try
            {
                string trxName = string.Format("{0}_MaskIDRequestReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (job != null)
                {
                    if (string.IsNullOrEmpty(job.CfSpecial.MaskID))
                    { outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.NG).ToString(); }
                    else
                    { outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.OK).ToString(); }
                    outputdata.EventGroups[0].Events[0].Items[1].Value = job.CassetteSequenceNo;
                    outputdata.EventGroups[0].Events[0].Items[2].Value = job.JobSequenceNo;
                    outputdata.EventGroups[0].Events[0].Items[3].Value = job.CfSpecial.MaskID;
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.NG).ToString();
                    outputdata.EventGroups[0].Events[0].Items[1].Value = "0";
                    outputdata.EventGroups[0].Events[0].Items[2].Value = "0";
                    outputdata.EventGroups[0].Events[0].Items[3].Value = string.Empty;
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, MaskIDRequestReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(MaskIDRequestReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, MASKID REQUEST REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaskIDRequestReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                MaskIDRequestReportReply(trackKey, eBitResult.OFF, sArray[0], null);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, MASKID REQUEST REPORT REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


    }
}
