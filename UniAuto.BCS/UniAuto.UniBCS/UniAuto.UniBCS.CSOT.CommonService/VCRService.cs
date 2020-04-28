using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using System.Threading;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class VCRService : AbstractService
    {
        private const string Key_VCRModeChangeTimeout = "{0}_VCRModeChangeCommandTimeout";
        private const string Key_VCREventReportTimeout = "{0}_VCREventReportTimeout";
        private const string Key_VCRMismatchJobDataTimeout = "{0}_VCRMismatchJobDataTimeout";
        private const string Key_VCREventReportReply = "{0}_VCREventReportReply";
        private const string Key_VCRMismatchJobDataRequestReportReply = "{0}_VCRMismatchJobDataRequestReportReply";
        private const string Key_VCRModeChangeCommand = "{0}_VCREnableModeChangeCommand";
        private Thread VCRLossCheckThread_PCK;
        

        class eVCRTYPE 
        {
            public const string A = "A";
            public const string B = "B";
        }
        public override bool Init()
        {
            initPCKVCRThread();
            return true;
        }

        private void initPCKVCRThread()
        {
            try
            {
                //Line line = ObjectManager.LineManager.GetLines().First(l => l.Data.LINETYPE == eLineType.CELL.CCPCK);
                //if (line == null) return;
                //if (ParameterManager.ContainsKey("VCRLossCheckFlag"))
                //{
                //    if (!ParameterManager["VCRLossCheckFlag"].GetBoolean()) return;
                //    VCRLossCheckThread_PCK = new Thread(new ThreadStart(VCRReadLossCheck_PCK)) { IsBackground = true };
                //    VCRLossCheckThread_PCK.Start();
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
 
        }

        #region [VCR Mode]
        //public void VCRMode(Trx inputData)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
        //        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

        //        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
        //        if (line == null) throw new Exception(string.Format("CAN'T FIND Line ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

        //        //if (line.Data.FABTYPE != eFabType.CELL.ToString())
        //            VCRMode(inputData);
        //        //else
        //        //    VCRMode_CF(inputData);

        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //public void VCRMode_Array2(Trx inputData)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
        //        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

        //        if (eqp.Data.VCRCOUNT <= 0)
        //        {
        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not found VCR Object.", inputData.Metadata.NodeNo, inputData.TrackKey));
        //            return;
        //        }

        //        string vcrNo = inputData.Metadata.Name.Substring(inputData.Metadata.Name.IndexOf('#') + 1, 2);

        //        eBitResult oldResult = eqp.File.VcrMode[int.Parse(vcrNo) - 1];

        //        lock (eqp.File) eqp.File.VcrMode[int.Parse(vcrNo) - 1] = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TRX NAME=[{2}],VCR NO=[{3}],VCR MODE=[{4}].", inputData.Metadata.NodeNo,
        //            inputData.TrackKey, inputData.Name, vcrNo,(int)eqp.File.VcrMode[int.Parse(vcrNo) - 1],
        //            eqp.File.VcrMode[int.Parse(vcrNo) - 1].ToString()));

        //        if (oldResult != eqp.File.VcrMode[int.Parse(vcrNo) - 1])
        //        {
        //            object[] _data = new object[5]
        //            { 
        //                inputData.TrackKey,  /*0 TrackKey*/
        //                eqp.Data.LINEID,    /*1 LineName*/
        //                eqp.Data.NODEID,
        //                "VCR" +  vcrNo,                /*2 EQPID*/
        //                eqp.File.VcrMode[int.Parse(vcrNo) - 1] == eBitResult.ON ? "ENABLE":"DISABLE"
        //            };
        //            Invoke(eServiceName.MESService, "VCRStateChanged", _data);
        //        }

        //        if (inputData.IsInitTrigger) return;
        //        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

        //        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //public void VCRMode_CF(Trx inputData)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

        //        if (eqp == null)
        //            throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

        //        if (eqp.Data.VCRCOUNT <= 0)
        //        {
        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not found VCR Object.", inputData.Metadata.NodeNo, inputData.TrackKey));
        //            return;
        //        }
        //        int eventcount = inputData.EventGroups[0].Events.Count;  //CF Sort Line VCR 會有兩支

        //        for (int i = 0; i < eventcount; i++)
        //        {
        //            eBitResult vcrMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[i].Items[0].Value);
        //            string vcrno = "1";

        //            if (inputData.Metadata.Name.IndexOf('#') >= 0)
        //                vcrno = inputData.Metadata.Name.Substring(inputData.Metadata.Name.IndexOf('#') + 1, 2);

        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TRX NAME=[{2}],VCR NO=[{3}], VCR MODE=[{4}].", inputData.Metadata.NodeNo,
        //            inputData.TrackKey, inputData.Name, vcrno,vcrMode.ToString()));

        //            eBitResult oldVcrMode = eqp.File.VcrMode[int.Parse(vcrno) - 1];

        //            if (oldVcrMode != vcrMode)
        //            {
        //                lock (eqp)
        //                    eqp.File.VcrMode[int.Parse(vcrno) - 1] = vcrMode;

        //                #region MES Report VCRStateChanged
        //                //VCRStateChanged(string trxID, string lineName, string machineName, string vcrName, string vcrStateName)
        //                //L2_B_VCR#01EnableMode
        //                object[] _data = new object[5]
        //                { 
        //                inputData.TrackKey,  /*0 TrackKey*/
        //                eqp.Data.LINEID,    /*1 LineName*/
        //                eqp.Data.NODEID,
        //                "VCR0" +  vcrno,                /*2 EQPID*/
        //                vcrMode == eBitResult.ON ? "ENABLE":"DISABLE"
        //                };
        //                Invoke(eServiceName.MESService, "VCRStateChanged", _data);
        //                #endregion
        //            }
        //        }

        //        if (inputData.IsInitTrigger) return;
        //        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

        //        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        public void VCRMode(Trx inputData)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null)
                    throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                if (eqp.Data.VCRCOUNT <= 0)
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not found VCR Object.", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                int eventcount = inputData.EventGroups[0].Events.Count;  //CF Sort Line VCR 會有兩支

                for (int i = 0; i < eventcount; i++)
                {
                    eBitResult vcrMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[i].Items[0].Value);
                    string vcrno = "1";

                    if (inputData.EventGroups[0].Events[i].Name.IndexOf('#') >= 0)
                        vcrno = inputData.EventGroups[0].Events[i].Name.Substring(inputData.EventGroups[0].Events[i].Name.IndexOf('#')+1, 2);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TRX NAME=[{2}],VCR NO=[{3}], VCR MODE=[{4}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, inputData.Name, vcrno, vcrMode.ToString()));

                    eBitResult oldVcrMode = eqp.File.VcrMode[int.Parse(vcrno) - 1];

                    if (oldVcrMode != vcrMode)
                    {
                        lock (eqp)
                            eqp.File.VcrMode[int.Parse(vcrno) - 1] = vcrMode;

                        #region MES Report VCRStateChanged
                        //VCRStateChanged(string trxID, string lineName, string machineName, string vcrName, string vcrStateName)
                        //L2_B_VCR#01EnableMode
                        object[] _data = new object[5]
                        { 
                        inputData.TrackKey,  /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp.Data.NODEID,
                        "VCR0" +  vcrno,                /*2 EQPID*/
                        vcrMode == eBitResult.ON ? "ENABLE":"DISABLE"
                        };
                        Invoke(eServiceName.MESService, "VCRStateChanged", _data);
                        #endregion
                    }
                }

                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [VCR Mode Change Command]
        public void VCRModeChangeCommand(string eqpNo, eBitResult vcrMode, string vcrNo, string trackKey)
        {
            try
            {
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                #region CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND VCR MODE CHANGE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_VCRModeChangeCommand, eqpNo)) as Trx;
                outputData.EventGroups[0].Events[0].Items[0].Value = vcrNo;
                if (vcrMode == eBitResult.ON)
                    outputData.EventGroups[0].Events[0].Items[1].Value = "1"; //ENABLE
                else
                    outputData.EventGroups[0].Events[0].Items[1].Value = "2";  //DISABLE

                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = trackKey;
                SendPLCData(outputData);

                string timeName = string.Format(Key_VCRModeChangeTimeout, eqpNo);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRModeChangeCommandReplyTimeout), outputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] VCR_NO=[{2}] VCR_ENABLE_MODE=[{3}]=[{4}], SET BIT=[ON].", eqp.Data.NODENO,
                        outputData.TrackKey, vcrNo, (int)vcrMode, vcrMode.ToString()));

                //VCRModeChangeCommandReply();
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void VCRModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT=[{2}] VCR ENABLE MODE CHANGE COMMAND REPLY RETURN_CODE=({3})[{4}].",
                    eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format(Key_VCRModeChangeTimeout, inputData.Metadata.NodeNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_VCRModeChangeCommand, inputData.Metadata.NodeNo)) as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void VCRModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format(Key_VCRModeChangeTimeout, sArray[0]);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_VCRModeChangeCommand, sArray[0])) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] VCR ENABLE MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [VCR Event Report]
        public void VCREventReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];//sy add 20160629 for #0X

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                        VCREventReportReply(commandNo,eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //Get Data
                string glassID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim(); //Watson Modify 20141125 For阿昌
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                string unitNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                string VCRNo = inputData.EventGroups[0].Events[0].Items[4].Value;

                //string VCRResult = ConstantManager["VCRRESULT"][inputData.EventGroups[0].Events[0].Items[5].Value].Value;
                string VCRResult = inputData.EventGroups[0].Events[0].Items[5].Value;
//"1：VCR Reading OK & Match With Job Data Glass ID
//2：VCR Reading OK & Miss Match With Job Data Glass ID
//3：VCR Reading Fail & Key In & Match With Job Data Glass ID
//4：VCR Reading Fail & Key In & Miss Match With Job Data Glass ID
//5：VCR Reading Fail & Pass"


                //Watson Modify 20150428 Modify Log Detail
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] VCR GLASS ID =[{2}],CST_SEQ_NO=[{3}] ,JOB_SEQ_NO=[{4}] , UNIT_NO=[{5}], VCRNO=[{6}] ,VCR RESULT =[{7}]({8}) , BIT =[ON]",
                    inputData.Metadata.NodeNo , inputData.TrackKey, glassID, cassetteSequenceNo,jobSequenceNo ,unitNo ,VCRNo , VCRResult,(eVCR_EVENT_RESULT)(int.Parse(VCRResult))));

                //Get Node
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                
                //Get Node
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));

                //Get Job
                Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                if (line.Data.LINETYPE == eLineType.CF.FCMSK_TYPE1)
                {
                    if (job == null)
                    {
                        int c = 0;
                        int j = 0;
                        if (!int.TryParse(cassetteSequenceNo, out c)) throw new Exception(string.Format("Create Job Failed Cassette Sequence No isn't number!!", cassetteSequenceNo));
                        if (c == 0) throw new Exception(string.Format("Create Job Failed Cassette Sequence No (0)!!", cassetteSequenceNo));

                        if (!int.TryParse(jobSequenceNo, out j)) throw new Exception(string.Format("Create Job Failed Job Sequence No isn't number!!", jobSequenceNo));
                        if (j == 0) throw new Exception(string.Format("Create Job Failed Job Sequence No (0)!!", jobSequenceNo));

                        job = new Job(c, j);
                        ObjectManager.JobManager.NewJobCreateMESDataEmpty(job);
                        ObjectManager.JobManager.AddJob(job);

                        job.GlassChipMaskBlockID = glassID;
                    }
                }
                else
                {
                    if (job == null)
                    {
                        //throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO =[{0}] JOB SEQUENCE NO =[{1}] IN JOB!", cassetteSequenceNo, jobSequenceNo));
                        //Watson Modify 20150316 For Clear Log
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CAN'T FIND JOB, CASSETTE SEQUENCENO =[{2}] JOB SEQUENCE NO =[{3}] IN JOBENTITY, VCR GLASS ID =[{4}]",
                        eqp.Data.NODENO, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, glassID));

                        VCREventReportReply(commandNo, eqpNo, eBitResult.ON, inputData.TrackKey);
                        return;                         
                    }
                }

                job.VCRJobID = glassID;
                job.VCR_Result = (eVCR_EVENT_RESULT)(int.Parse(VCRResult));

                //Jun Add 20150508 For CF & CELl Updata Glass ID in VCR B Type
                if (line.Data.FABTYPE == eFabType.CF.ToString() || line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (eqp.Data.VCRTYPE == eVCRTYPE.B.ToString())
                    {
                        if (job.VCR_Result != eVCR_EVENT_RESULT.READING_FAIL_PASS)
                            VCRMismatch_TypeB(job, glassID);
                    }
                }

                if (job.VCRJobID.Trim() != job.MesProduct.PRODUCTNAME.Trim())
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE =[{2}], VCR GLASS ID{3} AND MES PRODUCTNAME{4} IS DIFFERENT!!",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, glassID.Trim(),job.MesProduct.PRODUCTNAME.Trim()));
                }

                ObjectManager.JobManager.EnqueueSave(job);

                object[] _data = new object[5]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,
                    job,                /*2 EQPID*/
                    job.VCR_Result,
                };

                //Report to MES Agent
                Invoke(eServiceName.MESService, "VCRReadReport", _data);

                VCREventReportReply(commandNo, eqpNo, eBitResult.ON, inputData.TrackKey);

                //add VCREvent by zhuxingxing 20161024
                string VcrEvent = string.Empty;
                if (string.IsNullOrEmpty(commandNo))
                {
                    VcrEvent = eJobEvent.VCR_Report.ToString();
                }
                else
                {
                    VcrEvent = eJobEvent.VCR_Report.ToString() + "#" + commandNo;
                }
                //end 

                ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, string.Empty, string.Empty, VcrEvent, inputData.TrackKey, VCRNo,string.Empty);


                #region Report CFShortCutGlassProcessEnd to MES
                Unit unit = null;
                if (unitNo != "0")
                {
                    unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                }
                // OK or PASS go to Short Cut
                if (job.VCR_Result == eVCR_EVENT_RESULT.READING_OK_MATCH_JOB ||
                    job.VCR_Result == eVCR_EVENT_RESULT.READING_FAIL_PASS)
                {

                    if (line.File.CFShortCutMode == eShortCutMode.Enable &&
                        job.JobJudge == "1" && job.CfSpecial.PermitFlag == "Y" &&
                        job.CfSpecial.CFShortCutrecipeParameterRequestResult == eRecipeCheckResult.OK
                        && !job.CfSpecial.CFShortCutTrackOut)
                    {
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.CF.FCMPH_TYPE1:
                            case eLineType.CF.FCRPH_TYPE1:
                            case eLineType.CF.FCGPH_TYPE1:
                            case eLineType.CF.FCBPH_TYPE1:
                                if (unit != null && unit.Data.UNITATTRIBUTE == "VCR")
                                {
                                    Invoke(eServiceName.MESService, "CFShortCutGlassProcessEnd", new object[] { inputData.TrackKey, line, job });
                                }
                                break;
                        }
                    }
                }
                //NG or UMATCH go to Unloader
                if (job.VCR_Result == eVCR_EVENT_RESULT.READING_OK_MISMATCH_JOB ||
                    job.VCR_Result == eVCR_EVENT_RESULT.READING_FAIL_KEY_IN_MATCH_JOB ||
                    job.VCR_Result == eVCR_EVENT_RESULT.READING_FAIL_KEY_IN_MISMATCH_JOB)
                {
                    switch (line.Data.LINETYPE)
                    {
                        case eLineType.CF.FCMPH_TYPE1:
                        case eLineType.CF.FCRPH_TYPE1:
                        case eLineType.CF.FCGPH_TYPE1:
                        case eLineType.CF.FCBPH_TYPE1:
                            if (unit != null && unit.Data.UNITATTRIBUTE == "VCR")
                            {
                                Invoke(eServiceName.CFSpecialService, "GlassOutResultCommand", new object[] { inputData.TrackKey, job, ePermitFlag.F, job.CfSpecial.SamplingValue });
                            }
                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];//sy add 20160629 for #0X
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    VCREventReportReply(commandNo, inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void VCREventReportReply(string commandNo, string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata;
                if (string.IsNullOrEmpty(commandNo))//sy add 20160629 for #0X
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCREventReportReply, eqpNo)) as Trx;
                else
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCREventReportReply, eqpNo)+"#"+ commandNo) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                string timeoutName;
                if (string.IsNullOrEmpty(commandNo))//sy add 20160629 for #0X
                    timeoutName = string.Format(Key_VCREventReportTimeout, eqpNo);
                else
                    timeoutName = string.Format(Key_VCREventReportTimeout, eqpNo)+"#"+ commandNo;
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCREventReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}],VCR Event Report Reply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void VCREventReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_'); 
                string commandNo = string.Empty;
                if (sArray[1].Split(new char[] { '#' }).Length == 2)
                    commandNo = sArray[1].Split(new char[] { '#' })[1];//sy add 20160629 for #0X
                string timeName;
                if (string.IsNullOrEmpty(commandNo))//sy add 20160629 for #0X
                    timeName = string.Format(Key_VCRModeChangeTimeout, sArray[0]);
                else
                    timeName = string.Format(Key_VCRModeChangeTimeout, sArray[0]) + "#" + commandNo;
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata;
                if (string.IsNullOrEmpty(commandNo))//sy add 20160629 for #0X
                    outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_VCREventReportReply, sArray[0])) as Trx;
                else
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCREventReportReply, sArray[0]) + "#" + commandNo) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS REPLY, VCR EVENT REPORT REPLY TIMEOUT SET VALUE=[OFF].",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [VCR Mismatch Job Data Request]
        public void VCRMismatchJobDataRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                string CommandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    CommandNo = inputData.Name.Split(new char[] { '#' })[1];
                //Add By YangZhenteng For Cell POL Line VCR Event,2018/01/06,For Cell POL VCR Mismatch Report;
                if (!string.IsNullOrEmpty(CommandNo)) //Add By Yangzhenteng For POL Mismatch Request#02,延时处理;
                {
                    Thread.Sleep(500);
                }
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                //Get Node
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                //Get Line
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND LINE_NO =[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                //Get VCR Type //shihyang 20150925 Type B 機台 不會上報
                if (string.IsNullOrEmpty(eqp.Data.VCRTYPE)) throw new Exception("CAN'T FIND DB EQP VCRTYPE");
                if (eqp.Data.VCRTYPE != eVCRTYPE.A) throw new Exception("CAN'T FIND DB EQP VCRTYPE A");

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    //VCRMismatchJobDataRequestReply(line, eqpNo, eBitResult.OFF, eReturnCode1.Unknown, null, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, line, eqpNo, eBitResult.OFF, eReturnCode1.Unknown, null, inputData.TrackKey);
                    //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加参数CommandNo;
                    return;
                }
                //Get Data
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string jobDataofGlassID = inputData.EventGroups[0].Events[0].Items[2].Value;
                string vcrReadofGlassID = inputData.EventGroups[0].Events[0].Items[3].Value;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CAS_SEQ_NO=[{2}] ,JOB_SEQ_NO=[{3}] VCR READ GLASS ID =[{4}], EQP KEEP JOB DATA GLASS ID =[{5}] , SET BIT =[ON]",
                        eqp.Data.NODENO, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, vcrReadofGlassID, jobDataofGlassID));

                #region[POL OHV Panel Product In]
                //20171120 by huangjiayin
                if (line.Data.LINEID.Contains("CCPOL")
                    &&eqpNo=="L3"
                    && cassetteSequenceNo=="0"
                    && jobSequenceNo=="0")
                {
                    string polPanelId = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                    #region [MES OFFLINE]
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        string terminalMsg = "Line Control Status is Off-Line, can not do this function!";
                        //Edit By Yangzhenteng For Cell POL Line VCR Event;
                        VCRMismatchJobDataRequestReply(CommandNo, null, inputData.Name.Split('_')[0], eBitResult.ON, eReturnCode1.NG, null, inputData.TrackKey);
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqpNo, terminalMsg, "", "1" });
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] {2}SET BIT=[ON].",
                            eqpNo, inputData.TrackKey, terminalMsg));

                        return;
                    }
                    #endregion

                    #region[PanelInformationRequest]
                    //keycommandNo
                    string keyCommandNo = keyBoxReplyPLCKey.PanelRequestReplyCommandNo + "#" + (CommandNo == string.Empty ? "00" : CommandNo.PadLeft(2, '0'));  //add keyup CommandNo
                    if (Repository.ContainsKey(keyCommandNo))
                        Repository.Remove(keyCommandNo);
                    Repository.Add(keyCommandNo, CommandNo + "#" + polPanelId);
                    Invoke(eServiceName.MESService, "PanelInformationRequest", new object[6] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, polPanelId, CommandNo, "N" });//sy modify 20160911
                    #endregion

                    return;
                }
                #endregion

                //Get Job
                string logReturnCode = string.Empty;
                //Job jobRead = ObjectManager.JobManager.GetJob(vcrReadofGlassID.Trim());//shihyang 20150925 修改用VCR read Glass ID 去找
                Job jobRead = ObjectManager.JobManager.GetJobs(cassetteSequenceNo).FirstOrDefault(j => j.GlassChipMaskBlockID.Trim() == vcrReadofGlassID.Trim());
                Job jobData = ObjectManager.JobManager.GetJob(cassetteSequenceNo.Trim(), jobSequenceNo.Trim());//sy 20160126 用Data Glass ID 去找不到在用cst & job seq no 去找
                string log = string.Empty;
                if (jobRead != null && jobData != null)//shihyang 20150925 GlassID 找到帳就回OK//sy 修改jobRead&jobData 都找到才能回OK
                {
                    #region [jobRead != null && jobData != null]
                    logReturnCode = "_OK";
                    jobData.VCRJobID = vcrReadofGlassID.Trim();
                    if (!jobRead.Equals(jobData))//sy modify 20160616 防止機台報相同 相同就不換帳
                    {
                        #region [Glass資料互換]
                        Job temp = new Job();
                        temp = (Job)jobData.Clone();
                        jobData.GlassChipMaskBlockID = vcrReadofGlassID.Trim();
                        jobData.EQPJobID = vcrReadofGlassID.Trim();
                        jobData = VCRMismatchClone(line.Data.FABTYPE, jobData, jobRead);
                        if (line.Data.LINENAME.Contains(keyCellLineType.CUT) || line.Data.LINENAME.Contains(keyCellLineType.PCS))
                            jobData.MesProduct = (PRODUCTc)jobRead.MesProduct.Clone();//sy add 2016 0409 cuttingStart 需拿此資料上報/計算
                        lock (jobData)
                            ObjectManager.JobManager.EnqueueSave(jobData);//sy add 2016 0409

                        jobRead.GlassChipMaskBlockID = temp.GlassChipMaskBlockID.Trim();
                        jobRead.EQPJobID = temp.EQPJobID.Trim();
                        jobRead = VCRMismatchClone(line.Data.FABTYPE, jobRead, temp);
                        if (line.Data.LINENAME.Contains(keyCellLineType.CUT) || line.Data.LINENAME.Contains(keyCellLineType.PCS))
                            jobRead.MesProduct = (PRODUCTc)temp.MesProduct.Clone();//sy add 2016 0409 cuttingStart 需拿此資料上報/計算
                        lock (jobRead)
                            ObjectManager.JobManager.EnqueueSave(jobRead);//sy add 2016 0409
                        #endregion
                    }
                    //VCRMismatchJobDataRequestReply(line, eqpNo, eBitResult.ON, eReturnCode1.OK, jobData, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, line, eqpNo, eBitResult.ON, eReturnCode1.OK, jobData, inputData.TrackKey);
                    //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;

                    log = string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], VCR GLASS ID =[{3}], MISMATCH JOB DATA GLASS ID =[{4}] , SET BIT =[ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, vcrReadofGlassID, jobDataofGlassID);

                    //ObjectManager.JobManager.RecordJobHistory(jobRead, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.VCR_Mismatch.ToString() + logReturnCode, string.Empty, string.Empty);
                    #endregion
                }
                else if (jobRead == null && jobData != null)
                {
                    #region [jobRead = null & jobData != null]
                    logReturnCode = "_NG";
                    #region [CELL 找不到WIP sy mark 20160224]
                    ////CELL: JobData=> GlassID用VCR照到的, 
                    ////CST SEQ NO及Job SEQ NO用原本的,
                    ////OXR給O, Judge給OK, Grade也給OK, FLAG, 照填進去
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        //Jun Add 20150505 如果找不到In CST Job，先備份舊的Job，之後再更新Glass ID
                        Job jobBak = new Job();
                        jobBak = (Job)jobData.Clone();
                        List<Job> jobList = ObjectManager.JobManager.GetJobs(jobData.CassetteSequenceNo).Where(j => int.Parse(j.JobSequenceNo) >= 60000).ToList();
                        if (jobList != null && jobList.Count > 0)
                        {
                            //jobBak.JobSequenceNo = (jobList.Max(j => int.Parse(j.JobSequenceNo)) + 1).ToString();
                            for (int i = 60000; i <= 65535; i++)
                            {
                                Job chkJob = ObjectManager.JobManager.GetJob(jobBak.CassetteSequenceNo, i.ToString());
                                if (chkJob == null)
                                {
                                    jobBak.JobSequenceNo = i.ToString();
                                    break;
                                }
                            }
                            jobBak.JobKey = string.Format("{0}_{1}", jobBak.CassetteSequenceNo, jobBak.JobSequenceNo);
                        }
                        else
                        {
                            jobBak.JobSequenceNo = "60000";
                            jobBak.JobKey = string.Format("{0}_{1}", jobBak.CassetteSequenceNo, jobBak.JobSequenceNo);
                        }
                        jobBak.CurrentEQPNo = "";
                        ObjectManager.JobManager.AddJob(jobBak);
                        ObjectManager.JobManager.EnqueueSave(jobBak);

                        jobData.GlassChipMaskBlockID = vcrReadofGlassID.Trim();
                        jobData.EQPJobID = vcrReadofGlassID.Trim();
                        jobData.OXRInformation = new string('O', jobData.OXRInformation.Length);
                        IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
                        if (sub != null && sub.ContainsKey("VCRMismatchAndBCReplyNG"))
                            sub["VCRMismatchAndBCReplyNG"] = "1";
                        ObjectManager.JobManager.EnqueueSave(jobData);
                        //jobData = VCRMismatchMESClone(line, jobData, vcrReadofGlassID.Trim());
                        //CELL_VCR_Replace_JobDATA(line, ref jobData);
                    }
                    #endregion
                   //VCRMismatchJobDataRequestReply(line, eqpNo, eBitResult.ON, eReturnCode1.NG, jobData, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, line, eqpNo, eBitResult.ON, eReturnCode1.NG, jobData, inputData.TrackKey);
                    //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;
                    log = string.Format("VCR MISMATCH GLASSID=[{0}] IS NOT IN WIP.", vcrReadofGlassID);
                    #endregion
                }
                else if (jobRead != null && jobData == null)
                {
                    #region [jobRead != null & jobData = null]
                    logReturnCode = "_NG";
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        jobData = new Job();
                        jobData = (Job)jobRead.Clone();
                        jobData.CassetteSequenceNo = cassetteSequenceNo;
                        jobData.JobSequenceNo = jobSequenceNo;
                        jobData.JobKey = string.Format("{0}_{1}", cassetteSequenceNo, jobSequenceNo);
                        jobData.VCRJobID = vcrReadofGlassID.Trim();
                        ObjectManager.JobManager.AddJob(jobData);

                        jobRead.GlassChipMaskBlockID = jobData.GlassChipMaskBlockID + "FF";
                        ObjectManager.JobManager.EnqueueSave(jobRead);

                        IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");//sy 2016 0318 add BC 自行ON Flag 
                        if (sub != null && sub.ContainsKey("VCRMismatchAndBCReplyNG"))
                            sub["VCRMismatchAndBCReplyNG"] = "1";
                        ObjectManager.JobManager.EnqueueSave(jobData);
                    }
                    //VCRMismatchJobDataRequestReply(line, eqpNo, eBitResult.ON, eReturnCode1.NG, jobData, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, line, eqpNo, eBitResult.ON, eReturnCode1.NG, jobData, inputData.TrackKey);
                    //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;

                    log = string.Format("JOB DATA IN NOT IN WIP CASSETTE SEQ NO=[{0}],JOB SEQ NO=[{1}].", cassetteSequenceNo, jobSequenceNo);
                    #endregion
                }
                else
                {
                    #region [jobRead == null & jobData == null]
                    logReturnCode = "_NG";
                    //VCRMismatchJobDataRequestReply(line, eqpNo, eBitResult.ON, eReturnCode1.NG, null, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, line, eqpNo, eBitResult.ON, eReturnCode1.NG, jobData, inputData.TrackKey);
                    //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;

                    log = string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] JOBDATA IS NULL, CAS_SEQ_NO=[{2}] ,JOB_SEQ_NO=[{3}] NO JOB IN JOBENTITY!! VCR GLASS ID =[{4}], JOB DATA GLASS ID =[{5}] , SET BIT =[ON]",
                   eqp.Data.NODENO, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, vcrReadofGlassID, jobDataofGlassID);
                    #endregion
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log);

                log = string.Format("JOB DATA IN NOT IN WIP CASSETTE SEQ NO=[{0}],JOB SEQ NO=[{1}].", cassetteSequenceNo, jobSequenceNo);

                ObjectManager.JobManager.RecordJobHistory(jobData, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.VCR_Mismatch.ToString() + logReturnCode, inputData.TrackKey, string.Empty, string.Empty);
                if (logReturnCode == "_OK")
                {
                    string VCRJobIDTemp = jobRead.VCRJobID;
                    jobRead.VCRJobID = string.Format("Job ID Change From {0}_{1} ", jobData.CassetteSequenceNo, jobData.JobSequenceNo);
                    ObjectManager.JobManager.RecordJobHistory(jobRead, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.VCR_Mismatch.ToString() + "_Change", inputData.TrackKey, string.Empty, string.Empty);
                    jobRead.VCRJobID = VCRJobIDTemp;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                string CommandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    CommandNo = inputData.Name.Split(new char[] { '#' })[1];
                //Add By YangZhenteng For Cell POL Line VCR Event,2018/01/06;
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    //VCRMismatchJobDataRequestReply(null, inputData.Name.Split('_')[0], eBitResult.ON, eReturnCode1.NG, null, inputData.TrackKey);
                    VCRMismatchJobDataRequestReply(CommandNo, null, inputData.Name.Split('_')[0], eBitResult.ON, eReturnCode1.NG, null, inputData.TrackKey);
                    //Eidt By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;
                }
            }
        }

        private void VCRMismatchJobDataRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                //string timeName = string.Format(Key_VCRMismatchJobDataTimeout, sArray[0]);
                string CommandNo = string.Empty;
                if (sArray[1].Split(new char[] { '#' }).Length == 2)
                   CommandNo = sArray[1].Split(new char[] { '#' })[1];
                string timeName;
                if (string.IsNullOrEmpty(CommandNo))
                   timeName = string.Format(Key_VCRMismatchJobDataTimeout, sArray[0]);
                else
                   timeName = string.Format(Key_VCRMismatchJobDataTimeout, sArray[0]) + "#" + CommandNo;
                //Add By YangZhenteng For Cell POL Line VCR Event,2018/01/06;
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                //VCRMismatchJobDataRequestReply(null, sArray[0], eBitResult.OFF, eReturnCode1.Unknown, null, trackKey);
                VCRMismatchJobDataRequestReply(CommandNo, null, sArray[0], eBitResult.OFF, eReturnCode1.Unknown, null, trackKey);
                //Edit By YangZhenteng For Cell POL Line VCR Event,2018/01/06,增加CommandNO参数;
                    
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BCS REPLY, VCR MISMATCH JOB DATA REQUEST REPLY TIMEOUT SET VALUE=[OFF].",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [Unit VCR Enable Status] For CELL USE (WORD)
        public void UnitVCREnableStatusUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string status = inputData.EventGroups[0].Events[0].Items[0].Value;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                #region MES Report Rule
                if (eqp.File.UnitVCREnableStatus != status)
                {
                    if (eqp.File.UnitVCREnableStatus != string.Empty)
                    {
                        for (int u = 0; u < eqp.Data.VCRCOUNT; u++)
                        {
                            if (eqp.File.UnitVCREnableStatus.Substring(u, 1) == status.Substring(u, 1))
                                continue;
                            string vcrmode = status.Substring(u, 1) != "0" ? "ENABLE" : "DISABLE";
                        }
                    }
                }

                #endregion
                lock (eqp)
                {
                    eqp.File.UnitVCREnableStatus = status;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [{1}][{2}] UNIT_VCR_ENABLE_STATUS=[{3}].",
                eqp.Data.NODENO, sourceMethod, inputData.TrackKey, status));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void UnitVCREnableStatus(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                    return;

                string status = inputData.EventGroups[0].Events[0].Items[0].Value;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] SET UNITVCRENABLESTATUS=[{2}]",
                    eqp.Data.NODENO, inputData.TrackKey, status));

                #region MES Report Rule
                if (eqp.File.UnitVCREnableStatus != status)
                {
                    if (eqp.File.UnitVCREnableStatus != string.Empty)
                    {
                        for (int u = 0; u < eqp.Data.VCRCOUNT; u++)
                        {
                            if (eqp.File.UnitVCREnableStatus.Substring(u, 1) == status.Substring(u, 1))
                                continue;
                            //Report
                            string vcrmode = status.Substring(u, 1) != "0" ? "ENABLE" : "DISABLE";

                            object[] _data1 = new object[5]
                            { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp.Data.NODEID,
                            "VCR0" +  (u+1).ToString().PadLeft(2,'0'),                /*2 EQPID*/
                           vcrmode
                            };
                            Invoke(eServiceName.MESService, "VCRStateChanged", _data1);
                        }
                    }
                }

                #endregion
                lock (eqp)
                {
                    eqp.File.UnitVCREnableStatus = status;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }

        /// <summary>
        /// Trx Structure ARRAY與CELL不同
        /// </summary>
        public void VCRMismatchJobDataRequestReply(string CommandNo, Line line, string eqpNo, eBitResult value, eReturnCode1 rtncode, Job jobData, string trackKey)
        {
            if (line == null)
            {
                Line ll = ObjectManager.LineManager.GetLine(ServerName);
                #region ServerName != LineName 的處理方式
                if (ServerName.Contains(eLineType.CELL.CBPMT))
                    ll = GetLineByLines(keyCELLPMTLINE.CBPMI);
                #endregion
                // cs.chou T3 add Cell only one Line special//shihyang GAP LINE NO VCR 
                #region T3 ServerName != LineName
                if(ServerName.Contains(eLineType.CELL.CCGAP))
                    ll = GetLineByLines(keyCELLGAPLINE.CCGMI);
                #endregion
                if (ll.Data.FABTYPE == eFabType.ARRAY.ToString())
                    VCRMismatchJobDataRequestReply_ARRAY(ll, eqpNo, value, rtncode, null, trackKey); //ARRAY: NG回覆的JOB DATA不用填資料}
                else //Edit By YangZhenteng 2018/01/06 For Cell POL/Sor800 Line VCR Event
                {
                  if(line.Data.LINEID.Contains("CCPOL")||line.Data.LINEID.Contains("CCSOR800"))
                   {
                      VCRMismatchJobDataRequestReply_BEOL(CommandNo, line, eqpNo, value, rtncode, jobData, trackKey);
                   }
                  else
                  {
                    VCRMismatchJobDataRequestReply_COMM(line, eqpNo, value, rtncode, jobData, trackKey);
                  }                   
                }
                return;
            }
            if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
            {
                if (rtncode == eReturnCode1.NG)
                    jobData = null;
                VCRMismatchJobDataRequestReply_ARRAY(line, eqpNo, value, rtncode, jobData, trackKey); //ARRAY: NG回覆的JOB DATA不用填資料
            }
            else //Edit By YangZhenteng 2018/01/06 For Cell POL/SRO800 Line VCR Event
           {
               if (line.Data.LINEID.Contains("CCPOL")||line.Data.LINEID.Contains("CCSOR800"))
               {
                  VCRMismatchJobDataRequestReply_BEOL(CommandNo, line, eqpNo, value, rtncode, jobData, trackKey); 
                }
               else
               {
                  VCRMismatchJobDataRequestReply_COMM(line, eqpNo, value, rtncode, jobData, trackKey);
               }                
           } 
        }
        /// <summary>
        /// Trx Structure 不同
        /// </summary>
        private void VCRMismatchJobDataRequestReply_ARRAY(Line line, string eqpNo, eBitResult value, eReturnCode1 rtncode, Job jobData, string trackKey)
        {
            string err = string.Empty;
            try
            {
                Trx outputData = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo)) as Trx;
                outputData.ClearTrxWith0();
                string timeoutName = string.Format(Key_VCRMismatchJobDataTimeout, eqpNo);
                if (value == eBitResult.OFF)
                    outputData.EventGroups[0].Events[01].IsDisable = true;
                else
                    outputData.EventGroups[0].Events[1].Items[0].Value = ((int)rtncode).ToString();
                outputData.EventGroups[0].Events[2].Items[0].Value = ((int)value).ToString();
                outputData.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                #region [Reply EQ]
                if (jobData != null)
                {
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value = jobData.CassetteSequenceNo;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value = jobData.JobSequenceNo;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GroupIndex].Value = jobData.GroupIndex;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value = jobData.ProductType.Value.ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CSTOperationMode].Value = ((int)jobData.CSTOperationMode).ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value = ((int)jobData.SubstrateType).ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CIMMode].Value = ((int)jobData.CIMMode).ToString();   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value = ((int)jobData.JobType).ToString();   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value = jobData.JobJudge;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.SamplingSlotFlag].Value = jobData.SamplingSlotFlag;   ////INT
                    //outputData.EventGroups[0].Events[0].Items["OXRInformationRequestFlag"].Value = jobData.OXRInformationRequestFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.FirstRunFlag].Value = jobData.FirstRunFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value = jobData.JobGrade;   //ASCII
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value = jobData.GlassChipMaskBlockID;   //ASCII
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value = jobData.PPID;   //ASCII
                }
                else
                {
                    outputData.EventGroups[0].Events[0].IsDisable = true;
                    outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                    SendPLCData(outputData);

                    if (value == eBitResult.ON)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[{2}], Return Code=[{3}], Job WIP can't find.", eqpNo, trackKey, value, rtncode));

                        if (_timerManager.IsAliveTimer(timeoutName))
                            _timerManager.TerminateTimer(timeoutName);

                        _timerManager.CreateTimer(eqpNo + "_" + Key_VCRMismatchJobDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);
                    }
                    else
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[OFF].", eqpNo, trackKey));
                    }
                    return;
                }



                if (_timerManager.IsAliveTimer(timeoutName))
                    _timerManager.TerminateTimer(timeoutName);
                if (value == eBitResult.ON)
                    _timerManager.CreateTimer(eqpNo + "_" + Key_VCRMismatchJobDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);

                object[] _data = new object[4]
                { 
                    outputData,           /*0  TrackKey*/
                    jobData,              /*1  Job*/
                    line,                 /*2  Line*/ 
                    eqpNo                 /*3  EQP No*/ 
                };

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                switch (fabType)
                {
                    case eFabType.ARRAY:
                        Invoke(eServiceName.ArraySpecialService, "JobDataRequestReportReply_Array", _data);
                        break;
                    case eFabType.CF:
                        Invoke(eServiceName.CFSpecialService, "JobDataRequestReportReply_CF", _data);
                        break;
                    case eFabType.CELL:
                        Invoke(eServiceName.CELLSpecialService, "JobDataRequestReportReply_CELL", _data);
                        break;
                }

                #endregion

                if (_timerManager.IsAliveTimer(timeoutName))
                    _timerManager.TerminateTimer(timeoutName);

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + Key_VCRMismatchJobDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}],VCR Mismatch Job Data Request Reply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //20150924 shihyang edit
        private void VCRMismatchJobDataRequestReply_COMM(Line line, string eqpNo, eBitResult value, eReturnCode1 rtncode, Job job, string trackKey)
        {
            try
            {
                string log = string.Empty;
                Trx outputData = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo)) as Trx;

                string timeoutName = string.Format(Key_VCRMismatchJobDataTimeout, eqpNo);
                if (_timerManager.IsAliveTimer(timeoutName))
                    _timerManager.TerminateTimer(timeoutName);

                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)rtncode).ToString();
                outputData.EventGroups[0].Events[2].Items[0].Value = ((int)value).ToString();
                outputData.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                #region [If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputData.EventGroups[0].Events[1].IsDisable = true;
                    outputData.EventGroups[0].Events[0].IsDisable = true;
                    SendPLCData(outputData);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[OFF].", eqpNo, trackKey));
                    return;
                }
                #endregion
                #region [If job == null]
                if (job == null)
                {
                    outputData.EventGroups[0].Events[0].IsDisable = true;
                    SendPLCData(outputData);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[{2}], Return Code=[{3}], read & data Job WIP can't find.", eqpNo, trackKey, value, rtncode));
                   
                    _timerManager.CreateTimer(string.Format(Key_VCRMismatchJobDataTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);
               
                    return;
                }
                #endregion
                
                #region [ Job data]
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value = job.CassetteSequenceNo;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value = job.JobSequenceNo;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GroupIndex].Value = job.GroupIndex;   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value = job.ProductType.Value.ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CSTOperationMode].Value = ((int)job.CSTOperationMode).ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value = ((int)job.SubstrateType).ToString();   //INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CIMMode].Value = ((int)job.CIMMode).ToString();   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value = ((int)job.JobType).ToString();   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value = job.JobJudge;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.SamplingSlotFlag].Value = job.SamplingSlotFlag;   ////INT
                    //outputData.EventGroups[0].Events[0].Items["OXRInformationRequestFlag"].Value = jobData.OXRInformationRequestFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.FirstRunFlag].Value = job.FirstRunFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value = job.JobGrade;   //ASCII
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value = job.GlassChipMaskBlockID;   //ASCII 
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value = job.PPID;   //ASCII
                    #region CELL Special Job Data
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                    switch (line.Data.JOBDATALINETYPE)
                    {
                        #region [T3 rule]
                        case eJobDataLineType.CELL.CCPIL:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PILiquidType].Value = job.CellSpecial.PILiquidType;
                            break;
                        case eJobDataLineType.CELL.CCODF:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.AssembleSeqNo].Value = job.CellSpecial.AssembleSeqNo;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.UVMaskAlreadyUseCount].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                            break;
                        case eJobDataLineType.CELL.CCPCS:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize].Value = job.CellSpecial.BlockSize;
                            //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID].Value = job.CellSpecial.CUTProductID;
                            //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType].Value = job.CellSpecial.CUTProductType;
                            //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID2].Value = job.CellSpecial.CUTProductID2;
                           // outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType2].Value = job.CellSpecial.CUTProductType2;
                            // 20170721 by huangjiayin
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCodeList].Value = job.CellSpecial.PCSCassetteSettingCodeList;
                            // 20170725 by huangjiayin
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSBlockSizeList].Value = job.CellSpecial.PCSBlockSizeList;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize1].Value = job.CellSpecial.BlockSize1;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize2].Value = job.CellSpecial.BlockSize2;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode2].Value = job.CellSpecial.CUTCassetteSettingCode2;
                            break;
                        case eJobDataLineType.CELL.CCCUT:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductID].Value = job.CellSpecial.CUTProductID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductType].Value = job.CellSpecial.CUTProductType;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.RejudgeCount].Value = job.CellSpecial.RejudgeCount;  
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.VendorName].Value = job.CellSpecial.VendorName;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BURCheckCount].Value = job.CellSpecial.BURCheckCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                            break;
                        case eJobDataLineType.CELL.CCPCK:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelGroup].Value = job.CellSpecial.PanelGroup;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OQCBank].Value = job.CellSpecial.OQCBank;
                            break;
                        case eJobDataLineType.CELL.CCPDR:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.MaxRwkCount].Value = job.CellSpecial.MaxRwkCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CurrentRwkCount].Value = job.CellSpecial.CurrentRwkCount;
                            break;
                        case eJobDataLineType.CELL.CCTAM:
                        case eJobDataLineType.CELL.CCPTH:
                        case eJobDataLineType.CELL.CCGAP:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            break;
                        case eJobDataLineType.CELL.CCRWT:
                        case eJobDataLineType.CELL.CCCRP:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DotRepairCount].Value = job.CellSpecial.DotRepairCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.LineRepairCount].Value = job.CellSpecial.LineRepairCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                            break;
                        case eJobDataLineType.CELL.CCPOL:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.MainDefectCode].Value = job.CellSpecial.DefectCode;//Main Defect Code
                            break;
                        case eJobDataLineType.CELL.CCSOR:
                            //outputData.EventGroups[0].Events[0].Items[eJOBDATA.SortFlagNo].Value = job.CellSpecial.SortFlagNo;//Main Defect Code
                            break;
                        case eJobDataLineType.CELL.CCRWK:
                        case eJobDataLineType.CELL.CCQUP:
                        //case eJobDataLineType.CELL.CCQPP:
                        //case eJobDataLineType.CELL.CCPPK:
                        case eJobDataLineType.CELL.CCCHN:
                        case eJobDataLineType.CELL.CCQSR:
                            break;

                            //add by huangjiayin for t3 notch
                        case eJobDataLineType.CELL.CCNRD:
                        case eJobDataLineType.CELL.CCNLS:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                            break;
                        #endregion
                        default:
                            return;
                    }
                    #endregion
                }
                #endregion
                SendPLCData(outputData);

                _timerManager.CreateTimer(string.Format(Key_VCRMismatchJobDataTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);
                
                if (rtncode == eReturnCode1.NG) log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[{2}], Return Code=[{3}], Job WIP can't find.", eqpNo, trackKey, value, rtncode);
                else log = string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}],VCR Mismatch Job Data Request Reply Set Bit =[{2}].", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //Add By Yangzhenteng For Cell POL Line VCR Event,20180107
        private void VCRMismatchJobDataRequestReply_BEOL(string CommandNo, Line line, string eqpNo, eBitResult value, eReturnCode1 rtncode, Job job, string trackKey)
        {
            try
            {
                Trx outputData;
                string log = string.Empty;
                if (string.IsNullOrEmpty(CommandNo))
                    outputData = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo)) as Trx;
                else
                    outputData = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo) + "#" + CommandNo) as Trx;
                string TimeoutName;
                if (string.IsNullOrEmpty(CommandNo))
                    TimeoutName = string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo);
                else
                    TimeoutName = string.Format(Key_VCRMismatchJobDataRequestReportReply, eqpNo) + "#" + CommandNo;
                if (_timerManager.IsAliveTimer(TimeoutName))
                    _timerManager.TerminateTimer(TimeoutName);

                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)rtncode).ToString();
                outputData.EventGroups[0].Events[2].Items[0].Value = ((int)value).ToString();
                outputData.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                #region [If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputData.EventGroups[0].Events[1].IsDisable = true;
                    outputData.EventGroups[0].Events[0].IsDisable = true;
                    SendPLCData(outputData);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[OFF].", eqpNo, trackKey));
                    return;
                }
                #endregion
                #region [If job == null]
                if (job == null)
                {
                    outputData.EventGroups[0].Events[0].IsDisable = true;
                    SendPLCData(outputData);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[{2}], Return Code=[{3}], read & data Job WIP can't find.", eqpNo, trackKey, value, rtncode));

                    _timerManager.CreateTimer(TimeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);

                    return;
                }
                #endregion

                #region [ Job data]
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value = job.CassetteSequenceNo;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value = job.JobSequenceNo;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.GroupIndex].Value = job.GroupIndex;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value = job.ProductType.Value.ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CSTOperationMode].Value = ((int)job.CSTOperationMode).ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value = ((int)job.SubstrateType).ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CIMMode].Value = ((int)job.CIMMode).ToString();   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value = ((int)job.JobType).ToString();   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value = job.JobJudge;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.SamplingSlotFlag].Value = job.SamplingSlotFlag;   ////INT
                //outputData.EventGroups[0].Events[0].Items["OXRInformationRequestFlag"].Value = jobData.OXRInformationRequestFlag;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.FirstRunFlag].Value = job.FirstRunFlag;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value = job.JobGrade;   //ASCII
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value = job.GlassChipMaskBlockID;   //ASCII 
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value = job.PPID;   //ASCII
                #region CELL Special Job Data
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                    switch (line.Data.JOBDATALINETYPE)
                    {
                        #region [T3 rule]
                        case eJobDataLineType.CELL.CCPIL:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PILiquidType].Value = job.CellSpecial.PILiquidType;
                            break;
                        case eJobDataLineType.CELL.CCODF:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.AssembleSeqNo].Value = job.CellSpecial.AssembleSeqNo;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.UVMaskAlreadyUseCount].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                            break;
                        case eJobDataLineType.CELL.CCPCS:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize].Value = job.CellSpecial.BlockSize;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID].Value = job.CellSpecial.CUTProductID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType].Value = job.CellSpecial.CUTProductType;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID2].Value = job.CellSpecial.CUTProductID2;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType2].Value = job.CellSpecial.CUTProductType2;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode2].Value = job.CellSpecial.CUTCassetteSettingCode2;
                            break;
                        case eJobDataLineType.CELL.CCCUT:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductID].Value = job.CellSpecial.CUTProductID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductType].Value = job.CellSpecial.CUTProductType;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.RejudgeCount].Value = job.CellSpecial.RejudgeCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.VendorName].Value = job.CellSpecial.VendorName;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.BURCheckCount].Value = job.CellSpecial.BURCheckCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                            break;
                        case eJobDataLineType.CELL.CCPCK:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelGroup].Value = job.CellSpecial.PanelGroup;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OQCBank].Value = job.CellSpecial.OQCBank;
                            break;
                        case eJobDataLineType.CELL.CCPDR:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.MaxRwkCount].Value = job.CellSpecial.MaxRwkCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.CurrentRwkCount].Value = job.CellSpecial.CurrentRwkCount;
                            break;
                        case eJobDataLineType.CELL.CCTAM:
                        case eJobDataLineType.CELL.CCPTH:
                        case eJobDataLineType.CELL.CCGAP:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                            break;
                        case eJobDataLineType.CELL.CCRWT:
                        case eJobDataLineType.CELL.CCCRP:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DotRepairCount].Value = job.CellSpecial.DotRepairCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.LineRepairCount].Value = job.CellSpecial.LineRepairCount;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                            break;
                        case eJobDataLineType.CELL.CCPOL:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.MainDefectCode].Value = job.CellSpecial.DefectCode;//Main Defect Code
                            break;
                        case eJobDataLineType.CELL.CCSOR:
                            //outputData.EventGroups[0].Events[0].Items[eJOBDATA.SortFlagNo].Value = job.CellSpecial.SortFlagNo;//Main Defect Code
                            break;
                        case eJobDataLineType.CELL.CCRWK:
                        case eJobDataLineType.CELL.CCQUP:
                        //case eJobDataLineType.CELL.CCQPP:
                        //case eJobDataLineType.CELL.CCPPK:
                        case eJobDataLineType.CELL.CCCHN:
                        case eJobDataLineType.CELL.CCQSR:
                            break;

                        //add by huangjiayin for t3 notch
                        case eJobDataLineType.CELL.CCNRD:
                        case eJobDataLineType.CELL.CCNLS:
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                            outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                            break;

                        #endregion
                        default:
                            return;
                    }
                #endregion
                }
                #endregion
                SendPLCData(outputData);

                _timerManager.CreateTimer(TimeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VCRMismatchJobDataRequestReplyTimeout), trackKey);

                if (rtncode == eReturnCode1.NG) log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, VCRMismatchJobDataRequestReply Set Bit =[{2}], Return Code=[{3}], Job WIP can't find.", eqpNo, trackKey, value, rtncode);
                else log = string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}],VCR Mismatch Job Data Request Reply Set Bit =[{2}].", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private bool VCRMismatch_TypeA(Line line, Job vcrjob, string VCRReadGlassID)
        {
            try
            {
                //  [Array] 
                //- BC根据CassetteSeqNo, JobSeqNo找第一笔帐, 再根据CassetteSeqNo, VCR Glass ID找第二笔帐.
                //  若其中一笔帐找不到,则回复NG给机台. 若有找到兩片資料要作交換(MES不用) CstSeqNo,Job Seq No不能換。
                //  Download 給機台VCR Mismatch Job Data。
                if (vcrjob == null)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("VCR MISMATCH VCR READ GLASSID=[{0}] IN NOT IN WIP.",
                            VCRReadGlassID));
                    #region [CELL 找不到WIP]
                    //CELL: JobData=> GlassID用VCR照到的, 
                    //CST SEQ NO及Job SEQ NO用原本的,
                    //OXR給O, Judge給OK, Grade也給OK, FLAG, 照填進去
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        vcrjob = VCRMismatchMESClone(line, vcrjob, VCRReadGlassID.Trim());
                        CELL_VCR_Replace_JobDATA(line, ref vcrjob);
                    }
                    #endregion
                    return false;
                }

                //Jun Modify 20150505 需要使用j.GlassChipMaskBlockID，使用j..MESPRODUCT.PRODUCTNAME會有邏輯錯誤
                ////Watson Modify Serch J.MESPRODUCT.PRODUCTNAME 不是找 j.GlassChipMaskBlockID
                Job inCSTjob;
                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    inCSTjob = ObjectManager.JobManager.GetJobs(vcrjob.CassetteSequenceNo).FirstOrDefault(j => j.GlassChipMaskBlockID.Trim() == VCRReadGlassID.Trim());
                else
                    inCSTjob = ObjectManager.JobManager.GetJobs(vcrjob.CassetteSequenceNo).FirstOrDefault(j => j.CellSpecial.MASKID.Trim() == VCRReadGlassID.Trim());

                if (inCSTjob == null)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("VCR MISMATCH VCR READ GLASSID=[{0}] AND VCR READ CASSETTE SEQ NO=[{1}] IN NOT IN WIP.",
                            VCRReadGlassID, vcrjob.CassetteSequenceNo));
                    #region [CELL 找不到WIP]
                    //CELL: JobData=> GlassID用VCR照到的, 
                    //CST SEQ NO及Job SEQ NO用原本的,
                    //OXR給O, Judge給OK, Grade也給OK, FLAG, 照填進去
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        //Jun Add 20150505 如果找不到In CST Job，先備份舊的Job，之後再更新Glass ID
                        Job jobBak = new Job();
                        jobBak = (Job)vcrjob.Clone();
                        List<Job> jobList = ObjectManager.JobManager.GetJobs(vcrjob.CassetteSequenceNo).Where(j => int.Parse(j.JobSequenceNo) >= 60000).ToList();
                        if (jobList != null && jobList.Count > 0)
                        {
                            jobBak.JobSequenceNo = (jobList.Max(j => int.Parse(j.JobSequenceNo)) + 1).ToString();
                            jobBak.JobKey = string.Format("{0}_{1}", jobBak.CassetteSequenceNo, jobBak.JobSequenceNo);
                        }
                        else
                        {
                            jobBak.JobSequenceNo = "60000";
                            jobBak.JobKey = string.Format("{0}_{1}", jobBak.CassetteSequenceNo, jobBak.JobSequenceNo);
                        }
                        ObjectManager.JobManager.AddJob(jobBak);
                        ObjectManager.JobManager.EnqueueSave(jobBak);

                        vcrjob = VCRMismatchMESClone(line,vcrjob, VCRReadGlassID.Trim());
                        //CELL_VCR_Replace_JobDATA(line, ref vcrjob);
                    }
                    #endregion
                    return false;
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("VCR MISMATCH TYPE A VCR READ GLASSID=[{0}] AND VCR READ CST_SEQ_NO=[{1}], JOB_SEQ_NO=[{2}] REPLACE WIP GLASSID=[{3}] AND JOB_SEQ_NO=[{4}] JOB_DATA.",
                        VCRReadGlassID.Trim(), vcrjob.CassetteSequenceNo,vcrjob.JobSequenceNo, inCSTjob.GlassChipMaskBlockID, inCSTjob.JobSequenceNo));

                #region [Glass資料互換]
                Job temp = new Job();
                temp = (Job)vcrjob.Clone();   //找到的Job

                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    vcrjob.GlassChipMaskBlockID = VCRReadGlassID.Trim();
                else
                    vcrjob.CellSpecial.MASKID = VCRReadGlassID.Trim();
                vcrjob.EQPJobID = VCRReadGlassID.Trim();
                vcrjob = VCRMismatchClone(line.Data.FABTYPE, vcrjob, inCSTjob);

                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    inCSTjob.GlassChipMaskBlockID = temp.GlassChipMaskBlockID.Trim();
                else
                    inCSTjob.CellSpecial.MASKID = temp.CellSpecial.MASKID.Trim();
                inCSTjob.EQPJobID = temp.EQPJobID.Trim();
                inCSTjob = VCRMismatchClone(line.Data.FABTYPE, inCSTjob, temp);
                #endregion

                //Watson Add 20150425 For VCR Mismatch Save Job History
                ObjectManager.JobManager.RecordJobHistory(inCSTjob, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, eJobEvent.VCR_Mismatch_Copy.ToString(), this.CreateTrxID(), string.Empty, string.Empty);
  
                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }
        private void VCRMismatch_TypeB(Job job, string VCRReadGlassID)
        {
            try
            {
                //  [CF] 直接更新Glass ID.
                if (job ==null)
                    return;

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("VCR MISMATCH TYPE B VCR READ GLASSID=[{0}] CST_SEQ_NO=[{1}], JOB_SEQ_NO=[{2}] REPLACE WIP GLASSID=[{3}].",
                        VCRReadGlassID.Trim(), job.CassetteSequenceNo, job.JobSequenceNo,job.GlassChipMaskBlockID));

                job.VCRJobID = VCRReadGlassID;
                job.GlassChipMaskBlockID = VCRReadGlassID;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        private Job VCRMismatchClone(string fabtype,Job targetjob,Job sourcejob)
        {
            try
            {
                //Jun Modify 20150401 fabtype是CELL, CF, ARRAY 無法使用int.Parse
                //eFabType fabType = (eFabType)int.Parse(fabtype);

                #region [Common]
                targetjob.CSTOperationMode = sourcejob.CSTOperationMode;
                targetjob.JobJudge = sourcejob.JobJudge;
                targetjob.SamplingSlotFlag = sourcejob.SamplingSlotFlag;
                targetjob.OXRInformation = sourcejob.OXRInformation;
                targetjob.FirstRunFlag = sourcejob.FirstRunFlag;
                targetjob.JobGrade = sourcejob.JobGrade;
                targetjob.PPID = sourcejob.PPID;
                targetjob.MES_PPID = sourcejob.MES_PPID;
                #endregion

                if (fabtype == eFabType.CELL.ToString())
                {
                    #region CELL Job Clone
                    targetjob.CellSpecial = (JobCellSpecial)sourcejob.CellSpecial.Clone();                    
                    #endregion
                }
                else
                {
                    #region Array ,CF Job Clone
                    targetjob.CfSpecial = (JobCfSpecial)sourcejob.CfSpecial.Clone();
                    targetjob.ArraySpecial = (JobArraySpecial)sourcejob.ArraySpecial.Clone();

                    targetjob.GroupIndex = sourcejob.GroupIndex;
                    targetjob.ProductType = sourcejob.ProductType;
                    targetjob.ProductID = sourcejob.ProductID;
                    targetjob.SubstrateType = sourcejob.SubstrateType;
                    targetjob.CIMMode = sourcejob.CIMMode;
                    targetjob.JobType = sourcejob.JobType;
                    targetjob.CSTOperationMode = sourcejob.CSTOperationMode;
                    targetjob.JobJudge = sourcejob.JobJudge;
                    targetjob.SamplingSlotFlag = sourcejob.SamplingSlotFlag;
                    targetjob.OXRInformationRequestFlag = sourcejob.OXRInformationRequestFlag;
                    targetjob.FirstRunFlag = sourcejob.FirstRunFlag;
                    targetjob.JobGrade = sourcejob.JobGrade;
                    targetjob.INSPReservations = sourcejob.INSPReservations;
                    targetjob.EQPReservations = sourcejob.EQPReservations;
                    targetjob.LastGlassFlag = sourcejob.LastGlassFlag;
                    targetjob.InspJudgedData = sourcejob.InspJudgedData;
                    targetjob.TrackingData = sourcejob.TrackingData;
                    targetjob.EQPFlag = sourcejob.EQPFlag;
                    targetjob.OXRInformation = sourcejob.OXRInformation;
                    targetjob.ChipCount = sourcejob.ChipCount;

                    targetjob.JobProcessFlows = new SerializableDictionary<string, ProcessFlow>();
                    if (sourcejob.JobProcessFlows != null)
                    {
                        foreach (string key in sourcejob.JobProcessFlows.Keys)
                            targetjob.JobProcessFlows.Add(key, (ProcessFlow)sourcejob.JobProcessFlows[key].Clone());
                    }
                    targetjob.HoldInforList = new List<HoldInfo>();
                    if (sourcejob.HoldInforList != null)
                    {
                        foreach (HoldInfo info in sourcejob.HoldInforList)
                            targetjob.HoldInforList.Add((HoldInfo)info.Clone());
                    }
                    targetjob.DefectCodes = new List<DefectCode>();
                    if (sourcejob.DefectCodes != null)
                    {
                        foreach (DefectCode dc in sourcejob.DefectCodes)
                            targetjob.DefectCodes.Add((DefectCode)dc.Clone());
                    }
                    targetjob.QtimeList = new List<Qtimec>();
                    if (sourcejob.QtimeList != null)
                    {
                        foreach (Qtimec qt in sourcejob.QtimeList)
                            targetjob.QtimeList.Add((Qtimec)qt.Clone());
                    }
                    #endregion
                }
                return targetjob;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        private Job VCRMismatchMESClone(Line line,Job targetjob, string vcrGlassID)
        {
            try
            {
                targetjob.EQPJobID = targetjob.MesProduct.PRODUCTNAME;
                targetjob.LineRecipeName = targetjob.MesProduct.PRODUCTRECIPENAME;
                Dictionary<string, int> groupIndex = new Dictionary<string, int>();
                targetjob.GroupIndex = ObjectManager.JobManager.M2P_GetCellGroupIndex(line,targetjob.MesProduct.GROUPID);

                targetjob.SubstrateType = targetjob.MesCstBody.LOTLIST[0].PRODUCTGCPTYPE == string.Empty ? targetjob.SubstrateType : ObjectManager.JobManager.M2P_GetSubstrateType(targetjob.MesCstBody.LOTLIST[0].PRODUCTGCPTYPE);
                targetjob.CIMMode = eBitResult.ON;
                targetjob.JobType = ObjectManager.JobManager.M2P_GetJobType(targetjob.MesProduct.PRODUCTTYPE) == eJobType.Unknown ? targetjob.JobType : ObjectManager.JobManager.M2P_GetJobType(targetjob.MesProduct.PRODUCTTYPE);

                targetjob.JobJudge = ConstantManager[string.Format("PLC_{0}_JOBJUDGE", eFabType.CELL.ToString())][targetjob.MesProduct.PRODUCTJUDGE.Trim()].Value;

                targetjob.SamplingSlotFlag = targetjob.MesProduct.PROCESSFLAG.Equals("Y") ? "1" : "0";
                targetjob.FirstRunFlag = "0";

                targetjob.JobGrade = targetjob.MesProduct.PRODUCTGRADE.Trim();
                //CELL: JobData=> GlassID用VCR照到的
                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                    targetjob.GlassChipMaskBlockID = vcrGlassID;
                else
                    targetjob.CellSpecial.MASKID = vcrGlassID;

                targetjob.EQPJobID = vcrGlassID;

                //Jun Modify 20150507 CSOT要求如果找不到交換的Glass資料，OXR全部預設全給O
                //string mesOXR = targetjob.MesProduct.SUBPRODUCTGRADES.Trim();
                //if (mesOXR.Length > 0)
                //{
                //    targetjob.ChipCount = mesOXR.Length;
                //    targetjob.OXRInformationRequestFlag = targetjob.ChipCount > 56 ? "1" : "0";
                //    targetjob.OXRInformation = targetjob.MesProduct.SUBPRODUCTGRADES;
                //}
                //else
                //{
                //Jun Add 20150506 如果MES沒有資料，就全部寫O
                targetjob.OXRInformation = new string('O', targetjob.ChipCount);
                //}
                //job.HostDefectCodeData = Transfer_DefectCodeToFileFormat(targetjob.MesProduct.SUBPRODUCTDEFECTCODE);
                return targetjob;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        #region [CELL Special VCR Replace Method]
        public void JobDataRequestReportReply_CELL(Trx outputData, Job job, Line line, string eqpNo)
        {
            try
            {
                #region CELL Special Job Data
                outputData.EventGroups[0].Events[1].Items["EQPFlag"].Value = job.EQPFlag;   //BIN
                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                {
                    outputData.EventGroups[0].Events[1].Items["INSPReservations"].Value = job.INSPReservations;   //BIN
                    outputData.EventGroups[0].Events[1].Items["EQPReservations"].Value = job.EQPReservations;   //BIN
                    outputData.EventGroups[0].Events[1].Items["LastGlassFlag"].Value = job.LastGlassFlag;   ////INT
                    outputData.EventGroups[0].Events[1].Items["Insp.JudgedData"].Value = job.InspJudgedData;   //BIN
                    outputData.EventGroups[0].Events[1].Items["TrackingData"].Value = job.TrackingData;   //BIN

                    //outputData.EventGroups[0].Events[1].Items["OXRInformation"].Value = job.OXRInformation;   //BIN
                    #region [OXR Information]
                    string oxrInfo = string.Empty;
                    for (int i = 0; i < job.OXRInformation.Length; i++)
                    {
                        if (i.Equals(56)) break;
                        oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][job.OXRInformation.Substring(i, 1)].Value;
                    }
                    outputData.EventGroups[0].Events[1].Items["OXRInformation"].Value = oxrInfo;
                    #endregion

                    outputData.EventGroups[0].Events[1].Items["ChipCount"].Value = job.ChipCount.ToString();   //INT
                }

                switch (line.Data.JOBDATALINETYPE)
                {
                    case eJobDataLineType.CELL.CBPIL:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        outputData.EventGroups[0].Events[1].Items["PPOSlotNo"].Value = job.CellSpecial.PPOSlotNo;
                        outputData.EventGroups[0].Events[1].Items["ReworkCount"].Value = job.CellSpecial.ReworkCount;
                        break;
                    case eJobDataLineType.CELL.CCODF: 
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ArrayTTPEQVersion"].Value = job.CellSpecial.ArrayTTPEQVer;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        outputData.EventGroups[0].Events[1].Items["CFCasetteSeqNo"].Value = job.CellSpecial.CFCassetteSeqNo;
                        outputData.EventGroups[0].Events[1].Items["CFJobSeqno"].Value = job.CellSpecial.CFJobSeqNo;
                        outputData.EventGroups[0].Events[1].Items["ODFBoxChamberOpenTime#01"].Value = job.CellSpecial.ODFBoxChamberOpenTime01;
                        outputData.EventGroups[0].Events[1].Items["ODFBoxChamberOpenTime#02"].Value = job.CellSpecial.ODFBoxChamberOpenTime02;
                        outputData.EventGroups[0].Events[1].Items["ODFBoxChamberOpenTime#03"].Value = job.CellSpecial.ODFBoxChamberOpenTime03;
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["RepairCount"].Value = job.CellSpecial.RepairCount;
                        outputData.EventGroups[0].Events[1].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                        outputData.EventGroups[0].Events[1].Items["UVMaskAlreadyUseCount"].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                        break;
                    case eJobDataLineType.CELL.CBHVA:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        outputData.EventGroups[0].Events[1].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                        outputData.EventGroups[0].Events[1].Items["ReturnModeTurnAngle"].Value = job.CellSpecial.ReturnModeTurnAngle;
                        break;
                    case eJobDataLineType.CELL.CBCUT:
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                        outputData.EventGroups[0].Events[1].Items["CrossLineCassetteSettingCode"].Value = job.CellSpecial.CrossLineCassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSizeFlag"].Value = job.CellSpecial.PanelSizeFlag;
                        outputData.EventGroups[0].Events[1].Items["MMGFlag"].Value = job.CellSpecial.MMGFlag;
                        outputData.EventGroups[0].Events[1].Items["CrossLinePanelSize"].Value = job.CellSpecial.CrossLinePanelSize;
                        outputData.EventGroups[0].Events[1].Items["CUTProductID"].Value = job.CellSpecial.CUTProductID;
                        outputData.EventGroups[0].Events[1].Items["CUTCrossProductID"].Value = job.CellSpecial.CUTCrossProductID;
                        outputData.EventGroups[0].Events[1].Items["CUTProductType"].Value = job.CellSpecial.CUTProductType;
                        outputData.EventGroups[0].Events[1].Items["CUTCrossProductType"].Value = job.CellSpecial.CUTCrossProductType;
                        outputData.EventGroups[0].Events[1].Items["POLProductType"].Value = job.CellSpecial.POLProductType;
                        outputData.EventGroups[0].Events[1].Items["POLProductID"].Value = job.CellSpecial.POLProductID;
                        outputData.EventGroups[0].Events[1].Items["CrossLinePPID"].Value = job.CellSpecial.CrossLinePPID;
                        break;
                    case eJobDataLineType.CELL.CBPOL:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBDPK:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBPMT:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        outputData.EventGroups[0].Events[1].Items["RunMode"].Value = job.CellSpecial.RunMode;
                        outputData.EventGroups[0].Events[1].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        break;
                    case eJobDataLineType.CELL.CBGAP:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        outputData.EventGroups[0].Events[1].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        break;
                    case eJobDataLineType.CELL.CBPIS:
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBPRM:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["RunMode"].Value = job.CellSpecial.RunMode;
                        break;
                    case eJobDataLineType.CELL.CBGMO:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBLOI:
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["RepairResult"].Value = job.CellSpecial.RepairResult;
                        outputData.EventGroups[0].Events[1].Items["RunMode"].Value = job.CellSpecial.RunMode;
                        //outputData.EventGroups[0].Events[1].Items["VirtualPortEnableMode"].Value = job.CellSpecial.VirtualPortEnableMode;  Jun Modify 20150107 For New IO
                        break;
                    case eJobDataLineType.CELL.CBNRP:
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["RepairResult"].Value = job.CellSpecial.RepairResult;
                        outputData.EventGroups[0].Events[1].Items["RepairCount"].Value = job.CellSpecial.RepairCount;
                        outputData.EventGroups[0].Events[1].Items["RunMode"].Value = job.CellSpecial.RunMode;
                        break;
                    case eJobDataLineType.CELL.CBOLS:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBSOR:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["RunMode"].Value = job.CellSpecial.RunMode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                        break;
                    case eJobDataLineType.CELL.CBDPS:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        break;
                    case eJobDataLineType.CELL.CBATS:
                        outputData.EventGroups[0].Events[1].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        outputData.EventGroups[0].Events[1].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                        outputData.EventGroups[0].Events[1].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                        break;
                    case eJobDataLineType.CELL.CBDPI:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                        break;
                    case eJobDataLineType.CELL.CBUVA:
                        outputData.EventGroups[0].Events[1].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                        outputData.EventGroups[0].Events[1].Items["ProductID"].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[1].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[1].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                        break;

                    case eJobDataLineType.CELL.CBMCL:
                        outputData.EventGroups[0].Events[1].Items["MaskID"].Value = job.CellSpecial.MASKID;
                        break;
                }
                #endregion
                SendPLCData(outputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, JOB DATA REQUEST REPORT SET BIT =[{2}], RETURN CODE=[{3}].",
                    eqpNo, outputData.TrackKey, eBitResult.ON, eReturnCode1.OK));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CELL_VCR_Replace_JobDATA(Line line, ref Job job)
        {
            try
            {  // 初始化參數內容
                if (job == null)
                    return;
                //job.InspJudgedData = new string('0', 32);
                //job.TrackingData = new string('0', 32);
                //job.EQPFlag = new string('0', 32);

                // 取出 Abnormal Code List 內容
                List<CODEc> abnormalcodelist = job.MesProduct.ABNORMALCODELIST;
                string abnormalULD = string.Empty;  //Job Data
                string beveledFlag = string.Empty;  //Job Data
                string bubbleSampleFlag = string.Empty;  //Job Data
                string chippigFlag = string.Empty;  //Job Data
                string cutGradeFlag = string.Empty;  //待確認 
                string cutSlimReworkFlag = string.Empty;  //Job Data
                string CFSideResidueFlag = string.Empty;  //Job Data
                string CellCutRejudgeCount = string.Empty;  //Job Data
                string dimpleFlag = string.Empty;  //Job Data
                string hvaChippigFlag = string.Empty;  //Job Data 
                string lineReworkCount = string.Empty;//Job Data
                string LOIFlag = string.Empty;//Job Data
                string mgvFlag = string.Empty;  //Job Data
                string pitype = string.Empty;  //Job Data
                string pointReworkCount = string.Empty;//Job Data
                string PDRReworkCount = string.Empty;  //Job Data
                string ribMarkFlag = string.Empty;  //Job Data
                string repairFlag = string.Empty;  //Job Data
                string shellFlag = string.Empty;  //Job Data
                string sealAbnormalFlag = string.Empty;  //Job Data
                string sortFlag = string.Empty;  //Job Data
                string ttpFlag = string.Empty;  //Job Data
                string turnAngle = string.Empty;  //Job Data

                string abnormalTFT = string.Empty;  //**File Service**
                string abnormalCF = string.Empty;  //**File Service**
                string abnormalLCD = string.Empty;  //**File Service**
                string deCreateFlag = string.Empty;  //**File Service**
                string fGradeFlag = string.Empty;  //**File Service**

                foreach (CODEc abnormalcode in abnormalcodelist)
                {
                    //string abnormalSeq = abnormalcode.ABNORMALSEQ.Trim();
                    string abnormalSeq = abnormalcode.ABNORMALVALUE.Trim();
                    switch (abnormalSeq)
                    {
                        //For Job Data Use
                        case "ABCODEULD": abnormalULD = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "BEVELEFLAG": beveledFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "BUBBLESAMPLEFLAG": bubbleSampleFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "CELLCUTREJUDGECOUNT": CellCutRejudgeCount = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "CHIPPINGFLAG": chippigFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "CFSIDERESIDUEFLAG": CFSideResidueFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "CUTSLIMREWORKFLAG": cutSlimReworkFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "CUTGradeFlag": cutGradeFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "DIMPLEFLAG": dimpleFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "GLASSCHANGEANGLE": turnAngle = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "HVACHIPPINGFLAG": hvaChippigFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "LINEREWORKCOUNT": lineReworkCount = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "LOIFLAG": LOIFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "MGVFLAG": mgvFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "PITYPE": pitype = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "PDRREWORKCOUNT": pitype = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "POINTREWORKCOUNT": pointReworkCount = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "RIBMARKFLAG": ribMarkFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "REPAIRRESULTFLAG": repairFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "SHEFLAG": shellFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "SEALABNORMALFLAG": sealAbnormalFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "SORTFLAG": sortFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        case "TTPFLAG": ttpFlag = abnormalcode.ABNORMALCODE.Trim(); break;
                        //For File Service Use
                        case "ABNORMALTFT":
                            abnormalTFT =  abnormalcode.ABNORMALCODE.Trim(); ;
                            job.CellSpecial.AbnormalTFT = abnormalTFT;
                            break;
                        //case "ABNORMALCF": 
                        //    abnormalCF =  abnormalcode.ABNORMALCODE.Trim(); break;
                        //    job.CellSpecial.AbnormalCF = abnormalCF;
                        //    break;
                        case "ABNORMALLCD":
                            abnormalLCD =  abnormalcode.ABNORMALCODE.Trim();
                            job.CellSpecial.AbnormalLCD = abnormalLCD;
                            break;
                        case "DECREASEFLAG":
                            deCreateFlag = abnormalcode.ABNORMALCODE.Trim();
                            job.CellSpecial.DeCreateFlag = deCreateFlag;
                            break;
                        case "FGRADERISKFLAG":
                            fGradeFlag = abnormalcode.ABNORMALCODE.Trim();
                            job.CellSpecial.FGradeFlag = fGradeFlag;
                            break;
                    }
                }

                double subPnlSize = 0;
                List<PROCESSLINEc> processList = new List<PROCESSLINEc>();
                List<STBPRODUCTSPECc> stbList = new List<STBPRODUCTSPECc>();
                if (job.MesCstBody.LOTLIST != null)
                {
                    processList = job.MesCstBody.LOTLIST[0].PROCESSLINELIST;
                    stbList = job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST;
                }
                IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem("EQPFlag");
                //Jun Add 20150506 EQPFlag裡面需要Turn On VCRMismatchAndBCReplyNG Flag
                if (sub != null && sub.ContainsKey("VCRMismatchAndBCReplyNG"))
                    sub["VCRMismatchAndBCReplyNG"] = "1";

                #region Job Data Line Special
                switch (line.Data.JOBDATALINETYPE)
                {
                    #region [CCPIL]
                    case eJobDataLineType.CELL.CCPIL:
                        if (sub == null) return;
                        if (sub.ContainsKey("ShellFlag")) sub["ShellFlag"] = shellFlag == "" ? "0" : "1";
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCODF]
                    case eJobDataLineType.CELL.CCODF:
                        if (sub == null) return;
                        if (sub.ContainsKey("TTPFlag"))
                            sub["TTPFlag"] = ttpFlag.Trim() == "" ? "0" : "1";

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;
                    #endregion
                    #region [CCPCS]
                    case eJobDataLineType.CELL.CCPCS:
                        if (sub == null) return;
                        if (sub.ContainsKey("SealErrorFlag")) sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";//SealErrorFlag: From ODF to BlockCut
                        if (sub.ContainsKey("ChippigFlag")) sub["ChippigFlag"] = chippigFlag.Trim() == "" ? "0" : "1";//ChippingFlag: From ODF to BlockCut
                        if (sub.ContainsKey("BubbleSampleFlag")) sub["BubbleSampleFlag"] = bubbleSampleFlag.Trim() == "" ? "0" : "1";//BubbleSampleFlag: From ODF to BlockCut 
                        break;
                    #endregion
                    #region [CCCUT]
                    case eJobDataLineType.CELL.CCCUT:
                        if (sub == null) return;
                        if (sub.ContainsKey("SealErrorFlag")) sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";//SealErrorFlag: From BlockCut to CUT
                        if (sub.ContainsKey("ChippigFlag")) sub["ChippigFlag"] = chippigFlag.Trim() == "" ? "0" : "1";//ChippingFlag: From BlockCut to CUT
                        if (sub.ContainsKey("BeveledFlag")) sub["BeveledFlag"] = beveledFlag.Trim() == "" ? "0" : "1";//BeveledFlag: From CUT to CUT
                        if (sub.ContainsKey("CFSideResidueFlag")) sub["CFSideResidueFlag"] = CFSideResidueFlag.Trim() == "" ? "0" : "1";//CFSideResidueFlag: From CUT 
                        if (sub.ContainsKey("RibMarkFlag")) sub["RibMarkFlag"] = ribMarkFlag.Trim() == "" ? "0" : "1";//RibMarkFlag: From CUT
                        if (sub.ContainsKey("LOIFlag")) sub["LOIFlag"] = LOIFlag.Trim() == "" ? "0" : "1";//LOIFlag: From CUT to (CUT and RWT)
                        if (sub.ContainsKey("CutSlimReworkFlag")) sub["CutSlimReworkFlag"] = cutSlimReworkFlag.Trim() == "" ? "0" : "1";//CutSlimReworkFlag: From CUT to CUT
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCPOL]
                    case eJobDataLineType.CELL.CCPOL:
                        if (sub == null) return;
                        if (sub.ContainsKey("DimpleFlag")) sub["DimpleFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCRWK]
                    case eJobDataLineType.CELL.CCRWK:
                        if (sub == null) return;
                        if (sub.ContainsKey("LOIFlag")) sub["LOIFlag"] = LOIFlag.Trim() == "" ? "0" : "1";//LOIFlag: From CUT to (CUT and RWT)
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCQUP]
                    case eJobDataLineType.CELL.CCQUP:
                        break;
                    #endregion
                    #region [CCPDR]
                    case eJobDataLineType.CELL.CCPDR:
                        break;
                    #endregion
                    #region [CCPTH]
                    case eJobDataLineType.CELL.CCPTH:
                        break;
                    #endregion
                    #region [CCTAM]
                    case eJobDataLineType.CELL.CCTAM:;
                        break;
                    #endregion
                    #region [CCGAP]
                    case eJobDataLineType.CELL.CCGAP:
                        break;
                    #endregion
                    #region [CCRWT]
                    case eJobDataLineType.CELL.CCRWT:
                        break;
                    #endregion
                    #region [CCCRP]
                    case eJobDataLineType.CELL.CCCRP:
                        break;
                    #endregion

                    #region [CCSOR]
                    case eJobDataLineType.CELL.CCSOR:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        if (sub == null) return;
                        if (sub.ContainsKey("DCRandSorterFlag"))
                        {
                            switch (sortFlag)
                            {
                                case "1":
                                    sub["DCRandSorterFlag"] = "01";//DCR Flag Glass
                                    break;
                                case "2":
                                    sub["DCRandSorterFlag"] = "10";//Sorter Flag Glass
                                    break;
                                case "3":
                                    sub["DCRandSorterFlag"] = "11";//DCR and Sorter Flag Glass
                                    break;
                                default:
                                    sub["DCRandSorterFlag"] = "00";
                                    break;
                            }
                        }
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion
                    #region [CCQSR]
                    case eJobDataLineType.CELL.CCQSR:
                        job.CellSpecial.ControlMode = line.File.HostMode;
                        if (sub == null) return;
                        if (sub.ContainsKey("DCRandSorterFlag"))
                        {
                            switch (sortFlag)
                            {
                                case "1":
                                    sub["DCRandSorterFlag"] = "01";//DCR Flag Glass
                                    break;
                                case "2":
                                    sub["DCRandSorterFlag"] = "10";//Sorter Flag Glass
                                    break;
                                case "3":
                                    sub["DCRandSorterFlag"] = "11";//DCR and Sorter Flag Glass
                                    break;
                                default:
                                    sub["DCRandSorterFlag"] = "00";
                                    break;
                            }
                        }
                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                    #endregion

                    #region[CCNLS]
                    case eJobDataLineType.CELL.CCNLS:
                        break;
                    #endregion

                    #region[CCNRD]
                    case eJobDataLineType.CELL.CCNRD:
                        break;
                    #endregion



                    case eJobDataLineType.CELL.CBHVA:
                        if (sub == null) return;
                        if (sub.ContainsKey("SealErrorFlag"))
                            sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");

                        break;

                    case eJobDataLineType.CELL.CBCUT:
                        int proType;
                        if (job.MesCstBody.LOTLIST != null)
                        {
                            int.TryParse(job.MesCstBody.LOTLIST[0].BCPRODUCTTYPE, out proType);
                            job.ProductType.Value = proType;
                            //判斷是否為Cross Line的Glass

                            string[] subProLine = job.MesCstBody.LOTLIST[0].SUBPRODUCTLINES.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                            //Panel Size and Cross Panel Size
                            subProLine = job.MesCstBody.LOTLIST[0].SUBPRODUCTLINES.Split(';');
                            string[] subProPnlSize = job.MesCstBody.LOTLIST[0].SUBPRODUCTSIZES.Split(';');
                            for (int j = 0; j < subProLine.Length; j++)
                            {
                                if (subProLine[j].Trim() != "")
                                {
                                    if (double.TryParse(subProPnlSize[j], out subPnlSize))
                                        subPnlSize = subPnlSize * 100;

                                    if (subProLine[j] == line.Data.LINEID)
                                        job.CellSpecial.PanelSize = subPnlSize.ToString();
                                    else
                                        job.CellSpecial.CrossLinePanelSize = subPnlSize.ToString();
                                }
                            }

                            if (sub == null) return;
                            if (sub.ContainsKey("SealErrorFlag"))
                                sub["SealErrorFlag"] = sealAbnormalFlag.Trim() == "" ? "0" : "1";

                            if (sub.ContainsKey("UnfilledCornerFlag"))
                                sub["UnfilledCornerFlag"] = hvaChippigFlag.Trim() == "" ? "0" : "1";

                            if (sub.ContainsKey("PanelSizeFlag"))
                            {
                                if (job.MesCstBody.LOTLIST != null)
                                    sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                            }

                            //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        }
                        break;

                    case eJobDataLineType.CELL.CBPOL:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBDPK:
                        if (sub == null) return;
                        if (sub.ContainsKey("CuttingFlag"))
                            sub["CuttingFlag"] = CellCuttingFlag(job.MesProduct.SHORTCUTFLAG.Trim());
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBPIS:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBPRM:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBGMO:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBLOI:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBNRP:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBOLS:
                        if (sub == null) return;
                        if (sub.ContainsKey("CuttingFlag"))
                            sub["CuttingFlag"] = CellCuttingFlag(job.MesProduct.SHORTCUTFLAG.Trim());
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBSOR:
                        if (sub == null) return;
                        if (sub.ContainsKey("MGVFlag"))
                            sub["MGVFlag"] = mgvFlag != "Y" ? "0" : "1";
                        if (sub.ContainsKey("MHUFlag"))
                            sub["MHUFlag"] = job.MesProduct.MHUFLAG.Trim() != "Y" ? "0" : "1";

                        if (sub.ContainsKey("FMAFlag"))
                            sub["FMAFlag"] = job.MesProduct.FMAFLAG.Trim() != "Y" ? "0" : "1";

                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBDPS:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;

                    case eJobDataLineType.CELL.CBATS:
                        if (sub == null) return;
                        if (sub.ContainsKey("PanelSizeFlag"))
                        {
                            if (job.MesCstBody.LOTLIST != null)
                                sub["PanelSizeFlag"] = CellPanelSizeType(job.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE.Trim());
                        }

                        //job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                        break;
                }

                job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, "EQPFlag");
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private string CellPanelSizeType(string sizeType)
        {
            try
            {
                switch (sizeType)
                {
                    case "B1": return "1";
                    case "B2": return "2";
                    case "S1": return "3";
                    case "S2": return "4";
                    default: return "0";
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }
        private string CellCuttingFlag(string cuttingFlag)
        {
            try
            {
                switch (cuttingFlag)
                {
                    case "NC": return "0";
                    case "OLSOK": return "1";
                    case "LSCOK": return "2";
                    case "NG": return "3";
                    default: return "0";
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "0";
            }
        }

        private Line GetLineByLines(string lineApproxname)
        {
            try
            {
                foreach (Line line in ObjectManager.LineManager.GetLines())
                {
                    if (line.Data.LINEID.Contains(lineApproxname))
                        return line;
                }
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineApproxname));
                return null;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }
        #endregion

        #region[CELL BEOL VCR Read Loss Check Function>>Transfer Stop Command]

        //20180621 by huangjiayin
        public void VCRReadLossCheck(Line line, Job sendOutJob, Equipment checkedEq, string trxid)
        {
            try
            {
                if (!string.IsNullOrEmpty(sendOutJob.VCRJobID)) return;
                string jobKey = sendOutJob.JobKey;
                string jobId = sendOutJob.EQPJobID;
                string transactionId = trxid;
                string msg = string.Format("[VCRReadLoss] Key=[{0}],JobId=[{1}].", jobKey, jobId);

                //Warn Log
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msg);

                //CIM Message Set
                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { transactionId, checkedEq.Data.NODENO, msg, "", "1" });

                //Terminal Message
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { transactionId, line.Data.LINEID, msg });

                //Transfer Stop Command
                object[] _data1 = new object[]
                    { 
                        checkedEq.Data.NODENO,
                        "1",//1:stop;2:resume;
                        transactionId
                    };
                Invoke(eServiceName.EquipmentService, "TransferStopCommand", _data1);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        private void VCRReadLossCheck_PCK()
        {
            while (true)
            {
                try
                {
                    Line line = ObjectManager.LineManager.GetLines().First(l => l.Data.LINETYPE == eLineType.CELL.CCPCK);
                    if (line == null) return;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L3");
                    if (eqp == null) return;
                    Job job = ObjectManager.JobManager.GetJob(59);
                    if (job == null) return;
                    string trxid = this.CreateTrxID();

                    Invoke(eServiceName.VCRService, "VCRReadLossCheck", new object[] { line, job, eqp, trxid });
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                Thread.Sleep(60 * 1000);
            }
 
        }
        
        #endregion

    }
}
