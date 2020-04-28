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
using System;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Core;


namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public partial class CELLSpecialService : AbstractService
    {
        private const string InLineQTimeOverTimeout = "InLineQTimeOverTimeout";

        public void InLineQTimeOverCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.ON)
                {
                    if (eqp.Data.LINEID.Contains(keyCellLineType.ODF)) //sy add 20160907
                        InLineQTimeOverCommand_ODF(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey, string.Empty, string.Empty, null);
                    else
                        InLineQTimeOverCommand_PI(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey, string.Empty, string.Empty, null);

                    if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + InLineQTimeOverTimeout))
                        _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + InLineQTimeOverTimeout);

                }

                #region [拆出PLCAgent Data]  Word
                string returncode = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}],  NODE=[{3}] ,BIT=[{4}], RETURNCODE =[{5}]",
                             eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, bitResult.ToString(), returncode));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region Write PLC and Time out
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        /// <param name="cstseqno"></param>
        /// <param name="jobseqno"></param>
        /// <param name="odfoverqtime"></param>
        public void InLineQTimeOverCommand_ODF(string eqpNo, eBitResult value, string trackKey, string cstseqno, string jobseqno, eCELLInLineOverQtimeODF odfoverqtime)
        {
            try
            {
                int overqtimeDecvlaue = 0;
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_InLineQTimeOverCommand") as Trx;

                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].Items[2].Value = new string('0', 16);
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET Bit=[{2}].",
                    eqpNo, trackKey, value.ToString()));

                    return;
                }


                outputdata.EventGroups[0].Events[0].Items[0].Value = cstseqno;
                outputdata.EventGroups[0].Events[0].Items[1].Value = jobseqno;

                if (odfoverqtime.BOO2VAC)
                    overqtimeDecvlaue += 1;

                if (odfoverqtime.VPO2VAC)
                    overqtimeDecvlaue += 2;

                if (odfoverqtime.LCD2VAC)
                    overqtimeDecvlaue += 4;

                if (odfoverqtime.VAC2SUV)
                    overqtimeDecvlaue += 8;

                if (odfoverqtime.SUV2SMO)
                    overqtimeDecvlaue += 16;

                string overqtime = Convert.ToString(overqtimeDecvlaue, 2).PadLeft(16, '0');
                string qtimeT = new string(overqtime.ToCharArray().Reverse().ToArray());
                outputdata.EventGroups[0].Events[0].Items[2].Value = qtimeT;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + InLineQTimeOverTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InLineQTimeOverTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InLineQTimeOverTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(InLineQTimeOverCommandODFTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET Bit=[{2}]. CASSETTE SEQ NO=[{3}],JOB SEQ NO=[{4}], QTIMETYPE=[{5}].",
                    eqpNo, trackKey, value.ToString(), cstseqno, jobseqno, Convert.ToString(overqtimeDecvlaue, 2)));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        /// <param name="cstseqno"></param>
        /// <param name="jobseqno"></param>
        /// <param name="pioverqtime"></param>
        public void InLineQTimeOverCommand_PI(string eqpNo, eBitResult value, string trackKey, string cstseqno, string jobseqno, eCELLInLineOverQtimePI pioverqtime)
        {
            try
            {
                int overqtimeDecvlaue = 0;
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_InLineQTimeOverCommand") as Trx;

                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].Items[2].Value = new string('0', 16);
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET Bit=[{2}].",
                    eqpNo, trackKey, value.ToString()));

                    return;
                }


                outputdata.EventGroups[0].Events[0].Items[0].Value = cstseqno;
                outputdata.EventGroups[0].Events[0].Items[1].Value = jobseqno;

                #region [T2 InLineQTime]
                //if (pioverqtime.PIP2PPO)
                //    overqtimeDecvlaue += 1;

                //if (pioverqtime.PPO2PMO)
                //    overqtimeDecvlaue += 2;
                #endregion

                #region [T3 InLineQTime to do]
                //T3 add cs.chou 暫時註解
                if (pioverqtime.PIC2PPO)
                    overqtimeDecvlaue += 1;

                if (pioverqtime.PPO2PMO)
                    overqtimeDecvlaue += 2;

                if (pioverqtime.PPO2PPA)
                    overqtimeDecvlaue += 4;

                if (pioverqtime.PMO2PPA)
                    overqtimeDecvlaue += 8;

                if (pioverqtime.PPA2PAO)
                    overqtimeDecvlaue += 16;
                #endregion

                string overqtime = Convert.ToString(overqtimeDecvlaue, 2).PadLeft(16, '0');
                string qtimeT = new string(overqtime.ToCharArray().Reverse().ToArray());
                outputdata.EventGroups[0].Events[0].Items[2].Value = qtimeT;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + InLineQTimeOverTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InLineQTimeOverTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InLineQTimeOverTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(InLineQTimeOverCommandPITimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET Bit=[{2}]. CASSETTE SEQ NO=[{3}],JOB SEQ NO=[{4}], QTIMETYPE=[{5}].",
                    eqpNo, trackKey, value.ToString(), cstseqno, jobseqno, Convert.ToString(overqtimeDecvlaue, 2)));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InLineQTimeOverCommandODFTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INLINE QTIME OVER COMMAND TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                InLineQTimeOverCommand_ODF(sArray[0], eBitResult.OFF, trackKey, string.Empty, string.Empty, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InLineQTimeOverCommandPITimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INLINE QTIME OVER COMMAND TIMEOUT, SET BIT=[OFF]", sArray[0], trackKey));


                InLineQTimeOverCommand_PI(sArray[0], eBitResult.OFF, trackKey, string.Empty, string.Empty, null);


                //Trx trx = PLCAgent.GetTransactionFormat(string.Format("InLineQTimeOverCommand_PI", sArray[0])) as Trx;
                //if (trx != null)
                //{
                //    trx.EventGroups[0].Events[0].IsDisable = true;
                //    trx.EventGroups[0].Events[1].Items[0].Value = "0";
                //    trx.TrackKey = trackKey;
                //    SendPLCData(trx);
                //}

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Cassette Process By Manual Report
        private const string CassetteProcessByManualTimeout = "CassetteProcessByManualTimeout";
        public void CassetteProcessByManualReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    CassetteProcessByManualReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string operatorID = inputData.EventGroups[0].Events[0].Items[0].Value;
                string portNO = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glassCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] OPERATORID=[{2}],PORTNO=[{3}],GLASSCOUNT=[{4}], SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    operatorID.Trim(), portNO, glassCount));
                #endregion
                CassetteProcessByManualReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    CassetteProcessByManualReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void CassetteProcessByManualReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_CassetteProcessByManualReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;

                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + CassetteProcessByManualTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + CassetteProcessByManualTimeout);
                }
                #region[If Bit on]
                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + CassetteProcessByManualTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CassetteProcessByManualReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CassetteProcessByManualReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CASSETTE PROCESS BY MANUAL REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                CassetteProcessByManualReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Inspection Flag Set Report
        private const string InspectionFlagSetTimeout = "InspectionFlagSetTimeout";
        public void InspectionFlagSetReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    InspectionFlagSetReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string inspectionFlag = inputData.EventGroups[0].Events[0].Items[2].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTESEQUENCENO=[{2}],JOBSEQUENCENO=[{3}],INSPECTIONFLAG=[{4}],OPERATORID=[{5}], SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    cstSeqNO, jobSeqNo, inspectionFlag, operatorID.Trim()));

                InspectionFlagSetReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    InspectionFlagSetReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void InspectionFlagSetReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_InspectionFlagSetReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + InspectionFlagSetTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InspectionFlagSetTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InspectionFlagSetTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(InspectionFlagSetReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void InspectionFlagSetReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INSPECTION FLAG SET REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                InspectionFlagSetReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Run Mode Change Report
        private const string RunModeChangeTimeout = "RunModeChangeTimeout";
        public void RunModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    RunModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    RunModeChangeReportReply(inputData.Name, eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string runMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                //有些線有,有些沒有
                string operatorID = null;
                if (inputData.EventGroups[0].Events[0].Items.Count > 1)
                    operatorID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                #endregion

                #region update eq file
                //lock (eqp)
                //{
                //    eqp.File.EquipmentRunMode = runMode;
                //    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                //}

                Line line;
                if ((eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI)) || (eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                {
                    if (inputData.Name.Substring(inputData.Name.Length - 1, 1) == "1")
                    {
                        line = GetLineByLines(keyCELLPMTLINE.CBPMI);
                    }
                    else
                        line = GetLineByLines(keyCELLPMTLINE.CBPTI);
                }
                else
                    line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line != null)
                {
                    lock (line)
                    {
                        line.File.CellLineOperMode = runMode;

                        if (ConstantManager.ContainsKey("CELL_RUNMODE_" + line.Data.LINETYPE))
                        {
                            line.File.LineOperMode = ConstantManager["CELL_RUNMODE_" + line.Data.LINETYPE][runMode].Value.ToString();
                        }
                        ObjectManager.LineManager.EnqueueSave(line.File);
                    }
                }

                ////Watson 20150314 Modify 修改成文字描述，為OPI顯示
                //// For 俊成要求修改，若有問題可找俊成討論
                lock (eqp)
                {
                    eqp.File.EquipmentRunMode = line.File.LineOperMode;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                }

                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                    Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest", new object[] { inputData.TrackKey, line.Data.LINEID });
                else
                    Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID });
                #endregion


                #region OPI Service Send
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion

                //如果有operatorID,會多記錄operatorID
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] {5} RUNMODE=[{2}]({3}) {4}, SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    runMode, line.File.LineOperMode, operatorID == null ? "" : string.Format(",OPERATORID=[{0}]", operatorID), inputData.Name));

                RunModeChangeReportReply(inputData.Name, eqpNo, eBitResult.ON, inputData.TrackKey);

                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    RunModeChangeReportReply(inputData.Name, inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void RunModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string runMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                #region Update EQP File
                Line line;
                if ((eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI)) || (eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                {
                    if (inputData.Name.Substring(inputData.Name.Length - 1, 1) == "1")
                    {
                        line = GetLineByLines(keyCELLPMTLINE.CBPMI);
                    }
                    else
                        line = GetLineByLines(keyCELLPMTLINE.CBPTI);
                }
                else
                    line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                lock (line)
                {
                    line.File.CellLineOperMode = runMode;

                    if (ConstantManager.ContainsKey("CELL_RUNMODE_" + line.Data.LINETYPE))
                    {
                        line.File.LineOperMode = ConstantManager["CELL_RUNMODE_" + line.Data.LINETYPE][runMode].Value.ToString();
                    }
                    ObjectManager.LineManager.EnqueueSave(line.File);
                }

                ////Watson 20150314 Modify 修改成文字描述，為OPI顯示
                //// For 俊成要求修改，若有問題可找俊成討論
                lock (eqp)
                {
                    eqp.File.EquipmentRunMode = line.File.LineOperMode;
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                }

                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] [{5}] EQUIPMENT_RUN_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, line.File.CellLineOperMode, line.File.LineOperMode, inputData.Name));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void RunModeChangeReportReply(string inputDataName, string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata;
                if (inputDataName.Substring(inputDataName.Length - 1, 1) == "1")
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RunModeChangeForLoaderToPMIReportReply") as Trx;
                else if (inputDataName.Substring(inputDataName.Length - 1, 1) == "2")
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RunModeChangeForLoaderToPTIReportReply") as Trx;
                else
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RunModeChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(inputDataName + "_" + RunModeChangeTimeout))
                {
                    _timerManager.TerminateTimer(inputDataName + "_" + RunModeChangeTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(inputDataName + "_" + RunModeChangeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RunModeChangeReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RunModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RUN MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                RunModeChangeReportReply(sArray[1], sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Supply Stop by Pass Report
        private const string SupplyStopbyPassTimeout = "SupplyStopbyPassTimeout";
        public void SupplyStopbyPassReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    SupplyStopbyPassReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string supplyStopPassNode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] SUPPLYSTOPPASSNODE=[{2}],OPERATORID=[{3}], SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    supplyStopPassNode, operatorID));

                SupplyStopbyPassReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    SupplyStopbyPassReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void SupplyStopbyPassReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_SupplyStopbyPassReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + SupplyStopbyPassTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + SupplyStopbyPassTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + SupplyStopbyPassTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(SupplyStopbyPassReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void SupplyStopbyPassReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SUPPLY STOP BY PASS REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                SupplyStopbyPassReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region MMG Job Data Update Report
        private const string MMGJobDataUpdateReportTimeout = "MMGJobDataUpdateReportTimeout";
        public void MMGJobDataUpdateReport(Trx inputData)
        {
            try
            {

                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    MMGJobDataUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string returnCode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string productType = inputData.EventGroups[0].Events[0].Items[1].Value;
                string ppid = inputData.EventGroups[0].Events[0].Items[2].Value;
                string productID = inputData.EventGroups[0].Events[0].Items[3].Value;
                string cassetteSettingCode = inputData.EventGroups[0].Events[0].Items[4].Value;
                string panelSize = inputData.EventGroups[0].Events[0].Items[5].Value;
                string crossLineCassetteSettingCode = inputData.EventGroups[0].Events[0].Items[6].Value;
                string panelSizeFlagandMMGFlag = inputData.EventGroups[0].Events[0].Items[7].Value;

                string crossLinePanelSize = inputData.EventGroups[0].Events[0].Items[8].Value;
                string cUTProductID = inputData.EventGroups[0].Events[0].Items[9].Value;
                string cUTCrossProductID = inputData.EventGroups[0].Events[0].Items[10].Value;
                string cUTProductType = inputData.EventGroups[0].Events[0].Items[11].Value;
                string cUTCrossProductType = inputData.EventGroups[0].Events[0].Items[12].Value;
                string pOLProductType = inputData.EventGroups[0].Events[0].Items[13].Value;
                string pOLProductID = inputData.EventGroups[0].Events[0].Items[14].Value;
                string crossLinePPID = inputData.EventGroups[0].Events[0].Items[15].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,MMGJOBDATAUPDATEREPORT  CROSSLINEJOBITEMREQUESTRETURNCODE=[{4}] " +
                    ",PRODUCTTYPE=[{5}] ,PPID=[{6}] ,PRODUCTID=[{7}] ,CASSETTESETTINGCODE=[{8}] ,PANELSIZE=[{9}] ,CROSSLINECASSETTESETTINGCODE=[{10}]" +
                ",PANELSIZEFLAGANDMMGFLAG=[{11}] ,CROSSLINEPANELSIZE=[{12}] ,CUTPRODUCTID=[{13}] ,CUTCROSSPRODUCTID=[{14}] ,CUTPRODUCTTYPE=[{15}]" +
                ",CUTCROSSPRODUCTTYPE=[{16}] ,POLPRODUCTTYPE=[{17}] ,POLPRODUCTID,CROSSLINEPPID=[{18}].", eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID,
                returnCode, productType, ppid, productID, cassetteSettingCode, panelSize, crossLineCassetteSettingCode,
                            panelSizeFlagandMMGFlag, crossLinePanelSize, cUTProductID, cUTCrossProductID, cUTProductType, cUTCrossProductType, pOLProductType, pOLProductID, crossLinePPID));

                MMGJobDataUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MMGJobDataUpdateReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void MMGJobDataUpdateReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MMGJobDataUpdateReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MMGJobDataUpdateReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MMGJobDataUpdateReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MMGJobDataUpdateReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MMGJobDataUpdateReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MMGJobDataUpdateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MMG JOB DATA UPDATE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                MMGJobDataUpdateReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region MMG Control Mode Report
        private const string MMGControlModeReportTimeout = "MMGControlModeReportTimeout";
        public void MMGControlModeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    MMGControlModeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string mMGControlMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] , MMGCONTROLMODE =[{4}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, mMGControlMode));

                MMGControlModeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MMGControlModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void MMGControlModeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MMGControlModeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MMGControlModeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MMGControlModeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MMGControlModeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MMGControlModeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MMGControlModeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MMG CONTROL MODE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                MMGControlModeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region RTP PAM Material Info Request
        private const string RTPPAMMaterialInfoRequestTimeout = "RTPPAMMaterialInfoRequestTimeout";
        public void RTPPAMMaterialInfoRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    RTPPAMMaterialInfoRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, null);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string pFCD = inputData.EventGroups[0].Events[0].Items[0].Value;
                string cstSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] , PFCD =[{4}],CASSETTESEQUENCENO =[{5}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, pFCD, cstSeqNo));

                RTPPAMMaterialInfoRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, null);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    RTPPAMMaterialInfoRequestReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, null);
            }
        }
        /// <param name="replyitem">CstSeqNo,RTPMaterReturnCode,PFCD,TFTPOL1,TFTPOL2 ,TFTPOL3,CFPOL1,CFPOL2,CFPOL3</param>
        private void RTPPAMMaterialInfoRequestReply(string eqpNo, eBitResult value, string trackKey, IList<string> replyitem)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "RTPPAMMaterialInfoRequestReply") as Trx;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                if (replyitem == null)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                }

                if (replyitem.Count < 9)
                    return;
                outputdata.EventGroups[0].Events[0].Items[0].Value = replyitem[0]; //CassetteSequenceNo
                outputdata.EventGroups[0].Events[0].Items[1].Value = replyitem[1]; //RTPPAMMaterialInfoReturnCode
                outputdata.EventGroups[0].Events[0].Items[2].Value = replyitem[2]; //PFCD
                outputdata.EventGroups[0].Events[0].Items[3].Value = replyitem[3]; //TFTPOL#01
                outputdata.EventGroups[0].Events[0].Items[4].Value = replyitem[4]; //TFTPOL#02
                outputdata.EventGroups[0].Events[0].Items[5].Value = replyitem[5]; //TFTPOL#03
                outputdata.EventGroups[0].Events[0].Items[6].Value = replyitem[6]; //CFPOL#01
                outputdata.EventGroups[0].Events[0].Items[7].Value = replyitem[7]; //CFPOL#02
                outputdata.EventGroups[0].Events[0].Items[8].Value = replyitem[8]; //CFPOL#03

                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + RTPPAMMaterialInfoRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + RTPPAMMaterialInfoRequestTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + RTPPAMMaterialInfoRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RTPPAMMaterialInfoRequestReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}]." +
                    "CASSETTESEQUENCENO =[{3}], RTPPAMMATERIALINFORETURNCODE =[{4}], PFCD =[{5}], TFTPOL#01 =[{6}], TFTPOL#02 =[{7}], TFTPOL#03 =[{8}], CFPOL#01 =[{9}]" +
                    ",CFPOL#02 =[{10}] ,CFPOL#03 =[{11}] ", eqpNo, trackKey, value.ToString(), replyitem[0], replyitem[1], replyitem[2], replyitem[3], replyitem[4], replyitem[5], replyitem[6], replyitem[7], replyitem[8]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RTPPAMMaterialInfoRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RTP PAM MATERIAL INFO REQUEST, SET BIT=[OFF].", sArray[0], trackKey));

                RTPPAMMaterialInfoRequestReply(sArray[0], eBitResult.OFF, trackKey, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region TCV Dispatching Rule Change Report
        private const string TCVDispatchingRuleChangeReportTimeout = "TCVDispatchingRuleChangeReportTimeout";
        public void TCVDispatchingRuleChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    //TO DO 待確認
                    //TCVDispatchingRuleChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    TCVDispatchingRuleChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eCELL_TCVDispatchRule tCVDispatchingRule = (eCELL_TCVDispatchRule)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] , TCVDISPATCHINGRULE =[{4}],OPER_ID =[{5}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, tCVDispatchingRule.ToString(), operatorID));

                object[] _data = new object[4]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,               /*2 machineName */
                    tCVDispatchingRule
                };
                //Send MES Data LineLinkChanged(string trxID,string lineName,string eqpid,eCELL_TCVDispatchRule proresult)
                object retVal = base.Invoke(eServiceName.MESService, "LineLinkChanged", _data);

                TCVDispatchingRuleChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    TCVDispatchingRuleChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void TCVDispatchingRuleChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                eCELL_TCVDispatchRule tCVDispatchingRule = (eCELL_TCVDispatchRule)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                object[] _data = new object[4]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,               /*2 machineName */
                    tCVDispatchingRule
                };
                object retVal = base.Invoke(eServiceName.MESService, "LineLinkChanged", _data);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] TCV_DISPATCHING_RULE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, int.Parse(tCVDispatchingRule.ToString()), tCVDispatchingRule));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void TCVDispatchingRuleChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "TCVDispatchingRuleChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + TCVDispatchingRuleChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + TCVDispatchingRuleChangeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + TCVDispatchingRuleChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(TCVDispatchingRuleChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void TCVDispatchingRuleChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] TCV DISPATCHING RULE CHANGE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                TCVDispatchingRuleChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region TCV Inspection Mode  Report (TCVSamplingModeReport)
        private const string TCVSamplingModeTimeout = "TCVSamplingModeTimeout";
        public void TCVSamplingModeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    TCVSamplingModeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eCELL_TCVSamplingMode tcvsamplingMode = (eCELL_TCVSamplingMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string grade = inputData.EventGroups[0].Events[0].Items[1].Value;
                string waitTime = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] TCV SAMPLING MODE=[{2}]({5}),GRADE=[{3}],WAIT TIME=[{4}], SET Bit=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    (int)tcvsamplingMode, grade, waitTime, tcvsamplingMode.ToString()));

                object[] _data = new object[8]
                    { 
                        inputData.TrackKey,                   /*0  TrackKey*/
                        eqp.Data.LINEID,                      /*1  LineName*/
                        tcvsamplingMode.ToString(),     /*2  INSPMode*/
                        eqp.Data.NODEID,                      /*3  EQP ID*/
                        grade,                         /*4  Pull Mode Grade (CELL 使用)*/
                        waitTime,                         /*5  Wait Time (CELL 使用)*/
                        string.Empty,     /*6  SampleRatio (机台上报的Sampling Unit栏位值)*/
                        string.Empty                          /*7  Reason Code (CF 不使用)*/
                    };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "InspectionModeChanged", _data);


                TCVSamplingModeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    TCVSamplingModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void TCVSamplingModeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string tcvsamplingMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string grade = inputData.EventGroups[0].Events[0].Items[1].Value;
                string waitTime = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                object[] _data = new object[8]
                    { 
                        inputData.TrackKey,                   /*0  TrackKey*/
                        eqp.Data.LINEID,                      /*1  LineName*/
                        tcvsamplingMode.ToString(),     /*2  INSPMode*/
                        eqp.Data.NODEID,                      /*3  EQP ID*/
                        grade,                         /*4  Pull Mode Grade (CELL 使用)*/
                        waitTime,                         /*5  Wait Time (CELL 使用)*/
                        string.Empty,     /*6  SampleRatio (机台上报的Sampling Unit栏位值)*/
                        string.Empty                          /*7  Reason Code (CF 不使用)*/
                    };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "InspectionModeChanged", _data);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] TCV_SAMPLING_MODE=[{3}], GRADE=[{4}], WAIT TIME=[{5}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, tcvsamplingMode, grade, waitTime));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TCVSamplingModeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_TCVSamplingModeReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + TCVSamplingModeTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + TCVSamplingModeTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + TCVSamplingModeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(TCVSamplingModeReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void TCVSamplingModeReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] TCV SAMPLING MODE REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                TCVSamplingModeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ATS Loader Operation Mode Change Report
        private const string ATSLDOperModeChgReplyTimeout = "ATSLDOperModeChgReplyTimeout";
        public void ATSLoaderOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    ATSLoaderOperationModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    ATSLoaderOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eATSLoaderOperMode aTSLoaderOperationMode = (eATSLoaderOperMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] , ATSLOADEROPERATIONMODE =[{4}]({6}),OPERATORID =[{5}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, aTSLoaderOperationMode.ToString(), operatorID, (int)aTSLoaderOperationMode));





                ATSLoaderOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                lock (eqp) eqp.File.ATSLoaderOperMode = aTSLoaderOperationMode;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    ATSLoaderOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void ATSLoaderOperationModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string aTSLoaderOperationMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                #endregion

                lock (eqp) eqp.File.ATSLoaderOperMode = (eATSLoaderOperMode)int.Parse(aTSLoaderOperationMode);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] ATS_LOADER_OPERATION_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, aTSLoaderOperationMode, eqp.File.ATSLoaderOperMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    ATSLoaderOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void ATSLoaderOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ATSLoaderOperationModeChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ATSLDOperModeChgReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ATSLDOperModeChgReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ATSLDOperModeChgReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ATSLoaderOperationModeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ATSLoaderOperationModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ATS LOADER OPERATION MODE CHANGE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                ATSLoaderOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ATS Operation Permission Request Command
        private const string OperationPermissionRequestCommandTimeout = "OperationPermissionRequestCommandTimeout";
        public void OperationPermissionRequestCommand(string eqpNo, eBitResult value, string trackKey, eCELLATSOperPermission operatorPermission, string operID)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "OperationPermissionRequestCommand") as Trx;

                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)operatorPermission).ToString();
                    outputdata.EventGroups[0].Events[0].Items[1].Value = operID;
                }
                //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + OperationPermissionRequestCommandTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + OperationPermissionRequestCommandTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + OperationPermissionRequestCommandTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(OperationPermissionRequestCommandReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,OPERATOR_PERMISSION=[{2}]({3}), OPERATOR_ID=[{4}], SET BIT=[{2}].",
                    eqpNo, trackKey, operatorPermission, (int)operatorPermission, operID, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void OperationPermissionRequestCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,RETURN CODE =[{4}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, returnCode));


                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + OperationPermissionRequestCommandTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + OperationPermissionRequestCommandTimeout);
                }

                //To Do Call UI:
                Invoke(eServiceName.UIService, "OperationPermissionResultReport",
                        new object[] { eqp.Data.LINEID, eqp.Data.NODENO, returnCode.ToString() });

                OperationPermissionRequestCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eCELLATSOperPermission.UNKNOW, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    OperationPermissionRequestCommand(inputData.Name.Split('_')[0], eBitResult.OFF, inputData.TrackKey, eCELLATSOperPermission.UNKNOW, string.Empty);
            }
        }
        private void OperationPermissionRequestCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] OPERATION PERMISSION REQUEST COMMAND TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                OperationPermissionRequestCommand(sArray[0], eBitResult.OFF, trackKey, eCELLATSOperPermission.UNKNOW, string.Empty);

                //To Do Call UI: NG (TIME OUT)
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "OperationPermissionResultReport",
                            new object[] { eqp.Data.LINEID, eqp.Data.NODENO, eReturnCode1.NG.ToString() });
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ATSLoaderOperationModeChangeCommand
        private const string ATSLoaderOperationModeChangeTimeout = "ATSLoaderOperationModeChangeTimeout";
        public void ATSLoaderOperationModeChangeCommand(string eqpNo, eBitResult value, string trackKey, eCELLATSLDOperMode operationMode, string operID)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ATSLoaderOperationModeChangeCommand") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)operationMode).ToString();
                outputdata.EventGroups[0].Events[0].Items[1].Value = operID;
                //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ATSLoaderOperationModeChangeTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ATSLoaderOperationModeChangeTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ATSLoaderOperationModeChangeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ATSLoaderOperationModeChangeCommandTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,OPERATIONMODE=[{2}]({3}), OPERATORID=[{4}] ,SET BIT=[{5}].",
                    eqpNo, trackKey, operationMode, (int)operationMode, value.ToString(), operID, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ATSLoaderOperationModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + ATSLoaderOperationModeChangeTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + ATSLoaderOperationModeChangeTimeout);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,RETURN CODE =[{4}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, returnCode));

                //To Do Call UI:
                //OperationRunModeChangeResultReport(string lineName, string eqpNo, string cmdType, string rtnCode)
                //cmdType: RUNMODE / LOADEROPERATIONMODE
                Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                    new object[] { eqp.Data.LINEID, eqp.Data.NODENO, eOPIATSCmdType.LOADEROPERATIONMODE, returnCode.ToString() });

                ATSLoaderOperationModeChangeCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eCELLATSLDOperMode.UNKNOW, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    ATSLoaderOperationModeChangeCommand(inputData.Name.Split('_')[0], eBitResult.OFF, inputData.TrackKey, eCELLATSLDOperMode.UNKNOW, string.Empty);
            }
        }
        private void ATSLoaderOperationModeChangeCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ATS LOADER OPERATION MODE CHANGE COMMAND TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                ATSLoaderOperationModeChangeCommand(sArray[0], eBitResult.OFF, trackKey, eCELLATSLDOperMode.UNKNOW, string.Empty);

                //To Do Call UI: NG (TIME OUT)
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                            new object[] { eqp.Data.LINEID, eqp.Data.NODENO, eOPIATSCmdType.LOADEROPERATIONMODE, eReturnCode1.NG.ToString() });
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ATS RunModeChangeCommand
        private const string RunModeChangeCommandTimeout = "RunModeChangeCommandTimeout";
        public void RunModeChangeCommand(string eqpNo, eBitResult value, string trackKey, eCELLATSRunMode runMode, string operID)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "RunModeChangeCommand") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)runMode).ToString();
                outputdata.EventGroups[0].Events[0].Items[1].Value = operID;
                //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + RunModeChangeCommandTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + RunModeChangeCommandTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + RunModeChangeCommandTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RunModeChangeCommandReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,RUN_MODE=[{2}]({3}),OPERATOR_ID=[{4}] SET BIT=[{5}].",
                    eqpNo, trackKey, runMode, (int)runMode, operID, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void RunModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,RETURN CODE =[{4}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, returnCode));

                if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + RunModeChangeCommandTimeout))
                {
                    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + RunModeChangeCommandTimeout);
                }
                //To Do Call UI:
                //OperationRunModeChangeResultReport(string lineName, string eqpNo, string cmdType, string rtnCode)
                //cmdType: RUNMODE / LOADEROPERATIONMODE
                Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                    new object[] { eqp.Data.LINEID, eqp.Data.NODENO, eOPIATSCmdType.RUNMODE, returnCode.ToString() });

                RunModeChangeCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eCELLATSRunMode.UNKNOW, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    RunModeChangeCommand(inputData.Name.Split('_')[0], eBitResult.OFF, inputData.TrackKey, eCELLATSRunMode.UNKNOW, string.Empty);
            }
        }
        private void RunModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RUN MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                RunModeChangeCommand(sArray[0], eBitResult.OFF, trackKey, eCELLATSRunMode.UNKNOW, string.Empty);

                //To Do Call UI: NG (TIME OUT)
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    //To Do Call UI:
                    Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                        new object[] { eqp.Data.LINEID, sArray[0], eOPIATSCmdType.RUNMODE, eReturnCode1.NG.ToString() });
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Material Authority Request Report 換輪刀
        private const string MaterialAuthorityReqTimeout = "MaterialAuthorityReqTimeout";
        public void MaterialAuthorityRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    MaterialAuthorityRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, null);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string headNo = inputData.EventGroups[0].Events[0].Items[0].Value; //HeadNo
                string cuttingWheelID = inputData.EventGroups[0].Events[0].Items[1].Value; //CuttingWheelID
                string recipeID = inputData.EventGroups[0].Events[0].Items[2].Value; //RecipeID
                string cuttingWheelSettingUsage = inputData.EventGroups[0].Events[0].Items[3].Value; //CuttingWheelSettingUsage
                string operatorID = inputData.EventGroups[0].Events[0].Items[4].Value; //OperatorID
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] , HEADNO =[{4}],CUTTINGWHEELID =[{5}],RECIPEID =[{6}],CUTTINGWHEELSETTINGUSAGE =[{7}],OPERATORID =[{8}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, headNo, cuttingWheelID, recipeID, cuttingWheelSettingUsage, operatorID));

                #region MES Report
                object[] _data = new object[4]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    new List<string> { cuttingWheelID}
                };
                Invoke(eServiceName.MESService, "ValidateMaskRequest", _data);
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MaterialAuthorityRequestReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, ((int)eReturnCode1.NG).ToString());
            }
        }

        public void MaterialAuthorityRequestReply(string eqpNo, eBitResult value, string trackKey, string rtnCode)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MaterialAuthorityRequestReply") as Trx;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                if (rtnCode == null)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialAuthorityReqTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + MaterialAuthorityReqTimeout);
                    }
                    return;
                }


                outputdata.EventGroups[0].Events[0].Items[0].Value = rtnCode; //CuttingWheelAuthorityRequestReturnCode

                outputdata.TrackKey = trackKey;
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialAuthorityReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MaterialAuthorityReqTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MaterialAuthorityReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialAuthorityRequestReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}]." +
                    "CUTTINGWHEELAUTHORITYREQUESTRETURNCODE =[{3}] ", eqpNo, trackKey, value.ToString(), rtnCode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MaterialAuthorityRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MATERIAL AUTHORITY REQUEST REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                MaterialAuthorityRequestReply(sArray[0], eBitResult.OFF, trackKey, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region CELL Unload Dispatch Rule[SORT,PRM,PMT]
        #region UnloadDispatchingRule[SORT,PRM]
        private const string UnloadDispatchingRuleTimeout = "UnloadDispatchingRuleTimeout";
        public void UnloadDispatchingRuleReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    UnloadDispatchingRuleReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("Can't find Line ID =[{0}] in LineEntity!", eqp.Data.LINEID));

                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[inputData.EventGroups[0].Events.Count - 1].Items[0].Value);
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    UnloadDispatchingRuleReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);

                    return;
                }
                #endregion

                string eachlog = string.Empty; //各拆解規則不同，log記錄也不同
                #region [拆出PLCAgent Data]  Word and Bit
                //Jun Modify 20141209 使用Switch Case來判斷
                switch (line.Data.LINETYPE)
                {
                    #region [T2 Rule ]
                    case eLineType.CELL.CBCUT_1:
                    case eLineType.CELL.CBCUT_2:
                    case eLineType.CELL.CBCUT_3:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[3].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[4].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[5].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,ABNORMAL_CODE =[{3}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{4}]({6}),OPERATOR_ID =[{5}] ", grade1, grade2, grade3, ab, (int)abflag, opid, abflag);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) continue;

                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBDPK:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBDPS:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBGAP:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[1].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[2].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[3].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,ABNORMAL_CODE =[{1}] ,ABNORMAL_FLAG_CHECK_RULE =[{2}],OPERATOR_ID =[{3}]",
                                grade1, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBLOI:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab2 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string ab3 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string ab4 = inputData.EventGroups[0].Events[e].Items[7].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[8].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[9].Value;

                            string sortlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,  ABNORMAL_VALUE#01 =[{1}] ,GRADE#02 =[{2}],ABNORMAL_VALUE#02 =[{3}]," +
                                "GRADE#03 =[{4}],ABNORMAL_VALUE#03 =[{5}],GRADE#04 =[{6}],ABNORMAL_VALUE#04 =[{7}],ABNORMAL_FLAG_CHECK_RULE =[{8}],OPERATOR_ID =[{9}]",
                                        grade1, ab1, grade2, ab2, grade3, ab3, grade4, ab4, abflag, opid);
                            eachlog += sortlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) continue;

                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBNRP:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[3].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[4].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[5].Value;

                            string CUTlog = string.Format("\r\nPort#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,ABNORMAL_CODE =[{3}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{4}],OPERATOR_ID =[{5}]", grade1, grade2, grade3, ab, abflag, opid);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;
                    case eLineType.CELL.CBOLS:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string opid = inputData.EventGroups[0].Events[e].Items[1].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,OPERATOR_ID =[{1}]",
                                grade1, opid);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPRM:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion

                            #region Save Robort Dic by PRM Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = grade1.Trim();
                            disrules.Grade2 = grade2.Trim();
                            disrules.Grade3 = grade3.Trim();
                            disrules.Grade4 = grade4.Trim();
                            disrules.AbnormalCode1 = ab.Trim();
                            disrules.AbnormalCode2 = ab.Trim();
                            disrules.AbnormalCode3 = ab.Trim();
                            disrules.AbnormalCode4 = ab.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            if (line.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                line.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                            line.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPOL_1:
                    case eLineType.CELL.CBPOL_2:
                    case eLineType.CELL.CBPOL_3:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string group1CassetteGrade = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string group1GlassGrade1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string group1GlassGrade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string group1GlassGrade3 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string group1GlassGrade4 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string group1GlassGrade5 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string group1GlassGrade6 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string group2CassetteGrade = inputData.EventGroups[0].Events[e].Items[7].Value;
                            string group2GlassGrade1 = inputData.EventGroups[0].Events[e].Items[8].Value;
                            string group2GlassGrade2 = inputData.EventGroups[0].Events[e].Items[9].Value;
                            string group2GlassGrade3 = inputData.EventGroups[0].Events[e].Items[10].Value;
                            string group2GlassGrade4 = inputData.EventGroups[0].Events[e].Items[11].Value;
                            string group2GlassGrade5 = inputData.EventGroups[0].Events[e].Items[12].Value;
                            string group2GlassGrade6 = inputData.EventGroups[0].Events[e].Items[13].Value;
                            string group3CassetteGrade = inputData.EventGroups[0].Events[e].Items[14].Value;
                            string group3GlassGrade1 = inputData.EventGroups[0].Events[e].Items[15].Value;
                            string group3GlassGrade2 = inputData.EventGroups[0].Events[e].Items[16].Value;
                            string group3GlassGrade3 = inputData.EventGroups[0].Events[e].Items[17].Value;
                            string group3GlassGrade4 = inputData.EventGroups[0].Events[e].Items[18].Value;
                            string group3GlassGrade5 = inputData.EventGroups[0].Events[e].Items[19].Value;
                            string group3GlassGrade6 = inputData.EventGroups[0].Events[e].Items[20].Value;
                            string group4CassetteGrade = inputData.EventGroups[0].Events[e].Items[21].Value;
                            string group4GlassGrade1 = inputData.EventGroups[0].Events[e].Items[22].Value;
                            string group4GlassGrade2 = inputData.EventGroups[0].Events[e].Items[23].Value;
                            string group4GlassGrade3 = inputData.EventGroups[0].Events[e].Items[24].Value;
                            string group4GlassGrade4 = inputData.EventGroups[0].Events[e].Items[25].Value;
                            string group4GlassGrade5 = inputData.EventGroups[0].Events[e].Items[26].Value;
                            string group4GlassGrade6 = inputData.EventGroups[0].Events[e].Items[27].Value;
                            string group5CassetteGrade = inputData.EventGroups[0].Events[e].Items[28].Value;
                            string group5GlassGrade1 = inputData.EventGroups[0].Events[e].Items[29].Value;
                            string group5GlassGrade2 = inputData.EventGroups[0].Events[e].Items[30].Value;
                            string group5GlassGrade3 = inputData.EventGroups[0].Events[e].Items[31].Value;
                            string group5GlassGrade4 = inputData.EventGroups[0].Events[e].Items[32].Value;
                            string group5GlassGrade5 = inputData.EventGroups[0].Events[e].Items[33].Value;
                            string group5GlassGrade6 = inputData.EventGroups[0].Events[e].Items[34].Value;
                            string group6CassetteGrade = inputData.EventGroups[0].Events[e].Items[35].Value;
                            string group6GlassGrade1 = inputData.EventGroups[0].Events[e].Items[36].Value;
                            string group6GlassGrade2 = inputData.EventGroups[0].Events[e].Items[37].Value;
                            string group6GlassGrade3 = inputData.EventGroups[0].Events[e].Items[38].Value;
                            string group6GlassGrade4 = inputData.EventGroups[0].Events[e].Items[39].Value;
                            string group6GlassGrade5 = inputData.EventGroups[0].Events[e].Items[40].Value;
                            string group6GlassGrade6 = inputData.EventGroups[0].Events[e].Items[41].Value;
                            string group7CassetteGrade = inputData.EventGroups[0].Events[e].Items[42].Value;
                            string group7GlassGrade1 = inputData.EventGroups[0].Events[e].Items[43].Value;
                            string group7GlassGrade2 = inputData.EventGroups[0].Events[e].Items[44].Value;
                            string group7GlassGrade3 = inputData.EventGroups[0].Events[e].Items[45].Value;
                            string group7GlassGrade4 = inputData.EventGroups[0].Events[e].Items[46].Value;
                            string group7GlassGrade5 = inputData.EventGroups[0].Events[e].Items[47].Value;
                            string group7GlassGrade6 = inputData.EventGroups[0].Events[e].Items[48].Value;
                            //不爽記 太多
                            //string prmlog = string.Format("Port#" + (e + 1).ToString().PadLeft(2, '0') + " Grade#01 =[{0}],Grade#02 =[{1}],Grade#03 =[{2}] ,Grade#04 =[{3}],AbnormalCode =[{4}]," +
                            //    "AbnormalFlagCheckRule =[{5}],OperatorID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            //eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(group1GlassGrade1.Trim()) && group1GlassGrade1.Trim() != "") chkGrade.Add(group1GlassGrade1);
                            //if (!chkGrade.Contains(group1GlassGrade2.Trim()) && group1GlassGrade2.Trim() != "") chkGrade.Add(group1GlassGrade2);
                            //if (!chkGrade.Contains(group1GlassGrade3.Trim()) && group1GlassGrade3.Trim() != "") chkGrade.Add(group1GlassGrade3);
                            //if (!chkGrade.Contains(group1GlassGrade4.Trim()) && group1GlassGrade4.Trim() != "") chkGrade.Add(group1GlassGrade4);
                            //if (!chkGrade.Contains(group1GlassGrade5.Trim()) && group1GlassGrade5.Trim() != "") chkGrade.Add(group1GlassGrade5);
                            //if (!chkGrade.Contains(group1GlassGrade6.Trim()) && group1GlassGrade6.Trim() != "") chkGrade.Add(group1GlassGrade6);
                            //if (!chkGrade.Contains(group2GlassGrade1.Trim()) && group2GlassGrade1.Trim() != "") chkGrade.Add(group2GlassGrade1);
                            //if (!chkGrade.Contains(group2GlassGrade2.Trim()) && group2GlassGrade2.Trim() != "") chkGrade.Add(group2GlassGrade2);
                            //if (!chkGrade.Contains(group2GlassGrade3.Trim()) && group2GlassGrade3.Trim() != "") chkGrade.Add(group2GlassGrade3);
                            //if (!chkGrade.Contains(group2GlassGrade4.Trim()) && group2GlassGrade4.Trim() != "") chkGrade.Add(group2GlassGrade4);
                            //if (!chkGrade.Contains(group2GlassGrade5.Trim()) && group2GlassGrade5.Trim() != "") chkGrade.Add(group2GlassGrade5);
                            //if (!chkGrade.Contains(group2GlassGrade6.Trim()) && group2GlassGrade6.Trim() != "") chkGrade.Add(group2GlassGrade6);
                            //if (!chkGrade.Contains(group3GlassGrade1.Trim()) && group3GlassGrade1.Trim() != "") chkGrade.Add(group3GlassGrade1);
                            //if (!chkGrade.Contains(group3GlassGrade2.Trim()) && group3GlassGrade2.Trim() != "") chkGrade.Add(group3GlassGrade2);
                            //if (!chkGrade.Contains(group3GlassGrade3.Trim()) && group3GlassGrade3.Trim() != "") chkGrade.Add(group3GlassGrade3);
                            //if (!chkGrade.Contains(group3GlassGrade4.Trim()) && group3GlassGrade4.Trim() != "") chkGrade.Add(group3GlassGrade4);
                            //if (!chkGrade.Contains(group3GlassGrade5.Trim()) && group3GlassGrade5.Trim() != "") chkGrade.Add(group3GlassGrade5);
                            //if (!chkGrade.Contains(group3GlassGrade6.Trim()) && group3GlassGrade6.Trim() != "") chkGrade.Add(group3GlassGrade6);
                            //if (!chkGrade.Contains(group4GlassGrade1.Trim()) && group4GlassGrade1.Trim() != "") chkGrade.Add(group4GlassGrade1);
                            //if (!chkGrade.Contains(group4GlassGrade2.Trim()) && group4GlassGrade2.Trim() != "") chkGrade.Add(group4GlassGrade2);
                            //if (!chkGrade.Contains(group4GlassGrade3.Trim()) && group4GlassGrade3.Trim() != "") chkGrade.Add(group4GlassGrade3);
                            //if (!chkGrade.Contains(group4GlassGrade4.Trim()) && group4GlassGrade4.Trim() != "") chkGrade.Add(group4GlassGrade4);
                            //if (!chkGrade.Contains(group4GlassGrade5.Trim()) && group4GlassGrade5.Trim() != "") chkGrade.Add(group4GlassGrade5);
                            //if (!chkGrade.Contains(group4GlassGrade6.Trim()) && group4GlassGrade6.Trim() != "") chkGrade.Add(group4GlassGrade6);
                            //if (!chkGrade.Contains(group5GlassGrade1.Trim()) && group5GlassGrade1.Trim() != "") chkGrade.Add(group5GlassGrade1);
                            //if (!chkGrade.Contains(group5GlassGrade2.Trim()) && group5GlassGrade2.Trim() != "") chkGrade.Add(group5GlassGrade2);
                            //if (!chkGrade.Contains(group5GlassGrade3.Trim()) && group5GlassGrade3.Trim() != "") chkGrade.Add(group5GlassGrade3);
                            //if (!chkGrade.Contains(group5GlassGrade4.Trim()) && group5GlassGrade4.Trim() != "") chkGrade.Add(group5GlassGrade4);
                            //if (!chkGrade.Contains(group5GlassGrade5.Trim()) && group5GlassGrade5.Trim() != "") chkGrade.Add(group5GlassGrade5);
                            //if (!chkGrade.Contains(group5GlassGrade6.Trim()) && group5GlassGrade6.Trim() != "") chkGrade.Add(group5GlassGrade6);
                            //if (!chkGrade.Contains(group6GlassGrade1.Trim()) && group6GlassGrade1.Trim() != "") chkGrade.Add(group6GlassGrade1);
                            //if (!chkGrade.Contains(group6GlassGrade2.Trim()) && group6GlassGrade2.Trim() != "") chkGrade.Add(group6GlassGrade2);
                            //if (!chkGrade.Contains(group6GlassGrade3.Trim()) && group6GlassGrade3.Trim() != "") chkGrade.Add(group6GlassGrade3);
                            //if (!chkGrade.Contains(group6GlassGrade4.Trim()) && group6GlassGrade4.Trim() != "") chkGrade.Add(group6GlassGrade4);
                            //if (!chkGrade.Contains(group6GlassGrade5.Trim()) && group6GlassGrade5.Trim() != "") chkGrade.Add(group6GlassGrade5);
                            //if (!chkGrade.Contains(group6GlassGrade6.Trim()) && group6GlassGrade6.Trim() != "") chkGrade.Add(group6GlassGrade6);
                            //if (!chkGrade.Contains(group7GlassGrade1.Trim()) && group7GlassGrade1.Trim() != "") chkGrade.Add(group7GlassGrade1);
                            //if (!chkGrade.Contains(group7GlassGrade2.Trim()) && group7GlassGrade2.Trim() != "") chkGrade.Add(group7GlassGrade2);
                            //if (!chkGrade.Contains(group7GlassGrade3.Trim()) && group7GlassGrade3.Trim() != "") chkGrade.Add(group7GlassGrade3);
                            //if (!chkGrade.Contains(group7GlassGrade4.Trim()) && group7GlassGrade4.Trim() != "") chkGrade.Add(group7GlassGrade4);
                            //if (!chkGrade.Contains(group7GlassGrade5.Trim()) && group7GlassGrade5.Trim() != "") chkGrade.Add(group7GlassGrade5);
                            //if (!chkGrade.Contains(group7GlassGrade6.Trim()) && group7GlassGrade6.Trim() != "") chkGrade.Add(group7GlassGrade6);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBSOR_1:
                    case eLineType.CELL.CBSOR_2:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab2 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string ab3 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string ab4 = inputData.EventGroups[0].Events[e].Items[7].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[8].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[9].Value;

                            string sortlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,  ABNORMAL_VALUE#01 =[{1}] ,GRADE#02 =[{2}],ABNORMAL_VALUE#02 =[{3}]," +
                                "GRADE#03 =[{4}],ABNORMAL_VALUE#03 =[{5}],GRADE#04 =[{6}],ABNORMAL_VALUE#04 =[{7}],ABNORMAL_FLAG_CHECK_RULE =[{8}],OPERATOR_ID =[{9}]",
                                        grade1, ab1, grade2, ab2, grade3, ab3, grade4, ab4, abflag, opid);
                            eachlog += sortlog;
                            #endregion

                            #region Save Robort Dic by SOR Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = grade1.Trim();
                            disrules.Grade2 = grade2.Trim();
                            disrules.Grade3 = grade3.Trim();
                            disrules.Grade4 = grade4.Trim();
                            disrules.AbnormalCode1 = ab1.Trim();
                            disrules.AbnormalCode2 = ab2.Trim();
                            disrules.AbnormalCode3 = ab3.Trim();
                            disrules.AbnormalCode4 = ab4.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            if (line.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                line.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                            line.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPMT:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string ab = inputData.EventGroups[0].Events[e].Items[0].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[1].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[2].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " ABNORMAL_CODE =[{0}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{1}],OPERATOR_ID =[{2}]", ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            #region Save Robort Dic by PRM Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = string.Empty;
                            disrules.Grade2 = string.Empty;
                            disrules.Grade3 = string.Empty;
                            disrules.Grade4 = string.Empty;
                            disrules.AbnormalCode1 = ab.Trim();
                            disrules.AbnormalCode2 = ab.Trim();
                            disrules.AbnormalCode3 = ab.Trim();
                            disrules.AbnormalCode4 = ab.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            //可能是不同的line
                            //可能是不同的line
                            foreach (Line ll in ObjectManager.LineManager.GetLines())
                            {
                                if (ll.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                    ll.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                                ll.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            }
                            #endregion
                        }
                        break;
                    #endregion
                    //T3 cs.chou
                    #region [T3 拆解規則相同，不分Line，故寫在Default]
                    default:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string opid = inputData.EventGroups[0].Events[e].Items[3].Value;

                            string CUTlog = string.Format("\r\nPort#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ," +
                                "OPERATOR_ID =[{3}]", grade1, grade2, grade3, opid);
                            eachlog += CUTlog;
                            #endregion

                            #region [Check Port Setting Grade]
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, inputData.EventGroups[0].Events[e].Items[0].Name.Split('#')[1].Substring(0, 2).PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));


                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = ConstantManager["MES_PORTUSETYPE"][((int)ePortMode.EMPMode).ToString()].Value; //EMP
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? ConstantManager["MES_PORTUSETYPE"][((int)ePortMode.MIX).ToString()].Value : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = ConstantManager["MES_PORTUSETYPE"][((int)ePortMode.MIX).ToString()].Value;

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion

                            #region Save Robort Dic
                            //Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            if (grade1.Trim() == string.Empty && grade2.Trim() == string.Empty && grade3.Trim() == string.Empty)//機台上報 都為空時 為EM mode sy add 20160302
                                grade1 = "EM";
                            disrules.Grade1 = grade1.Trim();
                            disrules.Grade2 = grade2.Trim();
                            disrules.Grade3 = grade3.Trim();
                            disrules.Grade4 = string.Empty;
                            disrules.AbnormalCode1 = string.Empty;
                            disrules.AbnormalCode2 = string.Empty;
                            disrules.AbnormalCode3 = string.Empty;
                            disrules.AbnormalCode4 = string.Empty;
                            disrules.AbnormalFlag = "0";
                            disrules.OperatorID = opid.Trim();

                            if (line.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                line.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                            line.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            #endregion
                        }
                        break;
                    #endregion
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,UNLOADDISPATCHINGRULEREPORT " + eachlog,
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));

                ObjectManager.LineManager.EnqueueSave(line.File);
                UnloadDispatchingRuleReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //20150407 add for Update OPI Unload Dispatch Rule
                #region [ SOR,PMT,PRM Must Update OPI Info ]

                switch (line.Data.LINETYPE)
                {
                    #region [T2 USE]
                    case eLineType.CELL.CBSOR_1:
                    case eLineType.CELL.CBSOR_2:
                    case eLineType.CELL.CBPMT:
                    case eLineType.CELL.CBPRM:
                    #endregion
                    #region [T3 USE]
                    case eLineType.CELL.CCPDR:
                    case eLineType.CELL.CCTAM:
                    case eLineType.CELL.CCPTH:
                    case eLineType.CELL.CCGAP:
                    case eLineType.CELL.CCRWT:
                    case eLineType.CELL.CCSOR:
                    case eLineType.CELL.CCCHN:
                    case eLineType.CELL.CCCRP:
                    case eLineType.CELL.CCCRP_2:
                    #endregion

                        //20150407通知OPI更新
                        Invoke(eServiceName.UIService, "RobotUnloaderDispatchRuleReport", new object[] { inputData.TrackKey });

                        break;
                    default:
                        break;
                }

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    UnloadDispatchingRuleReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void UnloadDispatchingRuleReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[inputData.EventGroups[0].Events.Count - 1].Items[0].Value);
                #endregion

                string eachlog = string.Empty; //各拆解規則不同，log記錄也不同
                #region [拆出PLCAgent Data]  Word and Bit T2 Rule
                //Jun Modify 20141209 使用Switch Case來判斷
                switch (line.Data.LINETYPE)
                {
                    #region [T2 拆解rule]
                    case eLineType.CELL.CBCUT_1:
                    case eLineType.CELL.CBCUT_2:
                    case eLineType.CELL.CBCUT_3:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[3].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[4].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[5].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,ABNORMAL_CODE =[{3}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{4}],OPERATOR_ID =[{5}]", grade1, grade2, grade3, ab, abflag, opid);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) continue;

                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBDPK:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBDPS:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBGAP:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[1].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[2].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[3].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,ABNORMAL_CODE =[{1}] ,ABNORMAL_FLAG_CHECK_RULE =[{2}],OPERATOR_ID =[{3}]",
                                grade1, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBLOI:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab2 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string ab3 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string ab4 = inputData.EventGroups[0].Events[e].Items[7].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[8].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[9].Value;

                            string sortlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,  ABNORMAL_VALUE#01 =[{1}] ,GRADE#02 =[{2}],ABNORMAL_VALUE#02 =[{3}]," +
                                "GRADE#03 =[{4}],ABNORMAL_VALUE#03 =[{5}],GRADE#04 =[{6}],ABNORMAL_VALUE#04 =[{7}],ABNORMAL_FLAG_CHECK_RULE =[{8}],OPERATOR_ID =[{9}]",
                                        grade1, ab1, grade2, ab2, grade3, ab3, grade4, ab4, abflag, opid);
                            eachlog += sortlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) continue;

                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBNRP:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[3].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[4].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[5].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,ABNORMAL_CODE =[{3}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{4}],OPERATOR_ID =[{5}]", grade1, grade2, grade3, ab, abflag, opid);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;
                    case eLineType.CELL.CBOLS:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string opid = inputData.EventGroups[0].Events[e].Items[1].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,OPERATOR_ID =[{1}]",
                                grade1, opid);
                            eachlog += CUTlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPRM:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string ab = inputData.EventGroups[0].Events[e].Items[4].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[5].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[6].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ,GRADE#04 =[{3}],ABNORMAL_CODE =[{4}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{5}],OPERATOR_ID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            //if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            //if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            //if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion

                            #region Save Robort Dic by PRM Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = grade1.Trim();
                            disrules.Grade2 = grade2.Trim();
                            disrules.Grade3 = grade3.Trim();
                            disrules.Grade4 = grade4.Trim();
                            disrules.AbnormalCode1 = ab.Trim();
                            disrules.AbnormalCode2 = ab.Trim();
                            disrules.AbnormalCode3 = ab.Trim();
                            disrules.AbnormalCode4 = ab.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            if (line.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                line.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                            line.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPOL_1:
                    case eLineType.CELL.CBPOL_2:
                    case eLineType.CELL.CBPOL_3:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string group1CassetteGrade = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string group1GlassGrade1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string group1GlassGrade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string group1GlassGrade3 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string group1GlassGrade4 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string group1GlassGrade5 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string group1GlassGrade6 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string group2CassetteGrade = inputData.EventGroups[0].Events[e].Items[7].Value;
                            string group2GlassGrade1 = inputData.EventGroups[0].Events[e].Items[8].Value;
                            string group2GlassGrade2 = inputData.EventGroups[0].Events[e].Items[9].Value;
                            string group2GlassGrade3 = inputData.EventGroups[0].Events[e].Items[10].Value;
                            string group2GlassGrade4 = inputData.EventGroups[0].Events[e].Items[11].Value;
                            string group2GlassGrade5 = inputData.EventGroups[0].Events[e].Items[12].Value;
                            string group2GlassGrade6 = inputData.EventGroups[0].Events[e].Items[13].Value;
                            string group3CassetteGrade = inputData.EventGroups[0].Events[e].Items[14].Value;
                            string group3GlassGrade1 = inputData.EventGroups[0].Events[e].Items[15].Value;
                            string group3GlassGrade2 = inputData.EventGroups[0].Events[e].Items[16].Value;
                            string group3GlassGrade3 = inputData.EventGroups[0].Events[e].Items[17].Value;
                            string group3GlassGrade4 = inputData.EventGroups[0].Events[e].Items[18].Value;
                            string group3GlassGrade5 = inputData.EventGroups[0].Events[e].Items[19].Value;
                            string group3GlassGrade6 = inputData.EventGroups[0].Events[e].Items[20].Value;
                            string group4CassetteGrade = inputData.EventGroups[0].Events[e].Items[21].Value;
                            string group4GlassGrade1 = inputData.EventGroups[0].Events[e].Items[22].Value;
                            string group4GlassGrade2 = inputData.EventGroups[0].Events[e].Items[23].Value;
                            string group4GlassGrade3 = inputData.EventGroups[0].Events[e].Items[24].Value;
                            string group4GlassGrade4 = inputData.EventGroups[0].Events[e].Items[25].Value;
                            string group4GlassGrade5 = inputData.EventGroups[0].Events[e].Items[26].Value;
                            string group4GlassGrade6 = inputData.EventGroups[0].Events[e].Items[27].Value;
                            string group5CassetteGrade = inputData.EventGroups[0].Events[e].Items[28].Value;
                            string group5GlassGrade1 = inputData.EventGroups[0].Events[e].Items[29].Value;
                            string group5GlassGrade2 = inputData.EventGroups[0].Events[e].Items[30].Value;
                            string group5GlassGrade3 = inputData.EventGroups[0].Events[e].Items[31].Value;
                            string group5GlassGrade4 = inputData.EventGroups[0].Events[e].Items[32].Value;
                            string group5GlassGrade5 = inputData.EventGroups[0].Events[e].Items[33].Value;
                            string group5GlassGrade6 = inputData.EventGroups[0].Events[e].Items[34].Value;
                            string group6CassetteGrade = inputData.EventGroups[0].Events[e].Items[35].Value;
                            string group6GlassGrade1 = inputData.EventGroups[0].Events[e].Items[36].Value;
                            string group6GlassGrade2 = inputData.EventGroups[0].Events[e].Items[37].Value;
                            string group6GlassGrade3 = inputData.EventGroups[0].Events[e].Items[38].Value;
                            string group6GlassGrade4 = inputData.EventGroups[0].Events[e].Items[39].Value;
                            string group6GlassGrade5 = inputData.EventGroups[0].Events[e].Items[40].Value;
                            string group6GlassGrade6 = inputData.EventGroups[0].Events[e].Items[41].Value;
                            string group7CassetteGrade = inputData.EventGroups[0].Events[e].Items[42].Value;
                            string group7GlassGrade1 = inputData.EventGroups[0].Events[e].Items[43].Value;
                            string group7GlassGrade2 = inputData.EventGroups[0].Events[e].Items[44].Value;
                            string group7GlassGrade3 = inputData.EventGroups[0].Events[e].Items[45].Value;
                            string group7GlassGrade4 = inputData.EventGroups[0].Events[e].Items[46].Value;
                            string group7GlassGrade5 = inputData.EventGroups[0].Events[e].Items[47].Value;
                            string group7GlassGrade6 = inputData.EventGroups[0].Events[e].Items[48].Value;

                            //string prmlog = string.Format("Port#" + (e + 1).ToString().PadLeft(2, '0') + " Grade#01 =[{0}],Grade#02 =[{1}],Grade#03 =[{2}] ,Grade#04 =[{3}],AbnormalCode =[{4}]," +
                            //    "AbnormalFlagCheckRule =[{5}],OperatorID =[{6}]", grade1, grade2, grade3, grade4, ab, abflag, opid);
                            //eachlog += prmlog;
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            //Port port = ObjectManager.PortManager.GetPort((e + 1).ToString().PadLeft(2, '0'));
                            //if (port == null) continue;

                            //List<string> chkGrade = new List<string>();
                            //if (!chkGrade.Contains(group1GlassGrade1.Trim()) && group1GlassGrade1.Trim() != "") chkGrade.Add(group1GlassGrade1);
                            //if (!chkGrade.Contains(group1GlassGrade2.Trim()) && group1GlassGrade2.Trim() != "") chkGrade.Add(group1GlassGrade2);
                            //if (!chkGrade.Contains(group1GlassGrade3.Trim()) && group1GlassGrade3.Trim() != "") chkGrade.Add(group1GlassGrade3);
                            //if (!chkGrade.Contains(group1GlassGrade4.Trim()) && group1GlassGrade4.Trim() != "") chkGrade.Add(group1GlassGrade4);
                            //if (!chkGrade.Contains(group1GlassGrade5.Trim()) && group1GlassGrade5.Trim() != "") chkGrade.Add(group1GlassGrade5);
                            //if (!chkGrade.Contains(group1GlassGrade6.Trim()) && group1GlassGrade6.Trim() != "") chkGrade.Add(group1GlassGrade6);
                            //if (!chkGrade.Contains(group2GlassGrade1.Trim()) && group2GlassGrade1.Trim() != "") chkGrade.Add(group2GlassGrade1);
                            //if (!chkGrade.Contains(group2GlassGrade2.Trim()) && group2GlassGrade2.Trim() != "") chkGrade.Add(group2GlassGrade2);
                            //if (!chkGrade.Contains(group2GlassGrade3.Trim()) && group2GlassGrade3.Trim() != "") chkGrade.Add(group2GlassGrade3);
                            //if (!chkGrade.Contains(group2GlassGrade4.Trim()) && group2GlassGrade4.Trim() != "") chkGrade.Add(group2GlassGrade4);
                            //if (!chkGrade.Contains(group2GlassGrade5.Trim()) && group2GlassGrade5.Trim() != "") chkGrade.Add(group2GlassGrade5);
                            //if (!chkGrade.Contains(group2GlassGrade6.Trim()) && group2GlassGrade6.Trim() != "") chkGrade.Add(group2GlassGrade6);
                            //if (!chkGrade.Contains(group3GlassGrade1.Trim()) && group3GlassGrade1.Trim() != "") chkGrade.Add(group3GlassGrade1);
                            //if (!chkGrade.Contains(group3GlassGrade2.Trim()) && group3GlassGrade2.Trim() != "") chkGrade.Add(group3GlassGrade2);
                            //if (!chkGrade.Contains(group3GlassGrade3.Trim()) && group3GlassGrade3.Trim() != "") chkGrade.Add(group3GlassGrade3);
                            //if (!chkGrade.Contains(group3GlassGrade4.Trim()) && group3GlassGrade4.Trim() != "") chkGrade.Add(group3GlassGrade4);
                            //if (!chkGrade.Contains(group3GlassGrade5.Trim()) && group3GlassGrade5.Trim() != "") chkGrade.Add(group3GlassGrade5);
                            //if (!chkGrade.Contains(group3GlassGrade6.Trim()) && group3GlassGrade6.Trim() != "") chkGrade.Add(group3GlassGrade6);
                            //if (!chkGrade.Contains(group4GlassGrade1.Trim()) && group4GlassGrade1.Trim() != "") chkGrade.Add(group4GlassGrade1);
                            //if (!chkGrade.Contains(group4GlassGrade2.Trim()) && group4GlassGrade2.Trim() != "") chkGrade.Add(group4GlassGrade2);
                            //if (!chkGrade.Contains(group4GlassGrade3.Trim()) && group4GlassGrade3.Trim() != "") chkGrade.Add(group4GlassGrade3);
                            //if (!chkGrade.Contains(group4GlassGrade4.Trim()) && group4GlassGrade4.Trim() != "") chkGrade.Add(group4GlassGrade4);
                            //if (!chkGrade.Contains(group4GlassGrade5.Trim()) && group4GlassGrade5.Trim() != "") chkGrade.Add(group4GlassGrade5);
                            //if (!chkGrade.Contains(group4GlassGrade6.Trim()) && group4GlassGrade6.Trim() != "") chkGrade.Add(group4GlassGrade6);
                            //if (!chkGrade.Contains(group5GlassGrade1.Trim()) && group5GlassGrade1.Trim() != "") chkGrade.Add(group5GlassGrade1);
                            //if (!chkGrade.Contains(group5GlassGrade2.Trim()) && group5GlassGrade2.Trim() != "") chkGrade.Add(group5GlassGrade2);
                            //if (!chkGrade.Contains(group5GlassGrade3.Trim()) && group5GlassGrade3.Trim() != "") chkGrade.Add(group5GlassGrade3);
                            //if (!chkGrade.Contains(group5GlassGrade4.Trim()) && group5GlassGrade4.Trim() != "") chkGrade.Add(group5GlassGrade4);
                            //if (!chkGrade.Contains(group5GlassGrade5.Trim()) && group5GlassGrade5.Trim() != "") chkGrade.Add(group5GlassGrade5);
                            //if (!chkGrade.Contains(group5GlassGrade6.Trim()) && group5GlassGrade6.Trim() != "") chkGrade.Add(group5GlassGrade6);
                            //if (!chkGrade.Contains(group6GlassGrade1.Trim()) && group6GlassGrade1.Trim() != "") chkGrade.Add(group6GlassGrade1);
                            //if (!chkGrade.Contains(group6GlassGrade2.Trim()) && group6GlassGrade2.Trim() != "") chkGrade.Add(group6GlassGrade2);
                            //if (!chkGrade.Contains(group6GlassGrade3.Trim()) && group6GlassGrade3.Trim() != "") chkGrade.Add(group6GlassGrade3);
                            //if (!chkGrade.Contains(group6GlassGrade4.Trim()) && group6GlassGrade4.Trim() != "") chkGrade.Add(group6GlassGrade4);
                            //if (!chkGrade.Contains(group6GlassGrade5.Trim()) && group6GlassGrade5.Trim() != "") chkGrade.Add(group6GlassGrade5);
                            //if (!chkGrade.Contains(group6GlassGrade6.Trim()) && group6GlassGrade6.Trim() != "") chkGrade.Add(group6GlassGrade6);
                            //if (!chkGrade.Contains(group7GlassGrade1.Trim()) && group7GlassGrade1.Trim() != "") chkGrade.Add(group7GlassGrade1);
                            //if (!chkGrade.Contains(group7GlassGrade2.Trim()) && group7GlassGrade2.Trim() != "") chkGrade.Add(group7GlassGrade2);
                            //if (!chkGrade.Contains(group7GlassGrade3.Trim()) && group7GlassGrade3.Trim() != "") chkGrade.Add(group7GlassGrade3);
                            //if (!chkGrade.Contains(group7GlassGrade4.Trim()) && group7GlassGrade4.Trim() != "") chkGrade.Add(group7GlassGrade4);
                            //if (!chkGrade.Contains(group7GlassGrade5.Trim()) && group7GlassGrade5.Trim() != "") chkGrade.Add(group7GlassGrade5);
                            //if (!chkGrade.Contains(group7GlassGrade6.Trim()) && group7GlassGrade6.Trim() != "") chkGrade.Add(group7GlassGrade6);

                            //string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            //if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            //if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0];
                            //if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            //if (port.File.UseGrade != oldUseGrade)
                            //    Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port }];
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBSOR_1:
                    case eLineType.CELL.CBSOR_2:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string ab1 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string ab2 = inputData.EventGroups[0].Events[e].Items[3].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[4].Value;
                            string ab3 = inputData.EventGroups[0].Events[e].Items[5].Value;
                            string grade4 = inputData.EventGroups[0].Events[e].Items[6].Value;
                            string ab4 = inputData.EventGroups[0].Events[e].Items[7].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[8].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[9].Value;

                            string sortlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}] ,  ABNORMAL_VALUE#01 =[{1}] ,GRADE#02 =[{2}],ABNORMAL_VALUE#02 =[{3}]," +
                                "GRADE#03 =[{4}],ABNORMAL_VALUE#03 =[{5}],GRADE#04 =[{6}],ABNORMAL_VALUE#04 =[{7}],ABNORMAL_FLAG_CHECK_RULE =[{8}],OPERATOR_ID =[{9}]",
                                        grade1, ab1, grade2, ab2, grade3, ab3, grade4, ab4, abflag, opid);
                            eachlog += sortlog;
                            #endregion

                            #region Save Robort Dic by SOR Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = grade1.Trim();
                            disrules.Grade2 = grade2.Trim();
                            disrules.Grade3 = grade3.Trim();
                            disrules.Grade4 = grade4.Trim();
                            disrules.AbnormalCode1 = ab1.Trim();
                            disrules.AbnormalCode2 = ab2.Trim();
                            disrules.AbnormalCode3 = ab3.Trim();
                            disrules.AbnormalCode4 = ab4.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            if (line.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                line.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                            line.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            #endregion

                            //CUT LOI SOR的Unloader是以Grade來上報Port Use Type，所以當Port Mode改變時，不能上報TFT CF這些Port Mode
                            #region [Check Port Setting Grade]
                            List<string> chkGrade = new List<string>();
                            if (!chkGrade.Contains(grade1.Trim()) && grade1.Trim() != "") chkGrade.Add(grade1);
                            if (!chkGrade.Contains(grade2.Trim()) && grade2.Trim() != "") chkGrade.Add(grade2);
                            if (!chkGrade.Contains(grade3.Trim()) && grade3.Trim() != "") chkGrade.Add(grade3);
                            if (!chkGrade.Contains(grade4.Trim()) && grade4.Trim() != "") chkGrade.Add(grade4);

                            string portMode = string.Empty; string oldUseGrade = port.File.UseGrade;
                            if (chkGrade.Count < 1) port.File.UseGrade = "EMP";
                            if (chkGrade.Count == 1) port.File.UseGrade = chkGrade[0] == "MX" ? "MIX" : chkGrade[0];
                            if (chkGrade.Count > 1) port.File.UseGrade = "MIX";

                            if (port.File.UseGrade != oldUseGrade)
                                Invoke(eServiceName.MESService, "PortUseTypeChanged", new object[] { inputData.TrackKey, port.Data.LINEID, port });
                            #endregion
                        }
                        break;

                    case eLineType.CELL.CBPMT:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                        {
                            #region [PLC Word]
                            string ab = inputData.EventGroups[0].Events[e].Items[0].Value;
                            eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[1].Value);
                            string opid = inputData.EventGroups[0].Events[e].Items[2].Value;

                            string prmlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " ABNORMAL_CODE =[{0}]," +
                                "ABNORMAL_FLAG_CHECK_RULE =[{1}],OPERATOR_ID =[{2}]", ab, abflag, opid);
                            eachlog += prmlog;
                            #endregion

                            #region Save Robort Dic by PRM Rule
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                            if (port == null) throw new Exception(string.Format("Can't find Port (Node {0}, Port No {1}] in PortEntity!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                            clsDispatchRule disrules = new clsDispatchRule();
                            disrules.Grade1 = string.Empty;
                            disrules.Grade2 = string.Empty;
                            disrules.Grade3 = string.Empty;
                            disrules.Grade4 = string.Empty;
                            disrules.AbnormalCode1 = ab.Trim();
                            disrules.AbnormalCode2 = ab.Trim();
                            disrules.AbnormalCode3 = ab.Trim();
                            disrules.AbnormalCode4 = ab.Trim();
                            disrules.AbnormalFlag = ((int)abflag).ToString();
                            disrules.OperatorID = opid.Trim();

                            //可能是不同的line
                            foreach (Line ll in ObjectManager.LineManager.GetLines())
                            {
                                if (ll.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                                    ll.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                                ll.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                            }
                            #endregion
                        }
                        break;
                    #endregion

                    //T3 cs.chou Add
                    #region [T3 不分Type 各規則相同 寫在default中]
                    default:
                        for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)
                        {
                            #region [PLC Word]
                            string grade1 = inputData.EventGroups[0].Events[e].Items[0].Value;
                            string grade2 = inputData.EventGroups[0].Events[e].Items[1].Value;
                            string grade3 = inputData.EventGroups[0].Events[e].Items[2].Value;
                            string opid = inputData.EventGroups[0].Events[e].Items[3].Value;

                            string CUTlog = string.Format("\r\nPORT#" + (e + 1).ToString().PadLeft(2, '0') + " GRADE#01 =[{0}],GRADE#02 =[{1}],GRADE#03 =[{2}] ," +
                                "OPERATOR_ID =[{3}]", grade1, grade2, grade3, opid);
                            eachlog += CUTlog;
                            #endregion
                        }
                        break;
                    #endregion
                }
                #endregion

                ObjectManager.LineManager.EnqueueSave(line.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] UNLOAD_DISPATCHING_RULE={3}.",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, eachlog));

                //20150407 add for Update OPI Unload Dispatch Rule
                #region [ SOR,PMT,PRM Must Update OPI Info ]

                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CBSOR_1:
                    case eLineType.CELL.CBSOR_2:
                    case eLineType.CELL.CBPMT:
                    case eLineType.CELL.CBPRM:

                        //20150407通知OPI更新
                        Invoke(eServiceName.UIService, "RobotUnloaderDispatchRuleReport", new object[] { inputData.TrackKey });

                        break;
                    default:
                        break;
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UnloadDispatchingRuleReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UnloadDispatchingRuleReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + UnloadDispatchingRuleTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + UnloadDispatchingRuleTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + UnloadDispatchingRuleTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UnloadDispatchingRuleReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}]."
                    , eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UnloadDispatchingRuleReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] UNLOAD DISPATCHING RULE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                UnloadDispatchingRuleReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region PortAbnormalCodeCheckRule[PMT]
        public const string CELL_UDDispatchMIXGrade = "MX";
        private const string PortAbnormalCodeCheckRuleReplyTimeout = "PortAbnormalCodeCheckRuleReplyTimeout";
        public void PortAbnormalCodeCheckRuleChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    PortAbnormalCodeCheckRuleChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE ID =[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                string eachlog = string.Empty; //各拆解規則不同，log記錄也不同

                #region [拆出PLCAgent Data]  Word and Bit
                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[6].Items[0].Value);
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    PortAbnormalCodeCheckRuleChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                {
                    #region [PLC Word]
                    string ab = inputData.EventGroups[0].Events[e].Items[0].Value;
                    eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[1].Value);
                    string opid = inputData.EventGroups[0].Events[e].Items[2].Value;

                    string sortlog = string.Format("Port#" + (e + 1).ToString().PadLeft(2, '0') + " AbnormalValue =[{0}] ,AbnormalFlagCheckRule =[{1}],OperatorID =[{2}]",
                                  ab, abflag, opid);
                    eachlog += sortlog;
                    #endregion

                    #region Save Robort Dic by SORT Rule
                    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                    if (port == null) throw new Exception(string.Format("CAN'T FIND PORT NODE=[{0}], PORT NO=[{1}] IN PORTENTITY!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                    clsDispatchRule disrules = new clsDispatchRule();
                    disrules.Grade1 = CELL_UDDispatchMIXGrade;
                    disrules.Grade2 = CELL_UDDispatchMIXGrade;
                    disrules.Grade3 = CELL_UDDispatchMIXGrade;
                    disrules.Grade4 = CELL_UDDispatchMIXGrade;
                    disrules.AbnormalCode1 = ab.Trim();
                    disrules.AbnormalCode2 = ab.Trim();   //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalCode3 = ab.Trim();  //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalCode4 = ab.Trim();  //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalFlag = ((int)abflag).ToString();
                    disrules.OperatorID = opid.Trim();

                    //可能是不同的line
                    foreach (Line ll in ObjectManager.LineManager.GetLines())
                    {
                        if (ll.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                            ll.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                        ll.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                    }
                    #endregion
                }

                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] ,PORTABNORMALCODECHECKRULECHANGEREPORT " + eachlog,
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));

                if (inputData.IsInitTrigger) return;
                ObjectManager.LineManager.EnqueueSave(line.File);
                PortAbnormalCodeCheckRuleChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //20150407 add for Update OPI Unload Dispatch Rule
                #region [ SOR,PMT,PRM Must Update OPI Info ]

                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CBSOR_1:
                    case eLineType.CELL.CBSOR_2:
                    case eLineType.CELL.CBPMT:
                    case eLineType.CELL.CBPRM:

                        //20150407通知OPI更新
                        Invoke(eServiceName.UIService, "RobotUnloaderDispatchRuleReport", new object[] { inputData.TrackKey });

                        break;
                    default:
                        break;
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PortAbnormalCodeCheckRuleChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PortAbnormalCodeCheckRuleChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                string eachlog = string.Empty; //各拆解規則不同，log記錄也不同

                #region [拆出PLCAgent Data]  Word and Bit
                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[6].Items[0].Value);
                #endregion

                for (int e = 0; e < inputData.EventGroups[0].Events.Count - 1; e++)  //Bit in Last Event
                {
                    #region [PLC Word]
                    string ab = inputData.EventGroups[0].Events[e].Items[0].Value;
                    eCELL_AbnorFlagCheckRule abflag = (eCELL_AbnorFlagCheckRule)int.Parse(inputData.EventGroups[0].Events[e].Items[1].Value);
                    string opid = inputData.EventGroups[0].Events[e].Items[2].Value;

                    string sortlog = string.Format("Port#" + (e + 1).ToString().PadLeft(2, '0') + " AbnormalValue =[{0}] ,AbnormalFlagCheckRule =[{1}],OperatorID =[{2}]",
                                  ab, abflag, opid);
                    eachlog += sortlog;
                    #endregion

                    #region Save Robort Dic by SORT Rule
                    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0'));
                    if (port == null) throw new Exception(string.Format("CAN'T FIND PORT NODE=[{0}], PORT_NO=[{1}] IN PORTENTITY!", eqp.Data.NODEID, (e + 1).ToString().PadLeft(2, '0')));

                    clsDispatchRule disrules = new clsDispatchRule();
                    disrules.Grade1 = CELL_UDDispatchMIXGrade;
                    disrules.Grade2 = CELL_UDDispatchMIXGrade;
                    disrules.Grade3 = CELL_UDDispatchMIXGrade;
                    disrules.Grade4 = CELL_UDDispatchMIXGrade;
                    disrules.AbnormalCode1 = ab.Trim();
                    disrules.AbnormalCode2 = ab.Trim();   //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalCode3 = ab.Trim();  //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalCode4 = ab.Trim();  //string.Empty; Watson modify 20141226 For GOGI Robot
                    disrules.AbnormalFlag = ((int)abflag).ToString();
                    disrules.OperatorID = opid.Trim();

                    //可能是不同的line
                    foreach (Line ll in ObjectManager.LineManager.GetLines())
                    {
                        if (ll.File.UnlaoderDispatchRule.ContainsKey(port.Data.PORTID))
                            ll.File.UnlaoderDispatchRule.Remove(port.Data.PORTID);
                        ll.File.UnlaoderDispatchRule.Add(port.Data.PORTID, disrules);
                    }
                    #endregion
                }
                #endregion

                ObjectManager.LineManager.EnqueueSave(line.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT_ABNORMALCODE_CHECK_RULE=[{3}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, eachlog));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PortAbnormalCodeCheckRuleChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PortAbnormalCodeCheckRuleChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PortAbnormalCodeCheckRuleReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PortAbnormalCodeCheckRuleReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PortAbnormalCodeCheckRuleReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortAbnormalCodeCheckRuleChangeReportReplyTimeout), trackKey);
                } if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PortAbnormalCodeCheckRuleReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortAbnormalCodeCheckRuleChangeReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}]."
                    , eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PortAbnormalCodeCheckRuleChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] PORT ABNORMAL CODE CHECK RULE CHANGE REPORT REPLY, SET BIT=[OFF].", sArray[0], trackKey));

                PortAbnormalCodeCheckRuleChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region PMT Line Port Assignment Information Report
        private enum eCELLPMTPortAssInfo
        {
            Unknow = 0,
            PMT = 1,
            PTI = 2,
        }
        private const string PortAssignmentInformationTimeout = "PortAssignmentInformationTimeout";
        public void PortAssignmentInformationReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    PortAssignmentInformationReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    PortAssignmentInformationReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                eCELLPMTPortAssInfo port1 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eCELLPMTPortAssInfo port2 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                eCELLPMTPortAssInfo port3 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                eCELLPMTPortAssInfo port4 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                eCELLPMTPortAssInfo port5 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                eCELLPMTPortAssInfo port6 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value);
                string operatorID = inputData.EventGroups[0].Events[0].Items[6].Value; //OperatorID
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], NODE=[{3}] , PORT#01 =[{4}],PORT#02 =[{5}],PORT#03 =[{6}],PORT#04 =[{7}],PORT#05 =[{8}],PORT#06 =[{9}], OPERATIONID =[{10}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, port1, port2, port3, port4, port5, port6, operatorID));

                PortAssignmentInformationReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);


                #region Save To DB Dictionary
                Dictionary<string, eCELLPMTPortAssInfo> portAssbyLine = new Dictionary<string, eCELLPMTPortAssInfo>();
                portAssbyLine.Add("1", port1);
                portAssbyLine.Add("2", port2);
                portAssbyLine.Add("3", port3);
                portAssbyLine.Add("4", port4);
                portAssbyLine.Add("5", port5);
                portAssbyLine.Add("6", port6);
                Update_PortObj_DB_ByPMTLine(inputData.TrackKey, eqp.Data.NODEID, portAssbyLine, true);
                #endregion


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PortAssignmentInformationReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PortAssignmentInformationReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                eCELLPMTPortAssInfo port1 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eCELLPMTPortAssInfo port2 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                eCELLPMTPortAssInfo port3 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                eCELLPMTPortAssInfo port4 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                eCELLPMTPortAssInfo port5 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                eCELLPMTPortAssInfo port6 = (eCELLPMTPortAssInfo)int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value);
                string operatorID = inputData.EventGroups[0].Events[0].Items[6].Value; //OperatorID
                #endregion

                #region Save To DB Dictionary
                Dictionary<string, eCELLPMTPortAssInfo> portAssbyLine = new Dictionary<string, eCELLPMTPortAssInfo>();
                portAssbyLine.Add("1", port1);
                portAssbyLine.Add("2", port2);
                portAssbyLine.Add("3", port3);
                portAssbyLine.Add("4", port4);
                portAssbyLine.Add("5", port5);
                portAssbyLine.Add("6", port6);
                Update_PortObj_DB_ByPMTLine(inputData.TrackKey, eqp.Data.NODEID, portAssbyLine, false);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT_ASSIGNMENT_INFO PORT#01=[{3}], PORT#02=[{4}], PORT#03=[{5}], PORT#04=[{6}], PORT#05=[{7}], PORT#06 =[{8}]",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port1, port2, port3, port4, port5, port6));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PortAssignmentInformationReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PortAssignmentInformationReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PortAssignmentInformationTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PortAssignmentInformationTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PortAssignmentInformationTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortAssignmentInformationReportTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,SET BIT=[{2}].", eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PortAssignmentInformationReportTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] PORT ASSIGNMENT INFORMATION REPORT, SET BIT=[OFF].", sArray[0], trackKey));

                PortAssignmentInformationReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void Update_PortObj_DB_ByPMTLine(string trxID, string eqpID, Dictionary<string, eCELLPMTPortAssInfo> portAssbyLine, bool reportFlag)
        {
            Dictionary<Port, ePortEnableMode> dicPMIPort = new Dictionary<Port, ePortEnableMode>();
            Dictionary<Port, ePortEnableMode> dicPTIPort = new Dictionary<Port, ePortEnableMode>();
            string PMILineID = GetLineByLines(keyCELLPMTLINE.CBPMI).Data.LINEID;
            string PTILineID = GetLineByLines(keyCELLPMTLINE.CBPTI).Data.LINEID;

            foreach (string portno in portAssbyLine.Keys)
            {
                eCELLPMTPortAssInfo lineid = portAssbyLine[portno];
                Port port = ObjectManager.PortManager.GetPort(eqpID, portno.PadLeft(2, '0'));
                if (lineid == eCELLPMTPortAssInfo.PMT)
                {
                    if (port.Data.LINEID != PMILineID)
                    {
                        dicPTIPort.Add(port, ePortEnableMode.Disabled);
                        port.File.EnableMode = ePortEnableMode.Enabled;
                        port.Data.LINEID = PMILineID;
                        dicPMIPort.Add(port, ePortEnableMode.Enabled);
                        ObjectManager.PortManager.UpdateDB(port.Data);
                    }
                }

                if (lineid == eCELLPMTPortAssInfo.PTI)
                {
                    if (port.Data.LINEID != PTILineID)
                    {
                        dicPMIPort.Add(port, ePortEnableMode.Disabled);

                        port.File.EnableMode = ePortEnableMode.Enabled;
                        port.Data.LINEID = PTILineID;

                        dicPTIPort.Add(port, ePortEnableMode.Enabled);
                        ObjectManager.PortManager.UpdateDB(port.Data);
                    }
                }
            }

            if (reportFlag)
            {
                if (dicPMIPort.Count > 0)
                    base.Invoke(eServiceName.MESService, "PortEnableChangedByPMT", new object[3] { trxID, PMILineID, dicPMIPort });

                if (dicPTIPort.Count > 0)
                    base.Invoke(eServiceName.MESService, "PortEnableChangedByPMT", new object[3] { trxID, PTILineID, dicPTIPort });
            }

        }

        #endregion

        #region Inspection Abnormal Warning Report(MachineInspectionOverRatio)
        private const string InspectionAbnormalWarningTimeout = "InspectionAbnormalWarningTimeout";
        public void InspectionAbnormalWarningReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    InspectionAbnormalWarningReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string currentSamplingRule = inputData.EventGroups[0].Events[0].Items[0].Value;
                string lightWarningGlassCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                string heightWarningGlassCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                string byPassGlassCount = inputData.EventGroups[0].Events[0].Items[3].Value;
                string level = inputData.EventGroups[0].Events[0].Items[4].Value;
                string _overResult = (level == "1" ? "W" : level == "2" ? "A" : level == "3" ? "N" : string.Empty);

                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CURRENTSAMPLINGRULE=[{2}], LIGHTWARNINGGLASSCOUNT=[{3}],"
                    + "HEIGHTWARNINGGLASSCOUNT=[{4}],BYPASSGLASSCOUNT=[{5}],LEVEL=[{6}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    currentSamplingRule, lightWarningGlassCount, heightWarningGlassCount, byPassGlassCount, level));

                object[] _mesData = new object[4]
                    { 
                        inputData.TrackKey,      /*0  TrackKey*/
                        eqp.Data.LINEID,         /*1  LineName*/
                        eqp.Data.NODEID,         /*2  MachineName*/
                        _overResult,             /*3  OverResult*/
                    };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "MachineInspectionOverRatio", _mesData);

                InspectionAbnormalWarningReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    InspectionAbnormalWarningReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void InspectionAbnormalWarningReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_InspectionAbnormalWarningReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + InspectionAbnormalWarningTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InspectionAbnormalWarningTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InspectionAbnormalWarningTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(InspectionAbnormalWarningReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void InspectionAbnormalWarningReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INSPECTION ABNORMAL WARNING REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                InspectionAbnormalWarningReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Inspection Warning Count Setting Report
        private const string InspectionWarningCountSettingTimeout = "InspectionWarningCountSettingTimeout";
        public void InspectionWarningCountSettingReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    InspectionWarningCountSettingReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string currentSamplingRule = inputData.EventGroups[0].Events[0].Items[0].Value;
                string lightWarningGlassCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                string hightWarningGlassCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CURRENTSAMPLINGRULE=[{2}], LIGHTWARNINGGLASSCOUNT=[{3}],"
                    + "HIGHTWARNINGGLASSCOUNT=[{4}],OPERATORID=[{5}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    currentSamplingRule, lightWarningGlassCount, hightWarningGlassCount, operatorID));

                InspectionWarningCountSettingReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    InspectionWarningCountSettingReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void InspectionWarningCountSettingReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_InspectionWarningCountSettingReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + InspectionWarningCountSettingTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + InspectionWarningCountSettingTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + InspectionWarningCountSettingTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(InspectionWarningCountSettingReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void InspectionWarningCountSettingReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INSPECTION WARNINGC OUNT SETTING REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                InspectionWarningCountSettingReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Cross Line Job Item Request
        private const string CrossLineJobItemTimeout = "CrossLineJobItemTimeout";
        public void CrossLineJobItemRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    CrossLineJobItemRequestReply(eqpNo, eBitResult.OFF, inputData.TrackKey, null);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string cstSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string JobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion
                Job job = ObjectManager.JobManager.GetJob(cstSeqNo, JobSeqNo);

                if (job == null) throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!!", cstSeqNo, JobSeqNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}] SET BIT=[ON]",
                    eqp.Data.NODENO, inputData.TrackKey, cstSeqNo, JobSeqNo));

                CrossLineJobItemRequestReply(eqpNo, eBitResult.ON, inputData.TrackKey, job);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                CrossLineJobItemRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, null);
            }
        }
        public void CrossLineJobItemRequestReply(string eqpNo, eBitResult value, string trackKey, Job job)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "CrossLineJobItemRequestReply") as Trx;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                string logtemp = string.Empty;
                if (value == eBitResult.ON)
                {
                    if (job != null)
                    {
                        outputdata.EventGroups[0].Events[0].Items[0].Value = job.CellSpecial.CrossLineCassetteSettingCode; //CrossLineJobItemRequestReturnCode
                        outputdata.EventGroups[0].Events[0].Items[1].Value = job.ProductType.Value.ToString();//ProductType 
                        outputdata.EventGroups[0].Events[0].Items[2].Value = job.PPID;//PPID
                        outputdata.EventGroups[0].Events[0].Items[3].Value = job.CellSpecial.ProductID;//ProductID
                        outputdata.EventGroups[0].Events[0].Items[4].Value = job.CellSpecial.CassetteSettingCode;//CassetteSettingCode
                        outputdata.EventGroups[0].Events[0].Items[5].Value = job.CellSpecial.PanelSize;//PanelSize
                        outputdata.EventGroups[0].Events[0].Items[6].Value = job.CellSpecial.CrossLineCassetteSettingCode;//CrossLineCassetteSettingCode
                        outputdata.EventGroups[0].Events[0].Items[7].Value = job.CellSpecial.PanelSizeFlag;//PanelSizeFlagandMMGFlag
                        outputdata.EventGroups[0].Events[0].Items[8].Value = job.CellSpecial.CrossLinePanelSize;//CrossLinePanelSize
                        outputdata.EventGroups[0].Events[0].Items[9].Value = job.CellSpecial.CUTProductID;//CUTProductID
                        outputdata.EventGroups[0].Events[0].Items[10].Value = job.CellSpecial.CUTCrossProductID;//CUTCrossProductID
                        outputdata.EventGroups[0].Events[0].Items[11].Value = job.CellSpecial.CUTProductType;//CUTProductType
                        outputdata.EventGroups[0].Events[0].Items[12].Value = job.CellSpecial.CUTCrossProductType;//CUTCrossProductType
                        outputdata.EventGroups[0].Events[0].Items[13].Value = job.CellSpecial.POLProductType;//POLProductType
                        outputdata.EventGroups[0].Events[0].Items[14].Value = job.CellSpecial.POLProductID;//POLProductID
                        outputdata.EventGroups[0].Events[0].Items[15].Value = job.CellSpecial.CrossLinePPID;//CrossLinePPID
                        //Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    }
                    else
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                }
                else
                    outputdata.EventGroups[0].Events[0].IsDisable = true;

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + CrossLineJobItemTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + CrossLineJobItemTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + CrossLineJobItemTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CrossLineJobItemRequestReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CrossLineJobItemRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CROSS LINE JOB ITEM REQUEST REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                CrossLineJobItemRequestReply(sArray[0], eBitResult.OFF, trackKey, null);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA MaskVCREventReport
        private const string MaskVCREventTimeout = "MaskVCREventTimeout";
        public void MaskVCREventReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    MaskVCREventReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string readingMaskID = inputData.EventGroups[0].Events[0].Items[0].Value;
                string maskID = inputData.EventGroups[0].Events[0].Items[1].Value;
                string result = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] READINGMASKID=[{2}], MASKID=[{3}],"
                    + "RESULT=[{4}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    readingMaskID, maskID, result));

                MaskVCREventReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MaskVCREventReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void MaskVCREventReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MaskVCREventReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MaskVCREventTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MaskVCREventTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MaskVCREventTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskVCREventReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MaskVCREventReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MASK VCR EVENT REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                MaskVCREventReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA MaskActionReport
        private const string MaskActionReplyTimeout = "MaskActionReplyTimeout";
        public void MaskActionReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    MaskActionReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string action = inputData.EventGroups[0].Events[0].Items[0].Value;
                string maskID = inputData.EventGroups[0].Events[0].Items[1].Value;
                string type = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] ACTION=[{2}], MASKID=[{3}],"
                    + "TYPE=[{4}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    action, maskID, type));

                MaskActionReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MaskActionReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void MaskActionReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MaskActionReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MaskActionReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MaskActionReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MaskActionReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskActionReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MaskActionReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MASK ACTION REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                MaskActionReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA MaskInformationSettingReport
        private const string MaskInformationSettingReplyTimeout = "MaskInformationSettingReplyTimeout";
        public void MaskInformationSettingReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    MaskInformationSettingReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string wrningUseCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string maskUseCountRpeortInterval = inputData.EventGroups[0].Events[0].Items[1].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] WARNINGUSECOUNT=[{2}], MASKUSECOUNTRPEORTINTERVAL=[{3}],"
                    + "OPERATORID=[{4}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    wrningUseCount, maskUseCountRpeortInterval, operatorID));

                MaskInformationSettingReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    MaskInformationSettingReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void MaskInformationSettingReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "MaskInformationSettingReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + MaskInformationSettingReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + MaskInformationSettingReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + MaskInformationSettingReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskInformationSettingReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MaskInformationSettingReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] MASK INFORMATION SETTING REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                MaskInformationSettingReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA LoaderDispatchModeReport
        private const string LoaderDispatchModeReplyTimeout = "LoaderDispatchModeReplyTimeout";
        public void LoaderDispatchModeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    LoaderDispatchModeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string mode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] MODE=[{2}], OPERATORID=[{3}],SET BIT=[ON]",
                    eqp.Data.NODENO, inputData.TrackKey, mode, operatorID));

                LoaderDispatchModeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    LoaderDispatchModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void LoaderDispatchModeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "LoaderDispatchModeReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + LoaderDispatchModeReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LoaderDispatchModeReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + LoaderDispatchModeReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LoaderDispatchModeReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LoaderDispatchModeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] LOADER DISPATCH MODE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                LoaderDispatchModeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA LoaderDispatchRuleReport
        private const string LoaderDispatchRuleReplyTimeout = "LoaderDispatchRuleReplyTimeout";
        public void LoaderDispatchRuleReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    LoaderDispatchRuleReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string processJobType = inputData.EventGroups[0].Events[0].Items[0].Value;
                string processCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] PROCESSJOBTYPE=[{2}], PROCESSCOUNT=[{3}],OPERATORID=[{4}] SET BIT=[ON]",
                    eqp.Data.NODENO, inputData.TrackKey, processJobType, processCount, operatorID));

                LoaderDispatchRuleReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    LoaderDispatchRuleReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void LoaderDispatchRuleReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "LoaderDispatchRuleReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + LoaderDispatchRuleReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LoaderDispatchRuleReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + LoaderDispatchRuleReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LoaderDispatchRuleReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LoaderDispatchRuleReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] LOADER DISPATCH RULE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                LoaderDispatchRuleReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region UVA LoaderDispatchRuleMonitor 是整個word更新就會上報，只是Monitor
        public void LoaderDispatchRuleMonitor(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data]  Word
                string processJobType = inputData.EventGroups[0].Events[0].Items[0].Value;
                string productType = inputData.EventGroups[0].Events[0].Items[1].Value;
                string totalCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                string remainCount = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] PROCESSJOBTYPE=[{2}], PRODUCTTYPE=[{3}],TOTALCOUNT=[{4}],REMAINCOUNT=[{5}] SET BIT=[ON]",
                    eqp.Data.NODENO, inputData.TrackKey, processJobType, productType, totalCount, remainCount));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region GAP PretiltAngleFlagReport
        private const string PretiltAngleFlagTimeout = "PretiltAngleFlagTimeout";
        public void PretiltAngleFlagReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    PretiltAngleFlagReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;

                string logtemp = string.Empty;
                string ngFlag = string.Empty;
                for (int i = 1; i <= 16; i++)
                {
                    logtemp += "ChipPosition#" + i.ToString().PadLeft(2, '0') + " (" + inputData.EventGroups[0].Events[0].Items[i + 1].Value + "),";
                    ngFlag = inputData.EventGroups[0].Events[0].Items[i + 1].Value.Trim() + ",";
                }

                string operatorID = inputData.EventGroups[0].Events[0].Items[18].Value;
                #endregion

                Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                if (job != null)
                {
                    StringBuilder sb = new StringBuilder(new string('N', job.ChipCount));
                    string[] ngArray = ngFlag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ngPos in ngArray)
                    {
                        sb.Replace('N', 'Y', int.Parse(ngPos) - 1, 1);
                    }

                    job.CellSpecial.GAPNGFlag = sb.ToString();
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTESEQUENCENO=[{2}], JOBSEQUENCENO=[{3}],"
                    + logtemp + "OPERATORID=[{4}] SET BIT=[ON]", eqp.Data.NODENO, inputData.TrackKey,
                    cassetteSequenceNo, jobSequenceNo, operatorID));

                PretiltAngleFlagReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PretiltAngleFlagReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PretiltAngleFlagReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PretiltAngleFlagReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PretiltAngleFlagTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PretiltAngleFlagTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PretiltAngleFlagTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PretiltAngleFlagReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PretiltAngleFlagReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] PRETILT ANGLE FLAG REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                PretiltAngleFlagReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region VirtualPortOperationModeChangeReport
        private const string VirtualPortOperationModeChangeReportTimeout = "VirtualPortOperationModeChangeReportTimeout";
        public void VirtualPortOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    VirtualPortOperationModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    VirtualPortOperationModeChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string portMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[1].Value.PadLeft(2, '0');
                string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;

                #endregion

                eVirtualPortMode eVirtualPortMode;
                Enum.TryParse<eVirtualPortMode>(portMode, out eVirtualPortMode);

                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port != null)
                {
                    port.File.VirtualPortMode = portMode;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] PORTNO=[{2}], VIRTUAL_PORT_MODE=[{3}]({4}), OPERATORID=[{5}] SET BIT=[ON]",
                            eqp.Data.NODENO, inputData.TrackKey, portNo, portMode, eVirtualPortMode, operatorID));

                VirtualPortOperationModeChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    VirtualPortOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void VirtualPortOperationModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string portMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[1].Value.PadLeft(2, '0');
                string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                eVirtualPortMode eVirtualPortMode;
                Enum.TryParse<eVirtualPortMode>(portMode, out eVirtualPortMode);

                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port != null)
                {
                    port.File.VirtualPortMode = portMode;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] VIRTUAL_PORT_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, portMode, eVirtualPortMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void VirtualPortOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "VirtualPortOperationModeChangeReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + VirtualPortOperationModeChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + VirtualPortOperationModeChangeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + VirtualPortOperationModeChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(VirtualPortOperationModeChangeReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void VirtualPortOperationModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] VIRTUAL PORT OPERATION MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                VirtualPortOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ForceVCRReadingModeChangeReport
        private const string ForceVCRReadingModeChangeReportTimeout = "ForceVCRReadingModeChangeReportTimeout";
        public void ForceVCRReadingModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    ForceVCRReadingModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    ForceVCRReadingModeChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string vcrReadingMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;

                #endregion

                eVCRReadingMode eVCRReadingMode;
                Enum.TryParse<eVCRReadingMode>(vcrReadingMode, out eVCRReadingMode);

                eqp.File.ForceVCRReadingMode = vcrReadingMode;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] MODE=[{2}]({3}), OPERATORID=[{4}] SET BIT=[ON]",
                            eqp.Data.NODENO, inputData.TrackKey, vcrReadingMode, eVCRReadingMode, operatorID));

                ForceVCRReadingModeChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    ForceVCRReadingModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void ForceVCRReadingModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string vcrReadingMode = inputData.EventGroups[0].Events[0].Items[0].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                #endregion

                eVCRReadingMode eVCRReadingMode;
                Enum.TryParse<eVCRReadingMode>(vcrReadingMode, out eVCRReadingMode);

                lock (eqp) eqp.File.ForceVCRReadingMode = vcrReadingMode;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] FORCE_VCR_READING_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, vcrReadingMode, eVCRReadingMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ForceVCRReadingModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ForceVCRReadingModeChangeReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ForceVCRReadingModeChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ForceVCRReadingModeChangeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ForceVCRReadingModeChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ForceVCRReadingModeChangeReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void ForceVCRReadingModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] FORCE VCR READING MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                ForceVCRReadingModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


        //Watson Modify 20150211 For PMT Line PMI and PTI 
        //1 or 2 Lines in One ServerName ,Get Line use Key Word.
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
            catch
            {
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineApproxname));
                return null;
            }
        }

        //T3 shisyang 整理 20150914 //寫入 file  20150924
        #region [Job Data Check Mode For CELL]
        #region Cassette Setting Code Check Mode
        public void CassetteSettingCodeCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTE SETTING CODE CHECK MODE STATUS=[{2}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.CassetteSettingCodeCheckMode = triggerBit;
                }
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Job Data SeqNo Check Mode
        public void JobDataSeqNoCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTE SETTING CODE CHECK MODE STATUS=[{2}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.JobDataSeqNoCheckMode = triggerBit;
                }
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Turn Angle Check Mode
        public void TurnAngleCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTE SETTING CODE CHECK MODE STATUS=[{2}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.TurnAngleCheckMode = triggerBit;
                }
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Job Type for Dummy Check Mode
        public void JobTypeforDummyCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CASSETTE SETTING CODE CHECK MODE STATUS=[{2}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.JobTypeforDummyCheckMode = triggerBit;
                }
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        //T3 shisyang add 20150824//T3 cs.chou edit 20150911 修正部分名稱錯誤
        #region [Panel OX Information For Glass Request]
        private const string PanelOXInformationForGlassRequestTimeout = "PanelOXInformationForGlassRequestTimeout";
        public void PanelOXInformationForGlassRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    PanelOXInformationForGlassRequestReply(inputData.Metadata.NodeNo, "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value.Trim();
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items["JobSequenceNo"].Value.Trim();
                string unitNO = inputData.EventGroups[0].Events[0].Items["UnitNo"].Value.Trim();
                string operatorID = inputData.EventGroups[0].Events[0].Items["OperatorID"].Value.Trim();
                //T3. Add for ODF line LCI request type.
                string reqType = inputData.EventGroups[0].Events[0].Items["RequestType"] == null ? string.Empty : inputData.EventGroups[0].Events[0].Items["RequestType"].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO));

                string panelOXInfo = string.Empty;
                //T3. No request type or request type is 1:Assembled TFT
                if (string.IsNullOrEmpty(reqType) || reqType == "1")
                    panelOXInfo = slotData.OXRInformation;
                else
                {
                    if (reqType == "2")
                        panelOXInfo = slotData.CellSpecial.TFTPanelOXInfoUnassembled;
                    else if (reqType == "3")
                        panelOXInfo = slotData.CellSpecial.CFPanelOXInfoUnassembled;
                }
                eReturnCode1 rtnCode = string.IsNullOrEmpty(panelOXInfo) ? eReturnCode1.NG : eReturnCode1.OK;
                PanelOXInformationForGlassRequestReply(inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO, unitNO, rtnCode, panelOXInfo, eBitResult.ON, slotData.GlassChipMaskBlockID, inputData.TrackKey);
                //eReturnCode1 rtnCode = slotData.CellSpecial.PanelOXInformation == "" ? eReturnCode1.NG : eReturnCode1.OK;
                //PanelOXInformationForGlassRequestReply(inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO, unitNO, rtnCode, slotData.CellSpecial.PanelOXInformation, eBitResult.ON, slotData.GlassChipMaskBlockID, inputData.TrackKey);
                if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CCCUT_5)//CUT_5 是由L4 機台報廢 sy add 20160703
                        ScrapRuleCommand("L4", eBitResult.ON, inputData.TrackKey, cstSeqNO, slotData.CellSpecial.DisCardJudges,"");
                    else
                        ScrapRuleCommand(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, cstSeqNO, slotData.CellSpecial.DisCardJudges,"");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    PanelOXInformationForGlassRequestReply(inputData.Metadata.NodeNo,
                        inputData.EventGroups[0].Events[0].Items[0].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[1].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[2].Value.Trim(),
                        eReturnCode1.NG, "", eBitResult.ON, "", inputData.TrackKey);
                }
            }
        }
        private void PanelOXInformationForGlassRequestReply(string eqpNo, string cstSeqNO, string jobSeqNO, string unitNO,
            eReturnCode1 returnCode, string oxInfo, eBitResult value, string glsID, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PanelOXInformationForGlassRequestReply") as Trx;
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForGlassRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForGlassRequestTimeout);
                    }
                    #region[Log]
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  BIT=[OFF].", eqpNo, trackKey));
                    #endregion
                    return;
                }
                #endregion
                #region[Reply Data ]
                outputdata.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value = cstSeqNO;
                outputdata.EventGroups[0].Events[0].Items["JobSequenceNo"].Value = jobSeqNO;
                outputdata.EventGroups[0].Events[0].Items["UnitNo"].Value = unitNO; ;
                outputdata.EventGroups[0].Events[0].Items["OXRInformationDownloadReturnCode"].Value = ((int)returnCode).ToString();
                outputdata.EventGroups[0].Events[0].Items["OXRInformation"].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Bin(oxInfo);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForGlassRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForGlassRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + PanelOXInformationForGlassRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PanelOXInformationForGlassRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CST_SEQNO=[{2}],JOB_SEQNO=[{3}],UNIT_NO=[{4}],RETURN_CODE=[{5}],OXR_INFO=[{6}],SET BIT=[{7}].",
                        eqpNo, trackKey, cstSeqNO, jobSeqNO, unitNO, returnCode.ToString(), oxInfo, value));
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PanelOXInformationForGlassRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PANEL OX INFORMATION FOR GLASS REQUEST REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                PanelOXInformationForGlassRequestReply(sArray[0], "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Panel OX Information For Glass Update Report]
        private const string PanelOXInformationForGlassUpdateReportTimeout = "PanelOXInformationForGlassUpdateReportTimeout";
        public void PanelOXInformationForGlassUpdateReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    PanelOXInformationForGlassUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value;
                string oxInfo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion
                #region [Log]

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}] .",
                     inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("CAN'T FIND JOB DATA, CST_SEQNO=[{0}] JOB_SEQNO=[{1}]!", cstSeqNO, jobSeqNO));

                lock (slotData)
                {
                    slotData.OXRInformation = ObjectManager.JobManager.P2M_CELL_PanelBin2OX(oxInfo, slotData.ChipCount);
                }
                ObjectManager.JobManager.EnqueueSave(slotData);

                PanelOXInformationForGlassUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                ObjectManager.JobManager.RecordJobHistory(slotData, eqp.Data.NODEID, inputData.Metadata.NodeNo, "0", "00", "0", eJobEvent.OXUpdate.ToString(), inputData.TrackKey);// OXRUpdate Save Job  History 20150406 Tom
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    PanelOXInformationForGlassUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void PanelOXInformationForGlassUpdateReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PanelOXInformationForGlassUpdateReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;

                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForGlassUpdateReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForGlassUpdateReportTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PanelOXInformationForGlassUpdateReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PanelOXInformationForGlassUpdateReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PanelOXInformationForGlassUpdateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PANEL OX INFORMATION FOR GLASS UPDATE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
                PanelOXInformationForGlassUpdateReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Panel OX Information For Panel Request]
        private const string PanelOXInformationForPanelRequestTimeout = "PanelOXInformationForPanelRequestTimeout";
        public void PanelOXInformationForPanelRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    PanelOXInformationForPanelRequestReply(inputData.Metadata.NodeNo, "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO));

                eReturnCode1 rtnCode = slotData.CellSpecial.PanelOXInformation == "" ? eReturnCode1.NG : eReturnCode1.OK;

                PanelOXInformationForPanelRequestReply(inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO, unitNO, rtnCode, slotData.CellSpecial.PanelOXInformation, eBitResult.ON, slotData.GlassChipMaskBlockID, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    PanelOXInformationForPanelRequestReply(inputData.Metadata.NodeNo,
                        inputData.EventGroups[0].Events[0].Items[0].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[1].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[2].Value.Trim(),
                        eReturnCode1.NG, "", eBitResult.ON, "", inputData.TrackKey);
                }
            }
        }
        private void PanelOXInformationForPanelRequestReply(string eqpNo, string cstSeqNO, string jobSeqNO, string unitNO,
            eReturnCode1 returnCode, string oxInfo, eBitResult value, string glsID, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PanelOXInformationForPanelRequestReply") as Trx;
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForPanelRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForPanelRequestTimeout);
                    }
                    #region[Log]
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,CST_SEQNO=[{2}], JOB_SEQNO=[{3}], UNIT_NO=[{4}],RETURN_CODE=[{5}],OXR_INFO=[{6}],SET BIT=[{7}].",
                            eqpNo, trackKey, cstSeqNO, jobSeqNO, returnCode.ToString(), oxInfo, unitNO, value));
                    #endregion
                    return;
                }
                #endregion
                #region[Reply Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = cstSeqNO;
                outputdata.EventGroups[0].Events[0].Items[1].Value = jobSeqNO;
                outputdata.EventGroups[0].Events[0].Items[2].Value = unitNO; ;
                outputdata.EventGroups[0].Events[0].Items[3].Value = ((int)returnCode).ToString();
                outputdata.EventGroups[0].Events[0].Items[4].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Bin(oxInfo);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForPanelRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForPanelRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + PanelOXInformationForPanelRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PanelOXInformationForPanelRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,CST_SEQNO=[{2}], JOB_SEQNO=[{3}], UNIT_NO=[{4}],RETURN_CODE=[{5}],OXR_INFO=[{6}],SET BIT=[{7}].",
                        eqpNo, trackKey, cstSeqNO, jobSeqNO, returnCode.ToString(), oxInfo, unitNO, value));
                #endregion
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PanelOXInformationForPanelRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PanelOXInformationForPanelRequestReplyTimeout, SET BIT=[OFF].",
                    sArray[0], trackKey));

                PanelOXInformationForPanelRequestReply(sArray[0], "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Panel OX Information For Panel Update Report]
        private const string PanelOXInformationForPanelUpdateReportTimeout = "PanelOXInformationForPanelUpdateReportTimeout";
        public void PanelOXInformationForPanelUpdateReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    PanelOXInformationForPanelUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value;
                string oxInfo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}] OX_INFO=[{5}].",
                     inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO, oxInfo));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("CAN'T FIND JOB DATA, CST_SEQNO=[{0}] JOB_SEQNO=[{1}]!", cstSeqNO, jobSeqNO));

                lock (slotData)
                {
                    slotData.CellSpecial.PanelOXInformation = ObjectManager.JobManager.P2M_CELL_PanelBin2OX(oxInfo, slotData.ChipCount);
                }
                ObjectManager.JobManager.EnqueueSave(slotData);

                PanelOXInformationForPanelUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                ObjectManager.JobManager.RecordJobHistory(slotData, eqp.Data.NODEID, inputData.Metadata.NodeNo, "0", "00", "0", eJobEvent.OXUpdate.ToString(), inputData.TrackKey);// OXRUpdate Save Job  History 20150406 Tom
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    PanelOXInformationForPanelUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void PanelOXInformationForPanelUpdateReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PanelOXInformationForPanelUpdateReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;

                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PanelOXInformationForPanelUpdateReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PanelOXInformationForPanelUpdateReportTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PanelOXInformationForPanelUpdateReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PanelOXInformationForPanelUpdateReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PanelOXInformationForPanelUpdateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PanelOXInformationForPanelUpdateReportReplyTimeout, SET BIT=[OFF].",
                    sArray[0], trackKey));
                PanelOXInformationForPanelUpdateReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Block OX Information Request]
        private const string BlockOXInformationRequestTimeout = "BlockOXInformationRequestTimeout";
        public void BlockOXInformationRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    //RunModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    BlockOXInformationRequestReply(inputData.Metadata.NodeNo, "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO));

                eReturnCode1 rtnCode = slotData.CellSpecial.BlockOXInformation == "" ? eReturnCode1.NG : eReturnCode1.OK;

                BlockOXInformationRequestReply(inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO, unitNO, rtnCode, slotData.CellSpecial.BlockOXInformation, eBitResult.ON, slotData.GlassChipMaskBlockID, inputData.TrackKey);
                if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))
                {
                    if (line.Data.LINETYPE == eLineType.CELL.CCCUT_5)//CUT_5 是由L4 機台報廢 sy add 20160703
                        ScrapRuleCommand("L4", eBitResult.ON, inputData.TrackKey, cstSeqNO, slotData.CellSpecial.DisCardJudges,string.Empty);  //add commandNo by zhuxingxing 20160904
                    else
                        ScrapRuleCommand(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, cstSeqNO, slotData.CellSpecial.DisCardJudges,string.Empty);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    BlockOXInformationRequestReply(inputData.Metadata.NodeNo,
                        inputData.EventGroups[0].Events[0].Items[0].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[1].Value.Trim(),
                        inputData.EventGroups[0].Events[0].Items[2].Value.Trim(),
                        eReturnCode1.NG, "", eBitResult.ON, "", inputData.TrackKey);
                }
            }
        }
        private void BlockOXInformationRequestReply(string eqpNo, string cstSeqNO, string jobSeqNO, string unitNO,
            eReturnCode1 returnCode, string oxInfo, eBitResult value, string glsID, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "BlockOXInformationRequestReply") as Trx;
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + BlockOXInformationRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + BlockOXInformationRequestTimeout);
                    }
                    #region[Log]
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,CST_SEQNO=[{2}], JOB_SEQNO=[{3}], UNIT_NO=[{4}],RETURN_CODE=[{5}],OXR_INFO=[{6}],SET BIT=[{7}].",
                            eqpNo, trackKey, cstSeqNO, jobSeqNO, returnCode.ToString(), oxInfo, unitNO, value));
                    #endregion
                    return;
                }
                #endregion
                #region[Reply Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = cstSeqNO;
                outputdata.EventGroups[0].Events[0].Items[1].Value = jobSeqNO;
                outputdata.EventGroups[0].Events[0].Items[2].Value = unitNO; ;
                outputdata.EventGroups[0].Events[0].Items[3].Value = ((int)returnCode).ToString();
                outputdata.EventGroups[0].Events[0].Items[4].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(oxInfo);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + BlockOXInformationRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + BlockOXInformationRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + BlockOXInformationRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(BlockOXInformationRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,CST_SEQNO=[{2}], JOB_SEQNO=[{3}], UNIT_NO=[{4}],RETURN_CODE=[{5}],OXR_INFO=[{6}],SET BIT=[{7}].",
                        eqpNo, trackKey, cstSeqNO, jobSeqNO, returnCode.ToString(), oxInfo, unitNO, value));
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void BlockOXInformationRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]BlockOXInformationRequestReplyTimeout, SET BIT=[OFF].",
                    sArray[0], trackKey));

                BlockOXInformationRequestReply(sArray[0], "", "", "", eReturnCode1.NG, "", eBitResult.OFF, "", trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Block OX Information Update Report]
        private const string BlockOXInformationUpdateReportTimeout = "BlockOXInformationUpdateReportTimeout";
        public void BlockOXInformationUpdateReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    BlockOXInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value;
                string unitNO = inputData.EventGroups[0].Events[0].Items[2].Value;
                string oxInfo = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion
                #region [Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UNIT_NO=[{4}] OX_INFO=[{5}].",
                     inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, unitNO, oxInfo));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("CAN'T FIND JOB DATA, CST_SEQNO=[{0}] JOB_SEQNO=[{1}]!", cstSeqNO, jobSeqNO));

                lock (slotData)
                {//blockCount 先看MES有沒有給沒有就抓原本JOB多少UP多少
                    slotData.CellSpecial.BlockOXInformation = ObjectManager.JobManager.P2M_CELL_BlockBin2OX(oxInfo, int.Parse(slotData.CellSpecial.BlockCount) == 0 ? slotData.CellSpecial.BlockOXInformation.Length : int.Parse(slotData.CellSpecial.BlockCount));
                }
                ObjectManager.JobManager.EnqueueSave(slotData);

                BlockOXInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                ObjectManager.JobManager.RecordJobHistory(slotData, eqp.Data.NODEID, inputData.Metadata.NodeNo, "0", "00", "0", eJobEvent.OXUpdate.ToString(), inputData.TrackKey);// OXRUpdate Save Job  History 20150406 Tom
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    BlockOXInformationUpdateReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void BlockOXInformationUpdateReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "BlockOXInformationUpdateReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;

                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + BlockOXInformationUpdateReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + BlockOXInformationUpdateReportTimeout);
                }
                #region[If Bit on]
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + BlockOXInformationUpdateReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(BlockOXInformationUpdateReportReplyTimeout), trackKey);
                }
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void BlockOXInformationUpdateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BlockOXInformationUpdateReportReplyTimeout, SET BIT=[OFF].",
                    sArray[0], trackKey));
                BlockOXInformationUpdateReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Port Assignment Event]
        private const string PortAssignmentChangeReportTimeout = "PortAssignmentChangeReportTimeout";
        public void PortAssignmentChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    PortAssignmentChangeReportUpDate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    PortAssignmentChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string portAssignment1 = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string portAssignment2 = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string portAssignment3 = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string portAssignment4 = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string operatorID = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();
                #endregion
                #region [Port Assignment Status UpDate]
                List<Port> portList = new List<Port>();
                foreach (Port port in ObjectManager.PortManager.GetPorts(eqp.Data.NODEID))
                {
                    int portNo = int.Parse(port.Data.PORTNO);
                    switch (inputData.EventGroups[0].Events[0].Items[portNo - 1].Value.Trim())
                    {
                        case "1":
                            if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                            {
                                port.File.PortAssignment = eCELLPortAssignment.GAP;
                            }
                            else if (line.Data.LINETYPE == eLineType.CELL.CCPDR)//PDR LINE [PDR & CEM]
                            {
                                port.File.PortAssignment = eCELLPortAssignment.PDR;
                                //port assign PDR，Prefetch always Enable add by box.zhai 20170419
                                RobotStage robotStage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(eqp.Data.NODENO, port.Data.PORTNO);
                                if (robotStage != null)
                                {
                                    robotStage.Data.PREFETCHFLAG = "Y";
                                    robotStage.Data.PRIORITY = 2;
                                    ObjectManager.RobotStageManager.UpdateDB(robotStage.Data);
                                }
                                else
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Can not find RobotStage by [NODENO={2}] and [PORTNO={3}].", eqp.Data.NODENAME, inputData.TrackKey, eqp.Data.NODENO, port.Data.PORTNO));
                                }
                            }
                            break;
                        case "2":
                            if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                            {
                                port.File.PortAssignment = eCELLPortAssignment.GMI;
                            }
                            else if (line.Data.LINETYPE == eLineType.CELL.CCPDR)//PDR LINE [PDR & CEM]
                            {
                                port.File.PortAssignment = eCELLPortAssignment.CEM;
                                //port assign CEM，Prefetch always Disable add by box.zhai 20170419
                                RobotStage robotStage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(eqp.Data.NODENO, port.Data.PORTNO);
                                if (robotStage != null)
                                {
                                    robotStage.Data.PREFETCHFLAG = "N";
                                    robotStage.Data.PRIORITY = 1;
                                    ObjectManager.RobotStageManager.UpdateDB(robotStage.Data);
                                }
                                else
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Can not find RobotStage by [NODENO={2}] and [PORTNO={3}].", eqp.Data.NODENAME, inputData.TrackKey, eqp.Data.NODENO, port.Data.PORTNO));
                                }
                            }
                            break;
                        default:
                            port.File.PortAssignment = eCELLPortAssignment.UNKNOW;
                            break;
                    }
                    ObjectManager.PortManager.EnqueueSave(port.File);
                }
                #endregion
                #region [Report MES]
                //TO DO MES 還沒有Message
                #endregion
                PortAssignmentChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                #region[Log]
                if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                {                    
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] PortAssignment#01=[{2}], PortAssignment#02=[{3}],PortAssignment#03=[{4}],PortAssignment#04=[{5}],",
                        inputData.Metadata.NodeNo, inputData.TrackKey, portAssignment1 == "1" ? "GAP" : "GMI", portAssignment2 == "1" ? "GAP" : "GMI",
                        portAssignment3 == "1" ? "GAP" : "GMI", portAssignment4 == "1" ? "GAP" : "GMI"));                    
                }
                else//PDR
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] PortAssignment#01=[{2}], PortAssignment#02=[{3}],PortAssignment#03=[{4}],PortAssignment#04=[{5}],",
                        inputData.Metadata.NodeNo, inputData.TrackKey, portAssignment1 == "1" ? "PDR" : "CEM", portAssignment2 == "1" ? "PDR" : "CEM",
                        portAssignment3 == "1" ? "PDR" : "CEM", portAssignment4 == "1" ? "PDR" : "CEM"));
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    PortAssignmentChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PortAssignmentChangeReportUpDate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region [PLCAgent Data Word]
                string portAssignment1 = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string portAssignment2 = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string portAssignment3 = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string portAssignment4 = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string operatorID = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();
                #endregion
                #region [Port Assignment Status UpDate]
                List<Port> portList = new List<Port>();
                foreach (Port port in ObjectManager.PortManager.GetPorts(eqp.Data.NODEID))
                {
                    int portNo = int.Parse(port.Data.PORTNO);
                    switch (inputData.EventGroups[0].Events[0].Items[portNo - 1].Value.Trim())
                    {
                        case "1":
                            if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                            {
                                port.File.PortAssignment = eCELLPortAssignment.GAP;
                            }
                            else if (line.Data.LINETYPE == eLineType.CELL.CCPDR)//PDR LINE [PDR & CEM]
                            {
                                port.File.PortAssignment = eCELLPortAssignment.PDR;
                                //port assign PDR，Prefetch always Enable add by box.zhai 20170419
                                RobotStage robotStage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(eqp.Data.NODENO, port.Data.PORTNO);
                                if (robotStage != null)
                                {
                                    robotStage.Data.PREFETCHFLAG = "Y";
                                    robotStage.Data.PRIORITY = 2;
                                    ObjectManager.RobotStageManager.UpdateDB(robotStage.Data);
                                }
                                else
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Can not find RobotStage by [NODENO={2}] and [PORTNO={3}].", eqp.Data.NODENAME, inputData.TrackKey, eqp.Data.NODENO, port.Data.PORTNO));
                                }
                            }
                            break;
                        case "2":
                            if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                            {
                                port.File.PortAssignment = eCELLPortAssignment.GMI;
                            }
                            else if (line.Data.LINETYPE == eLineType.CELL.CCPDR)//PDR LINE [PDR & CEM]
                            {
                                port.File.PortAssignment = eCELLPortAssignment.CEM;
                                //port assign CEM，Prefetch always Disable add by box.zhai 20170419
                                RobotStage robotStage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(eqp.Data.NODENO, port.Data.PORTNO);
                                if (robotStage != null)
                                {
                                    robotStage.Data.PREFETCHFLAG = "N";
                                    robotStage.Data.PRIORITY = 1;
                                    ObjectManager.RobotStageManager.UpdateDB(robotStage.Data);
                                }
                                else
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Can not find RobotStage by [NODENO={2}] and [PORTNO={3}].", eqp.Data.NODENAME, inputData.TrackKey, eqp.Data.NODENO, port.Data.PORTNO));
                                }
                            }
                            break;
                        default:
                            port.File.PortAssignment = eCELLPortAssignment.UNKNOW;
                            break;
                    }
                    ObjectManager.PortManager.EnqueueSave(port.File);
                }
                #endregion
                #region [Report MES]
                //TO DO MES 還沒有Message
                #endregion
                #region[Log]
                if (line.Data.LINETYPE == eLineType.CELL.CCGAP)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] PortAssignment#01=[{2}], PortAssignment#02=[{3}],PortAssignment#03=[{4}],PortAssignment#04=[{5}],",
                        inputData.Metadata.NodeNo, inputData.TrackKey, portAssignment1 == "1" ? "GAP" : "GMI", portAssignment2 == "1" ? "GAP" : "GMI",
                        portAssignment3 == "1" ? "GAP" : "GMI", portAssignment4 == "1" ? "GAP" : "GMI"));
                }
                else//PDR
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] PortAssignment#01=[{2}], PortAssignment#02=[{3}],PortAssignment#03=[{4}],PortAssignment#04=[{5}],",
                        inputData.Metadata.NodeNo, inputData.TrackKey, portAssignment1 == "1" ? "PDR" : "CEM", portAssignment2 == "1" ? "PDR" : "CEM",
                        portAssignment3 == "1" ? "PDR" : "CEM", portAssignment4 == "1" ? "PDR" : "CEM"));
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                //if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                   // PortAssignmentChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        public void PortAssignmentChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PortAssignmentChangeReportReply") as Trx;

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + PortAssignmentChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + PortAssignmentChangeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + PortAssignmentChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortAssignmentChangeReportReplyTimeout), trackKey);
                }


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void PortAssignmentChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] FORCE VCR READING MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                PortAssignmentChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private const string PortAssignmentCommandTimeout = "PortAssignmentCommandTimeout";
        //public void PortAssignmentCommand(string eqpNo, eBitResult value, string trackKey, List<eCELLPortAssignment> runMode)
        public void PortAssignmentCommand(string lineID, PortAssignmentCommandRequest msg)
        {
            try
            {
                string err = string.Empty;

                #region [GET EQP]
                Equipment eqp;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.ASSIGNMENTLIST[0].EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.ASSIGNMENTLIST[0].EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortAssignmentCommand] " + err });
                    throw new Exception(err);
                }
                #endregion
                #region [CIM MODE OFF 不能改]
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}]CIM_MODE=[OFF], Can't Change Port Assignment!", eqp.Data.NODENO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion
                #region [GET PORTs]
                //List<Port> ports = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                //if (ports.Count < 4)
                //{
                //    err = string.Format("PORT Number error ,IN LINE[{0}] AND EQUIPMENT=[{1}]!", msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO);

                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                //            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortAssignmentCommand] " + err });

                //    throw new Exception(err);
                //}
                #endregion
                #region [Check Port]
                //int gapCount = 0;
                //int gmiCount = 0;
                //foreach (Port port in ports)
                //{
                //    #region [Port上有卡匣不能改]
                //    if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST &&
                //        ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                //    {
                //        err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TYPE!",
                //            eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());

                //        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                //            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                //        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //        return;
                //    }
                //    #endregion
                //    #region [GAP & GMI Count Check]
                //    if (port.File.PortAssignment == eCELLPortAssignment.GAP) gapCount++;
                //    if (port.File.PortAssignment == eCELLPortAssignment.GMI) gmiCount++;
                //    if (gapCount >2 ||gmiCount>2)
                //    {
                //        err = string.Format("EQUIPMENT=[{0}] Port Assignment Count error,[{1}]:Count [{2}] ",
                //            eqp.Data.NODENO,port.File.PortAssignment.ToString() , gapCount >2 ?gapCount:gmiCount);

                //        Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                //            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                //        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //        return;
                //    }
                #endregion
                //}
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqp.Data.NODENO + "_" + "PortAssignmentCommand") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.ASSIGNMENTLIST[0].ASSIGNMENT.ToString();//Port1
                outputdata.EventGroups[0].Events[0].Items[1].Value = msg.BODY.ASSIGNMENTLIST[1].ASSIGNMENT.ToString();//Port2
                outputdata.EventGroups[0].Events[0].Items[2].Value = msg.BODY.ASSIGNMENTLIST[2].ASSIGNMENT.ToString();//Port3
                outputdata.EventGroups[0].Events[0].Items[3].Value = msg.BODY.ASSIGNMENTLIST[3].ASSIGNMENT.ToString();//Port4
                //outputdata.EventGroups[0].Events[0].Items[4].Value = operID;
                //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] Port_Assignment_Mode=[{2}],[{3}],[{4}],[{5}], SET BIT=[ON].",
                  outputdata.TrackKey, outputdata.Metadata.NodeNo, msg.BODY.ASSIGNMENTLIST[0].ASSIGNMENT.ToString(),
                  msg.BODY.ASSIGNMENTLIST[1].ASSIGNMENT.ToString(), msg.BODY.ASSIGNMENTLIST[2].ASSIGNMENT.ToString(), msg.BODY.ASSIGNMENTLIST[3].ASSIGNMENT.ToString()));

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, PortAssignmentCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortAssignmentCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PortAssignmentCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion
                #region [log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ,RETURN CODE =[{4}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, returnCode));
                #endregion
                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, PortAssignmentCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_PortAssignmentCommand", inputData.Metadata.NodeNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "PortAssignmentCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));
                //To Do Call UI:
                //OperationRunModeChangeResultReport(string lineName, string eqpNo, string cmdType, string rtnCode)
                ////cmdType: RUNMODE / LOADEROPERATIONMODE
                //Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                //    new object[] { eqp.Data.LINEID, eqp.Data.NODENO, eOPIATSCmdType.RUNMODE, returnCode.ToString() });
                #region [EQP Reply NG]
                if (returnCode == eReturnCode1.NG)
                {
                    string err = string.Format("EQUIPMENT=[{0}]EQP Reply=[NG], Can't Change Port Assignment!", eqp.Data.NODENO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion
                //List<eCELLPortAssignment> runMode = new List<eCELLPortAssignment>();
                //for (int i = 0; i < 4; i++)
                //{
                //    runMode.Add(eCELLPortAssignment.UNKNOW);
                //}
                //PortAssignmentCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, runMode);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                //// 避免中間發生Exception BCS不把BIT ON起來
                //List<eCELLPortAssignment> runMode = new List<eCELLPortAssignment>();
                //for (int i = 0; i < 4; i++)
                //{
                //    runMode.Add(eCELLPortAssignment.UNKNOW);
                //}
                //if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                //PortAssignmentCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, runMode);
            }
        }
        private void PortAssignmentCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RUN MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, PortAssignmentCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_PortAssignmentCommand", eqp.Data.NODENO);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                //List<eCELLPortAssignment> runMode = new List<eCELLPortAssignment>();
                //for (int i = 0; i < 4; i++)
                //{
                //    runMode.Add(eCELLPortAssignment.UNKNOW);
                //}
                ////PortAssignmentCommand(sArray[0], eBitResult.OFF, trackKey, runMode);
                ////To Do Call UI: NG (TIME OUT)
                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                //if (eqp != null)
                //{
                //    //To Do Call UI:
                //    //Invoke(eServiceName.UIService, "OperationRunModeChangeResultReport",
                //    //    new object[] { eqp.Data.LINEID, sArray[0], eOPIATSCmdType.RUNMODE, eReturnCode1.NG.ToString() });
                //}

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortCurrentAssignmentStatusBlock(Trx inputData)
        {
            try
            {

                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                //取出 Port Real Status
                string port1 = inputData.EventGroups[0].Events[0].Items[0].Value;
                string port2 = inputData.EventGroups[0].Events[0].Items[1].Value;
                string port3 = inputData.EventGroups[0].Events[0].Items[2].Value;
                string port4 = inputData.EventGroups[0].Events[0].Items[3].Value;
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] PortAssignment#01=[{2}], PortAssignment#02=[{3}],PortAssignment#03=[{4}],PortAssignment#04=[{5}],",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port1, port2, port3, port4));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [LotIDCreateRequestReport]
        private const string LotIDCreateRequestReportTimeout = "LotIDCreateRequestReportTimeout";
        public void LotIDCreateRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "", "");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string lotID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string lotType = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();//1.Box 2 .Pallet .3.Tray
                string portNo = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();//portNo //PPK 要報Pallet NO
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] Panel ID=[{2}] .LotType=[{3}],PortNo=[{4}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, lotID, lotType, portNo));
                #endregion
                #region [QPP & PPK history]
                if (line.Data.LINETYPE == eLineType.CELL.CCQPP || line.Data.LINETYPE == eLineType.CELL.CCPPK)
                {
                    PPKEVENTHISTORY ppkHis = new PPKEVENTHISTORY();
                    ppkHis.UPDATETIME = DateTime.Now;
                    ppkHis.TRANSACTIONID = inputData.TrackKey;
                    ppkHis.EVENTNAME = MethodBase.GetCurrentMethod().Name;
                    ppkHis.NODENO = eqp.Data.NODENO;
                    ppkHis.NODEID = eqp.Data.NODEID;
                    ppkHis.BOXCOUNT = "1";
                    ppkHis.BOXID01 = "";
                    ppkHis.BOXID02 = "";
                    ppkHis.BOXTYPE = line.Data.LINETYPE == eLineType.CELL.CCQPP ? "DENSE" : lotType == "1" ? eBoxType.InBox.ToString() : eBoxType.OutBox.ToString();
                    ppkHis.STAGEORPORTORPALLETORCAR = "PORT";
                    ppkHis.PORTNO = portNo;
                    ppkHis.PALLETID = "";
                    ppkHis.PALLETNO = lotType == "2" ? portNo : "";
                    ppkHis.RETURNCODE = "";
                    ppkHis.PRODUCTTYPE = "";
                    ppkHis.PACKUNPACKMODE = ePalletMode.PACK.ToString();
                    ppkHis.LOTNAME = lotID;
                    ppkHis.REQUESTTYPE = lotType == "1" ? "Box" : "Pallet";
                    ppkHis.MAXCOUNT = "";
                    ppkHis.REMARK = line.File.HostMode == eHostMode.OFFLINE ? eHostMode.OFFLINE.ToString() : "";
                    ObjectManager.PalletManager.RecordPPKEventHistory(ppkHis);
                }
                #endregion
                #region Add NodeNo Reply Key
                string key = keyBoxReplyPLCKey.LotIDCreateRequestReportReply;
                string rep = inputData.Metadata.NodeNo;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region [MES]
                switch (lotType)
                {
                    #region [Box]
                    case "1"://box                        
                        string boxType = string.Empty;
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.CELL.CCPPK:
                                #region [OFFLINE]
                                if (line.File.HostMode == eHostMode.OFFLINE)
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                                    LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, lotID, "0");

                                    Cassette cst = ObjectManager.CassetteManager.GetCassette(lotID);
                                    if (cst == null)
                                    {
                                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("EQUIPMENT=[{0}] CAN NOT FIND BOX=[{1}] IN CST OBJECT!", inputData.Metadata.NodeNo, lotID));
                                        return;
                                    }
                                    lock (cst)
                                    {
                                        cst.SubBoxID = lotID;
                                    }
                                    ObjectManager.CassetteManager.EnqueueSave(cst);
                                    return;
                                }
                                #endregion
                                boxType = "OutBox";
                                #region Add INBOX Key
                                string lotIDkey = keyBoxReplyPLCKey.PaperBoxReply;
                                string lotIDrep = lotID;
                                if (Repository.ContainsKey(lotIDkey))
                                    Repository.Remove(lotIDkey);
                                Repository.Add(lotIDkey, lotIDrep);
                                #endregion
                                break;
                            case eLineType.CELL.CCPCK:
                                boxType = "InBox";
                                break;
                            case eLineType.CELL.CCPCS:
                                boxType = "DPBox";
                                break;
                            default:
                                break;
                        }
                        #region [OFFLINE]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                            LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, lotID, "0");
                            return;
                        }
                        #endregion
                        #region Add Port Key
                        string Portkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                        string Portrep = portNo;
                        if (Repository.ContainsKey(Portkey))
                            Repository.Remove(Portkey);
                        Repository.Add(Portkey, Portrep);
                        #endregion
                        Invoke(eServiceName.MESService, "BoxIdCreateRequest", new object[4] { inputData.TrackKey, eqp.Data.LINEID, lotID, boxType });
                        break;
                    #endregion
                    #region [Pallet]
                    case "2"://Pallet
                        switch (line.Data.LINETYPE)
                        {
                            #region [MES 20160530 修改邏輯 同QPP] //20160606 因現場Pallet 沒有ID 與MES 提出不同 修正回來
                            case eLineType.CELL.CCPPK:
                                #region [OFFLINE]
                                if (line.File.HostMode == eHostMode.OFFLINE)
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                                    LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, lotID, "0");
                                    Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(portNo);
                                    if (pallet != null)
                                    {
                                        lock (pallet)
                                        {
                                            pallet.File.NodeNo = inputData.Metadata.NodeNo;
                                            pallet.File.PalletID = lotID;
                                            pallet.File.PalletName = lotID;
                                            pallet.File.PalletMode = ePalletMode.PACK;
                                            pallet.File.PalletDataRequest = ((int)bitResult).ToString();
                                            ObjectManager.PalletManager.EnqueueSave(pallet.File);
                                        }
                                    }
                                    else
                                    {
                                        pallet = new Pallet(new PalletEntityFile());
                                        pallet.File.PalletNo = portNo;
                                        pallet.File.NodeNo = inputData.Metadata.NodeNo;
                                        pallet.File.PalletID = lotID;
                                        pallet.File.PalletName = lotID;
                                        pallet.File.PalletMode = ePalletMode.PACK;
                                        pallet.File.PalletDataRequest = ((int)bitResult).ToString();
                                        ObjectManager.PalletManager.AddPallet(pallet);
                                    }
                                    return;
                                }
                                #endregion
                                break;
                            #endregion
                            default:
                                break;
                        }
                        #region [OFFLINE]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                            LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, lotID, "0");
                            return;
                        }
                        #endregion
                        #region Add Pallet Key
                        string Palletkey = keyBoxReplyPLCKey.LotIDCreateRequestReportReplyForPort;
                        string Palletrep = portNo;
                        if (Repository.ContainsKey(Palletkey))//Pallet
                            Repository.Remove(Palletkey);
                        Repository.Add(Palletkey, Palletrep);
                        #endregion
                        Invoke(eServiceName.MESService, "PalletIdCreateRequest", new object[3] { inputData.TrackKey, eqp.Data.LINEID, lotID });
                        break;
                    #endregion
                    #region [Tray]
                    case "3"://Tray
                        #region [OFFLINE]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                            LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.OK, lotID, "0");
                            return;
                        }
                        #endregion
                        Invoke(eServiceName.MESService, "RuncardIdCreateRequest", new object[3] { inputData.TrackKey, eqp.Data.LINEID, lotID });
                        break;
                    #endregion
                    default: break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "0");
            }
        }
        public void LotIDCreateRequestReportReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string lotName, string count)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_LotIDCreateRequestReportReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + LotIDCreateRequestReportTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + LotIDCreateRequestReportTimeout);
                    }
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,BoxIDCreateRequestReportReply Set Bit =[{2}).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion

                    return;
                }
                #endregion
                #region[MES Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = lotName;  // LOTNAME
                outputdata.EventGroups[0].Events[0].Items[1].Value = count;  // limit count of a Box (INT);
                outputdata.EventGroups[0].Events[0].Items[2].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + LotIDCreateRequestReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LotIDCreateRequestReportTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + LotIDCreateRequestReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LotIDCreateRequestReportReplyTimeout), trackKey);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,LotIDCreateRequestReportReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
                #region [QPP & PPK history]
                if (line.Data.LINETYPE == eLineType.CELL.CCQPP || line.Data.LINETYPE == eLineType.CELL.CCPPK)
                {
                    PPKEVENTHISTORY ppkHis = new PPKEVENTHISTORY();
                    ppkHis.UPDATETIME = DateTime.Now;
                    ppkHis.TRANSACTIONID = outputdata.TrackKey;
                    ppkHis.EVENTNAME = MethodBase.GetCurrentMethod().Name;
                    ppkHis.NODENO = eqp.Data.NODENO;
                    ppkHis.NODEID = eqp.Data.NODEID;
                    ppkHis.BOXCOUNT = "1";
                    ppkHis.BOXID01 = "";
                    ppkHis.BOXID02 = "";
                    ppkHis.BOXTYPE = "";
                    ppkHis.STAGEORPORTORPALLETORCAR = "PORT";
                    ppkHis.PORTNO = "";
                    ppkHis.PALLETID = "";
                    ppkHis.PALLETNO = "";
                    ppkHis.RETURNCODE = rtncode.ToString();
                    ppkHis.PRODUCTTYPE = "";
                    ppkHis.PACKUNPACKMODE = ePalletMode.PACK.ToString();
                    ppkHis.LOTNAME = lotName;
                    ppkHis.REQUESTTYPE = "";
                    ppkHis.MAXCOUNT = count;
                    ppkHis.REMARK = line.File.HostMode.ToString();
                    ObjectManager.PalletManager.RecordPPKEventHistory(ppkHis);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LotIDCreateRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, LotIDCreateRequestReportReplyTimeout Set Bit (OFF).", sArray[0], trackKey));

                LotIDCreateRequestReportReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [GlassChangeLineRequest]
        private const string GlassChangeLineRequestTimeout = "GlassChangeLineRequestTimeout";
        public void GlassChangeLineRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassChangeLineRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CstSeqNo=[{2}] .JobSeqNo=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNo, jobSeqNo));
                #endregion
                #region [OFFLINE]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    //依設定 回機台OK/NG
                    eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));

                    GlassChangeLineRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtncode);

                    return;
                }
                #endregion
                #region Add Reply Key
                string key = keyBoxReplyPLCKey.GlassChangeLineRequestReply;
                string rep = inputData.Metadata.NodeNo;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion
                #region [MES]
                Job job = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo);
                if (job == null)
                {
                    //throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!", edcFile.CassetteSeqNo, edcFile.JobSeqNo));
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}] CAN'T REPORT DEFECT DATA!!",
                               eqp.Data.NODENO, cstSeqNo, jobSeqNo));
                    return;
                }
                List<Job> jobList = new List<Job>();
                jobList.Add(job);
                //InvkeMethod(eServiceName.MESService, "ValidateGlassRequest", new object[4] { trxID, lineName, eqp, joblist });
                Invoke(eServiceName.MESService, "ValidateGlassRequest", new object[4] { inputData.TrackKey, eqp.Data.LINEID, eqp, jobList });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                //LotIDCreateRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "0");
            }
        }
        public void GlassChangeLineRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassChangeLineRequestReply") as Trx;

                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassChangeLineRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + GlassChangeLineRequestTimeout);
                    }
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,BoxIDCreateRequestReportReply Set Bit =[{2}).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion

                    return;
                }
                #endregion
                #region[MES Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassChangeLineRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + GlassChangeLineRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + GlassChangeLineRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassChangeLineRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,GlassChangeLineRequestReply Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void GlassChangeLineRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP GlassChangeLineRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                GlassChangeLineRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [UV Mask Check Request]
        private const string UVMaskCheckRequestTimeout = "UVMaskCheckRequestTimeout";
        public void UVMaskCheckRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    UVMaskCheckRequestReply(inputData.Metadata.NodeNo, eReturnCode1.Unknown, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string uVMaskId = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] UVMask_ID=[{4}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, uVMaskId));
                #endregion
                Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);

                if (slotData == null) throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO));

                eReturnCode1 rtnCode = eReturnCode1.NG;
                if (line.File.HostMode != eHostMode.OFFLINE)
                {
                    foreach (string uVMask in slotData.CellSpecial.UVMaskNames.Split(','))
                    {
                        if (uVMask == uVMaskId)
                        {
                            rtnCode = eReturnCode1.OK;
                            break;
                        }
                    }
                }
                UVMaskCheckRequestReply(inputData.Metadata.NodeNo, rtnCode, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    UVMaskCheckRequestReply(inputData.Metadata.NodeNo, eReturnCode1.NG, eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        private void UVMaskCheckRequestReply(string eqpNo, eReturnCode1 returnCode, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UVMaskCheckRequestReply") as Trx;
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + UVMaskCheckRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + UVMaskCheckRequestTimeout);
                    }
                    #region[Log]
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,RETURN_CODE=[{2}],,SET BIT=[{3}].",
                            eqpNo, trackKey, returnCode.ToString(), value));
                    #endregion
                    return;
                }
                #endregion
                #region[Reply Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + UVMaskCheckRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + UVMaskCheckRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + UVMaskCheckRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UVMaskCheckRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,RETURN_CODE=[{2}],,SET BIT=[{3}].",
                        eqpNo, trackKey, returnCode.ToString(), value));
                #endregion
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void UVMaskCheckRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UVMaskCheckRequestReplyTimeout, SET BIT=[OFF].",
                    sArray[0], trackKey));
                UVMaskCheckRequestReply(sArray[0], eReturnCode1.Unknown, eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        //20171222 huangjiayin
        #region Cell UV Mask Check[EQP]
        public void UVMaskCheckInEQP(string trxId, Equipment eqp, Job job)
        {
            string errMsg = string.Empty;
            //each time DRC receive Job Check SUV UV Mask Jobs
            //only check: TFT Job & !'DUMY' Owner ID
            try
            {
                string[] uvMaskNames = job.CellSpecial.UVMaskNames.Split(',');
                //uvMaskNames 上Port就Check了是否维护，此处应该有值
                //程式只作上锁，不做解锁
                //SUV L16
                List<string> suvEqps = new List<string>();
                suvEqps.Add("L16");
                List<Job> uvMaskInUse = ObjectManager.JobManager.GetJobsbyEQPList(suvEqps).Where(w => w.JobType == eJobType.UV).ToList<Job>();

                if (uvMaskNames.Length < 1)//Job未维护UVMASKNAMES, 但上Port Check被关, DCR收片时又被开启
                {
                    errMsg = string.Format("UVMask Check Error!\r\nDRC Receive Job[{0}] ID=[{1}] has no UVMaskNames!",job.JobKey,job.GlassChipMaskBlockID);
                    throw new Exception(errMsg);
                }

                if (uvMaskInUse.Count<1)//SUV机台没有UVMASK
                {
                    errMsg = string.Format("UVMask Check Error!\r\nCan't find uv mask in suv!");
                    throw new Exception(errMsg);
                }

                //比对UVMASKNAMES与UVMASKINUSE
                foreach (Job j in uvMaskInUse)
                {
                    if (uvMaskNames.Contains(j.GlassChipMaskBlockID.Trim())) continue;
                    Unit suvUnit=ObjectManager.UnitManager.GetUnit("L16",j.CurrentUNITNo);
                    string suvUnitID=string.Empty;
                    if(suvUnit!=null)suvUnitID=suvUnit.Data.UNITID;
                    errMsg = string.Format("UVMask Check Error!\r\n UVMask=[{0}] now in SUV[{1}] is not match glass[{3}] UVmaskNmaes[{2}]!", j.GlassChipMaskBlockID, suvUnitID, job.CellSpecial.UVMaskNames,job.GlassChipMaskBlockID);
                    throw new Exception(errMsg);
                }
              

            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(errMsg))//程式异常报错
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                else//UV Mask Check NG Case
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", errMsg);
                    #region invoke[Transfer Stop Command:DRC]
                    object[] _data1 = new object[]
                    { 
                        eqp.Data.NODENO,
                        "1",//1:stop;2:resume;
                        trxId
                    };
                    Invoke(eServiceName.EquipmentService, "TransferStopCommand", _data1);
                    #endregion

                    #region invoke[Cell CIM Message]
                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxId, eqp.Data.NODENO, errMsg, "BCS", "1" }); 
                    #endregion

                    #region invoke[BCS Terminal Message]
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxId, eqp.Data.LINEID, errMsg });
                    #endregion

                }
 
            }

        }
        #endregion

        #region [ScrapRuleCommand]
        private const string ScrapRuleCommandTimeout = "ScrapRuleCommandTimeout";
        public void ScrapRuleCommand(string eqpNo, eBitResult value, string trackKey, string CassetteSequenceNo, string disCardJudges, string commandNo)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ScrapRuleCommand") as Trx;
                //add New commandNo by zhuxingxing
                if(string.IsNullOrEmpty(commandNo))
                {
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ScrapRuleCommand") as Trx;
                }
                else
                {
                    outputdata = GetServerAgent("PLCAgent").GetTransactionFormat((eqpNo + "_" + "ScrapRuleCommand") + "#" + commandNo )as Trx;
                }

                if (value.Equals(eBitResult.OFF))
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = CassetteSequenceNo;
                    outputdata.EventGroups[0].Events[0].Items[1].Value = disCardJudges;
                }
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                // add commandNo by zhuxingxing 20160904
                string timeoutName;
                if (string.IsNullOrEmpty(commandNo))
                {
                    timeoutName = string.Format(eqpNo + "_" + ScrapRuleCommandTimeout);
                }
                else
                {
                    timeoutName = string.Format(eqpNo + "_" + ScrapRuleCommandTimeout) + "#"+ commandNo;
                }
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                //end 

                //if (_timerManager.IsAliveTimer(eqpNo + "_" + ScrapRuleCommandTimeout))
                //{
                //    _timerManager.TerminateTimer(eqpNo + "_" + ScrapRuleCommandTimeout);
                //}

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ScrapRuleCommandTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ScrapRuleCommandReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,CassetteSequenceNo=[{2}],ScrapRule=[{3}] SET BIT=[{4}].",
                    eqpNo, trackKey, CassetteSequenceNo, disCardJudges, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ScrapRuleCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];//zxx add 20160904 for #0X

                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));
                //add by zhuxingxing 20160904
                string timeoutName;
                if(string.IsNullOrEmpty(commandNo))
                {
                    timeoutName = string.Format(eqp.Data.NODENO + "_" + ScrapRuleCommandTimeout);
                }
                else
                {
                    timeoutName = string.Format(eqp.Data.NODENO + "_" + ScrapRuleCommandTimeout) + "#" +commandNo;
                }
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                //if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + ScrapRuleCommandTimeout))
                //{
                //    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + ScrapRuleCommandTimeout);
                //}
                ScrapRuleCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, string.Empty, string.Empty, commandNo);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    ScrapRuleCommand(inputData.Name.Split('_')[0], eBitResult.OFF, inputData.TrackKey, string.Empty, string.Empty,string.Empty);
            }
        }
        private void ScrapRuleCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
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
                

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ScrapRuleCommand REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                ScrapRuleCommand(sArray[0], eBitResult.OFF, trackKey, string.Empty, string.Empty,commandNo);   //add commandNo by zhuxingxing 20160904
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [RemoveJobDataListReport]
        private const string RemoveJobDataListReportTimeout = "RemoveJobDataListReportTimeout";
        public void RemoveJobDataListReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    RemoveJobDataListReportReply(inputData.Metadata.NodeNo, bitResult, inputData.TrackKey);
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;
                #endregion
                #region [PLCAgent Data Word 2]
                string log = string.Empty;
                string type = inputData.EventGroups[0].Events[1].Items[0].Value;
                log += string.Format(" UNIT_OR_PORT=[{0}]({1})", type, type.Equals("1") ? "UNIT" : (type.Equals("2") ? "PORT" : "UNKNOWN"));
                string unitNo = inputData.EventGroups[0].Events[1].Items[1].Value;
                log += string.Format(" UNIT=[{0}]", unitNo);
                string portNo = inputData.EventGroups[0].Events[1].Items[2].Value;
                log += string.Format(" PORT=[{0}]", portNo);
                string slotNo = inputData.EventGroups[0].Events[1].Items[3].Value;
                log += string.Format(" SLOT_NO=[{0}]", slotNo);
                string removedFlag = inputData.EventGroups[0].Events[1].Items[4].Value;
                switch (removedFlag)
                {
                    case "1": log += " REMOVE_FLAG=[1](NORMAL REMOVE)"; break;
                    case "2": log += " REMOVE_FLAG=[2](RECOVERY)"; break;
                    case "3": log += " REMOVE_FLAG=[3](DELETE DIRTY DATA)"; break;
                    default: log += string.Format(" REMOVE_FLAG=[{0}](UNKNOWN)", removedFlag); break;
                }
                string jobCounts = inputData.EventGroups[0].Events[1].Items["JobCounts"].Value;
                log += string.Format(" JOB_COUNTS=[{0}]", jobCounts);
                #endregion
                RemoveJobDataListReportReply(inputData.Metadata.NodeNo, bitResult, inputData.TrackKey);
                #region [log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}]{6}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cstSeqNO, jobSeqNO, glsID, log));
                #endregion
                #region [Trx RemoveJobDataReport]

                Trx trxForOneSlot = GetServerAgent("PLCAgent").GetTransactionFormat(eqp.Data.NODENO + "_" + "RemoveJobDataReport") as Trx;
                trxForOneSlot.Tag = inputData.Name;//RemoveJobDataReport 區隔是否要寫PLC
                for (int eventCopycount = 0; eventCopycount < trxForOneSlot.EventGroups[0].Events.Count - 1; eventCopycount++)
                {
                    foreach (string itemName in trxForOneSlot.EventGroups[0].Events[eventCopycount].Items.Keys)
                    {
                        trxForOneSlot.EventGroups[0].Events[eventCopycount].Items[itemName].Value = inputData.EventGroups[0].Events[eventCopycount].Items[itemName].Value;//Word 1&2
                    }
                }
                trxForOneSlot.EventGroups[0].Events[2].Items[0].Value = ((int)bitResult).ToString();//Jobdata Bit 1
                trxForOneSlot.TrackKey = inputData.TrackKey;
                for (int i = 0; i < int.Parse(jobCounts); i++)
                {
                    Job job = ObjectManager.JobManager.GetJob(cstSeqNO, (int.Parse(jobSeqNO) + i).ToString());//sy modify 20160826
                    if (job != null)
                    {
                        trxForOneSlot.EventGroups[0].Events[0].Items[1].Value = job.JobSequenceNo;//sy modify 20160826 修改trxForOneSlot 的 JobSequenceNo 
                        switch (removedFlag)
                        {
                            case "1":
                                if (!job.RemoveFlag) Invoke(eServiceName.JobService, "RemoveJobDataReport", new object[] { trxForOneSlot });
                                else
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}]  IS ALREADY REMOVE.",
                                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, job.CassetteSequenceNo, job.JobSequenceNo));
                                break;
                            case "2":
                                if (job.RemoveFlag) Invoke(eServiceName.JobService, "RemoveJobDataReport", new object[] { trxForOneSlot });
                                else
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}]  IS ALREADY RECOCER.",
                                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, job.CassetteSequenceNo, job.JobSequenceNo));
                                break;
                            case "3":
                                Invoke(eServiceName.JobService, "RemoveJobDataReport", new object[] { trxForOneSlot });
                                break;
                            default:
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                       string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] REMOVE FLAG =[{5}] IS NOT DEFLINE.",
                                       eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cstSeqNO, jobSeqNO, removedFlag));
                                return;
                        }
                        Thread.Sleep(400);
                        //jobSeqNO = (int.Parse(jobSeqNO) + 1).ToString();          // modify by zhuxingxing 20160822 JobSeqNo add one
                    }
                    else
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}] BC WIP IS NULL.",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cstSeqNO, jobSeqNO, removedFlag));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void RemoveJobDataListReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RemoveJobDataListReportReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + RemoveJobDataListReportTimeout))
                    _timerManager.TerminateTimer(eqpNo + "_" + RemoveJobDataListReportTimeout);
                if (value == eBitResult.ON)
                    _timerManager.CreateTimer(eqpNo + "_" + RemoveJobDataListReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RemoveJobDataListReportReplyTimeout), trackKey);
                #endregion
                #region [Reply EQ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,RemoveJobDataListReportTimeout Set Bit =[{2}).",
                    eqpNo, trackKey, value.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RemoveJobDataListReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, RemoveJobDataListReportReplyTimeout Set Bit (OFF).", sArray[0], trackKey));

                RemoveJobDataListReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [GlassSamplingRequest]
        private const string GlassSamplingRequestTimeout = "GlassSamplingRequestTimeout";
        public void GlassSamplingRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassSamplingRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown,"","");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string cstSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                #endregion
                #region[Log]
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CstSeqNo=[{2}] .JobSeqNo=[{3}]",
                        inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNo, jobSeqNo));
                #endregion
                Job targetJob = ObjectManager.JobManager.GetJob(cstSeqNo, (int.Parse(jobSeqNo) + 1).ToString());
                eReturnCode1 rtn = eReturnCode1.NG;
                if (targetJob != null)//先看 jobSeqNo + 1 是否符合條件
                {
                    if (targetJob.SamplingSlotFlag == "0" && !targetJob.TrackingData.Contains("1"))
                    {
                        targetJob.SamplingSlotFlag = "1";
                        rtn = eReturnCode1.OK;
                    }
                }

                if (rtn != eReturnCode1.OK) //表示未找到 再找
                {
                    targetJob = ObjectManager.JobManager.GetJobs(cstSeqNo).FirstOrDefault(j => j.SamplingSlotFlag == "0" && !j.TrackingData.Contains("1"));
                    
                    if (targetJob != null)
                    {
                        targetJob.SamplingSlotFlag = "1";
                        rtn = eReturnCode1.OK;
                    }
                }
                if (rtn != eReturnCode1.OK)//NG
                {
                    GlassSamplingRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtn, "0", "0");
                }
                else
                {
                    GlassSamplingRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtn, targetJob.CassetteSequenceNo, targetJob.JobSequenceNo);
                    ObjectManager.JobManager.EnqueueSave(targetJob);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                GlassSamplingRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "0", "0");
            }
        }
        public void GlassSamplingRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string cstSeqNo, string jobSeqNo)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassSamplingRequestReply") as Trx;

                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassSamplingRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + GlassSamplingRequestTimeout);
                    }
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,GlassSamplingRequestReply Set Bit =[{2}).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion

                    return;
                }
                #endregion
                #region[Replay Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[0].Items[1].Value = cstSeqNo;
                outputdata.EventGroups[0].Events[0].Items[2].Value = jobSeqNo; 
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassSamplingRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + GlassSamplingRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + GlassSamplingRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassSamplingRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,GlassSamplingRequestReply Set Bit =[{2}] , ReturnCode[{3}].",
                    eqpNo, trackKey, value.ToString(), rtncode.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void GlassSamplingRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP GlassSamplingRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                 if (_timerManager.IsAliveTimer( sArray[0] + "_" + GlassSamplingRequestTimeout))
                {
                    _timerManager.TerminateTimer( sArray[0] + "_" + GlassSamplingRequestTimeout);
                }
                GlassSamplingRequestReply(sArray[0] , eBitResult.OFF,trackKey, eReturnCode1.NG, "0", "0");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [FTP File]
        public void CreateFtpFile_CELL(Line line, string subPath, Job job, Port port)
        {
            //20161122 add by huangjiayin: Cell Shop新增FileData生成的开关，如果false，则跳过不生成，默认true
            ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
            if (para.ContainsKey("FileDataEnable") && para["FileDataEnable"].GetBoolean() != true) return;

            switch (line.Data.LINETYPE)
            {
                case eLineType.CELL.CCPPK://20161128 sy add
                case eLineType.CELL.CCQPP://20161128 sy add
                case eLineType.CELL.CCOVP://20161128 sy add
                    break;
                case eLineType.CELL.CCPCS:
                    if (port != null)
                    {
                        if (!(port.Data.PORTID == "01" || port.Data.PORTID == "02"))
                        {
                            FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                            FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                        }
                    }
                    break;
                case eLineType.CELL.CCPIL:
                case eLineType.CELL.CCPIL_2:
                case eLineType.CELL.CCODF:
                case eLineType.CELL.CCODF_2://sy add 20160907
                case eLineType.CELL.CCPDR:
                case eLineType.CELL.CCTAM:
                case eLineType.CELL.CCPTH:
                case eLineType.CELL.CCGAP:
                    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, job, true);
                    break;
                case eLineType.CELL.CCRWT:
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
                case eLineType.CELL.CCCRP:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCCRP_2:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCNLS:
                case eLineType.CELL.CCNRD:
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true); 
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true); 
                    break;
                default:
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                    #region [CUT]
                    {
                        if (port != null)
                        {
                            if (!(port.Data.PORTID == "01" || port.Data.PORTID == "02"))
                            {
                                FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                                FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                            }
                        }
                    }
                    #endregion
                    else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                    #region [POL]
                    {
                        FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    }
                    #endregion
                    else
                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
            }
        }
        public void CreateFtpFile_CELL(Line line, string subPath, Job job)
        {

            //20161122 add by huangjiayin: Cell Shop新增FileData生成的开关，如果false，则跳过不生成，默认true
            ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
            if (para.ContainsKey("FileDataEnable") && para["FileDataEnable"].GetBoolean() != true) return;

            

            switch (line.Data.LINETYPE)
            {

                case eLineType.CELL.CCPPK://20161128 sy add
                case eLineType.CELL.CCQPP://20161128 sy add
                case eLineType.CELL.CCOVP://20161128 sy add
                    break;
                case eLineType.CELL.CCPCS:
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
                case eLineType.CELL.CCPIL:
                case eLineType.CELL.CCPIL_2:
                case eLineType.CELL.CCODF:
                case eLineType.CELL.CCODF_2://sy add 20160907
                case eLineType.CELL.CCPDR:
                case eLineType.CELL.CCTAM:
                case eLineType.CELL.CCPTH:
                case eLineType.CELL.CCGAP:
                    FileFormatManager.CreateFormatFile("CELLShopFEOL", subPath, job, true);
                    break;
                case eLineType.CELL.CCRWT:
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
                case eLineType.CELL.CCCRP:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCCRP_2:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCNLS:
                case eLineType.CELL.CCNRD:
                    FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                    FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
                default:
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                    #region [CUT]
                    {
                        FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    }
                    #endregion
                    else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                    #region [POL]
                    {
                        FileFormatManager.CreateFormatFile("CCLineJPS", subPath, job, true);
                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    }
                    #endregion
                    else
                        FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, job, true);
                    break;
            }
        }
        public void DeleteFtpFile_CELL(Line line, string subPath, Job job)
        {
            switch (line.Data.LINETYPE)
            {
                case eLineType.CELL.CCPCS:
                    FileFormatManager.DeleteFormatFile("CCLineJPS", subPath, job.GlassChipMaskBlockID + ".dat");
                    FileFormatManager.DeleteFormatFile("CELLShopBEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    break;
                case eLineType.CELL.CCPIL:
                case eLineType.CELL.CCPIL_2:
                case eLineType.CELL.CCODF:
                case eLineType.CELL.CCODF_2://sy add 20160907
                case eLineType.CELL.CCPDR:
                case eLineType.CELL.CCTAM:
                case eLineType.CELL.CCPTH:
                case eLineType.CELL.CCGAP:
                    FileFormatManager.DeleteFormatFile("CELLShopFEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    break;
                case eLineType.CELL.CCRWT:
                case eLineType.CELL.CCCRP:
                case eLineType.CELL.CCCRP_2:
                case eLineType.CELL.CCNLS:
                case eLineType.CELL.CCNRD:
                    FileFormatManager.DeleteFormatFile("CCLineJPS", subPath, job.GlassChipMaskBlockID + ".dat");
                    FileFormatManager.DeleteFormatFile("CELLShopBEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    break;
                default:
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                    #region [CUT]
                    {
                        FileFormatManager.DeleteFormatFile("CCLineJPS", subPath, job.GlassChipMaskBlockID + ".dat");
                        FileFormatManager.DeleteFormatFile("CELLShopBEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    }
                    #endregion
                    else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                    #region [POL]
                    {
                        FileFormatManager.DeleteFormatFile("CCLineJPS", subPath, job.GlassChipMaskBlockID + ".dat");
                        FileFormatManager.DeleteFormatFile("CELLShopBEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    }
                    #endregion
                    else
                        FileFormatManager.DeleteFormatFile("CELLShopBEOL", subPath, job.GlassChipMaskBlockID + ".dat");
                    break;
            }
        }
        public void MoveFtpFile_CELL(Line line, string sourceSubPath, string descPath, Job job, Port port)//jobdatarequest port=null
        {
            int delayTime = ParameterManager["FILEMOVEDELAYTIME"].GetInteger();//20161115 sy add 
            switch (line.Data.LINETYPE)
            {
                case eLineType.CELL.CCPCS:
                    if (port != null)
                    {
                        if (!(port.Data.PORTID == "01" || port.Data.PORTID == "02"))
                        {
                            FileFormatManager.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, job.GlassChipMaskBlockID + "_JPS.dat");
                            if (delayTime != 0)
                                Thread.Sleep(delayTime);
                            FileFormatManager.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                        }
                    }
                    break;
                case eLineType.CELL.CCPIL:
                case eLineType.CELL.CCPIL_2:
                case eLineType.CELL.CCODF:
                case eLineType.CELL.CCODF_2://sy add 20160907
                case eLineType.CELL.CCPDR:
                case eLineType.CELL.CCTAM:
                case eLineType.CELL.CCPTH:
                case eLineType.CELL.CCGAP:
                    FileFormatManager.MoveFormatFile("CELLShopFEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                    break;
                case eLineType.CELL.CCRWT://20161128 sy add
                case eLineType.CELL.CCCRP:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCCRP_2:// ADD BY LUOJUN 20170502
                case eLineType.CELL.CCNLS:
                case eLineType.CELL.CCNRD:
                    FileFormatManager.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, job.GlassChipMaskBlockID + "_JPS.dat");
                    if (delayTime != 0)
                        Thread.Sleep(delayTime);
                    FileFormatManager.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                    break;
                default:
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                    #region [CUT]
                    {
                        if (port != null)
                        {
                            if (!(port.Data.PORTID == "01" || port.Data.PORTID == "02"))//sy modify  20160705 放在deault
                            {
                                FileFormatManager.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, job.GlassChipMaskBlockID + "_JPS.dat");
                                if (delayTime != 0)
                                    Thread.Sleep(delayTime);
                                FileFormatManager.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                            }
                        }
                    }
                    #endregion
                    else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                    #region [POL]
                    {
                        FileFormatManager.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, job.GlassChipMaskBlockID + "_JPS.dat");
                        if (delayTime != 0)
                            Thread.Sleep(delayTime);
                        FileFormatManager.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                    }
                    #endregion
                    else
                        FileFormatManager.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, job.GlassChipMaskBlockID + ".dat");
                    break;
            }
        }
        #endregion
    }
}